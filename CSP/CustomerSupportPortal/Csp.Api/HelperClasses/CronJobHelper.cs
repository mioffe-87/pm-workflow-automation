using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using MySql.Data.MySqlClient;

namespace Csp.Api.HelperClasses
{
    public class CronJobHelper
    {
        
        public static bool IsJobPending(CronJobEnum jobId)
        {
            long count = 0;
                const string sqlQuery = "SELECT COUNT(*) FROM cronjobqueue c WHERE c.JobId = @p_jobId AND c.JobStatusId = @p_jobStatus";
                string connString = Secrets.CspDatabase;
                CronJobHelper.IsJobPendingforLongTime(jobId);
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("p_jobId", jobId);
                    command.Parameters.AddWithValue("p_jobStatus", CronJobStatus.Pending);
                    count = Convert.ToInt64(command.ExecuteScalar());
                }
                
                return count > 0;
        }
        public static void  IsJobPendingforLongTime(CronJobEnum jobId)
        {
            long count = 0;
            // checking for the job pending staus for more than 60 minutes
            const string sqlQuery = "SELECT * FROM cronjobqueue c WHERE c.JobId = @p_jobId AND c.JobStatusId = @p_jobStatus AND TIMESTAMPDIFF(HOUR,c.CreatedOn,NOW()) >1";
            string connString = Secrets.CspDatabase;
            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("p_jobId", jobId);
                command.Parameters.AddWithValue("p_jobStatus", CronJobStatus.Pending);
                count = Convert.ToInt64(command.ExecuteScalar());
            } 
           if (count >0)
           {
                CronJobHelper.UpdateJobStatus( jobId, CronJobStatus.Failed);                
           }
        }
        public static CronJobEnum QueueJob(CronJobEnum jobId)
        {
            const string sqlQuery =
                    "INSERT INTO cronjobqueue(JobId, JobStatusId, ScheduledOn, CreatedByUserAccountId, CreatedOn, LastUpdateByUserAccountId, LastUpdate) VALUES(@p_jobId, @p_jobStatus, @p_scheduledOn, @p_createdBy, @p_createdOn, @p_lastUpdateByUserAccountId, @p_lastUpdateDate);";
            string connString = Secrets.CspDatabase;

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("p_jobId", jobId);
                    command.Parameters.AddWithValue("p_jobStatus", CronJobStatus.Pending);
                    command.Parameters.AddWithValue("p_scheduledOn", DateTime.Now);
                    command.Parameters.AddWithValue("p_createdBy", ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                    command.Parameters.AddWithValue("p_createdOn", DateTime.Now);
                    command.Parameters.AddWithValue("p_lastUpdateDate", DateTime.Now);
                    command.Parameters.AddWithValue("p_lastUpdateByUserAccountId", ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                    command.ExecuteNonQuery();
                }
                return jobId;
        }


        public static bool UpdateJobStatus(CronJobEnum jobId, CronJobStatus status)
        {
            const string sqlQuery = "Update cronjobqueue set JobStatusId=@p_status, LastUpdateByUserAccountId=@p_lastUpdateByUserAccountId, LastUpdate=@p_lastUpdateDate, CompletedOn=@p_completedOn Where JobId=@p_jobId; delete from cronjobqueue where JobId=@p_jobId and JobStatusId=@p_status";
            string connString = Secrets.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("p_status", status);
                command.Parameters.AddWithValue("p_lastUpdateByUserAccountId", ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                command.Parameters.AddWithValue("p_lastUpdateDate", DateTime.Now);
                command.Parameters.AddWithValue("p_jobId", jobId); 
                command.Parameters.AddWithValue("p_completedOn", DateTime.Now);
                command.ExecuteNonQuery();
            }
            return true;
        }
    }
}
