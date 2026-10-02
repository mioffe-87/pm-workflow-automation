using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.HelperClasses;

namespace Csp.Api.Logic
{
    public class MarketplaceAccountLogic : BaseLogic, IMarketplaceAccountLogic
    {
        private readonly IConnectionStrings _connectionStrings;

        public MarketplaceAccountLogic(IConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public bool IsAccountExists(string email)
        {
            bool isAccountExists = false;
            string connString = Secrets.MarketplaceDatabase;
            string sqlText =
                "SELECT Email FROM accounts WHERE LOWER(Email) = LOWER(@email)";

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                var command = new MySqlCommand(sqlText, connection);

                command.Parameters.AddWithValue("email", email);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        isAccountExists = !(dataReader["email"] == null || dataReader["email"] is DBNull);
                    }
                }
            }

            return isAccountExists;
        }

        public void CreateAccount(MarketplaceAccount account)
        {
            string connString = Secrets.MarketplaceDatabase;
            string sqlText =
                "INSERT INTO accounts (Email, SalesforceUsername, SaleforceAccountId, CreatedBy) VALUES (@email, @salesforceUsername, @saleforceAccountId, @createdBy)";

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                var command = new MySqlCommand(sqlText, connection);

                command.Parameters.Add("@email", MySqlDbType.String).Value = account.Email;
                command.Parameters.Add("@salesforceUsername", MySqlDbType.String).Value = account.SalesforceUsername;
                command.Parameters.Add("@saleforceAccountId", MySqlDbType.String).Value = account.SalesforceAccountId;
                command.Parameters.Add("@createdBy", MySqlDbType.String).Value = account.CreatedBy;
                command.Parameters.Add("@createdDt", MySqlDbType.DateTime).Value = DateTime.Now;

                command.ExecuteNonQuery();
            }
        }
    }
}
