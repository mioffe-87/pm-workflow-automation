using System.Collections.Generic;
using System.ComponentModel;
using Csp.Api.Logic;

namespace Csp.Web.DataTransferObjects {
	public class TransferLicenseDto : DefaultDto {

		public TransferLicenseDto() {
			AvailableDevices = new TransferLicenseAvailableDevice[0];
		}

		public TransferLicenseAvailableDevice PreSelectedDevice { get; set; }

		public IEnumerable<TransferLicenseAvailableDevice> AvailableDevices { get; set; }

		public InitialState State { get; set; }

		public bool AllowSwap { get; set; }

		public long? UserSelectedDeviceId { get; set; }

		public bool? TransferResult { get; set; }

		public PostBackMode Mode { get; set; }
		[DisplayName("Search: ")]
		public string SearchArchivedText { get; set; }

		public enum PostBackMode {
			Transfer,
			SearchArchived,
			TransferArchived,
		}

		public enum InitialState {
			HasSrc,
			HasDest,
		}
	}
}