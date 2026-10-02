using System.Collections.Generic;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {

	public interface ILicenseTransferLogic {

		bool IsValidSourceForTransfer(long srcDeviceId, long userAccountId, long supportAccountId, IEnumerable<DeviceListItemModelCompatibility> devices = null);

		bool IsValidDestinationForTransfer(long destDeviceId, long userAccountId, long supportAccountId);
        SupportAccount GetSupportAccountIdforDeviceId(long deviceId);

		TransferLicenseResult TransferLicenses(long srcDeviceId, long destDeviceId, long userAccountId, string ipAddress);

		TransferLicenseResult TransferRmaLicenses(long srcRmaDeviceId, long destDeviceId, long userAccountId, string ipAddress);

		IEnumerable<TransferLicenseAvailableDevice> GetAvailableSrcDevices(long destDeviceId, long userAccountId, long supportAccountId);
		IEnumerable<TransferLicenseAvailableDevice> GetAvailableArchivedSrcDevices(long destDeviceId, string searchText, long userAccountId, long supportAccountId);

		IEnumerable<TransferLicenseAvailableDevice> GetAvailableDestDevices(long srcDeviceId, long userAccountId, long supportAccountId, IEnumerable<DeviceListItemModelCompatibility> devices = null);
	}
}
