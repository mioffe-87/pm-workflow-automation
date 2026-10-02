namespace Csp.Api.Services.Contracts {
	public interface IAscDeviceRegistrationService
	{
	    bool IsAsc(long supportAccountId);
		bool IsMemberOfAsc(long userAccountId, long supportAccountId);
		long[] GetDeviceSalesOrderSupportAccountId(string serialNumber);
		bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId);

		bool IsRegisteredAuthCode(string authCode);
		bool IsBklnAuthCode(string authCode);
		long GetBklnSupportCenterSupportAccountId(string authCode);

		bool HasAnyMemberships(long supportAccountId);

		string GetSupportAccountNameBySupportAccountId(long supportAccountId);

		void UpdateForDeviceRegistrationWithoutMembershipCheck(string serialNumber, string region, long? categoryId, long lastUpdateByUserAccountId, long supportAccountId);
	}
}
