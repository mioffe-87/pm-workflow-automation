
using Csp.Api.LoginService;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ILoginServiceLogic {
		UserProfile GetUserProfile(string emailAddress, string ipAddress, string key);      
        bool IsProfileExists(string email, string ipAddress);
        UserProfile Register(string emailAddress, string password, string firstName, string lastName,
                                                                string companyName, string addressLine1, string addressLine2, string city,
                                                                string region, string postalCode, string countryCode, string phoneNumber,
                                                                string ipAddress, bool isStandalone = false);

        bool ManageGoogleAuth(string crudType, out string responseMessage, string email = "", GoogleAuthRequest googleAuthRequest = null);
        UserProfile ActivateUser(string emailAddress);

        bool IsUserRegistered(string email);

        UserProfile GetUserProfileV2(string emailAddress);
    }
}
