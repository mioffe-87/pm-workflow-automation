using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Csp.Api.Model.Attributes {

	[AttributeUsage(AttributeTargets.Property)]
	class DataAccessModelAttribute : BaseDataAccessAttribute {


		/// <summary>
		/// <para>The DataAccesModel Attribute is used to identify if a property holds and object that is a decendant of BaseCSPModel </para>
		/// <para>and has properties that should be read from or persistend to the database.</para>
		/// <para>This uses named parameters rather than optional so only the ones that need to be changed can be done without including the other parameters.</para>
		/// <para>NAMED PARAMETERS: </para>
		/// <para>StorageName: This is used to set the name of the field the property is mapped to in the database. </para>
		/// <para>DataType: The default is DbType.String and must be set if the property has a separate datatype. </para>
		/// <para>UseForUpdate: The default is true, which means the property will be used if AddInModelParameters is used for an Update.</para>
		/// <para>UseForCreate: The default is true, which means the property will be used if AddInModelParameters is used for a Create. </para>
		/// <para>UseKeyOnly: The default is false, which means all properties in the object are read to be mapped, set to true to only use the key field.</para>
		/// <para>PropertyName: This is used if UseKeyOnly is set to true, it specifies the name of the property that has the key field. </para> 
		/// </summary>

		public DataAccessModelAttribute() {

			UseKeyOnly = false;
			PropertyName = "";

			UseForCreate = true;
			UseForUpdate = true;
			StorageName = "";
			DataType = DbType.String;
		}

		/// <summary>
		/// Default is false. This is Optional. 
		/// If true object that is in the property has only the field identified by property name read and written to.
		/// If false the object that is in the property has all properties with attributes read and written to.
		/// </summary>
		public bool UseKeyOnly {
			get;
			set;
		}

		/// <summary>
		/// Default is empty string. Optional but required if UseKeyOnly is true. This determines the name of the field that a object inside the property has the value set to.
		/// </summary>
		public string PropertyName {
			get;
			set;
		}

	}
}
