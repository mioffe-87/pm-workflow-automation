using Csp.Api.HelperClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.PartialShipment
{

    public class BaseContractDetail
    {
        public string MasterOrderNumber { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public string ShippingType { get; set; }

        public string ParentOrderNumber { get; set; }

        public string PanwQuote { get; set; }

        public string AdminEmail { get; set; }

        public string ContactDetailName { get; set; }

        public string AuthEmail { get; set; }

        public string OrderSummaryEmail { get; set; }

        public LocationDetail LocationDetail { get; set; }
    }

    public class MasterContractDetail : BaseContractDetail
    {
        public List<MasterContractItem> Items { get; set; }
    }


    public class OrderContractDetail : BaseContractDetail
    {

        public List<OrderContractItem> Devices { get; set; }
        public List<OrderContract> Supports { get; set; }
        public List<OrderContract> Subscriptions { get; set; }
        public List<OrderContractItem> AccessoryAndOtherAddOns { get; set; }
        public List<string> IONExcludedSkus { get; set; }
        //public string EstimatedShipDate { get; set; }
    }   
    public class PartialShipmentOrderDetail : OrderContractDetail
    {
        public ContactInformation ContactDetail { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
    }

    public class OrderContract
    {
        public string Category { get; set; }
        public int QuantityNeeded { get; set; }
        public List<OrderContractItem> Items { get; set; }
    }

    public static class OrderContractItemExtension
    {

        public static OrderContractItem ToOrderContractItem(this MasterContractItem masterContractItem, List<OrderContractItem> itemDetails = null)
        {
            var ship = GetSelectedItemQuantity(itemDetails, masterContractItem.ItemNumber, masterContractItem.Sku);
            var orderContractItem = new OrderContractItem
            {
                ItemNumber = masterContractItem.ItemNumber,
                ItemDescription = masterContractItem.ItemDescription,
                Sku = masterContractItem.Sku,
                OrderedQty = ApiCommon.IsNumericFromTryParse(masterContractItem.OrderedQty) ? (int)Convert.ToDouble(masterContractItem.OrderedQty.Trim(), CultureInfo.InvariantCulture.NumberFormat) : 0,
                FulfilledQty = ApiCommon.IsNumericFromTryParse(masterContractItem.FulfilledQty) ? (int)Convert.ToDouble(masterContractItem.FulfilledQty.Trim(), CultureInfo.InvariantCulture.NumberFormat) : 0,
                OpenQty = ApiCommon.IsNumericFromTryParse(masterContractItem.OpenQty) ? (int)Convert.ToDouble(masterContractItem.OpenQty.Trim(), CultureInfo.InvariantCulture.NumberFormat) : 0,
                Ship = ship
            };

            if (masterContractItem.AddtlData != null && masterContractItem.AddtlData.Count > 0)
            {
                var additionalDatawithCity = masterContractItem.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZLOC.ToString());
                var additionalDatawithZip = masterContractItem.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZZIP.ToString());
                var additionalProvince = masterContractItem.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZRGN.ToString());
                var additionalCountry = masterContractItem.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZCNT.ToString());
                if (additionalDatawithCity != null)
                {
                    orderContractItem.City = additionalDatawithCity.Value1;
                }

                if (additionalDatawithZip != null)
                {
                    orderContractItem.Zip = additionalDatawithZip.Value1;
                }

                if (additionalProvince != null)
                {
                    orderContractItem.Province = additionalProvince.Value1;
                }
                if (additionalCountry != null)
                {
                    orderContractItem.Country = additionalCountry.Value1;
                }
            }

            return orderContractItem;
        }

        private static int? GetSelectedItemQuantity(List<OrderContractItem> itemDetails, int itemNumber, string sku)
        {
            int? selectedQty = null;
            foreach (var item in itemDetails)
            {
                if (item.ItemNumber == itemNumber && item.Sku == sku)
                {
                    selectedQty = item.Ship;
                    break;
                }
            }
            return selectedQty;
        }

    }
   
}
