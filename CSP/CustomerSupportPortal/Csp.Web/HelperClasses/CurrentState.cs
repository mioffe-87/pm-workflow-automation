using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using static Csp.Web.HelperClasses.SiteHelper;

namespace Csp.Web.HelperClasses {

	[Serializable]
	public class CurrentState : ISerializable
	{

		#region Private Properties

		private CspPrincipal User { get; set; }

		private HashSet<Permission> LoggedInPermissions { get; set; }

		private SupportAccount SupportAccount { get; set; }

		private Group Group { get; set; }

		private Device Device { get; set; }

		private UserAccount UserAccount { get; set; }

		private String TabTitle { get; set; }

		private bool IsImpersonated { get; set; }

		private CurrentState()
		{
			IsImpersonated = false;
		}

		private string ImpersonatorEmail { get; set; }
		private long ImpersonatorUserAccountId { get; set; }

		#endregion

		#region Private Static Methods

		// Clearing Values ============================================

		private static void ClearGroup()
		{
			SetGroup(null);
			ClearDevice();
		}

		private static void ClearDevice()
		{
			SetDevice(null);
		}

		// Common ======================================================

		private static CspPrincipal GetCurrentState()
		{
			/*if (HttpContext.Current.Session == null)
				return null;
			var currentState = (CurrentState)HttpContext.Current.Session["currentUser"];
			if (currentState == null) {
				currentState = new CurrentState();
				SetCurrentState(currentState);
			}
			return currentState;*/

			if ((HttpContext.Current.User != null) && (HttpContext.Current.User is CspPrincipal))
			{
				return (CspPrincipal)HttpContext.Current.User;
			}
			return null;
		}

		private static void SetCurrentState(CurrentState currentState)
		{
			//HttpContext.Current.Session.Add("currentUser", currentState);
		}

		#endregion

		#region Public Static Methods

		// Common ============================================================

		public static void ClearAll()
		{
			var currentState = new CurrentState();
			SetCurrentState(currentState);
		}

		public static String GetDisplayName()
		{
			var currentState = GetCurrentState();
			var userAccount = (currentState != null && currentState.UserAccount != null) ? currentState.UserAccount : null;

			return (userAccount != null) ? string.Format("{0} {1}", userAccount.FirstName?.Trim(), userAccount.LastName?.Trim()) : string.Empty;
		}

		public static bool HasLoggedInPermission(Permission permission)
		{
			var currentState = GetCurrentState();
			return currentState != null && currentState.ActivePermissions != null && currentState.ActivePermissions.Contains(permission);
		}

		public static IEnumerable<string> GetPermissionsForLogging()
		{
			var currentState = GetCurrentState();

			if (currentState == null) return new string[0];
			if (currentState.ActivePermissions == null) return new string[0];

			return currentState.ActivePermissions.Select(x => x.ToString());
		}

		public static bool HasMultipleAccounts()
		{
			var currentState = GetCurrentState();

			if (currentState?.UserAccount == null) return false;

			var hasMultipleAccounts =
				currentState.RoleSecurities
					.Where(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Tac).Any() ||
				currentState.UserAccount.HasMultipleMemberships;

			return hasMultipleAccounts;
		}

		public static bool GetIsImpersonated()
		{
			var isImpersonated = false;
			var userInfo = UserHelper.GetSsoToken();

			if (userInfo != null)
			{
				isImpersonated = userInfo["isimpersonated"] != null && userInfo["isimpersonated"].Equals("true");
			}

			return isImpersonated;
		}

		// LoggedInUser ======================================================

		public static void SetLoggedInUser(CspPrincipal user)
		{
			// Comments by LK on 08.26
			// Need to review with SS
			/*CurrentState currentState = GetCurrentState();
			if (currentState == null)
				return;
			currentState.User = user;*/
		}

		public static CspPrincipal GetLoggedInUser()
		{
			return GetCurrentState();
		}

		// CustomerSupportAccount ==============================================

		public static void SetSupportAccount(SupportAccount supportAccount)
		{
			// Comments by LK on 08.26
			// Need to review with SS
			/*ClearGroup();
			CurrentState currentState = GetCurrentState();
			currentState.LoggedInPermissions = (supportAccount == null || currentState.User == null) ? null : currentState.User.ActivePermissions;
			currentState.SupportAccount = supportAccount;*/
		}

		public static SupportAccount GetSupportAccount()
		{
			var currentState = GetCurrentState();

			return currentState?.SupportAccount;
		}

		public static bool IsAccountLoaded()
		{
			return (GetSupportAccount() != null);
		}

		/// <summary>
		/// Returns the support account ID being viewed by an ASC (if one exists)
		/// </summary>
		/// <returns></returns>
		public static long? GetSupportAccountId()
		{
			var supportAccount = GetSupportAccount();

			return supportAccount?.Id;
		}

		/// <summary>
		/// Gets the support account id 
		/// </summary>
		/// <returns></returns>
		public static long GetCurrentAccountId()
		{
			var customer = GetSupportAccountId();
			long returnId = 0;
			if (customer.HasValue)
				returnId = customer.Value;
			if (returnId == 0)
				throw new Exception("CurrentState.GetCurrentAccountId(): Current AccountId can't be 0.  This has been called in an invalid state.");
			return returnId;
		}

		public static String GetSupportAccountName()
		{
			var supportAccount = GetSupportAccount();

			return supportAccount?.Name?.Trim();
		}

