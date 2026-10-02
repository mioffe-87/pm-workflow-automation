using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface ICurrentStateService {
		void SetSupportAccount(SupportAccount supportAccount);
		void SetGroup(Group group);
		void SetDevice(Device device);
		long? GetSupportAccountId();
		CspPrincipal GetCurrentUser();
        bool GetIsImpersonated();

    }
}
