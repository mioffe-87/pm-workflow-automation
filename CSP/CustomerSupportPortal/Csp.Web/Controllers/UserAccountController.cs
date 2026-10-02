/* 
 * *********************************!IMPORTANT CODE THAT IS DIFFERENT!*****************************************
 * 
 * The action methods in this controller need to be accessed from a secure call for a user that has permission 
 * to view information for any user that is visible. In addition the methods need to also be accessed from 
 * methods where the user is only authenticated with no permissions, in which case they can only see their
 * user account. Therefore each action method is not using the standard CspAuthorize attribute but is instead 
 * checking the permission inside of the action method in the return of the result. 
 * If the user does not have permission, or the id is not the same as the user, 
 * then the action methods should return null. only has Authorize to check authentication.
 * If you are reading this far, I would also like to share that in my opinion Han shot first. 
 * 
 * -Jason Sacks
 * 
 * */

using AutoMapper;
using Csp.Api.EntityClasses;
using Csp.Api.Exceptions;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.LoginService;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using Csp.Web.Extensions;
using NotificationCategory = Csp.Api.Model.Enums.NotificationCategory;
using static Csp.Api.HelperClasses.WebCommon;

namespace Csp.Web.Controllers {

	public class UserAccountController : BaseController {

		private readonly ILogLogic _logLogic;
		private readonly IDeviceLogic _deviceLogic;
		private readonly IUserAccountLogic _userAccountLogic;
		private readonly ILoginServiceLogic _loginServiceLogic;
		private readonly ILookupLogic _lookupLogic;
		private readonly ISupportAccountLogic _supportAccountLogic;
		private readonly IGroupLogic _groupLogic;
		private readonly ISupportAccountDomainsService _supportAccountDomainsService;
        private readonly IAwsLogic _awsLogic;
	    private readonly ICloudDeviceLogic _cloudDeviceLogic;
	    private readonly ILicenseLogic _licenseLogic;
	    private readonly IMembershipLogic _membershipLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly IApplicationSettings _applicationSettings;
        private readonly string fedramp_topic = ConfigurationManager.AppSettings.Get("Fedramp_TopicName");
        private readonly string fedramp_project = ConfigurationManager.AppSettings.Get("Fedramp_ProjectName");
     
        public UserAccountController(IDeviceLogic deviceLogic, ILogLogic logLogic, IUserAccountLogic userAccountLogic, 
            ILoginServiceLogic loginServiceLogic, ILookupLogic lookupLogic, ISupportAccountLogic supportAccountLogic, 
            IGroupLogic groupLogic, ISupportAccountDomainsService supportAccountDomainsService, IAwsLogic awsLogic,
            ICloudDeviceLogic cloudDeviceLogic, ILicenseLogic licenseLogic,IMembershipLogic membershipLogic,
            ISecurityLogic securityLogic, IApplicationSettings applicationSettings)
        {
			_deviceLogic = deviceLogic;
			_logLogic = logLogic;
			_userAccountLogic = userAccountLogic;
			_loginServiceLogic = loginServiceLogic;
			_lookupLogic = lookupLogic;
			_supportAccountLogic = supportAccountLogic;
			_groupLogic = groupLogic;
			_supportAccountDomainsService = supportAccountDomainsService;
            _awsLogic = awsLogic;
            _cloudDeviceLogic = cloudDeviceLogic;
            _licenseLogic = licenseLogic;
            _membershipLogic = membershipLogic;
            _securityLogic = securityLogic;
            _applicationSettings = applicationSettings;
        }

		#region Private members

		private bool IsUserAuthorized(long id, IEnumerable<Permission> permissions) {
			//scroll to top to see important comment -- oh and my expression here confuses resharper, even if in a simple foreach rather than linq

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			var result = permissions.Aggregate(false, (current, permission) => current ? current : User.HasPermission(permission));
			return result ? result : User.UserAccount.Id == id;
			// ReSharper restore ConditionIsAlwaysTrueOrFalse
		}

		private string GetRedirectUrlIfUserExists() {
			var ssoToken = UserHelper.GetSsoToken();

			if (ssoToken != null) {
				if (ssoToken["email"] != null) {
					var email = ssoToken["email"].ToString();
					var userAccountExists = _userAccountLogic.IsEmailExists(email);

					if (userAccountExists) {
						return string.Format("{0}{1}", SiteHelper.GetBaseUrl(),
                                                                 Url.Action("MyAccounts", "SupportAccount"));
					}
				}
			}

			return string.Empty;
		}

        #endregion

        [HttpGet]
        public ActionResult PreRegister(string code = null, string message = "")
        {
            return RedirectToAction("Register", "Home", new { code = code, message = message });
        }

	    [HttpPost]
	    public ActionResult DeviceType(DeviceTypeDto model)
	    {
            return Redirect(Url.ActionSecure("Register", "UserAccount", new { email = model.Email, deviceType = model.DeviceType, cloudType = model.CloudType }));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult PreRegister(UserAccountVerificationDto model)
        {
            var loginServiceLogic = new LoginServiceLogic();

            var domainResponseMessage = _userAccountLogic.GetDomainDetails(model.Email.Substring
                    (model.Email.IndexOf('@') + 1));
            if (domainResponseMessage.Contains(ApiConstants.BlockListedDomainMessage))
            {
                return RedirectToAction("PreRegister", new { message = domainResponseMessage });
            }
            if (!string.IsNullOrEmpty(model.SelfRegistrationCode))
            {
                var SelfRegistrationCodeResponse = _userAccountLogic.IsValidSelfRegistrationCode(model.SelfRegistrationCode);
                if (SelfRegistrationCodeResponse?.SupportAccountId > 0)
                {
                    if (!_userAccountLogic.CanAddEmailToSupportAccount(SelfRegistrationCodeResponse.SupportAccountId, model.Email))
                    {
                        _logLogic.LogNewUser(new Lognewuser()
                        {
                            Email = model.Email,
                            EventType = "Email validation failed at Pre-registration",
                            EventDescription = "Invalid domain: " + domainResponseMessage,
                            CreatedOn = DateTime.Now
                        });
                        return RedirectToAction("PreRegister", new { message = domainResponseMessage });
                    }
                }
            }

            //Google ReCaptcha Serverside validation
            CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);
            if (!response.Success)
            {
                _logLogic.LogNewUser(new Lognewuser()
                {
                    Email = model.Email,
                    EventType = "ReCaptcha validation failed at Pre-registration",
                    EventDescription = "Error From Google ReCaptcha API: " + Convert.ToString(response.ErrorMessage[0]),
                    CreatedOn = DateTime.Now
                });

                ViewBag.CaptchaError = "Captcha validation failed.Please try again";
                return RedirectToAction("PreRegister");
            }
            else
            {
                ViewBag.CaptchaError = "";
                ViewBag.BlockedDomainError = "";
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("PreRegister");
            }

            _logLogic.LogNewUser(new Lognewuser()
            {
                Email = model.Email,
                EventType = "IsProfileExists",
                EventDescription = "New User Registration: Calling IsProfileExists method of LoginService",
                CreatedOn = DateTime.Now
            });

            var isProfileExists = loginServiceLogic.IsProfileExists(model.Email, Common.GetClientIp());

            _logLogic.LogNewUser(new Lognewuser()
            {
                Email = model.Email,
                EventType = "IsEmailExists",
                EventDescription = "New User Registration: Calling IsEmailExists method of UserAccountLogic",
                CreatedOn = DateTime.Now
            });

            var userAccountExists = _userAccountLogic.IsEmailExists(model.Email);
            if (userAccountExists)
            {
                return Redirect(Url.ActionSecure("UserAccountAlreadyExists", "UserAccount", new { isSsoProfileExists = isProfileExists }));
            }

            if (!string.IsNullOrEmpty(model.SelfRegistrationCode))
            {
                return Redirect(Url.ActionSecure("Register", "UserAccount", new { email = model.Email, selfRegistrationCode = model.SelfRegistrationCode }));
            }

            return Redirect(Url.ActionSecure("DeviceType", "UserAccount", new { email = model.Email }));
        }

