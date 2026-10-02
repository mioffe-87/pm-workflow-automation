using System;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ILicensingApiLogic
    {
        ManageApiListDto GetAllApiDetails(long supportAccountId, long userAccountId);
        LicensingApi GetLicensingApiKey(long supportAccountId);
        ModifyApiDto CreateLicensingApiKey(PostApiDetails postData);
        ModifyApiDto ExtendLicensingApiKey(PostDeleteApiDetails postData);
        ModifyApiDto DisableLicensingApiKey(PostDeleteApiDetails postData);
    }
}
