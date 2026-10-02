using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CnAllocateAuthCodeTokenDetails
    {
        public List<string> CnxAuthCodes { get; set; }
        public int AvailableTokens { get; set; }
    }
}