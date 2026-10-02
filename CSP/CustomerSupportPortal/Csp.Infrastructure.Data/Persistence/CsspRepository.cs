using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using Csp.Infrastructure.Data.DBs.CSP;
using NHibernate.Type;
using NHibernate.Util;
using MySql.Data.MySqlClient;
using Csp.Api;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;

namespace Csp.Infrastructure.Data.Persistence
{
    public class CsspRepository : ICsspRepository
    {
        private readonly IConnectionStrings _connectionStrings;

        public CsspRepository(IConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public CsspInfo GetCsspInfo(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var csspInfo = (from sa in db.supportaccounts
                join csi in db.csspdatas on sa.SupportAccountId equals csi.SupportAccountId
                where sa.SupportAccountId == supportAccountId
                select new CsspInfo()
                {
                    SupportAccountId = sa.SupportAccountId,
                    SupportAccountName = sa.Name,
                    CsspDataId = csi.CsspDataId,
                    AscName = csi.AscName,
                    AscSfdcAccountId = csi.AscSfdcAccountId,
                    DistiName = csi.DistiName,
                    DistiSfdcAccountId = csi.DistiSfdcAccountId,
                    CanGeneratePrelicenseKeys = csi.CanGeneratePreLicenseKeys
                });
            return csspInfo.FirstOrDefault();
        }


        public IEnumerable<CsspSku> SearchCsspSkus(DataParameters parameters, long? supportAccountId, int? model, int? term, int? supportType, int? bundle, string filterProducts = "")
        {
            var db = SessionDbContext.CspObjectContext;

            string bundleType = String.Empty;

            if (bundle.HasValue)
            {
                switch (bundle.Value)
                {
                    case 1:
                    case 2:
                        bundleType = string.Format("-BND{0}-", bundle.Value);
                        break;
                    case 3:
                        bundleType = "-BASIC-";
                        break;
                    default:
                        bundleType = string.Empty;
                        break;
                }
            }

            var csspSkus = from s in db.csspskus
                           join pm in db.partmasters on s.PartMasterId equals pm.PartMasterId
                           join cm in db.csspmodels on s.CsspModelId equals cm.CsspModelId
                           join ct in db.csspterms on s.CsspTermId equals ct.CsspTermId
                           join cst in db.csspsupporttypes on s.CsspSupportTypeId equals  cst.CsspSupportTypeId
                           select new CsspSku()
                           {
                               CsspSkuId = s.CsspSkuId,
                               CsspModelId = cm.CsspModelId,
                               Model = cm.Model,
                               Description = s.SkuDescription,
                               PartNumber = pm.PartNumber,
                               CsspTermId = ct.CsspTermId,
                               Term = ct.Term,
                               CsspSupportTypeId = cst.CsspSupportTypeId,
                               SupportType = cst.SupportType
                           };

            if (!string.IsNullOrEmpty(filterProducts))
            {
                var listofproductstoexclude = filterProducts.Split(',').Select(int.Parse).ToList();
                csspSkus = csspSkus.Where(x => !listofproductstoexclude.Contains(x.CsspSkuId));
            }

            if (model.HasValue)
            {
                csspSkus = csspSkus.Where(x => x.CsspModelId == model.Value);
            }

            if (term.HasValue)
            {
                csspSkus = csspSkus.Where(x => x.CsspTermId == term.Value);
            }

            if (supportType.HasValue)
            {
                csspSkus = csspSkus.Where(x => x.CsspSupportTypeId == supportType.Value);
            }

            if (bundle.HasValue)
            {
                csspSkus = csspSkus.Where(x => x.PartNumber.Contains(bundleType));
            }

            if (supportAccountId.HasValue && !HasBacklineSupport(supportAccountId.Value)) // Dont show Backline Skus for a disti who doesnt have a ASC
            {
                csspSkus = csspSkus.Where(x => x.CsspSupportTypeId == 2);
            }

            if (supportAccountId != null)
            {
                var alreadyOrderedItems = GetListofProductsAlreadyOrdered(supportAccountId.Value);

                if (alreadyOrderedItems.Any())
                {
                    csspSkus = csspSkus.Where(q => !alreadyOrderedItems.Contains(q.CsspSkuId));
                }
            }

            var skus =  csspSkus.ToList();

           parameters.TotalRecordCount = skus.Count;

            return skus.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount).ToArray();

        }

        private bool HasBacklineSupport(long csspSupportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var csspAttributes = (from x in db.csspdatas
                where x.SupportAccountId == csspSupportAccountId
                select x).FirstOrDefault();

            if (csspAttributes != null && !string.IsNullOrEmpty(csspAttributes.AscName) && csspAttributes.AscName != "null")
            {
                return true;
            }
            return false;
        }

        private IEnumerable<int> GetListofProductsAlreadyOrdered(long csspSupportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var productsOrdered = (from x in db.csspshoppingcarts
                join y in db.csspshoppingcartitems on x.CsspShoppingCartId equals y.CsspShoppingCartId
                where x.SupportAccountId == csspSupportAccountId
                select y.CsspSkuId);

            return productsOrdered;

        }


        public IEnumerable<CsspModel> GetCsspModels()
        {
            var db = SessionDbContext.CspObjectContext;
            var models = (from cm in db.csspmodels
                select new CsspModel()
                {
                    CsspModelId = cm.CsspModelId,
                    Model = cm.Model
                }).ToList();
            return models;
        }

        public IEnumerable<CsspSupportType> GetCsspSupportTypes()
        {
            var db = SessionDbContext.CspObjectContext;
            var supportTypes = (from ct in db.csspsupporttypes
                          select new CsspSupportType()
                          {
                              CsspSupportTypeId = ct.CsspSupportTypeId,
                              SupportType = ct.SupportType
                          }).ToList();
            return supportTypes;
        }

        public IEnumerable<CsspTerm> GetCsspTerms()
        {
            var db = SessionDbContext.CspObjectContext;
            var terms = (from ct in db.csspterms
                                select new CsspTerm()
                                {
                                    CsspTermId = ct.CsspTermId,
                                    Term = ct.Term
                                }).ToList();
            return terms;
        }


        public CsspSku GetSkuDetails(int skuId)
        {
            var db = SessionDbContext.CspObjectContext;

            var csspSkus = from s in db.csspskus
                           join pm in db.partmasters on s.PartMasterId equals pm.PartMasterId
                           join cm in db.csspmodels on s.CsspModelId equals cm.CsspModelId
                           join ct in db.csspterms on s.CsspTermId equals ct.CsspTermId
                           join cst in db.csspsupporttypes on s.CsspSupportTypeId equals cst.CsspSupportTypeId
                           select new CsspSku()
                           {
                               CsspSkuId = s.CsspSkuId,
                               CsspModelId = cm.CsspModelId,
                               Model = cm.Model,
                               Description = s.SkuDescription,
                               PartNumber = pm.PartNumber,
                               CsspTermId = ct.CsspTermId,
                               Term = ct.Term,
                               CsspSupportTypeId = cst.CsspSupportTypeId,
                               SupportType = cst.SupportType
                           };

            csspSkus = csspSkus.Where(x => x.CsspSkuId == skuId);

            return csspSkus.FirstOrDefault();

        }

