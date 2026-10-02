using System.Collections.Generic;

namespace Csp.Api.Model.LicenseManagement
{
    public class LicenseManagementDeactivationRequest
    {        
        public long UserAccountId { get; set; }
        public List<LicenseManagementDeactivationRequestItem> DeactivationItems{ get; set; }        
    }
}
