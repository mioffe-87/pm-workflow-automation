using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class RegionDto
    {
        public long Id { get; set; }
        public long CountryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}