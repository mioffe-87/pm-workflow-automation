using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests { 

    /// <summary>
    ///  The purpose of this Class to test DeviceDefaultRolesDataAccess
    /// </summary>
    [TestClass]
    public class DeviceDefaultRolesDataAccessTest : DBConnections {
        List<Role> roleList = new List<Role>();
        DeviceDefaultRolesDataAccess deviceDefaultRolesDataAccess = new DeviceDefaultRolesDataAccess();
        DataParameters dataParameters = new DataParameters();
        long deviceId = 1;
        long roleId = 2;
        long createdByUserAccountId = 1;
      
        Role role = new Role();

        /// <summary>
        ///  The purpose of this method to test DeviceDefaultRolesDataAccess.Create
        /// </summary>
        [TestMethod]
        public void DeviceDefaultRolesDataAccessCreate() {
            
            DbCommand command = null;
            IDataReader dataReader;

            deviceDefaultRolesDataAccess.Create(deviceId, roleId, createdByUserAccountId);

            command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and roleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        /// <summary>
        /// The purpose of this method to test DeviceDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void DeviceDefaultRolesDataAccessDelete() {
            DbCommand command = null;
            IDataReader dataReader;

            deviceDefaultRolesDataAccess.Delete(deviceId, roleId,0);
         
            command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and RoleId= " + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }

        /// <summary>
        /// The purpose of this method to test DeviceDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void DeviceDefaultRolesDataAccessRead() {
            DbCommand command = null;
            IDataReader dataReader;

            roleList = deviceDefaultRolesDataAccess.Read(deviceId, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(roleList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Failed");
            }

            dataReader.Close();
            foreach (Role role in roleList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }

        /// <summary>
        ///  The purpose of this method to test DeviceDefaultRolesDataAccess Pagination
        /// </summary>
        [TestMethod]
        public void DeviceDefaultRolesDataAccessPagination() {
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            roleList = deviceDefaultRolesDataAccess.Read(deviceId, dataParameters);

            Assert.IsNotNull(roleList, "Read Failed with Pagination");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");

            if (roleList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");

            }
        }
    }
}
