using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using Csp.Api.HelperClasses;
using System.Web;

namespace Csp.Infrastructure.Data.Persistence
{
    public class SiteLicenseRepository : ISiteLicenseRepository
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IConnectionStrings _connectionStrings;

        public SiteLicenseRepository(IApplicationSettings applicationSettings, IConnectionStrings connectionStrings)
        {
            _applicationSettings = applicationSettings;
            _connectionStrings = connectionStrings;
        }

        public struct DeviceLicenseCount
        {
            public long LicenseId { get; set; }
            public int DeviceCount { get; set; }
        }

        private const string SitePartTypePanDbIdentification = "SITE";
        private const string CanaryEvalPstPanDbIdentification = "SITECANARYEVAL";
        private const string SiteCanaryPartSubTypePanDbIdentification = "SITECANARY";
        private const string RenSiteCanaryPartSubTypePanDbIdentification = "RENSITECANARY";
        private const string EsaSiteLicense = "SITEESA";
        private const string Esa4HRSiteLicense = "SITE4HRESA";

        public IEnumerable<SiteLicenseItem> GetSiteLicenses(long supportAccountId, DataParameters dataParameters)
        {
            var db = SessionDbContext.CspObjectContext;

            var now = DateTime.Now;

            var siteLicenses =
                from lic in db.licenses
                join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                join pt in db.parttypes on pm.PartTypeId equals pt.PartTypeId

                join lic2 in db.licenses on lic.BundleLicenseId equals lic2.LicenseId into tempLics
                from leftLics in tempLics.DefaultIfEmpty()

                join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId into tempPst
                from leftPst in tempPst.DefaultIfEmpty()

                join ascSa in db.supportaccounts on lic.SupportCenterSupportAccountId equals ascSa.SupportAccountId into tempAscSa
                from leftAscSa in tempAscSa.DefaultIfEmpty()

                let isAutoFocus = (leftPst.PanDbIdentification == SiteCanaryPartSubTypePanDbIdentification || leftPst.PanDbIdentification == RenSiteCanaryPartSubTypePanDbIdentification || leftPst.PanDbIdentification == CanaryEvalPstPanDbIdentification)

                where
                    lic.SiteLicenseSupportAccountId == supportAccountId
                    && pt.PanDbIdentification.Contains(SitePartTypePanDbIdentification)
                    && lic.IsActive == true

                orderby lic.RegistrationDate descending

                select new SiteLicenseItem
                {
                    LicenseId = lic.LicenseId,
                    AuthorizationCode = lic.AuthorizationCode,
                    PartNumber = pm.PartNumber,
                    Description = pm.Description,
                    ExpirationDate = lic.ExpirationDate,
                    IsAutofocus = isAutoFocus,
                    IsEnabled = lic.IsActive,
                    AscName = leftAscSa == null ? string.Empty : leftAscSa.Name,
                    AscId = lic.SupportCenterSupportAccountId,
                    MaxLicenses = isAutoFocus ? lic.VMUseCount : (long?)null,
                    ApiKeyState = AfApiKeyStatuses.Unknown,
                    BundleLicenseId = lic.BundleLicenseId,
                    MainAuthCode = leftLics == null ? string.Empty : leftLics.AuthorizationCode,
                    ParentLicenseId = leftLics == null ? null : leftLics.ParentLicenseId
                };

            siteLicenses = FilterAndPage(siteLicenses, dataParameters);

            return siteLicenses;
        }

