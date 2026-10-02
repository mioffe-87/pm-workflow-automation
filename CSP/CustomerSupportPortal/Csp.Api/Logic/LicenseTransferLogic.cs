using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel.Security;
using System.Web.Mvc;
using Csp.Api.DataAccess;
using Csp.Api.Logic.EvalWebService;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class LicenseTransferLogic : ILicenseTransferLogic {

		private readonly ILogDeviceRepository _logDeviceRepository;
		private readonly ILicenseLogic _licenseLogic;
		private readonly IDeviceRepository _deviceRepository;
		private readonly ICurrentStateService _currentStateService;
		private readonly ISupportAccountDevicesRepository _supportAccountDevicesRepository;

		public LicenseTransferLogic(ILogDeviceRepository logDeviceRepository, ILicenseLogic licenseLogic, IDeviceRepository deviceRepository, ICurrentStateService currentStateService, ISupportAccountDevicesRepository supportAccountDevicesRepository) {
			_logDeviceRepository = logDeviceRepository;
			_licenseLogic = licenseLogic;
			_deviceRepository = deviceRepository;
			_currentStateService = currentStateService;
			_supportAccountDevicesRepository = supportAccountDevicesRepository;
		}

		/// <summary>
		/// Determines if this source device supports transfers.
		/// Either it is marked Device.IsRMAReturn by a CSR
		/// or there is an existing spare with ModelCompatibility
		/// </summary>
		/// <param name="srcDeviceId"></param>
		/// <param name="userAccountId"></param>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
		public bool IsValidSourceForTransfer(long srcDeviceId, long userAccountId, long supportAccountId, IEnumerable<DeviceListItemModelCompatibility> devices = null) {

			//Must either be device.IsRMAReturn or have a PartMaster.ModelCompatibility equal to that of a currently available spare

			var deviceDataAccess = new DeviceDataAccess();
			var srcDevice = deviceDataAccess.Read(srcDeviceId, userAccountId);

			if (srcDevice.IsRMAReturn) return true;

            //if device has E-licenses then restrict license transfer from this device
            bool hasElicenses = deviceDataAccess.CheckIfDeviceHasEmergencyLicenses(srcDeviceId);
            if (hasElicenses) return false; 

			var dests = GetAvailableDestDevices(srcDeviceId, userAccountId, supportAccountId, devices);

			return dests.Any();
		}

		/// <summary>
		/// Removes NFR and LAB designations
		/// </summary>
		/// <param name="modelCompatibility"></param>
		/// <returns></returns>
		private static string StripModelCompatibility(string modelCompatibility) {
			return modelCompatibility.Replace("NFR-", string.Empty).Replace("LAB-", string.Empty);
		}

		public bool IsValidDestinationForTransfer(long destDeviceId, long userAccountId, long supportAccountId) {
			//Must either be device.IsRMAReplacement or a spare

			var deviceDataAccess = new DeviceDataAccess();
			var srcDevice = deviceDataAccess.Read(destDeviceId, userAccountId);

			if (srcDevice.IsRMAReplacement) return true;

			var srcs = GetAvailableSrcDevices(destDeviceId, userAccountId, supportAccountId);

			return srcs.Any();
		}

		/// <summary>
		/// Get's valid src devices based on the dest device.
		/// 
		/// If dest device is a pre-approved RMA, then looks
		/// for pre-approved src RMA devices with identical PartNumbers.
		/// 
		/// Otherwise, if dest device is a spare, then looks for
		/// matching modified ModelCompatibility
		/// </summary>
		/// <param name="destDeviceId"></param>
		/// <param name="userAccountId"></param>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
		public IEnumerable<TransferLicenseAvailableDevice> GetAvailableSrcDevices(long destDeviceId, long userAccountId, long supportAccountId) {

			var deviceDataAccess = new DeviceDataAccess();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();

			var destDevice = deviceDataAccess.Read(destDeviceId, userAccountId);

			var availableSrcs = new List<TransferLicenseAvailableDevice>();
            string modelCompatibility =  destDevice.ModelCompatibility.Replace("OSS-", string.Empty).Replace("SPR-", string.Empty);
            DataParameters transferDataParameters = new DataParameters()
            {
                PaginationCount = int.MaxValue,
                SearchField = "ModelCompatibility",
                SearchValue = modelCompatibility
            };

			var devices = _supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Prod, transferDataParameters, _currentStateService.GetCurrentUser().RoleSecurities, userAccountId)
    .Union(
        _supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Spare,transferDataParameters, _currentStateService.GetCurrentUser().RoleSecurities, userAccountId)
    )
				.Where(x => x.DeviceId != destDeviceId);

			//Put pre-approved devices on top, then order by serial number
			var orderedDevices = devices.OrderByDescending(x => x.IsRMAReturn).ThenBy(x => x.SerialNumber);
            var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

			foreach (var srcDevice in orderedDevices) {

				if (availableSrcs.Any(x => x.DeviceId == srcDevice.DeviceId))
					continue;

                bool hasElicenses = deviceDataAccess.CheckIfDeviceHasEmergencyLicenses(srcDevice.DeviceId);

                if (!hasElicenses)
                {
                    srcDevice.SupportAccountId = supportAccountId.ToString();
                    srcDevice.SupportAccountName = companyName;
                    availableSrcs.Add(new TransferLicenseAvailableDevice(srcDevice));
                    continue;
                }


                //Pre-approved transfers
                //if (destDevice.IsRMAReplacement) {
                //    if (srcDevice.IsRMAReturn && srcDevice.ModelName == destDevice.PartNumber)
                //    {
                //        srcDevice.SupportAccountId = supportAccountId.ToString();
                //        srcDevice.SupportAccountName = companyName;
                //        availableSrcs.Add(new TransferLicenseAvailableDevice(srcDevice));
                //        continue;
                //    }
                //}

                ////Un-approved spare transfer
                //if (DeviceLogic.IsModelNameSpare(destDevice.PartNumber)) {
                //    var srcModelCompatibility = StripModelCompatibility(srcDevice.ModelCompatibility);
                //    if (destDevice.ModelCompatibility.Contains(srcModelCompatibility)) {
                //        srcDevice.SupportAccountId = supportAccountId.ToString();
                //        srcDevice.SupportAccountName = companyName;
                //        availableSrcs.Add(new TransferLicenseAvailableDevice(srcDevice));
                //        continue;
                //    }
                //}
            }


            if (_currentStateService != null && _currentStateService.GetSupportAccountId() != null)
            {
                if (DependencyResolver.Current.GetService<IAscDeviceRegistrationService>().IsMemberOfAsc(userAccountId, Convert.ToInt64(_currentStateService.GetSupportAccountId())))
                {
                    var licenseTransferDataAccess = new LicenseTransferDataAccess();
                    var ascDeviceList = licenseTransferDataAccess.GetCustomerDeviceInforForAsc(Convert.ToInt64(_currentStateService.GetSupportAccountId()), destDevice.ModelCompatibility);

                    var listBoth = availableSrcs.Union(ascDeviceList,new TransferLicenseAvailableDeviceComparer()).ToList();
                    availableSrcs = listBoth;
                 }
            }
			
           return availableSrcs;
		}


		public IEnumerable<TransferLicenseAvailableDevice> GetAvailableArchivedSrcDevices(long destDeviceId, string searchText, long userAccountId, long supportAccountId) {

			if (string.IsNullOrEmpty(searchText)) return new TransferLicenseAvailableDevice[0];


			var data = _logDeviceRepository.SearchArchivedDevices(userAccountId, supportAccountId, searchText);

			return data;
		}

		/// <summary>
		/// Gets available destinations based on the source device.
		/// 
		/// If the source is RMA Return and Dest is RMA replacement, then the PartNumbers must match
		/// 
		/// If the destination device is a spare, then the stripped ModelCompatability must match
		/// </summary>
		/// <param name="srcDeviceId"></param>
		/// <param name="userAccountId"></param>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
		public IEnumerable<TransferLicenseAvailableDevice> GetAvailableDestDevices(long srcDeviceId, long userAccountId, long supportAccountId, IEnumerable<DeviceListItemModelCompatibility> devices = null) {

			var deviceDataAccess = new DeviceDataAccess();
            var supportAccountLogic = DependencyResolver.Current.GetService<ISupportAccountLogic>();

			var srcDevice = deviceDataAccess.Read(srcDeviceId, userAccountId);

			var srcModelCompatibility = StripModelCompatibility(srcDevice.ModelCompatibility ?? "");

			var availableDests = new List<TransferLicenseAvailableDevice>();

			if (devices == null) {
				devices = _supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Prod, new DataParameters(), _currentStateService.GetCurrentUser().RoleSecurities, userAccountId)
																	.Union(
																		_supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Spare,
																																										new DataParameters(), _currentStateService.GetCurrentUser().RoleSecurities, userAccountId)
																																										);
			}

			devices = devices.Where(x => x.DeviceId != srcDeviceId);

			//Put pre-approved devices on top, then order by serial number
			var orderedDevices = devices.OrderByDescending(x => x.IsRMAReturn).ThenBy(x => x.SerialNumber);
            var companyName = supportAccountLogic.GetSupportAccountNameById(supportAccountId);

			foreach (var destDevice in orderedDevices) {
				if (availableDests.Any(x => x.DeviceId == destDevice.DeviceId))
					continue;

				//Pre-approved transfers
				if (srcDevice.IsRMAReturn) {
					if (destDevice.IsRMAReplacement && destDevice.ModelName == srcDevice.PartNumber) {
                        destDevice.SupportAccountId = supportAccountId.ToString();
                        destDevice.SupportAccountName = companyName;
						availableDests.Add(new TransferLicenseAvailableDevice(destDevice));
						continue;
					}
				}

				//Un-approved spare transfer
				if (DeviceLogic.IsModelNameSpare(destDevice.ModelName)) {
					if (destDevice.ModelCompatibility.Equals(srcModelCompatibility) || destDevice.ModelCompatibility.EndsWith("-"+ srcModelCompatibility)) {
                        destDevice.SupportAccountId = supportAccountId.ToString();
                        destDevice.SupportAccountName = companyName;
						availableDests.Add(new TransferLicenseAvailableDevice(destDevice));
						continue;
					}
				}
			}

			return availableDests;
		}

		/// <summary>
		/// Verify the src and dest can be transferred, then
		/// </summary>
		/// <param name="srcDeviceId"></param>
		/// <param name="destDeviceId"></param>
		/// <param name="userAccountId"></param>
		/// <param name="ipAddress"></param>
		/// <returns></returns>
		public TransferLicenseResult TransferLicenses(long srcDeviceId, long destDeviceId, long userAccountId, string ipAddress) {

			if (srcDeviceId == destDeviceId) return new TransferLicenseResult { ErrorMessage = "Source and Destination must be different" };

			var deviceDataAccess = new DeviceDataAccess();

			var srcDevice = deviceDataAccess.Read(srcDeviceId, userAccountId);
			var destDevice = deviceDataAccess.Read(destDeviceId, userAccountId);

			if (string.IsNullOrEmpty(srcDevice.SerialNumber) || string.IsNullOrEmpty(destDevice.SerialNumber)) throw new SecurityAccessDeniedException();

			if (srcDevice.IsRMAReturn && destDevice.IsRMAReplacement && srcDevice.PartNumber == destDevice.PartNumber) {
				return TransferPreApproved(srcDeviceId, destDeviceId, userAccountId, ipAddress);
			}

			if (DeviceLogic.IsModelNameSpare(destDevice.PartNumber)) {
				var srcModelCompatibility = StripModelCompatibility(srcDevice.ModelCompatibility);

				if (destDevice.ModelCompatibility.Contains(srcModelCompatibility)) {
					return TransferSpare(srcDeviceId, destDeviceId, userAccountId, ipAddress);
				}
			}

			return new TransferLicenseResult {
				ErrorMessage = "Invalid Source or Destination device for license transfer: Source Device ID: " + srcDeviceId + " Destination Device ID: " + destDeviceId,
			};
		}

	    public SupportAccount GetSupportAccountIdforDeviceId(long deviceId)
	    {
            var deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.GetSupportAccountIdforDeviceId(deviceId);
	    }

		public TransferLicenseResult TransferRmaLicenses(long srcRmaDeviceId, long destDeviceId, long userAccountId, string ipAddress) {

			// 0) Before starting, validate the user has access to the srcDevice and dest device

			var supportAccountId = _deviceRepository.GetSupportAccountIdByDeviceId(destDeviceId);

			// 1) Temporarily add the RMA'd device back to device_master
			var tempSrcDeviceID = TemporarilyAddRmaDeviceToDeviceMaster(srcRmaDeviceId, supportAccountId);

			// 2) Transfer Spare
			var result = TransferSpare(tempSrcDeviceID, destDeviceId, userAccountId, ipAddress);

			if (!string.IsNullOrEmpty(result.ErrorMessage)) {
				return result;
			}

			// 3) Remove newly added RMA'd device from the device_master
			RemoveRmaDevice(tempSrcDeviceID);

			return result;
		}

		private TransferLicenseResult TransferPreApproved(long srcDeviceId, long destDeviceId, long userAccountId, string ipAddress) {
			var deviceDataAccess = new DeviceDataAccess();

			var srcDevice = deviceDataAccess.Read(srcDeviceId, userAccountId);
			var destDevice = deviceDataAccess.Read(destDeviceId, userAccountId);

			var userAccountDataAccess = new UserAccountDataAccess();
			var curUser = userAccountDataAccess.Read(userAccountId, userAccountId);

			var result = _licenseLogic.TransferIt(srcDevice.SerialNumber, destDevice.SerialNumber, curUser.Email);
            
			if (!result) return new TransferLicenseResult { ErrorMessage = "Failed to transfer licenses!" };

		    if (srcDevice.PartNumber.Contains("-200"))
                ApplyRelaxationForPa200(srcDeviceId,srcDevice.SerialNumber,userAccountId);

			var logTransactionsDataAccess = new LogTransactionsDataAccess();
			logTransactionsDataAccess.Log("transfer", srcDevice.SerialNumber, destDevice.SerialNumber, ipAddress, userAccountId, "Trasnfer Licenses.");

			return new TransferLicenseResult();
		}

	    private void ApplyRelaxationForPa200(long srcDeviceId,string serialnumber,long useraccountId)
	    {
            var licensetransferdataAccess=new LicenseTransferDataAccess();
            licensetransferdataAccess.ExtendPa200(srcDeviceId,useraccountId);
            var result = _licenseLogic.Relicenseit(serialnumber);
	    }

	    private TransferLicenseResult TransferSpare(long srcDeviceId, long destDeviceId, long userAccountId, string ipAddress) {

			var deviceDataAccess = new DeviceDataAccess();

			var srcDevice = deviceDataAccess.Read(srcDeviceId, userAccountId);
			var destDevice = deviceDataAccess.Read(destDeviceId, userAccountId);

			// 1) Email alert
			//EmailLicenseTransferWarning(srcDevice.SerialNumber, destDevice.SerialNumber);

			// 2) Transform spare

			//Log device
			var logDeviceDataAccess = new LogDeviceDataAccess();
			logDeviceDataAccess.Log(destDeviceId, "transformspare", userAccountId);

			//Log transaction
			var logTransactionsDataAccess = new LogTransactionsDataAccess();
			logTransactionsDataAccess.Log("transformSpare", srcDevice.SerialNumber, destDevice.SerialNumber, ipAddress, userAccountId, "Changing Spare (dest) PartNumber into " + srcDevice.PartNumber);

			//Update device to new part number
			var licenseTransferDataAccess = new LicenseTransferDataAccess();
			licenseTransferDataAccess.UpdateDevicePartNumber(destDeviceId, srcDevice.PartNumber);

			// 3) Transfer - now you can use the normal transfer logic
			var result = TransferPreApproved(srcDeviceId, destDeviceId, userAccountId, ipAddress);

            srcDevice.SupportAccountId = GetSupportAccountIdforDeviceId(srcDeviceId).Id;
		    destDevice.SupportAccountId = GetSupportAccountIdforDeviceId(destDeviceId).Id;

            if (srcDevice.SupportAccountId != destDevice.SupportAccountId)
		    {
                deviceDataAccess.TransferDeviceToSupportAccount(destDeviceId, srcDevice.SupportAccountId,userAccountId);
		    }

		    if (!string.IsNullOrEmpty(result.ErrorMessage)) return result;

			return new TransferLicenseResult();
		}

		private long TemporarilyAddRmaDeviceToDeviceMaster(long srcRmaDeviceId, long supportAccountId) {

			var deviceDataAccess = new DeviceDataAccess();

			var srcDeviceId = deviceDataAccess.AddRmaDeviceToDeviceMaster(srcRmaDeviceId);

			_deviceRepository.UpdateSupportAccountIdByDeviceId(srcRmaDeviceId, supportAccountId);

			return srcDeviceId;
		}

		private void RemoveRmaDevice(long tempSrcDeviceID) {

			var deviceDataAccess = new DeviceDataAccess();

			deviceDataAccess.RemoveRmaDeviceFromDeviceMaster(tempSrcDeviceID);
		}

		private void EmailLicenseTransferWarning(string srcSerialNumber, string destSerialNumber) {
			var configValues = new ConfigValues();
			var transferEmailTos = configValues.GetAppSetting(AppSettings.TransferEmailTo);
			var transferEmailArray = transferEmailTos.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

			var msg = new MailMessage();

			foreach (var email in transferEmailArray) {
				msg.To.Add(email.Trim());
			}

			msg.Subject = msg.Body = string.Format("License transfer from {0} to Spare - {1} from Mydevices", srcSerialNumber, destSerialNumber);

			var smtp = new SmtpClient();
			smtp.Send(msg);
		}
	}

	public class TransferLicenseAvailableDevice : IEquatable<TransferLicenseAvailableDevice> {
		public long DeviceId { get; set; }
		public string SerialNumber { get; set; }
		public string PartNumber { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }

		public TransferLicenseAvailableDevice() {

		}

		public TransferLicenseAvailableDevice(DeviceListItemModelCompatibility model) {
			DeviceId = model.DeviceId;
			PartNumber = model.ModelName;
			SerialNumber = model.SerialNumber;
            AccountId = model.SupportAccountId;
            AccountName = model.SupportAccountName;
		}

		public TransferLicenseAvailableDevice(Device model) {
			DeviceId = model.Id;
			PartNumber = model.ModelName;
			SerialNumber = model.SerialNumber;
		}

		public bool Equals(TransferLicenseAvailableDevice other) {
			return DeviceId == other.DeviceId
						 && SerialNumber == other.SerialNumber
						 && PartNumber == other.PartNumber;
		}
	}

	public class TransferLicenseResult {
		public string ErrorMessage { get; set; }
	}

    public class TransferLicenseAvailableDeviceComparer : IEqualityComparer<TransferLicenseAvailableDevice>
    {
        public bool Equals(TransferLicenseAvailableDevice x, TransferLicenseAvailableDevice y)
        {

            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the obj's properties are equal.
            return x.DeviceId == y.DeviceId && x.SerialNumber == y.SerialNumber&& x.PartNumber==y.PartNumber;
            
        }
      
        public int GetHashCode(TransferLicenseAvailableDevice obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;           
            int hashSerialNumber = obj.SerialNumber == null ? 0 : obj.SerialNumber.GetHashCode();
            int hashPartNumber = obj.PartNumber == null ? 0 : obj.PartNumber.GetHashCode();           
            int hashdeviceId= obj.DeviceId.GetHashCode();

            //Calculate the hash code for the obj.
            return hashSerialNumber ^ hashPartNumber ^ hashdeviceId;
           
        }
    }
}
