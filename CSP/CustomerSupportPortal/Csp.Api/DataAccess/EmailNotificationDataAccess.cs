using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DirectConnect.Model;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.SalesforceWebApi;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;

namespace Csp.Api.DataAccess
{
    class EmailNotificationDataAccess : BaseDataAccess,IEmailNotificationDataAccess
    {
        public const string GetPendingEmailNotifications = "GetPendingEmailNotifications";
        public const string UpdateEmailNotificationStatusById = "UpdateEmailNotificationStatusByNotificationId";
        public const string StatusComplete = ApiConstants.COMPLETE;

        public bool LogEmailNotificationByNotificationId(LogEmailNotification logEmailNotification)
        {
          
                try {

                    using (var mySqlConnection = new MySqlConnection(Secrets.CspDatabase))
                    {
                        var mySqlCommand = new MySqlCommand();

                        mySqlCommand.CommandType = CommandType.StoredProcedure;
                        mySqlCommand.CommandText = "logEmailNotificationByNotificationId";


                        mySqlCommand.Parameters.AddWithValue("p_AwsEmailNotificationId", logEmailNotification.AwsEmailNotificationId);
                        mySqlCommand.Parameters.AddWithValue("p_Status", logEmailNotification.Status);
                        mySqlCommand.Parameters.AddWithValue("p_Addressing", logEmailNotification.Addressing);
                        mySqlCommand.Parameters.AddWithValue("p_CreatedByUseraccountId", logEmailNotification.CreatedByUserAccountId);
                        mySqlCommand.Parameters.AddWithValue("p_ApplicationName", logEmailNotification.ApplicationName);
                        mySqlCommand.Parameters.AddWithValue("p_CountSent", logEmailNotification.CountSent);
                        mySqlCommand.Parameters.AddWithValue("p_CountToBeSent", logEmailNotification.CountToBeSent);
                        mySqlCommand.Parameters.AddWithValue("p_FeatureName", logEmailNotification.FeatureName);

                        mySqlCommand.Connection = mySqlConnection;
                        mySqlConnection.Open();

                        mySqlCommand.ExecuteNonQuery();
                    }
                    return true;

                }
                catch (Exception ex) 
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return false;
                }
           
        }

        public List<string> GetPendingEmailNotificationsList()
        {
            try
            {
                var notificationList = new List<string>();
                using (var command = Connection.GetStoredProcCommand(GetPendingEmailNotifications))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                           notificationList.Add(Convert.ToString(dataReader["AwsEmailNotificationId"]));
                        }
                    }
                }

                return notificationList;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new List<string>();
            }
        }

        public void UpdateEmailNotificationStatus(long countProcessed, string awsNotificationId)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand(UpdateEmailNotificationStatusById))
                {
                    Connection.AddInParameter(command, "p_awsNotificationId", DbType.String, awsNotificationId);
                    Connection.AddInParameter(command, "p_CountProcessed", DbType.Int16, countProcessed);
                    Connection.AddInParameter(command, "p_Status", DbType.String, StatusComplete);

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public Model.EmailTemplate GetEmailTemplateByTemplateId(int templateId)
        {
            try
            {

                var mySqlConnection = new MySqlConnection(Secrets.CspDatabase);

                const string sqlQuery= "SELECT e.subject,e.content FROM csp.emailtemplates e  WHERE templateId= @p_templateId";

                using (var connection = mySqlConnection)
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);

                    command.Parameters.AddWithValue("p_templateId", templateId);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            var emailTemplate = new Model.EmailTemplate()
                            {
                                Content = Convert.ToString(dataReader["content"]),
                                Subject = Convert.ToString(dataReader["subject"])
                            };

                            return emailTemplate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new Model.EmailTemplate();
            }
            return new Model.EmailTemplate();
        }
    }
}
