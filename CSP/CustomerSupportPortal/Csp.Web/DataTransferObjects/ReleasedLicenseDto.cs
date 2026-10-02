using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;



namespace Csp.Web.DataTransferObjects {
    
    public class ReleasedLicenseDto : DefaultDto {

        public long? SupportAccountId { get; set; }
        public IEnumerable<ReleasedLicenseItem> AvailableLicenses { get; set; }
    }

}