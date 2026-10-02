using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums {
	public enum LookupType {
		None = 0,
		[Description("PublicDomains")]
		PublicDomains = 1,
		BlockedIp = 2
	}
}
