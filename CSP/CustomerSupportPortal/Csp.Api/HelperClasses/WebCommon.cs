using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Model.LogsAndNotifications;

namespace Csp.Api.HelperClasses
{
    public class WebCommon
    {
        public static HttpWebRequest GetWebRequest(string url, string method, string contentType = "application/json; charset=utf-8", string jsonPayload = null, int timeout = 100000, string userName = null, string userKey = null, string licenseApiKey = "", string headerValue = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                        | SecurityProtocolType.Tls11
                        | SecurityProtocolType.Tls12
                        | SecurityProtocolType.Ssl3;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
            httpWebRequest.Timeout = timeout;
            if (!string.IsNullOrEmpty(headerValue))
            {
                httpWebRequest.Headers.Add("x-request-source", headerValue);
            }

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userKey))
            {
                httpWebRequest.Credentials = new NetworkCredential(userName, userKey);
            }

            if (!string.IsNullOrEmpty(licenseApiKey))
            {
                httpWebRequest.Headers["LicenseAPIKey"] = licenseApiKey;
            }

            if (!String.IsNullOrWhiteSpace(jsonPayload))
            {
                byte[] dataStream = Encoding.UTF8.GetBytes(jsonPayload);
                httpWebRequest.ContentLength = dataStream.Length;
                Stream newStream = httpWebRequest.GetRequestStream();
                newStream.Write(dataStream, 0, dataStream.Length);
                newStream.Close();
            }

