using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class UserAccountData {

		public static readonly useraccount[] UserAccountsBasic = new useraccount[]
      {
				new useraccount { UserAccountId = 1, Email = "donotreplycsp@paloaltonetworks.com", DisplayName = "SystemUser" },
        new useraccount { UserAccountId = 40893, Email = "mlavoie@paloaltonetworks.com", DisplayName = "mlavoie" },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var ua in UserAccountsBasic) {
					sb.Append(string.Format("INSERT INTO useraccount(UserAccountId, Email, DisplayName) VALUES({0}, '{1}', '{2}');", ua.UserAccountId, ua.Email, ua.DisplayName));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}