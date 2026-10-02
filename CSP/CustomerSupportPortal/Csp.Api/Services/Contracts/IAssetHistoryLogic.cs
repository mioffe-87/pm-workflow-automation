using Csp.Api.Model;
using Csp.Api.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IAssetHistoryLogic
    {
        List<AssetHistoryListItem> GetAssetHistory(string code, DateTime? startDate, DateTime? endDate, long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, List<RoleSecurity> roleSecurities, bool isExportReport);
    }
}
