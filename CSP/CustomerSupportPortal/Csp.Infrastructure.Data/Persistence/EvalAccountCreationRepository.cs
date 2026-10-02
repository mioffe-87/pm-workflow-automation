using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using Csp.Api.HelperClasses;
using System.Data;
using Csp.Api.Model;

namespace Csp.Infrastructure.Data.Persistence {
	public class EvalAccountCreationRepository : IEvalAccountCreationRepository {

		public bool SupportAccountExistsBySfAccountId(string sfAccountId) {

			sfAccountId = TrimSfAccountId(sfAccountId);

			using (var db = new cspEntities()) {
				var anySupportAccounts = db.supportaccounts.Where(x => x.IsActive == true && x.SalesforceAccountId.Substring(0, 15) == sfAccountId).Select(x => x.SalesforceAccountId.Substring(0, 15));
				//Must do the comparison in memory as it is CASE SENSITIVE
				return anySupportAccounts.Contains(sfAccountId.Substring(0, 15));
			}
		}

		public long CreateEvalAccount(string sfAccountId, string name, string phoneNumber, string addr1, string addr2, string city,
																	string region, string postalCode, string country, string createdByEmail) {

			if (string.IsNullOrEmpty(sfAccountId) || string.IsNullOrEmpty(name))
				throw new Exception("Invalid eval account data");

			using (var db = new cspEntities()) {

				//const string sql = "CALL AddUpdateEvalSupportAccount(@salesforceAccountId, @name, @phoneNumber, @addr1, @addr2, @city, @region, @postalCode, @country, @createdByUserAccountId);";
				const string addUpdateEvalSupportAccountSql = "CALL AddUpdateEvalSupportAccount({0},{1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9});";

				var createdByUserAccountId = GetCreatedByUserAccountId(createdByEmail);

				var countryName = ValidateCountryName(country);

				db.Database.ExecuteSqlCommand(addUpdateEvalSupportAccountSql, sfAccountId, name, phoneNumber, addr1, addr2, city, region, postalCode, countryName, createdByUserAccountId);

				sfAccountId = TrimSfAccountId(sfAccountId);

				const string getSupportAccountByIdSql = "SELECT s.SupportAccountId FROM supportaccount s WHERE BINARY s.IsActive = 1 AND SUBSTR(s.SalesforceAccountId, 1, 15) = {0} ORDER BY SupportAccountId DESC LIMIT 1";

				var supportAccountIds = db.Database.SqlQuery<long>(getSupportAccountByIdSql, sfAccountId);

				return supportAccountIds.First();
			}
		}

		private string ValidateCountryName(string country) {

			using (var db = new cspEntities()) {
				var user = db.countries.SingleOrDefault(x => x.Name == country);

				if (user != null)
					return country;


				const string defaultCountry = "United States";

				return defaultCountry;
			}
		}

		private long GetCreatedByUserAccountId(string createdByEmail) {

			using (var db = new cspEntities()) {
				var user = db.useraccounts.SingleOrDefault(x => x.Email == createdByEmail);

				if (user != null)
					return user.UserAccountId;


				const string defaultCreator = "donotreplycsp@paloaltonetworks.com";

				return db.useraccounts.Single(x => x.Email == defaultCreator).UserAccountId;
			}
		}

		public void LogEvalAccountCreation(bool isEval, long? deviceId, long? licenseId, string evalId, string sfAccountId,
																			 long? supportAccountId) {

			using (var db = new cspEntities()) {

				var alreadyExists =
					db.evalaccountcreationlogs.Any(x => x.DeviceId == deviceId && x.LicenseId == licenseId && x.EvalId == evalId);

				if (alreadyExists) {
					return;
				}

				var log = new evalaccountcreationlog {
					IsEval = isEval,
					DeviceId = deviceId,
					LicenseId = licenseId,
					EvalId = evalId,
					SfAccountId = sfAccountId,
					SupportAccountId = supportAccountId,
					CreatedOn = DateTime.Now,
				};

				db.evalaccountcreationlogs.Add(log);
				db.SaveChanges();
			}
		}

		public LatestIdsModel GetLatestInspectedIds() {

			using (var db = new cspEntities()) {
				var deviceId = db.evalaccountcreationlogs.Max(x => x.DeviceId);
				var licenseId = db.evalaccountcreationlogs.Max(x => x.LicenseId);

				return new LatestIdsModel {
					DeviceId = deviceId,
					LicenseId = licenseId,
				};
			}
		}

