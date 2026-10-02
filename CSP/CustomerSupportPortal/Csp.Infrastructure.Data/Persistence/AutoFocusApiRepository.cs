using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Csp.Api.Services.Contracts;

namespace Csp.Infrastructure.Data.Persistence
{
    public class AutoFocusApiRepository : IAutoFocusApiRepository
    {
        private const string GetKeyUrl = "api/key/{0}?cspPass={1}";
        private const string PostKeyUrl = "api/key/{0}?cspPass={1}";
        private const string PutKeyUrl = "api/key/{0}?cspPass={1}";
        private const string PutLicenseKeyUrl = "api/key/license/{0}?cspPass={1}";
        private const string DeleteKeyUrl = "api/key/{0}?cspPass={1}";


        private readonly ILogAutoFocusApiCallsRepository _autoFocusApiCallsRepository;
        private readonly IConfigValues _configValues;

        public AutoFocusApiRepository(ILogAutoFocusApiCallsRepository autoFocusApiCallsRepository, IConfigValues configValues)
        {
            _autoFocusApiCallsRepository = autoFocusApiCallsRepository;
            _configValues = configValues;
        }

        public AfApiKey GetKey(long supportAccountId, long userAccountId)
        {
            var jsonParser = new JavaScriptSerializer();

            var formattedUrl = string.Format(GetKeyUrl, supportAccountId, _configValues.GetAppSetting(AppSettings.AutoFocusApiPassword));
            var response = GetJsonDataSync<JsonResults.GetKeyJson>(formattedUrl);
            var responseJson = jsonParser.Serialize(response.Result);
            _autoFocusApiCallsRepository.LogApiCall("GET " + formattedUrl, string.Empty, responseJson, userAccountId);

            if (response.IsSuccessStatus())
            {
                AfApiKeyStatuses status;

                if (response.Result.IsDeleted)
                {
                    status = AfApiKeyStatuses.Deleted;
                }
                else
                {
                    if (!AfApiKeyStatuses.TryParse(UppercaseFirst(response.Result.status), out status))
                    {
                        status = AfApiKeyStatuses.Unknown;
                    }
                }

                return new AfApiKey
                {
                    Key = response.Result.key,
                    Status = status,
                };
            }

            return new AfApiKey
            {
                Status = AfApiKeyStatuses.Unknown,
            };
        }

        public AfApiKey PutStatusUpdate(long supportAccountId, bool enable, long userAccountId)
        {
            var jsonParser = new JavaScriptSerializer();

            var formattedUrl = string.Format(PutKeyUrl, supportAccountId, _configValues.GetAppSetting(AppSettings.AutoFocusApiPassword));
            
            var jsonContent = jsonParser.Serialize(new body
            {
                status = enable ? "enabled" : "disabled",
            });

            var response = PutJsonDataSync<JsonResults.GetKeyJson>(formattedUrl, jsonContent);
            var responseJson = jsonParser.Serialize(response.Result);
            _autoFocusApiCallsRepository.LogApiCall("PUT " + formattedUrl, jsonContent, responseJson, userAccountId);

            if (response.IsSuccessStatus())
            {
                AfApiKeyStatuses status;

                if (response.Result.IsDeleted)
                {
                    status = AfApiKeyStatuses.Deleted;
                }
                else
                {
                    if (!AfApiKeyStatuses.TryParse(UppercaseFirst(response.Result.status), out status))
                    {
                        status = AfApiKeyStatuses.Unknown;
                    }
                }

                return new AfApiKey
                {
                    Key = response.Result.key,
                    Status = status,
                };
            }

            return new AfApiKey
            {
                Status = AfApiKeyStatuses.Unknown,
            };
        }