		// Group ======================================================

		public static void SetGroup(Group group)
		{
			// Comments by LK on 08.26
			// Need to review with SS
			/*CurrentState currentState = GetCurrentState();
			if (currentState == null)
				return;
			currentState.Group = group;
			if (IsAccountLoaded() == false)
				currentState.LoggedInPermissions = (group == null || currentState.User == null) ? null : currentState.User.ActivePermissions;*/
		}

		public static Group GetGroup()
		{
			var currentState = GetCurrentState();

			return currentState?.Group;
		}

		public static long? GetGroupId()
		{
			Group group = GetGroup();
			return (group != null) ? group.Id : (long?)null;
		}

		public static String GetGroupName()
		{
			Group group = GetGroup();
			return (group != null) ? group.Name?.Trim() : null;
		}


		// Device ======================================================

		public static void SetDevice(Device device)
		{
			// Comments by LK on 08.26
			// Need to review with SS
			/*CurrentState currentState = GetCurrentState();
			if (currentState == null)
				return;
			currentState.Device = device;*/
		}

		private static Device GetDevice()
		{
			// Comments by LK on 08.26
			// Need to review with SS
			/*CurrentState currentState = GetCurrentState();
			return (currentState == null) ? null : currentState.Device;*/
			return null;
		}

		public static long? GetDeviceId()
		{
			Device device = GetDevice();
			return (device != null) ? device.Id : (long?)null;
		}

		public static String GetDeviceName()
		{
			Device device = GetDevice();
			return (device != null) ? device.DeviceName : null;
		}

		public static String GetDeviceSerialNumber()
		{
			Device device = GetDevice();
			return (device != null) ? device.SerialNumber : null;
		}

		#endregion

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{

			info.AddValue("User", User);

			info.AddValue("LoggedInPermissions", LoggedInPermissions);

			info.AddValue("SupportAccount", SupportAccount);

			info.AddValue("Group", Group);

			info.AddValue("Device", Device);

			info.AddValue("UserAccount", UserAccount);

			info.AddValue("TabTitle", TabTitle);

			info.AddValue("IsImpersonated", IsImpersonated);

		}

		public static bool HasDirectMembership()
		{
			var currentState = GetCurrentState();

			if (currentState?.UserAccount == null || currentState.SupportAccount == null) return false;

			return currentState.RoleSecurities.Any(x => x.SupportAccountId == currentState.SupportAccount.Id);
		}

		public static bool HasASCRole()
		{
			var currentState = GetCurrentState();

			if (currentState == null || currentState.UserAccount == null || currentState.SupportAccount == null) return false;

			return currentState.RoleSecurities.Any(x => x.SupportAccountId == currentState.SupportAccount.Id &&
			 x.SecurityClassification.Equals(RoleSecurity.SecurityClassifications.Asc));
		}

		public static string GetSalesforceLoginId()
		{
			var currentState = GetCurrentState();
			if (currentState?.UserAccount == null) return string.Empty;

			return currentState?.UserAccount.SalesforceLoginId?.Trim();

		}

		public static bool IsSE()
		{
			var currentState = GetCurrentState();

			if (currentState?.UserAccount == null || currentState.SupportAccount == null)
				return false;

			var supportAccount = currentState.SupportAccount;
			var isEmployeeAccount = supportAccount.AccountTypeId == 3;
			var hasDirectMembership = currentState.SupportAccount.Id != 245 && currentState.RoleSecurities.Any(x => x.SupportAccountId == currentState.SupportAccount.Id);

			return (isEmployeeAccount && hasDirectMembership);
		}

		public static bool IsPilotCustomer()
		{
			var currentState = GetCurrentState();
			if (currentState == null || currentState.SupportAccount == null) return false;

			return currentState.SupportAccount.IsPilotCustomer;
		}
		
		public static long? GetSelectedMembershipId() {
            var currentState = GetCurrentState();
            var userAccount = (currentState != null && currentState.UserAccount != null) ? currentState.UserAccount : null;

            return userAccount?.SelectedMembershipId;
        }


        public static string GetUserSelectedTheme()
        {
            var currentState = GetCurrentState();
            if (currentState?.UserAccount == null) return string.Empty;

            return currentState?.UserAccount.Theme?.Trim();

        }

        public static string GetUserContactInformation()
        {
            var currentState = GetCurrentState();
            if (currentState?.UserAccount == null) return string.Empty;

            return currentState?.UserAccount.PhoneNumber?.Trim();

        }

        public static bool GetIsAiEligible()
        {
            var currentState = GetCurrentState();
            if (currentState?.SupportAccount == null) return false;

            return (bool)(currentState?.SupportAccount.isAIEligible);

        }

        public static string GetImpersonateSettingAsString()
        {
            var currentState = GetCurrentState();
            if (currentState?.SupportAccount == null) return string.Empty;

            return currentState?.SupportAccount.ImpersonateSettingAsString;
        }

        public static bool GetIsClassicEligible()
        {
            var currentState = GetCurrentState();
            if (currentState?.SupportAccount == null) return false;

            return currentState.SupportAccount.IsClassicEligible;
        }

        public static bool GetShowHomepageBanner()
        {
            var currentState = GetCurrentState();
            if (currentState?.SupportAccount == null) return false;

            return currentState.SupportAccount.ShowHomepageBanner;
        }
    }
}