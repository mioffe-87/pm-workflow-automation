using System;
using System.Configuration;
using CspScheduledTasks.HelperClasses;
using Quartz;
using CspScheduledTasks.Logic;
using System.IO;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class OtpExpirationRequestJob : IJob
    {
        public OtpExpirationRequestJob()
        {
          
        }
       
        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new OtpExpirationRequestJobService().ProcessOtpExpirationRequest();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsOtpExpirationRequestCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\OtpExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsOtpExpirationRequestCompleted(string errorMessage)
        {
	       try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\OtpExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing OTP Expiration request: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\OtpExpiration" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
