using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.Exceptions;
using Csp.Api.HelperClasses;
using AutoMapper;
using Device = Csp.Api.EntityClasses.Device;
using License = Csp.Api.EntityClasses.License;

namespace Csp.Api.Logic {

	/// <summary>
	/// Extension class to get a string that represents the the type of the license. The value is used 
	/// to categorize the different licenses for each device to ensure the license is only added once. 
	/// </summary>
	internal static class LicenseExtension {

		/// <summary>
		/// Returns a string to represent the type of license to ensure it is only added once per device in the list. 
		/// </summary>
		/// <param name="license"></param>
		/// <returns></returns>
		internal static string GetLicenseType(this License license) {

			/* This gets the partnumber via cached value in nHibernate. This may no longer work depending on possible changes in NHibernate.
			 * If that is the case then the following line should be changed to a call. 
			 */
			var partNumber = license.PartMasterId_Partmaster_.PartNumber;

			/*This is set to the partNumber first. It is only overridden with a different value if it is found as
			 * one of the conditions.
			 */
			var licenseType = partNumber;

			if (partNumber.Contains("-TP")) {
				licenseType = "TP";
			} else if (partNumber.Contains("-SVC")) {
				licenseType = "SVC";
			} else if (partNumber.Contains("-URL2")) {
				licenseType = "URL2";
			} else if (partNumber.Contains("-URL4")) {
				licenseType = "URL4";
			} else if (partNumber.Contains("WF")) {
				licenseType = "WF";
			} else if (partNumber.Contains("PORTAL")) {
				licenseType = "PORTAL";
			} else if ((partNumber.Contains("-GP") && !partNumber.Contains("-PORTAL")) || partNumber.Contains("-GATEWAY")) {
				licenseType = "GP";
			}

			return licenseType;

		}

	}


	public class BulkRegistrationThreadLogic : BaseLogic, IBulkRegistrationThreadLogic {

		/// <summary>
		/// This private class is used to hold the device and licenses that are to be registered. 
		/// </summary>
		private class DeviceRegistrationItem {

			/// <summary>
			/// This constructor instantiates a DeviceRegistrationItem that holds a device and a list of licenses that are
			/// to be registed. 
			/// </summary>
			/// <param name="device">The device that is to be registered</param>
			public DeviceRegistrationItem(DeviceWithPartMaster device) {

				Device = device;
				Licenses = new Dictionary<string, License>();
			}

			/// <summary>
			/// The device that is to be registered
			/// </summary>
			public DeviceWithPartMaster Device { get; set; }
			/// <summary>
			/// This list of licenses that are to be registered. The key a value that 
			/// designates the type of license to ensure it is only added to a device once. The value is the license
			/// </summary>
			public Dictionary<string, License> Licenses { get; set; }

		}

		//List of Ids from the interface
		private long[] deviceIds;
		private long[] licenseIds;
		//The device registration information passed in from the interface. A single device object holds this info although it applies to all devices in the DeviceIds
		private Api.Model.Device deviceRegistrationInformation;
		//The user that is registering the device
		private long userId;
		//The list of device objects to be registered
		private List<Model.DeviceWithPartMaster> devices = new List<Model.DeviceWithPartMaster>();
		//The list of license objects to be registered 
		private List<License> licenses = new List<License>();
		//The list of items to be registered, holds a device with a list of licenses under it to ensure proper processing
		private Dictionary<string, List<DeviceRegistrationItem>> registrationItems = new Dictionary<string, List<DeviceRegistrationItem>>();
		//The error logs used for reporting problems to the user via email
		private Dictionary<string, List<string>> deviceErrorLog = new Dictionary<string, List<string>>();
		private Dictionary<string, List<string>> licenseErrorLog = new Dictionary<string, List<string>>();
		private List<string> criticalErrorLog = new List<string>();
		//NHibernate data access
		private IDeviceRepository deviceRepository;
		//NHibernate data access
		private ILicenseRepository licenseRepository;
		private readonly IDeviceLogic _deviceLogic;
		private readonly ILookupLogic _lookupLogic;
		private readonly ILicenseLogic _licenseLogic;
		//This does not use dependency injection, it is planned for this to be replaced at another date. 
		private List<Country> _countries = new List<Country>();