		[HttpGet]
        [EncryptedActionParameter]
		public ActionResult UserAccountAlreadyExists(bool isSsoProfileExists) {
			string returnUrl;
			var loginServiceReturnUrl = ConfigurationManager.AppSettings["LoginUrl"];
			var accountSelectionUrl = string.Format("&RelayState={0}{1}", SiteHelper.GetBaseUrl(),
                                                                                            Url.Action("MyAccounts", "SupportAccount"));

			if (isSsoProfileExists) {
				var redirectUrl = string.Format("&RelayState={0}{1}", SiteHelper.GetBaseUrl(),
																				Url.Action("Register", "UserAccount"));

				returnUrl = string.Format("{0}{1}", loginServiceReturnUrl, redirectUrl);
			} else {
				returnUrl = string.Format("{0}{1}", loginServiceReturnUrl, accountSelectionUrl);
			}

			ViewBag.ReturnUrl = returnUrl;
			ViewBag.ActionHelper = new ActionHelper(ControllerFields.UserAccountController);

			return View();
		}

        [HttpGet]
        [EncryptedActionParameter]
        public ActionResult DeviceType(string email)
        {
            string emailAddress = email ?? "";

            if (emailAddress == string.Empty)
            {
                return RedirectToAction("PreRegister");
            }

            var model = new DeviceTypeDto() {Email = emailAddress, DeviceType = 1};
            model.CloudTypes = _cloudDeviceLogic.GetCloudTypes();
            return View(model);
        }

		[HttpGet]
        [EncryptedActionParameter]
		public ActionResult Register(string email, int deviceType = (int)DeviceTypes.PanDevice, int cloudType = 0, string selfRegistrationCode = null) {
			var redirectUrl = GetRedirectUrlIfUserExists();

			if (!string.IsNullOrEmpty(redirectUrl)) {
				return Redirect(redirectUrl);
			}

			var ssoToken = UserHelper.GetSsoToken();
            

			string emailAddress = null;

		    if (deviceType == (int) DeviceTypes.CloudDevice)
            {
                cloudType = (cloudType > 0) ? cloudType : 1;
            }

			bool isUserExists = false;

            //If this is a valid "live" user w/o a CSP account we'll try to pre-fill everythign
			if (ssoToken != null) {
				emailAddress = ssoToken["email"].ToString();
				isUserExists = true;
			} 
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = email ?? "";

				if (emailAddress == string.Empty) {
					return RedirectToAction("PreRegister");
				}
			}

			var otherRegion = string.Empty;
			var userAccount = new UserAccount {
				Email = emailAddress,
				IsOptInEmail = true,
				IsOptInSecurityEmail = true,
				IsOptInPanOsUpdateEmail = true,
			};

			if (isUserExists) {
				var countryCode = UserHelper.GetTokenByKey("country", ssoToken);
				var regionCode = UserHelper.GetTokenByKey("state", ssoToken);
				var countryId = _lookupLogic.GetCountryIdByCode(countryCode);
				long regionId = 0;

				if (countryCode == CountryCode.US.ToDescription() || countryCode == CountryCode.CA.ToDescription()) {
					regionId = _lookupLogic.GetRegionIdByCode(regionCode, countryId);
				} else {
					otherRegion = regionCode;
				}

				userAccount.FirstName = UserHelper.GetTokenByKey("firstname", ssoToken);
				userAccount.LastName = UserHelper.GetTokenByKey("lastname", ssoToken);
				userAccount.Title = UserHelper.GetTokenByKey("title", ssoToken);
				userAccount.Address = new Address() {
					Address1 = UserHelper.GetTokenByKey("address1", ssoToken),
					Address2 = UserHelper.GetTokenByKey("address2", ssoToken),
					City = UserHelper.GetTokenByKey("city", ssoToken),
					Country = countryId.ToString(),
					Region = regionId.ToString(),
					PostalCode = UserHelper.GetTokenByKey("zip", ssoToken)
				};

				userAccount.PhoneNumber = UserHelper.GetTokenByKey("phone", ssoToken);
				userAccount.Email = UserHelper.GetTokenByKey("email", ssoToken);
			}


            var userAccountRegistrationDto = new UserAccountRegistrationDto
            {
                UserAccount = userAccount,
                Countries = _lookupLogic.GetCountries().ToList(),
                OtherRegion = otherRegion,
                EmailAddress = emailAddress,
                IsUserExists = isUserExists,
                ActionHelper = new ActionHelper(ControllerFields.UserAccountController),
                SelfRegistrationCode = selfRegistrationCode
            };

		    if (!string.IsNullOrEmpty(userAccountRegistrationDto.SelfRegistrationCode))
		    {
		        userAccountRegistrationDto.DeviceCode =
		            userAccountRegistrationDto.CustomerCode = userAccountRegistrationDto.SelfRegistrationCode;
		    }

            if (deviceType == (int)DeviceTypes.CloudDevice)
            {
                userAccountRegistrationDto.IsUsingCloudDeviceRegistration = true;
                //userAccountRegistrationDto.CloudProducts = _cloudDeviceLogic.GetCloudProducts(cloudTypeId: cloudType).ToList();
                //userAccountRegistrationDto.CloudRegions = _cloudDeviceLogic.GetCloudRegions(cloudTypeId: cloudType).ToList();
                switch (cloudType)
                {
                    case 1:
                        userAccountRegistrationDto.IsAwsRegistration = true;
                        break;
                    case 2:
                        userAccountRegistrationDto.IsAzureRegistration = true;
                        break;
                    case 3:
                        userAccountRegistrationDto.IsCsspRegistration = true;
                        break;
                    case 5:
                        userAccountRegistrationDto.IsGoogleCloudRegistration = true;
                        break;
                }
            }
            return View(userAccountRegistrationDto);
		}


        
		[HttpPost, AjaxOnly]
		public async Task<ActionResult> Register(UserAccountRegistrationDto model) {
			var countryId = model.UserAccount.Address.CountryId ?? 0;
			var countryCode = _lookupLogic.GetCountryCodeById(countryId);
            long systemUserAccountId;
            long.TryParse(ConfigurationManager.AppSettings["SystemUserAccountId"], out systemUserAccountId);
			string systemUseAccountEmail = "donotreplycsp@paloaltonetworks.com";
            long deviceId = 0;
		    long azureDeviceId = 0;
            string cloudAccountId = string.Empty;
            string errorMessage = string.Empty;
            bool isSelfRegistration = false;
            bool isPartialShipmentUser = false;
            long supportAccountIdToValidate = 0;

            //Google ReCaptcha Serverside validation
            CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);
		    if (!response.Success)
		    {
		        _logLogic.LogNewUser(new Lognewuser()
		        {
		            Email = model.UserAccount.Email,
		            EventType = "ReCaptcha validation failed at Regsitration",
		            EventDescription = "Error From Google ReCaptcha API: " + Convert.ToString(response.ErrorMessage[0]),
		            CreatedOn = DateTime.Now
		        });

		        ViewBag.CaptchaError = "Captcha validation failed.Please try again";
		        ModelState.Clear();
		        return PartialView("../Partial/_UserRegistration", model);
		    }
		    else
		    {
		        ViewBag.CaptchaError = "";
		    }

