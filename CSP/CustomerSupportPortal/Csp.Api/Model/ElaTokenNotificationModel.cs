using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class ElaTokenNotificationModel
    {
        public long UserAccountId { get; set; }

        public string UserEmail { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public long SupportAccountId { get; set; }

        public string SalesforceAccountId { get; set; }

        public long LicenseId { get; set; }

        public string AuthCode { get; set; }

        public int TokensConsumedPercent { get; set; }

        public List<string> InternalElaOps { get; set; }
    }
}
