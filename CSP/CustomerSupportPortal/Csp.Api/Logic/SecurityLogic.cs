using System;
using System.Linq;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Security;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Sfdc;
using Csp.Api.Services.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace Csp.Api.Logic {

	public class SecurityLogic : BaseLogic, ISecurityLogic {

		private readonly ISecurityRepository _securityRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ICurrentStateService _currentStateService;
        private readonly IApplicationSettings _applicationSettings;

        public SecurityLogic(ISecurityRepository securityRepository, IUserAccountRepository userAccountRepository, ICurrentStateService currentStateService,
            IApplicationSettings applicationSettings) {
			_securityRepository = securityRepository;
		    _userAccountRepository = userAccountRepository;
            _currentStateService = currentStateService;
            _applicationSettings = applicationSettings;
        }

		/// <summary>
		///This method gets the custom security principal (CspPrincipal) with user information for the logged in user by the email address. 
		/// </summary>
		/// <param name="email">The email of the user</param>
		/// <returns>CspPrincipal</returns>
		public CspPrincipal GetCspPrincipal(string email) {

			var cspPrincipal = new CspPrincipal();
			var userAccountDataAccess = new UserAccountDataAccess();

			//Get the user account and add it to the principal this is typically used to get the user account id for restricting data that is viewed. 
			var userAccount = userAccountDataAccess.ReadUserByEmailId(email);

			cspPrincipal.UserAccount = userAccount;

			return cspPrincipal;
		}

		/// <summary>
		/// This method returns the a set of permissions the logged in user has for a specific support account. The permissions only apply to the 
		/// support account the user is logged in to and determines what actions or options are available for the user to take. While the viewing 
		/// of an entire set of data can be turned on or off with a permission, the data filtering on visible data is dependent on the security 
		/// classification of the role that is in the membership. Additionally the security classification is used to determine if the permissions
		/// apply to the support account. For example, if the security classification is 1 or 2 then the user must be a member of a support 
		/// account or group (group visibility to an account is a new thing.) If the security classification is 3 the support account must be supported 
		/// by the ASC the user account is a member of. If the security classification is 4 then user must not be a member of the support account. 
		/// </summary>
		/// <param name="supportAccountId">The support account id of the support account that is to be accessed</param>
		/// <param name="userAccount">The user account of the user that is currently logged in</param>
		/// <returns>A hashset of active permissions</returns>
		public HashSet<Permission> GetSupportAccountPermissionsForPrincipal(long supportAccountId, UserAccount userAccount) {

            /*Loads the permissions into a hashset so each permission exists only once in the set. There can be a variety of memberships 
			 * a user can have that could result in a variety of permissions, some that are duplicate. This is a superset of all the permissions 
			 * a user can have for the support account. For example if a user has superuser and standard user privileges then the superset of all
			 * permissions for both roles are added to the set of active permissions.
			 */
            return GetPermissionsForPrincipal(userAccount, supportAccountId);
		}

		/// <summary>
		/// This method returns the a set of permissions the logged in user has for a specific group. The permissions only apply to the 
		/// group the user is logged in to and determines what actions or options are available for the user to take. While the viewing 
		/// of an entire set of data can be turned on or off with a permission, the data filtering on visible data is dependent on the security 
		/// classification of the role that is in the membership. Additionally the security classification is used to determine if the permissions
		/// apply to the group. For example, if the security classification is 1 then the user must be a member of a group but may also have support 
		/// account permissions loaded (group visibility to an account is a new thing.) If the security classification is 3 the support account must be supported 
		/// by the ASC the user account is a member of. If the security classification is 4 then user must not be a member of the support account. 
		/// </summary>
		/// <param name="groupId">The group the permissions are applicable for</param>
		/// <param name="userAccount">The user account that needs the permissions</param>
		/// <returns>A hashset of active permissions</returns>
		public HashSet<Permission> GetGroupPermissionsForPrincipal(long groupId, UserAccount userAccount) {

			/*Loads the permissions into a hashset so each permission exists only once in the set. There can be a variety of memberships 
			 * a user can have that could result in a variety of permissions, some that are duplicate. This is a superset of all the permissions 
			 * a user can have for the group. For example if a user has superuser for a support account and membership to the group 
			 * then the superset of all permissions for both roles are added to the set of active permissions.
			 */
			HashSet<Permission> permissionSet = new HashSet<Permission>();
			SecurityDataAccess securityDataAccess = new SecurityDataAccess();

			permissionSet = securityDataAccess.GetGroupPermissionsForPrincipal(groupId, userAccount);


			return permissionSet;
		}

		public IEnumerable<RoleSecurity> GetUserAccountRoleSecurities(long userAccountId) {

			//return _securityRepository.GetUserAccountRoleSecurities(userAccountId);
            return _securityRepository.GetRoleSecuritiesByUserAccountId(userAccountId);
        }

		public List<Role> GetAvailableRoles(RoleCategory roleCategory, long supportAccountId, long restrictedByUserAccountId) {

			var roleDataAccess = new RoleDataAccess();
			var availableRoles = roleDataAccess.ReadAvailableRoles(roleCategory, supportAccountId);
            var isSuperUser = _securityRepository.IsSuperUser(restrictedByUserAccountId, supportAccountId);
            var isDomainAdmin = _securityRepository.HasRole(supportAccountId, restrictedByUserAccountId, RoleNames.DomainAdministrator);

            MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();
            bool isTacAdmin = membershipRolesDataAccess.GetRolesByUserAccountId(restrictedByUserAccountId, ApiConstants.PANW_SUPPORT_ACCOUNT_ID).Select(x => x == RoleNames.TACAdmin).Any();

            // Comments By LK On 05/15/2016
            //if (availableRoles.Any(r => r.Name == RoleNames.TACSuperUser || r.Name == RoleNames.TACUser || r.Name == RoleNames.BulkRegistration || r.Name == RoleNames.ImpersonateUser)) {
            if (availableRoles.Any(r => r.Name == RoleNames.TACSuperUser || r.Name == RoleNames.TACUser || r.Name == RoleNames.TACAdmin || r.Name == RoleNames.ImpersonateUser)) {
				//This must be PAN - make sure the current user is a tac super user
				if (!_securityRepository.IsTacUserOrAdmin(restrictedByUserAccountId, supportAccountId)) {
                    if (availableRoles.Any(r => r.Name == RoleNames.TACSuperUser))
				    {
				        availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.TACSuperUser));
				    }

                    if (availableRoles.Any(r => r.Name == RoleNames.TACUser))
				    {
				        availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.TACUser));
				    }
                    
                    if (availableRoles.Any(r => r.Name == RoleNames.TACAdmin)) 
                    {
	                    availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.TACAdmin));
                    }

                    if (availableRoles.Any(r => r.Name == RoleNames.ImpersonateUser))
				    {
				        availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.ImpersonateUser));
				    }
				}
			}

			//Check for Threat Researcher and AutoFocus Trial Roles
            if (availableRoles.Any(x => x.Name.Contains(RoleNames.ThreatResearcher)) || availableRoles.Any(x => x.Name.Contains(RoleNames.AutofocusTrial)))
            {
                bool isActive;
				var hasCanaryLicense = _securityRepository.HasCanaryLicense(supportAccountId,out isActive);
                if (hasCanaryLicense)
                {
                    availableRoles.Where(r => r.Name.Contains(RoleNames.ThreatResearcher)).ForEach(r => r.IsActive = isActive);
                    availableRoles.Where(r => r.Name.Contains(RoleNames.AutofocusTrial)).ForEach(r => r.IsActive = isActive);

                    //Remove AutoFocus Trial Role if not a Super User
                    if (!isSuperUser && !isTacAdmin && !isDomainAdmin)
                        availableRoles.RemoveAll(r => r.Name.Contains(RoleNames.AutofocusTrial));
				}
                else
                {
                    availableRoles.RemoveAll(r => r.Name.Contains(RoleNames.ThreatResearcher));
                    availableRoles.RemoveAll(r => r.Name.Contains(RoleNames.AutofocusTrial));
                }
			}

            var isCsspAccount = _securityRepository.IsCsspSupportAccount(supportAccountId);

            if (!isCsspAccount)
            {
                if (availableRoles.Any(r => r.Name == RoleNames.CSSP))
                {
                    availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.CSSP));
                }
            }

            #region Partner Services Roles

            //Remove "Partner Program User" role if not TAC or TAC Super User
            if (availableRoles.Any(r => r.Id == (long)RoleType.PartnerProgramUser))
            {
                var isTacSuperUser = _securityRepository.IsTacSuperUser(restrictedByUserAccountId, supportAccountId);
                var isTacUserOrAdmin = _securityRepository.IsTacUserOrAdmin(restrictedByUserAccountId, supportAccountId);
                if (!(isTacUserOrAdmin || isTacSuperUser))
                    availableRoles.Remove(availableRoles.Single(r => r.Id == (long)RoleType.PartnerProgramUser));
            }

            //ATC User Roles - ATC Manager/Instructor
            if (availableRoles.Any(r => r.Id == (long)RoleType.ATCManager) || availableRoles.Any(r => r.Id == (long)RoleType.ATCInstructor))
            {
                bool atcManager;
                var showPartnerRoles = _securityRepository.ShowAtcPartnerRoles(restrictedByUserAccountId, supportAccountId, out atcManager);
                if (!showPartnerRoles)
                {
                    availableRoles.Remove(availableRoles.Single(r => r.Id == (long)RoleType.ATCManager));
                    availableRoles.Remove(availableRoles.Single(r => r.Id == (long)RoleType.ATCInstructor));
                }
                else if (atcManager)
                {
                    availableRoles.Where(r => !r.Name.Contains("ATC")).ForEach(r => r.IsDisabled = true);
                }
            }

            #endregion

           

            if ((!HasRole(supportAccountId, restrictedByUserAccountId, RoleNames.DomainAdministrator) && !isTacAdmin) || _currentStateService.GetIsImpersonated())
                {
                if (availableRoles.Any(r => r.Name == RoleNames.DomainAdministrator))
                {
                    availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.DomainAdministrator));
                }
            }

            if (!isSuperUser)
            {
                if (availableRoles.Any(r => r.Name == RoleNames.CreditAdmin))
                {
                    availableRoles.Remove(availableRoles.Single(r => r.Name == RoleNames.CreditAdmin));
                }
            }

            return availableRoles;
		}

		public List<Role> GetDefaultRoles(RoleCategory roleCategory, long supportAccountId) {

			RoleDataAccess roleDataAccess = new RoleDataAccess();
			return roleDataAccess.ReadDefaultRoles(roleCategory, supportAccountId);
		}

        public List<SelectableAccount> GetAvailableAccounts(long userAccountId) {
            return _securityRepository.GetListOfAccountsUserCanAccess(userAccountId).ToList();
        }

	    public long? GetDefaultSupportAccount(long userAccountId)
	    {
	        var defaultSupportAccountId = _userAccountRepository.GetDefaultAccount(userAccountId);

            return defaultSupportAccountId;
	    }

		public bool IsPanEmployeeRegisteredToSupportSite(string email) {
			return _securityRepository.IsPanEmployeeRegisteredToSupportSite(email);
		}

        /// <summary>
        /// Threat researchers and also any partner can see the autofocus link
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
		public bool CanSeeAutoFocusLink(long userAccountId, long supportAccountId)
        {
            NameValue category, afTrialCategory;
            var isThreatResearcher = _securityRepository.IsThreatResearcher(userAccountId, supportAccountId, out category);

            var isPartner = _securityRepository.IsPartnerForAnySupportAccount(userAccountId);

            var isPan245Member = _securityRepository.IsPan245Member(userAccountId);

            var isAutoFocusTrialUser = _securityRepository.IsAutoFocusTrialUser(userAccountId, supportAccountId, out afTrialCategory);

            return isThreatResearcher || isPartner || isPan245Member || isAutoFocusTrialUser;
		}


        #region Autofocus Trial Role

        /// <summary>
        /// Used by a daily job to process expired Autofocus trial roles 
        /// </summary>
        public void ProcessAutofocusTrialsExpiringToday()
        {
            try
            {
                //Fetch all active Autofocus Trial users whose trial role is going to expire today
                var autofocusTrialRoleUserIds = _securityRepository.GetAutofocusTrialUserIdsExpiringToday();
                foreach (var autofocusTrialRoleUserId in autofocusTrialRoleUserIds)
                {
                    _securityRepository.ProcessAutofocusTrialRoleExpiration(autofocusTrialRoleUserId);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
            }
        }

        #endregion

        public bool HasViewCaseManagementPrivilege(long supportAccountId, long userAccountId)
        {
            return _securityRepository.HasViewCaseManagementPrivilege(supportAccountId, userAccountId);
        }

	    public bool HasBpaAccess(long supportAccountId, long userAccountId)
	    {
            var securityDataAccess = new SecurityDataAccess();
	        return securityDataAccess.HasBpaAccess(supportAccountId, userAccountId);
	    }

	    public bool IsSuperUser(long supportAccountId, long userAccountId)
	    {
	        var securityDataAccess = new SecurityDataAccess();
	        return securityDataAccess.IsSuperUser(supportAccountId, userAccountId);
	    }

	    public bool HasRole(long supportAccountId, long userAccountId, string roleName)
	    {
		    return _securityRepository.HasRole(supportAccountId, userAccountId, roleName);
	    }

        public bool IsValidSupportAccount(long supportAccountId)
        {
            return _securityRepository.IsValidSupportAccount(supportAccountId);
        }

        public bool IsConsentedUser(long userAccountId, long supportAccountId)
        {
            return _securityRepository.IsConsentedUser(userAccountId, supportAccountId);
        }

        public bool IsTacUserOrAdmin(long supportAccountId, long userAccountId)
	    {
	        var securityDataAccess = new SecurityDataAccess();
	        return _securityRepository.IsTacUserOrAdmin(userAccountId, supportAccountId);
	    }

        public List<long> GetAscCustomerAccountsFromSalesForce(List<string> ascSalesForceAccountIds,long userAccountId)
        {
            try
            {
                //Read from Cache
                object cachedData = HttpRuntime.Cache.Get(userAccountId.ToString());
                if (cachedData != null)
                    return (List<long>)cachedData;

                string formattedAscIds = string.Join(",", ascSalesForceAccountIds.Select( s => $"\"{s}\""));
                var requestQuery = "{\"query\":\"query($ascAccountId: [String]) { license(ascAccountId:$ascAccountId , isCSPAssociated : true) {supportAccountId}}\\r\\n    \\r\\n\",\"variables\":{\"ascAccountId\":[ " + formattedAscIds  + "]}}";

                var responsePayload = WebCommon.GetEntitlementsFromSfdc<WebCommon.CommonApiResponse<AscResponse>>(requestQuery);
                if (responsePayload != null)
                {
                    List<long> customerAccounts = new List<long>();
                    foreach (var customerAccount in responsePayload.ResponseObject.AscSupportAccountsList)
                    {
                        customerAccounts.Add(customerAccount.SupportAccountId); 
                    }

                    //Add the response to cache. Cache validity for 30min
                    if (customerAccounts.Any())
                        HttpRuntime.Cache.Insert(userAccountId.ToString(), customerAccounts, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration);

                    return customerAccounts;
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return new List<long>();
        }

        public CspSession GetSessionDetails(string email, long supportAccountId)
        {
            var sessionDetails = new CspSession();
            try
            {
                string baseUrl = string.Format(ApiConstants.SessionManagementEndpoint, _applicationSettings.PortalBffHost, WebUtility.UrlEncode(email), supportAccountId);
                //var response = CommonHelper.ProcessWebRequest<CspSession>(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);

                var request = WebCommon.GetWebRequestWithClientCertificate(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                sessionDetails = JsonConvert.DeserializeObject<CspSession>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return sessionDetails;
        }

        public bool HasPermission(long supportAccountId, long userAccountId, Permission permission)
	    {
            var hasPermission = false;

            try
            {
                var userAccountDataAccess = new UserAccountDataAccess();

                var userAccount = userAccountDataAccess.Read(userAccountId, userAccountId);
                var permissionSet = GetPermissionsForPrincipal(userAccount, supportAccountId);
                
                hasPermission = permissionSet.Contains(permission);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            
            return hasPermission;
	    }

        private HashSet<Permission> GetPermissionsForPrincipal(UserAccount userAccount, long supportAccountId)
        {
            HashSet<Permission> permissionSet = new HashSet<Permission>();
            var userAccountDataAccess = new UserAccountDataAccess();

            try
            {
                var userEmail = !string.IsNullOrEmpty(userAccount?.Email) ? userAccount.Email : userAccountDataAccess.Read(userAccount.Id, userAccount.Id).Email;

                var cspPrincipal = _currentStateService.GetCurrentUser();

                if (cspPrincipal?.ActivePermissions?.Count > 0)
                {
                    permissionSet = cspPrincipal.ActivePermissions;
                }
                else
                { 
                    var response = GetSessionDetails(userEmail, supportAccountId);

                    if (response?.Privileges == null) return permissionSet;

                    foreach (var permission in response.Privileges)
                    {
                        permissionSet.Add(((Permission)Enum.Parse(typeof(Permission),
                            Convert.ToString(permission.Permission), true)));
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return permissionSet;
        }

        public UserStatus GetUserStatus(string email)
        {
            var userStatusDetails = new UserStatus();
            try
            {
                string baseUrl = string.Format(ApiConstants.UserStatusEndpoint, _applicationSettings.PortalBffHost, WebUtility.UrlEncode(email));
                var request = WebCommon.GetWebRequestWithClientCertificate(baseUrl, HttpMethod.Get.Method, _applicationSettings.PortalBffCertThumbPrint, string.Empty);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                               var commonResponse  = JsonConvert.DeserializeObject<CommonApiResponse<UserStatus>>(jsonResponse, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });

                                if (commonResponse != null) {
                                    userStatusDetails = commonResponse.ResponseObject;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return userStatusDetails;
        }    

    }

}
