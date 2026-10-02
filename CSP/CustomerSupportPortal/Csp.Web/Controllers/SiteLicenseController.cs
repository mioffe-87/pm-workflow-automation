using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Csp.Api.Model.Ela2;

namespace Csp.Web.Controllers
{
    [System.Web.Mvc.Authorize]
    public class SiteLicenseController : BaseController
    {
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly IVirtualMachineLogic _virtualMachineLogic;
        private readonly ITrapsELALogic _trapsELALogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IMembershipLogic _membershipLogic;
        private readonly IAscDeviceRegistrationService _ascService;

        public SiteLicenseController(ISiteLicenseLogic siteLicenseLogic, IEulaLogic eulaLogic, IVirtualMachineLogic virtualMachineLogic, ITrapsELALogic trapsELALogic,
           IUserAccountLogic userAccountLogic, IMembershipLogic membershipLogic, IAscDeviceRegistrationService ascService)
        {
            _siteLicenseLogic = siteLicenseLogic;
            _eulaLogic = eulaLogic;
            _virtualMachineLogic = virtualMachineLogic;
            _trapsELALogic = trapsELALogic;
            _userAccountLogic = userAccountLogic;
            _membershipLogic = membershipLogic;
            _ascService = ascService;
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult View(int id)
        {
            var dataParameters = new DataParameters
            {
                PaginationCount = 10,
            };

            var model = new SiteLicenseDto
            {
                ActionHelper = new ActionHelper("SiteLicense", id),
                HasEditAutoFocusApiKeyPerm = CurrentState.HasLoggedInPermission(Permission.EditAutoFocusApiKey),
                SiteLicenses = _siteLicenseLogic.GetSiteLicenses(id, User.UserAccount.Id, dataParameters).ToList(),
                DataParameters = dataParameters,
                SupportAccountId = id,
            };
            return PartialView("../Partial/_SiteLicenses", model);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountDevices)]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult GetSiteLicenses(long id, [DataSourceRequest] DataSourceRequest request)
        {
            var dataParameters = request.ToDataParameters();

            var siteLicenses = _siteLicenseLogic.GetSiteLicenses(id, User.UserAccount.Id, dataParameters);

            var result = new DataSourceResult
            {
                Data = siteLicenses,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount),
            };

