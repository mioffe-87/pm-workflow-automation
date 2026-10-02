using System;
using System.Collections.Generic;
using System.Dynamic;


namespace Csp.Web.HelperClasses {

	/// <summary>
	/// This class holds a dictionary of private IDs. It extends from DynamicObject
	/// to allow a developer to specify any property name that they choose. This class allows
	/// access to this property as if it was defined statically in the Class definition.
	/// This class is only designed to be used in the BaseViewPage in the UniqueID property. 
	/// </summary>
	public class DynamicUniqueHtmlAttrIds : DynamicObject {

		/// <summary>
		/// This dictionary holds the unique IDs that are passed to this class. The class is 
		/// used in a Dymanic property so a developer can use any property name they choose.
		/// The property name is used as the Key and the UniqueId for the property is used 
		/// in the value. 
		/// </summary>
		private Dictionary<string, string> uniqueIds = new Dictionary<string, string>();

		/// <summary>
		/// This checks to see if the property has been defined. If not it adds the unique 
		/// id to a key value pair in the dictionary. The name in the binder parameter is 
		/// used as the key and the value is the property name plus an underscore and a guid.
		/// The N is used to remove the dashes from the guid. 
		/// </summary>
		public override bool TryGetMember(GetMemberBinder binder, out object result) {

			var name = binder.Name.ToLower();
			if (!uniqueIds.ContainsKey(name)) {

				uniqueIds.Add(name, name + "_" + Guid.NewGuid().ToString("N"));
			}

			result = uniqueIds[name];

			return true;
		}

		/// <summary>
		/// Sets the value in the property. This is written to where a user can set the property value 
		/// though this is not typically how it is used. At the time of this comment I think this to always return false.
		/// this way it is always internally generated. I am not changing this now as it is possible some one is setting 
		/// the value for the ID externally and I do not wish to take a chance of breaking code at this point in the development life cycle. 
		/// </summary>
		public override bool TrySetMember(SetMemberBinder binder, object value) {
			/* This does not use a try catch statement, the reason is that 
			 * it should throw an exception if the value is null, null is not a valid id. */
			uniqueIds[binder.Name.ToLower()] = value.ToString();

			return true;
		}


	}

}
