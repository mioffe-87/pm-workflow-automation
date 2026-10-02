using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.PartialShipment;

namespace Csp.Api.Services.Contracts
{
    public interface IPartialShipmentLogic
    {
        MasterContractDetail GetMasterContractDetails(string masterOrderNumber, long userAccountId);
        IList<MasterContract> GetMasterContractList(long supportAccountId, long userAccountId);
        IList<ShippingOrder> GetShipmentHistory(long supportAccountId);
        ShippingOrderDetail GetShippingOrderDetail(string sapOrderNumber, long supportAccountId, long userAccountId);
        PartialShipmentOrderResult CreatePartialShipmentOrder(PartialShipmentOrder partialShipmentOrder, long supportAccountId, long userAccountId);
        IList<LocationDetail> GetAddresses(long supportAccountId);
        IList<ShippingMethodResult> GetShippingMethodList(bool isDomestic);
        OrderContractDetail GetOrderContractDetails(string masterOrderNumber, long userAccountId, bool isDataCenter);
        PartialShipmentOrderDetail GetPartialShipmentOrderForResubmit(string referenceId, string masterOrderNumber, long userAccountId);
    }
}
