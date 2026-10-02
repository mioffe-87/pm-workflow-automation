using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspInfo
    {
        public long SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public int CsspDataId { get; set; }
        public string DistiName { get; set; }
        public string DistiSfdcAccountId { get; set; }
        public string AscName { get; set; }
        public string AscSfdcAccountId { get; set; }
        public bool CanGeneratePrelicenseKeys { get; set; }
    }
}
