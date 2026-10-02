using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class OtpPinSummaryItem : OtpSummaryItemBase
    {
        public string UniqueId { get; set; }

        public string Pin { get; set; }

        public int UseCount { get; set; }

        public string Description { get; set; }
    }
}
