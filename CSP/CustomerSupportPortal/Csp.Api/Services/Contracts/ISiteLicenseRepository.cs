using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
        public interface ISiteLicenseRepository
        {
            IEnumerable<SiteLicenseItem> GetSiteLicenses(long supportAccountId, DataParameters dataParameters);
            AutoFocusLicenseDetails GetAutoFocusLicenseDetails(long supportAccountId, long licenseId);
            IEnumerable<EnterpriseAgreementItem> GetEnterpriseAgreements(long supportAccountId);
            Task<IEnumerable<EnterpriseAgreementItem>> GetVmELaItems(long supportAccountId);
            string GetEnterpriseAgreementMainAuthCode(long licenseId);
            bool HasSiteLicense(long supportAccountId);
            bool HasEsaLicense(long supportAccountId);
            bool Has4HREsaLicense(long supportAccountId);
            bool HasEaLicense(long supportAccountId);
            bool HasElaUrl4License(long supportAccountId);
            long GetELAConversionRequestId(long supportAccountId);
        
            // VM ELA related methods
            Task<VmElaTokenModel> GetVmElaTokens(string authCode);
            Task<IEnumerable<VmElaTokenModel>> GetVmElaAuthCodesInfo(long supportAccountId);
            Task<IEnumerable<VmElaChildModelInfo>> GetCurrentAccountVmElaModels(string grandParentAuthCode, long supportAccountId);
            Task SaveVmElaAllocatedQuantities(string grandParentAuthCode, long totalAllocatedTokens, Dictionary<string, IEnumerable<VmElaChildModelInfo>> models);
            Task ApplyVmElaAdditionalTokens(string authCode, long tokens);
            Task AddVmElaMoreTokens(string authCode);
            Task UpdateVmElaAdditionalTokensAppliedDate(string authCode);
            Task<string> GrantVmElaTokens(string grandParentAuthCode, long curUserId, string destinationEmail);
            bool HasPendingTokensGrantRequest(long supportAccountId, long userId);
            Task<IEnumerable<ElaTokensGrantModel>> GetPendingElaTokensGrantItems(long supportAccountId, long userId);
            Task<bool> VmElaModelsExist(string authCode, long supportAccountId);
            Task AcceptElaTokenGrant(string authCode, long userAccountId, bool accepted);
            List<ElaTokenNotificationModel> GetVmElaTokenNotificationList();
            long UpdateVmElaTokenNotificationLog(ElaTokenNotificationModel tokenNotificationModel);
            IEnumerable<ElaTokensGrantModel> GetPendingElaTokensByUser(long supportAccountId, long userAccountId, long entityId);
            long GetVmElaTokenId(string grandParentAuthCode, long curUserId, string destinationUser);
        }

        public class AutoFocusLicenseDetails
        {
            public int UseCount { get; set; }
            public DateTime ExpirationDate { get; set; }
        }
}