        public IEnumerable<CsspSku> GetSkuDetails(List<int> SkuIds)
        {
            var db = SessionDbContext.CspObjectContext;

            var csspSkus = from s in db.csspskus
                           join pm in db.partmasters on s.PartMasterId equals pm.PartMasterId
                           join cm in db.csspmodels on s.CsspModelId equals cm.CsspModelId
                           join ct in db.csspterms on s.CsspTermId equals ct.CsspTermId
                           join cst in db.csspsupporttypes on s.CsspSupportTypeId equals cst.CsspSupportTypeId
                           where SkuIds.Contains(s.CsspSkuId)
                           select new CsspSku()
                           {
                               CsspSkuId = s.CsspSkuId,
                               CsspModelId = cm.CsspModelId,
                               Model = cm.Model,
                               Description = s.SkuDescription,
                               PartNumber = pm.PartNumber,
                               CsspTermId = ct.CsspTermId,
                               Term = ct.Term,
                               CsspSupportTypeId = cst.CsspSupportTypeId,
                               SupportType = cst.SupportType
                           };

            return csspSkus.ToList();

        }

        public IEnumerable<CsspOrderDetails> GetOrders(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var orders = from cartItem in db.csspshoppingcartitems
                         join cart in db.csspshoppingcarts on cartItem.CsspShoppingCartId equals cart.CsspShoppingCartId
                         join u in db.useraccounts on cart.CreatedBy equals u.UserAccountId
                         join status in db.cssporderstatus on cart.CsspOrderStatusId equals status.CsspOrderStatusId
                         join sku in db.csspskus on cartItem.CsspSkuId equals sku.CsspSkuId
                         join pm in db.partmasters on sku.PartMasterId equals pm.PartMasterId
                         where cart.SupportAccountId == supportAccountId
                         select new CsspOrderDetails()
                         {
                             CsspShoppingCartId = cart.CsspShoppingCartId,
                             CsspShoppingCartNumber = cart.CartNumber,
                             OrderedBy = u.Email,
                             SapOrderId = cart.SapOrderId,
                             CsspSku = pm.PartNumber,
                             CsspSkuDesc = sku.SkuDescription,
                             CreatedDate = cart.CreatedDate,
                             Status = status.Status
                         };

            return orders.ToList();
        }

        public IEnumerable<CsspOrderHistory> GetOrderHistory(long supportAccountId, string orderNumber="")
        {
            var db = SessionDbContext.CspObjectContext;

            var orders = (from cart in db.csspshoppingcarts
                         join u in db.useraccounts on cart.CreatedBy equals u.UserAccountId
                         join status in db.cssporderstatus on cart.CsspOrderStatusId equals status.CsspOrderStatusId
                         where cart.SupportAccountId == supportAccountId
                         orderby cart.CreatedDate descending
                         select new CsspOrderHistory()
                         {
                             CsspShoppingCartId = cart.CsspShoppingCartId,
                             CsspCartNumber = cart.CartNumber,
                             OrderDate = cart.CreatedDate,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             OrderStatus = (status.CsspOrderStatusId == 1 || status.CsspOrderStatusId == 2 || status.CsspOrderStatusId == 3) ? "<div class=\"cssp-pending-order\">Pending</div>" : status.Status,
                             SupportAccountId = cart.SupportAccountId
                         });

            if (!string.IsNullOrEmpty(orderNumber))
            {
                orders = orders.Where(x => x.CsspCartNumber == orderNumber);
            }

            return orders.ToList();
        }

    private int? CreateCsspOrder(long supportAccountId, long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var csspOrder = db.csspshoppingcarts.Add(new csspshoppingcart()
            {
                CreatedBy = userAccountId,
                CsspOrderStatusId = 1, // New
                CreatedDate = DateTime.Now,
                CartNumber = string.Empty,
                SupportAccountId = supportAccountId
            });

            if (db.SaveChanges() > 0)
            {
                var csspCartId = csspOrder.CsspShoppingCartId;
                return csspCartId;
            }
            return null;
        }

        private bool AddCartNumberToCart(long supportAccountId, long userAccountId, int cartId, out string cartNum)
        {
            cartNum = "";
            //Reverse Support AccountId - Reverse User accountid - Salt + Order number.
            const int salt = 4321;
            string cartNumber = string.Format("{0}-{1}-{2}",
                string.Concat(supportAccountId.ToString().Reverse()),
                string.Concat(userAccountId.ToString().Reverse()),
                salt + cartId);
            var db = SessionDbContext.CspObjectContext;
            var cart = (from x in db.csspshoppingcarts
                        where x.CsspShoppingCartId == cartId
                    select x).FirstOrDefault();
            if (cart != null)
            {
                cart.CartNumber = cartNumber;
                cart.CsspOrderStatusId = 2; // Created
                if (db.SaveChanges() > 0)
                {
                    cartNum = cartNumber;
                    return true;
                }
            }
            return false;
        }

