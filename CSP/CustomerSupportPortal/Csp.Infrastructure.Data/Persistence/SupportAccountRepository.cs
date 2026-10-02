using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.EntityClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using PAN.Common.Core.Persistence;
using PAN.Common.Infrastructure.Data.Persistence;

namespace Csp.Infrastructure.Data.Persistence {
	public class SupportAccountRepository : ISupportAccountRepository {
		private readonly INHibernateUnitOfWork _unitOfWork;
		private readonly IRepository _repository;
        private readonly IConnectionStrings _connectionStrings;

		public SupportAccountRepository(INHibernateUnitOfWork unitOfWork, IRepository repository, IConnectionStrings connectionStrings) {
			_unitOfWork = unitOfWork;
			_repository = repository;
            _connectionStrings = connectionStrings;
        }

		/// <summary>
		/// Guesses a user account to use for device_master.acctid
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
        [Obsolete("Shouldn't Guess - don't we know in all cases?")]
		public long GuessUserAccountIdBySupportAccountId(long supportAccountId) {
			var likelyUserAccount =
				(
					from mr in _repository.Query<Api.EntityClasses.Membershiprole>()
					where
						mr.MembershipId_Membership_.ActivationDate <= DateTime.Now
						&& (
								 mr.MembershipId_Membership_.ExpirationDate.HasValue == false
								 || mr.MembershipId_Membership_.ExpirationDate >= DateTime.Now
							 )
						&& mr.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					orderby mr.MembershipId_Membership_.MembershipId
					select mr.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId
				).First();

			return likelyUserAccount;
		}

		public void UpdateDeviceSupportAccountInCsp(long deviceId, long destinationSupportAccountId) {
			using (var stateless = _unitOfWork.CurrentSession.SessionFactory.OpenSession()) {
				using (var tx = stateless.BeginTransaction()) {

					var device = stateless.Get<Api.EntityClasses.Device>(deviceId);

					var supportAccount = stateless.Get<Api.EntityClasses.Supportaccount>(destinationSupportAccountId);

					device.SupportAccountId_Supportaccount_ = supportAccount;

					stateless.Save(device);

					tx.Commit();
				}
			}
		}

		public string GetSupportAccountNameBySupportAccountId(long supportAccountId) {
			using (var db = new cspEntities()) {
				return db.supportaccounts.Single(x => x.SupportAccountId == supportAccountId).Name;
			}
		}

		public Supportaccount GetSupportAccountById(long id) {
			return _repository.Get<Supportaccount>(id);
		}

		/// <summary>
		/// Checks if the support account has any direct memberships (active or inactive) or registered assets
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
		public bool IsActiveSupportAccount(long supportAccountId) {
			using (var db = new cspEntities()) {

				//Don't worry about group memberships, you would almost always have a super user membership first
				var memberships =
					from mem in db.memberships
					where mem.SupportAccountId == supportAccountId
					select mem;

				var registeredAssets =
					from dev in db.devices
					where dev.RegistrationDate != null
								&& dev.SupportAccountId == supportAccountId
					select dev;

				return memberships.Any() || registeredAssets.Any();
			}
		}

		/// <summary>
		/// "Merges" the Eval customer account
		/// </summary>
		/// <param name="duplicateSupportAccountId">Support Account ID of the duplicate that will be deleted</param>
		/// <param name="keeperSupportAccountId">SupportAccountId of the eval Support Account that will now be mapped to the SAP sales</param>
		public void RemoveDuplicateAccountAndFixSapMappings(long duplicateSupportAccountId, long keeperSupportAccountId) {
			using (var db = new cspEntities()) {
				var duplicateSupportAccount = db.supportaccounts.FirstOrDefault(x => x.SupportAccountId == duplicateSupportAccountId);

				foreach (var sapMap in duplicateSupportAccount.sapsupportaccountmappings) {
					sapMap.SupportAccountId = keeperSupportAccountId;
				}
				db.SaveChanges();

				db.supportaccounts.Remove(duplicateSupportAccount);
				db.SaveChanges();
			}
		}

		public void ConvertEvalSupportAccountToCustomer(long supportAccountId) {
			const string customerAccountType = "Customer";
			using (var db = new cspEntities()) {
				var supportAccount = db.supportaccounts.Single(x => x.SupportAccountId == supportAccountId);
				supportAccount.AccountTypeId = db.accounttypes.FirstOrDefault(x => x.Name == customerAccountType).AccountTypeId;
				db.SaveChanges();
			}
		}

