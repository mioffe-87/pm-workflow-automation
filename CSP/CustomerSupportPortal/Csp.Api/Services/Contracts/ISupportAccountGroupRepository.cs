using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountGroupRepository {
		List<GroupListItem> ReadSupportAccountGroups(long supportAccountId, long restrictedByUserAccountId, RoleSecurity[] roleSecurities, DataParameters parameters);
		long GetSupportAccountIdByGroupId(long groupId);
	    void AddDeviceCategoryDevicesToGroup(long groupId, long? deviceCategoryId, long createdByUserAccountId, string deviceType);
	}
}
