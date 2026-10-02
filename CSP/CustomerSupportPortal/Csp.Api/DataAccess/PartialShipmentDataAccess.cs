using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Model.PartialShipment;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Csp.Api.Model.Csp.Api.Model;

namespace Csp.Api.DataAccess
{
    public class PartialShipmentDataAccess : BaseDataAccess, IPartialShipmentDataAccess
    {
        #region Constants

        public const int SIZE = 20;

        public const string SupportAccountIdParam = "p_SupportAccountId";
        public const string RequestTypeParam = "p_RequestType";
        public const string ApiParam = "p_Api";
        public const string RequestParam = "p_Request";
        public const string ResponseParam = "p_Response";
        public const string RequestedByUserParam = "p_RequestedByUserAccountId";


        public const string AddressLine1Param = "p_AddressLine1";
        public const string AddressLine2Param = "p_AddressLine2";
        public const string CityParam = "p_City";
        public const string RegionParam = "p_Region";
        public const string RegionNameParam = "p_RegionName";
        public const string CountryIdParam = "p_CountryId";
        public const string ZipCodeParam = "p_ZipCode";
        public const string CreatedOnParam = "p_CreatedOn";
        public const string CreatedByIdParam = "p_CreatedById";
        public const string OutputParam = "p_output";

        /*
             * csp.AddPartialShipment(IN p_MasterOrderId bigint(20), IN p_SapOrderNumber varchar(255), IN p_Status varchar(255), IN p_Message text,
                                            IN p_ReferenceId varchar(32), IN p_OrderDetails text, IN p_ContactInfo text, 
                                              IN p_PartialShipmentAddressId bigint(20), IN p_OrderInfo text, 
                                              IN p_ShippingDetails text, IN p_CreatedOn timestamp, IN p_CreatedById bigint(20),
                                              OUT p_output bigint(20))
             */

        public const string ParentOrderNumberParam = "p_ParentOrderNumber";
        public const string SapOrderNumberParam = "p_SapOrderNumber";
        public const string StatusParam = "p_Status";
        public const string MessageParam = "p_Message";
        public const string ReferenceIdParam = "p_ReferenceId";
        public const string ItemDetailsParam = "p_ItemDetails";
        public const string ContactInfoParam = "p_ContactInfo";
        public const string PartialShipmentAddressIdParam = "p_PartialShipmentAddressId";
        public const string OrderDetailsParam = "p_OrderDetails";
        public const string ShippingDetailsParam = "p_ShippingDetails";

        #endregion

