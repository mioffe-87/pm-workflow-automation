using System.Collections.Generic;
using Csp.Api.Model;
using License = Csp.Api.EntityClasses.License;

namespace Csp.Api.Services.Contracts {

	public interface ILicenseRepository {

		List<License> ReadLicenses(long[] licenseIdArray);
		void UpdateLicenseSupportAccountId(long deviceId, long supportAccountId);
		bool IsAuthCodeValid(string authCode);
        SiteLicenseResult IsSiteLicenseAuthCodeValid(string authCode);
		bool DoesCspLicenseHaveAsc(string authCode);
		long[] GetSupportAccountIdBySalesforceAccountId(string salesforceAccountId);
		void UpdateLicenseAsc(string authCode, long ascSupportAccountId);
		void LogToAscInformationReview(long licenseId, string ascSalesforceAccountId, string ascName, string authCode, string reason);
        List<long> GetActivatedTrialLicenseId(string serialNumber, List<string> partIds);

		bool UpdateSapIdAndSfdcIdForAwsAccount(string authCode, long supportAccountId, long userAccountId, out string currentSfdcAccountId, out string newSfdcAccountId);
		bool IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(long supportAccountId, string awsDefaultAccountId);

	    VmUpgrade GetVmUpgradeInfo(long deviceId, string transactionId);
        IEnumerable<ReleasedLicenseItem> GetReleasedVMLicenses(DataParameters parameters, long supportAccountId);
        bool IsCloudServiceAuthCode(string authCode);
	    bool IsPanoramaUpgradeAuthCodeActivation(string serialNumber, string authcode);
        bool IsPae(string authcode);

        // 2021-04-26: JK - CSP-6337
        bool IsSaasInline(string authcode);

    }

    public enum SiteLicenseResult
    {
        Valid,
        InvalidCode,
        NotASiteLicense,
    }
}
