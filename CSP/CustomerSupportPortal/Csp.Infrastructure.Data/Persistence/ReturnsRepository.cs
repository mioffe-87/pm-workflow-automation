using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using FluentNHibernate.Conventions;
using MySql.Data.MySqlClient;


namespace Csp.Infrastructure.Data.Persistence
{
    public class ReturnsRepository : IReturnsRepository
    {
        private readonly IConnectionStrings _connectionStrings;
        private readonly IApplicationSettings _applicationSettings;

        public ReturnsRepository(IConnectionStrings connectionStrings, IApplicationSettings applicationSettings)
        {
            _connectionStrings = connectionStrings;
            _applicationSettings = applicationSettings;
        }

        /// <summary>
        /// Get the list of jobs queued for RMA processing
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public List<long> GetRmaJobsQueuedForSupportAccount(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var jobsQueued = (from d in db.rmadevices join j in db.rmadevicejobqueues on d.DeviceId equals  j.DeviceId
                             where d.SupportAccountId == supportAccountId && j.Status != JobStatus.Complete.ToString()
                             select j.JobUniqueId).ToList();

            return jobsQueued;
        }

        /// <summary>
        /// Save RMA Jobs sent to TAC Toolbox for processing
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="rmaDevice"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public bool SaveDeviceRmaJobQueue(long jobId, long supportAccountId, RmaDeviceDetails rmaDevice, long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var now = DateTime.Now;

            var device = db.devices.FirstOrDefault(d => d.SerialNumber.Equals(rmaDevice.SerialNumber));
            var deviceId = device != null ? device.DeviceId : 0;

            var rmaDeviceJob = db.rmadevicejobqueues.FirstOrDefault(d => d.DeviceId == deviceId);

            if (rmaDeviceJob != null)
            {
                db.rmadevicejobqueues.Remove(rmaDeviceJob);

                var rmaDeviceDetail = db.rmadevices.FirstOrDefault(d => d.DeviceId == deviceId);
                if (rmaDeviceDetail != null)
                {
                    rmaDeviceDetail.SerialNumber = rmaDevice.SerialNumber;
                    rmaDeviceDetail.TechSupportFileName = rmaDevice.TechSupportFileName;
                    rmaDeviceDetail.ShippingAddress = rmaDevice.ShippingAddress;
                    rmaDeviceDetail.ShippingCity = rmaDevice.ShippingCity;
                    rmaDeviceDetail.ShippingRegion = rmaDevice.ShippingState;
                    rmaDeviceDetail.ShippingCountry = rmaDevice.ShippingCountry;
                    rmaDeviceDetail.ShippingPostalCode = rmaDevice.ShippingPostalCode;
                    rmaDeviceDetail.LastUpdate = now;
                    rmaDeviceDetail.LastUpdateByUserAccountId = userAccountId;
                }
            }
            else
            {
                db.rmadevices.Add(new rmadevice
                {
                    DeviceId = deviceId,
                    SupportAccountId = supportAccountId,
                    SerialNumber = rmaDevice.SerialNumber,
                    TechSupportFileName = rmaDevice.TechSupportFileName,
                    ShippingAddress = rmaDevice.ShippingAddress,
                    ShippingCity = rmaDevice.ShippingCity,
                    ShippingRegion = rmaDevice.ShippingState,
                    ShippingCountry = rmaDevice.ShippingCountry,
                    ShippingPostalCode = rmaDevice.ShippingPostalCode,
                    CreatedOn = now,
                    CreatedByUserAccountId = userAccountId
                });
            }

            //Add to job queue
            db.rmadevicejobqueues.Add(new rmadevicejobqueue
            {
                JobUniqueId = jobId,
                DeviceId = deviceId,
                CreatedByUserAccountId = userAccountId,
                CreatedOn = now
            });

            var success = db.SaveChanges() > 0;
            return success;
        }

        /// <summary>
        /// Update the status of RMA jobs after TAC Toolbox has finished processing
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="statusCode"></param>
        /// <param name="verdict"></param>
        /// <param name="statusMessage"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public bool UpdateRmaJobQueueStatus(long jobId, JobStatus status, int? statusCode, JobVerdict verdict, string statusMessage, long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var now = DateTime.Now;
            var success = false;

