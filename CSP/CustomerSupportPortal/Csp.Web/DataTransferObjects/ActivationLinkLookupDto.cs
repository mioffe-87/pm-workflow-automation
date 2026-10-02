using Csp.Api.Model;
using System.Collections.Generic;

namespace Csp.Web.DataTransferObjects
{
    public class ActivationLinkLookupDto : DefaultDto
    {
        public IEnumerable<ActivationLinksLookupItem> Items { set; get; }
    }
}