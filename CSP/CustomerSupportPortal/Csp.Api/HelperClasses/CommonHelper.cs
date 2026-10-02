using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Csp.Api.Model;
using Newtonsoft.Json;
using System.Web;

namespace Csp.Api.HelperClasses
{
    public class CommonHelper
    {
        public static CommonApiResponse<T> ProcessWebRequest<T>(string apiUrl, string method, string certThumbPrint, string jsonPayload, bool withCert = true, string headerValue = null, string cnName = "", string userAgentHeader = "")
        {
            try
            {
                var request = withCert ? WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, headerValue: headerValue, cnName: cnName, userAgentHeader: userAgentHeader)
                    : WebCommon.GetWebRequest(apiUrl, method, jsonPayload: jsonPayload, headerValue: headerValue);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<CommonApiResponse<T>>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return default(CommonApiResponse<T>);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static async Task<CommonApiResponse<T>> ProcessWebRequestAsync<T>(string apiUrl, string method, string certThumbPrint, string jsonPayload)
        {
            try
            {
                var request = WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload);

                using (var response = await request.GetResponseAsync())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<CommonApiResponse<T>>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return default(CommonApiResponse<T>);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static async Task<CommonApiResponse<T>> ProcessPendoWebRequestAsync<T>(string apiUrl, string method, string certThumbPrint, string jsonPayload)
        {
            try
            {
                var request = WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload);

                using (var response = await request.GetResponseAsync())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<CommonApiResponse<T>>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return default(CommonApiResponse<T>);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("404"))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                throw ex;
            }
        }

        public static string ProcessWebRequest(string apiUrl, string method, string certThumbPrint, string jsonPayload, bool withCert = true, string headerValue = null)
        {
            try
            {
                var request = withCert ? WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, headerValue: headerValue)
                    : WebCommon.GetWebRequest(apiUrl, method, jsonPayload: jsonPayload, headerValue: headerValue);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return jsonResponse;
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static Model.Flex.FileInfo DownloadFile(string apiUrl, string method, string certThumbPrint, string jsonPayload, bool withCert = true, string headerValue = null)
        {
            try
            {
                var request = withCert ? WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, headerValue: headerValue)
                    : WebCommon.GetWebRequest(apiUrl, method, jsonPayload: jsonPayload, headerValue: headerValue);

                using (var response = request.GetResponse())
                {
                    var fileName = response.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty);
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = response.GetResponseStream())
                        {
                            var responseFile = new Model.Flex.FileInfo { FileName = fileName, Content = new MemoryStream() };
                            reader.CopyTo(responseFile.Content);
                            return responseFile;
                        }
                    }
                }
                return new Model.Flex.FileInfo();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static T GetWebResponse<T>(string apiUrl, string method, string certThumbPrint, string jsonPayload, bool withCert = true, string userName = null, string password = null)
        {
            try
            {
                var request = withCert ? WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, username: userName, password: password)
                    : WebCommon.GetWebRequest(apiUrl, method, jsonPayload: jsonPayload);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static dynamic ProcessWebRequestWithAuthHeaders(string apiUrl, string method, string jsonPayload, string authHeader)
        {
            try
            {
                var request = WebCommon.GetWebRequestWithAuthHeaders(apiUrl, method, jsonPayload: jsonPayload, authHeader:authHeader);
                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<dynamic>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public static async Task<CommonApiResponse<T>> ProcessScheduleRequestAsync<T>(string apiUrl, string method, string certThumbPrint = "", string jsonPayload = "", string cnName = "")
        {
            try
            {
                var request = WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, cnName: cnName);

                using (var response = await request.GetResponseAsync())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                return JsonConvert.DeserializeObject<CommonApiResponse<T>>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
                return default(CommonApiResponse<T>);
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                throw ex;
            }
        }

        public static bool ProcessRequest(string apiUrl, string method, string certThumbPrint, string jsonPayload, bool withCert = true, string headerValue = null, string userAgentHeader = "")
        {
            try
            {
                var request = withCert ? WebCommon.GetWebRequestWithClientCertificate(apiUrl, method, certThumbPrint, jsonPayload, headerValue: headerValue, userAgentHeader: userAgentHeader)
                    : WebCommon.GetWebRequest(apiUrl, method, jsonPayload: jsonPayload, headerValue: headerValue);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
    }
}
