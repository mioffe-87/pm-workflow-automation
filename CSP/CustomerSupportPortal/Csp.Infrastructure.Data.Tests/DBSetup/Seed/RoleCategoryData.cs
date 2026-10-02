using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class RoleCategoryData {

		public static readonly Csp.Infrastructure.Data.DBs.CSP.rolecategory[] RoleCategoriesAll = new rolecategory[]
			{
				new rolecategory{RoleCategoryId = 1, CategoryName = "SupportAccount"},
				new rolecategory{RoleCategoryId = 2, CategoryName = "Group"},
			};

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var r in RoleCategoriesAll) {
					sb.Append(string.Format("INSERT INTO rolecategories(RoleCategoryId, CategoryName) VALUES({0}, '{1}');", r.RoleCategoryId, r.CategoryName));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}