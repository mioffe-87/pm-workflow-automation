using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Castle.Core.Internal;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using Csp.Api.DataAccess;
using Microsoft.Security.Application;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class SaasController : BaseController
    {
        private readonly ISaasLogic _saasLogic; 
        private readonly IEulaLogic _eulaLogic;
        private readonly IOtpLogic _otpLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly bool useCoreAPI = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCoreAPI"]);


        public SaasController(ISaasLogic saasLogic, IEulaLogic eulaLogic, IOtpLogic otpLogic, IDeviceLogic deviceLogic)
        {
            _saasLogic = saasLogic;
            _eulaLogic = eulaLogic;
            _otpLogic = otpLogic;
            _deviceLogic = deviceLogic;
        }

        /// <summary>
        /// Inital Page Load without model
        /// </summary>
        /// <returns>Saas details view</returns>
        public ActionResult ShowActivations(long id)
        {
						if (id != CurrentState.GetCurrentAccountId()) {
							return null;
						}

						var dataParameters = new DataParameters { PaginationCount = 10 };
            var saasViewModel = new SecaasViewModelDto();
            saasViewModel.SupportAccountId = id;
            saasViewModel.hasOTPAccess = User.HasPermission(Permission.GenerateSaasOTP);
            saasViewModel.DataParameters = dataParameters;
            return PartialView("../Partial/_SaasView", saasViewModel);
        }

        /// <summary>
        /// Show available SerialNumbers and relevant data on view.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authCode">Give authcode in case of search</param>
        /// <returns>Json Containing available Saas Details</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SearchAvailable([DataSourceRequest] DataSourceRequest request, long supportAccountId, string searchBy, string searchValue)
        {
            var dataParameters = request.ToDataParameters();
            if (VerifyAuthorizationBySupportAccount(supportAccountId))
            {
                bool currentAccountIsAsc = false;
                var ascSupportAccountIds = User.RoleSecurities.GetAscSupportAccountIds();
                if (ascSupportAccountIds.Any())
                    currentAccountIsAsc = ascSupportAccountIds.Any(p => p == supportAccountId) || CurrentState.HasDirectMembership();

                var availableSaasActivations = _saasLogic.GetAvailableSaasActivations(dataParameters, supportAccountId, searchBy, searchValue, User.UserAccount.Id, currentAccountIsAsc);

                var result = new DataSourceResult
                {
                    Data = availableSaasActivations,
                    Total = Convert.ToInt32(dataParameters.TotalRecordCount)
                };

                return Json(result);
            }
            else
                return Json(String.Empty);

        }

        /// <summary>
        /// Show the Activate Auth-code pop up
        /// </summary>
        /// <returns>model with eula details necessary to show pop up</returns>
        [HttpGet]
        public ActionResult AddSaasAuthCode(long supportAccountID, string authCode = "",string region ="")
        {
            var model = new ActivateSaasAuthCodeDto();
            model.IsPanoramaAssociation = false;
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
            model.Eula.Context = EulaContext.ActivateVmAuthCode;
            #endregion
            model.SupportAccountID = supportAccountID;
            if (!string.IsNullOrEmpty(authCode))
            {
                model.AuthorizationCode = authCode;
                model.IsPanoramaAssociation = true;
            }
            if (!string.IsNullOrEmpty(region))
            {
                model.LoggingRegion = region;
            }
           
            return PartialView("../Partial/_AddSaasAuthCode", model);
        }


        public JsonResult GetPanoramaSerialNumbers(long supportAccountID)
        {
            bool isValid = VerifyAuthorizationBySupportAccount(supportAccountID);
            if (isValid)
            {
                var panoramaSerialNumbers = _saasLogic.GetPanoramaSerialNumbers(supportAccountID);
                return Json(panoramaSerialNumbers, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(String.Empty);

        }
        public JsonResult GetLoggingServiceDevices(long supportAccountID)
        {
            bool isValid = VerifyAuthorizationBySupportAccount(supportAccountID);
            if (isValid)
            {
                var loggingServiceDevices = _saasLogic.GetLoggingServiceDevices(supportAccountID);
                return Json(loggingServiceDevices, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(String.Empty);

        }

        public JsonResult GetPanoramaSerialNumbersforOTP(long supportAccountID)
        {
            bool isValid = VerifyAuthorizationBySupportAccount(supportAccountID);
            if (isValid)
            {
                var panoramaSerialNumbers = _saasLogic.GetPanoramaWithCloudService(supportAccountID, "OTP", "");
                return Json(panoramaSerialNumbers, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(String.Empty);
        }

        public JsonResult GetLoggingRegions()
        {
            var regions = _saasLogic.GetLoggingRegions();
            
            return Json(regions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAuthCodeType(string authCode)
        {
            return Json(_saasLogic.GetCloudServiceAuthCodeType(authCode),JsonRequestBehavior.AllowGet);
        }
        public string GetFreeLoggingService(string region, long supportAccountId)
        {
            return _saasLogic.GetFreeLoggingService(Sanitizer.GetSafeHtmlFragment(region), supportAccountId);
        }
        /// <summary>
        /// Activate Auth code & panorama Serial number from pop up
        /// </summary>
        /// <param name="model">model containing Authcode, Panorama Serial Number</param>
        /// <returns>Success / Failure message for Activating Authcode</returns>
        [HttpPost, AjaxOnly]
        public ActionResult ActivateSaasAuthCode(ActivateSaasAuthCodeDto model)
        {
            var selectedSerialNumbers = model.Panoramas;
            var selectedLoggingService = model.LoggingServiceDevice;
            var supportAccountId = model.SupportAccountID;
            var isPanoramaAssociation = _saasLogic.IsPanoramaAssociation(model.AuthorizationCode);
            var isEligibleForCdlPanoramaAssociation = _saasLogic.IsEligibleForCdlPanoramaAssociation(model.AuthorizationCode);
            if (model != null && ModelState.IsValid)
            {
                string message = null;
                var isEligbleForActivation = _saasLogic.IsProductEligibleForActivation(model.AuthorizationCode, out message);
                var isSeUser = CurrentState.IsSE();

                var isSeEligbleForActivation = isSeUser // SE user can activate any AuthCode
                    || !isSeUser && !_saasLogic.IsSeAuthCode(model.AuthorizationCode); // non-SE user can activate only non-SE AuthCode

                string authCode = model.AuthorizationCode;
                AddSaasAuthCodeResult result;
                List<AddSaasAuthCodeResult> results = new List<AddSaasAuthCodeResult>();

               if (_saasLogic.IsCdlAuthCode(authCode) && !isPanoramaAssociation)
                {
                    var newActiveLink = _saasLogic.GetCdlMagicLink(authCode);
                    if (newActiveLink != null && newActiveLink.Issuccess && newActiveLink.Licenses.Count>0)
                    {
                        result = new AddSaasAuthCodeResult(string.Format(SaasAuthCodeMessages.CDLAuthCodeWithMagicLink, newActiveLink.Licenses[0].ActivationLink));
                        results.Add(result);
                    }
                    else
                    {

                        result = new AddSaasAuthCodeResult
                        {
                            ErrorMessage = SaasAuthCodeMessages.CDLAuthCodeWithNoMagicLink,
                        };
                        results.Add(result);
                    }
                }
                else if (isEligbleForActivation != EActivationActionType.CanActivate
                         && isEligbleForActivation != EActivationActionType.HasActivationLink
                         && !string.IsNullOrWhiteSpace(message))
                {
                    result = new AddSaasAuthCodeResult
                    {
                        ErrorMessage = message,
                    };
                    results.Add(result);
                }
                else
                {
                    try
                    {
                        if ((isEligbleForActivation == EActivationActionType.CanActivate
                            || isEligbleForActivation == EActivationActionType.HasActivationLink)
                            && isSeEligbleForActivation)
                        {
                            var authCodeType = _saasLogic.GetCloudServiceAuthCodeType(model.AuthorizationCode);

                            if (authCodeType.Type == SaasAuthCodeTypes.MagnifierBeta &&
                                _saasLogic.HasBetaAccess(supportAccountId))
                            {
                                if (model.LoggingServiceDevice == null || model.LoggingServiceDevice == "")
                                {
                                    result = new AddSaasAuthCodeResult
                                    {
                                        ErrorMessage = SaasAuthCodeMessages.LoggingServiceNotSelected,
                                    };
                                    results.Add(result);
                                }
                                else
                                {
                                    result = _saasLogic.AddSaasAuthCode(model.AuthorizationCode,
                                        model.LoggingServiceDevice, model.SupportAccountID, User.UserAccount.Id,
                                        model.LoggingRegion, isPanoramaAssociation);
                                    results.Add(result);
                                }
                            }
                            else
                            {
                                for(int i = 0; i < model.Panoramas.Count; i++)
                                {
                                    if (string.IsNullOrEmpty(model.Panoramas[i].ToString()) &&
                                        authCodeType.Type == SaasAuthCodeTypes.LoggingService && !useCoreAPI)
                                    {
                                        result = new AddSaasAuthCodeResult
                                        {
                                            ErrorMessage = SaasAuthCodeMessages.PanoramaNotSelected,
                                        };
                                        results.Add(result);
                                    }
                                    //Panorama is required for non-logging service activation or for logging service without a previous panorama
                                    else if ((string.IsNullOrEmpty(model.Panoramas[i].ToString())) &&
                                             ((authCodeType.Type == SaasAuthCodeTypes.LoggingService &&
                                               isPanoramaAssociation) ||
                                              authCodeType.Type != SaasAuthCodeTypes.LoggingService))
                                    {
                                        result = new AddSaasAuthCodeResult
                                        {
                                            ErrorMessage = SaasAuthCodeMessages.PanoramaNotSelected,
                                        };
                                        results.Add(result);
                                    }
                                    else
                                    {
                                        if (authCodeType.Type == SaasAuthCodeTypes.LoggingService &&
                                            model.LoggingRegion == null || model.LoggingRegion == "")
                                        {
                                            result = new AddSaasAuthCodeResult
                                            {
                                                ErrorMessage = SaasAuthCodeMessages.RegionNotSelected,
                                            };
                                            results.Add(result);
                                        }
                                        else if (authCodeType.Type == SaasAuthCodeTypes.LoggingService &&
                                                 model.IsLogQuotaChecked != true)
                                        {
                                            result = new AddSaasAuthCodeResult
                                            {
                                                ErrorMessage = SaasAuthCodeMessages.LogQuotaNotAccepted,
                                            };
                                            results.Add(result);
                                        }
                                        else
                                        {
                                            if (_saasLogic.CheckIfPreviouslyActivated(model.AuthorizationCode,
                                                model.Panoramas[i].ToString()))
                                            {
                                                result = new AddSaasAuthCodeResult
                                                {
                                                    ErrorMessage = SaasAuthCodeMessages.PanoramaAlreadyActivated,
                                                };
                                                results.Add(result);
                                            }
                                            else
                                            {
                                                if (_saasLogic.HasLcaasLicense(authCode, model.Panoramas[i].ToString()) == null)
                                                {
                                                    result = new AddSaasAuthCodeResult
                                                    {
                                                        ErrorMessage = "An error occurred while adding the auth code",
                                                    };
                                                    results.Add(result);
                                                }
                                                else if (_saasLogic.HasLcaasLicense(authCode, model.Panoramas[i].ToString()) == false
                                                         && isEligbleForActivation !=
                                                         EActivationActionType.HasActivationLink)
                                                {
                                                    result = new AddSaasAuthCodeResult
                                                    {
                                                        ErrorMessage =
                                                            "An activated Cortex Data Lake must be associated to a registered Panorama with Premium Support before activating Prisma Access",
                                                    };
                                                    results.Add(result);
                                                }
                                                else
                                                {
                                                    result = _saasLogic.AddSaasAuthCode(model.AuthorizationCode,
                                                        model.Panoramas[i].ToString(), model.SupportAccountID, User.UserAccount.Id,
                                                        model.LoggingRegion, isEligibleForCdlPanoramaAssociation);
                                                    results.Add(result);

                                                    if (!string.IsNullOrWhiteSpace(result
                                                        .ActivationLink)) // CSP-3170 Turn off Prisma Access activation from CSP. Show activation magic link; block activation from CSP
                                                    {
                                                        result = new AddSaasAuthCodeResult(
                                                            string.Format(SaasAuthCodeMessages.ExternalActivationLink,
                                                                model.AuthorizationCode, result.ActivationLink));
                                                        results.Add(result);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = new AddSaasAuthCodeResult
                            {
                                    ErrorMessage = isEligbleForActivation != EActivationActionType.CanActivate
                                        ? SaasAuthCodeMessages.MagniferRedirect
                                        : isSeUser
                                            ? SaasAuthCodeMessages.MagniferRedirectSe
                                            : SaasAuthCodeMessages.MagniferRedirectNonSe
                            };
                            results.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        result = new AddSaasAuthCodeResult
                        {
                            ErrorMessage = "An error occurred while adding the auth code",
                        };
                        results.Add(result);
                    }
                }

                if (results.Count > 0)
                {
                    foreach (var res in results)
                    {
                        if (string.IsNullOrEmpty(res.ErrorMessage))
                        {
                            try
                            {
                                supportAccountId = model.SupportAccountID;
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
                                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(
                                    string.Format(
                                        "Could not add EULA Agreements while accepting while activating Saas Auth Code - {0}. Eula Id Selected is - {1}",
                                        model.AuthorizationCode, model.Eula.DefaultEula.Id)));
                            }

                            model.Notifications.Add(
                                new ResponseNotification("Successfully Activated Cloud Service Auth-Code",
                                    ResponseState.Success));
                        }
                        else
                        {
                            model.Notifications.Add(new ResponseNotification(res.ErrorMessage, ResponseState.Error));
                        }
                    }
                }
            }
            model.Panoramas = selectedSerialNumbers;
            model.LoggingServiceDevice = selectedLoggingService;
            model.IsPanoramaAssociation = isPanoramaAssociation;
            if (!isPanoramaAssociation)
            {
                model.AuthorizationCode = string.Empty;
                model.LoggingRegion = null;
                model.Panoramas = null;
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
            model.Eula.Context = EulaContext.ActivateLicense;

            #endregion
            return PartialView("../Partial/_AddSaasAuthCode", model);
        }

        /// <summary>
        /// Generate CSV report for the available Saas data
        /// </summary>
        /// <param name="authCode">used in case a specific auth code is searched</param>
        /// <returns>csv file containing Saas data</returns>
        public FileResult SearchAvailableReport(long supportAccountID,string searchBy,string searchValue)
        {
						if (supportAccountID != CurrentState.GetCurrentAccountId()) {
							return null;
						}

            var csv = _saasLogic.GetAvailableSaasCsv(supportAccountID, searchBy, searchValue, User.UserAccount.Id);

            var dateStr = DateTime.Now.ToString("yyyyMMdd");

            return File(csv, "text/comma-separated-values", "SaasReport_" + dateStr + ".csv");
        }

        /// <summary>
        /// Open GenerateOTP view from saas page
        /// </summary>
        public ActionResult ViewGenerateOTP(long supportAccountID)
        {
            var model = new GenerateOTPdto();
            model.SupportAccountID = supportAccountID;
            return PartialView("../Partial/_GenerateOTP", model);
        }

        /// <summary>
        ///This is called on Generate OTP button click.
        /// </summary>
         [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GenerateOTP(GenerateOTPdto model)
        {
           if ((model.PanoramaSerialNumber != null || model.SerialNumber != null) && model.SupportAccountID != 0 && VerifyAuthorizationBySupportAccount(model.SupportAccountID))
           {
                //Validate if the SN is registered in the same support account which is provided in the request
                string serialNumber = String.IsNullOrEmpty(model.PanoramaSerialNumber) ? model.SerialNumber : model.PanoramaSerialNumber;
                long supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(serialNumber);
                if (model.SupportAccountID != supportAccountId)
                {
                    var ex = new UnauthorizedAccessException("User does not have permission to generate OTP for the given SerialNumber");
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw ex;
                }

                var returnData = new GenerateOtpResult();
                model.OtpType = model.DeviceType.Equals(OtpDeviceType.PanOS) ? OtpType.PanOS : model.OtpType;

               var secaasOtp = new SingleOtpRequest
               {
                   SerialNumber = model.OtpType.Equals(OtpType.PanOS) ? model.SerialNumber : model.PanoramaSerialNumber,
                   SupportAccountId = model.SupportAccountID,
                   OtpType = model.OtpType.ToString(),
                   UpdatedBy = User.UserAccount.Id.ToString()
               };

               //Generate OTP for PAN-OS device
                if (model.SerialNumber != null)
                {
                    //Check if it is a valid serial Number which user can acccess                   
                    secaasOtp.DeviceType = OtpDeviceType.PanOS.ToString();                    
                }
                else
               {
                   var appType =
                       model.PanoramaSerialNumber != null && model.OtpType.Equals(OtpType.ZtpService)
                           ? AppPortalAppType.ZtpService
                           : AppPortalAppType.LoggingService;

                   if (appType.Equals(AppPortalAppType.ZtpService))
                   {
                       //Generate Tenant Id and associate it to primary panorama
                       _saasLogic.CreateZtpTenantForPanorama(model.SupportAccountID, model.PanoramaSerialNumber,
                           User.UserAccount.Email);
                   }

                   
                   secaasOtp.DeviceType = OtpDeviceType.Panorama.ToString();
                   secaasOtp.OtpType = model.OtpType.ToString();
               }

                var response = _otpLogic.GenerateSingleOtp(secaasOtp);
                returnData = response.ResponseObject;
                if (!string.IsNullOrEmpty(returnData.OneTimePassword))
               {
                   model.OtpOutput.OneTimePassword = returnData.OneTimePassword;
                   model.OtpOutput.ExpiryDate = returnData.ExpirationDate;
               }
           }
           else
           {
               model.OtpOutput = null;
           }
            
            return PartialView("../Partial/_GenerateOTP", model);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
         public JsonResult GetCurrentOTP(string supportAccountID, string serialNumber, OtpType otpType)
        {
            bool isValidUser = VerifyAuthorizationBySupportAccount(long.Parse(supportAccountID));
            //Validate if the SN is registered in the same support account which is provided in the request
            long supportAccountId = _deviceLogic.GetSupportAccountIdByDeviceBySerialNumber(serialNumber);
            bool hasDeviceAccess = supportAccountId == long.Parse(supportAccountID);
            long tenantId = 0;
            if (isValidUser && hasDeviceAccess)
            {
                if (!otpType.Equals(OtpType.PanOS))
                {
                    var appType = otpType.Equals(OtpType.ZtpService) ? AppPortalAppType.ZtpService : AppPortalAppType.LoggingService;
                    tenantId = _saasLogic.GetPanoramaTenantId(long.Parse(supportAccountID), serialNumber, appType);
                }

                var otp = _saasLogic.GetSaasOTP(tenantId, serialNumber, otpType);
                return Json(otp, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(String.Empty);
        }

        static string sha256(string password)
        {
            string pwd = string.Format("{0}{1}", password, System.DateTime.Now.ToString());
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(pwd), 0, Encoding.ASCII.GetByteCount(pwd));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }

        public JsonResult GetPrimaryPanoramaForOTP(long supportAccountID)
        {
            bool isValid = VerifyAuthorizationBySupportAccount(supportAccountID);
            if (isValid)
            {
                var panoramaSerialNumbers = _saasLogic.GetPrimaryPanorama(supportAccountID);
                return Json(panoramaSerialNumbers, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(String.Empty);
        }

        //[TrialAccessAttribute]
        //public ActionResult TryaProduct()
        //{
        //    return View();
        //}

        public JsonResult GetPanoramaWithCloudServiceForTransfer(long supportAccountID, string serialNumber)
        {
            if (VerifyAuthorizationBySupportAccount(supportAccountID)
                && !string.IsNullOrWhiteSpace(serialNumber))
            {
                var panoramaSerialNumbers = _saasLogic.GetPanoramaWithCloudServiceForTransfer(supportAccountID, serialNumber);
                return Json(panoramaSerialNumbers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(String.Empty);
            }
        }
    }
}
