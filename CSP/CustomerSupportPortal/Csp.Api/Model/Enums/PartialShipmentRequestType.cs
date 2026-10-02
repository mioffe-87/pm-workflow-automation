using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum PartialShipmentRequestType
    {
        [Description("GET_MASTER_CONTRACT_LIST")]
        GetMasterContractList,

        [Description("GET_MASTER_CONTRACT_DETAILS")]
        GetMasterContractDetails,

        [Description("CREATE_PARTIAL_SHIPMENT")]
        CreatePartialShipment,

        [Description("GET_SHIPPING_ORDER_DETAIL")]
        ShippingOrderDetail,
    }
}
