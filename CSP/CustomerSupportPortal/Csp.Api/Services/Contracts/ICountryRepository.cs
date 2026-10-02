namespace Csp.Api.Services.Contracts {
	public interface ICountryRepository {
		CountryModel[] GetCountriesByIds(long[] countryIds);
	}

	public class CountryModel {
		public long CountryId { get; set; }
		public string CountryName { get; set; }
	}
}
