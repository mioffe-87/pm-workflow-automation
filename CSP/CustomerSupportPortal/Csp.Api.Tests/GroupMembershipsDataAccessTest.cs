using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {
    [TestClass]
    public class GroupMembershipsDataAccessTest : DBConnections {
        GroupMembershipsDataAccess groupMembershipsDataAccess = null;
        DataParameters dataParameters = null;
        Membership membership = null;
        List<Membership> memberList = null;

        [TestMethod]
        public void DefaultMembershipReadTest() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            groupMembershipsDataAccess = new GroupMembershipsDataAccess();
            memberList = new List<Membership>();
            long groupId = 1;

            memberList = groupMembershipsDataAccess.Read(groupId, ref dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from license where GroupId=" + groupId);
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(memberList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(memberList.Count, 0, "Read Failed");
            }
            dataReader.Close();
            foreach (Membership membership in memberList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from membership where GroupId=" + groupId + " and MembershipId=" + membership.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
                dataReader.Close();
            }
        }

        [TestMethod]
        public void ReadMembershipWithPaginationTest() {
            dataParameters = new DataParameters();
            groupMembershipsDataAccess = new GroupMembershipsDataAccess();
            memberList = new List<Membership>();
            long groupId = 1;
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            memberList = groupMembershipsDataAccess.Read(groupId, ref dataParameters);
            Assert.IsNotNull(memberList, "Read Failed");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");

            if (memberList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(memberList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(memberList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            }
        }

        [TestMethod]
        public void DeleteGroupMembershipTest() {
            groupMembershipsDataAccess = new GroupMembershipsDataAccess();
            long groupId = 1;
            groupMembershipsDataAccess.Delete(groupId);
            DbCommand command = null;
            command = Connection.GetSqlStringCommand("select count(1) as count from membership where GroupId=" + groupId);
            IDataReader dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
    }
}
