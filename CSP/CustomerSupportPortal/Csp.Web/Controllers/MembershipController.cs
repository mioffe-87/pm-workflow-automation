using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Security.Attributes;
using Csp.Api.Security;
using Csp.Web.HelperClasses;
using Csp.Web.DataTransferObjects;
using Csp.Web.Attributes;
using Csp.Api.HelperClasses;
using Csp.Api.Exceptions;
using Csp.Api.Logic;
using DocumentFormat.OpenXml.Wordprocessing;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Microsoft.Security.Application;

namespace Csp.Web.Controllers {
	public class MembershipController : BaseController {
		private readonly IDeviceLogic _deviceLogic;
		private readonly IMembershipLogic _membershipLogic;
		private readonly IUserAccountLogic _userAccountLogic;
		private readonly IMfaLogic _mfaLogic;
		private readonly ISecurityLogic _securityLogic;
        private readonly ILoginServiceLogic _loginServiceLogic;
        private readonly IApplicationSettings _applicationSettings;

        #region Constants
        private const string PartnerProgramUserErrMsg = "Partner Program User role can only be assigned to internal Palo Alto Networks employees";
        #endregion

				public MembershipController(IDeviceLogic deviceLogic, ISalesOrderLogic salesOrderLogic, IMembershipLogic membershipLogic, IUserAccountLogic userAccountLogic,
																		IMfaLogic mfaLogic, ISecurityLogic securityLogic , ILoginServiceLogic loginServiceLogic, IApplicationSettings applicationSettings) {
					_deviceLogic = deviceLogic;
					_membershipLogic = membershipLogic;
					_userAccountLogic = userAccountLogic;
					_mfaLogic = mfaLogic;
					_securityLogic = securityLogic;
                    _loginServiceLogic = loginServiceLogic;
                    _applicationSettings = applicationSettings;
                }

		#region private members

		private ActionResult GetUserByEmail(string email) {
			var userAccount = UserAccountLogic.GetUserAccountByEmail(email) ?? new UserAccount();

			var model = new UserAccountDto {
				UserAccount = userAccount
			};

			if (userAccount.Id > 0) {
				ModelState.Clear();
				return PartialView("../partial/_UserIdentification", model);

			}
			return Json("", JsonRequestBehavior.AllowGet);
		}

		private UserAccount GetUserAccountByEmail(string email) {
			UserAccount userAccount = null;

			userAccount = UserAccountLogic.GetUserAccountByEmail(email);

			return userAccount;
		}

		private List<Role> GetAvailableRoles(Membership membership) {
			if (membership.SupportAccountId > 0) {
				return SecurityLogic.GetAvailableRoles(RoleCategory.SupportAccount, membership.SupportAccountId, User.UserAccount.Id).ToList();
			}

			if (membership.GroupId > 0) {
				return SecurityLogic.GetAvailableRoles(RoleCategory.Group, CurrentState.GetCurrentAccountId(), User.UserAccount.Id).ToList();
			}

			return new List<Role>();
		}

		/// <summary>
		/// Validates caller has access to the target support account (directly or via group).
		/// Returns true if caller has membership on the account OR is a TAC user/admin.
		/// </summary>
		private bool HasAccessToTargetAccount(int membershipTypeId, long targetId)
		{
			var callerUserId = User.UserAccount.Id;

			long? targetAccountId = membershipTypeId == (int)MembershipType.Group
				? _membershipLogic.GetSupportAccountIdByGroupId(targetId)
				: targetId;

			if (!targetAccountId.HasValue || targetAccountId <= 0)
				return false;

			return _membershipLogic.HasAccessToSupportAccount(targetAccountId.Value, callerUserId) ||
				   _securityLogic.IsTacUserOrAdmin(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, callerUserId);
		}

		/// <summary>
		/// Validates caller has access to an existing membership and that the request data matches the database.
		/// Returns the validated membership, or null if validation fails.
		/// </summary>
		private Membership GetValidatedMembership(long membershipId, long requestUserAccountId)
		{
			var membership = MembershipLogic.GetMembership(membershipId, User.UserAccount.Id);
			if (membership == null)
				return null;

			// Validate UserAccountId from request matches database
			if (membership.UserAccount.Id != requestUserAccountId)
				return null;

			// Validate caller has access to the membership's account
			var targetTypeId = membership.GroupId > 0 ? (int)MembershipType.Group : (int)MembershipType.SupportAccount;
			var targetParentId = membership.GroupId > 0 ? membership.GroupId : membership.SupportAccountId;

			if (!HasAccessToTargetAccount(targetTypeId, targetParentId))
				return null;

			return membership;
		}

		#endregion

		#region Public Members
		// Comments by LK On 05/07/2026
		// This method is obsolete and will be removed in the future. Refer to CSP-26851
		/*
		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.DeleteMembership)]
		public ActionResult Delete(long id) {
			var currentUserAccountId = User.UserAccount.Id;
			var membership = MembershipLogic.GetMembership(id, currentUserAccountId);
			var roles = membership.Roles;

			// Check if the user has Domain Administrator role
			if (roles.Any(r => r.Id == (long) RoleType.DomainAdministrator)) {
				// Check if the logged in user has Domain Administrator role to perform the delete action
				var currentMembership = MembershipLogic.GetMembership(currentUserAccountId, currentUserAccountId);
				var currentMembershipRoles = currentMembership.Roles;

				if (currentMembershipRoles.All(r => r.Id != (long) RoleType.DomainAdministrator)) {
					return new JsonResult { Data = new { Success = false, Message = "You are not authorized to delete a Domain Administrator. Please contact customer support." } };
				}
			}

			MembershipLogic.DeleteMembership(id, User.UserAccount.Id);

			return new JsonResult { Data = new { Success = true } };
		}
		*/

