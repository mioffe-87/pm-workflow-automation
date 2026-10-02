using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IEulaDataAccess
    {
        IEnumerable<EulaLanguages> GetEulaLanguages();
        SimpleEula GetEula(int languageId);
        IEnumerable<SimpleEula> GetEula();
        bool EndUserAgreements(EulaAgreements agreements);
        string GetEulaText(int languageId, out int eulaId, out string fileName);
    }
}
