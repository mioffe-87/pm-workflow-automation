using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IUserAccountApiLogic
    {        
        string GenerateSelfRegistrationOtp(SelfRegistraionOtpRequest generateOtpRequest);
        dynamic GeneratePasswordResetOtp(SelfRegistraionOtpRequest generateOtpRequest);
        dynamic IsIdpEnabled(string domain);

        dynamic ContactSupport(dynamic model);

        dynamic ValidateSelfRegistrationOtp(SelfRegistraionOtpValidationRequest validateOtpRequest);

        dynamic RegisterClaisec(dynamic model);

        dynamic RegisterNetsec(dynamic model);

        dynamic RegisterByCode(dynamic model);

        CommonApiResponse<dynamic> GetAllCloudServices();
        dynamic RemoveBounceListEmails(EmailList model);
    }
}