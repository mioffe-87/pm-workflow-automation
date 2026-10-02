namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountDomainsRepository {
		bool SupportAccountHasDomains(long supportAccountId);
		long GetOrCreateDomain(string domain);
		void CreateSupportAccountDomain(long supportAccountId, long domainId);
	}
}
