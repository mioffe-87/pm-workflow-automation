using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;

namespace Csp.Api.HelperClasses
{
    public static class LicenseApis
    {
        private static readonly string LicenseApiUrl = ConfigurationManager.AppSettings["LicenseApi"];
        private static readonly string LicenseApiThumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];

        public static R GetLicenseEntitlementData<T, R>(T requestPayload)
           where T : class
           where R : class
        {
            try
            {
                var licenseEntitlementEndpoint = string.Format("{0}/{1}", LicenseApiUrl, "entitlements/v1/TenantEntitlement");
                string requestData = JsonConvert.SerializeObject(requestPayload);
                var licenseEntitlementsApiRequest = WebCommon.GetWebRequestWithClientCertificate(licenseEntitlementEndpoint, "POST", LicenseApiThumbprint, requestData);
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

        public static R GenericLicensingPost<T, R, String>(T requestPayload, String endpoint)
           where T : class
           where R : class
        {
            try
            {
                var licensingEndpoint = string.Format("{0}/{1}", LicenseApiUrl, endpoint);
                string requestData = JsonConvert.SerializeObject(requestPayload);
                var licenseApiRequest = WebCommon.GetWebRequestWithClientCertificate(licensingEndpoint, "POST", LicenseApiThumbprint, requestData);
                using (var licenseApiResponse = licenseApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licenseApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(licenseApiResponse.GetResponseStream()))
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

        public static R DeactivateLicenses<T, R>(T requestPayload)
           where T : class
           where R : class
        {
            try
            {
                var licenseEndpoint = string.Format("{0}/{1}", LicenseApiUrl, "coresvc/v1/license/deactivate");
                string requestData = JsonConvert.SerializeObject(requestPayload);
                var licenseApiRequest = WebCommon.GetWebRequestWithClientCertificate(licenseEndpoint, "PUT", LicenseApiThumbprint, requestData);
                using (var licenseApiResponse = licenseApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licenseApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(licenseApiResponse.GetResponseStream()))
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
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                using (var response = ex.Response)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.InternalServerError)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
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

        public static PanoramaProvisionResponse PanoramaProvision(PanoramaProvisionRequest requestPayload)
        {
            var response = new PanoramaProvisionResponse();
            string endpoint = string.Empty;
            try
            {
                endpoint = string.Format("{0}/{1}", LicenseApiUrl, "coresvc/v1/panorama/provision");
                string requestData = JsonConvert.SerializeObject(requestPayload);
                var licenseEntitlementsApiRequest = WebCommon.GetWebRequestWithClientCertificate(endpoint, "POST", LicenseApiThumbprint, requestData);
                using (var licenseEntitlementsApiResponse = licenseEntitlementsApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licenseEntitlementsApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created)
                    {
                        using (var reader = new StreamReader(licenseEntitlementsApiResponse.GetResponseStream()))
                        {
                            response = JsonConvert.DeserializeObject<PanoramaProvisionResponse>(reader.ReadToEnd(), new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            });

                            response.Status = true;
                            response.Message = ApiConstants.PanoramaProvisionSuccessMessage;
                        }
                    }

                    new FirewallFlexDataAccess().LogAuditHistory(requestPayload.SupportAccountId, FwFlexRequestType.TokenConsume, endpoint,
                        JsonConvert.SerializeObject(requestPayload), JsonConvert.SerializeObject(response), requestPayload.UserAccountId);
                }
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                using (var errorResponse = ex.Response)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)errorResponse).StatusCode;
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        response.Message = reader.ReadToEnd();
                    }
                }
                new FirewallFlexDataAccess().LogAuditHistory(requestPayload.SupportAccountId, FwFlexRequestType.TokenConsume, endpoint,
                      JsonConvert.SerializeObject(requestPayload), JsonConvert.SerializeObject(response), requestPayload.UserAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            response.Message = !response.Status && string.IsNullOrEmpty(response.Message) ? "Failed to provision panorama." : response.Message;
            return response;
        }

        // Ela2 API call to get activation links
        public static T GetEla2EligibilityData<T>(long supportAccountId) where T : class
        {
            try
            {
                string licenseEntitlementEndpoint = string.Format(ApiConstants.LicenseEntitlementEndpoint, LicenseApiUrl, supportAccountId); //string.Format("{0}/{1}{2}", LicenseApiUrl, "entitlements/v1/getuseraccount?support_account_id=", supportAccountId.ToString());
                var licenseEntitlementsApiRequest = WebCommon.GetWebRequestWithClientCertificate(licenseEntitlementEndpoint, "GET", LicenseApiThumbprint, null);

                using (var licenseEntitlementsApiResponse = licenseEntitlementsApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licenseEntitlementsApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(licenseEntitlementsApiResponse.GetResponseStream()))
                        {
                            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), new JsonSerializerSettings
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
                if (ex is WebException)
                {
                    try
                    {
                        using (var reader = new StreamReader((((WebException)ex)).Response.GetResponseStream()))
                        {
                            if (reader?.ReadToEnd()?.Contains("ela is not supported for the support account") ?? false) // valid 404; ignore
                            {
                                return null;
                            }
                        }
                    }
                    finally
                    {
                    }
                }

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }

            return default(T);
        }
    }
}
