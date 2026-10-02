using System;
using System.Collections.Generic;
using System.Web;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using ExpressiveAnnotations.Attributes;
using Csp.Api.Model;

namespace Csp.Api.Model {

	/// <summary>
	/// Device master table which holds detailed information for device. 
	/// PAN service creates device entry when a sales order shipped by SAP. 
	/// More device information pushed to device_master during device registration process.					
	/// </summary>
    public class Device : AuditCspModel {
        public Device() {
			Licenses = new List<License>();
			SupportProvider = new SupportAccount();
			Customer = new SupportAccount();
			UserAccount = new UserAccount();
			DefaultRoles = new List<Role>();
			Members = new List<Membership>();
		    IsCloudDevice = false;
		    IsAwsDevice = false;
		    IsAzureDevice = false;
		    IsGoogleCloudDevice = false;
			IsCirroSecureDevice = false;
            IsSaasStubDevice = false;
		    IsCsspProvisionedDevice = false;
		    IsEligibleForUpgrade = false;
		    IsUpgradeInProgress = false;
		    IsPanorama = false;
		    GeneratePanoramaLicense = false;
            IsTraps = false;
            IsTrapsManagementService = false;
            Contains4HRSupport = false;
            IsNgfwDlpDevice = false;
            IsCloudNgfw = false;
        }
        [DataAccess("PanOsVersion", DataType = DbType.String)]
        public string PanOsVersion { get; set; }
        
        [DataAccess("HyperVisorVersion", DataType = DbType.String)]
        public string HyperVisorVersion { get; set; }


		/// <summary>
		/// Device id.
		/// </summary>
		[DataAccess(DeviceFields.DeviceId, DataType = DbType.Int64, UseForCreate = false)]
        public long Id {
			set;
			get;
		}

        public bool IsCloudDevice { get; set; }

        public bool IsAwsDevice { get; set; }

        public bool IsAzureDevice { get; set; }

	    public bool IsGoogleCloudDevice { get; set; }

		public bool IsCirroSecureDevice { get; set; }

        public bool IsSaasStubDevice { get; set; }

	    public bool IsCsspProvisionedDevice { get; set; }

        public bool IsEligibleForUpgrade { get; set; }

        public bool IsUpgradeInProgress { get; set; }

        public string VmUpgradeFrom { get; set; }

        public string VmUpgradeTo { get; set; }

        public string TransactionId { get; set; }

        public string VmUpgradeAuthCode { get; set; }

        public bool IsUpgradeComplete { get; set; }

        public bool GeneratePanoramaLicense { get; set; }

        public bool IsPanorama { get; set; }

        public IEnumerable<ChassisComponent> LineCards { get; set; }

        public bool IsTraps { get; set; }

        public bool IsTrapsManagementService { get; set; }

        public bool Contains4HRSupport { get; set; }

        public bool IsNgfwDlpDevice { get; set; }
        public bool IsCloudNgfw { get; set; }
        public string TenantId { get; set; }


        /// <summary>
        /// Device serial number.
        /// </summary>
        [DataAccess(DeviceFields.SerialNumber),
        RequiredIf("IsCloudDevice == false", ErrorMessage = "Serial Number is required"),
		StringLength(35, ErrorMessage = "Maximum length is 35 characters"),
		Remote("IsValidSerial", "Validation", AdditionalFields = "Customer.Id")]
        public string SerialNumber {
			set;
			get;
		}

        [RequiredIf("IsAwsDevice == true", ErrorMessage = "Serial # is required"),
        StringLength(35, ErrorMessage = "Maximum length is 35 characters")]
        public string AwsSerialNumber
        {
            set;
            get;
        }

        [RequiredIf("IsAwsDevice == true", ErrorMessage = "CPU ID is required"),
        RegularExpression(@"^((?!AZR).)*$", ErrorMessage = "Invalid AWS CPU ID")] 
        public string AwsCpuid
        {
            set;
            get;
        }

        public string AwsUuid
        {
            set;
            get;
        }

        // Comments By LK On 05/10/2018
        // This attribute is still being referenced in many places. Please clean-up the code before marking it obsolete
        // This is obsolete. Now we capture AWS CPUID instead.
        //[Obsolete]
       public string InstanceId
        {
            set;
            get;
        }

