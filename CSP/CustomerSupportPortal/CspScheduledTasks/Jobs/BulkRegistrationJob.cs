using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.Configuration;
using System.Web.Mvc;
using CspScheduledTasks.SfdcServiceClient;
using CspScheduledTasks.ScheduledJobsServiceClient;
using CspScheduledTasks.HelperClasses;
using CspScheduledTasks.Logic;
using System.IO;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class BulkRegistrationJob : IJob
    {
        public BulkRegistrationJob()
        {
           
        }
       
        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new BulkRegistrationJobService().ProcessBulkRegistrations();
                if(response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsServiceProcessBulkRegistrationsCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkReg" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            
            }
        }

        private void ScheduledJobsServiceProcessBulkRegistrationsCompleted(string errorMessage)
        {
            try
            {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkReg" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing bulk registrations: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkReg" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
