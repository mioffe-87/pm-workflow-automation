using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums {
	public enum MembershipStatus {
		None = 0,
		Pending = 1,
		Accepted = 2,
		Rejected = 3
	}
    public enum ActionType
    {
        NONE,
        ACCEPT,
        REJECT
    }
}
