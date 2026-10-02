using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ICsspLogic
    {
        CsspInfo GetCsspInfo(long supportAccountId);

        IEnumerable<CsspSku> SearchCsspSkus(DataParameters parameters, long? supportAccountId, int? model,
            int? term, int? supportType, int? bundle, string filterProducts = "");

        IList<CsspModel> GetCsspModels(bool addCaptionItem = true);

        IList<CsspSupportType> GetCsspSupportTypes(bool addCaptionItem = true);

        IList<CsspTerm> GetCsspTerms(bool addCaptionItem = true);

        CsspSku GetSkuDetails(int skuId);

        IEnumerable<CsspSku> GetSkuDetails(List<int> SkuIds);

        bool SubmitCsspOrder(List<int> skuIds, long supportAccountId, long userAccountId, out int? cartId, out string cartNumber);

        CsspOrder GetCsspOrderDetails(int cartId);

        bool PushOrderToSap(CsspOrderSapInput sapInput, out string sapReturnMessage, out string inputXML);

        void CreateSapOrderAsync(long supportAccountId, int shoppingCartId, string cartNumber, string orderedBy);

        CsspOrderSapInput GetSapInput(long supportAccountId, int cartId);
		
		IEnumerable<CsspOrderDetails> GetOrders(long supportAccountId);

        IEnumerable<CsspOrderHistory> GetOrderHistory(long supportAccountId, string orderNumber="");

        IEnumerable<AvailableVm> GetAvailableVms(DataParameters parameters, long supportAccountId, long userAccountId, string authCode);

        void SendSapOrderNotification(CsspOrderSapInput sapInput, bool isOrderCreated, string sapMessage, string inputXML);

        IEnumerable<CsspOrderHistoryItems> GetOrderHistoryItems(int csspOrderId);

        RegisterVmResult RegisterVm(string uuId, string cpuId, string authCode, long supportAccountId, long userAccountId);
        ParseVmFileResult ParseVmFile(HttpPostedFileBase file);

        IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(DataParameters parameters, long supportAccountId,
            string serialNumber = "", string authorizationCode = "", string endUserAccountId ="", string endUserCompany = "", string contactEmail ="");

        CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> GetCsspProvisionedDevicesMonthly(DataParameters parameters, long supportAccountId, int year, int month);

        byte[] GetCsspProvisionedDevicesCsv(long supportAccountId,
            string serialNumber = "", string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "", string contactEmail = "");

        CsspProvisionedDevicesMonthlyData<byte[]> GetCsspProvisionedDevicesMonthlyCsv(long supportAccountId, int year, int month);

        bool AddEndUserInformationForCsspCustomer(CsspEndUser endUser, out string errorMessage);

        bool ReportCsspEndUserInfo(CsspEndUser endUser);

        IEnumerable<CsspEndUser> SearchEndUser(long csspSupportAccountId, string searchField, string searchValue);

        bool IsUserCssp(long userAccountId, long supportAccountId);

				// Comments By LK On 02/07/2020
				// This method is obsolete and is no longer being used
        // bool UpdateCsspEndUserDefaultAccountsToExistingPanAccounts(string referenceId, long csspSupportAccountId, string currentSfdcAccountId, long? currentSupportAccountId, string newSfdcAccountId, long? newSupportAccountId, out string errorMessage);

        bool StoreMultipleSfdcAccountMatchesForEndUser(string referenceId, long csspSupportAccountId,
                    string[] sfdcAccountIds, string preferredSfdcAccountId);

        bool StoreSfdcAccountForEndUser(string referenceId, long csspSupportAccountId, string sfdcAccountId);

        bool LogCsspPanAccountCreationDetails(long enduserId, string panAccountType, string logType, string notes,
          string createdBy);

        bool CanGeneratePrelicenseKeys(long supportAccountId);

        int GetVmUseCountByAuthCode(string authCode);

        PrelicenseResults ProcessPrelicenseKeys(long supportAccountId, string authorizationCode, int quantity, string createdBy);

        IEnumerable<CsspPreLicenseDevice> GetCsspPrelicenseDevices(DataParameters parameters, long supportAccountId, string authorizationCode, string csspBatchId = "", string serialNumber = "");

        CsspDownloadLicense DownloadLicense(List<long> aDeviceId);

        int GetPreLicenseRequestStatusId(int requestId);

        List<long> GetDeviceIdsToDownloadLicenseKeys(long supportAccountId, string searchField, string searchValue);

    }
}
