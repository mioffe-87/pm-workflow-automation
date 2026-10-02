using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Csp.Api.Model.Attributes {

	/// <summary>
	/// The DataAcces Attribute is used to identify if a property value should be read from or persistend to the database.
	/// While not restricted, this attribute is designed to be used on objects that descend from BaseCSPModel. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DataAccessAttribute : BaseDataAccessAttribute {

		/// <summary>
		/// <para>The DataAccess Attribute is used to identify if a property value of a BaseCspModel object that is read from or persisted to the database. </para>
		/// <para>This uses named parameters rather than optional so only the ones that need to be changed can be done without including the other parameters. </para>
		/// <para>NAMED PARAMETERS:</para>
		/// <para>Datatype: The default is DbType.String and must be set if the property has a separate datatype. </para>
		/// <para>UseForUpdate: The default is true, which means the property will be used if AddInModelParameters is used for an Update.</para>
		/// <para>UseForCreate:  The default is true, which means the property will be used if AddInModelParameters is used for a Create. </para>  
		/// </summary>
		/// <param name="storageName">Identifies the name of the field and parameter in the database that the property maps to. </param>
		public DataAccessAttribute(string storageName) {

			StorageName = storageName;
			DataType = DbType.String;
			UseForUpdate = true;
			UseForCreate = true;

		}
	}
}
