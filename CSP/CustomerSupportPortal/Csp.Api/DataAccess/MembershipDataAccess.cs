using Csp.Api.Model;
using System.Collections.Generic;
using Csp.Api.DataAccess.HelperClasses;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Csp.Api.Exceptions;
using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using Csp.Api.Model.Enums;

namespace Csp.Api.DataAccess {

	public class MembershipDataAccess : BaseDataAccess {
		/// <summary>
		/// This method is used to create MemberShip
		/// </summary>
		/// <param name="membership">Membership</param>
		/// <returns>Membership</returns>
		public Membership Create(Membership membership)
        {

            membership.RegisteredAppId = Convert.ToInt64(RegisteredApp.CSP);
			var resultMembership = new Membership();

			using (var command = Connection.GetStoredProcCommand("CreateMembership")) {

				Connection.AddInModelParameters(command, membership, CreateOrUpdate.Create);

				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {

						resultMembership.PopulateProperties(dataReader);
					}
				}
			}

			return resultMembership;
		}


		/// <summary>
		/// This method is used to get MemberShip by using membershipId
		/// </summary>
		/// <param name="membershipId">Membership Id</param>
		/// <returns>Membership</returns>
		public Membership Read(long membershipId, long restirctedByUserAccountId) {

			var membership = new Membership();
			using (var command = Connection.GetStoredProcCommand("GetMembershipByMembershipId")) {

				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddInRestrictedByUserAccountId(command, restirctedByUserAccountId);
				try {
					using (var dataReader = Connection.ExecuteReader(command)) {

						if (dataReader.Read()) {

							membership.PopulateProperties(dataReader);
						}
					}

				} catch (MySqlException exception) {
					if (exception.Message.Contains("Concurrent update error")) {

						throw new CspNotificationException("A user has already updated this Membership. Please refresh your data, review the changes made and try again");
					}
					throw;
				}
			}

			return membership;
		}


