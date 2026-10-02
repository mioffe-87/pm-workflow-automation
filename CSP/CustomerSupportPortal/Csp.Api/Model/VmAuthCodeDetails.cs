using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;
using LINQtoCSV;
using System.Collections.Generic;

namespace Csp.Api.Model {
	public class VmAuthCodeDetails : BaseCspModel {

		[DataAccess(VirtualMachineFields.AuthorizationCode, DataType = DbType.String)]
		public string AuthCode { get; set; }

		[DataAccess(VirtualMachineFields.RegistrationDate, DataType = DbType.DateTime)]
		public DateTime? RegistrationDate { get; set; }

		[DataAccess(VirtualMachineFields.ExpirationDate, DataType = DbType.DateTime)]
		public DateTime? ExpirationDate { get; set; }

		[DataAccess(VirtualMachineFields.IsActive, DataType = DbType.Boolean)]
		public bool IsActive { get; set; }

		[DataAccess(VirtualMachineFields.PartNumber, DataType = DbType.String)]
		public string PartNumber { get; set; }

		[DataAccess(VirtualMachineFields.VmStubDeviceId, DataType = DbType.Int64)]
		public long? VmStubDeviceId { get; set; }

		[DataAccess(VirtualMachineFields.PanDbIdentification, DataType = DbType.String)]
		public string PanDbIdentification { get; set; }

	}

	public class DeviceRec : BaseCspModel {

		[DataAccess("DeviceId", DataType = DbType.Int64)]
		public long DeviceId { get; set; }

		[DataAccess("SerialNumber", DataType = DbType.String)]
		public string SerialNumber { get; set; }
	}

    public class AvailableVirtualSeries<T> : AvailableVmBase
        where T : AvailableCn
    {
        [Required]
        public string AuthorizationCode { get; set; }

        public string MonthlyPeakUsageTotal { get; set; }

        public IEnumerable<T> Vms { get; set; }
    }

    public class AvailableCn : AvailableVmBase
    {
        [Required]
        public string SerialNumber { get; set; }

        public string PanoramaLicenseId { get; set; }       

        public string ProvisionedAndTotal
        {
            get
            {
                return VmUseCount + "/" + VmTotalCount;
            }
        }

        public string MonthlyPeakUsage { get; set; }

        public string MonthlyAverageUsage { get; set; }
    }

    public class AvailableVm : AvailableVmBase
    {
        [Required]
        public string AuthorizationCode { get; set; }

        public string DisplayQuantity
        {
            get
            {

                long qtyProvisioned = 0;

                if (VmTotalCount >= VmUseCount)
                {
                    qtyProvisioned = VmTotalCount - VmUseCount;
                }

                return IsNsx == 0 || IsPerpetualNsx == 1 || IsVmElaAuthCode
                    ? qtyProvisioned + "/" + VmTotalCount
                    : qtyProvisioned.ToString();
            }
        }

        public string TotalAvailable
        {
            get
            {
                long available = 0;
                if (VmTotalCount >= VmUseCount)
                {
                    available = VmTotalCount - VmUseCount;
                }

                return available.ToString();
            }
        }
    }

	public abstract class AvailableVmBase 
    {
		public int VmUseCount { get; set; }

		public string Description { get; set; }

        public int VmAllocated { get; set; }

		public long VmTotalCount { get; set; }

		public int IsNsx { get; set; }

		public int IsPerpetualNsx { get; set; }

        public string ProductSku { get; set; }

		public int AllowGeneratingPanorama { get; set; }

        public bool IsPerpetualVm { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public bool IsEval { get; set; }

        public bool IsVmRenewalEligible { get; set; }

        public bool IsVmElaAuthCode { get; set; }
        
        public string AscName { get; set; }

        public string DisplayExpirationDate
        {
            get
            {
                if (IsPerpetualVm)
                {
                    return "Perpetual";
                }
                else
                {
                    if (ExpirationDate.HasValue)
                    {
                        return ExpirationDate.Value.ToShortDateString();
                    }
                    else
                    {
                        return IsEval ? "N/A" : string.Empty;
                    }
                }
            }
        }
    }

	public class AvailableVmCalc {

		public string AuthorizationCode { get; set; }
		public string QuantityProvisioned {
			get {
				long quantityProvisioned = 0;
				if (VmTotalCount >= VmUseCount) {
					quantityProvisioned = VmTotalCount - VmUseCount;
				}
				if (IsNsx == false || IsPerpetualNsx == true)
					return quantityProvisioned + "/" + VmTotalCount;
				else
					return quantityProvisioned.ToString();
			}
		}