        private IQueryable<SiteLicenseItem> FilterAndPage(IQueryable<SiteLicenseItem> siteLicenses, DataParameters dataParameters)
        {
            switch (dataParameters.SortField)
            {
                case "AuthorizationCode":
                    siteLicenses = (dataParameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? siteLicenses.OrderByDescending(u => u.AuthorizationCode)
                                                                : siteLicenses.OrderBy(u => u.AuthorizationCode);
                    break;

                case "PartNumber":
                    siteLicenses = (dataParameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? siteLicenses.OrderByDescending(u => u.PartNumber)
                                                                : siteLicenses.OrderBy(u => u.PartNumber);
                    break;

                case "ExpirationDateDisplay":
                    siteLicenses = (dataParameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? siteLicenses.OrderByDescending(u => u.ExpirationDate)
                                                                : siteLicenses.OrderBy(u => u.ExpirationDate);
                    break;

                case "AscName":
                    siteLicenses = (dataParameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? siteLicenses.OrderByDescending(u => u.AscName)
                                                                : siteLicenses.OrderBy(u => u.AscName);
                    break;

                default:
                    break;
            }

            dataParameters.TotalRecordCount = siteLicenses.Count();

            if (dataParameters.SupportsPaging)
            {
                siteLicenses = siteLicenses.Skip((int)dataParameters.PaginationOffset).Take((int)dataParameters.PaginationCount);
            }

            return siteLicenses;
        }


        public AutoFocusLicenseDetails GetAutoFocusLicenseDetails(long supportAccountId, long licenseId)
        {
            var db = SessionDbContext.CspObjectContext;
            var now = DateTime.Now;

            var lics =
                from lic in db.licenses
                where
                    lic.SiteLicenseSupportAccountId == supportAccountId
                    && lic.LicenseId == licenseId
                    && lic.IsActive
                select new AutoFocusLicenseDetails
                {
                    UseCount = lic.VMUseCount,
                    ExpirationDate = lic.ExpirationDate.Value,
                };

            var afLic = lics.SingleOrDefault();
            return afLic;
        }


        public IEnumerable<EnterpriseAgreementItem> GetEnterpriseAgreements(long supportAccountId)
        {
            if (supportAccountId <= 0)
                return new List<EnterpriseAgreementItem>();
            var licenseDataAccess = new LicenseDataAccess();
            var enterpriseAgreements = licenseDataAccess.GetEnterpriseAgreements(supportAccountId);
            return enterpriseAgreements;
        }

        public async Task<IEnumerable<EnterpriseAgreementItem>> GetVmELaItems(long supportAccountId)
        {
            var result = new List<EnterpriseAgreementItem>();

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                using (DbCommand command = new MySqlCommand("GetVmElaInformation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("p_SupportAccountId", supportAccountId));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new EnterpriseAgreementItem
                            {
                                IsVmElaLicense = true,
                                EnterpriseAgreement = "VM-Series",
                                SupportAccountId = reader.GetColumnValue<long>("SupportAccountId"),
                                SupportAccountName = reader.GetColumnValue<string>("Name"),
                                MainAuthCode = reader.GetColumnValue<string>("GrandParentAuthcode"),
                                AuthorizationCode = reader.GetColumnValue<string>("GrandParentAuthcode"),
                                Description = reader.GetColumnValue<string>("Description"),
                                ExpirationDate = reader.GetColumnValue<DateTime>("ExpirationDate"),
                                VmElaGrandParentTotalTokens = reader.GetColumnValue<long>("GrandParentTotalTokens"),
                                VmElaGrandParentUseTokens = reader.GetColumnValue<long>("GrandParentUseTokens"),
                                MaxLicenses = reader.GetColumnValue<long?>("TotalTokens"),
                                UsedLicenses = reader.GetColumnValue<long?>("RegisteredTokens")
                            };
                            result.Add(item);
                        }
                    }
                }

                connection.Close();
            }

            return result;
        }

        private long GetCountOfDeviceEaIsAppliedTo(long licenseId)
        {
            var db = SessionDbContext.CspObjectContext;
            var deviceCount =
                (from l in db.licenses where l.BundleLicenseId == licenseId && l.IsActive && l.DeviceId != null select l.DeviceId).Distinct().Count();
            return deviceCount;
        }


        public string GetEnterpriseAgreementMainAuthCode(long licenseId)
        {
            var db = SessionDbContext.CspObjectContext;
            var mainAuthCode =
                (from x in db.licenses where x.LicenseId == licenseId select x.AuthorizationCode).FirstOrDefault();
            return mainAuthCode;
        }


        public bool HasSiteLicense(long supportAccountId)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = @"
                    SELECT COUNT(*) FROM 
                      (
                        (SELECT l.LicenseId FROM license l 
                          WHERE l.SiteLicenseSupportAccountId = ?p_SupportAccountId AND l.IsActive LIMIT 1)
                        UNION
                        (SELECT l.LicenseId FROM license l
                          INNER JOIN phoenixgrandparents p ON l.LicenseId = p.ParentLicenseId
                          WHERE l.SupportAccountId = ?p_SupportAccountId AND p.IsActive AND l.IsActive LIMIT 1)
                    ) AS temp;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    var result = command.ExecuteScalar();
                    if (result != null && (long)result > 0)
                        return true;
                }
                connection.Close();
            }

