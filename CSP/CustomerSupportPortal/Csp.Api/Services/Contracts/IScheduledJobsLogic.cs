using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IScheduledJobsLogic
    {
        CommonApiResponse<ScheduledJobs> ProcessBulkRegistrations();
        CommonApiResponse<ScheduledJobs> SyncMembershipsWithSfdc();
        CommonApiResponse<ScheduledJobs> ProcessPinExpirationRequest();
        CommonApiResponse<ScheduledJobs> ProcessOtpExpirationRequest();
        CommonApiResponse<ScheduledJobs> ProcessVmElaTokenNotification();       
        CommonApiResponse<ScheduledJobs> ProcessBulkOtpRequest();
        CommonApiResponse<ScheduledJobs> ProcessBulkDeviceTransfer();
        CommonApiResponse<ScheduledJobs> ProcessEmailNotificationStatusUpdate();
        CommonApiResponse<ScheduledJobs> ProcessEvalAccountCreationRequest();
    }
}