		public IEnumerable<EvalDeviceModel> GetDevicesCodesToProcess(LatestIdsModel latestInspectionIds) {

			var model = new List<EvalDeviceModel>();

			using (var db = new cspEntities()) {

				var deviceQuery = db.devices.Select(x => x);

				if (latestInspectionIds.DeviceId.HasValue) {
					deviceQuery = deviceQuery.Where(x => x.DeviceId > latestInspectionIds.DeviceId.Value);
				}

                var devices = deviceQuery.Where(x => x.SerialNumber != null && x.CustomerServiceNotes.StartsWith("E")).OrderBy(x => x.DeviceId).Select(x => new EvalDeviceModel
                {
					Type = EvalDeviceModel.EvalDeviceType.Panorama,
					Id = x.DeviceId,
					DeviceCode = x.SerialNumber,
					EvalId = x.CustomerServiceNotes,
				});

				model.AddRange(devices);

				var licenseQuery = db.licenses.Select(x => x);

				if (latestInspectionIds.LicenseId.HasValue) {
					licenseQuery = licenseQuery.Where(x => x.LicenseId > latestInspectionIds.LicenseId.Value);
				}

                var licenses = licenseQuery.Where(x => x.AuthorizationCode != null && x.Notes.StartsWith("E")).OrderBy(x => x.LicenseId).Select(x => new EvalDeviceModel
                {
					Type = EvalDeviceModel.EvalDeviceType.Vm,
					Id = x.LicenseId,
					DeviceCode = x.AuthorizationCode,
					EvalId = x.Notes,
				});

				model.AddRange(licenses);

				return model;
			}
		}

		public string[] GetVmsByAuthCode(string authCode) {

			using (var db = new cspEntities()) {
                var vms = new List<string>();
				var filteredLicenses = db.licenses.Where(x => x.AuthorizationCode == authCode && x.VMStubDeviceId != 0);
                if (IsCloudServciceAuthCode(authCode))
                {
                    if (filteredLicenses.Join(db.partmasters, l => l.PartMasterId, pm => pm.PartMasterId, (l, pm) => pm).Any(i => i.PartNumber.StartsWith("PAN-PRISMA-ACCESS")))
                    {
                        vms = (from l in filteredLicenses
                               join dev in db.devices on l.DeviceId equals dev.DeviceId
                               select dev.SerialNumber).Distinct().OrderBy(x => x).ToList();
                    }
                    else
                    {
                        vms = (
                                            from l in filteredLicenses
                                            join dev in db.devices on l.VMStubDeviceId equals dev.DeviceId
                                            select dev.SerialNumber
                                        ).Distinct().OrderBy(x => x).ToList();
                    }
                }
                else{
				vms = (
										from l in filteredLicenses
										join vmLics in db.licenses on l.VMStubDeviceId equals vmLics.VMStubDeviceId
										join dev in db.devices on vmLics.DeviceId equals dev.DeviceId
										where
											l.VMStubDeviceId != dev.DeviceId
										select dev.SerialNumber
									).Distinct().OrderBy(x => x).ToList();
                }

				//TRAPS HACK FIX
				var results = vms.ToArray();

				if (results.Length == 0) {
					results =
						(
							from l in db.licenses
							join d in db.devices on l.DeviceId equals d.DeviceId
							where l.AuthorizationCode == authCode
										&& l.partmaster.PartNumber.Contains("-TRAPS-")
							select d.SerialNumber
						).ToArray();
				}

				return results;
			}
		}

		public void LogVmEvalRequest(string deviceCode, string evalId, string salesforceAccountId) {
			using (var db = new cspEntities()) {

				var alreadyExists = db.vmevals.Any(x => x.DeviceCode == deviceCode && x.EvalId == evalId);

				if (alreadyExists) {
					return;
				}

				db.vmevals.Add(new vmeval {
					DeviceCode = deviceCode,
					EvalId = evalId,
					SalesforceAccountId = salesforceAccountId,
					CreatedOn = DateTime.Now,
				});
				db.SaveChanges();
			}
		}

		public void LogEvalSfdcMappings(string evalId, string sfdcId, long supportAccountId) {
			using (var db = new cspEntities()) {

				var evalAlreadyMapped = db.evalsfdcmappings.Any(x => x.EvalId == evalId);

				if (evalAlreadyMapped) return;

				db.evalsfdcmappings.Add(new evalsfdcmapping {
					EvalId = evalId,
					SalesforceAccountId = sfdcId,
					SupportAccountId = supportAccountId,
				});
				db.SaveChanges();
			}
		}

