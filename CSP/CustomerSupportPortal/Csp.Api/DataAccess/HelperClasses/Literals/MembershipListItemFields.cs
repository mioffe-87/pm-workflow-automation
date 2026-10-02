using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses.Literals {

	internal static class MembershipListItemFields {

		internal const string MembershipId = "MembershipId";
		internal const string AccountName = "AccountName";
		internal const string FirstName = "FirstName";
		internal const string LastName = "LastName";
		internal const string DisplayName = "DisplayName";
		internal const string Email = "Email";
		internal const string ActivationDate = "ActivationDate";
		internal const string ExpirationDate = "ExpirationDate";
		internal const string UserAccountId = "UserAccountId";
		internal const string IsSuperUser = "IsSuperUser";
		internal const string RoleNames = "RoleNames";
        internal const string AutoFocusTrialExpirationDate = "AutoFocusTrialExpirationDate";
        internal const string IsMfaEnabled = "IsMfaEnabled";
        internal const string MultiFactorAuthType = "MultiFactorAuthType";
        internal const string CountOfActiveAutofocusLicense = "HasActiveAutofocusLicense";
        internal const string EnableEmailMfa = "EnableEmailMfa";
    }
}
