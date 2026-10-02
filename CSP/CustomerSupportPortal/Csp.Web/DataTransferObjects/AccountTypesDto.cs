using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class AccountTypesDto : DefaultDto {
		public AccountTypesDto() {
			AccountTypes = new List<AccountType>();
			AssignableRoleAssignment = new RoleAssignmentDto();
			DefaultRoleAssignment = new RoleAssignmentDto();
		}

		public AccountType AccountType { get; set; }
		public List<AccountType> AccountTypes { get; set; }
		public RoleAssignmentDto AssignableRoleAssignment { get; set; }
		public RoleAssignmentDto DefaultRoleAssignment { get; set; }

	}
}