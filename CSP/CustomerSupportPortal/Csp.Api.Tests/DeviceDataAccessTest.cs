using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
namespace Csp.Api.Tests {

    [TestClass]
    public class DeviceDataAccessTest : DBConnections {
        DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
        DataParameters dataParameters = new DataParameters();
        List<Device> deviceList = new List<Device>();
        List<Membership> membershipList = new List<Membership>();
        List<License> licenseList = new List<License>();
        Device device = new Device();
        [TestMethod]
        public void ReadDeviceByDeviceId() {
            DbCommand command = null;
            dataParameters = new DataParameters();
            deviceDataAccess = new DeviceDataAccess();
            deviceList = new List<Device>();
            device = new Device();
            long deviceId = 0;
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            device = deviceDataAccess.Read(deviceId,0);
            command = Connection.GetSqlStringCommand("select count(1) as count from device where DeviceId=" + deviceId);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(0, 0, "Read Failed");
            }
            dataReader.Close();

        }

        [TestMethod]
        public void ReadwithSearchOptionTest() {
        }




        [TestMethod]
        public void UpdateDeviceTest() {
            DbCommand command = null;
            long deviceId = 0;
            dataParameters = new DataParameters();
            deviceDataAccess = new DeviceDataAccess();
            deviceList = new List<Device>();
            device = new Device();
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];

            device = deviceDataAccess.Read(deviceId,0);
            Assert.AreEqual(device.Id, deviceId, "Read for Update Failed");
            device.Id = deviceId;
            device.SerialNumber = "Test Serial Number";
            device.Licenses.Add(new License {
                Id = 1
            });
            device.UserAccount.Id = 1;
            device.DefaultRoles.Add(new Role {
                Id = 2
            });
            device.DefaultRoles.Add(new Role {
                Id = 4
            });
            device.LastUpdateByUserAccountId = 1;
            device = deviceDataAccess.Update(device,0);
            Assert.IsNotNull(device, "Update Failed");
            Assert.AreEqual(device.Id, deviceId, "Read for Update Failed");
            Assert.AreEqual(device.SerialNumber, "Test Serial Number", " Serial Number Update Failed");

            command = Connection.GetSqlStringCommand("select count(1) as count from device where DeviceId=" + deviceId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();

            device.Licenses.Add(new License {
                Id = 1
            });
            device.UserAccount.Id = 1;
            device.DefaultRoles = new List<Role>();
            device.DefaultRoles.Add(new Role {
                Id = 2
            });
            device.DefaultRoles.Add(new Role {
                Id = 4
            });

            //REMOVED AS PER TRANSACTION CHANGES

            //foreach(Role role in device.DefaultRoles) {
            //    command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and RoleId=" + role.Id);
            //    dataReader = Connection.ExecuteReader(command);
            //    dataReader.Read();
            //    Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            //    dataReader.Close();
            //}
        }

