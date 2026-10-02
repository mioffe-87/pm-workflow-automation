using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System.Web;

namespace Csp.Api.DataAccess {
	public class VirtualMachineDataAccess : BaseDataAccess, IVirtualMachineDataAccess 
    {

		private readonly ISupportAccountRepository _supportAccountRepository;
		private readonly IDeviceRepository _deviceRepository;
	    private readonly IConnectionStrings _connectionStrings;

		public VirtualMachineDataAccess(ISupportAccountRepository supportAccountRepository, IDeviceRepository deviceRepository, IConnectionStrings connectionStrings) {
			_supportAccountRepository = supportAccountRepository;
			_deviceRepository = deviceRepository;
		    _connectionStrings = connectionStrings;
		}

        public bool IsAuthCodeAlreadyRegistered(EVmType vmType, string authCode)
        {
			//Original PanDB query: select serialnum, acctid from device_master where active=1 and serialnum='$serial_number'

			const string sqlQuery = @"
SELECT d.SerialNumber
FROM device d
WHERE d.IsActive = 1
AND d.SerialNumber = @p_AuthCode;";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						return true;
					}
				}
			}

			return false;
		}

        public VmAuthCodeDetails GetAuthCodeDetails(EVmType vmType, string authCode)
        {
			//Original PanDB query: select auth_code, regdate, expiration, active, part_id,vmStubId from lic_features where auth_code='$serial_number'
			var sqlQuery = vmType == EVmType.Cn
                ? @"
SELECT l.AuthorizationCode, l.RegistrationDate, l.ExpirationDate, l.IsActive, pm.PartNumber, l.VMStubDeviceId, pst.PanDbIdentification
  FROM license l
  INNER JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
  INNER JOIN partsubtype pst ON pm.PartSubtypeId = pst.PartSubtypeId
  WHERE pm.PartNumber LIKE '%-CN-X-%'
    AND l.AuthorizationCode = @p_AuthorizationCode;"
                : string.Format(@"
SELECT 
  l.AuthorizationCode, 
  l.RegistrationDate, 
  l.ExpirationDate, 
  l.IsActive, 
  pm.PartNumber, 
  l.VMStubDeviceId,
  pst.PanDbIdentification
FROM license l
INNER JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
INNER JOIN partsubtype pst ON pm.PartSubtypeId = pst.PartSubtypeId
WHERE l.AuthorizationCode = '{0}';", authCode);
			using (var command = Connection.GetSqlStringCommand(sqlQuery)) 
            {
                if (vmType == EVmType.Cn)
                {
                    Connection.AddInLicenseAuthCodeParameter(command, authCode);
                }
				var vmAuthCodes = new List<VmAuthCodeDetails>();
				using (IDataReader dataReader = Connection.ExecuteReader(command)) 
                {
					if (dataReader.Read()) 
                    {
						var vmAuthCode = new VmAuthCodeDetails();
						vmAuthCode.PopulateProperties(dataReader);
						vmAuthCodes.Add(vmAuthCode);
					}
				}

				return vmAuthCodes.Count != 1 ? null : vmAuthCodes.Single();
			}
		}

		public string GetNewStubSn(EVmType vmType) 
        {
            using (var command = Connection.GetStoredProcCommand(vmType == EVmType.Cn ? "GetNewCnStubSn" : "GetNewVmStubSn")) 
            {
				using (var dataReader = Connection.ExecuteReader(command)) 
                {
					dataReader.Read();
					return dataReader["StubSn"].ToString();
				}
			}
		}

		public DateTime GetShipDate(EVmType vmType, string authCode) 
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
					dataReader.Read();
					var shipDate = Convert.ToDateTime(dataReader["ShipDate"]);
					return shipDate;
				}
			}
		}

        public void CreateStubDevice(EVmType vmType, string authCode, string newStubSn, DateTime shipDate, long supportAccountId, long userAccountId, bool isVmElaStubDevice = false)
        {
			CreateDevice(vmType, newStubSn, supportAccountId, shipDate, userAccountId);
			var stubDeviceId = _deviceRepository.ReadDeviceIdBySerialNumber(newStubSn);
			_supportAccountRepository.UpdateDeviceSupportAccountInCsp(stubDeviceId, supportAccountId);
            //for VM ELA grandparent authcode: we don't associate the authcode to any account, just return the SN
            if (!isVmElaStubDevice)
            {
                SaveStubDeviceIdToLicense(authCode, stubDeviceId, supportAccountId);
            }
        }

        private void CreateDevice(EVmType vmType, string newStubSn, long supportAccountId, DateTime shipDate, long userAccountId)
        {
            string sqlQuery = vmType == EVmType.Cn 
                ? @"
INSERT INTO device_master(serialnum, acctid, isvm, part_id, ship_date, regdate, ispublic) 
    VALUES(@p_SerialNumber, @p_AcctId, 1, 'PAN-PA-CN-SW', @p_ShipDate, CURRENT_TIMESTAMP, 1); "
                : @"
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
             'PAN-PA-VM-SW', 
             @p_ShipDate, 
             CURRENT_TIMESTAMP, 
             1); ";

			using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
				PANDBConnection.AddInParameter(command, "@p_SerialNumber", DbType.String, newStubSn);
				PANDBConnection.AddInParameter(command, "@p_AcctId", DbType.Int32, userAccountId);
				PANDBConnection.AddInParameter(command, "@p_ShipDate", DbType.DateTime, shipDate);

				PANDBConnection.ExecuteNonQuery(command);
			}

			//Update the supportAccountId by serial number because the trigger can't do this
			_deviceRepository.UpdateSupportAccountIdBySerialNumber(newStubSn, supportAccountId);
		}

		private void SaveStubDeviceIdToLicense(string authCode, long stubDeviceId, long supportAccountId) {
			const string sqlQuery = @"
update lic_features
set VmStubId=@p_StubDeviceId
where auth_code = @p_AuthCode";

			using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
				PANDBConnection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
				PANDBConnection.AddInParameter(command, "@p_StubDeviceId", DbType.Int64, stubDeviceId);

				PANDBConnection.ExecuteNonQuery(command);
			}

			SaveSupportAccountIdToCspLicense(authCode, supportAccountId, stubDeviceId);
		}

		private void SaveSupportAccountIdToCspLicense(string authCode, long supportAccountId, long stubDeviceId) {
			const string sqlQuery = @"
UPDATE license l
SET SupportAccountId = @p_SupportAccountId,DeviceId=@p_DeviceId
WHERE AuthorizationCode = @p_AuthCode";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
				Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "p_DeviceId", DbType.Int64, stubDeviceId);
                Connection.ExecuteNonQuery(command);
			}
		}


        public DeviceRec GetDeviceDataFromAuthCode(EVmType vmType, string newVmAuthCode)
        {
            var sqlQuery = @"
                    SELECT
	                    d.DeviceId,
                      d.SerialNumber
                    FROM device d
                    INNER JOIN license l ON d.DeviceId = l.DeviceId
                    WHERE l.AuthorizationCode = @p_AuthCode;";                

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_AuthCode", DbType.String, newVmAuthCode);

				using (var dataReader = Connection.ExecuteReader(command)) {
					dataReader.Read();

					var deviceRec = new DeviceRec();
					deviceRec.PopulateProperties(dataReader);

					return deviceRec;
				}
			}
		}

        public DeviceRec GetDeviceDataFromLfid(EVmType vmType, long lfId)
        {
            const string sqlQuery = @"
SELECT
	d.DeviceId,
  d.SerialNumber
FROM device d
INNER JOIN license l ON d.DeviceId = l.DeviceId
WHERE l.licenseid = @p_lfid;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_lfid", DbType.Int64, lfId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    dataReader.Read();
                    var deviceRec = new DeviceRec();
                    deviceRec.PopulateProperties(dataReader);
                    return deviceRec;
                }
            }
        }


        public bool CanProvisionPanorama(EVmType vmType, string authCode)
        {
            const string sqlQuery = @"
select count(distinct(a.deviceid)) from license a 
inner join license b 
on a.licenseid=b.bundlelicenseid 
inner join partmaster c on c.partmasterid = b.partmasterid 
where a.authorizationcode=@p_AuthCode;";
            var configValues = new ConfigValues();
            int iMaxPraProvForNsxCount =
                Convert.ToInt32(configValues.GetAppSetting(AppSettings.MaxPraProvisioningForNsxCount));
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        int iCount = Convert.ToInt32(dataReader[0]);
                        if (iCount < iMaxPraProvForNsxCount)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string GetSerialNumForPraDevice()
        {
            const string sqlQuery = @"
SELECT
  l.val1 as SerialNum
FROM lookups l
WHERE l.fld = @p_fld and l.key= @p_key";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_fld", DbType.String, "nextserialnumber");
                PANDBConnection.AddInParameter(command, "@p_key", DbType.String, "PRA-NSX");
                using (var dataReader = PANDBConnection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        var serialNum = dataReader["SerialNum"].ToString();
                        return serialNum;
                    }
                }
            }
            return string.Empty;
        }

        public string GetPraSerialNumber(EVmType vmType)
        {
            return GetSerialNumForPraDevice();
        }

        private bool UpdateNewSerialNumberForPraDevice(string val, string newVal)
        {
            const string sqlQuery = @"
UPDATE lookups l
SET l.val1 = @p_SerialNum
WHERE l.fld = @p_fld and l.key= @p_key and l.val1= @p_Val1";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_SerialNum", DbType.String, newVal);
                PANDBConnection.AddInParameter(command, "@p_fld", DbType.String, "nextserialnumber");
                PANDBConnection.AddInParameter(command, "@p_key", DbType.String, "PRA-NSX");
                PANDBConnection.AddInParameter(command, "@p_Val1", DbType.String, val);
                return PANDBConnection.ExecuteNonQuery(command) > 0;
            }
        }

        public bool UpdateNewSerialNumber(EVmType vmType, string val, string newVal)
        {
            return UpdateNewSerialNumberForPraDevice(val, newVal);
        }


        public IEnumerable<PraProvisioningRec> GetNsxProvisionedPanoramas(EVmType vmType, string authCode)
        {
            var nsxDevices = new List<PraProvisioningRec>();

//            const string sqlQuery = @"
//select d.deviceid,d.serialnumber,c.partnumber,e.name as license,b.authorizationcode as authcode, b.expirationdate as expiration from license a 
//inner join license b 
//on a.licenseid=b.bundlelicenseid 
//inner join partmaster c on c.partmasterid = b.partmasterid 
//inner join device d on b.deviceid=d.deviceid
//inner join partsubtype e on c.partsubtypeid=e.partsubtypeid
//inner join partmaster f on d.partmasterid=f.partmasterid
//where a.authorizationcode=@p_AuthCode and (f.modelcompatibility='PRANSX' OR f.modelcompatibility='PRA-CP')";

            const string sqlQuery = @" SELECT d.DeviceId , d.SerialNumber,p.PartNumber,ps.Name AS license,lbundle.AuthorizationCode AS authcode,
      lbundle.ExpirationDate AS expiration  
      FROM license lauthcode JOIN license lbundle ON lauthcode.LicenseId = lbundle.BundleLicenseId
      JOIN device d ON lbundle.DeviceId=d.DeviceId  
      JOIN partmaster p ON p.PartMasterId=lbundle.PartMasterId
      JOIN parttype  pt ON p.PartTypeId = pt.PartTypeId
      JOIN partsubtype ps ON ps.PartSubtypeId = p.PartSubtypeId
      WHERE lauthcode.AuthorizationCode = @p_AuthCode AND lbundle.isActive=1  AND p.ModelCompatibility IN ('PRA-CP','PRANSX') AND d.IsActive=1;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var nsxDevice = new PraProvisioningRec();
                        nsxDevice.DeviceId = Convert.ToInt64(dataReader["deviceid"]);
                        nsxDevice.SerialNumber = HttpUtility.HtmlEncode(dataReader["serialnumber"].ToString());
                        nsxDevice.PartNumber = HttpUtility.HtmlEncode(dataReader["partnumber"].ToString());
                        nsxDevice.License = string.Format("<div class='licenserow'>{0}</div>",
																													 HttpUtility.HtmlEncode(dataReader["license"].ToString()));
												nsxDevice.AuthCode = string.Format("<div class='licenserow'>{0}</div>",
																													 HttpUtility.HtmlEncode(dataReader["authcode"].ToString()));
												nsxDevice.Expiration = string.Format("<div class='licenserow'>{0}</div>",
																													 dataReader["expiration"] != DBNull.Value
                                                   ? HttpUtility.HtmlEncode(Convert.ToDateTime(dataReader["expiration"]).ToShortDateString())
                                                   : string.Empty); 
												if (nsxDevices.Find(x => x.SerialNumber == nsxDevice.SerialNumber) == null) {
													nsxDevices.Add(nsxDevice);
												} else {
													var nsxPanorama = nsxDevices.Find(x => x.SerialNumber == nsxDevice.SerialNumber);
													nsxPanorama.License += nsxDevice.License;
													nsxPanorama.AuthCode += nsxDevice.AuthCode;
													nsxPanorama.Expiration += nsxDevice.Expiration;
												}
                    }
                }
            }

            return nsxDevices;
        }

        public bool UpdateAccountIdForPraDevice(EVmType vmType, string serialNum, long supportAccountId)
        {
            const string sqlQuery = @"
UPDATE device_master d
SET d.acctid = @p_AccountId, d.regdate= @p_currentDate
WHERE d.serialnum= @p_serialNum";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_AccountId", DbType.Int64, supportAccountId);
                PANDBConnection.AddInParameter(command, "@p_serialNum", DbType.String, serialNum);
                PANDBConnection.AddInParameter(command, "@p_currentDate", DbType.DateTime, System.DateTime.Now);
                return PANDBConnection.ExecuteNonQuery(command) > 0;
            }
        }

        /// <summary>
        /// This method is check if vmupgrade is inprogress for given device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public bool IsUpgradeComplete(EVmType vmType, long deviceId)
	    {
            try
            {
                const string sqlQuery = @"SELECT COUNT(DISTINCT v.VmUpgradeId) 'Count' FROM license l JOIN vmupgrade v ON l.AuthorizationCode=v.UpgradedAuthCode
                                        WHERE v.VmUpgradeStatus='Complete' AND v.DeviceId=@p_deviceId order by v.VmUpgradeId desc limit 1;";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_deviceId", DbType.Int64, deviceId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                            return Convert.ToInt32(dataReader["Count"]) > 0;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
	    }

        public string RevokeVmElaLicense(EVmType vmType, string authCode, long supportAccountId)
	    {
	        string result = string.Empty;

            using (var command = Connection.GetStoredProcCommand("VmElaRevokeLicense"))
	        {
	            Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInParameter(command, "p_AuthCode", DbType.String, authCode);
                Connection.AddOutParameter(command, "p_Result", DbType.String, 500);
	            Connection.ExecuteNonQuery(command);

                if (command.Parameters["p_Result"].Value != DBNull.Value)
	            {
                    result = command.Parameters["p_Result"].Value.ToString();
	            }
	        }

            return result;
	    }

        public bool UpdateVmActivationUser(string serialNum, long userAccountId)
        {
            const string sqlQuery = @"
                                    UPDATE device_master d
                                    SET d.acctid = @p_UserAccountId
                                    WHERE d.serialnum= @p_serialNum";

            using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery))
            {
                PANDBConnection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                PANDBConnection.AddInParameter(command, "@p_serialNum", DbType.String, serialNum);
                return PANDBConnection.ExecuteNonQuery(command) > 0;
            }
        }
    }

}
