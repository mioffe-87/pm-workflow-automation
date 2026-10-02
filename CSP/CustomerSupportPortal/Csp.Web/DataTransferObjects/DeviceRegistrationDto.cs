using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Csp.Api.Model;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Csp.Api.Model.ElaEsa;

namespace Csp.Web.DataTransferObjects
{
        public enum EsaLicenseType
        {
            NOESA = 1,
            ESA = 2,
            FOURHRESA = 3
        }

        public class DeviceRegistrationDto : DefaultDto
        {

            public DeviceRegistrationDto()
            {
                AscRegisterToCustomer = true;
            }

            public Device Device { get; set; }

            public string CountryName { get; set; }

            public List<DeviceCategory> DeviceCategories { get; set; }

            public List<AwsRegion> AwsRegions { get; set; }

            public List<AwsProduct> AwsProducts { get; set; }

            public List<CloudRegion> CloudRegions { get; set; }
            public List<CloudProductType> CloudProducts { get; set; }

            [DisplayName("Backline Support AuthCode"), RequiredIf("IsAscCustomerRegistration == true && AscRegisterToCustomer == true", ErrorMessage = "Backline Support AuthCode is required"),
            Remote("IsValidBklnAuthCode", "Validation", AdditionalFields = "Device.Customer.Id,Device.SerialNumber")]
            public string AscCustBklnSupportAuthCode { get; set; }

            public bool IsAscCustomerRegistration { get; set; }

            public bool AscRegisterToCustomer { get; set; }

            public string CustomerCode { get; set; }

            public EulaModel Eula { get; set; }

            public List<DeviceRegistrationEsaOptions> DeviceEsaOptions { get; set; }

            public int DeviceType { get; set; }

            public long GroupId { get; set; }

        }


        #region New Device Registration Flow

        public class DeviceRegistrationValidationRequest
        {
            [Remote("IsValidSerial", "Validation", AdditionalFields = "SupportAccountId")]
            public string SerialNumber { get; set; }

            public long UserAccountId { get; set; }
            public long SupportAccountId { get; set; }
            public string DeviceType { get; set; }
        }

        public class DeviceRegistrationValidationResponse
        {
            [JsonProperty("hideLocationInfo")]
            public bool HideLocationInfo { get; set; }

            [JsonProperty("showEsa")]
            public bool ShowEsa { get; set; }

            [JsonProperty("show4HREsa")]
            public bool Show4HREsa { get; set; }

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }

            [JsonProperty("showComponents")]
            public bool ShowComponents { get; set; }

            [JsonProperty("componentDetails")]
            public List<ChassisComponent> ComponentDetails { get; set; }

            [JsonProperty("ascCustomOptions")]
            public AscCustResult AscCustomOptions { get; set; }

            [JsonProperty("isRegistered")]
            public bool IsRegistered { get; set; }

