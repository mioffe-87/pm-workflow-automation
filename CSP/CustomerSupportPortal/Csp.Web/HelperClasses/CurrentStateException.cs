using System;
using System.Linq;
using System.Xml.Linq;

namespace Csp.Web.HelperClasses {
	internal static class CurrentStateException {

		public static Exception GetCurrentStateException() {

			var perms = CurrentState.GetPermissionsForLogging().Select(p => new XElement(p)).ToList();

			var curUser = CurrentState.GetLoggedInUser();
			var curUserName = curUser == null
													? "NO USER LOGGED IN"
													: string.Format("{0} ({1})", curUser.UserAccount.DisplayName, curUser.UserAccount.Id);

			var xml =
				new XElement("CurrentState",
										 new XElement("LoggedIn",
																	new XElement("CurrentUser", curUserName),
																	new XElement("SupportAccountId", CurrentState.GetSupportAccountId())
																	),
										 new XElement("Current",
																	new XElement("CustomerSupportAccountId", CurrentState.GetSupportAccountId()),
																	new XElement("GroupId", CurrentState.GetGroupId()),
																	new XElement("DeviceId", CurrentState.GetDeviceId())
																	),
										 new XElement("Permissions", perms)
					);

			var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xml);

			return new Exception(xdoc.ToString());
		}
	}
}