using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class DisableDeviceRegistrationPinModel
    {
        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string Ids { get; set; }
    }
}
