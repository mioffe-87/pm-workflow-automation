using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model {

	public class DataParameters {

		#region Public Constructor and Properties

		/// <summary>
		/// This class is used to pass database parameters for searching, pagination and sorting. It is designed to be used as a 
		/// an adapter, or data transfer object, that takes the values from the application and either presents them to the interface
		/// and/or takes search, sort and pagination values and from the interfaces and passes it to the system and database. 
		/// </summary>
		public DataParameters() {
			PaginationCount = int.MaxValue;
		}

		/// <summary>
		/// True if paging should be implemented, otherwise
		/// no paging is needed
		/// </summary>
		public bool SupportsPaging {
			get { return PaginationOffset > 0 || PaginationCount != int.MaxValue; }
		}

		/// <summary>
		/// This is the name of the field used for searching in the database for records commonly displayed in a a Kendo grid.
		/// </summary>
		public string SearchField {
			get;
			set;
		}

		private string _searchValue;

		/// <summary>
		/// This is used to seach for a specific value in the search field for specific records. The data is commonly displayed in a Kendo grid. 
		/// The search in the database is typically done with a like in the database. 
		/// </summary>
		public string SearchValue {
			get { return _searchValue; }
			set { _searchValue = ApiCommon.EncodeString(value); }
		}

		/// <summary>
		/// This is used to determine the number of records per page that is returned from the database and displayed in the Kendo grid. The count can be changed 
		/// on a kendo grid and passed back to the database. This property holds that value.
		/// </summary>
		public int PaginationCount {
			get;
			set;
		}

		/// <summary>
		/// This is the field used to inform the database which field to sort on. This is determined by the Kendo grid and is used to pass the the field name 
		/// to the database. 
		/// </summary>
		public string SortField {

			get;
			set;
		}

		/// <summary>
		/// This determines the direction the sort field should be ordered by in the database for the grid. The SortDirection is a enum that has a extension class
		/// to provide the actual SQL sorting to pass to the database. To convert to the database value use SortDirection.replacethiswithsortvalue.ToSqlOrderBy()
		/// </summary>
		public SortDirection SortDirection {

			get;
			set;

		}

		/// <summary>
		/// This field is used to determine the offset of the record. This is passed from Kendo and is used to determine the start position of the result set. For example
		/// if a user changes to page 5, with 10 records per page then the query uses this offset in the limit for the start postion of the records that should display
		/// on page 5. 
		/// </summary>
		public long PaginationOffset {
			get;
			set;
		}

		/// <summary>
		/// This is the total record count of all records in a query. This is only set when reading records from the database. The value is passed back to the 
		/// Kendo grid to display the number of available records for the data that is being viewed. 
		/// </summary>
		public long TotalRecordCount {
			get;
			set;
		}



		#endregion
	}
}