        // Comments By LK On 05/10/2018
        // This attribute is still being referenced in many places. Please clean-up the code before marking it obsolete
        // This is obsolete. Now we capture AWS CPUID instead.
        //[Obsolete]
        public string ProductCode
        {
            set;
            get;
        }

        // Comments By LK On 05/10/2018
        // This attribute is still being referenced in many places. Please clean-up the code before marking it obsolete
        // This is obsolete. Now we capture AWS CPUID instead.
        //[Obsolete]
        public string ZoneOrRegion
        {
            set;
            get;
        }

        [RequiredIf("IsAzureDevice == true", ErrorMessage = "Serial Number is required"),
        RegularExpression("^[A,a,6][A-Z,a-z,0-9]{14}", ErrorMessage = "Invalid Azure Serial Number"),
        StringLength(15, ErrorMessage = "Invalid Azure Serial Number")]
        public string AzureSerialNumber
        {
            set;
            get;
        }

        [RequiredIf("IsAzureDevice == true", ErrorMessage = "CPU ID is required"),
        RegularExpression(@"^((?!AWS).)*$", ErrorMessage = "Invalid Azure CPU ID")] 
        public string AzureCpuid
        {
            set;
            get;
        }

        public string AzureUuid
        {
            set;
            get;
        }

        // Comments By LK On 05/10/2018
        // This attribute is still being referenced in many places. Please clean-up the code before marking it obsolete
        //[Obsolete]
        public string AzureProductCode
        {
            set;
            get;
        }

        // Comments By LK On 05/10/2018
        // This attribute is still being referenced in many places. Please clean-up the code before marking it obsolete
        //[Obsolete]
        public string AzureRegion
        {
            set;
            get;
        }

	    [RequiredIf("IsGoogleCloudDevice == true", ErrorMessage = "Serial Number is required"),
         RegularExpression("^7[A-Z,a-z,0-9]{14}", ErrorMessage = "Invalid Google Cloud Serial Number")]
        public string GoogleCloudSerialNumber { get; set; }

	    [RequiredIf("IsGoogleCloudDevice == true", ErrorMessage = "CPU ID is required"),
        RegularExpression(@"^GCPMP:(.)+:(.)+:(.)+", ErrorMessage = "Invalid Google Cloud CPU ID")]
        public string GoogleCloudCpuid { get; set; }
        
	    [RequiredIf("IsGoogleCloudDevice == true", ErrorMessage = "UUID is required")]
	    public string GoogleCloudUuid { get; set; }

		/// <summary>
		/// Device part id.  In CSP schema Foreign key to the PartMaster table.
		/// </summary>
		[DataAccess(DeviceFields.PartNumber)]
        public String ModelName {
			set;
			get;
		}


		/// <summary>
		/// List of Licenses associated with Device
		/// </summary>
        public List<License> Licenses {
			set;
			get;
		}


		/// <summary>
		/// Support Account which manages this device (ASC). Foreign key to SupportAccount table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = DeviceFields.SupportCenterSupportAccountId, UseKeyOnly = true)]
        public SupportAccount SupportProvider {
			set;
			get;
		}


		/// <summary>
		/// Support Account id which own this device. Foreign key to SupportAccount table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = DeviceFields.SupportAccountId, UseKeyOnly = true)]
        public SupportAccount Customer {
			set;
			get;
		}


		/// <summary>
		/// User account who register the device. Foreign key to UserAccount table.
		/// </summary>
		[DataAccessModel(DataType = DbType.Int64, PropertyName = "Id", StorageName = DeviceFields.UserAccountId, UseKeyOnly = true)]
        public UserAccount UserAccount {
			set;
			get;
		}


		/// <summary>
		///Determines the initial list of roles automatically assigned to a member when added.					
		/// </summary>
        public List<Role> DefaultRoles {
			set;
			get;
		}


		/// <summary>
		/// List of members Associated with Device
		/// </summary>
        public List<Membership> Members {
			set;
			get;
		}


		/// <summary>
		/// Device registered date. When the device shipped PAN service create entry with default value (NULL) and the value updated during device registration process.
		/// </summary>
		[DataAccess(DeviceFields.RegistrationDate, DataType = DbType.DateTime)]
        public DateTime? RegistrationDate {
			set;
			get;
		}


