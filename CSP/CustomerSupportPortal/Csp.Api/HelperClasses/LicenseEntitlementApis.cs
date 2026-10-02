using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.HelperClasses
{
    public static class LicenseEntitlementApis
    { 
        public static R GetLicenseEntitlementData<T, R>(T requestPayload)
           where T : class
           where R : class
        {
            try
            {
                var licenseEntitlementApi = ConfigurationManager.AppSettings["LicenseApi"];
                var licenseEntitlementApiThumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
                var licenseEntitlementEndpoint = string.Format("{0}/{1}", licenseEntitlementApi, "entitlements/v1/TenantEntitlement");
                string requestData = JsonConvert.SerializeObject(requestPayload);
                var licenseEntitlementsApiRequest = WebCommon.GetWebRequestWithClientCertificate(licenseEntitlementEndpoint, "POST", licenseEntitlementApiThumbprint, requestData);

                using (var licenseEntitlementsApiResponse = licenseEntitlementsApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licenseEntitlementsApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(licenseEntitlementsApiResponse.GetResponseStream()))
                        {
                            return JsonConvert.DeserializeObject<R>(reader.ReadToEnd(), new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
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
