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
    public static class NovaApis
    {
        public static T GetXsoarMarketplaceData<T>(string graphQLRequest) where T : class
        {
            try
            {
                var novaApi = ConfigurationManager.AppSettings["SfdcApi"];
                var novaApiThumbprint = ConfigurationManager.AppSettings["SfdcApiThumbprint"];
                var novaApiRequest = WebCommon.GetWebRequestWithClientCertificate(novaApi, "POST", novaApiThumbprint, graphQLRequest);
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings.Get("SfdcApiUserName") + ":" + ConfigurationManager.AppSettings.Get("SfdcApiPassword")));
                novaApiRequest.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                using (var novaApiResponse = novaApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)novaApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(novaApiResponse.GetResponseStream()))
                        {
                            string response = reader.ReadToEnd();
                            return JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings
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

            return default(T);
        }
    }
}
