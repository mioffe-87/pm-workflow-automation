using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface ISecurityLogic {

		CspPrincipal GetCspPrincipal(string email);

		HashSet<Permission> GetSupportAccountPermissionsForPrincipal(long supportAccountId, UserAccount userAccount);
		HashSet<Permission> GetGroupPermissionsForPrincipal(long groupId, UserAccount userAccount);

		IEnumerable<RoleSecurity> GetUserAccountRoleSecurities(long userAccountId);

		List<Role> GetAvailableRoles(RoleCategory roleCategory, long supportAccountId, long restrictedByUserAccountId);
		List<Role> GetDefaultRoles(RoleCategory roleCategory, long supportAccountId);

		List<SelectableAccount> GetAvailableAccounts(long userAccountId);

	    bool IsPanEmployeeRegisteredToSupportSite(string email);


		bool CanSeeAutoFocusLink(long userAccountId, long supportAccountId);

	    void ProcessAutofocusTrialsExpiringToday();

	    long? GetDefaultSupportAccount(long userAccountId);
        bool HasViewCaseManagementPrivilege(long supportAccountId, long userAccountId);

	    bool HasBpaAccess(long supportAccountId, long userAccountId);
	    bool IsSuperUser(long supportAccountId,long userAccountId);

		bool HasRole(long supportAccountId, long userAccountId,string roleName);

        bool IsValidSupportAccount(long supportAccountId);
        bool IsConsentedUser(long userAccountId, long supportAccountId);       
    
	    bool IsTacUserOrAdmin(long supportAccountId, long userAccountId);
		 List<long> GetAscCustomerAccountsFromSalesForce(List<string> ascSalesForceAccountIds, long userAccountId);

        CspSession GetSessionDetails(string email, long supportAccountId);
        UserStatus GetUserStatus(string email);

		bool HasPermission(long supportAccountId, long userAccountId, Permission permission);
    }
}
