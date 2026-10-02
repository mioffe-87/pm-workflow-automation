using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests {
    /// <summary>
    /// Summary description for UserAccountDataAccessTest
    /// </summary>
    [TestClass]
    public class UserAccountDataAccessTest : DBConnections {

        UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
        DataParameters dataParameters = new DataParameters();

        UserAccount userAccount = new UserAccount();
        List<UserAccount> userAccounts = new List<UserAccount>();

        long userAccountId = 1;
        long Createdby = 1;

        [TestMethod]
        public void UserAccountCreate() {

            userAccount = new UserAccount();
            userAccount.DisplayName = "TestDispaly3";

            userAccount.Email = "test" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "@test.com";
            userAccount.FirstName = "Test4";
            userAccount.LastName = "Last4";
            userAccount.CreatedByUserAccountId = Createdby;
            //userAccount.Address.Id = 1;
            userAccount.EmailLastSent = DateTime.Now;
            userAccount.CreatedOn = DateTime.Now;
            userAccount.CreatedByUserAccountId = Createdby;
            userAccount.Address.Address1 = "Pune1";
            userAccount.Address.Address2 = "Pune2";
            userAccount.Address.City = "Pune";
            userAccount.Address.Country = "India";
            userAccount.Address.Region = "MHA";
            userAccount.Address.PostalCode = "430001";
            userAccount.Title = "Test1";
            userAccount.PhoneNumber = "34895";
            userAccount.IsActive =true;
            userAccount.IsOptInEmail = false;
            userAccount.EmailResponse = "Test";
            userAccount.IsOptInSecurityEmail = false;
            userAccount = userAccountDataAccess.Create(userAccount);


            //Assert.IsNotNull(role, "Create Failed");
            //Assert.AreEqual(role.Id, 2, "Create Failed");

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=last_insert_id();");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(userAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(userAccount.DisplayName, dataReader["DisplayName"], "Create Failed");
                    Assert.AreEqual(userAccount.Email, dataReader["Email"], "Create Failed");
                    Assert.AreEqual(userAccount.CreatedByUserAccountId, Convert.ToInt32(dataReader["CreatedByUserAccountId"]), "Create Failed");
                    Assert.AreEqual(userAccount.FirstName, dataReader["FirstName"], "Create Failed");
                    Assert.AreEqual(userAccount.LastName, dataReader["LastName"], "Create Failed");
                    userAccountId = userAccount.Id;
                }
                else {
                    Assert.Fail("Create Failed");
                }
            }
            else {
                Assert.Fail("Create Failed");
            }

            dataReader.Close();


        }

        [TestMethod]
        public void UserAccountRead() {

            userAccount = userAccountDataAccess.Read(userAccountId);

            //Assert.IsNotNull(roleList, "Read Failed");

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=" + userAccountId);

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(userAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(userAccount.Id, Convert.ToInt32(dataReader["UseraccountID"]), "Read Failed");
                    Assert.AreEqual(userAccount.DisplayName, Convert.ToString(dataReader["DisplayName"]), "Read Failed");

                    Assert.AreEqual(userAccount.Email, Convert.ToString(dataReader["Email"]), "Read Failed");
                    Assert.AreEqual(userAccount.CreatedByUserAccountId, Convert.ToInt32(dataReader["CreatedByUserAccountId"]), "Read Failed");
                }
                else {
                    Assert.Fail("Read Failed");
                }
            }
            else {
                if(userAccount != null) {
                    Assert.Fail("Read Failed");
                }
            }

            dataReader.Close();
        }

        [TestMethod]
        public void UserAccountReadPagination() {

            //dataParameters.PaginationCount = 10;
            //dataParameters.PaginationOffset = 0;
            //roleList = userAccountDataAccess.Read(userAccountId, dataParameters);

            //Assert.IsNotNull(userAccounts, "Read Failed with Pagination");

            //Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            //Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");


            //if (userAccounts.Count > dataParameters.PaginationCount) {
            //    Assert.Fail("Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(userAccounts.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(userAccounts.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            //}



        }


        [TestMethod]
        public void UserAccountUpdate() {

             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select userAccountId from useraccount where UseraccountID=last_insert_id();");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                
                    if(dataReader.Read()) {
                        userAccountId =Convert.ToInt32(dataReader["userAccountId"]); 
                    }

            }

            dataReader.Close();
 
            userAccount = userAccountDataAccess.Read(userAccountId);

            userAccount.DisplayName = "TestDispalyUpdated";

            userAccount.Email = "test" + userAccountId + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "@test.com";

            userAccount.FirstName = "TestUpdated";
            userAccount.LastName = "LastUpdated";
            //userAccount.Address.Id = 1;
            userAccount.LastUpdateByUserAccountId  = Createdby;
            userAccount.IsOptInEmail = false;
            userAccount.IsOptInSecurityEmail = false;
            userAccount.IsActive = true;
            userAccount.IpAddress = "438534";
            userAccount.EmailResponse = "TestUpdate";
            userAccountDataAccess.Update(userAccount);
            //Assert.IsNotNull(roleList, "Read Failed");

             command = null;

            command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=" + userAccountId);

             dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (userAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(userAccount.Id, Convert.ToInt32(dataReader["UseraccountID"]), "Update Failed");
                    Assert.AreEqual(userAccount.DisplayName, Convert.ToString(dataReader["DisplayName"]), "Update Failed");

                    Assert.AreEqual(userAccount.Email, Convert.ToString(dataReader["Email"]), "Update Failed");
                    Assert.AreEqual(userAccount.CreatedByUserAccountId, Convert.ToInt32(dataReader["CreatedByUserAccountId"]), "Update Failed");
                }
                else {
                    Assert.Fail("Update Failed");
                }
            }
            else {
                if (userAccount != null) {
                    Assert.Fail("Update Failed");
                }
            }

            dataReader.Close();
        }

      
    }
}
