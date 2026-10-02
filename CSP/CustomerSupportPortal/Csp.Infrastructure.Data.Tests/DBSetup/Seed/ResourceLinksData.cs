using System.Collections.Generic;
using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	static class ResourceLinksData {

		public static readonly IEnumerable<resourcelink> SeedData = new resourcelink[]
      {
        new resourcelink{Id = 1, Name = "Help", Url = "https://live.paloaltonetworks.com/docs/DOC-6289"}, 
        new resourcelink{Id = 2, Name = "Applipedia", Url = "https://applipedia.paloaltonetworks.com/"}, 
        new resourcelink{Id = 3, Name = "Case Management", Url = "https://identity.paloaltonetworks.com/pf/adapter2adapter.ping?IdpAdapterId=supportOTidp&SpSessionAuthnAdapterId=sfdcMidway&TargetResource=https%3A%2F%2Fidentity.paloaltonetworks.com%2Fidp%2FstartSSO.ping%3FPartnerSpId%3Dhttps%3A%2F%2Fsaml.salesforce.com%26TargetResource%3Dhttps%3A%2F%2Flogin.salesforce.com%2F500%3Ffcf%3D00B7000000724oA"}, 
        new resourcelink{Id = 4, Name = "Dynamic Updates", Url = "/Updates/DynamicUpdates"}, 
        new resourcelink{Id = 5, Name = "Knowledge Base", Url = "https://live.paloaltonetworks.com/community/kb"}, 
        new resourcelink{Id = 6, Name = "PAN-DB URL Categorization", Url = "http://urlfiltering.paloaltonetworks.com/testASite.aspx"}, 
        new resourcelink{Id = 7, Name = "Security Advisories", Url = "https://securityadvisories.paloaltonetworks.com/"}, 
        new resourcelink{Id = 8, Name = "Software Updates", Url = "/Updates/SoftwareUpdates"}, 
        new resourcelink{Id = 9, Name = "Support Community", Url = "https://live.paloaltonetworks.com/"}, 
        new resourcelink{Id = 10, Name = "Technical Documentation", Url = "https://live.paloaltonetworks.com/community/documentation"}, 
        new resourcelink{Id = 11, Name = "Threat DB", Url = "https://threatvault.paloaltonetworks.com/"}, 
        new resourcelink{Id = 12, Name = "Wildfire Portal", Url = "https://wildfire.paloaltonetworks.com"}, 
      };

		public static void Seed() {
			using (var db = new cspEntities()) {

				var sb = new StringBuilder();

				foreach (var rl in SeedData) {
					sb.Append(string.Format("INSERT INTO resourcelinks(Id, Name, Url) VALUES({0}, '{1}', '{2}');", rl.Id, rl.Name, rl.Url));
				}

				db.Database.ExecuteSqlCommand(sb.ToString());
			}
		}
	}
}