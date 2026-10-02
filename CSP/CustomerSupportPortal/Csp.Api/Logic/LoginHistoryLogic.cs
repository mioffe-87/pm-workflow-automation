using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System;

namespace Csp.Api.Logic
{
    public class LoginHistoryLogic : BaseLogic, ILoginHistoryLogic
    {
        private readonly IConnectionStrings _connectionStrings;
        private readonly IApplicationSettings _applicationSettings;

        public LoginHistoryLogic(IConnectionStrings connectionStrings, IApplicationSettings applicationSettings) {
            _connectionStrings = connectionStrings;
            _applicationSettings = applicationSettings;
        }

        public void AddLoginHistory(LoginHistoryModel loginHistory)
        {
            string connString = _connectionStrings.CspDatabase;
            string sessionTimeout = _applicationSettings.SessionTimeout;

            if (!string.IsNullOrEmpty(loginHistory.ImpersonateUser))
            {
                const string sqlUpdateQuery = "UPDATE loginhistory SET LogoutTime = ?p_LogoutTime, IsOffline = 1" +
                                        " WHERE IsOffline = 0 AND SessionId = ?p_SessionId AND UserAccountId = ?p_UserAccountId";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlUpdateQuery, connection);

                    command.Parameters.AddWithValue("p_LogoutTime", DateTime.Now);
                    command.Parameters.AddWithValue("p_SessionId", loginHistory.SessionId);
                    command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);

