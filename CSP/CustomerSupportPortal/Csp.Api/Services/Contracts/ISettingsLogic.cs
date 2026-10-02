using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Xsoar;

namespace Csp.Api.Services.Contracts
{
    public interface ISettingsLogic
    {
        IDictionary<string, string> GetPanservicesWebConfigKeyValues(string key);
    }
}