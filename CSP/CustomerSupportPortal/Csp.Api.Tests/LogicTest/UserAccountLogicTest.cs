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
    public  class UserAccountLogicTest:DBConnections {

      
        DataParameters dataParameters = new DataParameters();
        UserAccountLogic userAccountLogic=new UserAccountLogic();
        UserAccount userAccount = new UserAccount();
        List<UserAccount> userAccounts = new List<UserAccount>();
        List<Role> roleList = new List<Role>();
        List<Privilege> privileges = new List<Privilege>();
        Role role = new Role();

        long roleID = 41;
        long privilegeId = 36;
        long userAccountId = 32  ;
        long Createdby = 1;

         [TestMethod]
        public void CreateUserAccountLogicTest() {


            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);
                }
            }
            dataReader.Close();
            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (dataReader.Read()) {
                    privilegeId = Convert.ToInt32(dataReader["privilegeID"]);
                }
            }
            dataReader.Close();
            command = null;

            userAccount = new UserAccount();
            userAccount.DisplayName = "TestDispaly3";
            userAccount.Email = "test" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "@test.com";
            userAccount.FirstName = "Test4";
            userAccount.LastName = "Last4";
            userAccount.CreatedByUserAccountId = Createdby;
            userAccount.EmailLastSent = DateTime.Now;
            userAccount.CreatedOn = DateTime.Now;
            userAccount.CreatedByUserAccountId = Createdby;
            userAccount.Address.Address1 = "Pune1";
            userAccount.Address.Address2 = "Pune2";
            userAccount.Address.City = "Pune";
            userAccount.Address.Country = "India";
            userAccount.Address.Region = "MHA";
            userAccount.Address.PostalCode = "430001";
            userAccount.IsOptInEmail = false;
            userAccount.IsOptInSecurityEmail = false;
            userAccount.IsActive = true;
            userAccount.IpAddress = "438534";
            userAccount.EmailResponse = "TestUpdate";
            userAccount.Title = "Test1";
            userAccount.PhoneNumber = "34895";

            userAccount = userAccountLogic.CreateUserAccount(userAccount);

            command = null;

            command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=last_insert_id();");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(userAccount != null) {
                    dataReader.Read();
                    Assert.IsNotNull(Convert.ToInt64(dataReader["UserAccountId"]), "Create Failed");
                    Assert.AreEqual(userAccount.Email,Convert.ToString(dataReader["Email"]), "Create Failed");
                    Assert.AreEqual(userAccount.DisplayName,Convert.ToString(dataReader["DisplayName"]), "Create Failed");
                    Assert.AreEqual(userAccount.Title, Convert.ToString(dataReader["Title"]), "Create Failed");
                    Assert.AreEqual(userAccount.FirstName, Convert.ToString(dataReader["FirstName"]), "Create Failed");
                    Assert.AreEqual(userAccount.LastName, Convert.ToString(dataReader["LastName"]), "Create Failed");
                    Assert.AreEqual(userAccount.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.Region, Convert.ToString(dataReader["Region"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.City, Convert.ToString(dataReader["City"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Create Failed");
                    Assert.AreEqual(userAccount.Address.Country, Convert.ToString(dataReader["Country"]), "Create Failed");
                    Assert.AreEqual(userAccount.IsOptInEmail,Convert.ToBoolean(dataReader["IsOptInEmail"]), "Create Failed");
                    Assert.AreEqual(userAccount.EmailLastSent.ToString(),Convert.ToString(dataReader["EmailLastSent"]), "Create Failed");
                    Assert.AreEqual(userAccount.EmailResponse,Convert.ToString( dataReader["EmailResponse"]), "Create Failed");
                    Assert.AreEqual(userAccount.IsOptInSecurityEmail,Convert.ToBoolean(dataReader["IsOptInSecurityEmail"]), "Create Failed");
                    Assert.AreEqual(userAccount.IpAddress,Convert.ToString( dataReader["IpAddress"]), "Create Failed");
                    Assert.IsNotNull(dataReader["CreatedOn"], "Create Failed");
                    Assert.AreEqual(userAccount.CreatedByUserAccountId,Convert.ToInt64(dataReader["CreatedByUserAccountId"]), "Create Failed");
                    Assert.AreEqual(userAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Create Failed");
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

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void GetUserAccountLogicTest() {

             DbCommand command = null;

             command = Connection.GetSqlStringCommand("SELECT  UserAccountId FROM useraccount order by UserAccountId desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     userAccountId = Convert.ToInt32(dataReader["UserAccountId"]);

                 }
             }
             dataReader.Close();
             command = null;



             userAccount = userAccountLogic.GetUserAccount(userAccountId);
             command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=" + userAccountId);
              dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (userAccount != null) {
                     dataReader.Read();
                     Assert.AreEqual(userAccount.Id, Convert.ToInt64(dataReader["UserAccountId"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Email, Convert.ToString(dataReader["Email"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.DisplayName, Convert.ToString(dataReader["DisplayName"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Title, Convert.ToString(dataReader["Title"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.FirstName, Convert.ToString(dataReader["FirstName"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.LastName, Convert.ToString(dataReader["LastName"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Region, Convert.ToString(dataReader["Region"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.City, Convert.ToString(dataReader["City"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Country, Convert.ToString(dataReader["Country"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.IsOptInEmail, Convert.ToBoolean(dataReader["IsOptInEmail"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.EmailLastSent.ToString(), Convert.ToString(dataReader["EmailLastSent"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.EmailResponse, Convert.ToString(dataReader["EmailResponse"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.IsOptInSecurityEmail, Convert.ToBoolean(dataReader["IsOptInSecurityEmail"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.IpAddress, dataReader["IpAddress"], "Get UserAccount Failed");
                     Assert.IsNotNull(dataReader["CreatedOn"], "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.CreatedByUserAccountId, Convert.ToInt64(dataReader["CreatedByUserAccountId"]), "Get UserAccount Failed");
                     Assert.AreEqual(userAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Create Failed");
                 }
                 else {
                     Assert.Fail("Read Failed");
                 }
             }
             else {
                 if (userAccount != null) {
                     Assert.Fail("Read Failed");
                 }
             }

             dataReader.Close();
         }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void UpdateUserAccountLogicTest() {

             DbCommand command = null;

             command = Connection.GetSqlStringCommand("SELECT  UserAccountId FROM useraccount order by UserAccountId desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     userAccountId = Convert.ToInt32(dataReader["UserAccountId"]);

                 }
             }
             dataReader.Close();
             command = null;
             command = Connection.GetSqlStringCommand("select userAccountId from useraccount where UseraccountID=" + userAccountId);

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {

                 if (dataReader.Read()) {
                     userAccountId = Convert.ToInt32(dataReader["userAccountId"]);
                 }

             }

             dataReader.Close();

             userAccount = userAccountLogic.GetUserAccount(userAccountId);

             userAccount.DisplayName = "TestDispalyUpdated";
             userAccount.Email = "test" + userAccountId + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "@test.com";
             userAccount.FirstName = "Test4Updated";
             userAccount.LastName = "Last4Updated";
             userAccount.EmailLastSent = DateTime.Now;
             userAccount.LastUpdateByUserAccountId = Createdby;
             userAccount.Address.Address1 = "Pune1Updated";
             userAccount.Address.Address2 = "Pune2Updated";
             userAccount.Address.City = "PuneUpdated";
             userAccount.Address.Country = "IndiaUpdated";
             userAccount.Address.Region = "MHAUpdated";
             userAccount.LastUpdateByUserAccountId = Createdby;
             userAccount.IsOptInEmail = true;
             userAccount.IsOptInSecurityEmail = true;
             userAccount.IsActive = true;
             userAccount.IpAddress = "438Updated";
             userAccount.EmailResponse = "TestUpdate";
             userAccount.Address.PostalCode = "4300Updated";
             userAccount.PhoneNumber = "34895Updated";
            
             userAccountLogic.UpdateUserAccount(userAccount);

             Assert.IsNotNull(userAccountLogic, "Read Failed");

             command = null;

             command = Connection.GetSqlStringCommand("select * from useraccount where UseraccountID=" + userAccountId);

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (userAccount != null) {
                     dataReader.Read();
                     Assert.AreEqual(userAccount.Id, Convert.ToInt64(dataReader["UserAccountId"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Email, Convert.ToString(dataReader["Email"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.DisplayName, Convert.ToString(dataReader["DisplayName"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Title, Convert.ToString(dataReader["Title"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.FirstName, Convert.ToString(dataReader["FirstName"])," Update UserAccount Failed");
                     Assert.AreEqual(userAccount.LastName, Convert.ToString(dataReader["LastName"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Address1, Convert.ToString(dataReader["Address1"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Address2, Convert.ToString(dataReader["Address2"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Region, Convert.ToString(dataReader["Region"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.City, Convert.ToString(dataReader["City"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.Address.Country, Convert.ToString(dataReader["Country"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.IsOptInEmail, Convert.ToBoolean(dataReader["IsOptInEmail"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.EmailLastSent.ToString(), Convert.ToString(dataReader["EmailLastSent"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.EmailResponse, Convert.ToString(dataReader["EmailResponse"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.IsOptInSecurityEmail, Convert.ToBoolean(dataReader["IsOptInSecurityEmail"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.IpAddress, dataReader["IpAddress"], " Update UserAccount Failed");
                     Assert.IsNotNull(dataReader["LastUpdate"], " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.LastUpdateByUserAccountId, Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]), " Update UserAccount Failed");
                     Assert.AreEqual(userAccount.IsActive, Convert.ToBoolean(dataReader["IsActive"]), " Update UserAccount Failed");
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

        /// <summary>
        /// 
        /// </summary>
         //[TestMethod]
         //public void DeleteUserAccountLogicTest() {

         //    DbCommand command = null;

         //    command = Connection.GetSqlStringCommand("select userAccountId from useraccount where UseraccountID=" + userAccountId);

         //    IDataReader dataReader = Connection.ExecuteReader(command);


         //    if (dataReader != null) {

         //        if (dataReader.Read()) {
         //            userAccountId = Convert.ToInt32(dataReader["userAccountId"]);
         //        }


         //    }

         //    dataReader.Close();
         //  //  userAccountLogic.DeleteUserAccount(userAccountId, Createdby);

         //    command = null;

         //    command = Connection.GetSqlStringCommand("select count(1) as count from useraccount where useraccountID=" + userAccountId);
         //    dataReader = Connection.ExecuteReader(command);
         //    dataReader.Read();
         //    Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
         //    dataReader.Close();

         //}

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void CreatePrivilegeLogicTest() {

             Privilege privilege = new Privilege();
             DbCommand command = null;
             IDataReader dataReader;


             privilege.Name = "privilegeTest123";
             //privilege.Permission = new Permission();
             privilege.CreatedByUserAccountId = 1;
             privilege.Description = "test";
             long privilegeId = 0;

             Privilege resultPrivilege = new Privilege();

             resultPrivilege = userAccountLogic.CreatePrivilege(privilege);
             privilegeId = resultPrivilege.Id;
             Assert.IsNotNull(resultPrivilege, "Create Failed");
             Assert.AreEqual(privilege.Name, resultPrivilege.Name, "Create Failed");
             Assert.AreEqual(privilege.Permission, resultPrivilege.Permission, "Create Failed");
             Assert.AreEqual(privilege.CreatedByUserAccountId, resultPrivilege.CreatedByUserAccountId, "Create Failed");
             Assert.AreEqual(privilege.Description, resultPrivilege.Description, "Create Failed");
             Assert.IsNotNull(privilege.CreatedOn, "Create Failed");
             Assert.IsNotNull(privilege.Id, "Create Failed");

             command = Connection.GetSqlStringCommand("select  * from Privilege where PrivilegeId=LAST_INSERT_ID()");

             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
             Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
             Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
             Assert.IsNotNull(privilege.CreatedOn, "Create Failed");
             Assert.IsNotNull(privilege.Id, "Create Failed");
             privilegeId = (long)dataReader["privilegeId"];
             dataReader.Close();

             command = null;
             command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + privilegeId);

             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Delete Failed");
             dataReader.Close();


         }

        
        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void GetPrivilegeLogicTest() {

           
             Privilege privilege = new Privilege();
             DbCommand command = null;
             IDataReader dataReader;

             command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     privilegeId = Convert.ToInt32(dataReader["privilegeID"]);
                 }
             }
             dataReader.Close();
             command = null;

             privilege = userAccountLogic.GetPrivilege(privilegeId);

             command = Connection.GetSqlStringCommand("Select Count(1) as count from privilege where PrivilegeId=" + privilegeId);
             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.IsNotNull(privilege, "Read Failed");
             }
             dataReader.Close();
         }


         /// <summary>
         /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
         /// </summary>
         /// <summary>
         /// 
         /// </summary>
         [TestMethod]
         public void UpdatePrivilegeLogicTest() {

             Privilege privilege = new Privilege();
             Privilege resultPrivilege = new Privilege();
             DbCommand command = null;
             IDataReader dataReader;
             PrivilegeDataAccess privilegeDataAccess = new PrivilegeDataAccess();

             command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     privilegeId = Convert.ToInt32(dataReader["privilegeID"]);
                 }
             }
             dataReader.Close();
             command = null;

             privilege = userAccountLogic.GetPrivilege(privilegeId);
             privilege.Name = "privilegeTestUpdate123";
             //privilege.Permission = (Permission)1;
             privilege.Description = "testUpdate123";
             privilege.LastUpdateByUserAccountId = 1;

             resultPrivilege = userAccountLogic.UpdatePrivilege(privilege);

             Assert.IsNotNull(resultPrivilege, " Update Privilege");
             Assert.AreEqual(privilege.Name, resultPrivilege.Name, " Update Privilege");
             Assert.AreEqual(privilege.Permission, resultPrivilege.Permission, " Update Privilege");
             Assert.AreEqual(privilege.LastUpdateByUserAccountId, resultPrivilege.LastUpdateByUserAccountId, " Update Privilege");
             Assert.AreEqual(privilege.Description, resultPrivilege.Description, " Update Privilege");
             Assert.IsNotNull(privilege.LastUpdateByUserAccountId, " Update Privilege");
             Assert.IsNotNull(privilege.Id, " Update Privilege");

             command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId= " + privilege.Id);

             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Privilege");
             dataReader.Close();


         }


         /// <summary>
         /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
         /// </summary>
         [TestMethod]
         public void DeletePrivilegeLogicTest() {

             DbCommand command = null;
             IDataReader dataReader;
             long deletedByUserAccountId = 1;
             command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     privilegeId = Convert.ToInt32(dataReader["privilegeID"]);
                 }
             }
             dataReader.Close();
             command = null;

             DeleteRolePrivilegesLogicTest();
             userAccountLogic.DeletePrivilege(privilegeId, deletedByUserAccountId);

             Assert.IsNotNull(privilegeId, "Delete Failed");
             Assert.IsNotNull(deletedByUserAccountId, "Delete Failed");

             userAccountLogic.DeletePrivilege(privilegeId, deletedByUserAccountId);

             command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + privilegeId);

             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
             dataReader.Close();

         }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void CreateRolePrivilegesLogicTest() {

             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     roleID = Convert.ToInt32(dataReader["roleID"]);
                 }
             }
             dataReader.Close();
             command = null;

             command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     privilegeId = Convert.ToInt32(dataReader["privilegeID"]);
                 }
             }
             dataReader.Close();
             command = null;

             userAccountLogic.CreateRolePrivilege(roleID, privilegeId, Createdby);

             command = Connection.GetSqlStringCommand("select roleID, privilegeId from roleprivileges where roleID=" + roleID + " and privilegeID= " + privilegeId);
             dataReader = Connection.ExecuteReader(command);
             if(dataReader.Read())
             {
                 Assert.AreEqual(roleID, Convert.ToInt32(dataReader["roleID"]), "Create Failed");
                 Assert.AreEqual(privilegeId, Convert.ToInt32(dataReader["privilegeId"]), "Create Failed");
             }
             dataReader.Close();
             command = null;


             command = Connection.GetSqlStringCommand("select count(1) as count  from roleprivileges where roleID=" + roleID + " and privilegeID= " + privilegeId);
             dataReader = Connection.ExecuteReader(command);

             dataReader.Read();
             Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
             dataReader.Close();


         }


        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void GetRolePrivilegesLogicTest() {



             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     roleID = Convert.ToInt32(dataReader["roleID"]);

                 }
             }
             dataReader.Close();
             command = null;

             privileges = userAccountLogic.GetRolePrivileges(roleID, dataParameters);


             foreach (Privilege privilege in privileges) {
                 command = Connection.GetSqlStringCommand("select  *  from Privilege where privilegeID= " + privilege.Id );
                 dataReader = Connection.ExecuteReader(command);
                 if (dataReader != null) {
                     while (dataReader.Read()) {
                         Assert.AreEqual(privilege.Id, (long)dataReader["privilegeId"], "Read Failed");
                         Assert.AreEqual(privilege.Name, dataReader["Name"], "Read Failed");
                         Assert.AreEqual(privilege.Description,dataReader["Description"], "Read Failed");
                     }

                 }
                 else {
                     Assert.AreEqual(0, 0, "Read Failed");
                 }
             }


             command = Connection.GetSqlStringCommand("select roleID  from roleprivileges where roleID=" + roleID );
             dataReader = Connection.ExecuteReader(command);
             if (dataReader.Read()) {
                 Assert.AreEqual(roleID, Convert.ToInt32(dataReader["roleID"]), "Create Failed");
             }
             dataReader.Close();
             command = null;
             
 
             command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID);
             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(privileges.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(privileges.Count, 0, "Read Failed");
             }

             dataReader.Close();
             foreach (Privilege privilege in privileges) {
                 command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeId=" + privilege.Id);
                 dataReader = Connection.ExecuteReader(command);
                 dataReader.Read();
                 Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                 dataReader.Close();
             }

         }


        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void PaginationRolePrivilegesLogicTest() {


             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     roleID = Convert.ToInt32(dataReader["roleID"]);

                 }
             }
             dataReader.Close();
             command = null;


             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;
             privileges = userAccountLogic.GetRolePrivileges(roleID, dataParameters);

             Assert.IsNotNull(privileges, "Read Failed with Pagination");

             Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

             command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleId=" + roleID);
             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(privileges.Count, 0, "Read Failed");
             }

             dataReader.Close();


             if (privileges.Count > dataParameters.PaginationCount) {
                 Assert.Fail("Read Failed with Pagination");
             }

             if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(privileges.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
             }

             if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(privileges.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

             }



         }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void DeleteRolePrivilegesLogicTest() {

             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     roleID = Convert.ToInt32(dataReader["roleID"]);

                 }
             }
             dataReader.Close();
             command = null;

             command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 if (dataReader.Read()) {
                     privilegeId = Convert.ToInt32(dataReader["privilegeID"]);

                 }
             }
             dataReader.Close();

             userAccountLogic.DeleteRolePrivilege(roleID, privilegeId, 0);

             command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID=" + privilegeId);
             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
             dataReader.Close();

         }

    }
}
