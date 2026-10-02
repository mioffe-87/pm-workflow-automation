using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {
    [TestClass]
    public class CountryDataAccessTest : DBConnections {
        DataParameters dataParameters = null;
        CountryDataAccess countryDataAccess = null;
        List<Country> countryList = null;

        [TestMethod]
        public void CountryDefaultCountriesRead() {
            dataParameters = new DataParameters();
            countryDataAccess = new CountryDataAccess();
            countryList = new List<Country>();
            DbCommand command = null;
            countryList = countryDataAccess.Read(dataParameters);
           
            command = Connection.GetSqlStringCommand("select count(1) as count from country");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(countryList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(countryList.Count, 0, "Read Failed");
            }
            dataReader.Close();
            foreach (Country country in countryList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from country where CountryId=" + country.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }
        [TestMethod]
        public void ReadWithPaginationTest() {
            dataParameters = new DataParameters();
            countryDataAccess = new CountryDataAccess();
            countryList = new List<Country>();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            countryList = countryDataAccess.Read(dataParameters);

            Assert.IsNotNull(countryList, "Read Failed");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from country");
            IDataReader dataReader = Connection.ExecuteReader(command);

            if (countryList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(countryList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(countryList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            }
        }
    }
}
