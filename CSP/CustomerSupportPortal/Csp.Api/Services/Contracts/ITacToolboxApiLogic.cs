using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ITacToolboxApiLogic
    {
        /// <summary>
        /// Adds the Device TechSupport file to Job Queue to be processed
        /// </summary>
        /// <param name="queueRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        JobQueueResponse AddToRmaJobQueue(JobQueueRequest queueRequest, long userAccountId);

        /// <summary>
        /// Get the status of Device TechSupport file processing
        /// </summary>
        /// <param name="statusRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        JobStatusResponse GetRmaJobsStatus(JobStatusRequest statusRequest, long userAccountId);

        /// <summary>
        /// Get the RMA eligibility status of PA200 devices
        /// </summary>
        /// <param name="deviceListRequest"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        PortalDeviceListResponse GetPortalDeviceList(PortalDeviceListRequest deviceListRequest, long userAccountId);
    }
}
