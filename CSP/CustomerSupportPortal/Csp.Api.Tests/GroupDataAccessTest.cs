using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {
    [TestClass]
    public class GroupDataAccessTest : DBConnections {
        GroupDataAccess groupDataAccess =new GroupDataAccess();
        DataParameters dataParameters = new DataParameters();
        Group group = new Group();
        List<Group> groupList = new List<Group>();
        List<Membership> memberList = new List<Membership>();
        long groupId = 0;

        [TestMethod]
        public void CreateGroupTest() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            groupDataAccess = new GroupDataAccess();
            groupList = new List<Group>();
            group = new Group();
            group.Name = "Test Name";
            group.Description = "TestDescription";
            group.PhoneNumber = "Test Phone";
            group.Address.Address1 = "Address1";
            group.Address.Address2 = "Address2";
            group.SupportAccountId = 1;
            group.CreatedByUserAccountId = 1;

            group = groupDataAccess.Create(group);
            Assert.IsNotNull(group, "Create Failed");
            command = Connection.GetSqlStringCommand("select * from csp.SupportAccountGroup where GroupId=last_insert_id();");

            IDataReader dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                if (group != null) {
                    dataReader.Read();
                    Assert.AreEqual(group.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
                    Assert.AreEqual(group.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
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
        public void ReadGroupByGroupIdTest() {
            DbCommand command = null;
            groupDataAccess = new GroupDataAccess();
            groupList = new List<Group>();
            group = new Group();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            group = groupDataAccess.Read(groupId,0);
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
        }

        

        [TestMethod]
        public void UpdateGroupTest() {
            DbCommand command = null;
            //long groupId = 6;
            dataParameters = new DataParameters();
            groupDataAccess = new GroupDataAccess();
            groupList = new List<Group>();
            group = new Group();

            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup where GroupId=last_insert_id();");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();

            group = groupDataAccess.Read(groupId,1);
            Assert.AreEqual(group.Id, groupId, "Read for Update Failed");
            group.Id = groupId;
            group.Name = "Updated TestName";
            group.Description = "Updated TestDescription";
            group.PhoneNumber = "TestPhone";
            group.LastUpdateByUserAccountId = 1;
            group = groupDataAccess.Update(group,1);
            Assert.IsNotNull(group, "Update Failed");
            Assert.AreEqual(group.Id, groupId, "Read for Update Failed");
            Assert.AreEqual(group.Name, "Updated TestName", " Name Update Failed");
            Assert.AreEqual(group.Description, "Updated TestDescription", " Description Update Failed");
            Assert.AreEqual(group.PhoneNumber, "TestPhone", " PhoneNumber Update Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId + " ");
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();
 
        }

        [TestMethod]
        public void DeleteAccountTypeTest() {
            DbCommand command = null;
            //long groupId = 5;
            groupDataAccess = new GroupDataAccess();
            group = new Group();
            command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup where GroupId=last_insert_id();");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                groupId = (long)dataReader["GroupId"];
            dataReader.Close();
            groupDataAccess.Delete(groupId, 1);

            command = Connection.GetSqlStringCommand("select count(1) as count from csp.SupportAccountGroup where GroupId=" + groupId);
             dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();

            
            dataReader.Close();
        }


        [TestMethod]
        public void ReadSupportAccountGroupsPagination() {

            List<GroupListItem> groupListItems = new List<GroupListItem>();
            DataParameters dataParameters = new DataParameters();
            GroupDataAccess groupDataAccess = new GroupDataAccess();
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


            groupListItems = groupDataAccess.ReadSupportAccountGroups(supportAccountId, restrictedUserAccountId, dataParameters);

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
    }
}
