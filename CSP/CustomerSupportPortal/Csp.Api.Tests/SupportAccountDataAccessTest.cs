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
    /// Summary description for SupportAccountDataAccessTest
    /// </summary>
    [TestClass]
    public class SupportAccountDataAccessTest : DBConnections {

        SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
        DataParameters dataParameters = new DataParameters();
         
        SupportAccount supportAccount = new SupportAccount();
        List<SupportAccount> supportAccounts = new List<SupportAccount>();

        long supportAccountId = 31;
        long Createdby = 1;

        [TestMethod]
        public void SupportAccountCreate() {


            supportAccount = new SupportAccount();
            supportAccount.Name = "TestDispaly";
            
            supportAccount.CreatedByUserAccountId = Createdby;
            supportAccount.CreatedByEmail = "test@gmail.com";
            supportAccount.CreatedByUserAccountId = Createdby;
            supportAccount.CustomerNumber = "Test123";
            supportAccount.IsActive = true;
            supportAccount.IsOptOutOfAllEmails = false;
            supportAccountDataAccess.Create(supportAccount);
              
 

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=last_insert_id();");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (supportAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(supportAccount.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                    Assert.AreEqual(supportAccount.Name, Convert.ToString(dataReader["Name"]), "Update Failed");
                    Assert.AreEqual(supportAccount.CustomerNumber, Convert.ToString(dataReader["CustomerNumber"]), "Create Failed");
                    Assert.AreEqual(supportAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Create Failed");
                    Assert.AreEqual(supportAccount.IsOptOutOfAllEmails, Convert.ToBoolean(dataReader["IsOptOutOfAllEmails"]), "Create Failed");
                    Assert.AreEqual(supportAccount.CreatedByUserAccountId, Convert.ToInt32(dataReader["CreatedByUserAccountId"]), "Create Failed");
                    supportAccountId = supportAccount.Id;
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

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void SupportAccountUpdate() {


            DbCommand command = null;
            IDataReader dataReader;
            long restrictedUserAccountId = 0;
            long supportAccountId = 0;


            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (dataReader.Read()) {
                    supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);

                }
            }

            dataReader.Close();
            command = null;


            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    restrictedUserAccountId = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();


            try {
                 supportAccount = supportAccountDataAccess.Read(supportAccountId, restrictedUserAccountId);
            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get SupportAccount Failed");
                return;
            }

            dataReader.Close();


            supportAccount.Name = "TestDispalyUpdated";
            supportAccount.Address.Address1 = "Pune1Updatet";
            supportAccount.Address.Address2 = "Pune2Updatet";
            supportAccount.Address.City = "PuneUpdatet";
            supportAccount.Address.Country = "IndiaUpdatet";
            supportAccount.Address.Region = "MHAUpdatet";
            supportAccount.Address.PostalCode = "430Updatet";
            supportAccount.CustomerNumber = "TestUpdatet";
            supportAccount.IsActive = true;
            supportAccount.IsOptOutOfAllEmails = true;
            supportAccount.SalesforceId = "2";
            supportAccount.SAPMasterId = 5;
            supportAccount.AccountTypeId = 1;
            supportAccount.LastUpdateByUserAccountId = Createdby;


            try {
                supportAccount = supportAccountDataAccess.Update(supportAccount, restrictedUserAccountId);
            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get SupportAccount Failed");
                return;
            }

            command = null;

            command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=" + supportAccount.Id);

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (supportAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(supportAccount.Id, (long)dataReader["SupportAccountId"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.SAPMasterId, Convert.ToInt32(dataReader["SAPId"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.SalesforceId, dataReader["SalesforceAccountId"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Name, Convert.ToString(dataReader["Name"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.PhoneNumber, dataReader["PhoneNumber"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.IsOptOutOfAllEmails, Convert.ToBoolean(dataReader["IsOptOutOfAllEmails"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.Region, Convert.ToString(dataReader["Region"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.City, Convert.ToString(dataReader["City"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.Address.Country, Convert.ToString(dataReader["Country"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.AccountTypeId, (long)dataReader["AccountTypeId"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.CustomerNumber, Convert.ToString(dataReader["CustomerNumber"]), " Update SupportAccoun Failed");
                    Assert.IsNotNull(dataReader["LastUpdate"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.LastUpdateByUserAccountId, dataReader["LastUpdateByUserAccountId"], " Update SupportAccoun Failed");
                    supportAccountId = (long)dataReader["SupportAccountId"];
                }
                else {
                    Assert.Fail("Update Failed");
                }
            }
            else {
                if (supportAccount != null) {
                    Assert.Fail("Update Failed");
                }
            }

            dataReader.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void SupportAccountRead() {

            DbCommand command = null;
            IDataReader dataReader;
            long restrictedUserAccountId = 0;
            long supportAccountId = 0;

            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (dataReader.Read()) {
                    supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
                }
            }
            dataReader.Close();
            command = null;


            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    restrictedUserAccountId = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();

            try {
                supportAccount = supportAccountDataAccess.Read(supportAccountId, restrictedUserAccountId);
            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get SupportAccount Failed");
                return;
            }

            command = null;

            command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=" + supportAccount.Id);

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (supportAccount != null) {
                    dataReader.Read();
                    Assert.AreEqual(supportAccount.Id, (long)dataReader["SupportAccountId"], "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.SAPMasterId, Convert.ToInt32(dataReader["SAPId"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.SalesforceId, dataReader["SalesforceAccountId"], "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Name, Convert.ToString(dataReader["Name"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.PhoneNumber, dataReader["PhoneNumber"], "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.IsOptOutOfAllEmails, Convert.ToBoolean(dataReader["IsOptOutOfAllEmails"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.Region, Convert.ToString(dataReader["Region"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.City, Convert.ToString(dataReader["City"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.Address.Country, Convert.ToString(dataReader["Country"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.AccountTypeId, (long)dataReader["AccountTypeId"], " Update SupportAccoun Failed");
                    Assert.AreEqual(supportAccount.CustomerNumber, Convert.ToString(dataReader["CustomerNumber"]), "Get SupportAccount Failed");
                    Assert.IsNotNull(dataReader["LastUpdate"], "Get SupportAccount Failed");
                    Assert.AreEqual(supportAccount.LastUpdateByUserAccountId, dataReader["LastUpdateByUserAccountId"], "Get SupportAccount Failed");
                    supportAccountId = (long)dataReader["SupportAccountId"];
                }
                else {
                    Assert.Fail("Read Failed");
                }
            }
            else {
                if (supportAccount != null) {
                    Assert.Fail("Read Failed");
                }
            }

            dataReader.Close();


        }

        [TestMethod]
        public void SupportAccountReadPagination() {

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

       
       



       

       


    }
}
