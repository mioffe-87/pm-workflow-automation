using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Csp.Api.Model.GoogleApis;
using Csp.Api.HelperClasses;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;

namespace Csp.Api.Logic
{
    public class ServiceProxy
    {
        private readonly HttpClient httpClient;
        private readonly GoogleApplicationCredentials applicationCredentials;

        public ServiceProxy()
        {
            this.httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(4)
            };

            var appCredsPath = ConfigurationManager.AppSettings["GoogleApplicationCredentials"];
            if (appCredsPath != null && File.Exists(appCredsPath.ToString()))
            {
                var creds = File.ReadAllText(appCredsPath.ToString(), Encoding.UTF8);
                this.applicationCredentials = JsonConvert.DeserializeObject<GoogleApplicationCredentials>(creds);
            }
        }

        /// <summary>
        /// Leverages the Google Application Credentials (service account), to create and sign a valid JWT
        /// </summary>
        /// <returns></returns>
        public string CreateServiceAccountJwt(string audience, out double expiresInSecs)
        {
            expiresInSecs = 21600; // 6 hours
            
            if (this.applicationCredentials == null)
            {
                return null;
            }

            List<string> segments = new List<string>();

            var header = new
            {
                alg = "RS256",
                typ = "JWT",
                kid = applicationCredentials.PrivateKeyId
            };

            var payload = new
            {
                sub = applicationCredentials.ClientEmail,
                iss = applicationCredentials.ClientEmail,
                aud = audience,
                iat = ApiCommon.SecondsSinceUnixEpoch,
                exp = ApiCommon.SecondsSinceUnixEpoch + expiresInSecs
            };

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));

            segments.Add(ApiCommon.Base64UrlEncode(headerBytes));
            segments.Add(ApiCommon.Base64UrlEncode(payloadBytes));

            string stringToSign = string.Join(".", segments.ToArray());

            byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            var reader = new Org.BouncyCastle.OpenSsl.PemReader(new StringReader(applicationCredentials.PrivateKey));

            RsaPrivateCrtKeyParameters privkey = (RsaPrivateCrtKeyParameters)reader.ReadObject();
            
            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");

            sig.Init(true, new RsaKeyParameters(true, privkey.Modulus, privkey.Exponent));

            sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
            byte[] signature = sig.GenerateSignature();

            segments.Add(ApiCommon.Base64UrlEncode(signature));

            var token = string.Join(".", segments.ToArray());

            return token;
        }

        public void TrackServiceRequest(System.Net.Http.HttpRequestMessage serviceRequest, string userEmail, string errorMessage = "")
        {
            string connString = Secrets.CspDatabase;
            const string sqlQuery = "INSERT INTO logservicerequest (LogType, Email, RequestedUrl, HttpMethod, ContentLength, ErrorMessage) VALUES (@type, @email, @requestedUrl, @httpMethod, @contentLength, @errorMessage)";

            using (var connection = new MySqlConnection(connString))
            {
                try
                {
                    connection.Open();

                    var command = new MySqlCommand(sqlQuery, connection);
                    command.Parameters.Add("@type", MySqlDbType.Int16).Value = 0; // Request
                    command.Parameters.Add("@email", MySqlDbType.String).Value = userEmail;
                    command.Parameters.Add("@requestedUrl", MySqlDbType.String).Value = serviceRequest.RequestUri.ToString();
                    command.Parameters.Add("@httpMethod", MySqlDbType.String).Value = serviceRequest.Method.Method;
                    command.Parameters.Add("@contentLength", MySqlDbType.Int64).Value = serviceRequest.Content != null ? serviceRequest.Content.Headers.ContentLength : 0;
                    command.Parameters.Add("@errorMessage", MySqlDbType.String).Value = errorMessage;

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
        }

        public void TrackServiceResponse(System.Net.Http.HttpResponseMessage serviceResponse, string userEmail)
        {
            string connString = Secrets.CspDatabase;
            const string sqlQuery = "INSERT INTO logservicerequest (LogType, Email, RequestedUrl, StatusCode, HttpMethod) VALUES (@type, @email, @requestedUrl, @statusCode, @httpMethod)";

            using (var connection = new MySqlConnection(connString))
            {
                try
                {
                    connection.Open();

                    var command = new MySqlCommand(sqlQuery, connection);
                    command.Parameters.Add("@type", MySqlDbType.Int16).Value = 1; // Response
                    command.Parameters.Add("@email", MySqlDbType.String).Value = userEmail;
                    command.Parameters.Add("@requestedUrl", MySqlDbType.String).Value = serviceResponse.RequestMessage.RequestUri.ToString();
                    command.Parameters.Add("@statusCode", MySqlDbType.Int16).Value = (int)serviceResponse.StatusCode;
                    command.Parameters.Add("@httpMethod", MySqlDbType.String).Value = serviceResponse.RequestMessage.Method.Method;

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
        }

        /// <summary>
        /// Sends a HTTP Request to a Service
        /// </summary>
        /// <param name="method">HTTP method for the request</param>
        /// <param name="requestUri">URI for the request</param>
        /// <param name="userIdEmail">Email address</param>
        /// <param name="content">Content of the request</param>
        /// <param name="authorization">String content for the authorization header</param>
        /// <param name="additionalHttpHeaders">A collection of key/value pairs that represents HTTP headers</param>
        /// <returns>The HTTP resonse message</returns>
        public async Task<HttpResponseMessage> Request(
            HttpMethod method,
            string requestUri,
            string userIdEmail,
            HttpContent content,
            string authorization,
            NameValueCollection additionalHttpHeaders = null)
        {
            var errorMessage = string.Format("Unable to get a response from {0}. Please try again later.", requestUri);
            using (var proxyRequest = new HttpRequestMessage(method, requestUri))
            {
                if (method != HttpMethod.Get)
                {
                    proxyRequest.Content = content;
                }

                if (!string.IsNullOrEmpty(authorization))
                {
                    proxyRequest.Headers.Add("Authorization", authorization);
                }

                // Adding extra headers for backend API
                if (additionalHttpHeaders != null)
                {
                    for (int i = 0; i < additionalHttpHeaders.Count; i++)
                    {
                        proxyRequest.Headers.Add(additionalHttpHeaders.GetKey(i), additionalHttpHeaders[i]);
                    }
                }

                System.Net.Http.HttpResponseMessage proxyResponse = null;
                // Tracking the request right before sending it
                this.TrackServiceRequest(proxyRequest, userIdEmail, "");

                try
                {
                    proxyResponse = await httpClient.SendAsync(proxyRequest);
                    // Tracks the response
                    this.TrackServiceResponse(proxyResponse, userIdEmail);

                    return proxyResponse;
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                    if (proxyResponse != null && proxyResponse.Content != null)
                    {
                        this.TrackServiceResponse(proxyResponse, userIdEmail);
                        return proxyResponse;
                    }

                    errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    // Something went wrong with the request, so this will track the failure request with an error message
                    this.TrackServiceRequest(proxyRequest, userIdEmail, ex.Message);

                    throw;
                }
            }
        }
    }
}