            return httpWebRequest;
        }

        public static HttpWebRequest GetWebRequestWithClientCertificate(string url, string method, string certificateThumbprint,
                                                                        string jsonPayload, string username = null, string password = null, string contentType = "application/json; charset=utf-8", int timeout = 100000, string headerValue = null, string cnName = "", string userAgentHeader = "")
        {
            X509Certificate2 clientCertificate = null;
            if (String.IsNullOrWhiteSpace(certificateThumbprint) && String.IsNullOrWhiteSpace(cnName))
            {
                throw new ArgumentNullException(string.IsNullOrWhiteSpace(certificateThumbprint) ? "CertificateThumbprint" : "CertificateCommonName"
                    , string.IsNullOrWhiteSpace(certificateThumbprint) ? "Certificate Thumbprint is required." : "Certificate Common Name is required.");
            }
            CredentialCache credentialCache = new CredentialCache
            {
                {
                    new Uri(url), "Basic",
                    new NetworkCredential(username, password)
                }
            };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = contentType;
            request.Method = method;
            request.Timeout = timeout;
            if (!string.IsNullOrEmpty(headerValue))
            {
                request.Headers.Add("x-request-source", headerValue);
            }
            if (!string.IsNullOrEmpty(userAgentHeader))
            {
                request.UserAgent = userAgentHeader;
            }
            if (!string.IsNullOrWhiteSpace(username))
            {
                request.Credentials = credentialCache;
            }
            if (!string.IsNullOrEmpty(cnName))
            {
                clientCertificate = CertificateManager.GetCertificateByCnName(cnName);
            }
            else
            {
                clientCertificate = CertificateManager.GetCertificateByCnName(certificateThumbprint);
            }
            if (clientCertificate == null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
            }
            else
            {
                request.ClientCertificates.Add(clientCertificate);
            }

            if (!String.IsNullOrWhiteSpace(jsonPayload))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonPayload);
                }
            }

            return request;
        }       
    
        public static HttpWebRequest GetWebRequestWithBasicAuthentication(string url, string method,string username, string password, string jsonPayload,
                                                                            string apiHeader = null, string apiKey = null,
                                                                            string contentType = "application/json; charset=utf-8", int timeout = 100000)
        {
            CredentialCache credentialCache = new CredentialCache
            {
                {
                    new Uri(url), "Basic",
                    new NetworkCredential(username, password)
                }
            };


            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = contentType;
            request.Method = method;
            request.Timeout = timeout;
            request.Credentials = credentialCache;

            if(!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add(apiHeader, apiKey);
            }

            if (!String.IsNullOrWhiteSpace(jsonPayload))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonPayload);
                }
            }

            return request;
        }


        public static HttpWebRequest GetWebRequestWithAuthHeaders(string url, string method, string contentType = "application/json; charset=utf-8", string jsonPayload = null, int timeout = 100000, string authHeader = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
            httpWebRequest.Timeout = timeout;
            if (!string.IsNullOrEmpty(authHeader))
            {
                httpWebRequest.Headers.Add("Authorization", authHeader);
            }

            
            if (!String.IsNullOrWhiteSpace(jsonPayload))
            {
                byte[] dataStream = Encoding.UTF8.GetBytes(jsonPayload);
                httpWebRequest.ContentLength = dataStream.Length;
                Stream newStream = httpWebRequest.GetRequestStream();
                newStream.Write(dataStream, 0, dataStream.Length);
                newStream.Close();
            }

            return httpWebRequest;
        }


        public static T GetEntitlementsFromSfdc<T>(string graphQlRequest)
        {
            try
            {
                var sfdcApi = ConfigurationManager.AppSettings["SfdcApi"];
                var sfdcApiThumbprint = ConfigurationManager.AppSettings["SfdcApiThumbprint"];
                var sfdcApiRequest = GetWebRequestWithClientCertificate(sfdcApi, "POST", sfdcApiThumbprint, graphQlRequest);

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings.Get("SfdcApiUserName") + ":" + ConfigurationManager.AppSettings.Get("SfdcApiPassword")));
                sfdcApiRequest.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                using (var sfdcApiResponse = sfdcApiRequest.GetResponse())
                {
                    var statusCode = ((HttpWebResponse)sfdcApiResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(sfdcApiResponse.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(jsonResponse)
                                && JsonConvert.DeserializeObject<dynamic>(jsonResponse).errors is IEnumerable<object>)
                            {
                                Elmah.ErrorSignal.FromCurrentContext().Raise(new System.ApplicationException("GetWebRequest::GetEntitlementsFromSfdc. Request: " + graphQlRequest));
                                Elmah.ErrorSignal.FromCurrentContext().Raise(new System.ApplicationException("GetWebRequest::GetEntitlementsFromSfdc. Response: " + jsonResponse));
                            }
                            return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
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

        public static void BffLogInfo(Services.Contracts.IApplicationSettings applicationSettings,
            string log, string source, EBffLogLevel level)
        {
            BffLogInfo(applicationSettings, log, source, level, DateTime.Now);
        }

        public static void BffLogInfo(Services.Contracts.IApplicationSettings applicationSettings,
            string log, string source, DateTime dateTime)
        {
            BffLogInfo(applicationSettings, log, source, EBffLogLevel.INFO, dateTime);
        }

        public static void BffLogInfo(Services.Contracts.IApplicationSettings applicationSettings,
            string log, string source)
        {
            BffLogInfo(applicationSettings, log, source, EBffLogLevel.INFO, DateTime.Now);
        }

        public static void BffLogInfo(Services.Contracts.IApplicationSettings applicationSettings,
            string log, string source, EBffLogLevel level, DateTime dateTime)
        {
            var jsonPayload = JsonConvert.SerializeObject(new BffLogInfo
            {
                Log = log,
                BffLogLevel = level,
                Source = source,
                Date = dateTime
            });
            CommonHelper.ProcessWebRequest<dynamic>(applicationSettings.EntitlementServiceBffHost + "/v1/log-info", 
                System.Net.Http.HttpMethod.Post.Method, applicationSettings.EntitlementBffThumbprint, jsonPayload, true);
        }

        public class CommonApiResponse<T>
    {
        [JsonProperty("data")]
        public T ResponseObject { get; set; }
    }

        public class FedrampEvent
        {
            public long supportAccountId { get; set; }
            public string emailAddress { get; set; }
        }

        public class UserProfileEvent
        {
            public string eventType { get; set; }
            public string email { get; set; }
            public bool domainAdmin { get; set; }
        }

    }
}
