using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class DeactiveVmDto
    {
        public string ParentAuthCode { get; set; }
        public IEnumerable<VmItemToDeactivate> VmItemsToDeactivate { get; set; }

        public DeactiveVmDto()
        {
            VmItemsToDeactivate = new List<VmItemToDeactivate>();
        }
    }
}