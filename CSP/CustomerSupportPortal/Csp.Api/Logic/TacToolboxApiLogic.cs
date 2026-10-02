using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;

namespace Csp.Api.Logic
{
    public class TacToolboxApiLogic : ITacToolboxApiLogic
    {
        #region Constants

        private const string API_KEY = "api_key";
        private const string API_ACTION = "api_action";
        private const string PORTAL_DATA = "portal_data";

        private const string PORTAL_PUT = "portal_put";
        private const string PORTAL_GET = "portal_get";
        private const string PORTAL_LIST = "portal_list";

        #endregion

        private readonly IReturnsRepository _returnsRepository;

        public TacToolboxApiLogic(IReturnsRepository returnsRepository)
        {
            _returnsRepository = returnsRepository;
        }

        /// <summary>
        /// Adds the TechSupport file to Job Queue for processing
        /// </summary>
        /// <param name="queueRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public JobQueueResponse AddToRmaJobQueue(JobQueueRequest queueRequest, long userAccountId)
        {
            var jsonRequestData = JsonConvert.SerializeObject(queueRequest.RmaDeviceDetails);
            var response = GetJobData<JobQueueResponse>(PORTAL_PUT, jsonRequestData, userAccountId);

            return response;
        }

        /// <summary>
        /// Get the status of TechSupport file processing
        /// </summary>
        /// <param name="statusRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public JobStatusResponse GetRmaJobsStatus(JobStatusRequest statusRequest, long userAccountId)
        {
            var jsonRequestData = JsonConvert.SerializeObject(statusRequest);
            var response = GetJobData<JobStatusResponse>(PORTAL_GET, jsonRequestData, userAccountId);

            return response;
        }

        /// <summary>
        /// Get the RMA eligibility status of PA200 devices
        /// </summary>
        /// <param name="deviceListRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public PortalDeviceListResponse GetPortalDeviceList(PortalDeviceListRequest deviceListRequest, long userAccountId)
        {
            var jsonRequestData = JsonConvert.SerializeObject(deviceListRequest);
            var response = GetJobData<PortalDeviceListResponse>(PORTAL_LIST, jsonRequestData, userAccountId);

            return response;
        }

        /// <summary>
        /// Common method to call TACToolbox APIs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiAction"></param>
        /// <param name="jsonRequestData"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        private T GetJobData<T>(string apiAction, string jsonRequestData, long userAccountId)
        {
            try
            {
                var baseUrl = ConfigurationManager.AppSettings["TacToolboxAPI"];
                var apiKey = ConfigurationManager.AppSettings["TacToolboxApiKey"];

                //Convert Portal data to Base64
                var encoder = new ASCIIEncoding();
                var base64EncodedRequestData = Convert.ToBase64String(encoder.GetBytes(jsonRequestData));

                var uri = new Uri(baseUrl);
                var queryParams = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(API_KEY, apiKey),
                    new KeyValuePair<string, string>(API_ACTION, apiAction),
                    new KeyValuePair<string, string>(PORTAL_DATA, base64EncodedRequestData)
                };

                using (var client = new HttpClient { BaseAddress = uri })
                {
                    uri = _addQueryParams(uri, queryParams);

                    //Log API Call in database
                    var tacToolboxApiLogId = _returnsRepository.LogTacToolboxApiCall(baseUrl, apiAction +" "+ jsonRequestData, userAccountId);

                    using (var responseMessage = client.GetAsync(uri).Result)
                    {
                        var jsonResponse = responseMessage.Content.ReadAsStringAsync().Result;
                        var jobDetails = JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });

                        //Update API Call Log with response data
                        if (tacToolboxApiLogId > 0)
                            _returnsRepository.LogTacToolboxApiResponse(tacToolboxApiLogId, jsonResponse);

                        return jobDetails;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        /// <summary>
        /// To append Query String parameters to Base URL
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        private static Uri _addQueryParams(Uri uri, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            try
            {
                var uriBuilder = new UriBuilder(uri);
                foreach (var queryParam in queryParams)
                {
                    if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                    {
                        uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + queryParam.Key + "=" + queryParam.Value;
                    }
                    else
                    {
                        uriBuilder.Query = queryParam.Key + "=" + queryParam.Value;
                    }
                }
                return uriBuilder.Uri;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }
    }
}
