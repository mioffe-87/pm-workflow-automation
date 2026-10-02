﻿using System.Collections.Generic;
using Csp.Api.Model;
﻿using Csp.Api.Security;


namespace Csp.Api.Services.Contracts {
	public interface IEpRepository {

		AvailableEp[] GetAvailableEps(DataParameters parameters, long supportAccountId, List<RoleSecurity> rolesSecurity);

		SearchAllDetailGroup[] ReadMultipleDeviceLicenses(long[] trapsDeviceIds);
	    string GetLicenseApiKey(long supportAccountId);

	    IEnumerable<AvailableEp> GetAvailableEps(string connectionString, DataParameters parameters, long userAccountId,
	        long supportAccountId);
	}

	public class SearchAllDetailGroup {
		public long DeviceId { get; set; }
		public List<SearchAllDetail> Licenses { get; set; }
	}
}
