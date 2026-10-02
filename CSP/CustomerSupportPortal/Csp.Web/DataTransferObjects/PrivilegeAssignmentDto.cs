using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects {
	public class PrivilegeAssignmentDto {
		public List<PrivilegeDto> Privileges { get; set; }
		public string SelectedPrivilegeIds { get; set; }
	}
}