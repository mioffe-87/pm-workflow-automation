using Csp.Api.Model;
using Csp.Api.Model.Csp.Api.Model;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ICommonApiLogic
    {
        Task<Model.CommonApiResponse<dynamic>> GetPendoDetails(PendoAccountDetails details);
        Task<CommonApiResponse<dynamic>> GetSideNavigationMenuItems(long supportAccountId, long userAccountId);

        Task<CommonApiResponse<dynamic>> GetSideNavigationSubMenuItems(long supportAccountId, long userAccountId, int menuItemId);
        bool HasBetaAccess(long supportAccountId, int betaFeatureId);
        CommonApiResponse<dynamic> ValidateAddress(AddressAPIRequest addressAPIRequest);
        CommonApiResponse<dynamic> IsEligibleFor4HREsa(AddressAPIResponse address);

        CommonApiResponse<dynamic> GetUserSelectedTheme(long userAccountId);
        CommonApiResponse<dynamic> GetAiEligibility(long userAccountId, long supportAccountId);
        CommonApiResponse<dynamic> GetAiAgentImpersonationStatus(long supportAccountId);
        Task<CommonApiResponse<dynamic>> GetUnAuthNextGenMenuItems(long supportAccountId, long userAccountId);
    }
}
