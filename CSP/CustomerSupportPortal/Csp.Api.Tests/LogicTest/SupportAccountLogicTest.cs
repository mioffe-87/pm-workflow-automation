using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;
using Csp.Api.Logic;

namespace Csp.Api.Tests.LogicTest {

    [TestClass]
    public class SupportAccountLogicTest : DBConnections {

       
       DataParameters dataParameters = new DataParameters();
       SupportAccountLogic supportAccountLogic = new SupportAccountLogic();
       SupportAccount supportAccount = new SupportAccount();
       List<SupportAccount> supportAccounts = new List<SupportAccount>();
       List<Role> roleList = new List<Role>();
       long supportAccountId = 23;
        
       long Createdby = 1;
       long roleId = 33;

        /// <summary>
        /// 
        /// </summary>
       [TestMethod]
       public void CreateSupportAccountLogicTest() {


           supportAccount = new SupportAccount();
           supportAccount.Name = "TestDispalySupport";
          
           supportAccount.SalesforceId = "1";
           supportAccount.CreatedByUserAccountId = Createdby;
           supportAccount.Address.Address1 = "Pune1";
           supportAccount.Address.Address2 = "Pune2";
           supportAccount.Address.City = "Pune";
           supportAccount.Address.Country = "India";
           supportAccount.Address.Region = "MHA";
           supportAccount.Address.PostalCode = "430001";
           supportAccount.CustomerNumber = "Test123";
           supportAccount.IsActive = true;
           supportAccount.IsOptOutOfAllEmails = false;
           supportAccount.PhoneNumber = "34895";
           supportAccount.SAPMasterId = 1;
           int roleID1=0, roleID2=0;
           DbCommand command = null;

           command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");
           IDataReader dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   roleID1 = Convert.ToInt32(dataReader["roleID"]);
               }
           }
           dataReader.Close();
           command = null;

           command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 2,1;");
           dataReader = Connection.ExecuteReader(command);


           if (dataReader != null) {
               if (dataReader.Read()) {
                   roleID2 = Convert.ToInt32(dataReader["roleID"]);
               }
           }
           dataReader.Close();
           command = null;

            
          
           supportAccountLogic.CreateSupportAccount(supportAccount);

           command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=last_insert_id();");

           dataReader = Connection.ExecuteReader(command);


