using System.Collections.Generic;
using System.Linq;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.EntityClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;
using Country = Csp.Api.EntityClasses.Country;
using Region = Csp.Api.EntityClasses.Region;

namespace Csp.Api.Logic {
	public class LookupLogic : BaseLogic, ILookupLogic {
		private readonly IRepository _repository;

		public LookupLogic() {

		}

		public LookupLogic(IRepository repository) {
			_repository = repository;
		}

		/// <summary>
		/// This method returns all Public Domains
		/// </summary>
		/// <returns></returns>
		public string GetPublicDomains() {
			var publicDomains =
				_repository.LoadAll<Lookup>()
									 .Where(l => l.LookupType.ToLower() == LookupType.PublicDomains.ToDescription().ToLower())
									 .Select(l => l.LookupValue).ToList();

			return (publicDomains.Any()) ? string.Join("|", publicDomains) : string.Empty;
		}

		public IEnumerable<Model.Country> GetCountries() {
			var countries = _repository.LoadAll<Country>().Select(c => new Model.Country() {
				Id = c.CountryId,
				Code = c.Code,
				Name = c.Name
			});

			return countries.OrderBy(x => x.Name).ToList();
		}

		public long GetCountryIdByCode(string countryCode) {
			var country = _repository.Query<Country>().FirstOrDefault(c => c.Code == countryCode);

			return (country != null) ? country.CountryId : 0;
		}

		public string GetCountryCodeById(long countryId) {
			var country = _repository.Get<Country>(countryId);

			return (country != null) ? country.Code : string.Empty;
		}
        public string GetCountryNameById(long countryId)
        {
            var country = _repository.Query<Country>().FirstOrDefault(c => c.CountryId == countryId);

            return (country != null) ? country.Name : "";
        }

		public IEnumerable<Model.Region> GetRegions(long countryId) {
			var regions =
				_repository.LoadAll<Region>().Where(r => r.CountryId_Country_.CountryId == countryId).Select(c => new Model.Region() {
					Id = c.RegionId,
					CountryId = countryId,
					Code = c.Code,
					Name = c.Name
				});

			return regions.ToList();
		}

		public long GetRegionIdByCode(string regionCode, long countryId) {
			var region = _repository.Query<Region>().FirstOrDefault(c => c.CountryId_Country_.CountryId == countryId && c.Code == regionCode);

			return (region != null) ? region.RegionId : 0;
		}

		public string GetRegionCodeById(long regionId, long countryId) {
			var region = _repository.Query<Region>().FirstOrDefault(c => c.CountryId_Country_.CountryId == countryId && c.RegionId == regionId);

			return (region != null) ? region.Code : string.Empty;
		}

		public string GetCspHelpById(long id) {
			var cspHelp = _repository.Get<Csphelp>(id);

			return (cspHelp != null) ? cspHelp.HelpContent : string.Empty;
		}

	    public IEnumerable<Model.Region> GetAllRegions()
	    {
            var regions =
                _repository.LoadAll<Region>().Select(c => new Model.Region()
                {
                    Id = c.RegionId,
                    CountryId = c.CountryId_Country_.CountryId,
                    Code = c.Code,
                    Name = c.Name
                });

            return regions.ToList();
	    }
	}
}
