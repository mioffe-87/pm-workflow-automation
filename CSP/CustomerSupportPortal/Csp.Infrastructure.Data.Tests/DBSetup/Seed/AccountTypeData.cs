using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class AccountTypeData {

		public static readonly accounttype[] AccountTypesAll = new accounttype[]
      {
        new accounttype { AccountTypeId = 1, Name = "Customer", AdministrativeRoleId = 2, SecurityClassification = 1 },
        new accounttype { AccountTypeId = 2, Name = "ASC_Support", AdministrativeRoleId = 2, SecurityClassification = 3 },
        new accounttype { AccountTypeId = 3, Name = "PAN Employees", AdministrativeRoleId = 2, SecurityClassification = 4 },
        new accounttype { AccountTypeId = 4, Name = "EVAL", AdministrativeRoleId = 2, SecurityClassification = 4 },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var at in AccountTypesAll) {
					sb.Append(string.Format("INSERT INTO accounttype(AccountTypeId, Name, AdministrativeRoleId, SecurityClassification) VALUES({0}, '{1}', {2}, {3});", at.AccountTypeId, at.Name, at.AdministrativeRoleId, at.SecurityClassification));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}