        public AfApiKey PutLicenseKey(long supportAccountId, int useCount, DateTime expirationDateTime, long userAccountId)
        {
            var jsonParser = new JavaScriptSerializer();

            var formattedUrl = string.Format(PutLicenseKeyUrl, supportAccountId, _configValues.GetAppSetting(AppSettings.AutoFocusApiPassword));

            var jsonContent = jsonParser.Serialize(new licensebody
            {
                numUsers = useCount.ToString(),
                ExpirationDateTime = expirationDateTime
            });

            var response = PutJsonDataSync<JsonResults.GetKeyJson>(formattedUrl, jsonContent);
            var responseJson = jsonParser.Serialize(response.Result);
            _autoFocusApiCallsRepository.LogApiCall("PUT " + formattedUrl, jsonContent, responseJson, userAccountId);

            if (response.IsSuccessStatus())
            {
                AfApiKeyStatuses status;

                if (response.Result.IsDeleted)
                {
                    status = AfApiKeyStatuses.Deleted;
                }
                else
                {
                    if (!AfApiKeyStatuses.TryParse(UppercaseFirst(response.Result.status), out status))
                    {
                        status = AfApiKeyStatuses.Unknown;
                    }
                }

                return new AfApiKey
                {
                    Key = response.Result.key,
                    Status = status,
                };
            }

            return new AfApiKey
            {
                Status = AfApiKeyStatuses.Unknown,
            };
        }

        public AfApiKey PostKeyCreation(long supportAccountId, long userAccountId)
        {
            var jsonParser = new JavaScriptSerializer();

            var formattedUrl = string.Format(PostKeyUrl, supportAccountId, _configValues.GetAppSetting(AppSettings.AutoFocusApiPassword));
            var jsonContent = jsonParser.Serialize(new body
            {
                status = "enabled",
            });

            var response = PostJsonDataSync<JsonResults.GetKeyJson>(formattedUrl, jsonContent);
            var responseJson = jsonParser.Serialize(response.Result);
            _autoFocusApiCallsRepository.LogApiCall("POST " + formattedUrl, jsonContent, responseJson, userAccountId);

            if (response.IsSuccessStatus())
            {
                AfApiKeyStatuses status;

                if (response.Result.IsDeleted)
                {
                    status = AfApiKeyStatuses.Deleted;
                }
                else
                {
                    if (!AfApiKeyStatuses.TryParse(UppercaseFirst(response.Result.status), out status))
                    {
                        status = AfApiKeyStatuses.Unknown;
                    }
                }

                return new AfApiKey
                {
                    Key = response.Result.key,
                    Status = status,
                };
            }

            return new AfApiKey
            {
                Status = AfApiKeyStatuses.Unknown,
            };
        }

        public AfApiKey DeleteKey(long supportAccountId, long userAccountId)
        {
            var jsonParser = new JavaScriptSerializer();

            var formattedUrl = string.Format(DeleteKeyUrl, supportAccountId, _configValues.GetAppSetting(AppSettings.AutoFocusApiPassword));
            var response = DeleteJsonDataSync<JsonResults.GetKeyJson>(formattedUrl);
            var responseJson = jsonParser.Serialize(response.Result);
            _autoFocusApiCallsRepository.LogApiCall("DELETE " + formattedUrl, string.Empty, responseJson, userAccountId);

            if (response.IsSuccessStatus())
            {
                AfApiKeyStatuses status;

                if (response.Result.IsDeleted)
                {
                    status = AfApiKeyStatuses.Deleted;
                }
                else
                {
                    if (!AfApiKeyStatuses.TryParse(UppercaseFirst(response.Result.status), out status))
                    {
                        status = AfApiKeyStatuses.Unknown;
                    }
                }

                return new AfApiKey
                {
                    Key = response.Result.key,
                    Status = status,
                };
            }

            return new AfApiKey
            {
                Status = AfApiKeyStatuses.Unknown,
            };
        }


