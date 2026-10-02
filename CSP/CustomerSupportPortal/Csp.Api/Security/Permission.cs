
namespace Csp.Api.Security {


	/// <summary>
	/// This is the enum used to give a permission a meaningful name. The permission is used in a custom CSPAuthorizeAttribute and 
	/// is loaded into the CspPrincipal from the database to determine authorization access to parts of the system. The permissions 
	/// are stored in the privileges table of the database in the permission field. In the database the value is stored as a ordinal 
	/// value (integer) and are converted to a meaningful name in the system through this enum. The privilege table also has the name
	/// of the permissions as it is visible to administrators. Each range of permissions to different sections have a space of 
	/// 1000 items. It is extremely unlikely there will be more than 1,000 per functional area in the system. 
	/// </summary>
	public enum Permission {

		#region  SupportAccount Permissions Enum range 0 - 999

		ViewSupportAccount = 0,
		ViewSupportAccountGroups = 1,
		ViewSupportAccountDevices = 2,
		ViewSupportAccountMemberships = 3,
		EditSupportAccount = 4,
		ViewAccountType = 5,
		ViewCustomerSupportAccounts = 6,
		EditCompanyName = 7,
		EditDeviceTags = 8,
		EditWildfireUsers = 9,
		ViewDynamicUpdates = 10,
		ViewSoftwareUpdates = 11,
		ViewCaseManagement = 12,
        ViewAutoFocusLink = 13,
        EditAutoFocusApiKey = 14,
        ManageOauthAccess =16,
        #endregion

        #region Group Permissions Enum range 1000 - 1999

        ViewGroup = 1000,
		ViewGroupDevices = 1001,
		ViewGroupMemberships = 1002,
		CreateGroup = 1003,
		EditGroup = 1004,
		DeleteGroup = 1005,
        EditGroupMembership = 1006,
	    DeleteGroupMembership = 1007,
	    CreateGroupMembership = 1008,

		#endregion

		#region UserAccount Permissions Enum range 2000 - 2999

		ViewUserAccount = 2000,
		ViewUserAccountDeviceMemberships = 2001,
		ViewUserAccountGroupMemberships = 2002,
		ViewUserAccountSupportAccountMemberships = 2003,
		CreateUserAccount = 2004,
		EditUserAccount = 2005,
		DeleteUserAccount = 2006,

		#endregion

		#region Device Permissions Enum range 3000 - 3999

		ViewDevice = 3000,
		ViewDeviceLicenses = 3001,
		ViewDeviceGroups = 3002,
		ViewDeviceMemberships = 3003,
		EditDevice = 3004,
		BeginTransferDevice = 3005,
		ReviewAllDeviceTransfers = 3006,
		TransferLicenses = 3007,
		VmAuthRegister = 3008,
		RegisterDevice = 3009,
		BulkRegisterDevice = 3010,
        RegenerateRevokeLicenseKey = 3011,
        GenerateSaasOTP = 3012,
        BulkDeviceTransfer = 3013,
        CnSeriesRegister = 3014,

		#endregion

		#region Membership Permissions Enum range 4000 - 4999

		ManageMembership = 4000,
		ViewMembership = 4001,
		EditMembership = 4002,
		CreateMembership = 4003,
		DeleteMembership = 4004,

		#endregion

		#region Admin Permissions Enum range 5000 - 5999

		ViewAdmin = 5000,
		ViewAdminSite = 5001,
		ImpersonateUser = 5002,

		#endregion

        #region Report Permissions Enum range 6000 - 6999

        ViewASCDashboardReport = 6000,
        ViewPartnerServices = 6001,

		#endregion

        #region Misc Permissions Enum range 7000 - 7999

        ViewBulkRegistration = 7000,
        ViewPA200SSDPortal = 7001,
        ViewBPATool = 7009,
	    ViewBPAHistory = 7010,
	    ManageVmElaTokens = 7012,
        ManageFirewallFlexTokens = 7013,
	    ViewFirewallFlexTokens = 7014,

       
		#endregion

        #region Consent Request Permissions range 8000 - 8999
        CreateConsentRequest = 8000,
        ActivateProducts = 8001,
        #endregion
        ManageShipments = 3015,
        ViewEnterpriseAgreements = 7005,
		ViewNetsec = 8003,
        ViewTrainingCredits = 8004,
        ViewDeviceCertificates = 1010

    }
}
