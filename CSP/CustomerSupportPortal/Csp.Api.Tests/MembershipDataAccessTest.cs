using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Csp.Api.Tests {

    [TestClass]
    public class MembershipDataAccessTest:DBConnections  {
        List<Role> roleList = new List<Role>();
        MembershipDataAccess membershipDataAccess = new MembershipDataAccess();
        DataParameters dataParameters = new DataParameters();
        

        /// <summary>
        /// The purpose of this method to test RoleDataAccess.Create
        /// </summary>
        [TestMethod]
        public void MembershipDataAccessCreate() {
            DbCommand command = null;
            IDataReader dataReader;
            Role role = new Role();
            Membership  membership  = new Membership();
            membership.GroupId = 1;
            membership.ExpirationDate = DateTime.Today;
            membership.DeviceId = 1;
            membership.UserAccount.Id = 1;
            membership.CreatedByUserAccountId = 1;
            membership.Description = "Test";
            membership.DeviceId = 2;
            membership.ActivationDate = DateTime.Today;
            membership.SupportAccountId = 1;
            membership.Roles = new List<Role>();
            membership.Roles.Add(new Role {
                Id = 1
            });


            Membership localMembership = new Membership();

            localMembership=membershipDataAccess.Create(membership);
            Assert.IsNotNull(localMembership,"Create Failed");


            command = Connection.GetSqlStringCommand("Select * from membership where MembershipId=LAST_INSERT_ID() ");
            dataReader = Connection.ExecuteReader(command);

            if (dataReader.Read()) {
                Assert.AreEqual(membership.DeviceId,(long)dataReader["DeviceId"], "Create Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId,(long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(membership.Description,Convert.ToString(dataReader["Description"]), "Create Failed");
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "Create Failed");
                Assert.AreEqual(membership.UserAccount.Id, (long)dataReader["UserAccountId"], "Create Failed");
                Assert.AreEqual(membership.CreatedByUserAccountId, (long)dataReader["CreatedByUserAccountId"], "Create Failed");
                Assert.AreEqual(membership.SupportAccountId, (long)dataReader["SupportAccountId"], "Create Failed");
                Assert.AreEqual(membership.GroupId, (long)dataReader["GroupId"], "Create Failed");
                Assert.IsNotNull(membership.CreatedOn, "Create Failed");
                Assert.IsNotNull(membership.ExpirationDate, "Create Failed");
                Assert.IsNotNull(membership.ActivationDate, "Create Failed");
            }
            else {
                Assert.Fail();
            }
            dataReader.Close();

            //Removed Because of transaction logic Changes

            //foreach (Role localRole in membership.Roles) {
            //    command = Connection.GetSqlStringCommand("Select Count(*) as count from membershiproles where RoleId=" + localRole.Id + " And membershipId=" + localMembership.Id);
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
        public void MembershipDataAccessDelete() {
           
            DbCommand command = null;
            IDataReader dataReader;
            long deletedByUserAccountId = 1;
            long membershipId = 12;

            membershipDataAccess.Delete(membershipId, deletedByUserAccountId);

            command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" + membershipId );
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
        /// The purpose of this method to test AccountTypeDefaultRolesDataAccess.Read
        /// </summary>
        [TestMethod]
        public void MembershipDataAccessRead() {
            //DbCommand command = null;
            //IDataReader dataReader;
            //SearchOptions options = new SearchOptions();
            //List<Membership> MembershipList = new List<Membership>();

            //MembershipList = membershipDataAccess.Read(options, dataParameters);
            //command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" +  options.SearchByIds);
            //dataReader = Connection.ExecuteReader(command);

            //if (dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(MembershipList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.AreEqual(MembershipList.Count, 0, "Read Failed");
            //}

            //dataReader.Close();
            //foreach (Membership membership in MembershipList) {
            //    command = Connection.GetSqlStringCommand("Select Count(1) as count from membership where MembershipID=" + membership.Id);
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
        public void MembershipDataAccessUpdate() {
            DbCommand command = null;
            IDataReader dataReader;
            Role role = new Role();
            MembershipDataAccess MmmbershipDataAccess = new MembershipDataAccess();
            Membership membership = new Membership();
            Membership localMembership = new Membership();

            membership = MmmbershipDataAccess.Read(14);
            membership.GroupId = 1;
            membership.ExpirationDate = DateTime.Today;
            membership.DeviceId = 1;
            membership.UserAccount.Id = 1;
            membership.LastUpdateByUserAccountId = 2;
            membership.Description = "TestUpdatetestnarayanCont";
            membership.DeviceId = 2;
            membership.ActivationDate = DateTime.Today;
            membership.SupportAccountId = 1;
            membership.Roles = new List<Role>();
            membership.Roles.Add(new Role {
                Id = 1
            });

            localMembership = membershipDataAccess.Update(membership);
            Assert.IsNotNull(localMembership, "MembershipDataAccessUpdate Failed");


            command = Connection.GetSqlStringCommand("Select * from membership where MembershipId=" + membership.Id);
            dataReader = Connection.ExecuteReader(command);

            if (dataReader.Read()) {
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.LastUpdateByUserAccountId, (long)dataReader["LastUpdateByUserAccountId"], "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.Description, Convert.ToString(dataReader["Description"]), "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.DeviceId, (long)dataReader["DeviceId"], "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.UserAccount.Id, (long)dataReader["UserAccountId"], "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.SupportAccountId, (long)dataReader["SupportAccountId"], "MembershipDataAccessUpdate Failed");
                Assert.AreEqual(membership.GroupId, (long)dataReader["GroupId"], "MembershipDataAccessUpdate Failed");
                Assert.IsNotNull(membership.CreatedOn, "MembershipDataAccessUpdate Failed");
                Assert.IsNotNull(membership.ExpirationDate, "MembershipDataAccessUpdate Failed");
                Assert.IsNotNull(membership.ActivationDate, "MembershipDataAccessUpdate Failed");
            }
            else {
                Assert.Fail();
            }

            dataReader.Close();

            // as per Transaction Changes

            //foreach (Role localRole in membership.Roles) {
            //    command = Connection.GetSqlStringCommand("Select Count(*) as count from membershiproles where RoleId=" + localRole.Id + " And membershipId=" + localMembership.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            //}

            //dataReader.Close();

        }

        /// <summary>
        /// 
        /// </summary>
         [TestMethod]
        public void MembershipDataAccessReadByID() {
           
            long membershipId = 11;
            Role role = new Role();
            Membership membership = new Membership();
            DbCommand command = null;
            IDataReader dataReader;


            command = null;

            command = Connection.GetSqlStringCommand("select MembershipID from membership order by MembershipID desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    membershipId = Convert.ToInt32(dataReader["MembershipID"]);

                }
            }

            dataReader.Close();

            membership = membershipDataAccess.Read(membershipId);

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
        }


         [TestMethod]
         public void ReadSupportAccountMembershipsPagination() {
             List<MembershipListItem> memberships = new List<MembershipListItem>();
             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;

             long supportAccountId = 0;
             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 if(dataReader.Read()) {
                     supportAccountId = Convert.ToInt64(dataReader["supportAccountId"]);

                 }
             }

             dataReader.Close();
             memberships = membershipDataAccess.ReadSupportAccountMembers(supportAccountId, dataParameters);

             Assert.IsNotNull(memberships, "Read Failed with Pagination");

             Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
              command = null;

             command = Connection.GetSqlStringCommand("SELECT COUNT(1) as count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.SupportAccountId=" + supportAccountId);
              dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(memberships.Count, 0, "Read Failed");
             }

             dataReader.Close();


             if(memberships.Count > dataParameters.PaginationCount) {
                 Assert.Fail("Read Failed with Pagination");
             }

             if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberships.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
             }

             if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberships.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

             }


              command = Connection.GetSqlStringCommand("Select UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId where M.SupportAccountId=" + supportAccountId);
             dataReader = Connection.ExecuteReader(command);
             if(dataReader != null) {
                 while(dataReader.Read());
                 if(dataReader["FirstName"] != DBNull.Value)
                     if(memberships.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"]))== null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["LastName"] != DBNull.Value)
                     if(memberships.Find(x => x.LastName == Convert.ToString(dataReader["LastName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["DisplayName"] != DBNull.Value)
                     if(memberships.Find(x => x.DisplayName == Convert.ToString(dataReader["DisplayName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["Email"] != DBNull.Value)
                     if(memberships.Find(x => x.Email == Convert.ToString(dataReader["Email"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ActivationDate"] != DBNull.Value)
                     if(memberships.Find(x => x.ActivationDate == Convert.ToDateTime(dataReader["ActivationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ExpirationDate"] != DBNull.Value)
                     if(memberships.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
             }
             else {
                 Assert.AreEqual(memberships.Count, 0, "Read Failed");
             }
             dataReader.Close();



             //foreach(Membership membership in memberships) {
             //    command = Connection.GetSqlStringCommand("select count(1) as count from Membership where SupportAccountId=" + supportAccountId + " and MembershipID=" + membership.Id);
             //    dataReader = Connection.ExecuteReader(command);
             //    dataReader.Read();

             //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

             //    dataReader.Close();
             //}


         }


         [TestMethod]
         public void ReadSupportAccountMemberships() {
             List<MembershipListItem> memberships = new List<MembershipListItem>();

             dataParameters = new DataParameters();
             long supportAccountId = 0;
             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 if(dataReader.Read()) {
                     supportAccountId = Convert.ToInt64(dataReader["supportAccountId"]);

                 }
             }

             dataReader.Close();
             memberships = membershipDataAccess.ReadSupportAccountMembers(supportAccountId, dataParameters);

             Assert.IsNotNull(memberships, "Read Failed with Pagination");

            
             command = null;

             command = Connection.GetSqlStringCommand("SELECT COUNT(1) as count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.SupportAccountId=" + supportAccountId);
             dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(memberships.Count, Convert.ToInt64(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(memberships.Count, 0, "Read Failed");
             }

             dataReader.Close();


             command = Connection.GetSqlStringCommand("Select UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId where M.SupportAccountId=" + supportAccountId);
          
              
             dataReader = Connection.ExecuteReader(command);
             if(dataReader != null) {
                 while(dataReader.Read())
                     ;
                 if(dataReader["FirstName"] != DBNull.Value)
                     if(memberships.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["LastName"] != DBNull.Value)
                     if(memberships.Find(x => x.LastName == Convert.ToString(dataReader["LastName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["DisplayName"] != DBNull.Value)
                     if(memberships.Find(x => x.DisplayName == Convert.ToString(dataReader["DisplayName"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["Email"] != DBNull.Value)
                     if(memberships.Find(x => x.Email == Convert.ToString(dataReader["Email"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ActivationDate"] != DBNull.Value)
                     if(memberships.Find(x => x.ActivationDate == Convert.ToDateTime(dataReader["ActivationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
                 if(dataReader["ExpirationDate"] != DBNull.Value)
                     if(memberships.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                         Assert.Fail("Read Fail");
                     }
             }
             else {
                 Assert.AreEqual(memberships.Count, 0, "Read Failed");
             }
             dataReader.Close();



             //foreach(Membership membership in memberships) {
             //    command = Connection.GetSqlStringCommand("select count(1) as count from Membership where SupportAccountId=" + supportAccountId + " and MembershipID=" + membership.Id);
             //    dataReader = Connection.ExecuteReader(command);
             //    dataReader.Read();

             //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

             //    dataReader.Close();
             //}


         }

         [TestMethod]
         public void ReadUserAccountMembershipsPagination() {
             List<Membership> memberships = new List<Membership>();
             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;

             long userAccountId = 0;
             DbCommand command = null;

             command = Connection.GetSqlStringCommand("select userAccountId from userAccount order by userAccountId limit 0,1;");

             IDataReader dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 if(dataReader.Read()) {
                     userAccountId = Convert.ToInt64(dataReader["userAccountId"]);

                 }
             }

             dataReader.Close();
             memberships = membershipDataAccess.ReadUserAccountMemberships(userAccountId, dataParameters);

             Assert.IsNotNull(memberships, "Read Failed with Pagination");

             Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

              command = null;

             command = Connection.GetSqlStringCommand("select count(1) as count from membership where UserAccountId=" + userAccountId);
              dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(memberships.Count, 0, "Read Failed");
             }


             if(memberships.Count > dataParameters.PaginationCount) {
                 Assert.Fail("Read Failed with Pagination");
             }

             if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberships.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
             }

             if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberships.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

             }
             dataReader.Close();



             foreach(Membership membership in memberships) {
                 command = Connection.GetSqlStringCommand("select count(1) as count from membership where MembershipId=" + membership.Id + " and userAccountId=" + userAccountId);
                 dataReader = Connection.ExecuteReader(command);
                 dataReader.Read();

                 Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                 dataReader.Close();
             }


         }


         [TestMethod]
         public void ReadGroupDefaultMembershipTest() {
             DbCommand command = null;
             dataParameters = new DataParameters();
             List<MembershipListItem> memberList = new List<MembershipListItem>();
             long groupId = 0;

             command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if(dataReader.Read())
                 groupId = (long)dataReader["GroupId"];
             dataReader.Close();
             memberList = membershipDataAccess.ReadGroupMembers(groupId, dataParameters);
             command = Connection.GetSqlStringCommand("SELECT COUNT(1) count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.GroupId=" + groupId);
             dataReader = Connection.ExecuteReader(command);
             if(dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(memberList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(memberList.Count, 0, "Read Failed");
             }
             dataReader.Close();


             command = Connection.GetSqlStringCommand("SELECT UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId  Join SupportAccountGroup SAG on  SAG.GroupId=M.GroupId   where M.GroupId=" + groupId);
             dataReader = Connection.ExecuteReader(command);
             if(dataReader != null) {
                 while(dataReader.Read());
                 if(dataReader["FirstName"] != DBNull.Value)
                     if(memberList.Find(x => x.FirstName == Convert.ToString(dataReader["FirstName"]))== null) {
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

             //foreach(MembershipListItem membership in memberList) {
             //    command = Connection.GetSqlStringCommand("select count(1) as count from membership where GroupId=" + groupId + " and MembershipId=" + membership.Id);
             //    dataReader = Connection.ExecuteReader(command);
             //    dataReader.Read();
             //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
             //    dataReader.Close();
             //}
         }

         [TestMethod]
         public void ReadMembershipWithPaginationTest() {
             DbCommand command = null;
             dataParameters = new DataParameters();
             List<MembershipListItem> memberList = new List<MembershipListItem>();

             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;
             dataParameters.SortField = "FirstName"; 
             long groupId = 0;

             command = Connection.GetSqlStringCommand("select GroupId from csp.SupportAccountGroup order by GroupId desc limit 0,1;");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if(dataReader.Read())
                 groupId = (long)dataReader["GroupId"];
             dataReader.Close();
             memberList = membershipDataAccess.ReadGroupMembers(groupId, dataParameters);

             Assert.IsNotNull(memberList, "Read Failed");
             Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

             command = null;

             command = Connection.GetSqlStringCommand("SELECT COUNT(1) count from membership Join UserAccount UA on  UA.UserAccountId=membership.UserAccountId where membership.GroupId=" + groupId);
             dataReader = Connection.ExecuteReader(command);


             if(dataReader != null) {
                 dataReader.Read();
                 Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
             }
             else {
                 Assert.AreEqual(memberList.Count, 0, "Read Failed");
             }


             if(memberList.Count > dataParameters.PaginationCount) {
                 Assert.Fail("Read Failed with Pagination");
             }
             if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
             }
             if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                 Assert.AreEqual(memberList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
             }

             command = Connection.GetSqlStringCommand("SELECT UA.FirstName,UA.LastName, UA.DisplayName, UA.Email, M.ActivationDate, M.ExpirationDate from membership M Join UserAccount UA on  UA.UserAccountId=M.UserAccountId  Join SupportAccountGroup SAG on  SAG.GroupId=M.GroupId   where M.GroupId=" + groupId);
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

         [TestMethod]
         public void ReadMembershipsbydeviceIdDefaultRead() {
             DbCommand command = null;
             long deviceId = 0;
             dataParameters = new DataParameters();

             List<MembershipListItem> memberList = new List<MembershipListItem>();
             command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if(dataReader.Read())
                 deviceId = (long)dataReader["DeviceId"];
             memberList = membershipDataAccess.ReadDeviceMembers(deviceId, dataParameters);

              
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

         [TestMethod]
         public void ReadMembershipsWithPaginationTest() {
             DbCommand command = null;
             long deviceId = 0;
             dataParameters = new DataParameters();

             List<MembershipListItem> memberList = new List<MembershipListItem>();

             dataParameters.PaginationCount = 10;
             dataParameters.PaginationOffset = 0;
             command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
             IDataReader dataReader = Connection.ExecuteReader(command);
             if(dataReader.Read())
                 deviceId = (long)dataReader["DeviceId"];
             memberList = membershipDataAccess.ReadDeviceMembers(deviceId, dataParameters);

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

    }
}
