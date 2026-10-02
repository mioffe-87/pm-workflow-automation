using System;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using System.Security.Principal;
using System.Web;
using System.Collections.Generic;
using System.Web.Security;
using Csp.Api.HelperClasses;
using AccountType = Csp.Api.Model.AccountType;
using System.Threading.Tasks;

namespace Csp.Api.Logic {
	public class PermissionService : IPermissionService {

		private readonly ISupportAccountLogic _supportAccountLogic;
		private readonly IGroupLogic _groupLogic;
		private readonly ISecurityLogic _securityLogic;
		private readonly ICurrentStateService _currentStateService;
        private readonly ISupportAccountUserLogic _supportAccountUserLogic;

        private CspPrincipal _user;

		public PermissionService(ISupportAccountLogic supportAccountLogic, IGroupLogic groupLogic, ISecurityLogic securityLogic, ICurrentStateService currentStateService, ISupportAccountUserLogic supportAccountUserLogic) {
			_supportAccountLogic = supportAccountLogic;
			_groupLogic = groupLogic;
			_securityLogic = securityLogic;
			_currentStateService = currentStateService;
            _supportAccountUserLogic = supportAccountUserLogic;
		}

		public void UpdateCurrentStateAndPermissions(CspPrincipal user, string controller, string action, string id, SupportAccount supportAccount,
																								 Group group, Device device) {

			SetRoleSecurities();

			long lId;
			if (!Int64.TryParse(id, out lId)) {
				return;
			}

			if (controller.ToLower() == "supportaccount" && action.ToLower() == "getasccontactinfo") {
				return;
			}

			//If the Id has a value or there is a group, support account or device( device may soon be deprecated or enhanced), then load the permissions and set the current state
			if ((lId > 0) && (group.Id == 0) && (device.Id == 0) && (supportAccount.Id == 0)) {

				//HACK - help load support account ids
				if (controller.ToLower() == "updates"
                    || controller.ToLower() == "bulkregistration"
					|| (controller.ToLower() == "useraccount" && action.ToLower() == "create")
					|| (controller.ToLower() == "device" && action.ToLower() == "devicecategories")) {
					controller = "supportaccount";
				}

				SetUserPermissions(controller, action, lId);
				SetCurrentState(controller, action, lId);
				return;
			}

			/*The following code seems redundant,as SetCurrentState(controller, action, id) does the same basic thing, I am not sure if I put this in or it was added later or 
			 * simply not removed. However it appears like both of these calls are not needed to set the current state.*/

			if (lId <= 0) {
				return;
			}


			if (supportAccount.Id > 0) {
				SetCurrentStateSupportAccount(supportAccount);
				return;
			}


			if (group.Id > 0) {
				SetCurrentStateGroup(group);
				return;
			}

			if (device.Id > 0) {
				SetCurrentStateDevice(device);
				return;
			}
		}

		private const string SupportAccountControllerName = "supportaccount";
		private const string GroupControllerName = "group";


		private void SetCurrentStateSupportAccount(long id) {

			var supportAccount =
				_user != null
					? _supportAccountLogic.GetSupportAccount(id, _user.UserAccount.Id)
					: null;

			SetCurrentStateSupportAccount(supportAccount);
		}

		private void SetCurrentStateSupportAccount(SupportAccount routedAccount) {

			if ((_user == null) || (routedAccount == null)) {
				return;
			}

			_currentStateService.SetSupportAccount(routedAccount);
		}

		private void SetCurrentStateGroup(long id) {

			var group =
				_user != null
				? _groupLogic.GetGroup(id, _user.UserAccount.Id)
				: null;

			SetCurrentStateGroup(group);
		}

		private void SetCurrentStateGroup(Group routedGroup) {

			if ((_user == null) || (routedGroup == null)) {
				return;
			}

			// Important to call set account current state first as it clears out the group
			// Or you may hit this directly from a bookmark and we have to load the Account before the Group
			SetCurrentStateSupportAccount(routedGroup.SupportAccountId);
			_currentStateService.SetGroup(routedGroup);
		}

		private void SetCurrentStateDevice(Device routedDevice) {

			if ((_user == null) || (routedDevice == null)) {
				return;
			}

			_currentStateService.SetDevice(routedDevice);
		}

