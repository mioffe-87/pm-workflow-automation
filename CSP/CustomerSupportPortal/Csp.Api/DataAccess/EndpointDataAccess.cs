using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Api.Logic;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class EndpointDataAccess : BaseDataAccess, IEPDataAccessLogic
    {
        private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly IDeviceRepository _deviceRepository;

        public EndpointDataAccess(ISupportAccountRepository supportAccountRepository, IDeviceRepository deviceRepository)
        {
            _supportAccountRepository = supportAccountRepository;
            _deviceRepository = deviceRepository;
        }

        public bool IsAuthCodeAlreadyRegistered(string authCode, string epType)
        {
            const string sqlQuery = @"
SELECT l.LicenseId
FROM license l
WHERE 
(l.SupportAccountId is not null OR l.DeviceId is not null)
AND l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

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

        public bool IsEpLicense(string authCode, string epType)
        {
	        string sqlQuery = "";
	        switch (epType)
	        {
						case EndpointConstants.Esm:
							sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype elt on pst.pandbidentification = elt.licensetype
AND l.AuthorizationCode = @p_AuthCode AND elt.isesm=1;";
			        break;
						case EndpointConstants.Workstation:
							sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype elt on pst.pandbidentification = elt.licensetype
AND l.AuthorizationCode = @p_AuthCode AND elt.isworkstation=1;";
			        break;
						case EndpointConstants.Server:
							sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype elt on pst.pandbidentification = elt.licensetype
AND l.AuthorizationCode = @p_AuthCode AND elt.isserver=1;";
							break;
	        }
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                var isendpoint = Connection.ExecuteScalar(command);
								if (isendpoint != null && isendpoint != DBNull.Value)
								{
									return true;
								}
            }
            return false;
        }

        public bool IsEpLicenseEsmOrBundle(string authCode)
        {
            bool isEpLicense = false;
            const string sqlQuery = @"
SELECT count(1)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointstubs eps on pst.pandbidentification = eps.PandbIdentification
AND l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                var found = Connection.ExecuteScalar(command);
                
                if (found != null)
                {
                    isEpLicense = true;
                }

            }
            return isEpLicense;
        }

        public bool IsEpLicense(string authCode)
        {
            const string sqlQuery = @"
SELECT eplt.isesm,eplt.isworkstation,eplt.isserver
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

								using (var dataReader = Connection.ExecuteReader(command)) {
									if (dataReader.Read()) {

										if (
												(dataReader["isesm"] != null && dataReader["isesm"] != DBNull.Value && dataReader["isesm"].ToString() == "1") || 
												(dataReader["isworkstation"] != null && dataReader["isworkstation"] != DBNull.Value && dataReader["isworkstation"].ToString() == "1") || 
												((dataReader["isserver"] != null && dataReader["isserver"] != DBNull.Value && dataReader["isserver"].ToString() == "1"))
											) {
											return true;
										}
									}
									return false;
								}
            }
        }

        private string GetNewStubSn()
        {
            string sqlQuery = @"
SELECT val1 FROM lookups l WHERE l.fld='nextserialnumber' AND l.key='endpoint'";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                int iNew = Convert.ToInt32(PANDBConnection.ExecuteScalar(command)) + 1;
                if (UpdateNextSerialNumber(iNew))
                {
                    string newSerialNum = "012102" + iNew.ToString("000000");
                    return newSerialNum;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private bool UpdateNextSerialNumber(int iNew)
        {
            string sqlQuery = "UPDATE lookups l SET l.val1=" + iNew +
                              " WHERE l.fld='nextserialnumber' AND l.key='endpoint'";
            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                return PANDBConnection.ExecuteNonQuery(command) > 0 ? true : false;
            }
        }

        private DateTime? GetShipDate(string authCode)
        {
            const string sqlQuery = @"
SELECT
  l.ShipDate
FROM license l
WHERE l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (!dataReader.Read())
                    {
                        return null;
                    }
                    else
                    {
                        var shipDate = Convert.ToDateTime(dataReader["ShipDate"]);

                        return shipDate;
                    }
                }
            }
        }

        public DateTime? GetExpiryDateForEpLicenseAuthCode(string authCode)
        {
            const string sqlQuery = @"
SELECT
  l.ShipDate,
  l.GracePeriodDays,
  l.LicenseTermMonths
FROM license l
WHERE l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        DateTime shipDate = Convert.ToDateTime(dataReader["ShipDate"]);
                        int graceDays = Convert.ToInt32(dataReader["GracePeriodDays"]);
                        int termMonths = Convert.ToInt32(dataReader["LicenseTermMonths"]);
                        if (graceDays == 0 && termMonths > 0)
                        {
                            return shipDate.AddMonths(termMonths);
                        }
                        if (termMonths == 0 && graceDays > 0)
                        {
                            return shipDate.AddDays(graceDays);
                        }
                        return shipDate;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public bool CreateStubDevice(string authCode, long supportAccountId, out long deviceId, out string serialNum)
        {
            deviceId = -1;
            serialNum = string.Empty;
            try
            {
                serialNum = GetNewStubSn();
                if (serialNum == string.Empty)
                    return false;

                DateTime? shipDate = GetShipDate(authCode);

                if (!shipDate.HasValue)
                {
                    return false;
                }
                string stubPartId;
                string licType = GetEndPointLicenseType(authCode, out stubPartId);
                CreateDevice(serialNum,stubPartId, supportAccountId, shipDate.Value, licType);
                deviceId = _deviceRepository.ReadDeviceIdBySerialNumber(serialNum);
                /*
                _supportAccountRepository.UpdateDeviceSupportAccountInCsp(stubDeviceId, supportAccountId);
                SaveStubDeviceIdToLicense(authCode, stubDeviceId, supportAccountId);
                 */
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public long GetDeviceIdBySerialNumber(string serialNumber)
        {
            return _deviceRepository.ReadDeviceIdBySerialNumber(serialNumber);
        }

        // Comments By LK On 05/10/2018
        // This method is still being referenced by CreateStubDevice. Please clean-up the code before marking it obsolete
        //[Obsolete("I don't believe this code is being used.  If so, please replace the GuessUserAccountId method with the actual usera ccount it")]
        private void CreateDevice(string newStubSn, string partId, long supportAccountId, DateTime shipDate, string licType)
        {

            var userAccountId = _supportAccountRepository.GuessUserAccountIdBySupportAccountId(supportAccountId);

            const string sqlQuery = @"
INSERT INTO device_master 
            (serialnum, 
             acctid, 
             isvm, 
             part_id, 
             ship_date, 
             regdate, 
             ispublic) 
VALUES      (@p_SerialNumber, 
             @p_AcctId, 
             1, 
             @p_PartId, 
             @p_ShipDate, 
             CURRENT_TIMESTAMP, 
             1); ";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_SerialNumber", DbType.String, newStubSn);
                PANDBConnection.AddInParameter(command, "@p_AcctId", DbType.Int32, userAccountId);
                PANDBConnection.AddInParameter(command, "@p_PartId", DbType.String, partId);
                PANDBConnection.AddInParameter(command, "@p_ShipDate", DbType.DateTime, shipDate);
                PANDBConnection.ExecuteNonQuery(command);
            }

            //Update the supportAccountId by serial number because the trigger can't do this
            _deviceRepository.UpdateSupportAccountIdBySerialNumber(newStubSn, supportAccountId);
        }

        public void UpdateSupportAccountIdBySerialNumber(string serialNum, long supAccountId, long userAccountId)
        {
            //Update the supportAccountId by serial number because the trigger can't do this
            _deviceRepository.UpdateSupportAccountIdBySerialNumber(serialNum, supAccountId);
        }

        public void GetInfoToCreateStub(string authCode, out string licType,out string stubPartId, out string serialNum, out DateTime? shipDate)
        {
            serialNum = GetNewStubSn();
            licType = GetEndPointLicenseType(authCode, out stubPartId);
            shipDate = GetShipDateofEpAuthCode(authCode);
        }

        public DateTime? GetShipDateofEpAuthCode(string authCode)
        {
            const string sqlQuery = @"
SELECT ShipDate
FROM license Where AuthorizationCode = @p_AuthCode";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                var shipDate = Connection.ExecuteScalar(command);
                if (shipDate != null && shipDate != DBNull.Value)
                {
                    return Convert.ToDateTime(shipDate);
                }
            }
            return null;
        }

        public bool SaveStubDeviceIdToLicense(string authCode, long stubDeviceId, long supportAccountId,
                                              DateTime expiryDate)
        {
            const string sqlQuery = @"
update lic_features
set devID=@p_devId, expiration = @p_expirationDate
where auth_code = @p_AuthCode";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                PANDBConnection.AddInParameter(command, "@p_devId", DbType.Int64, stubDeviceId);
                PANDBConnection.AddInParameter(command, "@p_expirationDate", DbType.DateTime, expiryDate);

                PANDBConnection.ExecuteNonQuery(command);
            }

            SaveSupportAccountIdToCspLicense(authCode, supportAccountId);

            return true;
        }

        public bool SaveStubDeviceIdToLicense(string authCode, long stubDeviceId, long supportAccountId)
        {
            const string sqlQuery = @"
update lic_features
set devID=@p_devId
where auth_code = @p_AuthCode";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                PANDBConnection.AddInParameter(command, "@p_devId", DbType.Int64, stubDeviceId);

                PANDBConnection.ExecuteNonQuery(command);
            }

            SaveSupportAccountIdToCspLicense(authCode, supportAccountId);

            return true;
        }

        private void SaveSupportAccountIdToCspLicense(string authCode, long supportAccountId)
        {
            const string sqlQuery = @"
UPDATE license l
SET SupportAccountId = @p_SupportAccountId
WHERE AuthorizationCode = @p_AuthCode";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);

                Connection.ExecuteNonQuery(command);
            }
        }
				/*
        public string GetEpLicenseType(string authCode)
        {
            const string sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.AuthorizationCode = @p_AuthCode;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                var LicType = Connection.ExecuteScalar(command).ToString();
                if (LicType == EndpointLicenseType.EpSms || LicType == EndpointLicenseType.Ep100 ||
                    LicType == EndpointLicenseType.Ep500)
                {
                    return LicType;
                }
            }
            return string.Empty;
        }
			


        public long GetStubDeviceId(long supportAccountId)
        {
            //Get the AuthCode associated with this supportaccountid with part sub type as ESM
            const string sqlQuery = @"
SELECT DeviceId
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
WHERE pst.pandbidentification=@p_epType
AND l.SupportAccountId = @p_SupportAccountId;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_epType", DbType.String, EndpointLicenseType.EpSms);
                var deviceId = Connection.ExecuteScalar(command);
                if (deviceId != null)
                {
                    return Convert.ToInt64(deviceId);
                }
            }
            return -1;
        }				 
				 */

        public long GetStubDeviceId(long supportAccountId, string esmAuthCode, out string serialNumber)
        {
            serialNumber = string.Empty;
            long deviceId = -1;
            //Get the AuthCode associated with this supportaccountid with part sub type as ESM
            const string sqlQuery = @"
SELECT d.deviceId,d.serialnumber 
FROM device d 
inner join license l on d.deviceid = l.deviceid
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointstubs eps on pst.pandbidentification = eps.PandbIdentification
AND l.SupportAccountId = @p_SupportAccountId AND l.AuthorizationCode= @p_AuthCode;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, esmAuthCode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        deviceId = dataReader["deviceId"] == null ? -1 : Convert.ToInt64(dataReader["deviceId"]);
                        serialNumber = Convert.ToString(dataReader["SerialNumber"]);
                    }
                }
            }
            return deviceId;
        }

			/*
        public AvailableEp GetEndPointInfo(string authCode)
        {
            AvailableEp lic = new AvailableEp();
            const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate, pst.PanDbIdentification
FROM Device d
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
WHERE pst.pandbidentification=@p_epType
AND l.AuthorizationCode= @p_AuthCode;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_epType", DbType.String, EndpointLicenseType.EpSms);
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        lic.SerialNumber = dataReader["SerialNumber"].ToString();
                        lic.AuthorizationCode = dataReader["AuthorizationCode"].ToString();
                        lic.Description = dataReader["Description"].ToString();
                    }
                }
            }
            return lic;
        }
			 * */

        /*
        public string GetEndPointLicenseType(string authCode)
        {
            const string sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.AuthorizationCode = @p_AuthCode and pm.modelcompatibility like @p_mc;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%",EndpointConstants.ModelCompat));
                var LicType = Connection.ExecuteScalar(command);
                if (LicType != null)
                {
                    return LicType.ToString();
                }
            }
            return null;
        }
         */

        public bool IsAuthCodeAlreadyRegistered(string authCode, long supportId)
        {
            const string sqlQuery = @"
SELECT licenseId
FROM license l
WHERE l.AuthorizationCode = @p_AuthCode AND l.SupportAccountId= @p_SupportId;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportId);
                var license = Connection.ExecuteScalar(command);
                return license != null;
            }
        }

        public bool IsEpLicenseTypeAlreadyRegistered(long supportAccountId, string licType)
        {
            const string sqlQuery = @"
SELECT licenseId
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.SupportAccountId = @p_SupportId AND pst.PanDbIdentification = @p_licType;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_licType", DbType.String, licType);
                var license = Connection.ExecuteScalar(command);
                return license != null;
            }
        }

				public IEnumerable<AvailableEp> GetAvailableEps(long supportAccountId, string searchField, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
            {
							return GetAvailableEps(supportAccountId);
            }
            switch (searchField)
            {
                case "SerialNumber":
                    return GetAvailableEpsBySerialNumber(supportAccountId, searchValue);
                case "DeviceName":
										return GetAvailableEpsByDeviceName(supportAccountId, searchValue);
                case "DeviceCategoryId":
                    long categoryId;
                    if (long.TryParse(searchValue, out categoryId))
                    {
                        return GetAvailableEpsByDeviceTag(supportAccountId, categoryId);
                    }
                    return new List<AvailableEp>();
                case "AuthCode":
                    long? deviceId = GetDeviceIdForEpAuthCode(searchValue);
                    if (deviceId.HasValue)
                    {
											return GetAvailableEpsByDeviceId(supportAccountId, deviceId.Value);
                    }
                    return new List<AvailableEp>();
                default:
                    return GetAvailableEps(supportAccountId);
            }
        }

				private IEnumerable<AvailableEp> GetAvailableEpsByDeviceTag(long supportAccountId, long categoryId)
        {
            try
            {
                const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate,l.expirationdate, pst.PanDbIdentification, pst.LicenseFeaturesName,pst.Name as PartSubTypeName, d.DeviceName, dc.DeviceCategoryName ,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport,ifnull(eplt.iswarranty,0) as iswarranty  
FROM Device d
Left Outer Join DeviceCategory dc on d.DeviceCategoryId = dc.DeviceCategoryId
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
where d.SupportAccountId = @p_SupportId && l.isactive=1 && d.DeviceCategoryId = @p_DeviceCategoryId && pm.ModelCompatibility like @p_mc Order By d.DeviceId,eplt.sortindex;";
                var epLicenses = new List<AvailableEp>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%",EndpointConstants.ModelCompat));
                    Connection.AddInParameter(command, "@p_DeviceCategoryId", DbType.Int64, categoryId);
                    Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var epLic = new AvailableEp()
                            {
                                SerialNumber = dataReader["SerialNumber"].ToString(),
                                AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                                Description = dataReader["Description"].ToString(),
                                UseCount =
                                    Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]),
                                DeviceId =
                                    dataReader["DeviceId"] == null
                                        ? (long?)null
                                        : Convert.ToInt64(dataReader["DeviceId"]),
                                RegistrationDate =
                                    dataReader["RegistrationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["RegistrationDate"]),
                                ExpirationDate =
                                    dataReader["ExpirationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["ExpirationDate"]),
                                PartSubType = dataReader["PanDbIdentification"].ToString(),
                                PartSubTypeName = dataReader["PartSubTypeName"].ToString(),
                                LicenseFeatureName = dataReader["LicenseFeaturesName"].ToString().ToUpper(),
                                DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
																IsEsm = dataReader["isesm"].ToString() == "1",
																IsWorkstation = dataReader["isworkstation"].ToString() == "1",
																IsServer = dataReader["isserver"].ToString() == "1",
																IsSupport = dataReader["issupport"].ToString() == "1"
                            };
                            epLicenses.Add(epLic);
                        }
                    }
                }
								return epLicenses;
            }
            catch (Exception)
            {
                return null;
            }
        }

				private IEnumerable<AvailableEp> GetAvailableEpsByDeviceName(long supportAccountId, string deviceName)
        {
            try
            {
                const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate,l.expirationdate, pst.PanDbIdentification, pst.LicenseFeaturesName,pst.Name as PartSubTypeName, d.DeviceName, dc.DeviceCategoryName ,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport,ifnull(eplt.iswarranty,0) as iswarranty  
FROM Device d
Left Outer Join DeviceCategory dc on d.DeviceCategoryId = dc.DeviceCategoryId
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
WHERE d.SupportAccountId = @p_SupportId && l.isactive=1 && d.DeviceName = @p_DeviceName && pm.ModelCompatibility like @p_mc Order By d.DeviceId,eplt.sortindex;";
                var epLicenses = new List<AvailableEp>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));
                    Connection.AddInParameter(command, "@p_DeviceName", DbType.String, deviceName);
                    Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var epLic = new AvailableEp()
                            {
                                SerialNumber = dataReader["SerialNumber"].ToString(),
                                AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                                Description = dataReader["Description"].ToString(),
                                UseCount =
                                    Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]),
                                DeviceId =
                                    dataReader["DeviceId"] == null
                                        ? (long?)null
                                        : Convert.ToInt64(dataReader["DeviceId"]),
                                RegistrationDate =
                                    dataReader["RegistrationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["RegistrationDate"]),
                                ExpirationDate =
                                    dataReader["ExpirationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["ExpirationDate"]),
                                PartSubType = dataReader["PanDbIdentification"].ToString(),
                                PartSubTypeName = dataReader["PartSubTypeName"].ToString(),
                                LicenseFeatureName = dataReader["LicenseFeaturesName"].ToString().ToUpper(),
                                DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
																IsEsm = dataReader["isesm"].ToString() == "1",
																IsWorkstation = dataReader["isworkstation"].ToString() == "1",
																IsServer = dataReader["isserver"].ToString() == "1",
																IsSupport = dataReader["issupport"].ToString() == "1"
                            };
                            epLicenses.Add(epLic);
                        }
                    }
                }
                return epLicenses;
            }
            catch (Exception)
            {
                return null;
            }
        }

				private IEnumerable<AvailableEp> GetAvailableEpsBySerialNumber(long supportAccountId, string serialNum)
        {
            try
            {
                const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate,l.expirationdate, pst.PanDbIdentification, pst.LicenseFeaturesName,pst.Name as PartSubTypeName, d.DeviceName, dc.DeviceCategoryName ,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport,ifnull(eplt.iswarranty,0) as iswarranty  
FROM Device d
Left Outer Join DeviceCategory dc on d.DeviceCategoryId = dc.DeviceCategoryId
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
WHERE d.SupportAccountId = @p_SupportId && l.isactive=1 && d.SerialNumber = @p_SerialNum && pm.ModelCompatibility like @p_mc Order By d.DeviceId,eplt.sortindex;";
                var epLicenses = new List<AvailableEp>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));
                    Connection.AddInParameter(command, "@p_SerialNum", DbType.String, serialNum);
                    Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var epLic = new AvailableEp()
                            {
                                SerialNumber = dataReader["SerialNumber"].ToString(),
                                AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                                Description = dataReader["Description"].ToString(),
                                UseCount =
                                    Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]),
                                DeviceId =
                                    dataReader["DeviceId"] == null
                                        ? (long?)null
                                        : Convert.ToInt64(dataReader["DeviceId"]),
                                RegistrationDate =
                                    dataReader["RegistrationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["RegistrationDate"]),
                                ExpirationDate =
                                    dataReader["ExpirationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["ExpirationDate"]),
                                PartSubType = dataReader["PanDbIdentification"].ToString(),
                                PartSubTypeName = dataReader["PartSubTypeName"].ToString(),
                                LicenseFeatureName = dataReader["LicenseFeaturesName"].ToString().ToUpper(),
                                DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
																IsEsm = dataReader["isesm"].ToString() == "1",
																IsWorkstation = dataReader["isworkstation"].ToString() == "1",
																IsServer = dataReader["isserver"].ToString() == "1",
																IsSupport = dataReader["issupport"].ToString() == "1"
                            };
                            epLicenses.Add(epLic);
                        }
                    }
                }
                return epLicenses;
            }
            catch (Exception)
            {
                return null;
            }
        }

				private IEnumerable<AvailableEp> GetAvailableEpsByDeviceId(long supportAccountId, long deviceId)
        {
            try
            {
                const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate,l.expirationdate, pst.PanDbIdentification, pst.LicenseFeaturesName,pst.Name as PartSubTypeName, d.DeviceName, dc.DeviceCategoryName  ,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport,ifnull(eplt.iswarranty,0) as iswarranty  
FROM Device d
Left Outer Join DeviceCategory dc on d.DeviceCategoryId = dc.DeviceCategoryId
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
WHERE d.SupportAccountId = @p_SupportId && l.isactive=1 && d.DeviceId = @p_DeviceId && pm.ModelCompatibility like @p_mc Order By d.DeviceId,eplt.sortindex;";
                var epLicenses = new List<AvailableEp>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var epLic = new AvailableEp()
                            {
                                SerialNumber = dataReader["SerialNumber"].ToString(),
                                AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                                Description = dataReader["Description"].ToString(),
                                UseCount =
                                    Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]),
                                DeviceId =
                                    dataReader["DeviceId"] == null
                                        ? (long?)null
                                        : Convert.ToInt64(dataReader["DeviceId"]),
                                RegistrationDate =
                                    dataReader["RegistrationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["RegistrationDate"]),
                                ExpirationDate =
                                    dataReader["ExpirationDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(dataReader["ExpirationDate"]),
                                PartSubType = dataReader["PanDbIdentification"].ToString(),
                                PartSubTypeName = dataReader["PartSubTypeName"].ToString(),
                                LicenseFeatureName = dataReader["LicenseFeaturesName"].ToString().ToUpper(),
                                DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
																IsEsm = dataReader["isesm"].ToString() == "1",
																IsWorkstation = dataReader["isworkstation"].ToString() == "1",
																IsServer = dataReader["isserver"].ToString() == "1",
																IsSupport = dataReader["issupport"].ToString() == "1"
                            };
                            epLicenses.Add(epLic);
                        }
                    }
                }
                return epLicenses;
            }
            catch (Exception)
            {
                return null;
            }
        }

				public IEnumerable<AvailableEp> GetAvailableEps(long supportAccountId)
        {
            try
            {
                const string sqlQuery = @"
SELECT d.SerialNumber, l.AuthorizationCode, pm.Description, l.VMUseCount, l.DeviceId, d.RegistrationDate,l.expirationdate, pst.PanDbIdentification, pst.LicenseFeaturesName,pst.Name as PartSubTypeName, d.DeviceName, dc.DeviceCategoryName, pst.Name ,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport,ifnull(eplt.iswarranty,0) as iswarranty 
FROM Device d
Left Outer Join DeviceCategory dc on d.DeviceCategoryId = dc.DeviceCategoryId
inner join License l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
WHERE d.SupportAccountId = @p_SupportId && l.isactive=1 && pm.ModelCompatibility like @p_mc  Order By d.DeviceId,eplt.sortindex;";
                var epLicenses = new List<AvailableEp>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));
                    Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var epLic = new AvailableEp()
                                {
                                    SerialNumber = dataReader["SerialNumber"].ToString(),
                                    AuthorizationCode = dataReader["AuthorizationCode"].ToString(),
                                    Description = dataReader["Description"].ToString(),
                                    UseCount =
                                        Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                            ? (int?)null
                                            : Convert.ToInt32(dataReader["VMUseCount"]),
                                    DeviceId =
                                        dataReader["DeviceId"] == null
                                            ? (long?) null
                                            : Convert.ToInt64(dataReader["DeviceId"]),
                                    RegistrationDate =
                                        dataReader["RegistrationDate"] == DBNull.Value
                                            ? (DateTime?) null
                                            : Convert.ToDateTime(dataReader["RegistrationDate"]),
                                    ExpirationDate =
                                        dataReader["ExpirationDate"] == DBNull.Value
                                            ? (DateTime?)null
                                            : Convert.ToDateTime(dataReader["ExpirationDate"]),
                                    PartSubType = dataReader["PanDbIdentification"].ToString(),
                                    PartSubTypeName = dataReader["PartSubTypeName"].ToString(),
                                    LicenseFeatureName = dataReader["LicenseFeaturesName"].ToString().ToUpper(),
                                    DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                    DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
                                    LicenseName = dataReader["Name"].ToString(),
																		IsEsm = dataReader["isesm"].ToString() == "1",
																		IsWorkstation = dataReader["isworkstation"].ToString() == "1",
																		IsServer = dataReader["isserver"].ToString() == "1",
																		IsSupport = dataReader["issupport"].ToString() == "1",
																		IsWarranty = dataReader["iswarranty"].ToString() == "1"
                                };
                            epLicenses.Add(epLic);
                        }
                    }
                }
                return epLicenses;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ConvertSupportNameToFriendlySupportName(string name, string featurename)
        {
            return string.Concat(name, " ", featurename.First().ToString().ToUpper() + featurename.Substring(1).ToLower());
        }

        public DateTime? GetLicenseExpirationDateBySupportAccountId(long supportId)
        {
            const string sqlQuery = @"
SELECT Min(ExpirationDate)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.SupportAccountId = @p_SupportId And l.IsActive=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportId);
                var licExpiry = Connection.ExecuteScalar(command);
                if (licExpiry != null)
                {
                    return Convert.ToDateTime(licExpiry);
                }
                return (DateTime?)null;
            }
        }

        public DateTime? GetLicenseExpirationDateByEpStubId(long deviceId)
        {
            const string sqlQuery = @"
SELECT Min(ExpirationDate)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.DeviceId = @p_DeviceId And l.IsActive=1 And (eplt.isserver=1 or eplt.isworkstation=1 or eplt.isvdi=1 or eplt.isagent=1)";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                var licExpiry = Connection.ExecuteScalar(command);
                if (licExpiry != null && licExpiry !=DBNull.Value)
                {
                    return Convert.ToDateTime(licExpiry);
                }
                return (DateTime?)null;
            }
        }

        public long? GetLicenseCountBySupportAccountId(long supportId)
        {
            const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.SupportAccountId = @p_SupportId And l.IsActive=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportId);
                var licCount = Connection.ExecuteScalar(command);
                if (licCount != null)
                {
                    return Convert.ToInt64(licCount);
                }
                return (long?)null;
            }
        }

        public long? GetLicenseCountByEpStubId(long deviceId)
        {
            const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.DeviceId = @p_DeviceId And l.IsActive=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                var licCount = Connection.ExecuteScalar(command);
                if (licCount != null)
                {
                    return Convert.ToInt64(licCount);
                }
                return (long?)null;
            }
        }

        public long? GetClientLicenseCountByEpStubId(long deviceId)
        {
            const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.DeviceId = @p_DeviceId And l.IsActive=1 And eplt.isworkstation=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                var licCount = Connection.ExecuteScalar(command);
                if (licCount != DBNull.Value)
                {
                    return Convert.ToInt64(licCount);
                }
                return (long?)null;
            }
        }

        public long? GetServerLicenseCountByEpStubId(long deviceId)
        {
            const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.DeviceId = @p_DeviceId And l.IsActive=1 And eplt.isserver=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                var licCount = Connection.ExecuteScalar(command);
                if (licCount != DBNull.Value)
                {
                    return Convert.ToInt64(licCount);
                }
                return (long?)null;
            }
        }

				public long? GetVdiLicenseCountByEpStubId(long deviceId) {
					const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.DeviceId = @p_DeviceId And l.IsActive=1 And eplt.isvdi=1";
					using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
						Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
						var licCount = Connection.ExecuteScalar(command);
						if (licCount != DBNull.Value) {
							return Convert.ToInt64(licCount);
						}
						return (long?)null;
					}
				}

        public bool IsAuthCodeEp(string authCode)
        {
            const string sqlQuery = @"
SELECT pm.ModelCompatibility
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                var modelCompat = Connection.ExecuteScalar(command);
                if (modelCompat != null)
                {
                    return IsEndPointLicense(modelCompat.ToString());
                }
                return false;
            }
        }

        private bool IsEndPointLicense(string modelCompat)
        {
            if (modelCompat.Contains(EndpointConstants.ModelCompat))
            {
                return true;
            }
            return false;
        }

        public int GetNumberOfEsmTypesRegistered(long supportId)
        {
            const string sqlQuery = @"
SELECT COUNT(1)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccounTid = @p_SupportId And l.IsActive=1 And eplt.isesm=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportId);
                var licCount = Connection.ExecuteScalar(command);
                if (licCount != null)
                {
                    return Convert.ToInt32(licCount);
                }
                return 0;
            }
        }


        public List<string> GetEndPointLicenseTypes(long stubDeviceId)
        {
            const string sqlQuery = @"
SELECT pandbidentification
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
AND l.DeviceId = @p_DeviceId AND pm.ModelCompatibility=@p_mc;";
            var epLicTypes = new List<String>();
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, stubDeviceId);
                Connection.AddInParameter(command, "@p_mc", DbType.String, EndpointConstants.ModelCompat);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        epLicTypes.Add(dataReader["pandbidentification"].ToString());
                    }
                }
                return epLicTypes;

            }
        }


        public long? GetEpDeviceInfo(string authCode, out string serialNumber, out string deviceName, out string panDBIdentification)
        {
            serialNumber = string.Empty;
            deviceName = string.Empty;
            panDBIdentification = string.Empty;
            const string sqlQuery = @"SELECT d.DeviceId, d.SerialNumber,l.LicenseId, d.DeviceName,pst.PanDbIdentification
  from Device d
inner join license l on d.deviceId = l.deviceId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  JOIN partsubtype pst ON p.PartSubtypeId = pst.PartSubtypeId
where l.AuthorizationCode=@p_AuthCode";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        serialNumber = Convert.ToString(dataReader["SerialNumber"]);
                        deviceName = dataReader["deviceName"] != null
                                         ? dataReader["deviceName"].ToString()
                                         : string.Empty;
                        long? deviceId = Convert.ToInt64(dataReader["DeviceId"]);
                        panDBIdentification = dataReader["PanDbIdentification"] != null
                                         ? dataReader["PanDbIdentification"].ToString()
                                         : string.Empty;
                        return deviceId;
                    }
                }
            }
            return null;
        }


        public List<EpLicenses> GetEpLicenses(long deviceId)
        {
            const string sqlQuery = @"
SELECT d.SerialNumber,l.LicenseId,l.AuthorizationCode,l.ExpirationDate,pst.PanDbIdentification,pst.Name,pst.LicenseFeaturesName,ifnull(eplt.isesm,0) as isesm,ifnull(eplt.isworkstation,0) as isworkstation,ifnull(eplt.isserver,0) as isserver,ifnull(eplt.issupport,0) as issupport, pm.PartNumber from Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
WHERE d.DeviceId = @p_deviceId and l.isactive=1 Order By eplt.sortindex;";
            var epLicTypes = new List<EpLicenses>();
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_deviceId", DbType.Int64, deviceId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var lic = new EpLicenses();
                        lic.LicenseId = Convert.ToInt64(dataReader["LicenseId"]);
                        lic.AuthorizationCode = HttpUtility.HtmlEncode(dataReader["AuthorizationCode"].ToString());
                        var partNumber = HttpUtility.HtmlEncode(dataReader["PartNumber"].ToString());
                        var isMalbork = EpLogic.IsSkuMalbork(partNumber);

                        if (!isMalbork && dataReader["isesm"].ToString() == "1")
                        {
                            lic.ExpirationDate = HttpUtility.HtmlEncode("Perpetual");
                        }
                        else if (dataReader["ExpirationDate"] != DBNull.Value)
                        {
                            lic.ExpirationDate = Convert.ToDateTime(dataReader["ExpirationDate"]).ToShortDateString();
                        }
                        else
                        {
                            lic.ExpirationDate = "";
                        }
                        lic.FeatureName = HttpUtility.HtmlEncode(dataReader["PanDbIdentification"].ToString());
                        if (lic.FeatureName == EndpointConstants.SvcEsm)
                        {
                            lic.PartSubTypeName = HttpUtility.HtmlEncode(ConvertSupportNameToFriendlySupportName(
                                dataReader["Name"].ToString(), dataReader["LicenseFeaturesName"].ToString()));
                        }
                        else if (isMalbork)
                        {
                            lic.PartSubTypeName = HttpUtility.HtmlEncode(EpLogic.GetMalborkLicenseDescription(partNumber, dataReader["Name"].ToString()));
                        }
                        else
                        {
                            lic.PartSubTypeName = HttpUtility.HtmlEncode(dataReader["Name"].ToString());
                        }
                        epLicTypes.Add(lic);
                    }
                }
                return epLicTypes;

            }
        }


        public bool IsEpLicenseTypeAlreadyRegistered(long supportAccountId, string licType, long stubId)
        {
					string sqlQuery = String.Empty;

	        if (licType == EndpointConstants.Esm)
	        {
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.isesm = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
	        }
					else if (licType == EndpointConstants.Workstation)
					{
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.isworkstation = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
					} else if (licType == EndpointConstants.Server) {
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.isserver = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
					} else if (licType == EndpointConstants.Support) {
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.issupport = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
					} else if (licType == EndpointConstants.Warranty) {
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.iswarranty = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
					} else if (licType == EndpointConstants.Vdi) {
						sqlQuery = @"
SELECT l.licenseId
FROM Device d
inner join license l on d.deviceId = l.deviceId
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.SupportAccountId = @p_SupportId AND eplt.isvdi = 1 AND d.DeviceId = @p_deviceId AND pm.ModelCompatibility = @p_mc;";
					}
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_SupportId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_deviceId", DbType.Int64, stubId);
                Connection.AddInParameter(command, "@p_mc", DbType.String, EndpointConstants.ModelCompat);
                var license = Connection.ExecuteScalar(command);
                return license != null;
            }
        }


        public long? GetDeviceIdForEpAuthCode(string authCode)
        {
            const string sqlQuery = @"
SELECT DeviceId
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
WHERE l.AuthorizationCode = @p_AuthCode AND pm.ModelCompatibility like @p_mc;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));
                var deviceInfo = Connection.ExecuteScalar(command);
                if (deviceInfo!= DBNull.Value && deviceInfo != null)
                {
                    return Convert.ToInt64(deviceInfo);
                }
            }
            return null;
        }


        public string GetSerialNumberByDeviceId(long deviceId)
        {
            const string sqlQuery = @"
SELECT
  d.SerialNumber
FROM Device d
WHERE d.DeviceId = @p_DeviceId;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {

                        var serialNumber = dataReader["SerialNumber"];

                        if (serialNumber != null)
                        {
                            return serialNumber.ToString();
                        }
                    }
                    return null;
                }
            }
        }


        public bool SaveLicenseKey(string authCode, string licenseKey)
        {
            const string sqlQuery = @"
update lic_features
set license_key=@p_licenseKey
where auth_code = @p_AuthCode";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                PANDBConnection.AddInParameter(command, "@p_licenseKey", DbType.String, licenseKey);
                if (PANDBConnection.ExecuteNonQuery(command) == 1)
                {
                    return true;
                }
            }
            return false;
        }


				public bool GetLicenseKey(string authCode, long supportAccountId, out string licenseKey)
        {
            bool retVal = false;
            licenseKey = string.Empty;
            const string sqlQuery = @"
SELECT
  l.LicenseKey
FROM License l
WHERE l.SupportAccountId = @p_SupportAccountId 
	AND l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
								Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        if (dataReader["LicenseKey"] != null)
                        {
                            licenseKey = dataReader["LicenseKey"].ToString();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(licenseKey))
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        public void UpdateDeviceRegistrationDate(string serialNumber, DateTime regDate)
        {
            const string sqlQuery = @"
update device_master
set regdate=@p_regDate
where serialnum = @p_SerialNum";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_regDate", DbType.DateTime, regDate);
                PANDBConnection.AddInParameter(command, "@p_SerialNum", DbType.String, serialNumber);
                PANDBConnection.ExecuteNonQuery(command);
            }
        }


        public void RollBackPushToDeviceMasterForTraps(long deviceId, string authCode)
        {
            const string sqlQuery = @"
delete from lic_features
where devId = @p_DevId and auth_code <> @p_AuthCode";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_DevId", DbType.Int64, deviceId);
                PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                PANDBConnection.ExecuteNonQuery(command);
            }

            const string sqlQueryUpdateLicFeatures = @"
