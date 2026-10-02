using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses {

	/// <summary>
	/// This enum is used in the AddInModelParameters method of the 
	/// Csp.Api.DataAccess.HelperClasses.DatabaseExtension Class. 
	/// This enum is used to filter properties with a DataAcces attribute 
	/// to ensure the correct properties are used for a Create or Update statement. 
	/// </summary>
	public enum CreateOrUpdate {
		Create,
		Update,
	}
}
