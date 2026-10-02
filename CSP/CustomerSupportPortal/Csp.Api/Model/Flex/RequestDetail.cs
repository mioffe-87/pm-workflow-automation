using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class RequestDetail
    {
        public string RequestUrl { get; set; }
        public string Credential { get; set; }
        public string JsonRequest { get; set; }
    }
}
