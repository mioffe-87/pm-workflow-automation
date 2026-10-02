using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class VmUpgrade
    {
        public long DeviceId { get; set; }
        public string TransactionId { get; set; }
        public string FromModel { get; set; }
        public string ToModel { get; set; }
        public string OriginalAuthCode { get; set; }
        public string UpgradedAuthCode { get; set; }
        public string  VmUpgradeStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