		//This sets the current state for the support account or group based on the controller and action information
		private void SetCurrentState(string controller, string action, long id) {

            if (controller.ToLower() == SupportAccountControllerName)
            {
                SetCurrentStateSupportAccount(id);
                
            }
            else
            if ((controller.ToLower() == GroupControllerName)
								 && ((action.ToLower() == "editgroup") || (action.ToLower() == "viewgroup") || (action.ToLower() == "groupdevices") || (action.ToLower() == "groupusers") || (action.ToLower() == "viewgrouppartial"))) {

				SetCurrentStateGroup(id);
			}

		}

		/// <summary>
		/// This sets the user permission based on the controller and action. Depending on if the Controller is for group or Support account
		/// is check to determine if the Id is for the support account or the group. The permissions are loaded for the Id
		/// based on what the controller is. Some actions in the group controller are not included as those actions are 
		/// actually taken under the support account, such as adding/creating a group. However the actions are actually in the group controller.
		/// </summary>
		/// <param name="controller">The name of the controller</param>
		/// <param name="action">Name of the action</param>
		/// <param name="id">The Id for the support account or group</param>
		private void SetUserPermissions(string controller, string action, long id) {

			if (_user == null) {
				return;
			}

			if (controller.ToLower() == SupportAccountControllerName) {
				_user.SupportAccountIdForPermissionSet = id;
				_user.GroupIdForPermissionSet = null;
				//_user.ActivePermissions = _securityLogic.GetSupportAccountPermissionsForPrincipal(id, _user.UserAccount);
			} else if ((controller.ToLower() == GroupControllerName) && (action.ToLower() != "addgroup") && (action.ToLower() != "create")) {
				_user.SupportAccountIdForPermissionSet = null;
				_user.GroupIdForPermissionSet = id;
                _user.ActivePermissions = _securityLogic.GetGroupPermissionsForPrincipal(id, _user.UserAccount);
            }
		}

		private void SetRoleSecurities() {
			if (_user == null) {
				return;
			}
			if (_user.RoleSecurities.Any()) {
				return;
			}

			_user.RoleSecurities = _securityLogic.GetUserAccountRoleSecurities(_user.UserAccount.Id).ToList();
		}

        public void SetCurrentPrincipal(string controller, string action, long id)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
            {
                return;
            }

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);
            if (ticket == null || string.IsNullOrEmpty(ticket.Name)) return;

            var cspPrincipal = new CspPrincipal();
            long groupId = 0;

