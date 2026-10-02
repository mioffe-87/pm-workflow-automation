using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests {
    [TestClass]
    public class AccountTypeAssignableRolesDataAccessTest : DBConnections {
        DataParameters dataParameters = null;
        AccountTypeAssignableRolesDataAccess accountTypeAssignableRolesDataAccess = null;
        List<Role> roleList = null;
        Role role = null;
        long Createdby = 1;
        long Deletedby = 1;
        long accountTypeId = 0;
        [TestMethod]
        public void CreateAccountTypeAssignableTest() {
           
            DbCommand command = null;
            accountTypeAssignableRolesDataAccess = new AccountTypeAssignableRolesDataAccess();
            //role = new Role();
            roleList = new List<Role>();
            dataParameters = new DataParameters();
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read()) 
                accountTypeId = (long)dataReader["AccountTypeId"];
            if (accountTypeId != 0) {
                accountTypeAssignableRolesDataAccess.Create(accountTypeId, 2, Createdby);
                roleList = accountTypeAssignableRolesDataAccess.Read(accountTypeId, dataParameters);
                Assert.IsNotNull(roleList, "Create Failed");

                command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and roleId=2 ");
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
                dataReader.Close();
            }
            else {
                Assert.Fail("No Account Found in the database to assign the role");
            }
        }

        [TestMethod]
        public void DefaultAccountTypeAssignableRolesRead() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            accountTypeAssignableRolesDataAccess = new AccountTypeAssignableRolesDataAccess();
            roleList = new List<Role>();
            
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];
           
            roleList = accountTypeAssignableRolesDataAccess.Read(accountTypeId, dataParameters);
          
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId);
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
                command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }

        [TestMethod]
        public void ReadWithPaginationTest() {
            DbCommand command = null;
            roleList = new List<Role>();
            accountTypeAssignableRolesDataAccess = new AccountTypeAssignableRolesDataAccess();
            dataParameters = new DataParameters();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];
            roleList = accountTypeAssignableRolesDataAccess.Read(accountTypeId, dataParameters);
            Assert.IsNotNull(roleList, "Read Failed");
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

       

        [TestMethod]
        public void DeleteAccountTypeAssignableTest() {
            accountTypeAssignableRolesDataAccess = new AccountTypeAssignableRolesDataAccess();
            //long accountTypeId = 16;
            DbCommand command = null;
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];
            accountTypeAssignableRolesDataAccess.Delete(accountTypeId, 2, Deletedby);
           
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and roleId=2 ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }


    }
}
