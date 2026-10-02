using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.Extensions;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Microsoft.Security.Application;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;


namespace Csp.Web.Controllers
{
    public class DeviceDetailsController : ApiController
    {

        private readonly IApplicationSettings _applicationSettings;
        private readonly IDeviceDataAccess _deviceDataAccess;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;
        private readonly ILicenseLogic _licenseLogic;
        private readonly ISecurityLogic _securityLogic;
        private readonly IUserProfileDataAccess _userProfileDataAccess;
        private readonly IAssetApiLogic _assetApiLogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;

        private readonly int _aws = Convert.ToInt32(DeviceType.AWS);
        private readonly int _azure = Convert.ToInt32(DeviceType.AZURE);
        private readonly int _gcp = Convert.ToInt32(DeviceType.GCP);
        private readonly int _cssp = Convert.ToInt32(DeviceType.CSSP);

        public DeviceDetailsController(IApplicationSettings applicationSettings, IDeviceDataAccess deviceDataAccess,
            IDeviceLogic deviceLogic,IEulaLogic eulaLogic , IAscDeviceRegistrationService ascDeviceRegistrationService, ILicenseLogic licenseLogic, ISecurityLogic securityLogic,
            IUserProfileDataAccess userProfileDataAccess, IAssetApiLogic assetApiLogic, IUserAccountLogic userAccountLogic, ISiteLicenseLogic siteLicenseLogic)
        {
            _applicationSettings = applicationSettings;
            _deviceDataAccess = deviceDataAccess;
            _deviceLogic = deviceLogic;
            _eulaLogic = eulaLogic;
            _ascDeviceRegistrationService = ascDeviceRegistrationService;
            _licenseLogic = licenseLogic;
            _securityLogic = securityLogic;
            _userProfileDataAccess = userProfileDataAccess;
            _assetApiLogic = assetApiLogic;
            _userAccountLogic = userAccountLogic;
            _siteLicenseLogic = siteLicenseLogic;
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetEligibleDevicesForTransfer(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<DeviceTransferItem>();

            try
            {
                returnData = _deviceDataAccess.GetEligibleDevicesForTransfer(supportAccountId, userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDestinationAccounts(long userAccountId,long supportAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<NameValueOnly>();

            try
            {
                returnData = _deviceDataAccess.GetDestinationAccounts(userAccountId,supportAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(returnData);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public IHttpActionResult InitiateBulkTransfer([FromBody]BulkDeviceTransferModel requestData)
        {
            var returnData = new Resultdetails();

            try
            {
                returnData.isSuccess = _deviceDataAccess.CreateBulkTransferRequest(requestData);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(returnData);
        }

        // Comments By LK On 12/02/2019
        // GetDeviceAddress method is moved to PANServices to address authorization vulnerabilities
        /*
#region RMA Shipping Improvements

[HttpGet]
public IHttpActionResult GetDeviceAddress(string serialNumber)
{
    var privateKey = ConfigurationManager.AppSettings["EncryptionProviderPrivateKey"];
    var encryptionProvider = new EncryptionProvider();
    var headers = Request.Headers;

    if (headers.Contains("x-api-key"))
    {
        var key = headers.GetValues("x-api-key").FirstOrDefault() ?? string.Empty;
        var isKeyValid = encryptionProvider.IsKeyValid(serialNumber, key, privateKey);
        if (!isKeyValid)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(String.Format("Invalid key {0}", key)));
            return Unauthorized();
        }

        var device = _deviceLogic.GetDeviceAddressBySerialNumber(serialNumber);

        return Json(device);
    }

    return Unauthorized();
}

#endregion
        */

        #region Device Registration

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetDeviceTypes()
        {
            var returnData = new List<NameValueOnly>();

            try
            {
                returnData = _deviceDataAccess.GetDeviceTypes();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetPanOs()
        {
            var returnData = new List<NameValueOnly>();

            try
            {
                returnData = _deviceDataAccess.GetPanOs();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetHyperVisors(string filter = null)
        {
            var returnData = new List<NameValueOnly>();

            try
            {
                returnData = _deviceDataAccess.GetHyperVisors();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(returnData);
        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetSupportAgreements()
        {
            try
            {
                var eula = new EulaModel()
                {
                    Languages = _eulaLogic.GetEulaLanguages(),
                    Eulas = _eulaLogic.GetEula(),
                    DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                    SelectedLanguage = EulaLanguage.English
                };
                eula.IsEulaInMultipleLanguages = eula.Eulas.Count() > 1;
                eula.SelectedEulaId = eula.DefaultEula.Id;
                eula.Context = EulaContext.RegisterDevice;

                return this.Json(eula);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new EulaModel());
            }

        }

        [HttpGet]
        [JwtTokenAuthentication]
        public IHttpActionResult GetAllDeviceCategories(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<DeviceCategory>();

            try
            {
                returnData = _deviceLogic.GetAllDeviceCategories(supportAccountId,userAccountId);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(returnData);
        }

          [HttpPost]
          [RequireCspApiCert]
          public IHttpActionResult DeviceRegistration([FromBody]DeviceRegistrationRequestDetail request)
          {
              StringBuilder commaSeperatedMessages = new StringBuilder();
              IEnumerable<string> messages = new string[] { };
              List<ResponseNotification> deviceRegistrationResponse = new List<ResponseNotification>();

              try
              {
                  // XSS Protection: Sanitize user inputs
                  if (request != null)
                  {
                      request.SerialNumber = Sanitizer.GetSafeHtmlFragment(request.SerialNumber ?? string.Empty);
                      request.UserEmail = Sanitizer.GetSafeHtmlFragment(request.UserEmail ?? string.Empty);
                      request.Address1 = Sanitizer.GetSafeHtmlFragment(request.Address1 ?? string.Empty);
                      request.Address2 = Sanitizer.GetSafeHtmlFragment(request.Address2 ?? string.Empty);
                      request.City = Sanitizer.GetSafeHtmlFragment(request.City ?? string.Empty);
                      request.Region = Sanitizer.GetSafeHtmlFragment(request.Region ?? string.Empty);
                      request.CountryName = Sanitizer.GetSafeHtmlFragment(request.CountryName ?? string.Empty);
                      request.PostalCode = Sanitizer.GetSafeHtmlFragment(request.PostalCode ?? string.Empty);
                  }

                  bool isRegistered = false;
                  var result = new List<AvailableVm>();
                  string validationMessage = IsValidSerialNunmber(request.SerialNumber, request.CustomerSupportAccountId, out isRegistered);
                  if (!string.IsNullOrEmpty(validationMessage))
                  {
                      return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.BadRequest, this, new string[] { validationMessage });
                  }

                  //Eval Account Validation - check if registering a production device
                  bool isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(request.CustomerSupportAccountId);
                  if (isEvalSupportAccount)
                  {
                      bool isEvalDevice = _userAccountLogic.IsEvalDeviceCode(request.SerialNumber);

                      if (!isEvalDevice)
                      {
                          return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.BadRequest, this, new string[] { Constants.EvalAccountMessage });
                      }
                  }

                  DeviceRegistrationRequest deviceRegistrationRequest = new DeviceRegistrationRequest()
                  {
                      SerialNumber = request.SerialNumber,
                      CustomerSupportAccountId = request.CustomerSupportAccountId,
                      UserEmail = request.UserEmail,
                      UserAccountId = request.UserAccountId,
                      IsPanorama = IsPanoramaSerialNumber(request.SerialNumber),
                      ShowEsa = false,
                      EulaId = 3,
                      Address1 = request.Address1,
                      Address2 = request.Address2,
                      City = request.City,
                      Region = request.Region,
                      CountryId = request.CountryId,
                      CountryName = request.CountryName,
                      PostalCode = request.PostalCode
                  };

                  var resultDevice = CreateDeviceModelByDeviceType(deviceRegistrationRequest);

                  resultDevice.IsCloudDevice = false;

                  deviceRegistrationResponse = ValidateRequest(resultDevice, deviceRegistrationRequest);


                  foreach (ResponseNotification response in deviceRegistrationResponse)
                  {
                      if (commaSeperatedMessages.Length > 0) commaSeperatedMessages.Append(",");
                      commaSeperatedMessages.Append(response.Message);
                  }
                  if (commaSeperatedMessages.Length > 0)
                  {
                      messages = commaSeperatedMessages.ToString().Split(',');
                      return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.BadRequest, this, messages);
                  }


                  //Format model
                  DeviceRegistrationDto deviceRequest = new DeviceRegistrationDto()
                  {
                      Device = resultDevice,
                      AscCustBklnSupportAuthCode = deviceRegistrationRequest.AscBacklineAuthCode,
                      Eula = new EulaModel()
                      {
                          SelectedEulaId = deviceRegistrationRequest.EulaId
                      }
                  };

                  deviceRegistrationResponse = RegisterDevice(deviceRequest);
                  foreach (ResponseNotification response in deviceRegistrationResponse)
                  {
                      if (commaSeperatedMessages.Length > 0) commaSeperatedMessages.Append(",");
                      commaSeperatedMessages.Append(response.Message);
                  }
                  messages = commaSeperatedMessages.ToString().Split(',');
                  if (messages.Any(m => m.Contains(Constants.DeviceRegistrationSuccessMessage)))
                  {
                      return this.Json(new
                      {
                          data = new
                          {
                              success = true,
                              message = Constants.DeviceRegistrationSuccessMessage
                          }
                      });
                  }
                  else
                  {
                      return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.BadRequest, this, messages);
                  }
              }
              catch (Exception ex)
              {
                  Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                  return WebExtensions.HttpCustomErrorResponse(HttpStatusCode.BadRequest, this, new string[] { "Failed to register device. Please contact Support" });
              }
          }
          [HttpPost]
          [JwtTokenAuthentication]
          public IHttpActionResult RegisterDevice([FromBody]DeviceRegistrationRequest deviceRegistrationRequest)
          {
              if (!JwtTokenManager.IsAuthorized(deviceRegistrationRequest.UserEmail))
              {
                  return Unauthorized();
              }

              List<ResponseNotification> deviceRegistrationResponse = new List<ResponseNotification>();
              try
              {
                  var sample = JsonConvert.SerializeObject(new DeviceRegistrationRequest());
                  if (deviceRegistrationRequest != null)
                  {
                    // XSS Protection: Sanitize user inputs
                    const string escapePlus = @"&#43;";
                    
                      deviceRegistrationRequest.SerialNumber = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.SerialNumber ?? string.Empty);
                      deviceRegistrationRequest.UserEmail = !string.IsNullOrEmpty(deviceRegistrationRequest.UserEmail) ? Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.UserEmail).Replace(escapePlus, "+") : deviceRegistrationRequest.UserEmail;                           
                      deviceRegistrationRequest.Address1 = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Address1 ?? string.Empty);
                      deviceRegistrationRequest.Address2 = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Address2 ?? string.Empty);
                      deviceRegistrationRequest.City = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.City ?? string.Empty);
                      deviceRegistrationRequest.Region = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Region ?? string.Empty);
                      deviceRegistrationRequest.RegionText = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.RegionText ?? string.Empty);
                      deviceRegistrationRequest.PostalCode = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.PostalCode ?? string.Empty);
                      deviceRegistrationRequest.Comments = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Comments ?? string.Empty);
                      deviceRegistrationRequest.CommentsEncoded = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.CommentsEncoded ?? string.Empty);
                      deviceRegistrationRequest.DeviceName = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.DeviceName ?? string.Empty);
                      deviceRegistrationRequest.DeviceNameEncoded = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.DeviceNameEncoded ?? string.Empty);
                      deviceRegistrationRequest.CustomerSupportAccountName = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.CustomerSupportAccountName ?? string.Empty);
                      deviceRegistrationRequest.CountryName = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.CountryName ?? string.Empty);
                      deviceRegistrationRequest.PanOsVersion = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.PanOsVersion ?? string.Empty);
                      deviceRegistrationRequest.Cpuid = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Cpuid ?? string.Empty);
                      deviceRegistrationRequest.Uuid = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Uuid ?? string.Empty);
                      deviceRegistrationRequest.Hypervisor = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.Hypervisor ?? string.Empty);
                      deviceRegistrationRequest.TenantId = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.TenantId ?? string.Empty);
                      deviceRegistrationRequest.AscBacklineAuthCode = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.AscBacklineAuthCode ?? string.Empty);
                    deviceRegistrationRequest.AwsAccountId = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.AwsAccountId ?? string.Empty);
                    deviceRegistrationRequest.AgreementID = Sanitizer.GetSafeHtmlFragment(deviceRegistrationRequest.AgreementID ?? string.Empty);

                      var resultDevice = CreateDeviceModelByDeviceType(deviceRegistrationRequest);

                      if (deviceRegistrationRequest.DeviceType == (int)DeviceType.CLOUDAZURE
                          || deviceRegistrationRequest.DeviceType == (int)DeviceType.CLOUDAWS
                          || deviceRegistrationRequest.DeviceType == (int)DeviceType.CLOUDAIAWS)
                      {
                          BaseRequest baseRequest = new BaseRequest()
                          {
                              DeviceType = deviceRegistrationRequest.DeviceType,
                              EulaId = 3,
                              SerialNumber = deviceRegistrationRequest.SerialNumber,
                              SupportAccountId = deviceRegistrationRequest.CustomerSupportAccountId,
                              TenantId = deviceRegistrationRequest.TenantId,
                              UserAccountId = deviceRegistrationRequest.UserAccountId,
                              UserEmail = deviceRegistrationRequest.UserEmail,
                              MarketPlaceAccountType = deviceRegistrationRequest.DeviceType == (int)DeviceType.CLOUDAZURE ? "Azure" : "AWS"
                          };
                          var result = _assetApiLogic.RegisterTenantWithValidation(baseRequest);
                          var state = result.IsSuccess ? ResponseState.Success : ResponseState.Error;
                          deviceRegistrationResponse.Add(new ResponseNotification(result.Message, state));
                          return this.Json(deviceRegistrationResponse);
                      }
                    else if (deviceRegistrationRequest.DeviceType == (int)DeviceType.ROUTE53)
                    {
                        BaseRequest baseRequest = new BaseRequest()
                        {
                            DeviceType = deviceRegistrationRequest.DeviceType,
                            EulaId = 3,
                            SerialNumber = deviceRegistrationRequest.SerialNumber,
                            SupportAccountId = deviceRegistrationRequest.CustomerSupportAccountId,
                            TenantId = deviceRegistrationRequest.TenantId,
                            UserAccountId = deviceRegistrationRequest.UserAccountId,
                            UserEmail = deviceRegistrationRequest.UserEmail,
                            MarketPlaceAccountType = "AWS",
                            AwsAccountId = deviceRegistrationRequest.AwsAccountId,
                            AgreementID = deviceRegistrationRequest.AgreementID
                        };
                        var result = _assetApiLogic.RegisterTenantWithValidation(baseRequest);
                        var state = result.IsSuccess ? ResponseState.Success : ResponseState.Error;
                        deviceRegistrationResponse.Add(new ResponseNotification(result.Message, state));
                        return this.Json(deviceRegistrationResponse);
                    }

                    //Validate the request before registering
                    // skip the validation for cssp serialnumber
                    if (deviceRegistrationRequest.DeviceType != _cssp)
                          deviceRegistrationResponse = ValidateRequest(resultDevice, deviceRegistrationRequest);

                    if (deviceRegistrationRequest.DeviceType == _cssp)
                    {
                        string validationMessage = string.Empty;
                        bool isValidCutomerAccount = _deviceLogic.ValidateEndUserCustomerAccountId(deviceRegistrationRequest.CsspReferenceId, out  validationMessage);
                        if (!isValidCutomerAccount)
                        {
                            deviceRegistrationResponse.Add(new ResponseNotification(validationMessage, ResponseState.Error));
                            return this.Json(deviceRegistrationResponse);
                        }

                        bool isValidEndUserSerialNumber = _deviceLogic.ValidateEndUserSerialNumber(deviceRegistrationRequest.SerialNumber, out validationMessage);
                        if (!isValidEndUserSerialNumber)
                        {
                            deviceRegistrationResponse.Add(new ResponseNotification(validationMessage, ResponseState.Error));
                            return this.Json(deviceRegistrationResponse);
                        }                        

                    }
                    

                    if (deviceRegistrationResponse.Any())
                          return this.Json(deviceRegistrationResponse);

                      //IsCloudDevice is set to 1 for validation of panorama. Reset the flag back to 0 in case of NGFW & Panorama
                      if (deviceRegistrationRequest.DeviceType == 0)
                          resultDevice.IsCloudDevice = false;

                      //Format model
                      DeviceRegistrationDto deviceRequest = new DeviceRegistrationDto()
                      {
                          Device = resultDevice,
                          AscCustBklnSupportAuthCode = deviceRegistrationRequest.AscBacklineAuthCode,
                          Eula = new EulaModel()
                          {
                              SelectedEulaId = deviceRegistrationRequest.EulaId
                          }

                      };

                      deviceRegistrationResponse = RegisterDevice(deviceRequest);

                      if(deviceRegistrationRequest.LineCards != null)
                          _deviceLogic.UpdateChassisComponents(deviceRegistrationRequest.LineCards);
                  }

              }
              catch (NotSupportedException nex)
              {
                  Elmah.ErrorSignal.FromCurrentContext().Raise(nex);
                  throw;
              }
              catch (Exception ex)
              {
                  Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
              }

              return Json(deviceRegistrationResponse);
          }
        #region Private Methods

        private void SaveEulaAcceptance(long supportAccountId, string email, string serialNumber,long deviceId,int eulaId)
        {
            try
            {
                var eula = new EulaAgreements()
                {
                    DeviceId = deviceId ,
                    EulaContextId = EulaContext.RegisterDevice,
                    EulaId = eulaId,
                    SupportAccountId = supportAccountId,
                    SignedBy = email,
                    Notes = string.Format("SerialNumber entered is {0}", serialNumber)
                };

                _eulaLogic.EndUserAgreements(eula);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        private List<ResponseNotification> RegisterDevice(DeviceRegistrationDto deviceRequest)
        {

            List<ResponseNotification> deviceRegistrationResponse = new List<ResponseNotification>();
            try
            {
                bool isSiteLicenseGranted = false;
                long supportAccountId = deviceRequest.Device.Customer.Id;
                long userAccountId = deviceRequest.Device.UserAccount.Id;
                string userEmail = deviceRequest.Device.UserAccount.Email;

                // Comments By LK On 08/27
                // Remove CurrentState dependencies
                // CurrentState.GetGroup does not exist for account level device registration
                /*Device deviceResponse = _deviceLogic.RegisterDevice(deviceRequest.Device, deviceRequest.Device.CountryName, userAccountId,
                                out isSiteLicenseGranted, CurrentState.GetGroup(),
                                deviceRequest.AscCustBklnSupportAuthCode, false);*/

                Device deviceResponse = _deviceLogic.RegisterDevice(deviceRequest.Device, deviceRequest.Device.CountryName, userAccountId,
                    out isSiteLicenseGranted, null,
                    deviceRequest.AscCustBklnSupportAuthCode, false);

                // For AWS support account update SapID, SFDCID 

                // Comments By LK On 02/08/2020
                // Disable the logic to update SalesforceAccountId programatically
                //_deviceLogic.UpdateSapIdSfdcIdForAwsSupportAccount(supportAccountId, deviceResponse.Id, userAccountId);

                if (!isSiteLicenseGranted && Convert.ToInt32(deviceRequest.DeviceType) != _cssp)
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                        .Raise(
                            new Exception(string.Format("Grant Site License Failed! SN: {0} UserAccountId: {1}",
                               deviceRequest.Device.SerialNumber, deviceRequest.Device.Customer.Id)));

                    deviceRegistrationResponse.Add(new ResponseNotification(Constants.GrantSiteLicenseErrorMessage, ResponseState.Warning));
                }

                string deviceRegistrationMessage = Constants.DeviceRegistrationSuccessMessage;

                ResponseState deviceResponseState = ResponseState.Success;

                if (!string.IsNullOrEmpty(deviceRequest.AscCustBklnSupportAuthCode))
                {
                    deviceRegistrationMessage = string.Format("Device was successfully registered to {0} ({1}).", deviceResponse.Customer.Name, deviceResponse.Customer.Id);
                }

                // Grant Panorama License
                if (deviceRequest.Device.IsPanorama && deviceRequest.Device.GeneratePanoramaLicense)
                {
                    var result = _licenseLogic.GeneratePanoramaDeviceMgtLicense(deviceRequest.Device.SerialNumber, deviceRequest.Device.CpuId, deviceRequest.Device.UuId, deviceRequest.Device.HyperVisorVersion, deviceRequest.Device.PanOsVersion);
                    if (!result)
                    {
                        deviceRegistrationMessage = string.Format("{0}, failed to activate Panorama device management license.", deviceRegistrationMessage);
                        deviceResponseState = ResponseState.Warning;
                    }
                }

                SaveEulaAcceptance(deviceResponse.Customer.Id,userEmail,deviceRequest.Device.SerialNumber,deviceResponse.Id,deviceRequest.Eula.SelectedEulaId);
                deviceRegistrationResponse.Add(new ResponseNotification(deviceRegistrationMessage,deviceResponseState));

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                deviceRegistrationResponse.Add(new ResponseNotification(ex.Message.ToString(),ResponseState.Error));
            }

            return deviceRegistrationResponse;
        }

        private List<ResponseNotification> ValidateRequest(Device deviceRequest , DeviceRegistrationRequest deviceRegistrationRequest)
        {
            List<ResponseNotification> deviceRegistrationResponse = new List<ResponseNotification>();
            try
            {
                if (deviceRegistrationRequest != null)
                {
                    #region DeviceRegistrationRequest Model Validation
                    //check if model is valid
                    ValidationContext validationContextReg = new ValidationContext(deviceRegistrationRequest);
                    ICollection<ValidationResult> errors = new List<ValidationResult>();
                    bool Valid = Validator.TryValidateObject(deviceRegistrationRequest, validationContextReg, errors, true);

                    if (!Valid)
                    {
                        foreach (var error in errors)
                        {
                            deviceRegistrationResponse.Add(new ResponseNotification(error.ErrorMessage, ResponseState.Error));
                        }
                    }
                    #endregion


                    //Check if Device model is valid
                    ValidationContext validationContext = new ValidationContext(deviceRequest);
                    ICollection<ValidationResult> errorsList = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(deviceRequest, validationContext, errorsList, true);

                    if (!isValid)
                    {
                        foreach (var error in errorsList)
                        {
                            deviceRegistrationResponse.Add(new ResponseNotification(error.ErrorMessage, ResponseState.Error));
                        }
                    }
                    else
                    {
                        #region Serial Number Validation Against DB
                        //Check if SN exists and not registered 
                        if (!string.IsNullOrEmpty(deviceRequest.SerialNumber))
                        {
                            string message = _deviceLogic.CheckIsSerialNumberValidandNotRegisterd(deviceRequest.SerialNumber);

                            if (!string.IsNullOrEmpty(message))
                            {
                                deviceRegistrationResponse.Add(new ResponseNotification(message, ResponseState.Error));

                            }
                        }
                        #endregion

                        #region User Access Validation
                        //Check if user has access to register device in the given support account

                        //ASC registering SN in customer account
                        bool isAscUser = _userProfileDataAccess.IsAscUserBySfdcLoginId(string.Empty, deviceRequest.UserAccount.Id);

                        if (!isAscUser)
                        {
                            var userPermissions = _securityLogic.GetSupportAccountPermissionsForPrincipal(
                                deviceRegistrationRequest.CustomerSupportAccountId, deviceRequest.UserAccount);
                            bool hasAccess = userPermissions.Contains(Permission.RegisterDevice);

                            if (!hasAccess)
                            {
                                deviceRegistrationResponse.Add(
                                    new ResponseNotification(Constants.UnauthorizedMessage, ResponseState.Error));
                            }
                        }

                        #endregion

                    }
                }

            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
            return deviceRegistrationResponse;
        }

        private Device CreateDeviceModelByDeviceType(DeviceRegistrationRequest deviceRegistrationRequest)
        {
            Device deviceModel = new Device();
            try
            {
                if (deviceRegistrationRequest != null)
                {

                    deviceModel.SerialNumber = deviceRegistrationRequest.SerialNumber;
                    deviceModel.Address1 = deviceRegistrationRequest.Address1;
                    deviceModel.Address2 = deviceRegistrationRequest.Address2;
                    deviceModel.City = deviceRegistrationRequest.City;
                    deviceModel.CountryId = deviceRegistrationRequest.CountryId;
                    deviceModel.CountryName = deviceRegistrationRequest.CountryId > 0 ? _deviceLogic.GetCountryCodeById(deviceRegistrationRequest.CountryId.Value) : deviceRegistrationRequest.CountryName;
                    deviceModel.Region = string.IsNullOrEmpty(deviceRegistrationRequest.RegionText) ? deviceRegistrationRequest.Region : deviceRegistrationRequest.RegionText;
                    deviceModel.RegionText = deviceRegistrationRequest.RegionText;
                    deviceModel.PostalCode = deviceRegistrationRequest.PostalCode;
                    deviceModel.Comments = deviceRegistrationRequest.Comments;
                    deviceModel.CommentsEncoded = WebUtility.UrlDecode(deviceRegistrationRequest.CommentsEncoded);
                    deviceModel.PanOsVersion = deviceRegistrationRequest.PanOsVersion;
                    deviceModel.DeviceCategoryId = deviceRegistrationRequest.DeviceCategoryId;
                    deviceModel.GeneratePanoramaLicense = deviceRegistrationRequest.GeneratePanoramaLicense;
                    deviceModel.IsPanorama = deviceRegistrationRequest.IsPanorama;
                    deviceModel.LineCards = deviceRegistrationRequest.LineCards;
                    deviceModel.LastUpdateByEmail = deviceRegistrationRequest.UserEmail;
                    deviceModel.LastUpdateByUserAccountId = deviceRegistrationRequest.UserAccountId;
                    deviceModel.IsCloudDevice = false; //for NGFW & Panorama
                    deviceModel.Customer = new SupportAccount()
                    {
                        Id = deviceRegistrationRequest.CustomerSupportAccountId,
                        Name = deviceRegistrationRequest.CustomerSupportAccountName
                    };
                    deviceModel.UserAccount = new UserAccount()
                    {
                        Id = deviceRegistrationRequest.UserAccountId,
                        Email = deviceRegistrationRequest.UserEmail
                    };

                    deviceModel.UuId = deviceRegistrationRequest.Uuid != null ? deviceRegistrationRequest.Uuid : null;
                    deviceModel.CpuId = deviceRegistrationRequest.Cpuid != null ? deviceRegistrationRequest.Cpuid : null;
                    deviceModel.HyperVisorVersion = deviceRegistrationRequest.Hypervisor != null ? deviceRegistrationRequest.Hypervisor : null;
                    deviceModel.IsVirtualMachine = deviceRegistrationRequest.IsVirtualMachine;

                    if (deviceRegistrationRequest.DeviceType == _aws)
                    {
                        deviceModel.AwsSerialNumber = deviceRegistrationRequest.SerialNumber;
                        deviceModel.AwsCpuid = deviceRegistrationRequest.Cpuid;
                        deviceModel.AwsUuid = deviceRegistrationRequest.Uuid;
                        deviceModel.IsAwsDevice = true;
                        deviceModel.IsCloudDevice = true;
                        deviceModel.HyperVisorVersion = ApiConstants.HYPERVISOR_VERSION_AWS;
                        deviceModel.SerialNumber = string.Empty;
                    }
                    else if (deviceRegistrationRequest.DeviceType == _azure)
                    {
                        deviceModel.IsAzureDevice = true;
                        deviceModel.AzureCpuid = deviceRegistrationRequest.Cpuid;
                        deviceModel.AzureSerialNumber = deviceRegistrationRequest.SerialNumber;
                        deviceModel.AzureUuid = deviceRegistrationRequest.Uuid;
                        deviceModel.IsCloudDevice = true;
                        deviceModel.HyperVisorVersion = ApiConstants.HYPERVISOR_VEERSION_AZURE;
                        deviceModel.SerialNumber = string.Empty;
                    }
                    else if (deviceRegistrationRequest.DeviceType == _cssp)
                    {
                        throw new NotSupportedException("ERROR: New purchases and activations for this product are no longer allowed. Support is scheduled to end in October 2026.");
                    }
                    else if(deviceRegistrationRequest.DeviceType == _gcp)
                    {
                        deviceModel.IsGoogleCloudDevice = true;
                        deviceModel.GoogleCloudSerialNumber = deviceRegistrationRequest.SerialNumber;
                        deviceModel.GoogleCloudCpuid = deviceRegistrationRequest.Cpuid;
                        deviceModel.GoogleCloudUuid = deviceRegistrationRequest.Uuid;
                        deviceModel.IsCloudDevice = true;
                        deviceModel.HyperVisorVersion = ApiConstants.HYPERVISOR_VERSION_GOOGLE;
                        deviceModel.SerialNumber = string.Empty;
                    }
                    else if (deviceRegistrationRequest.IsPanorama)
                    {
                        deviceModel.IsCloudDevice = true;
                    }

                    deviceModel.DeviceName = deviceRegistrationRequest.DeviceName;
                    deviceModel.DeviceNameEncoded = WebUtility.UrlDecode(deviceRegistrationRequest.DeviceNameEncoded);
                    // ApplyEsaLicenseToDevice: null - ESA not expected, 1 - ESA/4HR ESA eligible but not selected, 2: - ESA, 3 - 4HR ESA
                    if (deviceRegistrationRequest.ShowEsa // device is eligible for ESA or 4HR ESA 
                        || deviceRegistrationRequest.Show4HREsa)
                    {
                        deviceModel.ApplyEsaLicenseToDevice = deviceRegistrationRequest.ApplyEsaLicenseToDevice > EsaLicenseType.NOESA
                            ? (int)deviceRegistrationRequest.ApplyEsaLicenseToDevice // and 2:ESA or 3:4HR ESA is selected
                            : 1; // device is eligible but neither ESA nor 4HR ESA is not selected
                    }
                    else // device is not eligible for ESA/4HR ESA 
                    {
                        deviceModel.ApplyEsaLicenseToDevice = null;
                    }
                }
            }
            catch (NotSupportedException nex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(nex);
                throw;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return deviceModel;
        }

        private bool IsPanoramaSerialNumber(string serialNumber)
        {
            var message = _deviceLogic.CheckIsSerialNumberPanorama(serialNumber);
            if (!string.IsNullOrEmpty(message) && message == "This serial number is Panorama")
                return true;
            return false;
        }

        private string IsValidSerialNunmber(string serialNumber, long supportAccountId, out bool isRegistered)
        {
            var message = _deviceLogic.IsValidSerialNumber(serialNumber, supportAccountId, out isRegistered);
            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }
            return string.Empty;
        }

        #endregion

        #endregion

    }
}

