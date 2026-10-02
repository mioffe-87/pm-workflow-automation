using Csp.Api.Model;
using Csp.Api.Model.Flex;
using System.Collections.Generic;

namespace Csp.Api.Services.Contracts
{
    public interface IDeviceDataAccess
    {
        void LogWipeOutNFRWildfire(string data, string apiCalled, double responseTimeInMS, string status, string exception);

        void LogWildfire(string data, string apiCalled, double responseTimeInMs, string status, string exception,
            short isExceptionReprocessed);

        bool IsDeviceCodeCirroSecure(string deviceCode, out long devId);

        bool RegisterCirroSecureDevice(long deviceId, long supportAccountId, long userAccountId);
        void CheckGovernmentCloud(List<DeviceListItem> deviceItems);

        void ProcessBulkDeviceTransfer(BulkDeviceTransferRequest request);
        List<BulkDeviceTransferRequest> GetBulkTransferRequests();
        bool BackupBulkTransferDevices(long transferRequestId);

        List<DeviceTransferItem> GetEligibleDevicesForTransfer(long supportAccountId, long userAccountId);
        List<NameValueOnly> GetDestinationAccounts(long userAccountId, long loggedInAccountId);
        bool CreateBulkTransferRequest(BulkDeviceTransferModel requestData);

        Device GetDeviceDetailsBySerialNumber(string serialNumber);

        string GetUserPreferences(long userAccountId, long supportAccountId);

        void UpdateHideExpiredLicensesFlag(long userAccountId, long supportAccountId, string preferences);

        bool IsProcessingCard(string serialNumber);

        bool IsK2Device(string serialNumber, out string mode);
        List<NameValueOnly> GetDeviceTypes();
        List<NameValueOnly> GetPanOs();
        List<NameValueOnly> GetHyperVisors();
        bool IsChassisSerialNumber(string serialNumber);
        long[] GetDeviceSalesOrderSupportAccountId(string serialNumber);
        bool HasBklnSupportInSalesOrder(string serialNumber, long supportCenterSupportAccountId);
        string GetCountryCodeById(long countryId);
        List<long> GetDeviceIdsByBulkDeviceTransferId(long transferRequestId);
        bool CheckForWFSubscription(long supportAccountId);
        bool IsValidDeviceForOtp(string serialNumber, long supportAccountId);
        IEnumerable<ActivationLinkModel> GetTrialLicenseActivationActiveLinksBySupportAccountId(long supportAccountId, string type, string productFamily, int duration, out string sfAccount);
        long GetDlpTenantIdBySupportAccountId(long supportAccountId);

        bool IsNgfwDlpDevice(long deviceId);

        bool IsNgfwDlpDevice(string serialNumber);

        void LogGrantDeviceInfo(long SupportAccountId, string SerialNumber, string MethodName, string ErrorMessage, string LogType, long? UserAccountId);

        string GetCountryNameByCode(string countryCode);

        void EaEsaDeviceInsert(long supportAccountId, string partId, string serialNumber, string createdBy);
        string GetModelNameBySerialNumber(string serialNumber);
    }
}
