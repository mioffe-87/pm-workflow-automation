using Csp.Api.Model;
using Csp.Api.Security;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ISupportAccountUserLogic
    {
        UserProfileModel GetUserData(string email);
        CommonApiResponse<dynamic> GetOktaSSOData(string email);
        CommonApiResponse<dynamic> GetCspMembershipData(string email, long supportAccountId);
        CommonApiResponse<dynamic> HandleStatus(string email, string status);

        EmailSubscriptions UserSubscriptions(long userAccountId, string crudType, EmailSubscriptions emailSubscriptions = null);

        CommonApiResponse<dynamic> SupportAccountUsers(long supportAccountId, long userAccountId, string action, SupportAccountUser supportAccountUser, bool extension = false);

        CommonApiResponse<dynamic> SupportAccountUserPrivileges(long supportAccountId, long userAccountId);

        CommonApiResponse<dynamic> GetAssignableRoles(long supportAccountId, long userAccountId);

        CommonApiResponse<dynamic> GetSiteLicense(long supportAccountId);

       
        CommonApiResponse<dynamic> CreateUser(UserDetail userDetail);

        CommonApiResponse<dynamic> UpdateUser(UserDetail userDetail);

        CommonApiResponse<dynamic> GetCountries();

        CommonApiResponse<dynamic> GetRegions(int countryId);

        #region Account Details
        CommonApiResponse<dynamic> GetSupportAccountDetails(long supportAccountId, bool isCspHeaderRequired);
        CommonApiResponse<dynamic> UpdateSupportAccountInfo(long supportAccountId, long userAccountId, SupportAccountInfo supportAccountInfo);
        CommonApiResponse<dynamic> GetDefaultRoles(long supportAccountId, long userAccountId, bool isCspHeaderRequired);
        CommonApiResponse<dynamic> ConvertEvalAccount(EvalAccountDetails info);
        CommonApiResponse<dynamic> GetAscInformation(long supportAccountId);
        CommonApiResponse<dynamic> UpdateAscInformation(long supportAccountId, UpdateAscInformation ascInfo);
        CommonApiResponse<dynamic> UpdateDefaultRoles(long supportAccountId, long userAccountId, RolesInfo roles);
        CommonApiResponse<dynamic> GetSelfRegistrationDetails(long supportAccountId, long userAccountId);
        CommonApiResponse<dynamic> UpdateEmailDomainType(long supportAccountId, UpdateEmailDomain emailDomainInfo);
        CommonApiResponse<dynamic> CreateSelfRegistrationDetails(SelfRegistration info);
        CommonApiResponse<dynamic> UpdateSelfRegistrationDetails(UpdateSelfRegistration info);
        #endregion

        #region SSO
        SSOThirdPartyInformation GetSSOInformation(long userAccountId, long supportAccountId, string email);

        #endregion

        #region Self registration
        CommonApiResponse<dynamic> ValidateCaptcha(dynamic request);
        CommonApiResponse<dynamic> ValidatePasswordResetCaptcha(dynamic request);
        CommonApiResponse<dynamic> ValidateEmail(dynamic request);    
        CommonApiResponse<dynamic> ValidateSelfRegistrationCode(dynamic request);
        CommonApiResponse<dynamic> ValidateProductSku(dynamic request);
        CommonApiResponse<dynamic> ValidateClaisec(dynamic request);
        CommonApiResponse<dynamic> ValidateNetsec(dynamic request);
        CommonApiResponse<dynamic> ValidatePublicDomain(long supportAccountId, string email);
        CommonApiResponse<dynamic> GetEligibleAccountsByUserEmail(string email);
        #endregion

        CommonApiResponse<dynamic> EnableUser(EnableUser requestDto);

        dynamic GetMembershipByRole(long supportAccountId, long roleId, long number, long size, string sort);

        dynamic NotifySuperUser(NotifySuperUserRequest request);

        Task<bool> ValidateSfdcContact(long supportAccountId, long userAccountId, string email);

        CommonApiResponse<dynamic> UnBlockUser(UnblockUser requestDto);

        void InvalidateJwtTokens(long userAccountId);
    }
}
