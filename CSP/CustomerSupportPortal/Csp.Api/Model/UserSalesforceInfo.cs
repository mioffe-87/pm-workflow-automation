using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {

	public class UserSalesforceInfo {
		public long? SelectedMembershipId {
			get;
			set;
		}

		public string SelectedSalesForceLoginId {
			get;
			set;
		}

		public string SelectedSalesForceContactId {
			get;
			set;
		}

		public string SelectedSalesForceAccountId {
			get;
			set;
		}
	}

}
