using System;
using System.Configuration;
using CspScheduledTasks.HelperClasses;
using Quartz;
using CspScheduledTasks.Logic;
using System.IO;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class VmElaTokenNotificationJob : IJob
    {
        public VmElaTokenNotificationJob()
        {
          
        }

        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new VmElaTokenNotificationJobService().ProcessVmElaTokenNotification();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsServiceVmElaTokenNotificationCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\VmElaToken" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsServiceVmElaTokenNotificationCompleted(string errorMessage)
        {            
            try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\VmElaToken" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing VM ELA Token Notifications: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\VmElaToken" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
