using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using Csp.Api.Model;
using System.Data;
using Csp.Api.Logic;
using Csp.Api.DataAccess;

namespace Csp.Api.Tests.LogicTest {
    /// <summary>
    /// Summary description for SecurityLogicTest
    /// </summary>
    [TestClass]
    public class SecurityLogicTest : DBConnections {
        SecurityLogic securityLogic = new SecurityLogic();
        DataParameters dataParameters = new DataParameters();

        [TestMethod]
        public void CreateRoleTest() {
            List<Role> roleList = new List<Role>();
            long privilegeID = 0;
            long CreatedByUserAccountId = 0;
            DataParameters dataParameters = new DataParameters();
            DbCommand command;
            Role role = new Role();
            IDataReader dataReader = null;

            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt64(dataReader["privilegeID"]);

                }
            }
            dataReader.Close();

            command = null;

            command = Connection.GetSqlStringCommand("select UserAccountID from UserAccount order by UserAccountID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    CreatedByUserAccountId = Convert.ToInt64(dataReader["UserAccountID"]);

                }
            }
            dataReader.Close();

            role.Name = "Test2";
            role.Classification = SecurityClassification.Default;
            role.CreatedByUserAccountId = CreatedByUserAccountId;
            role.Description = "TestDesc2";
            role.Privileges = new List<Privilege>();
            role.Privileges.Add(new Privilege {
                Id = privilegeID
            });
            

            Role resultRole = new Role();
            resultRole = securityLogic.CreateRole(role);

          
            command = Connection.GetSqlStringCommand("select  * from  role Where  roleId=LAST_INSERT_ID()");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(resultRole.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            Assert.AreEqual(resultRole.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
            Assert.AreEqual(resultRole.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            Assert.AreEqual(resultRole.Classification,((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
            Assert.IsNotNull(resultRole.CreatedOn, "Create Failed");
            dataReader.Close();


            foreach (Privilege privilege in role.Privileges) {
                command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where PrivilegeId=" + privilege.Id + " and RoleId=" + resultRole.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            }
            dataReader.Close();

        }

        [TestMethod]
        public void GetRoleTest() {
            long roleId = 0;
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
            role = securityLogic.GetRole(roleId, includePrivileges, includeAudit);
            Assert.IsNotNull(role, "Read Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);

            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.IsNotNull(role, "Read Failed");
            }
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select  * from  role Where  roleId= "+roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(role.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            if(dataReader["CreatedByUserAccountId"] != DBNull.Value)
                Assert.AreEqual(role.CreatedByUserAccountId, Convert.ToInt64(dataReader["CreatedByUserAccountId"]), "Read Failed");
            if(dataReader["LastUpdateByUserAccountId"] != DBNull.Value)
                Assert.AreEqual(role.LastUpdateByUserAccountId, Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]), "Read Failed");
            Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            Assert.AreEqual(role.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
            Assert.IsNotNull(role.CreatedOn, "Create Failed");
            dataReader.Close();

        }

        [TestMethod]
        public void GetAllRolesTest() {
            //long roleId = 0;
            //bool includePrivileges = false;
            //bool includeAudit = false;
            //Role role = null;
            //DbCommand command = null;
            //IDataReader dataReader;
            //command = null;

            //command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            //dataReader = Connection.ExecuteReader(command);


            //if(dataReader != null) {
            //    if(dataReader.Read()) {
            //        roleId = Convert.ToInt32(dataReader["roleID"]);

            //    }
            //}
            //dataReader.Close();
            //role = new Role();
            //role = securityLogic.GetRole(roleId, includePrivileges, includeAudit);
            //Assert.IsNotNull(role, "Read Failed");

            //command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleId=" + roleId);
            //dataReader = Connection.ExecuteReader(command);

            //if(dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.IsNotNull(role, "Read Failed");
            //}
            //dataReader.Close();

        }

        [TestMethod]
        public void UpdateRoleTest() {
            long roleId = 0;
            long privilegeID = 0;
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

            role = securityLogic.GetRole(roleId, includePrivileges, includeAudit);
            Assert.IsNotNull(role, "Read Failed while update");

            role.Name = "Updated name";
            role.Classification = SecurityClassification.Global;
            role.Description = "Updated Description";
            role.LastUpdateByUserAccountId = role.CreatedByUserAccountId;
            role.Privileges = new List<Privilege>();
            
            
            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt64(dataReader["privilegeID"]);
                    role.Privileges.Add(new Privilege {
                        Id = privilegeID
                    });
                   
                }
            }




            role = securityLogic.UpdateRole(role);

            Assert.IsNotNull(role, "Update Failed");

            command = Connection.GetSqlStringCommand("select * from role where RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);

            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(role.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                Assert.AreEqual(role.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(role.LastUpdateByUserAccountId, (long)dataReader["LastUpdateByUserAccountId"], "Create Failed");
                Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
                Assert.AreEqual(role.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
                Assert.IsNotNull(role.CreatedOn, "Create Failed");
            }
            else {
                Assert.IsNotNull(role, "Update Failed");
            }
            dataReader.Close();





        }

        [TestMethod]
        public void DeleteRoleTest() {
            long roleId = 0;
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

            securityLogic.DeleteRole(roleId, 1);

            command = Connection.GetSqlStringCommand("select count(1) as count from role where RoleID=" + roleId);

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

        [TestMethod]
        public void CreatePrivilegeTest() {
            Privilege privilege = new Privilege();
            DbCommand command = null;
            IDataReader dataReader;
            long CreatedByUserAccountId = 0;

            command = null;

            command = Connection.GetSqlStringCommand("select UserAccountID from UserAccount order by UserAccountID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    CreatedByUserAccountId = Convert.ToInt64(dataReader["UserAccountID"]);

                }
            }
            dataReader.Close();

            privilege.Name = "privilegeTest";
            //privilege.Permission = Permission.TestPermission;
            privilege.CreatedByUserAccountId = CreatedByUserAccountId;
            privilege.Description = "test";
          

            Privilege resultPrivilege = new Privilege();

            resultPrivilege = securityLogic.CreatePrivilege(privilege);

       

            command = Connection.GetSqlStringCommand("select  * from Privilege where PrivilegeId=LAST_INSERT_ID()");

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
            Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            //Assert.AreEqual(privilege.Permission, ((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)), "Create Failed");
            Assert.IsNotNull(privilege.CreatedOn, "Create Failed");
            Assert.IsNotNull(privilege.Id, "Create Failed");
            dataReader.Close();




            command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=LAST_INSERT_ID()");

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        [TestMethod]
        public void GetPrivilegeTest() {
            DbCommand command = null;
            IDataReader dataReader;
            long privilegeID = 0;

            Privilege privilege = new Privilege();


            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt64(dataReader["privilegeID"]);

                }
            }
            dataReader.Close();

            privilege = securityLogic.GetPrivilege(privilegeID);

            command = Connection.GetSqlStringCommand("select  * from Privilege where PrivilegeId=" + privilegeID);

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
            Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            //Assert.AreEqual(privilege.Permission, ((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)), "Create Failed");
            Assert.IsNotNull(privilege.CreatedOn, "GetPrivilege Failed");
            Assert.IsNotNull(privilege.Id, "GetPrivilege Failed");
            dataReader.Close();

        }

        [TestMethod]
        public void GetAllPrivileges() {

        }

        [TestMethod]
        public void UpdatePrivilegeTest() {
            Privilege privilege = new Privilege();
            Privilege resultPrivilege = new Privilege();
            DbCommand command = null;
            IDataReader dataReader;
            long privilegeID = 0;

            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt64(dataReader["privilegeID"]);

                }
            }

            dataReader.Close();

            privilege = securityLogic.GetPrivilege(privilegeID);

            privilege.Name = "privilegeTestUpdate123";
            //privilege.Permission = Permission.TestPermission ;
            privilege.Description = "testUpdate123";
            privilege.LastUpdateByUserAccountId =privilege.CreatedByUserAccountId;

            resultPrivilege = securityLogic.UpdatePrivilege(privilege);

            command = Connection.GetSqlStringCommand("select  * from Privilege where PrivilegeId=" + privilegeID);

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
            Assert.AreEqual(privilege.LastUpdateByUserAccountId, (long)dataReader["LastUpdateByUserAccountId"], "Create Failed");
            Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            //Assert.AreEqual(privilege.Permission, ((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)), "Create Failed");
            Assert.IsNotNull(privilege.CreatedOn, "UpdatePrivilegeTest Failed");
            Assert.IsNotNull(privilege.LastUpdate, "UpdatePrivilegeTest Failed");
            Assert.IsNotNull(privilege.Id, "UpdatePrivilegeTest Failed");
            dataReader.Close();


        }

        [TestMethod]
        public void DeletePrivilegeTest() {
            DbCommand command = null;
            IDataReader dataReader;
            long privilegeID = 0;

            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt64(dataReader["privilegeID"]);

                }
            }

            dataReader.Close();

            securityLogic.DeletePrivilege(privilegeID, 1);


            command = Connection.GetSqlStringCommand("select count(1) as count from Privilege where PrivilegeId=" + privilegeID);

            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "DeletePrivilegeTest Failed");
            dataReader.Close();


        }


        [TestMethod]
        public void CreateRolePrivilege() {
            long roleID = 1;
            long privilegeID = 3;
            long Createdby = 1;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);

                }
            }
            dataReader.Close();
            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt32(dataReader["privilegeID"]);

                }
            }
            command = null;

            command = Connection.GetSqlStringCommand("select userAccountID from userAccount order by userAccountID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    Createdby = Convert.ToInt32(dataReader["userAccountID"]);

                }
            }
            dataReader.Close();
            securityLogic.DeleteRolePrivilege(roleID, privilegeID, Createdby);

            securityLogic.CreateRolePrivilege(roleID, privilegeID, Createdby);


            //Assert.IsNotNull(role, "Create Failed");
            //Assert.AreEqual(role.Id, 2, "Create Failed");

            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID= " + privilegeID);
            dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();



        }


        [TestMethod]
        public void GetRolePrivileges() {
            long roleID = 1;

            List<Privilege> privileges = new List<Privilege>();

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);

                }
            }

            dataReader.Close();

            privileges = securityLogic.GetRolePrivileges(roleID, dataParameters);

            //Assert.IsNotNull(roleList, "Read Failed");

            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID);

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(privileges.Count, Convert.ToInt32(dataReader["count"]), "GetRolePrivileges Failed");
            }
            else {
                Assert.AreEqual(privileges.Count, 0, "GetRolePrivileges Failed");
            }

            dataReader.Close();
            foreach(Privilege privilege in privileges) {
                command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeId=" + privilege.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "GetRolePrivileges Failed");

                dataReader.Close();
            }

            foreach(Privilege privilege in privileges) {
                command = Connection.GetSqlStringCommand("select * from privilege where  privilegeId=" + privilege.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.Id, Convert.ToInt64(dataReader["privilegeId"]), "GetRolePrivileges Failed");
                //Assert.AreEqual(privilege.Permission, ((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)), "GetRolePrivileges Failed");
                Assert.IsNotNull(privilege.CreatedOn, "GetPrivilege Failed");
                        
                dataReader.Close();
            }



        }


        [TestMethod]
        public void GetRolePrivilegesWithPagination() {
            DbCommand command = null;
            long roleID = 1;
            List<Privilege> privileges = new List<Privilege>();

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);

                }
            }

            dataReader.Close();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            privileges = securityLogic.GetRolePrivileges(roleID, dataParameters);

            Assert.IsNotNull(privileges, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleId=" + roleID);
            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(privileges.Count, 0, "Read Failed");
            }

            dataReader.Close();


            if(privileges.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(privileges.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(privileges.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }


            foreach(Privilege privilege in privileges) {
                command = Connection.GetSqlStringCommand("select * from privilege where  privilegeId=" + privilege.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(privilege.Name, Convert.ToString(dataReader["Name"]), "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.Description, Convert.ToString(dataReader["Description"]), "GetRolePrivileges Failed");
                Assert.AreEqual(privilege.Id, Convert.ToInt64(dataReader["privilegeId"]), "GetRolePrivileges Failed");
                //Assert.AreEqual(privilege.Permission, ((Permission)Enum.Parse(typeof(Permission), Convert.ToString(dataReader["Permission"]), true)), "GetRolePrivileges Failed");
                Assert.IsNotNull(privilege.CreatedOn, "GetPrivilege Failed");

                dataReader.Close();
            }

        }

        [TestMethod]
        public void DeleteRolePrivilege() {
            DbCommand command = null;
            long roleID = 1;
            long privilegeID = 1;

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);

                }
            }

            dataReader.Close();
            command = null;

            command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    privilegeID = Convert.ToInt32(dataReader["privilegeID"]);

                }
            }


            securityLogic.DeleteRolePrivilege(roleID, privilegeID, 0);
            dataReader.Close();
            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID=" + privilegeID);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        //[TestMethod]
        //public void UpdateRolePrivileges() {
        //    DbCommand command = null;
        //    long roleID = 1;
        //    long privilegeID = 1;

        //    command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

        //    IDataReader dataReader = Connection.ExecuteReader(command);


        //    if(dataReader != null) {
        //        if(dataReader.Read()) {
        //            roleID = Convert.ToInt32(dataReader["roleID"]);

        //        }
        //    }

        //    dataReader.Close();
        //    command = null;

        //    command = Connection.GetSqlStringCommand("select privilegeID from privilege order by privilegeID desc limit 0,1;");

        //    dataReader = Connection.ExecuteReader(command);


        //    if(dataReader != null) {
        //        if(dataReader.Read()) {
        //            privilegeID = Convert.ToInt32(dataReader["privilegeID"]);

        //        }
        //    }


        //    securityLogic.DeleteRolePrivilege(roleID, privilegeID, 0);
        //    dataReader.Close();
        //    command = null;

        //    command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID=" + privilegeID);
        //    dataReader = Connection.ExecuteReader(command);
        //    dataReader.Read();
        //    Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
        //    dataReader.Close();

        //}
    }
}
