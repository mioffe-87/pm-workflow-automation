using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.Security.Attributes;
using Csp.Api.Security;
using Csp.Web.HelperClasses;
using Csp.Web.DataTransferObjects;
using Kendo.Mvc.UI;
using Csp.Web.Attributes;
using Csp.Api.HelperClasses;

namespace Csp.Web.Controllers {

	public class GroupController : BaseController {

		private readonly IDeviceLogic _deviceLogic;
		private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IMfaLogic _mfaLogic;
        private readonly ISecurityLogic _securityLogic;

        public GroupController(IDeviceLogic deviceLogic, IDeviceTransferLogic deviceTransferLogic, IMfaLogic mfaLogic, ISecurityLogic securityLogic) {
			_deviceLogic = deviceLogic;
			_deviceTransferLogic = deviceTransferLogic;
		    _mfaLogic = mfaLogic;
            _securityLogic = securityLogic;
        }

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditGroup)]
		public ActionResult RemoveDevice(long groupId, long deviceId) {

			GroupLogic.DeleteGroupDevice(groupId, deviceId, User.UserAccount.Id);
			return Json("Success");
		}

		[HttpGet, CspAuthorize(Permission.ViewGroup)]
		public ActionResult ViewGroupPartial(long id, GroupDestination groupDestination = GroupDestination.DevicesProd) {

			var model = new GroupDto {
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id, ActionHelper.EntityAction.View),
				Group = GroupLogic.GetGroup(id, User.UserAccount.Id),
				State = GroupDto.CreateState.NoCreate,
				GroupDestination = groupDestination,
			};

			return PartialView("../partial/_GroupDetails", model);
		}

		[ChildActionOnly, CspAuthorize(Permission.ViewGroupDevices)]
		/*This does not have HttpGet as the AddGroup HttpPost action posts back to the details, not the single form. This is needed
		 * to all for permissions to be loaded and the new group id to be passed to the other tabs. However due to this, this action
		 * is called on a post and a get.*/
		public ActionResult Devices(long id, GroupDestination tab = GroupDestination.DevicesProd) {

			var model = new DevicesDto {
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id),

				SelectedTab = tab,
			};

			return PartialView("../Partial/_Devices", model);

		}


		/// <summary>
		/// This method is initially used to load ProductionDevices _DeviceList partial view when first time called
		/// </summary>
		/// <returns>Partail View</returns>

		[AjaxOnly, CspAuthorize(Permission.ViewGroupDevices)]
		/*This does not have HttpGet as the AddGroup HttpPost action posts back to the details, not the single form. This is needed
		 * to all for permissions to be loaded and the new group id to be passed to the other tabs. However due to this, this action
		 * is called on a post and a get.*/
		public ActionResult ProductionDevices(long id) {

			var dataParameters = new DataParameters { PaginationCount = 10 };
			var deviceListItem = _deviceLogic.GetGroupDevices(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), dataParameters);

			var model = new DeviceListDto {
				DataParameters = dataParameters,
				Devices = deviceListItem,
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id),
				IsProductionDeviceList = true,
				AllDeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), CurrentState.GetLoggedInUser().UserAccount.Id),
				ShowReviewPendingTransfersButton = _deviceTransferLogic.ShowReviewPendingTransfersButton(User.UserAccount, CurrentState.GetSupportAccountId().Value),
			};

			return PartialView("../Partial/_DeviceList", model);

		}


		/// <summary>
		/// This method is used for sorting and searching implementation.
		/// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
		/// </summary>
		/// <param name="request">Grid DataSourceRequest</param>
		/// <param name="id">Group Account ID</param>
		/// <returns></returns>

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewGroupDevices)]
		public ActionResult ProductionDevices(long id, [DataSourceRequest] DataSourceRequest request) {
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
			var deviceListItem = _deviceLogic.GetGroupDevices(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), dataParameters);

			var result = new DataSourceResult {
				Data = deviceListItem,
				Total = Convert.ToInt32(dataParameters.TotalRecordCount)
			};


			return Json(result);
		}


		public FileResult ProductionDevicesReport(long id, string searchField, string searchValue) {

			var csv = _deviceLogic.GetGroupDevicesCsv(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), searchField, searchValue);

			var dateStr = DateTime.Now.ToString("yyyyMMdd");

			return File(csv, "text/comma-separated-values", "ProductionAssetsReport_" + dateStr + ".csv");
		}

		/// <summary>
		/// This method is initially used to load SpareDevices _DeviceList partial view when first time called
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Partail View</returns>

		[HttpGet, AjaxOnly, CspAuthorize(Permission.ViewGroupDevices)]
		public ActionResult SpareDevices(long id) {

			var dataParameters = new DataParameters { PaginationCount = 10 };
			var deviceListItem = _deviceLogic.GetGroupSpareDevices(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), dataParameters);

			var model = new DeviceListDto {
				DataParameters = dataParameters,
				Devices = deviceListItem,
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id),
				IsProductionDeviceList = false,
				AllDeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), CurrentState.GetLoggedInUser().UserAccount.Id),
				ShowReviewPendingTransfersButton = _deviceTransferLogic.ShowReviewPendingTransfersButton(User.UserAccount, CurrentState.GetSupportAccountId().Value),
			};

			return PartialView("../Partial/_DeviceList", model);
		}


		/// <summary>
		/// This method is used for sorting and searching implementation.
		/// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
		/// </summary>
		/// <param name="request">Grid DataSourceRequest</param>
		/// <param name="id">Group Account ID</param>
		/// <returns></returns>

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewGroupDevices)]
		public ActionResult SpareDevices(long id, [DataSourceRequest] DataSourceRequest request) {
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
            var deviceListItem = _deviceLogic.GetGroupSpareDevices(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), dataParameters);

			var result = new DataSourceResult {
				Data = deviceListItem, // Process data (paging and sorting applied)
				Total = Convert.ToInt32(dataParameters.TotalRecordCount) // Total number of records
			};

			return Json(result);
		}

		public FileResult SpareDevicesReport(long id, string searchField, string searchValue) {

			var csv = _deviceLogic.GetGroupSpareDevicesCsv(id, User.UserAccount.Id, CurrentState.GetCurrentAccountId(), searchField, searchValue);

			var dateStr = DateTime.Now.ToString("yyyyMMdd");

			return File(csv, "text/comma-separated-values", "ProductionSparesReport_" + dateStr + ".csv");
		}

		/// <summary>
		/// This method is initially used to load _Memberships partial view when first time called
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Partail View</returns>
		[HttpGet, CspAuthorize(Permission.ViewGroupMemberships)]
		/*This does not have HttpGet as the AddGroup HttpPost action posts back to the details, not the single form. This is needed
		 * to all for permissions to be loaded and the new group id to be passed to the other tabs. However due to this, this action
		 * is called on a post and a get.*/
		public ActionResult Users(long id) {

			var dataParameters = new DataParameters { PaginationCount = 10 };
            var supportAccountId = GroupLogic.GetSupportAccountIdByGroupId(id);
		    var isMfaRequired = _mfaLogic.IsMfaRequiredForAccount(supportAccountId);

			var membershipDto = new MembershipListDto {
				DataParameters = dataParameters,
				Memberships = Mapper.Map<List<SupportAccountMembershipModel>>(MembershipLogic.GetGroupMembers(id, User.UserAccount.Id, dataParameters)),
				AvailableRoles =
					SecurityLogic.GetAvailableRoles(RoleCategory.Group, CurrentState.GetCurrentAccountId(), User.UserAccount.Id)
											 .ToList()
											 .Select(r => new NameValue() { Name = r.Name, Value = r.Id.ToString(), IsActive = r.IsActive})
											 .ToList(),
				DefaultRoles = GroupLogic.GetGroupDefaultRoles(id, new DataParameters())
											 .ToList()
											 .Select(r => new NameValue() { Name = r.Name, Value = r.Id.ToString() })
											 .ToList(),
				MembershipTypeId = (int)MembershipType.Group,
				MembershipParentId = id,
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id),
                IsMfaRequired = isMfaRequired,
                AllowMfaEdit = (_securityLogic.IsSuperUser(CurrentState.GetCurrentAccountId(), User.UserAccount.Id) ||
                                _securityLogic.IsTacUserOrAdmin(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, User.UserAccount.Id) ||
                                _securityLogic.HasRole(CurrentState.GetCurrentAccountId(), User.UserAccount.Id, RoleNames.DomainAdministrator))
            };

			return PartialView("../Partial/_Memberships", membershipDto);
		}


		/// <summary>
		/// This method is used for sorting and searching implementation.
		/// This method is called by Grid using Ajax method to load result in _Memberships partial view.
		/// </summary>
		/// <param name="request">Grid DataSourceRequest</param>
		/// <param name="id">Group Account ID</param>
		/// <returns></returns>

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewGroupMemberships)]
		public ActionResult Users([DataSourceRequest] DataSourceRequest request, long id) {

			var dataParameters = request.ToDataParameters();

			var result = new DataSourceResult {
				Data = MembershipLogic.GetGroupMembers(id, User.UserAccount.Id, dataParameters),
				Total = Convert.ToInt32(dataParameters.TotalRecordCount)
			};

			return Json(result);
		}

		public FileResult UsersCsv(long id, string searchField, string searchValue) {

			var csv = MembershipLogic.GetGroupMembersCsv(id, User.UserAccount.Id, searchField, searchValue);

			var dateStr = DateTime.Now.ToString("yyyyMMdd");

			return File(csv, "text/comma-separated-values", "GroupMembershipReport_" + dateStr + ".csv");
		}

		[HttpGet, CspAuthorize(Permission.EditGroup)]
		public ActionResult EditGroupPartial(long id) {

			var model = new GroupDto {
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id, ActionHelper.EntityAction.Edit),
				Group = GroupLogic.GetGroup(id, User.UserAccount.Id),
				State = GroupDto.CreateState.NoCreate
			};

			return PartialView("../partial/_GroupDetails", model);
		}


		[HttpGet, CspAuthorize(Permission.CreateGroup)]
		public ActionResult AddGroup(long id) {

			var model = new GroupDto {
				Group = new Group { SupportAccountId = id },
				ActionHelper = new ActionHelper(ControllerFields.GroupController, id, ActionHelper.EntityAction.Create),
				State = GroupDto.CreateState.PreCreate,
			};

			return PartialView("../partial/_GroupDetails", model);
		}

		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.CreateGroup)]
		public ActionResult AddGroup(GroupDto groupDto) {


			/* This action posts back to the group details not the create page like other forms. 
			* This is because the other tabs need the group id from the newly created group. 
			* For this to happen this must reload the entire details, rather than just the 
			* _group partial view on the group tab.*/
			var roleIds = groupDto.RoleAssignment.SelectedRoleIds.Split(new[] { ',' });
			var defaultRoles = new List<Role>();

			foreach (var id in roleIds) {
				long roleId;
				long.TryParse(id, out roleId);
				defaultRoles.Add(new Role() { Id = roleId });
			}

			groupDto.Group.DefaultRoles = defaultRoles;

			var model = new GroupDto {
				Group = groupDto.Group,
				State = GroupDto.CreateState.PreCreate
			};
			groupDto.Group.CreatedByUserAccountId = User.UserAccount.Id;

			if (!string.IsNullOrEmpty(groupDto.RoleAssignment.SelectedRoleIds)) {
				ModelState.Remove("Group.Address.CountryId");
				ModelState.Remove("Group.Address.Address1");
				ModelState.Remove("Group.Address.City");
				ModelState.Remove("Group.Address.PostalCode");
				ModelState.Remove("Group.Address.Region");

				if (ModelState.IsValid) {

					model.State = GroupDto.CreateState.PostCreate;
					model.Group = GroupLogic.CreateGroup(model.Group);
					model.Notifications.Add(new ResponseNotification("Group sucessfully saved", ResponseState.Success));
					model.ActionHelper = new ActionHelper(ControllerFields.GroupController, model.Group.Id,
																								ActionHelper.EntityAction.Edit);
					User.ActivePermissions = SecurityLogic.GetGroupPermissionsForPrincipal(model.Group.Id, User.UserAccount);
					ModelState.Clear();
				} else {
					model.ActionHelper = new ActionHelper(ControllerFields.GroupController, model.Group.Id,
																								ActionHelper.EntityAction.Create);
				}
			} else {
				model.Notifications.Add(new ResponseNotification("At least one role is required.", ResponseState.Error));
			}

			var assignedRoles = model.Group.DefaultRoles;
			var availableRoles = SecurityLogic.GetAvailableRoles(RoleCategory.Group, model.Group.SupportAccountId, User.UserAccount.Id).ToList();

			var roles = availableRoles.Select(role => new RoleDto() { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
			model.RoleAssignment.Roles = roles;

			return PartialView("../partial/_GroupDetails", model);
		}

		[HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.DeleteGroup)]
		public ActionResult Delete(long id) {


			GroupLogic.DeleteGroup(id, User.UserAccount.Id);


			return Json("Success");
		}

		[HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup)]
		public ActionResult GetAvailableDeviceGroups(long id) {

			var groupListItems = GroupLogic.GetAvailableGroups(id, User.UserAccount.Id, new DataParameters());
			ViewBag.ActionHelper = new ActionHelper(ControllerFields.GroupController, id);

			return PartialView("../Partial/_DeviceGroups", groupListItems);

		}

		[HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.EditGroup)]
		public ActionResult AddGrouptoDevice(string deviceId, string groupId) {
			var stringSeparators = new[] { "," };


			var result = groupId.Split(stringSeparators, StringSplitOptions.None);

			foreach (var resultgroupId in result) {
				if (resultgroupId != string.Empty) {
					GroupLogic.CreateGroupDevice(Convert.ToInt64(resultgroupId), Convert.ToInt64(deviceId), User.UserAccount.Id);
				}
			}

			return Json("Success", JsonRequestBehavior.AllowGet);

		}

		[HttpGet, CspAuthorize(Permission.EditGroup)]
		public JsonResult IsUserGroupNameAvailable(Int64 Id, string Name) {

			if (Name.Length > 0) {

				var searchResults = GroupLogic.CheckIfGroupNameExist(Id, Name);

				//We need to check for an exact match here as the search only does a "like"
				if (searchResults == "0") {

					return Json(true, JsonRequestBehavior.AllowGet);
				}
				return Json("Group name already exists", JsonRequestBehavior.AllowGet);
			}

			return Json(true, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Load landing page partial is exactly like LoadLandingPage, but returns a partial so it can launch a pop-up
		/// </summary>
		/// <param name="id"></param>
		/// <param name="groupDestination"></param>
		/// <returns></returns>
		public ActionResult LoadLandingPagePartial(long id, GroupDestination groupDestination = GroupDestination.DevicesProd) {

			switch (groupDestination) {
				case GroupDestination.DevicesProd:
					return RedirectToAction("ViewGroupPartial", "Group", new { id = id, groupDestination = GroupDestination.DevicesProd });

				case GroupDestination.DevicesSpare:
					return RedirectToAction("ViewGroupPartial", "Group", new { id = id, groupDestination = GroupDestination.DevicesSpare });

				case GroupDestination.DevicesVmCodes:
					return RedirectToAction("ViewGroupPartial", "Group", new { id = id, groupDestination = GroupDestination.DevicesVmCodes });

				case GroupDestination.Memberships:
					return RedirectToAction("ViewGroupPartial", "Group", new { id = id, groupDestination = GroupDestination.Memberships });

                case GroupDestination.DevicesCnCodes:
                    return RedirectToAction("ViewGroupPartial", "Group", new { id = id, groupDestination = GroupDestination.DevicesCnCodes });

				default: throw new InvalidEnumArgumentException();
			}
		}


	}

	public enum GroupDestination {
		DevicesProd,
		DevicesSpare,
		DevicesVmCodes,
		Memberships,
		EPAuthCodes,
        SiteLicenses,
		SearchAll,
        ClaimedAssets,
        PrepBulkRegistration,
        CustomizeBulkRegistration,
        BulkRegistrationReview,
        EnterpriseAgreements,
        AssetHistory,
        SaaService,
		LineCards,
		DevicesCnCodes
	}
}
