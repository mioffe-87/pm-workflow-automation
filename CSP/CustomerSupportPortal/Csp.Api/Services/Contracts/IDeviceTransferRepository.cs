using System;
using System.Collections.Generic;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface IDeviceTransferRepository {
		bool IsDevicePendingTransfer(long deviceId);
		long CreatePendingTransfer(BeginDeviceTransferModel model);
		IEnumerable<PendingDeviceTransfer> GetAllDevicesPendingTransfer();
		IEnumerable<PendingDeviceTransfer> GetDevicesPendingTransferByUserAccountId(long userAccountId,long supportAccountId,long deviceTransferId = 0);
        void ApprovePendingTransfer(long deviceTransferId, long userAccountId);
        void RejectPendingTransfer(long deviceTransferId, long userAccountId);
		PreDeviceTransferData GetPreTransferData(long deviceTransferId);
		void RemoveSourcePermissions(long deviceId);
		LatestDeviceTransferStatus[] GetLatestDeviceTransferStatuses(long[] deviceIds);
        PendingDeviceTransferDetails GetPendingDeviceTransferDetails(long deviceId);
        string ValidateSupportAccountForDeviceTransfer(long supportAccountId, long? devId = null, string identifier = "");
	}

	public class LatestDeviceTransferStatus {

		public long DeviceId { get; set; }
		public DeviceTransferStates State { get; set; }
		public long CreatedBy { get; set; }
		public string CreatedByDisplayName { get; set; }
		public DateTime CreatedOn { get; set; }
		public long? ReviewedBy { get; set; }
		public string ReviewedByDisplayName { get; set; }
		public DateTime? ReviewedOn { get; set; }
	}

	public class PendingDeviceTransfer {
		public long DeviceTransferId { get; set; }
		public long DeviceId { get; set; }
		public string DeviceName { get; set; }
		public string SerialNumber { get; set; }
		public string PartNumber { get; set; }
		public long DestinationUserAccountId { get; set; }
		public DateTime CreatedOn { get; set; }
		public long CreatedBy { get; set; }
		public string CreatedByDisplayName { get; set; }
        public string SourceSupportAccountId { get; set; }
        public string FromUser{ get; set; }
    }

	public class PreDeviceTransferData {
		public long DeviceId { get; set; }
		public string CreatedByDisplayName { get; set; }
	}

    public class PendingDeviceTransferDetails {
        public long DeviceTransferId { get; set; }
        public long DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByDisplayName { get; set; }
        public string CreatedByFirstName { get; set; }
        public string CreatedByLastName { get; set; }
        public string CreatedByEmail { get; set; }
        public long DestinationUserAccountId { get; set; }
        public string DestinationUserAccountDisplayName { get; set; }
        public string DestinationUserAccountFirstName { get; set; }
        public string DestinationUserAccountLastName { get; set; }
        public string DestinationUserAccountEmail { get; set; }
    }
}
