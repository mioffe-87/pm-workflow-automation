using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using FluentNHibernate.Conventions;
using Csp.Api.Model.ElaEsa;
using System.Threading.Tasks;
using System.Threading;
using Csp.Api.Model.Csp.Api.Model;

namespace Csp.Web.Controllers {
	public class DeviceValidationController : ApiController {
		private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;
		private readonly IDeviceLogic _deviceLogic;
		private readonly IUserAccountLogic _userAccountLogic;
        private readonly IUserProfileDataAccess _userProfileDataAccess;
        private readonly IVirtualMachineLogic _virtualMachineLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly IElaEsaLogic elaEsaLogic;

        private readonly int _aws = Convert.ToInt32(DeviceType.AWS);
        private readonly int _azure = Convert.ToInt32(DeviceType.AZURE);
        private readonly int _gcp = Convert.ToInt32(DeviceType.GCP);
        private readonly int _cssp = Convert.ToInt32(DeviceType.CSSP);

		public DeviceValidationController(IAscDeviceRegistrationService ascDeviceRegistrationService, IDeviceLogic deviceLogic, IUserAccountLogic userAccountLogic,IUserProfileDataAccess userProfileDataAccess,
            IVirtualMachineLogic virtualMachineLogic , ISiteLicenseLogic siteLicenseLogic, IElaEsaLogic elaEsaLogic) {
			_ascDeviceRegistrationService = ascDeviceRegistrationService;
			_deviceLogic = deviceLogic;
			_userAccountLogic = userAccountLogic;
            _userProfileDataAccess = userProfileDataAccess;
            _virtualMachineLogic = virtualMachineLogic;
            _siteLicenseLogic = siteLicenseLogic;
            this.elaEsaLogic = elaEsaLogic;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.AllowAnonymous]
        public bool IsPanoramaSerialNumber(string serialNumber){
            var message = _deviceLogic.CheckIsSerialNumberPanorama(serialNumber);
            if (!string.IsNullOrEmpty(message) && message == "This serial number is Panorama")
                return true;
            return false;
        }

		private AscCustResult ShouldShowAscCustOptions(string serialNumber, long supportAccountId) {
			var thisUser = (CspPrincipal)User;

			var userAccountId = thisUser.UserAccount.Id;

			var message = _deviceLogic.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
            var isPanorama = IsPanoramaSerialNumber(serialNumber);

			var isSerialNumberValid = (message == "");
			if (!isSerialNumberValid) {
				return new AscCustResult {
					ShowAscCustOptions = false,
				};
			}

		    var lightCyberDeviceName = _deviceLogic.GetLegacyLightCyberDeviceName(serialNumber);
            
            //Verify the current support account is an ASC and the current user is an active member of that ASC
		    bool isMemberOfAsc = _ascDeviceRegistrationService.IsMemberOfAsc(userAccountId,supportAccountId);
			if(!isMemberOfAsc) {
				//Also check if this is an eval account registering a non-eval device
				var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
				var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

				if (isEvalSupportAccount && !isEvalDevice) {
					return new AscCustResult {
						ShowEvalAccountWarning = true,
                        SerialNumberIsPanorama = isPanorama,
					    LegacyLightCyberDeviceName = lightCyberDeviceName
					};
				}

				return new AscCustResult {
					ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
				    LegacyLightCyberDeviceName = lightCyberDeviceName
				};
			}

			//Verify the serial number belongs to another support account (remember that sales orders are tied to SAP ID which has a one-to-many relationship with SupportAccountIDs)
			var deviceSupportAccountIds = _ascDeviceRegistrationService.GetDeviceSalesOrderSupportAccountId(serialNumber);
			var belongsToAnotherSupportAccount = deviceSupportAccountIds.Any(saId => saId != supportAccountId);
			if (!belongsToAnotherSupportAccount) {
				return new AscCustResult {
					ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
				    LegacyLightCyberDeviceName = lightCyberDeviceName
				};
			}

			//We're only supporiting
			if (deviceSupportAccountIds.Count() > 1) {
				return new AscCustResult {
					ShowAscCustOptions = false,
					OneToManyMappingException = true,
                    SerialNumberIsPanorama = isPanorama,
				    LegacyLightCyberDeviceName = lightCyberDeviceName
				};
			}

			//Verify that customer support account has one or more unregistered bkln licenses in it's sales order history
			var hasBklnLicenses = _ascDeviceRegistrationService.HasBklnSupportInSalesOrder(serialNumber, supportAccountId);

			if (!hasBklnLicenses) {
				return new AscCustResult {
					ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
				    LegacyLightCyberDeviceName = lightCyberDeviceName
				};
			}

			var customerSupportAccountId = deviceSupportAccountIds.Single();
			var customerSupportAccountName = _ascDeviceRegistrationService.GetSupportAccountNameBySupportAccountId(customerSupportAccountId);

			//Then they're ok
			return new AscCustResult {
				ShowAscCustOptions = true,
				CustomerSupportAccountName = customerSupportAccountName,
				CustomerSupportAccountId = customerSupportAccountId,
                SerialNumberIsPanorama = isPanorama,
			    LegacyLightCyberDeviceName = lightCyberDeviceName
			};
		}

