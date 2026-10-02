using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class VmViewModelDto<T> : DefaultDto
        where T: class
    {
        public int? SearchDeviceCategoryId { get; set; }
        public long SupportAccountId { get; set; }
        public IEnumerable<T> AvailableVms { get; set; }
        public string DefaultDeviceCode { get; set; }
        public bool HasTokenGrantRequest { get; set; }
    }

    public class RegisterVmDto : DefaultDto
    {
        [Required]
        public string UuId { get; set; }
        [Required]
        public string CpuId { get; set; }
        [Required]
        public string AuthCode { get; set; }
        [DisplayName("OS Release"), Required]
        public string PanOsVersion { get; set; }
        [DisplayName("Virtual Platform"), Required]
        public string HyperVisorVersion { get; set; }
        public long? NewDeviceId { get; set; }

    }

    public class GenerateKeysDto : DefaultDto
    {
        public string AuthCode { get; set; }
        public int VmUseCount { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        public int? PreLicenseQuantity { get; set; }
        public bool IsPrelicenseRequestSuccess { get; set; }
        public string ResponseMessage { get; set; }
        public int PreLicenseRequestId { get; set; }
        public string BatchId { get; set; }
        public string RequestedBy { get; set; }
        public long SupportAccountId { get; set; }
        public bool IsAsync { get; set; }
        public string PrelicenseDeviceUrl { get; set; }

    }
}