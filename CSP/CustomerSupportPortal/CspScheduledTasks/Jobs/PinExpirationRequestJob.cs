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
    public class PinExpirationRequestJob : IJob
    {
        public PinExpirationRequestJob()
        {
            
        }
        
        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new PinExpirationRequestJobService().ProcessPinExpirationRequest();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsPinExpirationRequestCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\PinExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsPinExpirationRequestCompleted(string errorMessage)
        {
            try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\PinExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing PIN Expiration request : " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\PinExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
