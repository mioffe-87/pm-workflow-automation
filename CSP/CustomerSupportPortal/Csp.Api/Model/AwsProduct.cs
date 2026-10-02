using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class AwsProduct : AuditCspModel
    {
        public string ProductName { get; set; }
        public string ProductId { get; set; }
        public string ProductCode { get; set; }
        public string PartId { get; set; }
    }
}
