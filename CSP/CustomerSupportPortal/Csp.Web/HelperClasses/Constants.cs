using System;


namespace Csp.Web.HelperClasses {
	public static class Constants {

		public const string DropDownPleaseSelect = "- Please Select -";
		public const string DropDownAccountTypeSelect = "- Account Type Select -";
		public const string DropDownCountrySelect = "- Country Select -";
		public const string DropDownRegionSelect = "- Region/State Select -";
        public const string NotApplicable = "NOT APPLICABLE";
        public const string DropDownLoggingRegionSelect = "- Region Select -";
        public const string DropDownSpacer = "----------------";
        public const string DropDownContactSelect = "- Contact Select -";
        public const string DropDownOsReleaseSelect = "- OS Release Select -";
        public const string DropDownVirtualPlatformSelect = "- Virtual Platform Select -";
        public const string DropDownPanoramaSelect = "- Panorama Select -";
        public const string DropDownLoggingServiceSelect = "- Cortex Data Lake Select -";
		public const string SuccessfullySaved = "Successfully Saved.";

		public const string RequiredField = " Required";

		public const string DeletePreMessage = "Are you sure you want to remove this ";

		public const string HOME = "Home";
		public const string ACCOUNT = "Company Account";
		public const string GROUPS = "Groups";
		public const string DEVICES = "Assets";
		public const string MEMBERS = "Members";

		public const string GROUP_DEVICES = "Devices";
		public const string GROUP_MEMBERS = "Members";

        public const string CSSP = "Cssp";

		public const string PARENT_COOKIE = "parentidpopentoken";
        public const string SUPPORT_COOKIE = "SUPPORT";
        public const string SESSION_COOKIE = "idpopentoken";

        public const int PANORAMA_INTERGRATION_PLUGIN = 238;

		public const string SALESFORCE_COOKIE = "_sdsat_eMail";
		public const string DeleteDefaultAccountPreMessage = "Deleting user from their default account will prevent them from logging in to the Customer Support Portal. Are you sure you want to proceed";
        public const string DeleteLastSuperUserPreMessage = "Please delegate another user as a Super User. You cannot have an account without a Super User";

        public const string GrantSiteLicenseErrorMessage = "The system was unable to grant a site license.";
        public const string DeviceRegistrationSuccessMessage = "Device was successfully registered.";
        public const string AscDeviceRegistrationSuccessMessage = "Device was successfully registered to {}";
        public const string InvalidSerialNumberMessage = "Serial number does not exist.";
        public const string InvalidSupportAccountId = "Invalid Support Account Id";
        public const string InvalidUserAccountId = "Invalid User Account Id";
        public const string UnauthorizedMessage = "You do not have permission to access this content. Please contact Palo Alto Networks Support";
        public const string EvalAccountMessage = "To register a new purchased asset, the Eval account must be converted to an active CSP account. Please go to the Company Account page to activate your account.";

        public const string DeleteLastDomainAdminPreMessage = "Please delegate another user as a Domain Administrator. You cannot have an account without a Domain Administrator";

        public const string ProfileNameExists = "This profile name already exists. Please choose a different name.";
        public const string CreditsUsed = "You are not able to reduce the cost of your deployment profile because all of your firewalls have been registered. Please deactivate a firewall before making this update.";
        public const string NotEnoughCredits = "Insufficient Credits.Total Available: {0}, Pending: {1}, Available for Use: {2}. Please contact Palo Alto Networks TAC to resolve Pending Credits (if any).";
        public const string TokensConsumedMessage = "You are not able to reduce the cost of your deployment profile because you have consumed more than the cost you are estimating.";
        public const string CreditTransferredSuccessMessage = "Credit transferred successfully.";
        public const string CreditTransferredFailedMessage = "We are unable to transfer credits now, please try again later.";
        public const string TokenActivationSuccessMessage = "Token(s) activated successfully.";
        public const string TokenActivationFailedMessage = "An error has occurred, please refresh the page and try again. If the problem persists, please contact Customer Support.";
        public const string PanoramaProvisionSuccessMessage = "You have successfully provisioned panorama.";
        public const string PanoramaProvisionFailedMessage = "We are unable to generate a serial number for your Panorama. Please try again or contact Customer Support.";
        public const string EstimateCostNotWorkingMessage = "The Calculate Estimated Cost feature is currently not working, please try again later.";
        public const string RegisterFirewallFailedMessage = "We are unable to register your firewall. Please try again or contact Customer Support.";
        public const string VMSeriesVcpuRestrictionMessage = "VM-Series vCPU maximum limit is 64.";
        public const int VMSeriesVcpuQuantityMax = 64;
        public const string CNSeriesVcpuRestrictionMessage = "CN-Series vCPU maximum limit is 47.";
        public const int CNSeriesVcpuQuantityMax = 47;
        public const string DeleteMembership = "Membership has been deleted successfully.";
        public const string DeviceTransferedSuccessfully = "The device is successfully transferred to support account.";
        public const string DeviceTransferPending = "The device is now pending transfer until the recipient receives the device.";
        public const string UnexpectedError = "An unexpected error has occurred.";
        public const string TransferDeploymentProfileFailureMessage = "The system is unable to process your transfer deployment profile request. Please try again or contact Customer Support.";
        public const string TransferDeploymentProfileIncompatibleMessage = "Deployment Profile cannot be transferred, since some subscriptions are incompatible with the target credit pool.";
        public const string NotEnoughCreditsToTransfer = "Target credit pool do not have enough credits to transfer this deployment profile.";
        public const int PanoramaSelectedSerialNumbersMax = 5;
        public const string PanoramaDeleteFailedMessage = "We are unable to generate Delete Panorama. Please try again or contact Customer Support.";
        public const string TokenPoolEditSuccessMessage = "Pool name has been updated successfully.";
        public const string TokenPoolEditFailedMessage = "An error has occurred on updating pool name. Please try again or contact Customer Support.";
        public const string BlockListedDomainMessage = "Domain name of the email address is not supported by Palo Alto Networks. If this is your email address, please open a case at https://support.paloaltonetworks.com.";
        public const string PublicDomainMessage = "Error.  Please Enter a Public Domain Email Address " +
                  " You will be added to a CSP account that requires public domain email addresses, e.g., sally @gmail.com." +
                  " If you have questions on your email address, please contact a Super User of this CSP account.";
        public const string PrivateDomainMessage = "Error.  Please Enter a Private Domain Email Address" +
                  " You will be added to a CSP account that requires private domain email addresses, e.g., sally @acme.com." +
                  " If you have questions on your email address, please contact a Super User of this CSP account.";
        public const string InvalidToken = "Invalid token";
        public const string InvalidFilter = "There's no filter with this type : {0}";
    }
}