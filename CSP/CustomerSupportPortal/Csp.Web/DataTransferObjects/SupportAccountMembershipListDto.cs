using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class SupportAccountMembershipListDto : DefaultDto {

		#region "Constuctor"

		public SupportAccountMembershipListDto() {
			SupportAccountMemberships = new List<SupportAccountMembershipListItem>();
		}

		#endregion

		#region Public Properties

		public List<SupportAccountMembershipListItem> SupportAccountMemberships {
			get;
			set;
		}

		#endregion

	}
}