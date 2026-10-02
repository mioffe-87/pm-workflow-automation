using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Model.Attributes;
using Csp.Web.Attributes;
using ExpressiveAnnotations.Attributes;

namespace Csp.Web.DataTransferObjects {

	public class UserAccountRegistrationDto  : DefaultDto {

        public UserAccountRegistrationDto()
        {
            IsUsingCloudDeviceRegistration = false;
            IsCsspProvisionedDevice = false;
        }

		public UserAccount UserAccount {
			get;
			set;
		}

        public bool IsCsspProvisionedDevice { get; set; }

		// Comments By LK
		// Hack to fix issue with compare validator
		// Refer: http://connect.microsoft.com/VisualStudio/feedback/details/665793/jquery-unobtrusive-validate-equalto-fails-with-compare-attribute
		// Refer: http://aspnet.codeplex.com/workitem/8596
		public string EmailAddress { get; set; }
		[System.ComponentModel.DataAnnotations.Compare("EmailAddress", ErrorMessage = "Your Email Addresses do not match"),
		StringLength(200, ErrorMessage = "Maximum length is 200 characters")]
		public string ConfirmEmail { get; set; }

        public bool IsUsingCloudDeviceRegistration { get; set; }

        public bool IsAzureRegistration { get; set; }
        public bool IsAwsRegistration { get; set; }
        public bool IsCsspRegistration { get; set; }
        public bool IsGoogleCloudRegistration { get; set; }

        //Panorama related properties
        #region Panorama related properties
        public bool IsPanoramaRegistration { get; set; }
        public string PanOsVersion { get; set; }
        public string HyperVisorVersion { get; set; }

        [StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
        public string UuId { get; set; }

	    [StringLength(64, ErrorMessage = "Maximum length is 64 characters")]
        public string CpuId { get; set; }

	    #endregion

        [RegularExpression(@"^([0-9]*(-)+[A-Za-z0-9\s_@%!#?,.\\:/\^\|\$\&\*-]+)$", ErrorMessage = "CSSP Customer Reference Id is invalid"), RequiredIf("IsCsspProvisionedDevice == true", ErrorMessage = "CSSP Customer Reference Id is required"),
        Remote("ValidateCsspEndUserCustomerAccountIdForNewUserRegistration", "Validation", AdditionalFields = "UserAccountRegistrationDto.CsspSerialNumber")]
        public string CustomerAccountId { get; set; }

        [RequiredIf("IsCsspProvisionedDevice == true", ErrorMessage = "Serial Number is required"),
        Remote("ValidateCsspEndUserSerialNumberForNewUserRegistration", "Validation")]
        public string CsspSerialNumber { get; set; }	

		[Required(ErrorMessage = "Region/State is required"),
		StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
		public string OtherRegion { get; set; }

		public List<Country> Countries { get; set; }

        public List<AwsProduct> AwsProducts { get; set; }

        public List<AwsRegion> AwsRegions { get; set; }

        public List<CloudRegion> CloudRegions { get; set; }
        public List<CloudProductType> CloudProducts { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == false", ErrorMessage = "Device Serial Number or Authorization Code or Parent Order Number is required")]
		public string DeviceCode { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == false", ErrorMessage = "Customer Id or Sales Order Number is required")]
		public string CustomerCode { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true", ErrorMessage = "Support Account Name is required")]
        public string SupportAccountName { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsAwsRegistration == true", ErrorMessage = "Serial # is required")]
        public string AwsSerialNumber { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsAwsRegistration == true", ErrorMessage = "CPU ID is required"),
        RegularExpression(@"^((?!AZR).)*$", ErrorMessage = "Invalid AWS CPU ID")] // This check is added to prevent the Support account created with Azure Default Account when entering AWS VM
        public string AwsCpuid { get; set; }

        public string AwsUuid { get; set; }

        [Obsolete]
        public string AwsInstanceId { get; set; }

        [Obsolete]
        public string AwsProductCode { get; set; }

        [Obsolete]
        public string AwsRegion { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsAzureRegistration == true", ErrorMessage = "Serial Number is required"),
        RegularExpression("^[A,a,6][A-Z,a-z,0-9]{14}", ErrorMessage = "Invalid Azure Serial Number"),
        StringLength(15, ErrorMessage = "Invalid Azure Serial Number")]
        public string AzureSerialNumber { get; set; }

        [Obsolete]
        public string AzureProductCode { get; set; }

        [Obsolete]
        public string AzureRegion { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsAzureRegistration == true", ErrorMessage = "CPU ID is required"),
        RegularExpression(@"^((?!AWS).)*$", ErrorMessage = "Invalid Azure CPU ID")] // This check is added to prevent the Support account created with AWS Default Account when entering Azure VM
        public string AzureCpuid { get; set; }

        public string AzureUuid { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsGoogleCloudRegistration == true", ErrorMessage = "Serial Number is required"),
	     RegularExpression("^7[A-Z,a-z,0-9]{14}", ErrorMessage = "Invalid Google Cloud Serial Number")]
	    public string GoogleCloudSerialNumber { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsGoogleCloudRegistration == true", ErrorMessage = "CPU ID is required"),
	     RegularExpression(@"^GCPMP:(.)+:(.)+:(.)+", ErrorMessage = "Invalid Google Cloud CPU ID")]
	    public string GoogleCloudCpuid { get; set; }

        [RequiredIf("IsUsingCloudDeviceRegistration == true && IsGoogleCloudRegistration == true", ErrorMessage = "UUID is required")]
	    public string GoogleCloudUuid { get; set; }


		[BooleanRequired(ErrorMessage = "You must accept the terms and conditions")]
		public bool IsTermsAgreed { get; set; }

		public bool IsUserExists { get; set; }
		public bool HasMultipleAccounts { get; set; }
		public long SapId { get; set; }
		public string RedirectUrl { get; set; }
		public long SelectedSupportAccountId { get; set; }

        public string SelfRegistrationCode { get; set; }

        // Panorama
        public RegisterVmDto VMRegistrationDto { get; set; }
	}
}