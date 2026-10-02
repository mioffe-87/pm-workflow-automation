using System.Configuration;
using System.Data;
using Csp.Api.Model;
using System.Collections.Generic;
using Csp.Api.DataAccess.HelperClasses;
using System;
using MySql.Data.MySqlClient;
using Csp.Api.Exceptions;
using System.Data.Common;
using System.Runtime.InteropServices;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess {

	public class SupportAccountDataAccess : BaseDataAccess {

		/// <summary>
		/// This metod is used for updating support Account Details
		/// </summary>
		/// <param name="supportAccount">support Account</param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <returns>SupportAccount</returns>
		public SupportAccount Update(SupportAccount supportAccount, long restrictedByUserAccountId) {

			var resultSupportAccount = new SupportAccount();

            using (var command = Connection.GetStoredProcCommand("UpdateSupportAccount"))
            {
				Connection.AddInModelParameters(command, supportAccount, CreateOrUpdate.Update);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				try {

					using (var dataReader = Connection.ExecuteReader(command)) {
						if (dataReader.Read()) {

							resultSupportAccount.PopulateProperties(dataReader);
						}
					}


				} catch (MySqlException exception) {
					if (exception.Message.Contains("Concurrent update error")) {

						throw new CspNotificationException("A user has already updated this Support Account. Please refresh your data, review the changes made and try again");
					} else {
						throw;
					}

				}
			}
			return resultSupportAccount;
		}


		/// <summary>
		/// This method is used for retriving Support account details based on accountId
		/// </summary>
		/// <param name="supportAccountId">supportAccount Id</param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <returns>SupportAccount</returns>
		public SupportAccount Read(long supportAccountId, long restrictedByUserAccountId) {

			var supportAccount = new SupportAccount();

            using (var command = Connection.GetStoredProcCommand("GetSupportAccountBySupportAccountId"))
            {
				Connection.AddInSupportAccountIdParameter(command, supportAccountId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);

				using (var dataReader = Connection.ExecuteReader(command)) {


					if (dataReader.Read()) {

						supportAccount.PopulateProperties(dataReader);
					}
				}
			}

			return supportAccount;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="userAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public List<SupportAccountListItem> ReadSupportAccountbyUserAccountId(long userAccountId, DataParameters parameters) {

			var supportAccountListItems = new List<SupportAccountListItem>();

			using (var command = Connection.GetStoredProcCommand("GetSupportAccountsByUserAccountId")) {
				Connection.AddInUserAccountIdParameter(command, userAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var supportAccountListItem = new SupportAccountListItem();
						supportAccountListItem.PopulateProperties(dataReader);
						supportAccountListItems.Add(supportAccountListItem);

					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return supportAccountListItems;
		}

        public List<SelectableAccount> GetAscBklnAccounts(long supportAccountId)
        {

            var selectableAccount = new List<SelectableAccount>();

            using (var command = Connection.GetStoredProcCommand("GetSupportAccountAscUnregisteredBkln"))
            {
                Connection.AddInParameter(command, "p_SupportCenterSupportAccountId", DbType.Int64, supportAccountId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var account = new SelectableAccount()
                        {
                            SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"].ToString()),
                            AccountTypeId = 1,
                            CompanyName = dataReader["Name"].ToString(),
                        };
                        selectableAccount.Add(account);
                    }
                }
            }
            return selectableAccount;
        }

		public List<SupportAccountMembershipListItem> ReadSupportAccountMembershipsbyUserAccountId(long userAccountId, long restrictedByUserAccountId, DataParameters parameters) {

			var supportAccountMembershipListItems = new List<SupportAccountMembershipListItem>();

			using (var command = Connection.GetStoredProcCommand("GetSupportAccountMembershipsByUserAccountId")) {
				Connection.AddInUserAccountIdParameter(command, userAccountId);
				Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {


					while (dataReader.Read()) {

						var supportAccountMembershipListItem = new SupportAccountMembershipListItem();
						supportAccountMembershipListItem.PopulateProperties(dataReader);
						supportAccountMembershipListItems.Add(supportAccountMembershipListItem);

					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return supportAccountMembershipListItems;
		}

        public MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid)
        {
            List<MatchingSupportAccount> matches = new List<MatchingSupportAccount>();


            string sqlQuery = @"SELECT
              s.SupportAccountId,
              s.Name,
              (SELECT
                  COUNT(d.DeviceId)
                FROM csp.device d
                WHERE d.SupportAccountId = s.SupportAccountId) NumberOfAssets,
              (SELECT
                  COUNT(m.MembershipId)
                FROM csp.membership m
                WHERE s.SupportAccountId = m.SupportAccountId) NumberOfMembers
            FROM csp.supportaccount s
            WHERE s.SalesforceAccountId =@p_Sfdcid
            AND s.IsActive = 1
            GROUP BY s.SupportAccountId,s.Name";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
               Connection.AddInParameter(command, "@p_Sfdcid", DbType.String, sfdcid);                
                using (IDataReader dataReader = Connection.ExecuteReader(command))
               {
                   while (dataReader.Read())
                 {
                        matches.Add(new MatchingSupportAccount
                        {
                            Name = Convert.ToString(dataReader["Name"]),
                            SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                            NumberOfAssets= Convert.ToInt32(dataReader["NumberOfAssets"]),
                            NumberOfMembers= Convert.ToInt32(dataReader["NumberOfMembers"])
                        });
                    }
                    
                }
            }


            return matches.ToArray();
        }

    

           
        
        public List<SelectorAccountListItem> ReadSupportAccountsAndGroupsByEmailId(string emailId) {

			List<SelectorAccountListItem> selectorAccountListItems = null;

			using (var command = Connection.GetStoredProcCommand("GetSupportAccountsAndGroupsByEmailId")) {
				Connection.AddInEmailParameter(command, emailId);

				using (IDataReader dataReader = Connection.ExecuteReader(command)) {

					selectorAccountListItems = new List<SelectorAccountListItem>();
					while (dataReader.Read()) {

						var selectorAccountListItem = new SelectorAccountListItem();
						selectorAccountListItem.PopulateProperties(dataReader);
						selectorAccountListItems.Add(selectorAccountListItem);
					}
				}
			}

			return selectorAccountListItems;
		}

		/// <summary>
		/// This metod is used for adding & updating currently selected account details
		/// </summary>
		/// <returns>SupportAccount</returns>
		public bool AddUpdateSelectedSupportAccount(string email, long membershipId, long supportAccountId) {

			// Updating Support Account Details
			using (var command = Connection.GetStoredProcCommand("AddUpdateUserSelectedMembership")) {
				Connection.AddInEmailParameter(command, email);
				Connection.AddInMembershipIdParameter(command, membershipId);
				Connection.AddInSupportAccountIdParameter(command, supportAccountId);

				var result = Connection.ExecuteNonQuery(command);

				return result == 1;
			}
		}

		public List<NameValue> GetSupportAccountsBySapId(long id) {

			const string sqlQuery = @"
        SELECT s.supportaccountid, s.name from csp.supportaccount s
        INNER JOIN csp.sapsupportaccountmapping m on m.supportaccountid = s.supportaccountid
        WHERE m.sapid = @p_SAPId
        ORDER BY name;";
			var supportAccountList = new List<NameValue>();

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_SAPId", DbType.Int32, id);

				using (IDataReader dataReader = Connection.ExecuteReader(command)) {
					while (dataReader.Read()) {
						supportAccountList.Add(new NameValue() {
							Name = dataReader["name"].ToString(),
							Value = dataReader["supportaccountid"].ToString()
						});
					}
					return supportAccountList;
				}
			}
		}

        public bool IsThirdPartyIdpBetaAccount(string domain)
        {
            const string sqlQuery = @"SELECT * FROM csp.betaaccess b WHERE b.EmailAddress = @p_Domain AND b.FeatureName='3PIdP';";
           
            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInParameter(command, "@p_Domain", DbType.String, domain);

                using (IDataReader dataReader = Connection.ExecuteReader(command)) {
                    while (dataReader.Read())
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        public CBHReport GetCbhMetrics()
        {
            CBHReport report = new CBHReport()
            {
                RecentRequests = new List<CBHReportDetail>()
            };

            try
            {
                var sqlConnection = new MySqlConnection(Connection.ConnectionString);
                var sqlCommand = new MySqlCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "GetConsentRequestStats";
                sqlCommand.Connection = sqlConnection;
                sqlConnection.Open();

                var CbhDetails= new DataSet();
                var adapter = new MySqlDataAdapter(sqlCommand);
                adapter.Fill(CbhDetails);

                if (CbhDetails.Tables.Count > 0 && CbhDetails.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drow in CbhDetails.Tables[0].Rows)
                    {
                        report.TotalCount = Convert.ToInt32(drow["p_totalCount"]);
                        report.ActiveCount = Convert.ToInt32(drow["p_activeCount"]);
                        report.RecentCount = Convert.ToInt32(drow["p_newCount"]);
                    }
                }
                if (CbhDetails.Tables.Count > 0 && CbhDetails.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow drow in CbhDetails.Tables[1].Rows)
                    {
                        CBHReportDetail detail = new CBHReportDetail()
                        {
                            SourceAccountName = Convert.ToString(drow["Source Account"]),
                            TargetAccountName = Convert.ToString(drow["Target Account"]),
                            CreatedOn = Convert.ToString(drow["CreatedOn"])
                        };
                       report.RecentRequests.Add(detail);
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return report;
        }

        public List<string> GetCbhEmailsList()
        {
            const string sqlQuery = @"SELECT DISTINCT l.LookupValue FROM csp.lookups l WHERE l.LookupKey='cbhreport';";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    List<string> emailslist = new List<string>();
                    while (dataReader.Read())
                    {
                        emailslist.Add(Convert.ToString(dataReader["LookupValue"]));
                    }
                    return emailslist;
                }
            }
        }

        public List<NameValueOnly> GetSupportAccountDetailsByIds(string supportAccountIds)
        {
            string sqlQuery = "SELECT s.Name,s.SupportAccountId FROM SupportAccount s WHERE s.SupportAccountId IN (" + supportAccountIds + ") AND s.IsActive=1";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    List<NameValueOnly> accountsList = new List<NameValueOnly>();
                    while (dataReader.Read())
                    {
                        accountsList.Add(new NameValueOnly()
                        {
                            Name = Convert.ToString(dataReader["Nme"]),
                            Value = Convert.ToString(dataReader["SupportAccountId"])
                        });
                    }
                    return accountsList;
                }
            }
        }

        public void UpdateSupportAccountEmailDomainType(long supportAccountId, string domainName)
        {
            var sqlQuery = @"UPDATE csp.supportaccount s JOIN csp.emaildomaintype et ON s.EmailDomainTypeId = et.DomainTypeId AND lower(et.name) = 'unknown' 
                          SET s.EmailDomainTypeId = (SELECT e.DomainTypeId FROM csp.emaildomaintype e
                          WHERE lower(e.name) = lower(@p_domainName))
                          WHERE s.SupportaccountId = @p_SupportAccountId";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    command.CommandType = CommandType.Text;
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_domainName", DbType.String, domainName);
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
