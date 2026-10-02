using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Ion;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess
{
    public class IonDataAccess : BaseDataAccess, IIonDataAccess
    {
        public IonResponse GetIons(IonRequest request, bool isDownload = false)
        {
            const string RECORD_COUNT_QUERY = @"
SELECT COUNT(DISTINCT d.DeviceId) Total
";
            const string DEVICE_QUERY = @"
SELECT DISTINCT d.DeviceId
";
            const string HW_QUERY = @"
  FROM device d 
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping t ON ta.AssociatedTenantId = t.TenantId
  JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId AND dp.PartTypeId = 3
  JOIN partsubtype ds ON dp.PartSubtypeId = ds.PartSubtypeId
  LEFT JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1
  LEFT JOIN partmaster lp ON l.PartMasterId = lp.PartMasterId AND lp.ProductFamily = 'CGX'
  LEFT JOIN partsubtype ls ON lp.PartSubtypeId = ls.PartSubtypeId
  WHERE d.SupportAccountId = @p_SupportAccountId
  AND d.IsActive = 1
  AND dp.ProductFamily = 'CGX'
  AND NOT (dp.Partnumber LIKE '%VE%'
  OR dp.Partnumber LIKE '%VION%')
  AND dp.Parttypeid = 3
  AND (l.LicenseId IS NULL 
  OR l.LicenseId IS NOT NULL
  AND l.ExpirationDate >= NOW())
";
            const string VM_QUERY = @"
  FROM device d
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping t ON ta.AssociatedTenantId = t.TenantId
  JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId
  JOIN partsubtype ds ON dp.PartSubtypeId = ds.PartSubtypeId
  JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1
  JOIN partmaster lp ON l.PartMasterId = lp.PartMasterId
  JOIN partsubtype ls ON lp.PartSubtypeId = ls.PartSubtypeId
  WHERE d.SupportAccountId = @p_SupportAccountId
  AND d.IsActive = 1
  AND dp.ProductFamily = 'CGX'
  AND (dp.Partnumber LIKE '%VE%'
  OR dp.Partnumber LIKE '%VION%')
  AND dp.Parttypeid = 3
";
            const string HW_LICENSE_QUERY = @"
SELECT DISTINCT d.DeviceId, d.SerialNumber, -1 LicenseId, '' AuthorizationCode, dp.PartNumber, dp.Description, NULL ExpirationDate, 0 SaasQuantity, 1 IsParent, cgm.CgTenantId TenantId
  FROM device d
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping cgm ON ta.AssociatedTenantId = cgm.TenantId
  JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId
  WHERE d.DeviceId IN ({0}) {1}
UNION
SELECT DISTINCT d.DeviceId, d.SerialNumber, l.LicenseId, l.AuthorizationCode, lp.PartNumber, lp.Description, l.ExpirationDate, l.SaasQuantity, 0 IsParent, cgm.CgTenantId TenantId
  FROM device d
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping cgm ON ta.AssociatedTenantId = cgm.TenantId
  JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1
  JOIN partmaster lp ON l.PartMasterId = lp.PartMasterId 
  WHERE d.DeviceId IN ({0}) {1}
";
            const string VM_LICENSE_QUERY = @"
SELECT DISTINCT d.DeviceId, d.SerialNumber, l.LicenseId, l.AuthorizationCode, lp.PartNumber, lp.Description, CASE WHEN ls.PartTypeId <> 3 THEN l.ExpirationDate ELSE NULL END ExpirationDate, l.SaasQuantity, ls.PartTypeId = 3 IsParent, t.CgTenantId TenantId
  FROM device d
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping t ON ta.AssociatedTenantId = t.TenantId
  JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId
  JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1
  JOIN partmaster lp ON l.PartMasterId = lp.PartMasterId 
  JOIN partsubtype ls ON lp.PartSubtypeId = ls.PartSubtypeId
  WHERE d.DeviceId IN ({0})
  {1}
";
            bool hasFilter = !string.IsNullOrEmpty(request.SearchField) && !string.IsNullOrEmpty(request.SearchValue) && SearchHelper.ValidateSearchField(request.SearchField);
            string whereClause = string.Empty;
            string sortClause = string.Empty;
            string limitClause = string.Empty;
            if (hasFilter
                && !isDownload)
            {
                whereClause = string.Format("\r\n  AND {0}", request.SearchField == "modelName"
                    ? "(dp.PartNumber = @p_SearchValue OR lp.PartNumber = @p_SearchValue)"
                    : string.Format("{0} = @p_SearchValue", ReplaceFieldName(request.SearchField)));
            }

            if (!string.IsNullOrEmpty(request.SortField) && (SortHelper.ValidateSorting(request.SortField, request.SortOrder)))
            {
                sortClause = string.Format("\r\n  ORDER BY {0} {1}", ReplaceFieldName(request.SortField),
                    !string.IsNullOrEmpty(request.SortOrder) ? request.SortOrder : string.Empty);
            }

            if (request.PerPage > 0)
            {
                limitClause = string.Format("\r\n  LIMIT {0}, {1}", (request.Page - 1) * request.PerPage, request.PerPage);
            }

            try
            {
                string sqlQuery = string.Format(@"{0}{2}{3}{4};
SELECT GROUP_CONCAT(DISTINCT a.DeviceId) DeviceIds
  FROM
  (
    {1}{2}{3}{4}{5}
  ) a", RECORD_COUNT_QUERY, DEVICE_QUERY,
                    (request.DeviceType == EDeviceType.Physical ? HW_QUERY : VM_QUERY),
                    whereClause, sortClause, limitClause);
                int total = 0;
                string deviceIds = string.Empty;
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.UInt64, request.SupportAccountId);
                    if (hasFilter && !isDownload)
                    {
                        Connection.AddInParameter(command, "@p_SearchValue", DbType.String, request.SearchValue);
                    }

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            total = dataReader.GetColumnValue<int>("Total");
                        }

                        if (dataReader.NextResult()
                            && dataReader.Read())
                        {
                            deviceIds = dataReader.GetColumnValue<string>("DeviceIds");
                        }
                    }
                }

                // Get licenses
                List<IonItem> items = new List<IonItem>();
                if (!string.IsNullOrWhiteSpace(deviceIds))
                {
                    sqlQuery = string.Format("{0}{1}{2};",
                        string.Format(request.DeviceType == EDeviceType.Physical ? HW_LICENSE_QUERY : VM_LICENSE_QUERY, deviceIds,
                            // to show VM only, filter by current tenant
                            string.Empty),
                        request.DeviceType != EDeviceType.AddOns && (request.SearchField == "authCode" || request.SearchField == "modelName") ? string.Empty : whereClause,
                        sortClause);
                    using (var command = Connection.GetSqlStringCommand(sqlQuery))
                    {
                        if (hasFilter && !isDownload)
                        {
                            Connection.AddInParameter(command, "@p_SearchValue", DbType.String, request.SearchValue);
                        }
                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            while (dataReader.Read())
                            {
                                items.Add(new IonItem()
                                {
                                    DeviceId = dataReader.GetColumnValue<string>("DeviceId"),
                                    SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                    LicenseId = dataReader.GetColumnValue<string>("LicenseId"),
                                    AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                    ModelName = dataReader.GetColumnValue<string>("PartNumber"),
                                    Description = dataReader.GetColumnValue<string>("Description"),
                                    Expiration = dataReader.GetColumnValue<DateTime?>("ExpirationDate"),
                                    Quantity = dataReader.GetColumnValue<int>("SaasQuantity"),
                                    IsParent = dataReader.GetColumnValue<bool>("IsParent"),
                                    TenantId = dataReader.GetColumnValue<string>("TenantId")
                                });
                            }
                        }
                    }
                }

                return new IonResponse
                {
                    Items = items,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public IonResponse GetAddOnsIons(IonRequest request, bool isDownload = false)
        {
            string whereClause = string.Empty;
            string sortClause = string.Empty;
            string limitClause = string.Empty;
            const string TENANTS_QUERY = @"
SELECT DISTINCT t.CgTenantId
";
            const string RECORD_COUNT_QUERY = @"
SELECT COUNT(d.DeviceId) Total
";
            const string ADDONS_QUERY = @"
SELECT d.DeviceId, d.SerialNumber, l.LicenseId, l.AuthorizationCode, lp.PartNumber, lp.Description, l.ExpirationDate, SUM(l.SaasQuantity) SaasQuantity, t.CgTenantId TenantId
";
            string ADDONS_LICENSE_QUERY = @"
  FROM device d
  LEFT JOIN tenantlookup ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping t ON ta.TenantId = t.TenantId
  LEFT JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId
  LEFT JOIN partsubtype ds ON dp.PartSubtypeId = ds.PartSubtypeId
  JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive = 1
  JOIN partmaster lp ON l.PartMasterId = lp.PartMasterId
  WHERE d.SupportAccountId = @p_SupportAccountId
  -- {0}
  AND d.IsActive = 1
  AND lp.ProductFamily = 'CGX'
  AND ds.PanDbIdentification = 'STUB'
  AND lp.Partnumber NOT LIKE '%-DL%'
  GROUP BY d.Deviceid, lp.Partmasterid";
            bool hasFilter = !string.IsNullOrEmpty(request.SearchField) && !string.IsNullOrEmpty(request.SearchValue) && SearchHelper.ValidateSearchField(request.SearchField);
            if (hasFilter
                && !isDownload)
            {
                whereClause = string.Format("  AND {0} = @p_SearchValue", ReplaceFieldName(request.SearchField));
            }

            if (!string.IsNullOrEmpty(request.SortField) && (SortHelper.ValidateSorting(request.SortField, request.SortOrder)))
            {
                sortClause = string.Format("\r\n  ORDER BY {0} {1}", ReplaceFieldName(request.SortField), !string.IsNullOrEmpty(request.SortOrder) ? request.SortOrder : string.Empty);
            }

            if (request.PerPage > 0)
            {
                limitClause = string.Format("\r\n  LIMIT {0}, {1}", (request.Page - 1) * request.PerPage, request.PerPage);
            }

            try
            {
                List<string> tenants = new List<string>();
                if (!isDownload)
                {
                    // 1. Get all relevant tenants; filter is applied regardless of tenant.
                    // The end-point first time request tenant is always NULL. Search for all tenants to assign first one when currentTenant isn NULL. 
                    string tenantsQuery = string.Format("{0}{1}{2};", TENANTS_QUERY, ADDONS_LICENSE_QUERY.Replace("{0}", whereClause), sortClause);
                    using (var command = Connection.GetSqlStringCommand(tenantsQuery)) // get tenants
                    {
                        Connection.AddInParameter(command, "@p_SupportAccountId", DbType.UInt64, request.SupportAccountId);
                        if (hasFilter && !isDownload)
                        {
                            Connection.AddInParameter(command, "@p_SearchValue", DbType.String, request.SearchValue);
                        }
                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            while (dataReader.Read())
                            {
                                tenants.Add(dataReader.GetColumnValue<string>("CgTenantId"));
                            }
                        }
                    }
                } // no need for Download to identify current tenant.

                string currentTenant = !string.IsNullOrWhiteSpace(request.TenantId)
                    ? tenants.SingleOrDefault(t => t == request.TenantId) ?? tenants.FirstOrDefault()
                    : tenants.FirstOrDefault();
                // 2. Query w/out tenant filter to make sure searched value is searched in all tenants.
                List<IonItem> items = new List<IonItem>();
                int total = 0;
                int count = 0;
                string sqlQuery = string.Format("{0}{2}{3};\r\n{1}{2}{3}{4}{5};", RECORD_COUNT_QUERY, ADDONS_QUERY,
                    ADDONS_LICENSE_QUERY.Replace("{0}", isDownload ? string.Empty : "AND t.CgTenantId = @p_CgTenantId"),
                    whereClause, sortClause, limitClause);
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.UInt64, request.SupportAccountId);
                    if (!isDownload) // When is not download, search in current tenant only
                    {
                        Connection.AddInParameter(command, "@p_CgTenantId", DbType.UInt64, currentTenant);
                    }
                    if (hasFilter && !isDownload)
                    {
                        Connection.AddInParameter(command, "@p_SearchValue", DbType.String, request.SearchValue);
                    }
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            total = dataReader.GetColumnValue<int>("Total");
                        }

                        dataReader.NextResult();
                        while (dataReader.Read())
                        {
                            items.Add(new IonItem()
                            {
                                DeviceId = dataReader.GetColumnValue<string>("DeviceId") + "_" + count++,
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                LicenseId = dataReader.GetColumnValue<string>("LicenseId"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                ModelName = dataReader.GetColumnValue<string>("PartNumber"),
                                Description = dataReader.GetColumnValue<string>("Description"),
                                Expiration = dataReader.GetColumnValue<DateTime?>("ExpirationDate"),
                                Quantity = dataReader.GetColumnValue<int>("SaasQuantity"),
                                TenantId = dataReader.GetColumnValue<string>("TenantId")
                            });
                        }
                    }
                }

                return new IonResponse
                {
                    Tenants = tenants,
                    CurrentTenant = currentTenant,
                    Items = items,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public DeviceIonTransferDetails GetIonDeviceTransferDetails(Device device)
        {
            string query = @"
SELECT DISTINCT d.DeviceId, d.SerialNumber
  FROM device d
  JOIN partmaster dp ON d.PartMasterId = dp.PartMasterId
  JOIN partsubtype ds ON dp.PartSubtypeId = ds.PartSubtypeId
  LEFT JOIN tenantassociation ta ON d.SerialNumber = ta.SerialNumber
  LEFT JOIN cloudgenixmapping t ON ta.AssociatedTenantId = t.TenantId
  LEFT JOIN license l ON d.DeviceId = l.DeviceId AND l.IsActive
  WHERE d.SupportAccountId = @p_SupportAccountId
  AND d.IsActive = 1
  AND dp.PartTypeId = 3
  AND dp.ProductFamily = 'CGX'
  AND l.LicenseId IS NULL -- has no licenses attached
;";
            try
            {
                List<dynamic> items = new List<dynamic>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.UInt64, device?.Customer?.Id ?? 0);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            items.Add(new
                            {
                                DeviceId = dataReader.GetColumnValue<long>("DeviceId"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber")
                            });
                        }
                    }
                }

                return new DeviceIonTransferDetails
                {
                    DeviceId = device.Id,
                    SerialNumber = device.SerialNumber,
                    Model = device.ModelName,
                    TenantId = device.TenantId,
                    TransferEligibleDevices = items
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public void SaveIonDeviceLightDetails(DeviceIonDetailsLightInfo details, long userId)
        {
            string query = @"
UPDATE device d
  SET d.DeviceName = @DeviceName,
  d.DeviceCategoryId = @TagId,
  d.PanOsVersion = @PanOsVersion,
  d.Address1 = @Address1,
  d.Address2 = @Address2,
  d.CountryId = @CountryId,
  d.Region = @Region,
  d.PostalCode = @PostalCode,
  d.City = @City,
  d.Comments = @Comments
  WHERE d.DeviceId = @DeviceId
;";
            try
            {
                List<dynamic> items = new List<dynamic>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@DeviceId", DbType.UInt64, details.DeviceId);
                    Connection.AddInParameter(command, "@DeviceName", DbType.String, details.DeviceName);
                    Connection.AddInParameter(command, "@TagId", DbType.UInt64, details.TagId);
                    Connection.AddInParameter(command, "@PanOsVersion", DbType.String, details.PanOsVersion);
                    Connection.AddInParameter(command, "@Address1", DbType.String, details.Address1);
                    Connection.AddInParameter(command, "@Address2", DbType.String, details.Address2);
                    Connection.AddInParameter(command, "@City", DbType.String, details.City);
                    Connection.AddInParameter(command, "@CountryId", DbType.UInt64, details.CountryId);
                    Connection.AddInParameter(command, "@Region", DbType.String, details.Region);
                    Connection.AddInParameter(command, "@PostalCode", DbType.String, details.PostalCode);
                    Connection.AddInParameter(command, "@Comments", DbType.String, details.Comments);
                    if (Connection.ExecuteNonQuery(command) == 0)
                    {
                        Exception ex = new Exception(string.Format("ION Device SN {0} failed to update. Details: {1}",
                            details.SerialNumber, JsonConvert.SerializeObject(details)));
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public void TransferIonDeviceLicenses(DeviceIonTransferLicenses transfer, long userId)
        {
            var url = ConfigurationManager.AppSettings["LicenseApi"] + "/coresvc/v1/license/transfercgxlicense";
            var thumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
            var payload = JsonConvert.SerializeObject(new { source = transfer.SourceSn, destination = transfer.DestinationSn });
            using (var httpResponse = (HttpWebResponse)WebCommon.GetWebRequestWithClientCertificate(url, "POST", thumbprint, payload).GetResponse())
            {
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = httpResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            dynamic returnData = JsonConvert.DeserializeObject<dynamic>(result, new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            });
                            if ((bool?)returnData.isTransferred != true)
                            {
                                Exception ex = new Exception(string.Format("Error TransferIonLicenses. From SN: '{0}', To SN: '{1}', Response object: {2}",
                                    transfer.SourceSn, transfer.DestinationSn, result));
                                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                                throw ex;
                            }
                        }
                    }
                }
                else
                {
                    Exception ex = new Exception(string.Format("Error TransferIonLicenses. From SN: '{0}', To SN: '{1}', Response: {2}",
                        transfer.SourceSn, transfer.DestinationSn, httpResponse.StatusCode));
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw ex;
                }
            }
        }

        public bool IsOnPremController(long supportAccountId)
        {
            string sqlQuery = @"
SELECT COUNT(1) > 0 IsOnPremController
  FROM lookups l
  WHERE l.LookupType = 'SW_UPDATE_ON_PREM_CONTROLLER'
  AND l.LookupKey = 'CSP_ID'
  AND l.LookupValue = @supportAccountId;";

            return GetBoolanFlagFromDb(sqlQuery, "IsOnPremController", (n, c) =>
            {
                n.AddInParameter(c, "@supportAccountId", DbType.UInt64, supportAccountId);
            });
        }

        private string ReplaceFieldName(string fieldName)
        {
            return fieldName.ToLower().Replace("serialnumber", "d.SerialNumber").Replace("authcode", "l.AuthorizationCode")
                .Replace("modelname", "lp.PartNumber").Replace("description", "lp.Description").Replace("expiration", "l.ExpirationDate");
        }
    }
}
