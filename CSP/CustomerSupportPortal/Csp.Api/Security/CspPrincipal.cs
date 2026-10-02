using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Csp.Api.Model;
using Newtonsoft.Json;

namespace Csp.Api.Security {


	public class CspPrincipal : IPrincipal {

		public bool IsValid() {
			return UserAccount != null && UserAccount.Id > 0;
        }

		public bool IsInRole(string role) {
			return false;
		}

		public CspPrincipal() {
			ActivePermissions = new HashSet<Permission>();
			UserAccount = new UserAccount();
			RoleSecurities = new List<RoleSecurity>();
		}

		public long? GroupIdForPermissionSet {
			get;
			set;
		}

		public long? SupportAccountIdForPermissionSet {
			get;
			set;
		}


		public UserAccount UserAccount {
			get;
			set;
		}
        public SupportAccount SupportAccount
        {
            get;
            set;
        }

        public HashSet<Permission> ActivePermissions {
			get;
			set;
		}


		public IIdentity Identity {
			get;
			set;
		}

		public Boolean HasPermission(Permission permission) {
			return ActivePermissions.Contains(permission);
		}

		public List<RoleSecurity> RoleSecurities { get; set; }

        public List<SessionRole> Roles { get; set; }

        public Group Group
        {
            get;
            set;
        }
    }

	public class RoleSecurity {

		public RoleSecurity() {
			SupportAccountId = null;
			GroupId = null;
			_securityClassificationNumSet = false;
			_categoryNumSet = false;
		}

		public bool IsValid() {
			if (SupportAccountId.HasValue == GroupId.HasValue) return false;

			if (_securityClassificationNumSet == false) return false;

			if (_categoryNumSet == false) return false;

			return true;
		}

		public long? SupportAccountId { get; set; }
		public long? GroupId { get; set; }

		private bool _securityClassificationNumSet;
		public int SecurityClassificationNum {
			set {
				SecurityClassifications tempSecurityClassification;
				if (Enum.TryParse(value.ToString(), out tempSecurityClassification)) {
					_securityClassificationNumSet = true;
					SecurityClassification = tempSecurityClassification;
				} else {
					_securityClassificationNumSet = false;
					throw new ArgumentOutOfRangeException("Security Classification is out of range");
				}
			}
		}
		public SecurityClassifications SecurityClassification { get; private set; }

		private bool _categoryNumSet;
		public int CategoryNum {
			set {
				Categories tempCategory;
				if (Enum.TryParse(value.ToString(), out tempCategory)) {
					_categoryNumSet = true;
					Category = tempCategory;
				} else {
					_categoryNumSet = false;
					throw new ArgumentOutOfRangeException("Category is out of range");
				}
			}
		}
		public Categories Category { get; private set; }

		public enum SecurityClassifications {
			Standard = 1,
			Super = 2,
			Asc = 3,
			Tac = 4,
            Fedramp =5
		}

		public enum Categories {
			Account = 1,
			Group = 2,
		}
	    
	}

	public static class RoleSecurityHelper {

		public static bool IsTac(this IEnumerable<RoleSecurity> roleSecurities, long supportAccountId) {
			var isTac = roleSecurities.Any(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Tac
				&& x.SupportAccountId != supportAccountId);
			return isTac;
		}

		public static long? GetTacSupportAccountId(this IEnumerable<RoleSecurity> roleSecurities) {
			//return roleSecurities.Where(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Tac).Select(x => x.SupportAccountId).FirstOrDefault(); // for testing only
            return roleSecurities.Where(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Tac).Select(x => x.SupportAccountId).FirstOrDefault();
        }

		public static bool IsSupportAccountAdmin(this IEnumerable<RoleSecurity> roleSecurities, long supportAccountId) {
			var isAdmin = roleSecurities.Any(x =>
																				 x.SecurityClassification == RoleSecurity.SecurityClassifications.Super
																				 && x.Category == RoleSecurity.Categories.Account
																				 && x.SupportAccountId == supportAccountId);

			return isAdmin;
		}

		public static long[] GetAdminSupportAccountIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var adminSupportAccountIds = roleSecurities.Where(x =>
																				 x.SecurityClassification == RoleSecurity.SecurityClassifications.Super
																				 && x.Category == RoleSecurity.Categories.Account && x.SupportAccountId.HasValue).Select(x => x.SupportAccountId.Value);

			return adminSupportAccountIds.ToArray();
		}

		public static bool IsSupportAccountStandard(this IEnumerable<RoleSecurity> roleSecurities, long supportAccountId) {
			var isAdmin = roleSecurities.Any(x =>
																				 x.SecurityClassification == RoleSecurity.SecurityClassifications.Standard
																				 && x.Category == RoleSecurity.Categories.Account
																				 && x.SupportAccountId == supportAccountId);

			return isAdmin;
		}

		public static long[] GetStandardSupportAccountIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var stdSupportAccountIds = roleSecurities.Where(x =>
																				 x.SecurityClassification == RoleSecurity.SecurityClassifications.Standard
																				 && x.Category == RoleSecurity.Categories.Account
																				 && x.SupportAccountId.HasValue).Select(x => x.SupportAccountId.Value);

