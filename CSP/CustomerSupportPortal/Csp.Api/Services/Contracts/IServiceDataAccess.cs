using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IServiceDataAccess
    {
        List<AlertModel> GetAlertsData(bool isPublic);
        List<ConfigModel> GetConfigLevels();
        List<ConfigModel> GetConfigSubLevels(int levelid);
        void CaptureBPA(string logId, string userEmail, string serialNumber);
    }
}
