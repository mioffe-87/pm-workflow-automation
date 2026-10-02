using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using System.IO;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.Attributes;
using System.Text;
using System.Web;
using Csp.Api.DataAccess;
using Csp.Api.Security;
using Csp.Web.Security.Attributes;
using Csp.Web.HelperClasses;
using Csp.Api.Model;
using Csp.Infrastructure.Data.DBs.CSP;
using Newtonsoft.Json;
using Microsoft.Security.Application;
using System.Web.Script.Serialization;
using Csp.Api.Model.LicenseManagement;

namespace Csp.Web.Controllers
{
    public class LicenseController : BaseController
    {

        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ILicenseTransferLogic _licenseTransferLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly ILicensingApiLogic _licensingApiLogic;
        private readonly IVirtualMachineLogic _virtualMachineLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly ILookupLogic _lookupLogic;
        private readonly ISaasLogic _saasLogic;
        private readonly ILicenseManagementLogic licenseManagementLogic;

        public LicenseController(IUserAccountLogic userAccountLogic,
                                ILicenseTransferLogic licenseTransferLogic,
                                IDeviceLogic deviceLogic,
                                IEulaLogic eulaLogic,
                                ILicenseLogic licenseLogic,
                                ILicensingApiLogic licensingApiLogic,
                                IVirtualMachineLogic virtualMachineLogic,
                                ISiteLicenseLogic siteLicenseLogic,
                                ISupportAccountLogic supportAccountLogic,
                                ILookupLogic lookupLogic,
                                ISaasLogic saasLogic,
                                ILicenseManagementLogic licenseManagementLogic)
        {
            _userAccountLogic = userAccountLogic;
            _licenseTransferLogic = licenseTransferLogic;
            _deviceLogic = deviceLogic;
            _eulaLogic = eulaLogic;
            _licenseLogic = licenseLogic;
            _licensingApiLogic = licensingApiLogic;
            _virtualMachineLogic = virtualMachineLogic;
            _siteLicenseLogic = siteLicenseLogic;
            _supportAccountLogic = supportAccountLogic;
            _lookupLogic = lookupLogic;
            _saasLogic = saasLogic;
            this.licenseManagementLogic = licenseManagementLogic;
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult ActivateAuthCode(string serialNumber, string authorizationCode, int eulaId, int isUpgrade)
        {
            serialNumber = Sanitizer.GetSafeHtmlFragment(serialNumber);
            authorizationCode = Sanitizer.GetSafeHtmlFragment(authorizationCode);
            var model = new DeviceLicensesDto
            {
                ActivateLicense = new ActivateLicense
                {
                    SerialNumber = serialNumber,
                    AuthorizationCode = authorizationCode,
                    UpgradeAuthorizationCode = authorizationCode,
                    IsUpgradeLicense = isUpgrade == 1,
                    Show4HrLocation = false
                },
                Eula = new EulaModel()
                {
                    SelectedEulaId = eulaId
                },
                ActionHelper = new ActionHelper(ControllerFields.LicenseController),
            };

            try
            {
                string authCode = authorizationCode;
                string vmAuthCodeBeforeUpgrade = string.Empty;

                if (_licenseLogic.IsPae(authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification(string.Format("Activation of Prisma Access Edition subscription license must be done in the <a href='{0}' target='_blank'>hub</a>.",
                        System.Configuration.ConfigurationManager.AppSettings["AppPortalURL"]), ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                // 2021-04-26: JK - CSP-6337
                if (_licenseLogic.IsSaasInline(authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification(string.Format("Activation of SaaS Security Inline subscription license must be done in the <a href='{0}' target='_blank'>hub</a>.",
                        System.Configuration.ConfigurationManager.AppSettings["AppPortalURL"]), ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                if (_licenseLogic.IsCloudServiceAuthCode(authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification("Cloud Services authorization codes can only be activated on the Cloud Services page.",
                                ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                //Panorama upgrade with PAN-M-**-P-UPG-**: renewal authcode must be applied first.
                if (_licenseLogic.IsPanoramaUpgradeAuthCodeActivation(serialNumber, authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification("Please activate the Renewal Support license first", ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                // IoT license. CSP-3634 Wrong Error message is shown while activation of IOT Subscription on VM from CSP
                if (_licenseLogic.IsIotAuthCodeActivation(authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification("IoT can only be activated on the Hub. Instructions provided in the subscription confirmation email.", ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                if (_licenseLogic.IsAiOpsAuthCodeActivation(authorizationCode))
                {
                    model.Notifications.Add(new ResponseNotification("AI Ops can only be activated on the Hub. Instructions provided in the subscription confirmation email.", ResponseState.Error));
                    BindEula(model);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                if (!string.IsNullOrEmpty(model.ActivateLicense.SerialNumber) && !string.IsNullOrEmpty(model.ActivateLicense.AuthorizationCode))
                {

                    //Calls stored procedure to get the bundle on he device and the bundle of the license. If they are not equal then it is a Replacement Bundle. 
                    var licenseBundle = LicenseLogic.GetLicenseAuthorizationBundle(serialNumber, authorizationCode);
                    model.ActivateLicense.DeviceBundleModel = licenseBundle.DeviceBundleModel;
                    model.ActivateLicense.LicenseBundleModel = licenseBundle.LicenseBundleModel;

                    var isSiteLicenseResult = LicenseLogic.IsSiteLicense(model.ActivateLicense.AuthorizationCode);
                    if (isSiteLicenseResult.IsSiteLicense)
                    {
                        if (isSiteLicenseResult.IsAlreadyApplied)
                        {
                            model.Notifications.Add(new ResponseNotification("This site license has already been applied",
                                ResponseState.Error));
                            BindEula(model);
                            return Json(model, JsonRequestBehavior.AllowGet);
                        }
                        if (isSiteLicenseResult.IsActive == false)
                        {
                            model.Notifications.Add(new ResponseNotification("This site license is inactive",
                                ResponseState.Error));
                            BindEula(model);
                            return Json(model, JsonRequestBehavior.AllowGet);
                        }
                    }
                    model.ActivateLicense.IsSiteLicense = isSiteLicenseResult.IsSiteLicense;
                    model.ActivateLicense.Is4HourRma = LicenseLogic.Is4HourRma(model.ActivateLicense.AuthorizationCode);

                    if (model.ActivateLicense.Is4HourRma)
                    {

                        //Populate the city and postal code to be used in the 4 Hour RMA notification.
                        var license = LicenseLogic.GetLicenseByAuthcode(model.ActivateLicense.AuthorizationCode);
                        model.ActivateLicense.City = license.City ?? "";
                        model.ActivateLicense.PostalCode = license.PostalCode ?? "";
                        model.ActivateLicense.BillToCountryId = license.BillToCountryId;
                        model.ActivateLicense.CountryId = license.CountryId;
                        model.ActivateLicense.Country = _lookupLogic.GetCountryNameById(license.CountryId);
                        var deviceID = _deviceLogic.GetDeviceIdBySerialNumber(serialNumber);
                        var device = _deviceLogic.GetDevice(deviceID, User.UserAccount.Id);
                        model.Device = device;
                        model.DeviceId = deviceID;
                        if (license.CountryId != device.CountryId || license.City != device.City || license.PostalCode != device.PostalCode)
                        {
                            model.ActivateLicense.Show4HrLocation = true;
                        }

                    }

                    /* Only activate the license if it is not a site license, not 4 hour RMA and not a replacement bundle. 
                     * In any of those cases we want to return the _ActivateLicense with the updated model. The _ActivateLicense 
                     * will check if any of the conditions are true and display a continuation pop up to display a confirmation message
                     * to the user.
                     */
                    if (!model.ActivateLicense.IsSiteLicense && !model.ActivateLicense.Is4HourRma && !model.ActivateLicense.IsReplacementBundle)
                    {

                        if (isUpgrade == 1)
                        {
                            vmAuthCodeBeforeUpgrade = _virtualMachineLogic.GetVmAuthCodeBeforeUpgrade(EVmType.Vm, serialNumber);
                        }

                        var licenseResult = LicenseLogic.ActivateLicense(serialNumber, authorizationCode: authorizationCode);

                        //Clear the model state so nothing is preserved in the view. All values needed are populated in the model
                        ModelState.Clear();

                        if (licenseResult == "")
                        {

                            //Store Eula Agreements
                            #region Eula Agreements
                            try
                            {
                                var eulaContextId = EulaContext.ActivateLicense;
                                var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                                var signedBy = User.UserAccount.Email;
                                var deviceId = _deviceLogic.GetDeviceIdBySerialNumber(serialNumber);
                                var licenseId = LicenseLogic.GetLicenseByAuthcode(authorizationCode).Id;

                                var ea = new EulaAgreements()
                                {
                                    DeviceId = deviceId,
                                    LicenseId = licenseId,
                                    SupportAccountId = supportAccountId,
                                    EulaContextId = eulaContextId,
                                    EulaId = eulaId,
                                    SignedBy = signedBy,
                                    Notes = string.Format("Authcode entered is {0}", authCode)
                                };

                                _eulaLogic.EndUserAgreements(ea);
                            }
                            catch (Exception)
                            {
                                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while activating AuthCode {0}. Eula Id Selected is - {1}", authorizationCode, eulaId)));
                            }
                            #endregion
                            //If there is no error message from ActivateLicense, then clear the Authorization Code so it does not display to the user and they can enter a new auth code
                            model.ActivateLicense.AuthorizationCode = null;
                            model.ActivateLicense.ActivateSuccessful = true;

                            if (model.ActivateLicense.IsUpgradeLicense)
                            {
                                model.Notifications.Add(new ResponseNotification("License successfully activated. VM upgrade is In-Progress", ResponseState.Information));
                            }
                            else
                            {
                                model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));
                            }

                            #region Store Upgrade AuthCodes
                            /*
                         * NOTE: Since Licensing is doing this we dont need this anymore
                         * 
                        if (isUpgrade == 1)
                        {
                            string upgradeFrom, upgradeTo, transactionId, vmAuthCodeAfterUpgrade;
                            long deviceId;
                            _virtualMachineLogic.GetVmUpgradeModels(serialNumber, authorizationCode,out deviceId, out vmAuthCodeAfterUpgrade, out upgradeFrom, out upgradeTo, out transactionId);
                            _virtualMachineLogic.StoreVmUpgrade(deviceId, transactionId, upgradeFrom, upgradeTo,
                                vmAuthCodeBeforeUpgrade, vmAuthCodeAfterUpgrade, User.UserAccount.Email);
                        }
                        */
                            #endregion

                        }
                        else
                        {

                            model.Notifications.Add(new ResponseNotification(licenseResult, ResponseState.Error));

                        }
                    }
                }
                #region Bind Eula to Model
                BindEula(model);
                #endregion
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                model.Notifications.Add(new ResponseNotification("An error occurred while processing your request. Please contact customer support.", ResponseState.Error));
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void BindEula(DeviceLicensesDto model)
        {
            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateLicense;
        }

        [AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult ContinueLicenseActivation(string licType, string serialNumber, string authorizationCode,
                                                      int eulaId, bool update4HRLocation)
        {
            authorizationCode = Sanitizer.GetSafeHtmlFragment(authorizationCode);
            var model = new DeviceLicensesDto
            {
                ActivateLicense = new ActivateLicense
                {
                    SerialNumber = serialNumber,
                    AuthorizationCode = authorizationCode,
                    Show4HrLocation = false
                },
                Eula = new EulaModel()
                {
                    SelectedEulaId = eulaId
                },
                ActionHelper = new ActionHelper(ControllerFields.LicenseController),
            };

            bool isLicenseActivated = false;

            if (licType == "SiteLicense")
            {

                //Get the support account id as the id is used to idenfity the company that the device is registered under
                var supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(serialNumber);

                /* 
                 * The boxes in the field have a dependency on direct calls to the licenseing web service. Additionally the CSP has a different architecture 
                 * from PAN-DB. In the CSP a device is directly tied to the company (support account) and users can belong to more than one company (support account).
                 * For this reason the company can no longer be determined by the pandb.accounts table. The resolution was to remove grant site license
                 * and activate site license from the web service and add it to the CSP. The code is now included in this project and the calls for grant site license
                 * and activate site license should be done internally rather than the web service. The call is done with a version of the LicenseMgr as provided by Frank Fang.
                 * 
                 */
                var siteLicenseResult = LicenseLogic.ActivateSiteLicense(supportAccountId, authorizationCode);
                var grantSiteLicense = false;
                if (siteLicenseResult[0].errmsg == null)
                    grantSiteLicense = LicenseLogic.GrantSiteLicenses(supportAccountId);


                //If there is no error message and we were able to grant the site license then the activation was a success
                if (siteLicenseResult[0].errmsg == null && grantSiteLicense)
                {

                    model.ActivateLicense.AuthorizationCode = null;
                    model.ActivateLicense.ActivateSuccessful = true;
                    model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));
                    isLicenseActivated = true;
                }
                //displays the appropriate message depending on the failure for which call has failed. 
                else if (siteLicenseResult[0].errmsg != null)
                {

                    model.Notifications.Add(new ResponseNotification(siteLicenseResult[0].errmsg, ResponseState.Error));

                }
                else if (!grantSiteLicense)
                {

                    model.Notifications.Add(new ResponseNotification("An unknown error occured while granting site licenses.", ResponseState.Error));

                }

            }
            else
            {
                if (update4HRLocation == true)
                {
                    var license = LicenseLogic.GetLicenseByAuthcode(authorizationCode);
                    var device4HRCountryCode = license.CountryId > 0 ? _lookupLogic.GetCountryCodeById(license.CountryId) : "";

                    if (!_licenseLogic.UpdateDeviceLocationFor4HrRma(serialNumber, license.City, license.PostalCode, device4HRCountryCode))
                        model.Notifications.Add(new ResponseNotification("Unable to Update 4HR Location for Authorization Code.", ResponseState.Error));
                }
                //If the license is not a site license then the normal web service should be called to activate the authorization code.

                var licenseResult = LicenseLogic.ActivateLicense(model.ActivateLicense.SerialNumber, authorizationCode: model.ActivateLicense.AuthorizationCode);

                if (licenseResult == "")
                {
                    model.ActivateLicense.AuthorizationCode = null;
                    model.ActivateLicense.ActivateSuccessful = true;
                    model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));
                    isLicenseActivated = true;
                }
                else
                {
                    model.Notifications.Add(new ResponseNotification(licenseResult, ResponseState.Error));

                }
                ModelState.Clear();
            }
            if (isLicenseActivated)
            {
                //Store Eula Agreements
                #region Eula Agreements
                try
                {
                    var eulaContextId = EulaContext.ActivateLicense;
                    var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
                    var signedBy = User.UserAccount.Email;
                    var deviceId = _deviceLogic.GetDeviceIdBySerialNumber(serialNumber);
                    var licenseId = LicenseLogic.GetLicenseByAuthcode(authorizationCode).Id;

                    var ea = new EulaAgreements()
                    {
                        DeviceId = deviceId,
                        LicenseId = licenseId,
                        SupportAccountId = supportAccountId,
                        EulaContextId = eulaContextId,
                        EulaId = eulaId,
                        SignedBy = signedBy,
                        Notes = string.Format("Authcode entered is {0}", authorizationCode)
                    };

                    _eulaLogic.EndUserAgreements(ea);

                }
                catch (Exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while activating AuthCode {0}. Eula Id Selected is - {1}", authorizationCode, eulaId)));
                }
                #endregion
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// This method is called if the user selects the option to continue the license activation in the cases of 
        /// 4 hour RMA, Site Licenses or Replacement Bundles.
        /// </summary>
        /// <param name="activateLicense"></param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDevice)]
        public ActionResult ContinueActivateLicense(ActivateLicense activateLicense)
        {

            var model = new DeviceLicensesDto
            {
                ActivateLicense = activateLicense,
            };

            model.ActionHelper = new ActionHelper(ControllerFields.LicenseController);

            if (model.ActivateLicense.IsSiteLicense)
            {

                //Get the support account id as the id is used to idenfity the company that the device is registered under
                var supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(model.ActivateLicense.SerialNumber);

                /* 
                 * The boxes in the field have a dependency on direct calls to the licenseing web service. Additionally the CSP has a different architecture 
                 * from PAN-DB. In the CSP a device is directly tied to the company (support account) and users can belong to more than one company (support account).
                 * For this reason the company can no longer be determined by the pandb.accounts table. The resolution was to remove grant site license
                 * and activate site license from the web service and add it to the CSP. The code is now included in this project and the calls for grant site license
                 * and activate site license should be done internally rather than the web service. The call is done with a version of the LicenseMgr as provided by Frank Fang.
                 * 
                 */
                var siteLicenseResult = LicenseLogic.ActivateSiteLicense(supportAccountId, model.ActivateLicense.AuthorizationCode);
                var grantSiteLicense = false;
                if (siteLicenseResult[0].errmsg == null)
                    grantSiteLicense = LicenseLogic.GrantSiteLicenses(supportAccountId);

                //Clears the model state to ensure only the values in the model is displayed
                ModelState.Clear();
                model.ActivateLicense.IsSiteLicense = false;

                //If there is no error message and we were able to grant the site license then the activation was a success
                if (siteLicenseResult[0].errmsg == null && grantSiteLicense)
                {

                    model.ActivateLicense.AuthorizationCode = null;
                    model.ActivateLicense.ActivateSuccessful = true;
                    model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));

                }
                //displays the appropriate message depending on the failure for which call has failed. 
                else if (siteLicenseResult[0].errmsg != null)
                {

                    model.Notifications.Add(new ResponseNotification(siteLicenseResult[0].errmsg, ResponseState.Error));

                }
                else if (!grantSiteLicense)
                {

                    model.Notifications.Add(new ResponseNotification("An unknown error occured while granting site licenses.", ResponseState.Error));

                }

            }
            else
            {
                //If the license is not a site license then the normal web service should be called to activate the authorization code.

                var licenseResult = LicenseLogic.ActivateLicense(activateLicense.SerialNumber, authorizationCode: activateLicense.AuthorizationCode);

                if (licenseResult == "")
                {
                    model.ActivateLicense.AuthorizationCode = null;
                    model.ActivateLicense.ActivateSuccessful = true;
                    model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));

                }
                else
                {
                    model.Notifications.Add(new ResponseNotification(licenseResult, ResponseState.Error));

                }
                ModelState.Clear();
            }
            return PartialView("../Partial/_ActivateLicense", model);
        }

        private bool? ActivatePanoramaMgmtLicese(string serialNumber, string panoramaVmRegInfo, string email)
        {
            bool? result = null;
           
            try
            {
                var vmRegistrionDto = JsonConvert.DeserializeObject<RegisterVmDto>(panoramaVmRegInfo, new JsonSerializerSettings()
                {
                    MaxDepth = 32,
                    TypeNameHandling = TypeNameHandling.None
                });
                string[] version = !string.IsNullOrEmpty(vmRegistrionDto.PanOsVersion) ? vmRegistrionDto.PanOsVersion.Split('.') : new string[] { };

                // below 8.1: just update the PanOS
                if (version.Length > 0 && (Convert.ToInt32(version[0]) < 8 ||
                    (Convert.ToInt32(version[0]) == 8 && Convert.ToInt32(version[1]) < 1)))
                {
                    var deviceDataAccess = new DeviceDataAccess();
                    deviceDataAccess.UpdatePANDBDevicePanOsVersion(serialNumber, email, vmRegistrionDto.PanOsVersion);
                    result = true;
                }
                else
                {
                    var resultArray = LicenseLogic.LicenseIt(serialNumber,
                        string.IsNullOrEmpty(vmRegistrionDto.CpuId) ? string.Empty : vmRegistrionDto.CpuId,
                        string.IsNullOrEmpty(vmRegistrionDto.UuId) ? string.Empty : vmRegistrionDto.UuId,
                        string.IsNullOrEmpty(vmRegistrionDto.AuthCode) ? string.Empty : vmRegistrionDto.AuthCode,
                        string.IsNullOrEmpty(vmRegistrionDto.HyperVisorVersion) ? string.Empty : vmRegistrionDto.HyperVisorVersion,
                        string.IsNullOrEmpty(vmRegistrionDto.PanOsVersion) ? string.Empty : vmRegistrionDto.PanOsVersion);
                    result = true;
                    if (resultArray.Any(message => message.errmsg != null))
                    {
                        result = false;
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + resultArray.First().errmsg));
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + e.Message));
            }

            return result;
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult ActivateTrialLicenseJson(string selectedLicenses, string mode, string serialNumber, int? eulaid, string panoramaVmRegInfo)
        {

            /*
             * much of this code is from the original offshore development and has not been changed much. 
             * In the long run it would be better to refactor this code to work in the same manner as the other code
             * using a model across the board for all values (including the selected licenses) with standard validation.
             * TECHDEBT: refactoring is of high priority. The code is convoluted and logically wrong. E.g. LicenseFeatureName.Dpm is sent regardless of whether activation succeeds or fails.
             */

            List<string> iots = new List<string>(new string[] { "IOTSecurity3rd", "IOTSecurityDataLake", "IOTSecurity" });

            var model = new ActivateTrialLicenseJsonDto
            {
                IsSuccess = false,
            };
            serialNumber = Sanitizer.GetSafeHtmlFragment(serialNumber);

            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            if (string.IsNullOrEmpty(selectedLicenses))
            {
                const string message = "Please select atleast one license to activate";
                model.Notifications.Add(new ResponseNotification(message, ResponseState.Error));
                ModelState.AddModelError("SelectedLicenses", message);
            }

            if (!string.IsNullOrEmpty(selectedLicenses) && !string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(serialNumber))
            {
                //Use nullable bool for result so that if any activation fails, it reports failure
                bool? result = null;
                bool hasMagicLink = false;
                var licenseList = selectedLicenses.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

                //Panorama License Activation
                if (!string.IsNullOrEmpty(panoramaVmRegInfo))
                {
                    result = ActivatePanoramaMgmtLicese(serialNumber, panoramaVmRegInfo, User.UserAccount.Email);
                }
                else
                {
                    const int evalDaysStandard30 = 30;
                    const int evalDaysInfinite = 0;

                    //If there is a DPM license, then move it to the end of the list (b/c for NFR lics you must do a plain lic first because the first plain lic will wipe all others)
                    licenseList = MoveDpmToLastPlace(licenseList);

                    /* 
                     * This same action method is called to activate both a trail license and an evaluation licenses. 
                     */
                    if (mode == "Trial")
                    {
                        foreach (var license in licenseList)
                        {
                            var standardTrialEvalDays = 30;
                            switch (license)
                            {
                                case "DNS":
                                case "SDWAN":
                                case "Gatewaytrial":
                                case "AWF":
                                case "ATP":
                                case "AIOps":
                                    standardTrialEvalDays = 90;
                                    break;
                                case "SaaSInline":
                                    standardTrialEvalDays = 60;
                                    break;
                                default:
                                    standardTrialEvalDays = 30;
                                    break;
                            }
                            if (license == "") continue;

                            var trialEvalDays = (license != LicenseFeatureName.Dpm.ToDescription() && license != LicenseFeatureName.Dpb.ToDescription())
                                ? standardTrialEvalDays
                                : evalDaysInfinite;

                            if (iots.Contains(license)  // Magic link cases
                                || license == "SaaSInline"
                                || license == "AIOps")
                            {
                                hasMagicLink = true;
                            }
                            else if (license == "DLP" && _deviceLogic.IsNgfwDlpDevice(serialNumber)) // NFR device
                            {
                                result = (result ?? true) && LicenseLogic.GrantTrial(serialNumber, license, trialEvalDays, "", "", User.UserAccount.Email);
                            }
                            else
                            {
                                //Grants the trial license
                                result = (result ?? true) && LicenseLogic.GrantTrial(serialNumber, license, trialEvalDays, "", "", User.UserAccount.Email);
                            }
                        }
                    }
                    else
                    {
                        var isFirstRemoveAllLicenses = true;
                        var nvrsvcExp = LicenseLogic.GetNFRSVCExpirationFromPandb(serialNumber);

                        foreach (var license in licenseList)
                        {

                            if (license == "") continue;

                            // 09/21/2017: all NRF models are eligible for DPM/DPB now.
                            //Grant the eval license for NFR Devices
                            var isDpmNfrBnd = false;
                            bool is90DayTrial = license.Contains(LicenseFeatureName.Dns.ToDescription()) || license.Contains(LicenseFeatureName.SdWan.ToDescription());
                            if (license.Contains(LicenseFeatureName.Dpm.ToDescription()) ||
                                license.Contains(LicenseFeatureName.Dpb.ToDescription())

                                )
                            {
                                result = (result ?? true) &&
                                    LicenseLogic.GrantTrial(serialNumber, license, evalDaysInfinite, "", "", User.UserAccount.Email);
                            }
                            else
                            {
                                result = (result ?? true) && (
                                             isDpmNfrBnd ? LicenseLogic.GrantTrial(serialNumber, license, evalDaysInfinite, "", "", User.UserAccount.Email)
                                             : LicenseLogic.GrantNFREval(serialNumber, license, is90DayTrial ? 90 : evalDaysStandard30, nvrsvcExp, isFirstRemoveAllLicenses)
                                         );
                            }

                            isFirstRemoveAllLicenses = false;
                        }
                    }

                    if (result == true && (licenseList.Contains(LicenseFeatureName.Dpm.ToDescription()) || licenseList.Contains(LicenseFeatureName.Dpb.ToDescription())))
                    {

                        // Send DPM Activation Email
                        var userEmails = _userAccountLogic.GetAdminUserAccountsBySupportAccountId(supportAccountId).Select(u => u.Email).ToList();
                        var emails = string.Join(",", userEmails);
                        if (licenseList.Contains(LicenseFeatureName.Dpm.ToDescription()))
                            EmailHelper.SendDpmFeatureActivationEmail(User.UserAccount.Email, emails, serialNumber);
                        if (licenseList.Contains(LicenseFeatureName.Dpb.ToDescription()))
                            EmailHelper.SendDpbFeatureActivationEmail(User.UserAccount.Email, emails, serialNumber);
                    }
                }

                ModelState.Clear();

                if (hasMagicLink
                    || (result ?? false))
                {
                    // Create License Agreements
                    #region Store Eula Agreements
                    if (eulaid.HasValue)
                    {
                        try
                        {
                            var partNumbers = new List<string>();
                            // Fetch the License Ids
                            foreach (var license in licenseList)
                            {
                                partNumbers.Add(LicenseLogic.GetPartIdByTrialFeature(license));
                            }
                            var licenseIds = LicenseLogic.GetActivatedTrialLicenseId(serialNumber, partNumbers);
                            var deviceId = _deviceLogic.GetDeviceIdBySerialNumber(serialNumber);
                            foreach (var licenseId in licenseIds)
                            {
                                _eulaLogic.EndUserAgreements(new EulaAgreements()
                                {
                                    DeviceId = deviceId,
                                    LicenseId = licenseId,
                                    SupportAccountId = supportAccountId,
                                    EulaContextId = EulaContext.ActivateLicense,
                                    SignedBy = User.UserAccount.Email,
                                    EulaId = eulaid.Value
                                });
                            }
                        }
                        catch (Exception)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements for Trial Licenses. Device Serial Number is {0}. Eula Id Selected is - {1}", serialNumber, eulaid)));
                        }
                    }
                    #endregion

                    if (hasMagicLink)
                    {
                        int iotCount = licenseList.Where(p => p.Contains("IOT")).Count();
                        string firstIOTLicenseType = string.Empty;
                        string combinedMessageText = string.Empty;
                        foreach (var license in licenseList)
                        {
                            string licenseType = string.Empty;
                            string messageText = string.Empty;

                            try
                            {
                                // CSP-3027 CSP Initiated 30 day IOT trial radial button
                                if (license == LicenseFeatureName.IOTSecurity.ToDescription())
                                {
                                    licenseType = "IOTSecurity";
                                    messageText = "IoT security";
                                }
                                else if (license == LicenseFeatureName.IOTSecurityDataLake.ToDescription())
                                {
                                    licenseType = "IOTSecurityDataLake";
                                    messageText = "IoT security - Requires Cortex Data Lake";
                                }
                                else if (license == LicenseFeatureName.IOTSecurity3rd.ToDescription())
                                {
                                    licenseType = "IOTSecurity3rd";
                                    messageText = "IoT security - 3rd Party Integrations add-on";
                                }
                                else if (license == LicenseFeatureName.SaaSInline.ToDescription())
                                {
                                    licenseType = "SAASINLINE";
                                    messageText = "SaaS Security Inline";
                                }
                                else if (license == LicenseFeatureName.AiOps.ToDescription())
                                {
                                    licenseType = "AIOPS";
                                    messageText = "AI Ops";
                                }
                                else
                                    continue;

                                //If the user selected 2 IOT packages(checkboxes) send one email and one magic link
                                if (iotCount > 1 && license.Contains("IOT") && firstIOTLicenseType == "")
                                {
                                    //store the 1st package licenseType, skip to the next item in the loop
                                    firstIOTLicenseType = licenseType;
                                    combinedMessageText = messageText;
                                    continue;
                                }
                                else if (iotCount > 1 && license.Contains("IOT") && firstIOTLicenseType != null)
                                {
                                    //concat the 1st & 2nd IOT license type and messageTexts
                                    licenseType = firstIOTLicenseType + "," + licenseType;
                                    combinedMessageText += " and " + messageText;
                                    messageText = combinedMessageText;
                                }

                                string successMessage = "Your request to activate the " + messageText +
                                                        " trial has been received. Please check your email to activate.";
                                string errorEmailMessage = "Failed to email the " + messageText +
                                                           " Activation link. Please contact customer support.";
                                string errorGenerateMessage = "Failed to generate " + messageText +
                                                              " Activation link. Please contact customer support.";

                                IEnumerable<ActivationLinkModel> activeLink =
                                    _deviceLogic.GetTrialLicenseActivationActiveLinksBySupportAccountId(
                                        supportAccountId, licenseType, serialNumber);
                                if (activeLink != null)
                                {
                                    if (activeLink != null)
                                    {
                                        if (EmailHelper.SendTrialActivationEmail(User.UserAccount.Email,
                                            User.UserAccount.FirstName + " " + User.UserAccount.LastName, messageText,
                                            activeLink))
                                        {
                                            model.Notifications.Add(new ResponseNotification(successMessage,
                                                ResponseState.Success));
                                        }
                                        else
                                        {
                                            model.Notifications.Add(new ResponseNotification(errorEmailMessage,
                                                ResponseState.Error));
                                        }
                                    }
                                    else
                                    {
                                        model.Notifications.Add(new ResponseNotification(errorGenerateMessage,
                                            ResponseState.Error));
                                    }
                                }
                                else
                                {
                                    model.Notifications.Add(new ResponseNotification(
                                        "Failed to get Activation Link. Please contact customer support.",
                                        ResponseState.Error));
                                }
                            }
                            catch (Exception ex)
                            {
                                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                                model.Notifications.Add(new ResponseNotification(
                                    "Failed to process Activation Link. Please contact customer support.",
                                    ResponseState.Error));
                            }
                        }
                    }

                    if (result ?? false)
                    {
                        model.Notifications.Add(new ResponseNotification("Your selected licenses have been successfully activated.", ResponseState.Success));
                    }

                    model.IsSuccess = true;
                    string partNumber, modelName;
                    LicenseLogic.GetPartNumberAndModel(serialNumber, out partNumber, out modelName);
                    bool isNFR = partNumber.Contains("-NFR");
                    bool hasNfrSvcOrBndLic = false;
                    if (isNFR)
                        hasNfrSvcOrBndLic = LicenseLogic.HasNfrSvcOrBndLic(serialNumber, partNumber);

                    //Panorama
                    var isPanorama = partNumber.StartsWith("PAN-PRA");
                    if (isPanorama && !string.IsNullOrEmpty(modelName))
                    {
                        //modelName: PRA-1000, PRA-25, PRA-100
                        var panoramaSku = string.Format("PAN-P-{0}", modelName.Substring(4));
                        model.VMRegistrationDto = new RegisterVmDto();
                        model.IsEligibleForPanoramaLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.PanoramLicense_Feature, panoramaSku,
                            serialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);
                    }

                    var isCsspAccount = _supportAccountLogic.IsCsspAccount(supportAccountId);
                    bool isCloudDevice = (!string.IsNullOrEmpty(partNumber) && partNumber.ToUpper().Contains("CLOUD"));
                    if (isCloudDevice && modelName == "VM-300")
                    {
                        //Exclude VM300 Cloud SKU
                        model.IsEligibleForDpmLicense = false;
                    }
                    else if (!isCsspAccount) //CSSP account should not be able to enable DPM feature
                    {
                        model.IsEligibleForDpmLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.DecryptionPortMirror_Feature, FeatureAccess.DecryptionPortMirror_SKU,
                            serialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);
                    }

                    if (!isCsspAccount) //CSSP account should not be able to enable DPB feature
                    {
                        model.IsEligibleForDpbLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.NetworkPacketBroker_Feature, FeatureAccess.SSLDecryptionBroker_SKU,
                            serialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);
                    }
                    model.AvailableLicenses = LicenseLogic.GetTrialLicenseListBySerialNumber(serialNumber, mode);
                    return Json(model, JsonRequestBehavior.AllowGet);
                }

                model.Notifications.Add(new ResponseNotification("Failed to activate your selected licenses.", ResponseState.Error));
            }

            string partNumber1, modelName1;
            LicenseLogic.GetPartNumberAndModel(serialNumber, out partNumber1, out modelName1);
            bool isNFR1 = partNumber1.Contains("-NFR");
            bool hasNfrSvcOrBndLic1 = false;
            if (isNFR1)
                hasNfrSvcOrBndLic1 = LicenseLogic.HasNfrSvcOrBndLic(serialNumber, partNumber1);

            //Panorama
            var isPanorama1 = partNumber1.StartsWith("PAN-PRA");
            if (isPanorama1 && !string.IsNullOrEmpty(modelName1))
            {
                //modelName: PRA-1000, PRA-25, PRA-100
                var panoramaSku = string.Format("PAN-P-{0}", modelName1.Substring(4));
                model.VMRegistrationDto = new RegisterVmDto();
                model.IsEligibleForPanoramaLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.PanoramLicense_Feature, panoramaSku,
                    serialNumber, modelName1, User.UserAccount.Email, supportAccountId, isNFR1, hasNfrSvcOrBndLic1);
            }

            var isCloudDevice1 = (!string.IsNullOrEmpty(partNumber1) && partNumber1.ToUpper().Contains("CLOUD"));
            var isCsspAccount1 = _supportAccountLogic.IsCsspAccount(supportAccountId);
            if (isCloudDevice1 && modelName1 == "VM-300")
            {
                //Exclude VM300 Cloud SKU
                model.IsEligibleForDpmLicense = false;
            }
            else if (!isCsspAccount1) //CSSP account should not be able to enable DPM feature
            {
                model.IsEligibleForDpmLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.DecryptionPortMirror_Feature, FeatureAccess.DecryptionPortMirror_SKU,
                    serialNumber, modelName1, User.UserAccount.Email, supportAccountId, isNFR1, hasNfrSvcOrBndLic1);
            }

            if (!isCsspAccount1) //CSSP account should not be able to enable DPB feature
            {
                model.IsEligibleForDpbLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.NetworkPacketBroker_Feature, FeatureAccess.SSLDecryptionBroker_SKU,
                    serialNumber, modelName1, User.UserAccount.Email, supportAccountId, isNFR1, hasNfrSvcOrBndLic1);
            }
            model.AvailableLicenses = LicenseLogic.GetTrialLicenseListBySerialNumber(serialNumber, mode);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private string[] MoveDpmToLastPlace(string[] licenseList)
        {
            if (licenseList.Length <= 1 || !licenseList.Any(l => l.Contains(LicenseFeatureName.Dpm.ToDescription())))
                return licenseList;

            for (var i = 0; i < licenseList.Length - 1; i++)
            {
                if (licenseList[i].Contains(LicenseFeatureName.Dpm.ToDescription()))
                {
                    //Remove and add to back of list
                    var swap = licenseList[i];
                    licenseList[i] = licenseList[licenseList.Length - 1];
                    licenseList[licenseList.Length - 1] = swap;
                    break;
                }
            }

            return licenseList;
        }

        public class ActivateTrialLicenseJsonDto : ActivateTrialLicenseDto
        {
            public bool IsSuccess { get; set; }
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult ActivateSiteLicense(long id)
        {
            var model = new ActivateSiteLicenseViewModel
            {
                Eula = new EulaModel
                {
                    Languages = _eulaLogic.GetEulaLanguages(),
                    Eulas = _eulaLogic.GetEula(),
                    DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                    SelectedLanguage = EulaLanguage.English
                }
            };

            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateLicense;
            return PartialView("../Partial/_ActivateSiteLicense", model);
        }

        [HttpPost, AjaxOnly, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult ActivateSiteLicense(ActivateSiteLicenseViewModel model)
        {

            model.Eula = new EulaModel
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateLicense;
            model.ApiKey = string.Empty;

            var message = LicenseLogic.IsSiteLicenseAuthorizationCodeValid(model.AuthCode);
            if (!ModelState.IsValid || !string.IsNullOrEmpty(message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    model.Notifications.Add(new ResponseNotification(message, ResponseState.Warning));
                }
                return PartialView("../Partial/_ActivateSiteLicense", model);
            }


            //Get the support account id as the id is used to idenfity the company that the device is registered under
            var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
            if (supportAccountId == 0)
            {
                throw new InvalidEnumArgumentException();
            }

            var eulaId = model.Eula.SelectedEulaId;


            /* 
                * The boxes in the field have a dependency on direct calls to the licenseing web service. Additionally the CSP has a different architecture 
                * from PAN-DB. In the CSP a device is directly tied to the company (support account) and users can belong to more than one company (support account).
                * For this reason the company can no longer be determined by the pandb.accounts table. The resolution was to remove grant site license
                * and activate site license from the web service and add it to the CSP. The code is now included in this project and the calls for grant site license
                * and activate site license should be done internally rather than the web service. The call is done with a version of the LicenseMgr as provided by Frank Fang.
                * 
                */
            var siteLicenseResult = LicenseLogic.ActivateSiteLicense(supportAccountId, model.AuthCode);
            var grantSiteLicense = false;
            if (siteLicenseResult[0].errmsg == null)
                grantSiteLicense = LicenseLogic.GrantSiteLicenses(supportAccountId);


            //If there is no error message and we were able to grant the site license then the activation was a success
            if (siteLicenseResult[0].errmsg == null && grantSiteLicense)
            {
                //try to enable the API Key, do not need to check the result.
                long lfid;
                if (!string.IsNullOrEmpty(siteLicenseResult[0].lfid) && Int64.TryParse(siteLicenseResult[0].lfid, out lfid))
                {
                    var enableResult = _siteLicenseLogic.EnableOrGenerateAutofocusApiKey(supportAccountId, User.UserAccount.Id, lfid);
                    if (enableResult.IsSuccess)
                    {
                        model.ApiKey = enableResult.ApiKey;
                    }
                }

                model.AuthCode = null;
                model.Notifications.Add(new ResponseNotification("License successfully activated.", ResponseState.Success));

                //Store Eula Agreements
                #region Eula Agreements
                try
                {
                    var eulaContextId = EulaContext.ActivateLicense;
                    var signedBy = User.UserAccount.Email;
                    var licenseId = LicenseLogic.GetLicenseByAuthcode(model.AuthCode).Id;

                    var ea = new EulaAgreements()
                    {
                        DeviceId = null,
                        LicenseId = licenseId,
                        SupportAccountId = supportAccountId,
                        EulaContextId = eulaContextId,
                        EulaId = eulaId,
                        SignedBy = signedBy,
                        Notes = string.Format("Authcode entered is {0}", model.AuthCode)
                    };

                    _eulaLogic.EndUserAgreements(ea);

                }
                catch (Exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Could not add EULA Agreements while activating AuthCode {0}. Eula Id Selected is - {1}", model.AuthCode, eulaId)));
                }
                #endregion
            }
            //displays the appropriate message depending on the failure for which call has failed. 
            else if (siteLicenseResult[0].errmsg != null)
            {

                model.Notifications.Add(new ResponseNotification(siteLicenseResult[0].errmsg, ResponseState.Error));

            }
            else if (!grantSiteLicense)
            {

                model.Notifications.Add(new ResponseNotification("An unknown error occured while granting site licenses.", ResponseState.Error));

            }


            return PartialView("../Partial/_ActivateSiteLicense", model);
        }


        [HttpGet, CspAuthorize(Permission.EditDevice)]
        public FileStreamResult DownloadLicenseKey(long licenseId)
        {

            var licenseKey = LicenseLogic.GetLicenseKeybyId(licenseId, User.UserAccount.Id);

            licenseKey.Key = licenseKey.Key.Replace("\n", Environment.NewLine);

            var byteArray = Encoding.ASCII.GetBytes(licenseKey.Key);
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/plain", String.Format(licenseKey.Name));
        }


        [HttpGet, CspAuthorize(Permission.TransferLicenses)]
        public ActionResult Transfer(long deviceId, bool? isSource)
        {

            var device = _deviceLogic.GetDevice(deviceId, User.UserAccount.Id);

            var model = new TransferLicenseDto
            {
                PreSelectedDevice = new TransferLicenseAvailableDevice(device),
                Mode = TransferLicenseDto.PostBackMode.Transfer,
            };

            var isSpare =
                isSource.HasValue
                ? isSource.Value == false
                : DeviceLogic.IsModelNameSpare(device.ModelName);

            model.State = isSpare
                                            ? TransferLicenseDto.InitialState.HasDest
                                            : TransferLicenseDto.InitialState.HasSrc;

            model.AllowSwap = model.State == TransferLicenseDto.InitialState.HasDest
                            ? _licenseTransferLogic.IsValidSourceForTransfer(deviceId, User.UserAccount.Id, CurrentState.GetCurrentAccountId())
                            : _licenseTransferLogic.IsValidDestinationForTransfer(deviceId, User.UserAccount.Id, CurrentState.GetCurrentAccountId());

            switch (model.State)
            {

                case TransferLicenseDto.InitialState.HasDest:
                    model.AvailableDevices = _licenseTransferLogic.GetAvailableSrcDevices(deviceId, User.UserAccount.Id,
                                                                                                                                                                     CurrentState.GetCurrentAccountId());
                    break;

                case TransferLicenseDto.InitialState.HasSrc:
                    model.AvailableDevices = _licenseTransferLogic.GetAvailableDestDevices(deviceId, User.UserAccount.Id,
                                                                                                                                                                     CurrentState.GetCurrentAccountId());
                    break;
            }

            return PartialView("../Partial/_TransferLicense", model);
        }

        [HttpPost, CspAuthorize(Permission.TransferLicenses)]
        public ActionResult Transfer(TransferLicenseDto model)
        {

            switch (model.Mode)
            {

                case TransferLicenseDto.PostBackMode.Transfer:

                    return TransferDevice(model);

                case TransferLicenseDto.PostBackMode.SearchArchived:

                    return SearchArchivedDevices(model);

                case TransferLicenseDto.PostBackMode.TransferArchived:

                    return TransferArchivedDevice(model);

                default: throw new InvalidEnumArgumentException();
            }
        }

        private ActionResult TransferDevice(TransferLicenseDto model)
        {

            var srcDeviceId = model.State == TransferLicenseDto.InitialState.HasDest
                                                    ? model.UserSelectedDeviceId.Value
                                                    : model.PreSelectedDevice.DeviceId;

            var destDeviceId = model.State == TransferLicenseDto.InitialState.HasDest
                                                    ? model.PreSelectedDevice.DeviceId
                                                    : model.UserSelectedDeviceId.Value;

            var ipAddress = Request.UserHostAddress;

            var result = _licenseTransferLogic.TransferLicenses(srcDeviceId, destDeviceId, User.UserAccount.Id, ipAddress);



            var destDevice = _deviceLogic.GetDevice(destDeviceId, User.UserAccount.Id);
            var srcSuportAccount = _licenseTransferLogic.GetSupportAccountIdforDeviceId(srcDeviceId);

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                model.TransferResult = true;
                //model.Notifications.Add(new ResponseNotification("Transfer Successful", ResponseState.Information));
                model.Notifications.Add(new ResponseNotification("Licenses have been successfully transferred to spare.  Destination device is now in support account:  <a href=\"/SupportAccount/AccountDevices/" + srcSuportAccount.Id + "?devicecode=" + destDevice.SerialNumber + "\">" + srcSuportAccount.Name + "(" + srcSuportAccount.Id + ")</a>", ResponseState.Information));
            }
            else
            {
                model.TransferResult = false;
                model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
            }

            return PartialView("../Partial/_TransferLicense", model);
        }

        private ActionResult SearchArchivedDevices(TransferLicenseDto model)
        {

            var device = _deviceLogic.GetDevice(model.PreSelectedDevice.DeviceId, User.UserAccount.Id);

            var availableDevies = _licenseTransferLogic.GetAvailableArchivedSrcDevices(model.PreSelectedDevice.DeviceId, model.SearchArchivedText, User.UserAccount.Id,
                                                                                                                                                             CurrentState.GetCurrentAccountId());

            ModelState.Clear();


            model.PreSelectedDevice = new TransferLicenseAvailableDevice(device);

            model.State = TransferLicenseDto.InitialState.HasDest;//We're always dealing with spares
            model.Mode = TransferLicenseDto.PostBackMode.TransferArchived;

            model.AllowSwap = false;

            model.AvailableDevices = availableDevies;

            return PartialView("../Partial/_TransferLicense", model);
        }

        private ActionResult TransferArchivedDevice(TransferLicenseDto model)
        {

            var srcDeviceId = model.UserSelectedDeviceId.Value;

            var destDeviceId = model.PreSelectedDevice.DeviceId;

            var ipAddress = Request.UserHostAddress;

            var result = _licenseTransferLogic.TransferRmaLicenses(srcDeviceId, destDeviceId, User.UserAccount.Id, ipAddress);

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                model.TransferResult = true;
                model.Notifications.Add(new ResponseNotification("Transfer Successful", ResponseState.Success));
            }
            else
            {
                model.TransferResult = false;
                model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
            }

            return PartialView("../Partial/_TransferLicense", model);
        }

        [HttpGet, Authorize]
        public ActionResult DeviceLicenses(long id, bool isCsspDevice = false)
        {

            var device = _deviceLogic.GetDevice(id, User.UserAccount.Id);

            bool isCloudDevice = (string.IsNullOrEmpty(device.ModelName) == false &&
                                  device.ModelName.ToUpper().Contains("CLOUD"));
            var k2DeviceMode = string.Empty;
            var isK2Device = _deviceLogic.IsK2Device(device.SerialNumber, out k2DeviceMode);

            if (device.IsVirtualMachine && !isCsspDevice && !isCloudDevice)
            {
                device.IsEligibleForUpgrade = true;
                device.IsUpgradeInProgress = _virtualMachineLogic.IsInTheMiddelOfUpgrade(EVmType.Vm, device.SerialNumber);
            }

            var model = new DeviceLicensesDto
            {
                DeviceId = id,
                Device = device,
                ActivateLicense = new ActivateLicense
                {
                    SerialNumber = device.SerialNumber,
                    Show4HrLocation = false
                },
                Licenses = new LicensesDto
                {
                    ActionHelper = new ActionHelper("Device", id, ActionHelper.EntityAction.Edit),
                    IsLicenseForCloudDevice = isCloudDevice,
                    IsLicenseForCsspDevice = isCsspDevice,
                    Licenses = LicenseLogic.GetDeviceLicenses(id),
                },
                DeviceLicenseMode = DeviceLicenseMode.None,
                AvailableLicenses = new List<object>(),
                IsPostback = false,
                IsK2Device = isK2Device
            };

            if (device.IsUpgradeInProgress)
            {
                if (model.Licenses.Licenses.Any())
                {
                    var licRecord = (from x in model.Licenses.Licenses
                                     where x.AuthorizationCode.StartsWith("V") || (x.LicenseName != null && x.LicenseName.ToUpper().Contains("PA-VM")) || (x.LicenseFeaturesName != null && x.LicenseFeaturesName.ToUpper().Contains("PA-VM"))
                                     select x).FirstOrDefault();

                    if (licRecord != null)
                    {
                        string upgradeFrom, upgradeTo, originalVmAuthCode, currentVmAuthCode;

                        _virtualMachineLogic.GetVmUpgradeModels(EVmType.Vm, id, licRecord.TransactionId, out originalVmAuthCode,
                            out currentVmAuthCode, out upgradeFrom, out upgradeTo);

                        model.Device.VmUpgradeFrom = upgradeFrom;
                        model.Device.VmUpgradeTo = upgradeTo;
                        model.Device.TransactionId = licRecord.TransactionId;
                        model.Device.VmUpgradeAuthCode = currentVmAuthCode;

                        if (!string.IsNullOrEmpty(model.Device.VmUpgradeFrom) &&
                            !string.IsNullOrEmpty(model.Device.VmUpgradeTo))
                        {
                            licRecord.LicenseName = string.Format("VM Upgrade from {0} to {1}", upgradeFrom, upgradeTo);
                        }
                    }
                }
            }

            if (device.IsEligibleForUpgrade && !device.IsUpgradeInProgress)
            {
                device.IsUpgradeComplete = _virtualMachineLogic.IsUpgradeComplete(EVmType.Vm, id);
            }


            model.WF500Keys = _deviceLogic.GetWF500SoftwareKeys(model.Device.SerialNumber);
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            model.SupportAccountId = supportAccountId;
            string partNumber, modelName;
            LicenseLogic.GetPartNumberAndModel(model.Device.SerialNumber, out partNumber, out modelName);
            bool isNFR = partNumber.Contains("-NFR");
            bool hasNfrSvcOrBndLic = false;
            if (isNFR)
                hasNfrSvcOrBndLic = LicenseLogic.HasNfrSvcOrBndLic(model.Device.SerialNumber, partNumber);

            //Panorama
            var isPanorama = partNumber.StartsWith("PAN-PRA");
            if (isPanorama && !string.IsNullOrEmpty(modelName))
            {
                //modelName: PRA-1000, PRA-25, PRA-100
                var panoramaSku = string.Format("PAN-P-{0}", modelName.Substring(4));
                model.VMRegistrationDto = new RegisterVmDto();
                model.IsEligibleForPanoramaLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.PanoramLicense_Feature, panoramaSku,
                    model.Device.SerialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);

            }
            model.IsEligibleForPanoramaCloudLicenseTransfer = LicenseLogic.isEligibleForCloudLicenseTransfer(model.Device.SerialNumber, supportAccountId);
            model.IsHAPanorama = _saasLogic.IsHAPanorama(model.Device.SerialNumber);

            //feature license
            var isCsspAccount = _supportAccountLogic.IsCsspAccount(supportAccountId);
            if (isCloudDevice && modelName == "VM-300")
            {
                //Exclude VM300 Cloud SKU
                model.IsEligibleForDpmLicense = false;
            }
            else 
            {
                model.IsEligibleForDpmLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.DecryptionPortMirror_Feature, FeatureAccess.DecryptionPortMirror_SKU,
                    model.Device.SerialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);
            }

            if (!isCsspAccount)
            {
                model.IsEligibleForDpbLicense = LicenseLogic.IsElligibleForFeatureLicense(FeatureAccess.NetworkPacketBroker_Feature, FeatureAccess.SSLDecryptionBroker_SKU,
                    model.Device.SerialNumber, modelName, User.UserAccount.Email, supportAccountId, isNFR, hasNfrSvcOrBndLic);
            }

            model.IsEligibleForFeatureLicense = model.IsEligibleForDpbLicense || model.IsEligibleForDpmLicense;

            var trialLicenseExcludes = System.Configuration.ConfigurationManager.AppSettings["TrialLicenseExcludes"].Split(',');
            var showTrialLicense = !trialLicenseExcludes.Any(d => model.Device.ModelName.Contains(d)) && !model.Device.IsCgx && !model.Device.IsTrialLicenseExcludePanorama;
            //var showNfrLicense = model.Licenses.Licenses.Any(license =>
            //        ((license.ExpirationDate > DateTime.Now) && (license.ModelName.Contains("PAN-SVC-NFR")) && (!license.ModelName.Contains("-PRA"))));
            //sseetham
            //if (showNfrLicense) {
            //    model.DeviceLicenseMode = DeviceLicenseMode.Eval;
            //} else 

            if (showTrialLicense)
            {
                model.DeviceLicenseMode = DeviceLicenseMode.Trial;
            }

            if (model.DeviceLicenseMode != DeviceLicenseMode.None)
            {

                var mode = model.DeviceLicenseMode ==
                                     DeviceLicenseMode.Trial
                                         ? "Trial"
                                         : "Eval";

                model.AvailableLicenses = LicenseLogic.GetTrialLicenseListBySerialNumber(model.Device.SerialNumber, mode);
            }

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.ActivateLicense;

            return PartialView("../Partial/_DeviceLicenses", model);
        }


        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult DeactivateLicenses()
        {

            return PartialView("../Partial/_DeactivateLicenses", new DeactivateLicensesDto());
        }

        [HttpPost, CspAuthorize(Permission.EditDevice)]
        public ActionResult DeactivateLicenses(DeactivateLicensesDto model)
        {

            var success = _licenseLogic.DeactivateLicense(model.DeactivationTokenText);

            model = new DeactivateLicensesDto();

            if (success)
            {
                model.Notifications.Add(new ResponseNotification("License Deactivation Successfull", ResponseState.Success));
            }
            else
            {
                model.Notifications.Add(new ResponseNotification("An error occurred during license deactivation.  Please contact customer support.", ResponseState.Error));
            }

            return PartialView("../Partial/_DeactivateLicenses", model);
        }

        [HttpPost, CspAuthorize(Permission.EditDevice)]
        public ActionResult DeactivateLicenseFile(HttpPostedFileBase[] deativationTokenFile)
        {

            var fileText = HttpUtility.HtmlDecode(_licenseLogic.ParseDeactivateLicenseFile(deativationTokenFile.Single()));

            return Json(fileText);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult RollbackUpgrade(string transactionId, string authorizationCode)
        {
            var model = new RollbackDto();
            bool isRollbackSuccessful = _virtualMachineLogic.RollbackUpgrade(EVmType.Vm, transactionId);
            model.IsSuccess = isRollbackSuccessful;
            if (isRollbackSuccessful)
            {
                string originalDevice = _virtualMachineLogic.GetOriginalModelAfterRollback(EVmType.Vm, transactionId, authorizationCode);
                model.OriginalVm = originalDevice;
            }
            if (isRollbackSuccessful)
            {
                model.Notifications.Add(new ResponseNotification("VM Upgrade successfully Rollbacked.",
                    ResponseState.Success));
            }
            else
            {
                model.Notifications.Add(new ResponseNotification("VM Rollback failed",
                    ResponseState.Error));
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.EditDevice)]
        public ActionResult GetVmUpgradeModels(string serialNumber, string upgradeAuthCode)
        {
            var model = new VmModelUpgradeDto();
            serialNumber = Sanitizer.GetSafeHtmlFragment(serialNumber);
            bool isValidForUpgrade = string.IsNullOrEmpty(_licenseLogic.IsAuthorizationCodeValidForUpgrade(upgradeAuthCode, serialNumber));
            model.IsValidUpgradeAuthCode = isValidForUpgrade;
            if (isValidForUpgrade)
            {
                string fromModel;
                string toModel;

                _virtualMachineLogic.GetVmUpgradeModels(EVmType.Vm, serialNumber, upgradeAuthCode, out fromModel, out toModel);
                model.FromModel = fromModel;
                model.ToModel = toModel;
                model.SerialNumber = serialNumber;
                model.UpgradeMessage = string.Format("The firewall {0} will be upgraded from {1} to {2}, do you want to continue?<br><u>Note:</u> Subscriptions should match between both auth-codes to ensure there is no service disruption.", serialNumber,
                    fromModel, toModel);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult TransferCloudLicenses(string fromSerialNumber, string toSerialNumber, int eulaId)
        {
            var result = "";
            var model = new DeviceLicensesDto()
            {
                Eula = new EulaModel()
                {
                    SelectedEulaId = eulaId
                },
            };

            try
            {
                if (_saasLogic.HasLcaasLicense("Transfer", toSerialNumber) == true)
                    result = "Error:" + SaasAuthCodeMessages.PanoramaAlreadyActivated;
                else if (_licenseLogic.TransferCloudLicenses(fromSerialNumber, toSerialNumber))
                {
                    result = "Cloud Licenses Transferred successfully!";
                    #region Bind Eula to Model
                    BindEula(model);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = "Error: An error occurred while processing your request. Please contact customer support.";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewDevice)]
        public ActionResult ShowMasterAutoCodeInfo(long masterAutocodeLicId, string serialNumber, string modelName, string authCode)
        {
            var model = new MasterAuthCodeInfo
            {
                AuthCode = authCode,
                ModelName = modelName,
                SerialNumber = serialNumber
            };

            model.MasterAuthCode = _deviceLogic.GetAuthcodeInfo(masterAutocodeLicId);
            return PartialView("../Partial/_MasterAuthCodeInformation", model);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult GetAuthCodeType(string authCode)
        {
            var authCodeType = string.Empty;
            authCode = Sanitizer.GetSafeHtmlFragment(authCode);
            try
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    authCodeType = _licenseLogic.GetAuthCodeType(authCode);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(authCodeType, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult GetPanOsVersion(string authCode)
        {
            var panOsVersion = string.Empty;
            authCode = Sanitizer.GetSafeHtmlFragment(authCode);
            

            try
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    panOsVersion = _licenseLogic.GetPanOsVersion(authCode);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(panOsVersion, JsonRequestBehavior.AllowGet);
        }

        [Filters.JwtTokenAuthentication]
        [HttpPost]
        public ActionResult DeactivateManagedLicenses(LicenseManagementDeactivationRequest requestData)
        {
            var status = false;
            string message = string.Empty;
            if (!JwtTokenManager.IsAuthorized(requestData.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Json(new { Success = false, Message = "User is unauthorized to Deactivate License." });
            }

            if (!((User as CspPrincipal)?.HasPermission(Permission.ActivateProducts) ?? false))
            {
                return Json(new { Success = false, Message = "User does not have permissions to Deactivate License." });
            }

            try
            {
                var result = licenseManagementLogic.DeactivateLicenses(requestData);

                status = result.Success;
                message = (result.ErrorDetails != null) ? result.ErrorDetails.Message : string.Empty;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { Success = status, Message = message });
        }
    }
}