		// Comments by LK On 05/07/2026
		// This method is obsolete and will be removed in the future. Refer to CSP-26851
		/*
		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditMembership)]
		public ActionResult Edit(long id) {
			var model = new MembershipDto {
				Membership = MembershipLogic.GetMembership(id, User.UserAccount.Id),
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, id, ActionHelper.EntityAction.Edit),
			};
			model.Membership.UserAccount = UserAccountLogic.GetUserAccount(model.Membership.UserAccount.Id, User.UserAccount.Id);

			var assignedRoles = model.Membership.Roles;
			var availableRoles = GetAvailableRoles(model.Membership);

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();

			model.RoleAssignment.Roles = roles;
			return PartialView("../partial/_Membership", model);
		}
		*/

		// Comments by LK On 05/07/2026
		// This method is obsolete and will be removed in the future. Refer to CSP-26851
		/*
		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditMembership)]
		public ActionResult Edit(MembershipDto membershipDto) {
			var roleIds = membershipDto.RoleAssignment.SelectedRoleIds.Split(new[] { ',' });
			var defaultRoles = new List<Role>();

			foreach (var id in roleIds) {
				long roleId;
				long.TryParse(id, out roleId);
				defaultRoles.Add(new Role() { Id = roleId });
			}

			membershipDto.Membership.Roles = defaultRoles;

			var model = new MembershipDto {
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, membershipDto.Membership.Id, ActionHelper.EntityAction.Edit),
				Membership = membershipDto.Membership,

			};

			// Check if assigned role exists in available roles
			List<Role> authorisedRoles = GetAvailableRoles(model.Membership);
			foreach (Role role in model.Membership.Roles) {
				IEnumerable<Role> matchingRole = authorisedRoles.Where(x => x.Id == role.Id);
				if (!matchingRole.Any()) {
					string errorMsg = string.Format("Unauthorised role id- {0} assignment by useraccountid:{1} to useraccountid: {2} for supportaccountid:{3}", role.Id, model.Membership.LastUpdateByUserAccountId, model.Membership.UserAccount.Id, model.Membership.SupportAccountId);
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));
					ModelState.AddModelError("Roles", "An error occurred while updating membership. Please contact customer support.");
					break;
				}
			}

			if (!string.IsNullOrEmpty(membershipDto.RoleAssignment.SelectedRoleIds)) {
				try {
					ModelState.Remove("Membership.UserAccount.DisplayName");
					ModelState.Remove("Membership.UserAccount.FirstName");
					ModelState.Remove("Membership.UserAccount.LastName");
					ModelState.Remove("Membership.UserAccount.PhoneNumber");
					ModelState.Remove("Membership.UserAccount.Email");
					ModelState.Remove("RoleAssignment.AvailableRoles");

					if (ModelState.IsValid) {

						model.Membership.LastUpdateByUserAccountId = User.UserAccount.Id;
						model.Membership = MembershipLogic.UpdateMembership(model.Membership);
						model.Membership.UserAccount = UserAccountLogic.GetUserAccount(model.Membership.UserAccount.Id,
																																					 User.UserAccount.Id);

						if (model.Membership.ShowThreatResearcherUseCountWarning) {
							model.Notifications.Add(new ResponseNotification("The Threat Researcher role could not be added - use count exceeded", ResponseState.Warning));
						} else {
							ModelState.Clear();
							model.Notifications.Add(new ResponseNotification("Membership was updated sucessfully", ResponseState.Success));
						}

                        if (model.Membership.Roles.Any(r => r.Id == (long)RoleType.DomainAdministrator))
                        {
                            // Domain Admin should always use PAN SSO 
                            //Check if Domain Admin exists in SSO
                            var userProfile = _loginServiceLogic.GetUserProfile(model.Membership.UserAccount.Email, model.Membership.UserAccount.IpAddress, _applicationSettings.LoginServicePrivateKey);
                            if (userProfile == null)
                            {
                                //Register Domain Admin on SSO
                                userProfile = _loginServiceLogic.Register(model.Membership.UserAccount.Email, GetUniqueKey(18),
                                    model.Membership.UserAccount.FirstName ?? string.Empty,
                                    model.Membership.UserAccount.LastName, string.Empty,
                                    model.Membership.UserAccount.Address.Address1??string.Empty,
                                    model.Membership.UserAccount.Address.Address2 ?? string.Empty,
                                    model.Membership.UserAccount.Address.City ?? string.Empty,
                                    model.Membership.UserAccount.Address.Region ?? string.Empty,
                                    model.Membership.UserAccount.Address.PostalCode ?? string.Empty,
                                    model.Membership.UserAccount.Address.Country ?? string.Empty, model.Membership.UserAccount.PhoneNumber ?? string.Empty,
                                    Common.GetClientIp(),true);

                            }
                        }
                    }

				} catch (CspNotificationException exception) {
					model.Notifications.Add(new ResponseNotification(exception.Message, ResponseState.Error));
				}
			} else {
				model.Notifications.Add(new ResponseNotification("At least one role is required.", ResponseState.Error));
			}

			var assignedRoles = model.Membership.Roles;
			var availableRoles = GetAvailableRoles(model.Membership);

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
			model.RoleAssignment.Roles = roles;

			return PartialView("../partial/_Membership", model);
		}
		*/

