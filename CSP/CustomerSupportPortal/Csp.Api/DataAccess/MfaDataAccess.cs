using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;

namespace Csp.Api.DataAccess
{
    public class MfaDataAccess: BaseDataAccess,IMfaDataAccess
    {
        #region Constants

        public const string EnableMfaForSupportAccount = "EnableMfaForSupportAccount";
        public const string UserAccountMfaAggregate = "UserAccountMfaAggregate";
        public const string UpdateMfaUserRegistration = "UpdateMfaUserRegistration";
        public const string PendingMfaRegistrations = "GetPendingMfaRegistrations";
        public const string IsMfaRequiredForUser = "IsMfaRequiredForUser";
        
        public const string UserAccountIdParam = "p_UserAccountId";
        public const string EnableMfaParam = "p_enableMfa";
        public const string RegistrationStatusParam = "p_RegistrationStatus";
        public const string StatusCodeParam = "p_StatusCode";
        public const string StatusMessageParam = "p_StatusMessage";
        public const string UpdatedByParam = "p_UpdatedBy";
        public const string IsRequiredParam = "p_IsRequired";
        public const string UserEmailParam = "p_UserEmail";
        public const string ApiParam = "p_Api";
        public const string RequestParam = "p_Request";
        public const string ResponseParam = "p_Response";
        public const string MultiFactorAuthTypeParam = "p_MultiFactorAuthType";
        public const string GoogleAuthSecretParam = "p_Secret";
        public const string UnenrollGoogleAuthParam = "p_unEnrollGoogleAuth";
        public const string ActionDate = "p_actionDate";
        public const string CreatedOn = "p_createdOn";

        public const string UserAccountId = "UserAccountId";
        public const string FirstName = "FirstName";
        public const string LastName = "LastName";
        public const string Email = "Email";
        public const string IsMfaEnabled = "IsMfaEnabled";
        public const string IsMfaRequired = "IsMfaRequired";
        
        
        #endregion

        /// <summary>
        /// Enable MFA at account level and queue the users for registration with Ping Identity 
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="enabledByUserAccountId"></param>
        /// <param name="isRequired"></param>
        public List<string> EnableMfaForAccount(long supportAccountId, long enabledByUserAccountId, bool isRequired)
        {
            var userEmails = new List<string>();
            using (var command = Connection.GetStoredProcCommand(EnableMfaForSupportAccount))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInCreatedByUserAccountIdParameter(command, enabledByUserAccountId);
                Connection.AddInParameter(command, IsRequiredParam, DbType.Boolean, isRequired);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var email = dataReader["UserEmail"].ToString();
                        userEmails.Add(email);
                    }
                }
            }

            return userEmails;
        }

        /// <summary>
        /// Get the aggregate of MFA status across all the user memberships
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool GetUserAccountMfaAggregate(long userAccountId, long supportAccountId)
        {
            bool mfaEnabled = false;

            using (var command = Connection.GetStoredProcCommand(UserAccountMfaAggregate))
            {
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        bool.TryParse(dataReader[IsMfaEnabled].ToString(), out mfaEnabled);
                    }
                }
            }

            return mfaEnabled;
        }

        /// <summary>
        /// Get all the pending user registrations for MFA
        /// </summary>
        /// <returns></returns>
        public List<UserAccount> GetPendingMfaRegistrations()
        {
            var users = new List<UserAccount>();
            using (var command = Connection.GetStoredProcCommand(PendingMfaRegistrations))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var user = new UserAccount
                        {
                            Id = Convert.ToInt64(dataReader[UserAccountId]),
                            FirstName = dataReader[FirstName].ToString(),
                            LastName = dataReader[LastName].ToString(),
                            Email = dataReader[Email].ToString(),
                            IsMfaEnabled = Convert.ToBoolean(dataReader[IsMfaEnabled])
                        };
                        users.Add(user);
                    }
                }
            }

            return users;
        }

        /// <summary>
        /// Updates the user registration status
        /// </summary>
        /// <param name="enableMfa"></param>
        /// <param name="registrationModel"></param>
        /// <returns></returns>
        public bool UpdateUserRegistration(bool enableMfa, MfaRegistrationModel registrationModel)
        {
            using (var command = Connection.GetStoredProcCommand(UpdateMfaUserRegistration))
            {
                Connection.AddInParameter(command, EnableMfaParam, DbType.Boolean, enableMfa);
                Connection.AddInParameter(command, UserAccountIdParam, DbType.Int64, registrationModel.UserAccountId);
                Connection.AddInParameter(command, RegistrationStatusParam, DbType.Int16, registrationModel.RegistrationStatus);
                Connection.AddInParameter(command, StatusCodeParam, DbType.String, registrationModel.StatusCode);
                Connection.AddInParameter(command, StatusMessageParam, DbType.String, registrationModel.StatusMessage);
                Connection.AddInParameter(command, UpdatedByParam, DbType.Int64, registrationModel.UpdatedByUserAccountId);
                Connection.AddInParameter(command, MultiFactorAuthTypeParam, DbType.Int16, registrationModel.MultiFactorAuthType);
                Connection.AddInParameter(command, GoogleAuthSecretParam, DbType.String, registrationModel.GoogleAuthenticatorSecret);
                Connection.AddInParameter(command, UnenrollGoogleAuthParam, DbType.Boolean, registrationModel.UnEnrollGoogleAuth);

                Connection.ExecuteNonQuery(command);

                return true;
            }
        }

        /// <summary>
        /// Logs all Ping Id API Calls
        /// </summary>
        /// <param name="email"></param>
        /// <param name="api"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void LogMfaApiCall(string email, string api, string request, string response)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("LogMfaApiCall"))
                {
                    Connection.AddInParameter(command, UserEmailParam, DbType.String, email);
                    Connection.AddInParameter(command, ApiParam, DbType.String, api);
                    Connection.AddInParameter(command, RequestParam, DbType.String, request);
                    Connection.AddInParameter(command, ResponseParam, DbType.String, response);

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        /// <summary>
        /// Checks if MFA is required for user so he cannot opt out
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public bool IsMfaRequiredforUser(string userEmail)
        {
            bool isMfaRequired = false;

            using (var command = Connection.GetStoredProcCommand(IsMfaRequiredForUser))
            {
                Connection.AddInEmailParameter(command, userEmail);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        isMfaRequired = Convert.ToBoolean(dataReader[IsMfaRequired]);
                    }
                }
            }

            return isMfaRequired;
        }

        public void LogMfa(MfaRegistrationModel registrationModel)
        {
            const string sqlQuery = "INSERT INTO csp.logmfa (UserAccountId, UserEmail, ActionDate, ActionDescription, MfaStatus, MfaType, CreatedOn, CreatedByUserAccountId) VALUES(@p_userAccountId, @p_userEmail, NOW(), 'Unenroll Google Authenticator', @p_RegistrationStatus, @p_MultiFactorAuthType, NOW(), @p_UpdatedBy);";

            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, UserAccountIdParam, DbType.Int64, registrationModel.UserAccountId);
                    Connection.AddInParameter(command, UserEmailParam, DbType.String, registrationModel.UserEmail);
                    Connection.AddInParameter(command, RegistrationStatusParam, DbType.Int16, registrationModel.RegistrationStatus);
                    Connection.AddInParameter(command, MultiFactorAuthTypeParam, DbType.Int16, registrationModel.MultiFactorAuthType);
                    Connection.AddInParameter(command, UpdatedByParam, DbType.Int64, registrationModel.UpdatedByUserAccountId);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
    }
}