		/// <summary>
		/// Device ship date.
		/// </summary>
		[DataAccess(DeviceFields.ShipDate, DataType = DbType.DateTime)]
        public DateTime ShipDate {
			set;
			get;
		}


		/// <summary>
		/// Status of the device.
		/// </summary>
		[DataAccess(DeviceFields.IsActive, DataType = DbType.Int32)]
        public bool IsActive {
			set;
			get;
		}


		/// <summary>
		/// Shipment number.
		/// </summary>
		[DataAccess(DeviceFields.ShipmentNumber, DataType = DbType.Int64)]
        public long ShipmentNumber {
			set;
			get;
		}


		/// <summary>
		/// Device name given during registration process.
		/// </summary>
		private string _deviceName;
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
        public string DeviceName {
			set { _deviceName = ApiCommon.DecodeString(value); }
			get { return _deviceName; }
		}

		[DataAccess(DeviceFields.DeviceName)]
        public string DeviceNameEncoded {
			set { DeviceName = value; }
			get { return ApiCommon.EncodeString(DeviceName); }
		}

		/// <summary>
		/// Comment added during registration process.
		/// </summary>
		private string _comments;
		[StringLength(255, ErrorMessage = "Maximum length is 255 characters")]
        public string Comments {
			set { _comments = ApiCommon.DecodeString(value); }
			get { return _comments; }
		}

		[DataAccess(DeviceFields.Comments)]
        public string CommentsEncoded {
			set { Comments = value; }
			get { return ApiCommon.EncodeString(Comments); }
		}

		/// <summary>
		/// Used during RMA process by licensing services.  
		/// When a replacement device sent to user the old device src marked as 1.
		/// </summary>
		[DataAccess(DeviceFields.IsRMAReturn, DataType = DbType.Int32)]
        public bool IsRMAReturn {
			set;
			get;
		}


		/// <summary>
		/// Used during RMA process by licensing services.  
		/// When a replacement device sent to user the new device dest marked as1.
		/// </summary>
		[DataAccess(DeviceFields.IsRMAReplacement, DataType = DbType.Int32)]
        public bool IsRMAReplacement {
			set;
			get;
		}


		/// <summary>
		/// Not used.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(DeviceFields.SalesForceAccountId)]
        public string SalesForceAccountId {
			set;
			get;
		}


		/// <summary>
		/// Updated during registration process.
		/// </summary>
		[StringLength(3000, ErrorMessage = "Maximum length is 3000 characters"),
		DataAccess(DeviceFields.CustomerServiceNotes)]
        public string CustomerServiceNotes {
			set;
			get;
		}


		/// <summary>
		/// Used by IBC sync application.
		/// </summary>
		[DataAccess(DeviceFields.IsSAPSynched, DataType = DbType.Int32)]
        public bool IsSAPSynched {
			set;
			get;
		}


		/// <summary>
		/// A device may have number of virtual machines running on it based on the device type. 
		/// The capable devices are shipped including minimum number of virtual machines 
		/// and this virtual machine count can be increased by purchasing additional licenses. 
		/// The maximum virtual machine that a device can have is specified in the virtual systems table.
		/// </summary>
		[DataAccess(DeviceFields.VirtualSystemsAvailable, DataType = DbType.Int16)]
        public Int16 VirtualSystemsAvailable {
			set;
			get;
		}


		/// <summary>
		/// This field is used to determine the support start date. 
		/// When the registration date is later than the ship_date + grace period 
		/// the support starts from end of the grace period. 
		/// </summary>
		[DataAccess(DeviceFields.GracePeriodDays, DataType = DbType.Int16)]
        public Int16 GracePeriodDays {
			set;
			get;
		}


		/// <summary>
		/// Billing country updated by SAP during initial entry.  In CSP schema Foreign key to the Country table.
		/// </summary>
		[DataAccess(DeviceFields.BillToCountryId, DataType = DbType.Int64)]
        public long BillToCountryId {
			set;
			get;
		}

        [DataAccess(DeviceFields.Address1),
         StringLength(250, ErrorMessage = "Maximum length is 250 characters"),
         RequiredIf("IsCloudDevice == false", ErrorMessage = "Address Line 1 is required")]
        public string Address1 { get; set; }

