using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class RoleData {

		public static readonly role[] RolesAll = new role[]
      {
        new role { RoleId = 1, Name = "Standard User", Category = 1, SecurityClassification = 1 },
        new role { RoleId = 2, Name = "Super User", Category = 1, SecurityClassification = 2 },
        new role { RoleId = 3, Name = "ASC User", Category = 1, SecurityClassification = 3 },
        new role { RoleId = 4, Name = "TAC User", Category = 1, SecurityClassification = 4 },
        new role { RoleId = 5, Name = "Group Default", Category = 2, SecurityClassification = 1 },
        new role { RoleId = 6, Name = "TAC Super User", Category = 1, SecurityClassification = 2 },
        new role { RoleId = 7, Name = "ASC Full Service User", Category = 1, SecurityClassification = 3 },
        new role { RoleId = 8, Name = "Sales Engineering User", Category = 1, SecurityClassification = 1 },
        new role { RoleId = 9, Name = "Bulk Registration", Category = 1, SecurityClassification = 1 },
        new role { RoleId = 10, Name = "Group Super User", Category = 2, SecurityClassification = 1 },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var r in RolesAll) {
					sb.Append(string.Format("INSERT INTO role(RoleId, Name, Category, SecurityClassification) VALUES({0}, '{1}', {2}, {3});", r.RoleId, r.Name, r.Category, r.SecurityClassification));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}