		[HttpPost, AjaxOnly]
		public JsonResult IsValidEmail(string email, int membershipTypeId, long membershipParentId) {
			var message = string.Empty;
			bool isNewUser = false;
			bool isThirdPartyIdpConfigured = false;
            email = !string.IsNullOrEmpty(email) ? email.Replace("'", "") : String.Empty;

			if (User.HasPermission(Permission.CreateMembership) ||
			    (membershipTypeId == (int) MembershipType.Group &&
			     User.HasPermission(Permission.CreateGroupMembership))) {

				if (string.IsNullOrEmpty(email)) {
					message = "Email is required, but not provided.";
					return new JsonResult {Data = new {Success = false, Message = message, isNewUser, isThirdPartyIdpConfigured}};
				}

				if (!Regex.IsMatch(email.Trim(), @"^[_\w\.'-]+@[\w\.-]+\.\w{2,12}$")) {
					message = string.Format("Please enter a valid email. You entered '{0}'", email.Trim());
					return new JsonResult {Data = new {Success = false, Message = message, isNewUser, isThirdPartyIdpConfigured}};
				}

				isThirdPartyIdpConfigured = SiteHelper.IsThirdPartyIdpConfigured(email);

				// Check if the user already has a membership for SupportAccount/Group/Device
				if (membershipTypeId == (int) MembershipType.SupportAccount) {
					if (!MembershipLogic.HasMembershipforSupportAccountByEmail(email, membershipParentId)) {
						var userAccount = GetUserAccountByEmail(email);

						//Third party idp configured users can be added directly on members tab without first creating useraccount
						if (userAccount == null) {
							isNewUser = true;

							if (!isThirdPartyIdpConfigured)
								message =
									"User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar";
						}
					}
					else {
						message = "User is already a member of the support account.";
					}
				}
				else if (membershipTypeId == (int) MembershipType.Group) {
					if (!MembershipLogic.HasMembershipforGroupByEmail(email, membershipParentId))
					{
						var userAccount = GetUserAccountByEmail(email);

						if (userAccount == null) {
							isNewUser = true;

							if (!isThirdPartyIdpConfigured)
								message =
									"User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar";
						}
					}
					else {
						message = "User is already a member of the group.";
					}
				}
				else if (membershipTypeId == (int) MembershipType.Device) {
					if (!MembershipLogic.HasMembershipforDeviceByEmail(email, membershipParentId)) {
						var userAccount = GetUserAccountByEmail(email);

						if (userAccount == null) {
							isNewUser = true;

							if (!isThirdPartyIdpConfigured)
								message =
									"User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar";
						}
					}
					else {
						message = "User is already a member of the device.";
					}
				}

				if (string.IsNullOrEmpty(message))
					return new JsonResult
						{Data = new {Success = true, Message = string.Empty, isNewUser, isThirdPartyIdpConfigured}};
			}

			return new JsonResult { Data = new { Success = false, Message = message, isNewUser, isThirdPartyIdpConfigured } };
		}

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken]
		public ActionResult CreateMembership(MembershipListItem model) {
			// Only allow Group memberships
			if (model.MembershipTypeId != (int)MembershipType.Group)
			{
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			// Security Fix: Validate caller has access to target account
			if (!HasAccessToTargetAccount(model.MembershipTypeId, model.MembershipParentId))
			{
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			if (User.HasPermission(Permission.CreateMembership) ||
			    (model.MembershipTypeId == (int) MembershipType.Group &&
			     User.HasPermission(Permission.CreateGroupMembership)))
			{
				if (model.Roles != null && model.Roles.Count > 0)
				{
					try
					{
                        if (_userAccountLogic.CanAddEmailToSupportAccount(User.SupportAccount.Id, model.Email))
                        {
                            if (ModelState.IsValid)
                            {
                                if (!SiteHelper.IsThirdPartyIdpConfigured(model.Email))
                                {


                                    // Check if the user already has a membership for SupportAccount/Group/Device
                                    if (model.MembershipTypeId == (int)MembershipType.SupportAccount)
                                    {
                                        if (!MembershipLogic.HasMembershipforSupportAccountByEmail(model.Email, model.MembershipParentId))
                                        {
                                            var userAccount = GetUserAccountByEmail(model.Email);

                                            if (userAccount != null && userAccount.Id > 0)
                                            {
                                                model.UserAccountId = userAccount.Id;
                                                model.FirstName = userAccount.FirstName;
                                                model.LastName = userAccount.LastName;
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("Email",
                                                    "User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar.");
                                            }
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Email", "User is already a member.");
                                        }
                                    }
                                    else if (model.MembershipTypeId == (int)MembershipType.Group)
                                    {
                                        if (!MembershipLogic.HasMembershipforGroupByEmail(model.Email, model.MembershipParentId))
                                        {
                                            var userAccount = GetUserAccountByEmail(model.Email);

                                            if (userAccount != null && userAccount.Id > 0)
                                            {
                                                model.UserAccountId = userAccount.Id;
                                                model.FirstName = userAccount.FirstName;
                                                model.LastName = userAccount.LastName;
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("Email",
                                                    "User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar.");
                                            }
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Email", "User is already a member.");
                                        }
                                    }
                                    else if (model.MembershipTypeId == (int)MembershipType.SupportAccount)
                                    {
                                        if (!MembershipLogic.HasMembershipforDeviceByEmail(model.Email, model.MembershipParentId))
                                        {
                                            var userAccount = GetUserAccountByEmail(model.Email);

                                            if (userAccount != null && userAccount.Id > 0)
                                            {
                                                model.UserAccountId = userAccount.Id;
                                                model.FirstName = userAccount.FirstName;
                                                model.LastName = userAccount.LastName;
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("Email",
                                                    "User not found.  To add a user who has never registered, use the 'Create New User' under Members on the side navigation bar.");
                                            }
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Email", "User is already a member.");
                                        }
                                    }
                                }
                                else
                                {
                                    MailAddress userMailAddress = new MailAddress(model.Email);

                                    var userAccount = GetUserAccountByEmail(model.Email);
                                    if (userAccount == null)
                                    {
                                        userAccount = _userAccountLogic.CreateUserAccount(new UserAccount()
                                        {
                                            Email = model.Email,
                                            DisplayName = String.Concat(userMailAddress.User, new Random().Next(123, 99999)),
                                            CreatedByEmail = CurrentState.GetLoggedInUser().UserAccount.Email,
                                            CreatedByUserAccountId = CurrentState.GetLoggedInUser().UserAccount.Id,
                                            IsActive = true,
                                            CreatedOn = DateTime.Now,
                                            IsOptInEmail = true,
                                            IsOptInPanOsUpdateEmail = true,
                                            IsOptInSecurityEmail = true,
                                            FirstName = string.Empty,
                                            LastName = string.Empty
                                        });

                                        //create Jive id only for new users
                                        if (userAccount != null && userAccount.Id > 0)
                                        {
                                            _userAccountLogic.CreateJiveId(userAccount.Id, userAccount.Email);
                                        }
                                    }


                                    if (userAccount != null && userAccount.Id > 0)
                                    {
                                        _userAccountLogic.CreateJiveId(userAccount.Id, userAccount.Email);
                                        model.UserAccountId = userAccount.Id;
                                    }
                                }
                            }

                            // Check if the model state is still valid to continue
                            if (ModelState.IsValid)
                            {
                                var membership = new Membership();

                                var assignedRoles = model.Roles;
                                var roles = assignedRoles.Select(r => new Role() { Id = long.Parse(r.Value), Name = r.Name }).ToList();

                                membership.Roles = roles;
                                membership.UserAccount = new UserAccount() { Id = model.UserAccountId };
                                membership.ActivationDate = model.ActivationDate;
                                membership.ExpirationDate = model.ExpirationDate;
                                membership.Description = model.Description;
                                membership.IsMfaEnabled = model.IsMfaEnabled;
                                membership.EnableEmailMfa = model.EnableEmailMfa;

                                long? supportAccountId = null;
                                if (model.MembershipTypeId == (int)MembershipType.SupportAccount)
                                {
                                    membership.SupportAccountId = model.MembershipParentId;
                                    supportAccountId = membership.SupportAccountId;
                                }
                                else if (model.MembershipTypeId == (int)MembershipType.Group)
                                {
                                    membership.GroupId = model.MembershipParentId;
                                    supportAccountId = MembershipLogic.GetSupportAccountIdByGroupId(membership.GroupId);
                                }
                                else if (model.MembershipTypeId == (int)MembershipType.Device)
                                {
                                    membership.DeviceId = model.MembershipParentId;
                                }

                                membership.CreatedByUserAccountId = User.UserAccount.Id;
                                var isExistingMember = false;
                                if (supportAccountId.HasValue)
                                {
                                    isExistingMember =
                                        MembershipLogic.HasAccessToSupportAccount(supportAccountId.Value, model.UserAccountId);
                                }

                                // Comments By LK On 09/26/2016
                                // To check if the user is a panw employee, so we can ignore the threat researcher use count validation
                                membership.IsValidateThreatResearcherUseCount =
                                    !model.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION) ? true : false;

                                if (membership.Roles.Any(r => r.Id == (long)RoleType.PartnerProgramUser))
                                {
                                    if (!model.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                                    {
                                        ModelState.AddModelError("Roles", PartnerProgramUserErrMsg);
                                    }
                                }

                                if (ModelState.IsValid)
                                {
                                    membership = MembershipLogic.CreateMembership(membership, supportAccountId ?? 0);
                                    model.MembershipId = membership.Id;
                                    model.IsMfaEnabled = membership.IsMfaEnabled;
                                    model.MultiFactorAuthType = membership.MultiFactorAuthType;
                                    model.Editable = true;
                                    model.FirstName = membership.UserAccount.FirstName ?? string.Empty;
                                    model.LastName = membership.UserAccount.LastName ?? string.Empty;
                                }

                                //Do not assign ThreatResearcher Role if User is already a ThreatResearcher
                                if (!string.IsNullOrEmpty(membership.ThreatResearcherAssignmentError))
                                {
                                    ModelState.AddModelError("Roles", membership.ThreatResearcherAssignmentError);
                                }
                                else if (membership.ShowThreatResearcherUseCountWarning)
                                {
                                    ModelState.AddModelError("Roles", "Threat Researcher use count exceeded");
                                }
                                else if (!string.IsNullOrEmpty(membership.AutofocusTrialAssignmentError))
                                {
                                    ModelState.AddModelError("Roles", membership.AutofocusTrialAssignmentError);
                                }
                                else if (membership.ShowATCManagerUseCountWarning)
                                {
                                    ModelState.AddModelError("Roles", "ATC Manager seat limitation exceeded");
                                }

                                if (ModelState.IsValid)
                                {
                                    // Link Membership to SFDC Contact
                                    MembershipLogic.AddUpdateSalesforceContact(membership, User.UserAccount.Email, User.UserAccount.Id);
                                    ModelState.Clear();

                                    if (supportAccountId.HasValue && !isExistingMember)
                                    {

                                        EmailHelper.SendUserRegistrationEmailToSuperUsers(model.FirstName, model.LastName,
                                            supportAccountId.Value, model.UserAccountId, model.MembershipId);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_userAccountLogic.IsValidNonPublicEmail(model.Email))
                            {
                                ModelState.AddModelError("Email", "Cannot add user with a private-domain email address, e.g., user@acme.com to a CSP account that only accepts members with public-domain, e.g., user@gmail.com, email addresses");
                            }
                            else
                            {
                                ModelState.AddModelError("Email", "Cannot add user with a public-domain email address, e.g., user@gmail.com to a CSP account that only accepts members with private-domain, e.g., user@acme.com, email addresses");
                            }
                        }
                    }
					catch (Exception ex)
					{
						Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
						ModelState.AddModelError("Error",
							"An error occurred while creating membership. Please contact customer support.");
					}
				}
				else
				{
					ModelState.AddModelError("Roles", "At least one role is required.");
				}

				return Json(new[] {model}.ToDataSourceResult(new DataSourceRequest(), ModelState));
			}

			return RedirectToAction("UnAuthorizedAccess", "Error");
		}

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken]
		public ActionResult UpdateMembership(MembershipListItem model) {
			// Only allow Group memberships
			if (model.MembershipTypeId != (int)MembershipType.Group)
			{
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			if (User.HasPermission(Permission.EditMembership) ||
			    (model.MembershipTypeId == (int) MembershipType.Group && User.HasPermission(Permission.EditGroupMembership)))
			{
				if (model.Roles != null && model.Roles.Count > 0)
				{
					try
					{
						if (ModelState.IsValid)
						{
							var membership = GetValidatedMembership(model.MembershipId, model.UserAccountId);
							if (membership == null)
							{
								return RedirectToAction("UnAuthorizedAccess", "Error");
							}

							// Use validated UserAccountId from database for CanDeleteUser check
							dynamic canDeleteUser = CanDeleteUser(membership.UserAccount.Id, model.MembershipTypeId, ApiConstants.UPDATE, model.Roles).Data;
							if (canDeleteUser != null && !canDeleteUser.Success)
							{
								ModelState.AddModelError("Roles", canDeleteUser.message);
								return Json(new[] {model}.ToDataSourceResult(new DataSourceRequest(), ModelState));
							}

							var assignedRoles = model.Roles;
							var roles = assignedRoles.Select(r => new Role() {Id = long.Parse(r.Value), Name = r.Name}).ToList();

							membership.Roles = roles;
							membership.ActivationDate = model.ActivationDate;
							membership.ExpirationDate = model.ExpirationDate;
							membership.Description = model.Description;
							membership.LastUpdateByUserAccountId = User.UserAccount.Id;
							membership.IsMfaEnabled = model.IsMfaEnabled;
                            membership.EnableEmailMfa = model.EnableEmailMfa;

                            if (_securityLogic.IsSuperUser(CurrentState.GetCurrentAccountId(), User.UserAccount.Id) || 
                                _securityLogic.IsTacUserOrAdmin(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, User.UserAccount.Id) || 
                                _securityLogic.HasRole(CurrentState.GetCurrentAccountId(), User.UserAccount.Id, RoleNames.DomainAdministrator))
                            {
                                membership.MultiFactorAuthType = model.MultiFactorAuthType;
                            }

                            // Comments By LK On 09/26/2016
                            // To check if the user is a panw employee, so we can ignore the threat researcher use count validation
                            membership.IsValidateThreatResearcherUseCount =
								!model.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION) ? true : false;

							if (membership.Roles.Any(r => r.Id == (long) RoleType.PartnerProgramUser))
							{
								if (!model.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
								{
									ModelState.AddModelError("Roles", PartnerProgramUserErrMsg);
								}
							}

                            // Check if assigned role exists in available roles
                            List<Role> availableRoles = GetAvailableRoles(membership);

                            foreach (Role role in membership.Roles) {
								IEnumerable<Role> matchingRole = availableRoles.Where(x => x.Id == role.Id);
								if (!matchingRole.Any()) {
									string errorMsg = string.Format("Unauthorised role id- {0} assignment by useraccountid:{1} to useraccountid: {2} for supportaccountid:{3}", role.Id, membership.LastUpdateByUserAccountId, membership.UserAccount.Id, membership.SupportAccountId);
									Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));
									ModelState.AddModelError("Roles", "An error occurred while updating membership. Please contact customer support.");
									break;
								}
							}

							if (ModelState.IsValid)
							{
								membership = MembershipLogic.UpdateMembership(membership);
                                model.IsMfaEnabled = membership.IsMfaEnabled;
                                model.MultiFactorAuthType = membership.MultiFactorAuthType;
                            }

							//Do not assign ThreatResearcher Role if User is already a ThreatResearcher
							if (!string.IsNullOrEmpty(membership.ThreatResearcherAssignmentError))
							{
								ModelState.AddModelError("Roles", membership.ThreatResearcherAssignmentError);
							}
							//Threat Researcher UseCount exceeded
							else if (membership.ShowThreatResearcherUseCountWarning)
							{
								ModelState.AddModelError("Roles", "Threat Researcher use count exceeded");
							}
							else if (!string.IsNullOrEmpty(membership.AutofocusTrialAssignmentError))
							{
								ModelState.AddModelError("Roles", membership.AutofocusTrialAssignmentError);
							}
							else if (membership.ShowATCManagerUseCountWarning)
							{
								ModelState.AddModelError("Roles", "ATC Manager seat limitation exceeded");
							}
							else
							{
								if (!string.IsNullOrEmpty(membership.AutofocusTrialRoleWarning))
								{
									model.Notification = membership.AutofocusTrialRoleWarning;
									model.Roles.RemoveAll(r => r.Name.Contains(RoleNames.AutofocusTrial));
									ModelState.Clear();
								}
							}

                            if (membership.Roles.Any(r => r.Id == (long)RoleType.DomainAdministrator))
                            {
                                // Domain Admin should always use PAN SSO 
                                

                                //Check if Domain Admin exists in SSO
                                var userProfile = _loginServiceLogic.GetUserProfile(model.Email,Common.GetClientIp(), _applicationSettings.LoginServicePrivateKey);
                                if (userProfile == null)
                                {
                                    //Register Domain Admin on SSO
                                    userProfile = _loginServiceLogic.Register(model.Email, GetUniqueKey(18),
                                       string.Empty,
                                       string.Empty, string.Empty,
                                       string.Empty,
                                       string.Empty,
                                       string.Empty,
                                       string.Empty,
                                       string.Empty,
                                       string.Empty, string.Empty,
                                        Common.GetClientIp(), true);

                                }
                            }
                            model.LastName = model.LastName ?? string.Empty;
                            model.FirstName = model.FirstName ?? string.Empty;
                        }

					}
					catch (Exception ex)
					{
						Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
						ModelState.AddModelError("Roles",
							"An error occurred while updating membership. Please contact customer support.");
					}
				}
				else
				{
					ModelState.AddModelError("Roles", "At least one role is required.");
				}

				return Json(new[] {model}.ToDataSourceResult(new DataSourceRequest(), ModelState));
			}

			return RedirectToAction("UnAuthorizedAccess", "Error");
		}

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken]
		public ActionResult DeleteMembership(MembershipListItem model) {
			// Only allow Group memberships
			if (model.MembershipTypeId != (int)MembershipType.Group)
			{
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			if (User.HasPermission(Permission.DeleteMembership) ||
			    (model.MembershipTypeId == (int) MembershipType.Group &&
			     User.HasPermission(Permission.DeleteGroupMembership)))
			{
				try
				{
                    var membership = GetValidatedMembership(model.MembershipId, model.UserAccountId);
                    if (membership == null)
                    {
                        return RedirectToAction("UnAuthorizedAccess", "Error");
                    }

                    MembershipLogic.DeleteMembership(model.MembershipId, User.UserAccount.Id);
                    if (ModelState.IsValid)
                    {
                        // // Call SDFC Delete entitlement contact when the user is removed
                        MembershipLogic.DeleteSalesforceContact(model.Email, membership.SupportAccountId, User.UserAccount.Id);
                        ModelState.Clear();
                    }
                    //if (model.IsMfaEnabled)
                    //{
                    //    //Check if user is still eligible for MFA, opt out of MFA if not eligible
                    //    bool isMfaRequired;
                    //    var isMfaEligible = _mfaLogic.IsUserEligibleforMfa(model.Email, out isMfaRequired);
                    //    if (!isMfaEligible)
                    //    {
                    //        var user = new UserAccount {Id = model.UserAccountId, Email = model.Email, IsMfaEnabled = model.IsMfaEnabled};
                    //        _mfaLogic.DisableMfa(user, 0, User.UserAccount.Id);
                    //    }
                    //}
                }
				catch (Exception ex)
				{
					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
					ModelState.AddModelError("Error",
						"An error occurred while deleting membership. Please contact customer support.");
				}

				return Json(ModelState.ToDataSourceResult());
			}

			return RedirectToAction("UnAuthorizedAccess", "Error");
		}

		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup), CspAuthorize(Permission.CreateMembership)]
		public ActionResult CreateGroupMembership(long id) {
			var membership = new Membership {
				ActivationDate = DateTime.Now,
				GroupId = id,
				Roles = GroupLogic.GetGroupDefaultRoles(id, new DataParameters())
			};

			var model = new MembershipDto {
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, 0, ActionHelper.EntityAction.Create),
				Membership = membership,
			};