			if (countryCode == CountryCode.US.ToDescription() ||
					countryCode == CountryCode.CA.ToDescription()) {
				// Ignore OtherState validation
				ModelState.Remove("OtherRegion");
			} else {
				// Ignore State/Province validation
				ModelState.Remove("UserAccount.Address.Region");
			}
            
		
			model.Countries = _lookupLogic.GetCountries().ToList();
			model.ActionHelper = new ActionHelper(ControllerFields.UserAccountController);

		    if (model.IsUsingCloudDeviceRegistration)
		    {
		        ModelState.Remove("DeviceCode");
		        ModelState.Remove("CustomerCode");
		        if (model.IsAwsRegistration)
		        {
		            //model.CloudProducts = _cloudDeviceLogic.GetCloudProducts(1).ToList();
		            //model.CloudRegions = _cloudDeviceLogic.GetCloudRegions(1).ToList();
		        }
		        else if (model.IsAzureRegistration)
		        {
		            //model.CloudProducts = _cloudDeviceLogic.GetCloudProducts(2).ToList();
		            //model.CloudRegions = _cloudDeviceLogic.GetCloudRegions(2).ToList();
		        }
                if (!model.IsAwsRegistration && !model.IsAzureRegistration)
		        {
		            model.IsCsspProvisionedDevice = true;
		            ModelState.Remove("SupportAccountName"); // CSSP
		        }
		    }
		    else
		    {
                ModelState.Remove("CsspSerialNumber"); 
                ModelState.Remove("CustomerAccountId"); 
		    }

			if (!ModelState.IsValid) {
				ModelState.Clear();
				return PartialView("../Partial/_UserRegistration", model);
			}

            if (model.IsUsingCloudDeviceRegistration)
            {
                if (model.IsAwsRegistration || model.IsGoogleCloudRegistration)
                {
                    // AWS Info validation
                    string serialNumber = model.AwsSerialNumber, cpuid = model.AwsCpuid, uuid = model.AwsUuid;
                    if (model.IsGoogleCloudRegistration)
                    {
                        serialNumber = model.GoogleCloudSerialNumber;
                        cpuid = model.GoogleCloudCpuid;
                        uuid = model.GoogleCloudUuid;
                    }

                    bool validInstance = _licenseLogic.CreateCloudInstanceWithAccountId(serialNumber, cpuid, uuid,
                        systemUseAccountEmail, model.IsAwsRegistration, out deviceId, out cloudAccountId, out errorMessage);

                    if (validInstance == false)
                    {
                        model.Notifications.Add(
                            new ResponseNotification(errorMessage,
                                ResponseState.Error));

                        ModelState.Clear();
                        return PartialView("../Partial/_UserRegistration", model);
                    }
                    else
                    {
                        if (_deviceLogic.IsInstanceAlreadyRegistered(serialNumber))
                        {
                            errorMessage = string.Format("Validation failed: {0} Instance already registered", model.IsAwsRegistration ? "AWS" : "Google Cloud");
                            model.Notifications.Add(
                                new ResponseNotification(errorMessage,
                                    ResponseState.Error));

                            ModelState.Clear();
                            return PartialView("../Partial/_UserRegistration", model);
                        }
                    }
                }
                else if (model.IsAzureRegistration)
                {
                    bool validInstance = _deviceLogic.CreateAzureInstance(model.AzureSerialNumber,
                        model.AzureCpuid, model.AzureUuid, systemUseAccountEmail, out azureDeviceId,
                        out errorMessage);

                    if (validInstance == false)
                    {
                        model.Notifications.Add(
                            new ResponseNotification(errorMessage,
                                ResponseState.Error));

                        ModelState.Clear();
                        return PartialView("../Partial/_UserRegistration", model);
                    }
                    else
                    {
                        if (_deviceLogic.IsInstanceAlreadyRegistered(model.AzureSerialNumber))
                        {
                            errorMessage = "Validation failed: Azure Instance already registered";
                            model.Notifications.Add(
                                new ResponseNotification(errorMessage,
                                    ResponseState.Error));

                            ModelState.Clear();
                            return PartialView("../Partial/_UserRegistration", model);
                        }
                    }
                }
                else
                {
                    if (!_deviceLogic.ValidateCsspEndUserInputCombination(model.CsspSerialNumber, model.CustomerAccountId,
                        out errorMessage))
                    {
                        model.Notifications.Add(
                                new ResponseNotification(errorMessage,
                                    ResponseState.Error));

                        ModelState.Clear();
                        return PartialView("../Partial/_UserRegistration", model);
                    }
                }
            }
            else
            {
                ModelState.Remove("SupportAccountName");
                ModelState.Remove("AwsInstanceId");
                ModelState.Remove("AwsProductCode");
                ModelState.Remove("AwsRegion");
                ModelState.Remove("AzureSerialNumber");
                ModelState.Remove("AzureProductCode");
                ModelState.Remove("AzureRegion");
                ModelState.Remove("CsspSerialNumber");
                ModelState.Remove("CustomerAccountId");
            }

			var supportAccountId = model.SelectedSupportAccountId;
			var isMembershipPending = model.SelectedSupportAccountId > 0;

