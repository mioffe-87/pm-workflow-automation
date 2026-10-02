using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

    public class ActivationLinksModel : BaseCspModel
    {
        public ActivationLinkModel AggregatedLink { set; get; }

        public int Total { set; get; }
	}
}
