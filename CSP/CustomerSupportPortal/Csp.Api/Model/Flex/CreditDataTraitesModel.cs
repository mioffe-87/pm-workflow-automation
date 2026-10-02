using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDataTraitsModel
    {
        public bool HasSaasCloud { get; set; }

        public bool HasCloudNgfw { get; set; }
    }
}
