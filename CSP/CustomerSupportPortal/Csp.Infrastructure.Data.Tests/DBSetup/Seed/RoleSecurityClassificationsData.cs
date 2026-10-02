using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class RoleSecurityClassificationsData {

		public static readonly Csp.Infrastructure.Data.DBs.CSP.rolesecurityclassification[] RoleSecurityClassificationsAll = new rolesecurityclassification[]
			{
				new rolesecurityclassification{RoleSecurityClassificationId = 1, RoleSecurityClassficationName = "Default"},
				new rolesecurityclassification{RoleSecurityClassificationId = 2, RoleSecurityClassficationName = "Super"},
				new rolesecurityclassification{RoleSecurityClassificationId = 3, RoleSecurityClassficationName = "Support"},
				new rolesecurityclassification{RoleSecurityClassificationId = 4, RoleSecurityClassficationName = "Global"},
			};

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var r in RoleSecurityClassificationsAll) {
					sb.Append(string.Format("INSERT INTO rolesecurityclassifications(RoleSecurityClassificationId, RoleSecurityClassficationName, Description) VALUES({0}, '{1}', '');", r.RoleSecurityClassificationId, r.RoleSecurityClassficationName));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}