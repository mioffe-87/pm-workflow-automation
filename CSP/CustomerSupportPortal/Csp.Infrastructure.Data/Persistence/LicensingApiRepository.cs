using System;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using System.Data;

namespace Csp.Infrastructure.Data.Persistence
{
    public class LicensingApiRepository : ILicensingApiRepository
    {
        private IConnectionStrings _connectionStrings;

        public LicensingApiRepository(IConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        
        public LicensingApi GetLicensingApiKey(long supportAccountId)
        {
            LicensingApi apiKeys = new LicensingApi();
            const string sqlQuery = @"select ApiKey, ExpirationDate from licensingapikeys where SupportAccountId = @p_supportAccountId and IsActive AND ProductId=1;";

            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(sqlQuery, connection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddWithValue("@p_supportAccountId", supportAccountId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            apiKeys.LicensingApiKey = reader["ApiKey"].ToString();
                            apiKeys.ExpirationDate = DateTime.Parse(reader["ExpirationDate"].ToString());
                        }
                    }
                }
                connection.Close();
            }

            if (!string.IsNullOrEmpty(apiKeys.LicensingApiKey))
            {
                return new LicensingApi()
                {
                    LicensingApiKey = apiKeys.LicensingApiKey,
                    ExpirationDate = apiKeys.ExpirationDate
                };
            }

            return new LicensingApi();
        }

        public bool ExtendLicensingApi(long supportAccountId, long userAccountId, long apiKeyId, DateTime newExpiryDate)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;

                var activeKeys =
                    from lak in db.licensingapikeys
                    where
                        lak.IsActive
                        && lak.SupportAccountId == supportAccountId
                        && lak.LicensingApiKeyId == apiKeyId
                    select lak;

                foreach (var key in activeKeys)
                {
                    key.UpdatedBy = userAccountId;
                    key.ExpirationDate = newExpiryDate;
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }
    }
}