		public int VmUseCount { get; set; }
		public long VmTotalCount { get; set; }
		public bool IsNsx { get; set; }
		public bool IsPerpetualNsx { get; set; }
		public string PartDescription { get; set; }
	}

	public class AvailableVmCsv {
		[CsvColumn(FieldIndex = 1, Name = "Authorization Code")]
		public string AuthorizationCode { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Quantity Provisioned")]
		public string QuantityProvisioned { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Part Description")]
		public string PartDescription { get; set; }

	    [CsvColumn(FieldIndex = 4, Name = "Child Auth Code")]
	    public string ChildAuthCode { get; set; }

	    [CsvColumn(FieldIndex = 5, Name = "Serial Number")]
	    public string ChildSerialNumber { get; set; }
	}

    public class AvailableCnCsv
    {
        // CN
        [CsvColumn(FieldIndex = 1, Name = "CN Series auth code")]
        public string AuthorizationCode { get; set; }
        [CsvColumn(FieldIndex = 2, Name = "CN Series Part Number")]
        public string ProductSku { get; set; }
        //[CsvColumn(FieldIndex = 0, Name = "Authorization Code")]
        //public string Description { get; set; }
        [CsvColumn(FieldIndex = 3, Name = "Total Available")]
        public string TotalAvailable { get; set; }
        //[CsvColumn(FieldIndex = 0, Name = "ASC")]
        //public string AscName { get; set; }
        [CsvColumn(FieldIndex = 4, Name = "CN Series auth code expiration date")]
        public string ExpirationDate { get; set; }
        // Panorama
        [CsvColumn(FieldIndex = 5, Name = "Panorama Serial Number")]
        public string SerialNumber { get; set; }
        [CsvColumn(FieldIndex = 6, Name = "Panorama Part Number")]
        public string PanoramaProductSku { get; set; }
        [CsvColumn(FieldIndex = 7, Name = "Num. Licenses")]
        public string ProvisionedAndTotal { get; set; }
        //[CsvColumn(FieldIndex = 8, Name = "Panorama ")]
        //public string MonthlyPeakUsage { get; set; }

        public AvailableCnCsv()
        {
        }

        public AvailableCnCsv(AvailableVirtualSeries<AvailableCn> cn, AvailableCn panorama)
        {
            // CN part
            AuthorizationCode = cn.AuthorizationCode;
            ProductSku = cn.ProductSku;
            //Description = cn.Description;
            TotalAvailable = cn.VmUseCount.ToString();
            //AscName = cn.AscName;
            ExpirationDate = cn.DisplayExpirationDate;
            // Panorama part
            SerialNumber = panorama.SerialNumber;
            PanoramaProductSku = panorama.ProductSku;
            ProvisionedAndTotal = panorama.ProvisionedAndTotal;
            //MonthlyPeakUsage = panorama.MonthlyPeakUsage;
        }
    }

	public class DeviceCategoryItem : BaseCspModel {

		[DataAccess(DeviceCategoryFields.DeviceCategoryId, DataType = DbType.Int32)]
		public int DeviceCategoryId { get; set; }

		[DataAccess(DeviceCategoryFields.DeviceCategoryName, DataType = DbType.Int64)]
		public string DeviceCategoryName { get; set; }
	}

	public class RegisterVmResult {
		public string ErrorMessage { get; set; }
		public long NewDeviceId { get; set; }
	}

	public class ParseVmFileResult {
		public string Message { get; set; }
		public string CpuId { get; set; }
		public string UuId { get; set; }
	}

    public class ParseVMRequest
    {
        public string fileContent { get; set; }
    }

	public class AddVmAuthCodeResult {

		public AddVmAuthCodeResult() {

		}

		public AddVmAuthCodeResult(string errorMessage) {
			ErrorMessage = errorMessage;
		}

		public string ErrorMessage { get; set; }
        public string AuthCode { get; set; }
	}

	public class PraProvisioningRec : BaseCspModel {
		public long DeviceId { get; set; }
		public string SerialNumber { get; set; }
		public string PartNumber { get; set; }
		public string License { get; set; }
		public string AuthCode { get; set; }
		public string Expiration { get; set; }
	}

    public class PrelicenseResults
    {
        public int PrelicenseRequestId { get; set; }
        public bool IsAsync { get; set; }
        public bool HasError { get; set; }
        public string BatchId { get; set; }
    }

}