            [JsonProperty("elaEsaCapInfo")]
            public IEnumerable<ElaEsaCapInfoWithNewDevice> ElaEsaCapInfo { get; set; }
        }

        public class AscCustResult
        {
            [JsonProperty("showAscCustOptions")]
            public bool ShowAscCustOptions { get; set; }

            [JsonProperty("oneToManyMappingException")]
            public bool OneToManyMappingException { get; set; }

            [JsonProperty("showEvalAccountWarning")]
            public bool ShowEvalAccountWarning { get; set; }

            [JsonProperty("serialNumberIsPanorama")]
            public bool SerialNumberIsPanorama { get; set; }

            [JsonProperty("customerSupportAccountName")]
            public string CustomerSupportAccountName { get; set; }

            [JsonProperty("customerSupportAccountId")]
            public long? CustomerSupportAccountId { get; set; }

            [JsonProperty("legacyLightCyberDeviceName")]
            public string LegacyLightCyberDeviceName { get; set; }
        }

        public class DeviceRegistrationRequest
        {
            [JsonProperty("deviceType")]
            public int DeviceType { get; set; }

            [JsonProperty("panOsVersion")]
            public string PanOsVersion { get; set; }

            [JsonProperty("generatePanoramaLicense")]
            public bool GeneratePanoramaLicense { get; set; }

            [JsonProperty("isPanorama")]
            public bool IsPanorama { get; set; }

            [JsonProperty("lineCards")]
            public List<ChassisComponent> LineCards { get; set; }

            [JsonProperty("serialNumber")]
            public string SerialNumber { get; set; }

            [JsonProperty("customerSupportAccountId")]
            [Required(ErrorMessage = "Support Account Id is required")]
            public long CustomerSupportAccountId { get; set; }

            [JsonProperty("customerSupportAccountName")]
            public string CustomerSupportAccountName { get; set; }

            [JsonProperty("userAccountId")]
            [Required(ErrorMessage = "User Account Id is required")]
            public long UserAccountId { get; set; }

            [JsonProperty("userEmail")]
            [Required(ErrorMessage = "User Email is required")]
            public string UserEmail { get; set; }

            [JsonProperty("deviceName")]
            public string DeviceName { get; set; }

            [JsonProperty("deviceNameEncoded")]
            public string DeviceNameEncoded { get; set; }

            [JsonProperty("comments")]
            public string Comments { get; set; }

            [JsonProperty("commentsEncoded")]
            public string CommentsEncoded { get; set; }

            [JsonProperty("address1")]
            public string Address1 { get; set; }

            [JsonProperty("address2")]
            public string Address2 { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("postalCode")]
            public string PostalCode { get; set; }

            [JsonProperty("countryId")]
            public long? CountryId { get; set; }

            [JsonProperty("countryName")]
            public string CountryName { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("regionText")]
            public string RegionText { get; set; }

            [JsonProperty("isVirtualMachine")]
            public bool IsVirtualMachine { get; set; }

            [JsonProperty("deviceCategoryId")]
            public long? DeviceCategoryId { get; set; }

            [JsonProperty("deviceCategoryName")]
            public string DeviceCategoryName { get; set; }

            [JsonProperty("applyEsaLicenseToDevice")]
            public EsaLicenseType ApplyEsaLicenseToDevice { get; set; }

            [JsonProperty("ascBacklineAuthCode")]
            public string AscBacklineAuthCode { get; set; }

            [JsonProperty("uuid")]
            public string Uuid { get; set; }

            [JsonProperty("cpuid")]
            public string Cpuid { get; set; }

            [JsonProperty("hypervisor")]
            public string Hypervisor { get; set; }

        [JsonProperty("csspReferenceId")]
        public string CsspReferenceId { get; set; }

            [JsonProperty("eulaId")]
            public int EulaId { get; set; }

            [JsonProperty("showEsa")]
            public bool ShowEsa { get; set; }

            [JsonProperty("show4HREsa")]
            public bool Show4HREsa { get; set; }

            [JsonProperty("tenantId")]
            public string TenantId { get; set; }

            [JsonProperty(PropertyName = "awsAccountId")]
            public string AwsAccountId { get; set; }

            [JsonProperty(PropertyName = "agreementId")]
            public string AgreementID { get; set; }
        }

        public class DeviceRegistrationRequestDetail
        {

            [JsonProperty("serialNumber")]
            public string SerialNumber { get; set; }

            [JsonProperty("customerSupportAccountId")]
            [Required(ErrorMessage = "Support Account Id is required")]
            public long CustomerSupportAccountId { get; set; }

            [JsonProperty("customerSupportAccountName")]
            public string CustomerSupportAccountName { get; set; }

            [JsonProperty("userAccountId")]
            [Required(ErrorMessage = "User Account Id is required")]
            public long UserAccountId { get; set; }

            [JsonProperty("userEmail")]
            [Required(ErrorMessage = "User Email is required")]
            public string UserEmail { get; set; }

            [JsonProperty("address1")]
            public string Address1 { get; set; }

            [JsonProperty("address2")]
            public string Address2 { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("postalCode")]
            public string PostalCode { get; set; }

            [JsonProperty("countryId")]
            public long? CountryId { get; set; }

            [JsonProperty("countryName")]
            public string CountryName { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }
        }

        #endregion
}