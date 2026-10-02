﻿using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts {
	public interface ISupportAccountDevicesRepository {
		List<DeviceListItemModelCompatibility> ReadSupportAccountDevices(long supportAccountId, SupportAccountDeviceType supportAccountDeviceType, DataParameters parameters, List<RoleSecurity> roleSecurities, long userAccountId);
	    bool IsSupportAccountCssp(long supportAccountId);
        bool HasCsspProvisionedVm(long supportAccountId, string csspDeviceTransferLog);
	    IEnumerable<long> GetCsspProvisionedDevices(long csspSupportAccountId);
        List<SearchAllModel> ReadSupportUserDevices(long userAccountId, long? supportAccountId, string deviceType, DataParameters parameters, bool ignorePagination = false);
        List<SearchAllModel> ReadSupportUserPae(long userAccountId, IEnumerable<long> supportAccountIds, DataParameters parameters);

        IEnumerable<SearchAllModel> ReadSupportUserDevices_V2(long userAccountId, long? supportAccountId, string deviceType, DataParameters parameters);
    }

	public enum SupportAccountDeviceType {
		Prod,
		Spare,
		Both,
	}
}