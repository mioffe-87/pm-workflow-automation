using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using System.ComponentModel.DataAnnotations;

namespace Csp.Web.DataTransferObjects {

	public class RoleDto : DefaultDto {

		public RoleDto() {
			PrivilegeAssignment = new PrivilegeAssignmentDto();
		}

		public Role Role { get; set; }
		public bool IsSelected { get; set; }
		public PrivilegeAssignmentDto PrivilegeAssignment { get; set; }
	}
}