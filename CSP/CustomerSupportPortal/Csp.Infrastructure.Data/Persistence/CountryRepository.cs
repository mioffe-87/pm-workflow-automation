using System.Linq;
using System.Transactions;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class CountryRepository : ICountryRepository {

		public CountryModel[] GetCountriesByIds(long[] countryIds) {

			using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
			using (var db = new cspEntities()) {

				var data =
					from c in db.countries
					where countryIds.Contains(c.CountryId)
					select new CountryModel {
						CountryId = c.CountryId,
						CountryName = c.Name,
					};

				var results = data.ToArray();
				trans.Complete();

				return results;
			}
		}
	}
}
