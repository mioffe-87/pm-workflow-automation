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
    public class RegionDataAccessTest : DBConnections {
        DataParameters dataParameters = null;
        RegionDataAccess regionDataAccess = null;
        List<Region> regionList = null;
        [TestMethod]
        public void DefaultRegionsRead() {
            dataParameters = new DataParameters();
            regionDataAccess = new RegionDataAccess();
            regionList = new List<Region>();
            DbCommand command = null;
            Int16 countryId = 2;

            regionList = regionDataAccess.Read(2, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from region where CountryId=" + countryId);
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(regionList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(regionList.Count, 0, "Read Failed");
            }
            dataReader.Close();
            foreach (Region region in regionList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from region where CountryId=" + countryId + " and CountryRegionId=" + region.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }
        [TestMethod]
        public void ReadWithPaginationTest() {
            dataParameters = new DataParameters();
            regionDataAccess = new RegionDataAccess();
            regionList = new List<Region>();
            Int16 countryId = 1;
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            regionList = regionDataAccess.Read(countryId, dataParameters);
            Assert.IsNotNull(regionList, "Read Failed");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");

            if (regionList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(regionList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(regionList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            }
        }
    }
}
