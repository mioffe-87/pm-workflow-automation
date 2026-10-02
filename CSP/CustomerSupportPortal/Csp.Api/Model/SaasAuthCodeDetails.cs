using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;
using Csp.Api.Model.Enums;
using LINQtoCSV;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class SaasAuthCodeDetails : BaseCspModel
    {
        [DataAccess(SaasFields.AuthorizationCode, DataType = DbType.String)]
        public string AuthCode { get; set; }

        [DataAccess(SaasFields.RegistrationDate, DataType = DbType.DateTime)]
        public DateTime? RegistrationDate { get; set; }

        [DataAccess(SaasFields.ExpirationDate, DataType = DbType.DateTime)]
        public DateTime? ExpirationDate { get; set; }

        [DataAccess(SaasFields.IsActive, DataType = DbType.Boolean)]
        public bool IsActive { get; set; }

        [DataAccess(SaasFields.PartNumber, DataType = DbType.String)]
        public string PartNumber { get; set; }

        [DataAccess(SaasFields.VmStubDeviceId, DataType = DbType.Int64)]
        public long? VmStubDeviceId { get; set; }

        [DataAccess(SaasFields.PanDbIdentification, DataType = DbType.String)]
        public string PanDbIdentification { get; set; }

        [DataAccess(SaasFields.DeviceId, DataType = DbType.Int64)]
        public long? DeviceId { get; set; }


    }

    public class AvailableSaaSActivations
    {
        private string _serialNumber;
        private string _quantity;
        private string _modelName;
        private string _expirationDate;

        [Required]
        public string AuthorizationCode { get; set; }

        public string SerialNumber
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _serialNumber;

                var serialNumbers =
                (
                    from l in BundleLicenses
                    select string.Format("<div class=''>{0}</div>", l.SerialNumber)
                ).ToArray();


                return string.Join("\n", serialNumbers);

            }
            set { _serialNumber = value; }
        }

        public string Quantity
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _quantity;

                var quantities =
                (
                    from l in BundleLicenses
                    select string.Format("<div class=''>{0}</div>", l.Quantity)
                ).ToArray();


                return string.Join("\n", quantities);

            }
            set { _quantity = value; }
        }

        public string ModelName
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _modelName;

                var modelNames =
                (
                    from l in BundleLicenses
                    select string.Format("<div class=''>{0}</div>", l.ModelName)
                ).ToArray();


                return string.Join("\n", modelNames);

            }
            set { _modelName = value; }
        }

        public string Description { get; set; }

        public string ExpirationDate
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _expirationDate;

                var expirationDates =
                (
                    from l in BundleLicenses
                    select string.Format("<div class=''>{0}</div>", l.ExpirationDate)
                ).ToArray();


                return string.Join("\n", expirationDates);

            }
            set { _expirationDate = value; }
        }

        public string AssoicatedDevices { get; set; }

        public long? LicenseID { get; set; }

        public string LicenseFeatureName { get; set; }

        public string Region { get; set; }

        public string PanDbIdentification { get; set; }

        public bool ShowPanoramaAssociation { get; set; }

        public long? BundleLicenseId { get; set; }

        public List<BundleLicenseItem> BundleLicenses { get; set; }

        public string Capacity { get; set; }

        public List<AvailableSaaSActivations> DependendMasterRecords { get; set; }

        public List<BundleLicenseItemBase> Children { get; set; }

        public int GroupId{ get; set; }

        public string ProductType { get; set; }

        public long? SupportAccountId { get; set; }

        public string SupportAccountName { get; set; }

        public string AscAccountName { get; set; }

        public EAllAssetType AssetType { get; set; }
    }

    public class BundleLicenseItemBase
    {
        public int Id { get; set; }

        public long? LicenseId { get; set; }

        public long BundleLicenseId { get; set; }

        public string AuthorizationCode { get; set; }

        public string ModelName { get; set; }

        public string Quantity { get; set; }

        public string Capacity { get; set; }

        public string Description { get; set; }

        public string ExpirationDate { get; set; }
    }

    public class BundleLicenseItem : BundleLicenseItemBase
    {
        public string SerialNumber { get; set; }

        public string Region { get; set; }

        public string ASC { get; set; }
    }

    public class AddSaasAuthCodeResult
    {

        public AddSaasAuthCodeResult()
        {

        }

        public AddSaasAuthCodeResult(string errorMessage, string activationLink = null)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }

        public string ActivationLink { get; set; }
    }



    public class AvailableSaasCalc
    {

        public string AuthorizationCode { get; set; }

        public string SerialNumber { get; set; }

        public string ModelName { get; set; }

        public string Quantity { get; set; }
        
        public string Description { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string AssoicatedDevices { get; set; }

        public string Region { get; set; }
    }

    public class PanoramaSerialNumber
    {
        public string SerialNumber { get; set; }
    }
    public class LoggingServiceDevice
    {
        public string SerialNumber { get; set; }
    }


    public class AuthCodeType
    {
        public string Type { get; set; }
        public bool IsPanoramaAssociation { get; set; }
        public string Region { get; set; }
    }

    public class AvailableSaasCsv
    {
        [CsvColumn(FieldIndex = 1, Name = "Authorization Code")]
        public string AuthorizationCode { get; set; }

        [CsvColumn(FieldIndex = 2, Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [CsvColumn(FieldIndex = 4, Name = "Model Name")]
        public string ModelName { get; set; }

        [CsvColumn(FieldIndex = 5, Name = "Quantity")]
        public string Quantity { get; set; }

        [CsvColumn(FieldIndex = 6, Name = "License Description")]
        public string Description { get; set; }

        [CsvColumn(FieldIndex=7,Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }        

        [CsvColumn(FieldIndex = 8, Name = "Associated Services/Devices")]
        public string AssoicatedDevices { get; set; }

        [CsvColumn(FieldIndex = 9, Name = "Region")]
        public string Region { get; set; }
    }

    public class OTPOutput
    {

        public string OneTimePassword { get; set; }

        public string ExpiryDate { get; set; }
    }

    public class CoreAPIResponse
    {
        public bool Issuccess { get; set; } 
        public string Error_message { get; set; }
        public string Activation_link { get; set; }
    }

    public class LicenseItem
    {
        public string ActivationLink { get; set; }
        public string AuthorizationCode { get; set; }
        public string ExpirationDate { get; set; }
        public string ProductGroup { get; set; }
        public string ProductSubGroup { get; set; }
        public string Sku { get; set; }
    }

    public class EasyOnboardingAPIResponse
    {
        public bool Issuccess { get; set; }
        public string Error_message { get; set; }
        public string Activation_link { get; set; }
        public List<LicenseItem> Licenses { get; set; }
    }

    public class CoreAPIRequest
    {
        public string Auth_code { get; set; }
        public string User_email { get; set; }
        public string Associated_SN { get; set; }
        public string Region { get; set; }
        public long Support_account_id { get; set; }
    }

    public class LicenseCoreAPIResponse
    {
        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }
        [JsonProperty("sku")]
        public string Sku { get; set; }
        [JsonProperty("app_id")]
        public string AppId { get; set; }
        [JsonProperty("application_name")]
        public string ApplicationName { get; set; }
        [JsonProperty("is_success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("is_activated")]
        public bool IsActivated { get; set; }
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }
        [JsonProperty("license_expiration")]
        public string LicenseExpiration { get; set; }
        [JsonProperty("expiry_ts")]
        public long ExpiryTs { get; set; }
        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }
        [JsonProperty("application_display_name")]
        public string ApplicationDisplayName { get; set; }
        [JsonProperty("license_type")]
        public string LicenseType { get; set; }
        [JsonProperty("data_size")]
        public long DataSize { get; set; }
        [JsonProperty("purchased_size")]
        public string PurchasedSize { get; set; }
        [JsonProperty("tenant_id")]
        public string TenantId { get; set; }
        [JsonProperty("cp")]
        public string CP { get; set; }
        [JsonProperty("is_paid")]
        public string IsPaid { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("error_number")]
        public string ErrorNumber { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }


    }

    public class LicenseCoreAPIRequest
    {
        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }
        [JsonProperty("user_email")]
        public string UserEmail { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("support_account_id")]
        public long SupportAccountId { get; set; }
        [JsonProperty("is_eula_accepted")]
        public bool IsEulaAccepted { get; set; }
        [JsonProperty("tenant_id")]
        public string TenantId { get; set; }
        [JsonProperty("cdl_tenant_id")]
        public string CDLTenantId { get; set; }
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }
        [JsonProperty("panorama_serial_number")]
        public string PanoramaSerialNumber { get; set; }
        //[JsonProperty("add_ons")]
        //public List<Addons> add_ons { get; set; }

    }

    public class SecaasOtp
    {
        public long Id { get; set; }
        public string SerialNumber { get; set; }
        public long SupportAccountId { get; set; }
        public string LookUp { get; set; }
        public string Otp { get; set; }
        public OtpType OtpType { get; set; }
        public string UpdatedBy { get; set; }
        public string ProductCategory { get; set; }

        public SecaasOtp()
        {
            OtpType = OtpType.LoggingService;
        }
    }

    public class CdlAssociationWithPanoramaEligibility
    {
        public bool IsCdl { get; set; }
        public bool IsCdlActivated { get; set; }
        public bool CdlHasPanoramaAssociation { get; set; }
        public bool IsCdlExpired { get; set; }
    }

}
