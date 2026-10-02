using System;
using System.Collections.Generic;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Model
{
    public class UserProfileModel
    {
        public long? UserAccountId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Title { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public long? Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool? optContentEmails { get; set; }
        public bool? optSecurityEmails { get; set; }
        public bool? optSoftwareEmails { get; set; }
        public string Errormessage { get; set; }

        public long? SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public bool HasMultipleAccounts { get; set; }
        public string[] Roles { get; set; }

        public bool IsMfaRequired { get; set; }
        public bool IsFedramp { get; set; }
        public bool IsMfaEnabled { get; set; }
        public MultiFactorAuthType MultiFactorAuthType { get; set; }
        public bool IsThirdPartyIdpConfigured { get; set; }
        public string Comments { get; set; }

        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class countries
    {
        public long countryId { get; set; }
        public string countrycode { get; set; }
        public string countryname { get; set; }
        public int  ZipcodeLength { get; set; }
        public string ZipcodeValidation { get; set; }
        public bool IsZipcodeRequired { get; set; }
       
    }

    public class regions
    {
        public long countryId { get; set; }
        public long regionId { get; set; }
        public string regionName { get; set; }
        public string regionCode { get; set; }
    }

	public class EmailSubscriptions {
		public string email { get; set; }
		public bool? optContent { get; set; }
		public bool? optSecurity { get; set; }
		public bool? optPanOS { get; set; }
		public bool? optApertureContent { get; set; }
		public bool? optApertureSoftware { get; set; }
		public bool? optTrapsContent { get; set; }
		public bool? unsubscribeAll { get; set; }
		public bool? optTrapsSoftware { get; set; }
		public bool? optCompliance { get; set; }
        public bool? optPrismaContent { get; set; }
        public bool? optPrismaAccess { get; set; }
        public bool? optPrismaSaas { get; set; }
        public bool? optVmSeriesUpdate { get; set; }
        public bool? optStrata { get; set; }
        public bool? optPrismaBrowser { get; set; }
        public bool? optNetSec { get; set; }
        public long? lastUpdateByUserAccountId { get; set; }
        public long? emailSubscriptionId { get; set; }
        public bool? optPrismaAccessPlus { get; set; }
    }

    public class NewUserDetails
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string CreatedByUserEmail { get; set; }

        public string CreatedByUserName { get; set; }

        public DateTime CreatedOn { get; set; }

        public string RolesAssigned { get; set; }

        public string GroupName { get; set; }
    }

    public class UserConfig
    {
        public bool IsFirstLogin { get; set; }
        public bool IsDeviceCodeRegistered { get; set; }
    }

    public class UserAcknowledge
    {
        [JsonProperty(PropertyName = "useremail")]
        public string UserEmail { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long? SupportAccountId { get; set; }
       
    }
    #region rewrite - start

    public class SupportAccountUser
    {
        [JsonProperty(PropertyName = "membershipId")]
        public long MembershipId { get; set; }

        [JsonProperty(PropertyName = "registeredAppId")]
        public int RegisteredAppId { get; set; }

        [JsonProperty(PropertyName = "createdByUserAccountId")]
        public long CreatedByUserAccountId { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedByUserAccountId")]
        public long LastUpdatedByUserAccountId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd H:mm:ss")]
        [JsonProperty(PropertyName = "activationDate")]
        public DateTime? ActivationDate { get; set; }

        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd H:mm:ss")]
        [JsonProperty(PropertyName = "expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "membershipRoles")]
        public long[] MembershipRoles { get; set; }

        [JsonProperty(PropertyName = "membershipStatus")]
        public int MembershipStatus { get; set; }

        [JsonProperty(PropertyName = "requestParameters")]
        public ApiRequestParameters RequestParameters { get; set; }

        [JsonProperty(PropertyName = "isMfaEnabled")]
        public bool IsMfaEnabled { get; set; }

        [JsonProperty(PropertyName = "multiFactorAuthType")]
        public int MultiFactorAuthType { get; set; }

        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "membershipExtensionParameters")]
        public MembershipExtensionRequest MembershipExtensionParameters { get; set; }
    }

    public class SupportAccountInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "canImpersonate")]
        public long? CanImpersonate { get; set; }

        [JsonProperty(PropertyName = "countryId")]
        public long? CountryId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "isMssp")]
        public bool? IsMssp { get; set; }

        [JsonProperty(PropertyName = "isMfaEnabled")]
        public bool? IsMfaEnabled { get; set; }

        [JsonProperty(PropertyName = "isFedRamp")]
        public bool? IsFedRamp { get; set; }

        [JsonProperty(PropertyName = "lastUpdateByUserAccountId")]
        public long LastUpdateByUserAccountId { get; set; }

        [JsonProperty(PropertyName = "regRequiresSUApproval")]
        public bool? RequiresSuApproval { get; set; }
    }

    public class RolesInfo
    {
        [JsonProperty(PropertyName = "assigningRoles")]
        public int[] Roles { get; set; }
    }

    public class SelfRegistrationBase
    {
        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
        [JsonProperty(PropertyName = "registeredAppId")]
        public long RegisteredAppId { get; set; }
    }

    public class SelfRegistration : SelfRegistrationBase
    {
        [JsonProperty(PropertyName = "createdByUserAccountId")]
        public long CreatedByUserAccountId { get; set; }
    }

    public class UpdateSelfRegistration: SelfRegistrationBase
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
        [JsonProperty(PropertyName = "updatedBy")]
        public long UpdatedBy { get; set; }
    }


    public class UpdateEmailDomain
    {
        [JsonProperty(PropertyName = "emailDomainTypeName")]
        public string EmailDomainTypeName { get; set; }
        [JsonProperty(PropertyName = "acceptLicenseAgreement")]
        public string AcceptLicenseAgreement { get; set; }
        [JsonProperty(PropertyName = "updatedByUserAccountId")]
        public long UpdatedByUserAccountId { get; set; }
    }

    public class AscSupportRequestDto
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }
        [JsonProperty(PropertyName = "website")]
        public string Website { get; set; }
    }

    public class AscNotificationDetails
    {
        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; set; }
        [JsonProperty(PropertyName = "fullName")]
        public string FullName { get; set; }
        [JsonProperty(PropertyName = "isCaseCommentNotification")]
        public bool IsCaseCommentNotification { get; set; }
        [JsonProperty(PropertyName = "isCaseCreationNotification")]
        public bool IsCaseCreationNotification { get; set; }
    }
    public class ContactDetails
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
    }

    public class UpdateAscInformation
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "ascSupportRequestDto")]
        public AscSupportRequestDto AscSupportRequestDto { get; set; }
        [JsonProperty(PropertyName = "ascNotificationDetails")]
        public AscNotificationDetails AscNotificationDetails { get; set; }
        [JsonProperty(PropertyName = "contact")]
        public ContactDetails Contact { get; set; }
    }

    public class EvalAccountDetails
    {
        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "sales_order_number")]
        public string SalesOrderNumber { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }
    }
    public class UserDetail
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address1 { get; set; }

        [JsonProperty(PropertyName = "address2")]
        public string Address2 { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "countryId")]
        public long? CountryId { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "defaultSupportAccountId")]
        public long DefaultSupportAccountId { get; set; }

        [JsonProperty(PropertyName = "optContent")]
        public bool OptContent { get; set; }

        [JsonProperty(PropertyName = "optSecurity")]
        public bool OptSecurity { get; set; }

        [JsonProperty(PropertyName = "optPanSoftware")]
        public bool OptPanSoftware { get; set; }

        [JsonProperty(PropertyName = "createdByUserAccountId")]
        public long CreatedByUserAccountId { get; set; }

        [JsonProperty(PropertyName = "registeredAppId")]
        public long RegisteredAppId { get; set; }

        [JsonProperty(PropertyName = "comments")]
        public string Comments { get; set; }
    }

    public class ApiRequestParameters
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("size")]
        public int PageSize { get; set; }

        [JsonProperty("sort")]
        public string Sort { get; set; }

        [JsonProperty("search_field")]
        public string SearchField { get; set; }

        [JsonProperty("search_value")]
        public string SearchValue { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
    
    public class GoogleAuthRequest
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "secret")]
        public string SecretKey { get; set; }

        [JsonProperty(PropertyName = "requestedBy")]
        public string RequestedBy { get; set; }
    }

    public class GoogleAuthResponse
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }

    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }

    public class SSOThirdPartyInformation
    {    
        [JsonProperty(PropertyName = "isConfigured")]
        public bool IsConfigured { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string ThirdPartySsoConfigUrl { get; set; }

        [JsonProperty(PropertyName = "showUrl")]
        public bool ShowUrl { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }


        [JsonIgnore]
        public string domain { get; set; }

    }

    public class MyAccountsResponse
    {
        [JsonProperty("Data")]
        public IEnumerable<SelectableAccount> Data { get; set; }

        [JsonProperty("Total")]
        public long total { get; set; }
    }
    
    #endregion rewrite - end
}
