using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses {

	/// <summary>
	/// Extension class used to add a p_ to a field name to change it to the parameter naming convetions of PANW.
	/// </summary>
	public static class StringParameterExtension {

		/// <summary>
		/// This convers the string field name to a parameter name that matches the PANW parameter naming convetions.
		/// </summary>
		/// <param name="name">This is the field name of the string that is used in the extension</param>
		/// <returns>Returns the field name with a "p_" prefix (e.g. fieldName becomes p_fieldName). </returns>
		public static string ToParameterName(this string name) {

			return "p_" + name;

		}

	}
}