        private AscCustResult ShouldShowAscCustOptions(string serialNumber, long supportAccountId, bool isMemberofAsc)
        {
            var thisUser = (CspPrincipal)User;

            var userAccountId = thisUser.UserAccount.Id;

            var message = _deviceLogic.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
            var isPanorama = IsPanoramaSerialNumber(serialNumber);

            var isSerialNumberValid = (message == "");
            if (!isSerialNumberValid)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                };
            }

            var lightCyberDeviceName = _deviceLogic.GetLegacyLightCyberDeviceName(serialNumber);

            //Verify the current support account is an ASC and the current user is an active member of that ASC
            bool isMemberOfAsc = _ascDeviceRegistrationService.IsMemberOfAsc(userAccountId, supportAccountId);
            if (!isMemberofAsc)
            {
                //Also check if this is an eval account registering a non-eval device
                var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
                var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

                if (isEvalSupportAccount && !isEvalDevice)
                {
                    return new AscCustResult
                    {
                        ShowEvalAccountWarning = true,
                        SerialNumberIsPanorama = isPanorama,
                        LegacyLightCyberDeviceName = lightCyberDeviceName
                    };
                }

                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            //Verify the serial number belongs to another support account (remember that sales orders are tied to SAP ID which has a one-to-many relationship with SupportAccountIDs)
            var deviceSupportAccountIds = _ascDeviceRegistrationService.GetDeviceSalesOrderSupportAccountId(serialNumber);
            var belongsToAnotherSupportAccount = deviceSupportAccountIds.Any(saId => saId != supportAccountId);
            if (!belongsToAnotherSupportAccount)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            //We're only supporiting
            if (deviceSupportAccountIds.Count() > 1)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                    OneToManyMappingException = true,
                    SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            //Verify that customer support account has one or more unregistered bkln licenses in it's sales order history
            var hasBklnLicenses = _ascDeviceRegistrationService.HasBklnSupportInSalesOrder(serialNumber, supportAccountId);

            if (!hasBklnLicenses)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                    SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            var customerSupportAccountId = deviceSupportAccountIds.Single();
            var customerSupportAccountName = _ascDeviceRegistrationService.GetSupportAccountNameBySupportAccountId(customerSupportAccountId);

            //Then they're ok
            return new AscCustResult
            {
                ShowAscCustOptions = true,
                CustomerSupportAccountName = customerSupportAccountName,
                CustomerSupportAccountId = customerSupportAccountId,
                SerialNumberIsPanorama = isPanorama,
                LegacyLightCyberDeviceName = lightCyberDeviceName
            };
        }


