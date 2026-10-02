using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic
{
    public class ReturnsLogic : IReturnsLogic
    {
        #region Constructor

        private readonly ITacToolboxApiLogic _tacToolboxApiLogic;
        private readonly IReturnsRepository _returnsRepository;

        public ReturnsLogic(IReturnsRepository returnsRepository, ITacToolboxApiLogic tacToolboxApiLogic)
        {
            _returnsRepository = returnsRepository;
            _tacToolboxApiLogic = tacToolboxApiLogic;
        }

        #endregion

        #region Constants

        private const string PROCESSING = "Processing - this may take some time to complete, please check back later";
        private const string ELIGIBLE = "Eligible for proactive replacement";
        private const string INELIGIBLE = "Device is not impacted";

        #endregion

        public List<long> GetRmaJobsQueuedForSupportAccount(long supportAccountId)
        {
            var jobsQueued = _returnsRepository.GetRmaJobsQueuedForSupportAccount(supportAccountId);
            return jobsQueued;
        }

        /// <summary>
        /// Queues Tech Support file for processing in TAC Toolbox
        /// </summary>
        /// <param name="techSupportFileName"></param>
        /// <param name="rmaDevice"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public bool QueueTechSuppportFileForProcessing(string techSupportFileName, RmaDeviceDetails rmaDevice, long supportAccountId, long userAccountId)
        {
            bool isQueuedForProcessing;
            try
            {
                //Call TACToolbox API and get the Job Id for the file uploaded
                var jobQueueRequest = new JobQueueRequest() { RmaDeviceDetails = new List<RmaDeviceDetails> { rmaDevice } };
                var jobQueueResponse = _tacToolboxApiLogic.AddToRmaJobQueue(jobQueueRequest, userAccountId);

                isQueuedForProcessing = jobQueueResponse.Code == 29;//Portal Put or Portal Get 100% success.
                var jobQueued = jobQueueResponse.JobsQueued.FirstOrDefault();
                var jobUniqueId = jobQueued.Key;

                _returnsRepository.SaveDeviceRmaJobQueue(jobUniqueId, supportAccountId, rmaDevice, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
                isQueuedForProcessing = false;
            }

            return isQueuedForProcessing;
        }

        /// <summary>
        /// Update the status of jobs being processed by TAC Toolbox
        /// </summary>
        /// <param name="jobIds"></param>
        /// <param name="userAccountId"></param>
        public void UpdateJobsStatus(List<long> jobIds,long userAccountId)
        {
            //Call TACToolbox API and get the Jobs status
            var jobStatusRequest = new JobStatusRequest() { JobIds = jobIds };
            var jobStatusResponse = _tacToolboxApiLogic.GetRmaJobsStatus(jobStatusRequest, userAccountId);

            //Save the status for each job queued
            foreach (var jobStatusInfo in jobStatusResponse.JobsStatusInfo)
            {
                var jobStatus = jobStatusInfo.Value.FirstOrDefault();
                if (jobStatus != null)
                {
                    _returnsRepository.UpdateRmaJobQueueStatus(jobStatus.JobId, jobStatus.Status, jobStatus.StatusCode,
                                                               jobStatus.Verdict,jobStatus.StatusMessage, userAccountId);
                }
            }
        }

        /// <summary>
        /// Get PA200 device list to be displayed 
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        public List<RmaDevice> GetDeviceList(long supportAccountId, long userAccountId)
        {
            var jobIds = GetRmaJobsQueuedForSupportAccount(supportAccountId);
            if (jobIds.Any())
                UpdateJobsStatus(jobIds, userAccountId);

            var rmaDevices = _returnsRepository.GetDeviceList(supportAccountId, userAccountId);

            foreach (var rmaDevice in rmaDevices)
            {
                rmaDevice.StatusMessage = string.IsNullOrEmpty(rmaDevice.Status) ? string.Empty
                                            : !(rmaDevice.Status.Equals(JobStatus.Error.ToString()) || rmaDevice.Status.Equals(JobStatus.Complete.ToString())) ? PROCESSING
                                                : rmaDevice.Verdict.Equals(JobVerdict.Eligible.ToString()) ? ELIGIBLE
                                                        : rmaDevice.Verdict.Equals(JobVerdict.Ineligible.ToString()) ? INELIGIBLE 
                                                            : rmaDevice.StatusMessage;
            }

            return rmaDevices;
        }

        /// <summary>
        /// Check if PA200 SSD Portal is available for the given support account and user role
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public bool ShowReturnsPortalLink(long supportAccountId, long userAccountId)
        {
            var eligiblePa200Devices = _returnsRepository.GetDeviceList(supportAccountId, userAccountId);
            var showReturnsPortal = eligiblePa200Devices.Any() && eligiblePa200Devices.Count <= 25;
            
            return showReturnsPortal;
        }

        /// <summary>
        /// Copy Tech Support File to Amazon S3 bucket
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool CopyTechSupportFile(Stream fileData, string fileName)
        {
            var bucketName = ConfigurationManager.AppSettings["RoundersS3Bucket"];
            using (var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USWest2))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = fileData
                };

                s3Client.PutObject(putRequest);
            }

            return true;
        }

    }
}
