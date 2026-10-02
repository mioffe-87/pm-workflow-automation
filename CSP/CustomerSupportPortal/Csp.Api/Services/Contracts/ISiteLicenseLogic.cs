using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Ela2;

namespace Csp.Api.Services.Contracts
{
    public interface ISiteLicenseLogic
    {
        IEnumerable<SiteLicenseItem> GetSiteLicenses(long supportAccountId, long userAccountId, DataParameters dataParameters);
        AutoFocusKeyResult RegenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult EnableOrGenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult GenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult DisableAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult EnableAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult DeleteAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);
        AutoFocusKeyResult GetAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId);

        Task<IEnumerable<EnterpriseAgreementItem>> GetEnterpriseAgreements(long supportAccountId, bool hasVmElaTokenManagementPermission);
        bool HasEsaLicense(long supportAccountId);
        bool Has4HREsaLicense(long supportAccountId);
        bool HasEaLicense(long supportAccountId);

        Ela2ButtonLinks GetEla2ButtonLinks(long supportAccountId, long userAccountId);
        void SendVmElaTokenUsageNotification();
        bool HasConvertURL4ToADVURLRoles(long supportAccountId, long userAccountId);
        bool HasUrl4License(long supportAccountId);
        ElaConversionStatusDataResponse GetConversionStatus(long requestId);
        long GetELAConversionRequestId(long supportAccountId);
        ElaConversionStatusDataResponse CreateConversionStatus(ElaConversionStatus elaConversionStatus);
        ElaConversionStatusLicenseDataResponse[]  LicenseUrl4ConversionAPI(ElaLicenseConversionStatus elaConversionStatus);
    }

    public class AutoFocusKeyResult
    {
        public bool IsSuccess { get; set; }
        public string ApiKey { get; set; }
        public AfApiKeyStatuses State { get; set; }
    }
}
