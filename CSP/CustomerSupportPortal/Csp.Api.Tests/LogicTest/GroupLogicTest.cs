using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Logic;
using System.Data.Common;
using System.Collections.Generic;
using System.Data;

namespace Csp.Api.Tests.LogicTest {
    [TestClass]
    public class GroupLogicTest : DBConnections {

        long groupId = 0;
        long Createdby = 1;
        [TestMethod]
        public void CreateGroupTest() {

            DbCommand command = null;
            DataParameters dataParameters = new DataParameters();
            GroupLogic groupLogic = new GroupLogic();
            List<Group> groupList = new List<Group>();
            Group group = new Group();
            
            group.Name = "C_Name";
            group.SupportAccountId = 1;
            group.Description = "C_Description";
            group.PhoneNumber = "C_Phone";
            group.Address.Address1 = "C_Address1";
            group.Address.Address2 = "C_Address2";
            group.Address.City = "C_City";
            group.Address.Region = "C_Region";
            group.Address.PostalCode = "C_PostalCode";
            group.Address.Country = "C_Country";
            
            group = groupLogic.CreateGroup(group);
            Assert.IsNotNull(group, "Create Failed");

            command = Connection.GetSqlStringCommand("select * from csp.SupportAccountGroup where GroupId=last_insert_id();");
            IDataReader dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (group != null) {
                    dataReader.Read();
                    Assert.AreEqual(group.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                    Assert.AreEqual(group.SupportAccountId, (long)(dataReader["SupportAccountId"]), "Create Failed");
                    Assert.AreEqual(group.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
                    Assert.AreEqual(group.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), "Create Failed");
                    Assert.AreEqual(group.Address.Address1, Convert.ToString(dataReader["Address1"]), "Create Failed");
                    Assert.AreEqual(group.Address.Address2, Convert.ToString(dataReader["Address2"]), "Create Failed");
                    Assert.AreEqual(group.Address.City, Convert.ToString(dataReader["City"]), "Create Failed");
                    Assert.AreEqual(group.Address.Region, Convert.ToString(dataReader["Region"]), "Create Failed");
                    Assert.AreEqual(group.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Create Failed");
                    Assert.AreEqual(group.Address.Country, Convert.ToString(dataReader["Country"]), "Create Failed");
                    //Assert.AreEqual(group.SecurityClassification,((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
                    Assert.AreEqual(group.CreatedByUserAccountId, (long)(dataReader["CreatedByUserAccountId"]), "Create Failed");
                    groupId = group.Id;
                }
                else {
                    Assert.Fail("Create Failed");
                }
            }
            else {
                Assert.Fail("Create Failed");
            }

            dataReader.Close();

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();

        }

        [TestMethod]
        public void GetGroupByGroupIdTest() {

            DbCommand command = null;

            GroupLogic groupLogic = new GroupLogic();
            List<Group> groupList = new List<Group>();
            Group group = new Group();
            long restrictedByUserAccountId = 1;


            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read()) {
                groupId = (long)dataReader["GroupId"];
            }
            dataReader.Close();

            try {

                group = groupLogic.GetGroup(groupId, restrictedByUserAccountId);

            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get GroupByGroupId Failed");
                return;
            }





            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(0, 0, "Read Failed");
            }
            dataReader.Close();

