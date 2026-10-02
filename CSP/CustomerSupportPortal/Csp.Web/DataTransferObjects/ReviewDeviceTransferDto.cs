using System.Collections.Generic;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class ReviewDeviceTransferDto : DefaultDto {
		public IEnumerable<ReviewDeviceTransferModel> DeviceTransfer { get; set; }
        public EulaModel Eula { get; set; }
	}

    //public enum NotificationCategory
    //{
    //    DEVICE_TRANSFER,
    //    VM_ELA_TOKEN_GRANT
    //}
}