            if (model.IsUsingCloudDeviceRegistration == false && supportAccountId == 0)
            {
				_logLogic.LogNewUser(new Lognewuser() {
					Email = model.UserAccount.Email,
					EventType = "ValidateUserRegistration",
					EventDescription =
						string.Format(
							"New User Registration: Calling ValidateUserRegistration method of UserAccountLogic. Email: {0}, Display Name: {1}, DeviceCode: {2}, CustomerCode: {3}",
							model.UserAccount.Email, model.UserAccount.DisplayName, model.DeviceCode, model.CustomerCode),
					CreatedOn = DateTime.Now
				});

				var userRegistration =
					_userAccountLogic.ValidateUserRegistration(model.UserAccount.Email,
																										 model.UserAccount.DisplayName,
																										 model.DeviceCode,
																										 model.CustomerCode);

                isSelfRegistration = userRegistration.IsSelfRegistrationCode;                
                isPartialShipmentUser = userRegistration.IsPartialShipmentUser;

				if (!string.IsNullOrEmpty(userRegistration.ErrorMessage)) {
					model.Notifications.Add(new ResponseNotification(userRegistration.ErrorMessage, ResponseState.Error));

					ModelState.Clear();
					return PartialView("../Partial/_UserRegistration", model);
				}

				if (userRegistration.HasMultipleSupportAccountIds) {

					model.Notifications.Add(
						new ResponseNotification(
							"Error: unable to create user account.  Please contact Support",
							ResponseState.Error));

					ModelState.Clear();
					return PartialView("../Partial/_UserRegistration", model);
				}

				if (userRegistration.OfferToEmailSuperUsers) {
					model.Notifications.Add(
						new ResponseNotification(
							@"Error: unable to create user account.  Please send an <a href=""javascript: EmailSuperUsers();"" style="" text-decoration: underline;font-weight: bold;"">email to the Account’s Super User(s)</a> to request access or contact Support.",
							ResponseState.Error));
					ModelState.Clear();
					return PartialView("../Partial/_UserRegistration", model);
				}

				supportAccountId = userRegistration.SupportAccountId.Value;

				// Check if the support account id is still zero
				if (supportAccountId == 0) {
					model.Notifications.Add(
						new ResponseNotification("Error: unable to create user account.  Please contact Support",
																		 ResponseState.Error));

					ModelState.Clear();
					return PartialView("../Partial/_UserRegistration", model);
				}
			}

            supportAccountIdToValidate = (model.IsCsspRegistration) ? _userAccountLogic.GetCsspSupportAccountId(model.CustomerAccountId)
                : supportAccountId;

            if (supportAccountIdToValidate > 0)
            {
                if (!_userAccountLogic.CanAddEmailToSupportAccount(supportAccountIdToValidate, model.UserAccount.Email))
                {
                    var domainResponseMessage = _userAccountLogic.GetDomainDetails(model.UserAccount.Email.Substring
                        (model.UserAccount.Email.IndexOf('@') + 1));
                    model.Notifications.Add(new ResponseNotification(domainResponseMessage, ResponseState.Error));
                    ModelState.Clear();
                    return PartialView("../Partial/_UserRegistration", model);
                }
            }

            var isSuccess = false;			
			var region = (countryCode == CountryCode.US.ToDescription() ||
										countryCode == CountryCode.CA.ToDescription())
										 ? model.UserAccount.Address.Region
										 : model.OtherRegion;

			model.UserAccount.Address.Region = region;
			model.UserAccount.IsActive = true;
			model.UserAccount.CreatedByEmail = model.UserAccount.Email;
			model.UserAccount.CreatedOn = DateTime.Now;
			model.UserAccount.CreatedByUserAccountId = systemUserAccountId;