		// list of messages used to inform the user in the email notification
		private const string licenseIsNotCompatibleMessage = "The following licenses were not activated as they were not compatible with any of the selected devices:";
		private const string deviceReadErrorMessage = "A problem occured while retrieving your devices. Please contact your customer service representative";
		private const string licenseReadErrorMessage = "A problem occured while retrieving your licenses. Please contact your customer service representative";
		private const string orphanLicenseMessage = "The following licenses were not activated as all of the selected devices already had the another authorization code of the same license. This has happened because you selected a higher quantity of the same license than you have selected devices.";
		private const string deviceAlreadyRegisteredMessage = "The following devices were already registered and could not be processed.";
		private const string deviceLicenseErrorMessage = "The following devices had an error during license activation, please contact a CSR for assistance.";
		private const string deviceRegistrationErrorMessage = "The following devices were unable to be registered, please contact a CSR for assistance.";

		/// <summary>
		/// Public constructor used to instantiate the BulkRegistrationThreadLogic. This is done via dependency injection  
		/// </summary>
		/// <param name="deviceRepositoryInterface">The nHibernate access to data</param>
		/// <param name="licenseRepositoryInterface">The nHibernate access to data </param>
		/// <param name="deviceLogic">Logic class used to access methods</param>
		/// <param name="lookupLogic">Logic class used to access business methods</param>
		public BulkRegistrationThreadLogic(IDeviceRepository deviceRepositoryInterface, ILicenseRepository licenseRepositoryInterface, IDeviceLogic deviceLogic, ILookupLogic lookupLogic, ILicenseLogic licenseLogic) {
			deviceRepository = deviceRepositoryInterface;
			licenseRepository = licenseRepositoryInterface;
			_deviceLogic = deviceLogic;
			_lookupLogic = lookupLogic;
			_licenseLogic = licenseLogic;
		}

		/// <summary>
		/// Registers the device 
		/// </summary>
		/// <param name="device">The device to be registered</param>
		private void RegisterDevice(DeviceWithPartMaster device) {

			/* Maps the device from an entity into a device from the internal API model
			 * The mapping occurs so that the normal device registration can be processed.
			 * This is a temporary solution. Eventually the entity classes will likley be 
			 * used for all logic processing in the future.
			 */

			/* This is another area where a call may need to be included rather than reading the cached value from
			 * Nhibernate. If the device has a support account Id then it is registered. If the device is already 
			 * registered the information should be added to the errorlog so it is included in the email to 
			 * inform the user.
			 */
			if (device.SupportAccountId.HasValue) {

				//The message is used as the key in the error log, if it does not exist, then create the entry
				if (!deviceErrorLog.ContainsKey(deviceAlreadyRegisteredMessage)) {

					deviceErrorLog.Add(deviceAlreadyRegisteredMessage, new List<string>());
				}
				deviceErrorLog[deviceAlreadyRegisteredMessage].Add(device.SerialNumber);
			}

			//populate the device with the information from the bulk registration interface
			device.Region = deviceRegistrationInformation.Region;
			device.PostalCode = deviceRegistrationInformation.PostalCode;
			device.City = deviceRegistrationInformation.City;
			device.CountryId = deviceRegistrationInformation.CountryId;
			device.LastUpdateByUserAccountId = userId;
			device.Customer.Id = deviceRegistrationInformation.Customer.Id;
			device.LastUpdateByEmail = deviceRegistrationInformation.LastUpdateByEmail;

			long countryId = device.CountryId ?? 0;
			var country = _countries.SingleOrDefault(c => c.Id == countryId);
			var countryName = (country != null) ? country.Name : string.Empty;

			var siteLicenseGranted = false;

			try {
				_deviceLogic.RegisterDevice(device, countryName, userId, out siteLicenseGranted);

			} catch (Exception ex) {

				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

				if (ex is CspNotificationException) {

					//use the message as the key, all devices with the error are listed under the key
					if (!deviceErrorLog.ContainsKey(deviceLicenseErrorMessage)) {

						deviceErrorLog.Add(deviceLicenseErrorMessage, new List<string>());
					}
					//add the device serial number under the error
					deviceErrorLog[deviceLicenseErrorMessage].Add(device.SerialNumber);
				} else {
					//use the message as the key, all devices with the error are listed under the key
					if (!deviceErrorLog.ContainsKey(deviceRegistrationErrorMessage)) {
						//
						deviceErrorLog.Add(deviceRegistrationErrorMessage, new List<string>());
					}
					//Add the device serial number under the error
					deviceErrorLog[deviceRegistrationErrorMessage].Add(device.SerialNumber);
				}
			}
		}

