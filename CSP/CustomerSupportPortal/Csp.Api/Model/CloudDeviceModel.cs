using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CloudType
    {
        public int CloudTypeId { get; set; }
        public string Name { get; set; }
    }

    public class CloudRegion
    {
        public int CloudRegionId {get;set;}
        public string RegionCode { get; set; }
        public string DisplayName { get; set; }
        public int? CloudTypeId { get; set; } 
    }

    public class CloudProductType
    {
        public int CloudProductLookupId { get; set; }
        public string ProductCode { get; set; }
        public string ProductId { get; set; }
        public string DisplayName { get; set; }
        public int? CloudTypeId { get; set; } 
    }
}
