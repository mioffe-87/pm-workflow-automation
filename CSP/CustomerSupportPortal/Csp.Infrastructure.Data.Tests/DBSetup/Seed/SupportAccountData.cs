using System.Text;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed
{
    static class SupportAccountData
    {

        //SELECT CONCAT('new supportaccount { SupportAccountId = ', s.SupportAccountId, ', AccountTypeId = ', s.AccountTypeId, ', Name = "', s.Name, '", IsActive = ', IF(s.IsActive = 1, 'true', 'false'), ' },')

        private static readonly supportaccount[] BasicSupportAccounts = new supportaccount[]
      {
        new supportaccount { SupportAccountId = 245, AccountTypeId = 3, Name = "Palo Alto Networks", IsActive = true },
        new supportaccount { SupportAccountId = 92, AccountTypeId = 1, Name = "eBay, Inc.", IsActive = true },
		new supportaccount { SupportAccountId = 3, AccountTypeId = 2, Name = "Accuvant", IsActive = true },
		new supportaccount { SupportAccountId = 66, AccountTypeId = 1, Name = "CompuNet", IsActive = true },
      };

        public static void Seed()
        {
            using (var db = new cspEntities())
            {

                var sb = new StringBuilder();
                foreach (var sa in BasicSupportAccounts)
                {
                    sb.Append(string.Format("INSERT INTO supportaccount(SupportAccountId, AccountTypeId, Name, IsActive) VALUES({0}, {1}, '{2}', {3});", sa.SupportAccountId, sa.AccountTypeId, sa.Name, (sa.IsActive ?? true) ? 1 : 0));
                }

                db.Database.ExecuteSqlCommand(sb.ToString());
            }
        }
    }
}