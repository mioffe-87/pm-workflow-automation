
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Csp.Api.Tests
{
    public abstract class  DBConnections
    {
        protected Database connection;

        public Database Connection
        {
            set
            {
                connection = value;
            }
            get
            {
                if (connection == null)
                {
                    connection = DatabaseFactory.CreateDatabase("CspDatabase");
                }
                return connection;
            }
        }
        
    }
}
