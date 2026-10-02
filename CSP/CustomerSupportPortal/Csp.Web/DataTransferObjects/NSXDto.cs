using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Csp.Api.Model;


namespace Csp.Web.DataTransferObjects
{
    public class NsxDto : DefaultDto
    {
        public IEnumerable<PraProvisioningRec> Devices { get; set; }
        public string AuthCode { get; set; }
        public long SupportAccountId { get; set; }
        public string PartNumber { get; set; }
    }
}