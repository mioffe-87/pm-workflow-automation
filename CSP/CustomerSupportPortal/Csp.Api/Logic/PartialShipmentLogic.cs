using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.PartialShipment;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Csp.Api.Logic
{
    public class PartialShipmentLogic : IPartialShipmentLogic
    {
        #region Constants

        private readonly string _thumbprint = ConfigurationManager.AppSettings["PartialShipmentThumbprint"];
        private readonly string _apiUrl = ConfigurationManager.AppSettings["PartialShipmentApiUrl"];
        private const string AdminEmailTextId = "ZAAE";
        private const string OrderSummaryEmailTextId = "Z040";
        private const int  TextLineLength = 70;

        #endregion

        private readonly IPartialShipmentDataAccess _partialShipmentDataAccess;

        public PartialShipmentLogic(IPartialShipmentDataAccess partialShipmentDataAccess)
        {
            _partialShipmentDataAccess = partialShipmentDataAccess;
        }


        public MasterContractDetail GetMasterContractDetails(string masterOrderNumber, long userAccountId)
        {
            var masterContractDetail = new MasterContractDetail();
            //Get the details from DB to prepare the request payload
            var masterOrder = _partialShipmentDataAccess.GetMasterOrderDetail(masterOrderNumber);
            if (masterOrder == null)
            {
                //TODO: Log the failure here
                return masterContractDetail;
            }

            var masterContractRequest = new MasterContractRequest
            {
                QueryHeader = new List<MasterContractRequestHeader>
                {
                    new MasterContractRequestHeader
                    {
                        MasterOrderNum = masterOrderNumber,
                        AdminEmail = masterOrder.AdminEmail,
                        SapSoldTo = masterOrder.SoldToSapId.HasValue ? masterOrder.SoldToSapId.ToString() : string.Empty,
                        SapEndUser = masterOrder.EndCustomerSapId.HasValue ? masterOrder.EndCustomerSapId.Value.ToString() : string.Empty,
                        QueryType = "DETAIL"
                    }
                }
            };

            var masterContractResponse = GetMasterContract(masterContractRequest, PartialShipmentRequestType.GetMasterContractDetails, masterOrder.SupportAccountId, userAccountId);

            //Build Master Contract Detail Response
            if (masterContractResponse != null && masterContractResponse.Headers!=null && masterContractResponse.Status!="Error")
            {
                var header = masterContractResponse.Headers.FirstOrDefault();
                if (header != null)
                {
                    var endCustomer = header.Partner.FirstOrDefault(p => p.PartnerType.Equals("Z1"));
                    masterContractDetail.MasterOrderNumber = masterOrderNumber;
                    masterContractDetail.CustomerName = endCustomer != null ? endCustomer.Name : string.Empty;
                    masterContractDetail.ParentOrderNumber = header.ParentPurchaseOrder;
                    masterContractDetail.Status = header.Status;
                    masterContractDetail.ShippingType = header.ShippingType;
                    masterContractDetail.Items = header.Items;
                    masterContractDetail.PanwQuote = header.PanwQuote;

                    var endCustomerWithWePartnerType = header.Partner.FirstOrDefault(p => p.PartnerType.Equals("WE"));
                    masterContractDetail.ContactDetailName = endCustomerWithWePartnerType != null ? endCustomerWithWePartnerType.Name : string.Empty;

                    masterContractDetail.LocationDetail = new LocationDetail();

                    if (endCustomerWithWePartnerType != null)
                    {
                        masterContractDetail.LocationDetail.Address1 = endCustomerWithWePartnerType.Street;
                        masterContractDetail.LocationDetail.Address2 = endCustomerWithWePartnerType.Street2;
                        masterContractDetail.LocationDetail.City = endCustomerWithWePartnerType.City;
                        masterContractDetail.LocationDetail.Region = endCustomerWithWePartnerType.State;
                        masterContractDetail.LocationDetail.PostalCode = endCustomerWithWePartnerType.Zip;
                        masterContractDetail.LocationDetail.CountryCode = endCustomerWithWePartnerType.Country;
                    }

                    //Populate AuthEmail property if there exists Text item  with "TextId" = "ZAAE".
                    if (header.MasterContractTexts != null && header.MasterContractTexts.Count > 0)
                    {
                        var adminEmailText = header.MasterContractTexts.FirstOrDefault(x => x.TextId.Equals(AdminEmailTextId));
                        if (adminEmailText != null)
                        {
                            masterContractDetail.AuthEmail = adminEmailText.TextLine;
                            masterContractDetail.AdminEmail = adminEmailText.TextLine;
                        }
                    }

                    //Populate OrderSummaryEmail property if there exists Text item  with "TextId" = "Z040".
                    if (header.MasterContractTexts != null && header.MasterContractTexts.Count > 0)
                    {
                        var orderSummaryEmailText = header.MasterContractTexts.FirstOrDefault(x => x.TextId.Equals(OrderSummaryEmailTextId));
                        if (orderSummaryEmailText != null)
                        {
                            masterContractDetail.OrderSummaryEmail = orderSummaryEmailText.TextLine;                            
                        }
                    }


                    if (header.Items != null && header.Items.Count > 0)
                    {
                        foreach (var item in header.Items)
                        {
                            if (item.AddtlData != null && item.AddtlData.Count > 0)
                            {
                                var itemCity = item.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZLOC.ToString());
                                var itemZip = item.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZZIP.ToString());
                                var itemRegion = item.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZRGN.ToString());
                                var itemCountry= item.AddtlData.FirstOrDefault(x => x.Qualifier == LOCATION.ZCNT.ToString());
                                if (itemCity != null)
                                {
                                    item.City = itemCity.Value1;
                                }
                                if (itemZip != null)
                                {
                                    item.Zip = itemZip.Value1;
                                }
                                if (itemRegion != null)
                                {
                                    item.Region = itemRegion.Value1;
                                }
                                if (itemCountry != null)
                                {
                                    item.Country = itemCountry.Value1;
                                }

                            }

                        }

                    }
                }
            }
            
            return masterContractDetail;
        }


        public IList<MasterContract> GetMasterContractList(long supportAccountId, long userAccountId)
        {
            var result = new List<MasterContract>();
            var masterOrders = _partialShipmentDataAccess.GetMasterOrders(supportAccountId);

            if (masterOrders.Any())
            {
                MasterContractRequest request = new MasterContractRequest { QueryHeader = new List<MasterContractRequestHeader>() };
                foreach (var order in masterOrders)
                {
                    request.QueryHeader.Add(new MasterContractRequestHeader
                    {
                        AdminEmail = order.AdminEmail,
                        MasterOrderNum = order.MasterOrderNumber,
                        SapSoldTo = order.SoldToSapId.HasValue ? order.SoldToSapId.Value.ToString() : string.Empty,
                        SapEndUser = order.EndCustomerSapId.HasValue ? order.EndCustomerSapId.Value.ToString() : string.Empty,
                        QueryType = MasterContractQueryType.List.ToString()
                    });

                }
                var masterContractResponse = GetMasterContract(request, PartialShipmentRequestType.GetMasterContractList, supportAccountId, userAccountId);
                if (masterContractResponse.Headers != null)
                    result = MergeMasterOrderListWithSapMasterContract(masterOrders, masterContractResponse);

            }

            return result;
        }

        public IList<ShippingOrder> GetShipmentHistory(long supportAccountId)
        {
            var shippingOrders = _partialShipmentDataAccess.GetShipmentHistory(supportAccountId);

            //Update Contact Info
            foreach (var order in shippingOrders)
            {
                try
                {
                    var contactInfo = JsonConvert.DeserializeObject<ShippingOrderContact>(order.ContactInfo,
                                        new JsonSerializerSettings
                                        {
                                            NullValueHandling = NullValueHandling.Ignore,
                                            MissingMemberHandling = MissingMemberHandling.Ignore
                                        });
                    order.Contact = contactInfo.ContactDetails != null ? contactInfo.ContactDetails.Email : string.Empty;
                    if (order.ShipToLocation == null)
                    {
                        order.ShipToLocation = contactInfo.LocationDetails;
                    }
                    else
                    {
                        order.ShipToLocation.RegionName = contactInfo.LocationDetails.RegionName;

                    }
                }
                catch (Exception)
                {
                    order.Contact = string.Empty;
                }
            }

            return shippingOrders;
        }

        public ShippingOrderDetail GetShippingOrderDetail(string sapOrderNumber, long supportAccountId, long userAccountId)
        {
            ShippingOrderDetail shippingOrderDetail = null;
            var url = string.Format("{0}/PPReport", _apiUrl);
            var request = new ShipmentDetailRequest
            {
                SAPOrderNumber = sapOrderNumber,
                Sender = "CSP"
            };
            var requestPayload = JsonConvert.SerializeObject(request);
            var responsePayload = InvokePartialShipmentApi<ShipmentDetailResponse>(url, "POST", requestPayload, PartialShipmentRequestType.ShippingOrderDetail, supportAccountId, userAccountId);

            if (responsePayload != null)
            {
                shippingOrderDetail = Mapper.Map<ShippingOrderDetail>(responsePayload);
                shippingOrderDetail.MasterOrderNumber = shippingOrderDetail.MasterOrderNumber.TrimStart('0');
                shippingOrderDetail.SAPOrderNumber = shippingOrderDetail.SAPOrderNumber.TrimStart('0');
                shippingOrderDetail.ItemsShipped = new List<ItemShipped>();
                //orderSummarEmail
                if (responsePayload.Text != null )
                {
                    var orderSummaryEmailText = responsePayload.Text.TextID.Equals(OrderSummaryEmailTextId);
                    if (orderSummaryEmailText)
                    {
                        shippingOrderDetail.OrderSummaryEmail = responsePayload.Text.Value;
                    }
                }
            
                foreach (var orderLine in responsePayload.OrderLines)
                {
                    if (orderLine.Details != null)
                    {
                        var itemQuantity = double.Parse(orderLine.Quantity) / orderLine.Details.Count;
                        orderLine.Details.ForEach(d =>
                        {
                            var itemShipped = new ItemShipped
                            {
                                Item = orderLine.Item,
                                Description = orderLine.Description,
                                SKU = orderLine.PartNumber,
                                Quantity = itemQuantity.ToString(),
                                SerialNumber = _partialShipmentDataAccess.IsDevice(orderLine.PartNumber)? d.SerialNumber:string.Empty,
                                AuthCode = _partialShipmentDataAccess.IsDevice(orderLine.PartNumber) ? string.Empty : d.AuthCode,
                                StartDate = d.StartDate,
                                EndDate = d.EndDate
                            };
                            shippingOrderDetail.ItemsShipped.Add(itemShipped);
                        });
                    }
                    else
                    {
                        var itemShipped = new ItemShipped
                        {
                            Item = orderLine.Item,
                            Description = orderLine.Description,
                            SKU = orderLine.PartNumber,
                            Quantity = orderLine.Quantity,
                        };
                        shippingOrderDetail.ItemsShipped.Add(itemShipped);
                    }


                }
            }

            return shippingOrderDetail;
        }

        public IList<LocationDetail> GetAddresses(long supportAccountId)
        {
            return _partialShipmentDataAccess.GetAddresses(supportAccountId);
        }
        public IList<ShippingMethodResult> GetShippingMethodList(bool isDomestic)
        {
            return _partialShipmentDataAccess.GetShippingMethodList(isDomestic);
        }

        public OrderContractDetail GetOrderContractDetails(string masterOrderNumber, long userAccountId,bool isDataCenter)
        {

            var masterContractDetail = this.GetMasterContractDetails(masterOrderNumber, userAccountId);
            return _partialShipmentDataAccess.ToOrderContractDetail(masterContractDetail,isDataCenter);
        }

        public PartialShipmentOrderDetail GetPartialShipmentOrderForResubmit(string referenceId, string masterOrderNumber, long userAccountId)
        {

            var masterContractDetail = this.GetMasterContractDetails(masterOrderNumber, userAccountId);
            return _partialShipmentDataAccess.GetPartialShipmentOrderForResubmit(masterContractDetail, referenceId);
        }

        public PartialShipmentOrderResult CreatePartialShipmentOrder(PartialShipmentOrder partialShipmentOrder, long supportAccountId, long userAccountId)
        {
            PartialShipmentOrderResult partialShipmentOrderResult = new PartialShipmentOrderResult();
            if (partialShipmentOrder.PartialShipmentAddressId == null && partialShipmentOrder.IsAddressSaveRequired)
            {
                partialShipmentOrder.PartialShipmentAddressId = _partialShipmentDataAccess.SaveAddress(partialShipmentOrder.LocationDetail, supportAccountId, userAccountId);
            }

            //Generate referenceId by reducing the string Length of a Guid to 24 without losing uniqueness.
            if (partialShipmentOrder.CrudType.ToLower() != "update")
            {
                partialShipmentOrder.ReferenceId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }

            partialShipmentOrder.CreatedOn = DateTime.Now;

            //ToDo: We need to create enum type to accomodate all possible statuses.
            partialShipmentOrder.Status = "Processing";

            partialShipmentOrder.PartialShipmentId = _partialShipmentDataAccess.SavePartialShipmentOrder(partialShipmentOrder, supportAccountId);
            if (partialShipmentOrder.PartialShipmentId != 0)
            {
              
                var orderContractItems = new List<OrderContractItem>();
                orderContractItems.AddRange(partialShipmentOrder.Devices);
                orderContractItems.AddRange(partialShipmentOrder.Subscription.SelectMany(s => s.Items));
                orderContractItems.AddRange(partialShipmentOrder.Supports.SelectMany(s => s.Items));
                orderContractItems.AddRange(partialShipmentOrder.AddOns);
                partialShipmentOrder.ItemDetails = orderContractItems;
                return SendPartialShipmentOrder(partialShipmentOrder, supportAccountId, userAccountId);
            }
            else
            {

                partialShipmentOrderResult = new PartialShipmentOrderResult
                {
                    IsSuccess = false,
                    //ToDo: Remove hardcoded error message.
                    ErrorMessage = "Cannot save partialShipmentOrder"
                };
                return partialShipmentOrderResult;
            }
        }

        #region Private Methods

        private PartialShipmentOrderResult SendPartialShipmentOrder(PartialShipmentOrder partialShipmentOrder, long supportAccountId, long userAccountId)
        {
            //Get the details from DB to prepare the request payload
            MasterOrder masterOrder = _partialShipmentDataAccess.GetMasterOrderDetail(partialShipmentOrder.ParentOrderNumber.ToString());
            var masterContractRequest = new MasterContractRequest
            {
                QueryHeader = new List<MasterContractRequestHeader>
                {
                    new MasterContractRequestHeader
                    {
                        MasterOrderNum = partialShipmentOrder.ParentOrderNumber.ToString(),
                        AdminEmail = masterOrder.AdminEmail,
                        SapSoldTo = masterOrder.SoldToSapId.HasValue ? masterOrder.SoldToSapId.ToString() : string.Empty,
                        SapEndUser = masterOrder.EndCustomerSapId.HasValue ? masterOrder.EndCustomerSapId.Value.ToString() : string.Empty,
                        QueryType = "DETAIL"
                    }
                }
            };

            var masterContractResponse = GetMasterContract(masterContractRequest, PartialShipmentRequestType.GetMasterContractDetails, masterOrder.SupportAccountId, userAccountId);


            string sapSoldTo = masterOrder.SoldToSapId.HasValue ? masterOrder.SoldToSapId.ToString() : "0";
            string sapEndUser = masterOrder.EndCustomerSapId.HasValue ? masterOrder.EndCustomerSapId.ToString() : "0";

            Model.Partner endCustomerWithWePartnerType = null;

            //Take out endCutomer Partner with "WE" Partner Type.
            if (masterContractResponse != null && masterContractResponse.Headers.Any())
            {
                var header = masterContractResponse.Headers.FirstOrDefault();
                if (header != null)
                {

                    endCustomerWithWePartnerType = header.Partner.FirstOrDefault(p => p.PartnerType.Equals("WE"));
                }
            }

           string textLineZ060= getZ060TextCompilation(partialShipmentOrder);

            SubOrderRequest request = new SubOrderRequest
            {
                Header = new List<Header>
                {
                   new Header
                   {
                       ShipType = partialShipmentOrder.ShippingDetail.ShippingMethod,
                       Ref1 = partialShipmentOrder.ReferenceId,
                       RefData = new List<RefData>{
                           new RefData{
                                Qualifier ="REQSHIPDATE",

                                 // ToDo: verify correctness of Value1 parameter.
                                 // Estimated delivery date should be today + 14 days.
                                 // Will this be applicable to this field?
                                 Value1 =DateTime.Now.AddDays(14).ToShortDateString()
                           },
                           new RefData{
                                Qualifier ="CONTNAME",
                                Value1 =partialShipmentOrder.ContactDetail.UserName
                           },
                           new RefData{
                                Qualifier ="CONTPHONE",
                                Value1 =partialShipmentOrder.ContactDetail.PhoneNumber
                           }
                       },
                       RefDoc = new List<RefDoc>
                       {
                           new RefDoc
                           {
                               DocNum = partialShipmentOrder.OrderDetail.ReferenceNumber,
                               Type = "CUSTOMERREF"
                           },
                            new RefDoc
                           {
                               DocNum = partialShipmentOrder.ParentOrderNumber.ToString(),
                               Type = "PARENTORDER"
                           }
                       },
                       Partners = new List<Model.PartialShipment.Partner>
                       {
                           new Model.PartialShipment.Partner
                           {
                               PartnerNum = sapSoldTo,
                               PartnerType = "AG"
                           },
                           new Model.PartialShipment.Partner
                           {
                              City = partialShipmentOrder.LocationDetail.City,
                              Country = partialShipmentOrder.LocationDetail.CountryCode,
                              Name = partialShipmentOrder.ContactDetail.Company,
                              Street2 = partialShipmentOrder.LocationDetail.Address2,
                              PartnerNum = endCustomerWithWePartnerType !=null ? endCustomerWithWePartnerType.PartnerNumber : string.Empty,
                              PartnerType = "WE",
                              RefNum = endCustomerWithWePartnerType !=null ? endCustomerWithWePartnerType.ReferenceNumber : string.Empty,
                              State= partialShipmentOrder.LocationDetail.Region,
                              Street = partialShipmentOrder.LocationDetail.Address1,
                              Zip = partialShipmentOrder.LocationDetail.PostalCode
                           },
                           new Model.PartialShipment.Partner
                           {
                               PartnerNum= sapEndUser,
                               PartnerType="Z1"
                           }
                       },
                       Texts = new List<Text>
                       {
                            new Text
                           {
                              TextId = "ZACC",
                              TextLine = new List<string> { partialShipmentOrder.ShippingDetail.CarrierAccountNumber }
                           },
                           new Text
                           {
                              TextId = "Z030",
                              TextLine = new List<string> {partialShipmentOrder.OrderDetail.AuthCodeEmail }
                           },
                           new Text
                           {
                              TextId = "Z040",
                              TextLine = new List<string> {partialShipmentOrder.OrderDetail.OrderSummaryEmail }
                           },
                           new Text
                           {
                              TextId = "Z060",
                              TextLine =  getTextLine(textLineZ060).ToList()
                           }
                       }
                   }
                },
                Lines = partialShipmentOrder.ItemDetails.Select(y =>
                      new Line
                      {
                          LineNum = y.ItemNumber.ToString(),
                          Material = new List<Material> { new Material { ID = y.Sku } },
                          Quantity = y.Ship.ToString()
                      }).ToList()
            };
            
            //This invocation is made asynchronous as we don't care about its result, and the control will proceed without blocking to this call.
            CreateSubOrder(request, PartialShipmentRequestType.GetMasterContractList, supportAccountId, userAccountId);

            return new PartialShipmentOrderResult
            {
                PartialShipmentId = partialShipmentOrder.PartialShipmentId,
                IsSuccess = true
            };
        }
        private IEnumerable<string> getTextLine(string textLine)
        {        
           
            for (int i = 0; i < textLine.Length; i += TextLineLength)
                yield return textLine.Substring(i, Math.Min(TextLineLength, textLine.Length - i));            
            
        }

        private string getZ060TextCompilation(PartialShipmentOrder partialShipmentOrder)
        {

            //Z060 Text Field compilation

            string textLineZ060 = (partialShipmentOrder.ShippingDetail.Carrier != null && partialShipmentOrder.ShippingDetail.ServiceLevel != null) ?
                               ((!string.IsNullOrEmpty(partialShipmentOrder.OrderDetail.NoteSelected) && !string.IsNullOrEmpty(partialShipmentOrder.OrderDetail.Note) ?
                                    (!string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingAddress) && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactName)
                                    && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactPhone) ?
                                   string.Concat(partialShipmentOrder.OrderDetail.NoteSelected, ": ", partialShipmentOrder.OrderDetail.Note,
                                   ", ", "Billing Address: ", partialShipmentOrder.ShippingDetail.BillingAddress, ", ", "Billing Contact Name: ", partialShipmentOrder.ShippingDetail.BillingContactName, ", ", "Billing Contact Phone: ", partialShipmentOrder.ShippingDetail.BillingContactPhone,
                                   ", ", "Carrier: ", partialShipmentOrder.ShippingDetail.Carrier, ", ", "Service Level: ", partialShipmentOrder.ShippingDetail.ServiceLevel)

                                   : string.Concat(partialShipmentOrder.OrderDetail.NoteSelected, ": ", partialShipmentOrder.OrderDetail.Note,
                                       ", ", "Carrier: ", partialShipmentOrder.ShippingDetail.Carrier, ", ", "Service Level: ", partialShipmentOrder.ShippingDetail.ServiceLevel)
                                       ) :

                                         (!string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingAddress) && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactName)
                                    && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactPhone) ?
                                   string.Concat("Billing Address: ", partialShipmentOrder.ShippingDetail.BillingAddress, ", ", "Billing Contact Name: ", partialShipmentOrder.ShippingDetail.BillingContactName, ", ", "Billing Contact Phone: ", partialShipmentOrder.ShippingDetail.BillingContactPhone,
                                   ", ", "Carrier: ", partialShipmentOrder.ShippingDetail.Carrier, ", ", "Service Level: ", partialShipmentOrder.ShippingDetail.ServiceLevel)

                                     : string.Concat("Carrier: ", partialShipmentOrder.ShippingDetail.Carrier, ", ", "Service Level: ", partialShipmentOrder.ShippingDetail.ServiceLevel))


                               )) :

                               ((!string.IsNullOrEmpty(partialShipmentOrder.OrderDetail.NoteSelected) && !string.IsNullOrEmpty(partialShipmentOrder.OrderDetail.Note) ?

                                ((!string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingAddress) && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactName)
                                    && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactPhone) ?
                               string.Concat(partialShipmentOrder.OrderDetail.NoteSelected, ": ", partialShipmentOrder.OrderDetail.Note,
                               ", ", "Billing Address: ", partialShipmentOrder.ShippingDetail.BillingAddress, ", ", "Billing Contact Name: ", partialShipmentOrder.ShippingDetail.BillingContactName, ", ", "Billing Contact Phone: ", partialShipmentOrder.ShippingDetail.BillingContactPhone)
                                : string.Concat(partialShipmentOrder.OrderDetail.NoteSelected, ": ", partialShipmentOrder.OrderDetail.Note)

                              )) :

                              ((!string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingAddress) && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactName)
                                    && !string.IsNullOrEmpty(partialShipmentOrder.ShippingDetail.BillingContactPhone) ?
                               string.Concat("Billing Address: ", partialShipmentOrder.ShippingDetail.BillingAddress, ", ", "Billing Contact Name: ", partialShipmentOrder.ShippingDetail.BillingContactName, ", ", "Billing Contact Phone: ", partialShipmentOrder.ShippingDetail.BillingContactPhone)
                                : string.Empty

                              ))
                              ));
            return textLineZ060;


        }
        private MasterContractResponse GetMasterContract(MasterContractRequest masterContractRequest, PartialShipmentRequestType requestType, long supportAccountId, long userAccountId)
        {
            var url = string.Format("{0}/CSPGetMasterPO", _apiUrl);
            var requestPayload = JsonConvert.SerializeObject(masterContractRequest);
            var masterContractResponse = InvokePartialShipmentApi<MasterContractResponse>(url, "POST", requestPayload, requestType, supportAccountId, userAccountId);

            return masterContractResponse;
        }

        private List<MasterContract> MergeMasterOrderListWithSapMasterContract(IList<MasterOrder> masterOrders, MasterContractResponse masterContractResponse)
        {
            var masterContractList = new List<MasterContract>();

            if (masterContractResponse != null)
            {
                masterContractList = (from m in masterOrders
                    join mc in masterContractResponse.Headers on m.MasterOrderNumber equals mc.MasterContract
                    select new MasterContract
                    {
                        MasterOrderId = m.Id,
                        MasterOrderNumber = m.MasterOrderNumber,
                        ParentOrderNumber = mc.ParentPurchaseOrder,
                        PanwQuote = mc.PanwQuote,
                        Status = mc.Status.Equals("Fully referenced", StringComparison.OrdinalIgnoreCase) ?
                            MasterContractStatus.Fulfilled.ToString() : MasterContractStatus.Open.ToString(),
                        AdminEmail = mc.MasterContractTexts.Where(t => t.TextId == AdminEmailTextId).Select(t => t.TextLine).FirstOrDefault()
                    }).ToList();
            }

            return masterContractList;
        }

        private async void CreateSubOrder(SubOrderRequest subOrderRequest, PartialShipmentRequestType requestType, long supportAccountId, long userAccountId)
        {
            var url = string.Format("{0}/CSPCreateSubOrder", _apiUrl);
            var requestPayload = JsonConvert.SerializeObject(subOrderRequest);
            //Assign object as a return type, as we don't care about the result;this is fire and forget.
            await Task.FromResult(InvokePartialShipmentApi<object>(url, "POST", requestPayload, requestType, supportAccountId, userAccountId));
        }

        private T InvokePartialShipmentApi<T>(string url, string requestMethod, string requestPayload, PartialShipmentRequestType requestType, long supportAccountId, long userAccountId)
        {
            try
            {
                var request = WebCommon.GetWebRequestWithClientCertificate(url, requestMethod, _thumbprint, requestPayload);
                var responsePayload = string.Empty;


                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            responsePayload = reader.ReadToEnd();
                        }
                    }
                }

                //Log response
                _partialShipmentDataAccess.LogApiCalls(supportAccountId, requestType, url, requestPayload, responsePayload, userAccountId);

                var responseObj = JsonConvert.DeserializeObject<T>(responsePayload,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                return responseObj;

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return default(T);
        }

        #endregion

    }
}
