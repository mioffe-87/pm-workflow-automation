using MySql.Data.MySqlClient;
using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Csp.Api.Services.Contracts;
using Csp.Api.HelperClasses;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class ServiceDataAccess : BaseDataAccess, IServiceDataAccess
    {
	    public List<AlertModel> GetAlertsData(bool isPublic)
			{
				var alertMessages = new List<AlertModel>();
                var scope = isPublic ? string.Format(@"{0},{1}", (int)Scope.UnAuthenticated, (int)Scope.Both) : string.Format(@"{0},{1}", (int)Scope.Authenticated, (int)Scope.Both);
                string sqlQuery = string.Format("SELECT MessageDetails, MessageHeader, Severity FROM alertmessages WHERE Active = 1 AND Scope IN ({0}) ORDER BY LastUpdate DESC LIMIT 5", scope);
                string connString = Secrets.CspDatabase;

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var alertMessage = new AlertModel
                            {
                                Detail = dataReader["MessageDetails"].ToString(),
                                Header = dataReader["MessageHeader"].ToString(),
                                Severity = (Severity)dataReader["Severity"]
                            };
                            alertMessages.Add(alertMessage);
                        }
                    }
                }
                return alertMessages;
			}

        public List<ConfigModel> GetConfigLevels()
        {
            var configLevelsList = new List<ConfigModel>();
            const string sqlQuery = "SELECT LevelId, LevelName FROM onlineservicesconfig ORDER BY LevelId ASC";
            string connString = Secrets.CspDatabase;
            ConfigModel configModel = null;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        configModel = new ConfigModel();

                        configModel.LevelId = dataReader["LevelId"] == null || dataReader["LevelId"] is DBNull ? 0 : Convert.ToInt16(dataReader["LevelId"].ToString());
                        configModel.LevelName = dataReader["LevelName"] == null || dataReader["LevelName"] is DBNull ? null : dataReader["LevelName"].ToString();

                        configLevelsList.Add(configModel);
                    }
                }
            }

            return configLevelsList;
        }

        public List<ConfigModel> GetConfigSubLevels(int levelid)
        {
            var configLevelsList = new List<ConfigModel>();
			const string sqlQuery = "SELECT SubLevelId, SublevelName ,LevelId FROM onlineservicessubconfig ORDER BY LevelId ASC";
            string connString = Secrets.CspDatabase;
            ConfigModel configModel = null;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);
                command.Parameters.Add("@levelId", MySqlDbType.Int16).Value = levelid;

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        configModel = new ConfigModel();

                        configModel.LevelId = dataReader["SubLevelId"] == null || dataReader["SubLevelId"] is DBNull ? 0 : Convert.ToInt32(dataReader["SubLevelId"].ToString());
                        configModel.LevelName = dataReader["SublevelName"] == null || dataReader["SublevelName"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["SublevelName"].ToString());
						configModel.ParentLevelId = dataReader["LevelId"] == null || dataReader["LevelId"] is DBNull ? 0 :Convert.ToInt32(dataReader["LevelId"].ToString());
                        configLevelsList.Add(configModel);
                    }
                }
            }

            return configLevelsList;
        }

        public void CaptureBPA(string logId, string userEmail, string serialNumber)
        {
            string connString = Secrets.CspDatabase;
            string sqlText =
                    "INSERT INTO logbpa (LogId, Email, SerialNumber) VALUES (@logId, @email, @serialNumber)";

            using (var connection = new MySqlConnection(connString))
            {
                try
                {
                    connection.Open();

                    var command = new MySqlCommand(sqlText, connection);
                    command.Parameters.Add("@logId", MySqlDbType.String).Value = logId;
                    command.Parameters.Add("@email", MySqlDbType.String).Value = userEmail;
                    command.Parameters.Add("@serialNumber", MySqlDbType.String).Value = serialNumber;

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
        }
    }
}
