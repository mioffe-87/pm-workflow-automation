using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence {
	public class EvalTrackerRepository : IEvalTrackerRepository {
		public EvalTrackerData GetSfAccountDataByEvalId(string evalId) {

			var sfAccountIds = new List<string>();
			var deviceCodes = new List<string>();

			using (var db = new cspEntities()) {
				var evals =
					(
						from vme in db.vmevals
						where vme.EvalId == evalId
						select new {
							vme.SalesforceAccountId,
							vme.DeviceCode,
						}).ToArray();

				sfAccountIds.AddRange(evals.Select(x => x.SalesforceAccountId));
				deviceCodes.AddRange(evals.Select(x => x.DeviceCode));
			}

			sfAccountIds = sfAccountIds.Distinct().ToList();
			deviceCodes = deviceCodes.Distinct().ToList();

			switch (sfAccountIds.Count()) {
				case 0:
					return null;

				case 1:
					return new EvalTrackerData {
						SfAccountId = sfAccountIds.Single(),
						DeviceCodes = deviceCodes.ToArray(),
					};

				default: //more than one
					return null;
			}
		}
	}
}
