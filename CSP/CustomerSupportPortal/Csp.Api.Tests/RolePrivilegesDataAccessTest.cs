using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests {
    /// <summary>
    /// Summary description for RolePrivilegesDataAccessTest
    /// </summary>
    [TestClass]
    public class RolePrivilegesDataAccessTest :DBConnections  {

        RolePrivilegesDataAccess rolePrivilegesDataAccess = new RolePrivilegesDataAccess();
        DataParameters dataParameters = new DataParameters();
        List<Privilege> privileges = new List<Privilege>();
        Role role = new Role();

        long roleID = 1;
        long privilegeID = 3;
        long Createdby = 1;

        [TestMethod]
        public void RolePrivilegesCreate()
        {
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
            dataReader.Close();
            rolePrivilegesDataAccess.Delete(roleID, privilegeID, 0);

            rolePrivilegesDataAccess.Create(roleID,privilegeID, Createdby);


            //Assert.IsNotNull(role, "Create Failed");
            //Assert.AreEqual(role.Id, 2, "Create Failed");

             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID= "+ privilegeID);
             dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
           

        }

        [TestMethod]
        public void RolePrivilegesRead()
        {

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    roleID = Convert.ToInt32(dataReader["roleID"]);

                }
            }

            dataReader.Close();

            privileges = rolePrivilegesDataAccess.Read(roleID, dataParameters);

            //Assert.IsNotNull(roleList, "Read Failed");

             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID);

             dataReader = Connection.ExecuteReader(command);


            if (dataReader != null)
            {
                dataReader.Read();
                Assert.AreEqual(privileges.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else
            {
                Assert.AreEqual(privileges.Count, 0, "Read Failed");
            }

            dataReader.Close();
            foreach (Privilege privilege in privileges)
            {
                command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeId=" + privilege.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

           

        }

        [TestMethod]
        public void RolePrivilegesReadPagination()
        {
            DbCommand command = null;

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
            privileges = rolePrivilegesDataAccess.Read(roleID, dataParameters);

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


            if (privileges.Count > dataParameters.PaginationCount)
            {
                Assert.Fail("Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1))
            {
                Assert.AreEqual(privileges.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1))
            {
                Assert.AreEqual(privileges.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }



        }

        [TestMethod]
        public void RolePrivilegesDelete()
        {
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


            rolePrivilegesDataAccess.Delete(roleID, privilegeID,0);
            dataReader.Close();
             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from roleprivileges where roleID=" + roleID + " and privilegeID=" + privilegeID);
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
           


        }
    }
    
}
