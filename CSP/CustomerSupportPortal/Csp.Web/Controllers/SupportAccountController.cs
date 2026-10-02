using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Logic;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Csp.Web.HelperClasses;
using Csp.Web.DataTransferObjects;
using Csp.Web.Attributes;
using Csp.Api.Exceptions;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using Csp.Api.HelperClasses;
using Group = Csp.Api.Model.Group;
using System.Net.Mail;
using System.Text;
using System.Web.Security;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Web.Controllers
{

    [Authorize]
    public class SupportAccountController : BaseController
    {

        private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IHomeLogic _homeLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly ISalesforceService _salesforceService;
        private readonly IPingLogic _pingLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly ISelfServiceRegistrationCodeService _selfServiceRegistrationCodeService;
        private readonly ICsspLogic _csspLogic;
        private readonly ISalesforceRestApiLogic _salesforceRestApiLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly IReturnsLogic _returnsLogic;
        private readonly IXSOARLogic _xsoarLogic;
        private readonly IImpersonateRequestLogic _impersonateRequestLogic;
        private readonly ILoginHistoryLogic _loginHistoryLogic;
        private readonly string impersonate_topic = ConfigurationManager.AppSettings.Get("Impersonate_TopicName");
        private readonly string impersonate_project = ConfigurationManager.AppSettings.Get("Impersonate_ProjectName");
        private readonly string data_collection_topic = ConfigurationManager.AppSettings.Get("Data_Collection_TopicName");
        private readonly string data_collection_project = ConfigurationManager.AppSettings.Get("Data_Collection_ProjectName");
        private readonly EventPublisher eventPublisher = new EventPublisher();
        public SupportAccountController(IDeviceTransferLogic deviceTransferLogic, IDeviceLogic deviceLogic, IUserAccountLogic userAccountLogic, IHomeLogic homeLogic,
            ISupportAccountLogic supportAccountLogic, ISalesforceService salesforceService, IPingLogic pingLogic, ISecurityLogic securityLogic,
            ISelfServiceRegistrationCodeService selfServiceRegistrationCodeService, ICsspLogic csspLogic,
            ISalesforceRestApiLogic salesforceRestApiLogic, ISiteLicenseLogic siteLicenseLogic, IReturnsLogic returnsLogic, 
            IXSOARLogic xsoarLogic, IImpersonateRequestLogic impersonateRequestLogic, ILoginHistoryLogic loginHistoryLogic)
        {
            _deviceTransferLogic = deviceTransferLogic;
            _deviceLogic = deviceLogic;
            _userAccountLogic = userAccountLogic;
            _homeLogic = homeLogic;
            _supportAccountLogic = supportAccountLogic;
            _salesforceService = salesforceService;
            _pingLogic = pingLogic;
            _securityLogic = securityLogic;
            _selfServiceRegistrationCodeService = selfServiceRegistrationCodeService;
            _csspLogic = csspLogic;
            _salesforceRestApiLogic = salesforceRestApiLogic;
            _siteLicenseLogic = siteLicenseLogic;
            _returnsLogic = returnsLogic;
            _xsoarLogic = xsoarLogic;
            _impersonateRequestLogic = impersonateRequestLogic;
            _loginHistoryLogic = loginHistoryLogic;
        }

        [HttpGet, ChildActionOnly]
        public ActionResult AccountInfo(long id)
        {
            var model = new AccountInfoDto()
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                PartialId = id
            };

            return PartialView("../Partial/_AccountInfo", model);
        }

		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult ViewAccount(long id) {
            return View("../AccountDetails/AccountDetails");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult View(long id)
        {
            var model = GetSupportAccountDto(id, User.UserAccount.Id);

            return PartialView("../Partial/_ViewSupportAccount", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountGroups)]
        public ActionResult AccountGroups(long id)
        {

            var model = new SupportAccountTabsDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = SupportAccountTabs.Groups,
                PartialId = id,
                HasGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Any(),
                IsCsspAccount = _supportAccountLogic.IsCsspAccount(id)
            };

            return View("../SupportAccount/Details", model);
        }

        /// <summary>
        /// This method is initially used to load _groups partial view when first time called
        /// </summary>
        /// <returns>Partail View</returns>
        [HttpGet, CspAuthorize(Permission.ViewSupportAccountGroups)]
        public ActionResult Groups(long id)
        {

            var dataParameters = new DataParameters { PaginationCount = 10 };

            var model = new GroupsListDto
            {
                DataParameters = dataParameters,
                Groups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, dataParameters),
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id)
            };

            return PartialView("../Partial/_Groups", model);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _groups partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Support Account ID</param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountGroups)]
        public ActionResult Groups(long id, [DataSourceRequest] DataSourceRequest request)
        {

            var dataParameters = request.ToDataParameters();
            var groupListItems = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, dataParameters);

            var result = new DataSourceResult
            {
                Data = groupListItems,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.CreateGroup)]
        public ActionResult CreateGroup(GroupListItem model, long supportAccountId)
        {

            if (model != null && ModelState.IsValid)
            {

                ModelState.Clear();

                var allGroupRoles = SecurityLogic.GetAvailableRoles(RoleCategory.Group, supportAccountId, User.UserAccount.Id);
                var defaultGroupRole = allGroupRoles.Where(x => x.Name == RoleNames.GroupDefault).ToList();

                var newGroup = new Group
                {
                    SupportAccountId = supportAccountId,
                    Name = model.SupportAccountGroupName,
                    Description = model.Description,
                    DefaultRoles = defaultGroupRole,
                    CreatedByUserAccountId = User.UserAccount.Id,
                };
                newGroup = GroupLogic.CreateGroup(newGroup);

                model.GroupId = newGroup.Id;
                model.CreatedOn = newGroup.CreatedOn;
                model.CreatedByEmail = newGroup.CreatedByEmail;
            }

            return Json(new[] { model }.ToDataSourceResult(new DataSourceRequest(), ModelState));
        }

        [HttpGet]
        public ActionResult MyAccounts(long? id, bool? isRedirect, string targetResource, string impersonateData = null, string caseId = null)
        {
            return RedirectToAction("MyAccount", new { id = id, isRedirect = isRedirect, impersonateData = impersonateData, targetResource = targetResource, caseId = caseId });          
        }

        private ActionResult ValidateMembership()
        {
            if (!User.UserAccount.IsValid)
            {
                if (User.RoleSecurities.Any())
                {
                    if (CurrentState.GetSupportAccount().IsFedramp)
                    {
                        var hasFedrampClassification = User.RoleSecurities.Find(x => x.SecurityClassification == RoleSecurity.SecurityClassifications.Fedramp);

                        // fedramp TAC user , pass ValidateMembership
                        if (hasFedrampClassification != null && hasFedrampClassification.SupportAccountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID)
                        {
                            return null;
                        }
                        if (hasFedrampClassification == null || !CurrentState.HasDirectMembership())
                        {
                            return RedirectToAction("UnAuthorized", "Error");
                        }
                    }

                    if (!User.ActivePermissions.Any())
                    {
                        return RedirectToAction("AccountSelector", "home", new
                        {
                            isExpired = true
                        });
                    }
                }
                else
                {
                    if (User?.UserAccount?.MembershipStatus?.ToLower() == MembershipStatus.Pending.ToString().ToLower())
                    {
                        return RedirectToAction("UnAuthorizedUser", "Error", new { hasExpired = false, isPending = true });
                    }
                    return RedirectToAction("UnAuthorizedUser", "Error", new { hasExpired = User?.UserAccount?.IsMembershipExpired, isPending = false, isValid = User?.UserAccount?.IsValid });
                }
            }
            if (User?.UserAccount?.MembershipStatus?.ToLower() == MembershipStatus.Pending.ToString().ToLower())
            {
                return RedirectToAction("UnAuthorizedUser", "Error", new { hasExpired = false, isPending = true });
            }
            return null;
        }

        private string HandleRedirect(bool? isRedirect, string targetResource) {
            if (isRedirect.HasValue && isRedirect.Value)
            {
                if (!string.IsNullOrEmpty(targetResource))
                {
                    if (targetResource.Contains("TargetResource"))
                    {
                        var redirectUrl = targetResource.Split(new string[] { "TargetResource=" }, StringSplitOptions.None);

                        if (redirectUrl.Length > 0)
                        {
                            targetResource = string.Format("{0}TargetResource={1}", redirectUrl[0], HttpUtility.UrlEncode(redirectUrl[1]));
                        }
                    }

                    return targetResource;
                }

                var configValues = new ConfigValues();
                var accountSelectorNoDestinationUrl = configValues.GetAppSetting(AppSettings.AccountSelectorNoDestinationUrl);

                return accountSelectorNoDestinationUrl;
            }
            return null;
        }

        private ActionResult CheckAndLogImpersonateData(string impersonatorEmail,long? id, bool? logHistory, string sessionId) {
            #region Impersonate check & log

            if (string.IsNullOrEmpty(impersonatorEmail))
            {
                return null;
            }
                //check if user has approved impersonate request to the current account
                UserAccount impersonatorUser = _userAccountLogic.GetUserAccountByEmail(impersonatorEmail);
                if (impersonatorUser != null)
                {
                    string impersonateRequestId = string.Empty;
                    bool hasAccessToImpersonate = _impersonateRequestLogic.HasValidImpersonateRequest(new ImpersonateRequest()
                    {
                        TargetUserAccountId = User.UserAccount.Id,
                        TargetSupportAccountId = id.Value,
                        RequestorUserAccountId = impersonatorUser.Id,
                        ImpersonateAction = ImpersonateAction.ActiveRequests.ToDescription(),
                        RequestorEmailAddress = impersonatorEmail,
                        TargetEmailAddress = User.UserAccount.Email
                    }, out impersonateRequestId);

                    try
                    {
                        if (logHistory.HasValue && logHistory == true)
                        {
                            // Add Login History
                            var loginHistory = new LoginHistoryModel()
                            {
                                SessionId = sessionId,
                                UserAccountId = User.UserAccount.Id,
                                SupportAccountId = id.Value,
                                IsOffline = false,
                                Browser = Request.Browser.Browser,
                                Platform = Request.Browser.Platform,
                                SourceIpAddress = Request.UserHostAddress,
                                ImpersonateUser = impersonatorEmail,
                                LoginStatus = hasAccessToImpersonate ? "Success" : "Failed",
                                ImpersonateRequestId = impersonateRequestId,
                                LoginTime = DateTime.Now,
                                LoginTimeAsString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            };

                            _loginHistoryLogic.AddLoginHistory(loginHistory);

                            eventPublisher.PublishCustomEvent(loginHistory, impersonate_project, impersonate_topic);
                        }
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    }

                    if (!hasAccessToImpersonate)
                    {
                        CurrentState.ClearAll();
                        return RedirectToAction("UnAuthorizedAccount", "Error");
                    }
                }
            return null;

            #endregion
        }

        private ActionResult ValidateAndSetImpersonateData(string impersonateData, string sessionId, out long? id, out string impersonatorEmail)
        {
            bool hasImpersonateData = !string.IsNullOrEmpty(impersonateData); //  requestUrl.ToLower().Contains("impersonatedata|");
            impersonatorEmail = string.Empty;
            id = 0;
            if (hasImpersonateData)
            {
                /*sample url formats :
                  hostname/supportAccount/myaccount?impersonatedata|4009002030330&queryparma=somevalue
                  hostname/supportAccount/myaccount?impersonatedata|4009002030330
                */
                //int startIndexOf = requestUrl.IndexOf('=');
                //int endIndexOf = requestUrl.IndexOf('&');
                //int length = endIndexOf > 0 ? (endIndexOf - startIndexOf) : (requestUrl.Length - startIndexOf);
                //string impersonateData = requestUrl.Substring(startIndexOf + 1, length - 1);

                //if (string.IsNullOrEmpty(impersonateData))
                //{
                //    CurrentState.ClearAll();
                //    return RedirectToAction("UnAuthorizedAccount", "Error");
                //}
                    string hexString = SecurityHelper.ConvertHexToString(impersonateData);
                    string decryptedString = string.IsNullOrEmpty(hexString) ? string.Empty : SecurityHelper.Decrypt(hexString);
                    string[] impersonateDetails = string.IsNullOrEmpty(decryptedString) ? new string[0] : decryptedString.Split('|');
                    if (impersonateDetails.Length == 2)
                    {
                        impersonatorEmail = impersonateDetails[0];
                        id = Convert.ToInt64(impersonateDetails[1]);
                        setImpersonateData(impersonatorEmail, sessionId);
                    }
                    else
                    {
                        CurrentState.ClearAll();
                        return RedirectToAction("UnAuthorizedAccount", "Error");
                    }
               
            }
            else
            {
                string impersonaterCookie = getImpersonateData();
                if (!string.IsNullOrEmpty(impersonaterCookie))
                {
                    impersonatorEmail = impersonaterCookie;
                }

            }
            return null;
        }

        [HttpGet]
        public ActionResult MyAccount(long? id, bool? isRedirect, string targetResource, bool? logHistory, string impersonateData = null, bool? isRelogin = false,string caseId=null)
        {

            string impersonatorEmail = string.Empty;
            string sessionId = Session.SessionID;
            string requestUrl = Request.Url.ToString();
            long? supportaccountid;

            if (isRelogin.HasValue && isRelogin == true)
            {
                UpdateLoginUserHistory();
                ClearAll();
                return RedirectToAction("Login", "Security", new { returnurl = targetResource });
            }

            HttpCookie registerHistoryCookie = Request.Cookies[ApiConstants.RegisterHistory_Identifier];
            if (registerHistoryCookie != null)
            {
                return RedirectToAction("Register", "Home", new { history = registerHistoryCookie.Value });
            }

            //check if impersonate session
            ValidateAndSetImpersonateData(impersonateData, sessionId, out supportaccountid, out impersonatorEmail);

            if (supportaccountid.HasValue && supportaccountid > 0) {
                id = supportaccountid;
            }

            //check if membership is valid
            ActionResult result = ValidateMembership();
            if (result != null) { return result; }

            if (!id.HasValue)
            {
               if (User.UserAccount.DefaultSupportAccountId.HasValue)
                {
                    return RedirectToAction("MyAccount", new
                    {
                        id = User.UserAccount.DefaultSupportAccountId.Value,
                        impersonateData = impersonateData
                    });
                }
            }
            //else
            //{
            //check if redirect & has target resource
            string redirectUrl = HandleRedirect(isRedirect, targetResource);
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    return Redirect(redirectUrl);
                }

                //log impersonate data 
                ActionResult impersonateResult = CheckAndLogImpersonateData(impersonatorEmail, id, logHistory, sessionId);
                if (impersonateResult != null) { return impersonateResult; }

                //var supportAccountId = _pingLogic.GuessCurrentMembershipId(User.UserAccount.Id, id.Value);
                var selectedMembershipId = CurrentState.GetSelectedMembershipId().Value;
                _userAccountLogic.SetSelectedMembershipAndSalesForce(User.UserAccount.Id, selectedMembershipId, id.Value);
            //}

            if (!string.IsNullOrEmpty(caseId)) {
                var configValues = new ConfigValues();
                string formatCaseDetailUrl = string.Format(configValues.GetAppSetting(AppSettings.CaseDetailUrl), caseId);
                return Redirect(formatCaseDetailUrl);
            }
            
            if (!User.UserAccount.Email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION)){
                UserAccount userAccount = _userAccountLogic.GetUserAccountByEmail(User.UserAccount.Email);
                if (userAccount != null 
                                        && (string.IsNullOrEmpty(userAccount.FirstName) || string.IsNullOrEmpty(userAccount.LastName) || string.IsNullOrEmpty(userAccount.PhoneNumber))
                                        && string.IsNullOrEmpty(impersonateData))
                    return RedirectToAction("EditProfile", new { id });
            }

            eventPublisher.PublishCustomEvent(new
            {
                support_account_id = id,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }, data_collection_project, data_collection_topic);

            return View("../SupportAccount/MyAccount"); 
        }

        [HttpGet]
        public ActionResult Home(long? id)
        {
            var currentData = _homeLogic.GetCurrentLoggedInData(id, User.UserAccount.Id);

            if (currentData.IsSupportAccountSelected)
            {
                var email = HttpContext.User.Identity.Name;
                var selectedMembershipId = CurrentState.GetSelectedMembershipId().Value;
                _supportAccountLogic.AddUpdateSelectedSupportAccount(email, selectedMembershipId, id.Value);

                _userAccountLogic.SetSelectedMembershipAndSalesForce(User.UserAccount.Id, selectedMembershipId, id.Value);
            }

            var canSeeAutoFocusLink =
                id.HasValue && _securityLogic.CanSeeAutoFocusLink(User.UserAccount.Id, id.Value);

            var hasReturnsPortalAccess = id.HasValue && _returnsLogic.ShowReturnsPortalLink(id.Value, User.UserAccount.Id);

            var model = new HomeDto
            {
                CurrentAccount = currentData.CurrentAccount,
                CurrentRoles = currentData.CurrentRoles.Distinct().OrderBy(x => x),
                //Resources = _homeLogic.GetResourceLinks(CurrentState.HasLoggedInPermission(Permission.ViewSoftwareUpdates),
                //	CurrentState.HasLoggedInPermission(Permission.ViewDynamicUpdates), id.HasValue,
                //	CurrentState.HasLoggedInPermission(Permission.ViewCaseManagement),
                //						canSeeAutoFocusLink, CurrentState.HasLoggedInPermission(Permission.ViewASCDashboardReport), hasReturnsPortalAccess, CurrentState.HasLoggedInPermission(Permission.ViewPartnerServices), User.UserAccount.Id,id.Value),
                DefaultSupportAccountId = currentData.DefaultSupportAccountId,
                IsDefault = currentData.IsDefault,
                CurrentSupportAccountId = currentData.CurrentSupportAccountId,
                NumberOfAccounts = _homeLogic.GetNumberOfAccountsUserCanAccess(User.UserAccount.Id),
                HasPendingMemberships = _homeLogic.HasPendingMemberships(User.UserAccount.Id),
                HasAnyPanAccess = User.RoleSecurities.HasAnyPanAccess(),
            };

            return PartialView("../Partial/_Home", model);
        }

        [HttpGet]
        public ActionResult SetDefault(long id)
        {

            _homeLogic.SetDefault(id, User.UserAccount.Id);

            return RedirectToAction("MyAccounts", new { id = id });
        }

        [HttpGet]
        public ActionResult ChangeAccount(long id)
        {

            var dataParameters = new DataParameters { PaginationCount = 10 };

            var model = new ChangeAccountDto
            {
                DataParameters = dataParameters,
                Accounts = _homeLogic.GetAvailableAccounts(User.UserAccount.Id, dataParameters, setRoles: true),
            };

            return PartialView("../Partial/_ChangeAccount", model);
        }

        [HttpPost]
        public ActionResult ChangeAccount([DataSourceRequest] DataSourceRequest request)
        {

            var dataParameters = request.ToDataParameters();

            var accounts = _homeLogic.GetAvailableAccounts(User.UserAccount.Id, dataParameters, setRoles: true);

            var result = new DataSourceResult
            {
                Data = accounts,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        /// <summary>
        ///     This method is obsolete and is no longer being used.
        /// </summary>
        /// <param name="id"></param>       
        /// <returns></returns>
        /// 
        //[HttpGet]
        //public ActionResult ChangeDeviceAccount(long id)
        //{

        //    var dataParameters = new DataParameters { PaginationCount = 10 };

        //    var model = new ChangeAccountDto
        //    {
        //        DataParameters = dataParameters,
        //        Accounts = _homeLogic.GetAvailableAccountsForDevice(User.UserAccount.Id, dataParameters, setRoles: true),
        //    };

        //    return PartialView("../Partial/_ChangeDeviceAccount", model);
        //}
        /// <summary>
        ///     This method is obsolete and is no longer being used.
        /// </summary>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult ChangeDeviceAccount([DataSourceRequest] DataSourceRequest request)
        //{

        //    var dataParameters = request.ToDataParameters();

        //    var accounts = _homeLogic.GetAvailableAccountsForDevice(User.UserAccount.Id, dataParameters, setRoles: true);

        //    var result = new DataSourceResult
        //    {
        //        Data = accounts,
        //        Total = Convert.ToInt32(dataParameters.TotalRecordCount)
        //    };

        //    return Json(result);
        //}

        /// <summary>
        ///     This method is obsolete and is no longer being used.
        /// </summary>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        /// 
        //public FileResult ChangeAccountReport(string searchField, string searchValue)
        //{

        //    var csv = _homeLogic.GetAvailableAccountsCsv(User.UserAccount.Id, searchField, searchValue);

        //    var dateStr = DateTime.Now.ToString("yyyyMMdd");

        //    return File(csv, "text/comma-separated-values", "AccountsReport_" + dateStr + ".csv");
        //}


        /// <summary>
        ///     This method is obsolete and is no longer being used.
        /// </summary>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        /// 
        //public FileResult ChangeDeviceAccountReport(string searchField, string searchValue)
        //{

        //    var csv = _homeLogic.GetAvailableDeviceAccountsCsv(User.UserAccount.Id, searchField, searchValue);

        //    var dateStr = DateTime.Now.ToString("yyyyMMdd");

        //    return File(csv, "text/comma-separated-values", "AccountsReport_" + dateStr + ".csv");
        //}


        /// <summary>
        /// This method is get the registered line cards based on supported account Id
        /// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
        /// </summary>        
        /// <param name="id">Support Account ID</param>
        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult GetDeviceLineCards(long id)
        {
            var dataParameters = new DataParameters { PaginationCount = 10, SearchField = "SerialNumber" };
            var lineCards = _deviceLogic.GetComponentsBySupportAccount(id);
            var model = new DeviceDto
            {
                DataParameters = dataParameters,
                Device = new Device { LineCards = lineCards },
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id)
            };

            return PartialView("../Partial/_ViewDeviceLineCards", model);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Support Account ID</param>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <returns> </returns>
        [HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult GetDeviceLineCards(long id, [DataSourceRequest] DataSourceRequest request, string searchField = null, string searchValue = null)
        {

            var dataParameters = request.ToDataParameters(searchField, searchValue);
            var lineCards = _deviceLogic.GetComponentsBySupportAccount(id, dataParameters);

            var result = new DataSourceResult
            {
                Data = lineCards,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            return Json(result);
        }
        /// <summary>
        /// This method is initially used to load ProductionDevices _DeviceList partial view when first time called
        /// </summary>
        /// <returns>Partail View</returns>
        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult ProductionDevices(long id, string defaultDeviceCode = null)
        {

            var dataParameters = new DataParameters { PaginationCount = 10, SearchField = "SerialNumber", SearchValue = defaultDeviceCode };
            bool hideExpiredLicenses = _deviceLogic.GetUserPreferences(User.UserAccount.Id, id);

            var deviceDto = new DeviceListDto
            {
                DataParameters = dataParameters,
                Devices = _deviceLogic.GetSupportAccountDevices(id, User.UserAccount.Id, dataParameters, hideExpiredLicenses),
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                IsProductionDeviceList = true,
                AllDeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), CurrentState.GetLoggedInUser().UserAccount.Id),
                ShowReviewPendingTransfersButton = _deviceTransferLogic.ShowReviewPendingTransfersButton(User.UserAccount, CurrentState.GetSupportAccountId().Value),
                DefaultDeviceCode = defaultDeviceCode,
                ShowBulkDeviceTransferButton = User.HasPermission(Permission.BulkDeviceTransfer) && _deviceLogic.ShowBulkDeviceTransfer(id, User.UserAccount.Id),
                HideExpiredLicenses = hideExpiredLicenses
            };


            return PartialView("../Partial/_DeviceList", deviceDto);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
        /// </summary>
        /// <param name="id">Support Account ID</param>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <param name="hideExpiredLicenses"></param>
        /// <returns> </returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult ProductionDevices(long id, [DataSourceRequest] DataSourceRequest request, bool hideExpiredLicenses, string searchField = null, string searchValue = null)
        {
            if (!SearchHelper.ValidateSearchField(searchField))
            {
                searchField = string.Empty;
                searchValue = string.Empty;
            }

            var dataParameters = request.ToDataParameters(searchField, searchValue);

            if (dataParameters.SearchField == "SerialNumber")
            {

                if (string.IsNullOrEmpty(dataParameters.SearchValue) == false && dataParameters.SearchValue.ToLower().StartsWith("i-"))
                {
                    dataParameters.SearchValue = dataParameters.SearchValue.Replace("i-", ""); // This will search aws instance ids as well.
                }
            }

            var deviceListItem = _deviceLogic.GetSupportAccountDevices(id, User.UserAccount.Id, dataParameters, hideExpiredLicenses);
            _deviceLogic.UpdateHideExpiredLicensesFlag(User.UserAccount.Id, id, hideExpiredLicenses);
            var result = new DataSourceResult
            {
                Data = deviceListItem,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        public FileResult ProductionDevicesReport(long id, string searchField, string searchValue, bool hideExpiredLicenses)
        {
            hideExpiredLicenses = _deviceLogic.GetUserPreferences(User.UserAccount.Id, id);

            var csv = _deviceLogic.GetSupportAccountDevicesCsv(id, User.UserAccount.Id, searchField, searchValue, hideExpiredLicenses);

            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            return File(csv, "text/comma-separated-values", "ProductionAssetsReport_" + dateStr + ".csv");
        }

        /// <summary>
        /// This method is initially used to load SpareDevices _DeviceList partial view when first time called
        /// </summary>
        /// <returns>Partail View</returns>
        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult SpareDevices(long id, string defaultDeviceCode = null)
        {

            var dataParameters = new DataParameters { PaginationCount = 10, SearchField = "SerialNumber", SearchValue = defaultDeviceCode };
            var deviceDto = new DeviceListDto
            {
                DataParameters = dataParameters,
                Devices = _deviceLogic.GetSupportAccountSpareDevices(id, User.UserAccount.Id, dataParameters),
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                IsProductionDeviceList = false,
                AllDeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), CurrentState.GetLoggedInUser().UserAccount.Id),
                ShowReviewPendingTransfersButton = _deviceTransferLogic.ShowReviewPendingTransfersButton(User.UserAccount, CurrentState.GetSupportAccountId().Value),
                DefaultDeviceCode = defaultDeviceCode,
            };
            return PartialView("../Partial/_DeviceList", deviceDto);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _DeviceList partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Support Account ID</param>
        /// <param name="searchField"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult SpareDevices(long id, [DataSourceRequest] DataSourceRequest request, string searchField = null, string searchValue = null)
        {
            searchField = !string.IsNullOrEmpty(searchField) ? searchField.Replace("'", "") : String.Empty;
            searchValue = !string.IsNullOrEmpty(searchValue) ? searchValue.Replace("'", "") : String.Empty;
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
            var dataParameters = sanitizedRequest.ToDataParameters(searchField, searchValue);
            var deviceListItem = _deviceLogic.GetSupportAccountSpareDevices(id, User.UserAccount.Id, dataParameters);

            var result = new DataSourceResult
            {
                Data = deviceListItem,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        public FileResult SpareDevicesReport(long id, string searchField, string searchValue)
        {

            var csv = _deviceLogic.GetSupportAccountSpareDevicesCsv(id, User.UserAccount.Id, searchField, searchValue);

            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            return File(csv, "text/comma-separated-values", "ProductionSparesReport_" + dateStr + ".csv");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountMemberships)]
        public ActionResult AccountUsers(long id, string config = null)
        {

            var model = new SupportAccountTabsDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = SupportAccountTabs.Users,
                PartialId = id,
                HasGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Any(),
                IsCsspAccount = _supportAccountLogic.IsCsspAccount(id)
            };

            return View("../SupportAccount/Details", model);
        }

        /// <summary>
        /// This method is initially used to load _Memberships partial view when first time called
        /// </summary>
        /// <returns>Partail View</returns>
        [HttpGet, CspAuthorize(Permission.ViewSupportAccountMemberships)]
        public ActionResult Users(long id)
        {
            //      var dataParameters = new DataParameters { PaginationCount = 10 };
            //bool isMfaRequired = _mfaLogic.IsMfaRequiredForAccount(id);
            //      var userInfo = UserHelper.GetSsoToken();
            //      List<SupportAccountDomainModel> uniqueDomains;
            //      int domainAdmin = (int)RoleType.DomainAdministrator;

            //      var memberships = MembershipLogic.GetSupportAccountMembers(id, User.UserAccount.Id, dataParameters, out uniqueDomains);

            //      foreach (var udomain in uniqueDomains)
            //      {
            //          udomain.IsThridPartyIdpConfigured = SiteHelper.IsThirdPartyIdpConfigured(udomain.Domain);
            //      }

            //      foreach (var umembership in memberships)
            //      {
            //          if (umembership.Roles.Any(r => r.Value == domainAdmin.ToString()))
            //              umembership.IsThridPartyIdpConfigured = false;
            //          else
            //              umembership.IsThridPartyIdpConfigured = (from u in uniqueDomains where u.Domain == umembership.Domain select u.IsThridPartyIdpConfigured).FirstOrDefault();
            //      }

            //      var membershipDto = new MembershipListDto
            //      {
            //          DataParameters = dataParameters,
            //          Memberships = memberships,
            //          AvailableRoles =
            //              SecurityLogic.GetAvailableRoles(RoleCategory.SupportAccount, CurrentState.GetCurrentAccountId(), User.UserAccount.Id)
            //                                       .ToList()
            //                                       .Select(r => new NameValue { Name = r.Name, Value = r.Id.ToString(), IsActive = r.IsActive, IsDisabled = r.IsDisabled})
            //                                       .ToList(),
            //          DefaultRoles =
            //                  SecurityLogic.GetDefaultRoles(RoleCategory.SupportAccount, CurrentState.GetCurrentAccountId())
            //                                           .ToList()
            //                                           .Select(r => new NameValue { Name = r.Name, Value = r.Id.ToString() })
            //                                           .ToList(),
            //          MembershipTypeId = (int)MembershipType.SupportAccount,
            //          MembershipParentId = id,
            //          NumberOfPendingMemberships = MembershipLogic.GetNumberOfPendingMembershipsBySupportAccountId(CurrentState.GetCurrentAccountId(), User.RoleSecurities),
            //          ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
            //          IsMfaRequired = isMfaRequired,
            //          IsUserImpersonated = userInfo != null && userInfo["isimpersonated"] != null && userInfo["isimpersonated"].Equals("true"),
            //          IsUserInternal = User != null && User.UserAccount.Email.ToLower().EndsWith("@paloaltonetworks.com")
            //      };

            var membershipDto = new MembershipListDto
            {
                MembershipTypeId = (int)MembershipType.SupportAccount
            };

            return PartialView("../Partial/_Memberships", membershipDto);
        }


        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _Memberships partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Support Account ID</param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountMemberships)]
        public ActionResult Users([DataSourceRequest] DataSourceRequest request, long id)
        {

            var dataParameters = request.ToDataParameters();
            List<SupportAccountDomainModel> uniqueDomains;

            var memberships = MembershipLogic.GetSupportAccountMembers(id, User.UserAccount.Id, dataParameters, out uniqueDomains);

            foreach (var udomain in uniqueDomains)
            {
                udomain.IsThridPartyIdpConfigured = SiteHelper.IsThirdPartyIdpConfigured(udomain.Domain);
            }

            foreach (var umembership in memberships)
            {
                if (!(umembership.Roles.Select(r => r.Name == RoleType.DomainAdministrator.ToDescription()).Any()))
                    umembership.IsThridPartyIdpConfigured = (from u in uniqueDomains where u.Domain == umembership.Domain select u.IsThridPartyIdpConfigured).FirstOrDefault();
            }

            var result = new DataSourceResult
            {
                Data = memberships,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            return Json(result);
        }

        [CspAuthorize(Permission.ViewSupportAccountMemberships)]
        public FileResult UsersCsv(long id, string searchField, string searchValue)
        {
            var supportAccount = _supportAccountLogic.GetSupportAccount(id, User.UserAccount.Id);
            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            if (supportAccount == null)
            {
                var ex = new UnauthorizedAccessException("User does not have permissions to download users csv.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            var csv = MembershipLogic.GetSupportAccountMembersCsv(id, User.UserAccount.Id, searchField, searchValue);  
            return File(csv, "text/comma-separated-values", "MembershipReport_" + dateStr + ".csv");
        }

        /// <summary>
        /// This method is initially used to load _WidfireUsers partial view when first time called
        /// </summary>
        /// <returns>Partail View</returns>
        [CspAuthorize(Permission.EditWildfireUsers)]
        public ActionResult WildfireUsers(long id = 0)
        {
            var supportAccountId = CurrentState.GetCurrentAccountId();
            var dataParameters = new DataParameters { PaginationCount = 10 };

            var wildfireUserAccountListDto = new WildfireUserAccountListDto
            {
                DataParameters = dataParameters,
                WildfireUserAccounts = _userAccountLogic.GetWildfireUsersBySupportAccountId(supportAccountId, dataParameters).ToList(),
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, supportAccountId)
            };

            return View(wildfireUserAccountListDto);
        }


        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _WidfireUsers partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, CspAuthorize(Permission.EditWildfireUsers)]
        public ActionResult WildfireUsers([DataSourceRequest] DataSourceRequest request)
        {
            var supportAccountId = CurrentState.GetCurrentAccountId();
            var dataParameters = request.ToDataParameters();
            var wildfireUserAccounts = _userAccountLogic.GetWildfireUsersBySupportAccountId(supportAccountId, dataParameters).ToList();

            var result = new DataSourceResult
            {
                Data = wildfireUserAccounts,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult AccountDevices(long id, string assettype = null, string devicecode = null)
        {

            var model = new SupportAccountTabsDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = SupportAccountTabs.Devices,
                PartialId = id,
                HasGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Any(),
                DefaultAssetType = assettype,
                DefaultDevicecode = devicecode,
                IsCsspAccount = _supportAccountLogic.IsCsspAccount(id)
            };

            return View("../SupportAccount/Details", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult Devices(long id, string assettype = null, string devicecode = null)
        {
            var hasBulkRegistrationAccess = User.HasPermission(Permission.ViewBulkRegistration);
            //var hasEaLicense = _siteLicenseLogic.HasEaLicense(id);


            var model = new DevicesDto
            {

                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),

                SelectedTab = GetSelectedDeviceTab(assettype),

                DefaultDeviceCode = devicecode,
                HasBulkRegistrationAccess = hasBulkRegistrationAccess, // (hasBulkRegistrationAccess && !hasEaLicense),
                HasEnterpriseAgreementAccess = User.HasPermission(Permission.ViewEnterpriseAgreements) //User.RoleSecurities.IsSupportAccountAdmin(id)  || User.UserAccount.Email.ToLower().EndsWith("@paloaltonetworks.com")
            };

            //	return PartialView("../Partial/_Devices", model);
            return View("../SupportAccount/Devices", model);
        }

        private GroupDestination GetSelectedDeviceTab(string assettype)
        {
            switch (assettype)
            {
                case AssetTypes.Prod:
                    return GroupDestination.DevicesProd;

                case AssetTypes.Spare:
                    return GroupDestination.DevicesSpare;

                case AssetTypes.Traps:
                    return GroupDestination.EPAuthCodes;

                case AssetTypes.Vm:
                    return GroupDestination.DevicesVmCodes;

                case "SiteLicenses":
                    return GroupDestination.SiteLicenses;

                case "SearchAll":
                    return GroupDestination.SearchAll;

                case "ClaimedAssets":
                    return GroupDestination.ClaimedAssets;

                case "PrepBulkRegistration":
                    return GroupDestination.PrepBulkRegistration;

                case "CustomizeBulkRegistration":
                    return GroupDestination.CustomizeBulkRegistration;

                case "BulkRegistrationReview":
                    return GroupDestination.BulkRegistrationReview;

                case AssetTypes.CnSeries:
                    return GroupDestination.DevicesCnCodes;

                default:
                    return GroupDestination.DevicesProd;
            }
        }

        [HttpGet, CspAuthorize(Permission.EditSupportAccount)]
        public ActionResult EditAccount(long id)
        {

            var model = new SupportAccountTabsDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = SupportAccountTabs.Account,
                PartialId = id,
                HasGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Any(),
                IsCsspAccount = _supportAccountLogic.IsCsspAccount(id)
            };

            return View("../SupportAccount/Details", model);
        }

        [HttpGet, AjaxOrChildActionOnly, CspAuthorize(Permission.EditSupportAccount)]
        public ActionResult Edit(long id)
        {

			var model = GetSupportAccountDto(id, User.UserAccount.Id);
			var email = User.UserAccount.Email;
			var domain = new MailAddress(email).Host;
			var thirdPartySsoConfigUrl = string.Format(ConfigurationManager.AppSettings["ThirdPartySsoConfigUrl"], CurrentState.GetSupportAccountId());
            model.ShowThirdPartyIdpSettings = SiteHelper.IsThirdPartyIdpConfigured(email);
            var supportAccount = SupportAccountLogic.GetSupportAccount(id, User.UserAccount.Id);			
            model.IsFedRampAccount = SupportAccountLogic.IsFedrampAccount(id);
            model.IsMSSPAccount = SupportAccountLogic.IsMSSPAccount(id);           
            var thirdPartyIdpConfiguration = SiteHelper.GetThirdPartyIdpConfiguration(domain);
            model.ThirdPartyIdpEntityId = (thirdPartyIdpConfiguration != null) ? thirdPartyIdpConfiguration.entityId : null;
            model.ThirdPartySsoConfigUrl = thirdPartySsoConfigUrl;     

            return PartialView("../Partial/_SupportAccount", model);
        }

        //[HttpGet, AjaxOnly, AscSuperUserAuthorize]
        //public async Task<ActionResult> ManageAscPreferences(long id)
        //{
        //    AscContactInfo ascContactInfo = null;
        //    AscAccountCaseNotification ascAccountCaseNotificationInfo = null;
        //    var model = new ManageAscPreferencesDto();

        //    try
        //    {
        //        var salesforceId = _supportAccountLogic.GetSalesforceIdBySupportAccountId(id);

        //        if (!string.IsNullOrEmpty(salesforceId))
        //        {
        //            ascContactInfo = await _salesforceRestApiLogic.GetAscContactInfo(salesforceId);
        //        }
        //        else
        //        {
        //            ascContactInfo = new AscContactInfo()
        //            {
        //                Id = salesforceId
        //            };
        //        }

        //        if (!string.IsNullOrEmpty(salesforceId))
        //        {
        //            ascAccountCaseNotificationInfo = await _salesforceRestApiLogic.GetAscAccountCaseNotificationInfo(salesforceId);
        //        }
        //        else
        //        {
        //            ascAccountCaseNotificationInfo = new AscAccountCaseNotification()
        //            {
        //                SfdcAccountId = salesforceId
        //            };
        //        }
        //    }
        //    catch (CspNotificationException ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //        model.Notifications.Add(new ResponseNotification("An error occurred while processing your request. Please contact customer support.", ResponseState.Error));
        //    }

        //    model.ContactInfo = ascContactInfo;
        //    model.CaseNotification = ascAccountCaseNotificationInfo;

        //    return PartialView("../Partial/_ManageAscPreferences", model);
        //}

        //[HttpPost, AjaxOnly, ValidateAntiForgeryToken, AscSuperUserAuthorize]
        //public async Task<ActionResult> ManageAscPreferences(ManageAscPreferencesDto model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (model.CaseNotification != null)
        //            {
        //                model.CaseNotification.IsCriticalCaseCreationNotification = model.CaseNotification.IsCaseCreationNotification;
        //            }

        //            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
        //            var salesforceId = _supportAccountLogic.GetSalesforceIdBySupportAccountId(supportAccountId);
        //            AscContactInfo ascContactInfo = null;
        //            AscAccountCaseNotification ascAccountCaseNotificationInfo = null;
        //            string ascCaseNotificationId = null;

        //            if (!string.IsNullOrEmpty(model.ContactInfo.Id))
        //            {
        //                ascContactInfo = await _salesforceRestApiLogic.GetAscContactInfo(model.ContactInfo.Id);
        //            }

        //            if (!model.ContactInfo.Equals(ascContactInfo))
        //            {
        //                salesforceId = await _salesforceRestApiLogic.UpdateAscContactInfo(model.ContactInfo);

        //                model.ContactInfo.Id = salesforceId;
        //            }

        //            if (!string.IsNullOrEmpty(model.CaseNotification.SfdcAccountId))
        //            {
        //                ascAccountCaseNotificationInfo = await _salesforceRestApiLogic.GetAscAccountCaseNotificationInfo(salesforceId);
        //            }

        //            if (!model.CaseNotification.Equals(ascAccountCaseNotificationInfo))
        //            {
        //                ascCaseNotificationId = await _salesforceRestApiLogic.AddUpdateAscAccountCaseNotificationInfo(model.CaseNotification);
        //            }

        //            if (!string.IsNullOrEmpty(ascCaseNotificationId) && string.IsNullOrEmpty(model.CaseNotification.EmailAddress))
        //            {
        //                model.CaseNotification = new AscAccountCaseNotification()
        //                {
        //                    SfdcAccountId = salesforceId
        //                };
        //            }
        //            else if (string.IsNullOrEmpty(model.CaseNotification.Id))
        //            {
        //                model.CaseNotification.Id = ascCaseNotificationId;
        //            }

        //            ModelState.Clear();

        //            model.Notifications.Add(new ResponseNotification("Your changes have been saved successfully", ResponseState.Success));
        //        }

        //    }
        //    catch (CspNotificationException ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //        model.Notifications.Add(new ResponseNotification("An error occurred while processing your request. Please contact customer support.", ResponseState.Error));
        //    }

        //    return PartialView("../Partial/_ManageAscPreferences", model);
        //}

        [HttpGet, CspAuthorize(Permission.EditSupportAccount)]
        public JsonResult GetSupportAccountUsers([DataSourceRequest] DataSourceRequest request)
        {
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            var searchTerm = string.Empty;

            if (request.Filters != null)
            {
                if (request.Filters.Count > 0)
                {
                    searchTerm = ((Kendo.Mvc.FilterDescriptor)((request.Filters[0]))).Value.ToString();
                }
            }
            var results = _userAccountLogic.GetSupportAccountUsersWithSfdcContactId(supportAccountId, searchTerm);

            var users = results.ToList();

            users.Insert(0, new NameValueDescription()
            {
                Name = Constants.DropDownContactSelect
            });
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

            return Json(users.ToDataSourceResult(sanitizedRequest), JsonRequestBehavior.AllowGet);
        }

        private SupportAccountDto GetSupportAccountDto(long supportAccountId, long userAccountId)
        {
            var supportAccount = SupportAccountLogic.GetSupportAccount(supportAccountId, userAccountId);
            var assignedRoles = supportAccount.DefaultRoles;
            var availableRoles = SecurityLogic.GetAvailableRoles(RoleCategory.SupportAccount, supportAccount.Id, User.UserAccount.Id).ToList();

            var roles = availableRoles.Select(role => new RoleDto { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();

            var registrationCodeStatus = _selfServiceRegistrationCodeService.GetCurrentRegistrationCodeStatus(supportAccountId);
            var registrationRequiresSuApproval = _supportAccountLogic.RegistrationRequiresSuApproval(supportAccountId);

            var currentData = _homeLogic.GetCurrentLoggedInData(supportAccountId, User.UserAccount.Id);
            bool isDomainAdmin = _securityLogic.HasRole(supportAccountId, User.UserAccount.Id, RoleNames.DomainAdministrator);
            return new SupportAccountDto
            {
                SupportAccount = supportAccount,
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, supportAccount.Id, ActionHelper.EntityAction.Edit),
                RoleAssignment = {
                    Roles = roles
                },
                AccountType = supportAccount.AccountTypeName,
                RegistrationCodeStatus = registrationCodeStatus,
                RegistrationRequiresSuApproval = registrationRequiresSuApproval,
                CurrentRoles = currentData.CurrentRoles.Distinct().ToList(),
                EnableMfa = supportAccount.IsMfaEnabled,
                ImpersonateSetting = (ImpersonateStatus)supportAccount.ImpersonateSetting,
                IsDomainAdministrator = isDomainAdmin
            };

        }

        //[HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccount)]
        //public ActionResult ConvertAccount(SupportAccountDto supportAccountDto)
        //{

        //    if (string.IsNullOrEmpty(supportAccountDto.AccountConversionDeviceCode) || string.IsNullOrEmpty(supportAccountDto.AccountConversionCustomerCode))
        //    {
        //        supportAccountDto = GetSupportAccountDto(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);
        //        return PartialView("../Partial/_ViewSupportAccount", supportAccountDto);
        //    }

        //    var result = SupportAccountLogic.ConvertEvalAccountToCust(supportAccountDto.SupportAccount.Id,
        //                                                                                                 supportAccountDto.AccountConversionDeviceCode, supportAccountDto.AccountConversionCustomerCode, User.UserAccount.Id);

        //    supportAccountDto = GetSupportAccountDto(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);

        //    if (!string.IsNullOrEmpty(result.Error))
        //    {
        //        supportAccountDto.Notifications.Add(new ResponseNotification(result.Error, ResponseState.Error));
        //    }

        //    if (result.IsActiveOtherAccount)
        //    {
        //        supportAccountDto.AccountType = "Eval";
        //        supportAccountDto.Notifications.Add(new ResponseNotification("An active account has already been created using this code.  You have been added to the active account and can now change to that account from the Home tab", ResponseState.Warning));
        //    }
        //    else
        //    {
        //        supportAccountDto.AccountType = "Customer";
        //        supportAccountDto.Notifications.Add(new ResponseNotification("Your account has been converted", ResponseState.Success));
        //    }

        //    return PartialView("../Partial/_ViewSupportAccount", supportAccountDto);
        //}


        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditSupportAccount)]
        public async Task<ActionResult> Edit(SupportAccountDto supportAccountDto, string registrationCodeAction)
        {
            var supportAccountId = supportAccountDto.SupportAccount.Id;

            if (!User.HasPermission(Permission.EditCompanyName))
            {
                ModelState.Remove("SupportAccount.Name");
                var supportAccount = SupportAccountLogic.GetSupportAccount(supportAccountId, User.UserAccount.Id);
                supportAccountDto.SupportAccount.Name = supportAccount.Name;
            }

            var supportAccountRoles = GetSupportAccountDto(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);

            if (supportAccountDto.ConvertAccount)
            {
                if (string.IsNullOrEmpty(supportAccountDto.AccountConversionDeviceCode) || string.IsNullOrEmpty(supportAccountDto.AccountConversionCustomerCode))
                {
                    //supportAccountDto = GetSupportAccountDto(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);
                    supportAccountDto = supportAccountRoles;
                    supportAccountDto.RegistrationCodeStatus = _selfServiceRegistrationCodeService.GetCurrentRegistrationCodeStatus(supportAccountDto.SupportAccount.Id);
                    supportAccountDto.RegistrationRequiresSuApproval = _supportAccountLogic.RegistrationRequiresSuApproval(supportAccountId);
                    return PartialView("../Partial/_SupportAccount", supportAccountDto);
                }
                ModelState.Clear();

                var result = SupportAccountLogic.ConvertEvalAccountToCust(supportAccountDto.SupportAccount.Id,
                                                                                                         supportAccountDto.AccountConversionDeviceCode, supportAccountDto.AccountConversionCustomerCode, User.UserAccount.Id);

                if (!string.IsNullOrEmpty(result.Error))
                {
                    supportAccountDto.Notifications.Add(new ResponseNotification(result.Error, ResponseState.Error));
                }

                if (result.IsActiveOtherAccount)
                {
                    supportAccountDto.AccountType = "Eval";
                    supportAccountDto.Notifications.Add(new ResponseNotification("An active account has already been created using this code.  You have been added to the active account and can now change to that account from the Home tab", ResponseState.Warning));
                }
                else
                {
                    supportAccountDto.AccountType = "Customer";
                    supportAccountDto.Notifications.Add(new ResponseNotification("Your account has been converted", ResponseState.Success));
                }

                supportAccountDto.RegistrationCodeStatus = _selfServiceRegistrationCodeService.GetCurrentRegistrationCodeStatus(supportAccountDto.SupportAccount.Id);
                supportAccountDto.RegistrationRequiresSuApproval = _supportAccountLogic.RegistrationRequiresSuApproval(supportAccountId);
                supportAccountDto.CurrentRoles = supportAccountRoles.CurrentRoles;

                supportAccountDto.SupportAccount.SalesforceId = supportAccountRoles.SupportAccount.SalesforceId;
                supportAccountDto.RoleAssignment.Roles = supportAccountRoles.RoleAssignment.Roles;
                supportAccountDto.ActionHelper = supportAccountRoles.ActionHelper;

                return PartialView("../Partial/_SupportAccount", supportAccountDto);
            }

            var successMessage = "Account successfully saved";
            switch (registrationCodeAction)
            {
                case "regenerate":
                    _selfServiceRegistrationCodeService.GenerateRegistrationCode(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);
                    successMessage = "Link successfully regenerated";
                    break;

                case "generate":
                    _selfServiceRegistrationCodeService.GenerateRegistrationCode(supportAccountDto.SupportAccount.Id, User.UserAccount.Id);
                    successMessage = "Link successfully generated";
                    break;

                case "disable":
                    _selfServiceRegistrationCodeService.DisableRegistrationCode(supportAccountDto.SupportAccount.Id);
                    successMessage = "Link successfully disabled";
                    break;
            }

            var roleIds = supportAccountDto.RoleAssignment.SelectedRoleIds.Split(new[] { ',' });
            var defaultRoles = new List<Role>();

            foreach (var id in roleIds)
            {
                long roleId;
                long.TryParse(id, out roleId);
                defaultRoles.Add(new Role { Id = roleId });
            }

            supportAccountDto.SupportAccount.DefaultRoles = defaultRoles;

            var model = new SupportAccountDto
            {
                SupportAccount = supportAccountDto.SupportAccount,
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, supportAccountId, ActionHelper.EntityAction.Edit),
                CurrentRoles = supportAccountRoles.CurrentRoles.Distinct().ToList()
            };

            if (!string.IsNullOrEmpty(supportAccountDto.RoleAssignment.SelectedRoleIds))
            {
                try
                {

                    ModelState.Remove("SupportAccount.Address.CountryId");
                    ModelState.Remove("SupportAccount.Address.Address1");
                    ModelState.Remove("SupportAccount.Address.Address2");
                    ModelState.Remove("SupportAccount.Address.City");
                    ModelState.Remove("SupportAccount.Address.Region");
                    ModelState.Remove("SupportAccount.Address.PostalCode");

                    if (ModelState.IsValid)
                    {
                        bool isDomainAdmin = _securityLogic.HasRole(supportAccountId, User.UserAccount.Id, RoleNames.DomainAdministrator);

                        var enforceMfaPolicy = supportAccountDto.EnableMfa && !supportAccountDto.SupportAccount.IsMfaEnabled;
                        var relaxMfaPolicy = !supportAccountDto.EnableMfa && supportAccountDto.SupportAccount.IsMfaEnabled;
                        model.SupportAccount.LastUpdateByUserAccountId = User.UserAccount.Id;
                        model.SupportAccount.IsMfaEnabled = supportAccountDto.EnableMfa;
                        model.EnableMfa = supportAccountDto.EnableMfa;
                        //update values only in case of DA
                        model.ImpersonateSetting = isDomainAdmin ? supportAccountDto.ImpersonateSetting : supportAccountRoles.ImpersonateSetting;
                        model.SupportAccount.ImpersonateSetting = isDomainAdmin ? (long)supportAccountDto.ImpersonateSetting : (long)supportAccountRoles.ImpersonateSetting;

                        model.SupportAccount = SupportAccountLogic.UpdateSupportAccount(model.SupportAccount, User.UserAccount.Id);
                        SupportAccountLogic.SetRegistrationRequiresSuApproval(model.SupportAccount.Id, supportAccountDto.RegistrationRequiresSuApproval);
                        model.IsDomainAdministrator = supportAccountRoles.IsDomainAdministrator;
                        //log "requires super user approval" flag changes to Elmah
                        string suApprovalLog = string.Format("Requires Super User Approval of support account {0} changed to {1} by user '{2}' on {3}", supportAccountId, supportAccountDto.RegistrationRequiresSuApproval, User.UserAccount.Email, DateTime.Now);
                        var suExceptionInfo = new Exception(suApprovalLog);
                        Elmah.ErrorSignal.FromCurrentContext().Raise(suExceptionInfo);



                        //               if (enforceMfaPolicy || relaxMfaPolicy)
                        //{
                        //                   var updatedByUser = string.Format("{0} {1}", User.UserAccount.FirstName, User.UserAccount.LastName);
                        //    //Enable or relax MFA for the all the members of the support account
                        //    _mfaLogic.EnableMfaForAccount(supportAccountId, User.UserAccount.Id, supportAccountDto.EnableMfa,
                        //                              supportAccountDto.SupportAccount.Name, updatedByUser);

                        //                   //log "enable 2 factor authentication" changes to Elmah
                        //    string mfaLog = string.Format("Enable 2 factor authentication of support account {0} changed to {1} by user '{2}' on {3}",supportAccountId,supportAccountDto.EnableMfa, User.UserAccount.Email, DateTime.Now);
                        //                   var mfaExceptionInfo = new Exception(mfaLog);
                        //                   Elmah.ErrorSignal.FromCurrentContext().Raise(mfaExceptionInfo);
                        //               }

                        ModelState.Clear();

                        model.Notifications.Add(new ResponseNotification(successMessage, ResponseState.Success));
                    }

                }
                catch (CspNotificationException ex)
                {
                    model.Notifications.Add(new ResponseNotification(ex.Message, ResponseState.Error));

                }
            }
            else
            {
                model.Notifications.Add(new ResponseNotification("At least one role is required.", ResponseState.Error));
            }

            var assignedRoles = model.SupportAccount.DefaultRoles;
            var availableRoles = SecurityLogic.GetAvailableRoles(RoleCategory.SupportAccount, supportAccountId, User.UserAccount.Id).ToList();

            var roles = availableRoles.Select(role => new RoleDto { IsSelected = assignedRoles.Any(r => r.Id == role.Id), Role = role }).ToList();
            model.RoleAssignment.Roles = roles;

            model.AccountType = model.SupportAccount.AccountTypeName;

            model.RegistrationCodeStatus = _selfServiceRegistrationCodeService.GetCurrentRegistrationCodeStatus(supportAccountDto.SupportAccount.Id);
            model.RegistrationRequiresSuApproval = _supportAccountLogic.RegistrationRequiresSuApproval(supportAccountId);

            // notify super users of the account if require super approval flag is disabled
            if (!model.RegistrationRequiresSuApproval && supportAccountRoles.RegistrationRequiresSuApproval)
                EmailHelper.SendRequireSuperUserApprovalNotification(supportAccountId, User.UserAccount.FirstName, User.UserAccount.LastName, User.UserAccount.Id);

            //notify domain admins if the impersonation setting is changed
            if (model.SupportAccount.ImpersonateSetting != (long)supportAccountRoles.ImpersonateSetting)
            {
                //CSP-10144: Commented the notifications publisher as its being handled on java side. 
                #region Publish Event to Pubsub
                //AppNotificationData impersonateSettingData = new AppNotificationData()
                //{
                //    AccountId = Convert.ToString(model.SupportAccount.Id),
                //    EntityId = Convert.ToString((long)model.SupportAccount.ImpersonateSetting),
                //    UserId = Convert.ToString(User.UserAccount.Id)
                //};

                //AppNotification impersonateSettingNotification = new AppNotification()
                //{
                //    AppData = impersonateSettingData,
                //    Category = NotificationCategory.ImpersonateSettingsChange.ToDescription(),
                //    DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription(), NotificationDeliveryChannel.Email.ToDescription() },
                //    Source = NotificationSource.Impersonate.ToDescription()
                //};

                //await EventPublisher.PublishEvent(impersonateSettingNotification);
                #endregion

                #region Publish Event to pubsub to Update existing impersonate requests

                string topic = ConfigurationManager.AppSettings.Get("Impersonate_Setting_TopicName");
                var additionalData = new AdditionalDetails()
                {
                    NewAccountSetting = (int)model.SupportAccount.ImpersonateSetting,
                    CurrentAccountSetting = (int)supportAccountRoles.ImpersonateSetting
                };

                AppNotificationData impersonateChangeSettingData = new AppNotificationData()
                {
                    AccountId = Convert.ToString(model.SupportAccount.Id),
                    EntityId = Convert.ToString((long)model.SupportAccount.ImpersonateSetting),
                    UserId = Convert.ToString(User.UserAccount.Id),
                    AdditionalData = additionalData
                };

                AppNotification impersonateSettingChangeNotification = new AppNotification()
                {
                    AppData = impersonateChangeSettingData,
                    Category = NotificationCategory.ImpersonateSettingsChange.ToDescription(),
                    Source = NotificationSource.Csp.ToDescription()
                };

                await eventPublisher.PublishCustomEvent(impersonateSettingChangeNotification, topic);
                #endregion
            }

            return PartialView("../Partial/_SupportAccount", model);
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditGroup)]
        public ActionResult UpdateGroupInline([DataSourceRequest]DataSourceRequest request, GroupListItem model)
        {

            var group = GroupLogic.GetGroup(model.GroupId, User.UserAccount.Id);

            group.Name = model.SupportAccountGroupName;
            group.Description = model.Description;
            group.LastUpdateByUserAccountId = User.UserAccount.Id;

            GroupLogic.UpdateGroupNameAndDescription(group, User.UserAccount.Id);

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

            return Json(new[] { model }.ToDataSourceResult(sanitizedRequest, ModelState));
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public JsonResult GetAccountType()
        {
            var accountTypeLists = SupportAccountLogic.GetAccountTypes(new DataParameters());

            return Json(accountTypeLists, JsonRequestBehavior.AllowGet);
        }

   //     [Authorize]
   //     public ActionResult Selector(long id)
   //     {
   //         return RedirectToAction("MyAccounts");

   //         /*
			//var actionHelper = new ActionHelper(ControllerFields.SupportAccountController);
			//ViewBag.ActionHelper = actionHelper;

			////var token = UserHelper.GetSsoToken();

			//var selectorAccountListItems = new List<SelectorAccountListItem>();

			////if (token != null) {

			////var email = (string)token["email"];

			//selectorAccountListItems = _supportAccountLogic.GetSupportAccountsAndGroupsByEmailId(HttpContext.User.Identity.Name);

			////if user has only 1 account, then directly send the user to that account.
			//if (selectorAccountListItems.Count != 1)
			//	return View(selectorAccountListItems);

			////if redirecting to another site, use return parameter r to get url and return else 
			//// identify if support account or group account and redirect appropriately

			//var email = HttpContext.User.Identity.Name;
			//var membershipId = selectorAccountListItems.Single().MembershipId;
			//_supportAccountLogic.AddUpdateSelectedSupportAccount(email, membershipId, _pingLogic.GetSupportAccountIdFromMembership(membershipId));

			//_userAccountLogic.SetSelectedMembershipAndSalesForce(User.UserAccount.Id, membershipId, _pingLogic.GetSupportAccountIdFromMembership(membershipId));

			//if (Request.QueryString["r"] != null) {
			//	return Redirect(Request.QueryString["r"]);
			//}

			//var accountId = selectorAccountListItems.First().Id;
			//return RedirectToAction("MyAccounts", ControllerFields.SupportAccountController, new { id = accountId });*/
   //     }

        //[HttpPost, ValidateAntiForgeryToken]
        //public ActionResult SelectorAccountSelected(SelectorAccountListItem selectedAccount, string urlParam)
        //{

        //    var email = HttpContext.User.Identity.Name;
        //    urlParam = Sanitizer.GetSafeHtmlFragment(urlParam);

        //    if (selectedAccount == null)
        //    {
        //        throw new ArgumentException("Must select an account controller");
        //    }

        //    CurrentState.ClearAll();

        //    _supportAccountLogic.AddUpdateSelectedSupportAccount(email, selectedAccount.MembershipId, _pingLogic.GetSupportAccountIdFromMembership(selectedAccount.MembershipId));

        //    _userAccountLogic.SetSelectedMembershipAndSalesForce(User.UserAccount.Id, selectedAccount.MembershipId, _pingLogic.GetSupportAccountIdFromMembership(selectedAccount.MembershipId));

        //    if (string.IsNullOrEmpty(urlParam) || urlParam == "null")
        //    {
        //        return RedirectToAction("MyAccounts");
        //    }

        //    return Json(urlParam);
        //}

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult LoadAccountPage(long id)
        {

            if (User.HasPermission(Permission.EditSupportAccount))
            {
                return RedirectToAction("EditAccount", "SupportAccount", new { id = id });
            }

            if (User.HasPermission(Permission.ViewSupportAccount))
            {
                return RedirectToAction("ViewAccount", "SupportAccount", new { id = id });
            }

            throw new InvalidEnumArgumentException("Either you can edit or view the account.  Those are the only choices");
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetAccountPage(long id)
        {

            var u = new UrlHelper(ControllerContext.RequestContext);

            if (User.HasPermission(Permission.EditSupportAccount))
            {
                return Json(u.Action("EditAccount", "SupportAccount", new { id = id }), JsonRequestBehavior.AllowGet);
            }

            if (User.HasPermission(Permission.ViewSupportAccount))
            {
                return Json(u.Action("ViewAccount", "SupportAccount", new { id = id }), JsonRequestBehavior.AllowGet);
            }

            throw new InvalidEnumArgumentException("Either you can edit or view the account.  Those are the only choices");
        }

        [HttpGet, AjaxOnly]
        public ActionResult GetAscContactInfo(long id)
        {
            AscContactInfo contactInfo = null;

            var salesforceId = _supportAccountLogic.GetSalesforceIdBySupportAccountId(id);

            if (!string.IsNullOrEmpty(salesforceId))
            {
                contactInfo = _salesforceService.GetAscContactInfo(salesforceId);
            }

            var contactInfoDto = new AscContactInfoDto
            {
                AscContactInfo = contactInfo
            };

            return PartialView("../Partial/_AscContactInfo", contactInfoDto);
        }

        public ActionResult CsspInfo(long id, string prelicenseId = null)
        {

            var model = new SupportAccountTabsDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = SupportAccountTabs.Cssp,
                PartialId = id,
                HasGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Any(),
                IsCsspAccount = _supportAccountLogic.IsCsspAccount(id),
                FirstQueryStringValue = prelicenseId
            };

            return View("../SupportAccount/Details", model);

        }

        public ActionResult Cssp(long id, string prelicenseId = null)
        {
            var model = new CsspDto();
            model.IsPrelicensingEnabled = _csspLogic.CanGeneratePrelicenseKeys(id);
            if (string.IsNullOrEmpty(prelicenseId))
            {
                model.SupportAccountId = id;
                model.SelectedTab = "Index";
            }
            else
            {
                model.SupportAccountId = id;
                model.SelectedTab = "PreLicenseDevices";
                model.FirstQueryStringValue = prelicenseId;
            }

            return PartialView("../Partial/_Cssp", model);
        }

        /*

	    public ActionResult Cssp(long id)
	    {
            var csspInfo = _csspLogic.GetCsspInfo(id);
            var shoppingCart = new List<CsspSku>();
            var csspDto = new CsspInfoDto()
            {
                Cssp = csspInfo,
                OrderItems = shoppingCart
            };

            return PartialView("../Partial/_CsspDetails", csspDto);
	    }

         * */

        [HttpGet, AjaxOnly]
        public JsonResult GetBanner()
        {
            return Json(_homeLogic.GetBannerMessage(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly]
        public ActionResult HasCaseManagementAccess(long supportAccountId)
        {
            var isSuccess = false;
            isSuccess = User.HasPermission(Permission.ViewCaseManagement);

            return Json(new JsonResult()
            {
                Data = new
                {
                    Success = isSuccess
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAvailableAccounts(string keyword)
        {
            long supportAccountId = 0;
            long.TryParse(keyword, out supportAccountId);
            var searchField = (supportAccountId > 0) ? "SupportAccountId" : "IdOrName";
            var availableAccountsList = new List<NameValue>();

            var dataParameters = new DataParameters { PaginationCount = 300, SortField = "CompanyName", SortDirection = SortDirection.Ascending };
            if (!string.IsNullOrEmpty(keyword))
            {
                dataParameters.SearchField = searchField;
                dataParameters.SearchValue = keyword;
            }
            var accounts = _securityLogic.GetAvailableAccounts(User.UserAccount.Id);
            var defaultSupportAccountId = _securityLogic.GetDefaultSupportAccount(User.UserAccount.Id);
            var supportAccount = _supportAccountLogic.GetSupportAccount(defaultSupportAccountId ?? 0, User.UserAccount.Id);

            if (defaultSupportAccountId != null)
            {
                availableAccountsList.Add(new NameValue { Name = string.Format("{0} (Default)", supportAccount.Name), Value = supportAccount.Id.ToString(), ValueLong = supportAccount.Id });
                accounts.RemoveAll(a => a.SupportAccountId == defaultSupportAccountId.Value);
            }

            var availableAccounts = accounts.Select(x => new NameValue() { Name = x.CompanyName, Value = x.SupportAccountId.ToString(), ValueLong = x.SupportAccountId }).ToList();
            availableAccountsList.AddRange(availableAccounts);

            return Json(availableAccountsList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult Spares(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.Spare),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }


        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult EP(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.Traps),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult VM(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.Vm),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult CnSeries(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.CnSeries),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult Sitelicenses(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.SiteLicenses),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult CloudServices(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GroupDestination.SaaService,
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult AssetHistory(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GroupDestination.AssetHistory,
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult BulkRegistration(long id)
        {
            var hasBulkRegistrationAccess = User.HasPermission(Permission.ViewBulkRegistration);
            //var hasEaLicense = _siteLicenseLogic.HasEaLicense(id);


            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),

                DefaultDeviceCode = null,
                SelectedTab = GroupDestination.ClaimedAssets,
                HasBulkRegistrationAccess = hasBulkRegistrationAccess, // (hasBulkRegistrationAccess && !hasEaLicense),
               // HasEnterpriseAgreementAccess = User.RoleSecurities.IsSupportAccountAdmin(id) || User.UserAccount.Email.ToLower().EndsWith("@paloaltonetworks.com")
            };

            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult EA(long id)
        {
           // var hasBulkRegistrationAccess = User.HasPermission(Permission.ViewBulkRegistration);
           // var hasEaLicense = _siteLicenseLogic.HasEaLicense(id);


            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GroupDestination.EnterpriseAgreements,
                DefaultDeviceCode = null,
               // HasBulkRegistrationAccess = (hasBulkRegistrationAccess && !hasEaLicense),
                HasEnterpriseAgreementAccess = User.HasPermission(Permission.ViewEnterpriseAgreements) // User.RoleSecurities.IsSupportAccountAdmin(id) // || User.UserAccount.Email.ToLower().EndsWith("@paloaltonetworks.com")
            };

            return View("../SupportAccount/Devices", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult SearchAll(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GetSelectedDeviceTab(AssetTypes.SearchAll),
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }

        [HttpGet]
        public ActionResult Preferences(long? id)
        {
            if (!id.HasValue)
            {
                var defaultSupportAccountId = _homeLogic.GetOrCreateDefaultSupportAccountId(User.UserAccount.Id);
                if (defaultSupportAccountId.HasValue)
                {
                    return RedirectToAction("Preferences", new { id = defaultSupportAccountId });
                }
            }
            return View("../SupportAccount/Preferences");
        }

        [HttpGet]
        public ActionResult EditProfile(long? id)
        {
            return View("../SupportAccount/AccountInfo");
        }

        [HttpGet]
        public ActionResult EnableGoogleAuthenticator(long id)
        {
            return View("../SupportAccount/EnableGoogleAuthenticator");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult LineCards(long id)
        {
            var model = new DevicesDto
            {
                ActionHelper = new ActionHelper(ControllerFields.SupportAccountController, id),
                SelectedTab = GroupDestination.LineCards,
                DefaultDeviceCode = null,
                HasBulkRegistrationAccess = false,
                HasEnterpriseAgreementAccess = false
            };
            return View("../SupportAccount/Devices", model);
        }




        [CspAuthorize(Permission.BulkDeviceTransfer)]
        public ActionResult BulkDeviceTransfer(long id)
        {
            return View("../Device/BulkDeviceTransfer");
        }


        [HttpGet]
        [CspAuthorize(Permission.ViewBPATool)]
        public ActionResult RunBPA(long id)
        {
            return View("../SupportAccount/BestPracticeAssessment");
        }

        //[HttpGet]
        //[CspAuthorize(Permission.ViewBPAHistory)]
        //public ActionResult BPAHistory(long? id)
        //{
        //    return View("../SupportAccount/BestPracticeAssessment");
        //}

        [HttpGet, CspAuthorize(Permission.ManageShipments)]
        public ActionResult ManageShipments(long id)
        {
            return View("../PartialShipment/PartialShipmentOrder");
        }

        [HttpGet]
        public ActionResult ManageShipmentDetail(long masterOrderNumber, long userAccountId, long supportAccountId)
        {
            return View("../PartialShipment/PartialShipmentOrderDetail");
        }

        [HttpGet, CspAuthorize(Permission.ViewNetsec)]
        public ActionResult CloudSecurity(long id)
        {
            return View("../CloudSecurity/CloudSecurity");
        }

        [HttpGet, CspAuthorize(Permission.ViewNetsec)]
        public ActionResult NetworkSecurity(long id)
        {
            return View("../NetworkSecurity/NetworkSecurity");
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult PendingTransfers()
        {
            return View("../PendingTransfers/PendingTransfers");
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult LicensingApi(long id)
        {
            return View("../License/LicensingApi");
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult RegisterDevice(long id, long groupId = 0, string source = null)
        {
            // Comments By LK On 08/27
            // Remove CurrentState dependencies
            /*if (groupId > 0)
            {
                CurrentState.SetGroup(new Group()
                {
                    Id = groupId
                });
            }*/
            return View("../Device/DeviceRegistration");
        }


        [HttpGet, CspAuthorize(Permission.ViewSoftwareUpdates)]
        public ActionResult SoftwareUpdates(long id)
        {
            return View("../Updates/SoftwareUpdates");
        }

        [HttpGet, CspAuthorize(Permission.ViewDynamicUpdates)]
        public ActionResult DynamicUpdates(long id)
        {
            return View("../Updates/DynamicUpdates");
        }


        [HttpGet]
        public ActionResult Day1Config(long? id)
        {
            return View("../Tools/Day1Config");
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult DeviceCertification(long id)
        {
            return View("../Otp/DeviceCertification");
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult DeviceCertificationOtpSummary(long id)
        {
            return View("../Otp/DeviceCertificationOtpSummary");
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult DeviceCertificationPinSummary(long id)
        {
            return View("../Otp/DeviceCertificationPinSummary");
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult GenerateOneTimePassword(long id)
        {
            return View("../Otp/OneTimePassword");
        }

        [CspAuthorize(Permission.EditDevice)]
        public ActionResult GenerateRegistrationPin(long id)
        {
            return View("../Otp/DeviceRegistrationPin");
        }

        [HttpGet, CspAuthorize(Permission.CreateConsentRequest)]
        public ActionResult ConsentRequest(string consentId = null)
        {
            return View("../SupportAccount/ConsentRequest");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ZTPService(long id)
        {
            return View("../Ztp/ZeroTouchProvision");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult XSOARProducts(long id)
        {
            return View("../XSOAR/XSOARProducts");
        }

        public ActionResult FirewallFlex()
        {
            // Comments By LK On 08.12.2021: No need to pass the SupportAccountId since the activate link may belong to a different support a/c other than default
            return View("../Flex/FirewallFlex");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult LicenseManagement(long id)
        {
            return View("../LicenseManagement/Index");
        }

        [CspAuthorize(Permission.ViewFirewallFlexTokens)]
        public ActionResult FirewallFlexDashboard(long? id)
        {
            if (!id.HasValue)
            {
                var defaultSupportAccountId = _homeLogic.GetOrCreateDefaultSupportAccountId(User.UserAccount.Id);

                if (defaultSupportAccountId.HasValue)
                {
                    return RedirectToAction("FirewallFlexDashboard", new { id = defaultSupportAccountId });
                }
                else
                {
                    return RedirectToAction("UnAuthorizedUser", "Error");
                }
            }
            ViewBag.IsCreditAdmin = User.HasPermission(Permission.ManageFirewallFlexTokens);
            return View("../Flex/FirewallFlexDashboard");
        }

        [CspAuthorize(Permission.ViewFirewallFlexTokens)]
        public ActionResult FirewallFlexDevices(long id)
        {
            return View("../Flex/FirewallFlexDevices");
        }

        [CspAuthorize(Permission.ViewTrainingCredits)]
        public ActionResult TrainingCredits(long id)
        {
            return View("../Training/TrainingCredits");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public FileStreamResult DownloadLicense(string serialNumber, string status)
        {
            var fileName = ConfigurationManager.AppSettings["XSOARFileName"] + "_" + serialNumber + ConfigurationManager.AppSettings["XSOARExtension"];
            var licenseData = _xsoarLogic.GetXSOARLicenseData(serialNumber, status);

            licenseData = licenseData.Replace("\n", Environment.NewLine);

            var byteArray = Encoding.ASCII.GetBytes(licenseData);
            var stream = new MemoryStream(byteArray);
            return File(stream, "text/plain", String.Format(fileName));
        }

        [HttpGet]
        public ActionResult ImpersonateRequest(string requestId = null)
        {
            //allow impersonate request page only for impersonate role 
            if (string.IsNullOrEmpty(requestId))
            {
                if (User.UserAccount.Email.Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION))
                {
                    bool hasImpersonateUser = _securityLogic.HasRole(ApiConstants.PANW_SUPPORT_ACCOUNT_ID, User.UserAccount.Id, RoleNames.ImpersonateUser);

                    if (hasImpersonateUser)
                        return View("../SupportAccount/ImpersonateRequest");

                    return RedirectToAction("UnAuthorized", "Error");
                }

                return RedirectToAction("UnAuthorized", "Error");

            }
            return View("../SupportAccount/ImpersonateRequest");
        }

        public ActionResult ExtendedExpertise(long id)
        {
            return View("../ExtendedExpertise/Index");
        }

        [HttpGet, CspAuthorize(Permission.ActivateProducts)]
        public ActionResult ProductActivation(long id)
        {
            return View("../SupportAccount/ProductActivation");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Ions(long id)
        {
            return View("../Ions/Index");
        }

        public ActionResult ConsumerProduct(long id)
        {
            return View("../ConsumerProduct/OkyoGarde");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Wildfire(long id)
        {
            return View("../Wildfire/Index");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Onboarding(long id)
        {
            return View("../CyberArk/Index");
        }

        private void setImpersonateData(string email, string sessionId)
        {
            string encrypted= SecurityHelper.ConvertStringToHex(SecurityHelper.Encrypt(email));
            HttpCookie impersonateCookie = new HttpCookie(ApiConstants.Impersonation_Identifier);
            impersonateCookie.Value = encrypted;
            impersonateCookie.Expires = DateTime.Now.AddHours(2);
            impersonateCookie.Domain = ApiConstants.PANW_EMPLOYEE_DOMAIN;
            Response.Cookies.Add(impersonateCookie);
            
            HttpCookie sessionCookie = new HttpCookie(ApiConstants.Impersonate_Session_Identifier);
            sessionCookie.Value = sessionId;
            sessionCookie.Expires = DateTime.Now.AddHours(2);
           // sessionCookie.Domain = ApiConstants.PANW_EMPLOYEE_DOMAIN;
            Response.Cookies.Add(sessionCookie);
        }

        private string getImpersonateData()
        {
            HttpCookie httpCookie = Request.Cookies[ApiConstants.Impersonation_Identifier];
            if (httpCookie == null)
            {
                return string.Empty;
            }
            string encryptedEmail = httpCookie.Value;
            string decrypted = string.IsNullOrEmpty(encryptedEmail)? string.Empty: SecurityHelper.ConvertHexToString(encryptedEmail);
            string email = string.IsNullOrEmpty(decrypted) ? string.Empty : SecurityHelper.Decrypt(decrypted);
            return email;
        }

        [HttpGet, CspAuthorize(Permission.ManageOauthAccess)]
        public ActionResult OAuthManagement(long id)
        {
            return View("../SupportAccount/OAuthManagement");
        }

        [HttpGet, CspAuthorize(Permission.ViewNetsec)]
        public ActionResult Assets(long id)
        {
            return View("../SupportAccount/Assets");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult NotifySuperUser(long id)
        {
            return View("../NotifySuperUser/NotifySuperUser");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public JsonResult KeepSessionAlive()
        {
            var data = "Success";
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private void ClearAll()
        {
            CurrentState.ClearAll();
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
        }

        private void UpdateLoginUserHistory()
        {
            try
            {
                HttpCookie historyCookie = Request.Cookies[ApiConstants.Login_User_Session_Identifier];
                if (historyCookie != null)
                {
                    string sessionId = historyCookie.Value;
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        // Update login history
                        var loginUserHistory = new LoginHistoryModel()
                        {
                            SessionId = sessionId,
                            UserAccountId = User.UserAccount.Id,
                            SupportAccountId = CurrentState.GetSupportAccountId()
                        };

                        _loginHistoryLogic.UpdateLoginUserHistory(loginUserHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult OutgoingTransfers()
        {
            return View("../OutgoingTransfers/OutgoingTransfers");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetHelp(long? supportAccountId)
        {
            //if (!CurrentState.GetIsAiEligible())
            //{
            //    return RedirectToAction("MyAccount", new
            //    {
            //        id = supportAccountId
            //    });
            //}
            return View();
        }  

        [HttpGet, CspAuthorize(Permission.EditSupportAccount)]
        public ActionResult PurchaseAdmin(long? supportAccountId)
        {
            return View("../SupportAccount/PurchaseAdmin");
        }    
    }
}
