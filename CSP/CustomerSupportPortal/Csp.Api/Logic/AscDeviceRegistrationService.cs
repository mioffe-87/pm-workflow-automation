using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class AscDeviceRegistrationService : IAscDeviceRegistrationService {
		private readonly IAscDeviceRegistrationRepository _ascDeviceRegistrationRepository;
		private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly IDeviceDataAccess _deviceDataAccess;

		public AscDeviceRegistrationService(IAscDeviceRegistrationRepository ascDeviceRegistrationRepository, ISupportAccountRepository supportAccountRepository,IDeviceDataAccess deviceDataAccess) {
			_ascDeviceRegistrationRepository = ascDeviceRegistrationRepository;
			_supportAccountRepository = supportAccountRepository;
            _deviceDataAccess = deviceDataAccess;
        }

	    public bool IsAsc(long supportAccountId)
	    {
	        var isAsc = _ascDeviceRegistrationRepository.IsAsc(supportAccountId);
	        return isAsc;
	    }

		public bool IsMemberOfAsc(long userAccountId, long supportAccountId) {
			var isAsc = _ascDeviceRegistrationRepository.IsAsc(supportAccountId);
			if (!isAsc) return false;

			var isMember = _ascDeviceRegistrationRepository.IsSupportMemberOfSupportAccount(userAccountId, supportAccountId);
			return isMember;
		}

		public long[] GetDeviceSalesOrderSupportAccountId(string serialNumber) {
			//return _ascDeviceRegistrationRepository.GetDeviceSalesOrderSupportAccountId(serialNumber);
            return _deviceDataAccess.GetDeviceSalesOrderSupportAccountId(serialNumber);
        }

		public bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId) {
			//return _ascDeviceRegistrationRepository.HasBklnSupportInSalesOrder(serialNumber, supportCenterSupportAccountId);
            return _deviceDataAccess.HasBklnSupportInSalesOrder(serialNumber, supportCenterSupportAccountId);
        }


		public bool IsRegisteredAuthCode(string authCode) {
			return _ascDeviceRegistrationRepository.IsRegisteredAuthCode(authCode);
		}

		public bool IsBklnAuthCode(string authCode) {
			return _ascDeviceRegistrationRepository.IsBklnAuthCode(authCode);
		}

		public long GetBklnSupportCenterSupportAccountId(string authCode) {
			return _ascDeviceRegistrationRepository.GetBklnSupportCenterSupportAccountId(authCode);
		}

		public bool HasAnyMemberships(long supportAccountId) {
			return _ascDeviceRegistrationRepository.HasAnyMemberships(supportAccountId);
		}

		public string GetSupportAccountNameBySupportAccountId(long supportAccountId) {
			return _supportAccountRepository.GetSupportAccountNameBySupportAccountId(supportAccountId);
		}

		public void UpdateForDeviceRegistrationWithoutMembershipCheck(string serialNumber, string region, long? categoryId,
																						long lastUpdateByUserAccountId, long supportAccountId) {

			_ascDeviceRegistrationRepository.UpdateForDeviceRegistrationWithoutMembershipCheck(serialNumber, region,
																																																categoryId,
																																																lastUpdateByUserAccountId,
																																																supportAccountId);
		}
	}
}
