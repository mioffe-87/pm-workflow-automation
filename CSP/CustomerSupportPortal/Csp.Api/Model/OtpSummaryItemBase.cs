using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class OtpSummaryItemBase
    {
        public long Id { get; set; }

        public string ExpirationDate { get; set; }
        
        public long ExpirationDateTics { get; set; }

        public string Requestor { get; set; }

        public string CreatedDate { get; set; }
        
        public long CreatedDateTicks { get; set; }

        public bool IsError { get; set; }

        public bool IsExpired { get; set; }

        public bool IsHistory { get; set; }
    }
}
