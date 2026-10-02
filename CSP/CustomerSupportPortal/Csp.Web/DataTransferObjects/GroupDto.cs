using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using Csp.Web.Controllers;

namespace Csp.Web.DataTransferObjects {

	public class GroupDto : DefaultDto {



		#region Private Members

		private Group group;

		#endregion


		#region Public Members

		public enum CreateState {
			NoCreate,
			PreCreate,
			PostCreate
		}


		public GroupDto() {

			RoleAssignment = new RoleAssignmentDto();
		}



		public Group Group {
			get {
				return group;
			}
			set {
				group = value;
			}
		}

		public RoleAssignmentDto RoleAssignment {
			get;
			private set;
		}

		public CreateState State {
			get;
			set;
		}

		public GroupDestination? GroupDestination { get; set; }

		#endregion


	}
}