        [DataAccess(DeviceFields.Address2),
         StringLength(250, ErrorMessage = "Maximum length is 250 characters")]
	    public string Address2 { get; set; }

		/// <summary>
		/// Updated during device registration process.
		/// </summary>
		[DataAccess(DeviceFields.City),
		StringLength(100, ErrorMessage = "Maximum length is 100 characters"),
        RequiredIf("IsCloudDevice == false", ErrorMessage = "City is required")]
        public string City {
			set;
			get;
		}


		/// <summary>
		/// Updated during device registration process.
		/// </summary>
		[DataAccess(DeviceFields.PostalCode),
		StringLength(15, ErrorMessage = "Maximum length is 15 characters"),
        RequiredIf("IsCloudDevice == false", ErrorMessage = "Postal Code is required")]
        public string PostalCode {
			set;
			get;
		}


		/// <summary>
		/// Updated during device registration process.   In CSP schema Foreign key to the Country table.
		/// </summary>
		[DataAccess(DeviceFields.CountryId),
        RequiredIf("IsCloudDevice == false", ErrorMessage = "Country is required")]
        public long? CountryId {
			set;
			get;
		}


		/// <summary>
		/// Updated during device registration process.   
		/// </summary>
		[DataAccess(DeviceFields.Region),
		StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
        public string Region {
			set;
			get;
		}

        [StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
        public string RegionText {
            set;
            get;
        }

		/// <summary>
		/// Updated during device registration process.   In CSP schema Foreign key to the Country table.
		/// </summary>
		[DataAccess(DeviceFields.IsVirtualMachine, DataType = DbType.Int32)]
        public bool IsVirtualMachine {
			set;
			get;
		}


		/// <summary>
		/// If a device is virtual machine then CPUID of the client system is added here.
		/// </summary>
        [StringLength(64, ErrorMessage = "Maximum length is 64 characters"),
		DataAccess(DeviceFields.CpuId)]
        public string CpuId {
			set;
			get;
		}


		/// <summary>
		/// If a device is virtual machine then UUID of the client system is added here.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(DeviceFields.UuId)]
        public string UuId {
			set;
			get;
		}


		/// <summary>
		/// If a device is virtual machine then Mac address of the client system is added here.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		DataAccess(DeviceFields.MacAddress)]
        public string MacAddress {
			set;
			get;
		}

		[DataAccess(DeviceFields.DeviceCategoryId, DataType = DbType.Int32)]
        public long? DeviceCategoryId {
			set;
			get;
		}

		[DataAccess("CountryName", DataType = DbType.String)]
		public string CountryName { set; get; }

		[DataAccess("DeviceCategoryName", DataType = DbType.String)]
		public string DeviceCategoryName { set; get; }

        [RegularExpression(@"^([0-9]*(-)+[A-Za-z0-9\s_@%!#?,.\\:/\^\|\$\&\*-]+)$", ErrorMessage = "CSSP Customer Reference Id is invalid"), RequiredIf("IsCsspProvisionedDevice == true", ErrorMessage = "CSSP Customer Reference Id is required"),
        Remote("ValidateCsspEndUserCustomerAccountId", "Validation", AdditionalFields = "Device.CsspSerialNumber")]
        public string CustomerAccountId { get; set; }

        [RequiredIf("IsCsspProvisionedDevice == true", ErrorMessage = "Serial Number is required"),
        Remote("ValidateCsspEndUserSerialNumber", "Validation", AdditionalFields = "Customer.Id")]
        public string CsspSerialNumber { get; set; }

        public bool IsCsspEndUserDeviceRegistrationSuccess { get; set; }

        public bool SupportAccountHasEsa { get; set; }

        [RequiredIf("SupportAccountHasEsa == true", ErrorMessage = "Enterprise Support Agreement option is required")]
        public int? ApplyEsaLicenseToDevice { get; set; }

        #region CGX

        public bool IsCgx { get; set; }
        public bool IsTrialLicenseExcludePanorama { get; set; }

        #endregion

    }

    public class DeviceWithPartMaster : Device {
		[DataAccess(DeviceFields.PartNumber, DataType = DbType.String)]
        public string PartNumber {
			set;
			get;
		}

		[DataAccess(DeviceFields.ModelCompatibility, DataType = DbType.String)]
        public string ModelCompatibility {
			set;
			get;
		}

		public long? SupportAccountId { get; set; }

	}
}
public class ChassisComponent : BaseCspModel
{
    [DataAccess("SalesOrderNumber", DataType = DbType.Int64)
    , Remote("IsSalesOrderComponentsRegistered", "Validation")
    , Required(ErrorMessage = "Sales order number required")]
    public string SalesOrderNumber
    {
        set;
        get;
    }

