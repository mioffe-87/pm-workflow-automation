using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class VmElaTokenModel
    {
        public string GrandParentAuthCode { get; set; }
        public long TotalTokens { get; set; }
        public long AllocatedTokens { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int TermMonths { get; set; }
        public DateTime? AdditionalTokenAppliedDate { get; set; }

        //support account Id, Name
        public Dictionary<long, string> GrantedAccounts { get; set; }

        public IEnumerable<VmElaChildModelInfo> CurrentAccountModelsInfo { get; set; }

        public VmElaTokenModel()
        {
            CurrentAccountModelsInfo = new List<VmElaChildModelInfo>();
        }
    }

    public class VmElaChildModelInfo
    {
        public long LicenseId { get; set; }
        public string Model { get; set; }
        public long AllocatedQuantity { get; set; }
        public long ProvisionedQuantity { get; set; }
        public long Tokens { get; set; }
        public int ExchangeRate { get; set; }
    }
}
