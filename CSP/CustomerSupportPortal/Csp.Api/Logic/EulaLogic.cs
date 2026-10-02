using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic
{
    public class EulaLogic : IEulaLogic
    {
        private readonly IEulaDataAccess _eulaDataAccess;
        private readonly IDownloadUrlLogic _downloadUrlLogic;
        public EulaLogic(IEulaDataAccess iEulaDataAccess, IDownloadUrlLogic downloadUrlLogic)
        {
            _eulaDataAccess = iEulaDataAccess;
            _downloadUrlLogic = downloadUrlLogic;
        }

        public IEnumerable<Model.EulaLanguages> GetEulaLanguages()
        {
            return _eulaDataAccess.GetEulaLanguages();
        }

        public Model.SimpleEula GetEula(int languageId)
        {
            var model = _eulaDataAccess.GetEula(languageId);
            _downloadUrlLogic.PopulateEulaDownloadUrl(model);
            return model;
        }

        public IEnumerable<Model.SimpleEula> GetEula()
        {
            var models = _eulaDataAccess.GetEula();
            foreach (var simpleEula in models)
            {
                _downloadUrlLogic.PopulateEulaDownloadUrl(simpleEula);
            }
            return models;
        }

        public bool EndUserAgreements(Model.EulaAgreements agreements)
        {
            return _eulaDataAccess.EndUserAgreements(agreements);
        }


        public string GetEulaText(int languageId, out int eulaId, out string fileName, out string downloadUrl)
        {
            string eulaText = _eulaDataAccess.GetEulaText(languageId, out eulaId, out fileName);
            downloadUrl = _downloadUrlLogic.GetDownloadUrl("EULA", fileName);
            return eulaText;
        }
    }
}
