using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CspScheduledTasks.HelperClasses;
using CspScheduledTasks.ScheduledJobsServiceClient;
using Quartz;
using CspScheduledTasks.Logic;
using System.IO;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class EmailNotificationsJob : IJob
    {
        public EmailNotificationsJob()
        {

        }

        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new EmailNotificationStatusUpdateJobService().ProcessEmailNotificationStatusUpdate();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsEmailNotificationStatusUpdateCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\EmailNotificationStatusUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);

            }
        }

        private void ScheduledJobsEmailNotificationStatusUpdateCompleted(string errorMessage)
        {
            try
            {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\EmailNotificationStatusUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing EmailNotificationStatusUpdate: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\EmailNotificationStatusUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
