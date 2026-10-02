using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class SaasDataAccess : BaseDataAccess,ISaasDataAccess
    {

        public IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long supportAccountId)
        {
            var panoramaSerialNumbers = new List<PanoramaSerialNumber>();

            const string sqlQuery = @"SELECT d.SerialNumber
                         FROM panoramapartmaster m
                         JOIN device d ON m.PartMasterId = d.PartMasterId
                         JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1 
                         AND l.ExpirationDate >= NOW()
                         JOIN Partmaster p ON l.Partmasterid = p.Partmasterid AND p.PartTypeId in (8, 13)
                         WHERE m.IsVirtual = 1
                         AND d.SupportAccountId = @p_SupportAccountId
                         AND not
                         (
                           SELECT count(1) > 0
                           FROM license lc
                           JOIN partmaster pc on lc.PartMasterId = pc.PartMasterId
                           AND (pc.partnumber LIKE '%LGS%' OR pc.partnumber LIKE '%CLCS%' OR pc.partnumber LIKE '%FLS%')
                           WHERE lc.IsActive = 1
                           AND d.DeviceId = lc.DeviceId
                           AND lc.ExpirationDate >= now()
                           AND lc.AuthorizationCode LIKE 'P%'
                        );";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var serialnum = new PanoramaSerialNumber();
                        serialnum.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        panoramaSerialNumbers.Add(serialnum);
                    }
                }
            }
            return panoramaSerialNumbers;
        }

        public IEnumerable<AvailableSaaSActivations> GetAvailableSaasActivations(long supportAccountId)
        {
            var availableActivations = new List<AvailableSaaSActivations>();
            var bundleLicenses = new List<BundleLicenseItem>();
            using (var command = Connection.GetStoredProcCommand("GetCloudServicesBySupportAccount"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                var allLicenses = new Dictionary<long, AvailableSaaSActivations>();
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var activation = new AvailableSaaSActivations();
                        var licenseId = Convert.ToInt64(dataReader["LicenseId"]);
                        var bundleLicenseId = Convert.ToInt64(dataReader["BundleLicenseId"]);
                        activation.LicenseID = licenseId;
                        activation.AuthorizationCode = dataReader["AuthCode"] != DBNull.Value ? dataReader["AuthCode"].ToString() : String.Empty;
                        activation.SerialNumber = dataReader["SerialNumber"] != DBNull.Value ? dataReader["SerialNumber"].ToString() : String.Empty;
                        activation.Quantity = dataReader["Quantity"] != DBNull.Value ? dataReader["Quantity"].ToString() : String.Empty;
                        activation.Capacity = dataReader["Capacity"] != DBNull.Value ? dataReader["Capacity"].ToString() : String.Empty;
                        activation.ModelName = dataReader["ModelName"] != DBNull.Value ? dataReader["ModelName"].ToString() : String.Empty;
                        activation.Description = dataReader["description"] != DBNull.Value ? dataReader["description"].ToString() : String.Empty;
                        activation.ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? DateTime.Parse(dataReader["ExpirationDate"].ToString()).ToShortDateString() : String.Empty;
                        activation.AssoicatedDevices = dataReader["AssociatedDevice"] != DBNull.Value ? dataReader["AssociatedDevice"].ToString() : String.Empty;
                        activation.Region = dataReader["Region"] != DBNull.Value ? dataReader["Region"].ToString() : String.Empty;
                        activation.PanDbIdentification = dataReader["PanDbIdentification"] != DBNull.Value ? dataReader["PanDbIdentification"].ToString() : String.Empty;
                        activation.ShowPanoramaAssociation = dataReader["ShowPanoramaAssociation"] != DBNull.Value ? Convert.ToBoolean(dataReader["ShowPanoramaAssociation"]) : false;
                        activation.BundleLicenseId = dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0;
                        if (!allLicenses.Keys.Contains(licenseId))
                        {
                            allLicenses.Add(licenseId, activation); // add the license to dictionary for further binding of siblings and children
                        }
                        availableActivations.Add(activation);
                    }

                    dataReader.NextResult();
                    while (dataReader.Read())
                    {
                        bundleLicenses.Add(new BundleLicenseItem()
                        {
                            BundleLicenseId = dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0,
                            AuthorizationCode = dataReader["AuthCode"] != DBNull.Value ? dataReader["AuthCode"].ToString() : String.Empty,
                            SerialNumber = dataReader["SerialNumber"] != DBNull.Value ? dataReader["SerialNumber"].ToString() : String.Empty,
                            Quantity = dataReader["quantity"] != DBNull.Value ? dataReader["quantity"].ToString() : String.Empty,
                            ModelName = dataReader["ModelName"] != DBNull.Value ? dataReader["ModelName"].ToString() : String.Empty,
                            ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? DateTime.Parse(dataReader["ExpirationDate"].ToString()).ToShortDateString() : String.Empty
                        });
                    }

                    // Read Prisma Access Phase 1 Other master records
                    dataReader.NextResult();
                    while (dataReader.Read())
                    {
                        // PartMasterId
                        // ModelName, Description, PanDbIdentification
                        var bundleLicenseId = dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0;
                        if (bundleLicenseId > 0
                            && allLicenses.Keys.Contains(bundleLicenseId))
                        {
                            AvailableSaaSActivations item = allLicenses[bundleLicenseId];
                            if (item.DependendMasterRecords == null)
                            {
                                item.DependendMasterRecords = new List<AvailableSaaSActivations>();
                            }
                            item.DependendMasterRecords.Add(new AvailableSaaSActivations()
                            {
                                LicenseID = Convert.ToInt64(dataReader["LicenseId"]),
                                AuthorizationCode = dataReader["AuthCode"] != DBNull.Value ? dataReader["AuthCode"].ToString() : String.Empty,
                                SerialNumber = dataReader["SerialNumber"] != DBNull.Value ? dataReader["SerialNumber"].ToString() : String.Empty,
                                Quantity = dataReader["Quantity"] != DBNull.Value ? dataReader["Quantity"].ToString() : String.Empty,
                                Capacity = dataReader["Capacity"] != DBNull.Value ? dataReader["Capacity"].ToString() : String.Empty,
                                ModelName = dataReader["ModelName"] != DBNull.Value ? dataReader["ModelName"].ToString() : String.Empty,
                                Description = dataReader["Description"] != DBNull.Value ? dataReader["Description"].ToString() : String.Empty,
                                ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? DateTime.Parse(dataReader["ExpirationDate"].ToString()).ToShortDateString() : String.Empty,
                                AssoicatedDevices = dataReader["AssociatedDevice"] == DBNull.Value || dataReader["AssociatedDevice"] != DBNull.Value && dataReader["AssociatedDevice"].ToString() == "N/A" 
                                    ? item.AssoicatedDevices 
                                    : dataReader["AssociatedDevice"].ToString(),
                                Region = dataReader["Region"] != DBNull.Value ? dataReader["Region"].ToString() : String.Empty,
                                PanDbIdentification = dataReader["PanDbIdentification"] != DBNull.Value ? dataReader["PanDbIdentification"].ToString() : String.Empty,
                                ShowPanoramaAssociation = dataReader["ShowPanoramaAssociation"] != DBNull.Value ? Convert.ToBoolean(dataReader["ShowPanoramaAssociation"]) : false,
                                BundleLicenseId = bundleLicenseId
                            });
                        }
                    }

                    // Prisma Access Phase 1 Child records of master records
                    dataReader.NextResult();
                    int childrenCount = 0;
                    while (dataReader.Read())
                    {
                        var bundleLicenseId = dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0;
                        if (bundleLicenseId > 0
                            && allLicenses.Keys.Contains(bundleLicenseId))
                        {
                            AvailableSaaSActivations item = allLicenses[bundleLicenseId];
                            if (item.Children == null)
                            {
                                item.Children = new List<BundleLicenseItemBase>();
                            }
                            item.Children.Add(new BundleLicenseItemBase()
                            {
                                Id = ++childrenCount,
                                LicenseId = dataReader["LicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["LicenseId"]) : 0,
                                AuthorizationCode = dataReader["AuthCode"] != DBNull.Value ? dataReader["AuthCode"].ToString() : String.Empty,
                                ModelName = dataReader["ModelName"] != DBNull.Value ? dataReader["ModelName"].ToString() : String.Empty,
                                Description = dataReader["Description"] != DBNull.Value ? dataReader["Description"].ToString() : String.Empty,
                                Quantity = dataReader["Quantity"] != DBNull.Value ? dataReader["Quantity"].ToString() : String.Empty,
                                Capacity = dataReader["Capacity"] != DBNull.Value ? dataReader["Capacity"].ToString() : String.Empty,
                                ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? DateTime.Parse(dataReader["ExpirationDate"].ToString()).ToShortDateString() : String.Empty,
                                BundleLicenseId = bundleLicenseId
                            });
                        }
                    }
                }
            }

            AddLicensesToSaasBundles(availableActivations, bundleLicenses);

            return availableActivations;
        }

      

        private void AddLicensesToSaasBundles(List<AvailableSaaSActivations> availableSaasActivations,
            List<BundleLicenseItem> bundleLicenses)
        {
            foreach (var sa in availableSaasActivations)
            {
                if (sa.BundleLicenseId.HasValue && sa.BundleLicenseId.Value > 0)
                {
                    var bundleLicenseId = sa.BundleLicenseId.Value;
                    var licenses = bundleLicenses.Where(x => x.BundleLicenseId == bundleLicenseId).ToList();

                    if (licenses.Any())
                    {
                        sa.BundleLicenses = licenses;
                    }
                }
            }
        }

        public IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudService(long SupportAccountID,string mode,string serialNumber)
        {

            var panoramaSerialNumbers = new List<PanoramaSerialNumber>();
            using (var command = Connection.GetStoredProcCommand("GetPanoramaForOTP"))
            {
                Connection.AddInSupportAccountIdParameter(command, SupportAccountID);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var serialnum = new PanoramaSerialNumber();
                        serialnum.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        panoramaSerialNumbers.Add(serialnum);
                    }
                }
            }
            return panoramaSerialNumbers;
        }

        /// <summary>
        /// Check if the given auth Code is already registered
        /// </summary>
        /// <param name="authCode">Authorization code to be checked</param>
        /// <returns>Registered / Unregistered flag</returns>
        public bool IsAuthCodeAlreadyRegistered(string authCode)
        {

            //Original PanDB query: select serialnum, acctid from device_master where active=1 and serialnum='$serial_number'

            const string sqlQuery = @"
SELECT * FROM license l WHERE l.AuthorizationCode = @p_AuthCode
  AND l.RegistrationDate is NULL;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the authcode details, which has to be activated 
        /// </summary>
        /// <param name="authCode">Authorization code that needs to be activated</param>
        /// <returns>all the details about AuthCode</returns>
        public SaasAuthCodeDetails GetAuthCodeDetails(string authCode)
        {

            const string sqlQuery = @"
SELECT 
  l.AuthorizationCode, 
  l.RegistrationDate, 
  l.ExpirationDate, 
  l.IsActive, 
  pm.PartNumber, 
  l.VMStubDeviceId,
  pst.PanDbIdentification,
l.DeviceId
FROM license l
INNER JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
INNER JOIN partsubtype pst ON pm.PartSubtypeId = pst.PartSubtypeId
WHERE l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                var saasAuthCodes = new List<SaasAuthCodeDetails>();

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        var SaasAuthCode = new SaasAuthCodeDetails();
                        SaasAuthCode.PopulateProperties(dataReader);
                        saasAuthCodes.Add(SaasAuthCode);
                    }
                }

                return saasAuthCodes.Count != 1 ? null : saasAuthCodes.Single();
            }
        }

        public bool IsHAPanorama(string serialNumber)
        {

            const string sqlQuery = @"SELECT s.PanoramaSerialNumber FROM secaaspanoramaregistration s
                                      JOIN tenantlookup t ON s.TenantId = t.TenantId
                                      JOIN appportalapps a ON t.AppPortalAppsId = a.AppPortalAppsId
                                      WHERE(( s.PanoramaSerialNumber = @p_serialnumber AND s.SecondarySerialNumber IS NOT NULL )
                                      OR( s.SecondarySerialNumber = @p_serialnumber AND s.PanoramaSerialNumber IS NOT NULL ))
                                      AND s.IsActive =1 AND s.IsHA =1 AND a.AppInternalName = 'Logging Service';";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_serialnumber", DbType.String, serialNumber);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public string GetFreeLoggingService(string region, long supportAccountId)
        {
            var serialNumber = "";
            const string sqlQuery = @"SELECT t.SerialNumber FROM tenantlookup t 
                  JOIN device d ON t.SerialNumber = d.SerialNumber
                  JOIN license l ON d.DeviceId = l.DeviceId
                  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
                  JOIN partsubtype p1 ON p.PartSubtypeId = p1.PartSubtypeId
                  WHERE  t.SupportAccountId = @supportAccountId AND t.TenantRegion = @region
                   AND p1.PanDbIdentification ='FLCAASSC' AND l.IsActive =1;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@supportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@region", DbType.String, region);

                

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        serialNumber = dataReader["SerialNumber"] != DBNull.Value ? HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString()) : "";
                    }
                }

                return serialNumber;
            }
        }

        public bool IsPanoramaAssociation(string authcode)
        {
            bool isActivated = IsAuthCodeAlreadyRegistered(authcode);
            bool hasPanorama = false;
            string query = @"SELECT 1
                              FROM license l  
                              JOIN license l1 ON l.LicenseId = l1.VMParentLicenseId
                              JOIN device d ON l1.DeviceId = d.DeviceId
                              JOIN partmaster pd ON d.PartMasterId = pd.PartMasterId
                              JOIN partmaster pl ON l.PartMasterId = pl.PartMasterId
                              WHERE l.AuthorizationCode = @authCode AND
                              pd.PartNumber NOT LIKE 'PAN-PA-SAAS-SW%'
                              AND pl.PartNumber LIKE '%lgs%';";
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "@authCode", DbType.String, authcode);

                var result = Connection.ExecuteScalar(command);
                hasPanorama = result != null && Convert.ToInt16(result) == 1 ? true : false;
                
            }
            if (isActivated && !hasPanorama)
                return true;
            else
                return false;
        }

        public bool IsEligibleForCdlPanoramaAssociation(string authcode)
        {
            bool hasCDLPanorama = false;
            try
            {
                string query = @"SELECT plc.partnumber LIKE '%LGS%' OR plc.partnumber LIKE '%CLCS%' 
                       OR plc.partnumber LIKE '%FLS%' IsCdl,
                       CASE WHEN dc.deviceid IS NOT NULL AND lc.supportaccountid IS NOT NULL 
                       AND lc.expirationdate IS NOT NULL THEN 1 ELSE 0 END IsCdlActivated,
                       lp.authorizationcode IS NOT NULL AND COUNT(1) > 0 CdlHasPanoramaAssociation,
                       NOT lc.expirationdate >= NOW() IsCdlExpired
                       FROM license lc
                       JOIN partmaster plc ON lc.partmasterid = plc.partmasterid
                       LEFT JOIN device dc ON lc.deviceid = dc.deviceid and dc.isactive = 1
                       LEFT JOIN license lp on lc.licenseid = lp.vmparentlicenseid AND lp.isactive = 1
                       AND lp.expirationdate >= NOW() AND lp.authorizationcode REGEXP '^P'
                       LEFT JOIN partmaster plp ON lp.partmasterid = plp.partmasterid 
                       AND (plp.partnumber LIKE '%LGS%'  OR plp.partnumber LIKE '%CLCS%' 
                       OR plp.partnumber LIKE '%FLS%')
                       WHERE lc.authorizationcode in (@authCode)
                       AND lc.isactive = 1;";

                CdlAssociationWithPanoramaEligibility cdlPanorama = new CdlAssociationWithPanoramaEligibility();

                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@authCode", DbType.String, authcode);
                    using (IDataReader dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var item = new CdlAssociationWithPanoramaEligibility()
                            {
                                IsCdl = Convert.ToInt16(dataReader["IsCdl"]) == 1 ? true : false,
                                IsCdlActivated = Convert.ToInt16(dataReader["IsCdlActivated"]) == 1 ? true : false,
                                CdlHasPanoramaAssociation = Convert.ToInt16(dataReader["CdlHasPanoramaAssociation"]) == 1 ? true : false,
                                IsCdlExpired = Convert.ToInt16(dataReader["IsCdlExpired"]) == 1 ? true : false
                            };
                            cdlPanorama = item;
                        }
                    }
                }

                if (cdlPanorama.IsCdl && cdlPanorama.IsCdlActivated 
                                      && !cdlPanorama.CdlHasPanoramaAssociation && !cdlPanorama.IsCdlExpired)
                {
                    hasCDLPanorama = true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return hasCDLPanorama;
            }

            return hasCDLPanorama;
        }

        public string GetLoggingServiceRegion(string authcode)
        {            
            string query = @"SELECT t.TenantRegion FROM license l
                      JOIN device d ON l.DeviceId = d.DeviceId 
                      JOIN tenantlookup t ON d.SerialNumber = t.SerialNumber
                      WHERE l.AuthorizationCode = @authCode
                      AND l.IsActive = 1 AND t.IsActive =1;";
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInParameter(command, "@authCode", DbType.String, authcode);

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return dataReader["TenantRegion"] != DBNull.Value ? HttpUtility.HtmlEncode(dataReader["TenantRegion"].ToString().ToLower()) : "";
                    }
                }
            }
            return "";
        }

        public EActivationActionType IsProductEligibleForActivation(string authcode, out string message)
        {
            try
            {
                message = string.Empty; // backward compatibility
                string query = @"
SELECT CASE WHEN e.HasActivationLink IS NOT NULL THEN 1 ELSE 0 END HasActivationLink
  FROM csp.license l 
  JOIN csp.partmaster p ON l.PartMasterId = p.PartMasterId
  JOIN csp.partsubtype  ps ON p.PartSubtypeId = ps.PartSubtypeId
  JOIN csp.excludedproductactivation e ON (e.partmasterid = p.PartMasterId OR e.pandbidentification = ps.PanDbIdentification)  
  WHERE l.AuthorizationCode = @p_authcode 
  AND l.IsActive = 1
  ORDER BY e.CreatedOn DESC
  LIMIT 1;";
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_authcode", DbType.String, authcode);
                    var dataReader = Connection.ExecuteReader(command);
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["HasActivationLink"]) ? EActivationActionType.HasActivationLink : EActivationActionType.CannotActivate;
                    }

                    return EActivationActionType.CanActivate;
                }

                // CSP-1169 AppPortal activated GPCS: Block re-activation on CSP
                query = @"
