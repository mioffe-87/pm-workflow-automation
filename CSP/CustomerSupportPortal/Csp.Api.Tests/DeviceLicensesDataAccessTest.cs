using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {
    [TestClass]
    public class DeviceLicensesDataAccessTest : DBConnections {
        //DeviceDataAccess deviceLicensesDataAccess = null;
        //DataParameters dataParameters = null;
        //License license = null;
        //List<License> licenseList = null;

        [TestMethod]
        public void DefaultLicenseReadTest() {
            //dataParameters = new DataParameters();
            //deviceLicensesDataAccess = new DeviceDataAccess();
            //licenseList = new List<License>();
            //DbCommand command = null;
            //long deviceId = 1;

            //licenseList = deviceLicensesDataAccess.ReadLicenses(deviceId, dataParameters);
            //command = Connection.GetSqlStringCommand("select count(1) as count from license where DeviceId=" + deviceId);
            //IDataReader dataReader = Connection.ExecuteReader(command);
            //if (dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(licenseList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.AreEqual(licenseList.Count, 0, "Read Failed");
            //}
            //dataReader.Close();
            //foreach (License license in licenseList) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from license where DeviceId=" + deviceId + " and LicenseId=" + license.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //    dataReader.Close();
            //}
        }

        [TestMethod]
        public void ReadLicenseWithPaginationTest() {
            //dataParameters = new DataParameters();
            //deviceLicensesDataAccess = new DeviceDataAccess();
            //licenseList = new List<License>();
            //long deviceId = 1;
            //dataParameters.PaginationCount = 10;
            //dataParameters.PaginationOffset = 0;
            //licenseList = deviceLicensesDataAccess.ReadLicenses(deviceId, dataParameters);
            //Assert.IsNotNull(licenseList, "Read Failed");
            //Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            //Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");

            //if (licenseList.Count > dataParameters.PaginationCount) {
            //    Assert.Fail("Read Failed with Pagination");
            //}
            //if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(licenseList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            //}
            //if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(licenseList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            //}
        }
    }
}