    [DataAccess("LineCardId", DataType = DbType.Int64)]
    public Int64 LineCardId
    {
        set;
        get;
    }

    [DataAccess(DeviceFields.DeviceId, DataType = DbType.Int64)]
    public Int64 DeviceId
    {
        set;
        get;
    }

    [DataAccess("ChasisPartNumber", DataType = DbType.String)]
    public string ChasisPartNumber
    {
        set;
        get;
    }
    [DataAccess(DeviceFields.ShipmentNumber, DataType = DbType.Int64)]
    public Int64 ShipmentNumber
    {
        set;
        get;
    }

    [DataAccess(DeviceFields.PartMasterId, DataType = DbType.Int64)]
    public Int64 PartMasterId
    {
        set;
        get;
    }

    [DataAccess(DeviceFields.PartNumber, DataType = DbType.String)]
    public string PartNumber
    {
        set;
        get;
    }

    [DataAccess("Description", DataType = DbType.String)]
    public string Description
    {
        set;
        get;
    }

    [DataAccess("ModelCompatibility", DataType = DbType.String)]
    public string ModelCompatibility
    {
        set;
        get;
    }

    [DataAccess("ProductFamily", DataType = DbType.String)]
    public string ProductFamily
    {
        set;
        get;
    }

    [DataAccess("Quantity", DataType = DbType.Int64), Remote("ValidateComponentQuantity", "Validation", AdditionalFields = "ActualQuantity,PartNumber,SupportAccountId")]
    public Int64 Quantity
    {
        set;
        get;
    }

    [DataAccess("Quantity", DataType = DbType.Int64)]
    public Int64 ActualQuantity
    {
        set;
        get;
    }

    [DataAccess("RegisteredQuantity", DataType = DbType.Int64)]
    public Int64 RegisteredQuantity
    {
        set;
        get;
    }

    [DataAccess("ChassisSerialNumber", DataType = DbType.String), Remote("IsValidSerialAndRegistered", "Validation")]
    public string ChassisSerialNumber
    {
        set;
        get;
    }

    [DataAccess("ChassisSerialNumber", DataType = DbType.String)]
    public string OldChassisSerialNumber
    {
        set;
        get;
    }

    [DataAccess(DeviceFields.Address1),
     StringLength(250, ErrorMessage = "Maximum length is 250 characters"),
     Required(ErrorMessage = "Address Line 1 is required")]
    public string Address1 { get; set; }

    [DataAccess(DeviceFields.Address2),
     StringLength(250, ErrorMessage = "Maximum length is 250 characters")]
    public string Address2 { get; set; }

    [DataAccess(DeviceFields.City)]
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "Maximum length is 100 characters")]
    public string City { set; get; }

    [DataAccess(DeviceFields.PostalCode)]
    [Required(ErrorMessage = "Postal Code is required")]
    [StringLength(15, ErrorMessage = "Maximum length is 15 characters")]
    public string PostalCode { set; get; }

    [DataAccess(DeviceFields.Region)]
    [StringLength(100, ErrorMessage = "Maximum length is 100 characters")]
    public string Region { set; get; }

    [DataAccess("CountryName", DataType = DbType.String)]
    public string CountryName { set; get; }

    [DataAccess("CountryId", DataType = DbType.Int64)]
    [Required(ErrorMessage = "Country is required")]
    public Int64 CountryId { set; get; }

    [DataAccess("SupportAccountId", DataType = DbType.Int64)]
    public Int64 SupportAccountId { set; get; }

    [DataAccess("UserAccountId", DataType = DbType.Int64)]
    public Int64 UserAccountId { set; get; }

}

