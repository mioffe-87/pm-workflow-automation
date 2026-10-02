using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using Csp.Api.HelperClasses;

namespace Csp.Infrastructure.Data.Persistence {
    public class VmRegistrationRepository : IVmRegistrationRepository
    {
	    private readonly IConnectionStrings _connectionStrings;

	    public VmRegistrationRepository(IConnectionStrings connectionStrings)
	    {
	        _connectionStrings = connectionStrings;
	    }

        public long[] GetRegisteredDevicesWithoutSupportAccountIds(EVmType vmType)
			{
            {
				var db = SessionDbContext.CspObjectContext;

				var dayAfterRelease = new DateTime(2013, 12, 7);

				var deviceIds =
					from d in db.devices
					where d.RegistrationDate != null
								&& d.SupportAccountId == null
								&& d.CreatedOn >= dayAfterRelease
					select d.DeviceId;

				return deviceIds.ToArray();
			}
		}

        public long? GetRegisteringUserSupportAccountIfOnlyOne(EVmType vmType, long deviceId)
        {

			const long systemId = 1;

			{
				var db = SessionDbContext.CspObjectContext;

				var creatorId = db.devices.Single(x => x.DeviceId == deviceId).CreatedByUserAccountId;
                if (creatorId == systemId)
                {
					return null;
				}

				var memberships =
					(
					from m in db.memberships
					join mr in db.membershiproles on m.MembershipId equals mr.MembershipId
					join r in db.roles on mr.RoleId equals r.RoleId
					where m.UserAccountId == creatorId
								&& m.SupportAccountId != null
                    select new
                    {
						SupportAccountId = m.SupportAccountId.Value,
						r.Category,
					}).ToArray();

                if (!memberships.Any())
                {
					return null;
				}

				var anyTacOrAsc =
					memberships.Any(x =>
						x.Category == (int)RoleSecurity.SecurityClassifications.Asc ||
						x.Category == (int)RoleSecurity.SecurityClassifications.Tac);

                if (anyTacOrAsc)
                {
					return null;
				}

				var supportAccountIds = memberships.Select(x => x.SupportAccountId).Distinct().ToArray();
                if (supportAccountIds.Length != 1)
                {
					return null;
				}

				return supportAccountIds.Single();
			}
		}

        public long? GetRelatedVmSupportAccountIfExists(EVmType vmType, long deviceId)
			{
            {
				var db = SessionDbContext.CspObjectContext;

				var licenses = db.licenses.Where(x => x.DeviceId == deviceId).ToArray();

                var stubDeviceIds = licenses.Where(x => x.VMStubDeviceId != null && x.VMStubDeviceId > 0)
                    .Select(x => x.VMStubDeviceId.Value).Distinct();

                foreach (var stubDeviceId in stubDeviceIds)
                {
					var device = db.devices.Single(x => x.DeviceId == stubDeviceId);
                    if (device.SupportAccountId.HasValue)
                    {
						return device.SupportAccountId.Value;
					}
				}

                var parentLicenseIds = licenses.Where(x => x.VMParentLicenseId != null && x.VMParentLicenseId > 0)
                    .Select(x => x.VMParentLicenseId.Value).Distinct();

                foreach (var licId in parentLicenseIds)
                {
					var device = (
												 from lic in db.licenses
												 join dev in db.devices on lic.DeviceId equals dev.DeviceId
												 where lic.LicenseId == licId
												 select dev).SingleOrDefault();
                    if (device != null && device.SupportAccountId.HasValue)
                    {
						return device.SupportAccountId.Value;
					}
				}

				return null;
			}
		}

        public void SetDeviceSupportAccountId(EVmType vmType, long deviceId, long supportAccountId)
			{
            {
				var db = SessionDbContext.CspObjectContext;

				var dev = db.devices.Single(x => x.DeviceId == deviceId && x.SupportAccountId == null);
				dev.SupportAccountId = supportAccountId;
				db.SaveChanges();
			}
		}

		private readonly IEnumerable<string> _panoranamIds = new string[]
			{
				"PHOENIX10ASB",
				"PHOENIX10BSB",
				"PHOENIX10APB",
				"PHOENIX10BPB",
				"PHOENIX10ESB",
                "PHOENIX1ESB",
                "PHOENIX3ESB",
                "PHOENIX50ESB",
                "PHOENIX500ESB",
                "PHOENIX700ESB"
			};

        private const string CnPartNumber = "PAN-PA-CN-SW";
        private const string VmPartNumber = "PAN-PA-VM-SW";
        private const string VmPstPanDbId = "PHOENIX";

