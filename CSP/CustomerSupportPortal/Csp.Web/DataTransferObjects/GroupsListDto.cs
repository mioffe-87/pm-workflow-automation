using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class GroupsListDto : DefaultDto {

		#region "Constuctor"

		public GroupsListDto() {
			Groups = new List<GroupListItem>();
		}

		#endregion

		#region Public Properties

		public List<GroupListItem> Groups {
			get;
			set;
		}

		#endregion

	}
}