using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class MembershipDto : DefaultDto {
		#region Private Members

		private Membership membership;

		#endregion


		#region Public Properties

		public MembershipDto() {

			RoleAssignment = new RoleAssignmentDto();
		}

		public Membership Membership {
			get {
				return membership;
			}
			set {
				membership = value;
			}
		}

		public RoleAssignmentDto RoleAssignment {
			get;
			private set;
		}


		#endregion


	}
}