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
    public class BulkOtpRequestJob : IJob    {

        public BulkOtpRequestJob()
        {

        }      

        public async void Execute(IJobExecutionContext context)
        {
            try
            {              
                var response = await new BulkOtpRequestJobService().ProcessBulkOtpRequest();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsBulkOtpRequestCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkOtp" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks"+ "Error: " + ex.Message + " Stack: " + ex.StackTrace);
                
            }
        }

        private void ScheduledJobsBulkOtpRequestCompleted(string errorMessage)
        {
	        try
	        {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkOtp" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing bulk OTP request: " + errorMessage);              
		        
	        }
	        catch (Exception ex)
	        {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkOtp" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks"+
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
	        }
        }
    }
}
