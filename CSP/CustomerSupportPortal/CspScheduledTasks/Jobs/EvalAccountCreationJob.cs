using System;
using System.Configuration;
using CspScheduledTasks.HelperClasses;
using Quartz;
using CspScheduledTasks.Logic;
using System.IO;


namespace CspScheduledTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class EvalAccountCreationJob : IJob
    {
        public EvalAccountCreationJob()
        {

        }
        public async void Execute(IJobExecutionContext context)
        {
            try
            {
                var response = await new EvalAccountCreationJobService().ProcessEvalAccounts();
                if (response?.ResponseObject?.IsSuccess == false)
                {
                    ScheduledJobsServiceProcessEvalAccountsCompleted(response.ResponseObject.ErrorMessage);
                }
               
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\ProcessEvalAccounts" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "ProcessEvalAccountss" + "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }

        private void ScheduledJobsServiceProcessEvalAccountsCompleted(string errorMessage)
        {
            try
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\ProcessEvalAccounts" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing EvalAccount Creation request: " + errorMessage);

            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\ProcessEvalAccounts" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error in CSP Scheduled Tasks" + "Error processing EvalAccount Creation" +
                    "Error: " + ex.Message + " Stack: " + ex.StackTrace);
            }
        }
    }
}
