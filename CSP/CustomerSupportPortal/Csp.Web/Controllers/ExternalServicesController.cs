using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.UpdatesDownload;
using Csp.Api.Services.Contracts;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Newtonsoft.Json;

namespace Csp.Web.Controllers
{
    public class ExternalServicesController : ApiController
    {

        private readonly IUserProfileDataAccess _userProfileDataAccess;
        private readonly ICommonApiLogic _commonApiLogic;
        private readonly ISoftwareUpdateLogic _softwareUpdateLogic;

        //This controller should not inject any interfaces that are using INhibernate or Irepository
        public ExternalServicesController(IUserProfileDataAccess userProfileDataAccess, ICommonApiLogic commonApiLogic, ISoftwareUpdateLogic softwareUpdateLogic)
        {
            _userProfileDataAccess = userProfileDataAccess;
            _commonApiLogic = commonApiLogic;
            _softwareUpdateLogic = softwareUpdateLogic;
        }

        [HttpPost, HttpOptions]
        public async Task<IHttpActionResult> GetPortalContextDetails([FromBody] UserDetailModel model)
        {
            var contextDetails = new PortalContextDetails();

            try
            {
                var privateKey = ConfigurationManager.AppSettings["AESEncryptionKey"];
                var encryptionProvider = new EncryptionProvider();

                var isKeyValid = encryptionProvider.IsKeyValidAES(model.Email, model.Key, privateKey);
                if (!isKeyValid)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(String.Format("Invalid key {0}", model.Key)));
                    return this.Json(new PortalContextDetails());
                }


                var membershipDataAccess = new MembershipDataAccess();
                var coveoSearchToken = string.Empty;
                // var mem = await membershipDataAccess.GetMembershipDetailsBySfdcPortalUsername(model.Username, true);
                var membershipDetails = !string.IsNullOrEmpty(model.Username)
                    ? await membershipDataAccess.GetMembershipDetailsBySfdcPortalUsername(model.Username, true)
                    : null;

                contextDetails.Organization = ConfigurationManager.AppSettings["CoveoOrganizationId"];
                contextDetails.SearchHub = ConfigurationManager.AppSettings["CoveoSearchHub"];
                contextDetails.SearchDataSource = SiteHelper.GetCoveoSearchDataSource();
                contextDetails.SelfHelpDataSource = SiteHelper.GetCoveoSelfHelpDataSource();
                contextDetails.RestEndpointUrl = ConfigurationManager.AppSettings["CoveoSearchRestEndpointUrl"];
                contextDetails.cspEndpointUrl = ConfigurationManager.AppSettings["PortalBffUiHost"];

                if (membershipDetails != null && membershipDetails.MembershipId > 0)
                {
                    //CSP-5667 - The PendoKey should be visible only if the user is authenticated.
                    coveoSearchToken = SiteHelper.GetCoveoSearchToken(model.Email);

                    contextDetails.PendoKey = ConfigurationManager.AppSettings["PendoApiKey"];
                    contextDetails.DisplayName = HttpUtility.HtmlEncode(membershipDetails.DisplayName);
                    contextDetails.UserAccountId = membershipDetails.UserAccountId.ToString();
                    contextDetails.SupportAccountId = membershipDetails.SupportAccountId.ToString();
                    contextDetails.SupportAccountName = HttpUtility.HtmlEncode(membershipDetails.CompanyName);
                    contextDetails.HasMultipleAccounts = true;
                    contextDetails.ImpersonationSetting = membershipDetails.ImpersonateSetting;
                    contextDetails.IsAuthUser =
                        (membershipDetails.UserAccountId != 0 && membershipDetails.SupportAccountId != 0)
                            ? true
                            : false;
                 
                    var aiEligible = _commonApiLogic.GetAiEligibility(membershipDetails.UserAccountId, membershipDetails.SupportAccountId);
                    if (aiEligible != null && aiEligible.ResponseObject != null) {
                        var responseObjectAi = aiEligible.ResponseObject;
                        contextDetails.IsAiEligible = Convert.ToBoolean(responseObjectAi.IsAIEligible);
                    }

                    var aiAgentImpersonationStatus = _commonApiLogic.GetAiAgentImpersonationStatus(membershipDetails.SupportAccountId);
                    if (aiAgentImpersonationStatus != null && aiAgentImpersonationStatus.ResponseObject != null)
                    {
                        var responseObjectAiAgent = aiAgentImpersonationStatus.ResponseObject;
                        contextDetails.AiAgentImpersonation = Convert.ToBoolean(responseObjectAiAgent.aiAgentImpersonation);
                        contextDetails.AiAgentDeclineReason = responseObjectAiAgent.aiAgentDeclineReason?.ToString() ?? "";
                    }

                    var userTheme = _commonApiLogic.GetUserSelectedTheme(membershipDetails.UserAccountId);
                    if (userTheme != null && userTheme.ResponseObject != null)
                    {
                        var responseObject = userTheme.ResponseObject;
                        contextDetails.UserTheme = responseObject.settingValue.ToString();
                    }
                    else
                    {
                        contextDetails.UserTheme = "LIGHT";
                    }
                }
                else
                {
                    // Anonymous user - generate token using CoveoUnauthApiKey
                    coveoSearchToken = SiteHelper.GetCoveoSearchToken(null);
                    contextDetails.AiAgentImpersonation = false;
                    contextDetails.AiAgentDeclineReason = "";
                }
                
