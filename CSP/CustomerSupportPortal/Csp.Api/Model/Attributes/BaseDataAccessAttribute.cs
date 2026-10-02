using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Csp.Api.Model.Attributes {

	/// <summary>
	/// The base class holding common properties for descendant DataAccess attributes. The DataAccess attributes are used 
	/// to identify properties that should be persisted in the database. 
	/// </summary>
	public abstract class BaseDataAccessAttribute : Attribute {


		/// <summary>
		/// Identifies the name of the field and parameter in the database that the property maps to
		/// </summary>
		public string StorageName {
			get;
			set;
		}

		/// <summary>
		/// Default Is DBType.String. Optional and used to identify the DBType for the parameter for properties used in create or update stored procedures
		/// </summary>
		public DbType DataType {
			get;
			set;
		}

		/// <summary>
		/// Default is True. Optional and used to determine if the property should be used in calls to insert stored procedures
		/// </summary>
		public bool UseForCreate {
			get;
			set;
		}

		/// <summary>
		/// Default is True. Optional and used to determine if the property should be used in calls to update stored procedures
		/// </summary>
		public bool UseForUpdate {
			get;
			set;
		}

	}
}
