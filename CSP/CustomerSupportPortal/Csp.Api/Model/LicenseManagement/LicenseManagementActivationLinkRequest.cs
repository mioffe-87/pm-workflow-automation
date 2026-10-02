using System.Collections.Generic;

namespace Csp.Api.Model.LicenseManagement
{
    public class LicenseManagementActivationLinkRequest
    {        
        public long UserAccountId { get; set; }
        public List<string> AuthCodes { get; set; }
        public long SupportAccountId { get; set; }
    }
}
