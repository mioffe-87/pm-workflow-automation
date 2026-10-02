using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class DeviceDto : DefaultDto {

		public Device Device { get; set; }

		public List<DeviceCategory> DeviceCategories { set; get; }
	}
}