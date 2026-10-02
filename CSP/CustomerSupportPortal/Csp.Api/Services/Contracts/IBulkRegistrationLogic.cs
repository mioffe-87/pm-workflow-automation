using System;
using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IBulkRegistrationLogic
    {
        bool IsValidCustomerAndDeviceCode(long supportAccountId, string customerCode, string deviceCode);
        List<ClaimedAssetListItem> GetAvailablePurchases(long supportAccountId, string customerCode, string deviceCode);
        List<ClaimedAssetListItem> GetClaimedPurchases(long supportAccountId);
        List<PrepBulkRegistration.SelectableDevice> GetClaimedOrRegisteredDevicesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, SelectDevicesFilterTypes selectDevicesFilter);
        List<PrepBulkRegistration.Availablelicense> GetAvailableLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, string modelCompatibility);
        //List<PrepBulkRegistration.DeviceAndLicenses> GetAppliedLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, long[] deviceIds);
        PrepBulkRegistration GetPrepBulkRegistrationDetails(long supportAccountId, long bulkRegistrationId);
        List<BulkRegistrationHistoryListItem> GetBulkRegistrationHistory(long supportAccountId);
        long? GetCurrentBulkRegistrationId(long supportAccountId, long userAccountId);
        void ClaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases);
        void UnclaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases);
        List<BulkRegistrationDeviceModel> GetBulkRegistrationDevicesWithLicenses(long bulkRegistrationId, long supportAccountId);
        void UpdateBulkRegistrationDevicesAndLicenses(long bulkRegistrationId, List<BulkRegistrationDeviceModel> devices);
        void UpdateBulkRegistrationState(long bulkRegistrationId, BulkRegistrationStates state);
        void DiscardDraft(long supportAccountId, long userAccountId, long bulkRegistrationId);
        long CreateUpdateBulkRegistration(long? currentBulkRegistrationId, PrepBulkRegistration draft, long userAccountId, long supportAccountId);
        List<NameValue> GetBulkRegistrationDeviceIdsWithModelCompatibility(long bulkRegistrationId);

        List<BulkRegistrationModel> GetAllPendingBulkRegistrations();
        void UpdateBulkRegistrationDeviceStatus(long bulkRegistrationDeviceId, bool isSuccess, string registrationNotes, string registrationErrors);
        void UpdateBulkRegistrationLicenseStatus(long bulkRegistrationLicenseId, bool isSuccess, string activationNotes, string activationErrors);
        void UnclaimAllPurchases(long userAccountId, long supportAccountId);
        bool HasBetaAccess(long supportAccountId);
    }

    public enum ClaimedPurchaseStates
    {
        Claimed,
        Unclaimed,
        Registered,
    }

    public enum BulkRegistrationStates
    {
        Draft,
        Discarded,
        Pending,
        Success,
        Failure
    }

    public enum ClaimedPurchaseAssetType
    {
        License,
        Device
    }

    public class PrepBulkRegistrationResponse
    {
        public bool IsSuccess { get; set; }
        public long? BulkRegistrationId { get; set; }
    }

    public enum SelectDevicesFilterTypes
    {
        ClaimedPurchases,
        RegisteredDevices,
        Both
    }

    public class PrepBulkRegistration
    {
        public SelectDevicesFilterTypes SelectDevicesFilter { get; set; }
        public IEnumerable<object> SelectedDevices { get; set; }

        public IEnumerable<DeviceCategory> DeviceCategories { get; set; }
        public string DefaultDeviceName { get; set; }
        public long? DefaultDeviceCategoryId { get; set; }
        public string DefaultDeviceAddress1 { get; set; }
        public string DefaultDeviceAddress2 { get; set; }
        public string DefaultDeviceCity { get; set; }
        public long? DefaultDeviceCountryId { get; set; }
        public long? DefaultDeviceRegionId { get; set; }
        public string DefaultDeviceOtherRegion { get; set; }
        public string DefaultDevicePostalCode { get; set; }
        public string DefaultDeviceComments { get; set; }
        public bool IsOverrideDefaults { get; set; }

        //public IEnumerable<object> AvailableLicenses { get; set; }
        public IEnumerable<object> BulkRegistrations { get; set; }
        public List<DeviceAndLicenses> DevsAndLics { get; set; }

        public class SelectableDevice
        {
            public string ModelCompatibility { get; set; }
            public string ProductFamily { get; set; }
            public long DeviceId { get; set; }
            public string SerialNumber { get; set; }
            public string PartNumber { get; set; }
            public DateTime ShipDate { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public bool IsSelected { get; set; }

            public List<ActivatedLicense> ActivatedLicenses { get; set; }
        }

        public class Availablelicense
        {
            public string SalesOrderNumber { get; set; }
            public DateTime ShipDate { get; set; }
            public string PartNumber { get; set; }
            public string PartDescription { get; set; }
            public int Quantity { get; set; }
            public string Term { get; set; }
            public List<long> LicenseIds { get; set; }
            public List<BundleModel> Bundles { get; set; }
        }

        public class ActivatedLicense
        {
            public long DeviceId { get; set; }
            public long LicenseId { get; set; }
            public long PartMasterId { get; set; }
            public string PartNumber { get; set; }
            public string AuthorizationCode { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public List<BundleModel> Bundles { get; set; }
        }

        public class DeviceAndLicenses
        {
            public long? BulkRegistrationDeviceId { get; set; }
            public long DeviceId { get; set; }
            public string SerialNumber { get; set; }
            public string AssetPartNumber { get; set; }

            public string AssetDisplayName
            {
                get { return SerialNumber + " " + AssetPartNumber; }
            }

            public long? StandardSupportLicenseId { get; set; }
            public string StandardSupportAuthCode { get; set; }
            public DateTime? StandardSupportExpiration { get; set; }

            public string StandardSupportDisplayName { get; set; }
            /*
            public string StandardSupportDisplayName
            {
                get
                {
                    return StandardSupportLicenseId.HasValue && StandardSupportExpiration.HasValue
                        ? StandardSupportAuthCode + " " + StandardSupportExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? ThreatProtectionLicenseId { get; set; }
            public string ThreatProtectionAuthCode { get; set; }
            public DateTime? ThreatProtectionExpiration { get; set; }

            public string ThreatProtectionDisplayName { get; set; }
            /*
            public string ThreatProtectionDisplayName
            {
                get
                {
                    return ThreatProtectionLicenseId.HasValue && ThreatProtectionExpiration.HasValue
                        ? ThreatProtectionAuthCode + " " + ThreatProtectionExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? UrlLicenseId { get; set; }
            public string UrlAuthCode { get; set; }
            public DateTime? UrlExpiration { get; set; }

            public string UrlDisplayName { get; set; }
            /*
            public string UrlDisplayName
            {
                get
                {
                    return UrlLicenseId.HasValue && UrlExpiration.HasValue
                        ? UrlAuthCode + " " + UrlExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? GpPortalLicenseId { get; set; }
            public string GpPortalAuthCode { get; set; }
            public DateTime? GpPortalExpiration { get; set; }

            public string GpPortalDisplayName { get; set; }
            /*
            public string GpPortalDisplayName
            {
                get
                {
                    return GpPortalLicenseId.HasValue && GpPortalExpiration.HasValue
                        ? GpPortalAuthCode + " " + GpPortalExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? GpGatewayLicenseId { get; set; }
            public string GpGatewayAuthCode { get; set; }
            public DateTime? GpGatewayExpiration { get; set; }

            public string GpGatewayDisplayName { get; set; }
            /*
            public string GpGatewayDisplayName
            {
                get
                {
                    return GpGatewayLicenseId.HasValue && GpGatewayExpiration.HasValue
                        ? GpGatewayAuthCode + " " + GpGatewayExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? WildfireLicenseId { get; set; }
            public string WildfireAuthCode { get; set; }
            public DateTime? WildfireExpiration { get; set; }

            public string WildfireDisplayName { get; set; }
            /*
            public string WildfireDisplayName
            {
                get
                {
                    return WildfireLicenseId.HasValue && WildfireExpiration.HasValue
                        ? WildfireAuthCode + " " + WildfireExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? BundleLicenseId { get; set; }
            public string BundleAuthCode { get; set; }
            public DateTime? BundleExpiration { get; set; }

            public string BundleDisplayName { get; set; }
            /*
            public string BundleDisplayName
            {
                get
                {
                    return BundleLicenseId.HasValue && BundleExpiration.HasValue
                        ? BundleAuthCode + " " + BundleExpiration.Value.ToShortDateString()
                        : string.Empty;
                }
            }*/

            public long? DnsLicenseId { get; set; }
            public string DnsAuthCode { get; set; }
            public DateTime? DnsExpiration { get; set; }

            public string DnsDisplayName { get; set; }

            public long? SdwanLicenseId { get; set; }
            public string SdwanAuthCode { get; set; }
            public DateTime? SdwanExpiration { get; set; }

            public string SdwanDisplayName { get; set; }

            public long? PaaLicenseId { get; set; }
            public string PaaAuthCode { get; set; }
            public DateTime? PaaExpiration { get; set; }

            public string PaaDisplayName { get; set; }

            public long? PanoramaManagementLicenseId { get; set; }
            public string PanoramaManagementAuthCode { get; set; }
            public DateTime? PanoramaManagementExpiration { get; set; }

            public string PanoramaManagementDisplayName { get; set; }

            public List<ActivatedLicense> ActivatedLicenses { get; set; }
        }

        public class BundleModel
        {
            public long PartMasterId { get; set; }
            public string PartNumber { get; set; }
            public long BundlePartMasterId { get; set; }
        }

	    public class Availablelicenses
	    {
		    public long LicenseId { get; set; }
		    public long PartMasterId { get; set; }
		    public string PartNumber { get; set; }
		    public string Description { get; set; }
		    public string SalesOrderNumber { get; set; }
		    public DateTime ShipDate { get; set; }
		    public string Term { get; set; }
	    }
    }
}
