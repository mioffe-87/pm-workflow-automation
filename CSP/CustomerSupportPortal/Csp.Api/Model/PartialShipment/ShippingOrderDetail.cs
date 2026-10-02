using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.PartialShipment
{
    public class ShippingOrderDetail
    {
        public string MasterOrderNumber { get; set; }

        public string SAPOrderNumber { get; set; }

        public string CPQQuoteNumber { get; set; }

        public string PoNumber { get; set; }

        public string DocumentDate { get; set; }

        public string TermsOfPayment { get; set; }

        public string TermsOfDelivery { get; set; }

        public string FreightTerms { get; set; }

        public string BillOfLading { get; set; }

        public string SalesPerson { get; set; }

        public string ContactName { get; set; }

        public string InvoiceDate { get; set; }

        public string InvoiceNumber { get; set; }

        public string ASC { get; set; }

        public string ShipType { get; set; }

        public string SoldTo { get; set; }

        public string BillTo { get; set; }

        public string EndCustomer { get; set; }
       
        public string ShipTo { get; set; }

        public string ReSeller { get; set; }

        public string AuthCodeEmail { get; set; }

        public string OrderSummaryEmail { get; set; }

        public string EstimatedShipDate { get; set; }
        
        public string ShipDate { get; set; }
       
        public List<ItemShipped> ItemsShipped { get; set; }
    }

    public class ItemShipped
    {
        public string Item { get; set; }

        public string Description { get; set; }

        public string SKU { get; set; }

        public string Quantity { get; set; }

        public string SerialNumber { get; set; }

        public string AuthCode { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }

    

}
