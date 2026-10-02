using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model
{

    public class AssetRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "category_id")]
        public int? AssetCategory { get; set; }

        [JsonProperty(PropertyName = "category_ids")]
        public int[] CategoryId { get; set; }

        [JsonProperty(PropertyName = "family_id")]
        public int? AssetFamily { get; set; }

        [JsonProperty(PropertyName = "type_id")]
        public int[] AssetTypes { get; set; }

        [JsonProperty(PropertyName = "model_id")]
        public int[] Models { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string[] AssetNames { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public string[] SerialNumbers { get; set; }

        [JsonProperty(PropertyName = "license_type_id")]
        public string[] Licenses { get; set; }

        [JsonProperty(PropertyName = "expiration_date_id")]
        public int[] ExpirationDateId { get; set; }

        [JsonProperty(PropertyName = "order_by")]
        public string OrderBy { get; set; }

        [JsonProperty(PropertyName = "search_text")]
        public string SearchText { get; set; }

        [JsonProperty(PropertyName = "sort_by")]
        public string[] SortBy { get; set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "size")]
        public int? PageSize { get; set; }

        [JsonProperty(PropertyName = "bpa_run")]
        public int[] BpaRun { get; set; }

        [JsonProperty(PropertyName = "authorization_code")]
        public string[] AuthorizationCode { get; set; }

        [JsonProperty(PropertyName = "tenant_id")]
        public string[] TenantId { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string[] Sku { get; set; }

        [JsonProperty(PropertyName = "asset_decommission_id")]
        public int[] AssetDecommissionId { get; set; }

        [JsonProperty(PropertyName = "cve_status")]
        public string[] CveStatus { get; set; }

        [JsonProperty(PropertyName = "entitlement_license_type")]
        public string[] EntitlementLicenseType { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string[] SerialNumber { get; set; }

        [JsonProperty(PropertyName = "asset_tag")]
        public string[] AssetTags { get; set; }

        [JsonProperty(PropertyName = "eol_status")]
        public string[] EolStatus { get; set; }

        [JsonProperty(PropertyName = "inactive")]
        public string[] InActive { get; set; }
        
        [JsonProperty(PropertyName = "archived_entitlements")]
        public bool ArchivedEntitlements { get; set; }
    }

    public class AssetDetailsRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "asset_name")]
        public string AssetName { get; set; }

        [JsonProperty(PropertyName = "os")]
        public string OS { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "asc_id")]
        public long? AscSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "type_id")]
        public long type_id { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public long DeviceId { get; set; }
    }

    public class FlexDetailsRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "allocation_id")]
        public string AllocationId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public long SerialNumber { get; set; }
    }

    public class TrialLicense
    {
        [JsonProperty(PropertyName = "grace_period_days")]
        public string GracePeriodDays { get; set; }

        [JsonProperty(PropertyName = "license_type")]
        public string LicenseType { get; set; }
    }

    public class BaseRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "user_email")]
        public string UserEmail { get; set; }

        [JsonProperty(PropertyName = "eula_id")]
        public long EulaId { get; set; }

        [JsonProperty(PropertyName = "tenant_id")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "device_type")]
        public int DeviceType { get; set; }

        [JsonProperty(PropertyName = "marketplace_account_type")]
        public string MarketPlaceAccountType { get; set; }

        [JsonProperty(PropertyName = "aws_account_id")]
        public string AwsAccountId { get; set; }

        [JsonProperty(PropertyName = "agreement_id")]
        public string AgreementID { get; set; }
    }

    public class LicenseRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UUId { get; set; }

        [JsonProperty(PropertyName = "cpuid")]
        public string CPUId { get; set; }
    }

    public class TrialLicenseRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "licenses")]
        public List<TrialLicense> TrialLicenses { get; set; }
    }

    public class TrialLicenseResponseObject
    {
        [JsonProperty(PropertyName = "licenses")]
        public List<TrialLicenseResponse> TrialLicenses { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public TrialLicenseMessage Messages { get; set; }
    }

    public class TrialLicenseMessage
    {
        [JsonProperty(PropertyName = "success")]
        public List<string> Success { get; set; }

        [JsonProperty(PropertyName = "error")]
        public List<string> Error { get; set; }

        [JsonProperty(PropertyName = "pending")]
        public List<string> Pending { get; set; }
    }

    public class TrialLicenseResponse
    {
        [JsonProperty(PropertyName = "id")]
        public long LicenseId { get; set; }

        [JsonProperty(PropertyName = "authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty(PropertyName = "expiration_date")]
        public string ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "asc_support_account_id")]
        public long? AscSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "license_type")]
        public string LicenseType { get; set; }

        [JsonProperty(PropertyName = "feature_name")]
        public string FeatureName { get; set; }

        [JsonProperty(PropertyName = "license_key")]
        public string LicenseKey { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "is_trial_license")]
        public bool IsTrialLicense { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "is_perpetual")]
        public bool IsPerpetual { get; set; }
    }

    public class DeviceInformation
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "asset_tag_id")]
        public string[] TagId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public int UserAccountId { get; set; }

        [JsonProperty(PropertyName = "os")]
        public string[] OS { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "location")]
        public LocationInformation Location { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "remarks")]
        public string Remarks { get; set; }
    }

    public class LocationInformation
    {
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "countryId")]
        public long CountryId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "zipcode")]
        public string ZipCode { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }
    }

    public class DeviceTagRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "created_by_email")]
        public string CreatedByEmail { get; set; }
    }

    public class OfflineLicenseRequest : BaseRequest
    {
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }
        [JsonProperty(PropertyName = "cpuid")]
        public string Cpuid { get; set; }
        [JsonProperty(PropertyName = "os_version")]
        public string OsVersion { get; set; }
        [JsonProperty(PropertyName = "vm_type")]
        public string VmType { get; set; }
    }

    public class TransferAssetLicenseRequest
    {
        [JsonProperty(PropertyName = "is_spare")]
        public bool IsSpare { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "source_serial_number")]
        public string SourceSerialNumber { get; set; }

        [JsonProperty(PropertyName = "destination_serial_number")]
        public string DestinationSerialNumber { get; set; }
    }

    public class BaseComponentRequest
    {
        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }
    }

    public class AssociateComponentRequest : BaseComponentRequest
    {
        [JsonProperty(PropertyName = "chassis_serial_number")]
        public string ChassisSerialNumber { get; set; }

        [JsonProperty(PropertyName = "component_serial_numbers")]
        public string[] ComponentSerialNumbers { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "country_id")]
        public long CountryId { get; set; }

        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "last_updated_by_user_account_id")]
        public long LastUpdatedBy { get; set; }

        [JsonProperty(PropertyName = "user_email")]
        public string UserEmail { get; set; }

        [JsonProperty(PropertyName = "eula_id")]
        public int EulaId { get; set; }

    }

    public class ComponentRequest : BaseComponentRequest
    {
        [JsonProperty(PropertyName = "chassis_serial_number")]
        public string ChassisSerialNumber { get; set; }

        [JsonProperty(PropertyName = "sales_order_number")]
        public string SalesOrderNumber { get; set; }
    }

    public class ActivateSiteLicenseRequest :BaseRequest
    {
        [JsonProperty(PropertyName = "authorization_code")]
        public string AuthorizationCode { get; set; }
    }

    public class AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }
    }

    public class MagicLinkRequest : BaseComponentRequest
    {
        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "feature_name")]
        public string FeatureName { get; set; }

        [JsonProperty(PropertyName = "authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty(PropertyName = "bundle_authorization_code")]
        public string BundleAuthorizationCode { get; set; }
        // psudo commit


    }

    public class AssetNameUpdateRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class PanOsVersionUpdateRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "pan_os_version")]
        public string PanOsVersion { get; set; }
    }

    public class RemarksUpdateRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "remarks")]
        public string Remarks { get; set; }
    }

    public class LocationUpdateRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "location")]
        public LocationInformation Location { get; set; }
    }

    public class AssetTagsUpdateRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "asset_tag_id")]
        public string[] AssetTagId { get; set; }
    }

    public class SelfRegistraionOtpRequest
    {
        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }
    }

    public class SelfRegistraionOtpValidationRequest
    {
        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "otp")]
        public string otp { get; set; }
    }

    public class SelfregistrationRequest
    {
        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string tenantId { get; set; }

        [JsonProperty(PropertyName = "captcha")]
        public string captcha { get; set; }

        [JsonProperty(PropertyName = "createNewSupportAccount")]
        public bool createNewSupportAccount { get; set; }

        [JsonProperty(PropertyName = "userDetails")]
        public UserDetail UserCreationRequestDto { get; set; }

        [JsonProperty(PropertyName = "ipAddress")]
        public string ipAddress { get; set; }

        [JsonProperty(PropertyName = "existingCspId")]
        public string existingCspId { get; set; }
    }

    public class SKUChangetoSpareRequest : AssetUpdateBaseRequest
    {
        [JsonProperty(PropertyName = "prod_sku")]
        public string ProdSku { get; set; }
    }

    public class EmailList
    {
        [JsonProperty(PropertyName = "emailAddresses")]
        public List<string> EmailAddresses { get; set; }
    }

    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static RegistrationResult Success(string message = "Registration request was submitted successfully")
        {
            return new RegistrationResult { IsSuccess = true, Message = message };
        }

        public static RegistrationResult Error(string message)
        {
            return new RegistrationResult { IsSuccess = false, Message = message };
        }
    }
}