        private AscCustResult ShouldShowAscOptions(string serialNumber, long supportAccountId, bool isMemberofAsc)
        {
            //var thisUser = (CspPrincipal)User;

            //var userAccountId = thisUser.UserAccount.Id;

            // var message = _deviceLogic.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
            //var isPanorama = IsPanoramaSerialNumber(serialNumber);

            //var isSerialNumberValid = (message == "");
            //if (!isSerialNumberValid)
            //{
            //    return new AscCustResult
            //    {
            //        ShowAscCustOptions = false,
            //    };
            ////}

            //var isPanorama = false;
            var lightCyberDeviceName = _deviceLogic.GetLegacyLightCyberDeviceName(serialNumber);

            //Verify the current support account is an ASC and the current user is an active member of that ASC
            // bool isMemberOfAsc = _ascDeviceRegistrationService.IsMemberOfAsc(userAccountId, supportAccountId);
            //if (!isMemberofAsc)
            //{
            //    //Also check if this is an eval account registering a non-eval device
            //    var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
            //    var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

            //    if (isEvalSupportAccount && !isEvalDevice)
            //    {
            //        return new AscCustResult
            //        {
            //            ShowEvalAccountWarning = true,
            //            SerialNumberIsPanorama = isPanorama,
            //            LegacyLightCyberDeviceName = lightCyberDeviceName
            //        };
            //    }

            //    return new AscCustResult
            //    {
            //        ShowAscCustOptions = false,
            //        SerialNumberIsPanorama = isPanorama,
            //        LegacyLightCyberDeviceName = lightCyberDeviceName
            //    };
            //}

            //Verify the serial number belongs to another support account (remember that sales orders are tied to SAP ID which has a one-to-many relationship with SupportAccountIDs)
            var deviceSupportAccountIds = _ascDeviceRegistrationService.GetDeviceSalesOrderSupportAccountId(serialNumber);
            var belongsToAnotherSupportAccount = deviceSupportAccountIds.Any(saId => saId != supportAccountId);
            if (!belongsToAnotherSupportAccount)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                   // SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            //We're only supporiting
            if (deviceSupportAccountIds.Count() > 1)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                    OneToManyMappingException = true,
                   // SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            //Verify that customer support account has one or more unregistered bkln licenses in it's sales order history
            var hasBklnLicenses = _ascDeviceRegistrationService.HasBklnSupportInSalesOrder(serialNumber, supportAccountId);

            if (!hasBklnLicenses)
            {
                return new AscCustResult
                {
                    ShowAscCustOptions = false,
                   // SerialNumberIsPanorama = isPanorama,
                    LegacyLightCyberDeviceName = lightCyberDeviceName
                };
            }

            var customerSupportAccountId = deviceSupportAccountIds.Single();
            var customerSupportAccountName = _ascDeviceRegistrationService.GetSupportAccountNameBySupportAccountId(customerSupportAccountId);

            //Then they're ok
            return new AscCustResult
            {
                ShowAscCustOptions = true,
                CustomerSupportAccountName = customerSupportAccountName,
                CustomerSupportAccountId = customerSupportAccountId,
               // SerialNumberIsPanorama = isPanorama,
                LegacyLightCyberDeviceName = lightCyberDeviceName
            };
        }


        #region Device Registration - Validation
        // Blocked CSP IDs for new firewall registrations
        private static readonly HashSet<long> BlockedFirewallRegistrationCspIds = new HashSet<long> { 1722357 };

