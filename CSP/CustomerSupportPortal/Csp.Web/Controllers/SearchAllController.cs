using System;
using System.Linq;
using System.Security;
using System.Web.Mvc;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Kendo.Mvc.UI;
using Csp.Web.Security.Attributes;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Collections.Generic;
using Kendo.Mvc.Extensions;

namespace Csp.Web.Controllers {
	[Authorize]
	public class SearchAllController : BaseController {

		private readonly ISearchAllLogic _searchAllLogic;

		public SearchAllController(ISearchAllLogic searchAllLogic) {
			_searchAllLogic = searchAllLogic;
		}

		[HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
		public ActionResult View(long? id) {
            var supportAccountId = id.HasValue ? id.Value : 0;
            var isSearchAllAccounts = id.HasValue == false;
			var hasAnyPanAccess = User.RoleSecurities.HasAnyPanAccess();
			if (isSearchAllAccounts && hasAnyPanAccess) {
				throw new SecurityException("Access to Search All Accounts is unavailable to PAN users");
			}

			var searchallModel = _searchAllLogic.GetDefaultViewModel(User.UserAccount.Id, id);
            searchallModel.IsSuperuser = new SecurityDataAccess().IsSuperUser(supportAccountId, User.UserAccount.Id);
            var model = new SearchAllDto()
            {
                SearchAll = searchallModel,
                ActionHelper = new ActionHelper(ControllerFields.SearchAllController, Convert.ToInt64(id))                
            };

            if (isSearchAllAccounts)
            {
                model.SearchAll.DefaultSupportAccountId = null;
                return View(model);
            }
            
			else
       return PartialView("../Partial/_SearchAll", model.SearchAll);
            //return View(model);
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult View(long? id, [DataSourceRequest] DataSourceRequest request) {

            var dataParameters = request.ToDataParameters();
            var searchResults = _searchAllLogic.SearchAllV2(User.UserAccount.Id, id, dataParameters);

            var result = new DataSourceResult {
                Data = searchResults,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        public FileResult SearchAllReport(long? id, string searchField, string searchValue) {

			var csv = _searchAllLogic.SearchAllCsv(id, User.UserAccount.Id, searchField, searchValue);

			var dateStr = DateTime.Now.ToString("yyyyMMdd");

			return File(csv, "text/comma-separated-values", "SearchAllReport_" + dateStr + ".csv");
		}

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult ActivationLinksLookup()
        {
            return PartialView("../Partial/_ActivationLinksLookup", new ActivationLinkLookupDto
            {
                DataParameters = new DataParameters { PaginationOffset = 0, PaginationCount = 10, SearchValue = string.Empty },
                Items = new ActivationLinksLookupItem[0]
            });
        }

        [HttpPost, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult ActivationLinksLookup([DataSourceRequest]DataSourceRequest request, string searchValue)
        {
            try
            {
                long supportAccountId = User.SupportAccountIdForPermissionSet ?? 0;
                if (!new SecurityDataAccess().IsSuperUser(supportAccountId, User.UserAccount.Id))
                {
                    return Json(new DataSourceResult
                    {
                        Errors = "You are not authorized to access Active Links Lookup.",
                        Data = new ActivationLinksLookupItem[0],
                        Total = 0
                    }, JsonRequestBehavior.AllowGet);
                }

                DataParameters parameters = request.ToDataParameters();
                IEnumerable<ActivationLinksLookupItem> data = _searchAllLogic.GetActivationLinksLookup(parameters, supportAccountId, searchValue);
                DataSourceResult result = data.ToDataSourceResult(request);
                result.Total = (int)parameters.TotalRecordCount;
                result.Data = data;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new DataSourceResult
                {
                    Errors = "Error has occured during activation links lookup. Please try again later or contact System Administrator.",
                    Data = new ActivationLinksLookupItem[0],
                    Total = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
