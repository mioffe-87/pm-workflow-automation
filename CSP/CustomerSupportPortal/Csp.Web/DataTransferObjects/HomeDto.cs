using System.Collections.Generic;
using System.Web.Mvc;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class HomeDto : DefaultDto {

		public string CurrentAccount { get; set; }

		public IEnumerable<string> CurrentRoles { get; set; }

		public IEnumerable<ResourceLink> Resources { get; set; }
		
		public bool IsDefault { get; set; }

		public long? CurrentSupportAccountId { get; set; }

		public long? DefaultSupportAccountId { get; set; }

		public long NumberOfAccounts { get; set; }

		public bool HasAnyPanAccess { get; set; }

        public bool HasPendingMemberships { get; set; }

		public HomeDto() {
			CurrentRoles = new string[0];
		}
	}
}