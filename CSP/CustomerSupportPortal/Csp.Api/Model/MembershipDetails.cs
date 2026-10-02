using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class MembershipDetails
    {
        public long MembershipId { get; set; }
        public string CompanyName { get; set; }
        public long UserAccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
        public long SupportAccountId { get; set; }
        public string SalesforceAccountId { get; set; }
        public string SalesforceContactId { get; set; }
        public bool IsGroupUser { get; set; }
        public bool HasMultipleAccounts { get; set; }
        public int ImpersonateSetting { get; set; }
    }
}
