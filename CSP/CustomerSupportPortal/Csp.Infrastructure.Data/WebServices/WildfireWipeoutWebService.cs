using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Csp.Api.Services.Contracts;
using Elmah;

namespace Csp.Infrastructure.Data.WebServices {
	public class WildfireWipeoutWebService : IWildfireWipeoutWebService {

		private readonly IApplicationSettings _applicationSettings;

		public WildfireWipeoutWebService(IApplicationSettings applicationSettings) {
			_applicationSettings = applicationSettings;
		}

		public ApiResult[] WildFireWipeout(string serialnumber) {

			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			var results = new List<ApiResult>();

			var wipeoutUrls = _applicationSettings.WildfireWipeoutApi.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			foreach (var url in wipeoutUrls) {

				var apiUrl = string.Format(url, serialnumber);
				var start = DateTime.Now;
				var result = new ApiResult {
					Data = serialnumber,
					ServiceUrl = apiUrl,
				};


				try {

					var request = (HttpWebRequest)HttpWebRequest.Create(apiUrl);

					using (var response = (HttpWebResponse)request.GetResponse()) {

						result.RunTimeMilliseconds = (DateTime.Now - start).Milliseconds;
						result.ResponseStatusCode = response.StatusCode.ToString();
						result.IsSuccess = response.StatusCode == HttpStatusCode.OK;
						results.Add(result);
					}
				} catch (Exception ex) {

					result.RunTimeMilliseconds = (DateTime.Now - start).Milliseconds;
					result.IsSuccess = false;
					result.ErrorMessage = ex.Message;
					results.Add(result);

					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				}
			}
			return results.ToArray();
		}

		public ApiResult[] DeleteNotificationEmail(string email, string deviceSerialNumber) {

			var wipeoutUrls = _applicationSettings.WildfireDeleteNotifyApi.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			var parameters = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("apikey", _applicationSettings.WildfireApiKey),
				new KeyValuePair<string, string>("email", email)
			};

			if (!string.IsNullOrEmpty(deviceSerialNumber)) {
				parameters.Add(new KeyValuePair<string, string>("device", deviceSerialNumber));
			}

			var results = ApiPushCall(wipeoutUrls, parameters);

			return results;
		}

		public ApiResult[] ReplaceEmailNotification(string oldDeviceSerialNumber, string newDeviceSerialNumber) {
			var wipeoutUrls = _applicationSettings.WildfireDeleteNotifyApi.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			var parameters = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("apikey", _applicationSettings.WildfireApiKey),
				new KeyValuePair<string, string>("olddevice", oldDeviceSerialNumber),
				new KeyValuePair<string, string>("newdevice", newDeviceSerialNumber)
			};

			var results = ApiPushCall(wipeoutUrls, parameters);

			return results;
		}

		private ApiResult[] ApiPushCall(IEnumerable<string> urls, IEnumerable<KeyValuePair<string, string>> parameters) {

			//Handle invalid certificate
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			var results = new List<ApiResult>();

			var formattedParams = new List<string>();
			foreach (var param in parameters) {
				formattedParams.Add(string.Format("{0}={1}", param.Key, HttpUtility.UrlEncode(param.Value)));
			}

			var allParams = string.Join("&", formattedParams);

			foreach (var url in urls) {

				var start = DateTime.Now;
				var result = new ApiResult {
					Data = allParams,
					ServiceUrl = url,
				};


				try {

					var request = (HttpWebRequest)HttpWebRequest.Create(url);
					request.Method = "POST";
					request.ContentType = "application/x-www-form-urlencoded";
					request.ContentLength = allParams.Length;

					using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
						streamWriter.Write(allParams);
						streamWriter.Close();
					}

					using (var response = (HttpWebResponse)request.GetResponse()) {

						result.RunTimeMilliseconds = (DateTime.Now - start).Milliseconds;
						result.ResponseStatusCode = response.StatusCode.ToString();
						result.IsSuccess = response.StatusCode == HttpStatusCode.OK;
						results.Add(result);
					}
				} catch (Exception ex) {

					result.RunTimeMilliseconds = (DateTime.Now - start).Milliseconds;
					result.IsSuccess = false;
					result.ErrorMessage = ex.Message;
					results.Add(result);

					if (HttpContext.Current != null)
						ErrorSignal.FromCurrentContext().Raise(ex);
				}
			}
			return results.ToArray();
		}
	}
}
