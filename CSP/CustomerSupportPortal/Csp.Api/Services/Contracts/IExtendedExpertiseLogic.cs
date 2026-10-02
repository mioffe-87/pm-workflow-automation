using Csp.Api.Model;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Model.Xsoar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IExtendedExpertiseLogic
    {
        ExtendedExpertiseCoreData GetCoreData(long supportAccountId, long userAccountId);

        IEnumerable<ExtendedExpertiseItem> GetEngagements(long supportAccountId, long userAccountId, string status = null);
    }
}