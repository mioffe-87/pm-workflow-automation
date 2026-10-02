using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class RegistrationPinModel
    {
        public long SupportAccountId { get; set; }

        public string RegistrationPinUniqueId { get; set; }

        public string RegistrationPin { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public long CreatedBy { get; set; }

    }
}
