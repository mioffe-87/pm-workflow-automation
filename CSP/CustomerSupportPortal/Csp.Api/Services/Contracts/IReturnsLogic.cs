using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IReturnsLogic
    {
        /// <summary>
        /// Get RMA Jobs queued for the SupportAccount
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        List<long> GetRmaJobsQueuedForSupportAccount(long supportAccountId);

        /// <summary>
        /// Queues Tech Support file for processing in TAC Toolbox
        /// </summary>
        /// <param name="techSupportFileName"></param>
        /// <param name="serialNumber"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        bool QueueTechSuppportFileForProcessing(string techSupportFileName, RmaDeviceDetails rmaDevice,
            long supportAccountId, long userAccountId);
        
        /// <summary>
        /// Update the status of jobs being processed by TAC Toolbox
        /// </summary>
        /// <param name="jobIds"></param>
        /// <param name="userAccountId"></param>
        void UpdateJobsStatus(List<long> jobIds, long userAccountId);

        /// <summary>
        /// Get the list of eligible PA-200 devices to be displayed on SSD Portal
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        List<RmaDevice> GetDeviceList(long supportAccountId, long userAccountId);

        /// <summary>
        /// Check if PA-200 SSD Portal is available for the support account or not
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        bool ShowReturnsPortalLink(long supportAccountId, long userAccountId);
        
        /// <summary>
        /// Copy Tech Support file to Amazon S3 bucket
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool CopyTechSupportFile(Stream fileData, string fileName);
    }
}
