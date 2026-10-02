using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    public class PartialShipmentOrder
    {
        public long PartialShipmentId { get; set; }
        public long ParentOrderNumber { get; set; }
        public string SapOrderNumber { get; set; }

        public string CrudType { get; set; }

        //ToDo: Should we use Status Enum?
        public string Status { get; set; }
        public string Message { get; set; }
        public string ReferenceId { get; set; }

        //public List<OrderContract> ItemDetails { get; set; }
        public List<OrderContractItem> Devices { get; set; }
        public List<OrderContract> Supports { get; set; }
        public List<OrderContract> Subscription { get; set; }        
        public List<OrderContractItem> AddOns { get; set; }

        public ContactInformation ContactDetail { get; set; }
        public LocationDetail LocationDetail { get; set; }

        public long? PartialShipmentAddressId { get; set; }

        public bool IsAddressSaveRequired { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public ShippingDetail ShippingDetail { get; set; }

        public List<OrderContractItem> ItemDetails { get; set; }

        public DateTime? CreatedOn { get; set; }
        public long? CreatedById { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedById { get; set; }
    }

    public class LocationDetail : Address
    {
        public string AddressSelected { get; set; }
        public string RegionName { get; set; }
        public string CountryCode { get; set; }
    }

    public class OrderDetail
    {
        public string AuthCodeEmail { get; set; }
        public string OrderSummaryEmail { get; set; }
        public string ReferenceNumber { get; set; }
        public string Note { get; set; }
        public string NoteSelected { get; set; }
    }

    public class ShippingDetail
    {
        //ToDo: Do we need enum to support types of shipping Method.
        public string ShippingMethod { get; set; }
        public bool IsDataCenter { get; set; }      
        public string BillingAddress { get; set; }
        public string BillingContactName{ get; set; }
        public string BillingContactPhone { get; set; }
        public string CarrierAccountNumber { get; set; }
        public string Carrier { get; set; }
        public string ServiceLevel { get; set; }

    }

    public class ShipmentOrderDetail
    {
        public ContactInformation ContactDetail { get; set; }
        public LocationDetail LocationDetail { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
        public List<OrderContractItem> Device { get; set; }
        public List<OrderContractItem> Supports { get; set; }
        public List<OrderContractItem> SubscriptionOrAddOns { get; set; }
    }

    //ToDo: We need to supply all possible Statuses.
    public enum Status
    {
        Open
    }
}