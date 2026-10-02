using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

    public class ActivationLinkModel : BaseCspModel
    {
        public long LicenseId { set; get; }

        public string AuthorizationCode { set; get; }
        
        public DateTime? LicenseExpiration { set; get; }
        
        public string LinkType { set; get; }
        
        public string ActivationLink { set; get; }
        
        public DateTime? ActivationLinkExpiration { set; get; }
	}
}
