using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {
      public class ReleasedLicenseItem {

        public string AuthorizationCode { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