		private string TrimSfAccountId(string sfAccountId) {
			if (sfAccountId == null || sfAccountId.Length <= 15) {
				return sfAccountId;
			}

			return sfAccountId.Substring(0, 15);
		}

        public bool IsCloudServciceAuthCode(string authCode )
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var result = (from lic in db.licenses 
                              join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                              join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId
                              where lic.AuthorizationCode == authCode
                              && (ps.PanDbIdentification.Contains("aas") || ps.PanDbIdentification.ToLower().Contains("mgfr") || pm.PartNumber.StartsWith("PAN-PRISMA-ACCESS"))
                              select lic).Any();
                return result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        /// <summary>
        /// Get Serial Number by Auth Code.
        /// </summary>
        /// <param name="authCode">Auth Code.</param>
        /// <returns>Serial Number of the Auth Code.</returns>
        public string GetSnByAuthCode(string authCode)
        {
            const string query = @"
SELECT l.AuthorizationCode, d.SerialNumber
  FROM license l
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE l.AuthorizationCode = @p_AuthCode
  AND l.IsActive = 1
  AND d.IsActive = 1;
";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new MySqlParameter("p_AuthCode", authCode));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString("SerialNumber");
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get Serial Numbers linked to Auth Codes.
        /// </summary>
        /// <param name="authCodes">Collection of Auth Codes to get the Serial Numbers for.</param>
        /// <returns>Collection of Auth Codes and linked to them Serial Numbers.</returns>
        public IEnumerable<SnAuthCode> GetSnsByAuthCodes(IEnumerable<string> authCodes)
        {
            string authCodesFlattend = string.Join(",", authCodes.Select(i => "'" + i + "'").ToArray());
            string query = @"
SELECT l.AuthorizationCode, d.SerialNumber
  FROM license l
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE l.AuthorizationCode IN ({0})
  AND l.IsActive = 1
  AND d.IsActive = 1;
".Replace("{0}", authCodesFlattend);
            List<SnAuthCode> items = new List<SnAuthCode>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new MySqlParameter("p_AuthCodes", authCodesFlattend));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new SnAuthCode
                            {
                                AuthCode = reader.GetString("AuthorizationCode"),
                                SerialNumber = reader.GetString("SerialNumber")
                            });
                        }
                    }
                }
            }

            return items;
        }

        public IEnumerable<TempEvalDeviceModel> GetDevicesCodesFromTempTable(int limit)
        {
            var evalDeviceModelList = new List<TempEvalDeviceModel>();

            try
            {
                using (var connection = new MySqlConnection(Secrets.TempDatabase))
                {
                    connection.Open();
                    //get all eval record in the temp table pending to create
                    var query = @"SELECT * FROM temp.vmdeltaevals where Iscreated <> 1 LIMIT @p_limit;";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_limit", limit);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var evalDeviceModel = new TempEvalDeviceModel
                                {
                                    Type = EvalDeviceModel.EvalDeviceType.Vm,
                                    Id = Convert.ToInt64(reader.GetString("ProductId")),
                                    DeviceCode = reader.GetString("DeviceCode"),
                                    EvalId = reader.GetString("EvalId"),
                                    SfAccountId = reader.GetString("SalesforceAccountId"),
                                    Name = reader.GetString("Name"),
                                    PhoneNumber = reader.GetString("PhoneNumber"),
                                    City = reader.GetString("City"),
                                    Region = reader.GetString("Region"),
                                    PostalCode = reader.GetString("PostalCode"),
                                    Country = reader.GetString("Country"),
                                };
                                evalDeviceModelList.Add(evalDeviceModel);
                            }
                        }
                        connection.Close();
                    }
                }
            }

            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
            return evalDeviceModelList;
        }
        public void UpdateEvalTempRecord(bool isEval, long productId, string evalId, string sfAccountId,
                                                                             long? supportAccountId)
        {

            try
            {
                using (var connection = new MySqlConnection(Secrets.TempDatabase))
                {
                    connection.Open();

                    var query = "update temp.vmdeltaevals set Iscreated = 1  where ProductId=@p_productId and SalesforceAccountId=@p_sfAccountId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@p_productId", productId);
                        command.Parameters.AddWithValue("@p_sfAccountId", sfAccountId);
                        command.ExecuteScalar();
                    }
                    connection.Close();
                }
            }

            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

        }
    }
}
