using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Xsoar;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class XSOARLogic : BaseLogic, IXSOARLogic
    {
        private readonly ISfdcEntitlementLogic _sfdcLogic;
        private readonly INovaLogic _novaLogic;
        private readonly IXSOARDataAccess _xsoarDataAccess;
        private const string LicenseDownloadLink = "/SupportAccount/DownloadLicense?serialNumber={0}&status={1}";
        private const string DownloadEmptyDescription = "Download build in progress";
        private const string DownloadDescription = "XSOAR License Key";
        private const string AssetEnvironment = "Production";

        public XSOARLogic(ISfdcEntitlementLogic sfdcLogic, IXSOARDataAccess xsoarDataAccess, INovaLogic novaLogic)
        {
            _sfdcLogic = sfdcLogic;
            _xsoarDataAccess = xsoarDataAccess;
            _novaLogic = novaLogic;
        }

        /// <summary>
        /// get product details from sfdc api 
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public IEnumerable<XSOARListItem> GetXSOARProductDetails(long supportAccountId, string status)
        {
            List<XSOARListItem> xsoarItems = new List<XSOARListItem>();
            
            var accountId = _xsoarDataAccess.GetSalesForceAccountId(supportAccountId);

            var novaItems = _sfdcLogic.GetXSOAREntitlements(new SfdcEntitlementSearchCriteria
            {
                SearchType = SfdcEntitlementSearchType.AccountId,
                AccountId = accountId
            }, status).Where(i => i.Product != null && i.Product.ProductFamily == SfdcProductFamily.Xsoar);

            var serialNumbers = novaItems.GroupBy(x => x.SerialNumber).Select(y => y.First().SerialNumber).Distinct();

            foreach (var serialNumber in serialNumbers)
            {
                var licenseDesc = string.Empty;
                string startDate = string.Empty;
                string endDate = string.Empty;
                string statusDesc = string.Empty;

                var novaFilter = novaItems.Where(i => i.SerialNumber == serialNumber).OrderBy(i => i.Status).ThenByDescending(i => i.EffectivePeriod != null
                    && !string.IsNullOrEmpty(i.EffectivePeriod.EndDate)
                    ? Convert.ToDateTime(i.EffectivePeriod.EndDate)
                    : (DateTime?)null);

                var products = novaFilter.Select(i => i.Product != null ? i.Product : new SfdcEntitlementProduct());

                foreach (var product in products)
                {
                    var licfeatureDetail = string.Empty;
                    foreach (var licfeatue in product.LicensingFeatures)
                    {
                        if (licfeatue.Quantity == null && licfeatue.Workloads != null)
                        {
                            licfeatureDetail = licfeatureDetail + " : Workload = " + licfeatue.Workloads;
                        }
                        else
                        {
                            licfeatureDetail = licfeatureDetail + " : QTY = " + licfeatue.Quantity;
                        }
                    }
                    licenseDesc = string.IsNullOrEmpty(licenseDesc) ? product.Name + licfeatureDetail : !licenseDesc.Contains(product.Name) ? licenseDesc + "|" + product.Name + licfeatureDetail : licenseDesc;

                    var effectivePeriod = novaFilter.Where(i => i.Product != null && i.Product.Name == product.Name).Select(i => i.EffectivePeriod != null
                        ? i.EffectivePeriod : new SfdcEntitlementEffectivePeriod()).FirstOrDefault();

                    var productStatus = novaFilter.Where(i => i.Product != null && i.Product.Name == product.Name).Select(i => i.Status).FirstOrDefault().ToString();

                    statusDesc = string.IsNullOrEmpty(statusDesc) ? productStatus : statusDesc + "|" + productStatus;

                    startDate = string.IsNullOrEmpty(startDate) ?   effectivePeriod.StartDate : startDate + "|" + effectivePeriod.StartDate.ToString();

                    endDate = string.IsNullOrEmpty(endDate) ? effectivePeriod.EndDate.ToString() : endDate + "|" + effectivePeriod.EndDate.ToString();
                }

                var instanceUrl = novaFilter.Select(i => i.InstanceUrl?.MultiTenantUrls != null
                    ? string.Join("|", i.InstanceUrl.MultiTenantUrls.Select(s => s.Trim()))
                    : i.InstanceUrl?.Url ?? "").FirstOrDefault()?.ToString();

                var licenseData = novaFilter.Select(i => i.LicenseData != null ? i.LicenseData : "").FirstOrDefault().ToString();

                var xsoar = new XSOARListItem()
                {
                    AssetEnvironment = AssetEnvironment,
                    SerialNumber = serialNumber,
                    LicenseDescription = licenseDesc,
                    DownloadDescription = !string.IsNullOrEmpty(licenseData) ? DownloadDescription : DownloadEmptyDescription,
                    Status = statusDesc,
                    StartDate = startDate,
                    ExpirationDate = endDate,
                    LicenseDownloadLink = !string.IsNullOrEmpty(licenseData) ? string.Format(LicenseDownloadLink, serialNumber, status) : string.Empty,
                    TenantURL = instanceUrl
                };
                xsoarItems.Add(xsoar);
            }
            return xsoarItems;
        }

        /// <summary>
        /// get license data from sfdc api for license download
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public string GetXSOARLicenseData(string serialNumber, string status)
        {
            var novaItems = _sfdcLogic.GetXSOAREntitlements(new SfdcEntitlementSearchCriteria
            {
                SearchType = SfdcEntitlementSearchType.SerialNumber,
                SerialNumber = serialNumber
            }, status).Where(i => i.Product != null && i.Product.ProductFamily == SfdcProductFamily.Xsoar);
            if (novaItems == null || novaItems.Count() <= 0) return string.Empty;
            return novaItems.Where(i => i.SerialNumber == serialNumber).Select(i => i.LicenseData != null ? i.LicenseData : string.Empty).FirstOrDefault().ToString();
        }

        /// <summary>
        /// Get marketplace details from sfdc api
        /// </summary>
        /// <param name="supportAccountId">Support Account ID</param>
        /// <returns></returns>
        public IEnumerable<XsoarMarketplacePurchaseDataModel> GetMarketplaceDetails(long supportAccountId)
        {
            return _novaLogic.GetMarketplaceDetails(supportAccountId);
        }
    }
}