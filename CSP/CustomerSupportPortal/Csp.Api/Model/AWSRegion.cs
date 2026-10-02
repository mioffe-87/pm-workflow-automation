using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class AwsRegion : AuditCspModel
    {
        public string ZoneOrRegion { get; set; }

        public string ZoneOrRegionText { get; set; }

    }
}
