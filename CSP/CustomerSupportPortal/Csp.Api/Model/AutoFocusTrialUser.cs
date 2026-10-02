using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class AutoFocusTrialUser
    {
        public long AutoFocusTrialId { get; set; }

        public long? UserAccountId { get; set; }

        public long? SupportAccountId { get; set; }

        public DateTime ActivationDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public DateTime? RemovalDate { get; set; }

        public bool IsActive { get; set; }
    }
}
