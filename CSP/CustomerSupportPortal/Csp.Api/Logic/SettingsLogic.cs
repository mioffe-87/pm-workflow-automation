using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Xsoar;

namespace Csp.Api.Logic
{
    public class SettingsLogic : ISettingsLogic
    {
        private readonly string PANSERVICES_HOST = ConfigurationManager.AppSettings["PanServicesUrl"];

        public IDictionary<string, string> GetPanservicesWebConfigKeyValues(string key)
        {
            try
            {
                string url = PANSERVICES_HOST + "/api/Settings/getwebconfigkeyvalues?key=" + key;
                string thumbprint = ConfigurationManager.AppSettings["PanServicesCertThumbprint"];
                string userName = ConfigurationManager.AppSettings["PanServicesUserName"];
                string userKey = ConfigurationManager.AppSettings["PanServicesUserKey"];
                WebRequest request = !string.IsNullOrWhiteSpace(thumbprint)
                    ? WebCommon.GetWebRequestWithClientCertificate(url, "GET", thumbprint, null, userName, userKey)
                    : WebRequest.Create(url);
                request.ContentType = "application/json";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(dataStream))
                            {
                                dynamic result = JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
                                if (result.status == "Ok")
                                {
                                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(result.data.ToString(), new JsonSerializerSettings()
                                    {
                                        MaxDepth = 32,
                                        TypeNameHandling = TypeNameHandling.None
                                    });
                                }
                                else
                                {
                                    throw new WebException(result.message);
                                }
                            }
                        }
                    }
                    else
                    {
                        Exception ex = new Exception(string.Format("SettingsLogic::GetPanservicesWebConfigKeyValues(" + key + ") Error: {0}, {1}", response.StatusCode, response.StatusDescription));
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("SettingsLogic::GetPanservicesWebConfigKeyValues(" + key + ") Exception: {0}, {1}", ex.Message, ex.StackTrace)));
                throw;
            }

            return new Dictionary<string, string>();
        }
    }
}
