namespace Csp.Api.Services.Contracts
{
    public interface ISelfServiceRegistrationCodeService
    {
        RegistrationCodeStatus GetCurrentRegistrationCodeStatus(long supportAccountId);
        void DisableRegistrationCode(long supportAccountId);
        RegistrationCodeStatus GenerateRegistrationCode(long supportAccountId, long createdByUserAccountId);
    }

    public class RegistrationCodeStatus
    {
        public bool RegistrationCodeIsActive { get; set; }
        public string SelfServiceRegistrationCodeUrl { get; set; }
    }
}
