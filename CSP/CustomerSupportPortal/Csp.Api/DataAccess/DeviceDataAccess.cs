using Csp.Api.Exceptions;
using Csp.Api.Model;
using System.Collections.Generic;
using System;
using System.Configuration;
using Csp.Api.DataAccess.HelperClasses;
using System.Data;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using Csp.Api.SalesforceWebApi;
using Csp.Api.HelperClasses;
using System.Web;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Enums;
using System.Data.Entity.Infrastructure;

namespace Csp.Api.DataAccess {

    public class DeviceDataAccess : BaseDataAccess, IDeviceDataAccess {

        private readonly string TrialIoTApiActiveLinkPrefix = ConfigurationManager.AppSettings["TrialIoTApiActiveLinkPrefix"].ToString();
        private const string CloudServiceStubPartNumber = "PAN-PA-SAAS-SW";

        #region Private Methods

        /// <summary>
        /// This method is used to retrive list of Device based on Stored Procedure Name
        /// </summary>
        /// <param name="storedProcedureName">Stored Procedure Name</param>
        /// <param name="supportAccountId">Support Account Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <returns>List of DeviceListItem</returns>
        private List<T> readGenericSupportAccountDevices<T>(string storedProcedureName, long supportAccountId, long restrictedByUserAccountId, DataParameters parameters) where T : BaseCspModel, new() {
            var deviceListItemList = new List<T>();


            using (var command = Connection.GetStoredProcCommand(storedProcedureName)) {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
                Connection.AddSortParameters(command, parameters);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddSearchParameters(command, parameters);

                using (var dataReader = Connection.ExecuteReader(command)) {

                    while (dataReader.Read()) {

                        var deviceListItem = new T();

                        deviceListItem.PopulateProperties(dataReader);
                        deviceListItemList.Add(deviceListItem);
                    }
                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return deviceListItemList;
        }


        /// <summary>
        /// This method is used to retrive list of Device based on Stored Procedure Name
        /// </summary>
        /// <param name="storedProcedureName">Stored Procedure Name</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <param name="groupId"></param>
        /// <returns>List</returns>
        private List<DeviceListItem> ReadDevicesByGroupId(string storedProcedureName, long groupId, long restrictedByUserAccountId, DataParameters parameters) {
            var deviceListItemList = new List<DeviceListItem>();

            if (!SortHelper.ValidateSorting(parameters.SortField, parameters.SortDirection.ToSqlOrderBy()))
            {
                parameters.SortField = string.Empty;
                parameters.SortDirection = SortDirection.Ascending;
            }
            if (!SearchHelper.ValidateSearchField(parameters.SearchField))
            {
                parameters.SearchField = string.Empty;
                parameters.SearchValue = string.Empty;
            }

            using (var command = Connection.GetStoredProcCommand(storedProcedureName)) {

                Connection.AddInGroupIdParameter(command, groupId);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
                Connection.AddSortParameters(command, parameters);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddSearchParameters(command, parameters);

                // Geting Device List From database based on GroupID
                using (var dataReader = Connection.ExecuteReader(command)) {

                    while (dataReader.Read()) {
                        var deviceListItem = new DeviceListItem();
                        deviceListItem.PopulateProperties(dataReader);
                        deviceListItemList.Add(deviceListItem);
                    }

                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return deviceListItemList;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method is used to Update Device Details
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <returns>Device</returns>
        public Device Update(Device device, long restrictedByUserAccountId) {

            var resultDevice = new Device();
            using (var command = Connection.GetStoredProcCommand("UpdateDevice")) {
                Connection.AddInModelParameters(command, device, CreateOrUpdate.Update);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);

                try {
                    using (var dataReader = Connection.ExecuteReader(command)) {

                        if (dataReader.Read()) {

                            resultDevice.PopulateProperties(dataReader);
                        }

                    }

                } catch (MySqlException exception) {
                    if (exception.Message.Contains("Concurrent update error")) {

                        throw new CspNotificationException(
                            "A user has already updated this Device. Please refresh your data, review the changes made and try again");
                    }
                    throw;
                }
            }

            return resultDevice;
        }


        /// <summary>
        /// This method is used to retrieve the Device details based on Device ID
        /// </summary>
        /// <param name="deviceId">deviceId</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <returns>Device</returns>
        public DeviceWithPartMaster Read(long deviceId, long restrictedByUserAccountId) {
            var trialLicenseExcludesPanorama = System.Configuration.ConfigurationManager.AppSettings["TrialLicenseExcludesPanorama"].Split(',');
            using (var command = Connection.GetStoredProcCommand("GetDeviceByDeviceId")) {
                Connection.AddInDeviceIdParameter(command, deviceId);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
                using (var dataReader = Connection.ExecuteReader(command)) {
                    var device = new DeviceWithPartMaster();

                    if (dataReader.Read()) {
                        device.PopulateProperties(dataReader);
                        device.IsCirroSecureDevice = device.ModelCompatibility.Contains("CIRRO");
                        device.IsSaasStubDevice = device.PartNumber.ToUpper().Contains(CloudServiceStubPartNumber);
                        device.IsTraps = device.ModelName.ToUpper().Contains("PAN-PA-EP");
                        device.IsTrapsManagementService = dataReader["TenantId"] != DBNull.Value && dataReader["TenantID"].ToString() != "" ? true : false;
                        device.IsCgx = device.ModelCompatibility.Contains("CGX");
                        device.IsTrialLicenseExcludePanorama = trialLicenseExcludesPanorama.Any(d => device.ModelCompatibility.Contains(d));
                        device.IsPanorama = device.ModelName.ToUpper().Contains("PAN-PRA-");
                        device.TenantId = dataReader["TenantId"] != DBNull.Value ? dataReader["TenantID"].ToString() : string.Empty;
                    }

                    return device;
                }
            }
        }


        /// <summary>
        /// This method is used to retrive list of Device based on Support Account ID
        /// </summary>
        /// <param name="supportAccountId">Support Account Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <returns>List of DeviceListItem</returns>
        public List<DeviceListItemModelCompatibility> ReadSupportAccountDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters) {

            return readGenericSupportAccountDevices<DeviceListItemModelCompatibility>("GetDevicesBySupportAccountId",
                                                                                                                                                                supportAccountId,
                                                                                                                                                                restrictedByUserAccountId, parameters);
        }


        /// <summary>
        /// This method is used to retrive list of Spare Device based on Support Account ID
        /// </summary>
        /// <param name="supportAccountId">Support Account Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <returns>List of DeviceListItem</returns>
        public List<DeviceListItemModelCompatibility> ReadSupportAccountSpareDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters) {

            return readGenericSupportAccountDevices<DeviceListItemModelCompatibility>("GetSpareDevicesBySupportAccountId",
                                                                                                                                                                supportAccountId,
                                                                                                                                                                restrictedByUserAccountId, parameters);
        }


        /// <summary>
        /// This method is used to retrive list of device in a prticular group. 
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Paination Parameter</param>
        /// <returns>List of DeviceListItem</returns>
        public List<DeviceListItem> ReadGroupDevices(long groupId, long restrictedByUserAccountId, DataParameters parameters) {

            return ReadDevicesByGroupId("GetDevicesByGroupId", groupId, restrictedByUserAccountId, parameters);
        }

        /// <summary>
        /// This method is used to retrive list of spare device in a prticular group. 
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Paination Parameter</param>
        /// <returns>List of DeviceListItem</returns>
        public List<DeviceListItem> ReadGroupSpareDevices(long groupId, long restrictedByUserAccountId, DataParameters parameters) {

            return ReadDevicesByGroupId("GetSpareDevicesByGroupId", groupId, restrictedByUserAccountId, parameters);
        }

        public List<SoftwareKey> ReadWF500SoftwareKeys(string serialNumber) {

            const string selectStatement = "select lictype, lickey from wf_hw_data device join wf_config_license license on device.id = license.hw_id where serialnum = @p_serialNumber";

            var softwareKeyList = new List<SoftwareKey>();

            using (var panCommand = PANDBConnection.GetSqlStringCommand(selectStatement)) {

                PANDBConnection.AddInParameter(panCommand, "@p_serialNumber", DbType.String, serialNumber);

                using (var dataReader = PANDBConnection.ExecuteReader(panCommand)) {

                    while (dataReader.Read()) {

                        var softwareKey = new SoftwareKey {

                            LicenseType = dataReader["lictype"].ToString(),
                            LicenseKey = dataReader["lickey"].ToString()
                        };
                        softwareKeyList.Add(softwareKey);
                    }
                }
            }
            return softwareKeyList;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters"></param>
        /// <returns>List of DeviceListItem</returns>
        public List<DeviceListItem> ReadAvailableSerialNumber(long groupId, long restrictedByUserAccountId, DataParameters parameters) {
            var deviceListItems = new List<DeviceListItem>();
            using (var command = Connection.GetStoredProcCommand("GetAvailableSerialNumberByUserAccountID")) {
                Connection.AddInGroupIdParameter(command, groupId);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddSortParameters(command, parameters);

                Connection.AddSearchParameters(command, parameters);

                using (var dataReader = Connection.ExecuteReader(command)) {

                    while (dataReader.Read()) {

                        var deviceListItem = new DeviceListItem();
                        deviceListItem.DeviceId = Convert.ToInt64(dataReader["DeviceId"]);
                        deviceListItem.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        deviceListItems.Add(deviceListItem);

                    }

                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return deviceListItems;
        }

        public List<DeviceListItem> ReadAvailableSpareSerialNumber(long groupId, long restrictedByUserAccountId, DataParameters parameters) {
            var deviceListItems = new List<DeviceListItem>();
            using (var command = Connection.GetStoredProcCommand("GetAvailableSpareSerialNumberByUserAccountID")) {
                Connection.AddInGroupIdParameter(command, groupId);
                Connection.AddInRestrictedByUserAccountId(command, restrictedByUserAccountId);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddSortParameters(command, parameters);

                Connection.AddSearchParameters(command, parameters);

                using (var dataReader = Connection.ExecuteReader(command)) {

                    while (dataReader.Read()) {

                        var deviceListItem = new DeviceListItem();
                        deviceListItem.DeviceId = Convert.ToInt64(dataReader["DeviceId"]);
                        deviceListItem.SerialNumber = HttpUtility.HtmlEncode(dataReader["SerialNumber"].ToString());
                        deviceListItems.Add(deviceListItem);

                    }

                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return deviceListItems;


        }

        public string CheckIsSerialNumberPanorama(string serialNumber) {
            using (var command = Connection.GetStoredProcCommand("CheckIsSerialNumberValidandPanorama")) 
            {
                var result = "";

                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddOutParameter(command, "p_result", DbType.String, 50);
                Connection.ExecuteNonQuery(command);

                if (command.Parameters["p_result"].Value != DBNull.Value)
                {
                    result = command.Parameters["p_result"].Value.ToString();
                }

                return result;
            }
        }

        public int CheckIsSerialNumberValidandNotRegisterd(string serialNumber)
        {
            int result = 0;

            using (var command = Connection.GetStoredProcCommand("CheckIsSerialNumberValidandNotRegisterd"))
            {
                Connection.AddInParameter(command, "@p_serialNumber", DbType.String, serialNumber);

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    dataReader.Read();

                    result = Convert.ToInt32(dataReader["p_result"]);

                    return result;
                }
            }
        
            return result;
            
        }

        public bool CheckIsSerialNumberEligible(string serialNumber)
        {
            const string selectStatement = @"SELECT COUNT(1) > 0 IsEligible 
                                            FROM device d
                                            JOIN
                                            (
                                                SELECT pm.PartMasterId FROM csp.partmaster pm WHERE pm.PartTypeId = 3 AND pm.ProductFamily REGEXP 'PA-([0-9]{2,4})'
                                                UNION
                                                SELECT pm.PartMasterId FROM csp.partmaster pm WHERE pm.PartTypeId = 3 AND pm.ProductFamily REGEXP 'PA-VM-([0-9]{2,4})'
                                                UNION
                                                SELECT pm.PartMasterId FROM csp.panoramapartmaster p JOIN partmaster pm ON p.PartMasterId = pm.PartMasterId
                                                UNION
                                                SELECT pm.PartMasterId FROM csp.partmaster pm WHERE ProductFamily LIKE 'WF-%'
                                            ) a ON a.PartMasterId = d.PartMasterId
                                            WHERE d.SerialNumber = @d_serialNumber ";
            var isEligible = false;
            using (var selectCommand = Connection.GetSqlStringCommand(selectStatement))
            {
                Connection.AddInParameter(selectCommand, "@d_serialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(selectCommand))
                {
                    while (dataReader.Read())
                    {
                        isEligible = Convert.ToBoolean(dataReader["IsEligible"]);
                    }
                }
            }
            return isEligible;
        }


        public void UpdateForDeviceRegistration(Device device, long lastUpdateByUserAccountId, long supportAccountId) {
            using (var command = Connection.GetStoredProcCommand("UpdateForDeviceRegistration")) {

                Connection.AddInParameter(command, "p_serialNumber", DbType.String, device.SerialNumber);
                Connection.AddInParameter(command, "p_region", DbType.String, device.Region);
                Connection.AddInParameter(command, "p_deviceCategoryId", DbType.Int32, device.DeviceCategoryId);
                Connection.AddInParameter(command, "p_lastUpdateByUserAccountId", DbType.Int64, lastUpdateByUserAccountId);
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);

                Connection.ExecuteNonQuery(command);
            }
        }

        public void UpdatePANDBDeviceMaster(Device device, string countryName) {

            const string updateSqlQuey = @"update device_master set acctid=(select max(aid) max_aid from accounts where email=@p_email), 
                                            device_name=@p_DeviceName , regdate=NOW(),  comments=@p_Comments ,devCity=@p_City , devZipCode=@p_PostalCode , 
                                            devCountry=@p_CountryName, devAddress1=@p_Address1, devAddress2=@p_Address2, panosversion=@p_PanOsVersion,
                                            `grouping`=@p_Group    
                                            where serialnum=@p_SerialNumber and active=1;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey)) {
                PANDBConnection.AddInParameter(panCommand, "@p_email", DbType.String, device.LastUpdateByEmail);
                PANDBConnection.AddInParameter(panCommand, "@p_DeviceName", DbType.String, device.DeviceName);
                PANDBConnection.AddInParameter(panCommand, "@p_Comments", DbType.String, device.Comments);
                PANDBConnection.AddInParameter(panCommand, "@p_City", DbType.String, device.City);
                PANDBConnection.AddInParameter(panCommand, "@p_PostalCode", DbType.String, device.PostalCode);
                PANDBConnection.AddInParameter(panCommand, "@p_CountryName", DbType.String, countryName);
                PANDBConnection.AddInParameter(panCommand, "@p_SerialNumber", DbType.String, device.SerialNumber);
                PANDBConnection.AddInParameter(panCommand, "@p_PanOsVersion", DbType.String, device.PanOsVersion);
                PANDBConnection.AddInParameter(panCommand, "@p_Address1", DbType.String, device.Address1);
                PANDBConnection.AddInParameter(panCommand, "@p_Address2", DbType.String, device.Address2);
                PANDBConnection.AddInParameter(panCommand, "@p_Group", DbType.String, device.DeviceCategoryId);
                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

        public void UpdatePANDBDevicePanOsVersion(string serialNumber, string email, string panOsVersion)
        {
            const string updateSqlQuey = "update device_master set acctid=(select max(aid) max_aid from accounts where email=@p_email), regdate=NOW(), panosversion=@p_PanOsVersion  where serialnum=@p_SerialNumber and active=1;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey))
            {
                PANDBConnection.AddInParameter(panCommand, "@p_email", DbType.String, email);
                PANDBConnection.AddInParameter(panCommand, "@p_SerialNumber", DbType.String, serialNumber);
                PANDBConnection.AddInParameter(panCommand, "@p_PanOsVersion", DbType.String, panOsVersion);
                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

        public void UpdateDeviceMetadata(Device device, string countryName) {

            const string updateSqlQuey = @"update device_master set acctid=(select max(aid) max_aid from accounts where email=@p_email), 
                        device_name=@p_DeviceName , regdate=NOW(),  comments=@p_Comments ,devCity=@p_City , devZipCode=@p_PostalCode , devCountry=@p_CountryName,
                        devAddress1=@p_Address1, devAddress2=@p_Address2, PanOsVersion=@p_panOsVersion, HyperVisorVersion=@p_hyperVisorVersion  
                        where lid=@p_DeviceId and active=1;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey)) {
                PANDBConnection.AddInParameter(panCommand, "@p_email", DbType.String, device.LastUpdateByEmail);
                PANDBConnection.AddInParameter(panCommand, "@p_DeviceName", DbType.String, device.DeviceName);
                PANDBConnection.AddInParameter(panCommand, "@p_Comments", DbType.String, device.Comments);
                PANDBConnection.AddInParameter(panCommand, "@p_City", DbType.String, device.City);
                PANDBConnection.AddInParameter(panCommand, "@p_PostalCode", DbType.String, device.PostalCode);
                PANDBConnection.AddInParameter(panCommand, "@p_CountryName", DbType.String, countryName);
                PANDBConnection.AddInParameter(panCommand, "@p_DeviceId", DbType.Int64, device.Id);
                PANDBConnection.AddInParameter(panCommand, "@p_panOsVersion", DbType.String, device.PanOsVersion);
                PANDBConnection.AddInParameter(panCommand, "@p_hyperVisorVersion", DbType.String, device.HyperVisorVersion);
                PANDBConnection.AddInParameter(panCommand, "@p_Address1", DbType.String, device.Address1);
                PANDBConnection.AddInParameter(panCommand, "@p_Address2", DbType.String, device.Address2);
                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

        public void UpdateDeviceHyperVisorInfo(long deviceId, string panOsVersion, string hyperVisorVersion)
        {
            const string updateSqlQuey = @"update device_master set PanOsVersion=@p_panOsVersion, HyperVisorVersion=@p_hyperVisorVersion  
                        where lid=@p_DeviceId and active=1;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey))
            {
                PANDBConnection.AddInParameter(panCommand, "@p_DeviceId", DbType.Int64, deviceId);
                PANDBConnection.AddInParameter(panCommand, "@p_panOsVersion", DbType.String, panOsVersion);
                PANDBConnection.AddInParameter(panCommand, "@p_hyperVisorVersion", DbType.String, hyperVisorVersion);
                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

        public long GetDeviceIdBySerialNumber(string serialNumber) {

            const string sqlQuery = @"
        SELECT d.DeviceId
        FROM device d
        WHERE d.SerialNumber = @p_SerialNumber;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);

                using (IDataReader dataReader = Connection.ExecuteReader(command)) {
                    dataReader.Read();

                    var deviceId = Convert.ToInt64(dataReader["DeviceId"]);

                    return deviceId;
                }
            }
        }

        public List<DeviceCategoryListItem> ReadDeviceCategories(long supportAccountId, long userAccountId, DataParameters parameters) {
            List<DeviceCategoryListItem> deviceCategoryListItems = null;

            using (var command = Connection.GetStoredProcCommand("GetDeviceCategoriesBySupportAccountId")) {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddSortParameters(command, parameters);
                Connection.AddSearchParameters(command, parameters);

                using (var dataReader = Connection.ExecuteReader(command)) {

                    deviceCategoryListItems = new List<DeviceCategoryListItem>();
                    while (dataReader.Read()) {

                        /* this is extremly slow, use the new code
                        var deviceSetListItem = new DeviceCategoryListItem();
                        deviceSetListItem.PopulateProperties(dataReader);
                        */

                        var deviceSetListItem = new DeviceCategoryListItem
                        {
                            DeviceCategoryId = dataReader.GetColumnValue<long>("DeviceCategoryId"),
                            DeviceCategoryName = dataReader.GetColumnValue<string>("DeviceCategoryName"),
                            SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                            UserAccountId = dataReader.GetColumnValue<long>("UserAccountId"),
                            Email = dataReader.GetColumnValue<string>("Email"),
                            IsPublic = dataReader.GetColumnValue<bool>("IsPublic")
                        };
                        deviceCategoryListItems.Add(deviceSetListItem);
                    }
                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return deviceCategoryListItems;
        }

        public DeviceCategory CreateDeviceCategory(DeviceCategory deviceCategory) {
            const string sqlQuery = @"INSERT INTO devicecategory
                                    (
                                      DeviceCategoryName
                                     ,Description
																		 ,SupportAccountId
																		 ,UserAccountId
																		 ,IsPublic
                                     ,IsActive
                                     ,CreatedOn
                                     ,CreatedByUserAccountId
                                    )
                                    VALUES
                                    (
                                      @p_DeviceCategoryName
                                     ,@p_Description
                                     ,@p_SupportAccountId
                                     ,@p_UserAccountId
                                     ,@p_IsPublic
                                     ,@p_IsActive
                                     ,@p_CreatedOn
                                     ,@p_CreatedByUserAccountId
                                    );SELECT LAST_INSERT_ID();";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInModelParameters(command, deviceCategory, CreateOrUpdate.Create);

                long deviceCategoryId = Convert.ToInt64(Connection.ExecuteScalar(command));

                return ReadDeviceCategory(deviceCategoryId);
            }
        }

        public DeviceCategory ReadDeviceCategory(long deviceCategoryId) {
            const string sqlQuery = "SELECT * FROM devicecategory WHERE DeviceCategoryId = @p_DeviceCategoryId;";
            var deviceCategory = new DeviceCategory();
            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInParameter(command, "@p_DeviceCategoryId", DbType.Int64, deviceCategoryId);

                using (IDataReader dataReader = Connection.ExecuteReader(command)) {
                    if (dataReader.Read()) {
                        deviceCategory.PopulateProperties(dataReader);
                    }

                }
            }

            return deviceCategory;
        }
        public DeviceCategory UpdateDeviceCategory(DeviceCategory deviceCategory) {
            const string sqlQuery = @"UPDATE devicecategory 
                                    SET
                                      DeviceCategoryName = @p_DeviceCategoryName
                                     ,Description = @p_Description
																		 ,SupportAccountId = @p_SupportAccountId											
																		 ,UserAccountId = @p_UserAccountId											
																		 ,IsPublic = @p_IsPublic											
                                     ,IsActive = @p_IsActive
                                     ,LastUpdate = @p_LastUpdate
                                     ,LastUpdateByUserAccountId = @p_LastUpdateByUserAccountId
                                    WHERE
                                      DeviceCategoryId = @p_DeviceCategoryId";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInModelParameters(command, deviceCategory, CreateOrUpdate.Update);

                Connection.ExecuteNonQuery(command);
            }

            return ReadDeviceCategory(deviceCategory.DeviceCategoryId);
        }

        public void DeleteDeviceCategory(DeviceCategory deviceCategory) {
            // Set DeviceCategoryId to NULL for all devices prior deleting a DeviceCategory
            string sqlQuery = @"UPDATE device 
                                    SET
                                      DeviceCategoryId = NULL
                                     ,LastUpdate = @p_LastUpdate
                                     ,LastUpdateByUserAccountId = @p_LastUpdateByUserAccountId
                                    WHERE
                                      DeviceCategoryId = @p_DeviceCategoryId";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInModelParameters(command, deviceCategory, CreateOrUpdate.Update);

                Connection.ExecuteNonQuery(command);
            }

            sqlQuery = @"DELETE FROM devicecategorymapping WHERE DeviceCategoryId = @p_DeviceCategoryId";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceCategoryId", DbType.Int64, deviceCategory.DeviceCategoryId);

                Connection.ExecuteNonQuery(command);
            }

            sqlQuery = @"DELETE FROM devicecategory WHERE DeviceCategoryId = @p_DeviceCategoryId";
            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInModelParameters(command, deviceCategory, CreateOrUpdate.Update);

                Connection.ExecuteNonQuery(command);
            }
        }

        public bool IsDeviceCategoryExists(string deviceCategoryName, long supportAccountId, long deviceCategoryId) {
            var isExists = false;
            var sqlQuery = "SELECT COUNT(DeviceCategoryId) FROM devicecategory WHERE ";

            if (deviceCategoryId > 0) {
                sqlQuery += "DeviceCategoryId <> @p_DeviceCategoryId AND ";
            }

            sqlQuery += "DeviceCategoryName = @p_DeviceCategoryName AND SupportAccountId = @p_SupportAccountId;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {

                if (deviceCategoryId > 0) {
                    Connection.AddInParameter(command, "@p_DeviceCategoryId", DbType.Int32, deviceCategoryId);
                }

                Connection.AddInParameter(command, "@p_DeviceCategoryName", DbType.String, deviceCategoryName);
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int32, supportAccountId);

                using (var dataReader = Connection.ExecuteReader(command)) {
                    if (dataReader.Read()) {
                        isExists = (dataReader.GetInt32(0) > 0);
                    }

                }
            }

            return isExists;
        }

