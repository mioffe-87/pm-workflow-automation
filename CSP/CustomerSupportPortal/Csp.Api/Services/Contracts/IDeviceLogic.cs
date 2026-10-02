using System.Collections.Generic;
using System.Text;
using Csp.Api.Model;
using System.Data;
using System;
using Csp.Api.Model.Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
    public interface IDeviceLogic
    {

        Device UpdateDevice(Device device, long restrictedByUserAccountId);
        Device GetDevice(long deviceId, long restrictedByUserAccountId);
        IEnumerable<ChassisComponent> GetChassisUnassignedComponentQuantities(string chassisSerialNumber);
        IEnumerable<ChassisComponent> GetUnassignedComponentQuantitiesBySalesOrder(string salesOrderId);
        IEnumerable<ChassisComponent> GetComponentsBySupportAccount(long supportAccountId, DataParameters parameters = null);
        Int32 GetUnassignedComponentQuantity(string partNumber, Int64 supportAccountId);
        string IsSalesOrderComponentsRegistered(string salesOrderNumber);
        void UpdateChassisComponents(List<ChassisComponent> chassisComponents);
        void UpdateComponent(ChassisComponent chassisComponent);
        void DeleteComponent(ChassisComponent component);
        void UpdateChassisLocation(ChassisComponent component);
        ChassisComponent GetChassisLocation(string chassisSerialNumber);
        List<DeviceListItem> GetSupportAccountDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool hideExpiredLicenses, bool isDebug = false, StringBuilder sb = null);
        byte[] GetSupportAccountDevicesCsv(long supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue, bool hideExpiredLicenses);
        List<DeviceListItem> GetSupportAccountSpareDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool isDebug = false, StringBuilder sb = null);
        byte[] GetSupportAccountSpareDevicesCsv(long supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue);
        List<DeviceListItem> GetGroupDevices(long groupId, long restrictedByUserAccountId, long supportAccountId, DataParameters parameters);
        byte[] GetGroupDevicesCsv(long groupId, long restrictedByUserAccountId, long supportAccountId, string searchField, string searchValue);
        List<DeviceListItem> GetGroupSpareDevices(long groupId, long restrictedByUserAccountId, long supportAccountId, DataParameters parameters);
        byte[] GetGroupSpareDevicesCsv(long groupId, long restrictedByUserAccountId, long supportAccountId, string searchField, string searchValue);
        List<DeviceListItem> GetAvailableSerialNumber(long groupId, long restrictedByUserAccountId, bool isProd, DataParameters parameters);
        string CheckIsSerialNumberPanorama(string serialNumber);
        string CheckIsSerialNumberValidandNotRegisterd(string serialNumber);
        string CheckIsSerialNumberEligible(string serialNumber);
        string CheckIsSerialNumberValidandAndRegisterd(string serialNumber);
        bool CheckIsSerialNumberValid(string serialNumber);
        Device RegisterDevice(Device device, string countryName, long lastUpdateByUserAccountId, out bool isSiteLicenseGranted, Group currentGroup = null, string ascCustBklnSupportAuthCode = null, bool isCalledFromSelfServiceRegistration = false);
        List<DeviceCategory> GetAllDeviceCategories(long supportAccountId, long userAccountId);
        List<DeviceCategoryListItem> GetDeviceCategories(long supportAccountId, long userAccountId, DataParameters dataParameters);
        DeviceCategory CreateDeviceCategory(DeviceCategory deviceCategory);
        DeviceCategory UpdateDeviceCategory(DeviceCategory deviceCategory);
        DeviceCategory GetDeviceCategory(long deviceCategoryId);
        void DeleteDeviceCategory(DeviceCategory deviceCategory);
        bool IsDeviceCategoryExists(string deviceCategoryName, long supportAccountId, long deviceCategoryId);
        List<SoftwareKey> GetWF500SoftwareKeys(string serialNumber);
        List<WildfireDeviceItem> GetWildfireDevices(long userAccountId, long supportAccountId, DataParameters parameters);
        long GetSupportAccountIdByDeviceBySerialNumber(string serialNumber);
        long GetDeviceIdBySerialNumber(string serialNumber);
        bool PushToDeviceMaster(string serialNumber, int? userAccountId, string partId, System.DateTime? shipDate = null);
        bool CreateAwsInstance(string serialNumber, string cpuId, string uuId, string user, out long deviceId,
                               out string awsAccountId, out string errorMessage);

        bool RegisterAWSDevices(long supportAccountId, string awsAccountId, long deviceId, long userAccountId);
        bool IsInstanceAlreadyRegistered(string serialNumber);

        bool UpdateSapIdSfdcIdForAwsSupportAccount(long supportAccountId, long deviceId, long userAccountId);

        bool CreateAzureInstance(string serialNumber, string cpuId, string uuId, string user, out long deviceId,
            out string errorMessage);

        List<DeviceCategoryListItem> GetDeviceCategoriesWithDeviceCount(long supportAccountId, long userAccountId, long groupId, string deviceType);
        bool ValidateEndUserSerialNumber(string serialNumber, out string eMessage);

        bool ValidateEndUserCustomerAccountId(string customerAccountId, out string eMessage);

        bool ValidateCsspEndUserInputCombination(string serialNumber, string customerAccountId, out string eMessage);
        IEnumerable<PanOs> GetPanOsVersions();
        PendingDeviceTransferDetails GetPendingDeviceTransferDetails(long deviceId);
        IEnumerable<HyperVisor> GetHyperVisorVersions(string filter = null);
        string GetLegacyLightCyberDeviceName(string serialNumber);
        bool Has4HRSupport(long deviceId);
        string GetAuthcodeInfo(long licenseId);

        bool ShowBulkDeviceTransfer(long supportAccountId, long userAccountId);

        Address GetDeviceAddressBySerialNumber(string serialNumber);

        bool GetUserPreferences(long userAccountId, long supportAccountId);
        void UpdateHideExpiredLicensesFlag(long userAccountId, long supportAccountId, bool hideExpiredLicenses);

        bool IsProcessingCard(string serialNumber);
        bool IsK2Device(string serialNumber, out string mode);

        string IsValidSerial(string serialNumber, long supportAccountId);
        bool IsChassisSerialNumber(string serialNumber);

        string GetCountryCodeById(long countryId);
        string IsValidSerialNumber(string serialNumber, long supportAccountId, out bool isRegistered);
        bool IsValidDeviceForOtp(string serialNumber, long supportAccountId);

        IEnumerable<ActivationLinkModel> GetTrialLicenseActivationActiveLinksBySupportAccountId(long supportAccountId, string type, string serialNumber);

        bool IsNgfwDlpDevice(long deviceId);

        bool IsNgfwDlpDevice(string serialNumber);

        string GetCountryNameByCode(string countryCode);

        bool IsSpareDevice(string serialNumber);
    }
        
}
