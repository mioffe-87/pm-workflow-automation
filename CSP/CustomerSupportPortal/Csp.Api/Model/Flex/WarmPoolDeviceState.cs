using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class WarmPoolDeviceState
    {
        public long Id { get; set; }

        public long DeviceId { get; set; }

        public string SerialNumber { get; set; }

        public string TargetState { get; set; }
    }
}
