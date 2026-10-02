using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using CspScheduledTasks.SfdcServiceClient;
using Quartz.Job;
using CspScheduledTasks.HelperClasses;
using CspScheduledTasks.Logic;
using static Csp.Api.HelperClasses.WebCommon;
using Csp.Api.Model;
using System.IO;

namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class SfdcAddUpdateContactJob : IJob
    {
        public SfdcAddUpdateContactJob()
        {
           
        }

        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new SfdcAddUpdateContactJobService().SyncMembershipsWithSfd();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    SfdcServiceSyncMembershipsWithSfdcCompleted(response.ResponseObject.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\SfdcAddUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void SfdcServiceSyncMembershipsWithSfdcCompleted(string errorMessage)
        {           
            try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\SfdcAddUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing membership sync with Sfdc contact: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\SfdcAddUpdate" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error in CSP Scheduled Tasks" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
