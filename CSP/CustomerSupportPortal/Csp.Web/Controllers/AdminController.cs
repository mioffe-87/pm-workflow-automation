using System;
using System.Linq;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using System.Collections.Generic;

namespace Csp.Web.Controllers
{
    public class AdminController : BaseController
    {
        /// <summary>
        /// This method is used for software uploads.
        /// </summary>
        /// <returns></returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult SoftwareUploads() {
            var defaultDto = new DefaultDto() {
                ActionHelper = new ActionHelper(ControllerFields.SoftwareUploadsController),
            };

            return PartialView("../Partial/_SoftwareUploads", defaultDto);
        }

        /// <summary>
        /// AdminUtilities
        /// </summary>
        /// <returns></returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult AdminUtilities() {
            return PartialView("../Partial/_AdminUtilities");
        }

        /// <summary>
        /// This method is used to get all available roles.
        /// </summary>
        /// <returns>Roles</returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult ManageRoles() {
            var dataParameters = new DataParameters { PaginationCount = 10 };
   
            var roles = new RolesDto {
                DataParameters = dataParameters,
                ActionHelper = new ActionHelper(ControllerFields.AdminController),
                Roles = SecurityLogic.GetAllAvailableRoles(dataParameters)
            };

            return View(roles);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load results in partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult ManageRoles([DataSourceRequest] DataSourceRequest request) {
            var dataParameters = request.ToDataParameters();
            var roles = SecurityLogic.GetAllAvailableRoles(dataParameters);

            var result = new DataSourceResult {
                Data = roles,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        /// <summary>
        /// This method  is used to add or update a role.
        /// </summary>
        /// <param name="id">Role id</param>
        /// <returns>Role</returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult AddUpdateRole(long? id) {
            return GetRoleInformation(id);
        }

        /// <summary>
        /// This method is used to add or update a Role.
        /// </summary>
        /// <param name="model">RoleDto</param>
        /// <returns>RoleDto</returns>
        [HttpPost, AjaxOnly]
        public ActionResult AddUpdateRole(RoleDto model) {
            string message;
            // To find add or update
            var addRole = (model.Role != null && model.Role.Id == 0);

            try {
                var privilegeIds = model.PrivilegeAssignment.SelectedPrivilegeIds.Split(new[] { ',' });
                var assignedPrivileges = new List<Privilege>();

                foreach (var pid in privilegeIds) {
                    long privilegeIdId;
                    long.TryParse(pid, out privilegeIdId);
                    assignedPrivileges.Add(new Privilege() { Id = privilegeIdId });
                }

                if (addRole) {
                    model.Role.CreatedOn = DateTime.Now;
                    model.Role.CreatedByUserAccountId = User.UserAccount.Id;
                    model.Role.Privileges = assignedPrivileges;
                    model.Role = SecurityLogic.CreateRole(model.Role);
                }
                else {
                    model.Role.LastUpdate = DateTime.Now;
                    model.Role.LastUpdateByUserAccountId = User.UserAccount.Id;
                    model.Role.Privileges = assignedPrivileges;
                    model.Role = SecurityLogic.UpdateRole(model.Role);
                }
                
                message = string.Format(ApiConstants.ROLES + " has been {0} successfully.", (addRole) ? "created" : "updated");

                return GetRoleInformation(model.Role.Id, new ResponseNotification(message, ResponseState.Success));
            }
            catch (Exception ex) {
                message = string.Format("Failed to {0} " + ApiConstants.ROLES + ".", (addRole) ? "create" : "update");
                model.Notifications.Add(new ResponseNotification(message, ResponseState.Error));
                return GetRoleInformation(model.Role.Id, new ResponseNotification(message, ResponseState.Error));
            }
        }

        /// <summary>
        /// This method is used to get role information based on role id.
        /// </summary>
        /// <param name="id">Role id</param>
        /// <param name="notification">ResponseNotification</param>
        /// <returns>RoleDto</returns>
        private ActionResult GetRoleInformation(long? id, ResponseNotification notification = null) {
            var dataParameters = new DataParameters { PaginationCount = 100 };
            var roleData = (id.HasValue) ? SecurityLogic.GetRole(id.Value) : new Role();

            var availablePrivileges = SecurityLogic.GetAllPrivileges();
            var assignedPrivileges = (id.HasValue) ? SecurityLogic.GetRolePrivileges(id.Value, dataParameters) : new List<Privilege>(); ;

            var privileges = availablePrivileges.Select(privilege => new PrivilegeDto() { IsSelected = assignedPrivileges.Any(p => p.Id == privilege.Id), Privilege = privilege }).ToList();

            var model = new RoleDto() {
                Role = roleData,
                ActionHelper = new ActionHelper(ControllerFields.AdminController),
                PrivilegeAssignment = { Privileges = privileges }
            };

            if (notification != null) {
                model.Notifications.Add(new ResponseNotification(notification.Message, notification.State));
            }
            ModelState.Clear();
            return PartialView("../Partial/_AddUpdateRole", model);
        }

        /// <summary>
        /// This method is used to delete a role.
        /// </summary>
        /// <param name="id">Role id</param>
        /// <returns>JsonResult</returns>
        [HttpPost, AjaxOnly]
        public ActionResult DeleteRole(long id) {
            string message;
            try {
                SecurityLogic.DeleteRole(id);
                message = ApiConstants.ROLES + " has been deleted successfully.";

                return new JsonResult {
                    Data = new {
                        Success = true,
                        Message = message
                    }
                };
            }
            catch (Exception) {
                message = "Failed to delete " + ApiConstants.ROLES + ".";
            }

            return new JsonResult {
                Data = new {
                    Success = false,
                    Message = message
                }
            };
        }

        /// <summary>
        /// This method is to get all available account types.
        /// </summary>
        /// <returns>AccountTypesDto</returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult ManageAccountTypes() {
            var dataParameters = new DataParameters { PaginationCount = 10 };
        
            var accountTypes = new AccountTypesDto() {
                DataParameters = dataParameters,
                ActionHelper = new ActionHelper(ControllerFields.AdminController),
                AccountTypes = SecurityLogic.GetAllAccountTypes(dataParameters)
            };

            return View(accountTypes);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load results in partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult ManageAccountTypes([DataSourceRequest] DataSourceRequest request) {
            
            var dataParameters = request.ToDataParameters();
            var accountType = SecurityLogic.GetAllAccountTypes(dataParameters);

           var result = new DataSourceResult {
                Data = accountType,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        /// <summary>
        /// This method is used to read or return empty Account type.
        /// </summary>
        /// <param name="id">Account type id</param>
        /// <returns>Account type</returns>
        [HttpGet, CspAuthorize(Permission.ViewAdmin)]
        public ActionResult AddUpdateAccountType(long? id) {
            return GetAccountTypeInformation(id);
        }

        /// <summary>
        /// This method is used to add or update an Account type.
        /// </summary>
        /// <param name="model">AccountTypesDto</param>
        /// <returns>AccountTypesDto</returns>
        [HttpPost, AjaxOnly]
        public ActionResult AddUpdateAccountType(AccountTypesDto model) {
            string message;
            var addAccountType = (model.AccountType != null && model.AccountType.Id == 0);

            try {
                var assignableRoleIds = model.AssignableRoleAssignment.SelectedRoleIds.Split(new[] {','});
                var defaultRoleIds = model.DefaultRoleAssignment.SelectedRoleIds.Split(new[] {','});
                var assignableRoles = new List<Role>();
                var defaultRoles = new List<Role>();

                foreach (var id in assignableRoleIds) {
                    long roleId;
                    long.TryParse(id, out roleId);
                    assignableRoles.Add(new Role() {Id = roleId});
                }

                foreach (var id in defaultRoleIds) {
                    long roleId;
                    long.TryParse(id, out roleId);
                    defaultRoles.Add(new Role() {Id = roleId});
                }

                if (addAccountType) {
                    model.AccountType.CreatedOn = DateTime.Now;
                    model.AccountType.CreatedByUserAccountId = User.UserAccount.Id;
                    model.AccountType.AssignableRoles = assignableRoles;
                    model.AccountType.DefaultRoles = defaultRoles;
                    model.AccountType = SecurityLogic.CreateAccountType(model.AccountType);
                }
                else {
                    model.AccountType.LastUpdate = DateTime.Now;
                    model.AccountType.LastUpdateByUserAccountId = User.UserAccount.Id;
                    model.AccountType.AssignableRoles = assignableRoles;
                    model.AccountType.DefaultRoles = defaultRoles;
                    model.AccountType = SecurityLogic.UpdateAccountType(model.AccountType);
                }

                message = string.Format(ApiConstants.ACCOUNTTYPES + " has been {0} successfully.",
                                        (addAccountType) ? "created" : "updated");

                return GetAccountTypeInformation(model.AccountType.Id, new ResponseNotification(message, ResponseState.Success));
            }
            catch (Exception ex) {
                message = string.Format("Failed to {0} " + ApiConstants.ACCOUNTTYPES + ".",
                                        (addAccountType) ? "create" : "update");
                model.Notifications.Add(new ResponseNotification(message, ResponseState.Error));
                return GetAccountTypeInformation(model.AccountType.Id, new ResponseNotification(message, ResponseState.Error));
            }
        }

        /// <summary>
        /// This method is used to get Account type information for an Account type id.
        /// </summary>
        /// <param name="id">Account type id</param>
        /// <param name="notification">ResponseNotification</param>
        /// <returns>AccountTypesDto</returns>
        private ActionResult GetAccountTypeInformation(long? id, ResponseNotification notification = null) {
            var dataParameters = new DataParameters();
            var accountType = (id.HasValue) ? SecurityLogic.GetAccountType(id.Value) : new AccountType();

            var availableRoles = SecurityLogic.GetAllAvailableRoles(dataParameters).ToList();
            var assignedAssignableRoles = accountType.AssignableRoles;
            var assignedDefaultRoles = accountType.DefaultRoles;

            var assignableRoles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedAssignableRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
            var defaultRoles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedDefaultRoles.Any(r => r.Id == role.Id), Role = role }).ToList();

            var model = new AccountTypesDto {
                AccountType = accountType,
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, accountType.Id),
                AssignableRoleAssignment = { Roles = assignableRoles },
                DefaultRoleAssignment = { Roles = defaultRoles }
            };

            if (notification != null) {
                model.Notifications.Add(new ResponseNotification(notification.Message, notification.State));
            }
            ModelState.Clear();
            return PartialView("../Partial/_AddUpdateAccountType", model);
        }

        /// <summary>
        /// This method is used to get admistrative role for Account type.
        /// </summary>
        /// <param name="accountTypeId">Account type id</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetAdmistrativeRoleForAccountType(long accountTypeId) {
            return Json(SecurityLogic.GetAdmistrativeRoleForAccountType(accountTypeId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This method is used to delete account type.
        /// </summary>
        /// <param name="id">Account type id</param>
        /// <returns>JsonResult</returns>
        [HttpPost, AjaxOnly]
        public ActionResult DeleteAccountType(long id) {
            string message;
            try {
                SecurityLogic.DeleteAccountType(id, User.UserAccount.Id);
                message = ApiConstants.ACCOUNTTYPES + " has been deleted successfully.";

                return new JsonResult {
                    Data = new {
                        Success = true,
                        Message = message
                    }
                };
            }
            catch (Exception) {
                message = "Failed to delete " + ApiConstants.ACCOUNTTYPES + ".";
            }

            return new JsonResult {
                Data = new {
                    Success = false,
                    Message = message
                }
            };
        }
    }
}
