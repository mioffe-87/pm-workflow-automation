using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Csp.Api.Model.PartialShipment;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IPartialShipmentDataAccess
    {
        void LogApiCalls(long supportAccountId, PartialShipmentRequestType requestType, string api, string request, string response, long userAccountId);
        MasterOrder GetMasterOrderDetail(string masterOrderNumber);
        IList<MasterOrder> GetMasterOrders(long supportAccountId);
        IList<ShippingOrder> GetShipmentHistory(long supportAccountId);
        long SavePartialShipmentOrder(PartialShipmentOrder partialShipmentOrder, long supportAccountId);
        OrderContractDetail ToOrderContractDetail(MasterContractDetail masterContractDetail,bool isDataCenter);
        PartialShipmentOrderDetail GetPartialShipmentOrderForResubmit(MasterContractDetail masterContractDetail,string referenceId);
        long SaveAddress(LocationDetail address, long supportAccountId, long userAccountId);
        IList<LocationDetail> GetAddresses(long supportAccountId);        
        IList<ShippingMethodResult> GetShippingMethodList(bool isDomestic);
        bool IsDevice(string partNumber);


    }
}
