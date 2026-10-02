using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IBulkRegistrationRepository
    {
        bool IsValidCustomerAndDeviceCode(long supportAccountId, string customerCode, string deviceCode);
        List<ClaimedAssetListItem> GetAvailablePurchases(long supportAccountId, string customerCode, string deviceCode);
        List<ClaimedAssetListItem> GetClaimedPurchases(long supportAccountId);
        void RemoveUsedClaims(long supportAccountId);
        long? GetCurrentBulkRegistrationId(long supportAccountId, long userAccountId);
        IEnumerable<PrepBulkRegistration.SelectableDevice> GetClaimedDevices(long supportAccountId);
        IEnumerable<PrepBulkRegistration.SelectableDevice> GetRegisteredDevices(long supportAccountId);
        List<BulkRegistrationHistoryListItem> GetBulkRegistrationHistory(long supportAccountId);
        void ClaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases);
        void UnclaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases);

        List<BulkRegistrationDeviceModel> GetBulkRegistrationsWithDevicesWithLicenses(long bulkRegistrationId, long supportAccountId);
        void UpdateBulkRegistrationDevicesAndLicenses(long bulkRegistrationId, List<BulkRegistrationDeviceModel> devices);
        void UpdateBulkRegistrationState(long bulkRegistrationId, BulkRegistrationStates bulkRegistrationState);
        IEnumerable<PrepBulkRegistration.Availablelicense> GetAvailableLicensesForPrepBulkReg(long supportAccountId, long userAccountId, long? bulkRegistrationId, string strippedModelCompatibility);
        //List<PrepBulkRegistration.DeviceAndLicenses> GetAppliedLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, long[] deviceIds);
        PrepBulkRegistration GetPrepBulkRegistrationDetails(long supportAccountId, long bulkRegistrationId);
        long CreateBulkRegistration(PrepBulkRegistration draft, long userAccountId, long supportAccountId);

        List<NameValue> GetBulkRegistrationDeviceIdsWithModelCompatibility(long bulkRegistrationId);

        long UpdateBulkRegistration(PrepBulkRegistration draft, long bulkRegistrationId, long userAccountId, long supportAccountId);

        List<BulkRegistrationModel> GetAllPendingBulkRegistrations();
        void UpdateBulkRegistrationDeviceStatus(long bulkRegistrationDeviceId, bool isSuccess, string registrationNotes, string registrationErrors);
        void UpdateBulkRegistrationLicenseStatus(long bulkRegistrationLicenseId, bool isSuccess, string activationNotes, string activationErrors);
        void UnclaimAllPurchases(long userAccountId, long supportAccountId);
        bool HasBetaAccess(long supportAccountId);
        List<PrepBulkRegistration.ActivatedLicense> GetActivatedLicensesForDeviceIds(List<long> deviceIds);
    }
}
