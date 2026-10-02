using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class BannerMessage
    {
        public string MessageHeader { get; set; }
        public string MessageDetails { get; set; }
        public bool IsActive { get; set; }
    }
}
