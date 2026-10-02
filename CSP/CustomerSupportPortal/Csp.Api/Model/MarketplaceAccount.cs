using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class MarketplaceAccount {
        public string Email { get; set; }
        public string SalesforceUsername { get; set; }
        public bool AccountStatus { get; set; }
        public string SalesforceAccountId { get; set; }
        public string SalesforceAccountName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDt { get; set; }
    }
}
