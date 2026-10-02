using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ILookupRepository {
		string GetCountryCodeById(long countryId);
        string GetCountryNameById(long countryId);
        BannerMessage GetBannerMessage();
	}
}