        [System.Web.Http.HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ValidateSerialNumber(string serialNumber, int deviceType, long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                // Block new firewall registrations for specific CSP IDs (except spare devices)
                if (BlockedFirewallRegistrationCspIds.Contains(supportAccountId) && !_deviceLogic.IsSpareDevice(serialNumber))
                {
                    return this.Json(new DeviceRegistrationValidationResponse()
                    {
                        ErrorMessage = "New firewall registrations are blocked for this account. Please contact Support."
                    });
                }

                DeviceRegistrationValidationResponse response = new DeviceRegistrationValidationResponse()
                {
                    HideLocationInfo = true,
                    AscCustomOptions = new AscCustResult()
                };

                //Check for Panorama
                response.HideLocationInfo = IsPanoramaSerialNumber(serialNumber);
                response.AscCustomOptions.SerialNumberIsPanorama = response.HideLocationInfo;

                if (deviceType == Convert.ToInt32(DeviceType.CLOUDAZURE) || deviceType == Convert.ToInt32(DeviceType.CLOUDAWS))
                {
                    string validationMessage = string.Empty;
                    response.HideLocationInfo = true;
                    return this.Json(response);
                }

                if (deviceType == Convert.ToInt32(DeviceType.NGFW))
                {
                    bool isRegistered = false;

                    //SN Valid and not registered
                    var message = _deviceLogic.IsValidSerialNumber(serialNumber, supportAccountId, out isRegistered);
                    response.IsRegistered = isRegistered;

                    if (!string.IsNullOrEmpty(message))
                    {
                        response.ErrorMessage = message;
                        return this.Json(response);
                    }
                }

                response.AscCustomOptions.LegacyLightCyberDeviceName = _deviceLogic.GetLegacyLightCyberDeviceName(serialNumber);

                //ASC registering SN
                bool isAscUser = _userProfileDataAccess.IsAscUserBySfdcLoginId(string.Empty, userAccountId);
                
                if (isAscUser)
                {
                    AscCustResult ascCustOptions = ShouldShowAscOptions(serialNumber, supportAccountId, true);
                   response.AscCustomOptions = ascCustOptions;
                   response.AscCustomOptions.SerialNumberIsPanorama = response.HideLocationInfo; // to avoid overriding by above line
                }

                //SN 7000 series
                bool isChassis = _deviceLogic.IsChassisSerialNumber(serialNumber);
                if (isChassis)
                {
                    response.ShowComponents = true;
                    response.ComponentDetails = _deviceLogic.GetChassisUnassignedComponentQuantities(serialNumber).ToList();
                }

                //Validation for CSSP SerialNumber
                if (deviceType == Convert.ToInt32(DeviceType.CSSP))
                {
                    string validationMessage = string.Empty;

                    var result = _deviceLogic.ValidateEndUserSerialNumber(serialNumber, out validationMessage);
                    response.ErrorMessage = validationMessage;
                }

               

                //Eval Account Validation - check if registering a production device
                bool isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);
                if (isEvalSupportAccount)
                {
                    bool isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);

                    if (!isEvalDevice)
                        response.ErrorMessage = Constants.EvalAccountMessage;
                }