			return stdSupportAccountIds.ToArray();
		}

		public static long[] GetAscSupportAccountIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var ids = from rs in roleSecurities
								where
									rs.SecurityClassification == RoleSecurity.SecurityClassifications.Asc
									&& rs.Category == RoleSecurity.Categories.Account
									&& rs.SupportAccountId.HasValue
								select rs.SupportAccountId.Value;

			return ids.ToArray();
		}

		public static long[] GetGroupIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var ids = from rs in roleSecurities
								where
									rs.Category == RoleSecurity.Categories.Group
									&& rs.GroupId.HasValue
								select rs.GroupId.Value;

			return ids.ToArray();
		}

		public static long[] GetVmSupportAccountIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var ids = from rs in roleSecurities
								where rs.Category == RoleSecurity.Categories.Account
											&& rs.SupportAccountId.HasValue
								select rs.SupportAccountId.Value;

			return ids.Distinct().ToArray();
		}

		public static long[] GetTrapsSupportAccountIds(this IEnumerable<RoleSecurity> roleSecurities) {
			var ids = from rs in roleSecurities
								where rs.Category == RoleSecurity.Categories.Account
											&& rs.SupportAccountId.HasValue
								select rs.SupportAccountId.Value;

			return ids.Distinct().ToArray();
		}

		private const long PanSupportAccountId = 245;
		public static bool HasAnyPanAccess(this IEnumerable<RoleSecurity> roleSecurities) {
			var anyPanAccess = roleSecurities.Any(x => x.SupportAccountId == PanSupportAccountId);
			return anyPanAccess;
		}

        public static bool IsAscUser(this IEnumerable<RoleSecurity> roleSecurities, long supportAccountId) {
            var isAscUser = roleSecurities.Any(x =>
                x.SecurityClassification == RoleSecurity.SecurityClassifications.Asc
                && x.Category == RoleSecurity.Categories.Account
                && x.SupportAccountId == supportAccountId);

            return isAscUser;
        }
	}

    public enum AccountType
    {
        Customer = 1,
        ASC = 2,
        Eval = 4
    }

    public class CspSession
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("account")]
        public Account Account { get; set; }

        [JsonProperty("privileges")]
        public List<Privilege> Privileges { get; set; }

        [JsonProperty("roles")]
        public List<SessionRole> Roles { get; set; }

        [JsonProperty("securityClassifications")]
        public List<SessionSecurityClassification> SecurityClassifications { get; set; }
    }

	public class User
	{
		[JsonProperty("userAccountId")]
		public long Id { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("displayName")]
		public string DisplayName { get; set; }

		[JsonProperty("firstName")]
		public string FirstName { get; set; }

		[JsonProperty("lastName")]
		public string LastName { get; set; }

		[JsonProperty("defaultSupportAccountId")]
		public long? DefaultSupportAccountId { get; set; }

		[JsonProperty("salesforceLoginId")]
		public string SalesforceLoginId { get; set; }

		[JsonProperty("hasMultipleMemberships")]
		public bool HasMultipleMemberships { get; set; }

		[JsonProperty("isValid")]
		public bool IsValid { get; set; }

		[JsonProperty("membershipStatus")]
		public string MembershipStatus { get; set; }

		[JsonProperty("isMembershipExpired")]
		public bool IsMembershipExpired { get; set; }
		
		[JsonProperty("selectedMembershipId")]
        public long SelectedMembershipId { get; set; }

		[JsonProperty("theme")]
        public string Theme { get; set; }

		[JsonProperty("contactInformation")]
		public string ContactInformation { get; set; }

    }

    public class Account
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("roles")]
        public List<Role> Roles { get; set; }

        [JsonProperty("accountTypeId")]
        public string AccountTypeId { get; set; }

        [JsonProperty("salesforceAccountId")]
        public string SalesforceAccountId { get; set; }

        [JsonProperty("isFedramp")]
        public bool IsFedramp { get; set; }

		 [JsonProperty("isPilotCustomer")]
        public bool IsPilotCustomer { get; set; }

		[JsonProperty("impersonateSetting")]
		public string ImpersonateSetting { get; set; }

		[JsonProperty("isAIEligible")]
		public bool IsAIEligible { get; set; }

        [JsonProperty("isClassicEligible")]
        public bool IsClassicEligible { get; set; }

        [JsonProperty("showHomepageBanner")]
        public bool ShowHomepageBanner { get; set; }

    }

    public class SessionRole
    {
        [JsonProperty("roleId")]
        public int RoleId { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("isInternal")]
        public bool IsInternal { get; set; }
    }

    public class Privilege
    {
        [JsonProperty("privilegeId")]
        public int PrivilegeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("permission")]
        public int Permission { get; set; }

        [JsonProperty("roleId")]
        public int? RoleId { get; set; }
    }

    public class SessionSecurityClassification
    {
        [JsonProperty("membershipId")]
        public long MembershipId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("groupId")]
        public long GroupId { get; set; }

        [JsonProperty("roleId")]
        public int RoleId { get; set; }

        [JsonProperty("securityClassification")]
        public int SecurityClassification { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }
    }

    public class UserStatus
    {
        [JsonProperty("log_user_status_id")]
        public long LogUserStatusId { get; set; }

        [JsonProperty("user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class EnableUser
    {
        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("isActive")]
        public bool isActive { get; set; }

        [JsonProperty("updateByUserAccountId")]
        public long updateByUserAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public string userAccountId { get; set; }
    }
    public class UnblockUser
    {
       
	    [JsonProperty("email")]
        public string email { get; set; }
		
        [JsonProperty("isBlock")]
        public bool isBlock { get; set; }

        [JsonProperty("updateByUserAccountId")]
        public long updateByUserAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long userAccountId { get; set; }
    }
}
