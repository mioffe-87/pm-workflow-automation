using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.Model;

namespace Csp.Infrastructure.Data.Persistence {

	public class LookupRepository : ILookupRepository {

		public string GetCountryCodeById(long countryId) {

			using (var db = new cspEntities()) {
				var country = db.countries.SingleOrDefault(x => x.CountryId == countryId);

				return country != null ? country.Code : string.Empty;
			}
		}

	    public string GetCountryNameById(long countryId)
	    {
	        var db = SessionDbContext.CspObjectContext;

            var country = db.countries.SingleOrDefault(x => x.CountryId == countryId);

            return country != null ? country.Name : string.Empty;
	    }

        public BannerMessage GetBannerMessage()
        {
            var db = SessionDbContext.CspObjectContext;

            var bannerMessage = db.bannermessages.FirstOrDefault();
            if (bannerMessage != null)
            {
                return new BannerMessage()
                {
                    MessageHeader = bannerMessage.MessageHeader,
                    MessageDetails = bannerMessage.MessageDetails,
                    IsActive = bannerMessage.Active
                };
            }
            else
            {
                return new BannerMessage() { IsActive = false };
            }
        }
	}
}
