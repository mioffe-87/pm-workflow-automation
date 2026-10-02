using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspProductSelectorDto : DefaultDto
    {
        public IList<CsspModel> CsspModels { get; set; }
        public IList<CsspSupportType> CsspSupportTypes { get; set; }
        public IList<CsspTerm> CsspTerms { get; set; }

        public IEnumerable<CsspSku> CsspSkus { get; set; }

    }
}