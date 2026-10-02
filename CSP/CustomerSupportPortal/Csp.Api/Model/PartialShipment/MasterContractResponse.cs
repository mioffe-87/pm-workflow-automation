using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Csp.Api.Model.PartialShipment;

namespace Csp.Api.Model
{
    public class MasterContractResponse
    {
        [JsonProperty("Header")]
        public List<MasterContractResponseHeader> Headers { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Messages")]
        public Message Messages { get; set; }
    }


    public class MasterContractResponseHeader
    {
        [JsonProperty("DocumentNum")]
        public string MasterContract { get; set; }

        [JsonProperty("ReferenceQuote")]
        public string PanwQuote { get; set; }

        [JsonProperty("ShippingType")]
        public string ShippingType { get; set; }

        [JsonProperty("FreightTerms")]
        public string FreightTerms { get; set; }

        [JsonProperty("PONum")]
        public string ParentPurchaseOrder { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Partners")]
        public List<Partner> Partner { get; set; }

        [JsonProperty("Lines")]
        public List<MasterContractItem> Items { get; set; }

        [JsonProperty("Texts")]
        public List<MasterContractText> MasterContractTexts { get; set; }

    }

    public class Message
    {
        public string Text { get; set; }

        public string Code { get; set; }
    }

    public class MasterContractItem
    {
        
        [JsonProperty("ItemNum")]
        public int ItemNumber { get; set; }

        [JsonProperty("ItemDescription")]
        public string ItemDescription { get; set; }

        [JsonIgnore]
        public string ProductLine { get; set; }

        [JsonProperty("Material")]
        public string Sku { get; set; }

        [JsonProperty("OrderedQty")]
        public string OrderedQty { get; set; }

        [JsonProperty("FulfilledQty")]
        public string FulfilledQty { get; set; }

        [JsonProperty("OpenQty")]
        public string OpenQty { get; set; }

        public string UOM { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("Zip")]
        public string Zip { get; set; }

        [JsonProperty("Region")]
        public string Region { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        public List<RefData> AddtlData { get; set; }  
     }


    public class OrderContractItem
    {      
        public int ItemNumber { get; set; }               
        public string ItemDescription { get; set; }     
        public string Sku { get; set; }        
        public int OrderedQty { get; set; }      
        public int FulfilledQty { get; set; }       
        public int OpenQty { get; set; }      
        public string Category { get; set; }      
        public int UniqueId { get; set; }
        public int? Ship { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
    }
     
    public class Partner
    {
        [JsonProperty("PartnerNum")]
        public string PartnerNumber { get; set; }

        [JsonProperty("PartnerType")]
        public string PartnerType { get; set; }

        [JsonProperty("RefNum")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Street")]
        public string Street { get; set; }

        [JsonProperty("Street2")]
        public string Street2 { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Zip")]
        public string Zip { get; set; }
        [JsonProperty("Country")]
        public string Country { get; set; }
}

    public class MasterContractText
    {
        public string TextId { get; set; }
        public string TextLine { get; set; }
    }

    public enum LOCATION{
        ZLOC,
        ZZIP,
        ZRGN,
        ZCNT
    }

}
