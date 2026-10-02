using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Csp.Api.Exceptions;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Device = Csp.Api.EntityClasses.Device;
using License = Csp.Api.EntityClasses.License;
using Csp.Api.HelperClasses;
using System.IO;

namespace Csp.Api.Logic
{
    public class BulkRegistrationService : IBulkRegistrationService
    {
        private List<BulkRegistrationModel> bulkRegistrations = new List<BulkRegistrationModel>();
        private List<DeviceWithPartMaster> devices = new List<DeviceWithPartMaster>();
        private List<License> licenses = new List<License>();

        //The error logs used for reporting problems to the user via email
        private Dictionary<string, List<string>> deviceErrorLog = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> licenseErrorLog = new Dictionary<string, List<string>>();
        private List<string> criticalErrorLog = new List<string>();

        private IDeviceRepository _deviceRepository;
        private ILicenseRepository _licenseRepository;

        private readonly IBulkRegistrationLogic _bulkRegistrationLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ILicenseLogic _licenseLogic;

        // list of messages used to inform the user in the email notification
        private const string licenseIsNotCompatibleMessage = "The following licenses were not activated as they were not compatible with any of the selected devices:";
        private const string bulkRegistrationReadErrorMessage = "A problem occured while retrieving your bulk registrations. Please contact your customer service representative";
        private const string deviceReadErrorMessage = "A problem occured while retrieving your devices. Please contact your customer service representative";
        private const string licenseReadErrorMessage = "A problem occured while retrieving your licenses. Please contact your customer service representative";
        private const string orphanLicenseMessage = "The following licenses were not activated as all of the selected devices already had the another authorization code of the same license. This has happened because you selected a higher quantity of the same license than you have selected devices.";
        private const string deviceAlreadyRegisteredMessage = "The following devices were already registered and could not be processed.";
        private const string deviceLicenseErrorMessage = "The following devices had an error during license activation, please contact a CSR for assistance.";
        private const string deviceRegistrationErrorMessage = "The following devices were unable to be registered, please contact a CSR for assistance.";

        public BulkRegistrationService(IDeviceRepository deviceRepository, ILicenseRepository licenseRepository, IBulkRegistrationLogic bulkRegistrationLogic, IDeviceLogic deviceLogic, ILicenseLogic licenseLogic)
        {
			_deviceRepository = deviceRepository;
			_licenseRepository = licenseRepository;
            _bulkRegistrationLogic = bulkRegistrationLogic;
			_deviceLogic = deviceLogic;
			_licenseLogic = licenseLogic;
		}

        public void Process()
        {
            try
            {
                PopulatePendingBulkRegistrations();
                PopulateDevices();
                PopulateLicenses();

                var ctx = HttpContext.Current;
                var regThread = new Thread(() =>
                {
                    HttpContext.Current = ctx;
                    Register();
                });

                regThread.Start();
            }
            catch (Exception ex)
            {

                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkReg" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, ex.ToString());
                throw new Exception("In BulkReg Process Method: " + ex);
            }
        }

        private void Register()
        {
            try
            {

                var configValues = new ConfigValues();
                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);

                foreach (var reg in bulkRegistrations)
                {
                    var urlViewBulkRegistrationHistory =
                        string.Format("{0}/BulkRegistration/ViewHistory/{1}?bulkRegistrationId={2}", applicationUrl,
                            reg.SupportAccountId, reg.Id);

                    deviceErrorLog = new Dictionary<string, List<string>>();
                    licenseErrorLog = new Dictionary<string, List<string>>();

                    foreach (var dev in reg.BulkRegistrationDevices)
                    {
                        RegisterDevice(reg, dev);

                        foreach (var lic in dev.BulkRegistrationLicenses)
                        {
                            ActivateLicense(dev, lic);
                        }
                    }

                    _licenseLogic.GrantSiteLicenses(reg.SupportAccountId);

                    var bulkRegState = (deviceErrorLog.Count > 0 || licenseErrorLog.Count > 0)
                        ? BulkRegistrationStates.Failure
                        : BulkRegistrationStates.Success;

                    _bulkRegistrationLogic.UpdateBulkRegistrationState(reg.Id,
                        bulkRegState);


                    EmailResults(reg, urlViewBulkRegistrationHistory);
                }
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\BulkReg" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, ex.ToString());
                throw new Exception("Registration Error: " + ex);
            }
        }
        
