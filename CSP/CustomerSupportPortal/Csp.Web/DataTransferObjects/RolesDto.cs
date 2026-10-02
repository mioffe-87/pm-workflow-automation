using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class RolesDto : DefaultDto {
		public RolesDto() {
			Roles = new List<Role>();
		}

		public List<Role> Roles {
			get;
			set;
		}

	}
}