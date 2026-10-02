using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface IPermissionService {
		void UpdateCurrentStateAndPermissions(CspPrincipal user, string controller, string action, string id, SupportAccount supportAccount,
																					Group group, Device device);

        void SetCurrentPrincipal(string controller, string action, long lId);
    }
}