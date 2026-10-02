using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.Tests;
using System.Data;
using System.Data.Common;

namespace Csp.Web.Tests {

    /// <summary>
    ///  The purpose of this Class to test AccountTypeDefaultRolesDataAccess
    /// </summary>
    [TestClass]
    public class AccountTypeDefaultRolesDataAccessTest : DBConnections {
        List<Role> roleList = new List<Role>();
        AccountTypeDefaultRolesDataAccess accountTypeDefaultRolesDataAccess = new AccountTypeDefaultRolesDataAccess();
        DataParameters dataParameters = new DataParameters();
        long accountId = 9;
        long roleId = 2;

        long createdByUserAccountId = 1;
        long deletedByUserAccountId = 1;
        Role role = new Role();

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Create
        /// </summary>
        [TestMethod]
        public void AccountTypeDefaultRolesDataAccessCreate() {
            DbCommand command = null;
            IDataReader dataReader;
           
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleId = Convert.ToInt32(dataReader["roleID"]);

                }
            }
            dataReader.Close();
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    accountId = Convert.ToInt32(dataReader["AccountTypeId"]);

                }
            }
            dataReader.Close();
            
            accountTypeDefaultRolesDataAccess.Create(accountId, roleId, createdByUserAccountId);
            Assert.IsNotNull(role, "Create Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void AccountTypeDefaultRolesDataAccessDelete() {
           
            DbCommand command = null;
            IDataReader dataReader;

            accountTypeDefaultRolesDataAccess.Delete(accountId, roleId, deletedByUserAccountId);

            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void AccountTypeDefaultRolesDataAccessRead() {
            DbCommand command = null;
            IDataReader dataReader;
                 
            roleList = accountTypeDefaultRolesDataAccess.Read(accountId, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountId);
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
                command = Connection.GetSqlStringCommand("Select count(1) as count from csp.accounttypedefaultroles where AccountTypeId = " + accountId + " and RoleId = " + role.Id );
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Pagination
        /// </summary>
        [TestMethod]
        public void AccountTypeDefaultRolesDataAccePagination() {
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            roleList = accountTypeDefaultRolesDataAccess.Read(accountId, dataParameters);

            Assert.IsNotNull(roleList, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountId);
            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Failed");
            }

            dataReader.Close();

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