        public bool AddOrderItems(int cartId, List<int> skuIds)
        {
            var db = SessionDbContext.CspObjectContext;
            foreach (var skuId in skuIds)
            {
                db.csspshoppingcartitems.Add(new csspshoppingcartitem()
                {
                    CsspShoppingCartId = cartId,
                    CsspSkuId = skuId
                });
            }
            if (db.SaveChanges() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SubmitCsspOrder(List<int> skuIds, long supportAccountId, long userAccountId, out int? cartId, out string cartNumber)
        {
            var csspCartId = CreateCsspOrder(supportAccountId, userAccountId);
            cartId = csspCartId;
            cartNumber = "";
            if (cartId.HasValue)
            {
                if (AddCartNumberToCart(supportAccountId, userAccountId, cartId.Value, out cartNumber))
                {
                    if (AddOrderItems(cartId.Value, skuIds))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public CsspOrder GetCsspOrderDetails(int cartId)
        {
            var db = SessionDbContext.CspObjectContext;

            var csspOrder = (from x in db.csspshoppingcarts
                where x.CsspShoppingCartId == cartId
                select new CsspOrder()
                {
                    CsspOrderId = cartId,
                    CsspCartNumber = x.CartNumber,
                    SupportAccountId = x.SupportAccountId,
                    SapOrderNumber = x.SapOrderId
                }).FirstOrDefault();

            if (csspOrder != null)
            {
                var orderItems = (from x in db.csspshoppingcarts
                    join y in db.csspshoppingcartitems on x.CsspShoppingCartId equals y.CsspShoppingCartId
                    join z in db.csspskus on y.CsspSkuId equals z.CsspSkuId
                    join pm in db.partmasters on z.PartMasterId equals pm.PartMasterId
                    where x.CsspShoppingCartId == cartId
                    select new CsspOrderItems()
                    {
                        CsspProductId = z.CsspSkuId,
                        PartNumber = pm.PartNumber,
                        SkuDescription = z.SkuDescription
                    }).ToList();

                csspOrder.OrderItems = orderItems;

            }

            return csspOrder;

        }


        public CsspOrderSapInput GetSapInput(long supportAccountId, int cartId)
        {
            var db = SessionDbContext.CspObjectContext;

            var sapRequest = new CsspOrderSapInput();
            sapRequest.SupportAccountId = supportAccountId;
            sapRequest.CartId = cartId;

            var result = (from sc in db.csspshoppingcarts
                join sci in db.csspshoppingcartitems on sc.CsspShoppingCartId equals sci.CsspShoppingCartId
                join cssps in db.csspskus on sci.CsspSkuId equals cssps.CsspSkuId
                join pm in db.partmasters on cssps.PartMasterId equals pm.PartMasterId
                where sc.CsspShoppingCartId == cartId
                select pm.PartNumber).ToList();

            sapRequest.OrderItems = result;

            var sfdcIds = (from s in db.supportaccounts
                join cd in db.csspdatas on s.SupportAccountId equals cd.SupportAccountId
                where s.SupportAccountId == supportAccountId
                select new
                {
                    s.SalesforceAccountId,
                    cd.DistiSfdcAccountId,
                    cd.AscSfdcAccountId,
                    cd.SfdcAccountOwnerUserId,
                    cd.SoldToSalesOrg
                }).FirstOrDefault();

            if (sfdcIds != null)
            {
                sapRequest.SpSfdcAccountId = sfdcIds.SalesforceAccountId;
                if (string.IsNullOrEmpty(sfdcIds.DistiSfdcAccountId) || sfdcIds.DistiSfdcAccountId == "null")
                {
                    sapRequest.DistiSfdcAccountId = sapRequest.SpSfdcAccountId;
                }
                else
                {
                    sapRequest.DistiSfdcAccountId = sfdcIds.DistiSfdcAccountId;
                }
                if (string.IsNullOrEmpty(sfdcIds.AscSfdcAccountId) || sfdcIds.AscSfdcAccountId == "null")
                {
                    sapRequest.AscSfdcAccountId = sapRequest.SpSfdcAccountId;
                }
                else
                {
                    sapRequest.AscSfdcAccountId = sfdcIds.AscSfdcAccountId;
                }
                sapRequest.SalesRepAccountId = sfdcIds.SfdcAccountOwnerUserId;
                sapRequest.SoldToSalesOrg = sfdcIds.SoldToSalesOrg;
            }
            return sapRequest;
        }



        public bool SetSapOrderNumberForOrder(int cartId, string sapOrderNum)
        {
            using (var db = new cspEntities())
            {
                var csspOrder = (from x in db.csspshoppingcarts
                    where x.CsspShoppingCartId == cartId
                    select x).FirstOrDefault();
                if (csspOrder != null)
                {
                    csspOrder.SapOrderId = sapOrderNum;
                    csspOrder.CsspOrderStatusId = 3; //Pending
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }


        private bool EnrichCsspOrderLineItem(int shoppingCartItemId, out string authCode)
        {
            authCode = "<div class=\"cssp-pending-order\">Pending</div>";

            var db = SessionDbContext.CspObjectContext;

            var authCodeResults = (from cart in db.csspshoppingcarts
                join cartitem in db.csspshoppingcartitems on cart.CsspShoppingCartId equals cartitem.CsspShoppingCartId
                join sku in db.csspskus on cartitem.CsspSkuId equals sku.CsspSkuId
                join mod in db.csspmodels on sku.CsspModelId equals mod.CsspModelId
                join so in db.salesorders on cart.SapOrderId equals so.SAPSalesOrderNumber
                join soi in db.salesorderitems on so.SalesOrderId equals soi.SalesOrderId
                join l in db.licenses on new {LicenseId = (long) soi.LicenseId, sku.PartMasterId} equals
                    new {l.LicenseId, l.PartMasterId}
                where cartitem.CsspShoppingCartItemsId == shoppingCartItemId
                select l.AuthorizationCode).FirstOrDefault();

            if (authCodeResults != null)
            {
                authCode = authCodeResults;
                return true;
            }
            return false;
        }


        public IEnumerable<CsspOrderHistoryItems> GetOrderHistoryItems(int csspOrderId)
        {
            var db = SessionDbContext.CspObjectContext;

            var orderItems = (from item in db.csspshoppingcartitems
                join sku in db.csspskus on item.CsspSkuId equals sku.CsspSkuId
                join pm in db.partmasters on sku.PartMasterId equals pm.PartMasterId
                where item.CsspShoppingCartId == csspOrderId
                select new CsspOrderHistoryItems()
                {
                    CsspShoppingCartId = csspOrderId,
                    CsspShoppingCartItemId = item.CsspShoppingCartItemsId,
                    PartNumber = pm.PartNumber,
                    SkuDescription = sku.SkuDescription,
                    SkuPartmasterId = pm.PartMasterId,
                    UseCount = pm.UseCount,
                    AuthorizationCode = string.Empty
                }).ToList();

            if (orderItems.Count > 0)
            {
                foreach (var oi in orderItems)
                {
                    string authCode;
                    var result = EnrichCsspOrderLineItem(oi.CsspShoppingCartItemId, out authCode);
                    oi.AuthorizationCode = authCode;
                }
            }
            return orderItems;
        }

        public IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(long supportAccountId, string serialNumber = "", string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "", string contactEmail = "", DataParameters parameters = null)
        {
            var db = SessionDbContext.CspObjectContext;
            var spskuQ = (from spsku in db.csspskus select spsku.PartMasterId);
            var provisionedDevices = (from d in db.devices
                                      //Reducing the cost of LINQ Stmt - (JIRA# GCSOS-866) - MM
                                      //join l in db.licenses on d.DeviceId equals l.DeviceId
                                      join l in db.licenses.Where(l => l.AuthorizationCode != null) on d.DeviceId equals l.DeviceId
                                      //Reducing the cost of LINQ Stmt - (JIRA# GCSOS-866) - MM
                                      //join l1 in db.licenses on l.VMParentLicenseId equals l1.LicenseId
                                      join l1 in db.licenses.Where(l1 => l1.SupportAccountId == supportAccountId) on l.VMParentLicenseId equals l1.LicenseId
                                      join pm in db.partmasters on l1.PartMasterId equals pm.PartMasterId
                                      //Reducing the cost of LINQ Stmt - (JIRA# GCSOS-866) - MM
                                      //join spsku in db.csspskus on pm.PartMasterId equals spsku.PartMasterId
                                      join eudevices in db.csspenduserdevices on d.DeviceId equals eudevices.ProvisionedDeviceId into eudeviceinfo
                                      from eudevices in eudeviceinfo.DefaultIfEmpty()
                                      join eusers in db.csspenduserinformations on eudevices.CsspEndUserInformationId equals eusers.CsspEndUserInformationId into euserinfo
                                      from eusers in euserinfo.DefaultIfEmpty()
                                      //Reducing the cost of LINQ Stmt - (JIRA# GCSOS-866) - MM
                                      //where l1.SupportAccountId == supportAccountId && l.AuthorizationCode !=null
                                      where spskuQ.Contains(pm.PartMasterId)
                                      select new CsspProvisionedDevices()
                                      {
                                          DeviceId = d.DeviceId,
                                          MasterAuthCode = l1.AuthorizationCode,
                                          SerialNumber = d.SerialNumber,
                                          Model = pm.ModelCompatibility,
                                          DeviceName = d.DeviceName,
                                          EndUserCustomerAccountId = eusers != null ? eusers.CustomerReferenceId : string.Empty,
                                          EndUserAddress = eusers != null ? eusers.Address : string.Empty,
                                          EndUserCompany = eusers != null ? eusers.CompanyName : string.Empty,
                                          EndUserContactEmail = eusers != null ? eusers.ContactEmail : string.Empty,
                                          EndUserCity = eusers != null ? eusers.City : string.Empty,
                                          EndUserState = eusers != null ? eusers.State : string.Empty,
                                          EndUserPostalCode = eusers != null ? eusers.PostalCode : string.Empty,
                                          EndUserCountry = eusers != null ? eusers.Country : string.Empty,
                                          EndUserPhoneNumber = eusers != null ? eusers.PhoneNumber : string.Empty,
                                          EndUserCompanyWebsite = eusers != null ? eusers.CompanyWebsite : string.Empty,
                                          EndUserIndustry = eusers != null ? eusers.Industry : string.Empty,
                                          DnbNumber = eusers != null ? eusers.DnBNumber : string.Empty,
                                          ProvisionedDate = d.RegistrationDate.HasValue ? d.RegistrationDate.Value.ToString() : string.Empty,
                                          HasEndUserInfo = eusers != null ? true : false
                                      }
                );

            if (!string.IsNullOrEmpty(authorizationCode))
            {
                provisionedDevices = provisionedDevices.Where(x => x.MasterAuthCode == authorizationCode);
            }

            if (!string.IsNullOrEmpty(serialNumber))
            {
                provisionedDevices = provisionedDevices.Where(x => x.SerialNumber == serialNumber);
            }

            if (!string.IsNullOrEmpty(endUserAccountId))
            {
                provisionedDevices = provisionedDevices.Where(x => x.EndUserCustomerAccountId == endUserAccountId);
            }

            if (!string.IsNullOrEmpty(endUserCompany))
            {
                provisionedDevices = provisionedDevices.Where(x => x.EndUserCompany.StartsWith(endUserCompany));
            }

            if (!string.IsNullOrEmpty(contactEmail))
            {
                provisionedDevices = provisionedDevices.Where(x => x.EndUserContactEmail == contactEmail);
            }

            var devices = provisionedDevices.ToList();

            if (parameters != null)
            {
                parameters.TotalRecordCount = devices.Count;
                return devices.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount).ToArray();
            }
            else
            {
                return devices;
            }

        }

				public CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> GetCsspProvisionedDevicesMonthly(long supportAccountId, int year, int month) {
					var data = new CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> {
						SupportAccountId = supportAccountId,
						Month = month,
						Year = year
					};
					var csspProvisionedDevices = new List<CsspProvisionedDevicesMonthly>();
					var query = @"
SELECT DISTINCT w.DeviceId, p.PartNumber, w.SerialNumber, w.RegistrationDate, w.StartDate, w.EndDate, w.DeactivationDate, w.TermEndDate,
  w.OriginalAuthcode, p.ModelCompatibility, i.CsspEndUserInformationId, i.CustomerReferenceId, i.DnBNumber,
  i.CompanyName, i.ContactEmail, i.Country, i.State, i.City, i.PostalCode, i.PhoneNumber, i.CompanyWebsite,
  i.Industry, w.DeviceName
  FROM
  (
    SELECT d.DeviceId, u.PartNumber, u.SerialNumber, u.RegistrationDate, u.StartDate, u.EndDate, u.DeactivationDate, u.TermEndDate,
      u.OriginalAuthcode, d.DeviceName
      FROM csp.csspautoinvoicing a
      JOIN csp.csspvmusage u ON u.CsspAutoInvoicingId = a.Id
      JOIN csp.partmaster p ON u.PartNumber = p.PartNumber
      JOIN csp.device d ON u.SerialNumber = d.SerialNumber
      WHERE a.IsSfdcUploadSuccessful = 1
      AND a.CSSPSupportAccountId = @supportAccountId
      AND a.Year = @year
      AND a.Month = @month
    UNION
    SELECT DISTINCT b.lid, a.PartNumber, a.SerialNumber, a.RegistrationDate, a.StartDate, a.EndDate, a.DeactivationDate, a.TermEndDate,
      a.OriginalAuthcode, b.device_name DeviceName
      FROM
      (
        SELECT u.PartNumber, u.SerialNumber, u.RegistrationDate, u.StartDate, u.EndDate, u.DeactivationDate, u.TermEndDate,
          u.OriginalAuthcode
          FROM csp.csspautoinvoicing a
          JOIN csp.csspvmusage u ON u.CsspAutoInvoicingId = a.Id
          JOIN csp.partmaster p ON u.PartNumber = p.PartNumber
          LEFT JOIN csp.device d ON u.SerialNumber = d.SerialNumber
          WHERE a.IsSfdcUploadSuccessful = 1
          AND a.CSSPSupportAccountId = @supportAccountId
          AND a.Year = @year
          AND a.Month = @month
          AND d.DeviceId IS NULL
      ) a
      JOIN pandb.device_master_bkp b ON a.SerialNumber = b.serialnum -- AND (b.reason = 'Deactivate' OR b.reason LIKE 'AdminSite Reset%')
  ) w
  JOIN csp.partmaster p ON w.PartNumber = p.PartNumber
  LEFT JOIN csp.csspenduserdevices c ON w.DeviceId = c.ProvisionedDeviceId
  LEFT JOIN csp.csspenduserinformation i ON c.CsspEndUserInformationId = i.CsspEndUserInformationId;";
					var querySa = @"
SELECT s.SupportAccountId, s.Name
  FROM supportaccount s
  WHERE s.supportAccountId = @supportAccountId;";

                    using (var connection = new MySqlConnection(Secrets.CspDatabase))
                    {
						connection.Open();
						using (var command = new MySqlCommand(query, connection)) {
							command.Parameters.Add("@supportAccountId", MySqlDbType.Int64).Value = supportAccountId;
							command.Parameters.Add("@year", MySqlDbType.Int32).Value = year;
							command.Parameters.Add("@month", MySqlDbType.Int32).Value = month;
							using (var dataReader = command.ExecuteReader()) {
								while (dataReader.Read()) {
									csspProvisionedDevices.Add(new CsspProvisionedDevicesMonthly {
										PartNumber = dataReader.GetColumnValue<string>("PartNumber"),
										SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
										StartDate = dataReader.GetColumnValue<DateTime>("StartDate").ToShortDateString(),
										EndDate = dataReader.GetColumnValue<DateTime>("EndDate").ToShortDateString(),
										ProvisionedDate = dataReader.GetColumnValue<DateTime>("RegistrationDate").ToShortDateString(),
										DeactivationDate = dataReader.GetColumnValue<DateTime?>("DeactivationDate").HasValue
												? dataReader.GetColumnValue<DateTime?>("DeactivationDate").Value.ToShortDateString()
												: string.Empty,
										TermEndDate = dataReader.GetColumnValue<DateTime>("TermEndDate").ToShortDateString(),
										MasterAuthCode = dataReader.GetColumnValue<string>("OriginalAuthcode"),
										Model = dataReader.GetColumnValue<string>("ModelCompatibility"),
										EndUserCustomerAccountId = dataReader.GetColumnValue<string>("CustomerReferenceId"),
										DnbNumber = dataReader.GetColumnValue<string>("DnBNumber"),
										EndUserCompany = dataReader.GetColumnValue<string>("CompanyName"),
										EndUserContactEmail = dataReader.GetColumnValue<string>("ContactEmail"),
										EndUserCountry = dataReader.GetColumnValue<string>("Country"),
										EndUserState = dataReader.GetColumnValue<string>("State"),
										EndUserCity = dataReader.GetColumnValue<string>("City"),
										EndUserPostalCode = dataReader.GetColumnValue<string>("PostalCode"),
										EndUserPhoneNumber = dataReader.GetColumnValue<string>("PhoneNumber"),
										EndUserCompanyWebsite = dataReader.GetColumnValue<string>("CompanyWebsite"),
										EndUserIndustry = dataReader.GetColumnValue<string>("Industry"),
										DeviceName = dataReader.GetColumnValue<string>("DeviceName")
									});
								}
							}
						}
						using (var command = new MySqlCommand(querySa, connection)) {
							command.Parameters.Add("@supportAccountId", MySqlDbType.Int64).Value = supportAccountId;
							using (var dataReader = command.ExecuteReader()) {
								if (dataReader.Read()) {
									data.SupportAccountName = dataReader.GetColumnValue<string>("Name");
								}
							}
						}
					}

					data.Items = csspProvisionedDevices ?? new List<CsspProvisionedDevicesMonthly>();
					return data;
				}

        public IEnumerable<CsspEndUser> SearchEndUser(long csspSupportAccountId, string searchField, string searchValue)
        {
            switch (searchField)
            {
                case "AccountId":
                    return SearchEndUsersByEndUserAccountId(csspSupportAccountId,searchValue);
                default:
                    return SearchEndUsersByEndUserAccountName(csspSupportAccountId,searchValue);
            }
        }

        private IEnumerable<CsspEndUser> SearchEndUsersByEndUserAccountId(long csspSupportAccountId, string enduserAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var result = (from x in db.csspenduserinformations
                          join y in db.countries on x.Country equals  y.Code
                where x.CustomerReferenceId == enduserAccountId && x.CsspSupportAccountId == csspSupportAccountId
                select new CsspEndUser()
                {
                    CSSPEndUsersId = x.CsspEndUserInformationId,
                    CSSPSupportAccountId = x.CsspSupportAccountId,
                    CustomerAccountId = x.CustomerReferenceId,
                    CompanyName = x.CompanyName,
                    DnBNumber = x.DnBNumber,
                    Address = x.Address,
                    Country = x.Country,
                    CountryId = y.CountryId,
                    Region = x.State,
                    City = x.City,
                    PostalCode = x.PostalCode,
                    EndUserContactEmail = x.ContactEmail,
                    PhoneNumber = x.PhoneNumber,
                    CompanyWebsite = x.CompanyWebsite,
                    Industry = x.Industry,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate
                }).FirstOrDefault();
            return new List<CsspEndUser>() { result };
        }

        private IEnumerable<CsspEndUser> SearchEndUsersByEndUserAccountName(long csspSupportAccountId, string enduserAccountName)
        {
            var db = SessionDbContext.CspObjectContext;

            var result = (from x in db.csspenduserinformations
                          join y in db.countries on x.Country equals y.Code
                          where x.CompanyName.StartsWith(enduserAccountName) && x.CsspSupportAccountId == csspSupportAccountId
                          select new CsspEndUser()
                          {
                              CSSPEndUsersId = x.CsspEndUserInformationId,
                              CSSPSupportAccountId = x.CsspSupportAccountId,
                              CustomerAccountId = x.CustomerReferenceId,
                              CompanyName = x.CompanyName,
                              DnBNumber = x.DnBNumber,
                              Address = x.Address,
                              Country = x.Country,
                              CountryId = y.CountryId,
                              Region = x.State,
                              City = x.City,
                              PostalCode = x.PostalCode,
                              EndUserContactEmail = x.ContactEmail,
                              PhoneNumber = x.PhoneNumber,
                              CompanyWebsite = x.CompanyWebsite,
                              Industry = x.Industry,
                              CreatedBy = x.CreatedBy,
                              CreatedDate = x.CreatedDate
                          }).ToList();
            return result;
        }


        public bool IsUserCssp(long userAccountId, long supportAccountId)
        {
            var now = DateTime.Now;
            var db = SessionDbContext.CspObjectContext;
            var hasMembership = (
                from m in db.memberships
                join mr in db.membershiproles on m.MembershipId equals mr.MembershipId
                join r in db.roles on mr.RoleId equals r.RoleId
                where m.SupportAccountId == supportAccountId
                            && m.UserAccountId == userAccountId
                            && r.Name == RoleNames.CSSP
                            && m.ActivationDate < now
                            && (
                                m.ExpirationDate == null
                                || m.ExpirationDate > now)

                select m
                ).Any();
            return hasMembership;
        }


        public bool UpdateSupportAccountForProvisionedDevice(long endUserDeviceId, long userAccountId, long supportAccountId)
        {
            bool result = false;
            var db = SessionDbContext.CspObjectContext;
                // Update Support Account of device
                var device = (from x in db.devices
                    where x.DeviceId == endUserDeviceId
                    select x).FirstOrDefault();

                var licenses = (from x in db.licenses
                    where x.DeviceId == endUserDeviceId
                    select x).ToList();

                if (device != null)
                {
                    result = true;
                    device.SupportAccountId = supportAccountId;
                    device.LastUpdateByUserAccountId = userAccountId;
                    device.LastUpdate = System.DateTime.Now;

                    foreach (var license in licenses)
                    {
                        license.SupportAccountId = supportAccountId;
                        license.LastUpdateByUserAccountId = userAccountId;
                        license.LastUpdate = System.DateTime.Now;
                    }
                    db.SaveChanges();
                }
            return result;
        }


        public bool ValidateEndUserSerialNumber(string serialNumber, out string eMessage)
        {
            bool hasPassedValidation = false;
            eMessage = string.Empty;
            var db = SessionDbContext.CspObjectContext;

            var devId = (from x in db.devices where x.SerialNumber == serialNumber select x.DeviceId).FirstOrDefault();
            if (devId < 1)
            {
                eMessage = "Serial Number does not exist";
                return false;
            }
            var isRegisteredToCsspAccount = db.devices.Where(x => x.SerialNumber == serialNumber).Select(x => x.supportaccount.IsCssp).Any();

            if (!isRegisteredToCsspAccount)
            {
                // Check if its already registered to end user account. Self service reg of same device by multiple users is allowed.

                var canProceed = (from x in db.devices
                    join y in db.csspenduserdevices on x.DeviceId equals y.ProvisionedDeviceId
                    join z in db.csspenduserpanaccounts on y.CsspEndUserInformationId equals z.CsspEndUserInformationId
                    select x.SupportAccountId).Any();

                if (!canProceed)
                {
                    eMessage = "01-This Serial Number is not valid";
                    return false;
                }
            }

            var hasCsspLicense = (from x in db.licenses
                join y in db.devices on x.DeviceId equals y.DeviceId
                join z in db.csspskus on x.PartMasterId equals z.PartMasterId
                select x).Any();

            if (hasCsspLicense)
            {
                hasPassedValidation = true;
            }
            else
            {
                eMessage = "02-This Serial Number is not valid";
            }

            return hasPassedValidation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerAccountId">{Supportaccountid}-{ReferenceId}</param>
        /// <param name="eMessage"></param>
        /// <returns></returns>
        public bool ValidateEndUserCustomerAccountId(string customerAccountId, out string eMessage)
        {
            // Validate Customer Reference Id
            eMessage = string.Empty;
            var db = SessionDbContext.CspObjectContext;
            var isValidCustomerAccountId = customerAccountId.Contains("-");
            if (!isValidCustomerAccountId)
            {
                eMessage = "01-CSSP Customer Reference Id is not valid";
                return false;
            }
            var endUserAccountInfo = customerAccountId.Split('-');
            
            var csspSupAccountId = Convert.ToInt64(customerAccountId.Split('-')[0]);
            var csspEndUserAccountReferenceId = string.Join("-", customerAccountId.Split('-').Skip(1));

            var isCsspSupportAccount =
                db.supportaccounts.Any(x => x.SupportAccountId == csspSupAccountId && x.IsCssp);

            if (!isCsspSupportAccount)
            {
                eMessage = "02CSSP Customer Reference Id is not valid";
                return false;
            }

            var isValidRefId =
                db.csspenduserinformations.Any(x => x.CustomerReferenceId == csspEndUserAccountReferenceId);

            if (!isValidRefId)
            {
                eMessage = "03-CSSP Customer Reference Id is not valid";
                return false;
            }
            return true;
        }


        public bool ValidateCsspEndUserInputCombination(string serialNumber, string customerAccountId, out string eMessage)
        {
            var hasPassedSerialNumberValidation = ValidateEndUserSerialNumber(serialNumber, out eMessage);
            if (hasPassedSerialNumberValidation)
            {
                var hasPassedCustomerAccountIdValidation = ValidateEndUserCustomerAccountId(customerAccountId,
                    out eMessage);
                if (hasPassedCustomerAccountIdValidation)
                {
                    var db = SessionDbContext.CspObjectContext;
                    // Device Registered To What Account?
                    var supAccountId = db.devices.Where(x=>x.SerialNumber == serialNumber).Select(x=>x.SupportAccountId).FirstOrDefault();
                    var supAccountIdFromCustomerAccountIdInput = Convert.ToInt64(customerAccountId.Split('-')[0]);
                    if (supAccountId != supAccountIdFromCustomerAccountIdInput)
                    {
                        eMessage = "Cssp Customer Reference Id and Serial Number entered is not a valid combination";
                    }
                    return true;
                }
            }
            return false;
        }


        public bool IsSupportAccountForEndUserAlreadyExist(string customerAccountId, long csspSupportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var isExist = (from x in db.csspenduserinformations
                join y in db.csspenduserpanaccounts on x.CsspEndUserInformationId equals y.CsspEndUserInformationId
                where x.CustomerReferenceId == customerAccountId && x.CsspSupportAccountId == csspSupportAccountId
                select y.SupportAccountId).FirstOrDefault();
            return isExist.HasValue;
        }

        public bool UpdateSupportAccountIdForCsspEndUser(string customerAccountId, long csspSupportAccountId, long endUserSupportAccountId, string sfdcAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var panRecord = (from x in db.csspenduserinformations
                join y in db.csspenduserpanaccounts on x.CsspEndUserInformationId equals y.CsspEndUserInformationId
                where x.CsspSupportAccountId == csspSupportAccountId && x.CustomerReferenceId == customerAccountId
                select y).FirstOrDefault();

             var enduserId =
                    db.csspenduserinformations.Where(
                        x =>
                            x.CustomerReferenceId == customerAccountId && x.CsspSupportAccountId == csspSupportAccountId)
                        .Select(x => x.CsspEndUserInformationId).FirstOrDefault();

            if (panRecord != null)
            {
                panRecord.SupportAccountId = endUserSupportAccountId;
            }
            else
            {
                db.csspenduserpanaccounts.Add(new csspenduserpanaccount()
                {
                    CsspEndUserInformationId = enduserId,
                    SupportAccountId = endUserSupportAccountId
                });
            }

            db.csspenduserpanaccountslogs.Add(new csspenduserpanaccountslog()
            {
                CsspEnduserInformationId = enduserId,
                LogCreatedBy = "CSP User",
                LogType = "INFO",
                PanAccountType = "CSP",
                Notes = string.Format("CSP Account ID is {0} and SFDC Account associated is {1}",endUserSupportAccountId,sfdcAccountId)
            });

            return db.SaveChanges() > 0;
        }


        public bool LogOrderXML(int shoppingCartId, string orderXml, string sapResponse)
        {
            using (var db = new cspEntities())
            {
                db.cssporderxmls.Add(new cssporderxml()
                {
                    CsspShoppingCartId = shoppingCartId,
                    OrderXml = orderXml,
                    SapResponse = sapResponse
                });
                return db.SaveChanges() > 0;
            }
        }


        public bool UpdatePanAccountForEndUser(long deviceId, long supportAccountId, out long? endUserId, out bool isCspAccountUpdated, out bool isSfdcAccountUpdated)
        {
            endUserId = null;
            isCspAccountUpdated = false;
            isSfdcAccountUpdated = false;
            var db = SessionDbContext.CspObjectContext;
            var enduserId =
                (from x in db.csspenduserdevices where x.ProvisionedDeviceId == deviceId select x.CsspEndUserInformationId)
                    .FirstOrDefault();
            if (enduserId > 0)
            {
                endUserId = enduserId;
                var enduserPanAccounts =
                    (from x in db.csspenduserpanaccounts where x.CsspEndUserInformationId == enduserId select x)
                        .FirstOrDefault();

                var sfdcAccountId =
                            (from x in db.supportaccounts
                                where x.SupportAccountId == supportAccountId
                                select x.SalesforceAccountId).FirstOrDefault();

                if (enduserPanAccounts != null)
                {
                    if (!enduserPanAccounts.SupportAccountId.HasValue)
                    {
                        isCspAccountUpdated = true;
                        enduserPanAccounts.SupportAccountId = supportAccountId;
                    }

                    if (string.IsNullOrEmpty(enduserPanAccounts.SFDCAccountId))
                    {
                        isSfdcAccountUpdated = true;
                        enduserPanAccounts.SFDCAccountId = sfdcAccountId;
                    }
                    if (isCspAccountUpdated || isSfdcAccountUpdated)
                    {
                        return db.SaveChanges() > 0;
                    }
                }
                else
                {
                    isCspAccountUpdated = true;
                    isSfdcAccountUpdated = true;
                    db.csspenduserpanaccounts.Add(new csspenduserpanaccount()
                    {
                        SupportAccountId = supportAccountId,
                        SFDCAccountId = sfdcAccountId,
                        CsspEndUserInformationId = enduserId
                    });
                    return db.SaveChanges() > 0;
                }

            }
            return false;
        }


        public bool UpdatePanAccountForEndUser(long csspSupportAccountId, string customerAccountId, long? customerSupportAccountId, string defaultSfdcAccountId, string sfdcAccountId, long? supportAccountId, out string errorMessage)
        {
            errorMessage = string.Empty;
            using (var db = new cspEntities())
            {

                if (db.csspdatas.Any(x =>x.SupportAccountId == csspSupportAccountId && x.DefaultSfdcAccountId == defaultSfdcAccountId))
                {
                    if (customerSupportAccountId.HasValue) // End Customer might have already created a Support Account
                    {
                        var currentSupAcct =
                            db.supportaccounts
                                .FirstOrDefault(x => x.SupportAccountId == customerSupportAccountId.Value);
                        if (currentSupAcct != null)
                        {
                            currentSupAcct.SalesforceAccountId = sfdcAccountId;
                        }
                    }

                    var accountRecord = (from x in db.csspenduserinformations
                        join y in db.csspenduserpanaccounts on x.CsspEndUserInformationId equals
                            y.CsspEndUserInformationId
                        where
                            x.CustomerReferenceId == customerAccountId && x.CsspSupportAccountId == csspSupportAccountId
                        select y).FirstOrDefault();

                    if (accountRecord != null)
                    {
                        accountRecord.SFDCAccountId = sfdcAccountId;
                        accountRecord.SupportAccountId = supportAccountId;
                    }

                    return db.SaveChanges() > 0;
                }
                else
                {
                    errorMessage = "Invalid SFDC Default Account for CSSP!";
                    return false;
                }
            }
        }


        public bool StoreMultipleSfdcAccountMatchesForEndUser(string referenceId, long csspSupportAccountId, string[] sfdcAccountIds, string preferredSfdcAccountId)
        {
            using (var db = new cspEntities())
            {
                var userId = (from x in db.csspenduserinformations
                    where x.CustomerReferenceId == referenceId && x.CsspSupportAccountId == csspSupportAccountId
                    select x.CsspEndUserInformationId).FirstOrDefault();

                if (userId > 0)
                {
                    foreach (var sfdcId in sfdcAccountIds)
                    {
                        db.csspendusermultiplesfdcmatches.Add(new csspendusermultiplesfdcmatch()
                        {
                            CsspEndUserInformationId = userId,
                            SfdcAccountId = sfdcId,
                            IsBestMatch = sfdcId == preferredSfdcAccountId
                        });
                    }
                    return db.SaveChanges() > 0;
                }
            }
            return false;
        }


        public bool StoreSfdcAccountForEndUser(string referenceId, long csspSupportAccountId, string sfdcAccountId)
        {
            using (var db = new cspEntities())
            {
                var userId = (from x in db.csspenduserinformations
                    where x.CustomerReferenceId == referenceId && x.CsspSupportAccountId == csspSupportAccountId
                    select x.CsspEndUserInformationId).FirstOrDefault();

                if (userId > 0)
                {

                    //Assumption: If there are multiple support accounts for the sfdc account, select the one that was created first.

                    var supAccountId = (from x in db.supportaccounts
                        where x.SalesforceAccountId == sfdcAccountId
                        orderby x.CreatedOn
                        select x.SupportAccountId).FirstOrDefault();

                    var endUserPanAccount = (from x in db.csspenduserpanaccounts
                        where x.CsspEndUserInformationId == userId
                        select x).FirstOrDefault();

                    if (supAccountId > 0)
                    {
                        if (endUserPanAccount != null)
                        {
                            endUserPanAccount.SupportAccountId = supAccountId;
                            endUserPanAccount.SFDCAccountId = sfdcAccountId;
                        }
                        else
                        {
                            db.csspenduserpanaccounts.Add(new csspenduserpanaccount()
                            {
                                CsspEndUserInformationId = userId,
                                SupportAccountId = supAccountId,
                                SFDCAccountId = sfdcAccountId
                            });
                        }
                    }
                    else
                    {
                        if (endUserPanAccount != null)
                        {
                            endUserPanAccount.SFDCAccountId = sfdcAccountId;
                        }
                        else
                        {
                            db.csspenduserpanaccounts.Add(new csspenduserpanaccount()
                            {
                                CsspEndUserInformationId = userId,
                                SFDCAccountId = sfdcAccountId
                            });
                        }
                    }

                }
                return db.SaveChanges() > 0;
            }
        }


        public bool LogCsspPanAccountCreationDetails(long enduserId, string panAccountType, string logType, string notes, string createdBy)
        {
            using (var db = new cspEntities())
            {
                db.csspenduserpanaccountslogs.Add(new csspenduserpanaccountslog()
                {
                    CsspEnduserInformationId = enduserId,
                    PanAccountType = panAccountType,
                    LogType = logType,
                    Notes = notes,
                    LogCreatedBy = createdBy
                });
                return db.SaveChanges() > 0;
            }
        }


        public void GetEndUserSupportAccount(string customerAccountId, long csspSupportAccountId, out long? enduserSupportAccountId, out string sfdcAccount)
        {
            enduserSupportAccountId = null;
            sfdcAccount = null;
            var db = SessionDbContext.CspObjectContext;
            var result = (from x in db.csspenduserinformations
                                    join y in db.csspenduserpanaccounts on x.CsspEndUserInformationId equals y.CsspEndUserInformationId
                                    where x.CustomerReferenceId == customerAccountId && x.CsspSupportAccountId == csspSupportAccountId
                                    select new { y.SupportAccountId, y.SFDCAccountId}).FirstOrDefault();
            if (result != null)
            {
                enduserSupportAccountId = result.SupportAccountId;
                sfdcAccount = result.SFDCAccountId;
            }
        }


        public bool CanGeneratePrelicenseKeys(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            return db.csspdatas.Any(x => x.SupportAccountId == supportAccountId && x.CanGeneratePreLicenseKeys);
        }

        private string GenerateBatchId()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        private string GetNewBatchId()
        {
            string batchId = string.Empty;
            bool keepChecking = true;
            while (keepChecking)
            {
                batchId = GenerateBatchId();
                keepChecking = IsBatchIdAlreadyTaken(batchId);
            }
            return batchId;
        }

        private bool IsBatchIdAlreadyTaken(string batchId)
        {
            var db = SessionDbContext.CspObjectContext;
            return db.csspprelicenserequests.Any(x => x.BatchId == batchId);
        }


        public int CreatePrelicenseRequest(long supportAccountId, string authorizationCode, int quantity, string createdBy, out string batchId)
        {
            batchId = GetNewBatchId();
            var db = SessionDbContext.CspObjectContext;
            var newRequest = db.csspprelicenserequests.Add(new csspprelicenserequest()
            {
                AuthorizationCode = authorizationCode,
                CsspSupportAccountId = supportAccountId,
                Quantity = quantity,
                RequestedBy = createdBy,
                RequestedDate = DateTime.Now,
                StatusId = 1,
                BatchId = batchId
            });
            db.SaveChanges();
            return newRequest.CsspPrelicenseRequestId;
        }

        public IEnumerable<CsspPreLicenseDevice> GetCsspPrelicenseDevices(DataParameters parameters, long supportAccountId, string authorizationCode, string csspBatchId = "", string serialNumber = "")
        {
            var db = SessionDbContext.CspObjectContext;
            var csspLicenseKeys = (from d in db.devices
                                      join l in db.licenses on d.DeviceId equals l.DeviceId
                                      join l1 in db.licenses on l.VMParentLicenseId equals l1.LicenseId
                                      join pm in db.partmasters on l1.PartMasterId equals pm.PartMasterId
                                      where l1.SupportAccountId == supportAccountId
                                   select new CsspPreLicenseDevice()
                                      {
                                          DeviceId = d.DeviceId,
                                          MasterAuthCode = l1.AuthorizationCode,
                                          SerialNumber = d.SerialNumber,
                                          Model = pm.ModelCompatibility,
                                          ProvisionedDate = d.RegistrationDate.HasValue ? d.RegistrationDate.Value.ToString() : string.Empty,
                                      }
                );

            if (!string.IsNullOrEmpty(authorizationCode))
            {
                csspLicenseKeys = csspLicenseKeys.Where(x => x.MasterAuthCode == authorizationCode);
            }

            if (!string.IsNullOrEmpty(serialNumber))
            {
                csspLicenseKeys = csspLicenseKeys.Where(x => x.SerialNumber == serialNumber);
            }

            if (!string.IsNullOrEmpty(csspBatchId))
            {
                var deviceIds = GetPreLicensedDeviceIdsByRequestId(csspBatchId);
                csspLicenseKeys = csspLicenseKeys.Where(x => deviceIds.Contains(x.DeviceId));
            }

            var licenseKeys = csspLicenseKeys.ToList();

            parameters.TotalRecordCount = licenseKeys.Count;
            return licenseKeys.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount);
        }

        public List<long> GetPreLicensedDeviceIdsByRequestId(string batchId)
        {
            using (var db = new cspEntities())
            {
                var ldeviceIds = (from l in db.licenses
                    join r in db.csspprelicenserequests on l.PreLicenseId equals r.CsspPrelicenseRequestId
                    where r.BatchId == batchId && l.DeviceId.HasValue
                    select l.DeviceId.Value).Distinct().ToList();
                return ldeviceIds;
            }
        }


        public CsspDownloadLicense DownloadLicense(List<long> aDeviceId)
        {
            using (var db = new cspEntities())
            {
                var result = new CsspDownloadLicense();
                string sqlQuery = "SELECT  d.DeviceId,d.SerialNumber,p1.LicenseFeaturesName AS LicenseName, l.LicenseKey FROM csp.device d JOIN csp.license l ON d.DeviceId=l.DeviceId JOIN csp.partmaster p ON l.PartMasterId=p.PartMasterId JOIN csp.partsubtype p1 ON p.PartSubtypeId=p1.PartSubtypeId WHERE d.DeviceId IN (@csvDeviceIds) AND l.IsActive=1 AND l.LicenseKey IS NOT null ORDER BY d.DeviceId";
                var data = db.Database.SqlQuery<CsspDeviceLicense>(sqlQuery, new MySqlParameter("csvDeviceIds", string.Join(",", aDeviceId))).ToList();

                if (data.Count > 0)
                {
                    result.PrelicenseDeviceIds = aDeviceId;
                    result.Licenses = data;
                }
                return result;
            }
        }


        public int GetPreLicenseRequestStatusId(int requestId)
        {
            using (var db = new cspEntities())  
            {
                return db.csspprelicenserequests.Where(x => x.CsspPrelicenseRequestId == requestId)
                    .Select(x => x.StatusId)
                    .FirstOrDefault();
            }
        }


        public List<long> GetDeviceIdsToDownloadLicenseKeysByAuthorizationCode(long supportAccountId, string authorizationCode)
        {
            var db = SessionDbContext.CspObjectContext;

            if (!string.IsNullOrEmpty(authorizationCode))
            {
                var csspDeviceIds = (from d in db.devices
                    join l in db.licenses on d.DeviceId equals l.DeviceId
                    join l1 in db.licenses on l.VMParentLicenseId equals l1.LicenseId
                    where l1.SupportAccountId == supportAccountId && l1.AuthorizationCode == authorizationCode
                    select d.DeviceId).ToList();
                return csspDeviceIds;
            }
            else
            {
                var csspDeviceIds = (from d in db.devices
                                     join l in db.licenses on d.DeviceId equals l.DeviceId
                                     join l1 in db.licenses on l.VMParentLicenseId equals l1.LicenseId
                                     where l1.SupportAccountId == supportAccountId
                                     select d.DeviceId).ToList();
                return csspDeviceIds;
            }
        }

        public List<long> GetDeviceIdsToDownloadLicenseKeysByPrelicenseBatchId(long supportAccountId, string batchId)
        {
            return GetPreLicensedDeviceIdsByRequestId(batchId);
        }
    }
}
