using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CspScheduledTasks.ScheduledJobsServiceClient;
using Quartz;
using CspScheduledTasks.HelperClasses;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class AutofocusTrialExpirationJob : IJob
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

                scheduledJobsService.ProcessAutofocusTrialExpirationCompleted += ScheduledJobsServiceProcessAutofocusTrialExpirationCompleted;
                scheduledJobsService.ProcessAutofocusTrialExpirationAsync();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"], "Error in CSP Scheduled Tasks", "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsServiceProcessAutofocusTrialExpirationCompleted(object sender, ProcessAutofocusTrialExpirationCompletedEventArgs args)
        {
            try
            {
                if (args != null && args.Result != null && !args.Result.IsSuccess)
                {
                    EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"],
                        "Error in CSP Scheduled Tasks",
                        string.Format("Error processing Autofocus Trial role expiration {0}", args.Result.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(ConfigurationManager.AppSettings["NotificationList"], "Error in CSP Scheduled Tasks", "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