		/// <summary>
		/// Activates the licese on the device. 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="license"></param>
		private void ActivateLicense(DeviceWithPartMaster device, License license) {

			//Activate the license
			var errorMessage = _licenseLogic.ActivateLicense(device.SerialNumber, authorizationCode: license.AuthorizationCode);

			//Add the error message as a key if it does not exist and there is an error message.
			if (!String.IsNullOrEmpty(errorMessage)) {
				if (!licenseErrorLog.ContainsKey(errorMessage)) {
					licenseErrorLog.Add(errorMessage, new List<string>());
				}
				//Add the serial number and authorization code under the license
				licenseErrorLog[errorMessage].Add("Device Serial Number: " + device.SerialNumber + " License Authorization Code: " + license.AuthorizationCode);
			}
		}

		/// <summary>
		/// Sends the results in an email. The logs are iterated through and an exception report is generated with the error message and the 
		/// serial numbers and authorization codes that create the errors. 
		/// </summary>
		private void EmailResults() {
			EmailHelper.SendBulkRegistrationEmail(deviceRegistrationInformation.LastUpdateByEmail, deviceErrorLog, licenseErrorLog, criticalErrorLog);
		}

		/// <summary>
		/// Gets a list of populated device objects for the IDs  that were selected in the interface
		/// </summary>
		private void PopulateDevices() {
			try {
				devices = deviceRepository.ReadDevices(deviceIds);
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				criticalErrorLog.Add(deviceReadErrorMessage);
			}
		}

		/// <summary>
		/// Gets a list of populated license objects for the IDs that were selected in the interface
		/// </summary>
		private void PopulateLicenses() {
			try {
				licenses = licenseRepository.ReadLicenses(licenseIds);
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				criticalErrorLog.Add(licenseReadErrorMessage);
			}
		}

		/// <summary>
		/// Gets the list of countries. This is used to get the country name for a specific country id
		/// </summary>
		private void PopulateCountries() {
			_countries = _lookupLogic.GetCountries().ToList();
		}

		/// <summary>
		/// This adds the list of devices to the dictionary of items to be registered. The key is the model compatibility to ensure only licenese that 
		/// are compatible are added to the list of devices. 
		/// </summary>
		private void AddDevicesToRegistrationItems() {

			foreach (var device in devices) {
				//Get the model compatibility to ensure licenses are added to the correct device
				var model = device.ModelCompatibility;

				//If the key does not exist then add the key to the list of registration items
				if (!registrationItems.ContainsKey(model)) {
					registrationItems.Add(model, new List<DeviceRegistrationItem>());

				}

				registrationItems[model].Add(new DeviceRegistrationItem(device));
			}
		}


		/// <summary>
		/// Adds the license under to one of the devices in the compatible model list of DeviceRegistration Items
		/// </summary>
		/// <param name="devicesToRegister">The list of devices and their licenses</param>
		/// <param name="license">The license to be added</param>
		/// <returns>If the license was added the result is true, otherwise it is false</returns>
		private bool AddLicense(List<DeviceRegistrationItem> devicesToRegister, License license) {

			//loop through all devices in the list
			foreach (var deviceToRegister in devicesToRegister) {

				//Use the extension method to get a string that represents the type of license
				var licenseType = license.GetLicenseType();

				//If the license type is not in the dictionary of licenses for the device then add it and return true
				if (!deviceToRegister.Licenses.ContainsKey(licenseType)) {

					deviceToRegister.Licenses.Add(licenseType, license);
					return true;
				}
			}
			//If we got here the list is iterated through and the license is not added to any device as they all already have a license
			return false;
		}


