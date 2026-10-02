using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    public class PartialShipmentOrderResult
    {
        public long? PartialShipmentId { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }  
    }
}
