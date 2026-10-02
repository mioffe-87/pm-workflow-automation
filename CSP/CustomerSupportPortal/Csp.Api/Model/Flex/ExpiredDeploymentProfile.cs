using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class ExpiredDeploymentProfile
    {
        public string AuthorizationCode { get; set; }
        public string FirewallType { get; set; }
        public long CreditsAllocated { get; set; }
        public long CreditsConsumed { get; set; }
        public long TokenId { get; set; }
        public string Subscriptions { get; set; }
        public int FirewallsCreated { get; set; }
        public int FirewallsProjected { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ProfileStatus { get; set; }
        public string CreatedBy { get; set; }
    }
}