        public SupportAccount GetSupportAccountIdforDeviceId(long deviceId)
        {
            var sqlQuery = "SELECT d.SupportAccountId, s.Name FROM Device d INNER JOIN supportaccount s ON d.SupportAccountId = s.SupportAccountId WHERE DeviceId = @p_DeviceId;";
            var supportAccount = new SupportAccount();
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {

                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int32, deviceId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        if (dataReader["SupportAccountId"] != null)
                        {
                            return supportAccount = new SupportAccount()
                            {
                                Id = Convert.ToInt64(dataReader["SupportAccountId"]),
                                Name = dataReader["Name"].ToString()
                            };
                        }
                    }

                }
            }

            return supportAccount;
        }

        public void TransferDeviceInPanDb(long deviceId, long userAccountId, string comments) {

            const string updateSqlQuey = @"
            UPDATE device_master 
            SET 
              acctid=@p_userAccountId, 
              comments=concat(comments, @p_comments)
            WHERE lid=@p_deviceId;";
            using (var panCommand = PANDBConnection.GetSqlStringCommand(updateSqlQuey)) {
                PANDBConnection.AddInParameter(panCommand, "@p_deviceId", DbType.Int64, deviceId);
                PANDBConnection.AddInParameter(panCommand, "@p_userAccountId", DbType.Int64, userAccountId);
                PANDBConnection.AddInParameter(panCommand, "@p_comments", DbType.String, comments);

                PANDBConnection.ExecuteNonQuery(panCommand);
            }
        }

        public long AddRmaDeviceToDeviceMaster(long deviceId) {

            const string sqlQuery = @"
            INSERT INTO device_master (
              lid
              ,serialnum  
              ,regdate
              ,acctid
              ,part_id
              ,ship_date
              ,active
              ,shipnum
              ,device_name
              ,comments
              ,last_modified
              ,src
              ,dest
              ,sfdcacctid
              ,vsys
              ,ispublic
              ,`grouping`
              ,pan_notes
              ,nssynched
              ,useCDN
              ,grace
              ,billtoCountry
              ,devCity
              ,devZipCode
              ,devCountry)
            SELECT 
              lid
              ,serialnum
              ,regdate
              ,acctid
              ,part_id
              ,ship_date
              ,active
              ,shipnum
              ,device_name
              ,comments
              ,last_modified
              ,src
              ,dest
              ,sfdcacctid
              ,vsys
              ,ispublic
              ,`grouping`
              ,pan_notes
              ,nssynched
              ,useCDN
              ,ifnull(grace, 0)
              ,billtoCountry
              ,devCity
              ,devZipCode
              ,devCountry 
            FROM device_master_bkp
            where lid = @p_deviceId
            ORDER BY bkpwhen desc LIMIT 1;";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
                PANDBConnection.AddInParameter(command, "@p_deviceId", DbType.Int64, deviceId);
                PANDBConnection.ExecuteNonQuery(command);
            }

            return deviceId;
        }

        public void RemoveRmaDeviceFromDeviceMaster(long deviceId) {
            const string sqlQuery = @"DELETE FROM device_master where lid = @p_deviceId";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
                PANDBConnection.AddInParameter(command, "@p_deviceId", DbType.Int64, deviceId);

                PANDBConnection.ExecuteNonQuery(command);
            }
        }

        public void LogWipeOutNFRWildfire(string data, string apiCalled, double responseTimeInMS, string status, string exception) {
            const string sqlQuery = "INSERT INTO pandb.wildfireapicalllog (Application, Server, Data, ApiCalled, ResponseTime, Status, Exception) VALUES (@application,@server,@data,@apicalled,@responsetime,@status,@exception);";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {

                PANDBConnection.AddInParameter(command, "@application", DbType.String, "CSP");
                PANDBConnection.AddInParameter(command, "@server", DbType.String, Environment.MachineName);
                PANDBConnection.AddInParameter(command, "@data", DbType.String, data);
                PANDBConnection.AddInParameter(command, "@apicalled", DbType.String, apiCalled);
                PANDBConnection.AddInParameter(command, "@responsetime", DbType.String, responseTimeInMS);
                PANDBConnection.AddInParameter(command, "@status", DbType.String, status);
                PANDBConnection.AddInParameter(command, "@exception", DbType.String, exception);

                PANDBConnection.ExecuteNonQuery(command);
            }
        }

        public bool TransferDeviceToSupportAccount(long deviceId, long? supportAccountId , long transferredByUserAccountId,bool removeSaas = false) 
        {
            using (var command = Connection.GetStoredProcCommand("DirectDeviceTransferToSupportAccount")) 
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                Connection.AddInParameter(command, "@p_removeSaas", DbType.Boolean, removeSaas);
                Connection.AddInParameter(command, "@p_transferedBy",DbType.Int64, transferredByUserAccountId);
                Connection.ExecuteNonQuery(command);
            }

            return true;
        }
        /// <summary>
        /// This method is used to get the components by chassis with quantity
        /// </summary>
        /// <param name="serialNumber">Serial number of the resgistering device</param>
        public IEnumerable<ChassisComponent> GetChassisUnassignedComponentQuantities(string chassisSerialNumber) {
            var components = new List<ChassisComponent>();
            using (var command = Connection.GetStoredProcCommand("usp_4HR_GetUnassignedComponentQuantitiesByChassis")) {
                Connection.AddInParameter(command, "p_ChassisSerialNumber", DbType.String, chassisSerialNumber);
                using (var dataReader = Connection.ExecuteReader(command)) {
                    while (dataReader.Read())
                    {
                        var component = new ChassisComponent();
                        component.PopulateProperties(dataReader);
                        components.Add(component);
                    }
                    return components;
                }
            }
        }

        /// <summary>
        /// This method is used to get the device line cards with quantity
        /// </summary>
        /// <param name="serialNumber">Serial number of the resgistering device</param>
        public IEnumerable<ChassisComponent> GetUnassignedComponentQuantitiesBySalesOrder(String salesOrderNumber)
        {
            var components = new List<ChassisComponent>();

            using (var command = Connection.GetStoredProcCommand("usp_4HR_GetUnassignedComponentQuantitiesBySalesOrder"))
            {
                Connection.AddInParameter(command, "p_SalesOrderNumber", DbType.String, salesOrderNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var component = new ChassisComponent();
                        component.PopulateProperties(dataReader);
                        components.Add(component);
                    }
                    return components;
                }

            }
        }

        /// <summary>
        /// This method is used to check whether SalesOrder Components alredy Registered
        /// </summary>
        /// <param name="salesOrderNumber">Sales order number</param>
        public string IsSalesOrderComponentsRegistered(string salesOrderNumber)
        {
            var salesOrder = string.IsNullOrEmpty(salesOrderNumber) ? salesOrderNumber : salesOrderNumber.PadLeft(10,'0');

            string sqlQuery = "SELECT SAPSalesOrderNumber FROM salesorder s WHERE s.SAPSalesOrderNumber = @p_SalesOrderNumber;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SalesOrderNumber", DbType.String, salesOrder);
                var registeredSaleOrder = Connection.ExecuteScalar(command);
                if (registeredSaleOrder == null || registeredSaleOrder == DBNull.Value)
                {
                    return "Sales order does not exist";
                }
            }

            var unassignedComponents = GetUnassignedComponentQuantitiesBySalesOrder(salesOrder);
            if (!unassignedComponents.Any())
            {
                return "Sales order already registered";
            }

            return string.Empty;
        }

        public bool Has4HRSupport(long deviceId)
        {
            string sqlQuery = "SELECT COUNT(l.PartMasterId) FROM  license l JOIN partmaster p ON l.PartMasterId =  p.PartMasterId AND l.DeviceId = @DeviceId "
            + " WHERE l.IsActive = 1 AND p.PartNumber LIKE '%-4HR-%'";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@DeviceId", DbType.Int64, deviceId);
                var count = Connection.ExecuteScalar(command);
                return Convert.ToInt32(count) > 0;
            }
        }

        /// <summary>
        /// This method is used to get the  registered and unregistered components of the support  account id 
        /// </summary>
        /// <param name="userAccountId">userAccountId</param>
        public IEnumerable<ChassisComponent> GetComponentsBySupportAccount(long supportAccountId)
        {
            var deviceLineCardList = new List<ChassisComponent>();

            using (var command = Connection.GetStoredProcCommand("usp_4HR_GetComponentsBySupportAccount"))
            {
                Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {

                    while (dataReader.Read())
                    {
                        var linecard = new ChassisComponent();
                        linecard.PopulateProperties(dataReader);
                        deviceLineCardList.Add(linecard);
                    }

                    return deviceLineCardList;
                }

            }
        }

        public Int32 GetUnassignedComponentQuantity(string partNumber, Int64 supportAccountId)
        {
            var deviceLineCardList = new List<ChassisComponent>();

            using (var command = Connection.GetStoredProcCommand("usp_4HR_GetUnassignedComponentQuantity"))
            {
                Connection.AddInParameter(command, "p_PartNumber", DbType.String, partNumber);
                Connection.AddInParameter(command, "p_supportAccountId", DbType.Int64, supportAccountId);
                var unassignedQunatity = Connection.ExecuteScalar(command);
                return Convert.ToInt32(unassignedQunatity);
            }
        }

        public void UpdateChassisComponents(List<ChassisComponent> chassisComponents)
        {
            if (chassisComponents == null || chassisComponents.Count == 0)
                return;
            var query = "START TRANSACTION;";
            foreach (var cComponents in chassisComponents)
            {
                query += String.Format("call usp_4HR_UpdateChassisComponent('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}','{9}','{10}','{11}'); ",
                    cComponents.ChassisSerialNumber, cComponents.PartNumber, cComponents.SupportAccountId, cComponents.Quantity, cComponents.Address1, 
                    cComponents.Address2,cComponents.CountryId, cComponents.Region, cComponents.City, cComponents.PostalCode,
                    string.IsNullOrEmpty(cComponents.SalesOrderNumber) ? cComponents.SalesOrderNumber : cComponents.SalesOrderNumber.PadLeft(10, '0'), cComponents.UserAccountId);
            }
            if (chassisComponents[0].SalesOrderNumber != null && chassisComponents[0].SalesOrderNumber != "")
            {
                query += String.Format("INSERT INTO csp.registeredfrusalesorders(SalesOrderNumber, SupportAccountId) VALUES('{0}',{1});  ", chassisComponents[0].SalesOrderNumber.PadLeft(10, '0'), chassisComponents[0].SupportAccountId);
            }
            query += "COMMIT;";
            using (var command = Connection.GetSqlStringCommand(query))
            {
                Connection.ExecuteNonQuery(command);
            }

        }

        public void UpdateComponent(ChassisComponent component)
        {
            using (var command = Connection.GetStoredProcCommand("usp_4HR_IncrementChassisComponentQuantity"))
            {
                Connection.AddInParameter(command, "p_ChassisSerialNumber", DbType.String, component.ChassisSerialNumber);
                Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, component.SupportAccountId);
                Connection.AddInParameter(command, "p_PartNumber", DbType.String, component.PartNumber);
                Connection.AddInParameter(command, "p_Address1", DbType.String, component.Address1);
                Connection.AddInParameter(command, "p_Address2", DbType.String, component.Address2);
                Connection.AddInParameter(command, "p_City", DbType.String, component.City);
                Connection.AddInParameter(command, "p_Region", DbType.String, component.Region);
                Connection.AddInParameter(command, "p_PostalCode", DbType.String, component.PostalCode);
                Connection.AddInParameter(command, "p_CountryId", DbType.Int32, component.CountryId);
                Connection.AddInParameter(command, "p_Quantity", DbType.Int32, component.Quantity);
                Connection.AddInParameter(command, "p_SAPSalesOrderNumber", DbType.String, component.SalesOrderNumber);
                Connection.AddInParameter(command, "p_UserId", DbType.Int64, component.UserAccountId);
                Connection.ExecuteNonQuery(command);
            }
        }

        public void DeleteComponent(ChassisComponent component)
        {
            using (var command = Connection.GetStoredProcCommand("usp_4HR_DecrementChassisComponentQuantity"))
            {
                Connection.AddInParameter(command, "p_ChassisSerialNumber", DbType.String, component.ChassisSerialNumber);
                Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, component.SupportAccountId);
                Connection.AddInParameter(command, "p_PartNumber", DbType.String, component.PartNumber);
                Connection.AddInParameter(command, "p_Quantity", DbType.Int32, component.Quantity);
                Connection.ExecuteNonQuery(command);
            }
        }

        public string CheckIsSerialNumberValidandRegisterd(string serialNumber)
        {
            using (var command = Connection.GetStoredProcCommand("Usp_CheckIsSerialNumberValidandRegisterd"))
            {
                var result = "";
                Connection.AddInParameter(command, "p_serialNumber", DbType.String, serialNumber);
                Connection.AddOutParameter(command, "p_result", DbType.String, 50);
                Connection.ExecuteNonQuery(command);

                if (command.Parameters["p_result"].Value != DBNull.Value)
                {
                    result = command.Parameters["p_result"].Value.ToString();
                }


                return result;
            }
        }

        /// <summary>
        /// This method is used to get chassis location
        /// </summary>
        /// <param name="chassisSerialNumber">Update specific Line card</param>
        public ChassisComponent GetChassisLocation(string chassisSerialNumber)
        {
            const string sqlQuery = "SELECT d.SerialNumber,d.Address1,d.Address2,d.City,d.CountryId,d.City,d.Region,d.PostalCode FROM device d WHERE d.SerialNumber = @p_ChassisSerialNumber";
            var components = new List<ChassisComponent>();
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_ChassisSerialNumber", DbType.String, chassisSerialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var component = new ChassisComponent();
                        component.PopulateProperties(dataReader);
                        components.Add(component);
                    }
                    return components.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// This method is used to update chassis location
        /// </summary>
        /// <param name="component">Update specific Line card</param>
        /// <param name="userAccountId">userAccountId</param>
        public void UpdateChassisLocation(ChassisComponent component)
        {
            using (var command = Connection.GetStoredProcCommand("usp_4HR_UpdateChassisComponentsLocation"))
            {
                Connection.AddInParameter(command, "p_ChassisSerialNumber", DbType.String, component.ChassisSerialNumber);
                Connection.AddInParameter(command, "p_Address1", DbType.String, component.Address1);
                Connection.AddInParameter(command, "p_Address2", DbType.String, component.Address2);
                Connection.AddInParameter(command, "p_City", DbType.String, component.City);
                Connection.AddInParameter(command, "p_Region", DbType.String, component.Region);
                Connection.AddInParameter(command, "p_PostalCode", DbType.String, component.PostalCode);
                Connection.AddInParameter(command, "p_CountryId", DbType.Int32, component.CountryId);
                Connection.ExecuteNonQuery(command);
            }
        }

        public bool IsProcessingCard(string serialNumber)
        {
            const string sqlQuery = @"SELECT d.DeviceId, d.SerialNumber FROM device d JOIN partmaster p ON d.PartMasterId = p.PartMasterId
                                    JOIN parttype pt ON p.PartTypeId = pt.PartTypeId
                                    JOIN partsubtype pst ON p.PartSubtypeId = pst.PartSubtypeId
                                    WHERE pt.PanDbIdentification = 'DEV' AND pst.PanDbIdentification = 'CARD' AND d.SerialNumber=@p_SerialNumber";
            var isProcessingCard = false;
            
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                        isProcessingCard = true;
                }
            }

            return isProcessingCard;
        }

        /// <summary>
        /// Check if the device is K2 device and get the mode if K2 device
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool IsK2Device(string serialNumber, out string mode)
        {
            var isK2Device = false;
            mode = string.Empty;

            using (var command = Connection.GetStoredProcCommand("GetK2DeviceMode"))
            {
                Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        isK2Device = Convert.ToBoolean(dataReader["IsK2Device"]);
                        mode = dataReader["K2DeviceMode"].ToString();
                    }
                }
            }

            return isK2Device;
        }

        #endregion

        #region IDeviceDataAccess Members

        public void ReconcileWildfireExpirationDatesDuringDeviceTransfer(long fromSupportAccountId, long toSupportAccountId)
        {
            using (var command = PANDBConnection.GetStoredProcCommand("uspTransferWildfireApiKey")) {
							PANDBConnection.AddInParameter(command, "@p_fromcompanyId", DbType.Int64, fromSupportAccountId);
							PANDBConnection.AddInParameter(command, "@p_tocompanyId", DbType.String, toSupportAccountId);
                using (PANDBConnection.ExecuteReader(command))
                {
                    
                }
            }
        }


        public void LogWildfire(string data, string apiCalled, double responseTimeInMs, string status, string exception,
            short isExceptionReprocessed)
        {
            const string sqlQuery =
                "insert into pandb.wildfireapicalllog (Application,Server,Data,ApiCalled,ResponseTime,Status,Exception,IsExceptionReProcessed,CreatedDt) VALUES (@application,@server,@data,@apicalled,@responsetime,@status,@exception,@isreprocessed,@createddt);";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@application", DbType.String, "CSP");
                PANDBConnection.AddInParameter(command, "@server", DbType.String, Environment.MachineName);
                PANDBConnection.AddInParameter(command, "@data", DbType.String, data);
                PANDBConnection.AddInParameter(command, "@apicalled", DbType.String, apiCalled);
                PANDBConnection.AddInParameter(command, "@responsetime", DbType.Double, responseTimeInMs);
                PANDBConnection.AddInParameter(command, "@status", DbType.String, status);
                PANDBConnection.AddInParameter(command, "@exception", DbType.String, exception);
                PANDBConnection.AddInParameter(command, "@isreprocessed", DbType.Int16, isExceptionReprocessed);
                PANDBConnection.AddInParameter(command, "@createddt", DbType.DateTime, System.DateTime.Now);
                PANDBConnection.ExecuteNonQuery(command);
            }
        }

        #endregion

        #region IDeviceDataAccess Members

        // logic needs to be confirmed
        public bool CheckForWFSubscription(long supportAccountId)
        {
            try
            {
                const string sqlQuery = @"SELECT  s.SupportAccountId  FROM csp.supportaccount s INNER JOIN csp.device d ON s.SupportAccountId = d.SupportAccountId
                                            INNER JOIN csp.license l ON d.DeviceId = l.DeviceId INNER JOIN csp.partmaster p ON l.PartMasterId = p.PartMasterId
                                            INNER JOIN csp.partsubtype p1 ON p.PartSubtypeId = p1.PartSubtypeId 
                                            WHERE s.SupportAccountId = @p_SupportAccountId AND l.ExpirationDate >= NOW() AND (p1.PanDbIdentification = 'WILDFIRE' OR p1.PanDbIdentification = 'RENWILDFIRE' 
                                            OR p1.PanDbIdentification = 'WILDFIREEVAL') ORDER BY l.ExpirationDate DESC LIMIT 1";

                long accountId = long.MinValue;

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.String, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            accountId = Convert.ToInt64(dataReader["SupportAccountId"]);
                        }

                        return (accountId == long.MinValue || accountId <= 0) ? false : true;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        #endregion

        #region IDeviceDataAccess Members


        public bool IsDeviceCodeCirroSecure(string deviceCode, out long devId) {
            devId = -1;
            const string sqlQuery = "select deviceid from device d join partmaster pm on d.partmasterid=pm.partmasterid where d.serialnumber=@p_SerialNumber and pm.modelcompatibility like '%CIRRO%' union select d.deviceid from device d join license l on d.deviceid=l.deviceid join partmaster pm on l.partmasterid=pm.partmasterid where l.authorizationcode=@p_AuthCode and pm.modelcompatibility like '%CIRRO%'";
            var deviceCategory = new DeviceCategory();
            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, deviceCode);
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, deviceCode);

                var cirroSecureDevice = Connection.ExecuteScalar(command);
                if (cirroSecureDevice != null && cirroSecureDevice != DBNull.Value) {
                    devId = Convert.ToInt64(cirroSecureDevice);
                    return true;
                }
            }

            return false;
        }

        public bool RegisterCirroSecureDevice(long deviceId, long supportAccountId, long userAccountId) {
            // Set DeviceCategoryId to NULL for all devices prior deleting a DeviceCategory
            string sqlQuery = @"UPDATE device 
                                    SET
                                      SupportAccountId = @p_supportAccountId
                                     ,UserAccountId = @p_userAccountId
                                    WHERE
                                      DeviceId = @p_DeviceId";

            using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_userAccountId", DbType.Int64, userAccountId);
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                return Connection.ExecuteNonQuery(command) > 0;
            }
        }

        public void CheckGovernmentCloud(List<DeviceListItem> deviceItems)
        {
            if (deviceItems == null || !deviceItems.Any())
                return;

            var query = new StringBuilder();
            query.Append(@"SELECT DeviceId FROM clouddevices WHERE RegionCode LIKE '%-gov-%' AND DeviceId IN (");
            for (int i = 0; i < deviceItems.Count; i++)
            {
                if (i == 0)
                    query.AppendFormat("{0}", deviceItems[i].DeviceId);
                else
                {
                    query.AppendFormat(",{0}", deviceItems[i].DeviceId);
                }
            }
            query.Append(");");
            var queryStr = query.ToString();

            List<long> devideIdsList= new List<long>();

            using (var command = Connection.GetSqlStringCommand(queryStr))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var deviceId = dataReader.GetColumnValue<long?>("DeviceId");
                        if (deviceId != null)
                            devideIdsList.Add(deviceId.Value);
                    }
                }
            }

            if (devideIdsList.Any())
            {
                foreach (var d in deviceItems)
                {
                   
                    if (devideIdsList.Contains(d.DeviceId))
                        d.IsAwsGovCloud = true;
                        d.HyperVisorVersion = "Amazon AWS Gov Cloud";
                }
            }
        }


        #endregion

        #region Bulk Device Transfer Methods
        public void ProcessBulkDeviceTransfer(BulkDeviceTransferRequest request)
        {
            try
            {
                using (var backupcommand = Connection.GetStoredProcCommand("ProcessBulkDeviceTransferRequests"))
                {
                    Connection.AddInParameter(backupcommand, "@p_bulktransferId", DbType.Int64, request.BulkDevicetransferId);
                    Connection.AddInParameter(backupcommand, "@p_destinationaccountId", DbType.Int64, request.DestinationAccountId);
                    Connection.ExecuteNonQuery(backupcommand);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public List<BulkDeviceTransferRequest> GetBulkTransferRequests()
        {
            var transferRequests= new List<BulkDeviceTransferRequest>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPendingBulkDeviceTransferRequests"))
                {
                   
                    using (var dataReader = Connection.ExecuteReader(command))
                    {

                        while (dataReader.Read())
                        {
                            var request = new BulkDeviceTransferRequest()
                            {
                                BulkDevicetransferId = Convert.ToInt64(dataReader["BulkDeviceTransferId"]),
                                DestinationAccountId = Convert.ToInt64(dataReader["DestinationSupportAccountId"]),
                                SourceAccountId = Convert.ToInt64(dataReader["SouceSupportAccountId"]),
                               
                            };

                            transferRequests.Add(request);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return transferRequests;
        }

        public List<long> GetDeviceIdsByBulkDeviceTransferId(long transferRequestId)
        {
            var deviceRequests = new List<long>();

            try
            {
                string sqlQuery = "SELECT deviceId FROM csp.devicetransfers d WHERE d.BulkDeviceTransferId=@p_TransferRequestId;";
                
                string connString = ConfigurationManager.ConnectionStrings["CspDatabase"].ToString();
                // retrieve all devices for the transfer request
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);
                    Connection.AddInParameter(command, "@p_TransferRequestId", DbType.String, transferRequestId);
                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (dataReader["deviceId"] != null)
                                deviceRequests.Add(Convert.ToInt32(dataReader["deviceId"].ToString()));
                        }
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deviceRequests;
        }

        public bool BackupBulkTransferDevices(long transferRequestId)
        {
            var deviceRequests = new List<long>();
            string backupReason = "Bulk Device Transfer Request Id:" + transferRequestId;
            const string backupBy = "donotreplycsp@paloaltonetworks.com";
            var isSuccess = false;
            try
            {
								string sqlQuery = "SELECT deviceId FROM csp.devicetransfers d WHERE d.BulkDeviceTransferId=@p_TransferRequestId;";
                string connString = Secrets.CspDatabase;

                // retrieve all devices for the transfer request
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);

										Connection.AddInParameter(command, "@p_TransferRequestId", DbType.Int64, transferRequestId);

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            if (dataReader["deviceId"] != null)
                                deviceRequests.Add(Convert.ToInt32(dataReader["deviceId"].ToString()));
                        }
                    }

                    connection.Close();
                }

               //Take backup of all devices
                if (deviceRequests.Count > 0)
                {
                    foreach (var deviceId in deviceRequests)
                    {
                        using (var backupcommand = PANDBConnection.GetStoredProcCommand("backup_device_master_by_lid"))
                        {
                            PANDBConnection.AddInParameter(backupcommand, "@mylid", DbType.Int64, deviceId);
                            PANDBConnection.AddInParameter(backupcommand, "@reason", DbType.String, backupReason);
                            PANDBConnection.AddInParameter(backupcommand, "@whoby", DbType.String, backupBy);
                            PANDBConnection.ExecuteNonQuery(backupcommand);
                        }
                    }

                    isSuccess = true;
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return isSuccess;

        }

        public Device GetDeviceDetailsBySerialNumber(string serialNumber)
        {
            var device = new Device();
            using (var command = Connection.GetStoredProcCommand("GetDeviceBySerialNumber"))
            {
                Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        device = new Device
                        {
                            Id = Convert.ToInt64(dataReader["DeviceId"]),
                            SerialNumber = Convert.ToString(dataReader["SerialNumber"]),
                            DeviceName = Convert.ToString(dataReader["DeviceName"]),
                            RegistrationDate = Convert.ToDateTime(dataReader["RegistrationDate"]),
                            IsRMAReturn = Convert.ToBoolean(dataReader["IsRMAReturn"]),
                            IsActive = Convert.ToBoolean(dataReader["IsActive"]),
                            SalesForceAccountId = dataReader["SalesForceAccountId"].ToString(),
                            Address1 = dataReader["Address1"].ToString(),
                            Address2 = dataReader["Address2"].ToString(),
                            City = dataReader["City"].ToString(),
                            Region = dataReader["Region"].ToString(),
                            PostalCode = dataReader["PostalCode"].ToString(),
                            CountryName = dataReader["Country"].ToString()
                        };

                    }
                }
            }

            return device;
        }


        public string GetUserPreferences(long userAccountId, long supportAccountId)
        {
            try
            {
                const string sqlQuery = " SELECT preferences FROM userpreferences p WHERE p.useraccountid=@p_UserAccountId ;";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);

                    var hideLicenses = Connection.ExecuteScalar(command);
                    return (Convert.ToString(hideLicenses));
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return string.Empty;
            }
        }


        public List<DeviceTransferItem> GetEligibleDevicesForTransfer(long supportAccountId, long userAccountId)
        {
            var deviceList = new List<DeviceTransferItem>();

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetEligibleDevicesForTransfer"))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {

                        while (dataReader.Read())
                        {
                            var device = new DeviceTransferItem()
                            {
                              
                                    SerialNumber = Convert.ToString(dataReader["SerialNumber"]),
                                    ModelName = Convert.ToString(dataReader["ModelName"]),
                                    DeviceName = Convert.ToString(dataReader["DeviceName"]),
                                    Group = Convert.ToString(dataReader["Group"]),
                                    DeviceTag = Convert.ToString(dataReader["DeviceCategoryName"]),
                                    DeviceId = Convert.ToInt64(dataReader["DeviceId"]),
                                    VmAuthcode = Convert.ToString(dataReader["VM Authcode"]),
                                    IsVM = Convert.ToBoolean(dataReader["IsVM"]),
                                    IsEA = Convert.ToBoolean(dataReader["IsEA"])
                             };

                                deviceList.Add(device);
                            }
                       
                        }
                    }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return deviceList;
        }

        public List<NameValueOnly> GetDestinationAccounts(long userAccountId,long loggedInAccount)
        {
            var accountList = new List<NameValueOnly>();

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetDestinationAccountsForBulkTransfer"))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, loggedInAccount);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {

                        while (dataReader.Read())
                        {
                            var account = new NameValueOnly()
                            {
                                Name = Convert.ToString(dataReader["Name"]),
                                Value = Convert.ToString(dataReader["SupportAccountId"]),
                                IsEA = Convert.ToBoolean(dataReader["isEA"])
                            };

                            accountList.Add(account);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }



            return accountList;
        }

        public bool CreateBulkTransferRequest(BulkDeviceTransferModel requestData)
        {
            bool isSuccess = false;
            int bulkTransferRequestId = 0;

            try
            {
                if (requestData != null)
                {
                    using (var command = Connection.GetStoredProcCommand("CreateBulkDeviceTransferRequest"))
                    {
                        Connection.AddInParameter(command, "@p_SourceSupportAccountId", DbType.Int64, requestData.SourceSupportAccountId);
                        Connection.AddInParameter(command, "@p_DestinationSupportAccountId", DbType.Int64, requestData.DestinationSupportAccountId);
                        Connection.AddInParameter(command, "@p_DeviceTransferStateId", DbType.Int64, 1);
                        Connection.AddInParameter(command, "@p_TransferredByUserAccountId", DbType.Int64, requestData.TransferredById);
                        Connection.AddOutParameter(command, "@p_bulkRequestId", DbType.Int64, Int32.MaxValue);
                        Connection.ExecuteNonQuery(command);

                        if (command.Parameters["@p_bulkRequestId"].Value != DBNull.Value)
                        {
                            bulkTransferRequestId = Convert.ToInt32(command.Parameters["@p_bulkRequestId"].Value);
                        }
                    }

                    if (bulkTransferRequestId > 0)
                        {
                            string connString = Secrets.CspDatabase;

                               var strDevices = string.Join(",", requestData.BulkTransferDevices);
                               string insertDevicesQuery =
                                    "INSERT INTO devicetransfers (DeviceId,DestinationUserAccountId,DeviceTransferStateId,CreatedBy,CreatedOn,BulkDeviceTransferId) " +
                                    "SELECT d.DeviceId,?p_TransferredByUserAccountId,1,?p_TransferredByUserAccountId,CURRENT_TIMESTAMP(),?p_bulktransferId  FROM csp.device d WHERE d.DeviceId IN (" + strDevices + ")";

                               
                               var strVMs = string.Join(",", requestData.BulkTransferVMs);
                               string insertVmQuery = "INSERT INTO devicetransfers (DeviceId,DestinationUserAccountId,DeviceTransferStateId,CreatedBy,CreatedOn,BulkDeviceTransferId) " +
                                                       " SELECT DISTINCT lf.DeviceId,?p_TransferredByUserAccountId,1,?p_TransferredByUserAccountId,CURRENT_TIMESTAMP(),?p_bulktransferId FROM license lfp JOIN license lf ON lfp.LicenseId=lf.VMParentLicenseId " +
                                                       " WHERE lfp.AuthorizationCode IN (" + strVMs + ")" +
                                                       " UNION " +
                                                       " SELECT DISTINCT lfp.DeviceId,?p_TransferredByUserAccountId,1,?p_TransferredByUserAccountId,CURRENT_TIMESTAMP(),?p_bulktransferId FROM license lfp WHERE lfp.AuthorizationCode IN (" + strVMs + ");";

                           

                            using (var conn = new MySqlConnection(connString))
                            {
                                conn.Open();
                                if (requestData.BulkTransferDevices.Any())
                                {
                                    var deviceCommand = new MySqlCommand(insertDevicesQuery, conn);
                                    deviceCommand.Parameters.AddWithValue("p_DestinationSupportAccountId",requestData.DestinationSupportAccountId);
                                    deviceCommand.Parameters.AddWithValue("p_TransferredByUserAccountId",requestData.TransferredById);
                                    deviceCommand.Parameters.AddWithValue("p_bulktransferId", bulkTransferRequestId);
                                    deviceCommand.ExecuteNonQuery();
                                }

                                if (requestData.BulkTransferVMs.Any())
                                {
                                    var vmCommand = new MySqlCommand(insertVmQuery, conn);
                                    vmCommand.Parameters.AddWithValue("p_DestinationSupportAccountId", requestData.DestinationSupportAccountId);
                                    vmCommand.Parameters.AddWithValue("p_TransferredByUserAccountId", requestData.TransferredById);
                                    vmCommand.Parameters.AddWithValue("p_bulktransferId", bulkTransferRequestId);
                                    vmCommand.ExecuteNonQuery();
                                }

                               conn.Close();
                            }

                        isSuccess = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                isSuccess = false;
            }

            return isSuccess;

        }


        public void UpdateHideExpiredLicensesFlag(long userAccountId, long supportAccountId, string preferences)
        {
            try
            {

                using (var command = Connection.GetStoredProcCommand("UpdateUserPreferences"))
                {
                    Connection.AddInParameter(command, "p_userAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "p_preferences", DbType.String, preferences);
                    Connection.ExecuteNonQuery(command);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        #endregion
        
        public bool CheckIfDeviceHasEmergencyLicenses(long deviceId)
        {
            string sqlQuery = " SELECT count(*) 'eCount' FROM csp.license l WHERE l.DeviceId = @p_DeviceId AND l.AuthorizationCode LIKE 'E%' AND l.IsActive=1; ";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    var device = new DeviceWithPartMaster();

                    if (dataReader.Read())
                    {
                        return Convert.ToInt32(dataReader["eCount"]) > 0;
                    }
                }
            }

            return false;
        }


        #region Device Registration - Day One Config


        public List<NameValueOnly> GetDeviceTypes()
        {
            List<NameValueOnly> deviceTypes = new List<NameValueOnly>();
            var arr = Array.ConvertAll((int[])Enum.GetValues(typeof(DeviceType)), Convert.ToInt32).ToList();
            string str = string.Join(",", arr);            
           
            try
            {
                string sqlQuery = string.Format(
                    @"SELECT c.CloudTypeId,c.Name FROM csp.cloudtype c where c.CloudTypeId IN ({0}) ORDER BY c.Name ;"
                    , string.Join(", ", str.ToString()));
               
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var deviceType = new NameValueOnly()
                            {
                                Name = Convert.ToString(dataReader["Name"]),
                                Value = Convert.ToString(dataReader["CloudTypeId"])
                            };

                            deviceTypes.Add(deviceType);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return deviceTypes;
        }

        public List<NameValueOnly> GetPanOs()
        {
            List<NameValueOnly> OsVersions = new List<NameValueOnly>();
            try
            {
                const string sqlQuery = @"SELECT p.panosversion FROM csp.panos p ORDER BY CAST(p.panosversion AS SIGNED) desc ;";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var version = new NameValueOnly()
                            {
                                Name = Convert.ToString(dataReader["panosversion"]),
                                Value = Convert.ToString(dataReader["panosversion"])
                            };

                            OsVersions.Add(version);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return OsVersions;
        }

        public List<NameValueOnly> GetHyperVisors()
        {
            List<NameValueOnly> hyperVisors = new List<NameValueOnly>();
            try
            {
                const string sqlQuery = @"SELECT h.displayvalue,h.hypervisorversion FROM csp.hypervisor h ORDER BY h.displayvalue;";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var hypervisorItem = new NameValueOnly()
                            {
                                Name = Convert.ToString(dataReader["displayvalue"]),
                                Value = Convert.ToString(dataReader["hypervisorversion"])
                            };

                            hyperVisors.Add(hypervisorItem);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return hyperVisors;
        }

        public bool IsChassisSerialNumber(string serialNumber)
        {

            try
            {
                using (var command = Connection.GetStoredProcCommand("IsValidChassisSerialNumber"))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["IsValid"]);
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new Exception(ex.Message.ToString());
            }
        }

        public long[] GetDeviceSalesOrderSupportAccountId(string serialNumber)
        {
           try
            {
                const string sqlQuery = @"SELECT map.SupportAccountId
                from  csp.salesorderitem soitem
                    join csp.salesorder so on soitem.SalesOrderId = so.SalesOrderId
                join  csp.sapsupportaccountmapping map on so.SAPCustomerId = map.SAPId
                JOIN csp.device d ON soitem.DeviceId=d.DeviceId
                where d.SerialNumber = @p_SerialNumber";

                List<long> supportAccountIds= new List<long>();

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            supportAccountIds.Add(Convert.ToInt64(dataReader["SupportAccountId"]));
                        }

                        return supportAccountIds.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new long[0];
            }
        }

        public bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("HasBklnSupportInSalesOrder"))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "@p_ASCSupportAccountId", DbType.String, supportCenterSupportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                           return Convert.ToBoolean(dataReader["HasBackline"]);
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public string GetCountryCodeById(long countryId)
        {
            string countryCode = string.Empty;
            try
            {
                const string sqlQuery = @"SELECT c.Code FROM csp.country c WHERE c.CountryId= @p_CountryId";

                
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_CountryId", DbType.Int64, countryId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            countryCode = Convert.ToString(dataReader["Code"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return countryCode;
        }

        #endregion

        #region Thermite OTP Changes

        public bool IsValidDeviceForOtp(string serialNumber, long supportAccountId)
        {
            bool isValidDevice = false;
            try
            {
                const string sqlQuery = @"SELECT 1 FROM device d WHERE d.SerialNumber = @p_SerialNumber AND d.SupportAccountId= @p_SupportAccountId";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            isValidDevice = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return isValidDevice;
        }

        #endregion

        #region Activation link members

        public IEnumerable<ActivationLinkModel> GetTrialLicenseActivationActiveLinksBySupportAccountId(long supportAccountId, string type, string productFamily, int duration, out string sfAccount)
        {
            // CSP-3027 CSP Initiated 30 day IOT trial radial button
            // get all unexpired and unactivated registered Activation Links
            const string sqlQuery = @"
SELECT DISTINCT i.licenseid, l.AuthorizationCode, l.ExpirationDate LicenseExpiration, i.subgroup LinkType, o.license_hash ActivationLink, DATE_ADD(a.createdon, INTERVAL @p_duration DAY) ActivationLinkExpiration
  FROM supportaccount s
  JOIN assetactivation a ON s.SalesforceAccountId = a.salesforceaccountid
  JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
  JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
  JOIN license l ON i.licenseid = l.LicenseId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  WHERE s.SupportAccountId = @p_supportAccountId
  AND p.PartNumber LIKE '%-TRIAL%'
  AND o.product = @p_type
  AND i.subgroup = @p_type
  AND a.createdon IS NOT NULL
  AND a.createdon >= DATE_ADD(NOW(), INTERVAL -@p_duration DAY)
  AND l.LicenseId NOT IN 
  (
    -- exclude activated licenses
    SELECT DISTINCT l2.LicenseId
      FROM supportaccount s2
      JOIN supportaccount f2 ON s2.SalesforceAccountId = f2.SalesforceAccountId
      JOIN license l2 ON f2.SupportAccountId = l2.SupportAccountId
      JOIN partmaster p2 ON l2.PartMasterId = p2.PartMasterId
      WHERE s2.SupportAccountId = @p_supportAccountId
      AND p2.PartNumber LIKE '%-TRIAL%'
      AND p2.ProductFamily = @p_productFamily
      AND l2.ExpirationDate IS NOT NULL
      AND l2.ExpirationDate >= NOW()
      AND l2.IsActive
  )
  ORDER BY l.CreatedOn DESC;
";
            List<ActivationLinkModel> result = new List<ActivationLinkModel>();
            using (var command = Connection.GetStoredProcCommand(sqlQuery))
            {
                command.CommandType = CommandType.Text;
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_type", DbType.String, type);
                Connection.AddInParameter(command, "@p_productFamily", DbType.String, productFamily);
                Connection.AddInParameter(command, "@p_duration", DbType.Int32, duration);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        try
                        {
                            result.Add(new ActivationLinkModel
                            {
                                LicenseId = Convert.ToInt64(dataReader["licenseid"]), // required
                                AuthorizationCode = dataReader["AuthorizationCode"].ToString(), // required
                                LicenseExpiration = dataReader["LicenseExpiration"] != DBNull.Value ? Convert.ToDateTime(dataReader["LicenseExpiration"]) : (DateTime?)null, // expected NULL in case of unactivated link
                                LinkType = dataReader["LinkType"].ToString(), // required
                                ActivationLink = TrialIoTApiActiveLinkPrefix + dataReader["ActivationLink"].ToString(), // required
                                ActivationLinkExpiration = dataReader["ActivationLinkExpiration"] != DBNull.Value ? Convert.ToDateTime(dataReader["ActivationLinkExpiration"]) : (DateTime?)null // can be NULL?
                            });
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                    }
                }
            }

            const string sqlQueryAccount = @"
SELECT s.SalesforceAccountId FROM supportaccount s WHERE s.SupportAccountId = @p_supportAccountId;
";
            using (var command = Connection.GetStoredProcCommand(sqlQueryAccount))
            {
                command.CommandType = CommandType.Text;
                Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        sfAccount = dataReader["SalesforceAccountId"].ToString();
                    }
                    else
                    {
                        sfAccount = string.Empty;
                    }
                }
            }
            
            return result;
        }

        #endregion

        public long GetDlpTenantIdBySupportAccountId(long supportAccountId)
        {
            long dlpTenantId = 0;
            try
            {
                const string sqlQuery = @"select t.TenantId from tenantlookup t
                                    join appportalapps a on t.AppPortalAppsId = a.AppPortalAppsId
                                    where a.AppInternalName = 'dlp' and t.SupportAccountId = @p_SupportAccountId and t.isActive = 1;";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);

                    using (IDataReader dataReader = Connection.ExecuteReader(command))
                    {
                        dataReader.Read();

                        dlpTenantId = Convert.ToInt64(dataReader["TenantId"]);

                        return dlpTenantId;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }  

             return dlpTenantId;    
        }
        public bool IsNgfwDlpDevice(long deviceId)
        {
            bool IsNgfwDlpDevice = false;
            try
            {
                const string sqlQuery = @"SELECT count(1) IsNgfwDlpDevice
                                        FROM device d 
                                        JOIN license l ON d.DeviceId = l.DeviceId
                                        JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId                      
                                        JOIN partsubtype ps ON pm.PartSubtypeId = ps.PartSubtypeId
                                        WHERE ps.PanDbIdentification LIKE 'NGFWDLP%'
                                        AND d.DeviceId = @p_DeviceId
                                        AND d.IsActive = 1
                                        AND l.ExpirationDate IS NOT NULL
                                        AND l.ExpirationDate >= NOW()
                                        AND l.IsActive = 1
                                        LIMIT 1";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["IsNgfwDlpDevice"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }           

            return IsNgfwDlpDevice;
        }

        public bool IsNgfwDlpDevice(string serialNumber)
        {
            bool IsNgfwDlpDevice = false;
            try
            {
                const string sqlQuery = @"
SELECT count(1) > 0 IsNgfwDlpDevice
    FROM device d 
    JOIN license l ON d.DeviceId = l.DeviceId
    JOIN partmaster pm ON d.PartMasterId = pm.PartMasterId                      
    JOIN partsubtype ps ON pm.PartSubtypeId = ps.PartSubtypeId
    WHERE d.SerialNumber = @p_SerialNumber
    AND d.IsActive = 1
    AND l.IsActive = 1
    AND ps.PanDbIdentification LIKE '%NFRD%'
    AND l.ExpirationDate IS NOT NULL
    AND l.ExpirationDate >= NOW();";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return Convert.ToBoolean(dataReader["IsNgfwDlpDevice"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return IsNgfwDlpDevice;
        }

        public void LogGrantDeviceInfo(long SupportAccountId, string SerialNumber, string MethodName, string ErrorMessage, string LogType, long? UserAccountId = null)
        {
            
                const string sqlQuery = @"
                                    INSERT INTO loggrantdevicereginfo (
                                    SupportAccountId,
			                        SerialNumber,
			                        MethodName,
			                        ErrorMessage,
			                        LogType,
			                        CreatedByUserAccountId,
			                        CreatedOn )
                                     VALUES
                                    (
                                      @p_SupportAccountId
                                     ,@p_SerialNumber
                                     ,@p_MethodName
                                     ,@p_ErrorMessage
                                     ,@p_LogType
                                     ,@p_CreatedByUserAccountId									 
                                     ,@p_CreatedOn
                                    );";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, SupportAccountId);
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, SerialNumber);
                    Connection.AddInParameter(command, "@p_MethodName", DbType.String, MethodName);
                    Connection.AddInParameter(command, "@p_ErrorMessage", DbType.String, ErrorMessage);
                    Connection.AddInParameter(command, "@p_LogType", DbType.String, LogType);
                    Connection.AddInParameter(command, "@p_CreatedByUserAccountId", DbType.Int64, UserAccountId);
                    Connection.AddInParameter(command, "@p_CreatedOn", DbType.DateTime, DateTime.Now);
                    Connection.ExecuteNonQuery(command);
                }           
        }

        public string GetCountryNameByCode(string countryCode)
    
        {
            string countryname = string.Empty;
            try
            {
                const string sqlQuery = @"SELECT c.Name FROM csp.country c WHERE c.Code = @p_CountryCode";


                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_CountryCode", DbType.String, countryCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            countryname = Convert.ToString(dataReader["Name"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return countryname;
        }

        public void EaEsaDeviceInsert(long supportAccountId, string partId, string serialNumber, string createdBy)
        {
            using (var command = PANDBConnection.GetStoredProcCommand("Insert_eaEsaDevice"))
            {
                PANDBConnection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                PANDBConnection.AddInParameter(command, "@p_partId", DbType.String, partId);
                PANDBConnection.AddInParameter(command, "@p_serialnum", DbType.String, serialNumber);
                PANDBConnection.AddInParameter(command, "@p_createdBy", DbType.String, createdBy);

                PANDBConnection.ExecuteNonQuery(command);
            }
        }

        public string GetModelNameBySerialNumber(string serialNumber)
        {
            string modelName = string.Empty;
            try
            {
                const string sqlQuery = @"SELECT p.PartNumber AS ModelName 
                                          FROM device d 
                                          JOIN partmaster p ON d.PartMasterId = p.PartMasterId 
                                          WHERE d.SerialNumber = @p_SerialNumber";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            modelName = Convert.ToString(dataReader["ModelName"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return modelName;
        }
    }
}
