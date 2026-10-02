using System.ComponentModel.DataAnnotations;

namespace Csp.Web.DataTransferObjects {
	public class AddDeviceToGroupDto : DefaultDto {
		public long GroupId { get; set; }
		[Required(ErrorMessage = "A Serial Number is required")]
		public long? DeviceId { get; set; }
		public bool IsProduction { get; set; }
        [Required(ErrorMessage = "Device Tag is required")]
        public long? DeviceCategoryId { get; set; }
        public AddDeviceToGroupFilterTypes AddDeviceToGroupFilter { get; set; }
	}

    public enum AddDeviceToGroupFilterTypes
    {
        SerialNumber,
        DeviceCategory
    }
}