            return Json(result);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditAutoFocusApiKey)]
        public ActionResult SiteLicenseAction(SiteLicenseDto model)
        {
            switch (model.SiteLicenseAction)
            {
                case SiteLicenseDto.SiteLicenseActions.Regenerate:
                    var regenerateResult = _siteLicenseLogic.RegenerateAutofocusApiKey(model.SupportAccountId,
                        User.UserAccount.Id, model.SiteLicenseActionLicenseId.Value);

                    if (regenerateResult.IsSuccess)
                    {
                        //model.Notifications.Add(new ResponseNotification("AutoFocus Key Generated", ResponseState.Success));
                        model.DisplayApiKey = regenerateResult.ApiKey;
                    }
                    else
                    {
                        model.Notifications.Add(new ResponseNotification("AutoFocus Key could not be generated", ResponseState.Error));
                    }
                    break;

                case SiteLicenseDto.SiteLicenseActions.Enable:
                    var enableResult = _siteLicenseLogic.EnableOrGenerateAutofocusApiKey(model.SupportAccountId,
                        User.UserAccount.Id, model.SiteLicenseActionLicenseId.Value);

                    if (enableResult.IsSuccess)
                    {
                        //model.Notifications.Add(new ResponseNotification("AutoFocus Key Generated", ResponseState.Success));
                        model.DisplayApiKey = enableResult.ApiKey;
                    }
                    else
                    {
                        model.Notifications.Add(new ResponseNotification("AutoFocus Key could not be generated", ResponseState.Error));
                    }
                    break;

                case SiteLicenseDto.SiteLicenseActions.Disable:
                    var disableResult = _siteLicenseLogic.DisableAutofocusApiKey(model.SupportAccountId,
                        User.UserAccount.Id, model.SiteLicenseActionLicenseId.Value);
                    if (disableResult.IsSuccess)
                    {
                        model.Notifications.Add(new ResponseNotification("AutoFocus Key Disabled", ResponseState.Success));
                    }
                    else
                    {
                        model.Notifications.Add(new ResponseNotification("AutoFocus Key could not be disabled", ResponseState.Error));
                    }
                    break;

                case SiteLicenseDto.SiteLicenseActions.GetKey:
                    var getKeyResult = _siteLicenseLogic.GetAutofocusApiKey(model.SupportAccountId,
                        User.UserAccount.Id, model.SiteLicenseActionLicenseId.Value);

                    if (getKeyResult.IsSuccess)
                    {
                        //model.Notifications.Add(new ResponseNotification("AutoFocus Key Generated", ResponseState.Success));
                        model.DisplayApiKey = getKeyResult.ApiKey;
                    }
                    else
                    {
                        model.Notifications.Add(new ResponseNotification("AutoFocus Key could not be retrieved", ResponseState.Error));
                    }
                    break;
            }

            var dataParameters = new DataParameters
            {
                PaginationCount = 10,
            };
            model.HasEditAutoFocusApiKeyPerm = CurrentState.HasLoggedInPermission(Permission.EditAutoFocusApiKey);
            model.SiteLicenses = _siteLicenseLogic.GetSiteLicenses(model.SupportAccountId, User.UserAccount.Id, dataParameters).ToList();
            model.DataParameters = dataParameters;
            model.ActionHelper = new ActionHelper("SiteLicense", model.SupportAccountId);

            return PartialView("../Partial/_SiteLicenses", model);
        }

        [System.Web.Mvc.HttpGet]
        // [OutputCache(NoStore = true, Duration = 0)]
        public async Task<ActionResult> ShowEnterpriseAgreements(long id)
        {
            var ela2LicenseEligibility = _siteLicenseLogic.GetEla2ButtonLinks(id, User.UserAccount.Id);
            var model = new EnterpriseAgreementsDto { SupportAccountId = id, UserEmail = User?.UserAccount?.Email };
            model.ActionHelper = new ActionHelper("SiteLicense", id);
            model.SelectedTab = "EnterpriseAgreements";
            model.EnterpriseAgreements = (await _siteLicenseLogic.GetEnterpriseAgreements(id, User.HasPermission(Permission.ManageVmElaTokens))).ToList();
            model.Ela2ButtonLinkAccess = ela2LicenseEligibility;
            bool hasCovertRoles = _siteLicenseLogic.HasConvertURL4ToADVURLRoles(id, User.UserAccount.Id);
            bool hasUrl4License = _siteLicenseLogic.HasUrl4License(id);
            model.HasEla2ConvertADVURL2Roles = hasCovertRoles;
            model.HasUrl4SiteLicense = hasUrl4License;
            model.ConvertButtonEnabled = "";
            ViewBag.ConvertButtonText = "Convert to ADVURL";

            if (hasUrl4License)
            {
                long requestId = _siteLicenseLogic.GetELAConversionRequestId(id);

                try
                {
                    //Call GetConversionStatus API only if Initialized state already found in the DB
                    if (requestId > 0)
                    {
                        ElaConversionStatusDataResponse returnData = _siteLicenseLogic.GetConversionStatus(requestId);
                        if (returnData.Data != null)
                        {
                            //If the conversion status is in progress
                            if (returnData.Data.Status == EElaConversionStatus.InProgress.ToString())
                            {
                                //check if the logged-in user is the same as the user who requested the conversion
                                if (returnData.Data.UserAccountId == User.UserAccount.Id)
                                {
                                    ViewBag.ConvertButtonText = "Conversion started";
                                    model.ConvertStatus = returnData.Data.Status;
                                    model.ConvertButtonEnabled = "disabled";
                                    model.CheckForConversionStatus = true;
                                }
                                else
                                {
                                    //Hide Url4 Conversion button because the user is not the same.
                                    model.HasUrl4SiteLicense = false;
                                    model.CheckForConversionStatus = false;
                                }
                            }
                            else
                            {
                                //At this stage, hide Url4 Conversion button if the status not equal Inprogress.
                                model.HasUrl4SiteLicense = false;
                                model.CheckForConversionStatus = false;
                            }
                        }
                    }
                    else
                    {
                        //If Initialized state not found in the DB show the button because Conversion never attempted before
                        model.HasEla2ConvertADVURL2Roles = hasCovertRoles;
                    }
                }
                catch (Exception ex)
                {
                    //Hide Url4 Conversion button
                    model.HasUrl4SiteLicense = false;
                    model.CheckForConversionStatus = false;
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return PartialView("../Partial/_ShowEnterpriseAgreements", model);
        }

        public JsonResult GetConversionStatus()
        {
            JsonResult result = new JsonResult();
            try
            {
                long requestId = _siteLicenseLogic.GetELAConversionRequestId(CurrentState.GetCurrentAccountId());
                result = Json(_siteLicenseLogic.GetConversionStatus(requestId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException("Error occurred while trying to get Ela Url4 conversion status. Please contact customer support.");
            }

            return result;
        }

        [System.Web.Mvc.HttpPost, System.Web.Mvc.Authorize]
        public string ConvertUrl4ToAdvanceUrl()
        {
            long cspId = CurrentState.GetCurrentAccountId();
            string error = string.Empty;

            // Create & initialize request object for CreateConversionStatus
            ElaConversionStatus request = new ElaConversionStatus();
            request.UserAccountId = User.UserAccount.Id;
            request.SupportAccountId = cspId;
            request.ConversionType = ElaConversionStatusBase.URL4TOADVURL;
            request.StatusValue = EElaConversionStatus.Initialized.ToString();
            ElaConversionStatusDataResponse returnData = new ElaConversionStatusDataResponse();

            try
            {
                long requestId = _siteLicenseLogic.GetELAConversionRequestId(cspId);

                //Call CreateConversionStatus only if we don't have an existing Initialized state created in the DB.
                if (requestId == 0)
                    returnData = _siteLicenseLogic.CreateConversionStatus(request);

                //If Initialized state exist in the DB or created successfully in the DB then call License urlconversion  API
                if (returnData.Data != null || requestId > 0)
                {
                    ElaLicenseConversionStatus apiRequest = new ElaLicenseConversionStatus();
                    apiRequest.RequestId = returnData.Data != null? returnData.Data.RequestId: requestId;
                    apiRequest.SupportAccountId = cspId;
                    apiRequest.UserAccountId = User.UserAccount.Id;
                    apiRequest.ConversionType = ElaConversionStatusBase.URL4TOADVURL;

                    ElaConversionStatusLicenseDataResponse[] elaConversionStatusLicenseDataResponse = 
                        _siteLicenseLogic.LicenseUrl4ConversionAPI(apiRequest);

                    if (elaConversionStatusLicenseDataResponse != null && elaConversionStatusLicenseDataResponse[0] != null)
                    {
                        //If License conversion API returned false then log the error details.
                        if (!elaConversionStatusLicenseDataResponse[0].Success)
                        {
                            Exception ex = new Exception("License URL4 Conversion API returned this error: " +
                                                         elaConversionStatusLicenseDataResponse[0].HttpStatus +
                                                         ". " + elaConversionStatusLicenseDataResponse[0]
                                                             .errorDetails.Message);
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                            error = ex.Message;
                        }
                    }
                    else
                    {
                        Exception ex = new Exception("License API ELA Conversion(urlconversion) returned null or empty data");
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        error = "License API ELA Conversion returned null or empty data. Please contact support.";
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new HttpException(
                    "An error occurred while trying to call convert Ela Url4 API. Please contact customer support.");
            }

            return error;
        }

        [System.Web.Mvc.HttpGet, System.Web.Mvc.Authorize]
        public ActionResult ActivateElaAuthCode()
        {
            var model = new ActivateVmAuthCodeDto();

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateVmAuthCode;

            return PartialView("../Partial/_ElaAuthCodeActivation", model);
        }

        [System.Web.Mvc.HttpPost, System.Web.Mvc.Authorize]
        public async Task<ActionResult> ActivateElaAuthCode(ActivateVmAuthCodeDto model)
        {
            if (model != null && ModelState.IsValid)
            {
                ElaAuthCodeResult result = new ElaAuthCodeResult();
                var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                string authCode = model.AuthorizationCode;
                string partNumber = string.Empty;
                try
                {
                    var authCodeDetails = _virtualMachineLogic.GetAuthCodeDetails(EVmType.Vm, authCode);
                    if (authCodeDetails != null)
                        partNumber = authCodeDetails.PartNumber;
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    result.ErrorMessage = "An error occurred while getting auth code's details.";
                }

                if (!string.IsNullOrEmpty(partNumber))
                {
                    //VM-ELA floating SKUs pattern
                    var regVmElaExp = new Regex("^PAN-VM-ELA-BND2-(PLAT|PREM|USG|BKLN|BKLNUSG)-(1|3|5)YR");
                    var regExpTrapsEla = new Regex("^PAN-TRAPS-ELA-.+YR");

                    if (regVmElaExp.IsMatch(partNumber))
                    {
                        var isAsc = _ascService.IsAsc(supportAccountId);
                        result = isAsc
                            ? new ElaAuthCodeResult(@"This auth code cannot be activated on an ASC account. 
                                                      Please try activating the authcode on a customer account.")
                            : await ActivateVmElaAuthCode(model.AuthorizationCode);
                    }
                    else if (regExpTrapsEla.IsMatch(partNumber))
                    {
                        result = ActivateTrapsElaAuthCode(model.AuthorizationCode);
                    }
                    else
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(new Exception(string.Format("ActivateElaAuthCode-AuthCode partnumber: {0} is not supported", partNumber)));
                        result = new ElaAuthCodeResult("This kind of Auth-Code is not supported.");
                    }
                }
                else
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                        .Raise(new Exception(string.Format("ActivateElaAuthCode-AuthCode partnumber is empty for {0}", authCode)));
                    result = new ElaAuthCodeResult("AuthCode is not found.");
                }

                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    #region Store Eula Agreements

                    try
                    {
                        var eulaContextId = EulaContext.ActivateVmAuthCode;
                        var eulaId = model.Eula.SelectedEulaId;
                        var signedBy = User.UserAccount.Email;
                        var licenseId = LicenseLogic.GetLicenseByAuthcode(model.AuthorizationCode).Id;

                        var ea = new EulaAgreements()
                        {
                            LicenseId = licenseId,
                            SupportAccountId = supportAccountId,
                            EulaContextId = eulaContextId,
                            EulaId = eulaId,
                            SignedBy = signedBy,
                            Notes = string.Format("AuthCode entered is {0}", authCode)
                        };

                        _eulaLogic.EndUserAgreements(ea);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(new Exception(
                                string.Format(
                                    "Could not add EULA Agreements while accepting while activating VM Auth Code - {0}. Eula Id Selected is - {1}",
                                    model.AuthorizationCode, model.Eula.DefaultEula.Id)));
                    }

                    #endregion

                    model.AuthorizationCode = String.Empty;
                    model.Notifications.Add(new ResponseNotification("Successfully activated the Auth-Code", ResponseState.Success));
                }
                else
                {
                    model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
                    Elmah.ErrorSignal.FromCurrentContext()
                        .Raise(new Exception("Error from LicenseIt: " + result.ErrorMessage));
                }
            }

            #region Bind Eula Model

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateVmAuthCode;

            #endregion

            return PartialView("../Partial/_ElaAuthCodeActivation", model);
        }

        private ElaAuthCodeResult ActivateTrapsElaAuthCode(string authCode)
        {
            var result = new ElaAuthCodeResult();
            try
            {
                //result = _trapsELALogic.ActivateTrapsELAAuthCode(authCode, CurrentState.GetCurrentAccountId());
                //var licenseResult = LicenseLogic.LicenseIt("", "", "", authCode);
                LicenseLogic.ActivateTrapsELA(authCode, CurrentState.GetCurrentAccountId().ToString(), User.UserAccount.Email);
            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result.ErrorMessage = "An error occurred while adding the auth code";
            }

            return result;
        }

        private async Task<ElaAuthCodeResult> ActivateVmElaAuthCode(string authCode)
        {
            var result = new ElaAuthCodeResult();

            try
            {
                var supportAccountId = CurrentState.GetSupportAccountId();
                if (supportAccountId == null)
                    throw new Exception("Error to retrieve support account.");
                if (string.IsNullOrEmpty(authCode))
                    throw new Exception("Empty Auth-Code.");

                var result1 = await _virtualMachineLogic.ActivateVmElaAuthCode(EVmType.Vm, authCode, supportAccountId.Value, User.UserAccount.Id);
                result.ErrorMessage = result1.ErrorMessage;

                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    //if the current user doesn't have ELA token management permission, add it.
                    if (_membershipLogic.HasMembershipforSupportAccountByEmail(User.UserAccount.Email, supportAccountId.Value)
                        && !User.HasPermission(Permission.ManageVmElaTokens))
                    {
                        _userAccountLogic.GrantPermissionToUser(supportAccountId.Value, User.UserAccount.Id, Permission.ManageVmElaTokens);
                    }

                    var regVmResult = _virtualMachineLogic.GenerateVmElaModels(EVmType.Vm, authCode, supportAccountId.Value, User.UserAccount.Id);
                    if (string.IsNullOrEmpty(regVmResult.ErrorMessage))
                    {
                        //we don't need the result: GetVmElaTokens will check the bounded/unbounded status and apply the additional tokens.
                        await _virtualMachineLogic.GetVmElaTokens(EVmType.Vm, authCode);
                    }
                    else
                        throw new Exception(string.Format("Failed to generate 5 VM models for {0}.", authCode));
                }
            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result.ErrorMessage = "An error occurred while adding the auth code";
            }

            return await Task.FromResult(result);
        }

        //manage ELA Grandparent AuthCode tokens
        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.ManageVmElaTokens)]
        public async Task<ActionResult> ManageElaAuthCodeTokens(string authCode)
        {
            var model = await _virtualMachineLogic.GetVmElaTokens(EVmType.Vm, authCode);
            return PartialView("../Partial/_ElaAuthCodeTokenManagement", model);
        }

        //Kendo needs POST: get VM-ELA 5-Vm models' details
        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ManageVmElaTokens)]
        public async Task<ActionResult> GetCurrentAccountModelsInfo([DataSourceRequest] DataSourceRequest request, string grandParentAuthCode, long supportAccountId)
        {
            IEnumerable<VmElaChildModelInfo> items = await _virtualMachineLogic.GetCurrentAccountVmElaModels(EVmType.Vm, grandParentAuthCode, supportAccountId);
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
            DataSourceResult result = items.ToDataSourceResult(sanitizedRequest);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ManageVmElaTokens)]
        public async Task<ActionResult> SaveVmElaAllocatedQuantities([FromBody] string modelsInfo, [FromBody] string grandParanetAuthCode, [FromBody] long totalAllocatedTokens)
        {
            if (string.IsNullOrEmpty(modelsInfo))
            {
                return Json(new { Errro = "modelsInfo is NULL" }, JsonRequestBehavior.AllowGet);
            }
            modelsInfo = !string.IsNullOrEmpty(modelsInfo) ? modelsInfo.Replace("'", "") : String.Empty;
            var quantities = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<VmElaChildModelInfo>>>(modelsInfo, new JsonSerializerSettings()
            {
                MaxDepth = 32,
                TypeNameHandling = TypeNameHandling.None
            });
            var error = await _virtualMachineLogic.SaveVmElaAllocatedQuantities(EVmType.Vm, grandParanetAuthCode, totalAllocatedTokens, quantities);
            return Json(new { ErrorMsg = error }, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.ManageVmElaTokens)]
        public async Task<ActionResult> GrantElaAuthCodeTokens(string authCode)
        {
            VmElaGrantTokenDto model = new VmElaGrantTokenDto
            {
                GrandParentAuthCode = authCode,
                NotifyUser = true
            };
            return await Task.FromResult(PartialView("../Partial/_ElaAuthCodeGrantTokens", model));
        }

        [System.Web.Mvc.HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.ManageVmElaTokens)]
        public async Task<ActionResult> BeginGrantTokenProcess(VmElaGrantTokenDto model)
        {
            var supportAccountId = CurrentState.GetSupportAccountId();

            if (!ModelState.IsValid)
            {
                return PartialView("../Partial/_ElaAuthCodeGrantTokens", model);
            }

            var result = await _virtualMachineLogic.GrantVmElaTokens(EVmType.Vm, model.GrandParentAuthCode, User.UserAccount.Id, model.DestinationEmail, model.NotifyUser, supportAccountId ?? 0);
            if (!string.IsNullOrEmpty(result))
                model.Notifications.Add(new ResponseNotification(result, ResponseState.Error));
            else
            {
                model.Notifications.Add(new ResponseNotification(string.Format("You have grant {0} to use the Auth-Code successfully.", model.DestinationEmail), ResponseState.Success));
            }
            return PartialView("../Partial/_ElaAuthCodeGrantTokens", model);
        }

        //UI
        [System.Web.Mvc.HttpGet, System.Web.Mvc.Authorize]
        public async Task<ActionResult> GetPendingElaTokensGrantItems()
        {
            var model = new ElaTokensGrantDto();
            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateVmAuthCode;

            return await Task.FromResult(PartialView("../Partial/_ElaAcceptTokensGrant", model));
        }

        //Grid Read Action
        [System.Web.Mvc.HttpPost, System.Web.Mvc.Authorize]
        public async Task<ActionResult> GetPendingElaTokensGrantItemsData([DataSourceRequest] DataSourceRequest request)
        {
            var items = await _virtualMachineLogic.GetPendingElaTokensGrantItems(EVmType.Vm, CurrentState.GetSupportAccount().Id, User.UserAccount.Id);
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
            DataSourceResult result = items.ToDataSourceResult(sanitizedRequest);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, System.Web.Mvc.Authorize]
        public async Task<ActionResult> AcceptElaTokensGrant(long vmElaTokenGrantId, string authCode, DateTime grantDate, bool accepted, string grantedBy)
        {
            var supportAccountId = CurrentState.GetSupportAccountId();
            if (supportAccountId == null)
                return Json(new { ErrorMsg = "Error to retrieve support account." }, JsonRequestBehavior.AllowGet);

            var elaTokenModel = new ElaTokensGrantModel
            {
                AuthorizationCode = authCode,
                SupportAccountId = supportAccountId.Value,
                GrantDate = grantDate,
                SourceUser = grantedBy
            };

            var error = await _virtualMachineLogic.AcceptElaTokenGrant(EVmType.Vm, elaTokenModel, accepted, User.UserAccount.Id);

            if (string.IsNullOrEmpty(error))
            {
                SiteHelper.UpdateNotificationMessageStatus(entityId: vmElaTokenGrantId.ToString(), category: Api.Model.Enums.NotificationCategory.VMELATokenGrant.ToDescription());
            }

            return Json(new { ErrorMsg = error }, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ManageVmElaTokens)]
        public ActionResult RevokeVmElaLicense([FromBody] string authCode, [FromBody] long supportAccountId)
        {
            var result = _virtualMachineLogic.RevokeVmElaLicense(EVmType.Vm, authCode, supportAccountId);
            return Json(new { Message = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
