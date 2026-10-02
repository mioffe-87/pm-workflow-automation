using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class CountryData {

		public static readonly country[] CountriesBasic = new country[]
      {
        new country { CountryId = 1, Name = "Afghanistan", Code = "AF" },
        new country { CountryId = 233, Name = "United States", Code = "US" },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var c in CountriesBasic) {
					sb.Append(string.Format("INSERT INTO country(CountryId, Name, Code) VALUES({0}, '{1}', '{2}');", c.CountryId, c.Name, c.Code));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}