            var deviceRmaJob = db.rmadevicejobqueues.FirstOrDefault(dq => dq.JobUniqueId == jobId);
            if (deviceRmaJob != null)
            {
                deviceRmaJob.Status = status.ToString();
                deviceRmaJob.StatusCode = statusCode;
                deviceRmaJob.StatusMessage = statusMessage;
                deviceRmaJob.Verdict = verdict.ToString();
                deviceRmaJob.LastUpdate = now;
                deviceRmaJob.LastUpdateByUserAccountId = userAccountId;

                success = db.SaveChanges() > 0;
            }

            return success;
        }

        /// <summary>
        /// Log TAC Toolbox API Call
        /// </summary>
        /// <param name="apiCall"></param>
        /// <param name="requestData"></param>
        /// <param name="userAccountId"></param>
        public long LogTacToolboxApiCall(string apiCall, string requestData, long userAccountId)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;

                var apiCallLog = new logtactoolboxapicall
                {
                    ApiCall = apiCall,
                    Input = requestData,
                    CreatedOn = DateTime.Now,
                    CreatedByUserAccountId = userAccountId
                };

                db.logtactoolboxapicalls.Add(apiCallLog);
                db.SaveChanges();
                return apiCallLog.TacToolboxApiCallId;
            }
            catch (Exception ex)
            {
                var error = ((System.Data.Entity.Validation.DbEntityValidationException) ex).EntityValidationErrors;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
                return 0;
            }
        }

        /// <summary>
        /// Log TAC Toolbox API Response
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="responseData"></param>
        public void LogTacToolboxApiResponse(long logId, string responseData)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var tactoolboxApiLog = db.logtactoolboxapicalls.Find(logId);
                if (tactoolboxApiLog != null)
                {
                    tactoolboxApiLog.Output = responseData;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
            }
        }

        /// <summary>
        /// To fetch the list valid PA200 devices in Support Account to displayed on SSD Portal
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="userAccountId"></param>
        /// <returns></returns>
        public List<RmaDevice> GetDeviceList(long supportAccountId, long userAccountId)
        {
            var validPa200Devices = new List<RmaDevice>();
            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetEligibleDevicesForReturn";
                sqlCmd.CommandTimeout = 120;
                sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int64).Value = supportAccountId;
                sqlCmd.Parameters.Add("p_UserAccountId", MySqlDbType.Int64).Value = userAccountId;
	              sqlCmd.Parameters.Add("p_ReturnDevices",MySqlDbType.Int32).Value=1;
	              sqlCmd.Parameters.Add("p_Count", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                var dtRmaDevices = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtRmaDevices);

                if (dtRmaDevices.Rows.Count > 0 && dtRmaDevices.Rows.Count <= 25)
                {
                    foreach (DataRow dr in dtRmaDevices.Rows)
                    {
                        var rmaDevice = new RmaDevice()
                        {
                            DeviceId = (long) dr["DeviceId"],
                            SerialNumber = dr["SerialNumber"].ToString(),
                            ModelName = dr["ModelName"].ToString(),
                            SfdcAccountId = dr["SalesforceAccountId"].ToString(),
                            SfdcContactId = dr["SalesforceContactId"].ToString(),
                            DistributorId = dr["DistributerId"].ToString(),
                            EndCustomerId = dr["EndCustomerId"].ToString(),
                            ShippingAddress = dr["Address"].ToString(),
                            ShippingCity = dr["City"].ToString(),
                            ShippingRegion = dr["Region"].ToString(),
                            ShippingCountry = dr["Country"].ToString() ,
                            ShippingZipCode = dr["PostalCode"].ToString(),
                            Status = dr["Status"].ToString(),
                            StatusMessage = dr["StatusMessage"].ToString(),
                            Verdict = dr["Verdict"].ToString()
                        };

                        validPa200Devices.Add(rmaDevice);
                    }
                }

                return validPa200Devices;
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("GetPortalDeviceList: --> {0} {1}", supportAccountId, ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }
        }
    }
}
