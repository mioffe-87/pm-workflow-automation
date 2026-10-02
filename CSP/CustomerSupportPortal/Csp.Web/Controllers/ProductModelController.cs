using System;
using System.Web.Mvc;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.UI;
using Csp.Api.Services.Contracts;

namespace Csp.Web.Controllers {
	public class ProductModelController : BaseController {

		private const string DefaultMinimumRequiredVersion = "0.0.0";


		[HttpGet, CspAuthorize(Permission.ViewAdmin)]
		public ActionResult ProductModels() {

			var dataParameters = new DataParameters { PaginationCount = 10 };
			var productModelLogic = new ProductModelLogic();

			var productModelListDto = new ProductModelListDto() {
				DataParameters = dataParameters,
				ProductModels = productModelLogic.GetProductModels(dataParameters)
			};

			return PartialView("../Partial/_ProductModels", productModelListDto);
		}

		/// <summary>
		/// This method is used for sorting and searching implementation.
		/// This method is called by Grid using Ajax method to load results in ProductModels partial view.
		/// </summary>
		/// <param name="request">Grid DataSourceRequest</param>
		/// <returns></returns>
		[HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewAdmin)]
		public ActionResult ProductModels([DataSourceRequest] DataSourceRequest request) {
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
			var productModelLogic = new ProductModelLogic();
			var productModelListItems = productModelLogic.GetProductModels(dataParameters);

			var result = new DataSourceResult() {
				Data = productModelListItems,
				Total = Convert.ToInt32(dataParameters.TotalRecordCount)
			};

			return Json(result);
		}

		[HttpGet, AjaxOnly, CspAuthorize(Permission.ViewAdmin)]
		public ActionResult AddUpdateProductModel(long? id) {
			var productModelLogic = new ProductModelLogic();

			var productModel = (id.HasValue) ? productModelLogic.GetProductModel(id.Value) : new ProductModel { MinimumRequiredVersion = DefaultMinimumRequiredVersion };
			var model = new ProductModelDto() {
				ProductModel = productModel,
				ProductModelName = (productModel != null) ? productModel.Name : string.Empty
			};

			return PartialView("../Partial/_AddUpdateProductModel", model);
		}

		[HttpPost, AjaxOnly, CspAuthorize(Permission.ViewAdmin)]
		public ActionResult AddUpdateProductModel(ProductModelDto model) {
			string message;
			var productModelLogic = new ProductModelLogic();
			var productModel = model.ProductModel;
			var isAddNew = (productModel != null && productModel.ModelId == 0);

			try {
				if (ModelState.IsValid && productModel != null) {
					if ((model.ProductModelName != model.ProductModel.Name) && productModelLogic.IsProductModelExists(productModel.Name.Trim())) {
						model.Notifications.Add(new ResponseNotification("Product Model Name already exists. Please enter a unique name.", ResponseState.Error));
					} else {
						if (isAddNew) {
							productModel.CreatedOn = DateTime.Now;
							productModel.CreatedByUserAccountId = User.UserAccount.Id;

							productModel = productModelLogic.CreateProductModel(productModel);
						} else {
							productModel.LastUpdate = DateTime.Now;
							productModel.LastUpdateByUserAccountId = User.UserAccount.Id;

							productModel = productModelLogic.UpdateProductModel(productModel);
						}

						message = string.Format("Product Model has been {0} successfully.",
																		(isAddNew) ? "created" : "updated");
						model.Notifications.Add(new ResponseNotification(message, ResponseState.Success));
					}
				}
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				message = string.Format("Failed to {0} Product Model.", (isAddNew) ? "create" : "update");
				model.Notifications.Add(new ResponseNotification(message, ResponseState.Error));
			}

			model.ProductModel = productModel;

			return PartialView("../Partial/_AddUpdateProductModel", model);
		}

		[HttpPost, AjaxOnly, CspAuthorize(Permission.ViewAdmin)]
		public ActionResult DeleteProductModel(long id) {
			var productModelLogic = new ProductModelLogic();
			string message;

			try {
				var productModel = productModelLogic.GetProductModel(id);
				productModel.LastUpdate = DateTime.Now;
				productModel.LastUpdateByUserAccountId = User.UserAccount.Id;

				productModelLogic.DeleteProductModel(productModel);

				message = "Product Model has been deleted successfully.";
				return new JsonResult {
					Data = new {
						Success = true,
						Message = message
					}
				};
			} catch (Exception ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				message = "Failed to delete Product Model.";
			}

			return new JsonResult {
				Data = new {
					Success = false,
					Message = message
				}
			};
		}

	}
}
