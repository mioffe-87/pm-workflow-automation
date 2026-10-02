using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Security.Application;

namespace Csp.Web.Controllers {
	public class VMController : BaseController {

		private readonly IVirtualMachineLogic _virtualMachineLogic;
		private readonly IEulaLogic _eulaLogic;
        private readonly ILookupLogic _lookupLogic;

		public VMController(IVirtualMachineLogic virtualMachineLogic, IEulaLogic eulaLogic, ILookupLogic lookupLogic) {
			_virtualMachineLogic = virtualMachineLogic;
			_eulaLogic = eulaLogic;
            _lookupLogic = lookupLogic;
		}


        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.VmAuthRegister)]
        public ActionResult ViewReleasedLicenses() {

            //var virtualMachineLogic = new VirtualMachineLogic();

            var licenseViewModel = new ReleasedLicenseDto {
                ActionHelper = new ActionHelper("VM"),
            };
            licenseViewModel.ActionHelper.Id = CurrentState.GetSupportAccountId().Value;
            var supportAcctId = CurrentState.GetCurrentAccountId();
            var dataParameters = new DataParameters { PaginationCount = 10 };
            licenseViewModel.DataParameters = dataParameters;
            licenseViewModel.AvailableLicenses = _virtualMachineLogic.GetReleasedLicenses(EVmType.Vm, dataParameters, supportAcctId);
            licenseViewModel.SupportAccountId = supportAcctId;
            return PartialView("../Partial/_ReleasedVMAuthCodes", licenseViewModel);
        }

        [System.Web.Mvc.HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.VmAuthRegister)]
        public ActionResult SearchReleasedLicenses([DataSourceRequest] DataSourceRequest request) {
            var dataParameters = request.ToDataParameters();

            var releasedVms = new System.Collections.Generic.List<ReleasedLicenseItem>();//_virtualMachineLogic.GetAvailableVms(dataParameters, CurrentState.GetCurrentAccountId(), authCode);

            var result = new DataSourceResult {
                Data = releasedVms,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }


		[System.Web.Mvc.HttpGet, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult ViewAvailable(string defaultDeviceCode = null) {

			//var virtualMachineLogic = new VirtualMachineLogic();

			var vmViewModel = new VmViewModelDto<AvailableVm> {
				ActionHelper = new ActionHelper("VM"),
				DefaultDeviceCode = defaultDeviceCode,
			};
			vmViewModel.ActionHelper.Id = CurrentState.GetSupportAccountId().Value;
			var supportAcctId = CurrentState.GetCurrentAccountId();
			var dataParameters = new DataParameters { PaginationCount = 10 };
			vmViewModel.DataParameters = dataParameters;
     // vmViewModel.AvailableVms = _virtualMachineLogic.GetAvailableVms(dataParameters, supportAcctId, User.UserAccount.Id, defaultDeviceCode);
            vmViewModel.HasTokenGrantRequest = _virtualMachineLogic.HasPendingTokensGrantRequest(EVmType.Vm, supportAcctId, User.UserAccount.Id);
			vmViewModel.SupportAccountId = supportAcctId;
			return PartialView("../Partial/_VMView", vmViewModel);
		}

		[System.Web.Mvc.HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult SearchAvailable([DataSourceRequest] DataSourceRequest request, string authCode) {
			var dataParameters = request.ToDataParameters();

            var availableVms = _virtualMachineLogic.GetAvailableVms(dataParameters, CurrentState.GetCurrentAccountId(), User.UserAccount.Id, authCode);

			var result = new DataSourceResult {
				Data = availableVms,
				Total = Convert.ToInt32(dataParameters.TotalRecordCount)
			};

			return Json(result);
		}

		[System.Web.Mvc.HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public async Task<FileResult> SearchAvailableReport(string authCode) {

            var csv = await _virtualMachineLogic.GetAvailableVmsCsv(CurrentState.GetCurrentAccountId(), User.UserAccount.Id, authCode);

			var dateStr = DateTime.Now.ToString("yyyyMMdd");

			return File(csv, "text/comma-separated-values", "VmsReport_" + dateStr + ".csv");
		}

		[System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult RegisterVm(string authCode) {

			var registerVmDto = new RegisterVmDto {
				AuthCode = authCode,
			};

			return PartialView("../Partial/_VMRegister", registerVmDto);
		}


        [System.Web.Mvc.HttpPost, CspAuthorize(Permission.VmAuthRegister)]
        public ActionResult RegisterVm(RegisterVmDto model) 
        {
            RegisterVmResult result;

            //CPU ID start from GCP:
            if (model.HyperVisorVersion == ApiConstants.HYPERVISOR_VERSION_GOOGLE && !string.IsNullOrEmpty(model.CpuId) && !model.CpuId.StartsWith("GCP:", StringComparison.InvariantCultureIgnoreCase))
            {
                model.CpuId = "GCP:" + model.CpuId;
            }

            if (!ModelState.IsValid)
            {             
                return PartialView("../Partial/_VMRegister", model);
            }

            try
            {
                result = _virtualMachineLogic.RegisterVm(EVmType.Vm, model.UuId, model.CpuId, model.AuthCode,
                        CurrentState.GetCurrentAccountId(), User.UserAccount.Id, model.HyperVisorVersion, model.PanOsVersion);
            } 
            catch (Exception ex) 
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new RegisterVmResult {
                    ErrorMessage = "An error occurred while registering the VM",
                };
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

            return PartialView("../Partial/_VMRegister", model);
        }

        // new user registration for Panoram needs to access
        [System.Web.Mvc.AllowAnonymous]
		[System.Web.Mvc.HttpPost, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult RegisterVmFile(HttpPostedFileBase[] uploadUuIdCpuId) {

			ParseVmFileResult result;
			if (uploadUuIdCpuId.Length == 1) {
                result = _virtualMachineLogic.ParseVmFile(EVmType.Vm, uploadUuIdCpuId.Single());
			} else {
				result = new ParseVmFileResult {
					Message = "Upload failed.  File could not be found!"
				};
			}

			return Json(result);
		}

		[System.Web.Mvc.HttpGet, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult AddVmAuthCode() {
			var model = new ActivateVmAuthCodeDto();

			#region Bind Eula to Model

			model.Eula = new EulaModel() {
				Languages = _eulaLogic.GetEulaLanguages(),
				Eulas = _eulaLogic.GetEula(),
				DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
				SelectedLanguage = EulaLanguage.English
			};
			model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
			model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
			model.Eula.Context = EulaContext.ActivateVmAuthCode;

			#endregion

			return PartialView("../Partial/_AddVMAuthCode", model);
		}

		[System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
		public ActionResult ActivateVmAuthCode(ActivateVmAuthCodeDto model) {

			if (model != null && ModelState.IsValid) {
				string authCode = model.AuthorizationCode;
				AddVmAuthCodeResult result;
				try {
                    result = _virtualMachineLogic.AddVmAuthCode(EVmType.Vm, model.AuthorizationCode, CurrentState.GetCurrentAccountId(), User.UserAccount.Id);

				} catch (Exception ex) {

					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
					result = new AddVmAuthCodeResult {
						ErrorMessage = "An error occurred while adding the auth code",
					};
				}

				if (string.IsNullOrEmpty(result.ErrorMessage)) {
					#region Store Eula Agreements

					try {
						var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
						var eulaContextId = EulaContext.ActivateVmAuthCode;
						var eulaId = model.Eula.SelectedEulaId;
						var signedBy = User.UserAccount.Email;
						var licenseId = LicenseLogic.GetLicenseByAuthcode(model.AuthorizationCode).Id;

						var ea = new EulaAgreements() {
							LicenseId = licenseId,
							SupportAccountId = supportAccountId,
							EulaContextId = eulaContextId,
							EulaId = eulaId,
							SignedBy = signedBy,
							Notes = string.Format("AuthCode entered is {0}", authCode)
						};

						_eulaLogic.EndUserAgreements(ea);
					} catch (Exception) {
						Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while accepting while activating VM Auth Code - {0}. Eula Id Selected is - {1}", model.AuthorizationCode, model.Eula.DefaultEula.Id)));
					}

					#endregion

					model.AuthorizationCode = String.Empty;
					model.Notifications.Add(new ResponseNotification("Successfully Activated VM-Series Auth-Code", ResponseState.Success));
				} else {
					model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + result.ErrorMessage));
				}
			}

			#region Bind Eula Model

			model.Eula = new EulaModel() {
				Languages = _eulaLogic.GetEulaLanguages(),
				Eulas = _eulaLogic.GetEula(),
				DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
				SelectedLanguage = EulaLanguage.English
			};
			model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
			model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
			model.Eula.Context = EulaContext.ActivateVmAuthCode;

			#endregion

			return PartialView("../Partial/_AddVMAuthCode", model);
		}


		[System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.VmAuthRegister), ValidateAntiForgeryToken]
		public ActionResult AddVmAuthCode(AvailableVm model) {

			if (model != null && ModelState.IsValid) {
				AddVmAuthCodeResult result;
				try {
                    result = _virtualMachineLogic.AddVmAuthCode(EVmType.Vm, model.AuthorizationCode, CurrentState.GetCurrentAccountId(), User.UserAccount.Id);

				} catch (Exception ex) {

					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
					result = new AddVmAuthCodeResult {
						ErrorMessage = "An error occurred while adding the auth code",
					};
				}

				if (string.IsNullOrEmpty(result.ErrorMessage)) {
                    var availableVms = _virtualMachineLogic.GetAvailableVms(new DataParameters(), CurrentState.GetCurrentAccountId(), User.UserAccount.Id, model.AuthorizationCode);

					var match = availableVms.FirstOrDefault(x => x.AuthorizationCode == model.AuthorizationCode);

					if (match != null) {
						model = match;
					} else {
						ModelState.AddModelError("AuthorizationCode", "An error occurred while adding the auth code");
					}
				} else {
					ModelState.AddModelError("AuthorizationCode", result.ErrorMessage);
				}
			}

			return Json(new[] { model }.ToDataSourceResult(new DataSourceRequest(), ModelState));
		}

		[System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult CanProvisionPanorama(string authCode) {
            var result = _virtualMachineLogic.CanProvisionPanorama(EVmType.Vm, authCode);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		[System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult ProvisionPanorama(string authCode) {
			long supportAccountId = CurrentState.GetCurrentAccountId();
			long userAccountId = User.UserAccount.Id;
            var result = _virtualMachineLogic.ProvisionPanorama(EVmType.Vm, authCode, supportAccountId, userAccountId);
			return Json(result);
		}

		[System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult ViewProvisionedPanoramaDevices(string authCode, string partNumber = null) {
			var dto = new NsxDto();
			dto.AuthCode = authCode;
			dto.SupportAccountId = CurrentState.GetCurrentAccountId();
		    dto.PartNumber = partNumber;
			return PartialView("../Partial/_ProvisionPraDevice", dto);
		}

		[System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult GetProvisionedPanoramaDevices(string authCode, DataSourceRequest dreq) {
            var devices = _virtualMachineLogic.GetProvisionedPraDevices(EVmType.Vm, Sanitizer.GetSafeHtmlFragment(authCode));
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
            DataSourceResult result = devices.ToDataSourceResult(sanitizedRequest);
            return Json(result);
		}

        [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
        public async Task<ActionResult> Renewal([FromUri]string authCode, [FromUri]string description, [FromUri]int totalQuantity, [FromUri]int provisionedNum, [FromUri] string partNumber)
	    {
	        var supportAcctId = CurrentState.GetCurrentAccountId();
	        bool defaultToSelectAll, lockUi, hasPendingRenewals;
            //cannot use async: it has out parameters
            var latestQuoteNumber = _virtualMachineLogic.GetLatestQuoteInfo(EVmType.Vm, supportAcctId, authCode, out defaultToSelectAll, out lockUi, out hasPendingRenewals);
            var renewalModel = await _virtualMachineLogic.GetUnprovisionedRenewalDetails(EVmType.Vm, authCode, latestQuoteNumber);
            var actualProvisionedVMs = await _virtualMachineLogic.GetActualProvisionedVms(EVmType.Vm, supportAcctId, authCode);
	        var model = new VmRenewalModel
	        {
	            AuthCode = authCode,
                Description = description,
                LatestQuoteNumber = latestQuoteNumber,
                SelectAll = defaultToSelectAll,
                TotalQuantity = totalQuantity,
                ProvisionedQuantity = actualProvisionedVMs,
	            PartNumber = partNumber,
                LockUi = lockUi,
                HasPendingRenewals = hasPendingRenewals
	        };
	        if (renewalModel.UnprovisionedInfo != null)
	            model.UnprovisionedInfo = renewalModel.UnprovisionedInfo;
	        return PartialView("../Partial/_VmRenewal", model);
	    }

        [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
        public async Task<ActionResult> GetRenewalData([DataSourceRequest] DataSourceRequest request, string authCode, string quoteNumber, bool selectAll, bool hasPendingRenewals)
	    {
	        var supportAcctId = CurrentState.GetCurrentAccountId();
	        //get the provisioned SNs of this authcode
            var vmsInfo = await _virtualMachineLogic.GetVmRenewalDetails(EVmType.Vm, supportAcctId, authCode, quoteNumber, selectAll, hasPendingRenewals);            var sanitizedRequest = new DataSourceRequest();
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

	    [System.Web.Mvc.HttpPost, AjaxOnly, ValidateJsonAntiForgeryToken, CspAuthorize(Permission.VmAuthRegister)]
        public ActionResult SaveRenewalSerialNumbers([FromBody]VmRenewalModel model, bool allSelected, string quoteNumber)
	    {
	        var supportAcctId = CurrentState.GetCurrentAccountId();
	        var userAccountId = User.UserAccount.Id;
	        //retieve the latest quoteNumber in case CPQ update it
	        bool defaultToSelectAll, lockUi, hasPendingRenewals;
            var latestQuoteNumber = _virtualMachineLogic.GetLatestQuoteInfo(EVmType.Vm, supportAcctId, model.AuthCode, out defaultToSelectAll, out lockUi, out hasPendingRenewals);
	        string errorMsg;
	        if (lockUi)
	        {
	            errorMsg = "The quote has been submitted for approval, it cannot be changed at present.";
            }
	        else
	        {
	            //always apply to the latest quote.
                errorMsg = _virtualMachineLogic.SaveRenewalVMs(EVmType.Vm, supportAcctId, userAccountId, model, allSelected, latestQuoteNumber);	            
	        }

            return Json(new {ErrorMsg = errorMsg, QuoteNumber = latestQuoteNumber}, JsonRequestBehavior.AllowGet);
	    }

	    [System.Web.Mvc.HttpGet, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
	    public async Task<ActionResult> DeactivateVm([FromUri] string authCode)
	    {
	        var model = new DeactiveVmDto
	        {
                ParentAuthCode = authCode
	        };
	        return await Task.FromResult(PartialView("../Partial/_DeactivateVm", model));	        
	    }

	    [System.Web.Mvc.HttpPost, CspAuthorize(Permission.VmAuthRegister)]
        public async Task<ActionResult> GetVmElaProvisionedItems([DataSourceRequest] DataSourceRequest request, string authCode)
	    {
	        var supportAcctId = CurrentState.GetCurrentAccountId();
            var items = await _virtualMachineLogic.GetProvisionedVmsToDeactivate(EVmType.Vm, supportAcctId, authCode);
	        DataSourceResult result = items.ToDataSourceResult(request);
	        return await Task.FromResult(Json(result, JsonRequestBehavior.AllowGet));	        
	    }

	    [System.Web.Mvc.HttpPost, AjaxOnly, CspAuthorize(Permission.VmAuthRegister)]
	    public async Task<ActionResult> DeactivateVmItems([FromBody] DeactiveVmDto vmDto)
	    {
            //nothing to deactivate.
	        if (string.IsNullOrEmpty(vmDto.ParentAuthCode) || vmDto.VmItemsToDeactivate == null || vmDto.VmItemsToDeactivate.Count() == 0)
	        {
	            return Json(new { ErrorMsg = string.Empty }, JsonRequestBehavior.AllowGet);	        
	        }
	        var userAccountEmail = User.UserAccount.Email;
            string errorMsg = await _virtualMachineLogic.DeactivateProvisionedVms(EVmType.Vm, vmDto.VmItemsToDeactivate, userAccountEmail);
	        return Json(new { ErrorMsg = errorMsg }, JsonRequestBehavior.AllowGet);	        
	    }
	}
}
