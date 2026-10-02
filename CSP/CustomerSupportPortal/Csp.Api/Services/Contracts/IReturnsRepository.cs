using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface IReturnsRepository
    {
        List<long> GetRmaJobsQueuedForSupportAccount(long supportAccountId);

        bool SaveDeviceRmaJobQueue(long jobId, long supportAccountId, RmaDeviceDetails rmaDevice, long userAccountId);

        bool UpdateRmaJobQueueStatus(long jobId, JobStatus status, int? statusCode, JobVerdict verdict, string statusMessage, long userAccountId);

        long LogTacToolboxApiCall(string apiCall, string requestData, long userAccountId);

        void LogTacToolboxApiResponse(long logId, string responseData);

        List<RmaDevice> GetDeviceList(long supportAccountId, long userAccountId);
    }
}
