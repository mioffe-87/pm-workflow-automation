using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;

namespace Csp.Api.Tests {

    /// <summary>
    ///  The purpose of this Class to test SupportAccountDefaultRolesDataAccess
    /// </summary>
    [TestClass]
    public class SupportAccountDefaultRolesDataAccessTest : DBConnections {
        List<Role> roleList = new List<Role>();
        SupportAccountDefaultRolesDataAccess supportAccountDefaultRolesDataAccess = new SupportAccountDefaultRolesDataAccess();
        DataParameters dataParameters = new DataParameters();
        long supportAccountId = 23;
        long roleId =2;
        long createdByUserAccountId = 1;
        Role role = new Role();



        /// <summary>
        /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void SupportAccountDefaultRolesDataAccessRead() {
            DbCommand command = null;
            IDataReader dataReader;

            roleList = supportAccountDefaultRolesDataAccess.Read(supportAccountId, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId);
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
                command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId + " and RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }

        /// <summary>
        /// The purpose of this method to test SupportAccountDefaultRolesDataAccess Pagination
        /// </summary>
        [TestMethod]
        public void SupportAccountDefaultRolesDataAccessPagination() {
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;

            roleList = supportAccountDefaultRolesDataAccess.Read(supportAccountId, dataParameters);
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


        /// <summary>
        /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Create
        /// </summary>
        [TestMethod]
        public void SupportAccountDefaultRolesDataAccessCreate() {

            DbCommand command = null;
            IDataReader dataReader;

            supportAccountDefaultRolesDataAccess.Create(supportAccountId, roleId, createdByUserAccountId);

            command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId + " and roleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();

        }


        /// <summary>
        /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void SupportAccountDefaultRolesDataAccessDelete() {

            DbCommand command = null;
            IDataReader dataReader;

            supportAccountDefaultRolesDataAccess.Delete(supportAccountId, roleId, 0);
            command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId + " and roleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }
    }
}

