using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Web.DataTransferObjects {

	public class MembershipStatusDto : DefaultDto {

		public MembershipStatus MembershipStatus { get; set; }
		public string ActionType { get; set; }
	}
}