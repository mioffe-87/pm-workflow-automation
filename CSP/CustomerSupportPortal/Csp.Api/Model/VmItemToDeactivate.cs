using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class VmItemToDeactivate
    {
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
    }
}
