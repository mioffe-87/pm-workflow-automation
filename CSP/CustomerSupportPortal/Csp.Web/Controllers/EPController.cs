
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Text;
    using System.Web.Http.Results;
    using Csp.Api.Security;
    using Csp.Web.Security.Attributes;
using Microsoft.Security.Application;

    namespace Csp.Web.Controllers
{
		[Authorize]
    public class EPController : BaseController
    {
        private readonly IEPLogic _epLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly IConnectionStrings _connectionStrings;
        private readonly ITrapsELALogic _trapsELALogic;

        public EPController(IEPLogic epLogic, ILicenseLogic licenseLogic, IDeviceLogic deviceLogic, IEulaLogic eulaLogic, ITrapsELALogic trapsELALogic, IConnectionStrings connectionStrings)
        {
            _epLogic = epLogic;
            _licenseLogic = licenseLogic;
            _deviceLogic = deviceLogic;
            _eulaLogic = eulaLogic;
            _trapsELALogic = trapsELALogic;
            _connectionStrings = connectionStrings;
        }

        //This shows the view which has a Grid to display all End Point licenses
        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult ViewAvailable(string defaultDeviceCode = null)
        {
            var epViewModel = new EpViewModelDto()
                {
									ActionHelper = new ActionHelper("EP"),
									DefaultDeviceCode = defaultDeviceCode,
                };
            epViewModel.ActionHelper.Id = CurrentState.GetSupportAccountId().Value;
            var dataParameters = new DataParameters {PaginationCount = 10};
            epViewModel.DataParameters = dataParameters;
            epViewModel.AllDeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(),CurrentState.GetLoggedInUser().UserAccount.Id);
            return PartialView("../Partial/_EPView", epViewModel);
        }