        public IEnumerable<AvailableVirtualSeries<AvailableCn>> GetAvailableCns(DataParameters dataParameters, long supportAccountId, long userAccountId, 
            string authCode, string serialNumber, bool onlyGetCsspVms = false)
	    {
            var result = new Dictionary<long, AvailableVirtualSeries<AvailableCn>>();
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand("GetAvailableCns", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("@p_UserAccountId", userAccountId);
                    command.Parameters.AddWithValue("@p_SerialNumber", serialNumber);
                    command.Parameters.AddWithValue("@p_AuthorizationCode", authCode);
                    command.Parameters.AddWithValue("@p_PaginationOffset", dataParameters.PaginationOffset);
                    command.Parameters.AddWithValue("@p_PaginationCount", dataParameters.PaginationCount);
                    command.Parameters.AddWithValue("@p_SortField", dataParameters.SortField);
                    command.Parameters.AddWithValue("@p_SortDirection", dataParameters.SortDirection.ToSqlOrderBy());
                    command.Parameters.AddWithValue("@p_Searchfield", DBNull.Value);
                    command.Parameters.AddWithValue("@p_SearchValue", DBNull.Value);
                    var outParam = new MySqlParameter();
                    outParam.ParameterName = "p_TotalRecordCount";
                    outParam.MySqlDbType = MySqlDbType.Int64;
                    outParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outParam);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetColumnValue<long>("LicenseId"),
                                new AvailableVirtualSeries<AvailableCn>()
                                {
                                    AuthorizationCode = reader.GetColumnValue<string>("AuthorizationCode"),
                                    ProductSku = reader.GetColumnValue<string>("PartNumber"),
                                    Description = reader.GetColumnValue<string>("Description"),
                                    VmUseCount = reader.GetColumnValue<int>("VmUseCount"),
                                    AscName = reader.GetColumnValue<string>("AscName"),
                                    ExpirationDate = reader.GetColumnValue<DateTime?>("ExpirationDate"),
                                    Vms = new List<AvailableCn>()
                                });
                        }
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                long key = reader.GetColumnValue<long>("LicenseId");
                                if (result.Keys.Contains(key))
                                {
                                    var parent = result[key];
                                    ((List<AvailableCn>)parent.Vms).Add(new AvailableCn
                                    {
                                        SerialNumber = reader.GetColumnValue<string>("SerialNumber"),
                                        ProductSku = reader.GetColumnValue<string>("PartNumber"),
                                        VmTotalCount = 0,
                                        VmUseCount = reader.GetColumnValue<int>("TokenCount"),
                                        MonthlyPeakUsage = reader.GetColumnValue<string>("MonthlyPeakUsage"),
                                        MonthlyAverageUsage = reader.GetColumnValue<string>("MonthlyAverageUsage"),
                                        PanoramaLicenseId = reader.GetColumnValue<string>("PanoramaLicenseId")
                                    });
                                }
                            }
                        }
                    }

                    if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                    {
                        dataParameters.TotalRecordCount = (long)command.Parameters["p_TotalRecordCount"].Value;
                    }
                }

                foreach (var cn in result.Values)
                {
                    if (cn.Vms != null
                        && cn.Vms.Any())
                    {
                        try
                        {
                            var monthlyUsage = cn.Vms.Where(v => !string.IsNullOrWhiteSpace(v.MonthlyPeakUsage)).Select(v => int.Parse(v.MonthlyPeakUsage));
                            if (monthlyUsage.Any())
                            {
                                cn.MonthlyPeakUsageTotal = monthlyUsage.Sum().ToString();
                            }
                        }
                        catch (Exception exi)
                        {
                            cn.MonthlyPeakUsageTotal = "Error";
                        }

                        cn.VmAllocated = cn.Vms.Sum(i => ((AvailableCn) i).VmUseCount);

                        cn.VmTotalCount = cn.VmUseCount + cn.VmAllocated;
                        ((List<AvailableCn>)cn.Vms).ForEach(v => v.VmTotalCount = cn.VmTotalCount);
                    }
                }
            }

            return result.Values;
	    }

        //optimzied code which fixed the performance issue.
        public IEnumerable<AvailableVm> GetAvailableVms(DataParameters dataParameters, long supportAccountId, long userAccountId, string authCode, bool onlyGetCsspVms = false)
        {
            var result = new List<AvailableVm>();
            IEnumerable<string> excludeNsxPartIds = new string[] { };
            var excludePartIds = System.Configuration.ConfigurationManager.AppSettings["ExcludePanoramaForPartIds"];
            if (excludePartIds != null)
            {
                excludeNsxPartIds = excludePartIds.Contains(",") ? excludePartIds.Split(',') : new string[] { excludePartIds };
            }

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand("GetAvailableVmsAndCsspVms", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("@p_UserAccountId", userAccountId); //2483
                    command.Parameters.AddWithValue("@p_onlyGetCsspVms", onlyGetCsspVms);
                    command.Parameters.AddWithValue("@p_AuthCode", authCode);
                    command.Parameters.AddWithValue("@p_PaginationOffset", dataParameters.PaginationOffset);
                    command.Parameters.AddWithValue("@p_PaginationCount", dataParameters.PaginationCount);
                    if (dataParameters.SortField == "AuthorizationCode" || dataParameters.SortField == "Description")
                    {
                        command.Parameters.AddWithValue("@p_SortField", dataParameters.SortField);
                        command.Parameters.AddWithValue("@p_SortDirection",
                            dataParameters.SortDirection.ToSqlOrderBy());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@p_SortField", DBNull.Value);
                        command.Parameters.AddWithValue("@p_SortDirection", DBNull.Value);
                    }
                    command.Parameters.AddWithValue("@p_Searchfield", DBNull.Value);
                    command.Parameters.AddWithValue("@p_SearchValue", DBNull.Value);
                    var outParam = new MySqlParameter();
                    outParam.ParameterName = "p_TotalRecordCount";
                    outParam.MySqlDbType = MySqlDbType.Int64;
                    outParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outParam);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var panDbIdentification = reader.GetColumnValue<string>("PanDbIdentification");
                            var isPanorama = _panoranamIds.Contains(panDbIdentification);
                            var licenseType = reader.GetColumnValue<short?>("LicenseType");
                            var isPerpetualPanorama = isPanorama && (licenseType.HasValue && licenseType.Value != 0);
                            var enablePanGen = !excludeNsxPartIds.Contains(reader.GetColumnValue<string>("PartNumber"));
                            var vmUseCount = reader.GetColumnValue<int>("VMUseCount");
                            var vmQuantity = reader.GetColumnValue<int?>("VMQuantity");
                            vmQuantity = vmQuantity ?? 0;
                            var deviceId = reader.GetColumnValue<string>("DeviceId");
                            var isVmElaAuthCode = reader.GetColumnValue<bool>("IsVmElaAuthCode");
                            var ascName = reader.GetColumnValue<string>("AscName");
                            var vm = new AvailableVm
                            {
                                AuthorizationCode = reader.GetColumnValue<string>("AuthorizationCode"),
                                Description = reader.GetColumnValue<string>("Description"),
                                ProductSku = reader.GetColumnValue<string>("PartNumber"),
                                VmUseCount = (vmUseCount == 0 && deviceId == null && (vmQuantity ?? 0) == 0) ? 1 : vmUseCount,
                                VmTotalCount = (vmQuantity.Value == 0) && (!isPanorama || isPerpetualPanorama) ? 1 : vmQuantity.Value,
                                IsNsx = isPanorama ? 1 : 0,
                                IsPerpetualNsx = isPerpetualPanorama ? 1 : 0,
                                AllowGeneratingPanorama = (isPanorama && enablePanGen) ? 1 : 0,
                                IsPerpetualVm = licenseType.HasValue && (licenseType.Value != 0),
                                ExpirationDate = reader.GetColumnValue<DateTime?>("ExpirationDate"),
                                IsEval = panDbIdentification.ToUpper().Contains("EVAL"),
                                IsVmRenewalEligible = (vmQuantity > 0),
                                IsVmElaAuthCode = isVmElaAuthCode,
                                AscName = string.IsNullOrEmpty(ascName) ? string.Empty : ascName
                            };

                            if (isVmElaAuthCode)
                            {
                                vm.VmUseCount = vmUseCount;
                            }

                            result.Add(vm);
                        }
                    }

                    if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                    {
                        dataParameters.TotalRecordCount = (long)command.Parameters["p_TotalRecordCount"].Value;
                    }
                }
            }

            return result;
        }

        public bool IsInTheMiddelOfUpgrade(EVmType vmType, string serialNumber)
        {
            var db = SessionDbContext.CspObjectContext;
            return (from d in db.devices
                join l in db.licenses on d.DeviceId equals l.DeviceId
                where l.IsActive && d.SerialNumber == serialNumber &&
                      (l.TransactionId != null && l.TransactionId != string.Empty)
                select d.DeviceId).Any();
        }


        public void GetVmUpgradeModels(EVmType vmType, string vmSerialNumber, string upgradeAuthCode, out string fromModel,
            out string toModel)
        {
            var db = SessionDbContext.CspObjectContext;

            var devPartNum = (from d in db.devices
                join pm in db.partmasters on d.PartMasterIdAsShipped equals pm.PartMasterId
                where d.SerialNumber == vmSerialNumber
                                   select pm.PartNumber).FirstOrDefault();

            fromModel = devPartNum != null ? devPartNum.Replace("PAN-PA-", "") : string.Empty;
            
            var partIdUpgraded = (from l in db.licenses
                join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
                                  where l.AuthorizationCode == upgradeAuthCode
                                  select pm.PartNumber).FirstOrDefault();

            toModel = partIdUpgraded != null ? partIdUpgraded.Replace("PAN-", "") : string.Empty;

        }


        public string GetVmAuthCodeBeforeUpgrade(EVmType vmType, string vmSerialNumber)
        {
            var db = SessionDbContext.CspObjectContext;

            var authCode = (from x in db.licenses
                join y in db.devices on x.DeviceId equals y.DeviceId
                where x.IsActive && y.SerialNumber == vmSerialNumber && x.AuthorizationCode.StartsWith("V")
                select x.AuthorizationCode).FirstOrDefault();

            return authCode;

        }


        public bool StoreVmUpgrade(EVmType vmType, long deviceId, string transactionId, string fromModel, string toModel,
            string originalAuthCode, string upgradeAuthCode, string createdBy)
        {
            var db = SessionDbContext.CspObjectContext;

            db.vmupgrades.Add(new vmupgrade()
            {
                DeviceId = deviceId,
                TransactionId = transactionId,
                OriginalAuthCode = originalAuthCode,
                UpgradedAuthCode = upgradeAuthCode,
                FromModel = fromModel,
                ToModel = toModel,
                CreatedBy = createdBy,
                CreatedDate = System.DateTime.Now
            });

            return db.SaveChanges() > 0;

        }


        public void GetVmUpgradeModels(EVmType vmType, long deviceId, string transactionId, out string originalVmAuthCode,
            out string currentVmAuthCode, out string fromModel, out string toModel)
        {
            var db = SessionDbContext.CspObjectContext;
            var upgradeInfo =
                (from upgrd in db.vmupgrades
                    where upgrd.DeviceId == deviceId && upgrd.TransactionId == transactionId
                    select new
                    {
                        upgrd.FromModel,
                        upgrd.ToModel,
                        upgrd.OriginalAuthCode,
                        upgrd.UpgradedAuthCode
                    }).FirstOrDefault();

            originalVmAuthCode = upgradeInfo != null ? upgradeInfo.OriginalAuthCode : string.Empty;
            currentVmAuthCode = upgradeInfo != null ? upgradeInfo.UpgradedAuthCode : string.Empty;
            fromModel = upgradeInfo != null ? upgradeInfo.FromModel : string.Empty;
            toModel = upgradeInfo != null ? upgradeInfo.ToModel : string.Empty;
        }


        public bool IsUpgradeComplete(EVmType vmType, long deviceId)
        {
            var db = SessionDbContext.CspObjectContext;
            var isUpgradeComplete = (from x in db.licenses
                join y in db.vmupgrades on x.AuthorizationCode equals y.UpgradedAuthCode
                where x.DeviceId == deviceId
                      && x.IsActive && y.VmUpgradeStatus == "Complete"
                select x.DeviceId).Any();
            return isUpgradeComplete;
        }


        public string GetOriginalModelAfterRollback(EVmType vmType, string transactionId, string upgradeAuthcode)
        {
            var db = SessionDbContext.CspObjectContext;

            var deviceModel = (from up in db.vmupgrades
                join d in db.devices on up.DeviceId equals d.DeviceId
                join pm in db.partmasters on d.PartMasterId equals pm.PartMasterId
                where
                    up.TransactionId == transactionId && up.UpgradedAuthCode == upgradeAuthcode &&
                    up.VmUpgradeStatus == "Rollback"
                select pm.PartNumber).FirstOrDefault();

            return deviceModel;

        }


        public bool CanUserUpgradeVm(EVmType vmType, string email)
        {
            var db = SessionDbContext.CspObjectContext;

            bool hasAccess = false;

            bool isInBeta =
                (from b in db.betaaccesses where b.FeatureName == "ROME_VM_UPGRADE" select b.FeatureName).Any();

            if (isInBeta)
            {
                hasAccess =
                    (from b in db.betaaccesses
                        where b.FeatureName == "ROME_VM_UPGRADE" && b.EmailAddress == email
                        select b.EmailAddress).Any();
                return hasAccess;
            }
            else
            {
                return true;
            }
        }


        public int GetVmUseCountByAuthCode(EVmType vmType, string authCode)
        {
            var db = SessionDbContext.CspObjectContext;
            return db.licenses.Where(x => x.AuthorizationCode == authCode && x.IsActive)
                .Select(x => x.VMUseCount)
                .FirstOrDefault();
        }

        public string GetLatestQuoteInfo(EVmType vmType, long supportAcctId, string authCode, out bool selectAll, out bool lockUi, out bool hasPendingRenewals)
        {
            selectAll = false;
            lockUi = false;
            hasPendingRenewals = false;
            var latestQuoteNumber = string.Empty;
            if (supportAcctId < 1 || string.IsNullOrEmpty(authCode))
                return string.Empty;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                //get the latest quote in vmrenewal table (order by QuoteDate descending order since last fulfilled date.
                var query = @"
                        SELECT v.QuoteNumber FROM vmrenewal v
   		                WHERE v.SupportAccountId = ?p_SupportAccountId AND v.AuthorizationCode = ?p_AuthCode AND v.Status != 2
                        AND v.RequestDate > (SELECT QuoteDate FROM (
                            SELECT COUNT(*), CASE 
                                WHEN v1.QuoteDate IS NULL THEN 0
                                ELSE v1.QuoteDate
                            END AS QuoteDate
                            FROM vmrenewal v1 WHERE v1.SupportAccountId = ?p_SupportAccountId AND v1.AuthorizationCode = ?p_AuthCode AND v1.Status=2 ORDER BY v1.QuoteDate DESC LIMIT 1) AS tmp)
    	                ORDER BY v.QuoteDate DESC LIMIT 1";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    var ret = command.ExecuteScalar();
                    if (ret != null)
                        latestQuoteNumber = ret.ToString();
                    //check vmrenewalunprovisioned table again
                    if (string.IsNullOrEmpty(latestQuoteNumber))
                    {
												command.Parameters.Clear();
                        //get the latest quote in vmrenewalunprovisioned table (order by QuoteDate descending order since last fulfilled date.
                        command.CommandText = @"
                                SELECT v.QuoteNumber FROM vmrenewalunprovisioned v
   		                        WHERE v.Status != 2 AND v.AuthorizationCode = ?p_AuthCode
                                AND v.RequestDate > (SELECT QuoteDate FROM (
                                    SELECT COUNT(*), CASE 
                                        WHEN v1.QuoteDate IS NULL THEN 0
                                        ELSE v1.QuoteDate
                                    END AS QuoteDate
                                    FROM vmrenewalunprovisioned v1 WHERE v1.AuthorizationCode = ?p_AuthCode AND v1.Status=2 ORDER BY v1.QuoteDate DESC LIMIT 1) AS tmp)
    	                        ORDER BY v.QuoteDate DESC LIMIT 1";
                        command.Parameters.AddWithValue("?p_AuthCode", authCode);

                        ret = command.ExecuteScalar();
                        if (ret != null)
                        {
                            latestQuoteNumber = ret.ToString();
                            hasPendingRenewals = true;
    }
                        else
                            latestQuoteNumber = string.Empty;
}

                    //no quoted selections
                    if (string.IsNullOrEmpty(latestQuoteNumber))
                    {
	                    command.Parameters.Clear();
	                    command.CommandText = @"
                            SELECT COUNT(*) FROM vmrenewal v WHERE v.SupportAccountId=?p_SupportAccountId AND v.AuthorizationCode=?p_AuthCode AND v.QuoteNumber IS NULL;";
	                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
	                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            var count = (long) result;
                            selectAll = count == 0;
                            hasPendingRenewals = count > 0;
                        }
                    }
                    else
                    {
                        hasPendingRenewals = true;
                        //check if there is any quote submitted to approval after the latest fulfilled quote and need to lock UI: Status=3 - lockui
                        command.Parameters.Clear();
                        command.CommandText = @"
                            SELECT COUNT(*) FROM 
                            (
                                (SELECT 1 AS lockui FROM vmrenewal v WHERE v.SupportAccountId = ?p_SupportAccountId AND v.AuthorizationCode = ?p_AuthCode AND v.Status = 3 
                                  AND v.RequestDate > (SELECT QuoteDate FROM (
                                    SELECT COUNT(*), CASE 
                                        WHEN v1.QuoteDate IS NULL THEN 0
                                        ELSE v1.QuoteDate
                                    END AS QuoteDate
                                    FROM vmrenewal v1 WHERE v1.SupportAccountId = ?p_SupportAccountId AND v1.AuthorizationCode = ?p_AuthCode AND v1.Status=2 ORDER BY v1.QuoteDate DESC LIMIT 1) AS fulfilltmp) LIMIT 1)
                                UNION ALL
                                (SELECT 1 AS lockui FROM vmrenewalunprovisioned vu WHERE vu.AuthorizationCode = ?p_AuthCode AND vu.Status = 3 
                                  AND vu.QuoteDate > (SELECT QuoteDate FROM (
                                    SELECT COUNT(*), CASE 
                                        WHEN vu1.QuoteDate IS NULL THEN 0
                                        ELSE vu1.QuoteDate
                                    END AS QuoteDate
                                    FROM vmrenewalunprovisioned vu1 WHERE vu1.AuthorizationCode = ?p_AuthCode AND vu1.Status=2 ORDER BY vu1.QuoteDate DESC LIMIT 1) AS fulfilltmp1
                                  ) LIMIT 1)
                            ) AS temp;";
                        command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                        command.Parameters.AddWithValue("?p_AuthCode", authCode);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            lockUi = (long) result > 0;
                        }
                    }
                }
            }

            return latestQuoteNumber;
        }

        //there are pending renewals
        public async Task<IEnumerable<VmRenewalDevice>> GetProvisionedVmsWithPendingRenewals(EVmType vmType, long supportAcctId, string authcode, string quoteNumber, bool selectAll)
        {
            var result = new List<VmRenewalDevice>();
            var defaultToCheck = (string.IsNullOrEmpty(quoteNumber) && selectAll);
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (DbCommand command = new MySqlCommand("GetVmRenewalDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("p_SupportAccountId", supportAcctId));
                    command.Parameters.Add(new MySqlParameter("p_AuthorizationCode", authcode));
                    command.Parameters.Add(new MySqlParameter("p_QuoteNumber", quoteNumber));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sn = reader.GetColumnValue<string>("SerialNumber");
                            if (!string.IsNullOrEmpty(sn))
                            {
                                var info = new VmRenewalDevice
                                {
                                    SerialNumber = sn,
                                    IsSelected = defaultToCheck ? true: reader.GetColumnValue<bool>("IsSelected"),
                                    ExpirationDate = reader.GetColumnValue<DateTime>("ExpirationDate"),
                                    IsDowngraded = reader.GetColumnValue<bool>("IsDowngraded"),
                                    IsForfeited = defaultToCheck ? false : reader.GetColumnValue<bool>("IsForfeited")
                                };
                                int selectType = 0;
                                if (info.IsDowngraded)
                                    selectType = 1;
                                else if(info.IsForfeited)
                                    selectType = 2;
                                info.Status = GetRenewalType(selectType, true, info.ExpirationDate);
                                result.Add(info);
                            }
                        }
                    }
                }
                connection.Close();
            }

            return result;
        }

        //there is no pending renewals
        public async Task<IEnumerable<VmRenewalDevice>> GetProvisionedVmsWithFulfilledRenewals(EVmType vmType, long supportAcctId, string authcode, bool selectAll)
        {
            var result = new List<VmRenewalDevice>();
            var fulfilledQuote = string.Empty;
            var query = @"SELECT v.QuoteNumber FROM vmrenewal v
   		                    WHERE v.SupportAccountId = ?p_SupportAccountId AND v.AuthorizationCode = ?p_AuthCode AND v.Status = 2 ORDER BY v.QuoteDate DESC LIMIT 1;";

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                    command.Parameters.AddWithValue("?p_AuthCode", authcode);

                    var ret = await command.ExecuteScalarAsync();
                    if (ret != null)
                        fulfilledQuote = ret.ToString();

                    //no fulfilled quote
                    if (string.IsNullOrEmpty(fulfilledQuote))
                    {
                        query = @"
                            SELECT d.SerialNumber, vml.ExpirationDate, FALSE AS IsSelected, FALSE AS IsDowngraded, FALSE AS IsForfeited FROM license l
	                        INNER JOIN license vml ON l.LicenseId = vml.VMParentLicenseId
	                        INNER JOIN device d ON d.DeviceId = vml.DeviceId
	                        INNER JOIN partmaster p ON d.PartMasterId = p.PartMasterId
	                        WHERE l.SupportAccountId = ?p_SupportAccountId AND l.AuthorizationCode = ?p_AuthCode AND l.IsActive AND vml.IsActive AND vml.IsActive AND p.PartNumber != 'PAN-PA-VM-SW'
	      	                ORDER BY d.SerialNumber;";
                    }
                    else
                    {
                        query = @"
                            SELECT d.SerialNumber, vml.ExpirationDate, 
                            CASE
		                       WHEN v.SelectionType = 0 THEN TRUE
		                       ELSE FALSE
		                    END AS IsSelected,
		                    CASE
		                      WHEN v.SelectionType = 1 THEN TRUE
		                      ELSE FALSE
		                    END AS IsDowngraded,
		                    CASE 
		                      WHEN v.SelectionType = 2 THEN TRUE
		                      ELSE FALSE
		                    END AS IsForfeited                             
                            FROM license l
	                        INNER JOIN license vml ON l.LicenseId = vml.VMParentLicenseId
	                        INNER JOIN device d ON d.DeviceId = vml.DeviceId
	                        INNER JOIN partmaster p ON d.PartMasterId = p.PartMasterId
	      	                LEFT JOIN vmrenewal v ON (l.AuthorizationCode = v.AuthorizationCode AND v.QuoteNumber = ?p_QuoteNumber AND d.SerialNumber = v.SerialNumber AND v.Status = 2) 
	                        WHERE l.SupportAccountId = ?p_SupportAccountId AND l.AuthorizationCode = ?p_AuthCode AND l.IsActive AND vml.IsActive AND p.PartNumber != 'PAN-PA-VM-SW'
	      	                ORDER BY d.SerialNumber;";
                    }
                    command.CommandText = query;

                    if (!string.IsNullOrEmpty(fulfilledQuote))
                    {
	                    command.Parameters.AddWithValue("?p_QuoteNumber", fulfilledQuote);
                    }

                    //command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                    //command.Parameters.AddWithValue("?p_AuthCode", authcode);

                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sn = reader.GetColumnValue<string>("SerialNumber");
                            if (!string.IsNullOrEmpty(sn))
                            {
                                var info = new VmRenewalDevice
                                {
                                    SerialNumber = sn,
                                    IsSelected = reader.GetColumnValue<bool>("IsSelected"),
                                    ExpirationDate = reader.GetColumnValue<DateTime>("ExpirationDate"),
                                    IsDowngraded = reader.GetColumnValue<bool>("IsDowngraded"),
                                    IsForfeited = reader.GetColumnValue<bool>("IsForfeited")
                                };
                                int selectType = 0;
                                if (info.IsDowngraded)
                                    selectType = 1;
                                else if (info.IsForfeited)
                                    selectType = 2;
                                info.Status = GetRenewalType(selectType, false, info.ExpirationDate);

                                //set them back because there is no pending renewals, we should set it to default
                                info.IsSelected = selectAll;
                                info.IsDowngraded = false;
                                info.IsForfeited = false;
                                result.Add(info);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private string GetRenewalType(int? type, bool pendingRenewal, DateTime expirationDate)
        {
            bool pastDate = expirationDate < DateTime.Now;
            //no renewal record
            if (type != null)
            {
                switch (type.Value)
                {
                    //renew
                    case 0:
                        if (pendingRenewal)
                            return VmRenewalStatus.StatusRenewalPending;
                        else
                            return VmRenewalStatus.StatusActive;
                    //downgraded
                    case 1:
                        if (pendingRenewal)
                            return VmRenewalStatus.StatusRenewalDowngradedPending;
                        else if (pastDate)
                            return VmRenewalStatus.StatusDowngraded;
                        return VmRenewalStatus.StatusDowngradedPending;
                    //forfeited
                    case 2:
                        if (pendingRenewal)
                            return VmRenewalStatus.StatusRenewalForfeitedPending;
                        else if (pastDate)
                            return VmRenewalStatus.StatusForfeited;
                        return VmRenewalStatus.StatusForfeitedPending;
                }
            }

            return VmRenewalStatus.StatusActive;
        }

        public void SaveRenewalVMs(EVmType vmType, long supportAcctId, long userAcctId, VmRenewalModel model, bool allSelected, string quoteNumber)
        {
            if (model == null) 
                return;
            //no selections: check if there are unprovisioned
            if (!model.RenewalDevices.Any())
            {
                if (model.UnprovisionedInfo == null ||
                    (model.UnprovisionedInfo.RenewalQuantity == 0 && model.UnprovisionedInfo.DowngradeQuantity == 0
                     && model.UnprovisionedInfo.ForfeitQuantity == 0))
                    return;
            }
            var firstBatch = true;
            var error = string.Empty;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        using (MySqlCommand command = new MySqlCommand("VMRenewalSave", connection, transaction))
                        {
                            //quote number is empty: insert it to vmrenewal table directly, delete existing ones
                            if (string.IsNullOrEmpty(quoteNumber))
                            {
                                DeleteExistingNotQuotedRecords(vmType, connection, transaction, supportAcctId, model.AuthCode);
                            }

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "VMRenewalSave";

                            command.Parameters.Add(new MySqlParameter("p_SupportAccountId", supportAcctId));
                            command.Parameters.Add(new MySqlParameter("p_AuthorizationCode", model.AuthCode));
                            command.Parameters.Add(new MySqlParameter("p_QuoteNumber", quoteNumber));
                            command.Parameters.Add(new MySqlParameter("p_AllSelected", allSelected));
                            if (allSelected)
                            {
                                //if there are some provisioned
                                if (model.RenewalDevices.Any())
                                {
                                    command.Parameters.Add(new MySqlParameter("p_SerialNumbers", null));
                                    command.Parameters.Add(new MySqlParameter("p_firstBatch", true));
                                    int affectedRows = command.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                //get serial number: the limit in the stored procedure is 5000 chars
                                var serialBuffer = new StringBuilder();
                                var total = model.RenewalDevices.Count();
                                var selectNothing = true;
                                for (int i = 0; i < total; i++)
                                {
                                    var device = model.RenewalDevices.ElementAt(i);
                                    //<serial number>,<selected>,<downgraded>; 5000 bytes maximum
                                    if (serialBuffer.Length + device.SerialNumber.Length + 4 > 20000)
                                    {
                                        selectNothing = false;
                                        command.Parameters.Add(new MySqlParameter("p_SerialNumbers", serialBuffer.ToString()));
                                        command.Parameters.Add(new MySqlParameter("p_firstBatch", firstBatch));
                                        int affectedRows = command.ExecuteNonQuery();
                                        //reset buffer
                                        command.Parameters.RemoveAt("p_SerialNumbers");
                                        command.Parameters.RemoveAt("p_firstBatch");
                                        if (firstBatch)
                                            firstBatch = false;
                                        serialBuffer.Clear();
                                        if (i == total - 1)
                                            break;
                                    }

                                    if (serialBuffer.Length == 0)
                                        serialBuffer.AppendFormat("{0},{1}", device.SerialNumber, (device.IsSelected ? 0 : (device.IsDowngraded ? 1 : 2)));
                                    else
                                        serialBuffer.AppendFormat(";{0},{1}", device.SerialNumber, (device.IsSelected ? 0 : (device.IsDowngraded ? 1 : 2)));
                                }

                                //last part
                                if (serialBuffer.Length > 0)
                                {
                                    command.Parameters.Add(new MySqlParameter("p_SerialNumbers", serialBuffer.ToString()));
                                    command.Parameters.Add(new MySqlParameter("p_firstBatch", firstBatch));
                                    int affectedRows = command.ExecuteNonQuery();
                                }
                                else if (selectNothing && !string.IsNullOrEmpty(quoteNumber))
                                {
                                    //if selected before, delete existing i.e. some selected => non-selected  
                                    DeleteExistingNotQuotedRecords(vmType, connection, transaction, supportAcctId, model.AuthCode, quoteNumber);
                                }
                            }

                            //save unprovisioned VM's renewal info
                            if (model.UnprovisionedInfo != null)
                            {
                                SaveUnprovisionedRenewal(vmType, connection, transaction, model, quoteNumber);
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        error = e.Message;
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("SaveRenewalVMs exception: {0}", e.Message)));
                    }
                }
                connection.Close();
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
        }

        private int DeleteExistingNotQuotedRecords(EVmType vmType, MySqlConnection connection, MySqlTransaction transaction, long supportAccountId, string authCode, string quoteNumber = null)
        {
            if (vmType == EVmType.Cn) // CSP-5524 Multiple VM's reverting Quantity after fix
            {
                return 0;
            }

            string query = string.IsNullOrEmpty(quoteNumber) ?
                @"DELETE FROM vmrenewal WHERE SupportAccountId = ?p_SupportAccountId AND AuthorizationCode = ?p_AuthCode AND QuoteNumber IS NULL;" :
                @"DELETE FROM vmrenewal WHERE SupportAccountId = ?p_SupportAccountId AND AuthorizationCode = ?p_AuthCode AND QuoteNumber =?p_QuoteNumber;";
            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                if (!string.IsNullOrEmpty(quoteNumber))
                {
                    command.Parameters.AddWithValue("?p_QuoteNumber", quoteNumber);
                }

                command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                command.Parameters.AddWithValue("?p_AuthCode", authCode);

                var affectedRows = command.ExecuteNonQuery();
                return affectedRows;
            }
        }

        public async Task<IEnumerable<AvailableVmCsv>> GetChildAuthCodesInfo(EVmType vmType, long supportAcctId, AvailableVmCsv parentAuthCode)
        {
            var result = new List<AvailableVmCsv>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = @"SELECT vml.AuthorizationCode AS ChildAuthCode, d.SerialNumber AS ChildSerialNumber
                        FROM license l
                        INNER JOIN license vml ON l.LicenseId = vml.VMParentLicenseId
                        INNER JOIN device d ON d.DeviceId = vml.DeviceId
                        INNER JOIN partmaster p ON d.PartMasterId = p.PartMasterId
                        WHERE l.SupportAccountId=?p_SupportAccountId AND p.PartNumber != 'PAN-PA-VM-SW' AND l.AuthorizationCode=?p_AuthCode
                        ORDER BY d.SerialNumber;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                    command.Parameters.AddWithValue("?p_AuthCode", parentAuthCode.AuthorizationCode);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var record = new AvailableVmCsv
                            {
                                AuthorizationCode = parentAuthCode.AuthorizationCode,
                                QuantityProvisioned = parentAuthCode.QuantityProvisioned,
                                PartDescription = parentAuthCode.PartDescription,
                                ChildAuthCode = reader.GetColumnValue<string>("ChildAuthCode"),
                                ChildSerialNumber = reader.GetColumnValue<string>("ChildSerialNumber")
                            };
                            result.Add(record);
                        }
                    }
                }
            }

            return result;
        }

        private void SaveUnprovisionedRenewal(EVmType vmType, MySqlConnection connection, MySqlTransaction transaction, VmRenewalModel model, string quoteNumber)
        {
            if (string.IsNullOrEmpty(quoteNumber))
            {
                quoteNumber = null;
            }

            using (MySqlCommand command = new MySqlCommand("VMUnprovisionedRenewalSave", connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "VMUnprovisionedRenewalSave";
                command.Parameters.Clear();
                command.Parameters.Add(new MySqlParameter("p_AuthorizationCode", model.AuthCode));
                command.Parameters.Add(new MySqlParameter("p_QuoteNumber", quoteNumber));
                command.Parameters.Add(new MySqlParameter("p_RenewalQuantity", model.UnprovisionedInfo.RenewalQuantity));
                command.Parameters.Add(new MySqlParameter("p_DowngradeQuantity", model.UnprovisionedInfo.DowngradeQuantity));
                command.Parameters.Add(new MySqlParameter("p_ForfeitQuantity", model.UnprovisionedInfo.ForfeitQuantity));
                var affectedRows = command.ExecuteNonQuery();
            }
        }

        public async Task<VmRenewalModel> GetUnprovisionedRenewalDetails(EVmType vmType, string authcode, string latestQuoteNumber)
        {
            var result = new VmRenewalModel();
            if (string.IsNullOrEmpty(authcode))
                return result;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = string.Empty;
                if (string.IsNullOrEmpty(latestQuoteNumber))
                {
                    query = @"
                    SELECT RenewalQuantity, DowngradeQuantity, ForfeitQuantity FROM vmrenewalunprovisioned
                    WHERE AuthorizationCode = ?p_AuthCode AND QuoteNumber IS NULL ORDER BY RequestDate DESC LIMIT 1";
                }
                else
                {
                    query = @"
                    SELECT RenewalQuantity, DowngradeQuantity, ForfeitQuantity FROM vmrenewalunprovisioned
                    WHERE AuthorizationCode = ?p_AuthCode AND QuoteNumber = ?p_QuoteNumber ORDER BY RequestDate DESC LIMIT 1";   
                }

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;

                    if (!string.IsNullOrEmpty(latestQuoteNumber))
                    {
	                    command.Parameters.AddWithValue("?p_QuoteNumber", latestQuoteNumber);
                    }

                    command.Parameters.AddWithValue("?p_AuthCode", authcode);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.UnprovisionedInfo = new VmRenewalUnprovisionedInfo
                            {
                                RenewalQuantity = reader.GetColumnValue<int>("RenewalQuantity"),
                                DowngradeQuantity = reader.GetColumnValue<int>("DowngradeQuantity"),
                                ForfeitQuantity = reader.GetColumnValue<int>("ForfeitQuantity")
                            };
                            break;
                        }
                    }
                }

                connection.Close();
            }

            return result;
        }

        public async Task<int> GetActualProvisionedVms(EVmType vmType, long supportAcctId, string authcode)
        {
            int result = 0;
            if (string.IsNullOrEmpty(authcode))
                return result;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = @"SELECT COUNT(*) FROM license l
	                INNER JOIN license vml ON l.LicenseId = vml.VMParentLicenseId
	                INNER JOIN device d ON d.DeviceId = vml.DeviceId
	                INNER JOIN partmaster p ON d.PartMasterId = p.PartMasterId
	                WHERE l.SupportAccountId = ?p_SupportAccountId AND l.AuthorizationCode = ?p_AuthCode AND l.IsActive AND vml.IsActive AND p.PartNumber != 'PAN-PA-VM-SW'";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAcctId);
                    command.Parameters.AddWithValue("?p_AuthCode", authcode);

                    var ret = await command.ExecuteScalarAsync();
                    if (ret != null)
                        int.TryParse(ret.ToString(), out result);
                }
                connection.Close();
            }

            return result;
        }

        public async Task<IEnumerable<VmItemToDeactivate>> GetProvisionedVmsToDeactivate(EVmType vmType, long supportAccountId, string parentAuthCode)
        {
            var result = new List<VmItemToDeactivate>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT d.SerialNumber, vmp.PartNumber FROM license l
                    INNER JOIN license vml ON l.LicenseId = vml.VMParentLicenseId
                    INNER JOIN partmaster vmp ON vmp.PartMasterId = vml.PartMasterId
                    INNER JOIN device d ON d.DeviceId = vml.DeviceId
                    INNER JOIN partmaster p ON d.PartMasterId = p.PartMasterId
                    WHERE l.SupportAccountId = ?p_SupportAccountId AND l.AuthorizationCode = ?p_AuthCode AND l.IsActive AND vml.IsActive AND p.PartNumber != 'PAN-PA-VM-SW'
                    ORDER BY d.SerialNumber;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_AuthCode", parentAuthCode);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var partNumber = reader.GetColumnValue<string>("PartNumber");
                            var pos = -1;
                            if (!string.IsNullOrEmpty(partNumber))
                                pos = partNumber.IndexOf("-ELA");
                            var item = new VmItemToDeactivate
                            {
                                SerialNumber = reader.GetColumnValue<string>("SerialNumber"),
                                Description = pos > 3 ? string.Format("{0} ELA Subscription Bundle", partNumber.Substring(4, pos - 4)) : partNumber
                            };
                            result.Add(item);                            
                        }
                    }
                }

                await connection.CloseAsync();
            }

            return result;
        }

        public async Task DeactivateProvisionedVms(EVmType vmType, IEnumerable<VmItemToDeactivate> items, string email)
        {
            using (var connection = new MySqlConnection(Secrets.PanDatabase))
            {
                await connection.OpenAsync();
                using (DbCommand command = new MySqlCommand("Adminsite_ResetVMDevice", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var item in items)
                    {
                        if(string.IsNullOrEmpty(item.SerialNumber))
                            continue;
                        command.Parameters.Clear();
                        command.Parameters.Add(new MySqlParameter("SerialNumber", item.SerialNumber));
                        command.Parameters.Add(new MySqlParameter("Reason", "customer deactivation from CSP UI"));
                        command.Parameters.Add(new MySqlParameter("Whoby", email));
                        await command.ExecuteNonQueryAsync();
                    }                    
                }
                await connection.CloseAsync();
            }
        }

        public async Task<CnAllocateAuthCodeTokenDetails> GetCnAuthCodeTokenDetails(long supportAccountId)
        {
            var result = new CnAllocateAuthCodeTokenDetails();
            result.CnxAuthCodes = new List<string>();
            result.AvailableTokens = 0;

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                var query = @"SELECT l.AuthorizationCode,l.VMUseCount
                              FROM device d
                              JOIN partmaster pd ON d.PartMasterId = pd.PartMasterId
                              JOIN license l ON d.deviceId = l.VMStubDeviceId
                              JOIN partmaster pl ON l.PartMasterId = pl.PartMasterId
                              JOIN partsubtype psl ON pl.PartSubtypeId = psl.PartSubtypeId
                              LEFT JOIN supportaccount s ON l.SupportCenterSupportAccountId = s.SupportAccountId
                              LEFT JOIN phoenixgrandparents p ON p.ParentLicenseId = l.LicenseId
                              WHERE d.IsActive 
                              AND d.supportAccountId = @p_SupportAccountId 
                              AND l.IsActive                              
                              AND (l.VMParentLicenseId IS NULL OR l.VMParentLicenseId = 0)      
                              AND psl.PanDbIdentification LIKE '%TOKEN';";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.CnxAuthCodes.Add(reader.GetColumnValue<string>("AuthorizationCode"));
                            result.AvailableTokens += reader.GetColumnValue<int>("VMUseCount");
                        }
                    }
                }
                connection.Close();
            }
            return result;
        }

        public async Task<List<CnAvailablePanorama>> GetCnAvailablePanorama(long supportAccountId)
        {
            var result = new List<CnAvailablePanorama>();
            
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                var query = @"SELECT DISTINCT d.DeviceId, d.SerialNumber, p.PartNumber
                                FROM partmaster p
                                JOIN device d ON d.PartMasterId = p.PartMasterId
                                JOIN license l ON d.DeviceId = l.DeviceId
                                WHERE (p.ModelCompatibility LIKE 'PRA%'
                                OR p.ModelCompatibility LIKE 'AVI%')
                                AND p.PartTypeId = 3
                                AND d.SupportAccountId = @p_SupportAccountId
                                AND d.IsActive = 1
                                AND l.IsActive = 1
                                AND d.DeviceId NOT in (
                                    SELECT DISTINCT d.DeviceId       
                                    FROM (
                                        SELECT l.LicenseId
                                        FROM device d
                                        JOIN partmaster pd ON d.PartMasterId = pd.PartMasterId
                                        JOIN license l ON d.deviceId = l.VMStubDeviceId
                                        JOIN partmaster pl ON l.PartMasterId = pl.PartMasterId
                                        JOIN partsubtype psl ON pl.PartSubtypeId = psl.PartSubtypeId
                                        LEFT JOIN supportaccount s ON l.SupportCenterSupportAccountId = s.SupportAccountId
                                        LEFT JOIN phoenixgrandparents p ON p.ParentLicenseId = l.LicenseId
                                        WHERE d.IsActive = 1
                                        AND d.supportAccountId = @p_SupportAccountId 
                                        AND l.IsActive = 1
                                        AND (l.VMParentLicenseId IS NULL OR l.VMParentLicenseId = 0 )      
                                        AND psl.PanDbIdentification LIKE '%TOKEN') as z
                                    JOIN license cl ON z.LicenseId = cl.BundleLicenseId 
                                    JOIN device d ON cl.DeviceId = d.DeviceId   
                                    JOIN partmaster pd ON d.PartMasterId = pd.PartMasterId     
                                    WHERE  d.IsActive = 1
                                    AND cl.IsActive = 1);";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new CnAvailablePanorama
                            {
                                DeviceId = reader.GetColumnValue<int>("DeviceId"),
                                SerialNumber = reader.GetColumnValue<string>("SerialNumber"),
                                ModelName = reader.GetColumnValue<string>("PartNumber")
                            };
                            result.Add(item);
                        }
                    }
                }
                connection.Close();
            }
            return result;
        }
    }
}
