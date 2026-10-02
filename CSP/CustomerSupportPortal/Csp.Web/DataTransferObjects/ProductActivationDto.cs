using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ProductActivationDto
    {
        public List<ProductActivationItem> Activated { get; set; }
        public List<ProductActivationItem> ReadyForActivation { get; set; }
        public int TotalReadyForActivation { get; set; }
        public int TotalActivated { get; set; }
    }
}