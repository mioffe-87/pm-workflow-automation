using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class MembershipDetailForSfdcContactSync
    {
        public long MembershipId { get; set; }
        public long UserAccountId { get; set; }
        public long SupportAccountId { get; set; }
        public string SalesforceAccountId { get; set; }
        public string SalesforceContactId { get; set; }
        public string CompanyName { get; set; }
        public bool IsGroupUser { get; set; }
    }
}
