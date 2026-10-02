using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IUserProfileDataAccess
    {
        UserProfileModel GetUserData(string email);
        List<countries> GetCountries(bool isPartialShipment);
        List<regions> GetRegions();
        List<regions> GetRegions(int countryId);
        bool UpdateUserData(UserProfileModel userData);
        bool IsDisplayNameExists(string displayName, string email);
		List<ResourceLink> GetNavigationLinks(string email,bool isPublic,long supportaccountid);
        DateTime GetUserLastLoginDateTime(string email);
        List<QuickActionLinkItem> GetQuickActionLinks(string email, string supportaccountid);
        bool IsAscUserBySfdcLoginId(string portalUsername, long userAccountId = 0);
        string GetSalesforceLoginId(long supportAccountId, long userAccountId);
        //bool IsGoogleAuthEnrolled(string userEmail);
        bool EnableDayOneConfig(long supportAccountId);
        UserConfig IsFirstLogin(string email);
        bool IsFedrampUser(string userEmail);
        bool IsUserAcknowledge(string userEmail);
        bool UpdateUserAcknowledge(string userEmail, long? supportAccountId);
    }
}
