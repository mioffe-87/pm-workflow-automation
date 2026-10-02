using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csp.Api.Security;


namespace Csp.Api.Delegates {

	/// <summary>
	/// Delegate used as events in the ListUpdateHelper as events
	/// </summary>
	/// <param name="parentKey">The value of the parent key in the list</param>
	/// <param name="childKey">Value of the childKey in the list</param>
	/// <param name="userAccountId">The user account id of the user taking the action</param>
	public delegate void PerformListUpdate(long parentKey, long childKey, long userAccountId);

}
