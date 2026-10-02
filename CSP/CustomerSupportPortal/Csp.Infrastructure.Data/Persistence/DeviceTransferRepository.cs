using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.EntityClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using PAN.Common.Core.Persistence;
using Csp.Api.HelperClasses;
using Csp.Infrastructure.Data.DBs.CSP;


namespace Csp.Infrastructure.Data.Persistence {

	public class DeviceTransferRepository : IDeviceTransferRepository {
		private readonly IRepository _repository;
	    private readonly IConnectionStrings _connectionStrings;

		public DeviceTransferRepository(IRepository repository, IConnectionStrings connectionStrings) {
			_repository = repository;
		    _connectionStrings = connectionStrings;
		}

		public bool IsDevicePendingTransfer(long deviceId) {

            //var pendingDevice =
            //	from devTrans in _repository.Query<Api.EntityClasses.Devicetransfer>()
            //	where
            //		devTrans.DeviceTransferStateId_Devicetransferstate_.Name == DeviceTransferStates.Pending.ToString()
            //		&& devTrans.DeviceId_Device_.DeviceId == deviceId
            //	select devTrans.Id;

            //return pendingDevice.Any();
            const string query = @"SELECT COUNT(1) > 0 IsDPFT FROM csp.devicetransfers d WHERE d.DeviceId = @p_DeviceId AND
            d.DeviceTransferStateId IN (SELECT ds.Id FROM csp.devicetransferstates ds WHERE ds.Name = @p_DeviceTransferState);";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_DeviceTransferState", DeviceTransferStates.Pending.ToString());
                    command.Parameters.AddWithValue("@p_DeviceId", deviceId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return reader.GetColumnValue<bool?>("IsDPFT") ?? false;
                        }
                    }
                }
                connection.Close();
            }

            return false;
        }

		public long CreatePendingTransfer(BeginDeviceTransferModel model) {

            //var pendingState = _repository.Query<Api.EntityClasses.Devicetransferstate>().Single(x => x.Name == DeviceTransferStates.Pending.ToString());
            //var device = _repository.Load<Api.EntityClasses.Device>(model.DeviceId);
            //var createdBy = _repository.Load<Api.EntityClasses.Useraccount>(model.SourceUserAccountId);
            //var destinationUser = _repository.Query<Api.EntityClasses.Useraccount>().Single(x => x.Email == model.DestinationEmail);

            //var deviceTransfer = new Api.EntityClasses.Devicetransfer {
            //	DeviceTransferStateId_Devicetransferstate_ = pendingState,
            //	DeviceId_Device_ = device,
            //	CreatedBy_Useraccount_ = createdBy,
            //	CreatedOn = DateTime.Now,
            //	DestinationUserAccountId_Useraccount_ = destinationUser,
            //};

            //_repository.SaveAndCommit(deviceTransfer);
            //         long id = deviceTransfer.Id;

            //return id;
            long deviceTransferId = long.MinValue;
            try
            {
                const string query = @"INSERT INTO csp.devicetransfers (DeviceId, DestinationUserAccountId, DeviceTransferStateId, CreatedBy, CreatedOn) 
                                   SELECT @p_DeviceId, (SELECT u.UserAccountId FROM csp.useraccount u WHERE u.Email = @p_email), (SELECT d.Id FROM csp.devicetransferstates d
                                   WHERE d.Name = @p_name), @p_CreatedBy, NOW(); SELECT LAST_INSERT_ID();";
                using (var connection = new MySqlConnection(Secrets.CspDatabase))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                    {
                        command.Parameters.AddWithValue("@p_DeviceId", model.DeviceId);
                        command.Parameters.AddWithValue("@p_email", model.DestinationEmail);
                        command.Parameters.AddWithValue("@p_name", DeviceTransferStates.Pending.ToString());
                        command.Parameters.AddWithValue("@p_CreatedBy", model.SourceUserAccountId);
                        deviceTransferId = Convert.ToInt64(command.ExecuteScalar());
                    }
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("CreatePendingTransfer exception:" + ex.Message, ex));
            }
            return deviceTransferId;
        }

		public IEnumerable<PendingDeviceTransfer> GetAllDevicesPendingTransfer() {

			var allPendingTransfers = GetAllPendingTransfersQuery();

			return allPendingTransfers.ToArray();
		}

	    public IEnumerable<PendingDeviceTransfer> GetDevicesPendingTransferByUserAccountId(long userAccountId, long supportAccountId,long deviceTransferId = 0)
	    {

	        //var allPendingTransfers = GetAllPendingTransfersQuery();

	        //return allPendingTransfers.Where(x => x.DestinationUserAccountId == userAccount.Id).ToArray();

	        var result = new List<PendingDeviceTransfer>();
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
	        {
	            connection.Open();
	            using (var command = new MySqlCommand("GetAllPendingDeviceTransfers", connection)
	                {
	                    CommandType = CommandType.StoredProcedure
	                })
	            {

	                command.Parameters.AddWithValue("@p_UserAccountId", userAccountId);
                    command.Parameters.AddWithValue("@p_supportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("@p_devicetTransferId", deviceTransferId);

	                using (var reader = command.ExecuteReader())
	                {
	                    while (reader.Read())
	                    {
	                        var deviceTransfer = new PendingDeviceTransfer()
	                        {
                                CreatedBy = reader.GetColumnValue<long>("createdby"),
                                CreatedByDisplayName = reader.GetColumnValue<string>("FromUser"),
                                CreatedOn = reader.GetColumnValue<DateTime>("createdon"),
                                DestinationUserAccountId = reader.GetColumnValue<long>("DestinationUserAccountId"),
                                DeviceId = reader.GetColumnValue<long>("deviceId"),
                                DeviceName = reader.GetColumnValue<string>("DeviceName"),
                                DeviceTransferId = reader.GetColumnValue<long>("DeviceTransferId"),
                                PartNumber = reader.GetColumnValue<string>("PartNumber"),
                                SerialNumber = reader.GetColumnValue<string>("SerialNumber"),
                                SourceSupportAccountId = reader.GetColumnValue<string>("SourceSupportAccountId"),
                                FromUser = reader.GetColumnValue<string>("ToUser"),
                            };
	                        result.Add(deviceTransfer);
	                    }
	                }

	            }
	        }

	        return result.ToArray();
	    }

	    private IQueryable<PendingDeviceTransfer> GetAllPendingTransfersQuery() {
			var pendingTransfers =
				from devTrans in _repository.Query<Api.EntityClasses.Devicetransfer>()
				where devTrans.DeviceTransferStateId_Devicetransferstate_.Name == DeviceTransferStates.Pending.ToString()
				select new PendingDeviceTransfer {
					DeviceTransferId = devTrans.Id,
					DeviceId = devTrans.DeviceId_Device_.DeviceId,
					DeviceName = devTrans.DeviceId_Device_.DeviceName,
					SerialNumber = devTrans.DeviceId_Device_.SerialNumber,
					PartNumber = devTrans.DeviceId_Device_.PartMasterId_Partmaster_.PartNumber,
					DestinationUserAccountId = devTrans.DestinationUserAccountId_Useraccount_.UserAccountId,
					CreatedOn = devTrans.CreatedOn,
					CreatedBy = devTrans.CreatedBy_Useraccount_.UserAccountId,
					CreatedByDisplayName = devTrans.CreatedBy_Useraccount_.DisplayName,
				};

			return pendingTransfers;


		}

		public void ApprovePendingTransfer(long deviceTransferId,long userAccountId) {

            //var transfer = _repository.Get<Api.EntityClasses.Devicetransfer>(deviceTransferId);

            //transfer.DeviceTransferStateId_Devicetransferstate_ =
            //	_repository.Query<Api.EntityClasses.Devicetransferstate>()
            //						 .Single(x => x.Name == DeviceTransferStates.Accepted.ToString());

            //         transfer.ReviewedOn = DateTime.Now;
            //         transfer.ReviewedBy_Useraccount_ = _repository.Query<Api.EntityClasses.Useraccount>()
            //                 .Single(x => x.UserAccountId == userAccountId);

            //         _repository.SaveAndCommit(transfer);

            UpdateDeviceTransferState(DeviceTransferStates.Accepted.ToString(), deviceTransferId, userAccountId);

        }

		public void RejectPendingTransfer(long deviceTransferId, long userAccountId) {

            //var transfer = _repository.Get<Api.EntityClasses.Devicetransfer>(deviceTransferId);

            //transfer.DeviceTransferStateId_Devicetransferstate_ =
            //	_repository.Query<Api.EntityClasses.Devicetransferstate>()
            //						 .Single(x => x.Name == DeviceTransferStates.Rejected.ToString());
            //transfer.ReviewedOn = DateTime.Now;
            //transfer.ReviewedBy_Useraccount_ =
            //	_repository.Query<Api.EntityClasses.Useraccount>()
            //						 .Single(x => x.UserAccountId == userAccountId);

            //_repository.SaveAndCommit(transfer);

            UpdateDeviceTransferState(DeviceTransferStates.Rejected.ToString(), deviceTransferId, userAccountId);

        }

		public PreDeviceTransferData GetPreTransferData(long deviceTransferId) {

			//var dt = _repository.Load<Api.EntityClasses.Devicetransfer>(deviceTransferId);

			//return new PreDeviceTransferData {
			//	DeviceId = dt.DeviceId_Device_.DeviceId,
			//	CreatedByDisplayName = dt.CreatedBy_Useraccount_.DisplayName,
			//};

            var preDeviceTransferData = new PreDeviceTransferData();

            const string query = @"SELECT d.DeviceId, u.DisplayName FROM csp.devicetransfers d  INNER JOIN csp.useraccount u ON u.UserAccountId = d.CreatedBy WHERE d.Id = @p_deviceTransferId;";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_deviceTransferId", deviceTransferId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            preDeviceTransferData.DeviceId = reader.GetColumnValue<long>("DeviceId");
                            preDeviceTransferData.CreatedByDisplayName = reader.GetColumnValue<string>("DisplayName");
                        }
                    }
                }
                connection.Close();
            }

            return preDeviceTransferData;
        }

		/// <summary>
		/// When transferring devices we must remove access rights from the source account.
		/// Specifically the tables GroupDevices and Membership where DeviceID = X
		/// </summary>
		/// <param name="deviceId"></param>
		public void RemoveSourcePermissions(long deviceId) {

            //var groupDevicePermissions =
            //	_repository.Query<Api.EntityClasses.Groupdevice>(x => x.DeviceId_Device_.DeviceId == deviceId);

            //var directMembershipPermissions =
            //	_repository.Query<Api.EntityClasses.Membership>(x => x.DeviceId_Device_.DeviceId == deviceId);

            //_repository.DeleteAll(groupDevicePermissions);
            //_repository.DeleteAll(directMembershipPermissions);

            //_repository.Commit();

            const string query = @"DELETE FROM csp.groupdevices WHERE DeviceId = @p_DeviceId; DELETE FROM csp.membership WHERE DeviceId = @p_DeviceId;";

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_DeviceId", deviceId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

		public LatestDeviceTransferStatus[] GetLatestDeviceTransferStatuses(long[] deviceIds) {

			var pendingState = DeviceTransferStates.Pending.ToString();
			var acceptedState = DeviceTransferStates.Accepted.ToString();
			var rejectedState = DeviceTransferStates.Rejected.ToString();
			var cancelledState = DeviceTransferStates.Cancelled.ToString();


			var transfers =
				(from dt in _repository.Query<Devicetransfer>()
				 where deviceIds.Contains(dt.DeviceId_Device_.DeviceId)
				 select new LatestDeviceTransferStatus {
					 DeviceId = dt.DeviceId_Device_.DeviceId,
					 State =
					 dt.DeviceTransferStateId_Devicetransferstate_.Name == pendingState
					 ? DeviceTransferStates.Pending
					 : dt.DeviceTransferStateId_Devicetransferstate_.Name == acceptedState
					 ? DeviceTransferStates.Accepted
					 : dt.DeviceTransferStateId_Devicetransferstate_.Name == rejectedState
					 ? DeviceTransferStates.Rejected
					 : DeviceTransferStates.Cancelled,
					 CreatedOn = dt.CreatedOn,
					 CreatedBy = dt.CreatedBy_Useraccount_.UserAccountId,
					 CreatedByDisplayName = dt.CreatedBy_Useraccount_.FirstName + " " + dt.CreatedBy_Useraccount_.LastName,

					 ReviewedOn = dt.ReviewedOn,
					 ReviewedBy = dt.ReviewedBy_Useraccount_ == null ? (long?)null : dt.ReviewedBy_Useraccount_.UserAccountId,
					 ReviewedByDisplayName = dt.ReviewedBy_Useraccount_ == null ? "" : dt.ReviewedBy_Useraccount_.FirstName + " " + dt.ReviewedBy_Useraccount_.LastName,
				 }).ToArray();

			var latest = new List<LatestDeviceTransferStatus>();
			foreach (var id in deviceIds) {
				var match = transfers.Where(x => x.DeviceId == id).OrderByDescending(x => x.CreatedOn).FirstOrDefault();

				if (match != null) {
					latest.Add(match);
				}
			}

			return latest.ToArray();
		}

        public PendingDeviceTransferDetails GetPendingDeviceTransferDetails(long deviceId) {
            var pendingState = DeviceTransferStates.Pending.ToString();

            var db = SessionDbContext.CspObjectContext;

            var deviceTransfers =
                from dt in db.devicetransfers

                join dev in db.devices on dt.DeviceId equals dev.DeviceId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join srcUsr in db.useraccounts on dt.CreatedBy equals srcUsr.UserAccountId into tempSrcUsr
                from leftSrcUsr in tempSrcUsr.DefaultIfEmpty()

                join desUsr in db.useraccounts on dt.DestinationUserAccountId equals desUsr.UserAccountId into tempDesUsr
                from leftDesUsr in tempDesUsr.DefaultIfEmpty()

                join dts in db.devicetransferstates on dt.DeviceTransferStateId equals dts.Id 

                where
                    dt.DeviceId == deviceId
                    && dts.Name == pendingState

                select new PendingDeviceTransferDetails()
                {
                    DeviceTransferId = dt.DeviceTransferStateId,
                    DeviceId = dt.DeviceId,
                    DeviceName = (leftDev != null) ? leftDev.DeviceName : null,
                    SerialNumber = (leftDev != null) ? leftDev.SerialNumber : null,
                    PartNumber = null,
                    CreatedOn = dt.CreatedOn,
                    CreatedBy = dt.CreatedBy,
                    CreatedByDisplayName = (leftSrcUsr != null) ? leftSrcUsr.DisplayName : null,
                    CreatedByFirstName = (leftSrcUsr != null && !string.IsNullOrEmpty(leftSrcUsr.FirstName)) ? leftSrcUsr.FirstName : string.Empty,
                    CreatedByLastName = (leftSrcUsr != null && !string.IsNullOrEmpty(leftSrcUsr.LastName)) ? leftSrcUsr.LastName : string.Empty,
                    CreatedByEmail = (leftSrcUsr != null) ? leftSrcUsr.Email : null,
                    DestinationUserAccountId = dt.DestinationUserAccountId,
                    DestinationUserAccountDisplayName = (leftDesUsr != null) ? leftDesUsr.DisplayName : null,
                    DestinationUserAccountFirstName = (leftDesUsr != null && !string.IsNullOrEmpty(leftDesUsr.FirstName)) ? leftDesUsr.FirstName : string.Empty,
                    DestinationUserAccountLastName = (leftDesUsr != null && !string.IsNullOrEmpty(leftDesUsr.LastName)) ? leftDesUsr.LastName : string.Empty,
                    DestinationUserAccountEmail = (leftDesUsr != null) ? leftDesUsr.Email : null
                };

            return deviceTransfers.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
        }

        //check for  ELA/ESA
        public string ValidateSupportAccountForDeviceTransfer(long supportAccountId, long? devId = null, string identifier = "")
        {
            string panDbIdentification = string.Empty;
            try
            {
                using (var cspdb = new cspEntities())
                {
                    var command = cspdb.Database.Connection.CreateCommand();
                    command.CommandText = "ValidateSupportAccountForDeviceTransfer";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("p_SupportAccountId", supportAccountId));
                    command.Parameters.Add(new MySqlParameter("p_DeviceId", devId));
                    command.Parameters.Add(new MySqlParameter("p_Identifier", identifier));
                    command.CommandTimeout = 120;
                    command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            panDbIdentification = Convert.ToString(reader["PanDbIdentification"] != DBNull.Value ? reader["PanDbIdentification"] : string.Empty);
                        }
                    }
                    command.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("ValidateSupportAccountForDeviceTransfer exception:" + ex.Message, ex));
            }
            return panDbIdentification;
        }

        private void UpdateDeviceTransferState(string deviceTransferStates, long deviceTransferId, long userAccountId)
        {
            const string query = @"UPDATE csp.devicetransfers d SET d.DeviceTransferStateId = (SELECT d.Id FROM csp.devicetransferstates d WHERE d.Name = @p_name)
                                , d.ReviewedBy = @p_ReviewedBy, d.ReviewedOn = NOW() WHERE d.Id = @p_DeviceTransferId;";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_name", deviceTransferStates);
                    command.Parameters.AddWithValue("@p_DeviceTransferId", deviceTransferId);
                    command.Parameters.AddWithValue("@p_ReviewedBy", userAccountId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
	}
}