        private void RegisterDevice(BulkRegistrationModel bulkRegModel, BulkRegistrationDeviceModel deviceModel)
        {

            var device = devices.Single(dev => dev.Id == deviceModel.DeviceId);
            var errorMessage = string.Empty;
            StringBuilder registrationErrors = new StringBuilder();

            if (device != null)
            {
                if (device.SupportAccountId.HasValue)
                {
                    //The message is used as the key in the error log, if it does not exist, then create the entry
                    if (!deviceErrorLog.ContainsKey(deviceAlreadyRegisteredMessage))
                    {
                        deviceErrorLog.Add(deviceAlreadyRegisteredMessage, new List<string>());
                    }
                    deviceErrorLog[deviceAlreadyRegisteredMessage].Add(device.SerialNumber);
                    errorMessage = string.Format("{0}: {1}", deviceRegistrationErrorMessage, device.SerialNumber);
                    registrationErrors.AppendLine(string.Format("Device Serial Number: {0} - {1}", device.SerialNumber, deviceAlreadyRegisteredMessage));
                }

                //populate the device with the information from the bulk registration device defaults
                long deviceCategoryId = 0;
                string deviceCategoryName = string.Empty;

                if (deviceModel.CustomValueDeviceTag != null)
                {
                    long.TryParse(deviceModel.CustomValueDeviceTag.Value, out deviceCategoryId);
                    deviceCategoryName = deviceModel.CustomValueDeviceTag.Name;
                }

                device.DeviceName = deviceModel.CustomValueDeviceName;
                device.DeviceCategoryId = deviceCategoryId;
                device.DeviceCategoryName = deviceCategoryName;
                device.Region = (deviceModel.CustomValueRegion != null && deviceModel.CustomValueRegion.Id > 0) ? deviceModel.CustomValueRegion.Name : deviceModel.CustomValueOtherRegion;
                device.PostalCode = deviceModel.CustomValuePostalCode;
                device.City = deviceModel.CustomValueCity;
                device.Address1 = deviceModel.CustomValueAddress1;
                device.Address2 = deviceModel.CustomValueAddress2;

                if (deviceModel.CustomValueCountry != null)
                {
                    device.CountryId = deviceModel.CustomValueCountry.Id;
                    device.CountryName = deviceModel.CustomValueCountry.Name;
                }

                device.LastUpdateByUserAccountId = bulkRegModel.CreatedById ?? 0;
                device.Customer.Id = bulkRegModel.SupportAccountId;
                device.LastUpdateByEmail = bulkRegModel.CreatedByEmail;
                device.Comments = deviceModel.CustomValueRemarks;

                var countryCode = (deviceModel.CustomValueCountry != null) ? deviceModel.CustomValueCountry.Code : string.Empty;
                var userId = bulkRegModel.CreatedById ?? 0;
                var siteLicenseGranted = false;

                try
                {
                    _deviceLogic.RegisterDevice(device, countryCode, userId, out siteLicenseGranted);
                }
                catch (Exception ex)
                {
                    //if (HttpContext.Current != null)
                    //{
                    //    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    //}
                    //else
                    //{
                        Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                    //}
                    registrationErrors.AppendLine(string.Format("Device Serial Number: {0} - {1}",device.SerialNumber, ex.Message));

                    if (ex is CspNotificationException)
                    {
                        //use the message as the key, all devices with the error are listed under the key
                        if (!deviceErrorLog.ContainsKey(deviceLicenseErrorMessage))
                        {
                            deviceErrorLog.Add(deviceLicenseErrorMessage, new List<string>());
                        }

                        //add the device serial number under the error
                        deviceErrorLog[deviceLicenseErrorMessage].Add(device.SerialNumber);
                        errorMessage = string.Format("{0}: {1}", deviceRegistrationErrorMessage, device.SerialNumber);
                    }
                    else
                    {
                        //use the message as the key, all devices with the error are listed under the key
                        if (!deviceErrorLog.ContainsKey(deviceRegistrationErrorMessage))
                        {
                            deviceErrorLog.Add(deviceRegistrationErrorMessage, new List<string>());
                        }

                        //Add the device serial number under the error
                        deviceErrorLog[deviceRegistrationErrorMessage].Add(device.SerialNumber);
                        errorMessage = string.Format("{0}: {1}", deviceRegistrationErrorMessage, device.SerialNumber);
                    }
                }

                var isSuccess = (string.IsNullOrEmpty(errorMessage));
                _bulkRegistrationLogic.UpdateBulkRegistrationDeviceStatus(deviceModel.Id, isSuccess, errorMessage, registrationErrors.ToString());
            }
        }

        private void ActivateLicense(BulkRegistrationDeviceModel deviceModel, BulkRegistrationLicenseModel licenseModel)
        {
            //Activate the license
            var errorMessage = _licenseLogic.ActivateLicense(deviceModel.SerialNumber, authorizationCode: licenseModel.AuthorizationCode);
            var activationError = errorMessage;

            //Add the error message as a key if it does not exist and there is an error message.
            if (!string.IsNullOrEmpty(errorMessage))
            {
                if (!licenseErrorLog.ContainsKey(errorMessage))
                {
                    licenseErrorLog.Add(errorMessage, new List<string>());
                }

                //Add the serial number and authorization code under the license
                licenseErrorLog[errorMessage].Add("Device Serial Number: " + deviceModel.SerialNumber +
                                                  " License Authorization Code: " + licenseModel.AuthorizationCode);

                _bulkRegistrationLogic.UpdateBulkRegistrationLicenseStatus(licenseModel.Id, false, errorMessage, activationError);
            }
            else
            {
                _bulkRegistrationLogic.UpdateBulkRegistrationLicenseStatus(licenseModel.Id, true, string.Empty, activationError);
            }
        }
        
        private void EmailResults(BulkRegistrationModel bulkRegModel, string urlViewBulkRegistrationHistory)
        {
            var configValues = new ConfigValues();
            var emailTo = bulkRegModel.CreatedByEmail;

            EmailHelper.SendBulkRegistrationEmail(urlViewBulkRegistrationHistory, emailTo, deviceErrorLog, licenseErrorLog, criticalErrorLog);
        }

        private void PopulatePendingBulkRegistrations()
        {
            try
            {
                bulkRegistrations = _bulkRegistrationLogic.GetAllPendingBulkRegistrations();
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                criticalErrorLog.Add(bulkRegistrationReadErrorMessage);
            }
        }

        private void PopulateDevices()
        {
            try
            {
                var deviceIds = (from reg in bulkRegistrations from dev in reg.BulkRegistrationDevices select dev.DeviceId).ToArray();

                devices = _deviceRepository.ReadDevices(deviceIds);
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                criticalErrorLog.Add(deviceReadErrorMessage);
            }
        }

        private void PopulateLicenses()
        {
            try
            {
                var licenseIds = (from reg in bulkRegistrations from dev in reg.BulkRegistrationDevices from lic in dev.BulkRegistrationLicenses select lic.LicenseId).ToArray();

                licenses = _licenseRepository.ReadLicenses(licenseIds);
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                criticalErrorLog.Add(licenseReadErrorMessage);
            }
        }
    }
}
