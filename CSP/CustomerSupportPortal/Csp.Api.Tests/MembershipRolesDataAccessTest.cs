using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;


namespace Csp.Api.Tests {

    [TestClass]
    public  class MembershipRolesDataAccessTest:DBConnections {
        List<Role> roleList = new List<Role>();
        MembershipRolesDataAccess membershipRolesDataAccess = new MembershipRolesDataAccess();
        DataParameters dataParameters = new DataParameters();
        long MembershipId = 1;
        long roleId = 1;

        long createdByUserAccountId = 1;
        Role role = new Role();

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Create
        /// </summary>
        [TestMethod]
        public void MembershipRolesDataAccessCreate() {
            DbCommand command = null;
            IDataReader dataReader;

            membershipRolesDataAccess.Create(MembershipId, roleId, createdByUserAccountId);
            Assert.IsNotNull(role, "Create Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Delete
        /// </summary>
        [TestMethod]
        public void MembershipRolesDataAccessDelete() {
            DbCommand command = null;
            IDataReader dataReader;

            if ( roleId > 0) {
                membershipRolesDataAccess.Delete(MembershipId, roleId,0);
            }
            else {
                membershipRolesDataAccess.Delete(MembershipId);
            }

            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void MembershipRolesDataAccessRead() {
            DbCommand command = null;
            IDataReader dataReader;

            roleList = membershipRolesDataAccess.Read(MembershipId, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId);
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
                command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

        }

        /// <summary>
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Pagination
        /// </summary>
        [TestMethod]
        public void MembershipRolesDataAccessPagination() {
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            roleList = membershipRolesDataAccess.Read(MembershipId, dataParameters);

            Assert.IsNotNull(roleList, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId);
            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Failed");
            }

            dataReader.Close();

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
    }
}
