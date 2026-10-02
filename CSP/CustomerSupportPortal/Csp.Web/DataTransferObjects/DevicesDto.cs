using Csp.Web.Controllers;

namespace Csp.Web.DataTransferObjects {
	public class DevicesDto : DefaultDto {

		public GroupDestination SelectedTab { get; set; }

		public string DefaultDeviceCode { get; set; }

        public bool HasBulkRegistrationAccess { get; set; }

        public bool HasEnterpriseAgreementAccess { get; set; }

	}
}