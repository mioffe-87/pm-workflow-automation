using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using System.ComponentModel.DataAnnotations;

namespace Csp.Web.DataTransferObjects {

	public class RoleAssignmentDto {

		public List<RoleDto> Roles { get; set; }
		public string SelectedRoleIds { get; set; }

	}
}