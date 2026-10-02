using System.Collections.Generic;
using Csp.Api.Logic;

namespace Csp.Api.Services.Contracts {

	public interface ILogDeviceRepository {

		IEnumerable<TransferLicenseAvailableDevice> SearchArchivedDevices(long restrictedByUserAccountId, long supportAccountId, string searchText);
	}

	public class ArchivedDeviceModel {
		public long DeviceId { get; set; }
	}

	public static class PanDbIds {
		public const string Dev = "DEV";
		public const string Oss = "OSS";
		public const string Spare = "SPARE";
	}
}
