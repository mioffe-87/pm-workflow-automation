using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess
{
    public class EulaDataAccess : BaseDataAccess, IEulaDataAccess
    {
        public IEnumerable<EulaLanguages> GetEulaLanguages()
        {
            const string sqlQuery = @"
SELECT Id, Language
FROM eulalanguage l";
            var languages = new List<EulaLanguages>();
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        languages.Add(new EulaLanguages()
                            {
                                Id = Convert.ToInt32(dataReader["Id"]),
                                Language = dataReader["Language"].ToString()
                            });
                    }
                }
                return languages;
            }
        }

        public Model.SimpleEula GetEula(int languageId)
        {
            SimpleEula eulaModel = null;
            const string sqlQuery = @"
SELECT Id, EulaText,FileName,EusaFileName,Version,LanguageId
FROM Eula l Where l.IsCurrent=1 and l.LanguageId = @p_LanguageId";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_LanguageId", DbType.Int32, languageId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        eulaModel = new SimpleEula()
                            {
                                Id = Convert.ToInt32(dataReader["Id"]),
                                EulaText = dataReader["EulaText"].ToString(),
                                EulaFileName = dataReader["FileName"].ToString(),
                                EusaFileName = dataReader["EusaFileName"].ToString(),
                                Version = Convert.ToDecimal(dataReader["Version"]),
                                LanguageId = Convert.ToInt32(dataReader["LanguageId"]),
                                IsDefault = Convert.ToInt32(dataReader["LanguageId"]) == EulaLanguage.English
                            };
                    }

                }
            }
            return eulaModel;
        }


        public IEnumerable<SimpleEula> GetEula()
        {
            var eulas = new List<SimpleEula>();
            const string sqlQuery = @"
SELECT Id, EulaText,FileName,EusaFileName,Version,LanguageId
FROM Eula l where isCurrent=1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        eulas.Add(new SimpleEula()
                        {
                            Id = Convert.ToInt32(dataReader["Id"]),
                            EulaText = dataReader["EulaText"].ToString(),
                            EulaDownloadUrl = dataReader["FileName"].ToString(),
                            EusaDownloadUrl = dataReader["EusaFileName"].ToString(),
                            Version = Convert.ToDecimal(dataReader["Version"]),
                            LanguageId = Convert.ToInt32(dataReader["LanguageId"]),
                            IsDefault = Convert.ToInt32(dataReader["LanguageId"]) == EulaLanguage.English
                        });
                   }
                }
            }
            return eulas;
        }


        public bool EndUserAgreements(EulaAgreements ea)
        {
            if (ea.BulkRegistrationId != null && ea.BulkRegistrationId > 0)
            {
                const string sqlQuery = @"
INSERT INTO eulaagreements 
            (EulaId, 
             SupportAccountId, 
             EulaContextId, 
             DeviceId, 
             LicenseId,
             Notes, 
             SignedBy, 
             SignedDate,
             BulkRegistrationId) 
VALUES      (@p_EulaId, 
             @p_AcctId, 
             @p_EulaContextId, 
             @p_DeviceId, 
             @p_LicenseId,
             @p_Notes,  
             @p_SignedBy, 
             CURRENT_TIMESTAMP,
             @p_BulkRegistrationId); ";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_EulaId", DbType.Int32, ea.EulaId);
                    Connection.AddInParameter(command, "@p_AcctId", DbType.Int64, ea.SupportAccountId);
                    Connection.AddInParameter(command, "@p_EulaContextId", DbType.Int32, ea.EulaContextId);
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, ea.DeviceId);
                    Connection.AddInParameter(command, "@p_LicenseId", DbType.Int64, ea.LicenseId);
                    Connection.AddInParameter(command, "@p_Notes", DbType.String, ea.Notes);
                    Connection.AddInParameter(command, "@p_SignedBy", DbType.String, ea.SignedBy);
                    Connection.AddInParameter(command, "@p_BulkRegistrationId", DbType.Int64, ea.BulkRegistrationId);
                    return Connection.ExecuteNonQuery(command) > 0;
                }
            }
            else
            {
                const string sqlQuery = @"
INSERT INTO eulaagreements 
            (EulaId, 
             SupportAccountId, 
             EulaContextId, 
             DeviceId, 
             LicenseId,
             Notes, 
             SignedBy, 
             SignedDate) 
VALUES      (@p_EulaId, 
             @p_AcctId, 
             @p_EulaContextId, 
             @p_DeviceId, 
             @p_LicenseId,
             @p_Notes,  
             @p_SignedBy, 
             CURRENT_TIMESTAMP); ";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_EulaId", DbType.Int32, ea.EulaId);
                    Connection.AddInParameter(command, "@p_AcctId", DbType.Int64, ea.SupportAccountId);
                    Connection.AddInParameter(command, "@p_EulaContextId", DbType.Int32, ea.EulaContextId);
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, ea.DeviceId);
                    Connection.AddInParameter(command, "@p_LicenseId", DbType.Int64, ea.LicenseId);
                    Connection.AddInParameter(command, "@p_Notes", DbType.String, ea.Notes);
                    Connection.AddInParameter(command, "@p_SignedBy", DbType.String, ea.SignedBy);
                    return Connection.ExecuteNonQuery(command) > 0;
                }
            }
        }


        public string GetEulaText(int languageId, out int eulaId, out string fileName)
        {
            string eulaText = string.Empty;
            fileName = string.Empty;
            eulaId = 0;
            const string sqlQuery = @"
SELECT Id, EulaText, FileName
FROM Eula l Where l.IsCurrent=1 and l.LanguageId = @p_LanguageId Limit 1";
            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                Connection.AddInParameter(command, "@p_LanguageId", DbType.Int32, languageId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        eulaId = Convert.ToInt32(dataReader["Id"]);
                        eulaText = dataReader["EulaText"].ToString();
                        fileName = dataReader["FileName"].ToString();
                    }

                }
            }
            return eulaText;
        }
    }
}