                    command.ExecuteNonQuery();
                }
            }
            else
            {
                const string sqlUpdateQuery = "UPDATE loginhistory SET IsOffline = 1" +
                                        " WHERE IsOffline = 0 AND DATE_ADD(LoginTime, INTERVAL ?p_SessionTimeout MINUTE) < NOW() AND UserAccountId = ?p_UserAccountId";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlUpdateQuery, connection);

                    command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);
                    command.Parameters.AddWithValue("p_SessionTimeout", sessionTimeout);
                    command.Parameters.AddWithValue("p_CurrentDateTime", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }

            const string sqlInsertQuery = "INSERT INTO loginhistory (SessionId, UserAccountId, SupportAccountId, LoginTime, LoginStatus, IsOffline, Browser, Platform, SourceIpAddress, ImpersonatorEmail, ImpersonateRequestId)" +
                "VALUES(?p_SessionId, ?p_UserAccountId, ?p_SupportAccountId, ?p_LoginTime, ?p_LoginStatus, ?p_IsOffline, ?p_Browser, ?p_Platform, ?p_SourceIpAddress, ?p_ImpersonateUser, ?p_ImpersonateRequestId)";

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlInsertQuery, connection);

                command.Parameters.AddWithValue("p_SessionId", loginHistory.SessionId);
                command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);
                command.Parameters.AddWithValue("p_SupportAccountId", loginHistory.SupportAccountId);
                command.Parameters.AddWithValue("p_LoginTime", DateTime.Now);
                command.Parameters.AddWithValue("p_LoginStatus", loginHistory.LoginStatus);
                command.Parameters.AddWithValue("p_IsOffline", loginHistory.IsOffline);
                command.Parameters.AddWithValue("p_Browser", loginHistory.Browser);
                command.Parameters.AddWithValue("p_Platform", loginHistory.Platform);
                command.Parameters.AddWithValue("p_SourceIpAddress", loginHistory.SourceIpAddress);
                command.Parameters.AddWithValue("p_ImpersonateUser", loginHistory.ImpersonateUser);
                command.Parameters.AddWithValue("p_ImpersonateRequestId", loginHistory.ImpersonateRequestId);
                command.ExecuteNonQuery();
            }
        }

        public void AddLoginUserHistory(LoginHistoryModel loginHistory)
        {
            if (loginHistory.UserAccountId.HasValue && loginHistory?.UserAccountId > 0)
            {
                string connString = _connectionStrings.CspDatabase;
                const string sqlInsertQuery = "INSERT INTO loginuserhistory (SessionId, UserAccountId, SupportAccountId, LoginTime, Browser, Platform, SourceIpAddress) " +
                "SELECT ?p_SessionId, ?p_UserAccountId, ?p_SupportAccountId, ?p_LoginTime, ?p_Browser, ?p_Platform, ?p_SourceIpAddress " +
                "FROM csp.useraccount u WHERE u.UserAccountId = ?p_UserAccountId AND u.IsActive";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlInsertQuery, connection);

                    command.Parameters.AddWithValue("p_SessionId", loginHistory.SessionId);
                    command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);
                    command.Parameters.AddWithValue("p_SupportAccountId", loginHistory.SupportAccountId);
                    command.Parameters.AddWithValue("p_LoginTime", DateTime.Now);
                    command.Parameters.AddWithValue("p_IsOffline", loginHistory.IsOffline);
                    command.Parameters.AddWithValue("p_Browser", loginHistory.Browser);
                    command.Parameters.AddWithValue("p_Platform", loginHistory.Platform);
                    command.Parameters.AddWithValue("p_SourceIpAddress", loginHistory.SourceIpAddress);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLoginHistory(LoginHistoryModel loginHistory)
        {
            const string sqlQuery = "UPDATE loginhistory SET LogoutTime = ?p_LogoutTime, IsOffline = 1" +
                                    " WHERE IsOffline = 0 AND SessionId = ?p_SessionId AND UserAccountId = ?p_UserAccountId";
            string connString = _connectionStrings.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("p_LogoutTime", DateTime.Now);
                command.Parameters.AddWithValue("p_SessionId", loginHistory.SessionId);
                command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);
                //command.Parameters.AddWithValue("p_SupportAccountId", loginHistory.SupportAccountId);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateLoginUserHistory(LoginHistoryModel loginHistory)
        {
            const string sqlQuery = "UPDATE loginuserhistory SET LogoutTime = ?p_LogoutTime" +
                                    " WHERE SessionId = ?p_SessionId AND UserAccountId = ?p_UserAccountId";
            string connString = _connectionStrings.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("p_LogoutTime", DateTime.Now);
                command.Parameters.AddWithValue("p_SessionId", loginHistory.SessionId);
                command.Parameters.AddWithValue("p_UserAccountId", loginHistory.UserAccountId);
                command.ExecuteNonQuery();
            }
        }

        public LoginHistoryModel GetLoginHistory(string sessionId)
        {
            const string sqlQuery = "SELECT * FROM loginhistory WHERE SessionId = ?p_SessionId";
            string connString = _connectionStrings.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("p_SessionId", sessionId);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new LoginHistoryModel()
                        {
                            SessionId = Convert.ToString(dataReader["SessionId"]),
                            SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                            UserAccountId = Convert.ToInt64(dataReader["UserAccountId"]),
                            ImpersonateUser = Convert.ToString(dataReader["ImpersonatorEmail"]),
                            ImpersonateRequestId = Convert.ToString(dataReader["ImpersonateRequestId"]),
                            SourceIpAddress = Convert.ToString(dataReader["ImpersonateRequestId"]),
                            LoginTime = Convert.ToDateTime(dataReader["LoginTime"]),
                            LogoutTimeAsString = Convert.ToString(dataReader["LogoutTime"]),
                            LoginStatus = Convert.ToString(dataReader["LoginStatus"]),
                        };
                    }
                }
            }
            return new LoginHistoryModel();
        }

        public LoginHistoryModel GetLoginUserHistory(string sessionId)
        {
            const string sqlQuery = "SELECT * FROM loginuserhistory WHERE SessionId = ?p_SessionId AND LogoutTime IS NULL";
            string connString = _connectionStrings.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("p_SessionId", sessionId);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        return new LoginHistoryModel()
                        {
                            SessionId = Convert.ToString(dataReader["SessionId"]),
                            SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                            UserAccountId = Convert.ToInt64(dataReader["UserAccountId"]),
                            LoginTime = Convert.ToDateTime(dataReader["LoginTime"]),
                            LogoutTimeAsString = Convert.ToString(dataReader["LogoutTime"]),
                        };
                    }
                }
            }
            return new LoginHistoryModel();
        }
    }
}
