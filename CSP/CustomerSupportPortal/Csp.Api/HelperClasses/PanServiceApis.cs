using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace Csp.Api.HelperClasses
{
    public static class PanServiceApis
    {
        public static R RegenerateMagicLink<T, R>(T requestPayload, string licenseApiKey)
           where T : class
           where R : class
        {
            try
            {
                var panServicesUrl = ConfigurationManager.AppSettings["PanServicesUrl"];                
                var panServicesEndpoint = string.Format("{0}/{1}", panServicesUrl, "api/EasyOnboarding/RegenerateMagicLink");
                string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
                string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];

                string requestData = JsonConvert.SerializeObject(requestPayload);
                var panServicesApiRequest = WebCommon.GetWebRequest(panServicesEndpoint, "PUT", "application/json", requestData, userName: panServicesUserName, userKey: panServicesUserKey, licenseApiKey: licenseApiKey);

                using (var licenseApiResponse = panServicesApiRequest.GetResponse())
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
    }
}