		/// <summary>
		/// This Function is used to Retrive Membership by using GroupId.
		/// </summary>
		/// <param name="groupId">groupId</param>
		/// <param name="parameters">Data Parameters</param>
		/// <returns>List<Membership/></returns>
		public List<MembershipListItem> ReadGroupMembers(long groupId, long restrictedByUserAccountId, DataParameters parameters) {

			var membershipList = new List<MembershipListItem>();

			using (var command = Connection.GetStoredProcCommand("GetMembershipsByGroupId")) {

				Connection.AddPaginationParameters(command, parameters);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {

					while (dataReader.Read()) {

						var membershipListItem = new MembershipListItem();

						membershipListItem.PopulateProperties(dataReader);
						membershipList.Add(membershipListItem);
					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {

					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return membershipList;
		}


		/// <summary>
		/// This Function is used to Update Membership.
		/// </summary>
		/// <param name="membership">Membership</param>
		/// <returns>Membership</returns>
		public Membership Update(Membership membership) {

			var resultMembership = new Membership();

			using (var command = Connection.GetStoredProcCommand("UpdateMembership")) {

				Connection.AddInModelParameters(command, membership, CreateOrUpdate.Update);

				using (var dataReader = Connection.ExecuteReader(command)) {

					if (dataReader.Read()) {
						resultMembership.PopulateProperties(dataReader);
					}
				}
			}

			return resultMembership;
		}


		/// <summary>
		/// This Method is used to retrive list of Memberships based on Support Account ID
		/// </summary>
		/// <param name="supportAccountId">support Account Id</param>
		/// <param name="parameters">Data Parameters</param>
		/// <returns>List<Membership/></returns>
		public List<MembershipListItem> ReadSupportAccountMembers(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters) {

			if (parameters.SortField == "IsSuperUserBool")
				parameters.SortField = "IsSuperUser";

            if (parameters.SortField == "MfaStatus")
                parameters.SortField = "MultiFactorAuthType";

            var membershipList = new List<MembershipListItem>();
			using (var command = Connection.GetStoredProcCommand("GetMembershipsBySupportAccountId")) {

				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);
				try
				{
					using (var dataReader = Connection.ExecuteReader(command))
					{


						while (dataReader.Read())
						{

							var membershipListItem = new MembershipListItem();

                            membershipListItem.FirstName = Convert.ToString(dataReader["FirstName"]);
                            membershipListItem.LastName = Convert.ToString(dataReader["LastName"]);
                            membershipListItem.DisplayName = Convert.ToString(dataReader["DisplayName"]);
                            membershipListItem.Email = Convert.ToString(dataReader["Email"]);
                            membershipListItem.Comments = Convert.ToString(dataReader["Comments"]);
                            membershipListItem.ActivationDate =Convert.ToDateTime(dataReader["ActivationDate"]);                               
                            membershipListItem.ExpirationDate = dataReader["ExpirationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["ExpirationDate"]);
                            membershipListItem.MembershipId = Convert.ToInt32(dataReader["MembershipId"]);
                            membershipListItem.Description = Convert.ToString(dataReader["Description"]);                           
                            membershipListItem.IsMfaEnabled = Convert.ToBoolean(dataReader["IsMfaEnabled"]);
                            membershipListItem.EnableEmailMfa = Convert.ToBoolean(dataReader["EnableEmailMfa"]);
                            membershipListItem.MultiFactorAuthType = (MultiFactorAuthType)(dataReader["MultiFactorAuthType"] == null || dataReader["MultiFactorAuthType"] is DBNull ? 0 : dataReader["MultiFactorAuthType"]);
                            membershipListItem.AutoFocusTrialUser = Convert.ToBoolean(dataReader["AutoFocusTrialUser"]);
                            membershipListItem.AutoFocusTrialExpirationDate = dataReader["AutoFocusTrialExpirationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dataReader["AutoFocusTrialExpirationDate"]);
                            membershipListItem.LastUpdateByEmail     = Convert.ToString(dataReader["LastUpdatedBy"]);
                            membershipListItem.CreatedByEmail = Convert.ToString(dataReader["CreatedBy"]);
                            membershipListItem.HasActiveAutofocusLicense = Convert.ToBoolean(dataReader["HasActiveAutofocusLicense"]);
                            membershipListItem.RoleNames = Convert.ToString(dataReader["RoleNames"]);
                            membershipList.Add(membershipListItem);
						}
					}
				}
				catch(Exception ex)
				{
					// this is only for debugging.
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {

					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return membershipList;
		}


		/// <summary>
		/// This Method is used to Retrieve the List of MemberShips based on DeviceID
		/// </summary>
		/// <param name="deviceId">DeviceId</param>
		/// <param name="parameters">Data Parameters</param>
		/// <returns>List<Membership/></returns>
		public List<MembershipListItem> ReadDeviceMembers(long deviceId, long restrictedByUserAccountId, DataParameters parameters) {

			var membershipList = new List<MembershipListItem>();

			using (var command = Connection.GetStoredProcCommand("GetMembershipsByDeviceId")) {

				Connection.AddPaginationParameters(command, parameters);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var membership = new MembershipListItem();

						membership.PopulateProperties(dataReader);
						membershipList.Add(membership);
					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {

					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return membershipList;
		}


		/// <summary>
		/// This Function is used to delete MemberShips  by using membershipId
		/// </summary>
		/// <param name="membershipId">membership Id</param>
		/// <param name="deletedByUserAccountId">deleted By UserAccount Id</param>
		/// <returns>void<Membership/></returns>
		public void Delete(long membershipId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteMembershipByMembershipId")) {

				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddInDeletedByUserAccountIdParameter(command, deletedByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		public bool HasMembershipforSupportAccountByEmail(string email, long supportAccountId) {

			var result = false;
			const string sql = "select HasMembershipForSupportAccountByEmail(@p_Email,@p_SupportAccountId) result from dual";

			using (var command = Connection.GetSqlStringCommand(sql)) {

				Connection.AddInEmailParameter(command, email);
				Connection.AddInSupportAccountIdParameter(command, supportAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						result = Convert.ToBoolean(dataReader["result"]);
					}
				}
			}

			return result;
		}

		public bool HasMembershipforGroupByEmail(string email, long groupId) {

			var result = false;
			const string sql = "select HasMembershipForGroupByEmail(@p_Email,@p_GroupId) result from dual";

			using (var command = Connection.GetSqlStringCommand(sql)) {

				Connection.AddInEmailParameter(command, email);
				Connection.AddInGroupIdParameter(command, groupId);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						result = Convert.ToBoolean(dataReader["result"]);

					}
				}
			}

			return result;
		}

		public bool HasMembershipforDeviceByEmail(string email, long deviceId) {

			var result = false;
			const string sql = "select HasMembershipForDeviceByEmail(@p_Email,@p_DeviceId) result from dual";

			using (var command = Connection.GetSqlStringCommand(sql)) {

				Connection.AddInEmailParameter(command, email);
				Connection.AddInSupportAccountIdParameter(command, deviceId);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						result = Convert.ToBoolean(dataReader["result"]);

					}
				}
			}

			return result;
		}

		public void AssignMembershipRoles(long membershipId, long userAccountId, long supportAccountId, long createdByUserAccountId, bool assignSuperUser = false, bool allowedOrg = true) {

			using (var command = Connection.GetStoredProcCommand("AssignMembershipRoles")) {

				Connection.AddInParameter(command, "p_MembershipId", DbType.Int64, membershipId);
				Connection.AddInParameter(command, "p_UserAccountId", DbType.String, userAccountId);
				Connection.AddInParameter(command, "p_SupportAccountId", DbType.String, supportAccountId);
				Connection.AddInParameter(command, "p_CreatedByUserAccountId", DbType.String, createdByUserAccountId);
                Connection.AddInParameter(command, "p_AssignSuperUser", DbType.Boolean, assignSuperUser);
                Connection.AddInParameter(command, "p_allowedOrg", DbType.Boolean, allowedOrg);

                Connection.ExecuteNonQuery(command);
			}
		}

	    public string GetPreviousRolesByMembershipId(long membershipId)
	    {
	        string previousRoles = string.Empty;

	        try
	        {
                const string sqlQuery = "GetPreviousMembershipRolesById";
                string connString = Secrets.CspDatabase;

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_MembershipId", membershipId);
                    MySqlParameter returnParam = command.Parameters.Add("@RETURN_VALUE", MySqlDbType.String);
                    returnParam.Direction = System.Data.ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();

                    previousRoles = Convert.ToString(returnParam.Value);
                }
               
	        }
	        catch (Exception ex)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
               
	        }
	        return previousRoles;
	    }

        public List<SupportAccountMembershipModel> GetDeletedMembershipsBySupportAccountId(long supportAccountId)
	    {
	        List<SupportAccountMembershipModel> usersList = new List<SupportAccountMembershipModel>();

	        try
	        {

               using (var command = Connection.GetStoredProcCommand("GetDeletedMembershipsBySupportAccountId"))
	            {

	               Connection.AddInSupportAccountIdParameter(command,supportAccountId);

	                using (var dataReader = Connection.ExecuteReader(command))
	                {
	                    while (dataReader.Read())
	                    {
	                        // m.Name, m.Email, m.ActivationDate, m.ExpirationDate, m.Description, m.PreviousRoles,m.Roles,m.LastUpdatedBy ,m.CreatedBy
	                        usersList.Add(new SupportAccountMembershipModel()
	                        {
	                            Email = Convert.ToString(dataReader["Email"]),
                                ActivationDate = Convert.ToDateTime(dataReader["ActivationDate"]),
	                            Description = Convert.ToString(dataReader["Description"]),
	                            PreviousRoles = Convert.ToString(dataReader["Roles"]),
	                            LastUpdatedBy = Convert.ToString(dataReader["DeletedBy"]),
	                            CreatedBy = Convert.ToString(dataReader["CreatedBy"]),
                                Roles = GetRole(Convert.ToString(dataReader["Roles"])),
                                DisplayName = Convert.ToString(dataReader["Name"])
	                        });
	                    }
	                }

	                
	            }

                return usersList;
	        }
	        catch (Exception ex)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return usersList;
	        }
	    }

        private IEnumerable<NameValue> GetRole(string roles)
	    {
	        List<NameValue> roleCollection = new List<NameValue>();
	        try
	        {
	          string[] sRoles=  roles.Split(',');
	            
	            foreach (var role in sRoles)
	            {
	                roleCollection.Add(new NameValue()
	                {
                        Name = role,
                        Value = role
	                });
	            }
	        }
	        catch (Exception ex)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	        }
	        return roleCollection.AsEnumerable();
	    }

        public IEnumerable<MembershipForSa> GetMembershipsForSupportAccountId(long userAccountId,long supportAccountId)
        {
            List<MembershipForSa> membershipCollection = new List<MembershipForSa>();
            try
            {

                using (var command = Connection.GetStoredProcCommand("GetMembershipsForSupportAccountId"))
                {

                    Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                    Connection.AddInUserAccountIdParameter(command, userAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {

                            membershipCollection.Add(new MembershipForSa()
                            {
                                SupportAccountId = dataReader["cspid"] == null || dataReader["cspid"] is DBNull ? 0 : Convert.ToInt64(dataReader["cspid"]),
                                AscSupportAccountId = dataReader["asc cspid"] == null || dataReader["asc cspid"] is DBNull  ? 0 : Convert.ToInt64(dataReader["asc cspid"]),
                                GlobalSupportAccountId = dataReader["global cspid"] == null || dataReader["global cspid"] is DBNull ? 0 : Convert.ToInt64(dataReader["global cspid"]),
                                GroupSupportAccountId = dataReader["group cspid"] == null || dataReader["group cspid"] is DBNull ? 0 : Convert.ToInt64(dataReader["group cspid"]),
                                MembershipId = dataReader["MembershipId"] == null || dataReader["MembershipId"] is DBNull ? 0 : Convert.ToInt64(dataReader["MembershipId"]),
                                ConsentSupportAccountId = dataReader["consent cspid"] == null || dataReader["consent cspid"] is DBNull ? 0 : Convert.ToInt64(dataReader["consent cspid"])
                            });
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                
            }
            return membershipCollection.AsEnumerable();
        }

        public async Task<MembershipDetails> GetMembershipDetailsBySfdcPortalUsername(string portalUsername, bool hasMultipleAccounts = false)
        {
            try
            {

                MembershipDetails membershipDetails = new MembershipDetails();

                const string sql = @" SELECT m.MembershipId,u.UserAccountId,u.FirstName,u.LastName,COALESCE(s.SupportAccountId,sgs.SupportAccountId) 'SupportAccountId',
    COALESCE(s.Name,sgs.Name) 'Name', COALESCE(s.SalesforceAccountId, sgs.SalesforceAccountId) 'SalesforceAccountId',u.email,COALESCE(s.CanImpersonate,sgs.CanImpersonate) 'CanImpersonate'
    FROM csp.membership m JOIN csp.useraccount u ON m.MembershipId = u.SelectedMembershipId
    LEFT OUTER JOIN csp.supportaccount s ON m.SupportAccountId = s.SupportAccountId
    LEFT OUTER JOIN csp.supportaccountgroup sg ON m.GroupId = sg.GroupId
    LEFT OUTER JOIN csp.supportaccount sgs ON sg.SupportAccountId = sgs.SupportAccountId
    WHERE m.SalesforceLogInId = @p_portalUsername and (s.IsActive or sgs.IsActive) LIMIT 1; ";
				

                using (var command = Connection.GetSqlStringCommand(sql))
                {

                    Connection.AddInParameter(command, "p_portalUsername", DbType.String, portalUsername);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {

                            membershipDetails = new MembershipDetails()
                            {
                                MembershipId = Convert.ToInt64(dataReader["MembershipId"]),
                                SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                                UserAccountId = Convert.ToInt64(dataReader["UserAccountId"]),
                                CompanyName = Convert.ToString(dataReader["Name"]),
                                FirstName = Convert.ToString(dataReader["FirstName"]),
                                HasMultipleAccounts = false,
                                LastName = Convert.ToString(dataReader["LastName"]),
                                SalesforceAccountId = Convert.ToString(dataReader["SalesforceAccountId"]),
                                ImpersonateSetting = dataReader["CanImpersonate"] != DBNull.Value ? Convert.ToInt32(dataReader["CanImpersonate"]) : 2 // Require approval for each access request  
                            };
                        }
                    }

                }

                return membershipDetails; 
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new MembershipDetails();
            }
        }
    }
}