            if (id > 0 && (controller.ToLower() == "group") && (action.ToLower() != "addgroup") && (action.ToLower() != "create"))
            {
                groupId = id;
                var group = _groupLogic.GetGroup(groupId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

                if (group != null)
                {
                    id = group.SupportAccountId;
                    cspPrincipal.Group = group;
                }
            }
            else if (id == 0)
            {
                // Load the SupportAccountId from FormsAuthenticationTicket
                if (ticket?.UserData != null && ticket.UserData != "")
                {
                    Int64.TryParse(ticket.UserData, out id);
                }
            }

            var response = _securityLogic.GetSessionDetails(ticket.Name, id);

            if (response?.User == null || response.Privileges == null || response.Roles == null) return;

            cspPrincipal.UserAccount = new UserAccount()
            {
                Id = response.User.Id,
                Email = response.User.Email,
                DisplayName = response.User.DisplayName,
                FirstName = response.User.FirstName,
                LastName = response.User.LastName,
                SalesforceLoginId = response.User.SalesforceLoginId,
                HasMultipleMemberships = response.User.HasMultipleMemberships,
                IsValid = response.User.IsValid,
                DefaultSupportAccountId = response.User.DefaultSupportAccountId,
                MembershipStatus = response.User.MembershipStatus,
                IsMembershipExpired = response.User.IsMembershipExpired,
                PhoneNumber = response.User.ContactInformation,
                SelectedMembershipId = response.User.SelectedMembershipId,
                Theme = response.User.Theme
            };

            cspPrincipal.SupportAccount = new SupportAccount()
            {
                Id = response.Account.SupportAccountId,                
                Name = response.User.IsValid || response.Privileges.Count > 0 ? response.Account.Name : string.Empty,
                IsFedramp = response.Account.IsFedramp,
                AccountTypeId = response.Account.AccountTypeId == null ? 0 : GetAccountTypeId(response.Account.AccountTypeId),
                SalesforceId = response.Account.SalesforceAccountId,
                IsPilotCustomer = response.Account.IsPilotCustomer,
                isAIEligible = response.Account.IsAIEligible,
                ImpersonateSettingAsString = response.Account.ImpersonateSetting,
                IsClassicEligible = response.Account.IsClassicEligible,
                ShowHomepageBanner = response.Account.ShowHomepageBanner
            };

            if (groupId > 0)
            {
                cspPrincipal.ActivePermissions = _securityLogic.GetGroupPermissionsForPrincipal(groupId, cspPrincipal.UserAccount);
            }
            else
            {
                var permissionSet = new HashSet<Permission>();

                foreach (var permission in response.Privileges)
                {
                    permissionSet.Add(((Permission) Enum.Parse(typeof(Permission),
                        Convert.ToString(permission.Permission), true)));
                }

                cspPrincipal.ActivePermissions = permissionSet;
            }

            cspPrincipal.Roles = response.Roles;

            if (response.SecurityClassifications != null)
            {
                cspPrincipal.RoleSecurities = (from sc in response.SecurityClassifications
                    select new RoleSecurity
                    {
                        SecurityClassificationNum = sc.SecurityClassification,
                        CategoryNum = sc.Category,
                        SupportAccountId = sc.SupportAccountId,
                        GroupId = sc.GroupId
                    }).ToList();
            }

            if (!cspPrincipal.IsValid()) return;

            if (ticket.UserData != cspPrincipal.SupportAccount.Id.ToString())
            {
                // Update FormsAuthentication Cookie
                var newTicket = new FormsAuthenticationTicket
                (ticket.Version,
                    ticket.Name,
                    ticket.IssueDate,
                    ticket.Expiration,
                    false,
                    cspPrincipal.SupportAccount.Id.ToString(),
                    ticket.CookiePath);

                // Encrypt the ticket and store it in the cookie
                authCookie.Value = FormsAuthentication.Encrypt(newTicket);

                // Set the cookie's expiration time to the tickets expiration time
                if (ticket.IsPersistent) authCookie.Expires = newTicket.Expiration;

                HttpContext.Current.Response.Cookies.Add(authCookie);
                if (!string.IsNullOrEmpty(ticket.UserData))
                {
                    ValidateSfdcContact(cspPrincipal.UserAccount.Id, cspPrincipal.SupportAccount.Id, cspPrincipal.UserAccount.Email);
                }
            }

            cspPrincipal.Identity = new GenericIdentity(ticket.Name);
            HttpContext.Current.User = cspPrincipal;
        }

        private int GetAccountTypeId(string accountType)
        {
            if (accountType.ToUpper() != Model.Enums.AccountType.Customer.ToDescription())
            {
                return (int) Model.Enums.AccountType.Customer;
            }
            else if (accountType.ToUpper() != Model.Enums.AccountType.Asc.ToDescription())
            {
                return (int)Model.Enums.AccountType.Asc;
            }
            else if (accountType.ToUpper() != Model.Enums.AccountType.PanEmployee.ToDescription())
            {
                return (int)Model.Enums.AccountType.PanEmployee;
            }
            else if (accountType.ToUpper() != Model.Enums.AccountType.Eval.ToDescription())
            {
                return (int)Model.Enums.AccountType.Eval;
            }
            else if (accountType.ToUpper() != Model.Enums.AccountType.Marketplace.ToDescription())
            {
                return (int)Model.Enums.AccountType.Marketplace;
            }
            else
            {
                return 0;
            }
        }

        private async Task<bool> ValidateSfdcContact(long userAccountId, long supportAccountId, string email)
        {
            var response = true;
            if (supportAccountId > 0 && userAccountId > 0)
            {
                await _supportAccountUserLogic.ValidateSfdcContact(supportAccountId, userAccountId, email);
            }
            return response;
        }
    }
}