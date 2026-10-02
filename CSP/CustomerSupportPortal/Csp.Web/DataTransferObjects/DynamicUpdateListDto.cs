using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class DynamicUpdateListDto : DefaultDto {

		public DynamicUpdateListDto() {
			DynamicUpdates = new List<SoftwareUpdateListItem>();
		}

		public List<SoftwareUpdateListItem> DynamicUpdates { get; set; }
	}
}