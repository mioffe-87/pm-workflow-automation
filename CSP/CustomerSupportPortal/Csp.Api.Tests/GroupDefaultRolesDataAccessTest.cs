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
    public class GroupDefaultRolesDataAccessTest : DBConnections {
        GroupDefaultRolesDataAccess groupDefaultRolesDataAccess = new GroupDefaultRolesDataAccess();
        DataParameters dataParameters = new DataParameters();
        List<Role> roleList = new List<Role>();
        Role role = new Role();
        long groupID = 1;
        long Createdby = 1;

        [TestMethod]
        public void GroupDefaultRolesCreate() {

            groupDefaultRolesDataAccess.Create(groupID, 2, Createdby);


            //Assert.IsNotNull(role, "Create Failed");
            //Assert.AreEqual(role.Id, 2, "Create Failed");

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupID + " and roleId=2 ");
            IDataReader dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();


        }

        [TestMethod]
        public void GroupDefaultRolesRead() {


            groupID = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();

            roleList = groupDefaultRolesDataAccess.Read(groupID, dataParameters);

            //Assert.IsNotNull(roleList, "Read Failed");

             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupID);

             dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(roleList.Count, Convert.ToInt32(dataReader["count"]), "Read Faileds");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Failed2");
            }

            dataReader.Close();
            foreach(Role role in roleList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupID + " and roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }



        }

        [TestMethod]
        public void GroupDefaultRolesReadPagination() {

            groupID = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();

            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            roleList = groupDefaultRolesDataAccess.Read(groupID, dataParameters);

            Assert.IsNotNull(roleList, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            
             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupID);
             dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed1");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Faileds");
            }


            if(roleList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }



        }

        
        [TestMethod]
        public void GroupDefaultRolesDelete() {


            groupID = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();
            groupDefaultRolesDataAccess.Delete(groupID, 2, 0);

             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupID + " and roleId=2 ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();



        }

    }
}
