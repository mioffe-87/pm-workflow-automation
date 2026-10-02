using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class GroupMembershipListDto : DefaultDto {

		#region "Constuctor"

		public GroupMembershipListDto() {
			GroupMemberships = new List<GroupMembershipListItem>();
		}

		#endregion

		#region Public Properties

		public List<GroupMembershipListItem> GroupMemberships {
			get;
			set;
		}


		#endregion
	}
}