			var assignedRoles = model.Membership.Roles;
			var availableRoles = GetAvailableRoles(model.Membership);

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
			model.RoleAssignment.Roles = roles;


			return PartialView("../partial/_Membership", model);
		}


		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditSupportAccount), CspAuthorize(Permission.CreateMembership)]
		public ActionResult CreateAccountMembership(long id) {
			var membership = new Membership {
				ActivationDate = DateTime.Now,
				SupportAccountId = id,
				Roles = SupportAccountLogic.GetsupportAccountDefaultRoles(id, new DataParameters())
			};

			var model = new MembershipDto {
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, 0, ActionHelper.EntityAction.Create),
				Membership = membership
			};

			var assignedRoles = model.Membership.Roles;
			var availableRoles = GetAvailableRoles(model.Membership);

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
			model.RoleAssignment.Roles = roles;

			return PartialView("../partial/_Membership", model);
		}

		// Comments by LK On 05/07/2026
		// This method is obsolete and will be removed in the future. Refer to CSP-26851
		/*
		[HttpGet, CspAuthorize(Permission.EditDevice), CspAuthorize(Permission.CreateMembership)]
		public ActionResult CreateDeviceMembership(long id) {
			var device = _deviceLogic.GetDevice(id, User.UserAccount.Id);

			var membership = new Membership {
				ActivationDate = DateTime.Now,
				DeviceId = id,
				Roles = SecurityLogic.GetAvailableRoles(RoleCategory.Device, device.Customer.Id, User.UserAccount.Id)
			};

			var model = new MembershipDto {
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, 0, ActionHelper.EntityAction.Create),
				Membership = membership
			};

			return PartialView("../partial/_Membership", model);
		}
		*/

		// Comments by LK On 05/07/2026
		// This method is obsolete and will be removed in the future. Refer to CSP-26851
		/*
		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.CreateMembership)]
		public ActionResult Create(MembershipDto membershipDto) {
			var roleIds = membershipDto.RoleAssignment.SelectedRoleIds.Split(new[] { ',' });
			var defaultRoles = new List<Role>();

			foreach (var id in roleIds) {
				long roleId;
				long.TryParse(id, out roleId);
				defaultRoles.Add(new Role() { Id = roleId });
			}

			membershipDto.Membership.Roles = defaultRoles;

			var model = new MembershipDto {
				Membership = membershipDto.Membership,
				ActionHelper = new ActionHelper(ControllerFields.MembershipController, membershipDto.Membership.Id, ActionHelper.EntityAction.Create)
			};

			if (!string.IsNullOrEmpty(membershipDto.RoleAssignment.SelectedRoleIds)) {
				ModelState.Remove("Membership.UserAccount.DisplayName");
				ModelState.Remove("Membership.UserAccount.FirstName");
				ModelState.Remove("Membership.UserAccount.LastName");
				ModelState.Remove("Membership.UserAccount.PhoneNumber");
				ModelState.Remove("Membership.UserAccount.Email");
				ModelState.Remove("RoleAssignment.AvailableRoles");

				if (ModelState.IsValid) {
					model.Membership.CreatedByUserAccountId = User.UserAccount.Id;
					model.Membership = MembershipLogic.CreateMembership(membershipDto.Membership);
					model.Membership.UserAccount = UserAccountLogic.GetUserAccount(model.Membership.UserAccount.Id,
																																				 User.UserAccount.Id);

					// Link Membership to SFDC Contact
					MembershipLogic.AddUpdateSalesforceContact(model.Membership, User.UserAccount.Email, User.UserAccount.Id);

					if (model.Membership.ShowThreatResearcherUseCountWarning) {
						model.Notifications.Add(new ResponseNotification("The Threat Researcher role could not be added - use count exceeded", ResponseState.Warning));
					} else {
						ModelState.Clear();
						model.Notifications.Add(new ResponseNotification("Membership information saved", ResponseState.Success));
					}
				}
			} else {
				model.Notifications.Add(new ResponseNotification("At least one role is required.", ResponseState.Error));
			}

			var assignedRoles = model.Membership.Roles;
			var availableRoles = GetAvailableRoles(model.Membership);

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
			model.RoleAssignment.Roles = roles;

			model.ActionHelper = new ActionHelper(ControllerFields.MembershipController, membershipDto.Membership.Id, ActionHelper.EntityAction.Edit);
			return PartialView("../partial/_Membership", model);
		}
		*/

		[HttpGet, AjaxOnly, CspAuthorize(Permission.CreateMembership)]
		public ActionResult GetSupportAccountUserByEmail(long id, string email)
        {
            email = !string.IsNullOrEmpty(email) ? email.Replace("'", "") : String.Empty;
			if (!MembershipLogic.HasMembershipforSupportAccountByEmail(email, id)) {

				return GetUserByEmail(email);
			}
			return Json("member", JsonRequestBehavior.AllowGet);
		}


		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup), CspAuthorize(Permission.CreateMembership)]
		public ActionResult GetGroupUserByEmail(long id, string email)
        {
            email = !string.IsNullOrEmpty(email) ? email.Replace("'", "") : String.Empty;
			if (!MembershipLogic.HasMembershipforGroupByEmail(email, id)) {

				return GetUserByEmail(email);
			}
			return Json("member", JsonRequestBehavior.AllowGet);
		}


		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice), CspAuthorize(Permission.CreateMembership)]
		public ActionResult GetDeviceUserByEmail(long id, string email) {
            email = !string.IsNullOrEmpty(email) ? email.Replace("'", "") : String.Empty;
			if (!MembershipLogic.HasMembershipforDeviceByEmail(email, id)) {

				return GetUserByEmail(email);
			}
			return Json("member", JsonRequestBehavior.AllowGet);
		}

        [Authorize]
		public ActionResult UpdateMembershipStatus(string id, string type, string options = "", string notificationId = "")
		{

            ViewBag.options = Sanitizer.GetSafeHtmlFragment(options);
			var membership = _membershipLogic.GetMembershipByResponseGuid(id);
            var decidingSuperUser = User.UserAccount.Id;
            var actionType = (type.ToUpper() == ActionType.ACCEPT.ToString()) ? ActionType.ACCEPT.ToString() : ActionType.REJECT.ToString();
            var membershipStatusDto = new MembershipStatusDto();

		    if (membership == null)
		    {
		        membershipStatusDto.ActionType = ActionType.NONE.ToString();

		        return View(membershipStatusDto);
		    }

            var supportAccountId = membership.SupportAccountId_Supportaccount_.SupportAccountId;
            var userEmail = membership.UserAccountId_Useraccount_.Email;
            var hasPermissionsToDecide =
                _userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId)
                    .Any(u => decidingSuperUser == u.Id) ||
                _userAccountLogic.HasCPSPAdminRole(User.UserAccount.Id);

		    if (!hasPermissionsToDecide)
		    {
                membershipStatusDto.ActionType = ActionType.NONE.ToString();

                return View(membershipStatusDto);
		    }


            long membershipStatusId;
            long.TryParse(membership.MembershipStatus.ToString(), out membershipStatusId);

		    var isAlreadyDecided = membershipStatusId == (int) MembershipStatus.Accepted ||
		                           membershipStatusId == (int) MembershipStatus.Rejected;

		    if (isAlreadyDecided)
		    {
                membershipStatusDto.ActionType = ActionType.NONE.ToString();
                membershipStatusDto.MembershipStatus = (membershipStatusId == (int) MembershipStatus.Accepted)
		            ? MembershipStatus.Accepted
		            : MembershipStatus.Rejected;

		        return View(membershipStatusDto);
		    }


		    try {
				membershipStatusId = (actionType == ActionType.ACCEPT.ToString())
																 ? (int)MembershipStatus.Accepted
																 : (int)MembershipStatus.Rejected;

				membership.MembershipStatus = (short?)membershipStatusId;
		        if (actionType == ActionType.ACCEPT.ToString())
		        {
		            membership.ExpirationDate = null;
		        }

		        _membershipLogic.UpdateMembershipStatus(membership);

				membershipStatusDto.MembershipStatus = (membershipStatusId == (int)MembershipStatus.Accepted)																																												 ? MembershipStatus.Accepted
																																												 : MembershipStatus.Rejected;
				membershipStatusDto.ActionType = actionType;
                // Call SDFC Delete entitlement contact when the user is rejected
                if (actionType == ActionType.REJECT.ToString())
                {                  
                    _membershipLogic.DeleteSalesforceContact(userEmail, supportAccountId, User.UserAccount.Id);
                }
                if (!String.IsNullOrEmpty(notificationId)) {
					SiteHelper.UpdateNotificationMessageStatus(notificationId);
				}
				else
				{
					var userAccountId = (membership.UserAccountId_Useraccount_ != null)
						? membership.UserAccountId_Useraccount_.UserAccountId.ToString()
						: "";


					SiteHelper.UpdateNotificationMessageStatus(entityId: userAccountId, category: Api.Model.Enums.NotificationCategory.UserRegistration.ToDescription());
				}

				EmailHelper.SendPendingMembershipActionedEmailToUser(membership.MembershipId, membershipStatusDto.MembershipStatus, decidingSuperUser);
                EmailHelper.SendMembershipDecisionedEmailToSuperUsers(supportAccountId, membership.MembershipId, membershipStatusDto.MembershipStatus, decidingSuperUser);
			} catch (Exception ex) {
				membershipStatusDto.MembershipStatus = MembershipStatus.None;
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				membershipStatusDto.Notifications.Add(new ResponseNotification(string.Format("Failed to {0} the user request. Please contact support.", (actionType == "ACCEPT") ? "accept" : "reject"), ResponseState.Error));
			}

			return View(membershipStatusDto);
		}

	    [HttpGet, CspAuthorize(Permission.CreateMembership)]
	    public ActionResult AllPendingMemberships(long id)
	    {
	        //var model = new AllPendingMembershipsDto
	        //{
	        //    PendingMemberships = _membershipLogic.GetAllPendingMemberships(id),
	        //};

	        //return View(model);
           
            return new RedirectResult("~/SupportAccount/AccountUsers/" + id + "/Pending");
        }


        [HttpPost, AjaxOnly]
        public JsonResult CanDeleteUser(long userAccountId, long memberShipTypeId, string action, List<NameValue> newRoles)
	    {
            bool success = false;
            string message = string.Empty;
            StringBuilder errorMessage = new StringBuilder();
	        if (User.HasPermission(Permission.EditMembership) ||
                (memberShipTypeId == (int)MembershipType.Group && User.HasPermission(Permission.EditGroupMembership)))
	        {
	            try
	            {
	                long currentSupportAccountId = CurrentState.GetCurrentAccountId();
	                bool isSuperUser = _securityLogic.IsSuperUser(currentSupportAccountId, userAccountId);
	                var defaultSupportAccountId = _securityLogic.GetDefaultSupportAccount(userAccountId);

	            
	                if (isSuperUser)
	                {
                        var superUserscount = _membershipLogic.GetCountOfUsersByRole(currentSupportAccountId, (int)RoleType.SuperUser);

	                    if (action.Equals(ApiConstants.UPDATE))
	                    {
	                        int count = newRoles.Count(item => Convert.ToInt64(item.Value) == ((long)RoleType.SuperUser));
	                  
	                        //if super user role is removed while edit
	                        if (count == 0 && superUserscount <= 1)
	                        {
	                            errorMessage.AppendLine(Constants.DeleteLastSuperUserPreMessage);
	                        }
	                    }
	              
	                    else if (superUserscount <= 1)
	                    {
                            errorMessage.AppendLine(Constants.DeleteLastSuperUserPreMessage);
	                    }
	                }

                    bool isDomainAdmin = _securityLogic.HasRole(currentSupportAccountId, userAccountId,RoleNames.DomainAdministrator);
                    if (isDomainAdmin)
                    {
                        var domainAdminsCount = _membershipLogic.GetCountOfUsersByRole(currentSupportAccountId, (int)RoleType.DomainAdministrator);

                        if (action.Equals(ApiConstants.UPDATE))
                        {
                            int count = newRoles.Count(item => Convert.ToInt64(item.Value) == ((long)RoleType.DomainAdministrator));

                            //if domain admin role is removed while edit
                            if (count == 0 && domainAdminsCount <= 1)
                            {
                                errorMessage.AppendLine(Constants.DeleteLastDomainAdminPreMessage);
                            }
                        }

                        else if (domainAdminsCount <= 1)
                        {
                            errorMessage.AppendLine(Constants.DeleteLastDomainAdminPreMessage);
                        }
                    }

                    if (!string.IsNullOrEmpty(errorMessage.ToString()))
                    {
                        message = errorMessage.ToString();
                        return new JsonResult { Data = new { Success = string.IsNullOrEmpty(message), message } };
                    }
                
	                if (action.Equals(ApiConstants.DELETE))
	                {
	                    if (defaultSupportAccountId == currentSupportAccountId)
	                    {
	                        success = true;
	                        message = string.Concat(Constants.DeleteDefaultAccountPreMessage, "?");
	                        return new JsonResult { Data = new { Success = success, message } };
	                    }
	                    else
	                    {
	                        success = true;
	                        message = string.Concat(Constants.DeletePreMessage, " ", ApiConstants.MEMBERSHIP, "?");
	                        return new JsonResult { Data = new { Success = success, message } };
	                    }
	                }

                    return new JsonResult { Data = new { Success = string.IsNullOrEmpty(message), message } };
	            }

	            catch (Exception ex)
	            {
	                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	                ModelState.AddModelError("Error", "An error occurred while deleting membership. Please contact customer support.");
	                return new JsonResult { Data = new { success,message  } };
	            }
            }
            return new JsonResult { Data = new { Success = success, message = "An error occurred while deleting membership. Please contact customer support." } };
	    }

        //[HttpPost, AjaxOnly, CspAuthorize(Permission.CreateMembership)]
        //public JsonResult IsValidRole(string email, int roleId)
        //{
        //    var message = string.Empty;

        //    if (string.IsNullOrEmpty(email)) {
        //        message = "Email is required, but not provided.";
        //        return new JsonResult { Data = new { Success = false, Message = message } };
        //    }

        //    if (!Regex.IsMatch(email.Trim(), @"^[\w\.'-]+@[\w\.-]+\.\w{2,12}$")) {
        //        message = string.Format("Please enter a valid email. You entered '{0}'", email.Trim());
        //        return new JsonResult { Data = new { Success = false, Message = message } };
        //    }

        //    var userAccount = GetUserAccountByEmail(email);
        //    if (userAccount == null && roleId.ToString() == RoleType.DomainAdministrator.ToString())
        //    {
        //        message = "User not found.To add a domain administrator , use the 'Create New User' under Members on the side navigation bar";
        //    }

        //    if (string.IsNullOrEmpty(message))
        //        return new JsonResult { Data = new { Success = true, Message = string.Empty } };

        //    return new JsonResult { Data = new { Success = false, Message = message } };
        //}

        #endregion

        private string GetUniqueKey(int size)
        {            
            char[] chars  ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray(); 

            byte[] data = new byte[4*size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        [AjaxOnly][HttpPost]
        public JsonResult IsThirdPartyIdpConfigured(string email)
        {
            bool isSSOUser = true;
            bool isThirdPartyIdpConfigured = false;


            if (!string.IsNullOrEmpty(email))
            {
                //Check if User exists in SSO
                var userProfile = _loginServiceLogic.GetUserProfile(email, Common.GetClientIp(), _applicationSettings.LoginServicePrivateKey);
                if (userProfile == null)
                    isSSOUser = false;

                isThirdPartyIdpConfigured = SiteHelper.IsThirdPartyIdpConfigured(email);
            }
           

            return new JsonResult { Data = new { Success = true, isThirdPartyIdpConfigured , isSSOUser  } };
            
        }
    }
}
