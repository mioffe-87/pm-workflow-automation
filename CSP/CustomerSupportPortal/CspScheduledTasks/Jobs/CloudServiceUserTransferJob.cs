using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CspScheduledTasks.HelperClasses;
using CspScheduledTasks.ScheduledJobsServiceClient;
using Quartz;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class CloudServiceUserTransferJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var scheduledJobsService = new ScheduledJobsService
                {
                    AuthHeaderValue = new AuthHeader()
                    {
                        Username = ApiCommon.Hash(ConfigurationManager.AppSettings["ServiceAccountUsername"]),
                        Password = ApiCommon.Hash(ConfigurationManager.AppSettings["ServiceAccountPassword"])
                    }
                };

                scheduledJobsService.ProcessCloudServicesUsersTransferCompleted += ScheduledJobsCloudServicesUsersTransferCompleted;
                scheduledJobsService.ProcessCloudServicesUsersTransferAsync();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"], "Error in CSP Scheduled Tasks", "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsCloudServicesUsersTransferCompleted(object sender, ProcessCloudServicesUsersTransferCompletedEventArgs args)
        {
            try
            {
                if (args != null && args.Result != null && !args.Result.IsSuccess)
                {
                    EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"],
                        "Error in CSP Scheduled Tasks",
                        string.Format("Error processing cloud service users transfer {0}", args.Result.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"], "Error in CSP Scheduled Tasks", "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
