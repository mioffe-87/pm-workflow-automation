using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class RequestReturnDto : DefaultDto
    {
        public List<RmaDeviceDto> Devices { get; set; }
        public IEnumerable<CountryDto> Countries { get; set; }
        public IEnumerable<RegionDto> Regions { get; set; }
    }

}