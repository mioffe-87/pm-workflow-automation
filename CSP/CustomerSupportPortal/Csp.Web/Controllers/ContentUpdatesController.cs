using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Web.Filters;
using Csp.Web.HelperClasses;
using Csp.Api.Model.UpdatesDownload;
using Newtonsoft.Json;

namespace Csp.Web.Controllers
{
    public class ContentUpdatesController : ApiController
    {
        private const string ERROR_CELLULAR_FILTERS = "Error of getting Cellular filters.";
        private const string ERROR_CELLULAR_DATA = "Error of getting Cellular data.";

        private readonly ISoftwareUpdateLogic _softwareUpdateLogic;

        public ContentUpdatesController(ISoftwareUpdateLogic softwareUpdateLogic)
        {
            _softwareUpdateLogic = softwareUpdateLogic;
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetContentUpdatesFilters([FromBody]SoftwareUpdateRequest model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<SoftwareUpdateFilters>();
            string groupIds = string.Empty;

            try
            {
                if (model.ContentType == ContentTypeEnum.SOFTWARE.ToString() || model.ContentType == ContentTypeEnum.PLUGIN.ToString())
                {
                    returnData = _softwareUpdateLogic.GetFiltersForSoftwareUpdates(model.UserAccountId,
                        model.SupportAccountId, out groupIds);
                }
                else if (model.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    returnData = _softwareUpdateLogic.GetSoftwareUpgradeModels();
                }
                else
                {
                    returnData = _softwareUpdateLogic.GetFiltersForDynamicUpdates(model.UserAccountId,
                        model.SupportAccountId, string.Empty, out groupIds);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public async Task<IHttpActionResult> GetContentUpdates([FromBody]SoftwareUpdateRequest model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<SoftwareUpdateListItem>();
            try
            {
                string groupIds = string.Empty;

                // check if user is eligible to view the requested updates
                if (model.ContentType == ContentTypeEnum.SOFTWARE.ToString() || model.ContentType == ContentTypeEnum.PLUGIN.ToString())
                {
                    SoftwareUpdateRequest eligibleUpdates =
                        _softwareUpdateLogic.IsEligibleToViewUpdates(model, out groupIds);
                    model.GroupIds = groupIds;

                    if (!string.IsNullOrEmpty(eligibleUpdates.Platform) ||
                        !string.IsNullOrEmpty(eligibleUpdates.Product))
                        returnData = await _softwareUpdateLogic.GetSoftwareUpdates(model);
                }
                // CSP-18237 [BE] Endpoints for new CSP PAN-OS Advisory page
                else if (model.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    if (model.Product != "ts agent" && model.Product != "id agent" && string.IsNullOrWhiteSpace(model.CurrentVersionId))
                    {
                        throw new Exception("ContentUpdatesController::GetContentUpdates model.CurrentVersionId is required.");
                    }

                    returnData = await _softwareUpdateLogic.GetSoftwareUpdates(model);
                }
                else
                {
                    _softwareUpdateLogic.GetFiltersForDynamicUpdates(model.UserAccountId, model.SupportAccountId,
                        model.ContentType, out groupIds);
                    model.GroupIds = groupIds;

                    returnData = await _softwareUpdateLogic.GetDynamicUpdates(model);
                }
              
               
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public async Task<ContentDownloadResponseCellularFilters> GetCellularFilters([FromBody]ContentDownloadRequestBase model)
        {
            if (model == null)
            {
                return ContentDownloadResponseCellularFilters.SetError(ERROR_CELLULAR_FILTERS);
            }

            if (model.SupportAccountId <= 0)
            {
                return ContentDownloadResponseCellularFilters.SetError(ERROR_CELLULAR_FILTERS);
            }

            if (model.UserAccountId <= 0)
            {
                return ContentDownloadResponseCellularFilters.SetError(ERROR_CELLULAR_FILTERS);
            }

            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return ContentDownloadResponseCellularFilters.SetError("Unauthorized user.");
            }

            try
            {
                return await _softwareUpdateLogic.GetCellularFilters(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ContentDownloadResponseCellularFilters.SetError(ERROR_CELLULAR_FILTERS);
            }
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public async Task<ContentDownloadResponseCellularDownloads> GetCellularData([FromBody]ContentDownloadRequestCellularData model)
        {
            if (model == null)
            {
                return ContentDownloadResponseCellularDownloads.SetError(ERROR_CELLULAR_DATA);
            }

            if (model.SupportAccountId <= 0)
            {
                return ContentDownloadResponseCellularDownloads.SetError(ERROR_CELLULAR_DATA);
            }

            if (model.UserAccountId <= 0)
            {
                return ContentDownloadResponseCellularDownloads.SetError(ERROR_CELLULAR_DATA);
            }

            if (string.IsNullOrWhiteSpace(model.ProductFilterId))
            {
                return ContentDownloadResponseCellularDownloads.SetError(ERROR_CELLULAR_DATA);
            }

            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return ContentDownloadResponseCellularDownloads.SetError("Unauthorized user.");
            }

            try
            {
                var result = await _softwareUpdateLogic.GetCellularData(model);             

                return result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ContentDownloadResponseCellularDownloads.SetError(ERROR_CELLULAR_DATA);
            }
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetSoftwareUpgradeVersions([FromBody]SoftwareUpdateRequest model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId, model.SupportAccountId, ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<ContentDownloadResponseVersionUpgrade>();
            try
            {
                if (model.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    returnData = _softwareUpdateLogic.GetSoftwareUpgradeVersions();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }

        [JwtTokenAuthentication]
        [HttpPost, HttpOptions]
        public IHttpActionResult GetSoftwareUpgradeTooltip([FromBody]SoftwareUpdateRequest model)
        {
            if (!JwtTokenManager.IsAuthorized(model.UserAccountId.ToString(), ClaimType.UserAccountId))
            {
                return Unauthorized();
            }

            var returnData = new List<ContentDownloadResponseTooltipUpgrade>();
            try
            {
                if (model.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    returnData = _softwareUpdateLogic.GetSoftwareUpgradeTooltip();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return this.Json(returnData);
        }
    }
}
