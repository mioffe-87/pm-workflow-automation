using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ILookupLogic {
		string GetPublicDomains();
		IEnumerable<Country> GetCountries();
		long GetCountryIdByCode(string countryCode);
		string GetCountryCodeById(long countryId);

		IEnumerable<Region> GetRegions(long countryId);
		long GetRegionIdByCode(string regionCode, long countryId);
		string GetRegionCodeById(long regionId, long countryId);
        string GetCountryNameById(long countryId);
		string GetCspHelpById(long id);

        IEnumerable<Region> GetAllRegions();
      
    }
}