        /// <summary>
        /// Logs all Partial Shipment API calls from CSP to SAP
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <param name="api"></param>
        /// <param name="requestType"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="userAccountId"></param>
        public void LogApiCalls(long supportAccountId, PartialShipmentRequestType requestType, string api, string request, string response, long userAccountId)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("LogPartialShipmentCalls"))
                {
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, RequestTypeParam, DbType.String, requestType.ToDescription());
                    Connection.AddInParameter(command, ApiParam, DbType.String, api);
                    Connection.AddInParameter(command, RequestParam, DbType.String, request);
                    Connection.AddInParameter(command, ResponseParam, DbType.String, response);
                    Connection.AddInParameter(command, RequestedByUserParam, DbType.String, userAccountId);

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public MasterOrder GetMasterOrderDetail(string masterOrderNumber)
        {
            MasterOrder masterOrder = null;

            try
            {
                var getMasterOrderQuery =
                    @"SELECT m.MasterOrderId, m.ParentOrderNumber, m.SoldToSapId, m.EndCustomerSapId, m.SalesforceAccountId, m.SupportAccountId, m.TenantId,
                                        m.UserAccountId, u.Email, m.CreatedOn, m.CreatedByUserAccountId, m.UpdatedOn, m.UpdatedByUserAccountId
                                        FROM csp.masterorder m JOIN csp.useraccount u ON m.UserAccountId = u.UserAccountId WHERE ParentOrderNumber=@p_MasterOrderNumber;";

                using (var command = Connection.GetSqlStringCommand(getMasterOrderQuery))
                {
                    Connection.AddInParameter(command, "@p_MasterOrderNumber", DbType.String, masterOrderNumber);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            masterOrder = new MasterOrder
                            {
                                Id = dataReader.GetColumnValue<long>("MasterOrderId"),
                                MasterOrderNumber = dataReader.GetColumnValue<string>("ParentOrderNumber"),
                                SoldToSapId = dataReader.GetColumnValue<int?>("SoldToSapId"),
                                EndCustomerSapId = dataReader.GetColumnValue<int?>("EndCustomerSapId"),
                                SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                                UserAccountId = dataReader.GetColumnValue<long>("UserAccountId"),
                                AdminEmail = dataReader.GetColumnValue<string>("Email"),
                                TenantId = dataReader.GetColumnValue<long>("TenantId")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return masterOrder;
        }


        public IList<MasterOrder> GetMasterOrders(long supportAccountId)
        {
            IList<MasterOrder> result = new List<MasterOrder>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetMasterOrders"))
                {
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);

                    using (var reader = Connection.ExecuteReader(command))
                    {
                        while (reader.Read())
                        {
                            var masterOrder = new MasterOrder
                            {
                                Id = reader.GetColumnValue<long>("MasterOrderId"),
                                MasterOrderNumber = reader.GetColumnValue<string>("ParentOrderNumber"),
                                SoldToSapId = reader.GetColumnValue<int?>("SoldToSapId"),
                                EndCustomerSapId = reader.GetColumnValue<int?>("EndCustomerSapId"),
                                SupportAccountId = reader.GetColumnValue<long>("SupportAccountId"),
                                UserAccountId = reader.GetColumnValue<long>("UserAccountId"),
                                AdminEmail = reader.GetColumnValue<string>("Email"),
                                TenantId = reader.GetColumnValue<long>("TenantId"),
                                CreatedOn = reader.GetColumnValue<DateTime?>("CreatedOn"),
                                CreatedBy = reader.GetColumnValue<long?>("CreatedByUserAccountId"),
                                UpdatedOn = reader.GetColumnValue<DateTime?>("UpdatedOn"),
                                UpdatedBy = reader.GetColumnValue<long?>("UpdatedByUserAccountId")
                            };

                            result.Add(masterOrder);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result;
        }

        public IList<ShippingOrder> GetShipmentHistory(long supportAccountId)
        {
            var shippingOrders = new List<ShippingOrder>();

            try
            {
                var getShipmentHistory =
                    @"SELECT p.SapOrderNumber, m.ParentOrderNumber, p.CreatedOn, p.ContactInfo, p.Status, p.Message, p.PartialShipmentAddressId,
                      pa.AddressLine1, pa.AddressLine2,pa.City, pa.Region, pa.CountryId, c.Name AS Country, pa.ZipCode, p.ReferenceId
                      FROM masterorder m JOIN partialshipment p ON m.MasterOrderId = p.MasterOrderId
                      LEFT JOIN partialshipmentaddress pa ON p.PartialShipmentAddressId = pa.PartialShipmentAddressId
                      LEFT JOIN country c ON pa.CountryId = c.CountryId
                      WHERE m.SupportAccountId=@p_SupportAccountId;";

                using (var command = Connection.GetSqlStringCommand(getShipmentHistory))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var shippingOrder = new ShippingOrder
                            {
                                ReferenceId = dataReader.GetColumnValue<string>("ReferenceId") ?? string.Empty,
                                SapOrderNumber = dataReader.GetColumnValue<string>("SapOrderNumber") ?? string.Empty,
                                MasterContract = dataReader.GetColumnValue<string>("ParentOrderNumber"),
                                DateOrdered = dataReader.GetColumnValue<DateTime>("CreatedOn"),
                                DateOrderedTicks = dataReader.GetColumnValue<DateTime?>("CreatedOn").HasValue ? ((DateTime)dataReader["CreatedOn"]).Ticks : 0L,
                                ContactInfo = dataReader.GetColumnValue<string>("ContactInfo"),
                                Status = dataReader.GetColumnValue<string>("Status") ?? string.Empty,
                                Message = dataReader.GetColumnValue<string>("Message") ?? string.Empty,
                                ShipToLocation = dataReader.GetColumnValue<int>("PartialShipmentAddressId") == 0 ? null :
                                                 new LocationDetail
                                                 {
                                                     Address1 = dataReader.GetColumnValue<string>("AddressLine1"),
                                                     Address2 = dataReader.GetColumnValue<string>("AddressLine2"),
                                                     City = dataReader.GetColumnValue<string>("City"),
                                                     Region = dataReader.GetColumnValue<string>("Region"),
                                                     CountryId = dataReader.GetColumnValue<int>("CountryId"),
                                                     Country = dataReader.GetColumnValue<string>("Country"),
                                                     PostalCode = dataReader.GetColumnValue<string>("ZipCode")
                                                 }
                            };

                            shippingOrders.Add(shippingOrder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return shippingOrders;
        }

        public long SavePartialShipmentOrder(PartialShipmentOrder partialShipmentOrder, long supportAccountId)
        {

            long partialShipmentId = 0;
            try
            {
                using (var command = Connection.GetStoredProcCommand("AddPartialShipment"))
                {

                Connection.AddInParameter(command, ParentOrderNumberParam, DbType.Int64, partialShipmentOrder.ParentOrderNumber);
                Connection.AddInParameter(command, SapOrderNumberParam, DbType.String, partialShipmentOrder.SapOrderNumber);
                Connection.AddInParameter(command, StatusParam, DbType.String, partialShipmentOrder.Status);
                Connection.AddInParameter(command, MessageParam, DbType.String, partialShipmentOrder.Message);
                Connection.AddInParameter(command, ReferenceIdParam, DbType.String, partialShipmentOrder.ReferenceId);

                var orderContractItems = new List<OrderContractItem>();
                orderContractItems.AddRange(partialShipmentOrder.Devices);
                orderContractItems.AddRange(partialShipmentOrder.Subscription.SelectMany(s => s.Items));
                orderContractItems.AddRange(partialShipmentOrder.Supports.SelectMany(s => s.Items));
                orderContractItems.AddRange(partialShipmentOrder.AddOns);
                Connection.AddInParameter(command, ItemDetailsParam, DbType.String,
                    JsonConvert.SerializeObject(orderContractItems.ToList()));
                Connection.AddInParameter(command, ContactInfoParam, DbType.String, JsonConvert.SerializeObject(new { ContactDetails = partialShipmentOrder.ContactDetail, LocationDetails = partialShipmentOrder.LocationDetail }));
                Connection.AddInParameter(command, PartialShipmentAddressIdParam, DbType.Int64, partialShipmentOrder.PartialShipmentAddressId);
                Connection.AddInParameter(command, OrderDetailsParam, DbType.String, JsonConvert.SerializeObject(partialShipmentOrder.OrderDetail));
                Connection.AddInParameter(command, ShippingDetailsParam, DbType.String, JsonConvert.SerializeObject(partialShipmentOrder.ShippingDetail));
                Connection.AddInParameter(command, CreatedOnParam, DbType.DateTime, partialShipmentOrder.CreatedOn);
                Connection.AddInParameter(command, CreatedByIdParam, DbType.Int64, partialShipmentOrder.CreatedById);
                Connection.AddOutParameter(command, OutputParam, DbType.Int64, SIZE);

                    Connection.ExecuteNonQuery(command);


                    if (command.Parameters[OutputParam].Value != DBNull.Value)
                    {
                        partialShipmentId = Convert.ToInt64(command.Parameters[OutputParam].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return partialShipmentId;
            
        }


        public long SaveAddress(LocationDetail address, long supportAccountId, long userAccountId)
        {
            long partialShipmentAddressId = 0;

            //Sanitize string values.
            address.Address1 = address.Address1 == null ? null : address.Address1.Trim();
            address.Address2 = address.Address2 == null ? null : address.Address2.Trim();
            address.City = address.City == null ? null : address.City.Trim();
            address.Region = address.Region == null ? null : address.Region.Trim();
            address.RegionName = address.RegionName == null ? null : address.RegionName.Trim();
            address.PostalCode = address.PostalCode == null ? null : address.PostalCode.Trim();

            using (var command = Connection.GetStoredProcCommand("AddPartialShipmentAddress"))
            {

                Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);
                Connection.AddInParameter(command, AddressLine1Param, DbType.String, address.Address1);
                Connection.AddInParameter(command, AddressLine2Param, DbType.String, address.Address2);
                Connection.AddInParameter(command, CityParam, DbType.String, address.City);
                Connection.AddInParameter(command, RegionParam, DbType.String, address.Region);
                Connection.AddInParameter(command, RegionNameParam, DbType.String, address.RegionName);
                Connection.AddInParameter(command, CountryIdParam, DbType.Int64, address.CountryId);
                Connection.AddInParameter(command, ZipCodeParam, DbType.String, address.PostalCode);
                Connection.AddInParameter(command, CreatedOnParam, DbType.DateTime, DateTime.Now);
                Connection.AddInParameter(command, CreatedByIdParam, DbType.Int64, userAccountId);
                Connection.AddOutParameter(command, OutputParam, DbType.Int64, SIZE);

                Connection.ExecuteNonQuery(command);


                if (command.Parameters[OutputParam].Value != DBNull.Value)
                {
                    partialShipmentAddressId = Convert.ToInt64(command.Parameters[OutputParam].Value);
                }

                return partialShipmentAddressId;
            }

        }

        public IList<LocationDetail> GetAddresses(long supportAccountId)
        {
            var addresses = new List<LocationDetail>();

            try
            {
                var getAddresses =
                    @"SELECT pa.AddressLine1, pa.AddressLine2, pa.City, pa.Region,pa.RegionName, pa.ZipCode, c.Name AS Country, pa.CountryId
                      FROM partialshipmentaddress pa JOIN country c ON pa.CountryId = c.CountryId
                      WHERE pa.SupportAccountId=@p_SupportAccountId;";

                using (var command = Connection.GetSqlStringCommand(getAddresses))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {

                            addresses.Add(new LocationDetail
                            {
                                Address1 = dataReader.GetColumnValue<string>("AddressLine1"),
                                Address2 = dataReader.GetColumnValue<string>("AddressLine2"),
                                City = dataReader.GetColumnValue<string>("City"),
                                Region = dataReader.GetColumnValue<string>("Region"),
                                RegionName = dataReader.GetColumnValue<string>("RegionName"),
                                Country = dataReader.GetColumnValue<string>("Country"),
                                CountryId = dataReader.GetColumnValue<long>("CountryId"),
                                PostalCode = dataReader.GetColumnValue<string>("ZipCode")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return addresses;
        }
        public IList<ShippingMethodResult> GetShippingMethodList(bool isDomestic)
        {
            var shippingMelthodList = new List<ShippingMethodResult>();

            try
            {
                var getshippingMethod =
                    @"SELECT sm.Code,sm.shippingmethod FROM shippingmethod sm
                      WHERE sm.isDomestic=@p_isDomestic;";

                using (var command = Connection.GetSqlStringCommand(getshippingMethod))
                {
                    Connection.AddInParameter(command, "@p_isDomestic", DbType.Boolean, isDomestic);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            shippingMelthodList.Add(new Model.PartialShipment.ShippingMethodResult
                            {
                                Code = dataReader.GetColumnValue<string>("code"),
                                ShippingMethod = dataReader.GetColumnValue<string>("ShippingMethod"),

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return shippingMelthodList;
        }

        public PartialShipmentOrderDetail GetPartialShipmentOrderForResubmit(MasterContractDetail masterContractDetail, string referenceId)
        {
            var orderDetails = new PartialShipmentOrderDetail();
            if (masterContractDetail.LocationDetail != null)
              GetCountryDetails(masterContractDetail.LocationDetail);
            var orderContractDetail = new OrderContractDetail
            {
                
                AdminEmail = masterContractDetail.AdminEmail,
                ContactDetailName = masterContractDetail.ContactDetailName,
                AuthEmail = masterContractDetail.AuthEmail,
                LocationDetail = masterContractDetail.LocationDetail,                
                CustomerName = masterContractDetail.CustomerName,
                MasterOrderNumber = masterContractDetail.MasterOrderNumber,
                PanwQuote = masterContractDetail.PanwQuote,
                ParentOrderNumber = masterContractDetail.ParentOrderNumber,
                Status = masterContractDetail.Status,
                OrderSummaryEmail=masterContractDetail.OrderSummaryEmail,
                ShippingType=masterContractDetail.ShippingType,
                //EstimatedShipDate = DateTime.Now.AddDays(14).ToShortDateString(),
                Devices = new List<OrderContractItem>(),
                Supports = new List<OrderContract>(),
                Subscriptions = new List<OrderContract>(),
                AccessoryAndOtherAddOns= new List<OrderContractItem>()               
            };

            try
            {
                var shipmentOrderDetail = GetPartialShipmentDetailsByReferenceId(referenceId);
                bool isDataCenter = shipmentOrderDetail.ShippingDetail.IsDataCenter;
                if (masterContractDetail.Items != null)
                    orderContractDetail = GetOrderContractDetail(masterContractDetail, orderContractDetail,isDataCenter,shipmentOrderDetail);
                
                var serializedParent = JsonConvert.SerializeObject(orderContractDetail);
                orderDetails = JsonConvert.DeserializeObject<PartialShipmentOrderDetail>(serializedParent);
                orderDetails.LocationDetail = null;
                orderDetails.ContactDetail = shipmentOrderDetail.ContactDetail;
                orderDetails.OrderDetail = shipmentOrderDetail.OrderDetail;
                orderDetails.ShippingDetail = shipmentOrderDetail.ShippingDetail;
                orderDetails.LocationDetail = shipmentOrderDetail.LocationDetail;

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return orderDetails;
        }

        public OrderContractDetail ToOrderContractDetail(MasterContractDetail masterContractDetail,bool isDataCenter)
        {
            if (masterContractDetail.LocationDetail != null)
                GetCountryDetails(masterContractDetail.LocationDetail);
            var orderContractDetail = new OrderContractDetail
            {
                AdminEmail = masterContractDetail.AdminEmail,
                ContactDetailName = masterContractDetail.ContactDetailName,
                AuthEmail = masterContractDetail.AuthEmail,
                LocationDetail = masterContractDetail.LocationDetail,
                CustomerName = masterContractDetail.CustomerName,
                OrderSummaryEmail = masterContractDetail.OrderSummaryEmail,
                MasterOrderNumber = masterContractDetail.MasterOrderNumber,
                PanwQuote = masterContractDetail.PanwQuote,
                ParentOrderNumber = masterContractDetail.ParentOrderNumber,
                Status = masterContractDetail.Status,
                ShippingType=masterContractDetail.ShippingType,
                //EstimatedShipDate= DateTime.Now.AddDays(14).ToShortDateString(),
                Devices = new List<OrderContractItem>(),
                Supports = new List<OrderContract>(),
                Subscriptions = new List<OrderContract>(),               
                AccessoryAndOtherAddOns = new List<OrderContractItem>(),
                IONExcludedSkus = new List<string>()             
            };

            try
            {
                GetOrderContractDetail(masterContractDetail, orderContractDetail,isDataCenter);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return orderContractDetail;
        }

        private OrderContractDetail GetOrderContractDetail(MasterContractDetail masterContractDetail, OrderContractDetail orderContractDetail, bool isDataCenter,ShipmentOrderDetail shipmentOrderDetail = null)
        {
            var getDeviceOrderContracts = @"SELECT pt.PanDbIdentification AS Category, p.PartNumber  FROM partmaster p JOIN parttype pt ON p.PartTypeId = pt.PartTypeId
                      JOIN partsubtype ps ON ps.PartSubtypeId = p.PartSubtypeId
                      WHERE p.ProductFamily='CGX' AND pt.PanDbIdentification='DEV' AND p.PartNumber IN ({0}) AND ps.PanDbIdentification ='CGX-BASE';";

            var paramNames = masterContractDetail.Items.Select((x, i) => "@sku" + i.ToString()).ToArray();

            string inClauseForPartNumber = string.Join(",", paramNames);

            using (var command = Connection.GetSqlStringCommand(string.Format(getDeviceOrderContracts, inClauseForPartNumber)))
            {

                for (int i = 0; i < paramNames.Length; i++)
                {
                    Connection.AddInParameter(command, paramNames[i], DbType.AnsiString, masterContractDetail.Items[i].Sku);
                }
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        //Device level                       
                        var itemList = masterContractDetail.Items.Where(x => x.Sku == dataReader.GetColumnValue<string>("PartNumber")).ToList();
                        if (itemList != null)
                        {
                            foreach (var item in itemList)
                            {
                                var dev = item.ToOrderContractItem(shipmentOrderDetail == null ? new List<OrderContractItem>() : shipmentOrderDetail.Device);
                                dev.Category = ShipmentTypes.Device.ToString();
                                orderContractDetail.Devices.Add(dev);

                            }
                        }
                    }
                }
            }

            var nonDeviceItems = masterContractDetail.Items.Where(x => !orderContractDetail.Devices.Any(y => y.Sku == x.Sku)).ToArray();

            //Consider rearrangement if we have both DeviceSkus and nonDevice SKUs in masterContractDetailItems of CGX product families.
            if (orderContractDetail.Devices.Count > 0 && nonDeviceItems != null && nonDeviceItems.Length > 0)
            {

                //Extract Support level Line items
                var getOrderContractsWithSupport = @"SELECT DISTINCT c.MainPartNumber, p.PartNumber, pt.PanDbIdentification FROM csp.compatibilitymatrix c JOIN partmaster p ON c.MatchingPartNumber =  p.PartNumber JOIN parttype pt ON p.PartTypeId = pt.PartTypeId
                     WHERE c.MainPartNumber IN ({0})   AND p.PartNumber IN ({1}) AND pt.PanDbIdentification IN ({2});";

                var paramNamesForDeviceSku = orderContractDetail.Devices.Select((x, i) => "@deviceSku" + i.ToString()).ToArray();
                string inClauseForDeviceSku = string.Join(",", paramNamesForDeviceSku);

                var paramNamesForNonDeviceSku = nonDeviceItems.Select((x, i) => "@nonDeviceSku" + i.ToString()).ToArray();
                string inClauseForNonDeviceSku = string.Join(",", paramNamesForNonDeviceSku);

                var supportCategory = new[] { CategoryTypes.SUP.ToString() };
                //Enum.GetNames(typeof(CategoryTypes)).Where(x => x != CategoryTypes.DEV.ToString()).ToArray();
                var paramNamesForSupportCategory = supportCategory.Select((x, i) => "@categoryType" + i.ToString()).ToArray();
                string inClauseForSupportCategory = string.Join(",", paramNamesForSupportCategory);

                List<string> supportSkus = new List<string>();

                using (var command = Connection.GetSqlStringCommand(string.Format(getOrderContractsWithSupport, inClauseForDeviceSku, inClauseForNonDeviceSku, inClauseForSupportCategory)))
                {
                    for (int i = 0; i < paramNamesForDeviceSku.Length; i++)
                    {
                        Connection.AddInParameter(command, paramNamesForDeviceSku[i], DbType.AnsiString, orderContractDetail.Devices[i].Sku);
                    }

                    for (int i = 0; i < paramNamesForNonDeviceSku.Length; i++)
                    {
                        Connection.AddInParameter(command, paramNamesForNonDeviceSku[i], DbType.AnsiString, nonDeviceItems[i].Sku);
                    }

                    for (int i = 0; i < paramNamesForSupportCategory.Length; i++)
                    {
                        Connection.AddInParameter(command, paramNamesForSupportCategory[i], DbType.AnsiString, supportCategory[i]);
                    }

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var category = dataReader.GetColumnValue<string>("PanDbIdentification").ToUpper();
                            var deviceSkuName = dataReader.GetColumnValue<string>("MainPartNumber");
                            var supportLevelItemList = masterContractDetail.Items.Where(x => x.Sku == dataReader.GetColumnValue<string>("PartNumber"));
                            //Support level
                            if (supportLevelItemList != null)
                            {
                                foreach (var item in supportLevelItemList)
                                {
                                    var supportLevelItem = item.ToOrderContractItem(shipmentOrderDetail == null ? new List<OrderContractItem>() : shipmentOrderDetail.Supports);
                                    var support = orderContractDetail. Supports.FirstOrDefault(x => x.Category == deviceSkuName);

                                    supportLevelItem.Category = ShipmentTypes.Support.ToString();

                                    if (support != null)
                                    {

                                        support.Items.Add(supportLevelItem);
                                    }
                                    else
                                    {
                                        orderContractDetail.Supports.Add(new OrderContract { Category = deviceSkuName, Items = new List<OrderContractItem> { supportLevelItem } });
                                    }

                                    //Keep suppport SKus to exclude them from nonDeviceSkus.
                                    supportSkus.Add(supportLevelItem.Sku);
                                }
                            }
                        }
                    }
                }

                //Collection containing  subscription and AddOns.
                var remainingSkus = supportSkus.Count > 0 ? nonDeviceItems.Where(x => !supportSkus.Contains(x.Sku)) : nonDeviceItems;

                //If we have items that could fall into subscription or AddOns.
                if (remainingSkus != null && remainingSkus.Count() > 0)
                {
                    //Keep track of subscription categoryName to be clubbed together 
                    List<string> subscriptionCategoryNames = new List<string>();
                    // to hold different types within subscription
                    List<OrderContractItem> clarityReporting = new List<OrderContractItem>();
                    List<OrderContractItem> clarityNDVR = new List<OrderContractItem>();
                    List<OrderContractItem> zbfw = new List<OrderContractItem>();
                    List<OrderContractItem> dataSubscription= new List<OrderContractItem>();

                    //Extract remaining(subscrption and AddOns) level Line items
                    var getOrderContractsWithRemainingSkus = @"SELECT DISTINCT p.PartNumber, pt.PanDbIdentification,ps.PanDbIdentification SubType FROM partmaster p JOIN parttype pt ON p.PartTypeId = pt.PartTypeId
                                                                  LEFT OUTER JOIN partsubtype ps ON ps.PartSubtypeId = p.PartSubtypeId
                                                                 WHERE  p.PartNumber IN ({0})  AND pt.PanDbIdentification Not IN ({1})";

                    var paramNamesForRemainingSkus = remainingSkus.Select((x, i) => "@nonDeviceSku" + i.ToString()).ToArray();
                    string inClauseForRemainingSkus = string.Join(",", paramNamesForRemainingSkus);

                    var support = new[] { CategoryTypes.SUP.ToString() };
                    var paramNamesForDeviceAndSupportCategory = support.Select((x, i) => "@categoryType" + i.ToString()).ToArray();
                    string inClauseForDeviceAndSupportCategory = string.Join(",", paramNamesForDeviceAndSupportCategory);

                    using (var command = Connection.GetSqlStringCommand(string.Format(getOrderContractsWithRemainingSkus, inClauseForRemainingSkus, inClauseForDeviceAndSupportCategory)))
                    {
                        for (int i = 0; i < paramNamesForRemainingSkus.Length; i++)
                        {
                            Connection.AddInParameter(command, paramNamesForRemainingSkus[i], DbType.AnsiString, remainingSkus.ElementAt(i).Sku);
                        }

                        for (int i = 0; i < paramNamesForDeviceAndSupportCategory.Length; i++)
                        {
                            Connection.AddInParameter(command, paramNamesForDeviceAndSupportCategory[i], DbType.AnsiString, support[i]);
                        }

                        using (var dataReader = Connection.ExecuteReader(command))
                        {
                            while (dataReader.Read())
                            {
                                var category = dataReader.GetColumnValue<string>("PanDbIdentification").ToUpper();
                                var sku = dataReader.GetColumnValue<string>("PartNumber");
                                var subtype= dataReader.GetColumnValue<string>("SubType");

                                var levelItemList = masterContractDetail.Items.Where(x => x.Sku == sku).ToList();                               
                                if (levelItemList != null)
                                {
                                    foreach (var item in levelItemList)
                                    {
                                        var levelItem = item.ToOrderContractItem(shipmentOrderDetail == null ? new List<OrderContractItem>() : shipmentOrderDetail.SubscriptionOrAddOns);

                                        //Subscription category level
                                        if (category == CategoryTypes.SUB.ToString())
                                        {
                                            levelItem.Category = ShipmentTypes.Subscription.ToString();
                                            if (!isDataCenter)
                                            {
                                                if (subtype == SubscriptionTypes.CGX_NDVR.ToString().Replace("_", "-"))
                                                {
                                                    clarityNDVR.Add(levelItem);
                                                }
                                                else if (subtype == SubscriptionTypes.CGX_WANR.ToString().Replace("_", "-"))
                                                {
                                                    clarityReporting.Add(levelItem);                                                   
                                                }
                                            }
                                            if (subtype == SubscriptionTypes.CGX_ZBFW.ToString().Replace("_", "-"))
                                            {
                                                zbfw.Add(levelItem);
                                            }
                                            else if (subtype == SubscriptionTypes.CGX_BW.ToString().Replace("_", "-"))
                                            {
                                                dataSubscription.Add(levelItem);
                                            }
                                            else if (subtype == SubscriptionTypes.CGX_DC.ToString().Replace("_", "-"))
                                            {
                                                dataSubscription.Add(levelItem);
                                            }
                                            else if (subtype == SubscriptionTypes.CGX_SUB_BR.ToString().Replace("_", "-"))
                                            {
                                                dataSubscription.Add(levelItem);
                                            }
                                            else
                                                continue;

                                        }
                                        else
                                        {
                                            levelItem.Category = ShipmentTypes.AccessoryAndOtherAddOns.ToString();
                                            orderContractDetail.AccessoryAndOtherAddOns.Add(levelItem);
                                        }
                                    }
                                }
                            }
                        }
                        if (dataSubscription.Count > 0)
                        {
                            orderContractDetail.Subscriptions.Add(new OrderContract { Category = SubscriptionTypes.CGX_BW.ToDescription().ToString(), Items = dataSubscription });
                        }
                        if (clarityReporting.Count>0)
                        {
                            orderContractDetail.Subscriptions.Add(new OrderContract { Category = SubscriptionTypes.CGX_WANR.ToDescription().ToString(), Items = clarityReporting });
                        }
                        if(clarityNDVR.Count >0)
                        {
                            orderContractDetail.Subscriptions.Add(new OrderContract { Category = SubscriptionTypes.CGX_NDVR.ToDescription().ToString(), Items = clarityNDVR });
                        }
                        if (zbfw.Count > 0)
                        {
                            orderContractDetail.Subscriptions.Add(new OrderContract { Category = SubscriptionTypes.CGX_ZBFW.ToDescription().ToString(), Items = zbfw });
                        }
                       
                    }

                    }
                    //if (subscriptionCategoryNames.Count > 0)
                    //{
                    //    orderContractDetail.Subscriptions.Category = string.Join(",", subscriptionCategoryNames);
                    //}
                }
            


            //Provide unique Id for each item , for the purpose of UI CRUD operation
            var idMasterContractItem = 0;
            orderContractDetail.Devices.ForEach(x => x.UniqueId = ++idMasterContractItem);
            orderContractDetail.Supports.ForEach(x => x.Items.ForEach(y => y.UniqueId = ++idMasterContractItem));
            orderContractDetail.Subscriptions.ForEach(x => x.Items.ForEach(y => y.UniqueId = ++idMasterContractItem));
            orderContractDetail.AccessoryAndOtherAddOns.ForEach(x => x.UniqueId = ++idMasterContractItem);
            var excudedskus = getIONExcludedSkus();
            var result = excudedskus.Where(a => orderContractDetail.Devices.Any(b => b.Sku.ToString().IndexOf(a) >= 0)).ToList();
            if (result!=null &&  result.Count > 0)
            {
                orderContractDetail.IONExcludedSkus = excudedskus;
            }

            return orderContractDetail;
        }

        private ShipmentOrderDetail GetPartialShipmentDetailsByReferenceId(string referenceId)
        {
            var shipmentOrderDetail = new ShipmentOrderDetail();

            try
            {
                var shipmentOrder = @"SELECT p.ContactInfo, p.ItemDetails, p.OrderDetails, p.ShippingDetails,
                                      p.ReferenceId FROM  csp.partialshipment p JOIN csp.masterorder m ON m.MasterOrderId = p.MasterOrderId 
                                      WHERE  p.ReferenceId = @p_referenceId";

                using (var command = Connection.GetSqlStringCommand(shipmentOrder))
                {
                    Connection.AddInParameter(command, "@p_referenceId", DbType.String, referenceId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var contactInfo = JsonConvert.DeserializeObject<ContactLocation>(dataReader.GetColumnValue<string>("ContactInfo"), new JsonSerializerSettings());
                            var itemDetails = JsonConvert.DeserializeObject<List<OrderContractItem>>(dataReader.GetColumnValue<string>("ItemDetails"), new JsonSerializerSettings());

                            shipmentOrderDetail = new ShipmentOrderDetail()
                            {
                                ContactDetail = contactInfo != null && contactInfo.ContactDetails != null ? contactInfo.ContactDetails : new ContactInformation(),
                                ShippingDetail = JsonConvert.DeserializeObject<ShippingDetail>(dataReader.GetColumnValue<string>("ShippingDetails"), new JsonSerializerSettings()),
                                OrderDetail = JsonConvert.DeserializeObject<OrderDetail>(dataReader.GetColumnValue<string>("OrderDetails"), new JsonSerializerSettings()),
                                Device = itemDetails.Where(c => c.Category.ToString().ToLower() == "device").ToList(),
                                Supports = itemDetails.Where(c => c.Category.ToString().ToLower() == "support").ToList(),
                                SubscriptionOrAddOns = itemDetails.Where(c => c.Category.ToString().ToLower() == "accessoryandotheraddons" || c.Category.ToString().ToLower() == "subscription").ToList(),
                                LocationDetail = contactInfo != null && contactInfo.LocationDetails != null ? contactInfo.LocationDetails : new LocationDetail()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return shipmentOrderDetail;
        }

        private LocationDetail GetCountryDetails(LocationDetail locationDetail)
        {
           
            try
            {
                var countryIdSQL = @"SELECT c.CountryId,c.Name FROM csp.country c WHERE c.Code=@p_Country OR c.Name=@p_Country";

                using (var command = Connection.GetSqlStringCommand(countryIdSQL))
                {
                    Connection.AddInParameter(command, "@p_Country", DbType.String, locationDetail.CountryCode);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            locationDetail.CountryId = dataReader["CountryId"] == null || dataReader["CountryId"] is DBNull ? 0 : Convert.ToInt32(dataReader["CountryId"].ToString());
                            locationDetail.Country = dataReader["Name"] == null || dataReader["Name"] is DBNull ? string.Empty : System.Web.HttpUtility.HtmlEncode(dataReader["Name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return locationDetail;
        }

        public bool IsDevice(string partNumber)
        {
            try
            {
                var getMasterOrderQuery =
                    @"SELECT p1.PanDbIdentification FROM partmaster p  JOIN parttype p1 ON p.PartTypeId=p1.PartTypeId WHERE p.PartNumber
                     =@p_partNumber;";

                using (var command = Connection.GetSqlStringCommand(getMasterOrderQuery))
                {
                    Connection.AddInParameter(command, "@p_partNumber", DbType.String, partNumber);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            var Identification = dataReader.GetColumnValue<string>("PanDbIdentification");
                            return Identification?.ToUpper() == "DEV" ? true : false;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return false;
        }

        private List<string> getIONExcludedSkus()
        {
            var iONExcludedSkus = new List<string>();

            try
            {
                var sqlquery = @"Select * from csp.IonExcludedSkus";

                using (var command = Connection.GetSqlStringCommand(sqlquery))
                {                   
                    using (var dataReader = Connection.ExecuteReader(command))
                    {                        
                        while (dataReader.Read())
                        {
                            var sku = dataReader.GetColumnValue<string>("sku");
                            iONExcludedSkus.Add(sku);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return iONExcludedSkus;
        }

    }
}
