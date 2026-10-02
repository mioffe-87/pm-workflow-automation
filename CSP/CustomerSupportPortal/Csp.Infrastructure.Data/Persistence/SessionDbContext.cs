using System.Web;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	internal static class SessionDbContext {

		/// <summary>
		/// Returns a shared ObjectContext instance.
		/// </summary>
		public static cspEntities CspObjectContext {
			get {
				var ocKey = "ocm_" + HttpContext.Current.GetHashCode().ToString("x");
				if (!HttpContext.Current.Items.Contains(ocKey)) {
					HttpContext.Current.Items.Add(ocKey, new cspEntities());
				}

				return HttpContext.Current.Items[ocKey] as cspEntities;
			}
		}
	}
}
