using Csp.Api.Model.LicenseManagement;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using Csp.Api.Model.Enums;

namespace Csp.Api.DataAccess
{
    public class LicenseManagementDataAccess : BaseDataAccess, ILicenseManagementDataAccess
    {
        public IEnumerable<LicenseManagementActivatedListItem> GetActivatedLicenses(long supportAccountId)
        {
            List<LicenseManagementActivatedListItem> result = new List<LicenseManagementActivatedListItem>();
            const string sqlQuery = @"
            SELECT l.AuthorizationCode, d.SerialNumber, p1.PartNumber, p.PartNumber 'SKU', d.DeviceName, ascs.Name 'ASC', l.ExpirationDate, o.product ProductGroup
              FROM supportaccount s              
              JOIN license l ON s.SupportAccountId = l.SupportAccountId
              JOIN partmaster p ON l.PartMasterId = p.PartMasterId              
              JOIN device d ON l.DeviceId = d.DeviceId
              JOIN partmaster p1 ON d.PartMasterId = p1.PartMasterId
              JOIN partsubtype pst on p.PartSubtypeId = pst.PartSubtypeId
              LEFT JOIN license ascl ON d.DeviceId = ascl.DeviceId
              AND ascl.IsActive = 1 AND ascl.SupportCenterSupportAccountId IS NOT NULL
              AND ascl.PartMasterId NOT IN (SELECT PartMasterId FROM csspsku)
              LEFT JOIN supportaccount ascs ON ascl.SupportCenterSupportAccountId = ascs.SupportAccountId
              LEFT JOIN easyonboardingitem i ON l.licenseid = i.LicenseId
              LEFT JOIN easyonboarding o ON i.easyonboardingid = o.easyonboardingid
              WHERE s.SupportAccountId = @p_supportAccountId
              AND pst.PanDbIdentification like '%IOT%'
              AND l.ExpirationDate IS NOT NULL
              AND l.ExpirationDate >= NOW()
              AND l.IsActive;";
            using (var command = Connection.GetStoredProcCommand(sqlQuery))
            {
                command.CommandType = CommandType.Text;
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    int i = 1;
                    while (dataReader.Read())
                    {
                        try
                        {
                            result.Add(new LicenseManagementActivatedListItem
                            {
                                UniqueId = i++,
                                AuthCode = dataReader["AuthorizationCode"].ToString(),
                                SerialNumber = dataReader["SerialNumber"].ToString(),
                                ModelNumber = dataReader["PartNumber"].ToString(),
                                SKU = dataReader["SKU"].ToString(),
                                DeviceName = dataReader["DeviceName"].ToString(),
                                ASC = dataReader["ASC"].ToString(),
                                ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? (Convert.ToDateTime(dataReader["ExpirationDate"])).ToString("MM/dd/yyyy") : "",
                                ProductGroup = dataReader["ProductGroup"] != DBNull.Value ? dataReader["ProductGroup"].ToString() : string.Empty
                            });
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                }
            }
            return result;
        }

        public LicenseManagementAvailableData GetAvailableLicenses(long supportAccountId)
        {
            LicenseManagementAvailableData result = new LicenseManagementAvailableData();
            List<LicenseManagementAvailableListItem> availableList = new List<LicenseManagementAvailableListItem>();
            const string sqlQuery = @"
            SELECT DISTINCT i.licenseid, l.AuthorizationCode, p.Description, p.PartNumber, l.LicenseTermMonths, l.GracePeriodDays, l.RegistrationDate, l.ShipDate,
                CASE
                    WHEN p.PartNumber like '%-E%' THEN 'Eval'
                    WHEN p.PartNumber like '%TRIAL%' THEN 'Trial'
                    ELSE 'Prod'
                END AS 'LicenseType', o.product ProductGroup, o.license_hash Hash
              FROM supportaccount s
              JOIN assetactivation a ON s.SalesforceAccountId = a.salesforceaccountid
              JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
              JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
              JOIN license l ON i.licenseid = l.LicenseId
              JOIN partmaster p ON l.PartMasterId = p.PartMasterId              
              WHERE s.SupportAccountId = @p_supportAccountId 
              AND o.product = 'IOT'
              AND i.subgroup = 'IOT'
              AND a.createdon IS NOT NULL  
              AND l.IsActive = 1
              AND l.LicenseId NOT IN 
              (
                -- exclude activated licenses
                SELECT DISTINCT l2.LicenseId
                  FROM supportaccount s2
                  JOIN supportaccount f2 ON s2.SalesforceAccountId = f2.SalesforceAccountId
                  JOIN license l2 ON f2.SupportAccountId = l2.SupportAccountId
                  JOIN partmaster p2 ON l2.PartMasterId = p2.PartMasterId
                  JOIN partsubtype pst2 on p2.PartSubtypeId = pst2.PartSubtypeId
                  WHERE s2.SupportAccountId = @p_supportAccountId      
                  AND pst2.PanDbIdentification like '%IOT%'
                  AND l2.ExpirationDate IS NOT NULL
                  AND l2.ExpirationDate >= NOW()
                  AND l2.IsActive
              )
              ORDER BY l.CreatedOn DESC;";
            using (var command = Connection.GetStoredProcCommand(sqlQuery))
            {
                command.CommandType = CommandType.Text;
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    int i = 1;
                    while (dataReader.Read())
                    {
                        try
                        {
                            availableList.Add(new LicenseManagementAvailableListItem
                            {
                                UniqueId = i++,
                                AuthCode = dataReader["AuthorizationCode"].ToString(),
                                Description = dataReader["Description"].ToString(),
                                SKU = dataReader["PartNumber"].ToString(),
                                LicenseTermMonths = Convert.ToInt32(dataReader["LicenseTermMonths"]),
                                GracePeriodDays = Convert.ToInt32(dataReader["GracePeriodDays"]),
                                ShipDate = dataReader["ShipDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ShipDate"]) : (DateTime?)null,
                                RegistrationDate = dataReader["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["RegistrationDate"]) : (DateTime?)null,                                
                                Status = "Last Activated: " + (dataReader["RegistrationDate"] != DBNull.Value ? (Convert.ToDateTime(dataReader["RegistrationDate"])).ToString("MM/dd/yyyy") : "N/A"),
                                LicenseType = Convert.ToInt32((LicenseManagementLicenseType)Enum.Parse(typeof(LicenseManagementLicenseType), dataReader["LicenseType"].ToString())),
                                ProductGroup = dataReader["ProductGroup"] == DBNull.Value ? string.Empty : dataReader["ProductGroup"].ToString(),
                                Hash = dataReader["Hash"] == DBNull.Value ? string.Empty : dataReader["Hash"].ToString()
                            });
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                }
            }

            result.AvailableItems = availableList;
            return result;
        }
    }
}