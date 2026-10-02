using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Text;

namespace Csp.Infrastructure.Data.Helpers {
	public static class InspectEntityValidtionErrors {
		/// <summary>
		/// Allows detailed error inspection for EF
		/// Code from: http://stackoverflow.com/questions/10219864/ef-code-first-how-do-i-see-entityvalidationerrors-property-from-the-nuget-pac
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static int SaveChangesWithErrors(this DbContext context) {
			try {
				return context.SaveChanges();
			} catch (DbEntityValidationException ex) {
				var sb = new StringBuilder();

				foreach (var failure in ex.EntityValidationErrors) {
					sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
					foreach (var error in failure.ValidationErrors) {
						sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
						sb.AppendLine();
					}
				}

				throw new DbEntityValidationException(
						"Entity Validation Failed - errors follow:\n" + sb, ex
				); // Add the original exception as the innerException
			}
		}
	}
}
