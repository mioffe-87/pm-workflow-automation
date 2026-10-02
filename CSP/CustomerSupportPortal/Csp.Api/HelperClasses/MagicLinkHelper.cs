using System;
using System.Configuration;
using System.Net;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.HelperClasses
{
    public static class MagicLinkHelper
    {
        public static void SetBundleMagicLinks(List<LicenseListItem> licenses, string licensingApiKey)
        {
            SetRaptorBundleMagicLinks(licenses, licensingApiKey);
            SetSellingBundleWithIoTLicenseMagicLinks(licenses, licensingApiKey);
        }

        public static void SetRaptorBundleMagicLinks(List<LicenseListItem> licenses, string licensingApiKey)
        {

            try
            {
                var licenseRaptorBundle = licenses.Where(x => x.ModelName != null).SingleOrDefault(x => x.ModelName.Contains("-BND-ENT"));

                if (licenseRaptorBundle == null) return;

                //the child auth codes of the bundle starts with "B". this way, we can separate the bundle child authcode from a-la-carte IOT / SAAS INLINE auth code
                var licenseIoT = licenses.SingleOrDefault(x => x.AuthorizationCode.StartsWith("B")
                                                               && (x.LicenseFeaturesName.ToUpper().Contains("IOT") || x.Pandbidentification == "IOT"));
                var licenseSaaS = licenses.SingleOrDefault(x => x.AuthorizationCode.StartsWith("B")
                                                                && (x.LicenseFeaturesName.ToUpper() == "SAASINLINE" || x.Pandbidentification == "SAASINLINE"));

                if (licenseSaaS == null || licenseIoT == null ||
                    (licenseIoT.ExpirationDate != null && licenseSaaS.ExpirationDate != null)) return;
                var magicLinks =
                    GetMagicLinksByAuthCode<string, ActivationLinkResponse>(licenseRaptorBundle.AuthorizationCode, licensingApiKey);
                if (magicLinks == null || !magicLinks.IsValid()) return;

                var magicLinkIoT = magicLinks.Licenses.SingleOrDefault(x => x.ProductSubGroup == "IOT")
                    ?.ActivationLink;
                var magicLinkSaaS =
                    magicLinks.Licenses.SingleOrDefault(x => x.ProductSubGroup == "SAAS")
                        ?.ActivationLink;
                  if (!string.IsNullOrEmpty(magicLinkIoT))
                licenseIoT.MagicLink = magicLinkIoT;

            if (string.IsNullOrEmpty(magicLinkSaaS)) return;
            licenseSaaS.MagicLink = magicLinkSaaS;
            }

            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return;
        }

        public static void SetSellingBundleWithIoTLicenseMagicLinks(List<LicenseListItem> licenses, string licensingApiKey)
        {
            var licenseSellingBundleWithIoT = licenses.Where(x => x.ModelName != null).SingleOrDefault(x => x.ModelName.Contains("-BND-NETSEG-CAMPUS"));
            if (licenseSellingBundleWithIoT == null)
            {
                return;
            }

            //the child auth codes of the bundle starts with "B". this way, we can separate the bundle child authcode from others
            var licenseIoT = licenses.SingleOrDefault(x => x.AuthorizationCode.StartsWith("B") 
                && (x.LicenseFeaturesName.ToUpper().Contains("IOT") || x.Pandbidentification == "IOT"));
            if (licenseIoT == null
                || licenseIoT.ExpirationDate != null)
            {
                return;
            }

            var magicLinks = GetMagicLinksByAuthCode<string, ActivationLinkResponse>(licenseSellingBundleWithIoT.AuthorizationCode, licensingApiKey);
            if (magicLinks == null
                || !magicLinks.IsValid())
            {
                return;
            }

            var magicLinkIoT = magicLinks.Licenses.SingleOrDefault(x => x.ProductSubGroup == "IOT")?.ActivationLink;
            if (!string.IsNullOrEmpty(magicLinkIoT))
            {
                licenseIoT.MagicLink = magicLinkIoT;
            }
        }

        private static R GetMagicLinksByAuthCode<T, R>(T requestPayload, string licensingApiKey)
            where T : class
            where R : class
        {
            HttpWebRequest request = null;
            string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
            string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];

            string url = ConfigurationManager.AppSettings["PanServicesUrl"] +
                         "/api/EasyOnboarding/GetMagicLinkByAuthcode?authcode=" + requestPayload;
            request = WebCommon.GetWebRequestWithBasicAuthentication(url, "GET", panServicesUserName,
                panServicesUserKey, string.Empty,
                "LicenseAPIKey", licensingApiKey);
            try
            {
                using (WebResponse webResponse = request.GetResponse())
                {
                    using (Stream webStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            return JsonConvert.DeserializeObject<R>(responseReader.ReadToEnd(),
                                new JsonSerializerSettings()
                                {
                                    MaxDepth = 32,
                                    TypeNameHandling = TypeNameHandling.None
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return default(R);
        }
    }
}
