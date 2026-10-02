using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;

namespace Csp.Api.Tests {

    [TestClass]
    public class RoleDataAccessTest : DBConnections {

        List<Role> roleList = new List<Role>();
        RoleDataAccess roleDataAccessTest = new RoleDataAccess();
        DataParameters dataParameters = new DataParameters();
        long roleId = 35;
      

        /// <summary>
        /// The purpose of this method to test RoleDataAccess.Create
        /// </summary>
        [TestMethod]
        public void RoleDataAccessCreate() {

            DbCommand command;
            Role role = new Role();
            IDataReader dataReader = null;
            role.Name = "Test2";
            role.Classification = SecurityClassification.Default;
            role.CreatedByUserAccountId = 2;
            role.Description = "TestDesc2";
            role.Privileges = new List<Privilege>();
            role.Privileges.Add(new Privilege {
                 Id=(long)2  
            });
            
            Role resultRole = new Role();
            resultRole = roleDataAccessTest.Create(role);

            Assert.IsNotNull(resultRole, "Create Failed");
            Assert.AreEqual(role.Name, resultRole.Name, "Create Failed");
            Assert.AreEqual(role.Classification, resultRole.Classification, "Create Failed");
            Assert.AreEqual(role.CreatedByUserAccountId, resultRole.CreatedByUserAccountId, "Create Failed");
            Assert.AreEqual(role.Description, resultRole.Description, "Create Failed");
            Assert.IsNotNull(role.CreatedOn,"Create Failed");



            command = Connection.GetSqlStringCommand("select  * from  role Where  roleId=LAST_INSERT_ID()");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(role.Name,Convert.ToString(dataReader["Name"]), "Create Failed");
            Assert.AreEqual(role.CreatedByUserAccountId,(long) dataReader["CreatedByUserAccountId"], "Create Failed");
            Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            Assert.IsNotNull(role.CreatedOn, "Create Failed");
            dataReader.Close();


            command = Connection.GetSqlStringCommand("select count(1) as count  from  role Where  roleId=LAST_INSERT_ID()");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();


            //foreach (Privilege privilege in role.Privileges) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where PrivilegeId=" + privilege.Id + " and RoleId=" + resultRole.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();
          

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void RoleDataAccessDelete() {
           
            DbCommand command = null;
            IDataReader dataReader;

            long deletedByUserAccountId = 1;
            long roleId = 31;
            roleDataAccessTest.Delete(roleId, deletedByUserAccountId);

            Assert.IsNotNull(roleId,"Delete Failed");
            Assert.IsNotNull(deletedByUserAccountId, "Delete Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleID=" + roleId );

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleprivileges.RoleID=" + roleId);

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void RoleDataAccessRead() {
            //DbCommand command = null;
            //IDataReader dataReader;
            //SearchOptions options = new SearchOptions();

            //roleList = roleDataAccessTest.Read(options, dataParameters);
            //command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleId=" + options.SearchByIds);
            //dataReader = Connection.ExecuteReader(command);

            //if (dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(roleList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.AreEqual(roleList.Count, 0, "Read Failed");
            //}

            //dataReader.Close();
            //foreach (Role role in roleList) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //    dataReader.Close();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
        public void RoleDataAccessUpdate() {
           
            Role role = new Role();
            Role resultRole = new Role();
            DbCommand command = null;
            IDataReader dataReader;
            RoleDataAccess roleDataAccess = new RoleDataAccess();

            role = roleDataAccess.Read(35);
          
            role.Name = "Test2test123";
            role.Classification = SecurityClassification.Default;
            role.LastUpdateByUserAccountId =1;
            role.Description = "Test2test123";

            resultRole = roleDataAccessTest.Update(role);

            Assert.IsNotNull(resultRole, "Create Failed");
            Assert.AreEqual(role.Name, resultRole.Name, "Create Failed");
            Assert.AreEqual(role.Classification, resultRole.Classification, "Create Failed");
            Assert.AreEqual(role.LastUpdateByUserAccountId, resultRole.LastUpdateByUserAccountId, "Create Failed");
            Assert.AreEqual(role.Description, resultRole.Description, "Create Failed");
            Assert.IsNotNull(role.LastUpdate, "Create Failed");

            command = Connection.GetSqlStringCommand("select count(1)  as count from role where roleId=" + role.Id);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();

             // As per Transaction Changes
            //foreach (Privilege privilege in role.Privileges) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where PrivilegeId=" + privilege.Id + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}

            dataReader.Close();

 
        }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void RoleDataAccessReadByID() {

             long roleId = 31;
             bool includePrivileges = false;
             bool includeAudit = false;
             Role role = null;
             DbCommand command = null;
             IDataReader dataReader;
              command = null;

             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

              dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 if(dataReader.Read()) {
                     roleId = Convert.ToInt32(dataReader["roleID"]);

                 }
             }
             dataReader.Close();
             role = new Role();
             role = roleDataAccessTest.Read(roleId, includePrivileges, includeAudit);
             Assert.IsNotNull(role, "Create Failed");

             command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleId=" + roleId);
             dataReader = Connection.ExecuteReader(command);

             if (dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.IsNotNull(role, "Read Failed");
             }
             dataReader.Close();
         }
    }
}
