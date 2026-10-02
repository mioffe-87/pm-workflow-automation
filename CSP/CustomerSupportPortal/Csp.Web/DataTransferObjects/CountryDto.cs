using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class CountryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}