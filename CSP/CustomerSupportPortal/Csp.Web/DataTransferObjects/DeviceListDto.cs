using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class DeviceListDto : DefaultDto {

		public DeviceListDto() {
			Devices = new List<DeviceListItem>();
		}

		public List<DeviceListItem> Devices { get; set; }

		public bool IsProductionDeviceList { get; set; }

		public List<DeviceCategory> AllDeviceCategories { get; set; }

		public bool ShowReviewPendingTransfersButton { get; set; }

		public string DefaultDeviceCode { get; set; }


	    public bool ShowBulkDeviceTransferButton { get; set; }

	    public bool HideExpiredLicenses { get; set; }

	}
}