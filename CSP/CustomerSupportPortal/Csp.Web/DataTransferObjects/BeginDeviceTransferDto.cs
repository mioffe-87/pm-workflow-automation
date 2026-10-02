using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Csp.Api.Model.Attributes;

namespace Csp.Web.DataTransferObjects {
	public class BeginDeviceTransferDto : DefaultDto {

		[DisplayName("Serial Number:")]
		public string SerialNumber { get; set; }

		[DisplayName("Device Name:")]
		public string DeviceName { get; set; }

		[DisplayName("Part Number:")]
		public string PartNumber { get; set; }

		public long DeviceId { get; set; }

		[DisplayName("Destination Email:"), Email(ErrorMessage = "Please enter a valid email. Public domains are not allowed. Please use your company email address.")]
		public string DestinationEmail { get; set; }

		[DisplayName("Notify User:")]
		public bool NotifyUser { get; set; }

        public DeviceTransferBy TransferBy { get; set; }

        public long SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public bool HasAscTransferAccess { get; set; }
        public bool HasLgsAssociated { get; set; }
    }

    public enum DeviceTransferBy
    {
        EmailRecipient,
        SupportAccount
    }
}