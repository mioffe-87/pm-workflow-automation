using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.Model.Enums {
	/// <summary>
	/// This enum is used by the DataParameters to designate the sort direction of the result set. An extension class exists to allow
	/// these values to be converted to an SQL representation. This is achieved by calling SortDirection.Value.ToSqlOrderBy().
	/// </summary>
	public enum SortDirection {

		Ascending = 0,
		Descending = 1

	}

}
