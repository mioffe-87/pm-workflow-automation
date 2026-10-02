using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Csp.Api.Model;
using Csp.Api.HelperClasses;
using System.Net;
using System.Net.Http;

namespace Csp.Api.Logic
{
    public class ProductActivationPageService : IProductActivationPageService
    {
        private readonly ISearchAllLogic _searchAllLogic;
        private readonly IApplicationSettings _applicationSettings;

        public ProductActivationPageService(ISearchAllLogic searchAllLogic, IApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
            _searchAllLogic = searchAllLogic;
        }

        public GroupedData GetActivationPageData(long supportAccountId)
        {
            var result = new GroupedData();
            var dataParams = new DataParameters();

            var activationLinks = _searchAllLogic.GetActivationLinksLookup(dataParams, supportAccountId, "");
            MapToProductActivationItems(activationLinks, result);
            return result;
        }

        public dynamic GetProductActivationPageData(long supportAccountId)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Get.Method;
                var baseUrl = string.Format(ApiConstants.ProdcutActivationDataEndpoint, _applicationSettings.PortalBffHost, supportAccountId);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, string.Empty, true);
            }
            catch (WebException ex)
            {
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }

        private void MapToProductActivationItems(IEnumerable<ActivationLinksLookupItem> activationLinks, GroupedData result)
        {
            var filteredActivationLinks =
                activationLinks?.Where(x => x.Details.BundleLicenseId == null || x.Details.BundleLicenseId == "0");
            var readyForActivation =
                filteredActivationLinks.Where(x => string.IsNullOrWhiteSpace(x.Details.RegistrationDate));
            var activated =
                filteredActivationLinks.Where(x => !string.IsNullOrWhiteSpace(x.Details.RegistrationDate));

            result.ReadyForActivation = MapToProductActivationItems(readyForActivation);
            result.Activated = MapToProductActivationItems(activated);

        }

        private List<ProductActivationItem> MapToProductActivationItems(IEnumerable<ActivationLinksLookupItem> activationLinks)
        {
            var productActivationItems = new List<ProductActivationItem>();
            var activationHashGroups = activationLinks?.GroupBy(x => x.ActivationLinkHash);
            activationHashGroups.ForEach(activationHashGroup =>
            {
                var activationHashGroupData = activationHashGroup.ToList();
                var productActivationItem = new ProductActivationItem
                {
                    Products = new List<Product>(),
                    ActivationLink = activationHashGroup.Key,
                    Order = new Order(),
                    Status = activationHashGroupData.FirstOrDefault()?.Status,
                    Id = activationHashGroupData.FirstOrDefault()?.Id,
                    ActivationDate = activationHashGroupData.FirstOrDefault()?.Details.RegistrationDate
                };
                if (activationHashGroupData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.MasterOrderNumber)) != null)
                {
                    productActivationItem.Order.Master = new OrderInfo
                    {
                        Number = activationHashGroupData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.MasterOrderNumber))?.MasterOrderNumber
                    };
                }
                else if (activationHashGroupData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SalesOrderNumber)) != null)
                {
                    productActivationItem.Order.Regular = new OrderInfo
                    {
                        Number = activationHashGroupData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SalesOrderNumber))?.SalesOrderNumber
                    };
                }
                var productGroups = activationHashGroup.ToList().GroupBy(x => x.Details.SubGroup);
                productGroups.ForEach(productGroup =>
                {
                    var productGroupData = productGroup.ToList();
                    productActivationItem.Products.Add(new Product
                    {
                        Details = new List<Detail>
                        {
                            new Detail
                            {
                                Label = "Licenses",
                                Value = productGroupData.Count
                            }
                        },
                        ProductType = productGroup.Key,
                        Name = productGroupData.FirstOrDefault()?.Details.Product
                    });
                });
                productActivationItems.Add(productActivationItem);
            });
            return productActivationItems;
        }
    }
}
