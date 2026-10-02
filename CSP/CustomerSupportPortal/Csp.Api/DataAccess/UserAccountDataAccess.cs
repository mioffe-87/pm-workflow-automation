using System;
using Csp.Api.Model;
using System.Collections.Generic;
using Csp.Api.DataAccess.HelperClasses;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Csp.Api.Exceptions;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Api.HelperClasses;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Csp.Api.Model.Enums;

namespace Csp.Api.DataAccess {

	public class UserAccountDataAccess : BaseDataAccess {

		#region Public Methods


		/// <summary>
		/// This Method is used for creating User Account
		/// </summary>
		/// <param name="userAccount">user Account</param>
		/// <returns>UserAccount</returns>
		public UserAccount Create(UserAccount userAccount) {

			var resultUserAccount = new UserAccount();
            if (userAccount.RegisteredAppId == 0)
            {
                userAccount.RegisteredAppId = Convert.ToInt64(RegisteredApp.CSP);
            }
            using (var command = Connection.GetStoredProcCommand("CreateUserAccount")) {
				Connection.AddInModelParameters(command, userAccount, CreateOrUpdate.Create);

				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {

						resultUserAccount.PopulateProperties(dataReader);
					}
				}
			}

			return resultUserAccount;
		}


		/// <summary>
		/// This Method is uesd to retrive User account Details based on userAccountId
		/// </summary>
		/// <param name="userAccountId">userAccount Id</param>
		/// <returns>UserAccount</returns>
		public UserAccount Read(long userAccountId, long restrictedByUserAccountId) {

			var userAccount = new UserAccount();

			using (var command = Connection.GetStoredProcCommand("GetUserAccountByUserAccountId")) {
				Connection.AddInUserAccountIdParameter(command, userAccountId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {

						userAccount.PopulateProperties(dataReader);
					}
				}
			}

			//Read address for user Account need to implement

			return userAccount;
		}


		/// <summary>
		/// This Method is uesd to retrive User account Details based on userAccountId
		/// </summary>
		/// <param name="email">Email</param>
		/// <returns>UserAccount</returns>
		public UserAccount ReadUserByEmailId(string email) {

			var userAccount = new UserAccount();

			using (var command = Connection.GetStoredProcCommand("GetUserAccountByUserEmailId")) {
                Connection.AddInEmailParameter(command, email);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {

						userAccount.PopulateProperties(dataReader);
					}
				}
			}