        private static string UppercaseFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return char.ToUpper(text[0]) + text.Substring(1);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class body
        {
            public string status { get; set; }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class licensebody
        {
            public string numUsers { get; set; }
            public string expiration { get; private set; }

            private const string Expirationformat = "yyyy-MM-dd 00:00:00";
            public DateTime ExpirationDateTime
            {
                set { expiration = value.ToString(Expirationformat); }
            }
        }

        
        private Task<T> GetJsonDataSync<T>(string url)
        {
            var result = GetJsonDataAsync<T>(url);
            result.Wait();

            return result;
        }

        private async Task<T> GetJsonDataAsync<T>(string url)
        {
            //Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //Be sure to run "Install-Package Microsoft.Net.Http" from your nuget command line.


            var baseAddress = new Uri(_configValues.GetAppSetting(AppSettings.AutoFocusApiUrl));

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content_type",
                    "application/json; charset=utf-8");

                using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                    var jsonParser = new JavaScriptSerializer();
                    var parsed = jsonParser.Deserialize<T>(responseData);

                    return parsed;
                }
            }
        }


        private Task<T> PostJsonDataSync<T>(string url, string jsonContent)
        {
            var result = PostJsonDataAsync<T>(url, jsonContent);
            result.Wait();

            return result;
        }

        private async Task<T> PostJsonDataAsync<T>(string url, string jsonContent)
        {
            //Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //Be sure to run "Install-Package Microsoft.Net.Http" from your nuget command line.


            var baseAddress = new Uri(_configValues.GetAppSetting(AppSettings.AutoFocusApiUrl));

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content_type", "application/json; charset=utf-8");

                using (var content = new StringContent(jsonContent)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                })
                using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
                {
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                    var jsonParser = new JavaScriptSerializer();
                    var parsed = jsonParser.Deserialize<T>(responseData);

                    return parsed;
                }
            }
        }

        private Task<T> PutJsonDataSync<T>(string url, string jsonContent)
        {
            var result = PutJsonDataAsync<T>(url, jsonContent);
            result.Wait();

            return result;
        }

        private async Task<T> PutJsonDataAsync<T>(string url, string jsonContent)
        {
            //Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //Be sure to run "Install-Package Microsoft.Net.Http" from your nuget command line.


            var baseAddress = new Uri(_configValues.GetAppSetting(AppSettings.AutoFocusApiUrl));

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content_type", "application/json; charset=utf-8");

                using (var content = new StringContent(jsonContent)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                })
                using (var response = await httpClient.PutAsync(url, content).ConfigureAwait(false))
                {
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                    var jsonParser = new JavaScriptSerializer();
                    var parsed = jsonParser.Deserialize<T>(responseData);

                    return parsed;
                }
            }
        }


        private Task<T> DeleteJsonDataSync<T>(string url)
        {
            var result = DeleteJsonDataAsync<T>(url);
            result.Wait();

            return result;
        }

        private async Task<T> DeleteJsonDataAsync<T>(string url)
        {
            //Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //Be sure to run "Install-Package Microsoft.Net.Http" from your nuget command line.


            var baseAddress = new Uri(_configValues.GetAppSetting(AppSettings.AutoFocusApiUrl));

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content_type",
                    "application/json; charset=utf-8");

                using (var response = await httpClient.DeleteAsync(url).ConfigureAwait(false))
                {
                    var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


                    var jsonParser = new JavaScriptSerializer();
                    var parsed = jsonParser.Deserialize<T>(responseData);

                    return parsed;
                }
            }
        }
    }

    public static class TaskExtensions
    {
        private const string SuccessStatus = "RanToCompletion";

        public static bool IsSuccessStatus(this Task task)
        {
            return task.Status.ToString() == SuccessStatus
                   && task.Exception == null;
        }
    }
}
/*
 * 
 * Examples of Usage:
 
curl -XPUT bd9:5005/api/key/245 -H "Content-Type: application/json" -d '{ "status": "enabled" }'
 
 
curl -XPUT bd9:5005/api/key/license/245 -H "Content-Type: application/json" -d '{ "expiration": "2020-06-15 00:00:00", "numUsers": 4 }'
 * 
 * 
Hi Michael, Gayathri,

Dummy end points are up on bd9.paloaltonetworks.local:5005

For example:
curl -XGET bd9.paloaltonetworks.local:5005/api/key/245

Please look at
https://intranet.paloaltonetworks.com/docs/DOC-16108
 for documentation on the end points.

Implementation should be ready by sometime tomorrow.

Both delete and disable will be supported, it is up to you guys if you want to use it.

Tarun

*/