                contextDetails.SearchToken = coveoSearchToken;

                contextDetails.SearchPageUrl = (Convert.ToInt32(contextDetails.SupportAccountId) > 0)
                                                ? ConfigurationManager.AppSettings["CoveoSearchPageUrl"] + "/" + Convert.ToInt32(contextDetails.SupportAccountId)
                                                : ConfigurationManager.AppSettings["CoveoSearchPageUrl"];
                contextDetails.ResourceRevisionNumber = SiteHelper.GetResourceRevisionNumber();
                contextDetails.UserTypeKey = SiteHelper.GetUserTypeKey();
                contextDetails.UserType = SiteHelper.GetUserType(model.Email);
                contextDetails.EnvType = SiteHelper.GetCoveoEnvType();
                contextDetails.Roles = "ASC User";
                contextDetails.Email = model.Email;
                contextDetails.UserName = model.Username;
                contextDetails.IsAscUser = !string.IsNullOrEmpty(model.Username) &&
                                _userProfileDataAccess.IsAscUserBySfdcLoginId(model.Username);
            }
            catch (Exception ex)
            {
                // Comments By LK on 12/22/2020
                // Return SupportAccountId as -1 to prevent the infinite loop issue with KB portal
                contextDetails.SupportAccountId = "-1";
                var errorMsg = "Error in call to GetPortalContextDetails(): " + JsonConvert.SerializeObject(model);
                var exceptionDetails = ex.StackTrace.ToString();

                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg + " Exception: " + exceptionDetails));
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(contextDetails);
        }


        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetSideNavigationMenuItems(long supportAccountId, long userAccountId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = await _commonApiLogic.GetSideNavigationMenuItems(supportAccountId, userAccountId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [JwtTokenAuthentication]
        [HttpGet]
        public async Task<IHttpActionResult> GetSideNavigationSubMenuItems(long supportAccountId, long userAccountId, int menuItemId)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = await _commonApiLogic.GetSideNavigationSubMenuItems(supportAccountId, userAccountId, menuItemId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }



        [HttpGet]
        public async Task<IHttpActionResult> GetUnAuthMenuItems()
        {
            try
            {
                long defaultValue = 0;
                var result = await _commonApiLogic.GetSideNavigationMenuItems(defaultValue, defaultValue);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }


        [HttpGet]
        public async Task<IHttpActionResult> GetUnAuthSubMenuItems(int menuItemId)
        {
            try
            {
                long defaultValue = 0;
                var result = await _commonApiLogic.GetSideNavigationSubMenuItems(defaultValue, defaultValue, menuItemId);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> GetPendoDetails(long userAccountId, [FromBody] PendoAccountDetails details)
        {

            if (!JwtTokenManager.IsAuthorized(userAccountId, details.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }
            try
            {
                var result = await _commonApiLogic.GetPendoDetails(details);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public async Task<IHttpActionResult> GetDownloadUrl(long userAccountId, long supportAccountId, [FromBody] SoftwareUpdateFileInfo fileInfo)
        {
            if (!JwtTokenManager.IsAuthorized(userAccountId, supportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            try
            {
                if (fileInfo == null || string.IsNullOrEmpty(fileInfo.FileName))
                {
                    return this.Json(new
                    {
                        Success = false,
                        Message = "Unable to download content. Please contact support."
                    });
                }

                if (fileInfo.FileName.Contains("..") || fileInfo.FileName.Contains("/") || fileInfo.FileName.Contains("\\"))
                {
                    return this.Json(new
                    {
                        Success = false,
                        Message = "Invalid file name."
                    });
                }

                SoftwareUpdateFileInfo authorizedFileInfo = null;

                if (!string.IsNullOrEmpty(fileInfo.ContentType) && fileInfo.ContentType.Equals("CELLULAR", StringComparison.OrdinalIgnoreCase))
                {
                    var cellularRequest = new ContentDownloadRequestCellularData
                    {
                        UserAccountId = userAccountId,
                        SupportAccountId = supportAccountId,
                        ProductFilterId = fileInfo.ProductFilterId,
                        CarrierId = fileInfo.CarrierId,
                        Pagination = new ContentDownloadPagination { Current = 1, PageSize = int.MaxValue }
                    };

                    var cellularResponse = await _softwareUpdateLogic.GetCellularData(cellularRequest);

                    if (cellularResponse == null || !cellularResponse.Success || cellularResponse.Downloads == null || !cellularResponse.Downloads.Any())
                    {
                        return this.Json(new
                        {
                            Success = false,
                            Message = "Unable to download content. Please contact support."
                        });
                    }

                    var matchingCellularDownload = cellularResponse.Downloads.FirstOrDefault(download =>
                        !string.IsNullOrEmpty(download.FileName) &&
                        (download.FileName.Equals(fileInfo.FileName, StringComparison.OrdinalIgnoreCase) || download.ReleaseNotes.Equals(fileInfo.FileName, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(fileInfo.Platform) || download.Platform == fileInfo.Platform) &&
                        (string.IsNullOrEmpty(fileInfo.Product) || download.Product == fileInfo.Product)
                    );

                    if (matchingCellularDownload == null)
                    {
                        return this.Json(new
                        {
                            Success = false,
                            Message = "File not found in available cellular updates."
                        });
                    }

                    authorizedFileInfo = new SoftwareUpdateFileInfo
                    {
                        FileName = fileInfo.FileName,
                        FolderName = fileInfo.FolderName,
                        Platform = matchingCellularDownload.Platform,
                        Product = matchingCellularDownload.Product,
                        ContentType = fileInfo.ContentType
                        
                    };
                }
                else
                {
                    var softwareUpdateRequest = new SoftwareUpdateRequest
                    {
                        UserAccountId = userAccountId,
                        SupportAccountId = supportAccountId,
                        Platform = fileInfo.Platform,
                        Product = fileInfo.Product,
                        ContentType = fileInfo.ContentType
                       
                    };

                    List<SoftwareUpdateListItem> contentUpdates = new List<SoftwareUpdateListItem>();

                    if (!string.IsNullOrEmpty(fileInfo.ContentType) &&
                        (fileInfo.ContentType.Equals(ContentTypeEnum.SOFTWARE.ToString(), StringComparison.OrdinalIgnoreCase) ||
                         fileInfo.ContentType.Equals(ContentTypeEnum.PLUGIN.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        SoftwareUpdateRequest eligibleUpdates = _softwareUpdateLogic.IsEligibleToViewUpdates(softwareUpdateRequest, out string eligibleGroupIds);
                        softwareUpdateRequest.GroupIds = eligibleGroupIds;

                        if (!string.IsNullOrEmpty(eligibleUpdates.Platform) && eligibleUpdates.Platform == fileInfo.Platform && !string.IsNullOrEmpty(eligibleUpdates.Product) && eligibleUpdates.Product == fileInfo.Product)
                        {                          
                            contentUpdates = await _softwareUpdateLogic.GetSoftwareUpdates(softwareUpdateRequest);
                        }
                    }
                    else if (!string.IsNullOrEmpty(fileInfo.ContentType) &&
                             fileInfo.ContentType.Equals(ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        softwareUpdateRequest.CurrentVersionId = fileInfo.CurrentVersionId;
                        contentUpdates = await _softwareUpdateLogic.GetSoftwareUpdates(softwareUpdateRequest);
                    }
                    else
                    {
                        _softwareUpdateLogic.GetFiltersForDynamicUpdates(userAccountId, supportAccountId, fileInfo.ContentType, out string dynamicGroupIds);
                        softwareUpdateRequest.GroupIds = dynamicGroupIds;
                        if (!string.IsNullOrEmpty(fileInfo.Label) && fileInfo.Label.ToUpper() == Label.Apps.ToDescription())
                        softwareUpdateRequest.ContentType = Label.Apps.ToDescription();

                        contentUpdates = await _softwareUpdateLogic.GetDynamicUpdates(softwareUpdateRequest);
                    }

                    if (contentUpdates == null || contentUpdates.Count == 0)
                    {
                        return this.Json(new
                        {
                            Success = false,
                            Message = "Unable to download content. Please contact support."
                        });
                    }

                    var matchingUpdate = contentUpdates.FirstOrDefault(update =>
                        !string.IsNullOrEmpty(update.FileName) &&
                        (update.FileName.Equals(fileInfo.FileName, StringComparison.OrdinalIgnoreCase) || update.ReleaseNotesFileName.Equals(fileInfo.FileName, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(fileInfo.Platform) || update.Platform == fileInfo.Platform) &&
                        (string.IsNullOrEmpty(fileInfo.Product) || update.Product == fileInfo.Product)
                    );

                    if (matchingUpdate == null)
                    {
                        return this.Json(new
                        {
                            Success = false,
                            Message = "File not found in available updates."
                        });
                    }

                    authorizedFileInfo = new SoftwareUpdateFileInfo
                    {
                        FileName = fileInfo.FileName,
                        FolderName = fileInfo.FolderName,
                        Platform = matchingUpdate.Platform,
                        Product = matchingUpdate.Product,
                        ContentType = fileInfo.ContentType
                    };
                }

                var downloadUrl = _softwareUpdateLogic.GetDownloadUrl(authorizedFileInfo);
                return this.Json(new
                {
                    Success = true,
                    DownloadUrl = downloadUrl
                });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return this.Json(string.Empty);
        }

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetUnAuthNextGenMenuItems()
        {
            try
            {
                var result = await _commonApiLogic.GetUnAuthNextGenMenuItems(0, 0);
                return this.Json(result);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(string.Empty);
            }
        }
    }
}