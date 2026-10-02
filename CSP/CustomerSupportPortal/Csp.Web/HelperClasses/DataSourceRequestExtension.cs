using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.UI;
using Csp.Api.Model;

namespace Csp.Web.HelperClasses {

	public static class DataSourceRequestExtension {

		/// <summary>
		/// This extension class converts a Kendo datarequest to a Dataparameter that is passed to the data access layer. 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static DataParameters ToDataParameters(this DataSourceRequest request, string searchField = null, string searchValue = null) {

			DataParameters dataParameters = new DataParameters();
			int pageSize = 10;

			if (request.PageSize == 0) {
				request.PageSize = pageSize;
			} else {
				pageSize = request.PageSize;
			}

			dataParameters.PaginationCount = pageSize;
			dataParameters.PaginationOffset = (request.Page - 1) * pageSize;

			if (request.Sorts != null) {
				if (request.Sorts.Count > 0) {
					request.Page = 1;
					dataParameters.SortField = request.Sorts[0].Member.ToString();
					if (request.Sorts[0].SortDirection == System.ComponentModel.ListSortDirection.Ascending) {
						dataParameters.SortDirection = Api.Model.Enums.SortDirection.Ascending;

					} else {
						dataParameters.SortDirection = Api.Model.Enums.SortDirection.Descending;
					}

				}
			}


			if (request.Filters != null) {
				if (request.Filters.Count > 0) {
					request.Page = 1;

					dataParameters.SearchField = ((Kendo.Mvc.FilterDescriptor)((request.Filters[0]))).Member;
					dataParameters.SearchValue = ((Kendo.Mvc.FilterDescriptor)((request.Filters[0]))).Value.ToString();
				}
			}

			if (!string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) {
				dataParameters.SearchField = searchField;
				dataParameters.SearchValue = searchValue;
			}

			return dataParameters;
		}

	}
}