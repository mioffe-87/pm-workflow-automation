using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Model.Sfdc;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class ExtendedExpertiseLogic : BaseLogic, IExtendedExpertiseLogic
    {
        private readonly IXSOARDataAccess _xsoarDataAccess;
        readonly string CREDIT_MENU_URL = ConfigurationManager.AppSettings["ExtendedExpertiseCreditMenuUrl"];
        readonly string GOOGLE_FORM_URL = ConfigurationManager.AppSettings["ExtendedExpertiseGoogleFormUrl"];
        readonly string SfdcApiExtendedExpertiseAssetProductId = "6606d3aa-b764-4432-984e-f03502f3cb44";
        readonly string SfdcApiCreditExtendedExpertiseAssetProductId = "85ee16d8-9474-4bf7-b532-d6b7237fc170";

        public ExtendedExpertiseLogic(IXSOARDataAccess xsoarDataAccess)
        {
            _xsoarDataAccess = xsoarDataAccess;
        }

        public ExtendedExpertiseCoreData GetCoreData(long supportAccountId, long userAccountId)
        {
            if (!(supportAccountId > 0))
            {
                return new ExtendedExpertiseCoreData();
            }

            var salesForceAccountId = _xsoarDataAccess.GetSalesForceAccountId(supportAccountId);

            string graphQLRequest = "{ \"query\": \"query($salesAccountId: [String], $creditEEProductId: [String]) { "
                + "assets(salesAccountId: $salesAccountId, productId: $creditEEProductId){ salesAccountId, salesAccountName, unitOfMeasure, quantityAllocated, quantityNetBalance, quantityNetConsumption } }\", "
                + "\"variables\": { \"salesAccountId\":\"" + salesForceAccountId + "\", \"creditEEProductId\":\"" + SfdcApiCreditExtendedExpertiseAssetProductId + "\" } }";
            ExtendedExpertiseResponse responsePayload = NovaApis.GetXsoarMarketplaceData<ExtendedExpertiseResponse>(graphQLRequest);
            if (responsePayload != null && responsePayload.Errors != null)
            {
                throw new InvalidDataException(responsePayload.Errors.First().Message);
            }

            return responsePayload.MapToCoreData(new SecurityDataAccess().IsSuperUser(supportAccountId, userAccountId), GOOGLE_FORM_URL, CREDIT_MENU_URL);
        }

        /// <summary>
        /// Get Extened Expertise Engagements.
        /// </summary>
        /// <param name="supportAccountId">Support Account ID</param>
        /// <param name="userAccountId">User Account ID</param>
        /// <returns>List of engagemens.</returns>
        public IEnumerable<ExtendedExpertiseItem> GetEngagements(long supportAccountId, long userAccountId, string status = null)
        {
            if (!(supportAccountId > 0))
            {
                return new ExtendedExpertiseItem[0];
            }

            var salesForceAccountId = _xsoarDataAccess.GetSalesForceAccountId(supportAccountId);

            string graphQLRequest = "{ \"query\": \"query($salesAccountId: [String], $eeProductId: [String]) { "
                + "usageTransactions(salesAccountId: $salesAccountId, productId: $eeProductId) { productId, unitOfMeasure, companyCode, consumptionDate, quantityConsumed, usageDescription, xrefPackId, xrefTransactionId, xrefVendorId, xrefCData } }\", "
                + "\"variables\": { \"salesAccountId\":\"" + salesForceAccountId + "\", \"eeProductId\":\"" + SfdcApiExtendedExpertiseAssetProductId + "\" } }";
            ExtendedExpertiseResponse responsePayload = NovaApis.GetXsoarMarketplaceData<ExtendedExpertiseResponse>(graphQLRequest);
            if (responsePayload != null && responsePayload.Errors != null)
            {
                throw new InvalidDataException(responsePayload.Errors.First().Message);
            }

            IEnumerable<ExtendedExpertiseItem> list = responsePayload.MapToEngagements();
            return string.IsNullOrWhiteSpace(status)
                ? list.ToArray()
                : list.Where(i => i.Status == status).ToArray();
        }
    }
}
