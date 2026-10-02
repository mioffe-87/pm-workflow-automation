using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;

namespace Csp.Api.DataAccess.HelperClasses {

	/// <summary>
	/// Strink helper class used to convert a sortDirection used in the DataParameters object to a string used in SQL order by
	/// </summary>
	public static class SortDirectionExtension {

		/// <summary>
		/// Helper class that coverts the sort direction to a SQL order by string that can be used by the database
		/// </summary>
		/// <param name="sortDirection">The sort direction we are extending</param>
		/// <returns>A SQL usable sort direction of ASC or DESC</returns>
		public static string ToSqlOrderBy(this SortDirection sortDirection) {

			switch (sortDirection) {

				case SortDirection.Ascending:
					return "ASC";

				case SortDirection.Descending:
					return "DESC";

				default:
					return "ASC";
			}
		}
	}
}
