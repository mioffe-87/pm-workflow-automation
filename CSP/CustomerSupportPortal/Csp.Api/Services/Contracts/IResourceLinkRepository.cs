using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
	public interface IResourceLinkRepository
	{
		IEnumerable<ResourceLink> GetResourceLinks();
	    bool HasSLRBetaAccess(long supportAccountId);
	}
}
