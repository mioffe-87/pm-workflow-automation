using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Csp.Api;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using NPOI.SS.Formula;

namespace Csp.Web.Controllers {
	public class CnSeriesController : BaseController 
    {
        private readonly IVirtualMachineLogic vmLogic;
        private readonly IEulaLogic eulaLogic;
        private readonly ILookupLogic lookupLogic;
        private readonly ILicenseLogic licenseLogic;

        public CnSeriesController(IVirtualMachineLogic vmLogic, IEulaLogic eulaLogic, ILookupLogic lookupLogic, ILicenseLogic licenseLogic) 
        {
            this.vmLogic = vmLogic;
            this.eulaLogic = eulaLogic;
            this.lookupLogic = lookupLogic;
            this.licenseLogic = licenseLogic;
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult ViewAvailable(string defaultDeviceCode = null, string defaultSerialNumber = null)
        {
            var cnViewModel = new VmViewModelDto<AvailableVirtualSeries<AvailableCn>> { ActionHelper = new ActionHelper("CN"), DefaultDeviceCode = defaultDeviceCode, };
            cnViewModel.ActionHelper.Id = CurrentState.GetSupportAccountId().Value;
            var supportAcctId = CurrentState.GetCurrentAccountId();
            var dataParameters = new DataParameters { PaginationCount = 10 };
            cnViewModel.DataParameters = dataParameters;
            cnViewModel.AvailableVms = new AvailableVirtualSeries<AvailableCn>[0]; //vmLogic.GetAvailableCns(dataParameters, supportAcctId, User.UserAccount.Id, defaultDeviceCode, defaultSerialNumber);
            cnViewModel.HasTokenGrantRequest = vmLogic.HasPendingTokensGrantRequest(EVmType.Cn, supportAcctId, User.UserAccount.Id);
            cnViewModel.SupportAccountId = supportAcctId;
            return PartialView("../Partial/_CnSeriesView", cnViewModel);
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult ViewReleasedLicenses()
        {
            var licenseViewModel = new ReleasedLicenseDto { ActionHelper = new ActionHelper("CnSeries") };
            licenseViewModel.ActionHelper.Id = CurrentState.GetSupportAccountId().Value;
            var supportAcctId = CurrentState.GetCurrentAccountId();
            var dataParameters = new DataParameters { PaginationCount = 10 };
            licenseViewModel.DataParameters = dataParameters;
            licenseViewModel.AvailableLicenses = vmLogic.GetReleasedLicenses(EVmType.Cn, dataParameters, supportAcctId);
            licenseViewModel.SupportAccountId = supportAcctId;
            return PartialView("../Partial/_ReleasedCnSeriesAuthCodes", licenseViewModel);
        }

        [System.Web.Mvc.HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult SearchReleasedLicenses([DataSourceRequest] DataSourceRequest request)
        {
            var dataParameters = request.ToDataParameters();

            var releasedCnSeries = new System.Collections.Generic.List<ReleasedLicenseItem>();

            var result = new DataSourceResult
            {
                Data = releasedCnSeries,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }


        [System.Web.Mvc.HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult SearchAvailable([DataSourceRequest] DataSourceRequest request, string authCode, string serialNumber)
        {
            var dataParameters = request.ToDataParameters();

            var availableCnSeries = vmLogic.GetAvailableCns(dataParameters, CurrentState.GetCurrentAccountId(), User.UserAccount.Id, authCode, serialNumber);

            var result = new DataSourceResult
            {
                Data = availableCnSeries,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public async Task<FileResult> SearchAvailableReport(string authCode, string serialNumber)
        {
            var csv = await vmLogic.GetAvailableCnsCsv(CurrentState.GetCurrentAccountId(), User.UserAccount.Id, authCode, serialNumber);

            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            return File(csv, "text/comma-separated-values", "CnSeriesReport_" + dateStr + ".csv");
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult RegisterCnSeries(string authCode)
        {
            var registerCnSeriesDto = new RegisterVmDto
            {
                AuthCode = authCode,
            };

            return PartialView("../Partial/_CnSeriesRegister", registerCnSeriesDto);
        }


        [System.Web.Mvc.HttpPost, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult RegisterCnSeries(RegisterVmDto model)
        {
            RegisterVmResult result = null;

            //CPU ID start from GCP:
            if (model.HyperVisorVersion == ApiConstants.HYPERVISOR_VERSION_GOOGLE && !string.IsNullOrEmpty(model.CpuId) && !model.CpuId.StartsWith("GCP:", StringComparison.InvariantCultureIgnoreCase))
            {
                model.CpuId = "GCP:" + model.CpuId;
            }

            if (!ModelState.IsValid)
            {
                return PartialView("../Partial/_CnSeriesRegister", model);
            }

            try
            {
                result = vmLogic.RegisterVm(EVmType.Cn, model.UuId, model.CpuId, model.AuthCode, CurrentState.GetCurrentAccountId(), User.UserAccount.Id, model.HyperVisorVersion, model.PanOsVersion);
            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new RegisterVmResult { ErrorMessage = "An error occurred while registering the CN/SN series" };
            }

            ModelState.Remove("NewDeviceId");

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                model.Notifications.Add(new ResponseNotification("Success", ResponseState.Success));
                model.NewDeviceId = result.NewDeviceId;
            }
            else
            {
                model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
            }

            return PartialView("../Partial/_CnSeriesRegister", model);
        }

        // new user registration for Panoram needs to access
        [System.Web.Mvc.AllowAnonymous]
        [System.Web.Mvc.HttpPost, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult RegisterCnSeriesFile(HttpPostedFileBase[] uploadUuIdCpuId)
        {
            ParseVmFileResult result = null;
            if (uploadUuIdCpuId.Length == 1)
            {
                result = vmLogic.ParseVmFile(EVmType.Cn, uploadUuIdCpuId.Single());
            }
            else
            {
                result = new ParseVmFileResult { Message = "Upload failed.  File could not be found!" };
            }

            return Json(result);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult ActivateCnSeriesAuthCode(ActivateCnSeriesAuthCodeDto model)
        {
            if (model != null && ModelState.IsValid)
            {
                string authCode = model.AuthorizationCode;
                AddVmAuthCodeResult result;
                try
                {
                    if (model.History != null
                        && model.History.Any(i => string.Compare(i, model.AuthorizationCode, true) == 0))
                    {
                        result = new AddVmAuthCodeResult { AuthCode = authCode, ErrorMessage = "The auth code is already activated" };
                    }
                    else
                    {
                        result = vmLogic.AddVmAuthCode(EVmType.Cn, model.AuthorizationCode, CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
                    }
                }
                catch (Exception ex)
                {

                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    result = new AddVmAuthCodeResult() { AuthCode = authCode, ErrorMessage = Csp.Api.Logic.VirtualMachineLogic.VmAuthCodeMessages.AuthCodeAlreadyAssociated };
                }

                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    #region Store Eula Agreements

                    try
                    {
                        var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                        var eulaContextId = EulaContext.ActivateCnSeriesAuthCode;
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

                        eulaLogic.EndUserAgreements(ea);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while accepting while activating CN/SN Auth Code - {0}. Eula Id Selected is - {1}", model.AuthorizationCode, model.Eula.DefaultEula.Id)));
                    }

                    #endregion

                    if (!string.IsNullOrWhiteSpace(model.AuthorizationCode))
                    {
                        model.History.Add(model.AuthorizationCode);
                    }
                    model.AuthorizationCode = String.Empty;
                    model.Notifications.Add(new ResponseNotification("Successfully Activated CN-Series Auth-Code", ResponseState.Success));
                }
                else
                {
                    model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + result.ErrorMessage));
                }
            }

            #region Bind Eula Model

            model.Eula = new EulaModel()
            {
                Languages = eulaLogic.GetEulaLanguages(),
                Eulas = eulaLogic.GetEula(),
                DefaultEula = eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateCnSeriesAuthCode;

            #endregion

            return PartialView("../Partial/_AddCnSeriesAuthCode", model);
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult AddCnSeriesAuthCode()
        {
            var model = new ActivateCnSeriesAuthCodeDto();

            #region Bind Eula to Model

            model.Eula = new EulaModel()
            {
                Languages = eulaLogic.GetEulaLanguages(),
                Eulas = eulaLogic.GetEula(),
                DefaultEula = eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateCnSeriesAuthCode;

            #endregion

            return PartialView("../Partial/_AddCnSeriesAuthCode", model);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister), ValidateAntiForgeryToken]
        public ActionResult AddCnSeriesAuthCode(AvailableVm model)
        {
            if (model != null && ModelState.IsValid)
            {
                AddVmAuthCodeResult result;
                try
                {
                    result = vmLogic.AddVmAuthCode(EVmType.Cn, model.AuthorizationCode, CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
                }
                catch (Exception ex)
                {

                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    result = new AddVmAuthCodeResult { AuthCode = model.AuthorizationCode, ErrorMessage = "An error occurred while adding the auth code" };
                }

                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var availableCnSeries = vmLogic.GetAvailableCns(new DataParameters(), CurrentState.GetCurrentAccountId(), User.UserAccount.Id, model.AuthorizationCode, string.Empty);
                    var match = availableCnSeries.FirstOrDefault(x => x.AuthorizationCode == model.AuthorizationCode);
                    if (match != null)
                    {
                        model = new AvailableVm
                        {
                            AuthorizationCode = match.AuthorizationCode
                        };
                    }
                    else
                    {
                        ModelState.AddModelError("AuthorizationCode", "An error occurred while adding the auth code");
                    }
                }
                else
                {
                    ModelState.AddModelError("AuthorizationCode", result.ErrorMessage);
                }
            }

            return Json(new[] { model }.ToDataSourceResult(new DataSourceRequest(), ModelState));
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> AllocateTokensToPanorama()
        {
            var model = new AllocateCnSeriesTokensDto();
            var supportAccountId = CurrentState.GetCurrentAccountId();
            var cnAuthCodeTokenDetailsTask = vmLogic.GetCnAuthCodeTokenDetails(supportAccountId);
            var cnAvailablePanoramaTask = vmLogic.GetCnAvailablePanorama(supportAccountId);
            await Task.WhenAll(cnAuthCodeTokenDetailsTask, cnAvailablePanoramaTask);
            var cnAuthCodeTokendetails = await cnAuthCodeTokenDetailsTask;
            var cnAvailablePanorama = await cnAvailablePanoramaTask;
            model.Cnxs = cnAuthCodeTokendetails.CnxAuthCodes;
            model.AvailablePanoramas = cnAvailablePanorama.Select(x => new AllocateCnSeriesTokensToPanoramaDto()
            {
                Id = x.DeviceId,
                SerialNumber = x.SerialNumber,
                ModelName = x.ModelName
            }).ToList();
            model.AvailableTokens = cnAuthCodeTokendetails.AvailableTokens;
            model.MaxNewRecords = Convert.ToInt32(ConfigurationManager.AppSettings["MaxCnSeriesPanoramaAllocation"]);
            return PartialView("../Partial/_AllocateCnSeriesTokensToPanorama", model);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister), ValidateAntiForgeryToken]
        public ActionResult AllocateTokensToPanorama(AllocateCnSeriesTokensDto model)
        {
            var result = new AllocateCnSeriesTokensDto();
            if (model != null
                && ModelState.IsValid)
            {
                var successfulAllocationCount = 0;
                foreach (var p in model.Panoramas)
                {
                    try
                    {
                        licenseLogic.AllocateTokensToCnPanorama(p.SerialNumber, p.Tokens, p.CnxAuthCode);
                        successfulAllocationCount++;
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        result = new AllocateCnSeriesTokensDto { Panoramas = model.Panoramas, ErrorMessage = "An error occurred while allocating tokens" };
                    }
                }
                if (successfulAllocationCount > 0 && successfulAllocationCount != model.Panoramas.Count)
                {
                    result.Notifications.Add(new ResponseNotification("Tokens allocation is partially successful", ResponseState.Warning));
                }
                else if (successfulAllocationCount == model.Panoramas.Count && model.AvailableTokens == model.Panoramas.Sum(x => x.Tokens))
                {
                    result.Notifications.Add(new ResponseNotification("You have allocated all of your available tokens", ResponseState.Warning));
                }
                else if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.Notifications.Add(new ResponseNotification("Tokens successfully allocated", ResponseState.Success));
                }
                else
                {
                    result.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
                }
            }
            else
            {
                result.Notifications.Add(new ResponseNotification("Tokens allocation failed", ResponseState.Error));
            }
            return Json(result);
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult CanProvisionPanorama(string authCode)
        {
            var result = vmLogic.CanProvisionPanorama(EVmType.Cn, authCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ProvisionPanorama(string authCode)
        {
            long supportAccountId = CurrentState.GetCurrentAccountId();
            long userAccountId = User.UserAccount.Id;
            var result = vmLogic.ProvisionPanorama(EVmType.Cn, authCode, supportAccountId, userAccountId);
            return Json(result);
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ViewProvisionedPanoramaDevices(string authCode, string partNumber = null)
        {
            var dto = new NsxDto();
            dto.AuthCode = authCode;
            dto.SupportAccountId = CurrentState.GetCurrentAccountId();
            dto.PartNumber = partNumber;
            return PartialView("../Partial/_ProvisionPraDevice", dto);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetProvisionedPanoramaDevices(string authCode, DataSourceRequest dreq)
        {
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(dreq.Filters);
            var sorts = KendoHelper.SanitizeSorts(dreq.Sorts);
            var groups = KendoHelper.SanitizeGroups(dreq.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(dreq.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = dreq.Page;
            sanitizedRequest.PageSize = dreq.PageSize;
            sanitizedRequest.Sorts = sorts;
            var devices = vmLogic.GetProvisionedPraDevices(EVmType.Cn, authCode);
            DataSourceResult result = devices.ToDataSourceResult(sanitizedRequest);
            return Json(result);
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> Renewal([FromUri]string authCode, [FromUri]string description, [FromUri]int totalQuantity, [FromUri]int provisionedNum, [FromUri] string partNumber)
        {
            var supportAcctId = CurrentState.GetCurrentAccountId();
            bool defaultToSelectAll, lockUi, hasPendingRenewals;
            //cannot use async: it has out parameters
            var latestQuoteNumber = vmLogic.GetLatestQuoteInfo(EVmType.Cn, supportAcctId, authCode, out defaultToSelectAll, out lockUi, out hasPendingRenewals);
            var renewalModel = await vmLogic.GetUnprovisionedRenewalDetails(EVmType.Cn, authCode, latestQuoteNumber);
            var actualProvisionedCnSeries = await vmLogic.GetActualProvisionedVms(EVmType.Cn, supportAcctId, authCode);
            var model = new VmRenewalModel
            {
                AuthCode = authCode,
                Description = description,
                LatestQuoteNumber = latestQuoteNumber,
                SelectAll = defaultToSelectAll,
                TotalQuantity = totalQuantity,
                ProvisionedQuantity = actualProvisionedCnSeries,
                PartNumber = partNumber,
                LockUi = lockUi,
                HasPendingRenewals = hasPendingRenewals
            };
            if (renewalModel.UnprovisionedInfo != null)
            {
                model.UnprovisionedInfo = renewalModel.UnprovisionedInfo;
            }
            return PartialView("../Partial/_CnSeriesRenewal", model);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> GetRenewalData([DataSourceRequest] DataSourceRequest request, string authCode, string quoteNumber, bool selectAll, bool hasPendingRenewals)
        {
            var supportAcctId = CurrentState.GetCurrentAccountId();
            //get the provisioned SNs of this authcode
            var vmsInfo = await vmLogic.GetVmRenewalDetails(EVmType.Cn, supportAcctId, authCode, quoteNumber, selectAll, hasPendingRenewals);
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
            DataSourceResult result = vmsInfo.ToDataSourceResult(sanitizedRequest);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public ActionResult SaveRenewalSerialNumbers([FromBody]VmRenewalModel model, bool allSelected, string quoteNumber)
        {
            var supportAcctId = CurrentState.GetCurrentAccountId();
            var userAccountId = User.UserAccount.Id;
            //retieve the latest quoteNumber in case CPQ update it
            bool defaultToSelectAll, lockUi, hasPendingRenewals;
            var latestQuoteNumber = vmLogic.GetLatestQuoteInfo(EVmType.Cn, supportAcctId, model.AuthCode, out defaultToSelectAll, out lockUi, out hasPendingRenewals);
            string errorMsg;
            if (lockUi)
            {
                errorMsg = "The quote has been submitted for approval, it cannot be changed at present.";
            }
            else
            {
                //always apply to the latest quote.
                errorMsg = vmLogic.SaveRenewalVMs(EVmType.Cn, supportAcctId, userAccountId, model, allSelected, latestQuoteNumber);
            }

            return Json(new { ErrorMsg = errorMsg, QuoteNumber = latestQuoteNumber }, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> GetCnSeriesElaProvisionedItems([DataSourceRequest] DataSourceRequest request, string authCode)
        {
            var supportAcctId = CurrentState.GetCurrentAccountId();
            var items = await vmLogic.GetProvisionedVmsToDeactivate(EVmType.Cn, supportAcctId, authCode);
            DataSourceResult result = items.ToDataSourceResult(request);
            return await Task.FromResult(Json(result, JsonRequestBehavior.AllowGet));
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> DeactivateCnSeries([FromUri] string authCode)
        {
            var model = new DeactiveVmDto
            {
                ParentAuthCode = authCode
            };
            return await Task.FromResult(PartialView("../Partial/_DeactivateCnSeries", model));
        }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.CnSeriesRegister)]
        public async Task<ActionResult> DeactivateCnSeriesItems([FromBody] DeactiveVmDto vmDto)
        {
            //nothing to deactivate.
            if (string.IsNullOrEmpty(vmDto.ParentAuthCode) || vmDto.VmItemsToDeactivate == null || vmDto.VmItemsToDeactivate.Count() == 0)
            {
                return Json(new { ErrorMsg = string.Empty }, JsonRequestBehavior.AllowGet);
            }
            var userAccountEmail = User.UserAccount.Email;
            string errorMsg = await vmLogic.DeactivateProvisionedVms(EVmType.Cn, vmDto.VmItemsToDeactivate, userAccountEmail);
            return Json(new { ErrorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult DeactivateLicenses()
        {

            return PartialView("../Partial/_DeactivateCnSeriesLicenses", new DeactivateLicensesDto());
        }

        [System.Web.Mvc.HttpPost, CspAuthorize(Permission.EditDevice)]
        public ActionResult DeactivateLicenses(DeactivateLicensesDto model)
        {

            var success = licenseLogic.DeactivateLicense(model.DeactivationTokenText);

            model = new DeactivateLicensesDto();

            if (success)
            {
                model.Notifications.Add(new ResponseNotification("License Deactivation Successfull", ResponseState.Success));
            }
            else
            {
                model.Notifications.Add(new ResponseNotification("An error occurred during license deactivation.  Please contact customer support.", ResponseState.Error));
            }

            return PartialView("../Partial/_DeactivateCnSeriesLicenses", model);
        }
    }
}
