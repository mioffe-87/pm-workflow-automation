using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Xsoar;

namespace Csp.Api.Logic
{
    public class NovaLogic : INovaLogic
    {
        /// <summary>
        /// Get xsoar marketplace details from nova api
        /// </summary>
        /// <param name="supportAccountId">Support Account ID</param>
        /// <returns></returns>
        public IEnumerable<XsoarMarketplacePurchaseDataModel> GetMarketplaceDetails(long supportAccountId)
        {
            List<XsoarMarketplacePurchaseDataModel> xsoarPurchaseData = new List<XsoarMarketplacePurchaseDataModel>();
            if (!(supportAccountId > 0))
            {
                return Enumerable.Empty<XsoarMarketplacePurchaseDataModel>();
            }

            var graphQLRequest = "{\"query\":\"query ($supportAccountId: [Long], $assetProductId: [String])"
                                 + "{purchaseTransactions(supportAccountId:$supportAccountId,productId: $assetProductId){productName,purchaseDate,quantityPurchased}}\","
                                 + "\"variables\":{\"supportAccountId\":" + supportAccountId
                                 + ",\"assetProductId\":\"" + ConfigurationManager.AppSettings.Get("SfdcApiXsoarAssetProductId")
                                 + "\"}}";
            var responsePayload = NovaApis.GetXsoarMarketplaceData<XsoarMarketplacePurchase>(graphQLRequest);
            if (responsePayload != null && responsePayload.Errors != null)
            {
                throw new InvalidDataException(responsePayload.Errors.First().Message);
            }

            xsoarPurchaseData = responsePayload.MapToXsoarMarketplacePurchaseDataModel();
            return xsoarPurchaseData;
        }
    }
}
