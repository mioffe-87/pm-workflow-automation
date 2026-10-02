using Csp.Api.HelperClasses;
using Csp.Api.Model;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace CspScheduledTasks.Logic
{
    public class VmElaTokenNotificationJobService
    {
        private readonly string _applicationUrl = ConfigurationManager.AppSettings["ApplicationUrl"];
        private readonly string _cnName = ConfigurationManager.AppSettings["ScheduledJobCnName"];
        private readonly string _thumbPrint = ConfigurationManager.AppSettings["ScheduledJobThumbprint"];

        public async Task<CommonApiResponse<ScheduledJobs>> ProcessVmElaTokenNotification()
        {
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.ScheduledJobVmElaTokenEndpoint, _applicationUrl);
                return await CommonHelper.ProcessScheduleRequestAsync<ScheduledJobs>(baseUrl, method, string.Empty, string.Empty,_cnName);
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
