using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.Logic;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess {
	class LicenseTransferDataAccess : BaseDataAccess {

		public void UpdateDevicePartNumber(long deviceId, string newPartNumber) {

			const string sqlQuery = @"
				UPDATE device_master
				SET part_id = @p_PartNumber
				WHERE lid = @p_DeviceId";

			using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
				PANDBConnection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
				PANDBConnection.AddInParameter(command, "@p_PartNumber", DbType.String, newPartNumber);

				PANDBConnection.ExecuteNonQuery(command);
			}
		}

        public List<SelectableAccount> GetAscBklnAccounts(long supportAccountId)
        {

            var selectableAccount = new List<SelectableAccount>();

            using (var command = Connection.GetStoredProcCommand("GetSupportAccountAscUnregisteredBkln"))
            {
                Connection.AddInParameter(command, "p_SupportCenterSupportAccountId", DbType.Int64, supportAccountId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        var account = new SelectableAccount()
                        {
                            SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"].ToString()),
                            AccountTypeId = 1,
                            CompanyName = dataReader["Name"].ToString()
                        };
                        selectableAccount.Add(account);
                    }
                }
            }
            return selectableAccount;
        }

        public List<TransferLicenseAvailableDevice> GetCustomerDeviceInforForAsc(long supportAccountId, string modelCompatibility)
        {

            var ascDevices = new List<TransferLicenseAvailableDevice>();

            using (var command = Connection.GetStoredProcCommand("GetDeviceBySupportCenterSupportAccountId"))
            {
                Connection.AddInParameter(command, "p_SupportCenterSupportAccountId", DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, "p_srcmodelCompatibility", DbType.String, modelCompatibility);
                Connection.AddInParameter(command, "p_destmodelCompatibility", DbType.String, modelCompatibility);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        ascDevices.Add(new TransferLicenseAvailableDevice
                        {
                            DeviceId = long.Parse(dataReader["DeviceId"].ToString()),
                            SerialNumber = dataReader["SerialNumber"].ToString(),
                            PartNumber = dataReader["PartNumber"].ToString(),
                            AccountId = dataReader["SupportAccountId"].ToString(),
                            AccountName = dataReader["Name"].ToString(),
                        });
                    }
                }
            }

            return ascDevices;
        }

		public AscLicense LookForAscs(long deviceId) {

			const string sqlQuery = @"
				SELECT
					lf.lfid,
					lf.auth_code,
					lf.ASCId
				FROM device_master dm
					JOIN lic_features lf
						ON dm.lid = lf.devID
				WHERE dm.lid = @p_deviceId
				AND lf.ASCId IS NOT NULL
				AND lf.ASCId <> ''
				AND lf.active = 1;";

			using (var command = PANDBConnection.GetSqlStringCommand(sqlQuery)) {
				PANDBConnection.AddInParameter(command, "@p_deviceId", DbType.Int64, deviceId);

				using (var dataReader = PANDBConnection.ExecuteReader(command)) {

					var ascLicenses = new List<AscLicense>();

					if (dataReader.Read()) {
						ascLicenses.Add(new AscLicense {
							Lfid = long.Parse(dataReader["lfid"].ToString()),
							AuthCode = dataReader["auth_code"].ToString(),
							AscSalesforceAccountId = dataReader["ASCId"].ToString(),
						});
					}

					if (ascLicenses.Count == 1) {
						return ascLicenses.Single();
					}

					if (ascLicenses.Count == 0) {
						return null;
					}

					//Handle Error if count is more than 1
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error Transfering license ASC for Device ID: " + deviceId + " . Expected 1, but found " + ascLicenses.Count));
					return null;
				}
			}
		}

		public class AscLicense {
			public long Lfid { get; set; }
			public string AuthCode { get; set; }
			public string AscSalesforceAccountId { get; set; }
		}

        public void ExtendPa200(long deviceId,long useraccountid)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("ApplyRelaxationForPA200"))
                {
                    Connection.AddInParameter(command, "p_deviceID", DbType.Int64, deviceId);
                    Connection.AddInParameter(command, "p_LastUpdateByUserAccountId", DbType.Int64, useraccountid);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error In applying relaxation for pa200 : " + ex.Message.ToString()));
                //throw new Exception(ex.Message.ToString());
            }
        }
	}
}
