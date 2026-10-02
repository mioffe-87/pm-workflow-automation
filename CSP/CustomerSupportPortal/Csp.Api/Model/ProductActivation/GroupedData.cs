using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class GroupedData
    {
        public List<ProductActivationItem> Activated { get; set; }
        public List<ProductActivationItem> ReadyForActivation { get; set; }

        //public GroupedData MapToGroupedData(List<ActivationLinksLookupItem> activationLinks)
        //{

        //}
    }
}
