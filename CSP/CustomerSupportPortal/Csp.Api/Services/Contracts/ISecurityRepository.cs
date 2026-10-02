using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface ISecurityRepository {
		IEnumerable<RoleSecurity> GetUserAccountRoleSecurities(long userAccountId);
		bool IsTacSuperUser(long userAccountId, long supportAccountId);
	    bool IsTacUser(long userAccountId, long supportAccountId);
	    bool IsTacUserOrAdmin(long userAccountId, long supportAccountId);
		bool CanUserSignInToSupportAccount(long userAccountId, long supportAccountId);
		int GetNumberOfAccountsUserCanAccess(long userAccountId);
		List<SelectableAccount> GetListOfAccountsUserCanAccess(long userAccountId);
		SelectableAccount[] GetAvailableAccounts(long userAccountId, DataParameters dataParameters);
        SelectableAccount[] GetAvailableAccountsForDevice(long userAccountId, DataParameters dataParameters, List<SelectableAccount> ascBklnAccounts);
        bool IsPanEmployeeRegisteredToSupportSite(string email);
		string GetRoles(long userAccountId, long supportAccountId);
		bool HasActiveCanaryLicense(long supportAccountId);
		bool IsThreatResearcher(long userAccountId, long supportAccountId, out NameValue category);
		bool HasFreeCanaryLicenseUses(long supportAccountId);
	    int GetCurrentThreatResearchersCount(long supportAccountId);
        bool IsPartnerForAnySupportAccount(long restrictedByUserAccountId);
	    bool IsPan245Member(long userAccountId);
	    bool IsCsspSupportAccount(long supportAccountId);
	    bool IsSuperUser(long userAccountId, long supportAccountId);
	    bool? IsAutofocusTrialRoleActive(long userAccountId, long supportAccountId);
	    bool IsAutoFocusTrialUser(long userAccountId, long supportAccountId, out NameValue category);
	    bool AutofocusTrialAllowed(long supportAccountId);
	    List<AutoFocusTrialUser> GetAutoFocusTrialUsersForSupportAccount(long supportAccountId);
	    List<long> GetAutofocusTrialUserIdsExpiringToday();
	    void ProcessAutofocusTrialRoleExpiration(long autofocusTrialId);
	    bool HasCanaryLicense(long supportAccountId, out bool isActive);
        bool ShowAtcPartnerRoles(long userAccountId, long supportAccountId, out bool isAtcManagerOnly);
	    bool IsAtcManagerSeatsAvailable(long supportAccountId);
	    bool IsAccountMfaEnabled(long supportAccountId);
        bool HasViewCaseManagementPrivilege(long supportAccountId, long userAccountId);
        bool HasRole(long supportAccountId, long userAccountId,string roleName);
        bool IsValidSupportAccount(long supportAccountId);
        List<RoleSecurity> GetRoleSecuritiesByUserAccountId(long userAccountId);
        bool IsConsentedUser(long userAccountId,long supportAccountId);
    }
}
