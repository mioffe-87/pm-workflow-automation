using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.HelperClasses {

	//This does not appear to be used. 
	public class RoleComparer : IEqualityComparer<Role> {


		bool IEqualityComparer<Role>.Equals(Role role, Role comparedRole) {
			return (role.Id.Equals(comparedRole.Id));
		}

		int IEqualityComparer<Role>.GetHashCode(Role role) {

			if (Object.ReferenceEquals(role, null)) {
				return 0;
			}

			return role.Id.GetHashCode();
		}
	}
}
