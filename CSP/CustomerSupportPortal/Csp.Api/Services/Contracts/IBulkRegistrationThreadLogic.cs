using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts {

	public interface IBulkRegistrationThreadLogic {

		void StartBulkRegistration(long[] deviceIdsForRegistration, long[] licenseIdsForRegistration, Api.Model.Device deviceInfo, long currentUserId);
	}
}
