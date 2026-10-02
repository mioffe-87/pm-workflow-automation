namespace Csp.Api.Services.Contracts {
	public interface IAscDeviceRegistrationRepository {
		bool IsAsc(long supportAccountId);
		bool IsSupportMemberOfSupportAccount(long userAccountId, long supportAccountId);

		long[] GetDeviceSalesOrderSupportAccountId(string serialNumber);

		bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId);


		bool IsRegisteredAuthCode(string authCode);

		bool IsBklnAuthCode(string authCode);

		bool HasAnyMemberships(long supportAccountId);

		long GetBklnSupportCenterSupportAccountId(string authCode);

		void UpdateForDeviceRegistrationWithoutMembershipCheck(string serialNumber, string region, long? categoryId, long lastUpdateByUserAccountId, long supportAccountId);
	}
}
