using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess
{
    public class TrapsELADataAccess : BaseDataAccess, ITrapsELADataAccess
    {
        private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly IDeviceRepository _deviceRepository;

        public TrapsELADataAccess(ISupportAccountRepository supportAccountRepository, IDeviceRepository deviceRepository)
        {
            _supportAccountRepository = supportAccountRepository;
            _deviceRepository = deviceRepository;
        }

        public TrapsELAAuthCodeDetails GetAuthCodeDetails(string authCode)
        {
            const string sqlQuery = @"
SELECT 
  l.AuthorizationCode, 
  l.RegistrationDate, 
  l.ExpirationDate, 
  l.IsActive,
    l.ShipDate, 
    l.LicenseTermMonths, 
  pm.PartNumber, 
  pst.PanDbIdentification
FROM license l
INNER JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId
INNER JOIN partsubtype pst ON pm.PartSubtypeId = pst.PartSubtypeId
WHERE l.AuthorizationCode = @p_AuthCode;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                var trapsELAAuthcodes = new List<TrapsELAAuthCodeDetails>();

                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        var trapsAuthcode = new TrapsELAAuthCodeDetails();
                        trapsAuthcode.PopulateProperties(dataReader);
                        trapsELAAuthcodes.Add(trapsAuthcode);
                    }
                }

                return trapsELAAuthcodes.Count != 1 ? null : trapsELAAuthcodes.Single();
            }
        }

    }
}
