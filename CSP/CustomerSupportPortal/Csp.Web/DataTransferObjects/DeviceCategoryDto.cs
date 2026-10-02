using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class DeviceCategoryDto : DefaultDto {
		public DeviceCategory DeviceCategory {
			get;
			set;
		}
	}
}