		public bool IsEvalSupportAccount(long supportAccountId) {
			const string evalAccountType = "Eval";
			string accountType=string.Empty;

			using (var db = new cspEntities())
			{
				var supportaccount = db.supportaccounts.FirstOrDefault(x => x.SupportAccountId == supportAccountId);
				if (supportaccount != null)
				{
					accountType = supportaccount.accounttype.Name;
					
				}

				return accountType == evalAccountType;
			}
		}

		public MatchingSupportAccount[] GetSupportAccountsBySfdcid(string sfdcid) {
			var matches = new List<MatchingSupportAccount>();

			using (var db = new cspEntities()) {

				var supportAccounts = (
					from sa in db.supportaccounts
					where sa.SalesforceAccountId.Substring(0, 15) == sfdcid.Substring(0, 15) && sa.IsActive == true
					select new {
						sa.SupportAccountId,
						sa.Name,
					}).ToArray();

				foreach (var sa in supportAccounts) {
					var match = new MatchingSupportAccount {
						SupportAccountId = sa.SupportAccountId,
						Name = sa.Name,
					};

					match.NumberOfAssets = db.devices.Count(x => x.SupportAccountId == sa.SupportAccountId);
					match.NumberOfMembers = db.memberships.Count(x => x.SupportAccountId == sa.SupportAccountId);

					matches.Add(match);
				}
			}

			return matches.ToArray();
		}


        public bool IsSupportAccountExists(string supportAccountName)
        {
            using (var db = new cspEntities())
            {
                var accountExists = db.supportaccounts.Any(x => x.Name == supportAccountName);
                return accountExists;
            }
        }


        public bool CreateSupportAccount(string name, int type, long createdBy, out long supportAccountId)
        {
            bool isSuccess;
            supportAccountId = 0;
            try
            {
                using (var db = new cspEntities())
                {
                    var supportAccount = new supportaccount()
                    {
                        AccountTypeId = type,
                        CreatedByUserAccountId = createdBy,
                        CreatedOn = System.DateTime.Now,
                        IsOptOutOfAllEmails = false,
                        IsActive = true,
                        Name = name
                    };
                    db.supportaccounts.Add(supportAccount);
                    isSuccess = db.SaveChanges() > 0;
                    if (isSuccess)
                    {
                        supportAccountId = supportAccount.SupportAccountId;
                    }
                }
            }
            catch (Exception)
            {
                isSuccess = false;
            }
            return isSuccess;
        }


        public long? GetSupportAccountIdByName(string supportAccountName)
        {
            long? SupportAccountId = null;
            using (var db = new cspEntities())
            {
                var firstOrDefault = db.supportaccounts.FirstOrDefault(x => x.Name == supportAccountName);
                if (firstOrDefault != null)
                {
                    SupportAccountId = firstOrDefault.SupportAccountId;
                }
            }
            return SupportAccountId;
        }

				public bool CreateAwsAccountSupportAccountMapping(string awsAccountId, long supportAccountId)
				{
				    if (string.IsNullOrEmpty(awsAccountId))
				        return false;
					bool isSuccess;
					try {
						using (var db = new cspEntities()) {
							var saAwsMapping = new awsaccountsupportaccountmapping() {
								AwsAccountId = awsAccountId,
								SupportAccountId = supportAccountId
							};
							db.awsaccountsupportaccountmappings.Add(saAwsMapping);
							isSuccess = db.SaveChanges() > 0;
						}
					} catch (Exception) {
						isSuccess = false;
					}
					return isSuccess;
				}

				public bool IsAwsAccount(long supportAccountId) {
					bool result = false;
					using (var db = new cspEntities()) {
						var supact = (from asam in db.awsaccountsupportaccountmappings
																 join sa in db.supportaccounts
																	 on asam.SupportAccountId equals sa.SupportAccountId
																 where asam.SupportAccountId == supportAccountId
																 select sa).FirstOrDefault();
						if (supact != null)
						{
							result = true;
						}
					}
					return result;
				}

	    public bool RegistrationRequiresSuApproval(long supportAccountId)
	    {
	        var db = SessionDbContext.CspObjectContext;

	        var requiresApproval = db.supportaccounts.Single(x => x.SupportAccountId == supportAccountId).RegRequiresSUApproval;

	        return requiresApproval;
	    }

