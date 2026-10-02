using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class SupportAccountDomainsRepository : ISupportAccountDomainsRepository {

		public bool SupportAccountHasDomains(long supportAccountId) {

			using (var db = new cspEntities()) {

				var hasDomains = db.supportaccountdomains.Any(x => x.SupportAccountId == supportAccountId);

				return hasDomains;
			}
		}

		public long GetOrCreateDomain(string domain) {

			using (var db = new cspEntities()) {

				var d = db.domains.Where(x => x.DomainName == domain).OrderBy(x => x.DomainId).FirstOrDefault();

				if (d == null) {

					d = new domain {
						DomainName = domain,
					};

					db.domains.Add(d);
					db.SaveChanges();
				}

				return d.DomainId;
			}
		}

		public void CreateSupportAccountDomain(long supportAccountId, long domainId) {

			using (var db = new cspEntities()) {

				var exists = db.supportaccountdomains.Any(x => x.SupportAccountId == supportAccountId && x.DomainId == domainId);

				if (exists) {
					return;
				}

				db.supportaccountdomains.Add(new supportaccountdomain {
					SupportAccountId = supportAccountId,
					DomainId = domainId,
				});
				db.SaveChanges();
			}
		}
	}
}
