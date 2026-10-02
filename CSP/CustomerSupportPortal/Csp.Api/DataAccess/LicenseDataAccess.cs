using System.Linq;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;
using System;
using System.Text.RegularExpressions;
using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using System.Web;
using Csp.Api.HelperClasses;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using Csp.Api.Model.Attributes;

namespace Csp.Api.DataAccess
{

    public class LicenseDataAccess : BaseDataAccess
    {
        /// <summary>
        /// Used to determine if a license is a site license that is applied to all devices for a support account
        /// </summary>
        /// <param name="authorizationCode">The authorization code of the license</param>
        /// <returns>True if the license for the given authorization code is a site license</returns>
        /// 
        private const string ADNS = "Advanced DNS Security";
        private const string AWF = "Advanced WildFire License";
        public IsSiteLicenseResult IsSiteLicense(string authorizationCode)
        {
            const string sqlQuery = "select l.IsActive, l.SiteLicenseSupportAccountId from license l join partmaster pm on l.PartMasterId=pm.PartMasterId join PartType pt on pm.parttypeid = pt.parttypeid where pt.pandbidentification in ('SITE','RENSITE') and l.AuthorizationCode = @p_authorizationCode";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_authorizationCode", DbType.String, authorizationCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {

                        var isSiteLicenseActive = Convert.ToInt64(dataReader["IsActive"]) == 1;
                        var isSiteLicenseApplied = Convert.ToInt64(dataReader["SiteLicenseSupportAccountId"]) > 0;

                        return new IsSiteLicenseResult
                        {
                            IsSiteLicense = true,
                            IsActive = isSiteLicenseActive,
                            IsAlreadyApplied = isSiteLicenseApplied,
                        };
                    }
                }
            }

            return new IsSiteLicenseResult
            {
                IsSiteLicense = false,
            };
        }

        public bool Is4HourRma(string authorizationCode)
        {
            const string sqlQuery = "select count(licenseId) licenseCount from license l join partmaster pm on l.PartMasterId=pm.PartMasterId where l.IsActive=1 and partnumber like '%4HR-%' and authorizationCode= @p_authorizationCode";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_authorizationCode", DbType.String, authorizationCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return (Convert.ToInt64(dataReader["licenseCount"]) > 0);
                    }
                }
            }

            return false;
        }

        public License ReadLicenseByAuthCode(string authorizationCode)
        {

            var license = new License();
            const string sqlQuery = "select * from license where authorizationCode= @p_authorizationCode";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {

                Connection.AddInParameter(command, "@p_authorizationCode", DbType.String, authorizationCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        license.PopulateProperties(dataReader);
                    }
                }
            }

            return license;
        }

        public string IsAuthorizationCodeValid(string authorizationCode, string serialNumber)
        {

            var result = "";

            using (var command = Connection.GetStoredProcCommand("IsAuthorizationCodeValid"))
            {

                Connection.AddInParameter(command, "p_authorizationCode", DbType.String, authorizationCode);
                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddOutParameter(command, "p_result", DbType.String, 100);
                Connection.ExecuteNonQuery(command);

                if (command.Parameters["p_result"].Value != DBNull.Value)
                {
                    result = command.Parameters["p_result"].Value.ToString();
                }
            }

            return result;
        }

        public string IsAuthorizationCodeValidForUpgrade(string authorizationCode, string serialNumber)
        {

            var result = "";

            using (var command = Connection.GetStoredProcCommand("IsAuthorizationCodeValidForUpgrade"))
            {

                Connection.AddInParameter(command, "p_authorizationCode", DbType.String, authorizationCode);
                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddOutParameter(command, "p_result", DbType.String, 100);
                Connection.ExecuteNonQuery(command);

                if (command.Parameters["p_result"].Value != DBNull.Value)
                {
                    result = command.Parameters["p_result"].Value.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// This Method is used to Retrieve the List of License based on DeviceID
        /// </summary>
        /// <param name="parameters">Data Parameters</param>
        /// <param name="deviceId">deviceId</param>
        /// <returns>List<License></returns>
        public List<LicenseListItem> ReadDeviceLicenses(long deviceId)
        {
            var licenseList = new List<LicenseListItem>();

            using (var command = Connection.GetStoredProcCommand("GetLicensesByDeviceId"))
            {

                Connection.AddInDeviceIdParameter(command, deviceId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var license = new LicenseListItem();

                        license.PopulateProperties(dataReader);
                        license.AuthorizationCode = dataReader["AuthorizationCode"].ToString();
                        if (dataReader["TransactionId"] != DBNull.Value)
                        {
                            license.TransactionId = dataReader["TransactionId"].ToString();
                        }

                        if (EpLogic.IsSkuMalbork(license.ModelName))
                            license.LicenseName = EpLogic.GetMalborkLicenseDescription(license.ModelName, license.LicenseName);
                        licenseList.Add(license);
                    }
                }
            }

            return licenseList;
        }

        public void ReadMultipleDevicePaeLicenses(IEnumerable<SearchAllModel> devices)
        {
            if (!(devices != null
                && devices.Any()))
            {
                return;
            }
            const string sql = @"
SELECT a.SupportAccountId, a.DeviceId, a.ModelName, a.License, a.AuthorizationCode, a.ExpirationDate, SUM(a.Quantity) Quantity, a.IsParent
  FROM
  (
    SELECT d.DeviceId, d.SupportAccountId, p.PartMasterId, p.PartNumber ModelName, COALESCE(s.SupportDisplayName, s.NAME, p.PartNumber) License, l.AuthorizationCode, 
      l.ExpirationDate, l.SaasQuantity Quantity, CASE WHEN s.PanDbIdentification LIKE '%-MU-%' OR s.PanDbIdentification LIKE '%-RN-%' THEN 1 ELSE 0 END IsParent
      FROM device d 
      JOIN license l ON d.DeviceId = l.DeviceId
      JOIN partmaster p ON l.PartMasterId = p.PartMasterId
      JOIN partsubtype s ON p.PartSubtypeId = s.PartSubtypeId
      WHERE d.DeviceId IN ({0})
      AND l.IsActive = 1
      AND p.PartNumber NOT LIKE '%INT-%'
      AND l.AuthorizationCode IS NOT NULL 
      AND l.AuthorizationCode <> ''
      AND l.AuthorizationCode NOT LIKE 'K%'
      ORDER BY d.DeviceId, l.BundleLicenseId, l.RegistrationDate ASC
  ) a
  GROUP BY a.SupportAccountId, a.DeviceId, a.PartMasterId;
";
            Dictionary<string, long> keys = devices.Select(d => d.Id).Distinct().ToDictionary(k => "@deviceId" + k, v => v);
            Dictionary<long, List<SearchAllDetail>> details = new Dictionary<long, List<SearchAllDetail>>();
            Dictionary<long, List<string>> models = new Dictionary<long, List<string>>();
            Dictionary<long, List<SearchAllDetail>> children = new Dictionary<long, List<SearchAllDetail>>();
            using (var command = Connection.GetSqlStringCommand(string.Format(sql, string.Join(", ", keys.Keys))))
            {
                command.CommandType = CommandType.Text;
                foreach (KeyValuePair<string, long> key in keys)
                {
                    Connection.AddInParameter(command, key.Key, DbType.Int64, key.Value);
                }

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                        var isParent = dataReader["IsParent"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsParent"]) : true;
                        if (deviceId == 0)
                        {
                            continue;
                        }
                        if (isParent) // parse parent licences
                        {
                            if (!details.Keys.Contains(deviceId)) // details and models are linked; no need to separate initialization logic
                            {
                                details.Add(deviceId, new List<SearchAllDetail>());
                                models.Add(deviceId, new List<string>());
                            }
                            details[deviceId].Add(new SearchAllDetail
                            {
                                License = dataReader["License"] != DBNull.Value ? Convert.ToString(dataReader["License"]) : string.Empty,
                                AuthorizationCode = dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : string.Empty,
                                DtExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : (DateTime?)null,
                                ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]).ToString("d") : string.Empty,
                                Quantity = dataReader["Quantity"] != DBNull.Value ? Convert.ToString(dataReader["Quantity"]) : string.Empty
                            });
                            models[deviceId].Add(dataReader["ModelName"] != DBNull.Value ? Convert.ToString(dataReader["ModelName"]) : string.Empty);
                        }
                        else
                        {
                            if (!children.Keys.Contains(deviceId))
                            {
                                children.Add(deviceId, new List<SearchAllDetail>());
                            }
                            children[deviceId].Add(new SearchAllDetail
                            {
                                Model = dataReader["ModelName"] != DBNull.Value ? Convert.ToString(dataReader["ModelName"]) : string.Empty,
                                License = dataReader["License"] != DBNull.Value ? Convert.ToString(dataReader["License"]) : string.Empty,
                                AuthorizationCode = dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : string.Empty,
                                DtExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : (DateTime?)null,
                                ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]).ToString("d") : string.Empty,
                                Quantity = dataReader["Quantity"] != DBNull.Value ? Convert.ToString(dataReader["Quantity"]) : string.Empty
                            });
                        }
                    }
                }
            }
            if (models.Any())
            {
                devices.ToList().ForEach(d =>
                {
                    if (models.Keys.Contains(d.Id)) // models and details are synced for respective key; child licenses require a parent
                    {
                        d.ModelName = string.Join("<br/>", models[d.Id]);
                        d.SearchAllDetails = details[d.Id].ToArray();
                        d.ChildLicenses = children.Keys.Contains(d.Id) && children[d.Id].Any() ? children[d.Id].ToArray() : null;
                    }
                });
            }
        }

        public List<DeviceLicenseListItemGroup> ReadMultipleDeviceLicenses(long[] deviceIds, bool hideExpiredLicenses)
        {
            if (!deviceIds.Any()) return new List<DeviceLicenseListItemGroup>();

            string sqlText = @"Select	
            license.AuthorizationCode,
		    license.DeviceId,
		    license.LicenseId,  
		    HasLicenseKey(license.LicenseKey) AS ""HasLicenseKey"",
		    license.EffectiveDate, 
		    license.ExpirationDate,
			license.SupportCenterSupportAccountId,
			SA.Name AS SupportCenterSupportAccountName,
		    PS.LicenseFeaturesName,
		    PM.PartNumber AS ModelName,
		    CASE WHEN PM.ProductFamily = 'CGX' AND s.SupportType IS NOT NULL  
              THEN CONCAT(s.SupportType, ' Support')
    		  ELSE COALESCE(PS.SupportDisplayName, PS.name) END AS LicenseName,
            PS.LicenseType AS IsPerpetual,
			license.VMUseCount,
			PS.Pandbidentification,
            c.ProvisionedDeviceId IS NOT NULL AS IsCsspProvisionedDevice,
            CASE
              WHEN license.VMStubDeviceId > 0 AND license.VMParentLicenseId > 0 THEN
                license.VMParentLicenseId
              ELSE 0
            END AS MasterAuthCodeLicId,
		    license.SaasQuantity
                  FROM license 
				  LEFT JOIN PartMaster PM ON PM.PartMasterId=license.PartMasterId
				  LEFT JOIN partsubtype PS ON PS.PartSubtypeId=PM.PartSubtypeId
				  LEFT JOIN SupportAccount SA ON SA.SupportAccountId = license.SupportCenterSupportAccountId
		          LEFT JOIN csspenduserdevices c ON license.DeviceId = c.ProvisionedDeviceId
                  LEFT JOIN skusupportmapping s ON PM.PartMasterId = s.PartMasterId
				  WHERE license.DeviceId IN ({0})
				  AND (PS.Pandbidentification IS NULL OR PS.Pandbidentification NOT LIKE '%IOTVERT%')
				  AND license.IsActive = 1 ";

            if (hideExpiredLicenses)
                sqlText = sqlText + "   AND (license.ExpirationDate >= CURRENT_DATE() OR PS.LicenseType=1) ";

            sqlText = sqlText + @" ORDER BY license.EffectiveDate,
				  CASE	WHEN instr(PM.PartNumber,'BND') > 0 THEN 1 
						WHEN instr(PM.PartNumber,'TP') > 0 THEN 2 
						WHEN instr(PM.PartNumber,'URL') > 0 THEN 3
						WHEN instr(PM.PartNumber,'VSYS') > 0 THEN 4 
						WHEN instr(PM.PartNumber,'SVC') > 0 THEN 5
						ELSE 5 
				  END, COALESCE(PS.SupportDisplayName, PS.Name);";

            var paramNames = string.Join(", ", Enumerable.Range(0, deviceIds.Length).Select(i => "@p_DeviceId_" + i));

            using (var command = Connection.GetSqlStringCommand(string.Format(sqlText, paramNames)))
            {

                for (var i = 0; i < deviceIds.Length; i++)
                {
                    Connection.AddInParameter(command, "@p_DeviceId_" + i, DbType.Int64, deviceIds[i]);
                }
                using (var dataReader = Connection.ExecuteReader(command))
                {

                    var deviceLicenseGroupList = new List<DeviceLicenseListItemGroup>();
                    while (dataReader.Read())
                    {
                        var expirationDate = dataReader["ExpirationDate"];
                        var effectiveDate = dataReader["EffectiveDate"];
                        var supportCenterSupportAccountId = dataReader["SupportCenterSupportAccountId"];
                        var isPerpetual = dataReader["IsPerpetual"];
                        var partNumber = dataReader["ModelName"].ToString();
                        var isMalbork = EpLogic.IsSkuMalbork(partNumber);
                        var licenseName = dataReader["LicenseName"].ToString();

                        var license = new LicenseListItem
                        {
                            Id = Convert.ToInt64(dataReader["LicenseId"]),
                            AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                            LicenseName = isMalbork ? EpLogic.GetMalborkLicenseDescription(partNumber, licenseName) : licenseName,
                            ModelName = partNumber,
                            LicenseFeaturesName = dataReader["LicenseFeaturesName"].ToString(),
                            ExpirationDate =
                                (expirationDate == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(expirationDate),
                            EffectiveDate = (effectiveDate == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(effectiveDate),
                            SupportCenterSupportAccountId =
                                (supportCenterSupportAccountId == DBNull.Value)
                                    ? (long?)null
                                    : Convert.ToInt64(dataReader["SupportCenterSupportAccountId"]),
                            SupportCenterSupportAccountName = dataReader["SupportCenterSupportAccountName"].ToString(),
                            IsPerpetual = isPerpetual == DBNull.Value ? false : Convert.ToInt32(isPerpetual) == 1,
                            HasLicenseKey = Convert.ToInt32(dataReader["HasLicenseKey"]) == 1,
                            VMUseCount = Convert.ToInt32(dataReader["VMUseCount"]),
                            Pandbidentification = dataReader["Pandbidentification"].ToString(),
                            CsspProvisionedDevice = dataReader["IsCsspProvisionedDevice"].ToString() == "1",
                            MasterAuthCodeLicId = Convert.ToInt64(dataReader["MasterAuthCodeLicId"]),
                            SaasQuantity = Convert.ToInt32(dataReader["SaasQuantity"])
                        };

                        //LightCyber: if expirationDate >= 1/1/2033, then it is Perpetual
                        if (!string.IsNullOrEmpty(license.ModelName) && license.ModelName == "PAN-LCP-KEY"
                            && license.ExpirationDate != null && license.ExpirationDate.Value >= new DateTime(2033, 1, 1, 0, 0, 0))
                        {
                            license.IsPerpetual = true;
                        }

                        UpdateMSeriesQuantityOnTheLicense(license);

                        var deviceId = Convert.ToInt64(dataReader["DeviceId"]);

                        var deviceGroup = deviceLicenseGroupList.SingleOrDefault(x => x.DeviceId == deviceId);

                        if (deviceGroup == null)
                        {
                            deviceLicenseGroupList.Add(new DeviceLicenseListItemGroup
                            {
                                DeviceId = deviceId,
                                Licenses = new List<LicenseListItem>
                    {
                      license,
                    },
                            });
                        }
                        else
                        {
                            deviceGroup.Licenses.Add(license);
                        }
                    }

                    return deviceLicenseGroupList;
                }
            }
        }

        private void UpdateMSeriesQuantityOnTheLicense(LicenseListItem license)
        {
            string deviceManagementLicense = "device management license";
            if (license.LicenseName.Trim().ToLower() == deviceManagementLicense && !string.IsNullOrEmpty(license.Pandbidentification)
                && (license.Pandbidentification.StartsWith("MPRA") || license.Pandbidentification.StartsWith("RENMPRAUPG") || license.Pandbidentification.StartsWith("PRA")))
            {
                var mLicQuantity = 0;
                if (Int32.TryParse(Regex.Match(license.Pandbidentification, @"\d+").Value, out mLicQuantity))
                {
                    if (mLicQuantity > 0)
                    {
                        license.LicenseName = string.Format("{0} qty. {1}", license.LicenseName, mLicQuantity);
                    }
                }
            }
        }


        public LicenseAuthorizationBundle ReadLicenseAuthorizationBundle(string serialNumber, string authorizationCode)
        {

            var result = new LicenseAuthorizationBundle();

            using (var command = Connection.GetStoredProcCommand("GetLicenseAuthorizationBundles"))
            {

                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddInParameter(command, "p_AuthorizationCode", DbType.String, authorizationCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {

                    if (dataReader.Read())
                    {

                        result.DeviceBundleModel = HttpUtility.HtmlEncode(dataReader["DeviceBundle"].ToString());
                        result.LicenseBundleModel = HttpUtility.HtmlEncode(dataReader["LicenseBundle"].ToString());
                    }

                }
            }

            return result;
        }

        public List<object> GetTrialLicenseListBySerialNumber(string serialNumber, string mode)
        {
            List<object> resultList = new List<object>();
            using (var command = Connection.GetStoredProcCommand("GetTrailLicenseListBySerialNumber"))
            {
                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        if (mode == "Trial")
                        {
                            if (dataReader["HasLicense"].ToString() != "1")
                            {
                                var resultDictionay = new Dictionary<string, string>();
                                resultDictionay.Add("GracePeriodDays", HttpUtility.HtmlEncode(dataReader["GracePeriodDays"].ToString()));
                                resultDictionay.Add("Name", HttpUtility.HtmlEncode(dataReader["Name"].ToString()));
                                resultDictionay.Add("description", HttpUtility.HtmlEncode(dataReader["description"].ToString()));
                                resultDictionay.Add("LicenseType", HttpUtility.HtmlEncode(dataReader["LicenseType"].ToString()));
                                resultList.Add(resultDictionay);
                            }
                        }
                        else
                        {
                            var resultDictionay = new Dictionary<string, string>();
                            resultDictionay.Add("GracePeriodDays", HttpUtility.HtmlEncode(dataReader["GracePeriodDays"].ToString()));
                            resultDictionay.Add("Name", HttpUtility.HtmlEncode(dataReader["Name"].ToString()));
                            resultDictionay.Add("description", HttpUtility.HtmlEncode(dataReader["description"].ToString()));
                            resultDictionay.Add("LicenseType", HttpUtility.HtmlEncode(dataReader["LicenseType"].ToString()));
                            resultList.Add(resultDictionay);
                        }
                    }
                }
            }

            // Get Trial license details
            if (mode == "Trial")
            {
                if (resultList.Any(i => ((Dictionary<string, string>)i)["LicenseType"].ToUpper().Contains("IOT") || ((Dictionary<string, string>)i)["LicenseType"] == "SaaSInline"))
                {
                    var iotItems = resultList.Where(i => ((Dictionary<string, string>)i)["LicenseType"].ToUpper().Contains("IOT"));
                    var saaSInlineItem = (Dictionary<string, string>)resultList.SingleOrDefault(i => ((Dictionary<string, string>)i)["LicenseType"] == "SaaSInline");
                    var supportAccountId = GetSupportAccountIdBySerialNumber(serialNumber);

                    // get counter of unexpired IoT & SAAS Trial licenses to validate against IoT & SAAS Trials limit
                    const string queryIotAndSAASTrialActiveCount = @"
SELECT  COUNT(DISTINCT i.licenseid) IotTrialActiveCount
  FROM supportaccount s
  JOIN assetactivation a ON s.SalesforceAccountId = a.salesforceaccountid
  JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
  JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
  JOIN license l ON i.licenseid = l.LicenseId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  WHERE s.SupportAccountId  IN (SELECT distinct s2.SupportAccountId FROM csp.supportaccount s1
  JOIN csp.supportaccount s2 ON s1.SalesforceAccountId = s2.SalesforceAccountId
  WHERE s1.SupportAccountId = @p_supportAccountId)
  AND p.PartNumber IN ('PAN-IOT-TRIAL','PAN-IOT-DRDL-TRIAL')
  AND a.createdon IS NOT NULL
  AND a.createdon >= DATE_ADD(NOW(), INTERVAL - 30 DAY)
  ORDER BY l.CreatedOn DESC;

SELECT  COUNT(DISTINCT i.licenseid) SaaSInlineTrialActiveCount
  FROM supportaccount s
  JOIN assetactivation a ON s.SalesforceAccountId = a.salesforceaccountid
  JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
  JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
  JOIN license l ON i.licenseid = l.LicenseId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  WHERE s.SupportAccountId  IN (SELECT distinct s2.SupportAccountId FROM csp.supportaccount s1
  JOIN csp.supportaccount s2 ON s1.SalesforceAccountId = s2.SalesforceAccountId
  WHERE s1.SupportAccountId = @p_supportAccountId)
  AND p.PartNumber LIKE '%SAAS-INLINE-TRIAL%'
  AND a.createdon IS NOT NULL
  AND a.createdon >= DATE_ADD(NOW(), INTERVAL - 60 DAY)
  ORDER BY l.CreatedOn DESC;
";

                    var enumerable = iotItems.ToList();
                    using (var command = Connection.GetSqlStringCommand(queryIotAndSAASTrialActiveCount))
                    {
                        Connection.AddInParameter(command, "@p_supportAccountId", DbType.String, supportAccountId);
                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            if (dataReader.Read())
                            {
                                //Adding IotTrialActiveCount key/value to each IOT checkboxes
                                foreach (var x in enumerable)
                                {
                                    ((Dictionary<string, string>)x).Add("IotTrialActiveCount", dataReader["IotTrialActiveCount"] != DBNull.Value ? dataReader["IotTrialActiveCount"].ToString() : "0");
                                }
                            }
                            if (dataReader.NextResult() && dataReader.Read())
                            {
                                saaSInlineItem?.Add("SaaSInlineTrialActiveCount", dataReader["SaaSInlineTrialActiveCount"] != DBNull.Value ? dataReader["SaaSInlineTrialActiveCount"].ToString() : "0");
                            }
                        }
                    }

                    const string queryCdlLicensesCount = @"
SELECT COUNT(1) CdlLicensesCount 
  FROM device d 
  JOIN license l ON d.SupportAccountId = l.SupportAccountId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  WHERE d.SerialNumber = @p_serialNumber
  AND p.PartNumber LIKE '%-LGS%' 
  AND p.PartNumber NOT LIKE '%-METRICS%' 
  AND p.PartNumber NOT LIKE '%-DL%' 
  AND d.IsActive = 1
  AND l.IsActive = 1
  AND l.ExpirationDate IS NOT NULL
  AND l.ExpirationDate >= NOW();
";
                    using (var command = Connection.GetSqlStringCommand(queryCdlLicensesCount))
                    {
                        Connection.AddInParameter(command, "@p_serialNumber", DbType.String, serialNumber);
                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            if (dataReader.Read())
                            {
                                foreach (var y in enumerable)
                                {
                                    ((Dictionary<string, string>)y).Add("CdlLicensesCount", dataReader["CdlLicensesCount"] != DBNull.Value ? dataReader["CdlLicensesCount"].ToString() : "0");
                                }
                                saaSInlineItem?.Add("CdlLicensesCount", dataReader["CdlLicensesCount"] != DBNull.Value ? dataReader["CdlLicensesCount"].ToString() : "0");
                            }
                        }
                    }
                }
            }

            return resultList;
        }

        public LicenseKey ReadLicenseKeybyId(long licenseId)
        {

            var licenseKey = new LicenseKey();
            using (var command = Connection.GetStoredProcCommand("GetLicenseKeyByLicenseId"))
            {

                Connection.AddInLicenseIdParameter(command, licenseId);

                using (var dataReader = Connection.ExecuteReader(command))
                {

                    if (dataReader.Read())
                    {

                        //This is a simple object and only used in one place so I elected not to do mapping here
                        licenseKey.Key = dataReader["LicenseKey"].ToString();
                        licenseKey.Name = dataReader["KeyName"].ToString();
                        licenseKey.PartSubTypeName = dataReader["PartSubTypeName"].ToString();

                    }

                }
            }

            // Add adv in filename for ADNS and AWF
            if (licenseKey != null)
            {
                if (licenseKey.PartSubTypeName == ADNS || licenseKey.PartSubTypeName == AWF)
                {
                    int index = licenseKey.Name.IndexOf('-');

                    if (index != -1) 
                    {
                        licenseKey.Name = licenseKey.Name.Insert(index + 1, "adv"); 
                       
                    }
                   
                }
            }
            return licenseKey;
        }

        public string ReadNFRSVCExpirationFromPandb(string serialNumber)
        {

            string returnValue = "2000-1-1";
            string updateSqlQuey = "select (l.expiration) as expiration from lic_features l inner join device_master d on l.devid=d.lid where l.part_id like 'PAN-SVC-NFR%' and d.serialnum=@p_SerialNumber and l.active=1";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey))
            {

                PANDBConnection.AddInParameter(panCommand, "@p_SerialNumber", DbType.String, serialNumber);

                using (IDataReader dataReader = PANDBConnection.ExecuteReader(panCommand))
                {

                    if (dataReader.Read())
                    {
                        if (dataReader["expiration"] != DBNull.Value)
                        {
                            returnValue = (Convert.ToString(dataReader["expiration"]) != ""
                                                             ? Convert.ToString(dataReader["expiration"])
                                                             : "2000-1-1");
                        }
                    }
                }
            }

            return returnValue;
        }

        public string GetorUpdateConflictingLicense(string serialNumber, string authorizationCode)
        {


            using (var command = Connection.GetStoredProcCommand("GetOrUpdateConflictingDeviceLicense"))
            {

                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddInParameter(command, "p_AuthorizationCode", DbType.String, authorizationCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {

                    if (dataReader.Read())
                    {

                        //This is a simple object and only used in one place so I elected not to do mapping here
                        return dataReader["ConflictingLicense"].ToString();


                    }
                }
            }

            return "";
        }

        public bool UpdateDeviceLocationFor4HrRma(string serialNumber, string city, string postalCode, string countryCode)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("Update4HRDeviceLocation"))
                {

                    Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "p_City", DbType.String, city);
                    Connection.AddInParameter(command, "p_postalCode", DbType.String, postalCode);
                    Connection.AddInParameter(command, "p_CountryCode", DbType.String, countryCode);
                    Connection.ExecuteNonQuery(command);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateLicFeaturesForNsxProvisioning(string selectedAuthCode, string newSerialNumber)
        {
            bool isSuccess = false;
            long? bundleLfid = null;
            DateTime? bundleExpiration = null;
            long? nsxlfid = null;

            try
            {
                // Get the NSX license Id (lfid)
                // Get the bundlelfid/expiration
                const string sqlQuery = @"
                    SELECT 
                        f1.lfid AS bundlelfid, 
                        f1.expiration, 
                        f2.lfid AS lfid
                    FROM pandb.lic_features f1
                    LEFT JOIN pandb.device_master d ON d.serialNum = @p_snum
                    LEFT JOIN pandb.lic_features f2 ON f2.devId = d.lid AND f2.active = 1
                    INNER JOIN pandb.partmaster p1 ON p1.part_id = f2.part_id 
                    WHERE f1.auth_code = @p_authcode
                    AND p1.type_id LIKE '%SUP%';
                    ";

                using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
                {
                    PANDBConnection.AddInParameter(command, "@p_snum", DbType.String, newSerialNumber);
                    PANDBConnection.AddInParameter(command, "@p_authcode", DbType.String, selectedAuthCode);
                    using (var dataReader = PANDBConnection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            bundleLfid = dataReader["bundlelfid"] != DBNull.Value
                                             ? Convert.ToInt64(dataReader["bundlelfid"])
                                             : (long?)null;
                            bundleExpiration = dataReader["expiration"] != DBNull.Value
                                                   ? Convert.ToDateTime(dataReader["expiration"])
                                                   : (DateTime?)null;
                            nsxlfid = dataReader["lfid"] != DBNull.Value ? Convert.ToInt64(dataReader["lfid"]) : (long?)null;
                            isSuccess = true;
                        }
                    }
                }

                if (isSuccess && bundleLfid.HasValue && nsxlfid.HasValue)
                {
                    isSuccess = true;
                    if (bundleExpiration.HasValue)
                    {
                        const string updQuery = @"
update lic_features set expiration=@p_expiration,bundle_lfid=@p_bundlelfid where lfid=@p_lfid;";
                        using (var cmd = PANDBConnection.GetSqlStringCommand(updQuery))
                        {
                            PANDBConnection.AddInParameter(cmd, "@p_expiration", DbType.DateTime, bundleExpiration.Value);
                            PANDBConnection.AddInParameter(cmd, "@p_bundlelfid", DbType.Int64, bundleLfid);
                            PANDBConnection.AddInParameter(cmd, "@p_lfid", DbType.Int64, nsxlfid);
                            PANDBConnection.ExecuteNonQuery(cmd);
                        }
                    }
                    else
                    {
                        const string updQuery = @"
update lic_features set bundle_lfid=@p_bundlelfid where lfid=@p_lfid;";
                        using (var cmd = PANDBConnection.GetSqlStringCommand(updQuery))
                        {
                            PANDBConnection.AddInParameter(cmd, "@p_bundlelfid", DbType.Int64, bundleLfid);
                            PANDBConnection.AddInParameter(cmd, "@p_lfid", DbType.Int64, nsxlfid);
                            PANDBConnection.ExecuteNonQuery(cmd);
                        }
                    }
                }
                isSuccess = false;
            }
            catch (Exception oex)
            {

                var ex = oex.Message;
                throw;
            }
            return isSuccess;
        }

        public IEnumerable<EnterpriseAgreementItem> GetEnterpriseAgreements(long supportAccountId)
        {
            var result = new List<EnterpriseAgreementItem>();
            using (var command = Connection.GetStoredProcCommand("GetEnterpriseAgreements"))
            {
                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var item = new EnterpriseAgreementItem
                        {
                            SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                            SupportAccountName = dataReader.GetColumnValue<string>("Name"),
                            AuthorizationCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                            MainAuthCode = dataReader.GetColumnValue<string>("MainAuthCode"),
                            ExpirationDate = dataReader.GetColumnValue<DateTime>("ExpirationDate"),
                            LicenseId = dataReader.GetColumnValue<long>("LicenseId"),
                            PartNumber = dataReader.GetColumnValue<string>("PartNumber"),
                            Description = dataReader.GetColumnValue<string>("Description"),
                            BundleLicenseId = dataReader.GetColumnValue<long?>("BundleLicenseId"),
                            EnterpriseAgreement = dataReader.GetColumnValue<string>("EnterpriseAgreement"),
                            ParentLicenseId = dataReader.GetColumnValue<long?>("ParentLicenseId"),
                            CountOfDeviceEaIsAppliedTo = dataReader.GetColumnValue<int>("DeviceCount"),
                            AscId = dataReader.GetColumnValue<long>("AscId"),
                            AscName = dataReader.GetColumnValue<string>("AscName"),
                            BoundedOrUnbounded = "0"
                        };
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public IEnumerable<EnterpriseAgreementItem> GetEnterpriseAgreementsForTrapsELA(long supportAccountId)
        {
            var result = new List<EnterpriseAgreementItem>();

            using (var command = Connection.GetStoredProcCommand("GetEnterpriseAgreementsForTrapsELA"))
            {
                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var item = new EnterpriseAgreementItem
                        {
                            SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                            SupportAccountName = dataReader.GetColumnValue<string>("Name"),
                            AuthorizationCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                            MainAuthCode = dataReader.GetColumnValue<string>("MainAuthCode"),
                            ExpirationDate = dataReader.GetColumnValue<DateTime>("ExpirationDate"),
                            LicenseId = dataReader.GetColumnValue<long>("LicenseId"),
                            PartNumber = dataReader.GetColumnValue<string>("PartNumber"),
                            Description = dataReader.GetColumnValue<string>("Description"),
                            BundleLicenseId = dataReader.GetColumnValue<long?>("BundleLicenseId"),
                            EnterpriseAgreement = dataReader.GetColumnValue<string>("EnterpriseAgreement"),
                            ParentLicenseId = dataReader.GetColumnValue<long?>("ParentLicenseId"),
                            CountOfDeviceEaIsAppliedTo = dataReader.GetColumnValue<int>("DeviceCount"),
                            TotalQuantity = dataReader.GetColumnValue<int>("CapQuantity"),
                            BoundedOrUnbounded = dataReader.GetColumnValue<string>("BoundedOrUnbounded")
                        };
                        result.Add(item);
                    }
                }
            }

            return result;
        }


        public bool TransferCloudLicenses(string fromSerialNumber, string toSerialNumber)
        {
            try
            {
                var url = ConfigurationManager.AppSettings["LicenseApi"] + "/coresvc/v1/license/transfer/panorama";
                var thumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
                var payload = JsonConvert.SerializeObject(new { fromSerial = fromSerialNumber, toSerial = toSerialNumber });
                using (var httpResponse = (HttpWebResponse)WebCommon.GetWebRequestWithClientCertificate(url, "POST", thumbprint, payload).GetResponse())
                {
                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        Exception ex = new Exception(string.Format("Error TransferCloudLicenses. From SN: '{0}', To SN: '{1}', Response: 200", fromSerialNumber, toSerialNumber));
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        throw ex;
                    }
                }
                /* Legacy code
                using (var command = Connection.GetStoredProcCommand("TransferCloudLicense"))
                {
                    Connection.AddInParameter(command, "p_fromSerialNumber", DbType.String, fromSerialNumber);
                    Connection.AddInParameter(command, "p_toSerialNumber", DbType.String, toSerialNumber);
                    Connection.ExecuteNonQuery(command);
                    return true;
                }
                */
                return true;
            }
            catch (WebException ex)
            {
                string errorMessage = string.Empty;
                using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    errorMessage = streamReader.ReadToEnd();
                }
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error TransferCloudLicenses. From SN: '{0}', To SN: '{1}', Response: {2}, Exception: {3}:", fromSerialNumber, toSerialNumber, errorMessage, ex.Message)));
                throw;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error TransferCloudLicenses. From SN: '{0}', To SN: '{1}', Exception: {2}:", fromSerialNumber, toSerialNumber, ex.Message)));
                throw;
            }
        }
        public bool UpdateSupportForVMELAPanorama(string selectedAuthCode, string newSerialNumber)
        {
            bool isSuccess = false;
            string partNumber = string.Empty;
            string newSupportPartNumber = string.Empty;
            string supportAuthcode = string.Empty;
            long? lfid = null;

            try
            {

                // Step 1: Get part Id of VM ELA Authcode
                const string sqlQuery = @"SELECT lf.part_id FROM pandb.lic_features lf WHERE lf.auth_code = @p_authcode;";

                using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
                {
                    PANDBConnection.AddInParameter(command, "@p_authcode", DbType.String, selectedAuthCode);
                    using (var dataReader = PANDBConnection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            partNumber = Convert.ToString(dataReader["part_id"]);
                        }
                    }
                }
                // Step 2: Get License Id of Support Authcode
                const string supportQuery = @"SELECT  lf.lfid FROM device_master dm  JOIN lic_features lf ON dm.lid = lf.devID  WHERE dm.serialnum =  @p_serialnum;";

                using (var command = PANDBConnection.GetSqlStringCommand(supportQuery))
                {
                    PANDBConnection.AddInParameter(command, "@p_serialnum", DbType.String, newSerialNumber);
                    using (var dataReader = PANDBConnection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            lfid = dataReader["lfid"] != DBNull.Value ? Convert.ToInt64(dataReader["lfid"]) : (long?)null;
                        }
                    }
                }

                // Step 3: If VM ELA license support is not premium then get appropriate support SKU
                if (lfid != null && !string.IsNullOrEmpty(partNumber))
                {
                    if (partNumber.Contains("-USG-"))
                        newSupportPartNumber = "PAN-SVC-USG-PRA-1K-CP";
                    else if (partNumber.Contains("-BKLN-"))
                        newSupportPartNumber = "PAN-SVC-BKLN-PRA-1K-CP";
                    else if (partNumber.Contains("-BKLNUSG-"))
                        newSupportPartNumber = "PAN-SVC-BKLNUSG-PRA-1K-CP";
                    else if (partNumber.Contains("-PLAT-"))
                        newSupportPartNumber = "PAN-SVC-PLAT-PRA-1K-CP";
                    else if (partNumber.Contains("-STND-"))
                        newSupportPartNumber = "PAN-SVC-STND-PRA-1K-CP";
                    else if (partNumber.Contains("-STNDUSG-"))
                        newSupportPartNumber = "PAN-SVC-STNDUSG-PRA-1K-CP";
                }

                if (!string.IsNullOrEmpty(newSupportPartNumber))
                {
                    // Step 5: Back up the support Authcode before updating PartID
                    const string bkpQuery = @"CALL pandb.backup_lic_feature_by_lfid(@p_lfid,'update support for case creation for VM ELA panorama','CSP');";
                    using (var cmd = PANDBConnection.GetSqlStringCommand(bkpQuery))
                    {
                        PANDBConnection.AddInParameter(cmd, "@p_lfid", DbType.Int64, lfid);
                        PANDBConnection.ExecuteNonQuery(cmd);

                    }
                    // Step 6: Update part id of the support Authcode
                    const string updQuery = @"update lic_features set part_id=@p_part_id,last_modified= NOW() where lfid=@p_lfid;";
                    using (var cmd = PANDBConnection.GetSqlStringCommand(updQuery))
                    {
                        PANDBConnection.AddInParameter(cmd, "@p_part_id", DbType.String, newSupportPartNumber);
                        PANDBConnection.AddInParameter(cmd, "@p_lfid", DbType.Int64, lfid);
                        PANDBConnection.ExecuteNonQuery(cmd);
                    }
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error Adding Support:" + ex.Message));
                throw;
            }
            return isSuccess;
        }

        /// <summary>
        /// Gets the type of authcode based on it's PanDbIdentification
        /// </summary>
        /// <param name="authCode"></param>
        /// <returns></returns>
	    public string GetAuthCodeType(string authCode)
        {
            string licenseType = string.Empty;
            string authCodeTypeQuery = @"SELECT  ps.PanDbIdentification As PanDbIdentification FROM license l JOIN partmaster p ON l.PartMasterId = p.PartMasterId
                                          JOIN partsubtype ps ON p.PartSubtypeId = ps.PartSubtypeId
                                            WHERE l.AuthorizationCode=@p_AuthCode";

            using (var command = Connection.GetSqlStringCommand(authCodeTypeQuery))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authCode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        licenseType = HttpUtility.HtmlEncode(Convert.ToString(dataReader["PanDbIdentification"]));
                    }
                }
            }

            return licenseType;

        }

        /// <summary>
        /// Gets the type of panOs based on it's PanDbIdentification
        /// </summary>
        /// <param name="authCode"></param>
        /// <param name="licenseId"></param>
        /// <returns></returns>
	    public string GetPanOsVersion(string authCode)
        {
            string panOsVersion = string.Empty;
            string authCodeTypeQuery = @"select panos from csp.deploymentprofile d join csp.license l on d.DpLicenseId=l.LicenseId
where l.AuthorizationCode=@p_AuthCode";

            using (var command = Connection.GetSqlStringCommand(authCodeTypeQuery))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authCode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        panOsVersion = HttpUtility.HtmlEncode(Convert.ToString(dataReader["panos"]));
                    }
                }
            }

            return panOsVersion;

        }

        public bool IsIotAuthCodeActivation(string authcode)
        {
            string authCodeTypeQuery = @"SELECT COUNT(1) IsIotLicense FROM license l JOIN partmaster p ON l.PartMasterId = p.PartMasterId WHERE l.AuthorizationCode = @p_AuthCode AND p.PartNumber LIKE '%-IOT%';";

            using (var command = Connection.GetSqlStringCommand(authCodeTypeQuery))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authcode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["IsIotLicense"]);
                    }
                }
            }

            return false;
        }

        public bool IsAiOpsAuthCodeActivation(string authcode)
        {
            string authCodeTypeQuery = @"SELECT COUNT(1) IsAiOpsLicense FROM license l JOIN partmaster p ON l.PartMasterId = p.PartMasterId WHERE l.AuthorizationCode = @p_AuthCode AND p.PartNumber LIKE '%-AIOPS%';";

            using (var command = Connection.GetSqlStringCommand(authCodeTypeQuery))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authcode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["IsAiOpsLicense"]);
                    }
                }
            }

            return false;
        }

        public long GetSupportAccountIdBySerialNumber(string serialNumber)
        {
            long supportAccountId = 0;
            const string sqlQuery = @"SELECT d.SupportAccountId FROM device d WHERE d.SerialNumber = @p_serialNumber;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_serialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        supportAccountId = Int64.Parse(dataReader["SupportAccountId"].ToString());
                    }
                }
            }

            return supportAccountId;
        }

        public bool IsRaptorEnterpriseBundle(string authcode)
        {
            const string query = @"
                SELECT COUNT(1) > 0 IsRaptorBundleEnterprise
                  FROM license l 
                  JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
                  JOIN partsubtype ps ON pm.PartSubtypeId = ps.PartSubtypeId
                  WHERE l.AuthorizationCode = @p_AuthCode
                  AND l.IsActive = 1
                  AND ps.LicenseFeaturesName REGEXP '(410|440|450|460).*BENT$';
                ";
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authcode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["IsRaptorBundleEnterprise"]);
                    }
                }
            }
            return false;
        }

        public bool IsSellingBundleWithIot(string authcode)
        {
            const string query = @"
                SELECT COUNT(1) > 0 IsSellingBundleWithIoT
                  FROM license l 
                  JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
                  WHERE l.AuthorizationCode = @p_AuthCode
                  AND l.IsActive = 1
                  AND pm.partNumber REGEXP '((34|54)[0-9]{2}).*BND-NETSEG-CAMPUS.*';
                ";
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authcode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["IsSellingBundleWithIoT"]);
                    }
                }
            }
            return false;
        }
    }
}
