using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IImpersonateRequestLogic
    {
        CommonApiResponse<dynamic> ProcessRequest(ImpersonateRequest request);
        CommonApiResponse<dynamic> ProcessAutoApprovals(AutoApproveRequest request);
        bool HasValidImpersonateRequest(ImpersonateRequest request, out string requestId);
        bool UpdateDataForImpersonate(string sourceEmail, string targetUserEmail);
    }
}
