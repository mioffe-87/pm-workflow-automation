using System;
using Newtonsoft.Json;

namespace Csp.Api.Model.PartialShipment
{
    public class ShippingOrder
    {
        public string ReferenceId { get; set; }

        public string SapOrderNumber { get; set; }

        public string MasterContract { get; set; }

        public DateTime DateOrdered { get; set; }

        public long DateOrderedTicks { get; set; }

        [JsonIgnore]
        public string ContactInfo { get; set; }

        public string Contact { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public LocationDetail ShipToLocation { get; set; }
    }

    public class ShippingOrderContact
    {
        public ContactDetails ContactDetails { get; set; }

        public LocationDetail LocationDetails { get; set; }

    }

    public class ContactDetails
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Company { get; set; }
    }
}
