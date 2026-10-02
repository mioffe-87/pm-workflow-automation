using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CoveoConfigModel
    {
        public string EmailAddress { get; set; }
        public string TokenFilter { get; set; }
        public long SupportAccountId { get; set; }
    }
}
