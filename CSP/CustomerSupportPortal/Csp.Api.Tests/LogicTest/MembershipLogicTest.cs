using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using Csp.Api.Logic;
namespace Csp.Api.Tests.LogicTest {

    [TestClass]
    public class MembershipLogicTest : DBConnections {

        List<Role> roleList = new List<Role>();
        MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
        MembershipLogic membershipLogic = new MembershipLogic();
        DataParameters dataParameters = new DataParameters();

        long createdByUserAccountId = 27;

        [TestMethod]
        public void CreateMembershipLogicTest() {
            long CreatedByUserAccountId = 0;
            DbCommand command = null;
            IDataReader dataReader;
            Role role = new Role();
            Membership membership = new Membership();
            long groupId = 0;
           
            command = null;
            command = Connection.GetSqlStringCommand("select UserAccountID from UserAccount order by UserAccountID desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                if(dataReader.Read()) {
                    CreatedByUserAccountId = Convert.ToInt64(dataReader["UserAccountID"]);
                }
            }
            dataReader.Close();
            
            command = Connection.GetSqlStringCommand("SELECT  GroupId  FROM csp.supportaccountgroup order by GroupId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                if (dataReader.Read()) {
                    groupId = Convert.ToInt32(dataReader["groupId"]);
                }
            }
            dataReader.Close();