			return userAccount.Id <= 0 ? null : userAccount;
		}


		/// <summary>
		///This Method is used to Update User Account Details
		/// </summary>
		/// <param name="userAccount">userAccount</param>
		/// <returns>UserAccount</returns>
		public UserAccount Update(UserAccount userAccount, long restrictedByUserAccountId) {

			var localUserAccount = new UserAccount();

			using (var command = Connection.GetStoredProcCommand("UpdateUserAccount")) {
				Connection.AddInModelParameters(command, userAccount, CreateOrUpdate.Update);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);

				try {
					using (var dataReader = Connection.ExecuteReader(command)) {


						if (dataReader.Read()) {

							localUserAccount.PopulateProperties(dataReader);
						}
					}
				} catch (MySqlException exception) {
					if (exception.Message.Contains("Concurrent update error")) {

						throw new CspNotificationException("A user has already updated this User Account. Please refresh your data, review the changes made and try again");
					} else {
						throw;
					}

				}
			}

			return localUserAccount;
		}

		public bool IsDisplayNameExists(string displayName, long id) {
			var isDisplayNameExists = false;
			var sqlQuery = "SELECT DisplayName FROM UserAccount WHERE LOWER(DisplayName) = LOWER(@p_DisplayName)";

			if (id != 0) {
				sqlQuery = "SELECT DisplayName FROM UserAccount WHERE LOWER(DisplayName) = LOWER(@p_DisplayName) AND UserAccountId != @p_Id;";
			};

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_DisplayName", DbType.String, displayName);

				if (id != 0) {
					Connection.AddInParameter(command, "@p_Id", DbType.Int64, id);
				}

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						isDisplayNameExists = true;
					}
				}
			}

			return isDisplayNameExists;
		}

		public bool IsEmailExists(string email) {
			var isEmailExists = false;

			const string sqlQuery = "SELECT Email FROM UserAccount WHERE LOWER(Email) = LOWER(@p_Email);";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_Email", DbType.String, email);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						isEmailExists = true;
					}
				}
			}

			return isEmailExists;
		}

        public void UpdatePANDBDeviceMaster(string serialNumber, long supportAccountId)
        {

            const string updateSqlQuey = "update device set supportaccountid = @p_SupportAccountId WHERE serialnumber = @p_SerialNumber;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey))
            {
                PANDBConnection.AddInParameter(panCommand, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                PANDBConnection.AddInParameter(panCommand, "@p_SerialNumber", DbType.String, serialNumber);

                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

		public List<object> ReadPendingMembership() {

			List<object> resultList = new List<object>();

			string sqlQuery = "SELECT MembershipId, M.UserAccountId, U.FirstName, U.LastName, M.SupportAccountId, S.Name as SupportAccountName, MembershipResponseGUID ";
			sqlQuery += "FROM csp.membership M ";
			sqlQuery += "inner join csp.useraccount U on U.UserAccountId = M.UserAccountId ";
			sqlQuery += "inner join csp.supportAccount S on S.SupportAccountId = M.SupportAccountId ";
			sqlQuery += "where MembershipStatus = 1; ";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {

				using (var dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {
						var resultDictionay = new Dictionary<string, string>();
						resultDictionay.Add("MembershipId", dataReader["MembershipId"].ToString());
						resultDictionay.Add("UserAccountId", dataReader["UserAccountId"].ToString());
						resultDictionay.Add("FirstName", dataReader["FirstName"].ToString());
						resultDictionay.Add("LastName", dataReader["LastName"].ToString());
						resultDictionay.Add("SupportAccountId", dataReader["SupportAccountId"].ToString());
						resultDictionay.Add("SupportAccountName", dataReader["SupportAccountName"].ToString());
						resultDictionay.Add("MembershipResponseGUID", dataReader["MembershipResponseGUID"].ToString());
						resultList.Add(resultDictionay);
					}
				}
			}

			return resultList;
		}

		#endregion


		public bool UpdateSupportAccountForUser(long uid, long supId)
		{

			string sqlQuery =
				"Update UserAccount set DefaultSupportAccountId=@supportAccountId where UserAccountId=@userAccountId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "supportAccountId", DbType.Int64, supId);
				Connection.AddInParameter(command, "userAccountId", DbType.Int64, uid);
				return Connection.ExecuteNonQuery(command) > 0;
			}
		}

		public bool UpdateSalesforceInfoForMembership(MembershipModel membershipModel) {

			string sqlQuery =
				"Update Membership set SalesforceContactId=@salesforceContactId,SalesforceLogInId=@salesforceLogInId,JiveId=@jiveId,LastUpdate=@lastUpdate,LastUpdateByUserAccountId=@lastUpdateByUserAccountId where MembershipId=@membershipId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "salesforceContactId", DbType.String, membershipModel.SalesforceContactId);
				Connection.AddInParameter(command, "salesforceLogInId", DbType.String, membershipModel.SalesforceLogInId);
				Connection.AddInParameter(command, "jiveId", DbType.String, membershipModel.JiveId);
				Connection.AddInParameter(command, "lastUpdate", DbType.DateTime, DateTime.Now);
				Connection.AddInParameter(command, "lastUpdateByUserAccountId", DbType.Int64, membershipModel.LastUpdateByUserAccountId);
				Connection.AddInParameter(command, "membershipId", DbType.Int64, membershipModel.MembershipId);
				return Connection.ExecuteNonQuery(command) > 0;
			}
		}

		public bool SetJiveId(long userAccountId, string jiveId)
		{
			string sqlQuery =
				"Update UserAccount set jiveid=@jiveId where UserAccountId=@userAccountId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "jiveId", DbType.String, jiveId);
				Connection.AddInParameter(command, "userAccountId", DbType.Int64, userAccountId);
				return Connection.ExecuteNonQuery(command) > 0;
			}
		}

		public bool SetSelectedSalesforceData(long userAccountId, SelectedSalesForceData data) {
			string sqlQuery =
				"Update UserAccount set SelectedMembershipId=@selectedMembershipId, SelectedSalesForceAccountId=@selectedSalesForceAccountId,SelectedSalesForceContactId=@selectedSalesForceContactId,SelectedSalesForceLoginId=@selectedSalesForceLoginId where UserAccountId=@userAccountId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "selectedMembershipId", DbType.Int64, data.MembershipId);
				Connection.AddInParameter(command, "selectedSalesForceAccountId", DbType.String, data.SalesForceAccountId);
				Connection.AddInParameter(command, "selectedSalesForceContactId", DbType.String, data.SalesForceContactId);
				Connection.AddInParameter(command, "selectedSalesForceLoginId", DbType.String, data.SalesForceLoginId);
				Connection.AddInParameter(command, "userAccountId", DbType.Int64, userAccountId);
				return Connection.ExecuteNonQuery(command) > 0;
			}
		}

		public UserSalesforceInfo GetSalesforceInfo(long userAccountId)
		{
			UserSalesforceInfo userSalesforceInfo = new UserSalesforceInfo();

			const string sqlQuery = "SELECT SelectedMembershipId,SelectedSalesForceAccountId,SelectedSalesForceContactId,SelectedSalesForceLoginId FROM UserAccount WHERE UserAccountId=@userAccountId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "userAccountId", DbType.Int64, userAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read())
					{
						userSalesforceInfo.SelectedMembershipId = dataReader["SelectedMembershipId"] == null || dataReader["SelectedMembershipId"] is DBNull ? (Int64?)null : Convert.ToInt64(dataReader["SelectedMembershipId"]);
						userSalesforceInfo.SelectedSalesForceAccountId = dataReader["SelectedSalesForceAccountId"] == null || dataReader["SelectedSalesForceAccountId"] is DBNull ? null : dataReader["SelectedSalesForceAccountId"].ToString();
						userSalesforceInfo.SelectedSalesForceContactId = dataReader["SelectedSalesForceContactId"] == null || dataReader["SelectedSalesForceContactId"] is DBNull ? null : dataReader["SelectedSalesForceContactId"].ToString();
						userSalesforceInfo.SelectedSalesForceLoginId = dataReader["SelectedSalesForceLoginId"] == null || dataReader["SelectedSalesForceLoginId"] is DBNull ? null : dataReader["SelectedSalesForceLoginId"].ToString();

					}
				}
			}
			return userSalesforceInfo;
		}

		public SelectedSalesForceData GetSalesforceInfoByMembershipId(long membershipId) {
			SelectedSalesForceData userSalesforceInfo = new SelectedSalesForceData();
			long? groupId = null; 
			const string sqlQuery = "SELECT A.SalesforceContactId,A.SalesforceLogInId,A.GroupId, B.SalesforceAccountId FROM membership A left outer join supportaccount B on A.SupportAccountId=B.SupportAccountId WHERE A.MembershipId=@membershipId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "membershipId", DbType.Int64, membershipId);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						userSalesforceInfo.MembershipId = membershipId;
						userSalesforceInfo.SalesForceAccountId = (dataReader["SalesforceAccountId"] == null || dataReader["SalesforceAccountId"] is DBNull) ? null : dataReader["SalesforceAccountId"].ToString();
						userSalesforceInfo.SalesForceContactId = (dataReader["SalesforceContactId"] == null || dataReader["SalesforceContactId"] is DBNull) ? null : dataReader["SalesforceContactId"].ToString();
						userSalesforceInfo.SalesForceLoginId = (dataReader["SalesforceLogInId"] == null || dataReader["SalesforceLogInId"] is DBNull) ? null : dataReader["SalesforceLogInId"].ToString();
						groupId = (dataReader["GroupId"] == null || dataReader["GroupId"] is DBNull) ? (Int64?)null : Convert.ToInt64(dataReader["GroupId"]);
					}
				}

				if (userSalesforceInfo.SalesForceAccountId == null && groupId.HasValue)
				{
					string sfdcQuery = "Select B.SalesforceAccountId from supportaccountgroup A  inner join  supportaccount B on A.SupportAccountId=B.SupportAccountId where A.GroupId = @groupId";
					using (var sfdccommand = Connection.GetSqlStringCommand(sfdcQuery))
					{
						Connection.AddInParameter(command, "groupId", DbType.Int64, groupId.Value);

						using (var dataReader = Connection.ExecuteReader(command))
						{
							if (dataReader.Read())
							{
								userSalesforceInfo.SalesForceAccountId = dataReader["SalesforceAccountId"] == null
									? null
									: dataReader["SalesforceAccountId"].ToString();
							}
						}
					}
				}

			}
			return userSalesforceInfo;
		}

        public void BackupCSODAccountByEmail(string email)
        {
            string reason = "UserType updated to PART: User is attached with Partner accounts in SF";
            string whoby = "CSP Registration";
            string sqlQuery = "CALL cornerstone.uspBackupAccountByEmail('" + email + "','" + reason + "','" + whoby + "')";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.ExecuteNonQuery(command);
            }
        }

        public void UpdatePartnerInfoCsod(string email)
        {
            const string sqlQuery = @"
            UPDATE cornerstone.accounts a
                SET a.usertype = 'PART',
                a.updatedby = 'CSP Registration'
            WHERE email = @p_email";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_email", DbType.String, email);
                Connection.ExecuteNonQuery(command);
            }
        }

		public long GetDefaultSupportAccountByUser(long userAccountId)
		{
			long supportAccountId = 0;
			const string selectSqlQuey = "SELECT u.DefaultSupportAccountId FROM useraccount u WHERE u.UserAccountId= @p_UserAccountId;";


			using (var cspCommand = Connection.GetSqlStringCommand(selectSqlQuey)) {
				Connection.AddInParameter(cspCommand, "@p_UserAccountId", DbType.Int64, userAccountId);

				using (var dataReader = Connection.ExecuteReader(cspCommand)) {
					if (dataReader.Read()) {
                        string defaultcspId = Convert.ToString(dataReader["DefaultSupportAccountId"]);
                        if(!string.IsNullOrEmpty(defaultcspId)) {
                            supportAccountId = Convert.ToInt64(dataReader["DefaultSupportAccountId"]);
                        }
						
					}
				}
				Connection.ExecuteNonQuery(cspCommand);
			}

			return supportAccountId;
		}

        public NewUserDetails GetNewUserCreationDetails(long userAccountId, long supportAccountId, long membershipId)
        {
            NewUserDetails newUser = null;
            using (var command = Connection.GetStoredProcCommand("GetNewUserDetails"))
	        {
	            Connection.AddInUserAccountIdParameter(command, userAccountId);
	            Connection.AddInSupportAccountIdParameter(command, supportAccountId);
	            Connection.AddInMembershipIdParameter(command, membershipId);

	            using (var dataReader = Connection.ExecuteReader(command))
	            {
	                while (dataReader.Read())
	                {
                        newUser = new NewUserDetails
	                            {
	                                Id = Convert.ToInt64(dataReader["UserAccountId"]),
                                    CreatedByUserName = dataReader["CreatedByUser"].ToString(),
	                                CreatedByUserEmail = dataReader["CreatedByUserEmail"].ToString(),
                                    CreatedOn = Convert.ToDateTime(dataReader["CreatedOn"]),
	                                RolesAssigned = dataReader["Roles"].ToString(),
                                    GroupName = dataReader["GroupName"].ToString()
	                            };
	                }
	            }
	        }

            return newUser;
	    }

        public List<LicensingApi> GetSupportAccountsByLicensingApiKeyExpiration()
        {
            List<LicensingApi> supportAccountList = new List<LicensingApi>();
            using (var command = Connection.GetStoredProcCommand("GetSupportAccountsByLicensingKeyExpiration"))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        supportAccountList.Add(
                             new LicensingApi()
                         {
                             SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                             ExpirationDate = Convert.ToDateTime(dataReader["ExpirationDate"]),
                             Days = Convert.ToInt64(dataReader["Days"])
                         });
                    }
                }
            }

            return supportAccountList;
        }

        #region Transfer of Tenants - Move users

        public List<UserAccountTransferDetails> GetUsersPendingForTransfer()
        {
            var users = new List<UserAccountTransferDetails>();

            using (var command = Connection.GetStoredProcCommand("GetUsersPendingForTransfer"))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var user = new UserAccountTransferDetails
                        {
                            TransferId = Convert.ToInt64(dataReader["CloudServiceUserTransferId"]),
                            UserAccountId = Convert.ToInt64(dataReader["UserAccountId"]),
                            FirstName = dataReader["FirstName"].ToString(),
                            LastName = dataReader["LastName"].ToString(),
                            Email = dataReader["Email"].ToString(),
                            SourceSupportAccountId = Convert.ToInt64(dataReader["SourceSupportAccountId"]),
                            SourceSupportAccountName = dataReader["SourceSupportAccountName"].ToString(),
                            DestinationSupportAccountId = Convert.ToInt64(dataReader["DestinationSupportAccountId"]),
                            DestinationSupportAccountName = dataReader["DestinationSupportAccountName"].ToString(),
                            IsNewMember = Convert.ToBoolean(dataReader["IsNewMember"]),
                            CloudService = dataReader["CloudService"].ToString(),
                            SerialNumber = dataReader["SerialNumber"].ToString(),
                            TenantId = Convert.ToInt64(dataReader["TenantId"]),
                            CreatedByUserAccountId = Convert.ToInt64(dataReader["CreatedByUserAccountId"]),
                            CreatedByUserEmail = dataReader["CreatedByUserEmail"].ToString()
                        };

                        users.Add(user);
                    }
                }
            }

            return users;
        }

        // TECH DEBT: cleanup after testing complete of CSP-1871 Create RBAC roles on the hub from CSP - add mapping
        /*
        public bool AddRbacRole(string userEmail, long supportAccountId, string role, string byEmail)
        {
            var isRbacRoleCreated = false;

            using (var command = Connection.GetStoredProcCommand("RBAC_AddRoleMapping"))
            {
                Connection.AddInParameter(command, "p_email", DbType.String, userEmail);
                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "p_roles", DbType.String, role);
                Connection.AddInParameter(command, "p_ByEmail", DbType.String, byEmail);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        isRbacRoleCreated = string.IsNullOrEmpty(dataReader["Message"].ToString());
                    }
                }
            }

            return isRbacRoleCreated;
        }
        */

        public IEnumerable<RbackUserRole> GetRbacRolesForUser(string email, long supportAccountId)
	    {
            RbackUserRole[] rbacRoles = null;
            // TECH DEBT: cleanup after testing complete of CSP-1871 Create RBAC roles on the hub from CSP - add mapping
            /*
            using (var command = Connection.GetStoredProcCommand("RBAC_GetUserRole"))
	        {
                Connection.AddInParameter(command, "p_email", DbType.String, email);
	            Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);

	            using (var dataReader = Connection.ExecuteReader(command))
	            {
	                while (dataReader.Read())
	                {
                        rbacRoles.Add(dataReader["rolename"].ToString());
	                }
	            }
	        }
            */
            bool isPanServiceBasicAuthEnabled = ConfigurationManager.AppSettings["PanServicesIsBasicAuthEnabled"].ToUpper() == "TRUE";
            string panServicesCertThumbPrint = (isPanServiceBasicAuthEnabled) ? string.Empty : ConfigurationManager.AppSettings["PanServicesCertThumbprint"];
            string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
            string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];
            var rbacRoleProxyUrl = ConfigurationManager.AppSettings["RbacRoleProxy"]
                                   + "GetHub?supportAccountId=" + supportAccountId
                                   + (string.IsNullOrWhiteSpace(email)
	                                   ? string.Empty
	                                   : "&email=" + email);

            var httpResponse = (string.IsNullOrEmpty(panServicesCertThumbPrint)) ? WebCommon.GetWebRequest(rbacRoleProxyUrl, "GET", userName: panServicesUserName, userKey: panServicesUserKey).GetResponse() : 
							WebCommon.GetWebRequestWithClientCertificate(rbacRoleProxyUrl, "GET", panServicesCertThumbPrint, "").GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                rbacRoles = JsonConvert.DeserializeObject<RbackUserRole[]>(result, new JsonSerializerSettings()
                {
                    MaxDepth = 32,
                    TypeNameHandling = TypeNameHandling.None
                });
            }
            return rbacRoles;
        }

        public void RegisterAdminRbacRoleForUser(string email, long supportAccountId)
        {
                bool isPanServiceBasicAuthEnabled = ConfigurationManager.AppSettings["PanServicesIsBasicAuthEnabled"].ToUpper() == "TRUE";
                string panServicesCertThumbPrint = (isPanServiceBasicAuthEnabled) ? string.Empty : ConfigurationManager.AppSettings["PanServicesCertThumbprint"];
                string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
                string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];
            var rbacRoleProxyUrl = ConfigurationManager.AppSettings["RbacRoleProxy"] + "PostHub?supportAccountId=" +
		                               supportAccountId;
		        var jsonPayload = JsonConvert.SerializeObject(new
		        {
			        email = email,
			        type = "special_predefined",
			        csp_account_id = supportAccountId.ToString(),
			        role = "account_superuser"
		        });

			      var httpResponse = (string.IsNullOrEmpty(panServicesCertThumbPrint)) ? WebCommon.GetWebRequest(rbacRoleProxyUrl, "POST", jsonPayload: jsonPayload, userName: panServicesUserName, userKey: panServicesUserKey).GetResponse() :
		         WebCommon.GetWebRequestWithClientCertificate(rbacRoleProxyUrl,
				        "POST", panServicesCertThumbPrint, jsonPayload).GetResponse();

		        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        // TECH DEBT: cleanup after testing complete of CSP-1871 Create RBAC roles on the hub from CSP - add mapping
        /*
	    public bool DeleteRbacRoleMapping(string userEmail, long supportAccountId, string role, string byEmail)
	    {
	        var isMappingDeleted = false;
	        try
	        {
	            using (var command = Connection.GetStoredProcCommand("RBAC_DeleteRoleMapping"))
	            {
	                Connection.AddInParameter(command, "p_email", DbType.String, userEmail);
	                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);
	                Connection.AddInParameter(command, "p_roles", DbType.String, role);
	                Connection.AddInParameter(command, "p_ByEmail", DbType.String, byEmail);

	                using (var dataReader = Connection.ExecuteReader(command))
	                {
	                    if (dataReader.Read())
	                    {
	                        isMappingDeleted = string.IsNullOrEmpty(dataReader["Message"].ToString());
	                    }
	                }
	            }
	        }
	        catch (Exception ex)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	        }

            return isMappingDeleted;
	    }
        */

        public void UpdateUserTransferStatus(long transferId, string status)
        {
            using (var command = Connection.GetStoredProcCommand("UpdateCloudServiceUserTransferStatus"))
            {
                Connection.AddInParameter(command, "p_TransferId", DbType.Int64, transferId);
                Connection.AddInParameter(command, "p_TransferStatus", DbType.String, status);

                Connection.ExecuteNonQuery(command);
            }
        }

        #endregion

        public bool IsUserExistsByUniqueIdAndEmail(string uniqueId, string email) {
	        var isEmailExists = false;

					const string sqlQuery = "SELECT COUNT(aid) FROM accounts WHERE LOWER(Email) = LOWER(@p_Email) AND LOWER(UniqueId) = LOWER(@p_UniqueId);";

	        using (var command = PANLoginConnection.GetSqlStringCommand(sqlQuery)) {
                PANLoginConnection.AddInParameter(command, "@p_UniqueId", DbType.String, uniqueId);
                PANLoginConnection.AddInParameter(command, "@p_Email", DbType.String, email);

		        using (var dataReader = PANLoginConnection.ExecuteReader(command)) {
			        if (dataReader.Read()) {
				        isEmailExists = true;
			        }
		        }
	        }

	        return isEmailExists;
        }

        public List<string> GetAscSalesForceAccountIdsByUserAccountId(long userAccountId)
        {
            List<string> salesforceAccountIds = new List<string>();
            try
            {
                const string ascQuery = @"SELECT distinct s.SalesforceAccountId FROM csp.useraccount u JOIN csp.membership m ON u.UserAccountId = m.UserAccountId
                                            JOIN csp.membershiproles mr ON mr.MembershipId=m.MembershipId
                                            JOIN csp.role r ON r.RoleId=mr.RoleId
                                            JOIN csp.supportaccount s ON m.SupportAccountId=s.SupportAccountId
                                            WHERE u.UserAccountId= @p_UserAccountId AND r.SecurityClassification=3 AND 
                                          (m.ExpirationDate IS NULL OR m.ExpirationDate >= NOW())  ;";

                using (var command = Connection.GetSqlStringCommand(ascQuery))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            salesforceAccountIds.Add(Convert.ToString(dataReader["SalesforceAccountId"]));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return salesforceAccountIds;
        }

        public List<UserAccount> GetUsersByRole(long supportAccountId, int roleId)
        {
            List<UserAccount> users = new List<UserAccount>();

            try
            {

                const string getQuery = "SELECT distinct u.UserAccountId, u.Email  " +
                                        "  FROM csp.membership m JOIN csp.membershiproles mr ON m.MembershipId=mr.MembershipId " +
                                        " JOIN csp.useraccount u on m.userAccountId = u.userAccountId " +
                                        " WHERE (m.ExpirationDate IS NULL OR m.ExpirationDate >=CURDATE()) AND mr.RoleId=@p_RoleId AND m.SupportAccountId= @p_SupportAccountId";
                string connString = Secrets.CspDatabase;

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(getQuery, connection);

                    command.Parameters.AddWithValue("p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("p_RoleId", roleId);

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            users.Add(new UserAccount() { Email = Convert.ToString(dataReader["Email"]), Id = Convert.ToInt64(dataReader["UserAccountId"]) });
                        }

                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return users;
            }
        }
    }
}
