using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class PanoramaAssociationRequest
    {
        public long SupportAccountId { get; set; }
        public long UserAccountId { get; set; }
        public string UserEmail { get; set; }
        public string PrimarySerialNumber { get; set; }
        public string SecondarySerialNumber { get; set; }
    }
}