            return false;
        }


        public bool HasEsaLicense(long supportAccountId)
        {
            //var db = SessionDbContext.CspObjectContext;

            //var expirationDates =
            //    (from l in db.licenses
            //     join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
            //     join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
            //     where l.SiteLicenseSupportAccountId == supportAccountId && pst.PanDbIdentification == EsaSiteLicense
            //     select l.ExpirationDate).ToList();

            //return expirationDates.Any(x => x.HasValue && x.Value > DateTime.Now);

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = @"
                        SELECT COUNT(*) FROM csp.license l JOIN partmaster p ON l.PartMasterId = p.PartMasterId
                         JOIN csp.partsubtype ps ON p.PartSubtypeId = ps.PartSubtypeId
                          WHERE l.SiteLicenseSupportAccountId=?p_SupportAccountId AND ps.PanDbIdentification =?p_EsaSiteLicense AND l.ExpirationDate >= NOW() AND l.IsActive=1;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_EsaSiteLicense", EsaSiteLicense);

                    var result = command.ExecuteScalar();
                    if (result != null && (long)result > 0)
                        return true;
                }
                connection.Close();
            }

            return false;

        }

        public bool Has4HREsaLicense(long supportAccountId)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                var query = @"
                        SELECT COUNT(*) FROM csp.license l JOIN partmaster p ON l.PartMasterId = p.PartMasterId
                         JOIN csp.partsubtype ps ON p.PartSubtypeId = ps.PartSubtypeId
                          WHERE l.SiteLicenseSupportAccountId=?p_SupportAccountId AND ps.PanDbIdentification =?p_Esa4HRSiteLicense AND l.ExpirationDate >= NOW() AND l.IsActive=1;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_Esa4HRSiteLicense", Esa4HRSiteLicense);

                    var result = command.ExecuteScalar();
                    if (result != null && (long)result > 0)
                        return true;
                }
                connection.Close();
            }

            return false;
        }

        public bool HasEaLicense(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var eaSiteLicense = _applicationSettings.EaSiteLicense;
            var eaSiteLicenseTypes = eaSiteLicense.Split(',').ToList();

            var expirationDates =
                (from l in db.licenses
                 join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
                 join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
                 where l.SiteLicenseSupportAccountId == supportAccountId && eaSiteLicenseTypes.Contains(pst.PanDbIdentification)
                 select l.ExpirationDate).ToList();

            return expirationDates.Any(x => x.HasValue && x.Value > DateTime.Now);
        }

        public async Task<VmElaTokenModel> GetVmElaTokens(string authCode)
        {
            var query = @"
                SELECT parentLic.SupportAccountId, s.Name, gpLic.VMQuantity AS TotalTokens, gpLic.RegistrationDate, gpLic.LicenseTermMonths, p.AdditTokensAppliedDate, SUM(parentLic.VMQuantity * e.ExchangeRate) AS AllocatedTokens FROM phoenixgrandparents p
                  INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId
                  INNER JOIN license parentLic ON parentLic.LicenseId = p.ParentLicenseId
            	  INNER JOIN partmaster pm ON pm.PartMasterId = parentLic.PartMasterId
          	      INNER JOIN partsubtype ps ON ps.PartSubtypeId = pm.PartSubtypeId
          	      LEFT JOIN vmelatokenexchangerate e ON ps.PartSubtypeId = e.PartSubtypeId
                  INNER JOIN supportaccount s ON s.SupportAccountId = parentLic.SupportAccountId
                  WHERE gpLic.AuthorizationCode = ?p_AuthCode AND gpLic.CreatedOn > e.StartUtcDateTime AND (gpLic.CreatedOn <= e.EndUtcDateTime OR e.EndUtcDateTime IS NULL)
                  GROUP BY parentLic.SupportAccountId;";
            var result = new VmElaTokenModel
            {
                GrandParentAuthCode = authCode,
                GrantedAccounts = new Dictionary<long, string>()
            };
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        bool sharedReadComplete = false;
                        while (await reader.ReadAsync())
                        {
                            if (!sharedReadComplete)
                            {
                                result.TotalTokens = reader.GetColumnValue<long>("TotalTokens");
                                result.RegistrationDate = reader.GetColumnValue<DateTime?>("RegistrationDate");
                                result.TermMonths = reader.GetColumnValue<int>("LicenseTermMonths");
                                sharedReadComplete = true;
                            }
                            //read until there is AdditTokensAppliedDate defined
                            if (result.AdditionalTokenAppliedDate == null)
                                result.AdditionalTokenAppliedDate = reader.GetColumnValue<DateTime?>("AdditTokensAppliedDate");
                            result.AllocatedTokens += reader.GetColumnValue<long>("AllocatedTokens");
                            result.GrantedAccounts[reader.GetColumnValue<long>("SupportAccountId")] = reader.GetColumnValue<string>("Name");
                        }
                    }
                }

                await connection.CloseAsync();
            }

            return result;
        }

        //VM-ELA authcodes' info under this account
        public async Task<IEnumerable<VmElaTokenModel>> GetVmElaAuthCodesInfo(long supportAccountId)
        {
            var result = new List<VmElaTokenModel>();
            var query = @"
                    SELECT gpLic.AuthorizationCode AS GrandParentAuthcode, 
                           gpLic.LicenseTermMonths AS GrandParentTerms,
	                       gpLic.VMQuantity AS GrandParentTotalTokens,
	                       gpLic.RegistrationDate,
                           p.AdditTokensAppliedDate,
		                   CASE
			                   WHEN e.ExchangeRate IS NOT NULL THEN SUM(parentLic.VMQuantity*e.ExchangeRate)
			                   ELSE 0
		                   END AS TotalTokens		                    
		                   FROM phoenixgrandparents p
		                   INNER JOIN  
		                   (
  		                        -- get the grand parent license ID (grand parent authcode)
  		                        SELECT DISTINCT p.GrandparentLicenseId FROM phoenixgrandparents p
  		                          INNER JOIN license l ON l.LicenseId = p.ParentLicenseId
  		                          WHERE p.IsActive AND l.IsActive AND l.SupportAccountId = ?p_SupportAccountId
		                   ) AS _grandParentLics 
		                   ON p.GrandparentLicenseId = _grandParentLics.GrandparentLicenseId
		                   INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId
		                   -- pm: grandparent license partmaster info
		                   INNER JOIN partmaster pm ON pm.PartMasterId = gpLic.PartMasterId
		                   INNER JOIN license parentLic ON parentLic.LicenseId = p.ParentLicenseId
		                   -- pm1: parent license partmaster info
                           INNER JOIN partmaster pm1 ON pm1.PartMasterId = parentLic.PartMasterId
                           INNER JOIN partsubtype ps ON ps.PartSubtypeId = pm1.PartSubtypeId
                           LEFT JOIN vmelatokenexchangerate e ON ps.PartSubtypeId = e.PartSubtypeId
                           WHERE gpLic.CreatedOn > e.StartUtcDateTime AND (gpLic.CreatedOn <= e.EndUtcDateTime OR e.EndUtcDateTime IS NULL)
		                   GROUP BY gpLic.AuthorizationCode;";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var model = new VmElaTokenModel
                            {
                                GrandParentAuthCode = reader.GetColumnValue<string>("GrandParentAuthcode"),
                                TermMonths = reader.GetColumnValue<int>("GrandParentTerms"),
                                TotalTokens = reader.GetColumnValue<long>("GrandParentTotalTokens"),
                                AllocatedTokens = reader.GetColumnValue<long>("TotalTokens"),
                                RegistrationDate = reader.GetColumnValue<DateTime>("RegistrationDate"),
                                AdditionalTokenAppliedDate = reader.GetColumnValue<DateTime?>("AdditTokensAppliedDate")
                            };
                            result.Add(model);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<VmElaChildModelInfo>> GetCurrentAccountVmElaModels(string grandParentAuthCode, long supportAccountId)
        {
            var result = new List<VmElaChildModelInfo>();
            var query = @"
                    SELECT p.ParentLicenseId, 
                    pm.PartNumber, 
                    parentLic.VMQuantity, 
                    (parentLic.VMQuantity - parentLic.VMUseCount) AS ProvisionedQuantity, 
                    vm.ExchangeRate*parentLic.VMQuantity AS Tokens, 
                    vm.ExchangeRate 
                    FROM phoenixgrandparents p
                      INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId
                      INNER JOIN license parentLic ON parentLic.LicenseId = p.ParentLicenseId
                      INNER JOIN partmaster pm ON pm.PartMasterId = parentLic.PartMasterId
                      INNER JOIN vmelatokenexchangerate vm ON vm.PartSubtypeId = pm.PartSubtypeId
                      WHERE p.IsActive AND gpLic.AuthorizationCode =?p_AuthCode AND gpLic.IsActive AND parentLic.SupportAccountId=?p_SupportAccountId 
-- CSP-3443 Change Registration PIN to be PIN value
-- Likely, this logic is implemented for changable nature of value of vmelatokenexchangerate exchange rate
-- Removing it because it breaks CN-X exchange rate introduced 2020
--                      AND parentLic.CreatedOn > vm.StartUtcDateTime AND (parentLic.CreatedOn <= vm.EndUtcDateTime OR vm.EndUtcDateTime IS NULL)
                      ORDER BY vm.ExchangeRate;";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", grandParentAuthCode);
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var partNumber = reader.GetColumnValue<string>("PartNumber");
                            var item = new VmElaChildModelInfo
                            {
                                LicenseId = reader.GetColumnValue<long>("ParentLicenseId"),
                                Model = string.IsNullOrEmpty(partNumber) ? string.Empty : partNumber.Substring(4, partNumber.IndexOf("ELA", StringComparison.InvariantCulture) - 5),
                                AllocatedQuantity = reader.GetColumnValue<long>("VMQuantity"),
                                ProvisionedQuantity = reader.GetColumnValue<long>("ProvisionedQuantity"),
                                Tokens = reader.GetColumnValue<long>("Tokens"),
                                ExchangeRate = reader.GetColumnValue<int>("ExchangeRate")
                            };
                            result.Add(item);
                        }
                    }
                }
                await connection.CloseAsync();
            }

            return result;
        }

        public async Task SaveVmElaAllocatedQuantities(string grandParentAuthCode, long totalAllocatedTokens, Dictionary<string, IEnumerable<VmElaChildModelInfo>> models)
        {
            MySqlTransaction transaction = null;
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(Secrets.PanDatabase);
                connection.Open();
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                using (var command = new MySqlCommand("", connection))
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;

                    foreach (var key in models.Keys)
                    {
                        var updateBuffer = new StringBuilder();
                        foreach (var item in models[key])
                        {
                            command.Parameters.Clear();
                            command.CommandText = "SELECT lf.VMQuantity, lf.use_count FROM pandb.lic_features lf WHERE lf.lfid=?p_LicenseId;";
                            command.Parameters.AddWithValue("?p_LicenseId", item.LicenseId);

                            long vmQuantity = 0, vmUserCount = 0;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    vmQuantity = reader.GetColumnValue<long>("VMQuantity");
                                    vmUserCount = reader.GetColumnValue<long>("use_count");
                                }
                            }

                            var newAvailable = item.AllocatedQuantity - (vmQuantity - vmUserCount);
                            var minQuantity = vmQuantity - vmUserCount;
                            //for any other authcodes than grand parent authcode, use_count is the available count, 
                            //current consumed count=VmQuantity-use_count, new available count is: item.AllocatedQuantity - (VmQuantity-use_count)
                            //must set use_count first, otherwise VmQuantity is updated by the new value.
                            updateBuffer.AppendFormat(@"
                                UPDATE pandb.lic_features 
                                SET use_count= CASE 
                                  WHEN {0} > 0 THEN {0}
                                  ELSE 0 
                                END,
                                VmQuantity = CASE
                                  WHEN {0} > 0 THEN {1}
                                  ELSE {2}
                                END
                                WHERE lfid = {3};", newAvailable, item.AllocatedQuantity, minQuantity, item.LicenseId);
                        }
                        command.CommandText = updateBuffer.ToString();
                        await command.ExecuteNonQueryAsync();
                    }

                    command.Parameters.Clear();
                    //grand parent authcode: use_count is the allocated tokens
                    command.CommandText = "UPDATE lic_features SET use_count=?p_TotalAllocatedTokens WHERE auth_code=?p_AuthCode;";
                    command.Parameters.AddWithValue("?p_TotalAllocatedTokens", totalAllocatedTokens);
                    command.Parameters.AddWithValue("?p_AuthCode", grandParentAuthCode);

                    await command.ExecuteNonQueryAsync();

                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                    }
                }

                throw;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public async Task ApplyVmElaAdditionalTokens(string authCode, long tokens)
        {
            //update license
            using (var connection = new MySqlConnection(Secrets.PanDatabase))
            {
                await connection.OpenAsync();
                var update = @"
                    UPDATE lic_features SET VmQuantity=?p_Tokens, last_modified = CURRENT_TIMESTAMP()
                    WHERE auth_code=?p_AuthCode;";
                using (var command = new MySqlCommand(update, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_Tokens", tokens);
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    await command.ExecuteNonQueryAsync();
                }
                await connection.CloseAsync();
            }

            //update the AdditTokensAppliedDate in CSP
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var update = @"
                    UPDATE phoenixgrandparents p
                      INNER JOIN license l ON l.LicenseId = p.GrandparentLicenseId
                      SET p.AdditTokensAppliedDate = CURRENT_TIMESTAMP()
                      WHERE l.AuthorizationCode = ?p_AuthCode;";
                using (var command = new MySqlCommand(update, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    await command.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();
            }
        }

        //add 50% more tokens during activation
        public async Task AddVmElaMoreTokens(string authCode)
        {
            using (var connection = new MySqlConnection(Secrets.PanDatabase))
            {
                await connection.OpenAsync();
                var update = "UPDATE pandb.lic_features SET VmQuantity = VmQuantity*1.5 WHERE auth_code=?p_AuthCode;";
                using (var command = new MySqlCommand(update, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task UpdateVmElaAdditionalTokensAppliedDate(string authCode)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var update = @"
                    SELECT p1.AdditTokensAppliedDate FROM phoenixgrandparents p1
                            INNER JOIN license gpLic ON gpLic.LicenseId = p1.GrandparentLicenseId
                          WHERE gpLic.AuthorizationCode = ?p_AuthCode AND p1.AdditTokensAppliedDate IS NOT NULL LIMIT 1;";
                using (var command = new MySqlCommand(update, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        var appliedDate = Convert.ToDateTime(result.ToString());
                        command.Parameters.Clear();
                        command.CommandText = @"
                            UPDATE phoenixgrandparents p
                              INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId
                              SET p.AdditTokensAppliedDate = ?p_AppliedDate
                              WHERE gpLic.AuthorizationCode = ?p_AuthCode AND p.AdditTokensAppliedDate IS NULL;";
                        command.Parameters.AddWithValue("?p_AppliedDate", appliedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("?p_AuthCode", authCode);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                await connection.CloseAsync();
            }
        }

        public async Task<string> GrantVmElaTokens(string grandParentAuthCode, long curUserId, string destinationEmail)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT u.UserAccountId FROM useraccount u
                        WHERE u.Email=?p_Email;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_Email", destinationEmail);

                    //get dest customer Id
                    var customerId = await command.ExecuteScalarAsync();
                    if (customerId == null)
                        return "The destination email must be a registered user.";
                    long destCustomerId = (long)customerId;

                    command.Parameters.Clear();
                    command.CommandText = @"
                        SELECT COUNT(*) FROM elatokengrant e
                          INNER JOIN devicetransferstates d ON e.StateId = d.Id
                          WHERE e.AuthorizationCode=?p_AuthCode AND e.DestUserAccountId=?p_UserAccountId AND d.Name='Pending';";
                    command.Parameters.AddWithValue("?p_AuthCode", grandParentAuthCode);
                    command.Parameters.AddWithValue("?p_UserAccountId", destCustomerId);

                    //check if there is a pending grant or not
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && (long)result > 0)
                        return "This customer has a pending grant already.";

                    command.Parameters.Clear();
                    //insert pending record
                    command.CommandText = @"
                        INSERT INTO elatokengrant(AuthorizationCode, DestUserAccountId, StateId, CreatedBy, CreatedOn)
                          VALUES(?p_AuthCode, ?p_DestinationUserAccountId, 1, ?p_CreatedByUserAccountId, UTC_TIMESTAMP());";
                    command.Parameters.AddWithValue("?p_AuthCode", grandParentAuthCode);
                    command.Parameters.AddWithValue("?p_DestinationUserAccountId", destCustomerId);
                    command.Parameters.AddWithValue("?p_CreatedByUserAccountId", curUserId);

                    await command.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();

                return string.Empty;
            }
        }

        //check if there is any pending request for the current user who is also a member of the current account
        public bool HasPendingTokensGrantRequest(long supportAccountId, long userId)
        {
            var existingVmElas = new HashSet<string>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                //get the existing VM-ELA authcodes in the current account
                var query = @"
                      SELECT DISTINCT gpLic.AuthorizationCode FROM phoenixgrandparents p
		                    INNER JOIN  
		                    (
  		                    -- get the grand parent license ID (grand parent authcode)
  		                    SELECT DISTINCT p.GrandparentLicenseId FROM phoenixgrandparents p
  		                      INNER JOIN license l ON l.LicenseId = p.ParentLicenseId
  		                      WHERE p.IsActive AND l.IsActive AND l.SupportAccountId = ?p_SupportAccountId
		                    ) AS _grandParentLics 
		                    ON p.GrandparentLicenseId = _grandParentLics.GrandparentLicenseId
		                    INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var authCode = reader.GetColumnValue<string>("AuthorizationCode");
                            if (!string.IsNullOrEmpty(authCode))
                                existingVmElas.Add(authCode);
                        }
                    }

                    command.Parameters.Clear();
                    command.CommandText = @"
                    SELECT e.AuthorizationCode FROM membership m 
                      INNER JOIN elatokengrant e ON m.UserAccountId = e.DestUserAccountId
                      WHERE m.SupportAccountId = ?p_SupportAccountId AND m.UserAccountId = ?p_UserAccountId AND e.StateId = 1;";
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_UserAccountId", userId);

                    int count = 0;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var authCode = reader.GetColumnValue<string>("AuthorizationCode");
                            if (existingVmElas.Contains(authCode))
                                continue;
                            //if there are some, don't continue.
                            if (count > 0)
                                return true;
                            count++;
                        }
                    }

                    return count > 0;
                }

            }


        }

        public async Task<IEnumerable<ElaTokensGrantModel>> GetPendingElaTokensGrantItems(long supportAccountId, long userId)
        {
            var result = new List<ElaTokensGrantModel>();
            var existingVmElas = new HashSet<string>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                //get the existing VM-ELA authcodes in the current account
                var query = @"
                      SELECT DISTINCT gpLic.AuthorizationCode FROM phoenixgrandparents p
		                    INNER JOIN  
		                    (
  		                    -- get the grand parent license ID (grand parent authcode)
  		                    SELECT DISTINCT p.GrandparentLicenseId FROM phoenixgrandparents p
  		                      INNER JOIN license l ON l.LicenseId = p.ParentLicenseId
  		                      WHERE p.IsActive AND l.IsActive AND l.SupportAccountId = ?p_SupportAccountId
		                    ) AS _grandParentLics 
		                    ON p.GrandparentLicenseId = _grandParentLics.GrandparentLicenseId
		                    INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var authCode = reader.GetColumnValue<string>("AuthorizationCode");
                            if (!string.IsNullOrEmpty(authCode))
                                existingVmElas.Add(authCode);
                        }
                    }

                    command.Parameters.Clear();
                    command.CommandText = @"
                    SELECT e.Id, e.AuthorizationCode, s.SupportAccountId, s.Name AS AccountName, e.CreatedOn, u.Email FROM membership m 
                      INNER JOIN elatokengrant e ON m.UserAccountId = e.DestUserAccountId
                      INNER JOIN useraccount u ON u.UserAccountId = e.CreatedBy
                      INNER JOIN supportaccount s ON s.SupportAccountId = ?p_SupportAccountId
                      WHERE m.UserAccountId = ?p_UserAccountId AND m.SupportAccountId=?p_SupportAccountId AND e.StateId = 1;";
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_UserAccountId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var utcDate = reader.GetColumnValue<DateTime>("CreatedOn");
                            var item = new ElaTokensGrantModel
                            {
                                VmElaTokenGrantId = reader.GetColumnValue<long>("Id"),
                                AuthorizationCode = reader.GetColumnValue<string>("AuthorizationCode"),
                                SupportAccountId = supportAccountId,
                                AccountName = reader.GetColumnValue<string>("AccountName"),
                                GrantDate = new DateTime(utcDate.Ticks, DateTimeKind.Utc),
                                SourceUser = reader.GetColumnValue<string>("Email")
                            };

                            if (!existingVmElas.Contains(item.AuthorizationCode))
                                result.Add(item);
                        }
                    }
                }
                connection.Close();
            }

            return result;
        }

        //check if the 5 VM-models exist or not
        public async Task<bool> VmElaModelsExist(string authCode, long supportAccountId)
        {
            var exists = false;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var query = @"
                        SELECT COUNT(*) FROM phoenixgrandparents p 
                              INNER JOIN license gpLic ON gpLic.LicenseId = p.GrandparentLicenseId
                            INNER JOIN license parentLic ON parentLic.LicenseId = p.ParentLicenseId
                          WHERE gpLic.AuthorizationCode =?p_AuthCode AND parentLic.SupportAccountId=?p_SupportAccountId;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null && (long)result > 0)
                        exists = true;
                }
                await connection.CloseAsync();
            }

            return exists;
        }

        public async Task AcceptElaTokenGrant(string authCode, long userAccountId, bool accepted)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                await connection.OpenAsync();
                var query = @"
                    UPDATE elatokengrant 
                    SET StateId = ?p_StateId
                    WHERE AuthorizationCode=?p_AuthCode AND DestUserAccountId = ?p_UserAccountId AND StateId = 1;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_StateId", accepted ? 2 : 3);
                    command.Parameters.AddWithValue("?p_AuthCode", authCode);
                    command.Parameters.AddWithValue("?p_UserAccountId", userAccountId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Fetches the list of users for token usage notification
        /// </summary>
        /// <returns></returns>
        public List<ElaTokenNotificationModel> GetVmElaTokenNotificationList()
        {
            var elaTokenNotificationList = new List<ElaTokenNotificationModel>();

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (DbCommand command = new MySqlCommand("GetVmElaForTokenNotificationList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var elaAdmin = new ElaTokenNotificationModel
                            {
                                UserAccountId = reader.GetColumnValue<long>("UserAccountId"),
                                UserEmail = reader.GetColumnValue<string>("ElaAdminEmail"),
                                FirstName = reader.GetColumnValue<string>("FirstName"),
                                LastName = reader.GetColumnValue<string>("LastName"),
                                SalesforceAccountId = reader.GetColumnValue<string>("SalesforceAccountId"),
                                SupportAccountId = reader.GetColumnValue<long>("SupportAccountId"),
                                LicenseId = reader.GetColumnValue<long>("VmElaLicenseId"),
                                AuthCode = reader.GetColumnValue<string>("VmElaAuthCode"),
                                TokensConsumedPercent = reader.GetColumnValue<int>("TokensConsumed")
                            };
                            elaTokenNotificationList.Add(elaAdmin);
                        }
                    }
                }
                connection.Close();
            }

            return elaTokenNotificationList;
        }

        /// <summary>
        /// Updates VM ELA Token Notification Log
        /// </summary>
        /// <param name="tokenNotificationModel"></param>
        public long UpdateVmElaTokenNotificationLog(ElaTokenNotificationModel tokenNotificationModel)
        {
            long tokenNotificationId = 0;
            try
            {
                using (var connection = new MySqlConnection(Secrets.CspDatabase))
                {
                    connection.Open();
                    using (DbCommand command = new MySqlCommand("UpdateVmElaForTokenNotificationLog", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new MySqlParameter("p_LicenseId", tokenNotificationModel.LicenseId));
                        command.Parameters.Add(new MySqlParameter("p_AuthCode", tokenNotificationModel.AuthCode));
                        command.Parameters.Add(new MySqlParameter("p_SupportAccountId", tokenNotificationModel.SupportAccountId));
                        command.Parameters.Add(new MySqlParameter("p_UserAccountId", tokenNotificationModel.UserAccountId));
                        command.Parameters.Add(new MySqlParameter("p_TokensConsumed", tokenNotificationModel.TokensConsumedPercent));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tokenNotificationId = reader.GetColumnValue<long>("VmElaTokenNotificationLogId");
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
            }

            return tokenNotificationId;
        }

        public IEnumerable<ElaTokensGrantModel> GetPendingElaTokensByUser(long supportAccountId, long userAccountId, long entityId)
        {
            var result = new List<ElaTokensGrantModel>();

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();

                var query = @"SELECT DISTINCT e.AuthorizationCode, s.SupportAccountId, s.Name AS AccountName, e.CreatedOn, u.Email FROM 
                             elatokengrant e 
                            INNER JOIN membership m ON e.DestUserAccountId=m.UserAccountId
                            INNER JOIN useraccount u ON m.UserAccountId=u.UserAccountId
                            INNER JOIN supportaccount s ON s.SupportAccountId=m.SupportAccountId
                            INNER JOIN membershiproles mr ON m.MembershipId = mr.MembershipId 
  		                      WHERE (m.ExpirationDate >= NOW() OR m.ExpirationDate IS NULL)   AND   m.SupportAccountId = ?p_SupportAccountId AND e.StateId=1 
                             AND m.UserAccountId=?p_UserAccountId AND e.Id=?p_EntityId;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("?p_UserAccountId", userAccountId);
                    command.Parameters.AddWithValue("?p_EntityId", entityId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var utcDate = reader.GetColumnValue<DateTime>("CreatedOn");
                            var item = new ElaTokensGrantModel
                            {
                                AuthorizationCode = reader.GetColumnValue<string>("AuthorizationCode"),
                                SupportAccountId = supportAccountId,
                                AccountName = reader.GetColumnValue<string>("AccountName"),
                                GrantDate = new DateTime(utcDate.Ticks, DateTimeKind.Utc),
                                SourceUser = reader.GetColumnValue<string>("Email")
                            };

                            result.Add(item);
                        }
                    }

                }

            }

            return result;
        }

        //Get the vm ela tokenid
        public long GetVmElaTokenId(string grandParentAuthCode, long curUserId, string destinationUser)
        {
            long tokenId = 0;

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();

                string query = @"
                        SELECT distinct e.id FROM elatokengrant e
     INNER JOIN devicetransferstates d ON e.StateId = d.Id
     INNER JOIN useraccount u ON u.UserAccountId=e.DestUserAccountId
                          WHERE e.AuthorizationCode=?p_AuthCode AND u.email=?p_Email AND d.Name='Pending' AND e.createdBy=?p_UserAccountId;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_AuthCode", grandParentAuthCode);
                    command.Parameters.AddWithValue("?p_Email", destinationUser);
                    command.Parameters.AddWithValue("?p_UserAccountId", curUserId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tokenId = Convert.ToInt64(reader["id"]);
                        }
                    }
                }
                return tokenId;
            }
        }

        public bool HasElaUrl4License(long supportAccountId)
        {

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                var query = @"SELECT COUNT(1)
                              FROM csp.license l
                              JOIN csp.partmaster p ON l.PartMasterId = p.PartMasterId AND p.PartNumber = 'PAN-ENT-SUB-URL4-DL'
                              JOIN csp.device d ON l.DeviceId = d.DeviceId AND d.IsActive = 1
                              WHERE l.SupportAccountId = ?p_SupportAccountId
                              AND l.IsActive = 1
                              AND l.AuthorizationCode REGEXP 's';";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    var result = command.ExecuteScalar();
                    if (result != null && (long)result > 0)
                        return true;
                }
                connection.Close();
            }

            return false;
        }

        public long GetELAConversionRequestId(long supportAccountId)
        {

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                var query = @"
                        SELECT s.RequestId FROM csp.elaconversionhistory s WHERE s.SupportAccountId = ?p_SupportAccountId AND s.ConversionType = 'URL4TOADVURL' AND s.Status = 0";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
                    var result = command.ExecuteScalar();
                    if (result != null && (long)result > 0)
                        return (long)result;
                }
                connection.Close();
            }

            return 0;
        }
    }
}