update lic_features
set regdate=NULL,devid=NULL
where auth_code = @p_AuthCode";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQueryUpdateLicFeatures))
            {
                PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                PANDBConnection.ExecuteNonQuery(command);
            }

            const string sqlDeleteDeviceQuery = @"
delete from device_master
where lid = @p_DevId";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlDeleteDeviceQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_DevId", DbType.Int64, deviceId);
                PANDBConnection.ExecuteNonQuery(command);
            }

        }


        public void UpdateAccountIdBySerialNumber(string serialNum, long userAccountId)
        {
            const string sqlQuery = @"
update device_master
set acctid=@p_acctid, regdate = @p_devregdate
where serialnum = @p_SerialNum";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_acctid", DbType.Int32, userAccountId);
                PANDBConnection.AddInParameter(command, "@p_SerialNum", DbType.String, serialNum);
                PANDBConnection.AddInParameter(command, "@p_devregdate", DbType.DateTime, System.DateTime.Now);
                PANDBConnection.ExecuteNonQuery(command);
            }
        }


        public string GetEndPointLicenseType(string authCode, out string stubPartId)
        {
            string licType = string.Empty;
            stubPartId = string.Empty;
            const string sqlQuery = @"
SELECT pst.pandbidentification,eps.PartNumber
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointstubs eps on pst.pandbidentification = eps.PandbIdentification 
WHERE l.AuthorizationCode = @p_AuthCode and pm.modelcompatibility like @p_mc;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {

                        licType = dataReader["pandbidentification"].ToString();
                        stubPartId = dataReader["PartNumber"].ToString();
                    }
                    return licType;
                }
            }
        }


        public string GetEndPointLicenseType(string authCode)
        {
            string licType = string.Empty;
            const string sqlQuery = @"
SELECT eplt.isesm,eplt.isworkstation,eplt.isserver,eplt.issupport,eplt.iswarranty,eplt.isvdi
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification = eplt.licensetype
AND l.AuthorizationCode = @p_AuthCode and pm.modelcompatibility like @p_mc;";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                Connection.AddInParameter(command, "@p_mc", DbType.String, string.Format("%{0}%", EndpointConstants.ModelCompat));

                using (var dataReader = Connection.ExecuteReader(command))
                {
									if (dataReader.Read()) {
										if (dataReader["isesm"] != null && dataReader["isesm"] != DBNull.Value && dataReader["isesm"].ToString() == "1") {
											return EndpointConstants.Esm;
										} else if (dataReader["isworkstation"] != null && dataReader["isworkstation"] != DBNull.Value && dataReader["isworkstation"].ToString() == "1") {
											return EndpointConstants.Workstation;
										} else if (dataReader["isserver"] != null && dataReader["isserver"] != DBNull.Value && dataReader["isserver"].ToString() == "1") {
											return EndpointConstants.Server;
										} else if (dataReader["issupport"] != null && dataReader["issupport"] != DBNull.Value && dataReader["issupport"].ToString() == "1") {
											return EndpointConstants.Support;
										} else if (dataReader["iswarranty"] != null && dataReader["iswarranty"] != DBNull.Value && dataReader["iswarranty"].ToString() == "1") {
											return EndpointConstants.Warranty;
										} else if (dataReader["isvdi"] != null && dataReader["isvdi"] != DBNull.Value && dataReader["isvdi"].ToString() == "1") {
											return EndpointConstants.Vdi;
										}
									}
                    return licType;
                }
            }
        }


	    public List<EpLicenseType> GetAllEndpointLicenseTypes(bool onlyShowEsmServerOrWs = false)
	    {
		    const string sqlQuery = @"
SELECT *
FROM endpointlicensetype;";
		    var epLicTypes = new List<EpLicenseType>();
		    using (var command = Connection.GetSqlStringCommand(sqlQuery))
		    {
			    using (var dataReader = Connection.ExecuteReader(command))
			    {
				    while (dataReader.Read())
				    {
							var epLicType = new EpLicenseType();
							epLicType.LicType = dataReader["licensetype"].ToString();
					    epLicType.IsEsm = (dataReader["isesm"] != null && dataReader["isesm"] != DBNull.Value) && dataReader["isesm"].ToString() == "1";
							epLicType.IsWorkstation = (dataReader["isworkstation"] != null && dataReader["isworkstation"] != DBNull.Value) && dataReader["isworkstation"].ToString() == "1";
							epLicType.IsServer = (dataReader["isserver"] != null && dataReader["isserver"] != DBNull.Value) && dataReader["isserver"].ToString() == "1";
							epLicType.IsSupport = (dataReader["issupport"] != null && dataReader["issupport"] != DBNull.Value) && dataReader["issupport"].ToString() == "1";
							epLicType.SortIdx = (dataReader["sortindex"] != null && dataReader["sortindex"] != DBNull.Value) ? Convert.ToInt32(dataReader["sortindex"]) : 100;
                            epLicType.IsAgent = (dataReader["isagent"] != null && dataReader["isagent"] != DBNull.Value) && dataReader["isagent"].ToString() == "1";
							epLicTypes.Add(epLicType);
				    }
			    }
		    }
		    if (onlyShowEsmServerOrWs)
		    {
					epLicTypes = epLicTypes.Where(x => x.IsEsm == true || x.IsServer == true || x.IsWorkstation == true).ToList();
		    }
				return epLicTypes;
	    }

			#region IEPDataAccessLogic Members


			public bool EndPointHasBKLNSupport(long deviceId) {				
				string sqlQuery = @"
select count(1) 
from csp.device d 
join csp.license l on d.deviceid=l.deviceid 
join csp.partmaster pm on l.partmasterid=pm.partmasterid
where d.deviceid=@p_DeviceId and l.isactive=1 and pm.partnumber like '%BKLN%'";
							
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);

                var hasBacklineSupport = Connection.ExecuteScalar(command);
								if (hasBacklineSupport != null && hasBacklineSupport != DBNull.Value && Convert.ToInt32(hasBacklineSupport) > 0 )
								{
									return true;
								}
            }
            return false;
			}

			#endregion

			#region IEPDataAccessLogic Members


			public bool EndPointHasBKLNSupport(string authorizationCode) {
				string sqlQuery = @"
select pm.partnumber 
from csp.license l 
join csp.partmaster pm on l.partmasterid=pm.partmasterid
where l.AuthorizationCode=@p_AuthCode";

				using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
					Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authorizationCode);

					var isBacklineSupport = Connection.ExecuteScalar(command);
					if (isBacklineSupport != null && isBacklineSupport != DBNull.Value && isBacklineSupport.ToString().Contains("BKLN")) {
						return true;
					}
				}
				return false;
			}

			#endregion

			#region IEPDataAccessLogic Members


			public bool EndPointHasSupport(long deviceId) {
				string sqlQuery = @"
select count(1) 
from csp.device d 
join csp.license l on d.deviceid=l.deviceid 
join csp.partmaster pm on l.partmasterid=pm.partmasterid
where d.deviceid=@p_DeviceId and (pm.partnumber like '%PREM%' OR pm.partnumber like '%BKLN%')";

				using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
					Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);

					var hasSupport = Connection.ExecuteScalar(command);
					if (hasSupport != null && hasSupport != DBNull.Value && Convert.ToInt32(hasSupport) > 0 ) {
						return true;
					}
				}
				return false;
			}

			#endregion


            public bool IsBetaCustomer(string user)
            {
                const string sqlQuery = @"
SELECT BetaAccessId
FROM Betaaccess l
WHERE FeatureName = 'TRAPS_ANDROID' AND EmailAddress= @p_UserEmail;";
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_UserEmail", DbType.String, user);
                    var isBetaCustomer = Connection.ExecuteScalar(command);
                    return isBetaCustomer != null;
                }
            }


            public bool GetTrapsUseCount(string authCode, out int? workstationCount, out int? serverCount, out int? vdiCount, out int? agentCount)
            {
                var availableTypes = GetAllEndpointLicenseTypes();
                workstationCount = null;
                serverCount = null;
                vdiCount = null;
                agentCount = null;
                bool isSuccess = false;
                string sqlQuery = "SELECT l1.AuthorizationCode,l1.VMUseCount ,p1.PanDbIdentification FROM csp.license l JOIN csp.license l1 ON l.DeviceId=l1.DeviceId JOIN csp.partmaster p ON l1.PartMasterId=p.PartMasterId JOIN csp.partsubtype p1 ON p.PartSubtypeId=p1.PartSubtypeId WHERE l.AuthorizationCode=@p_AuthCode AND l1.IsActive=1";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
												

                        while (dataReader.Read())
                        {
                            isSuccess = true;
                            var epType = dataReader["PanDbIdentification"].ToString();
                            var epRecord = (from x in availableTypes where x.LicType == epType select x).FirstOrDefault();
                            if (epRecord != null)
                            {
                                if (epRecord.IsServer)
                                {
                                    serverCount = Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]);
                                }
                                else if (epRecord.IsWorkstation)
                                {
                                    workstationCount = Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]);
                                }
                                else if (epRecord.IsVdi)
                                {
                                    vdiCount = Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]);
                                }
                                else if (epRecord.IsAgent)
                                {
                                    agentCount = Convert.ToInt32(dataReader["VMUseCount"]) == 0
                                        ? 1
                                        : Convert.ToInt32(dataReader["VMUseCount"]);
                                }
                            }
                        }
                    }
                }
                return isSuccess;
            }


            public long? GetAgentLicenseCountByEpStubId(long deviceId)
            {
                const string sqlQuery = @"
SELECT Sum(l.VMUseCount)
FROM license l
inner join partmaster pm on l.partmasterid=pm.partmasterid
inner join partsubtype pst on pm.partsubtypeid = pst.partsubtypeid
inner join endpointlicensetype eplt on pst.pandbidentification=eplt.licensetype
AND l.DeviceId = @p_DeviceId And l.IsActive=1 And eplt.isagent=1";
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    var licCount = Connection.ExecuteScalar(command);
                    if (licCount != DBNull.Value)
                    {
                        return Convert.ToInt64(licCount);
                    }
                    return (long?)null;
                }
            }
    }
}