        //This returns a list of end point licenses and allows search by serialnumber,authcode,devicetag, category
				[HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult SearchAvailable([DataSourceRequest] DataSourceRequest request, string searchField = "", string searchValue = "")
        {
            var dataParameters = request.ToDataParameters();
            var availableEps = _epLogic.GetAvailableEps(dataParameters, CurrentState.GetCurrentAccountId(), searchField, searchValue, shouldGroupAuthCodes: true);
            var result = new DataSourceResult
            {
                Data = availableEps,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
			var jsonResult = Json(result);
			jsonResult.MaxJsonLength = int.MaxValue;
			return jsonResult;
        }

				[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
			public FileResult SearchAvailableReport(long supportAccountId, string searchField, string searchValue) {

				if (supportAccountId != CurrentState.GetCurrentAccountId())
				{
					return null;
				} 

				var csv = _epLogic.GetAvailableEpsCsv(supportAccountId, searchField, searchValue);

				var dateStr = DateTime.Now.ToString("yyyyMMdd");

				return File(csv, "text/comma-separated-values", "AdvancedEndpointProtectionReport_" + dateStr + ".csv");
			}


        //This adds a new ESM auth code.
        [ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult AddEpAuthCode(string authCode)
        {
            var model = new AvailableEp() { AuthorizationCode = authCode };
            AddEpAuthCodeResult result = null;
            try
            {
                result = _epLogic.AddEpAuthCode(model.AuthorizationCode, CurrentState.GetCurrentAccountId(),
                                       User.UserAccount.Id, User.UserAccount.Email);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new AddEpAuthCodeResult
                {
                    ErrorMessage = "An error occurred while adding the auth code",
                };
            }
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                model.DivSerialNumber = string.Empty;
                model.SerialNumber = string.Empty;
                model.Quantity = string.Empty;
                model.Description = string.Empty;
                model.LicenseFeatureName = string.Empty;
                model.DivExpirationDate = string.Empty;
                model.Actions = string.Empty;
                model.DeviceName = string.Empty;
                model.DeviceTag = string.Empty;
                model.PartSubTypeName = string.Empty;
            }
            else
            {
                ModelState.AddModelError("AuthorizationCode", result.ErrorMessage);
            }
            return Json(new[] {model}.ToDataSourceResult(new DataSourceRequest(), ModelState));
        }

        //Download End point License Key
				[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult DownloadLicenseKey(string authCode)
        {
            try
            {
	            var supportAccountId = CurrentState.GetCurrentAccountId();

                //var result = CallEndPointLicensing(authCode);
                string licenseKey;
                if (_epLogic.GetLicenseKey(authCode, supportAccountId, out licenseKey))
                {
                    var fileConResult = File(Encoding.UTF8.GetBytes(licenseKey),
                                             "text/plain",
                                             string.Format("{0}.txt", authCode));
                    return fileConResult;
                }
                return null;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                return null;
            }
        }

        //Download End point License Key
				[HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GenerateLicenseKey(string authCode, bool isMalborkLicense = false, string versionRequested = "", int serverCount = 0, int workstationCount = 0, int vdiCount =0)
        {
            return GetEndpointLicenseKey(authCode, isMalborkLicense, versionRequested, serverCount, workstationCount, vdiCount);
        }

        private ActionResult GetEndpointLicenseKey(string authCode, bool isMalborkLicense, string versionRequested, int serverCount, int workstationCount, int vdiCount)
        {
            bool returnVal = false;
            try
            {
                var result = CallEndPointLicensing(authCode, isMalborkLicense, versionRequested, serverCount, workstationCount, vdiCount);
                if (result.HasError == false)
                {
                    returnVal = _epLogic.SaveLicenseKey(authCode, result.LicenseKey);
                }
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
            }
            return Json(returnVal, JsonRequestBehavior.AllowGet);
        }

        // Calls end point service to fetch the license xml.
        // We used to only take authCode as input.
        // But now with Malbork we want Traps to be backwards compatible. So we are taking other inputs that comes from CSP UI.

        private EpLicenseGenerationResult CallEndPointLicensing(string authCode, bool isMalborkLicense, string versionRequested, int serverCount, int workstationCount, int vdiCount)
        {
            // Get the devId
            long supportId = CurrentState.GetCurrentAccountId();
            int iClient, iServer, iVdi, iAgentCount = 0;
            string serialNumber = string.Empty;
            string licenseApiKey = null;
            long stubId = _epLogic.GetStubDeviceId(supportId, authCode, out serialNumber);
            DateTime? expiryDate = _epLogic.GetLicenseExpirationDateByEpStubId(stubId);
            if (!isMalborkLicense) // Client has Starlight
            {
                iAgentCount = 0;
                long? clientLicCount = _epLogic.GetClientLicenseCountByEpStubId(stubId);
                long? serverLicCount = _epLogic.GetServerLicenseCountByEpStubId(stubId);
                long? vdiLicCount = _epLogic.GetVdiLicenseCountByEpStubId(stubId);
                iClient = clientLicCount.HasValue ? Convert.ToInt32(clientLicCount.Value) : 0;
                iServer = serverLicCount.HasValue ? Convert.ToInt32(serverLicCount.Value) : 0;
                iVdi = vdiLicCount.HasValue ? Convert.ToInt32(vdiLicCount.Value) : 0;
            }
            else
            {
                if (versionRequested != "3.3" && versionRequested != "3.4") // Client has Malbork and wants Malbork license
                {
                    iClient = 0;
                    iServer = 0;
                    iVdi = 0;
                    long? agentLicCount = _epLogic.GetAgentLicenseCountByEpStubId(stubId);
                    iAgentCount = agentLicCount.HasValue ? Convert.ToInt32(agentLicCount.Value) : 0;
                    //05/24/2017: This is 4.0.1 feature: comment it out at this time.
                    //licenseApiKey = _epLogic.GetLicenseApiKey(supportId);
                }
                else // Client has Malbork but wants Starlight
                {
                    iClient = workstationCount;
                    iServer = serverCount;
                    iVdi = vdiCount;
                }
            }
            string customer = CurrentState.GetSupportAccountName();
            string wildfireApiKey;
            if (System.Configuration.ConfigurationManager.AppSettings["AppName"] == "CSP DEV")
            {
                wildfireApiKey = System.Guid.NewGuid().ToString();
                if (expiryDate.HasValue == false)
                {
                    expiryDate = DateTime.Now.AddYears(1);
                }
            }
            else
            {
                wildfireApiKey = _licenseLogic.GetEPWildFireApiKey(stubId, supportId, expiryDate ?? System.DateTime.Now);
            }
            var result = _epLogic.GenerateLicense(expiryDate, customer, iVdi, iClient, iServer, iAgentCount, wildfireApiKey,serialNumber, false, licenseApiKey);
            return result;
        }

        //This validates the auth code entered. Checks if it is already being used/ valid ep auth code etc
				[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ValidateAuthCode(string authCode, long? deviceId)
        {
            if (deviceId.HasValue)
            {
                var result = _epLogic.ValidateAuthCode(authCode, CurrentState.GetCurrentAccountId(), deviceId.Value);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (!string.IsNullOrEmpty(authCode) && _trapsELALogic.IsTrapsELAAuthcode(authCode))
            {

                return RedirectToAction("ActivateTrapsELAAuthCode", new { authCode = authCode });
            }
            else
            {
                return RedirectToAction("ValidateEsmAuthCode", new {authCode = authCode});
            }
        }

        //This checks if the auth code is a valid End Point Server Management Console auth code.
				[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ValidateEsmAuthCode(string authCode)
        {
            var result = _epLogic.ValidateEsmAuthCode(authCode, CurrentState.GetCurrentAccountId());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //This shows the window to add a new auth code (license) to ESM (device)
				[HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult AddEndpointLicense(string authCode)
        {
            var model = new AddEpLicensesDto();
            if (!string.IsNullOrEmpty(authCode))
            {
                string serialNumber;
                string deviceName;
                string pandbIdentification;
                long? deviceId = _epLogic.GetEpDeviceInfo(authCode, out serialNumber, out deviceName, out pandbIdentification);
                bool isMalborkDevice = _epLogic.IsTMSMalbork(pandbIdentification);
                if (isMalborkDevice)
                    ViewBag.isMalborkDevice = true;
                else
                    ViewBag.isMalborkDevice = false;
                var licenseses = new List<EpLicenses>();
                if (deviceId.HasValue)
                {
                    licenseses = _epLogic.GetEpLicenses(deviceId.Value);
                }
                model.EsmAuthCode = authCode;
                model.DeviceId = deviceId;
                model.SerialNumber = serialNumber;
                model.DeviceName = deviceName;
                model.Licenses = licenseses;
                model.IsNewEndpointActivation = false;
            }
            else
            {
                model.IsNewEndpointActivation = true;
                ViewBag.isMalborkDevice = false;
            }

            #region Bind Eula to Model

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateEndpointAuthCode;

            #endregion

            return PartialView("../Partial/_AddEpLicense", model);
        }

        //This is called when a new endpoint needs to be activated (esm, bundle, eval etc)
        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult AddNewEpAuthCode(string authCode, int eulaId)
        {
            authCode = Sanitizer.GetSafeHtmlFragment(authCode);
            AddEpAuthCodeResult result = null;
            try
            {
                result = _epLogic.AddEpAuthCode(authCode, CurrentState.GetCurrentAccountId(),
                                       User.UserAccount.Id, User.UserAccount.Email);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new AddEpAuthCodeResult
                {
                    ErrorMessage = "An error occurred while adding the auth code",
                };
            }
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                if (_epLogic.IsAuthCodeEsm(authCode))
                {
                    result.IsEsmAuthCode = true;
                    result.EsmAuthCode = authCode;
                }

                #region Store Eula Agreements

                try
                {
                    var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                    var eulaContextId = EulaContext.ActivateEndpointAuthCode;
                    var signedBy = User.UserAccount.Email;
                    var licenseId = LicenseLogic.GetLicenseByAuthcode(authCode).Id;
                    var deviceId = result.DeviceId.HasValue ? result.DeviceId.Value : 0;
                    var ea = new EulaAgreements()
                    {
                        DeviceId = deviceId,
                        LicenseId = licenseId,
                        SupportAccountId = supportAccountId,
                        EulaContextId = eulaContextId,
                        EulaId = eulaId,
                        SignedBy = signedBy,
                        Notes = string.Format("Authcode entered is {0}",authCode)
                    };

                    _eulaLogic.EndUserAgreements(ea);
                }
                catch (Exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while activating EP Auth Code - {0}. Eula Id Selected is - {1}", authCode, eulaId)));
                }

                #endregion

                result.SuccessMessage = "Successfully Added Authcode";
                result.IsSuccess = true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //This adds the license to ESM
				[HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult AddEndpointLicense(string newAuthCode, string esmAuthCode, int eulaId)
        {

            if (String.IsNullOrEmpty(esmAuthCode))
            {
                return RedirectToAction("AddNewEpAuthCode", new { authCode = newAuthCode, eulaId = eulaId });
            }
            else
            {
                var result = _epLogic.AddEndpointLicense(newAuthCode, esmAuthCode,
                                           CurrentState.GetCurrentAccountId(), User.UserAccount.Id);

                if (string.IsNullOrEmpty(result.ErrorMessage))
                {

                    #region Store Eula Agreements

                    try
                    {
                        var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                        var eulaContextId = EulaContext.ActivateEndpointAuthCode;
                        var signedBy = User.UserAccount.Email;
                        var licenseId = LicenseLogic.GetLicenseByAuthcode(newAuthCode).Id;
                        var deviceId = result.DeviceId.HasValue ? result.DeviceId.Value : 0;
                        var ea = new EulaAgreements()
                        {
                            DeviceId = deviceId,
                            LicenseId = licenseId,
                            SupportAccountId = supportAccountId,
                            EulaContextId = eulaContextId,
                            EulaId = eulaId,
                            SignedBy = signedBy,
                            Notes = string.Format("Authcode entered is {0}", newAuthCode)
                        };

                        _eulaLogic.EndUserAgreements(ea);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while activating EP Auth Code - {0}. Eula Id Selected is - {1}", newAuthCode, eulaId)));
                    }

                    #endregion

                    result.SuccessMessage = "Successfully Added Authcode";
                    result.IsSuccess = true;
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //This shows the licenses for a ESM device. 
        [HttpPost, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetLicensesForEp(long? deviceId, DataSourceRequest dreq)
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
            if (deviceId.HasValue)
            {
                var devices = _epLogic.GetEpLicenses(deviceId.Value);
                DataSourceResult result = devices.ToDataSourceResult(sanitizedRequest);
                return Json(result);
            }
            else
            {
                var devices = new List<EpLicenses>();
                DataSourceResult result = devices.ToDataSourceResult(sanitizedRequest);
                return Json(result);
            }
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult TrapsVersion(string authCode)
        {
            var model = new TrapsVersionDto()
            {
                AuthorizationCode = authCode
            };
            int? workstationCount = null;
            int? serverCount = null;
            int? vdiCount = null;
            int? trapsCount = null;
            bool isSuccess = _epLogic.GetTrapsUseCount(authCode, out workstationCount, out serverCount, out vdiCount, out trapsCount);
            if (isSuccess)
            {
                model.CurrentServerUsecount = serverCount;
                model.CurrentWorkstationUsecount = workstationCount;
                model.CurrentVdiUsecount = vdiCount;

                model.ServerUsecount = serverCount;
                model.WorkstationUsecount = workstationCount;
                model.VdiUsecount = vdiCount;

                model.TotalUsecount = trapsCount ?? 0;

            }
            else
            {
                model.Notifications.Add(new ResponseNotification("Invalid Endpoint License!", ResponseState.Error));
            }
            return PartialView("../Partial/_TrapsVersion", model);
        }

				[CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ActivateTrapsELAAuthCode(string authCode)
        {
            ElaAuthCodeResult result;
            ELACoreAPIResponse elaCoreResponse = new ELACoreAPIResponse(); 
            try
            {
                //result = _trapsELALogic.ActivateTrapsELAAuthCode(authCode, CurrentState.GetCurrentAccountId());
                elaCoreResponse = LicenseLogic.ActivateTrapsELA(authCode, CurrentState.GetCurrentAccountId().ToString(), User.UserAccount.Email);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new ElaAuthCodeResult
                {
                    ErrorMessage = "An error occurred while adding the auth code",
                };
            }

            EpValidationResult validationResult = new EpValidationResult();
            if (!elaCoreResponse.issuccess)
            {
                validationResult.HasError = true;
                validationResult.ErrorMessage = elaCoreResponse.error_message;
            }
            else {
                validationResult.HasSuccess = true;
                validationResult.SuccessMessage = "Successfully Activated TrapsELA Auth-Code";
            }

            return Json(validationResult, JsonRequestBehavior.AllowGet);
        }
    }
}