SELECT COUNT(1)
    -- GPCS part
    FROM csp.license gl 
    JOIN csp.device gd ON gl.DeviceId = gd.DeviceId
    JOIN csp.partmaster glp ON gl.PartMasterId = glp.PartMasterId
    JOIN csp.tenantlookup gt ON gd.SerialNumber = gt.SerialNumber
    JOIN csp.tenantplatformmapping gm ON gt.TenantId = gm.TenantId
    -- Adelphi part
    JOIN csp.tenantlookup ga ON gm.PlatformId = ga.TenantId
    JOIN csp.appportalapps aa ON ga.AppPortalAppsId = aa.AppPortalAppsId
    -- LGS part
    JOIN csp.tenantplatformmapping lm ON ga.TenantId = lm.TenantId
    JOIN csp.tenantlookup lt ON lm.PlatformId = lt.TenantId AND lt.IsActive = 1
    JOIN csp.device ld ON lt.SerialNumber = ld.SerialNumber AND ld.IsActive = 1
    JOIN csp.license ll ON ld.DeviceId = ll.DeviceId AND ll.IsActive = 1
    JOIN csp.partmaster llp ON ll.PartMasterId = llp.PartMasterId
    WHERE gl.AuthorizationCode = @p_authcode
    AND aa.AppInternalName = 'prisma_access'
    AND glp.PartNumber LIKE '%-GPCS-%'
    AND llp.PartNumber LIKE '%-LGS-%'
    AND gl.IsActive = 1
    AND gd.IsActive = 1
    AND glp.IsActive = 1
    AND gt.IsActive = 1
    AND ga.IsActive = 1
    AND aa.IsActive = 1
    AND ll.IsActive = 1
    AND ld.IsActive = 1
    AND llp.IsActive = 1
    AND lt.IsActive = 1;";
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_authcode", DbType.String, authcode);

                    var result = Connection.ExecuteScalar(command);

                    if ((result != null && Convert.ToInt16(result) > 0))
                    {
                        message = string.Format(SaasAuthCodeMessages.AuthCodeActivatedInAppPortal, authcode);
                        return EActivationActionType.CannotActivate; // Adelphi activated AuthCode cannot be re-/activated in CSP
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return EActivationActionType.Undefined;
            }
        }

        public bool IsSeAuthCode(string authcode)
        {
            try
            {
                const string query =  // 'IsSeAuthCode': 0 means that user can activate, and 1 means user cannot activate product
                    @"SELECT (COUNT(1) > 0) IsSeAuthCode -- 0 - not SE AuthCode, unblock, 1 - SE AuthCode block product (if not SE user)
  FROM license l 
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  JOIN partsubtype s ON p.PartSubtypeId = s.PartSubtypeId
  WHERE s.PanDbIdentification LIKE '%se%aas%'
  AND l.AuthorizationCode = @p_AuthCode;";
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authcode);

                    var isSeAuthCode = Connection.ExecuteScalar(command);

                    return Convert.ToBoolean(isSeAuthCode);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

      

        public List<BundleLicenseItem> GetBundlelicenses(string bundleLicenseIds)
        {
            try
            {
                string query = @" SELECT DISTINCT    
                                     l.AuthorizationCode AS 'AuthCode',
                                     dc.serialnumber,     
                                     COALESCE(ap.AppDisplayName, ps.Name) AS 'ModelName',
                                     lchld.ExpirationDate,
     CASE
        When ap.AppDisplayName = 'TRAPS' THEN lChld.VMUseCount
        else lchld.SaasQuantity
        END AS 'Quantity',   
      lchld.BundleLicenseId,
      s.name 'ASC',
      p.Description
    FROM device d
      JOIN partmaster pd
        ON d.PartMasterId = pd.PartMasterId AND pd.PartNumber IN ( 'PAN-PA-BND-SAAS-SW') 
      JOIN license l ON d.deviceId = l.deviceId AND l.IsActive = 1
      JOIN license lChld ON l.LicenseId=lChld.BundleLicenseId
      JOIN device dc ON dc.DeviceId=lChld.DeviceId
      JOIN partmaster pc ON pc.PartMasterId=lChld.PartMasterId 
      JOIN partsubtype ps ON ps.PartSubtypeId=pc.PartSubtypeId
      JOIN partmaster p ON p.PartMasterId=l.PartMasterId
      LEFT OUTER JOIN supportaccount s ON l.SupportCenterSupportAccountId=s.SupportAccountId
      LEFT JOIN appnamelicensetypemapping a ON a.PanDbIdentification=ps.PanDbIdentification
      LEFT JOIN appportalapps ap ON ap.AppInternalName=a.AppInternalName
    WHERE  l.LicenseId IN ( " +  bundleLicenseIds + ");";

                var bundlelicenses = new List<BundleLicenseItem>();

                using (var command = Connection.GetSqlStringCommand(query))
                {
                   using (IDataReader dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var item = new BundleLicenseItem()
                            {
                                ExpirationDate = Convert.ToString(dataReader["ExpirationDate"]),
                                BundleLicenseId = Convert.ToInt64(dataReader["BundleLicenseId"]),
                                Description = Convert.ToString(dataReader["Description"]),
                                Region = string.Empty,
                                ModelName = Convert.ToString(dataReader["ModelName"]),
                                AuthorizationCode = Convert.ToString(dataReader["AuthCode"]),
                                SerialNumber = Convert.ToString(dataReader["serialnumber"]),
                                Quantity = Convert.ToString(dataReader["Quantity"]),
                                ASC = Convert.ToString(dataReader["ASC"])
                            };

                            bundlelicenses.Add(item);
                           
                        }
                    }
                }

                return bundlelicenses;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new List<BundleLicenseItem>();
             }
        }

        public long GetBundlelicenseIdBySNAuthcode(string serialNumber,string authcode)
        {
            try
            {
          
                using (var command = Connection.GetStoredProcCommand("GetBundlelicenseIdBySNAuthcode"))
                {
                    Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "p_Authcode", DbType.String, authcode);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return Convert.ToInt64(dataReader["LicenseId"]);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return 0;
            }
        }

        public bool VerifyAuthorizationBySerialNumberOrSupportAccountId(string email,string serialNumber,long supportAccountId)
        {
          using (var command = Connection.GetStoredProcCommand("GetMembershipBySerialNumberOrSupportAccountId"))
            {
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_email", DbType.String, email);
              Connection.AddInParameter(command, "@p_serialNumber", DbType.String, serialNumber);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return Convert.ToBoolean(dataReader["HasAccess"]) ;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get all Primary Panoramas for given app and supportaccount
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="serialNumber"></param>
        /// <param name="appPortalApp"></param>
        /// <returns></returns>
        public List<PanoramaSerialNumber> GetPrimaryPanorama(long supportAccountId)
        {
            List<PanoramaSerialNumber> primaryPanoramaList = new List<PanoramaSerialNumber>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPrimaryPanorama"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, supportAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            primaryPanoramaList.Add(new PanoramaSerialNumber
                            {
                                SerialNumber = Convert.ToString(dataReader["PanoramaSerialNumber"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return primaryPanoramaList;
        }

        /// <summary>
        /// Get Panorama(Primary or Secondary) Tenant by App Type
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="serialNumber"></param>
        /// <param name="appPortalApp"></param>
        /// <returns></returns>
        public long GetPanoramaTenantByAppType(long supportAccountId, string serialNumber, AppPortalAppType appPortalApp)
        {
            long tenantId = 0;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPanoramaTenantByAppType"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "p_AppType", DbType.String, appPortalApp.ToDescription());

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            tenantId = Convert.ToInt64(dataReader["TenantId"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return tenantId;
        }

        /// <summary>
        /// Get SaaS OTP by App Type
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="serialNumber"></param>
        /// <param name="otpType"></param>
        /// <returns></returns>
        public OTPOutput GetSaasOtpByType(long supportAccountId, string serialNumber, OtpType otpType)
        {
            OTPOutput otp = null;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetSaaSOTP"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "p_OtpType", DbType.String, otpType.ToDescription());

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            otp = new OTPOutput
                                    {
                                        OneTimePassword = dataReader["OTP"].ToString(),
                                        ExpiryDate = dataReader["UpdatedDate"].ToString()
                                    };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return otp;
        }

        /// <summary>
        /// Generates tenant id for any cloud service
        /// </summary>
        /// <param name="retryLimit"></param>
        /// <param name="isLoggingService"></param>
        /// <returns></returns>
        public long GenerateTenantId(int retryLimit, bool isLoggingService)
        {
            long tenantId = 0;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GenerateTenantId"))
                {
                    Connection.AddInParameter(command, "p_RetryLimit", DbType.Int32, retryLimit);
                    Connection.AddInParameter(command, "p_IsLGS", DbType.Boolean, isLoggingService);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            tenantId = Convert.ToInt64(dataReader["newTenantId"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return tenantId;
        }

        /// <summary>
        /// Creates cloud service tenant for Primary and Secondary Panorama
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="tenantId"></param>
        /// <param name="panoramaSerialNumber"></param>
        /// <param name="appInternalName"></param>
        /// <param name="userAccount"></param>
        public void CreateTenantForPanorama(long supportAccountId, long tenantId, string panoramaSerialNumber, string appInternalName, string userAccount)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("CreateTenantForPanorama"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "p_TenantId", DbType.Int64, tenantId);
                    Connection.AddInParameter(command, "p_PanoramaSerialNumber", DbType.String, panoramaSerialNumber);
                    Connection.AddInParameter(command, "p_AppInternalName", DbType.String, appInternalName);
                    Connection.AddInParameter(command, "p_UserEmail", DbType.String, userAccount);

                    Connection.ExecuteScalar(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType)
        {

            var panoramaSerialNumbers = new List<NameValueOnly>();
            using (var command = Connection.GetStoredProcCommand("GetDevicesForOTP"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInParameter(command, "p_UserAccountId", DbType.Int64, userAccountId);
                Connection.AddInParameter(command, "p_DeviceType", DbType.Int32, (int)deviceType);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var serialnum = new NameValueOnly
                        {
                            Name = Convert.ToString(dataReader["SerialNumber"]),
                            Value = Convert.ToString(dataReader["SerialNumber"])
                        };
                        panoramaSerialNumbers.Add(serialnum);
                    }
                }
            }
            return panoramaSerialNumbers;
        }

        /// <summary>
        /// Search for Panorama SNs with Cloud Services.
        /// Behaviour depends on 'serialNumber' value.
        /// </summary>
        /// <param name="SupportAccountID">Support Acoount ID. Mandatory parameter.</param>
        /// <returns></returns>
        public IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudServiceForTransfer(long supportAccountID, string serialNumber)
        {
            const string query = @"
SELECT DISTINCT d.SerialNumber -- Panoramas 
  FROM device d
--  JOIN secaaspanoramaregistration m ON d.SerialNumber = m.PanoramaSerialNumber AND m.IsActive = 1
  JOIN partmaster pd ON d.PartMasterId = pd.PartMasterId
  JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1 AND l.ExpirationDate >= NOW()
  JOIN partmaster pl ON l.PartMasterId = pl.PartMasterId
  WHERE d.IsActive = 1
  AND d.SupportAccountId = @p_SupportAccountID
  AND pd.PartTypeId = 3 
  AND pd.ModelCompatibility IN ('AVI', 'OSS-AVI', 'SPR-AVI', 'LAB-AVI', 'NFR-AVI', 'AVI500', 'OSS-AVI500',
    'SPR-AVI500', 'LAB-AVI500', 'NFR-AVI500', 'PRA', 'AVI200', 'AVI600',
    'PRA-E', 'LAB-PRA', 'NFR-PRA', 'PRANSX', 'PRA-CP')
  AND pl.PartNumber LIKE '%SVC%' -- with Service contract
  AND pl.PartNumber NOT LIKE '%WRNT%'
  AND d.DeviceId NOT IN -- and without Cloud Service
  (
    SELECT DISTINCT d2.DeviceId
      FROM device d2
      JOIN license l2 ON d2.DeviceId = l2.DeviceId AND l2.IsActive = 1 AND l2.ExpirationDate >= NOW()
      JOIN partmaster pl2 ON l2.PartMasterId = pl2.PartMasterId
      WHERE pl2.PartNumber LIKE '%LGS%' 
      OR pl2.PartNumber LIKE '%FLS%' 
      OR pl2.PartNumber LIKE '%GPCS%' 
      OR pl2.PartNumber LIKE 'PRA-PRISMA-ACCESS%'
  )
  AND d.SupportAccountId IN -- and when Source SN is Source Panorama with Cloud Services to transfer
  (
    SELECT DISTINCT d2.SupportAccountId
      FROM device d2
--      JOIN secaaspanoramaregistration m2 ON d2.SerialNumber = m2.PanoramaSerialNumber AND m2.IsActive = 1
      JOIN partmaster pd2 ON d2.PartMasterId = pd2.PartMasterId
--      JOIN partsubtype sd2 ON pd2.PartSubtypeId = sd2.PartSubtypeId
      JOIN license l2 ON d2.DeviceId = l2.DeviceId AND l2.IsActive = 1 AND l2.ExpirationDate >= NOW()
      JOIN partmaster pl2 ON l2.PartMasterId = pl2.PartMasterId
      WHERE pd2.PartTypeId = 3 
      AND d2.SupportAccountId = @p_SupportAccountID
      AND d2.SerialNumber = @p_serialNumber
      -- AND sd2.PanDbIdentification LIKE '%EVAL%' -- remove EVAL restriction CSP-6066 CSP Cloud License transfer: Enhancements (2)
      AND pd2.ModelCompatibility IN ('AVI', 'OSS-AVI', 'SPR-AVI', 'LAB-AVI', 'NFR-AVI', 'AVI500', 'OSS-AVI500',
        'SPR-AVI500', 'LAB-AVI500', 'NFR-AVI500', 'PRA', 'AVI200', 'AVI600',
        'PRA-E', 'LAB-PRA', 'NFR-PRA', 'PRANSX', 'PRA-CP')
      AND (pl2.PartNumber LIKE '%LGS%' 
      OR pl2.PartNumber LIKE '%FLS%' 
      OR pl2.PartNumber LIKE '%GPCS%' 
      OR pl2.PartNumber LIKE 'PRA-PRISMA-ACCESS%')
  )
  ORDER BY d.SerialNumber
;";
            var eligeblePanoramaSerialNumbers = new List<PanoramaSerialNumber>();
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountID);
                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var serialnum = new PanoramaSerialNumber();
                        serialnum.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        eligeblePanoramaSerialNumbers.Add(serialnum);
                    }
                }
            }

            return eligeblePanoramaSerialNumbers;
        }

        public bool IsCdlAuthCode(string authcode)
        {
            try
            {
                const string query =
                    @"SELECT (COUNT(1) > 0) IsCDLAuthCode FROM license l JOIN 
                     partmaster p ON l.PartMasterId = p.PartMasterId
                     WHERE  p.PartNumber LIKE '%LGS%' AND l.AuthorizationCode = @p_AuthCode;";
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authcode);

                    var isCdlAuthCode = Connection.ExecuteScalar(command);

                    return Convert.ToBoolean(isCdlAuthCode);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

     
        public string GetCDLTenantId(string authCode, long supportAccountID, string region)
        {

            var tenantID = "";
            const string sqlQuery = @"SELECT t.TenantId FROM tenantlookup t 
                                JOIN device d ON t.SerialNumber = d.SerialNumber
                                JOIN license l ON d.DeviceId = l.DeviceId
                  WHERE t.AppPortalAppsId = 4 AND t.IsActive AND t.SupportAccountId =@supportAccountId AND l.AuthorizationCode = @authCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@supportAccountId", DbType.Int64, supportAccountID);
                Connection.AddInParameter(command, "@authCode", DbType.String, authCode);
                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        tenantID = dataReader["TenantId"] != DBNull.Value ? HttpUtility.HtmlEncode(dataReader["TenantId"].ToString()) : "";
                    }
                }

                return tenantID;
            }
        }
    }
}
