using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class PartTypeData {

		public static readonly parttype[] PartTypesAll = new parttype[]
      {
        new parttype { PartTypeId = 1, PanDbIdentification = "BUN", IsActive = true },
        new parttype { PartTypeId = 2, PanDbIdentification = "CRED", IsActive = true },
        new parttype { PartTypeId = 3, PanDbIdentification = "DEV", IsActive = true },
        new parttype { PartTypeId = 4, PanDbIdentification = "RENACC", IsActive = true },
        new parttype { PartTypeId = 5, PanDbIdentification = "RENBUN", IsActive = true },
        new parttype { PartTypeId = 6, PanDbIdentification = "RENSITE", IsActive = true },
        new parttype { PartTypeId = 7, PanDbIdentification = "RENSUB", IsActive = true },
        new parttype { PartTypeId = 8, PanDbIdentification = "RENSUP", IsActive = true },
        new parttype { PartTypeId = 9, PanDbIdentification = "SER", IsActive = true },
        new parttype { PartTypeId = 10, PanDbIdentification = "SITE", IsActive = true },
        new parttype { PartTypeId = 11, PanDbIdentification = "SPARE", IsActive = true },
        new parttype { PartTypeId = 12, PanDbIdentification = "SUB", IsActive = true },
        new parttype { PartTypeId = 13, PanDbIdentification = "SUP", IsActive = true },
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();
				foreach (var pt in PartTypesAll) {
					sb.Append(string.Format("INSERT INTO parttype(PartTypeId, PanDbIdentification, IsActive) VALUES({0}, '{1}', {2});", pt.PartTypeId, pt.PanDbIdentification, (pt.IsActive ?? true) ? 1 : 0));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}