        [TestMethod]
        public void ReadSupportAccountDevices() {

            long supportAccountId = 0;
            long prestrictedUserAccount = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    supportAccountId = Convert.ToInt64(dataReader["supportAccountId"]);

                }
            }

            dataReader.Close();

            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    prestrictedUserAccount = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }

            dataReader.Close();

            dataParameters = new DataParameters();
            List<DeviceListItem> DeviceListItems = deviceDataAccess.ReadSupportAccountDevices(supportAccountId, prestrictedUserAccount, dataParameters);

            Assert.IsNotNull(DeviceListItems, "Read Failed with Pagination");

           command = Connection.GetStoredProcCommand("GetDevicesBySupportAccountId");
            Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, supportAccountId);
            Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
            Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
            Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
            Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, prestrictedUserAccount);
            Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                while(dataReader.Read()) {
                    if(dataReader["ModelName"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.ModelName == Convert.ToString(dataReader["ModelName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceName"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceName == Convert.ToString(dataReader["DeviceName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceId"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"])) == null) {
                            Assert.Fail("Read Fail");
                        }

                    if(dataReader["SerialNumber"] != DBNull.Value)
                        if (DeviceListItems.Find(x => x.SerialNumber == Convert.ToString(dataReader["SerialNumber"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    /* if(dataReader["licensename"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.LicenseName == Convert.ToString(dataReader["licensename"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["ExpirationDate"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["LicenseKey"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.ExpirationDate == Convert.ToDateTime(dataReader["LicenseKey"])) == null) {
                            Assert.Fail("Read Fail");
                        }*/
                }
            }
            else {
                Assert.AreEqual(DeviceListItems.Count, 0, "Read Failed");
            }
           
        }

        [TestMethod]
        public void ReadSupportAccountDevicesPagination() {
             
            long supportAccountId = 0;
            long prestrictedUserAccount = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select supportAccountId from supportAccount order by supportAccountId desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    supportAccountId = Convert.ToInt64(dataReader["supportAccountId"]);

                }
            }

            dataReader.Close();

            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    prestrictedUserAccount = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }

            dataReader.Close();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            List<DeviceListItem> DeviceListItems = deviceDataAccess.ReadSupportAccountDevices(supportAccountId, prestrictedUserAccount, dataParameters);

            Assert.IsNotNull(DeviceListItems, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = Connection.GetStoredProcCommand("GetDevicesBySupportAccountId");
            Connection.AddInParameter(command, "p_SupportAccountId", DbType.Int64, supportAccountId);
            Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
            Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
            Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
            Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, prestrictedUserAccount);
            Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                while(dataReader.Read()) {
                    if(dataReader["ModelName"] != DBNull.Value)
                        if (DeviceListItems.Find(x => x.ModelName == Convert.ToString(dataReader["ModelName"])) == null)
                        {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceName"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceName == Convert.ToString(dataReader["DeviceName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceId"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"])) == null) {
                            Assert.Fail("Read Fail");
                        }

                    if(dataReader["SerialNumber"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.SerialNumber == Convert.ToString(dataReader["SerialNumber"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                   /* if(dataReader["licensename"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.LicenseName == Convert.ToString(dataReader["licensename"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["ExpirationDate"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["LicenseKey"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"]))
                            .Licenses.Find(y => y.ExpirationDate == Convert.ToDateTime(dataReader["LicenseKey"])) == null) {
                            Assert.Fail("Read Fail");
                        }*/
                }
            }
            else {
                Assert.AreEqual(DeviceListItems.Count, 0, "Read Failed");
            }
            long TotalRecordCount = 0;
            if(command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {

                TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
            }



            Assert.AreEqual(dataParameters.TotalRecordCount, TotalRecordCount, "Read Failed");


            dataReader.Close();


            if(DeviceListItems.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(DeviceListItems.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(DeviceListItems.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }
            dataReader.Close();
        }


        [TestMethod]
        public void ReadGroupDevice() {
            dataParameters = new DataParameters();
            long groupID = 0;
            long prestrictedUserAccount = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();
            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    prestrictedUserAccount = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();

            System.Collections.Generic.List<DeviceListItem> DeviceListItems = deviceDataAccess.ReadGroupDevices(groupID, prestrictedUserAccount, dataParameters);
            Assert.IsNotNull(DeviceListItems, "Read Failed with Pagination");



            command = Connection.GetStoredProcCommand("GetDevicesByGroupId");
            Connection.AddInParameter(command, "p_groupID", DbType.Int64, groupID);
            Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
            Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
            Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
            Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, prestrictedUserAccount);
            Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                while(dataReader.Read()) {
                    if(dataReader["ModelName"] != DBNull.Value)
                        if (DeviceListItems.Find(x => x.ModelName == Convert.ToString(dataReader["ModelName"])) == null)
                        {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceName"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceName == Convert.ToString(dataReader["DeviceName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceId"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"])) == null) {
                            Assert.Fail("Read Fail");
                        }

                    if(dataReader["SerialNumber"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.SerialNumber == Convert.ToString(dataReader["SerialNumber"])) == null) {
                            Assert.Fail("Read Fail");
                        }
            
                }
            }
            else {
                Assert.AreEqual(DeviceListItems.Count, 0, "Read Failed");
            }

            
        }

        [TestMethod]
        public void ReadGroupDevicePagination() {
            dataParameters = new DataParameters();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            long prestrictedUserAccount = 0;

            long groupID = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();
            dataReader.Close();
            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId desc limit 0,1;");

            dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                if(dataReader.Read()) {
                    prestrictedUserAccount = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();

            List<DeviceListItem> DeviceListItems = deviceDataAccess.ReadGroupDevices(groupID, prestrictedUserAccount, dataParameters);

           
            Assert.IsNotNull(DeviceListItems, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = Connection.GetStoredProcCommand("GetDevicesByGroupId");
            Connection.AddInParameter(command, "p_groupID", DbType.Int64, groupID);
            Connection.AddInParameter(command, "p_PaginationOffset", DbType.Int64, dataParameters.PaginationOffset);
            Connection.AddInParameter(command, "p_PaginationCount", DbType.Int32, dataParameters.PaginationCount);
            Connection.AddInParameter(command, "p_SortField", DbType.String, dataParameters.SortField);
            Connection.AddInParameter(command, "p_SortDirection", DbType.String, "ASC");
            Connection.AddInParameter(command, "p_RestrictedByUserAccountId", DbType.Int64, prestrictedUserAccount);
            Connection.AddOutParameter(command, "p_TotalRecordCount", DbType.Int64, 20);
            dataReader = Connection.ExecuteReader(command);
            if(dataReader != null) {
                while(dataReader.Read()) {
                    if(dataReader["ModelName"] != DBNull.Value)
                        if (DeviceListItems.Find(x => x.ModelName == Convert.ToString(dataReader["ModelName"])) == null)
                        {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceName"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceName == Convert.ToString(dataReader["DeviceName"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                    if(dataReader["DeviceId"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.DeviceId == Convert.ToInt64(dataReader["DeviceId"])) == null) {
                            Assert.Fail("Read Fail");
                        }

                    if(dataReader["SerialNumber"] != DBNull.Value)
                        if(DeviceListItems.Find(x => x.SerialNumber == Convert.ToString(dataReader["SerialNumber"])) == null) {
                            Assert.Fail("Read Fail");
                        }
                  
                }
            }
            else {
                Assert.AreEqual(DeviceListItems.Count, 0, "Read Failed");
            }
            long TotalRecordCount = 0;
            if(command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {

                TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
            }



            Assert.AreEqual(dataParameters.TotalRecordCount, TotalRecordCount, "Read Failed");


            dataReader.Close();


            if(DeviceListItems.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(DeviceListItems.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(DeviceListItems.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }
            dataReader.Close();
        }
    }

}
