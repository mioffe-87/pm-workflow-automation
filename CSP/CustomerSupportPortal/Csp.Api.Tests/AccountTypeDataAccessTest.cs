using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {
    [TestClass]
    public class AccountTypeDataAccessTest : DBConnections {
        AccountTypeDataAccess accountTypeDataAccess = null;
        DataParameters dataParameters = null;
        List<AccountType> accountTypeList = null;
        AccountType accountType = null;
        long accountTypeId = 0;

        [TestMethod]
        public void CreateAccountTypeTest() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountTypeList = new List<AccountType>();
            accountType = new AccountType();
            accountType.Name = "Test Name";
            accountType.AdministrativeRole = new Role();
            accountType.AdministrativeRole.Id = 1;
            accountType.Classification = SecurityClassification.Default;
            //accountType.CreatedBy = new UserAccount();
            accountType.CreatedByUserAccountId = 1;
            accountType.AssignableRoles.Add(new Role {
                Id = 1
            });
            accountType.DefaultRoles.Add(new Role {
                Id = 2
            });
            accountType = accountTypeDataAccess.Create(accountType);
            command = Connection.GetSqlStringCommand("select * from accountType where AccountTypeId=last_insert_id();");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (accountType != null) {
                    dataReader.Read();
                    Assert.AreEqual(accountType.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                    Assert.AreEqual(accountType.AdministrativeRole.Id, (long)(dataReader["AdministrativeRoleId"]), "Create Failed");
                    accountTypeId = accountType.Id;
                }
                else {
                    Assert.Fail("Create Failed");
                }
            }
            else {
                Assert.Fail("Create Failed");
            }

            dataReader.Close();

            //REMOVED AS PER TRANSACTION CHANGES

            //Assert.IsNotNull(accountType, "Create Failed");
            //command = Connection.GetSqlStringCommand("select count(1) as count from accounttype where AccountTypeId=" + accountTypeId + " ");
            // dataReader = Connection.ExecuteReader(command);
            //dataReader.Read();
            //Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //dataReader.Close();
            //accountType.AssignableRoles.Add(new Role {
            //    Id = 1
            //});
            //accountType.DefaultRoles.Add(new Role {
            //    Id = 2
            //});
            //foreach (Role role in accountType.AssignableRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();


            //foreach (Role role in accountType.DefaultRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();
        }

        [TestMethod]
        public void AccountTypeByAccountIdRead() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountTypeList = new List<AccountType>();
            accountType = new AccountType();
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype ORDER BY AccountTypeId DESC LIMIT 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];

            dataReader.Close();
            //long accountTypeId = 25;
            accountType = accountTypeDataAccess.Read(accountTypeId);
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttype where AccountTypeId=" + accountTypeId);
             dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(0, 0, "Read Failed");
            }
            dataReader.Close();
        }

        [TestMethod]
        public void AccountTypeByDefaultRead() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountTypeList = new List<AccountType>();
            accountTypeList = accountTypeDataAccess.Read(dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttype");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(accountTypeList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(accountTypeList.Count, 0, "Read Failed");
            }
            dataReader.Close();
            foreach (AccountType accountType in accountTypeList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from accounttype where AccountTypeId=" + accountType.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }

        [TestMethod]
        public void ReadWithPaginationTest() {
            dataParameters = new DataParameters();
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountTypeList = new List<AccountType>();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            accountTypeList = accountTypeDataAccess.Read(dataParameters);
            Assert.IsNotNull(accountTypeList, "Read Failed");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");
            if (accountTypeList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(accountTypeList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(accountTypeList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            }
        }
       
        [TestMethod]
        public void UpdateAccountTypeTest() {
            DbCommand command = null;
            //long accountTypeId = 25;
            dataParameters = new DataParameters();
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountTypeList = new List<AccountType>();
            accountType = new AccountType();
            accountType = new AccountType();
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];
            dataReader.Close();
            accountType = accountTypeDataAccess.Read(accountTypeId);
            Assert.AreEqual(accountType.Id, accountTypeId, "Read for Update Failed");
            accountType.Id = accountTypeId;
            accountType.Name = "Test Name Updatedread";
            accountType.AdministrativeRole.Id = 1;
            accountType.Classification = SecurityClassification.Default;
            accountType.LastUpdateByUserAccountId = 1;
            accountType.AssignableRoles.Add(new Role {
                Id = 1
            });
            accountType.AssignableRoles.Add(new Role
            {
                Id = 4
            });
            accountType.DefaultRoles.Add(new Role {
                Id = 2
            });
            accountType.DefaultRoles.Add(new Role
            {
                Id = 4
            });
            accountType = accountTypeDataAccess.Update(accountType);
            Assert.IsNotNull(accountType, "Update Failed");
            Assert.AreEqual(accountType.Id, accountTypeId, "Read for Update Failed");
            Assert.AreEqual(accountType.Name, "Test Name Updatedread", " Name Update Failed");

            
            command = Connection.GetSqlStringCommand("select count(1) as count from accounttype where AccountTypeId=" + accountTypeId + " ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();

            // rEMOVED AS PER TRANSACTION CHANGES
            //accountType.AssignableRoles.Add(new Role {
            //    Id = 1
            //});
            //accountType.AssignableRoles.Add(new Role {
            //    Id = 4
            //});
           
            //foreach (Role role in accountType.AssignableRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            //    dataReader.Close();
            //}
            //accountType.DefaultRoles.Add(new Role {
            //    Id = 2
            //});
            //accountType.DefaultRoles.Add(new Role {
            //    Id = 4
            //});
            //foreach (Role role in accountType.DefaultRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            //    dataReader.Close();
            //}
        }
        
        [TestMethod]
        public void DeleteAccountTypeTest() {
            DbCommand command = null;
            accountTypeDataAccess = new AccountTypeDataAccess();
            accountType = new AccountType();
            command = Connection.GetSqlStringCommand("select AccountTypeId from accounttype order by AccountTypeId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                accountTypeId = (long)dataReader["AccountTypeId"];
            dataReader.Close();

            accountTypeDataAccess.Delete(accountTypeId, 0);

            command = Connection.GetSqlStringCommand("select count(1) as count from accounttype where AccountTypeId=" + accountTypeId);
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

           
            //foreach (Role role in accountType.AssignableRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypeassignableroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            //}
            //dataReader.Close();

            //foreach (Role role in accountType.DefaultRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from accounttypedefaultroles where AccountTypeId=" + accountTypeId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            //}
            //dataReader.Close();
        }
    }
}
