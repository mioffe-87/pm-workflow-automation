using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IPartnerEvalLogic
    {
        List<DeviceDetails> AllInfoReturned(string serialNumber, string partnerLoginId);
        DeviceDetails AllInfo(string serialNumber, string partnerLoginId, long userAccountId);
    }

    public class DeviceDetails
    {
        public string SerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string DeviceTag { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? DeviceExpiration { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Error { get; set; }
        public long? SupportAccountId { get; set; }
        public string Info { get; set; }
        public long? DeviceCat { get; set; }
    }

    //public class LicenseDetails
    //{
    //    public string AuthCode { get; set; }
    //    public string LicenseType { get; set; }
    //    public string Description { get; set; }
    //    public DateTime RegistrationDate { get; set; }
    //    public DateTime ExpirationDate { get; set; }
    //}
}