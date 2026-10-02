using System;
using System.Text.RegularExpressions;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class SupportAccountDomainsService : ISupportAccountDomainsService {

		private readonly ISupportAccountDomainsRepository _accountDomainsRepository;

		public SupportAccountDomainsService(ISupportAccountDomainsRepository accountDomainsRepository) {
			_accountDomainsRepository = accountDomainsRepository;
		}

		public void CreateFirstSupportAccountDomain(long supportAccountId, string email) {

			//Parse email to get domain
			var domain = GetDomainFromEmail(email);

			if (_accountDomainsRepository.SupportAccountHasDomains(supportAccountId)) {
				return;
			}

			var domainId = _accountDomainsRepository.GetOrCreateDomain(domain);

			_accountDomainsRepository.CreateSupportAccountDomain(supportAccountId, domainId);
		}

		private string GetDomainFromEmail(string email) {

			var match = Regex.Match(email, @"@(.+)$");

			if (!match.Success) {
				throw new Exception("Invalid email, can't parse domain: " + email);
			}

			if (match.Groups.Count != 2) {
				throw new Exception("Invalid email, can't parse domain2: " + email);
			}

			return match.Groups[1].Value;
		}
	}
}
