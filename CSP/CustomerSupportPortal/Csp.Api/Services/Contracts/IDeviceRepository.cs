using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {


	public interface IDeviceRepository {

		long ReadSupportAccountIdByDeviceBySerialNumber(string serialNumber);
		List<Api.Model.DeviceWithPartMaster> ReadDevices(long[] deviceIdArray);
		List<WildfireDeviceItem> GetWildfireDevices(long userAccountId, long supportAccountId, DataParameters parameters);
		long ReadDeviceIdBySerialNumber(string serialNumber);
		DeviceTagMatches[] GetDeviceTagsByIds(long[] deviceCategoryIds);
		string[] GetGroupNamesByDeviceId(long deviceId);
	    void UpdateSupportAccountIdBySerialNumber(string serialNumber, long supportAccountId);
        void UpdateSupportAccountIdByDeviceId(long deviceId, long supportAccountId);
		long GetSupportAccountIdByDeviceId(long deviceId);
		bool IsSerialNumberValid(string serialNumber);
		GroupNameId[] GetGroups(long supportAccountId);
		DeviceGroup[] GetDeviceGroups(long supportAccountId);
		GroupNameId[] GetGroups(long[] supportAccountIds);
		DeviceGroup[] GetDeviceGroups(long[] supportAccountIds);
		long GetSupportAccountIdByGroupId(long groupId);

	  bool IsAwsAccountSupportAccountMappingExists(long supportAccountId, string awsAccountId);
	  bool CreateAwsAccountSupportAccountMapping(long supportAccountId, string awsAccountId);
      bool RegisterCloudDevice(long deviceId, long supportAccountId, long userAccountId);
      bool RegisterCloudDevices(long deviceId, long supportAccountId, long userAccountId, string awsAccountId);

		bool UpdateSapIdAndSfdcIdForAwsAccount(long deviceId, long supportAccountId, long userAccountId, out string currentSfdcAccountId, out string newSfdcAccountId);
		bool IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(long supportAccountId, string awsDefaultAccountId);

		bool IsDeviceCodeCirroSecure(string deviceCode, out long devId);

		bool RegisterCirroSecureDevice(long deviceId, long supportAccountId, long userAccountId);

        bool IsInstanceAlreadyRegistered(string serialNumber);
        bool RegisterAzureDevice(long deviceId, long supportAccountId, long userAccountId);
        bool HasAscTransferBetaAccess(long? supportAccountId);
	    List<DeviceCategoryListItem> GetDeviceCategoriesWithDeviceCount(long supportAccountId, long userAccountId, long groupId, string deviceType);
	    bool IsCloudDevice(long deviceId, out int? cloudType);
        IEnumerable<PanOs> GetPanOs();
        IEnumerable<HyperVisor> GetHyperVisors(string filter = null);
	    bool AddDeviceToEsaExclusionList(string serialNumber, long supportAccountId, string user);
	    string GetDevicePartNumber(string serialNumber);
        void CheckGovernmentCloud(List<DeviceListItem> deviceItems);
    }

	public class WildfireDeviceItem {
		public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public bool IsSelected { get; set; }
    }

	public class DeviceTagMatches {
		public long DeviceCategoryId { get; set; }
		public string Name { get; set; }
	}

	public class DeviceGroup {
		public long DeviceId { get; set; }
		public long GroupId { get; set; }
	}
	public class GroupNameId {
		public long GroupId { get; set; }
		public string GroupName { get; set; }
	}
}