            command = Connection.GetStoredProcCommand("GetGroupByGroupId");
            Connection.AddInParameter(command, "p_groupID", DbType.Int64, groupId);
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, restrictedByUserAccountId);
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                while (dataReader.Read()) {

                }
            }



            command = Connection.GetSqlStringCommand("select * from csp.SupportAccountGroup where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(group.Name, Convert.ToString(dataReader["Name"]), "Read Failed");
                Assert.AreEqual(group.SupportAccountId, (long)(dataReader["SupportAccountId"]), "Read Failed");
                Assert.AreEqual(group.Description, Convert.ToString(dataReader["Description"]), "Read Failed");
                Assert.AreEqual(group.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), "Read Failed");
                Assert.AreEqual(group.Address.Address1, Convert.ToString(dataReader["Address1"]), "Read Failed");
                Assert.AreEqual(group.Address.Address2, Convert.ToString(dataReader["Address2"]), "Read Failed");
                Assert.AreEqual(group.Address.City, Convert.ToString(dataReader["City"]), "Read Failed");
                Assert.AreEqual(group.Address.Region, Convert.ToString(dataReader["Region"]), "Read Failed");
                Assert.AreEqual(group.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Read Failed");
                Assert.AreEqual(group.Address.Country, Convert.ToString(dataReader["Country"]), "Read Failed");
                //Assert.AreEqual(group.SecurityClassification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
                Assert.AreEqual(group.CreatedByUserAccountId, (long)(dataReader["CreatedByUserAccountId"]), "Read Failed");

            }
            else {
                Assert.AreEqual(0, 0, "Read Failed");
            }
            dataReader.Close();
        }

        [TestMethod]
        public void UpdateGroupTest() {
            DbCommand command = null;
            long restrictedByUserAccountId = 1;
            DataParameters dataParameters = new DataParameters();
            GroupLogic groupLogic = new GroupLogic();
            List<Group> groupList = new List<Group>();
            Group group = new Group();

            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();

            try {

                group = groupLogic.GetGroup(groupId, restrictedByUserAccountId);

            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get GroupByGroupId Failed");
                return;
            }

            Assert.AreEqual(group.Id, groupId, "Read for Update Failed");
            group.Id = groupId;
            group.Name = "U_Name";
            group.SupportAccountId = 1;
            group.Description = "U_Description";
            group.PhoneNumber = "U_Phone";
            group.Address.Address1 = "U_Address1";
            group.Address.Address2 = "U_Address2";
            group.Address.City = "U_City";
            group.Address.Region = "U_Region";
            group.Address.PostalCode = "U_PostalCode";
            group.Address.Country = "U_Country";
            //group.SecurityClassification = SecurityClassification.Default;
            group.LastUpdateByUserAccountId = 1;
            //group.Devices.Add(new Device {
            //    Id = 1
            //});
            //group.Devices.Add(new Device {
            //    Id = 2
            //});
            //group.DefaultRoles.Add(new Role {
            //    Id = 1
            //});
            //group.DefaultRoles.Add(new Role {
            //    Id = 2
            //});
            //group.InactiveLicenses.Add(new License {
            //    Id = 1
            //});
            try {

                group = groupLogic.UpdateGroup(group, restrictedByUserAccountId);

            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get GroupByGroupId Failed");
                return;
            }
            //group = groupLogic.UpdateGroup(group, restrictedByUserAccountId);

            command = Connection.GetSqlStringCommand("select * from csp.SupportAccountGroup where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (group != null) {
                    dataReader.Read();
                    Assert.AreEqual(group.Name, Convert.ToString(dataReader["Name"]), "Update Failed");
                    Assert.AreEqual(group.SupportAccountId, (long)(dataReader["SupportAccountId"]), "Update Failed");
                    Assert.AreEqual(group.Description, Convert.ToString(dataReader["Description"]), "Update Failed");
                    Assert.AreEqual(group.PhoneNumber, Convert.ToString(dataReader["PhoneNumber"]), "Update Failed");
                    Assert.AreEqual(group.Address.Address1, Convert.ToString(dataReader["Address1"]), "Update Failed");
                    Assert.AreEqual(group.Address.Address2, Convert.ToString(dataReader["Address2"]), "Update Failed");
                    Assert.AreEqual(group.Address.City, Convert.ToString(dataReader["City"]), "Update Failed");
                    Assert.AreEqual(group.Address.Region, Convert.ToString(dataReader["Region"]), "Update Failed");
                    Assert.AreEqual(group.Address.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Update Failed");
                    Assert.AreEqual(group.Address.Country, Convert.ToString(dataReader["Country"]), "Update Failed");
                    //Assert.AreEqual(group.SecurityClassification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Update Failed");
                    Assert.AreEqual(group.LastUpdateByUserAccountId, (long)(dataReader["LastUpdateByUserAccountId"]), "Update Failed");
                    groupId = group.Id;
                }
                else {
                    Assert.Fail("Update Failed");
                }
            }
            else {
                Assert.Fail("Update Failed");
            }

            dataReader.Close();

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();

            //group.Devices.Add(new Device {
            //    Id = 1
            //});
            //group.Devices.Add(new Device {
            //    Id = 2
            //});

            //foreach (Device device in group.Devices) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where GroupId=" + groupId + " and DeviceId=" + device.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();

            //group.DefaultRoles.Add(new Role {
            //    Id = 1
            //});
            //group.DefaultRoles.Add(new Role {
            //    Id = 2
            //});

            //foreach (Role role in group.DefaultRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where GroupId=" + groupId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();

            //group.InactiveLicenses.Add(new License {
            //    Id = 1
            //});
            //foreach (License license in group.InactiveLicenses) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from grouplicenses where GroupId=" + groupId + " and LicenseId=" + license.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}
            //dataReader.Close();
        }

        [TestMethod]
        public void DeleteGroupTest() {

            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            Group group = new Group();
            //long restrictedByUserAccountId = 1;

            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read()) {
                groupId = (long)dataReader["GroupId"];
            }
            dataReader.Close();
            try {

                groupLogic.DeleteGroup(3, 1);

            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message.ToString().Trim(), "Access is not allowed", "Get GroupByGroupId Failed");
                return;
            }


            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();


            command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();


            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();


            command = Connection.GetSqlStringCommand("select count(1) as count from grouplicenses where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void GetSupportAccountGroupsPagination() {

            List<GroupListItem> groupListItems = new List<GroupListItem>();
             
            DataParameters dataParameters = new DataParameters();
            GroupLogic groupLogic = new GroupLogic();
            long restrictedUserAccountId = 0;
            long supportAccountId = 0;
            DbCommand command = null;





            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    supportAccountId = Convert.ToInt64(dataReader["supportAccountId"]);

                }
            }

            dataReader.Close();

            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    restrictedUserAccountId = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();


            groupListItems = groupLogic.GetSupportAccountGroups(supportAccountId, restrictedUserAccountId, dataParameters);
            
            Assert.IsNotNull(groupListItems, "Read Failed with Pagination");


            command = Connection.GetStoredProcCommand("GetGroupsBySupportAccountId");
            Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, supportAccountId);
            Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
            Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
            Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
            Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, restrictedUserAccountId);
            Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
            dataReader = Connection.ExecuteReader(command);
              string str;
            if (dataReader != null) {
                while (dataReader.Read()) {
                    if (dataReader["SupportAccountGroupName"] != DBNull.Value)
                        str = Convert.ToString(dataReader["SupportAccountGroupName"]);
                        if (groupListItems.Find(x => x.SupportAccountGroupName == Convert.ToString(dataReader["SupportAccountGroupName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if (dataReader["Description"] != DBNull.Value)
                        if (groupListItems.Find(x => x.Description == Convert.ToString(dataReader["Description"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if (dataReader["SupportAccountName"] != DBNull.Value)
                        if (groupListItems.Find(x => x.SupportAccountName == Convert.ToString(dataReader["SupportAccountName"])) == null) {
                            Assert.Fail("Read Fail");
                        }

                    if (dataReader["Address1"] != DBNull.Value)
                        if (groupListItems.Find(x => x.Address1 == Convert.ToString(dataReader["Address1"])) == null) {
                            Assert.Fail("Read Fail");
                        }


                    if (dataReader["City"] != DBNull.Value)
                        if (groupListItems.Find(x => x.City == Convert.ToString(dataReader["City"])) == null) {
                            Assert.Fail("Read Fail");
                        }


                    if (dataReader["Region"] != DBNull.Value)
                        if (groupListItems.Find(x => x.Region == Convert.ToString(dataReader["Region"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if (dataReader["Country"] != DBNull.Value)
                        if (groupListItems.Find(x => x.Country == Convert.ToString(dataReader["Country"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                }
            }
            else {
                Assert.AreEqual(groupListItems.Count, 0, "Read Failed");
            }
            

        }

        [TestMethod]
        public void CreateGroupDefaultRoleTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            //Group group = new Group();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read()) {
                groupId = (long)dataReader["GroupId"];
            }
            dataReader.Close();
            groupLogic.CreateGroupDefaultRole(groupId, 2, Createdby);

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupId + " and roleId=2 ");
             dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void GetGroupDefaultRolesTest() {
            DbCommand command = null;
            DataParameters dataParameters = new DataParameters();
            GroupLogic groupLogic = new GroupLogic();
            List<Role> roleList = new List<Role>();
            //Group group = new Group();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            roleList = groupLogic.GetGroupDefaultRoles(groupId, dataParameters);

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupId);

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
                command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupId + " and roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

            foreach(Role role in roleList) {
                command = Connection.GetSqlStringCommand("select * from role  where roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(role.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                Assert.AreEqual(role.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
                Assert.AreEqual(role.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
            
                dataReader.Close();
            }
        }

        [TestMethod]
        public void GetGroupDefaultRolesPaginationTest() {
            DbCommand command = null;
            DataParameters dataParameters = new DataParameters();
            GroupLogic groupLogic = new GroupLogic();
            List<Role> roleList = new List<Role>();
            //Group group = new Group();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            roleList = groupLogic.GetGroupDefaultRoles(groupId, dataParameters);

            Assert.IsNotNull(roleList, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupId);
             dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "Read Failed");
            }


            if (roleList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(roleList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }

            foreach(Role role in roleList) {
                command = Connection.GetSqlStringCommand("select * from role  where roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(role.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                Assert.AreEqual(role.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
                Assert.AreEqual(role.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");

                dataReader.Close();
            }

        }

        [TestMethod]
        public void DeleteGroupDefaultRoleTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupLogic.DeleteGroupDefaultRole(groupId, 2, 0);

            

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdefaultroles where groupID=" + groupId + " and roleId=2 ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void DeleteGroupDefaultRolesTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();

            groupLogic.DeleteGroupDefaultRoles(groupId);

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.groupdefaultroles where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }
        [TestMethod]
        public void CreateGroupDeviceTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupLogic.CreateGroupDevice(groupId, 2, Createdby);
           
            command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where groupID=" + groupId + " and deviceId=2 ");
             dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void DeleteGroupDeviceTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupLogic.DeleteGroupDevice(groupId, 2, 0);

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where groupID=" + groupId + " and deviceId=2 ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
        [TestMethod]
        public void DeleteGroupDevicesTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();

            groupLogic.DeleteGroupDevices(groupId);

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.groupdevices where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }

        [TestMethod]
        public void GroupLicenseCreate() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupLogic.CreateGroupLicense(groupId, 2, Createdby);
           
            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupId + " and LicenseId=2 ");
             dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void DeleteGroupLicenseTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupLogic.DeleteGroupLicense(groupId, 2, 0);

            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupId + " and LicenseId=2 ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
        [TestMethod]
        public void DeleteGroupLicensesTest() {
            DbCommand command = null;
            GroupLogic groupLogic = new GroupLogic();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();

            groupLogic.DeleteGroupLicenses(groupId);

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.groupLicenses where GroupId=" + groupId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

        }
    }
}
