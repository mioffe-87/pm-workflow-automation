using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess
{
    public class AwsDataAccess : BaseDataAccess, IAwsDataAccess
    {
        /*
        public List<AwsRegion> GetAwsRegions()
        {
            var regions = new List<AwsRegion>();
            const string sqlQuery = @"
        SELECT Region,Description
        FROM awsregions;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        regions.Add(new AwsRegion()
                            {
                                ZoneOrRegion = dataReader["Region"].ToString(),
                                ZoneOrRegionText = dataReader["Description"].ToString()
                            });
                    }
                }
            }
            return regions;
        }

        public List<AwsProduct> GetAwsProductCodes()
        {
            var productCodes = new List<AwsProduct>();
            const string sqlQuery = @"
        SELECT productcode,ProductName
        FROM awslookup where isactive=1;";

            using (var command = Connection.GetSqlStringCommand(sqlQuery))
            {
                using (IDataReader dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        productCodes.Add(new AwsProduct()
                            {
                                ProductCode = dataReader["productcode"].ToString(),
                                ProductName = dataReader["ProductName"].ToString()
                            });
                    }
                }
            }
            return productCodes;
        }
         * */
    }
}
