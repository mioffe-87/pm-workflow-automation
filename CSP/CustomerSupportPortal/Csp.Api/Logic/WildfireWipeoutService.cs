using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {

	public class WildfireWipeoutService : IWildfireWipeoutService {

		private readonly IWildfireWipeoutWebService _wildfireWipeoutWebService;
		private readonly IApplicationSettings _applicationSettings;
		private readonly IEmailHelper _emailHelper;
		private readonly IWildfireRepository _wildfireRepository;
		private readonly IDeviceDataAccess _deviceDataAccess;

		public WildfireWipeoutService(IWildfireWipeoutWebService wildfireWipeoutWebService, IApplicationSettings applicationSettings, IEmailHelper emailHelper, IWildfireRepository wildfireRepository, IDeviceDataAccess deviceDataAccess) {
			_wildfireWipeoutWebService = wildfireWipeoutWebService;
			_applicationSettings = applicationSettings;
			_emailHelper = emailHelper;
			_wildfireRepository = wildfireRepository;
			_deviceDataAccess = deviceDataAccess;
		}

		public bool WildFireWipeout(string serialnumber) {

			var results = _wildfireWipeoutWebService.WildFireWipeout(serialnumber);

			foreach (var r in results) {
				var hasError = !string.IsNullOrEmpty(r.ErrorMessage);

				if (!hasError) {

					_deviceDataAccess.LogWipeOutNFRWildfire(r.Data, r.ServiceUrl, r.RunTimeMilliseconds,
																						 r.IsSuccess ? "Success" : (r.ResponseStatusCode + " - Invalid Status Code"),
																						 null);
				} else {

					_deviceDataAccess.LogWipeOutNFRWildfire(r.Data, r.ServiceUrl, r.RunTimeMilliseconds, "Exception", r.ErrorMessage);
					_emailHelper.SendGeneralMail(_applicationSettings.WildfireErrorMailTo, "Wildfire Integration Error.",
																			 "Details: " + r.ErrorMessage);
				}
			}

			var anyFailures = results.Any(x => x.IsSuccess == false);
			return !anyFailures;
		}

		public object ReplaceEmailNotification(string oldSerialNumber, string newSerialNumber) {
			throw new NotImplementedException();
		}

		public bool DeleteEmailNotification(long wildfireUserAccountId, long supportAccountId, long? deviceId) {

			//Get email
			var email = _wildfireRepository.GetEmailByWildfireUserAccountId(wildfireUserAccountId);

			//Get device serial number if needed
			string deviceSerialNumber = null;
			if (deviceId.HasValue) {
				deviceSerialNumber = _wildfireRepository.GetSerialNumberByDeviceId(deviceId.Value);
			}

			var results = _wildfireWipeoutWebService.DeleteNotificationEmail(email, deviceSerialNumber: deviceSerialNumber);

			foreach (var r in results) {
				var hasError = !string.IsNullOrEmpty(r.ErrorMessage);

				if (!hasError) {

					_deviceDataAccess.LogWipeOutNFRWildfire(r.Data, r.ServiceUrl, r.RunTimeMilliseconds,
																						 r.IsSuccess ? "Success" : (r.ResponseStatusCode + " - Invalid Status Code"),
																						 null);
				} else {

					_deviceDataAccess.LogWipeOutNFRWildfire(r.Data, r.ServiceUrl, r.RunTimeMilliseconds, "Exception", r.ErrorMessage);
					_emailHelper.SendGeneralMail(_applicationSettings.WildfireErrorMailTo, "Wildfire Integration Error.",
																			 "Details: " + r.ErrorMessage);
				}
			}

			var anyFailures = results.Any(x => x.IsSuccess == false);
			return !anyFailures;
		}

		public bool DeleteEmailNotificationForDevices(long wildfireUserAccountId, long supportAccountId, IEnumerable<long> newDeviceIds) {

			//Get existing deviceIds (if any): SELECT deviceid FROM wildfiredevice WHERE WildfireUserAccountId = :WildfireUserAccountId AND SupportAccountId = :SupportAccountId;
			var existingDeviceIds = _wildfireRepository.GetExistingDevicesForUser(wildfireUserAccountId, supportAccountId);

			//compare them to the newIds to see if any are removed
			var removedIds =
				from exist in existingDeviceIds
				where !newDeviceIds.Contains(exist)
				select exist;

			//call delete notification for each removed device
			var isSuccess = true;

			foreach (var removedId in removedIds) {
				var result = DeleteEmailNotification(wildfireUserAccountId, supportAccountId, removedId);

				isSuccess = isSuccess && result;
			}

			return isSuccess;
		}
	}
}