			try
			{
			    _logLogic.LogNewUser(new Lognewuser()
			    {
			        Email = model.UserAccount.Email,
			        EventType = "RegisterUserAccount",
			        EventDescription =
			            string.Format(
			                "New User Registration: Calling RegisterUserAccount method of UserAccountLogic. UserAccount: {0}, Support Account Id: {1}",
			                JsonConvert.SerializeObject(model.UserAccount), supportAccountId),
			        CreatedOn = DateTime.Now
			    });

			    long membershipId = 0;
			    long userAccountId;
			    UserProfile userProfile;
			    // Register User

			    //Get existing Super Users Count sseetham
                int noofSuperUsers = _membershipLogic.GetCountOfUsersByRole(supportAccountId, (int)RoleType.SuperUser);
			    
                //IsMembershipPending technically could be true here if there are multiple matching support accounts
                //though that feature is currently disabled.  I expect 100% of currently cases to show false at this point
                //noofSupers > 0 check atleast one super user exists to approve else auto approve
			    if (isMembershipPending == false  && supportAccountId > 0 && noofSuperUsers > 0) //If support account id = 0 then this is a new AWS account
			    {
			        isMembershipPending = _userAccountLogic.SupportAccountRequiresSuperUserApproval(supportAccountId);
			    }

                if (model.IsUsingCloudDeviceRegistration == false)
                {
                    model.UserAccount.IsFirstLogin = !isSelfRegistration;

                    _logLogic.LogNewUser(new Lognewuser()
                    {
                        Email = model.UserAccount.Email,
                        EventType = "DeviceCode",
                        EventDescription = model.DeviceCode,
                        CreatedOn = DateTime.Now
                    });

			        userProfile = _userAccountLogic.RegisterUserAccount(model.UserAccount, supportAccountId,
			            string.Empty, Common.GetClientIp(), out membershipId, out errorMessage, out userAccountId,
                        isMembershipPending, deviceCode: model.DeviceCode, isPartialShipmentUser: isPartialShipmentUser);

                    //Panorama registration: generate Panorama management license automatically
                    if (userAccountId >0 && model.IsPanoramaRegistration)
                    {
                        var device = new Csp.Api.Model.Device
                        {
                            Customer = new SupportAccount
                            {
                                Id = supportAccountId
                            },
                            LastUpdateByEmail = userProfile.Email,
                            SerialNumber = model.DeviceCode,
                            PanOsVersion = model.PanOsVersion,
                            UuId = model.UuId,
                            CpuId = model.CpuId,
                            HyperVisorVersion = model.HyperVisorVersion
                        };
			            bool isSiteLicenseGranted;
                        var registeredDevice = _deviceLogic.RegisterDevice(device, string.Empty, userAccountId, out isSiteLicenseGranted);
                        if (registeredDevice != null && registeredDevice.Id > 0)
			            {
			                _licenseLogic.GeneratePanoramaDeviceMgtLicense(model.DeviceCode, model.CpuId, model.UuId,
			                    model.HyperVisorVersion, model.PanOsVersion);
			            }
                    }
			    }
			    else
			    {
			        // Create Support account
			        // Create User Account
			        // Create membership
			        // Create SSO
			        // Create Sfdc contact
			        // Update membership with sfdc info
			        // Register aws device

			        if (model.IsAwsRegistration || model.IsGoogleCloudRegistration)
			        {
			            userProfile = _userAccountLogic.RegisterUserAccount(model.UserAccount, supportAccountId,
			                string.Empty, Common.GetClientIp(), out membershipId, out errorMessage, out userAccountId,
                            isMembershipPending, model.IsAwsRegistration, model.SupportAccountName, cloudAccountId, deviceId, isUsingCloudDeviceForRegistration: true, 
                            isGoogleCloudRegistration: model.IsGoogleCloudRegistration);
			        }
			        else if(model.IsAzureRegistration)
			        {
                        userProfile = _userAccountLogic.RegisterUserAccount(model.UserAccount, supportAccountId,
                            string.Empty, Common.GetClientIp(), out membershipId, out errorMessage, out userAccountId,
                            isMembershipPending, false, model.SupportAccountName, "", azureDeviceId, isUsingCloudDeviceForRegistration: true, isAzureRegistration: true);
			        }
			        else
			        {
                        userProfile = _userAccountLogic.RegisterUserAccount(model.UserAccount, supportAccountId,
                            string.Empty, Common.GetClientIp(), out membershipId, out errorMessage, out userAccountId, 
                            isUsingCloudDeviceForRegistration: true, isCsspRegistration: true,csspSerialNumber:model.CsspSerialNumber,customerAccountId:model.CustomerAccountId);
			        }
			    }


			    if (!string.IsNullOrEmpty(errorMessage))
			    {
			        model.Notifications.Add(
			            new ResponseNotification("Error: unable to create user account.  Please contact Support",
			                ResponseState.Error));

			        ModelState.Clear();
			        return PartialView("../Partial/_UserRegistration", model);
			    }		

			    // Send Registration Email to CspSupportEmail team
			    var emailModel = Mapper.Map<UserAccountRegistrationEmailModel>(model);
			    EmailHelper.SendUserRegistrationEmail(emailModel, supportAccountId);

			    // Send email to super user(s) for approval
			    if (isMembershipPending)
                {
                    
                    var updateMembershipStatusUrl = Url.Action("UpdateMembershipStatus", "Membership");
                    updateMembershipStatusUrl = string.Format("{0}{1}", SiteHelper.GetBaseUrl(), updateMembershipStatusUrl);
                  
                     #region Publish Event to Pubsub

                     AppNotificationData newUserAppNotificationData = new AppNotificationData()
                     {
                         AccountId = Convert.ToString(supportAccountId),
                         EntityId = Convert.ToString(userAccountId),
                         UserId = Convert.ToString(userAccountId)
                     };

                    AppNotification newUserNotification= new AppNotification()
                     {
                         AppData = newUserAppNotificationData,
                         Category = NotificationCategory.UserRegistration.ToDescription(),
                         DeliveryChannel = new List<string>(){NotificationDeliveryChannel.Web.ToDescription()},
                         Source = NotificationSource.Csp.ToDescription()
                     };

					EventPublisher eventPublisher = new EventPublisher();
                    await eventPublisher.PublishEvent(newUserNotification);
                     #endregion

                    EmailHelper.SendMembershipApprovalEmail(emailModel, supportAccountId, membershipId,
                        updateMembershipStatusUrl);

                }

			    else
			    {
                    EmailHelper.SendUserRegistrationEmailToSuperUsers(model.UserAccount.FirstName, model.UserAccount.LastName,
                   supportAccountId, userAccountId, membershipId);
			    }

                #region Publish event to Fedramp Topic

                if (_supportAccountLogic.IsFedrampAccount(supportAccountId))
                {
                    WebCommon.CommonApiResponse<FedrampEvent> customdata = new WebCommon.CommonApiResponse<FedrampEvent>();
                    customdata.ResponseObject = new FedrampEvent()
                    {
                        supportAccountId = supportAccountId,
                        emailAddress = model.UserAccount.Email

                    };
					EventPublisher eventPublisher= new EventPublisher();
                    await eventPublisher.PublishCustomEvent(customdata, fedramp_project, fedramp_topic);
                }

                #endregion

                isSuccess = true;
			}
			catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				model.Notifications.Add(
					new ResponseNotification(
						"An error occurred while processing your request. Please contact customer support.",
						ResponseState.Error));
			}

			if (isSuccess) {
				var email = model.UserAccount.Email;
                var name = string.Format("{0} {1}", model.UserAccount.FirstName,
                    model.UserAccount.LastName);

                var redirectUrl = Url.ActionSecure("RegistrationConfirmation", "UserAccount", new { email = email, name = name, isMembershipPending = isMembershipPending }); //Url.Action("RegistrationConfirmation", "UserAccount", new {uniqueId = uniqueId});
                model.RedirectUrl = redirectUrl;

			    if (model.IsUsingCloudDeviceRegistration)
			    {
			        if (model.IsAwsRegistration || model.IsGoogleCloudRegistration)
			        {
			            Elmah.ErrorSignal.FromCurrentContext()
			                .Raise(
			                    new Exception(string.Format("Successfully created {0} Support Account \"{1}\" for User \"{2}\"",
			                        model.IsAwsRegistration ? "AWS" : "Google Cloud", model.SupportAccountName, model.UserAccount.Email)));
			        }
                    if (model.IsAzureRegistration)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(string.Format("Successfully created AZURE Support Account \"{0}\" for User \"{1}\"",
                                    model.SupportAccountName, model.UserAccount.Email)));
                    }
                    if (model.IsCsspRegistration)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(string.Format("Successfully created End User CSSP Support Account \"{0}\" for User \"{1}\"",
                                    model.SupportAccountName, model.UserAccount.Email)));
                    }
			    }

			}

			ModelState.Clear();
			return PartialView("../Partial/_UserRegistration", model);
		}

		[HttpGet]
        [EncryptedActionParameter]
		public ActionResult RegistrationConfirmation(string uniqueId, string email, string name, bool isMembershipPending) {

		    var model = new RegistrationConfirmationViewModel
		    {
		        EmailAddress = email,
		        Name = name,		       
		        IsMembershipPending = isMembershipPending,
		        ActionHelper = new ActionHelper(ControllerFields.UserAccountController),
                IsAlreadySignedIn = UserHelper.GetSsoToken() != null, 
		    };
			

			return View(model);
		}

	    [HttpPost, AjaxOnly]
		public ActionResult ResendUserActivationEmail(string email) {
            var loginServiceLogic = new LoginServiceLogic();
           
            var isProfileExists = loginServiceLogic.IsProfileExists(email, Common.GetClientIp());
            if (!isProfileExists) {
                return RedirectToAction("UnAuthorizedAccess", "Error");
            }

            // Call Loginservice Activate API call
            var userProfile = _userAccountLogic.ActivateUser(email);

            var model = new DefaultDto();
			model.Notifications.Add(new ResponseNotification("An email confirmation has been sent.", ResponseState.Success));

			return PartialView("../Partial/_ResponseNotification", model.Notifications);
		}

		[HttpGet, Authorize]
		public ActionResult Details(long id) {
			//scroll to top to see important comment
			if (IsUserAuthorized(id, new[] { Permission.ViewUserAccount })) {


				var userAccountDto = new UserAccountDto {

					UserAccount = _userAccountLogic.GetUserAccount(id, User.UserAccount.Id),
					ActionHelper = new ActionHelper(ControllerFields.UserAccountController, id)
				};

				return PartialView("../Partial/_UserAccountDetails", userAccountDto);

			}

            return RedirectToAction("UnAuthorizedAccess", "Error");
        }

		[HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
		public ActionResult GetSupportAccountsForSap(long id) {
			var supportAccounts = _supportAccountLogic.GetSupportAccountsBySapId(id);

			var model = new SapSupportAccountDto() {
				Accounts = supportAccounts
			};

			return PartialView("../Partial/_SupportAccountsForSap", model);
		}

		[HttpGet, AjaxOnly, Authorize]
		public ActionResult TemporaryAnnouncement() {
			var model = new UserAccount {
				Id = User.UserAccount.Id,
				IsTempMessageViewed = User.UserAccount.IsTempMessageViewed
			};

			return PartialView("../Partial/_TemporaryAnnouncement", model);
		}

		[HttpPost, AjaxOnly, Authorize, ValidateAntiForgeryToken]
		public ActionResult TemporaryAnnouncement(UserAccount userAccount) {
			_userAccountLogic.UpdateTemporaryAnnouncement(userAccount, User.UserAccount.Id);
			return PartialView("../Partial/_TemporaryAnnouncement", userAccount);
		}



		[HttpGet, AjaxOnly, Authorize]
		public ActionResult SupportAccounts(long id) {
			//scroll to top to see important comment            
			if (IsUserAuthorized(id, new[] { Permission.ViewUserAccountSupportAccountMemberships })) {

				var dataParameters = new DataParameters { PaginationCount = 10 };
				var supportAccountMembershipListItem = _supportAccountLogic.GetSupportAccountMembershipsbyUserAccountId(id, User.UserAccount.Id, dataParameters);

				var supportAccountMembershipListDTO = new SupportAccountMembershipListDto {
					DataParameters = dataParameters,
					SupportAccountMemberships = supportAccountMembershipListItem,
					ActionHelper = new ActionHelper(ControllerFields.UserAccountController, id)
				};
				return PartialView("../Partial/_SupportAccountMembership", supportAccountMembershipListDTO);
			}

			return null;
		}


		[HttpPost, AjaxOnly, Authorize, ValidateAntiForgeryToken]
		public ActionResult SupportAccounts([DataSourceRequest] DataSourceRequest request, long id) {
			//scroll to top to see important comment            
			if (IsUserAuthorized(id, new[] { Permission.ViewUserAccountSupportAccountMemberships })) {

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
				var supportAccountMembershipListItem = _supportAccountLogic.GetSupportAccountMembershipsbyUserAccountId(id, User.UserAccount.Id, dataParameters);

				var result = new DataSourceResult {
					Data = supportAccountMembershipListItem,
					Total = Convert.ToInt32(dataParameters.TotalRecordCount)
				};
				return Json(result);
			}

			return null;
		}


		[HttpGet, Authorize]
		public ActionResult Devices(long id) {
			//scroll to top to see important comment            
			if (IsUserAuthorized(id, new[] { Permission.ViewUserAccountDeviceMemberships })) {

				ViewBag.ActionHelper = new ActionHelper(ControllerFields.UserAccountController, id);
				return PartialView("../Partial/_Devices");
			}

			return null;
		}

		[HttpGet, AjaxOnly, Authorize]
		public ActionResult View(UserAccount userAccount) {
			//scroll to top to see important comment            
			if (IsUserAuthorized(userAccount.Id, new[] { Permission.ViewUserAccount })) {
				var userAccountDto = new UserAccountDto {
					UserAccount = userAccount,
					ActionHelper = new ActionHelper(ControllerFields.UserAccountController, userAccount.Id)
				};

				return PartialView("../Partial/_ViewUserAccount", userAccountDto);
			}

			return null;
		}


		[HttpGet, AjaxOnly, Authorize]
		public ActionResult Edit(UserAccount userAccount) {
			//scroll to top to see important comment            
			if (IsUserAuthorized(userAccount.Id, new[] { Permission.EditUserAccount })) {
				var userAccountDto = new UserAccountDto {
					UserAccount = userAccount,
					ActionHelper =
							new ActionHelper(ControllerFields.UserAccountController, userAccount.Id, ActionHelper.EntityAction.Edit)
				};

				return PartialView("../partial/_UserAccount", userAccountDto);
			}

			return null;
		}


		[HttpPost, AjaxOnly, Authorize, ValidateAntiForgeryToken]
		public ActionResult Edit(UserAccountDto userAccountDto) {
			var userAccount = userAccountDto.UserAccount;
			var model = new UserAccountDto {
				UserAccount = userAccount,
				ActionHelper =
						new ActionHelper(ControllerFields.UserAccountController, userAccount.Id, ActionHelper.EntityAction.Edit)
			};

			ModelState.Remove("UserAccount.Address.Country");			

			//scroll to top to see important comment            
			if (IsUserAuthorized(userAccountDto.UserAccount.Id, new[] { Permission.EditUserAccount })) {
				try {

                    if (userAccountDto.SetDefaultAddress)
                    {
                        var supportAccount = SupportAccountLogic.GetSupportAccount(CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
                        model.UserAccount.Address = supportAccount.Address;
                        userAccountDto.SetDefaultAddress = false;
                        ModelState.Clear();
                        return PartialView("../partial/_UserAccount", model);
                    }

					if (ModelState.IsValid) {
						userAccount.LastUpdateByUserAccountId = User.UserAccount.Id;
						model.UserAccount = _userAccountLogic.UpdateUserAccount(userAccount, User.UserAccount.Id, Common.GetClientIp(), CurrentState.GetSupportAccountName());
						ModelState.Clear();
						model.Notifications.Add(new ResponseNotification("User information successfully saved.", ResponseState.Success));
					}

				} catch (CspNotificationException exception) {
					model.Notifications.Add(new ResponseNotification(exception.Message, ResponseState.Error));

				}

				return PartialView("../partial/_UserAccount", model);
			}

			return null;
		}


		[HttpGet, Authorize]
		public ActionResult Create(long id = 0) {
			var userAccountId = (User != null && User.UserAccount != null) ? User.UserAccount.Id : 0;

			// Comments By LK On 07/19/2019
			// Check if user has access to create new user
			if (!User.HasPermission(Permission.CreateUserAccount)) {
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			//scroll to top to see important comment            
			if (IsUserAuthorized(User.UserAccount.Id, new[] { Permission.CreateUserAccount })) {

                //Show message in case user is created by Group Super User
			    var accountGroups = GroupLogic.GetSupportAccountGroups(id, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Select(g => g.GroupId).ToList();
                var roleCategory = User.RoleSecurities.Where(s => s.SupportAccountId == id || accountGroups.Contains(s.GroupId ?? 0)).Select(r => r.Category).Distinct().ToList();
                var isTac = User.RoleSecurities.Any(r => r.SecurityClassification.Equals(RoleSecurity.SecurityClassifications.Tac));
                var createMembership = roleCategory.Contains(RoleSecurity.Categories.Account) || isTac;

				var userAccountDto = new UserAccountDto {
					UserAccount = new UserAccount() {
						IsOptInEmail = true,
						IsOptInSecurityEmail = true,
						IsOptInPanOsUpdateEmail = true
					},
                    CreateMembership = createMembership,
					ActionHelper =
							new ActionHelper(ControllerFields.UserAccountController, User.UserAccount.Id, ActionHelper.EntityAction.Create)
				};

				return View("Create", userAccountDto);
			}

			return RedirectToAction("UnAuthorizedAccess", "Error");
		}

		[HttpPost, AjaxOnly, Authorize, ValidateAntiForgeryToken]
		public ActionResult Create(UserAccountDto userAccountDto) {
			// Comments By LK On 07/19/2019
			// Check if user has access to create new user
			if (!User.HasPermission(Permission.CreateUserAccount)) {
				return RedirectToAction("UnAuthorizedAccess", "Error");
			}

			string errorMessage;
			var userAccount = userAccountDto.UserAccount;
			var model = new UserAccountDto {
				UserAccount = userAccount,
				ActionHelper =
						new ActionHelper(ControllerFields.UserAccountController, User.UserAccount.Id, ActionHelper.EntityAction.Create)
			};

			ModelState.Remove("UserAccount.Address.Country");

			//scroll to top to see important comment            
			if (!IsUserAuthorized(User.UserAccount.Id, new[] { Permission.CreateUserAccount })) {
				return null;
			}

            try
            {
                if (userAccountDto.SetDefaultAddress)
                {
                    var supportAccount = SupportAccountLogic.GetSupportAccount(CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
                    model.UserAccount.Address = supportAccount.Address;
                    userAccountDto.SetDefaultAddress = false;
                    ModelState.Clear();
                    return PartialView("../partial/_UserAccount", model);
                }

				if (!ModelState.IsValid) {
					return PartialView("../partial/_UserAccount", model);
				}

				var isProfileAlreadyExists = _userAccountLogic.IsProfileExists(userAccount.Email, Common.GetClientIp());

				var supportAccountId = CurrentState.GetCurrentAccountId();
				var countryId = userAccount.Address.CountryId ?? 0;
				var countryCode = _lookupLogic.GetCountryCodeById(countryId);

				userAccount.Address.Country = countryCode;
				userAccount.IsActive = true;
				userAccount.CreatedByEmail = User.UserAccount.Email;
				userAccount.CreatedOn = DateTime.Now;
				userAccount.CreatedByUserAccountId = User.UserAccount.Id;

				try {
					long membershipId = 0;
				    long userAccountId = 0;

					var userAccountExists = _userAccountLogic.IsEmailExists(model.UserAccount.Email);
					if (userAccountExists) {
						model.Notifications.Add(new ResponseNotification("This email is already registered", ResponseState.Error));
						return PartialView("../partial/_UserAccount", model);
					}

					_supportAccountDomainsService.CreateFirstSupportAccountDomain(supportAccountId, userAccount.Email);

                    //Don't create new membership in case user is created by Group Super User
                    var accountGroups = GroupLogic.GetSupportAccountGroups(supportAccountId, User.UserAccount.Id, User.RoleSecurities, new DataParameters()).Select(g => g.GroupId).ToList();
                    var roleCategory = User.RoleSecurities.Where(s => s.SupportAccountId == supportAccountId || accountGroups.Contains(s.GroupId ?? 0)).Select(r => r.Category).Distinct().ToList();
                    var isTac = User.RoleSecurities.Any(r => r.SecurityClassification.Equals(RoleSecurity.SecurityClassifications.Tac));
                    var createMembership = roleCategory.Contains(RoleSecurity.Categories.Account) || isTac;


					// Register User
					var userProfile = _userAccountLogic.RegisterUserAccount(model.UserAccount, supportAccountId, userAccountDto.Password, Common.GetClientIp(),
                                                                            out membershipId, out errorMessage, out userAccountId, createMembership : createMembership);

					if (userProfile != null) {

                        EmailHelper.SendUserRegistrationEmailToSuperUsers(model.UserAccount.FirstName, model.UserAccount.LastName, supportAccountId, userAccountId, 
                                                                            membershipId, !createMembership);
					
					}
					ModelState.Clear();
					model.Notifications.Add(new ResponseNotification("User information successfully saved.",
																													 ResponseState.Success));
				} catch (Exception ex) {
					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
					var displayError = ParseErrorMessage(ex.Message);
					model.Notifications.Add(new ResponseNotification(displayError, ResponseState.Error));
				}
			} catch (CspNotificationException exception) {
				model.Notifications.Add(new ResponseNotification(exception.Message, ResponseState.Error));
			}

			return PartialView("../partial/_UserAccount", model);
		}

		private string ParseErrorMessage(string message) {
			if (message.StartsWith("Duplicate entry ") &&
									message.EndsWith(" for key 'email_UNIQUE'")) {

				return "This email address is already registered.";

			} else {
				return "User creation failed.";
			}
		}

		[HttpGet, CspAuthorize(Permission.EditWildfireUsers)]
		public ActionResult CreateWildfireUser() {
			var model = new WildfireUserAccountVerificationDto {
				ActionHelper = new ActionHelper(ControllerFields.UserAccountController)
			};

			return PartialView("../Partial/_VerifyWildfireUser", model);
		}

		[HttpPost, AjaxOnly, CspAuthorize(Permission.EditWildfireUsers)]
		public ActionResult CreateWildfireUser(string email) {
			var message = string.Empty;

			try {
                if (!_userAccountLogic.IsValidNonPublicEmail(email))
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = false,
                            Message = "Please enter a valid email. Public domains are not allowed. Please use your company email address."
                        }
                    };
                }

				if (ModelState.IsValid) {
					var supportAccountId = CurrentState.GetCurrentAccountId();

					var isUserExistsForAccount = _userAccountLogic.IsWildfireUserAccountExists(email, supportAccountId);
					var wildfireUserAccountId = _userAccountLogic.GetWildfireUserAccountId(email);

					if (isUserExistsForAccount) {
						message = "Wildfire User already exists.";
					} else {
						return new JsonResult {
							Data = new {
								Success = true,
								Message = wildfireUserAccountId
							}
						};
					}
				}

			} catch (CspNotificationException) {
				message = "Failed to create a new Wildfire User. Please contact customer support.";
			}

			return new JsonResult {
				Data = new {
					Success = false,
					Message = message
				}
			};
		}

		[HttpGet, CspAuthorize(Permission.EditWildfireUsers)]
		public ActionResult AddUpdateWildfireUser(long userAccountId, string email) {
			var userAccount = new WildfireUserAccount();
			string otherRegion = string.Empty;

			if (userAccountId > 0) {
				var wildfireUserAccount = _userAccountLogic.GetWildfireUserAccountById(userAccountId);
				userAccount = new WildfireUserAccount() {
					Id = wildfireUserAccount.WildfireUserAccountId,
					Email = wildfireUserAccount.Email,
					FirstName = wildfireUserAccount.FirstName,
					LastName = wildfireUserAccount.LastName
				};
			}

			if (!string.IsNullOrEmpty(email)) {
				var userProfile = _loginServiceLogic.GetUserProfileV2(email);

				if (!string.IsNullOrEmpty(userProfile?.Email)) {
					var countryId = _lookupLogic.GetCountryIdByCode(userProfile.Country);
					long regionId = 0;

					if (userProfile.Country == CountryCode.US.ToDescription() || userProfile.Country == CountryCode.CA.ToDescription()) {
						regionId = _lookupLogic.GetRegionIdByCode(userProfile.State, countryId);
					} else {
						otherRegion = userProfile.State;
					}

					if (userAccountId == 0) {
						userAccount.Email = userProfile.Email;
						userAccount.FirstName = userProfile.FirstName;
						userAccount.LastName = userProfile.LastName;
					}

					userAccount.Address = new Address() {
						Address1 = userProfile.Address1,
						Address2 = userProfile.Address2,
						City = userProfile.City,
						CountryId = countryId,
						PostalCode = userProfile.Zipcode,
						Region = regionId.ToString()
					};

					userAccount.PhoneNumber = userProfile.Phone;
				} else {
					if (userAccountId == 0) {
						userAccount.Email = email;
					}
				}
			}

			var model = new WildfireUserAccountDto() {
				UserAccount = userAccount,
				Countries = _lookupLogic.GetCountries().ToList(),
				OtherRegion = otherRegion,
				ActionHelper =
						new ActionHelper(ControllerFields.UserAccountController, User.UserAccount.Id, ActionHelper.EntityAction.Create)

			};

			return PartialView("../Partial/_AddUpdateWildfireUser", model);
		}

		[HttpPost, AjaxOnly, ValidateJsonAntiForgeryToken, CspAuthorize(Permission.EditWildfireUsers)]
		public ActionResult AddUpdateWildfireUser(WildfireUserAccountDto model, List<long> deviceIds, List<string> serialNumbers, List<long> changedDeviceIds) {
            if (model.UserAccount != null && !_userAccountLogic.IsValidNonPublicEmail(model.UserAccount.Email))
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "Please enter a valid email. Public domains are not allowed. Please use your company email address."
                    }
                };
            }

			if (deviceIds.Count == 0) {
				return new JsonResult {
					Data = new {
						Success = false,
						Message = "Please select at least one device."
					}
				};
			}

			bool isNewUserAccount = (model.UserAccount.Id == 0);
			var message = string.Empty;
			var countryId = model.UserAccount.Address.CountryId ?? 0;
			var countryCode = _lookupLogic.GetCountryCodeById(countryId);

			if (countryCode == CountryCode.US.ToDescription() ||
							countryCode == CountryCode.CA.ToDescription()) {
				// Ignore OtherState validation
				ModelState.Remove("model.OtherRegion");
			} else {
				// Ignore State/Province validation
				ModelState.Remove("model.UserAccount.Address.Region");
			}

			try {
				if (ModelState.IsValid) {
					var supportAccountId = CurrentState.GetCurrentAccountId();
					var region = (countryCode == CountryCode.US.ToDescription() ||
												countryCode == CountryCode.CA.ToDescription())
													 ? model.UserAccount.Address.Region
													 : model.OtherRegion;

					model.UserAccount.Address.Region = region;

					// Register Wildfire Standalone User
					long wildfireUserAccountId;
					var userProfile = _userAccountLogic.AddUpdateWildfireUserAccount(model.UserAccount, supportAccountId, deviceIds,
																																					 Common.GetClientIp(), User.UserAccount.Id,
																																					 out wildfireUserAccountId, changedDeviceIds);

					if (!isNewUserAccount)
                    { 
						// Send Device Registration Email
						EmailHelper.SendWildfireDeviceRegistrationEmail(model.UserAccount.Email,
																														string.Format("{0} {1}", model.UserAccount.FirstName,
																																					model.UserAccount.LastName), serialNumbers);
                    }

					return new JsonResult {
						Data = new {
							Id = wildfireUserAccountId,
							Success = true,
							Message =
									(isNewUserAccount)
											? "Wildfire Standalone User account created successfully."
											: "Wildfire Standalone User account updated successfully."
						}
					};
				}
                else {
                    message = string.Join(" ",
                        ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList());
                }

			} catch (CspNotificationException) {
				message = "An error occurred while processing your request. Please contact customer support.";
			}

			return new JsonResult {
				Data = new {
					Success = false,
					Message = message
				}
			};
		}

		[HttpGet, Authorize]
		public ActionResult SendSuperUserEmail() {
			if (User.UserAccount.Id == 33401 || User.UserAccount.Id == 40893 || User.UserAccount.Id == 10422) {
				return View();
			} else {
				return RedirectToAction("NotAuthorized", "Home");
			}

		}

		[HttpPost, Authorize]
		public ActionResult SendSuperUserEmailPost() {
			var goLiveDate = new DateTime(2013, 12, 13);

			if (DateTime.Now < goLiveDate) {
				var updateMembershipStatusUrl = Url.Action("UpdateMembershipStatus", "Membership");
				updateMembershipStatusUrl = string.Format("{0}{1}", SiteHelper.GetBaseUrl(), updateMembershipStatusUrl);
				EmailHelper.SendMembershipApprovalEmailForAllPending(updateMembershipStatusUrl);

				return new JsonResult {
					Data = new {
						Success = true,
						Message = "Success!"
					}
				};
			} else {
				return new JsonResult {
					Data = new {
						Success = true,
						Message = "You do not have access!"
					}
				};
			}

		}


		[HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditWildfireUsers)]
		public ActionResult DeleteWildfireUser(long id) {
			string message;

			try {
				var supportAccountId = CurrentState.GetCurrentAccountId();

				_userAccountLogic.DeleteWildfireUserAccount(id, supportAccountId, Common.GetClientIp(), User.UserAccount.Email);

				message = "Wildfire User has been deleted successfully.";
				return new JsonResult {
					Data = new {
						Success = true,
						Message = message
					}
				};
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				message = "Failed to delete Wildfire User.";
			}

			return new JsonResult {
				Data = new {
					Success = false,
					Message = message
				}
			};
		}

		[HttpGet, AjaxOnly]
		public ActionResult RequestSuperUserAccess(string deviceCode, string customerCode, string email, string firstName, string lastName) {
			var isAllowed = _userAccountLogic.ShowEmailToSuperUserOption(deviceCode, customerCode);

			if (!isAllowed) {
				throw new Exception("Access denied for email super users");
			}

			_userAccountLogic.EmailSuperUsers(email, customerCode, firstName, lastName);

			return Json("", JsonRequestBehavior.AllowGet);
		}

        public static CaptchaResponse ValidateCaptcha(string response)
        {
            string privateKey = WebConfigurationManager.AppSettings["recaptchaPrivateKey"];
            var client = new WebClient();
            var jsonResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", privateKey, response));
            return JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult.ToString(), new JsonSerializerSettings()
            {
                MaxDepth = 32,
                TypeNameHandling = TypeNameHandling.None
            });
        }

        [HttpPost]
        public async Task<ActionResult> SendSuperUserReminderEmail(bool isExpired = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(User?.UserAccount?.Email))
                {
                    isExpired = User?.UserAccount?.IsMembershipExpired == null  ? isExpired : User.UserAccount.IsMembershipExpired;

                    var userProfile = _userAccountLogic.GetUserAccountByEmail(User.UserAccount.Email);
                    var defaultSupportAccountId = _securityLogic.GetDefaultSupportAccount(userProfile.Id);
                    var additionalData = new AdditionalDetails()
                    {
                        MembershipStatus = isExpired ? MembershipStatus.None.ToString() : MembershipStatus.Pending.ToString(),
                        IsExpired = isExpired
                    };

                    AppNotificationData newUserAppNotificationData = new AppNotificationData()
                    {
                        AccountId = Convert.ToString(defaultSupportAccountId),
                        EntityId = Convert.ToString(userProfile.Id),
                        UserId = Convert.ToString(userProfile.Id),
                        AdditionalData = additionalData
                    };

                    AppNotification newUserNotification = new AppNotification()
                    {
                        AppData = newUserAppNotificationData,
                        Category = NotificationCategory.SuperUserReminder.ToDescription(),
                        DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription(), NotificationDeliveryChannel.Email.ToDescription() },
                        Source = NotificationSource.Csp.ToDescription()
                    };

                    Events events = new Events()
                    {
                        Event = newUserNotification
                    }; 

					EventPublisher eventPublisher = new EventPublisher();
                    await eventPublisher.PublishEvent(events);

                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = true,
                            Message = "Email Sent Successfully."
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = false,
                    Message = "Sending email failed."
                }
            };
        }
    }
}