		/// <summary>
		/// This adds the licenses to the RegistrationItems
		/// </summary>
		private void AddLicensesToRegistrationItems() {

			//Loop through the list of licenses
			foreach (var license in licenses) {

				//Get the model compatibility to ensure the license is added to a device that is compatible. This may have to be changed due to changes in how NHibernate works with caching and sessions
				var model = license.PartMasterId_Partmaster_.ModelCompatibility;

				//If there is a list of devices in the Registration items that is compatible, then add the license to one of the devices
				if (registrationItems.ContainsKey(model)) {

					//If the license can not be added then the license should be reported as an orphaned license in the error log sent to the user in an email. 
					if (!AddLicense(registrationItems[model], license)) {
						AddLicenseError(orphanLicenseMessage, license.AuthorizationCode);
					}

				}
					//The license is compatible with all models so try to add it to one of the devices in the list
				else if (model == "*") {

					//Get all the devices that are listed under the different model compatability keys (e.g. all devices to be registered)
					var devicesItems = registrationItems.SelectMany(x => x.Value).ToList();

					//If the license can not be added then the license should be reported as an orphaned license in the error log sent to the user in an email. 
					if (!AddLicense(devicesItems, license)) {
						AddLicenseError(orphanLicenseMessage, license.AuthorizationCode);
					}

				} else {
					//The license does not have a compatible model in the list. For this reason add it to the error log and inform the user the the license is not compatible with any device
					AddLicenseError(licenseIsNotCompatibleMessage, license.AuthorizationCode);
				}

			}
		}

		/// <summary>
		/// This adds a error to the license error log to be reported to the user via email when the process is completed
		/// </summary>
		/// <param name="message">This is the message that is displayed to the user</param>
		/// <param name="authorizationCode">This is the authorization code that is displayed under the error</param>
		private void AddLicenseError(string message, string authorizationCode) {

			//The message is used as a key with all authorization codes displayed underneath. 
			if (!licenseErrorLog.ContainsKey(message)) {
				//This adds the error message as a key if it does not exist
				licenseErrorLog.Add(message, new List<string>());
			}
			//This adds the authorization code to be reported under the errormessage. 
			licenseErrorLog[message].Add(authorizationCode);
		}

		/// <summary>
		/// This adds all devices and licenses to the list of registration items. If there is an error it is added to the appropriate error log.
		/// </summary>
		private void PopulateRegistrationItems() {

			AddDevicesToRegistrationItems();
			AddLicensesToRegistrationItems();
		}

		/// <summary>
		/// This method is called before the thread starts. This gets all of the devices and licenses from the database and 
		/// populates them in objects. All the items are then added to the list of Registration items, with all devices under 
		/// a model compatibility key. Each device has a dictionary of licenses to register on the device with the license type as 
		/// the key so each type of license can only be registered on the device once. The list of countries is also populated for 
		/// use in the thread. 
		/// </summary>
		private void Initialize() {
			PopulateDevices();
			PopulateLicenses();
			PopulateRegistrationItems();
			PopulateCountries();
		}


		/// <summary>
		/// This process performs a bulk registration of all items in the registration list. This process occurs in the thread
		/// with the results emailed to the user that performed the bulk registration. 
		/// </summary>
		private void BulkRegister() {

			//Gets all the registration items that have a device and dictionary of licenses for registeration. 
			var registrationList = registrationItems.SelectMany(x => x.Value).ToList();

			//loop through the list of registration items (devices)
			foreach (var registrationItem in registrationList) {

				//Register the device that is held in the registration item. 
				RegisterDevice(registrationItem.Device);

				//Get all of the licenses under the device. 
				List<License> deviceLicenses = registrationItem.Licenses.Select(x => x.Value).ToList();

				//loop through the licenses and register each one. 
				foreach (var license in deviceLicenses) {

					ActivateLicense(registrationItem.Device, license);
				}
			}
			//Email the result to the user. 
			EmailResults();
		}


		/// <summary>
		/// This method is used to start a thread and perform the bulk registration of items selected in the bulk registration page. 
		/// </summary>
		/// <param name="deviceIdsForRegistration">The list of Device Ids selected by the user from the sales orders</param>
		/// <param name="licenseIdsForRegistration">The list of license Ids selected by the user from the sales orders</param>
		/// <param name="deviceInfo">The device information that entered in the bulk registration form </param>
		/// <param name="currentUserId">The Id of the user that is performing the bulk registration</param>
		public void StartBulkRegistration(long[] deviceIdsForRegistration, long[] licenseIdsForRegistration, Api.Model.Device deviceInfo, long currentUserId) {
			//populate the private fields with the values for processing bulk registration
			deviceIds = deviceIdsForRegistration;
			licenseIds = licenseIdsForRegistration;
			deviceRegistrationInformation = deviceInfo;
			userId = currentUserId;
			//perform initializes of data that happens outside of the thread. 
			Initialize();
			//Instatiate the thread with the BulkRegistration method
			Thread registrationThread = new Thread(new ThreadStart(BulkRegister));
			//Start the thread
			registrationThread.Start();

		}



	}
}