                //Check if account has ESA
                 response.ShowEsa=  deviceType == 0 && _siteLicenseLogic.HasEsaLicense(supportAccountId);
                //Check if account has 4HR ESA
                response.Show4HREsa = deviceType == 0 && _siteLicenseLogic.Has4HREsaLicense(supportAccountId);
                // get ELA/ESA Caps and the device price
                //IEnumerable<ElaEsaCapInfoWithNewDevice> elaCaps = Task.Run(async () =>
                //{
                //    Task<IEnumerable<ElaEsaCapInfo>> taskElaCapsInfo = elaEsaLogic.GetElaCapsAsync(supportAccountId);
                //    /* Requirement has changed in 
                //     * https://confluence.paloaltonetworks.com/display/IA/ELA+Revamp+-+Assets+allocation+thresholds#ELARevampAssetsallocationthresholds-ElaDecomDevicesCompleteend-point
                //     * The new requirement does not ask for price of the device. Instead, it asks to show link to ELA/ESA Caps.
                //    Task<ElaEsaDevicePriceInfo> taskDevicePriceInfo = elaEsaLogic.GetElaDevicePriceAsync(supportAccountId, serialNumber?.ToUpper());
                //    await Task.WhenAll(taskElaCapsInfo, taskDevicePriceInfo);
                //    IEnumerable<ElaEsaCapInfo> elaCapsInfo = taskElaCapsInfo.Result;
                //    ElaEsaDevicePriceInfo elaDevicePriceInfo = taskDevicePriceInfo.Result;
                //    if (elaCapsInfo.Any()
                //        && elaDevicePriceInfo != null)
                //    {
                //        return elaCapsInfo.Select(i => i.ToElaEsaCapInfoWithNewDevice(elaDevicePriceInfo)).Where(i => i != null).ToArray();
                //    }
                //    */
                //    await Task.WhenAll(taskElaCapsInfo);
                //    IEnumerable<ElaEsaCapInfo> elaCapsInfo = taskElaCapsInfo.Result;
                //    if (elaCapsInfo.Any())
                //    {
                //        return elaCapsInfo.Select(i => i.ToElaEsaCapInfoWithNewDevice()).Where(i => i != null).ToArray();
                //    }
                //    return null;
                //}).ConfigureAwait(false).GetAwaiter().GetResult();
                //response.ElaEsaCapInfo = elaCaps?.Any() ?? false ? elaCaps : null;
                response.ElaEsaCapInfo = null;
                return this.Json(response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(false);
            }

        }

        [System.Web.Http.HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ValidateBacklineAuthcode(string serialNumber,  long supportAccountId)
        {
            return this.Json(_ascDeviceRegistrationService.HasBklnSupportInSalesOrder(serialNumber, supportAccountId));
        }


        [System.Web.Http.HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult ValidateCsspReferenceId(string serialNumber, string customerAccountId)
        {
            string validationMessage= string.Empty;
            try
            {
                bool result = false;

                if (string.IsNullOrEmpty(serialNumber))
                {
                    result = _deviceLogic.ValidateEndUserCustomerAccountId(customerAccountId, out validationMessage);
                    if (!result)
                    {
                        return this.Json(validationMessage);
                    }
                }
                else
                {
                    result = _deviceLogic.ValidateCsspEndUserInputCombination(serialNumber, customerAccountId, out validationMessage);
                    if (!result)
                    {
                        return this.Json(validationMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json("An error occured. Please contact Support");
            }
            return this.Json(validationMessage);
        }


        [System.Web.Http.HttpPost]
        [JwtTokenAuthentication]
        public IHttpActionResult ValidateUploadedFile([FromBody]ParseVMRequest parseVmRequest)
        {
            //Expected string input : data:text/plain;base64,VVVJRDogVEhJUy1JUy1NWS1VVUlEDQpDUFVJRDogVEhJUy1JUy1NWS1DUFVJRA==
            ParseVmFileResult result = null;
            string uploadUuIdCpuId = parseVmRequest.fileContent;
            try
            {
                if (!string.IsNullOrEmpty(uploadUuIdCpuId))
                {
                    string[] strencodedStrings = uploadUuIdCpuId.Split(';');
                    string strencodedContentType = strencodedStrings.Length > 0 ? strencodedStrings[0] : string.Empty;

                    string[] strencodedContentTypesStrings = strencodedContentType.Split(':');

                    string contentType = strencodedContentTypesStrings.Length > 1
                        ? strencodedContentTypesStrings[1]
                        : string.Empty;
                    
                    string[] rawData = uploadUuIdCpuId.Split(',');
                    string fileContent = rawData.Length > 1 ? rawData[1] : string.Empty;

                    if (string.IsNullOrEmpty(contentType) || (contentType != "text/plain" && contentType != "text/xml"))
                    {
                        return this.Json(new ParseVmFileResult()
                        {
                            Message = "The file format is not supported"
                        });
                    }

                    if (string.IsNullOrEmpty(fileContent))
                    {
                        return this.Json(new ParseVmFileResult()
                        {
                            Message = "Invalid file content"
                        });
                    }

                    byte[] data = Convert.FromBase64String(fileContent);
                    string decodedContent = Encoding.UTF8.GetString(data);
                    result = _virtualMachineLogic.ParseVmFileByContent(EVmType.Vm, decodedContent, contentType);

                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(new ParseVmFileResult()
                {
                    Message = "An error occured. Please contact Support"
                });
            }

            return Json(result);
        }
        
       #endregion
    }

}
