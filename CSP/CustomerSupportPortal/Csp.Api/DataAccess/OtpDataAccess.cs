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
    public class OtpDataAccess : BaseDataAccess, IOtpDataAccess
    {
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
        /// Add or Update OTP By OTP Type
        /// </summary>
        /// <param name="otpModel"></param>
        /// <returns></returns>
        public bool AddUpdateOneTimePassword(OneTimePasswordModel otpModel, out DateTime? expirationDate)
        {
            expirationDate = null;
            try
            {
                using (var command = Connection.GetStoredProcCommand("AddUpdateOneTimePassword"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, otpModel.SupportAccountId);
                    Connection.AddInParameter(command, "p_serialNumber", DbType.String, otpModel.SerialNumber);
                    Connection.AddInParameter(command, "p_lookup", DbType.String, otpModel.LookUp);
                    Connection.AddInParameter(command, "p_OTP", DbType.String, otpModel.Otp);
                    Connection.AddInParameter(command, "p_UpdatedBy", DbType.String, otpModel.UpdatedBy);
                    Connection.AddInParameter(command, "p_OtpType", DbType.String, otpModel.OtpType.ToDescription());
                    Connection.AddInParameter(command, "p_DeviceType", DbType.String, otpModel.DeviceType.ToDescription());

                    var result = Connection.ExecuteScalar(command);
                    if(result != null && result != DBNull.Value)
                    {
                        expirationDate = Convert.ToDateTime(result);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        /// <summary>
        /// Generates OTP for PAN OS device
        /// </summary>
        /// <returns></returns>
        public string GenerateOneTimePassword(string serialNumber)
        {
            var oneTimePassword = string.Empty;
            try
            {
                using (var command = Connection.GetStoredProcCommand("GenerateOneTimePasswordForDevice"))
                {
                    Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            oneTimePassword = HttpUtility.HtmlEncode(Convert.ToString(dataReader["oneTimePassword"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return oneTimePassword;
        }

        public bool SaveRegistrationPin(RegistrationPinModel registrationPinModel)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("SaveDeviceRegistrationPin"))
                {
                    Connection.AddInParameter(command, "p_SupportAccountID", DbType.Int64, registrationPinModel.SupportAccountId);
                    Connection.AddInParameter(command, "p_UniqueId", DbType.String, registrationPinModel.RegistrationPinUniqueId);
                    Connection.AddInParameter(command, "p_RegistrationPin", DbType.String, registrationPinModel.RegistrationPin);
                    Connection.AddInParameter(command, "p_ExpirationDate", DbType.DateTime, registrationPinModel.ExpirationDate);
                    Connection.AddInParameter(command, "p_Description", DbType.String, registrationPinModel.Description);
                    Connection.AddInParameter(command, "p_CreatedBy", DbType.Int64, registrationPinModel.CreatedBy);

                    Connection.ExecuteScalar(command);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        #region Bulk OTP Generation

        /// <summary>
        /// Get the pending Bulk OTP Requests
        /// </summary>
        /// <returns></returns>
        public List<BulkOtpModel> GetPendingBulkOtpRequests()
        {
            var pendingBulkOtpRequests = new List<BulkOtpModel>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPendingBulkOtpRequests"))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var bulkOtpModel = new BulkOtpModel
                            {
                                BulkOtpId = Convert.ToInt64(dataReader["SecaasBulkOtpId"]),
                                PanoramaSerialNumber = Convert.ToString(dataReader["SerialNumber"]),
                                SupportAccountId = Convert.ToInt64(dataReader["SupportAccountId"]),
                                RequestPayload = Convert.ToString(dataReader["RequestPayload"]),
                                RequestedByUserAccountId = Convert.ToInt64(dataReader["CreatedBy"])
                            };
                            pendingBulkOtpRequests.Add(bulkOtpModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return pendingBulkOtpRequests;
        }


        /// <summary>
        /// Saves a new Bulk OTP request and updates any pending request
        /// </summary>
        /// <param name="bulkOtpRequest"></param>
        /// <returns></returns>
        public bool SaveBulkOtpRequest(BulkOtpModel bulkOtpRequest)
        {
            var isSaved = false;
            try
            {
                using (var command = Connection.GetStoredProcCommand("SaveBulkOtp"))
                {
                    Connection.AddInParameter(command, "p_BulkOtpId", DbType.Int64, bulkOtpRequest.BulkOtpId);
                    Connection.AddInParameter(command, "p_PanoramaSerialNum", DbType.String, bulkOtpRequest.PanoramaSerialNumber);
                    Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, bulkOtpRequest.SupportAccountId);
                    Connection.AddInParameter(command, "p_RequestPayload", DbType.String, bulkOtpRequest.RequestPayload);
                    Connection.AddInParameter(command, "p_ResponsePayload", DbType.String, bulkOtpRequest.ResponsePayload);
                    Connection.AddInParameter(command, "p_ProcessingStatus", DbType.Int16, bulkOtpRequest.ProcessingStatus);
                    Connection.AddInParameter(command, "p_StatusMessage", DbType.String, bulkOtpRequest.StatusMessage);
                    Connection.AddInParameter(command, "p_RequestedByUser", DbType.Int64, bulkOtpRequest.RequestedByUserAccountId);

                    Connection.ExecuteScalar(command);
                    isSaved =  true;
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                isSaved = false;
            }

            return isSaved;
        }

        /// <summary>
        /// Generate OTP for a bulk of devices
        /// </summary>
        /// <param name="bulkOtp"></param>
        /// <returns></returns>
        public List<DeviceOtp> GenerateBulkOtp(BulkOtpModel bulkOtp)
        {
            var devicesOtp = new List<DeviceOtp>();

            try
            {
                using (var command = Connection.GetStoredProcCommand("GenerateBulkOtp"))
                {
                    Connection.AddInParameter(command, "p_BulkOtpId", DbType.Int64, bulkOtp.BulkOtpId);
                    Connection.AddInParameter(command, "p_Panorama", DbType.String, bulkOtp.PanoramaSerialNumber);
                    Connection.AddInParameter(command, "p_SerialNumbers", DbType.String, bulkOtp.DeviceList);
                    Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, bulkOtp.SupportAccountId);
                    Connection.AddInParameter(command, "p_RequestedByUser", DbType.Int64,
                        bulkOtp.RequestedByUserAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var deviceOtp = new DeviceOtp
                            {
                                SerialNumber = Convert.ToString(dataReader["SerialNumber"]),
                                OneTimePassword = Convert.ToString(dataReader["OTP"]),
                                ErrorMessage = Convert.ToString(dataReader["ErrorMessage"])
                            };

                            devicesOtp.Add(deviceOtp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }

            return devicesOtp;
        }

        #endregion

        public OtpSummary<OtpSummaryItem> GetDeviceRegistrationOtpSummary(long supportAccountId, long userAccountId, DataParameters parameters, bool showHistory)
        {
            if (!SortHelper.ValidateSorting(parameters.SortField, parameters.SortDirection.ToSqlOrderBy()))
            {
                parameters.SortField = string.Empty;
                parameters.SortDirection = SortDirection.Ascending;
            }

            var otps = new OtpSummary<OtpSummaryItem>
            {
                SupportAccountId = supportAccountId,
                Items = new List<OtpSummaryItem>(), 
                Parameters = parameters 
            };
            using (var command = Connection.GetStoredProcCommand("GetDeviceRegistrationOtpSummary"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                Connection.AddSortParameters(command, parameters);
                Connection.AddPaginationParameters(command, parameters);
                Connection.AddInParameter(command, "p_ShowHistory", DbType.Boolean, showHistory);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    var items = new List<OtpSummaryItem>();
                    while (dataReader.Read())
                    {
                        items.Add( new OtpSummaryItem
                        {
                            Id = dataReader["Id"] != DBNull.Value ? Convert.ToInt64(dataReader["Id"]) : 0L,
                            SerialNumber = dataReader["SerialNumber"] != DBNull.Value ? Convert.ToString(dataReader["SerialNumber"]) : string.Empty,
                            DeviceType = dataReader["DeviceType"] != DBNull.Value ? Convert.ToString(dataReader["DeviceType"]) : string.Empty,
                            OTPType = dataReader["OTPType"] != DBNull.Value ? Convert.ToString(dataReader["OTPType"]) : string.Empty,
                            OTP = dataReader["OTP"] != DBNull.Value ? Convert.ToString(dataReader["OTP"]) : string.Empty,
                            Status = dataReader["Status"] != DBNull.Value ? Convert.ToString(dataReader["Status"]) : string.Empty,
                            ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToString(dataReader["ExpirationDate"]) : string.Empty,
                            ExpirationDateTics = dataReader["ExpirationDate"] != DBNull.Value ? ((DateTime)dataReader["ExpirationDate"]).Ticks : 0L,
                            Requestor = dataReader["Requestor"] != DBNull.Value ? Convert.ToString(dataReader["Requestor"]) : string.Empty,
                            CreatedDate = dataReader["CreatedDate"] != DBNull.Value ? Convert.ToString(dataReader["CreatedDate"]) : string.Empty,
                            ErrorMessage = dataReader["ErrorMessage"] != DBNull.Value ? Convert.ToString(dataReader["ErrorMessage"]) : string.Empty,
                            CreatedDateTicks = dataReader["CreatedDate"] != DBNull.Value ? ((DateTime)dataReader["CreatedDate"]).Ticks : 0L,
                            IsError = dataReader["IsError"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsError"]) : false,
                            IsExpired = dataReader["IsExpired"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsExpired"]) : false,
                            IsHistory = dataReader["IsHistory"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsHistory"]) : false
                        });
                    }

                    otps.Items = items;
                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return otps;
        }

        public OtpSummary<OtpPinSummaryItem> GetDeviceRegistrationPinSummary(long supportAccountId, long userAccountId, DataParameters parameters)
        {
            var otps = new OtpSummary<OtpPinSummaryItem>
            {
                SupportAccountId = supportAccountId,
                Items = new List<OtpPinSummaryItem>(),
                Parameters = parameters
            };
            using (var command = Connection.GetStoredProcCommand("GetDeviceRegistrationPinSummary"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                Connection.AddSortParameters(command, parameters);
                Connection.AddPaginationParameters(command, parameters);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    var items = new List<OtpPinSummaryItem>();
                    while (dataReader.Read())
                    {
                        items.Add(new OtpPinSummaryItem
                        {
                            Id = dataReader["Id"] != DBNull.Value ? Convert.ToInt64(dataReader["Id"]) : 0L,
                            UniqueId = dataReader["UniqueId"] != DBNull.Value ? Convert.ToString(dataReader["UniqueId"]) : string.Empty,
                            Pin = dataReader["Pin"] != DBNull.Value ? Convert.ToString(dataReader["Pin"]) : string.Empty,
                            UseCount = dataReader["UseCount"] != DBNull.Value ? Convert.ToInt32(dataReader["UseCount"]) : 0,
                            Description = dataReader["Description"] != DBNull.Value ? Convert.ToString(dataReader["Description"]) : string.Empty,
                            ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? ((DateTime)dataReader["ExpirationDate"]).ToShortDateString() : string.Empty,
                            ExpirationDateTics = dataReader["ExpirationDate"] != DBNull.Value ? ((DateTime)dataReader["ExpirationDate"]).Ticks : 0L,
                            Requestor = dataReader["Requestor"] != DBNull.Value ? Convert.ToString(dataReader["Requestor"]) : string.Empty,
                            CreatedDate = dataReader["CreatedDate"] != DBNull.Value ? ((DateTime)dataReader["CreatedDate"]).ToShortDateString() : string.Empty,
                            CreatedDateTicks = dataReader["CreatedDate"] != DBNull.Value ? ((DateTime)dataReader["CreatedDate"]).Ticks : 0L,
                            IsExpired = dataReader["IsExpired"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsExpired"]) : false,
                            IsHistory = dataReader["IsHistory"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsHistory"]) : false
                        });
                    }

                    otps.Items = items;
                }

                if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                {
                    parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
                }
            }

            return otps;
        }

        public int DisableDeviceRegistrationPin(long supportAccountId, long userAccountId, string ids)
        {
            using (var command = Connection.GetStoredProcCommand("DisableDeviceRegistrationPin"))
            {
                Connection.AddInSupportAccountIdParameter(command, supportAccountId);
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                Connection.AddInParameter(command, "p_IDs", DbType.String, ids);
                return Connection.ExecuteNonQuery(command);
            }
        }

        public void ProcessOtpExpirationRequest(long userAccountId)
        {
            using (var command = Connection.GetStoredProcCommand("ProcessOtpExpiration"))
            {
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                var affectedRecordsCount = Connection.ExecuteNonQuery(command);
            }
        }

        public void ProcessPinExpirationRequest(long userAccountId)
        {
            using (var command = Connection.GetStoredProcCommand("ProcessPinExpiration"))
            {
                Connection.AddInUserAccountIdParameter(command, userAccountId);
                var affectedRecordsCount = Connection.ExecuteNonQuery(command);
            }
        }

        public bool EnableThermiteApiTest()
        {
            string query = @"SELECT COUNT(1) > 0 EnableThermiteTest FROM betaaccess b WHERE b.FeatureName = 'THERMITE_TEST_GENERATE_OTP';";

            using (var command = Connection.GetSqlStringCommand(query))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        return dataReader["EnableThermiteTest"] != DBNull.Value ? Convert.ToBoolean(dataReader["EnableThermiteTest"]) : false;
                    }
                }
            }
            return false;
        }
    }
}
