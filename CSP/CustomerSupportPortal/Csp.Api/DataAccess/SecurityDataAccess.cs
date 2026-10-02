using Csp.Api.Model;
using System.Collections.Generic;
using Csp.Api.DataAccess.HelperClasses;
using System.Data;
using Csp.Api.Security;
using System;
using Csp.Api.HelperClasses;

namespace Csp.Api.DataAccess {

	public class SecurityDataAccess : BaseDataAccess {

		#region Public Methods

		/// <summary>
		/// This Method is used for Get Support Account Permissions ForPrincipal by using supportAccountId,userAccountId
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="userAccount"></param>
		/// <returns>HashSet</returns>
		public HashSet<Permission> GetSupportAccountPermissionsForPrincipal(long supportAccountId, UserAccount userAccount) {
			HashSet<Permission> permissionSet = new HashSet<Permission>();
			using (var command = Connection.GetStoredProcCommand("GetSupportAccountPermissionsForPrincipal")) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInUserAccountIdParameter(command, userAccount.Id);

				using (var dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {

						permissionSet.Add(((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)));
					}
				}
			}

            // Comments By LK On 08/05/2018
            // Combine stored procedures to avoid extra multiple database connections
			//if (HasViewCustomerSupportAccountsPrivilege(supportAccountId, userAccount.Id)) {
			//	permissionSet.Add(Permission.ViewCustomerSupportAccounts);
			//}

			//if (HasViewAdminSitePrivilege(userAccount.Email)) {
			//	permissionSet.Add(Permission.ViewAdminSite);
			//}

			return permissionSet;
		}


		/// <summary>
		/// This Method is used for  Get Group Permissions For Principal  based on groupId,userAccountId
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="userAccount">userAccount</param>
		/// <returns>HashSet</returns>
		public HashSet<Permission> GetGroupPermissionsForPrincipal(long groupId, UserAccount userAccount) {
			var permissionSet = new HashSet<Permission>();

			using (var command = Connection.GetStoredProcCommand("GetGroupPermissionsForPrincipal")) {

				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInUserAccountIdParameter(command, userAccount.Id);

				using (var dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {

						permissionSet.Add(
							((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)));
					}
				}

				if (HasViewAdminSitePrivilege(userAccount.Email)) {
					permissionSet.Add(Permission.ViewAdminSite);
				}
			}

			return permissionSet;
		}

		/// <summary>
		/// This method is user to validate user sas view customer support accounts privilege.
		/// </summary>
		/// <param name="supportAccountId">Support Account Id</param>
		/// <param name="userAccountId">User Account Id</param>
		/// <returns>Boolean</returns>
		private bool HasViewCustomerSupportAccountsPrivilege(long supportAccountId, long userAccountId) {
			string sqlString = string.Format("SELECT HasViewCustomerSupportAccountsPrivilege({0}, {1});",
																			 supportAccountId, userAccountId);

			using (var command = Connection.GetSqlStringCommand(sqlString)) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInUserAccountIdParameter(command, userAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {
						return (bool)dataReader[0];
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Method to validate Admin user Privilege
		/// </summary>
		/// <param name="userEmail">User Email</param>
		/// <returns>Boolean</returns>
		private bool HasViewAdminSitePrivilege(string userEmail) {

			userEmail = userEmail.Replace("'", "''");
			string sqlString = "SELECT id FROM adminusers WHERE Email = @p_Email;";

			using (var command = PANDBConnection.GetSqlStringCommand(sqlString)) {

				PANDBConnection.AddInParameter(command, "@p_Email", DbType.String, userEmail);

				if (PANDBConnection.ExecuteScalar(command) != null) {
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Checks if user has access to BPA in the current support account
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
	    public bool HasBpaAccess(long supportAccountId, long userAccountId)
        {
            var hasBPAAccess = false;

            using (var command = Connection.GetStoredProcCommand("GetBPAAuthorization"))
	        {

                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInUserAccountIdParameter(command, userAccountId);

	            using (var dataReader = Connection.ExecuteReader(command))
	            {
	                while (dataReader.Read())
	                {
	                    hasBPAAccess = Convert.ToBoolean(dataReader["HasBPAAccess"]);
	                }
	            }
	        }

	        return hasBPAAccess;
	    }


        public bool IsSuperUser(long supportAccountId, long userAccountId)
	    {
            var isSuperUser = false;

            try
            {
                using (var command = Connection.GetStoredProcCommand("IsSuperUserBySupportaccountId"))
                {

                    Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                    Connection.AddInUserAccountIdParameter(command, userAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            isSuperUser = Convert.ToBoolean(dataReader["IsSuperUser"]);
                        }
                    }
                }

                return isSuperUser;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
	    }

        public long LogApiCall(LogApiCall logApiCall)
        {
            const string sqlQuery = "INSERT INTO csp.logapicall(Application,ApiCalled,ApiMethod,RequestPayload,CreatedBy,CreatedOn) VALUES(@p_Application,@p_ApiCalled,@p_ApiMethod,@p_RequestPayload,@p_CreatedBy,@p_CreatedOn);SELECT LAST_INSERT_ID(); ";
           
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command,"p_Application", DbType.String, ApiConstants.CSP_APPLICATION);
                Connection.AddInParameter(command,"p_ApiCalled", DbType.String,logApiCall.ApiCalled);
                Connection.AddInParameter(command,"p_ApiMethod", DbType.String, logApiCall.ApiMethod);
                Connection.AddInParameter(command,"p_RequestPayload", DbType.String,logApiCall.RequestPayload);
                Connection.AddInParameter(command,"p_CreatedBy", DbType.String, logApiCall.CreatedBy);
                Connection.AddInParameter(command,"p_CreatedOn", DbType.DateTime, DateTime.Now);
                var result = Connection.ExecuteScalar(command);
                return Convert.ToInt64(result);
            }
        }

        public void UpdateLogApi(LogApiCall logApiCall)
        {
            const string sqlQuery = "Update csp.logapicall set ResponseStatus=@p_ResponseStatus, ResponsePayload=@p_ResponsePayload Where LogApiCallId=@p_LogApiCallId;";
            
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "p_ResponseStatus", DbType.String, logApiCall.ResponseStatus);
                Connection.AddInParameter(command, "p_ResponsePayload", DbType.String, logApiCall.ResponsePayload);
                Connection.AddInParameter(command, "p_LogApiCallId", DbType.Int64, logApiCall.LogApiCallId);
                Connection.ExecuteNonQuery(command);
            }
        }

        public bool IsUserExist(string userEmail)
        {
            string sqlString = "SELECT * FROM useraccount WHERE Email = @p_Email;";
            using (var command = Connection.GetSqlStringCommand(sqlString))
            {
                Connection.AddInParameter(command, "@p_Email", DbType.String, userEmail);
                if (Connection.ExecuteScalar(command) != null)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}