            command = null;
            long deviceId = 0;
            command = Connection.GetSqlStringCommand("SELECT DeviceId  FROM device order by DeviceId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                if (dataReader.Read()) {
                    deviceId = Convert.ToInt32(dataReader["DeviceId"]);
                }
            }
            dataReader.Close();

            command = null;
            long supportAccountId = 0;
            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                if(dataReader.Read()) {
                    supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
                }
            }
            dataReader.Close();

            membership.GroupId = groupId;
            membership.ExpirationDate = DateTime.Today;
            membership.DeviceId = deviceId;
            membership.UserAccount.Id = createdByUserAccountId;
            membership.CreatedByUserAccountId =createdByUserAccountId;
            membership.Description = "Test";
            membership.DeviceId = deviceId;
            membership.ActivationDate = DateTime.Today;
            membership.SupportAccountId = supportAccountId;
            membership.Roles = new List<Role>();
            membership.ExpirationDate = DateTime.Today.AddDays(60).Date;
            membership.IsJiveSupportCenterMember = true;
            membership.IsJiveTrainingCenterMember = true;
            membership.HasJiveDirectCustomerSupport = true;
            membership.JiveId = "test62354";
            membership.SalesforceContactId = "test645";
            membership.SalesforceLogInId = "sgduy3245";

            int roleID1 = 0;
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,2;");
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                while (dataReader.Read()) {
                    roleID1 = Convert.ToInt32(dataReader["roleID"]);
                    membership.Roles.Add(new Role {
                        Id = roleID1
                    });
                }
            }
            dataReader.Close();
            

            Membership localMembership = new Membership();

            localMembership = membershipLogic.CreateMembership(membership);
            Assert.IsNotNull(localMembership, "Create Failed");


            command = Connection.GetSqlStringCommand("Select * from membership where MembershipId=LAST_INSERT_ID() ");
            dataReader = Connection.ExecuteReader(command);

            if (dataReader.Read()) {
                Assert.AreEqual(localMembership.DeviceId, (long)dataReader["DeviceId"], "Create Failed");
                Assert.AreEqual(localMembership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(localMembership.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
                Assert.AreEqual(localMembership.DeviceId, (long)dataReader["DeviceId"], "Create Failed");
                Assert.AreEqual(localMembership.UserAccount.Id, (long)dataReader["UserAccountId"], "Create Failed");
                Assert.AreEqual(localMembership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(localMembership.SupportAccountId, (long)dataReader["SupportAccountId"], "Create Failed");
                Assert.AreEqual(localMembership.GroupId, (long)dataReader["GroupId"], "Create Failed");
                Assert.AreEqual(localMembership.ExpirationDate, (DateTime)dataReader["ExpirationDate"], "Create Failed");
                Assert.AreEqual(localMembership.ActivationDate, (DateTime)dataReader["ActivationDate"], "Create Failed");

                Assert.AreEqual(localMembership.IsJiveSupportCenterMember,Convert.ToBoolean(dataReader["IsJiveSupportCenterMember"]), "Create Failed");
                Assert.AreEqual(localMembership.IsJiveTrainingCenterMember, Convert.ToBoolean(dataReader["IsJiveTrainingCenterMember"]), "Create Failed");
                Assert.AreEqual(localMembership.HasJiveDirectCustomerSupport, Convert.ToBoolean(dataReader["HasJiveDirectCustomerSupport"]), "Create Failed");
                Assert.AreEqual(localMembership.JiveId, Convert.ToString(dataReader["JiveId"]), "Create Failed");
                Assert.AreEqual(localMembership.SalesforceLogInId, Convert.ToString(dataReader["SalesforceLogInId"]), "Create Failed");
                Assert.AreEqual(localMembership.SalesforceContactId, Convert.ToString(dataReader["SalesforceContactId"]), "Create Failed");

                Assert.AreEqual(localMembership.CreatedOn, (DateTime)dataReader["CreatedOn"], "Create Failed");
                Assert.IsNotNull(localMembership.CreatedOn, "Create Failed");
                Assert.IsNotNull(localMembership.ExpirationDate, "Create Failed");
                Assert.IsNotNull(localMembership.ActivationDate, "Create Failed");
            }
            else {
                Assert.Fail();
            }
            dataReader.Close();

            //Removed Because of transaction logic Changes

            foreach(Role localRole in membership.Roles) {
                command = Connection.GetSqlStringCommand("Select Count(*) as count from membershiproles where RoleId=" + localRole.Id + " And membershipId=" + localMembership.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            }
            dataReader.Close();

            //foreach(Role localRole in membership.Roles) {
            //    command = Connection.GetSqlStringCommand("Select * from  role where RoleId=" + localRole.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(localRole.Name, Convert.ToString(dataReader["Name"]), "Create Failed");
            //    Assert.AreEqual(localRole.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
            //    Assert.AreEqual(localRole.Description, Convert.ToString(dataReader["Description"]), "Create Failed");
            //    Assert.AreEqual(localRole.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "Create Failed");
            //    dataReader.Close();
            //}
           
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GetByIDMembershipLogicTest() {

            long membershipId = 0;
            Role role = new Role();
            Membership membership = new Membership();
            DbCommand command = null;
            IDataReader dataReader;


            command = null;

            command = Connection.GetSqlStringCommand("select MembershipID from membership order by MembershipID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    membershipId = Convert.ToInt32(dataReader["MembershipID"]);

                }
            }

            dataReader.Close();

            membership = membershipLogic.GetMembership(membershipId);

            command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" + membershipId);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.IsNotNull(membership, "Read Failed");
            }

            dataReader.Close();
            command = Connection.GetSqlStringCommand("Select * from membership where MembershipID=" + membershipId);
            dataReader = Connection.ExecuteReader(command);

            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.Description, Convert.ToString(dataReader["Description"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.UserAccount.Id, (long)dataReader["UserAccountId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.SupportAccountId, (long)dataReader["SupportAccountId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.GroupId, (long)dataReader["GroupId"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.ExpirationDate, (DateTime)dataReader["ExpirationDate"], "Get Membership By ID Failed");
                Assert.AreEqual(membership.ActivationDate, (DateTime)dataReader["ActivationDate"], "Get Membership By ID Failed");

                Assert.AreEqual(membership.IsJiveSupportCenterMember, Convert.ToBoolean(dataReader["IsJiveSupportCenterMember"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.IsJiveTrainingCenterMember, Convert.ToBoolean(dataReader["IsJiveTrainingCenterMember"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.HasJiveDirectCustomerSupport, Convert.ToBoolean(dataReader["HasJiveDirectCustomerSupport"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.JiveId, Convert.ToString(dataReader["JiveId"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.SalesforceLogInId, Convert.ToString(dataReader["SalesforceLogInId"]), "Get Membership By ID Failed");
                Assert.AreEqual(membership.SalesforceContactId, Convert.ToString(dataReader["SalesforceContactId"]), "Get Membership By ID Failed");

                Assert.AreEqual(membership.CreatedOn, (DateTime)dataReader["CreatedOn"], "Get Membership By ID Failed");
                Assert.IsNotNull(membership.CreatedOn, "Get Membership By ID Failed");
                Assert.IsNotNull(membership.ExpirationDate, "Get Membership By ID Failed");
                Assert.IsNotNull(membership.ActivationDate, "Get Membership By ID Failed");
            }
            else {
                Assert.IsNotNull(membership, "Read Failed");
            }

            dataReader.Close();

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void UpdateMembershipLogicTest() {

            long membershipId = 0;
            DbCommand command = null;
            IDataReader dataReader;
            Role role = new Role();
            Membership membership = new Membership();

            command = null;
            command = Connection.GetSqlStringCommand("select MembershipID from membership order by MembershipID desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                if (dataReader.Read()) {
                    membershipId = Convert.ToInt32(dataReader["MembershipID"]);
                }
            }
            dataReader.Close();

            long userAccountId = 0;
            command = Connection.GetSqlStringCommand("select UserAccountId from useraccount order by UserAccountId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                if(dataReader.Read()) {
                    userAccountId = Convert.ToInt32(dataReader["UserAccountId"]);
                }
            }
            dataReader.Close();

            long deviceId = 0;
            command = Connection.GetSqlStringCommand("SELECT DeviceId  FROM device order by DeviceId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                if(dataReader.Read()) {
                    deviceId = Convert.ToInt32(dataReader["DeviceId"]);
                }
            }
            dataReader.Close();

            long supportAccountId = 0;
            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                if(dataReader.Read()) {
                    supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);
                }
            }
            dataReader.Close();
            
            Membership localMembership = new Membership();
           
            membership = membershipLogic.GetMembership(membershipId);
            membership.ExpirationDate = DateTime.Today.AddDays(30).Date;
            membership.UserAccount.Id = userAccountId;
            membership.LastUpdateByUserAccountId = 2;
            membership.Description = "TestUpdatetestnarayanCont";
            membership.DeviceId = deviceId;
            membership.ActivationDate = DateTime.Today;
            membership.SupportAccountId = supportAccountId;
            membership.IsJiveSupportCenterMember = false;
            membership.IsJiveTrainingCenterMember = false;
            membership.HasJiveDirectCustomerSupport = false;
            membership.JiveId = "test62354update";
            membership.SalesforceContactId = "test645Update";
            membership.SalesforceLogInId = "sgduy3245Update";
            membership.Roles = new List<Role>();

            int roleID1 = 0;
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,2;");
            dataReader = Connection.ExecuteReader(command);

            if(dataReader != null) {
                while(dataReader.Read()) {
                    roleID1 = Convert.ToInt32(dataReader["roleID"]);
                    membership.Roles.Add(new Role {
                        Id = roleID1
                    });
                }
            }
            dataReader.Close();

            localMembership = membershipLogic.UpdateMembership(membership);
            Assert.IsNotNull(localMembership, "Create Failed");


            command = Connection.GetSqlStringCommand("Select * from membership where MembershipId=" + membership.Id);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader.Read()) {
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], " Update Membership Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], " Update Membership Failed");
                Assert.AreEqual(membership.Description, Convert.ToString(dataReader["Description"]), " Update Membership Failed");
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], " Update Membership Failed");
                Assert.AreEqual(membership.UserAccount.Id, (long)dataReader["UserAccountId"], " Update Membership Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], " Update Membership Failed");
                Assert.AreEqual(membership.SupportAccountId, (long)dataReader["SupportAccountId"], " Update Membership Failed");
                Assert.AreEqual(membership.GroupId, (long)dataReader["GroupId"], " Update Membership Failed");
                Assert.AreEqual(membership.ExpirationDate, (DateTime)dataReader["ExpirationDate"], " Update Membership Failed");
                Assert.AreEqual(membership.ActivationDate, (DateTime)dataReader["ActivationDate"], " Update Membership Failed");
                Assert.AreEqual(membership.CreatedOn, (DateTime)dataReader["CreatedOn"], " Update Membership Failed");

                Assert.AreEqual(membership.IsJiveSupportCenterMember, Convert.ToBoolean(dataReader["IsJiveSupportCenterMember"]), " Update Membership Failed");
                Assert.AreEqual(membership.IsJiveTrainingCenterMember, Convert.ToBoolean(dataReader["IsJiveTrainingCenterMember"]), " Update Membership Failed");
                Assert.AreEqual(membership.HasJiveDirectCustomerSupport, Convert.ToBoolean(dataReader["HasJiveDirectCustomerSupport"]), " Update Membership Failed");
                Assert.AreEqual(membership.JiveId, Convert.ToString(dataReader["JiveId"]), " Update Membership Failed");
                Assert.AreEqual(membership.SalesforceLogInId, Convert.ToString(dataReader["SalesforceLogInId"]), " Update Membership Failed");
                Assert.AreEqual(membership.SalesforceContactId, Convert.ToString(dataReader["SalesforceContactId"]), " Update Membership Failed");

                Assert.IsNotNull(membership.CreatedOn, "Create Failed");
                Assert.IsNotNull(membership.ExpirationDate, "Create Failed");
                Assert.IsNotNull(membership.ActivationDate, "Create Failed");
            }
            else {
                Assert.Fail();
            }

            dataReader.Close();

            foreach (Role localRole in membership.Roles) {
                command = Connection.GetSqlStringCommand("Select Count(*) as count from membershiproles where RoleId=" + localRole.Id + " And membershipId=" + localMembership.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            }

            dataReader.Close();

        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GetByIDSupportAccountMembersLogicTest() {

          
            Role role = new Role();
             DbCommand command = null;
            IDataReader dataReader;

            long supportAccountId = 0;

            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId asc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    supportAccountId = Convert.ToInt32(dataReader["supportAccountId"]);

                }
            }
            dataReader.Close();

            List<MembershipListItem> membershipList = new List<MembershipListItem>();
            

           membershipList= membershipLogic.GetSupportAccountMembers(supportAccountId, dataParameters);

           command = Connection.GetSqlStringCommand("SELECT COUNT(1) as count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.SupportAccountId=" + supportAccountId);
           dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(membershipList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.IsNotNull(membershipList, "Read Failed");
            }

            dataReader.Close();
            command = Connection.GetSqlStringCommand("Select UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId where M.SupportAccountId=" + supportAccountId);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                while(dataReader.Read())
                    ;
                if(dataReader["FirstName"] != DBNull.Value)
                    if(membershipList.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"])) == null) {
                        Assert.Fail("Read Fail");
                    }
                if(dataReader["LastName"] != DBNull.Value)
                    if(membershipList.Find(x => x.LastName == Convert.ToString(dataReader["LastName"])) == null) {
                        Assert.Fail("Read Fail");
                    }
                if(dataReader["DisplayName"] != DBNull.Value)
                    if(membershipList.Find(x => x.DisplayName == Convert.ToString(dataReader["DisplayName"])) == null) {
                        Assert.Fail("Read Fail");
                    }
                if(dataReader["Email"] != DBNull.Value)
                    if(membershipList.Find(x => x.Email == Convert.ToString(dataReader["Email"])) == null) {
                        Assert.Fail("Read Fail");
                    }
                if(dataReader["ActivationDate"] != DBNull.Value)
                    if(membershipList.Find(x => x.ActivationDate == Convert.ToDateTime(dataReader["ActivationDate"])) == null) {
                        Assert.Fail("Read Fail");
                    }
                if(dataReader["ExpirationDate"] != DBNull.Value)
                    if(membershipList.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                        Assert.Fail("Read Fail");
                    }
            }
            else {
                Assert.AreEqual(membershipList.Count, 0, "Read Failed");
            }
            dataReader.Close();


        }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
        public void GetDeviceMembersLogicTest() {

            
             DbCommand command = null;
             long deviceId = 2;
             dataParameters = new DataParameters();
             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;
             List<MembershipListItem> memberList = new List<MembershipListItem>();
             command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if (dataReader.Read()) {
                 deviceId = (long)dataReader["DeviceId"];
             }
             memberList = membershipLogic.GetDeviceMembers(deviceId, dataParameters);
             Assert.IsNotNull(memberList, "Read Failed");
             Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");


             if(memberList.Count > dataParameters.PaginationCount) {
                 Assert.Fail("Read Failed with Pagination");
             }
             if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
             }
             if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
             }

             command = Connection.GetStoredProcCommand("GetMembershipsByDeviceId");
             Connection.AddInParameter(command, "p_DeviceId", DbType.Int64, deviceId);
             Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
             Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
             Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
             Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
             Connection.AddInParameter(command, "p_SearchValue", DbType.String, "");
             Connection.AddInParameter(command, "p_Searchfield", DbType.String, "");
             Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
             dataReader = Connection.ExecuteReader(command);

             if(dataReader != null) {
                 while(dataReader.Read())
                     ;
                 if(dataReader["FirstName"] != DBNull.Value)
                     if(memberList.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["LastName"] != DBNull.Value)
                     if(memberList.Find(x => x.LastName == Convert.ToString(dataReader["LastName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["DisplayName"] != DBNull.Value)
                     if(memberList.Find(x => x.DisplayName == Convert.ToString(dataReader["DisplayName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["Email"] != DBNull.Value)
                     if(memberList.Find(x => x.Email == Convert.ToString(dataReader["Email"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ActivationDate"] != DBNull.Value)
                     if(memberList.Find(x => x.ActivationDate == Convert.ToDateTime(dataReader["ActivationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ExpirationDate"] != DBNull.Value)
                     if(memberList.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
             }
             else {
                 Assert.AreEqual(memberList.Count, 0, "Read Failed");
             }
             dataReader.Close();

        }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void GetGroupMembersLogicTest() {
             DbCommand command = null;
             long GroupId = 17;
             dataParameters = new DataParameters();

             List<MembershipListItem> membershipList = new List<MembershipListItem>();
             command = Connection.GetSqlStringCommand("select GroupId from Csp.supportAccountGroup order by GroupId asc limit 1");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if (dataReader.Read())
                 GroupId = (long)dataReader["GroupId"];
             membershipList = membershipLogic.GetGroupMembers(GroupId, dataParameters);
             command = Connection.GetSqlStringCommand("SELECT COUNT(1) count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.GroupId=" + GroupId);
             dataReader = Connection.ExecuteReader(command);
             if (dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(membershipList.Count, Convert.ToInt32(dataReader["count"]), "GetGroupMembersLogicTest Failed");
             }
             else {
                 Assert.AreEqual(membershipList.Count, 0, "GetGroupMembersLogicTest Failed");
             }
             dataReader.Close();

             command = Connection.GetSqlStringCommand("SELECT UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId  Join SupportAccountGroup SAG on  SAG.GroupId=M.GroupId   where M.GroupId=" + GroupId);
             dataReader = Connection.ExecuteReader(command);
             if(dataReader != null) {
                 while(dataReader.Read())
                     ;
                 if(dataReader["FirstName"] != DBNull.Value)
                     if(membershipList.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["LastName"] != DBNull.Value)
                     if(membershipList.Find(x => x.LastName == Convert.ToString(dataReader["LastName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["DisplayName"] != DBNull.Value)
                     if(membershipList.Find(x => x.DisplayName == Convert.ToString(dataReader["DisplayName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["Email"] != DBNull.Value)
                     if(membershipList.Find(x => x.Email == Convert.ToString(dataReader["Email"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ActivationDate"] != DBNull.Value)
                     if(membershipList.Find(x => x.ActivationDate == Convert.ToDateTime(dataReader["ActivationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ExpirationDate"] != DBNull.Value)
                     if(membershipList.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
             }
             else {
                 Assert.AreEqual(membershipList.Count, 0, "Read Failed");
             }
             dataReader.Close();

             
         }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
         public void DeleteMembershipLogicTest() {
            
             DbCommand command = null;
             IDataReader dataReader;
             long deletedByUserAccountId = 1;

             long membershipId = 0;

             command = Connection.GetSqlStringCommand("select MembershipID from membership order by MembershipID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     membershipId = Convert.ToInt32(dataReader["MembershipID"]);

                 }
             }

             dataReader.Close();

             membershipLogic.DeleteMembership(membershipId, deletedByUserAccountId);

             command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" + membershipId);
             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
             dataReader.Close();

             command = Connection.GetSqlStringCommand("Select Count(1) as count from membershiproles where MembershipID=" + membershipId);
             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
             dataReader.Close();

         }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
         public void GetUserAccountMembershipsLogicTest() {

             DbCommand command = null;
             IDataReader dataReader;
             List<Membership> MembershipList = new List<Membership>();
             long userAccountId = 0;
            command = Connection.GetSqlStringCommand("SELECT  UserAccountId FROM useraccount order by UserAccountId desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     userAccountId = Convert.ToInt32(dataReader["UserAccountId"]);

                 }
             }
             dataReader.Close();
          

             MembershipList=membershipLogic.GetUserAccountMemberships(userAccountId, dataParameters);

             command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" + userAccountId);
             dataReader = Connection.ExecuteReader(command);
             dataReader.Read();
             Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "GetUserAccountMembershipsLogicTest Failed");
             dataReader.Close();


             foreach(Membership membership in MembershipList) {
                 command = Connection.GetSqlStringCommand("select * from membership where membershipId=" + membership.Id);
                 dataReader = Connection.ExecuteReader(command);
                 if(dataReader != null) {
                     while(dataReader.Read()) {
                         Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.Description, Convert.ToString(dataReader["Description"]), "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.UserAccount.Id, (long)dataReader["UserAccountId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.SupportAccountId, (long)dataReader["SupportAccountId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.GroupId, (long)dataReader["GroupId"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.ExpirationDate, (DateTime)dataReader["ExpirationDate"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.ActivationDate, (DateTime)dataReader["ActivationDate"], "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.CreatedOn, (DateTime)dataReader["CreatedOn"], "GetUserAccountMembershipsLogicTest Failed");

                         Assert.AreEqual(membership.IsJiveSupportCenterMember, Convert.ToBoolean(dataReader["IsJiveSupportCenterMember"]), "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.IsJiveTrainingCenterMember, Convert.ToBoolean(dataReader["IsJiveTrainingCenterMember"]), "GetUserAccountMembershipsLogicTest Failed");
                         Assert.AreEqual(membership.HasJiveDirectCustomerSupport, Convert.ToBoolean(dataReader["HasJiveDirectCustomerSupport"]), "GetUserAccountMembershipsLogicTest Failed");
                        
                         if (membership.JiveId != null && dataReader["JiveId"] != DBNull.Value)
                             Assert.AreEqual(membership.JiveId, Convert.ToString(dataReader["JiveId"]), "GetDeviceMembersLogicTest Failed");

                         if (membership.SalesforceLogInId != null && dataReader["SalesforceLogInId"] != DBNull.Value)
                             Assert.AreEqual(membership.SalesforceLogInId, Convert.ToString(dataReader["SalesforceLogInId"]), "GetDeviceMembersLogicTest Failed");

                         if (membership.SalesforceContactId != null && dataReader["SalesforceContactId"] != DBNull.Value)
                             Assert.AreEqual(membership.SalesforceContactId, Convert.ToString(dataReader["SalesforceContactId"]), "GetDeviceMembersLogicTest Failed");

                         Assert.IsNotNull(membership.CreatedOn, "GetUserAccountMembershipsLogicTest Failed");
                         Assert.IsNotNull(membership.ExpirationDate, "GetUserAccountMembershipsLogicTest Failed");
                         Assert.IsNotNull(membership.ActivationDate, "GetUserAccountMembershipsLogicTest Failed");
                     }
                 }
                 else {
                     Assert.Fail("GetUserAccountMembershipsLogicTest Failed");
                 }
                 dataReader.Close();
             }
         }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
         public void CreateMembershipRoleLogicTest() {
             DbCommand command = null;
             IDataReader dataReader;

             long roleId = 0;
             command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     roleId = Convert.ToInt32(dataReader["roleId"]);

                 }
             }
             dataReader.Close();
             command = null;


             long MembershipId = 0;
             command = Connection.GetSqlStringCommand("select MembershipId from Membership order by MembershipId desc limit 0,1;");

             dataReader = Connection.ExecuteReader(command);


             if (dataReader != null) {
                 if (dataReader.Read()) {
                     MembershipId = Convert.ToInt32(dataReader["MembershipId"]);

                 }
             }
             dataReader.Close();
             command = null;


             membershipLogic.CreateMembershipRole(MembershipId, roleId, createdByUserAccountId);
             

             command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
             dataReader = Connection.ExecuteReader(command);

             dataReader.Read();
             Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
             dataReader.Close();
         }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void GetMembershipRolesLogicTest() {

            DbCommand command = null;
            IDataReader dataReader;
            long MembershipId = 0;

            long roleId = 0;
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    roleId = Convert.ToInt32(dataReader["roleId"]);

                }
            }
            dataReader.Close();
            command = null;


            command = Connection.GetSqlStringCommand("select MembershipId from Membership order by MembershipId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    MembershipId = Convert.ToInt32(dataReader["MembershipId"]);

                }
            }
            dataReader.Close();
            command = null;




            roleList = membershipLogic.GetMembershipRoles(MembershipId, dataParameters);
            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(roleList.Count, Convert.ToInt32(dataReader["count"]), "GetMembershipRolesLogicTest Failed");
            }
            else {
                Assert.AreEqual(roleList.Count, 0, "GetMembershipRolesLogicTest Failed");
            }

            dataReader.Close();
            foreach (Role role in roleList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "GetMembershipRolesLogicTest Failed");

                dataReader.Close();
            }

            foreach(Role role in roleList) {
                command = Connection.GetSqlStringCommand("select * from role where RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(role.Name, Convert.ToString(dataReader["Name"]), "GetMembershipRolesLogicTest Failed");
                if(dataReader["CreatedByUserAccountId"] != DBNull.Value)
                    Assert.AreEqual(role.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "GetMembershipRolesLogicTest Failed");
                Assert.AreEqual(role.Description, Convert.ToString(dataReader["Description"]), "GetMembershipRolesLogicTest Failed");
                Assert.AreEqual(role.Classification, ((SecurityClassification)Enum.Parse(typeof(SecurityClassification), Convert.ToString(dataReader["SecurityClassification"]), true)), "GetMembershipRolesLogicTest Failed");

               
                dataReader.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void DeleteMembershipRoleLogicTest1() {

            DbCommand command = null;
            IDataReader dataReader;


            long roleId = 0;
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    roleId = Convert.ToInt32(dataReader["roleId"]);

                }
            }
            dataReader.Close();
            command = null;


            long MembershipId = 0;
            command = Connection.GetSqlStringCommand("select MembershipId from Membership order by MembershipId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    MembershipId = Convert.ToInt32(dataReader["MembershipId"]);

                }
            }
            dataReader.Close();
            command = null;

            membershipLogic.DeleteMembershipRole(MembershipId, roleId, 0);
             
            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }

        [TestMethod]
        public void DeleteMembershipRoleLogicTest2() {

            DbCommand command = null;
            IDataReader dataReader;




            long MembershipId = 0;
            command = Connection.GetSqlStringCommand("select MembershipId from Membership order by MembershipId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    MembershipId = Convert.ToInt32(dataReader["MembershipId"]);

                }
            }
            dataReader.Close();
            command = null;


            long roleId = 0;
            command = Connection.GetSqlStringCommand("select roleID from role order by roleID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    roleId = Convert.ToInt32(dataReader["roleId"]);

                }
            }
            dataReader.Close();
            command = null;


            membershipLogic.DeleteMembershipRoles(MembershipId);
            

            command = Connection.GetSqlStringCommand("select count(1) as count from membershiproles where MembershipId=" + MembershipId + " and RoleId=" + roleId);
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
  
    }
}
 