           if (dataReader != null) {
               if (supportAccount != null) {
                   dataReader.Read();
                   Assert.IsNotNull(Convert.ToString(dataReader["SupportAccountId"]), "Create Failed");
                   Assert.AreEqual(supportAccount.SAPMasterId , Convert.ToInt32(dataReader["SAPId"]), "Create Failed");
                   Assert.AreEqual(supportAccount.SalesforceId, dataReader["SalesforceAccountId"], "Create Failed");
                   Assert.AreEqual(supportAccount.Name , Convert.ToString(dataReader["Name"]), "Create Failed");
                   Assert.AreEqual(supportAccount.PhoneNumber, dataReader["PhoneNumber"], "Create Failed");
                   Assert.AreEqual(supportAccount.IsOptOutOfAllEmails, Convert.ToBoolean(dataReader["IsOptOutOfAllEmails"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.Region, Convert.ToString(dataReader["Region"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.City , Convert.ToString(dataReader["City"]), "Create Failed");
                   Assert.AreEqual(supportAccount.Address.Country, Convert.ToString(dataReader["Country"]), "Create Failed");
                   Assert.AreEqual(supportAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Create Failed");
                   Assert.AreEqual(supportAccount.CustomerNumber, Convert.ToString(dataReader["CustomerNumber"]), "Create Failed");
                   Assert.IsNotNull(dataReader["CreatedOn"], "Create Failed");
                   Assert.AreEqual(supportAccount.CreatedByUserAccountId, dataReader["CreatedByUserAccountId"], "Create Failed");
                   supportAccountId =(long) dataReader["SupportAccountId"];
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
       public void UpdateSupportAccounLogicTest() {

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
               supportAccount = supportAccountLogic.GetSupportAccount(supportAccountId, restrictedUserAccountId);
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
           supportAccount.SAPMasterId = 5 ;
           supportAccount.AccountTypeId = 1;
           supportAccount.LastUpdateByUserAccountId  = Createdby;


           try {
               supportAccount = supportAccountLogic.UpdateSupportAccount(supportAccount, 2);
           }
           catch (Exception ex) {
               Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get SupportAccount Failed");
               return;
           }
          
           command = null;

           command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=" + supportAccount.Id );

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
       public void GetSupportAccountLogicTest() {

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

           try
           {
                supportAccount = supportAccountLogic.GetSupportAccount(supportAccountId, restrictedUserAccountId);
           }
           catch (Exception ex)
           {
               Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get SupportAccount Failed");
               return;
           }

           command = null;

           command = Connection.GetSqlStringCommand("select * from supportaccount where supportaccountID=" + supportAccount.Id );

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
       public void PaginationSupportAccountLogicTest() {

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

       /// <summary>
       /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Create
       /// </summary>
       [TestMethod]
       public void CreateSupportAccountDefaultLogicTest() {

           DbCommand command = null;
           IDataReader dataReader;

           int roleID = 0;
           

           command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");
             dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   roleID = Convert.ToInt32(dataReader["roleID"]);
               }
           }
           dataReader.Close();
           command = null;

           command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
               }
           }
           dataReader.Close();
           command = null;
           supportAccountLogic.CreatesupportAccountDefaultRoles(supportAccountId, roleId, Createdby);

           command = Connection.GetSqlStringCommand("select * from supportAccount where supportaccountID=last_insert_id() ");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   Assert.AreEqual(supportAccountId, Convert.ToInt32(dataReader["supportAccountId"]), "Create Failed");
                   Assert.AreEqual(roleId, Convert.ToInt32(dataReader["roleId"]), "Create Failed");
                   Assert.IsNotNull(Convert.ToInt32(dataReader["CreatedOn"]), "Create Failed");
                   Assert.AreEqual(Createdby, Convert.ToInt32(dataReader["CreatedByUserAccountId"]), "Create Failed");
                       
               }
           }
           dataReader.Close();
           command = null;

           command = Connection.GetSqlStringCommand("select count(1) as count  from supportaccountdefaultroles where SupportAccountId=" + supportAccountId + " and roleId=" + roleId);
           dataReader = Connection.ExecuteReader(command);
           dataReader.Read();
           Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
           dataReader.Close();

       }

       

       /// <summary>
       /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Read
       /// </summary>
       [TestMethod]
       public void GetSupportAccountsDefaultRolesLogicTest() {
           DbCommand command = null;
           IDataReader dataReader;

           command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
               }
           }

           roleList = supportAccountLogic.GetsupportAccountDefaultRoles(supportAccountId, dataParameters);

           foreach (Role role in roleList) {
               command = Connection.GetSqlStringCommand("select  *  from supportaccountdefaultroles where roleId=" + role.Id + " and SupportAccountId= " + supportAccountId);
               dataReader = Connection.ExecuteReader(command);
               if (dataReader != null) {
                   while (dataReader.Read()) {
                       Assert.AreEqual(role.Id, (long)dataReader["roleId"], "Read Failed");
                       Assert.AreEqual(supportAccountId, (long)dataReader["supportAccountId"], "Read Failed");  
                   }

               }
               else {
                   Assert.AreEqual(0, 0, "Read Failed");
               }
           }



           command = Connection.GetSqlStringCommand("select count(1) as count  from supportaccountdefaultroles where SupportAccountId=" + supportAccountId);
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
       public void PaginationSupportAccountDefaultLogicTest() {
           dataParameters.PaginationCount = 10;
           dataParameters.PaginationOffset = 0;
           DbCommand command = null;
           IDataReader dataReader;

           command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
               }
           }

           roleList = supportAccountLogic.GetsupportAccountDefaultRoles(supportAccountId, dataParameters);
           Assert.IsNotNull(roleList, "Read Failed with Pagination");
           Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");


           foreach (Role role in roleList) {
               command = Connection.GetSqlStringCommand("select  *  from supportaccountdefaultroles where roleId=" + role.Id + " and SupportAccountId= " + supportAccountId);
               dataReader = Connection.ExecuteReader(command);
               if (dataReader != null) {
                   while (dataReader.Read()) {
                       Assert.AreEqual(role.Id, (long)dataReader["roleId"], "Read Failed");
                       Assert.AreEqual(supportAccountId, (long)dataReader["supportAccountId"], "Read Failed");
                   }

               }
               else {
                   Assert.AreEqual(0, 0, "Read Failed");
               }
           }


           command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId);
            dataReader = Connection.ExecuteReader(command);


           if (dataReader != null) {
               dataReader.Read();
               Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
           }
           else {
               Assert.AreEqual(roleList.Count, 0, "Read Failed");
           }

           dataReader.Close();

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
       /// The purpose of this method to test SupportAccountDefaultRolesDataAccess.Delete
       /// </summary>
       [TestMethod]
       public void DeleteSupportAccountDefaultLogicTest() {

           DbCommand command = null;
           IDataReader dataReader;


           command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   roleId = Convert.ToInt32(dataReader["roleID"]);
               }
           }
           dataReader.Close();
           command = null;

           command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               if (dataReader.Read()) {
                   supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
               }
           }
           dataReader.Close();
           command = null;

           supportAccountLogic.DeleteSupportAccountDefaultRole(supportAccountId, roleId, 0);
           command = Connection.GetSqlStringCommand("select count(1) as count from supportaccountdefaultroles where SupportAccountId=" + supportAccountId + " and roleId=" + roleId);
           dataReader = Connection.ExecuteReader(command);
           dataReader.Read();
           Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
           dataReader.Close();

       }
    }
}