	    public void SetRegistrationRequiresSuApproval(long supportAccountId, bool registrationRequiresSuApproval)
	    {
            var db = SessionDbContext.CspObjectContext;

            var supportAccount = db.supportaccounts.Single(x => x.SupportAccountId == supportAccountId);
	        supportAccount.RegRequiresSUApproval = registrationRequiresSuApproval;

	        db.SaveChanges();
	    }


        public bool IsCsspAccount(long supportAccountId)
        {
            using (var db = new cspEntities())
            {
                var isCsspAccount = db.supportaccounts.Any(x => x.SupportAccountId == supportAccountId && x.IsCssp);
                return isCsspAccount;
            }
        }

        public bool IsFedrampAccount(long supportAccountId)
        {
            using (var db = new cspEntities())
            {
                var isFedrampAccount = db.supportaccounts.Any(x => x.SupportAccountId == supportAccountId && x.IsFedRamp == true);
                return isFedrampAccount;
            }
        }



        public string GetSupportAccountNameForEndUser(string csspAccountName)
        {
            var db = SessionDbContext.CspObjectContext;
            var supportAccount = db.csspenduserinformations.Where(x => x.CustomerReferenceId == csspAccountName).Select(y=>y.CompanyName).FirstOrDefault();
            return supportAccount;
        }


        public string GetDefaultSfdcAccountForCssp(long csspSupportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var sfdcAccountId = db.csspdatas.Where(x => x.SupportAccountId == csspSupportAccountId).Select(y => y.DefaultSfdcAccountId).FirstOrDefault();
            return sfdcAccountId;
        }

        public SupportAccount GetSeLabKitSupportAccountId(long userAccountId)
        {
            SupportAccount supportAccount = null;
            var query = @"
                SELECT s.SupportAccountId, s.Name FROM supportaccount s
                  INNER JOIN sesupportaccountmapping ssm ON s.SupportAccountId = ssm.SupportAccountId
                  WHERE ssm.UserAccountId = ?p_UserAccountId";

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_UserAccountId", userAccountId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            supportAccount = new SupportAccount();

                            supportAccount.Id = Convert.ToInt32(reader["SupportAccountId"]);
                            supportAccount.Name = reader["Name"].ToString();
                        }
                    }
                }
            }

            return supportAccount;
        }

        public SupportAccount CreateSeLabKitSupportAccount(long userAccountId, string salesforceAccountId, int accountTypeId, string accountName, string phoneNumber, string address1,
            string address2, string city, string region, string postalCode, long? countryId, long createdByUserAccountId)
        {
            SupportAccount supportAccount= null;

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand("CreateSeLabKitSupportAccount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@p_UserAccountId", userAccountId);
                    command.Parameters.AddWithValue("@p_SalesforceAccountId", salesforceAccountId);
                    command.Parameters.AddWithValue("@p_AccountTypeId", accountTypeId);
                    command.Parameters.AddWithValue("@p_Name", accountName);
                    command.Parameters.AddWithValue("@p_PhoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@p_Address1", address1);
                    command.Parameters.AddWithValue("@p_Address2", address2);
                    command.Parameters.AddWithValue("@p_City", city);
                    command.Parameters.AddWithValue("@p_Region", region);
                    command.Parameters.AddWithValue("@p_PostalCode", postalCode);
                    command.Parameters.AddWithValue("@p_CountryId", countryId);
                    command.Parameters.AddWithValue("@p_CreatedByUserAccountId", createdByUserAccountId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            supportAccount = new SupportAccount();

                            supportAccount.Id = Convert.ToInt32(reader["SupportAccountId"]);
                            supportAccount.Name = reader["Name"].ToString();
                        }
                    }
                }
            }

            return supportAccount;
        }

        public int CanImpersonate(long supportAccountId)
        {
            int canImpersonate = 0;

            var query = @"SELECT s.CanImpersonate FROM supportaccount s WHERE s.supportAccountId = ?p_SupportAccountId";

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            canImpersonate = Convert.ToInt32(reader["CanImpersonate"]);
                        }
                    }
                }
            }

            return canImpersonate;
        }

        public bool IsMSSPAccount(long supportAccountId)
        {
            bool isMSSP = false;

            var query = @"SELECT COALESCE(s.IsMssp,0) IsMssp FROM supportaccount s WHERE s.supportAccountId = ?p_SupportAccountId";

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isMSSP = Convert.ToBoolean(reader["IsMssp"]);
                        }
                    }
                }
            }

            return isMSSP;
        }
    }
}
