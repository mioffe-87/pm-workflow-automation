using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{
    class SubOrderRequest
    {
        public List<Header> Header { get; set; }
        public List<Line> Lines { get; set; } 
    }
    public class Header
    {
        public string ShipType { get; set; } // ShippingDetail->ShippingMethod      
        public string Ref1 { get; set; } // PartialShipmentOrder-> ReferenceId
        public List<RefData> RefData { get; set; } 
        public List<RefDoc> RefDoc { get; set; }
        public List<Partner> Partners { get; set; }
        public List<Text> Texts { get; set; }
    }

    public class RefData
    {
        public string Qualifier { get; set; } // Qualifier = "REQSHIPDATE'
        public string Value1 { get; set; } //ShippingDetail-<ReferenceNumber
    }

    public class RefDoc
    {
        public string Type { get; set; } // Type = "CUSTOMERREF'
        public string DocNum { get; set; }  // OrderDetail -> ReferenceNumber
    }

    public class Partner
    {
        //for PartnerType=AG, 
        public string PartnerNum { get; set; } //???
        public string PartnerType { get; set; } // PartnerType = 'WE',
        public string Name { get; set; } // PartialShipmentOrder.ContactInfo.Contact.Name
        public string Street2 { get; set; } // ??? PartialShipmentOrder.ContactInfo.Location.Address2
        public string Street { get; set; }  // PartialShipmentOrder.ContactInfo.Location.Address1
        public string City { get; set; }  // PartialShipmentOrder.ContactInfo.Location.City
        public string State { get; set; } // PartialShipmentOrder.ContactInfo.Location.Region
        public string Zip { get; set; } // PartialShipmentOrder.ContactInfo.Location.PostalCode
        public string Country { get; set; } // PartialShipmentOrder.ContactInfo.Location.Country

        public string RefNum { get; set;}
    }

    public class Text
    {
        public string TextId { get; set; }
        public List<string> TextLine { get; set; }
    }

    public class Material
    {
        public string ID { get; set; }
    }

    public class Line
    {
        public string LineNum { get; set; }
        public List<Material> Material { get; set; }
        public string Quantity { get; set; }
    }
}
