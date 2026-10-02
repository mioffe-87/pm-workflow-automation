using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using AutoMapper;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;

namespace Csp.Web.Controllers {

    [Authorize]
    public class UpdatesController : BaseController {

		private readonly ISoftwareUpdateLogic _softwareUpdateLogic;

        public ActionResult Index(long? id) {
            return View();
        }

        public ActionResult UrgentUpdates(long? id)
        {
            return View();
        }

        public UpdatesController(ISoftwareUpdateLogic softwareUpdateLogic) {
			_softwareUpdateLogic = softwareUpdateLogic;
		}

        /// <summary>
        /// This method is initially used to load SoftwareUpdates partial view when first time called
        /// </summary>
        [HttpGet, CspAuthorize(Permission.ViewSoftwareUpdates)]
        public async Task<ActionResult> SoftwareUpdates(long id = 0, string vmDevices = "", string nsx = "", string csspVms = "", string saasActivations = "", string epType = "")
        {
            bool isVmDevices = (vmDevices != null && vmDevices.ToUpper() == "TRUE");
            bool isSaasActivations = (saasActivations != null && saasActivations.ToUpper() == "TRUE");
            bool isNsx = (nsx != null && nsx.ToUpper() == "TRUE");
            bool isCssp = (csspVms != null && csspVms.ToUpper() == "TRUE");
            bool isEndPointType = !string.IsNullOrEmpty(epType);
            
            var userAccountId = (int)User.UserAccount.Id;
            var filterOptions = new Dictionary<string, string>();
            var panoramPluginupdates = new List<SoftwareUpdateListItem>();

            var supportAccountId = id > 0 ? id : CurrentState.GetSupportAccountId().Value;

            string eligibleGroups;
            _softwareUpdateLogic.GetFiltersForSoftwareUpdates(userAccountId, supportAccountId, out eligibleGroups);

            var productModels = _softwareUpdateLogic.GetAllProductModels();
            List<SoftwareUpdateFilters> filters = (from productModel in productModels 
                    select new SoftwareUpdateFilters
                    {
                        DisplayName = productModel.DisplayName,
                        Product = productModel.ProductLine,
                        Platform = productModel.Platform,
                        DisplayOrder = productModel.DisplayOrder.ToString()
                    }).ToList();
            
           List<SoftwareUpdateFilters> result = new List<SoftwareUpdateFilters>();

            if (isVmDevices)
            {
                result = filters.Where(x => x.DisplayName.ToLower().Contains("vm-series")).ToList();
                //if (isCssp)
                //{
                //   // result = result.Where(x => String.CompareOrdinal(x.VersionNumber, "7") > 0).ToList();
                //}
            }
            if (isSaasActivations)
            {
                result = filters.Where(x => x.Platform.ToLower().Contains("cloud services")).ToList();
            }

            if (isNsx)
            {
                result = filters.Where(x => x.Product == "panorama" && x.Platform != "m").ToList();
            }



            if (isEndPointType && !string.IsNullOrEmpty(epType) && _softwareUpdateLogic.ShowTrapsSoftwareUpdates(supportAccountId))
            {
                var isMalbork = EpLogic.IsMalbork(epType);
                var isTrapsEla = epType.Contains(EndpointLicenseType.TrapsELA);
                if (isMalbork || isTrapsEla)
                {
                    var trapsaList = filters.Where(x => x.Product != null && x.Product.ToLower() == "trapsa").ToList();
                    var trapsMList = filters.Where(x => x.Product != null && x.Product.ToLower() == "trapssm").ToList();
                   
                    result = trapsaList;
                    result.AddRange(trapsMList);
                }
                else
                {
                    var epTypes = epType.ToLower().Replace("ren", "").Replace("bkln", "").Replace("trapsmbeta", "trapsm");
                    result = result.Where(x => x.Product != null && x.Product.ToLower() == epTypes).ToList();
                }
            }
           
            if (result.Any())
            {
                string platforms = string.Join(",", result.Select(x => x.Platform).Distinct());
                string products = string.Join(",", result.Select(x => x.Product).Distinct());

                SoftwareUpdateRequest requestData = new SoftwareUpdateRequest()
                {
                    SupportAccountId = supportAccountId,
                    UserAccountId = userAccountId,
                    Product = products,
                    Platform = platforms,
                    ContentType = ContentTypeEnum.SOFTWARE.ToDescription(),
                    GroupIds = eligibleGroups
                };
                List<SoftwareUpdateListItem> softwareUpdates = await _softwareUpdateLogic.GetSoftwareUpdates(requestData);

                SoftwareUpdateRequest pluginRequest = new SoftwareUpdateRequest()
                {
                    SupportAccountId = supportAccountId,
                    UserAccountId = userAccountId,
                    Product = products,
                    Platform = platforms,
                    ContentType = ContentTypeEnum.PLUGIN.ToDescription(),
                    GroupIds = eligibleGroups
                };

                List<SoftwareUpdateListItem> pluginUpdates = await _softwareUpdateLogic.GetSoftwareUpdates(pluginRequest);
                
                if (softwareUpdates != null)
                {
                    softwareUpdates.AddRange(pluginUpdates);
                }
                List<SoftwareUpdateListItemDto> softwareUpdatesDto = Mapper.Map<List<SoftwareUpdateListItem>, List<SoftwareUpdateListItemDto>>(softwareUpdates);
                List<SoftwareUpdateListItemDto> panoramaPluginUpdatesDto = Mapper.Map<List<SoftwareUpdateListItem>, List<SoftwareUpdateListItemDto>>(panoramPluginupdates);

                foreach (var updatesDto in softwareUpdatesDto)
                {
                    updatesDto.DisplayName = result.FirstOrDefault(x => x.Platform.ToLower() == updatesDto.Platform.ToLower() && x.Product.ToLower() == updatesDto.Product.ToLower()) == null ? string.Empty : filters.FirstOrDefault(x => x.Platform.ToLower() == updatesDto.Platform.ToLower() && x.Product.ToLower() == updatesDto.Product.ToLower()).DisplayName;
                    updatesDto.Key = updatesDto.DisplayName;
                    SoftwareUpdateFilters first = null;
                    foreach (var x in filters)
                    {
                        if (x.DisplayName != null && x.DisplayName == updatesDto.DisplayName)
                        {
                            first = x;
                            break;
                        }
                    }
                    updatesDto.Id = first!=null ? Convert.ToInt32(first.DisplayOrder) : 0;
                }
                var softwareUpdateListDto = new SoftwareUpdateListDto
                {
                    ActionHelper = new ActionHelper(ControllerFields.UpdatesController), SoftwareUpdates = softwareUpdatesDto, PanoramaPluginUpdates = panoramaPluginUpdatesDto, FilterOptions = filterOptions
                };

                ViewBag.IsVmDevices = isVmDevices;

                if (isVmDevices || isNsx || isSaasActivations || isEndPointType)
                {
                    return PartialView("../Partial/_SoftwareUpdatesGrid", softwareUpdateListDto);
                }
                else
                {
                    return View(softwareUpdateListDto);
                }
            }

            return View(new SoftwareUpdateListDto());
        }

