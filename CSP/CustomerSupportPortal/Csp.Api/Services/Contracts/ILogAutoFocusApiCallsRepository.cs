namespace Csp.Api.Services.Contracts
{
    public interface ILogAutoFocusApiCallsRepository
    {
        void LogApiCall(string apiMethodCalled, string inputs, string outputs, long userAccountId);
    }
}