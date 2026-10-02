using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ICsspRepository
    {
        CsspInfo GetCsspInfo(long supportAccountId);

        IEnumerable<CsspSku> SearchCsspSkus(DataParameters parameters, long? supportAccountId, int? model,
            int? term, int? supportType, int? bundle, string filterProducts = "");

        IEnumerable<CsspModel> GetCsspModels();

        IEnumerable<CsspSupportType> GetCsspSupportTypes();

        IEnumerable<CsspTerm> GetCsspTerms();

        CsspSku GetSkuDetails(int skuId);

        IEnumerable<CsspSku> GetSkuDetails(List<int> SkuIds);

        bool SubmitCsspOrder(List<int> skuIds, long supportAccountId, long userAccountId, out int? cartId, out string cartNumber);

        CsspOrder GetCsspOrderDetails(int cartId);

        CsspOrderSapInput GetSapInput(long supportAccountId, int cartId);

		IEnumerable<CsspOrderDetails> GetOrders(long supportAccountId);

        IEnumerable<CsspOrderHistory> GetOrderHistory(long supportAccountId,string orderNumber="");

        bool SetSapOrderNumberForOrder(int cartId, string sapOrderNum);

        IEnumerable<CsspOrderHistoryItems> GetOrderHistoryItems(int csspOrderId);

        IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(long supportAccountId,
            string serialNumber = "", string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "", string contactEmail = "", DataParameters parameters = null);

        CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> GetCsspProvisionedDevicesMonthly(long supportAccountId, int year, int month);

        IEnumerable<CsspEndUser> SearchEndUser(long csspSupportAccountId, string searchField, string searchValue);

        bool IsUserCssp(long userAccountId, long supportAccountId);

        bool UpdateSupportAccountForProvisionedDevice(long endUserDeviceId, long userAccountId, long supportAccountId);

        bool ValidateEndUserSerialNumber(string serialNumber, out string eMessage);

        bool ValidateEndUserCustomerAccountId(string customerAccountId, out string eMessage);

        bool ValidateCsspEndUserInputCombination(string serialNumber, string customerAccountId, out string eMessage);

        bool IsSupportAccountForEndUserAlreadyExist(string customerAccountId, long csspSupportAccountId);

        void GetEndUserSupportAccount(string customerAccountId, long csspSupportAccountId, out long? enduserSupportAccountId, out string sfdcAccount);

        bool UpdateSupportAccountIdForCsspEndUser(string customerAccountId, long csspSupportAccountId,
            long endUserSupportAccountId, string sfdcAccountId);

        bool LogOrderXML(int shoppingCartId, string orderXml, string sapResponse);

        bool UpdatePanAccountForEndUser(long deviceId, long supportAccountId, out long? endUserId, out bool isCspAccountUpdated, out bool isSfdcAccountUpdated);

        bool UpdatePanAccountForEndUser(long csspSupportAccountId, string customerAccountId, long? customerSupportAccountId, string defaultSfdcAccountId, string sfdcAccountId,
            long? supportAccountId, out string errorMessage);

        bool StoreMultipleSfdcAccountMatchesForEndUser(string referenceId, long csspSupportAccountId,
            string[] sfdcAccountIds, string preferredSfdcAccountId);

        bool StoreSfdcAccountForEndUser(string referenceId, long csspSupportAccountId, string sfdcAccountId);

        bool LogCsspPanAccountCreationDetails(long enduserId, string panAccountType, string logType, string notes,
           string createdBy);

        bool CanGeneratePrelicenseKeys(long supportAccountId);

        int CreatePrelicenseRequest(long supportAccountId, string authorizationCode, int quantity, string createdBy, out string batchId);

        IEnumerable<CsspPreLicenseDevice> GetCsspPrelicenseDevices(DataParameters parameters, long supportAccountId, string authorizationCode, string csspBatchId = "", string serialNumber = "");

        CsspDownloadLicense DownloadLicense(List<long> aDeviceId);

        int GetPreLicenseRequestStatusId(int requestId);

        List<long> GetDeviceIdsToDownloadLicenseKeysByAuthorizationCode(long supportAccountId, string authorizationCode);

        List<long> GetDeviceIdsToDownloadLicenseKeysByPrelicenseBatchId(long supportAccountId, string batchId);

    }
}
