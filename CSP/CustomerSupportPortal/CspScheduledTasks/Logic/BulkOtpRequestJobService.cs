using Csp.Api.HelperClasses;
using Csp.Api.Model;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CspScheduledTasks.Logic
{
   public class BulkOtpRequestJobService
    {
        private readonly string _applicationUrl = ConfigurationManager.AppSettings["ApplicationUrl"];
        private readonly string _cnName = ConfigurationManager.AppSettings["ScheduledJobCnName"];
        private readonly string _thumbPrint = ConfigurationManager.AppSettings["ScheduledJobThumbprint"];
        public async Task<CommonApiResponse<ScheduledJobs>> ProcessBulkOtpRequest()
        {

            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.ScheduledJobBulkOtpRequestEndpoint, _applicationUrl);
                var response = await CommonHelper.ProcessScheduleRequestAsync<ScheduledJobs>(baseUrl, method, string.Empty, string.Empty,_cnName);              
                return response;
            }
            catch (Exception ex)
            {                      
                return new CommonApiResponse<ScheduledJobs>()
                {
                    ResponseObject = new ScheduledJobs()
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.ToString()
                    }
                };
            }
        }
    }
}
