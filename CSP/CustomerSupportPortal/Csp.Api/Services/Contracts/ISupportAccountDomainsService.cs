namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountDomainsService {
		void CreateFirstSupportAccountDomain(long supportAccountId, string email);
	}
}