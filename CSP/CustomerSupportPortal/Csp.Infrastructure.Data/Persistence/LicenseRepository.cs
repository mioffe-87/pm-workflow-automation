using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using License = Csp.Api.EntityClasses.License;
using Csp.Api.HelperClasses;

namespace Csp.Infrastructure.Data.Persistence {

	public class LicenseRepository : ILicenseRepository
	{
	    private IConnectionStrings _connectionStrings;

	    public LicenseRepository(IConnectionStrings connectionStrings)
	    {
	        _connectionStrings = connectionStrings;
	    }

        public IEnumerable<ReleasedLicenseItem> GetReleasedVMLicenses(DataParameters parameters, long supportAccountId) {
            
            IEnumerable<ReleasedLicenseItem> result = new List<ReleasedLicenseItem>();
            using (var db = new cspEntities()) {

                result =
                         (from deactivatedLic in db.deactivatedauthcodes
                         join lic in db.licenses on deactivatedLic.LicenseId equals lic.LicenseId
                         join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                         where deactivatedLic.SupportAccountId == supportAccountId
                         select new ReleasedLicenseItem() {
                             AuthorizationCode = lic.AuthorizationCode,
                             Description = pm.Description,
                             ExpirationDate = deactivatedLic.DeactivatedExpiration,
                             PartNumber = pm.PartNumber
                         })
                         .OrderBy(a => a.ExpirationDate)  
                         .Skip((int)parameters.PaginationOffset)
                         .Take((int)parameters.PaginationCount).ToList();
            }
            return result;
        }

		public List<License> ReadLicenses(long[] licenseIdArray) {

			if (!licenseIdArray.Any()) {
				return new List<License>();
			}

			using (var db = new cspEntities()) {

				var lics = db.licenses.Where(x => licenseIdArray.Contains(x.LicenseId)).ToList();

				return Mapper.Map<List<License>>(lics);
			}
		}

		public void UpdateLicenseSupportAccountId(long deviceId, long supportAccountId) {

			using (var db = new cspEntities()) {

				var licsToUpdate =
					(
						from lic in db.licenses
						where
							lic.DeviceId == deviceId
							&& lic.SupportAccountId != supportAccountId
						select lic
					).ToArray();

				foreach (var lic in licsToUpdate) {
					lic.SupportAccountId = supportAccountId;
					db.SaveChanges();
				}
			}
		}

		public bool IsAuthCodeValid(string authCode) {
			using (var db = new cspEntities()) {
				return db.licenses.Any(x => x.AuthorizationCode == authCode);
			}
		}

        private const string SitePartTypePanDbIdentification = "SITE";
        public SiteLicenseResult IsSiteLicenseAuthCodeValid(string authCode)
        {
            var db = SessionDbContext.CspObjectContext;

            var lic = db.licenses.SingleOrDefault(x => x.AuthorizationCode == authCode);

            if (lic == null)
            {
                return SiteLicenseResult.InvalidCode;
            }

            var isSite = lic.partmaster.parttype.PanDbIdentification.Contains(SitePartTypePanDbIdentification);

            return isSite ? SiteLicenseResult.Valid : SiteLicenseResult.NotASiteLicense;
	    }

	    public bool DoesCspLicenseHaveAsc(string authCode) {
			using (var db = new cspEntities()) {
				var lic = db.licenses.Single(x => x.AuthorizationCode == authCode);

				return lic.SupportCenterSupportAccountId.HasValue;
			}
		}

		public long[] GetSupportAccountIdBySalesforceAccountId(string salesforceAccountId) {
			using (var db = new cspEntities()) {
				return
					(
						from sa in db.supportaccounts
						where sa.SalesforceAccountId == salesforceAccountId
						&& sa.IsActive == true
						select sa.SupportAccountId
						).ToArray();
			}
		}

		public void UpdateLicenseAsc(string authCode, long ascSupportAccountId) {
			using (var db = new cspEntities()) {
				var lic = db.licenses.Single(x => x.AuthorizationCode == authCode);
				lic.SupportCenterSupportAccountId = ascSupportAccountId;
				db.SaveChanges();
			}
		}

		public void LogToAscInformationReview(long licenseId, string ascSalesforceAccountId, string ascName, string authCode,
																					string reason) {
			using (var db = new cspEntities()) {
				db.ascinformationreviews.Add(new ascinformationreview {
					LicenseId = licenseId,
					ASCId = ascSalesforceAccountId,
					ASCName = ascName,
					AuthorizationCode = authCode,
					Reason = reason,
				});

				db.SaveChanges();
			}
		}


        public List<long> GetActivatedTrialLicenseId(string serialNumber, List<string> partIds)
        {
            using (var db = new cspEntities())
            {
                return
                    (
                        from l in db.licenses
                        join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
                        join d in db.devices on l.DeviceId equals d.DeviceId
                        where l.IsActive && d.SerialNumber == serialNumber && partIds.Contains(pm.PartNumber)
                        select l.LicenseId
                        ).ToList();
            }
        }

				#region ILicenseRepository Members


