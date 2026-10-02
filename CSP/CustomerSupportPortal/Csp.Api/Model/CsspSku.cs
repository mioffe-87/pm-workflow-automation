using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspSku
    {
        public int CsspSkuId { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public int CsspModelId { get; set; }
        public string Model { get; set; }
        public int CsspSupportTypeId { get; set; }
        public string SupportType { get; set; }
        public int CsspTermId { get; set; }
        public string Term { get; set; }
    }
}
