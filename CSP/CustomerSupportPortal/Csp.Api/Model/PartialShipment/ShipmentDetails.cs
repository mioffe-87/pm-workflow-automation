using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.PartialShipment
{
    public class ShipmentDetailRequest
    {
        [JsonProperty("sapDocumentNumber")]
        public string SAPOrderNumber { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; }
    }

    public class ShipmentDetailResponse
    {
        [JsonProperty("ParentDocumentNumber")]
        public string MasterOrderNumber { get; set; }

        [JsonProperty("sapDocumentNumber")]
        public string SAPOrderNumber { get; set; }

        [JsonProperty("cpqQuoteNumber")]
        public string CPQQuoteNumber { get; set; }

        [JsonProperty("poNumber")]
        public string PoNumber { get; set; }

        [JsonProperty("documentDate")]
        public string DocumentDate { get; set; }

        [JsonProperty("termsOfPayment")]
        public string TermsOfPayment { get; set; }

        [JsonProperty("termsOfDelivery")]
        public string TermsOfDelivery { get; set; }

        [JsonProperty("freightTerms")]
        public string FreightTerms { get; set; }

        [JsonProperty("billOfLading")]
        public string BillOfLading { get; set; }

        [JsonProperty("salesPerson")]
        public string SalesPerson { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("invoiceDate")]
        public string InvoiceDate { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("ascAccount")]
        public string ASC { get; set; }

        [JsonProperty("shipType")]
        public string ShipType { get; set; }

        [JsonProperty("soldTo")]
        public string SoldTo { get; set; }

        [JsonProperty("billTo")]
        public string BillTo { get; set; }

        [JsonProperty("endCustomer")]
        public string EndCustomer { get; set; }

        [JsonProperty("shipTo")]
        public string ShipTo { get; set; }

        [JsonProperty("reseller")]
        public string ReSeller { get; set; }

        [JsonProperty("authCodeEmail")]
        public string AuthCodeEmail { get; set; }       
        public string OrderSummaryEmail { get; set; }

        [JsonProperty("estimatedShipDate")]
        public string EstimatedShipDate { get; set; }

        [JsonProperty("shipDate")]
        public string ShipDate { get; set; }

        [JsonProperty("orderLines")]
        public List<LineItem> OrderLines { get; set; }

        [JsonProperty("text")]
        public ShippingDetailText Text { get; set; }
    }

    public class LineItem
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("itemdescription")]
        public string Description { get; set; }

        [JsonProperty("partNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("itemDetails")]
        public List<ItemDetail> Details { get; set; }
    }

    public class ItemDetail
    {
        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("sn")]
        public string SerialNumber { get; set; }

        [JsonProperty("contractStartDate")]
        public string StartDate { get; set; }

        [JsonProperty("contractEndDate")]
        public string EndDate { get; set; }
    }
    public class ShippingDetailText
    {
        [JsonProperty("TextID")]
        public string TextID { get; set; }
        [JsonProperty("Value")]
        public string Value { get; set; }
    }

}