		/// <summary>
		/// This method is initially used to load SoftwareUpdates partial view when first time called
		/// </summary>
		[HttpGet, CspAuthorize(Permission.ViewDynamicUpdates)]
		public ActionResult DynamicUpdates(long id = 0) {
           return View(new DynamicUpdateListDto());
        }
        

	    [HttpGet]
        public ActionResult ShowChecksum(string md5Key, string sha, string fileSize,string fileName , string version)
	    {
            SoftwareUpdatesChecksumModel model = new SoftwareUpdatesChecksumModel()
            {
                FileName = fileName,
                FileSize = fileSize,
                HasMd5Key = !string.IsNullOrEmpty(md5Key),
                HasSha256Key = !string.IsNullOrEmpty(sha),
                Sha256Key = sha,
                Md5Key = md5Key,
                VersionNumber = version
            };
            return PartialView("../Partial/_SoftwareUpdateChecksum", model);
	    }

        ///<summary>
        /// This method is used to load distinct PanOS Verions (Software)
        /// </summary>
		[HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public async Task<ActionResult> GetPanOSVersions()
        {
            var result = await _softwareUpdateLogic.GetPanOSVersions();
            var jsonResult = Json(result, JsonRequestBehavior.AllowGet);
            return jsonResult;

        }
    }
}