				public bool UpdateSapIdAndSfdcIdForAwsAccount(string authCode, long supportAccountId, long userAccountId, out string currentSfdcAccountId, out string newSfdcAccountId) {
					int? sapId = null;
					bool hasChanges = false;
					currentSfdcAccountId = string.Empty;
					newSfdcAccountId = string.Empty;
					using (var db = new cspEntities()) {
						var supportaccount =
							(from lic in db.licenses
							 join soitems in db.salesorderitems on lic.LicenseId equals soitems.LicenseId
							 join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
							 join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
							 join sa in db.supportaccounts on map.SupportAccountId equals sa.SupportAccountId
							 where lic.AuthorizationCode == authCode
							 select new { sa.SalesforceAccountId, map.SAPId }).FirstOrDefault();

						if (supportaccount != null) {
							newSfdcAccountId = supportaccount.SalesforceAccountId.Trim();
							sapId = supportaccount.SAPId;

							if (sapId.HasValue) {
								db.sapsupportaccountmappings.Add(new sapsupportaccountmapping() {
									CreatedByUserAccountId = userAccountId,
									CreatedOn = System.DateTime.Now,
									SAPId = sapId.Value,
									SupportAccountId = supportAccountId,
									IsPrimary = true
								});
								hasChanges = true;
							}
						}

						var awsSupportAccount = (from sa in db.supportaccounts
																		 where sa.SupportAccountId == supportAccountId
																		 select sa).FirstOrDefault();

						if (awsSupportAccount != null && string.IsNullOrEmpty(newSfdcAccountId) == false) {
							currentSfdcAccountId = awsSupportAccount.SalesforceAccountId;
							hasChanges = true;
							awsSupportAccount.SalesforceAccountId = newSfdcAccountId;
							awsSupportAccount.LastUpdate = System.DateTime.Now;
						}

						if (hasChanges) {
							return db.SaveChanges() > 0;
						}

					}
					return false;
				}

				public bool IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(long supportAccountId, string awsDefaultAccountId) {
					bool result = false;
					using (var db = new cspEntities()) {
						string sfdcAccountId = "";

						// Comments By LK On 02/07/2020
						// Check if the Salesforce Account is an AWS Default Account or not
						var supportAccount = (from sa in db.supportaccounts
																	where sa.SupportAccountId == supportAccountId
																		select sa).FirstOrDefault();

						if (supportAccount != null && !string.IsNullOrEmpty(supportAccount.SalesforceAccountId)) {
							sfdcAccountId = supportAccount.SalesforceAccountId;
						}

						if (sfdcAccountId != awsDefaultAccountId) {
							return false;
						}

						sfdcAccountId = (from asam in db.awsaccountsupportaccountmappings
																 join sa in db.supportaccounts
																	 on asam.SupportAccountId equals sa.SupportAccountId
																 where asam.SupportAccountId == supportAccountId
																 select sa.SalesforceAccountId).FirstOrDefault();

						var hasSapId = db.sapsupportaccountmappings.Any(x => x.SupportAccountId == supportAccountId);
						if ((string.IsNullOrEmpty(sfdcAccountId) || sfdcAccountId == awsDefaultAccountId) && !hasSapId) {
							result = true;
						}
					}
					return result;
				}

				#endregion


        public Api.Model.VmUpgrade GetVmUpgradeInfo(long deviceId, string transactionId)
        {
            using (var db = new cspEntities())
            {
                var upgradeInfo = (from upgrd in db.vmupgrades
                    where upgrd.DeviceId == deviceId && upgrd.TransactionId == transactionId
                    select new VmUpgrade()
                    {
                        DeviceId = upgrd.DeviceId,
                        CreatedBy = upgrd.CreatedBy,
                        CreatedDate = upgrd.CreatedDate,
                        FromModel = upgrd.FromModel,
                        ToModel = upgrd.ToModel,
                        OriginalAuthCode = upgrd.OriginalAuthCode,
                        UpgradedAuthCode = upgrd.UpgradedAuthCode,
                        TransactionId = upgrd.TransactionId,
                        VmUpgradeStatus = upgrd.VmUpgradeStatus
                    }).FirstOrDefault();
                return upgradeInfo;
            }
        }

        public bool IsCloudServiceAuthCode(string authCode)
        {
            var db = new cspEntities();
            var result = from lic in db.licenses
                         join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                         join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId
                         where lic.AuthorizationCode == authCode
                         && ps.PanDbIdentification.Contains("aas")
                         select lic;
            return result.Any();
        }

	    public bool IsPanoramaUpgradeAuthCodeActivation(string serialNumber, string authcode)
	    {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
	        {
	            connection.Open();
                using (var command = new MySqlCommand("IsPanoramaUpgradeAuthCode", connection)
                {
	                CommandType = CommandType.StoredProcedure
	            })
                {
                    command.Parameters.AddWithValue("@p_SerialNumber", serialNumber);
                    command.Parameters.AddWithValue("@p_AuthCode", authcode);
                    var ret = command.ExecuteScalar();
                    int result = 0;
                    if (ret != null && int.TryParse(ret.ToString(), out result) && result != 0)
                    {
                        return true;
                    }
                }
	            connection.Close();
	        }

	        return false;
        }

        public bool IsPae(string authcode)
        {
            const string query = @"
SELECT COUNT(1) > 0 IsPae 
  FROM license l 
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  WHERE l.AuthorizationCode = @p_AuthCode
  AND l.IsActive = 1
  AND p.PartNumber LIKE 'PAN-PRISMA-ACCESS%';
";
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_AuthCode", authcode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.GetColumnValue<bool?>("IsPae") ?? false;
                        }
                    }
                }
                connection.Close();
            }

            return false;
        }

        // 2021-04-26: JK - CSP-6337
        public bool IsSaasInline(string authcode)
        {
            const string query = @"
                SELECT COUNT(1) > 0 IsSaasInline
                  FROM license l 
                  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
                  WHERE l.AuthorizationCode = @p_AuthCode
                  AND l.IsActive = 1
                  AND p.PartNumber LIKE '%SAAS-INLINE%';
                ";
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_AuthCode", authcode);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.GetColumnValue<bool?>("IsSaasInline") ?? false;
                        }
                    }
                }
                connection.Close();
            }

            return false;
        }

    }
}
