using System.Web.Mvc;
using System.Web.UI;
using Csp.Api.Services.Contracts;
using System;
using Csp.Api.Security;
using Csp.Web.Security.Attributes;
using Microsoft.Security.Application;

namespace Csp.Web.Controllers {
	[OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
	public class ValidationController : BaseController {

		private readonly IDeviceLogic _deviceLogic;
		private readonly IUserAccountLogic _userAccountLogic;
		private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;

		public ValidationController(IDeviceLogic deviceLogic, IUserAccountLogic userAccountLogic, IAscDeviceRegistrationService ascDeviceRegistrationService) {
			_deviceLogic = deviceLogic;
			_userAccountLogic = userAccountLogic;
			_ascDeviceRegistrationService = ascDeviceRegistrationService;
		}

		public JsonResult IsDisplayNameAvailable([Bind(Prefix = "UserAccount.DisplayName")] string displayName,
																						 [Bind(Prefix = "UserAccount.Id")] long id) {
            displayName = Sanitizer.GetSafeHtmlFragment(displayName);
			var isDisplayNameExists = UserAccountLogic.IsDisplayNameExists(displayName, id);

			if (!isDisplayNameExists)
				return Json(true, JsonRequestBehavior.AllowGet);

			var message = string.Format("{0} is not available.", displayName);
			return Json(message, JsonRequestBehavior.AllowGet);
		}

		public JsonResult IsValidNonPublicEmail(string email) {
			if (email == null) {
				email = Request.QueryString["UserAccount.Email"];
			}

			if (email != null) {
				email = email.Trim();
			}

			var isValid = _userAccountLogic.IsValidNonPublicEmail(email);

			if (isValid)
				return Json(true, JsonRequestBehavior.AllowGet);

			var message = string.Format("Public domains are not allowed. Please use your company email address.");
			return Json(message, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, CspAuthorize(Permission.EditDevice)]
		public JsonResult IsValidSerial([Bind(Prefix = "Device.SerialNumber")] string serialNumber, [Bind(Prefix = "Device.Customer.Id")] long supportAccountId) {

            //var message = _deviceLogic.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
            if (!string.IsNullOrEmpty(serialNumber))
            {
                var message = _deviceLogic.IsValidSerial(serialNumber, supportAccountId);

                if (!string.IsNullOrEmpty(message))
                {
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
            }
            ////Check if the device being registered is not a processing card
            //var isProcessingCard = _deviceLogic.IsProcessingCard(serialNumber);
            //if (isProcessingCard)
            //{
            //    return Json("Please go to Line Cards/Optics/FRUs page to register processing cards.", JsonRequestBehavior.AllowGet);
            //}

            //var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
            //var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

            //if (isEvalSupportAccount && !isEvalDevice) {
            //    return Json("To register a new purchased asset, the Eval account must be converted to an active CSP account.  Please go to the Company Account page to activate your account.", JsonRequestBehavior.AllowGet);
            //}

            return Json(true, JsonRequestBehavior.AllowGet);

        }

				[HttpGet, CspAuthorize(Permission.EditDevice)]
        public JsonResult IsValidSerialAndRegistered([Bind(Prefix = "ChassisComponent.ChassisSerialNumber")] string serialNumber)
        {
            var message = _deviceLogic.CheckIsSerialNumberValidandAndRegisterd(serialNumber);
            if (message == "")
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.EditDevice)]
        public JsonResult ValidateComponentQuantity([Bind(Prefix = "ChassisComponent.Quantity")]Int64 quantity, [Bind(Prefix = "ChassisComponent.ActualQuantity")]Int64 aclQuantity, [Bind(Prefix = "ChassisComponent.PartNumber")] string PartNumber, [Bind(Prefix = "ChassisComponent.SupportAccountId")] Int64 SupportAccountId)
        {            
            var availableQuantity = _deviceLogic.GetUnassignedComponentQuantity(PartNumber, SupportAccountId);         

            if (quantity > (availableQuantity + aclQuantity))
                return Json("Maximum available quantity " + (availableQuantity + aclQuantity), JsonRequestBehavior.AllowGet);
            else
                return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.EditDevice)]
        public JsonResult IsSalesOrderComponentsRegistered([Bind(Prefix = "ChassisComponent.SalesOrderNumber")]string salesOrderNumber)
        {
            var message = _deviceLogic.IsSalesOrderComponentsRegistered(salesOrderNumber);

            if ( message != "")
                return Json(message, JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.EditDevice)]
		public JsonResult IsValidAwsInstance([Bind(Prefix = "Device.InstanceId")] string instanceId, [Bind(Prefix = "Device.Customer.Id")] long supportAccountId) {

			var isAwsInstanceRegistered = _deviceLogic.IsInstanceAlreadyRegistered(instanceId);

			if (isAwsInstanceRegistered) {
				return Json("AWS Instance already registered", JsonRequestBehavior.AllowGet);
			}
			
			return Json(true, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public JsonResult IsAuthorizationCodeValid([Bind(Prefix = "ActivateLicense.AuthorizationCode")]string authorizationCode, [Bind(Prefix = "ActivateLicense.SerialNumber")]string serialNumber) {
			var message = LicenseLogic.IsAuthorizationCodeValid(authorizationCode, serialNumber);

			var isAuthorizationCodeValid = (message == "");

			if (isAuthorizationCodeValid) {
				return Json(true, JsonRequestBehavior.AllowGet);
			}

			return Json(message, JsonRequestBehavior.AllowGet);
		}

				[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public JsonResult IsAuthorizationCodeValidForUpgrade([Bind(Prefix = "ActivateLicense.UpgradeAuthorizationCode")]string authorizationCode, [Bind(Prefix = "ActivateLicense.SerialNumber")]string serialNumber)
        {
            var message = LicenseLogic.IsAuthorizationCodeValidForUpgrade(authorizationCode, serialNumber);

            var isAuthorizationCodeValid = (message == "");

            if (isAuthorizationCodeValid)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public JsonResult IsSiteLicenseAuthorizationCodeValid([Bind(Prefix = "AuthCode")]string authCode)
	    {
            var message = LicenseLogic.IsSiteLicenseAuthorizationCodeValid(authCode);

            var isAuthorizationCodeValid = string.IsNullOrEmpty(message);

            if (isAuthorizationCodeValid)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(message, JsonRequestBehavior.AllowGet);
	    }

		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
		public JsonResult IsValidBklnAuthCode([Bind(Prefix = "AscCustBklnSupportAuthCode")]string ascCustBklnSupportAuthCode, [Bind(Prefix = "Device.Customer.Id")]long supportAccountId, [Bind(Prefix = "Device.SerialNumber")]string serialNumber) {
			var message = LicenseLogic.IsAuthorizationCodeValid(ascCustBklnSupportAuthCode, serialNumber);

			var isAuthorizationCodeValid = (message == "");

			if (!isAuthorizationCodeValid) {
				if (message == "Authorization code is not compatible.") {
					message = "Error: Invalid authorization code. Product mismatch. Please contact Support for assistance.";
				}
				return Json(message, JsonRequestBehavior.AllowGet);
			}

			//verify it is not registered and is bkln support
			var isRegistered = _ascDeviceRegistrationService.IsRegisteredAuthCode(ascCustBklnSupportAuthCode);
			if (isRegistered) {
				return Json("Must be an un-applied Authorization Code", JsonRequestBehavior.AllowGet);
			}

			var isBkln = _ascDeviceRegistrationService.IsBklnAuthCode(ascCustBklnSupportAuthCode);

			if (!isBkln) {
				return Json("Must be a Support Authorization Code", JsonRequestBehavior.AllowGet);
			}

			var bklnSupportCenterSupportAccountId =
									_ascDeviceRegistrationService.GetBklnSupportCenterSupportAccountId(ascCustBklnSupportAuthCode);
			var isCorrectBklnForAsc = bklnSupportCenterSupportAccountId == supportAccountId;

			if (!isCorrectBklnForAsc) {
				return Json("Error: Invalid authorization code.  ASC mismatch. Please contact Support for assistance.", JsonRequestBehavior.AllowGet);
			}

			//Otherwise they're good!
			return Json(true, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, CspAuthorize(Permission.RegisterDevice)]
		public JsonResult AccountConversionDeviceCodeIsValid([Bind(Prefix = "AccountConversionDeviceCode")]string accountConversionDeviceCode, [Bind(Prefix = "ConvertAccount")]bool convertAccount) {
			if (convertAccount && string.IsNullOrEmpty(accountConversionDeviceCode)) {
				return Json("Devie Code is required", JsonRequestBehavior.AllowGet);
			}

			return Json(true, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, CspAuthorize(Permission.RegisterDevice)]
		public JsonResult AccountConversionCustomerCodeIsValid([Bind(Prefix = "AccountConversionCustomerCode")]string accountConversionCustomerCode, [Bind(Prefix = "ConvertAccount")]bool convertAccount) {
			if (convertAccount && string.IsNullOrEmpty(accountConversionCustomerCode)) {
				return Json("Customer Id or Sales Order Number is required", JsonRequestBehavior.AllowGet);
			}

			return Json(true, JsonRequestBehavior.AllowGet);
		}

        public JsonResult ValidateCsspEndUserSerialNumber([Bind(Prefix = "Device.CsspSerialNumber")] string serialNumber)
        {
            string validationMessage;
            return ValidateEndUserSerialNum(serialNumber, out validationMessage);
        }

        public JsonResult ValidateCsspEndUserCustomerAccountId([Bind(Prefix = "Device.CustomerAccountId")] string customerAccountId, [Bind(Prefix = "Device.CsspSerialNumber")] string serialNumber)
        {
            string validationMessage;
            return ValidateEndUserRefNum(serialNumber,customerAccountId, out validationMessage);
        }

        public JsonResult ValidateCsspEndUserSerialNumberForNewUserRegistration([Bind(Prefix = "CsspSerialNumber")] string serialNumber)
        {
            string validationMessage;
            return ValidateEndUserSerialNum(serialNumber, out validationMessage);
        }

        public JsonResult ValidateCsspEndUserCustomerAccountIdForNewUserRegistration([Bind(Prefix = "CustomerAccountId")] string customerAccountId, [Bind(Prefix = "CsspSerialNumber")] string serialNumber)
        {
            string validationMessage;
            return ValidateEndUserRefNum(serialNumber, customerAccountId, out validationMessage);
        }

        public JsonResult ValidateDeviceForOtp([Bind(Prefix = "SerialNumber")] string serialNumber, [Bind(Prefix = "SupportAccountID")] long supportAccountId)
	    {
	        var isValid = _deviceLogic.IsValidDeviceForOtp(serialNumber, supportAccountId);
            if (isValid)
	            return Json(true, JsonRequestBehavior.AllowGet);
	        else
	            return Json("Invalid Serial Number for OTP generation", JsonRequestBehavior.AllowGet);
	    }
	    
        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        private JsonResult ValidateEndUserSerialNum(string serialNumber, out string validationMessage)
	    {
            var result = _deviceLogic.ValidateEndUserSerialNumber(serialNumber, out validationMessage);
            if (!result)
            {
                return Json(validationMessage, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
	    }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        private JsonResult ValidateEndUserRefNum(string serialNumber,string customerAccountId, out string validationMessage)
        {
            bool result = false;
            if (string.IsNullOrEmpty(serialNumber))
            {
                result = _deviceLogic.ValidateEndUserCustomerAccountId(customerAccountId, out validationMessage);
                if (!result)
                {
                    return Json(validationMessage, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                result = _deviceLogic.ValidateCsspEndUserInputCombination(serialNumber, customerAccountId, out validationMessage);
                if (!result)
                {
                    return Json(validationMessage, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

	}
}
