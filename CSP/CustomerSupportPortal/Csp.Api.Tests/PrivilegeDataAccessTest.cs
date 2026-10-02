using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;


namespace Csp.Api.Tests {

        [TestClass]
        public class PrivilegeDataAccessTest:DBConnections {
        List<Role> roleList = new List<Role>();
        PrivilegeDataAccess privilegeDataAccess = new PrivilegeDataAccess();
        DataParameters dataParameters = new DataParameters();
       

        /// <summary>
        /// The purpose of this method to test RoleDataAccess.Create
        /// </summary>
        [TestMethod]
        public void PrivilegeDataAccessCreate() {
           
            Privilege privilege = new Privilege();
            DbCommand command = null;
            IDataReader dataReader;
           

            privilege.Name = "privilegeTest";
            //privilege.Permission = new Permission();
            privilege.CreatedByUserAccountId = 1;
            privilege.Description = "test";
            

            Privilege resultPrivilege = new Privilege();

            resultPrivilege = privilegeDataAccess.Create(privilege);

            Assert.IsNotNull(resultPrivilege, "Create Failed");
            Assert.AreEqual(privilege.Name, resultPrivilege.Name, "Create Failed");
            Assert.AreEqual(privilege.Permission, resultPrivilege.Permission , "Create Failed");
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
            dataReader.Close();

          


            command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=LAST_INSERT_ID()");

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

            
        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void PrivilegeDataAccessDelete() {

            DbCommand command = null;
            IDataReader dataReader;
            long deletedByUserAccountId = 1;
            long privilegeId = 8;

            privilegeDataAccess.Delete(privilegeId, deletedByUserAccountId);

            Assert.IsNotNull(privilegeId, "Delete Failed");
            Assert.IsNotNull(deletedByUserAccountId, "Delete Failed");
 
            privilegeDataAccess.Delete(privilegeId, deletedByUserAccountId);

            command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + privilegeId );
           
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        ///// <summary>
        ///// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Read
        ///// </summary>
        //[TestMethod]
        //public void PrivilegeDataAccessRead() {
        //    DbCommand command = null;
        //    IDataReader dataReader;
        //    SearchOptions options = new SearchOptions();
        //    List<Privilege> PrivilegeList = new List<Privilege>();

        //    PrivilegeList = privilegeDataAccess.Read(options, dataParameters);
        //    command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + options.SearchByIds);
        //    dataReader = Connection.ExecuteReader(command);

        //    if (dataReader != null) {
        //        dataReader.Read();
        //        Assert.AreEqual(PrivilegeList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
        //    }
        //    else {
        //        Assert.AreEqual(PrivilegeList.Count, 0, "Read Failed");
        //    }

        //    dataReader.Close();
        //    foreach (Privilege  privilege in PrivilegeList) {
        //        command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + privilege.Id);
        //        dataReader = Connection.ExecuteReader(command);
        //        dataReader.Read();
        //        Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
        //        dataReader.Close();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void PrivilegeDataAccessUpdate() {
  
            Privilege privilege = new Privilege();
            Privilege resultPrivilege = new Privilege();
            DbCommand command = null;
            IDataReader dataReader;
            PrivilegeDataAccess privilegeDataAccess = new PrivilegeDataAccess();
            privilege = privilegeDataAccess.Read(2);
            privilege.Name = "privilegeTestUpdate123";
          //  privilege.Permission = (Permission)1;
            privilege.Description = "testUpdate123";
            privilege.LastUpdateByUserAccountId = 1;

            resultPrivilege = privilegeDataAccess.Update(privilege);

            Assert.IsNotNull(resultPrivilege, "Create Failed");
            Assert.AreEqual(privilege.Name, resultPrivilege.Name, "Create Failed");
            Assert.AreEqual(privilege.Permission, resultPrivilege.Permission, "Create Failed");
            Assert.AreEqual(privilege.LastUpdateByUserAccountId, resultPrivilege.LastUpdateByUserAccountId, "Create Failed");
            Assert.AreEqual(privilege.Description, resultPrivilege.Description, "Create Failed");
            Assert.IsNotNull(privilege.LastUpdateByUserAccountId, "Create Failed");
            Assert.IsNotNull(privilege.Id, "Create Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId= " + privilege.Id );

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();


        }

            /// <summary>
            /// 
            /// </summary>
       [TestMethod]
        public void PrivilegeDataAccessReadByID() {
           
            long privilegeId = 2;
            Privilege privilege = new Model.Privilege();
            DbCommand command = null;
            IDataReader dataReader;

            privilege = privilegeDataAccess.Read(privilegeId);

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
    }
}
