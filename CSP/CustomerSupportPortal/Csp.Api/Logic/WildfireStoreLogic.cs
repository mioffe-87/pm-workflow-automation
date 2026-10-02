using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Csp.Api.Services.Contracts;
using System.Net.Http;
using System.Net.Http.Headers;
using Csp.Api.HelperClasses;
using System.Security.Cryptography.X509Certificates;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Api.Logic {
	public class WildfireStoreLogic : IWildfireStore
	{

		private IDeviceDataAccess iDeviceDataAccess;
        private string _baseurl = ConfigurationManager.AppSettings.Get("WFAPIBaseURL");
        private string _reconcileurl = ConfigurationManager.AppSettings.Get("WFAPIReconcileURL");
        private string _userName = ConfigurationManager.AppSettings.Get("WFAPIUserName");
        private string _password = ConfigurationManager.AppSettings.Get("WFAPIPassword");
        private string _cnName = ConfigurationManager.AppSettings.Get("WFAPICertCnName");

		public WildfireStoreLogic(IDeviceDataAccess deviceDataAccess)
		{
			iDeviceDataAccess = deviceDataAccess;
		}

		public bool StoreToWildfireCores(string apikey, int companyid, DateTime expiration) {
			bool result = false;
			DateTime dtBeforeCall, dtAfterResponse;
			TimeSpan dtResponseTime = TimeSpan.Zero;
			string errorMessage = String.Empty;

			foreach (string url in ConfigurationManager.AppSettings["WildfireStore"].ToString().Split(','))
				if (!string.IsNullOrEmpty(apikey)) {
					StringBuilder data = new StringBuilder();
					data.Append("apikey=" + HttpUtility.UrlEncode(apikey));
					data.Append("&company_id=" + HttpUtility.UrlEncode(companyid > 0 ? companyid.ToString() : "245"));
					data.Append("&exp_date=" + HttpUtility.UrlEncode(expiration > DateTime.MinValue ? expiration.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd")));

					var start = DateTime.Now;

					try {
						if (!string.IsNullOrEmpty(apikey)) {
							ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

							var request = (HttpWebRequest)HttpWebRequest.Create(url);
							request.Method = "POST";
							request.ContentType = "application/x-www-form-urlencoded";

							// Set the content length in the request headers  
							request.ContentLength = data.Length;

							StreamWriter myWriter = null;
							myWriter = new StreamWriter(request.GetRequestStream());
							myWriter.Write(data.ToString());
							myWriter.Close();

							dtBeforeCall = DateTime.Now;


							using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
								dtAfterResponse = DateTime.Now;
								var reader = new StreamReader(response.GetResponseStream());
								var txtResp = reader.ReadToEnd();
								if (txtResp.Contains("error")) {
									result = false;
									break;
								}
							}

							if (!dtAfterResponse.Equals(null))
								dtResponseTime = dtAfterResponse.Subtract(dtBeforeCall);

							result = true;
						}
					} catch (Exception e) {
						errorMessage = e.ToString().Replace('\'', '\"');
						result = false;
					} finally {
						if (result) {
							iDeviceDataAccess.LogWildfire(data.ToString(), url, dtResponseTime.Milliseconds, "Success", "", 0);
						} else {
							iDeviceDataAccess.LogWildfire(data.ToString(), url, dtResponseTime.Milliseconds, "Exception | Reverted", errorMessage, 1);
						}
					}
				}

			return result;
		}

        public WildfireApiResponse WildfireReconcile(long supportAccountId)
        {
            var statusCode = String.Empty;
            string errorMessage = String.Empty;
            DateTime dtBeforeCall, dtAfterResponse;
            TimeSpan dtResponseTime = TimeSpan.Zero;
            var responseWildfireKey = new ResponseWildfireKey();
            var requestUrl = string.Format("{0}{1}{2}{3}", _reconcileurl, "/", supportAccountId, "?ProductType=" + ConfigurationManager.AppSettings.Get("WFProductType"));

            try
            {
                dtBeforeCall = DateTime.Now;
                using (var client = new HttpClient(GetRequestHandler()))
                {
                    client.BaseAddress = new Uri(_reconcileurl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();

                    var responseKeys = client.PostAsJsonAsync(requestUrl, typeof(string));
                    responseKeys.Wait();
                    dtAfterResponse = DateTime.Now;
                    var responseResult = responseKeys.Result;
                    statusCode = responseResult.StatusCode.ToString();
                    if (responseResult.IsSuccessStatusCode)
                    {
                        var response = responseResult.Content.ReadAsStringAsync().Result;
                        responseWildfireKey = JsonConvert.DeserializeObject<ResponseWildfireKey>(response);
                    }
                    else
                        errorMessage = statusCode;

                    if (!dtAfterResponse.Equals(null))
                        dtResponseTime = dtAfterResponse.Subtract(dtBeforeCall);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString().Replace('\'', '\"');
            }
            finally
            {
                if (string.IsNullOrEmpty(errorMessage))
                    iDeviceDataAccess.LogWildfire(supportAccountId.ToString(), _reconcileurl, dtResponseTime.Milliseconds, "Success", "", 0);
                else
                    iDeviceDataAccess.LogWildfire(supportAccountId.ToString(), _reconcileurl, dtResponseTime.Milliseconds, "Exception | Reverted", errorMessage, 1);
            }
            return responseWildfireKey.data;
        }

        public WildfireApiResponse GenerateWildFireApiKey(WildfireApiRequest wildfireRequest)
        {
            var statusCode = String.Empty;
            string errorMessage = String.Empty;
            DateTime dtBeforeCall, dtAfterResponse;
            TimeSpan dtResponseTime = TimeSpan.Zero;
            var responseWildfireKey = new ResponseWildfireKey();

            try
            {
                dtBeforeCall = DateTime.Now;
                using (var client = new HttpClient(GetRequestHandler()))
                {
                    var response = client.PutAsJsonAsync(_baseurl, wildfireRequest).Result;
                    statusCode = response.StatusCode.ToString();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        responseWildfireKey = JsonConvert.DeserializeObject<ResponseWildfireKey>(result);
                    }
                    else
                        errorMessage = statusCode;

                    dtAfterResponse = DateTime.Now;
                    if (!dtAfterResponse.Equals(null))
                        dtResponseTime = dtAfterResponse.Subtract(dtBeforeCall);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString().Replace('\'', '\"');
            }
            finally
            {
                if (string.IsNullOrEmpty(errorMessage))
                    iDeviceDataAccess.LogWildfire(JsonConvert.SerializeObject(wildfireRequest), _baseurl, dtResponseTime.Milliseconds, "Success", "", 0);
                else
                    iDeviceDataAccess.LogWildfire(JsonConvert.SerializeObject(wildfireRequest), _baseurl, dtResponseTime.Milliseconds, "Exception | Reverted", errorMessage, 1);
            }
            return responseWildfireKey.data;
        }

        /// <summary>
        /// call to Retrieve wildfire keys
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public ResponseWildfireKeyList GetWildfireKeys(string supportAccountId, int page, int size)
        {
            var responseWildfireKeyList = new ResponseWildfireKeyList();
            var statusCode = default(string);

            try
            {
                using (var client = new HttpClient(GetRequestHandler()))
                {
                    client.BaseAddress = new Uri(_baseurl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();
                    var responseKeys = client.GetAsync("?SupportAccountId=" + supportAccountId + "&page=" + page + "&size=" + size);
                    responseKeys.Wait();
                    var result = responseKeys.Result;
                    statusCode = result.StatusCode.ToString();
                    if (result.IsSuccessStatusCode)
                    {
                        var response = result.Content.ReadAsStringAsync().Result;
                        var responsevalue = JsonConvert.DeserializeObject<ResponseWildfireKeyList>(response, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });

                        responseWildfireKeyList = responsevalue != null ? responsevalue : new ResponseWildfireKeyList();
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return responseWildfireKeyList;
        }

        public void RevokeWildfireKey(string apiKey)
        {
            var revokeUrl = _baseurl + "/" + apiKey;
            DateTime dtBeforeCall, dtAfterResponse;
            TimeSpan dtResponseTime = TimeSpan.Zero;
            string errorMessage = String.Empty;

            try
            {
                dtBeforeCall = DateTime.Now;
                using (var client = new HttpClient(GetRequestHandler()))
                {
                    client.BaseAddress = new Uri(_baseurl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();

                    var responseDelete = client.DeleteAsync(revokeUrl).Result;
                    dtAfterResponse = DateTime.Now;

                    if (!dtAfterResponse.Equals(null))
                        dtResponseTime = dtAfterResponse.Subtract(dtBeforeCall);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString().Replace('\'', '\"');
            }
            finally
            {
                if (string.IsNullOrEmpty(errorMessage))
                    iDeviceDataAccess.LogWildfire(apiKey, revokeUrl, dtResponseTime.Milliseconds, "Success", "", 0);
                else
                    iDeviceDataAccess.LogWildfire(apiKey, revokeUrl, dtResponseTime.Milliseconds, "Exception | Reverted", errorMessage, 1);
            }
        }

        #region Private Methods
        private WebRequestHandler GetRequestHandler()
        {
            WebRequestHandler requestHandler = new WebRequestHandler();
            X509Certificate2 certificate = null;
            certificate = CertificateManager.GetCertificateByCnName(_cnName);
            if (certificate != null)
                requestHandler.ClientCertificates.Add(certificate);
            return requestHandler;
        }

        private AuthenticationHeaderValue GetAuthenticationHeader()
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(_userName + ':' + _password)));
        }

        #endregion

	}
}
