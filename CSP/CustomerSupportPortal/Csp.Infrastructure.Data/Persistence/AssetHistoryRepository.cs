using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Infrastructure.Data.Persistence
{
    public class AssetHistoryRepository : IAssetHistoryRepository
    {
        private readonly IConnectionStrings _connectionStrings;
        private readonly IApplicationSettings _applicationSettings;
        private readonly long _devPartTypeId;// = 3;
        private readonly long _vmPartMasterId;// = 3786;
        private readonly long _ossPartSubTypeId;// = 39;
        private readonly long _sparePartSubTypeId;// = 92;

        public AssetHistoryRepository(IConnectionStrings connectionStrings, IApplicationSettings applicationSettings)
        {
            _connectionStrings = connectionStrings;
            _applicationSettings = applicationSettings;
            var db = SessionDbContext.CspObjectContext;

            var devPartTypeIdQuery = (from pt in db.parttypes where pt.PanDbIdentification == SearchAllConts.DevPandDbId select pt.PartTypeId);
            var vmPartMasterIdQuery = (from pm in db.partmasters where pm.PartNumber == SearchAllConts.VmPartNumber select pm.PartMasterId);
            var ossPartSubTypeIdQuery = (from pst in db.partsubtypes where pst.PanDbIdentification == SearchAllConts.OssPanDbId select pst.PartSubtypeId);
            var sparePartSubTypeIdQuery = (from pst in db.partsubtypes where pst.PanDbIdentification == SearchAllConts.SparePanDbId select pst.PartSubtypeId);


            var ids = (
                from devPartTypeId in devPartTypeIdQuery
                from vmPartMasterId in vmPartMasterIdQuery
                from ossPartSubTypeId in ossPartSubTypeIdQuery
                from sparePartSubTypeId in sparePartSubTypeIdQuery
                select new
                {
                    devPartTypeId,
                    vmPartMasterId,
                    ossPartSubTypeId,
                    sparePartSubTypeId,
                }).Single();

            _devPartTypeId = ids.devPartTypeId;
            _vmPartMasterId = ids.vmPartMasterId;
            _ossPartSubTypeId = ids.ossPartSubTypeId;
            _sparePartSubTypeId = ids.sparePartSubTypeId;
		}

        public List<AssetHistoryListItem> GetAssetHistory(string code, DateTime? startDate, DateTime? endDate, long defaultSupportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool isExportReport)
        {
            // Comments By LK On 01/04/2017
            // EF MySQL does not support OUT or INOUT
            /*
            var db = SessionDbContext.CspObjectContext;

            var outParam = new MySqlParameter();
            outParam.ParameterName = "p_TotalRecordCount";
            outParam.MySqlDbType = MySqlDbType.Int64;
            outParam.Direction = ParameterDirection.Output;

            var data = 
                    db.Database.SqlQuery<AssetHistoryListItem>(
                        "CALL GetAssetHistory(@p_SupportAccountId, @p_Code, @p_StartDate, @p_EndDate, @p_PaginationOffset, @p_PaginationCount, @p_SortField, @p_SortDirection, @p_RestrictedByUserAccountId, @p_TotalRecordCount);",
                        new MySqlParameter("p_SupportAccountId", supportAccountId), 
                        new MySqlParameter("p_Code", code),
                        new MySqlParameter("p_StartDate", null),
                        new MySqlParameter("p_EndDate", null),
                        new MySqlParameter("p_PaginationOffset", parameters.PaginationOffset),
                        new MySqlParameter("p_PaginationCount", parameters.PaginationCount),
                        new MySqlParameter("p_SortField", parameters.SortField),
                        new MySqlParameter("p_SortDirection", parameters.SortDirection),
                        new MySqlParameter("p_RestrictedByUserAccountId", restrictedByUserAccountId),
                        outParam);

            return data.ToList();*/

            var assetHistoryList = new List<AssetHistoryListItem>();
            var eaSiteLicense = _applicationSettings.EaSiteLicense;
            var eaSiteLicenseTypes = eaSiteLicense.Split(',').ToList();
            if (parameters == null)
            {
                parameters = new DataParameters();
            }
            else
            {
                if (!SortHelper.ValidateSorting(parameters.SortField, parameters.SortDirection.ToSqlOrderBy()))
                {
                    parameters.SortField = string.Empty;
                }
            }
            //Change the sort parameter to AssetType if it is AssetDisplayName
            var isDisplayNameSorting = !string.IsNullOrEmpty(parameters.SortField) && parameters.SortField.Equals("AssetTypeDisplayName");
            parameters.SortField = isDisplayNameSorting ? "AssetType" : parameters.SortField;

            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                var outParam = new MySqlParameter();
                outParam.ParameterName = "p_TotalRecordCount";
                outParam.MySqlDbType = MySqlDbType.Int64;
                outParam.Direction = ParameterDirection.Output;

                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetAssetHistory";
                sqlCmd.CommandTimeout = 120;
                sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int32).Value = defaultSupportAccountId;
                sqlCmd.Parameters.Add("p_Code", MySqlDbType.VarChar, 45).Value = code;

                if (startDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = startDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = null; ;
                }

                if (endDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = endDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = null;
                }

                sqlCmd.Parameters.Add("p_PaginationOffset", MySqlDbType.Int32).Value = (!isExportReport) ? parameters.PaginationOffset : 0;
                sqlCmd.Parameters.Add("p_PaginationCount", MySqlDbType.Int16).Value = (!isExportReport) ? parameters.PaginationCount : Int32.MaxValue;
                sqlCmd.Parameters.Add("p_SortField", MySqlDbType.VarChar, 65).Value = (!isExportReport) ? parameters.SortField : string.Empty;
                sqlCmd.Parameters.Add("p_SortDirection", MySqlDbType.VarChar, 20).Value = (!isExportReport) ? SortDirectionExtension.ToSqlOrderBy(parameters.SortDirection) : string.Empty;
                sqlCmd.Parameters.Add("p_RestrictedByUserAccountId", MySqlDbType.Int32).Value = restrictedByUserAccountId;
                sqlCmd.Parameters.Add(outParam);

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                var dtAssetHistory = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtAssetHistory);

                if (dtAssetHistory.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtAssetHistory.Rows)
                    {
                        long id = 0;
                        long supportAccountId = 0;
                        long partMasterId = 0;
                        long partSubtypeId = 0;
                        long registeredByUserId = 0;
                        long registeredAccountId = 0;
                        DateTime dateRegistered = DateTime.MinValue;
                        DateTime dateShipped = DateTime.MinValue;
                        DateTime datePurchased = DateTime.MinValue;
                        var assetType = dr["AssetType"].ToString();
                        var assetTypeDisplayName = string.Empty;
                        var modelCompatibility = dr["ModelCompatibility"].ToString();
                        var panDbIdentification = dr["PanDbIdentification"].ToString();

                        long.TryParse(dr["Id"].ToString(), out id);
                        long.TryParse(dr["SupportAccountId"].ToString(), out supportAccountId);
                        long.TryParse(dr["PartMasterId"].ToString(), out partMasterId);
                        long.TryParse(dr["PartSubtypeId"].ToString(), out partSubtypeId);
                        long.TryParse(dr["RegisteredByUserId"].ToString(), out registeredByUserId);
                        long.TryParse(dr["RegisteredAccountId"].ToString(), out registeredAccountId);

                        if (assetType == AssetTypeEnum.Device && modelCompatibility.Contains(EndpointConstants.ModelCompat))
                        {
                            assetTypeDisplayName = AssetTypeEnum.AdvancedEndpointProtection;
                        }
                        else if (assetType == AssetTypeEnum.Device && (partSubtypeId == _ossPartSubTypeId || partSubtypeId == _sparePartSubTypeId)) {
                            assetTypeDisplayName = AssetTypeEnum.Spare;
                        }
                        else if (assetType == AssetTypeEnum.License && eaSiteLicenseTypes.Contains(panDbIdentification))
                        {
                            assetTypeDisplayName = AssetTypeEnum.EnterpriseAgreement;
                        }
                        else if (assetType == AssetTypeEnum.License && panDbIdentification.Contains("SITE"))
                        {
                            assetTypeDisplayName = AssetTypeEnum.SiteLicense;
                        }
                        else
                        {
                            assetTypeDisplayName = assetType;
                        }

                        var item = new AssetHistoryListItem
                        {
                            Id = id,
                            AssetType = assetType,
                            AssetTypeDisplayName = assetTypeDisplayName,
                            Name = dr["Name"].ToString(),
                            Code = dr["Code"].ToString(),
                            SupportAccountId = supportAccountId,
                            SupportAccountName = dr["SupportAccountName"].ToString(),
                            PartMasterId = partMasterId,
                            PartSubtypeId = partSubtypeId,
                            ModelCompatibility = modelCompatibility,
                            PanDbIdentification = panDbIdentification,
                            RegisteredByUserId = registeredByUserId,
                            RegisteredByEmail = dr["RegisteredByEmail"].ToString(),
                            RegisteredByName = dr["RegisteredByName"].ToString(),
                            RegisteredAccountId = registeredAccountId,
                            RegisteredAccountName = dr["RegisteredAccountName"].ToString()
                        };

                        if (dr["DateRegistered"] != null)
                        {
                            DateTime.TryParse(dr["DateRegistered"].ToString(), out dateRegistered);

                            if (dateRegistered != DateTime.MinValue)
                            {
                                item.DateRegistered = dateRegistered;
                            }
                        }

                        if (dr["DateShipped"] != null)
                        {
                            DateTime.TryParse(dr["DateShipped"].ToString(), out dateShipped);

                            if (dateShipped != DateTime.MinValue)
                            {
                                item.DateShipped = dateShipped;
                            }
                        }

                        if (dr["DatePurchased"] != null)
                        {
                            DateTime.TryParse(dr["DatePurchased"].ToString(), out datePurchased);

                            if (datePurchased != DateTime.MinValue)
                            {
                                item.DatePurchased = datePurchased;
                            }
                        }

                        assetHistoryList.Add(item);
                    }
                }

                if (!isExportReport && sqlCmd.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                {
                    parameters.TotalRecordCount = (long)(sqlCmd.Parameters["p_TotalRecordCount"].Value);
                }

                if (!isExportReport && isDisplayNameSorting)
                {
                    assetHistoryList = assetHistoryList.OrderBy(a => a.AssetTypeDisplayName).ToList();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("GetAssetHistory: --> {0} {1}", defaultSupportAccountId, ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return assetHistoryList;
        }

        public List<AssetHistoryDeviceRegDetails> GetDeviceRegDetailsForAssetHistory(long[] deviceIds, long supportAccountId, long restrictedByUserAccountId, string code, DateTime? startDate, DateTime? endDate)
        {
            var deviceDetailsList = new List<AssetHistoryDeviceRegDetails>();
            var csvDeviceIds = (deviceIds != null && deviceIds.Length > 0) ? string.Join(",", deviceIds) : string.Empty;

            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetDeviceRegDetailsForAssetHistory";
                sqlCmd.CommandTimeout = 120;
                sqlCmd.Parameters.Add("p_DeviceIds", MySqlDbType.LongText).Value = csvDeviceIds;
                sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int32).Value = supportAccountId;
                sqlCmd.Parameters.Add("p_RestrictedByUserAccountId", MySqlDbType.Int32).Value = restrictedByUserAccountId;
                sqlCmd.Parameters.Add("p_Code", MySqlDbType.VarChar, 45).Value = code;

                if (startDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = startDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = null; ;
                }

                if (endDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = endDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = null;
                }

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                var dtDeviceDetails = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtDeviceDetails);

                if (dtDeviceDetails.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtDeviceDetails.Rows)
                    {
                        long registeredByUserId = 0;
                        DateTime lastUpdateDate = DateTime.MinValue;

                        long.TryParse(dr["RegisteredByUserId"].ToString(), out registeredByUserId);

                        var item = new AssetHistoryDeviceRegDetails
                        {
                            SerialNumber = dr["SerialNumber"].ToString(),
                            RegisteredByUserId = registeredByUserId,
                            RegisteredByEmail = dr["RegisteredByEmail"].ToString(),
                            RegisteredByName = dr["RegisteredByName"].ToString(),
                        };

                        if (dr["LastUpdateDate"] != null)
                        {
                            DateTime.TryParse(dr["LastUpdateDate"].ToString(), out lastUpdateDate);

                            if (lastUpdateDate != DateTime.MinValue)
                            {
                                item.LastUpdateDate = lastUpdateDate;
                            }
                        }

                        deviceDetailsList.Add(item);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("GetDeviceRegDetailsForAssetHistory: --> {0} {1}", csvDeviceIds, ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return deviceDetailsList;
        }

        public List<AssetHistorySpareLicenseTransferDetails> GetSpareLicenseTransferDetailsForAssetHistory(long[] deviceIds, long supportAccountId, long restrictedByUserAccountId, string code, DateTime? startDate, DateTime? endDate)
        {
            var spareTransferDetailsList = new List<AssetHistorySpareLicenseTransferDetails>();
            var csvDeviceIds = (deviceIds != null && deviceIds.Length > 0) ? string.Join(",", deviceIds) : string.Empty;

            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetSpareLicenseTransferDetailsForAssetHistory";
                sqlCmd.CommandTimeout = 120;
                sqlCmd.Parameters.Add("p_DeviceIds", MySqlDbType.LongText).Value = csvDeviceIds;
                sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int32).Value = supportAccountId;
                sqlCmd.Parameters.Add("p_RestrictedByUserAccountId", MySqlDbType.Int32).Value = restrictedByUserAccountId;
                sqlCmd.Parameters.Add("p_Code", MySqlDbType.VarChar, 45).Value = code;

                if (startDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = startDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_StartDate", MySqlDbType.Date).Value = null; ;
                }

                if (endDate.HasValue)
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = endDate.Value;
                }
                else
                {
                    sqlCmd.Parameters.Add("p_EndDate", MySqlDbType.Date).Value = null;
                }

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                var dtSpareLicenseTransferDetails = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtSpareLicenseTransferDetails);

                if (dtSpareLicenseTransferDetails.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtSpareLicenseTransferDetails.Rows)
                    {
                        long transferredByUserId = 0;
                        DateTime dateTransferred = DateTime.MinValue;

                        long.TryParse(dr["TransferredByUserId"].ToString(), out transferredByUserId);

                        var item = new AssetHistorySpareLicenseTransferDetails
                        {
                            IsTransferred = dr["IsTransferred"].ToString() != "0",
                            TransferredByUserId = transferredByUserId,
                            TransferredByEmail = dr["TransferredByEmail"].ToString(),
                            TransferredByName = dr["TransferredByName"].ToString(),
                            SourceSerialNumber = dr["SourceSerialNumber"].ToString(),
                            DestinationSerialNumber = dr["DestinationSerialNumber"].ToString()
                        };

                        if (dr["DateTransferred"] != null)
                        {
                            DateTime.TryParse(dr["DateTransferred"].ToString(), out dateTransferred);

                            if (dateTransferred != DateTime.MinValue)
                            {
                                item.DateTransferred = dateTransferred;
                            }
                        }

                        spareTransferDetailsList.Add(item);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("GetSpareTransferDetailsForAssetHistory: --> {0} {1}", csvDeviceIds, ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return spareTransferDetailsList;
        }
    }
}
