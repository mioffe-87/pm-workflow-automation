using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Data;
using Csp.Api.DataAccess;
using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Logic;

namespace Csp.Api.Tests.LogicTest {

    [TestClass]
    public class DeviceLogicTest : DBConnections {
        long deviceId = 0;


        [TestMethod]
        public void UpdateDeviceTest() {
            DbCommand command = null;
            DataParameters dataParameters = new DataParameters();
            DeviceLogic deviceLogic = new DeviceLogic();
            List<Device> deviceList = new List<Device>();
            Device device = new Device();
            long BillToCountryId = 0;
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select CountryId from Country order by CountryId asc limit 1");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                BillToCountryId = (long)dataReader["CountryId"];
            dataReader.Close();

            device = deviceLogic.GetDevice(deviceId ,0);

            Assert.AreEqual(device.Id, deviceId, "Read for Update Failed");

            device.Id = deviceId;
            device.SerialNumber = "Test Serial Number";

            device.Licenses.Add(new License {
                Id = 1
            });
            device.UserAccount.Id = 1;
           
            device.DefaultRoles.Add(new Role {
                Id = 4
            });

            device.LastUpdateByUserAccountId = 1;
            device.RegistrationDate = DateTime.Now ;
            device.ShipDate = DateTime.Now;
            device.IsActive = true;
            device.ShipmentNumber = 555345;
            device.DeviceName = "TestDeviceName";
            device.Comments = "Testing Device Update";
            device.IsRMAReturn = false;
            device.IsRMAReplacement = true;
            device.SalesForceAccountId = "77365";
            device.CustomerServiceNotes = "Testing CustomerServiceNotes" ;
            device.IsSAPSynched = true;
            device.VirtualSystemsAvailable = 12;
            device.GracePeriodDays = 12;
            device.BillToCountryId = BillToCountryId;
            device.City = "New york";
            device.PostalCode = "42414";
            device.CountryId = BillToCountryId;
            device.IsVirtualMachine = true;
            device.CpuId = "ECu7a793";
            device.UuId = "U52677wq";
            device.MacAddress = "00:EC:07:56";

            device = deviceLogic.UpdateDevice(device,0);
            

            command = Connection.GetSqlStringCommand("select count(1) as count from device where DeviceId=" + deviceId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select * from device where DeviceId=" + deviceId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.IsNotNull(device, "Update Failed");
            Assert.AreEqual(device.Id, Convert.ToInt32(dataReader["DeviceId"]), "Read for Update Failed");
            Assert.AreEqual(device.SerialNumber, Convert.ToString(dataReader["SerialNumber"]), " Serial Number Update Failed");
            Assert.AreEqual(device.RegistrationDate, Convert.ToDateTime(dataReader["RegistrationDate"]), "Read for Update Failed");
            Assert.AreEqual(device.ShipDate, Convert.ToDateTime(dataReader["ShipDate"]), "Read for Update Failed");
            Assert.AreEqual(device.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Read for Update Failed");
            Assert.AreEqual(device.ShipmentNumber, Convert.ToInt64(dataReader["ShipmentNumber"]), "Read for Update Failed");
            Assert.AreEqual(device.DeviceName, Convert.ToString(dataReader["DeviceName"]), "Read for Update Failed");
            Assert.AreEqual(device.Comments, Convert.ToString(dataReader["Comments"]), "Read for Update Failed");
            Assert.AreEqual(device.IsRMAReturn, Convert.ToBoolean(dataReader["IsRMAReturn"]), "Read for Update Failed");
            Assert.AreEqual(device.IsRMAReplacement, Convert.ToBoolean(dataReader["IsRMAReplacement"]), "Read for Update Failed");
            Assert.AreEqual(device.SalesForceAccountId, Convert.ToString(dataReader["SalesForceAccountId"]), "Read for Update Failed");
            Assert.AreEqual(device.CustomerServiceNotes, Convert.ToString(dataReader["CustomerServiceNotes"]), "Read for Update Failed");
            Assert.AreEqual(device.IsSAPSynched, Convert.ToBoolean(dataReader["IsSAPSynched"]), "Read for Update Failed");
            Assert.AreEqual(device.VirtualSystemsAvailable, Convert.ToInt16(dataReader["VirtualSystemsAvailable"]), "Read for Update Failed");
            Assert.AreEqual(device.GracePeriodDays, Convert.ToInt16(dataReader["GracePeriodDays"]), "Read for Update Failed");
            Assert.AreEqual(device.BillToCountryId, Convert.ToInt64(dataReader["BillToCountryId"]), "Read for Update Failed");
            Assert.AreEqual(device.City, Convert.ToString(dataReader["City"]), "Read for Update Failed");
            Assert.AreEqual(device.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Read for Update Failed");
            Assert.AreEqual(device.CountryId, Convert.ToInt64(dataReader["CountryId"]), "Read for Update Failed");
            Assert.AreEqual(device.IsVirtualMachine, Convert.ToBoolean(dataReader["IsVirtualMachine"]), "Read for Update Failed");
            Assert.AreEqual(device.CpuId, Convert.ToString(dataReader["CpuId"]), "Read for Update Failed");
            Assert.AreEqual(device.UuId, Convert.ToString(dataReader["UuId"]), "Read for Update Failed");
            Assert.AreEqual(device.MacAddress, Convert.ToString(dataReader["MacAddress"]), "Read for Update Failed");
            dataReader.Close();


            device.Licenses.Add(new License {
                Id = 1
            });
            device.UserAccount.Id = 1;
            device.DefaultRoles = new List<Role>();
           
            device.DefaultRoles.Add(new Role {
                Id = 4
            });


            foreach (Role role in device.DefaultRoles) {
                command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and RoleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
                dataReader.Close();
            }
        }

        [TestMethod]
        public void GetDeviceTest() {
            DbCommand command = null;
            DeviceLogic deviceLogic = new DeviceLogic();
            Device device = new Device();
            long deviceId = 0;
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            dataReader.Close();
            device = deviceLogic.GetDevice(deviceId,0);
            command = Connection.GetSqlStringCommand("select count(1) as count from device where DeviceId=" + deviceId);
            dataReader = Connection.ExecuteReader(command);
            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(0, 0, "Read Failed");
            }
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select * from device where DeviceId=" + deviceId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.IsNotNull(device, "Update Failed");
            Assert.AreEqual(device.Id, Convert.ToInt32(dataReader["DeviceId"]), "Read  Failed");
            Assert.AreEqual(device.SerialNumber, Convert.ToString(dataReader["SerialNumber"]), " Serial Number Update Failed");
            Assert.AreEqual(device.RegistrationDate, Convert.ToDateTime(dataReader["RegistrationDate"]), "Read  Failed");
            Assert.AreEqual(device.ShipDate, Convert.ToDateTime(dataReader["ShipDate"]), "Read  Failed");
            Assert.AreEqual(device.IsActive, Convert.ToBoolean(dataReader["IsActive"]), "Read  Failed");
            Assert.AreEqual(device.ShipmentNumber, Convert.ToInt64(dataReader["ShipmentNumber"]), "Read  Failed");
            Assert.AreEqual(device.DeviceName, Convert.ToString(dataReader["DeviceName"]), "Read  Failed");
            Assert.AreEqual(device.Comments, Convert.ToString(dataReader["Comments"]), "Read  Failed");
            Assert.AreEqual(device.IsRMAReturn, Convert.ToBoolean(dataReader["IsRMAReturn"]), "Read  Failed");
            Assert.AreEqual(device.IsRMAReplacement, Convert.ToBoolean(dataReader["IsRMAReplacement"]), "Read  Failed");
            Assert.AreEqual(device.SalesForceAccountId, Convert.ToString(dataReader["SalesForceAccountId"]), "Read  Failed");
            Assert.AreEqual(device.CustomerServiceNotes, Convert.ToString(dataReader["CustomerServiceNotes"]), "Read  Failed");
            Assert.AreEqual(device.IsSAPSynched, Convert.ToBoolean(dataReader["IsSAPSynched"]), "Read  Failed");
            Assert.AreEqual(device.VirtualSystemsAvailable, Convert.ToInt16(dataReader["VirtualSystemsAvailable"]), "Read  Failed");
            Assert.AreEqual(device.GracePeriodDays, Convert.ToInt16(dataReader["GracePeriodDays"]), "Read  Failed");
            Assert.AreEqual(device.BillToCountryId, Convert.ToInt64(dataReader["BillToCountryId"]), "Read  Failed");
            Assert.AreEqual(device.City, Convert.ToString(dataReader["City"]), "Read  Failed");
            Assert.AreEqual(device.PostalCode, Convert.ToString(dataReader["PostalCode"]), "Read  Failed");
            Assert.AreEqual(device.CountryId, Convert.ToInt64(dataReader["CountryId"]), "Read  Failed");
            Assert.AreEqual(device.IsVirtualMachine, Convert.ToBoolean(dataReader["IsVirtualMachine"]), "Read  Failed");
            Assert.AreEqual(device.CpuId, Convert.ToString(dataReader["CpuId"]), "Read  Failed");
            Assert.AreEqual(device.UuId, Convert.ToString(dataReader["UuId"]), "Read  Failed");
            Assert.AreEqual(device.MacAddress, Convert.ToString(dataReader["MacAddress"]), "Read  Failed");
            dataReader.Close();
        }



        [TestMethod]
        public void GetSupportAccountDevicesTest() {
            List<DeviceListItem> DeviceListItems = new List<DeviceListItem>();
            DataParameters dataParameters = new DataParameters();
            DeviceLogic deviceLogic = new DeviceLogic();
            long prestrictedUserAccount = 0;
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


            if(dataReader != null) {
                if(dataReader.Read()) {
                    prestrictedUserAccount = Convert.ToInt64(dataReader["UserAccountId"]);

                }
            }
            dataReader.Close();

            DeviceListItems = deviceLogic.GetSupportAccountDevices(supportAccountId, prestrictedUserAccount, dataParameters);

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
            
        }



        [TestMethod]
        public void GetSupportAccountDevicesWithPaginationTest() {
            List<DeviceListItem> DeviceListItems = new List<DeviceListItem>();
            DataParameters dataParameters = new DataParameters();
            DeviceLogic deviceLogic = new DeviceLogic();
            long prestrictedUserAccount = 0;
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
            DeviceListItems = deviceLogic.GetSupportAccountDevices(supportAccountId, prestrictedUserAccount, dataParameters);

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
        public void GetGroupDevicesTest() {
            DataParameters dataParameters = new DataParameters();
            List<DeviceListItem> DeviceListItems = new List<DeviceListItem>();
            DeviceLogic deviceLogic = new DeviceLogic();
            long groupID = 0;
            long prestrictedUserAccount = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
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

            DeviceListItems = deviceLogic.GetGroupDevices(groupID, prestrictedUserAccount, dataParameters);
            //Assert.IsNotNull(deviceList, "Read Failed");
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
        public void GetGroupDevicesWithPaginationTest() {
            DeviceLogic deviceLogic = new DeviceLogic();
            List<DeviceListItem> DeviceListItems = new List<DeviceListItem>();
            DataParameters dataParameters = new DataParameters();
            long prestrictedUserAccount = 0;
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;

            long groupID = 0;
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
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

            DeviceListItems = deviceLogic.GetGroupDevices(groupID,prestrictedUserAccount, dataParameters);

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

        [TestMethod]
        public void CreateDeviceDefaultRoleTest() {

            DbCommand command = null;
            DeviceLogic deviceLogic = new DeviceLogic();
            List<Role> roleList = new List<Role>();
            DataParameters dataParameters = new DataParameters();
            long Createdby = 1;
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
               dataReader.Close();
            if (deviceId != 0) {
                deviceLogic.CreateDeviceDefaultRole(deviceId, 2, Createdby);
                roleList = deviceLogic.GetDeviceDefaultRoles(deviceId, dataParameters);
                Assert.IsNotNull(roleList, "Create Failed");
                
                command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and roleId=2 ");
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
                dataReader.Close();
            }
            else {
                Assert.Fail("No Device Found in the database to assign the Device");
            }
        }

        [TestMethod]
        public void DeleteDeviceDefaultRoleTest() {
            DbCommand command = null;
            long Deletedby = 0;
            DeviceLogic deviceLogic = new DeviceLogic();
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            dataReader.Close();
            if (deviceId != 0) {
                deviceLogic.DeleteDeviceDefaultRole(deviceId, 2, Deletedby);

                command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and roleId=2 ");
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
                dataReader.Close();
            }
            else {
                Assert.Fail("No Device Found in the database to Delete");
            }
        }

        [TestMethod]
        public void GetDeviceDefaultRolesTest() {
            DataParameters dataParameters = new DataParameters();
            List<Role> roleList = new List<Role>();
            DeviceLogic deviceLogic = new DeviceLogic();
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];

            dataReader.Close();
            roleList = deviceLogic.GetDeviceDefaultRoles(deviceId, dataParameters);
            //Assert.IsNotNull(deviceList, "Read Failed");
            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId);
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
                command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId + " and roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

            foreach (Role role in roleList) {
                command = Connection.GetSqlStringCommand("select  * from  devicedefaultroles where DeviceId=" + deviceId + " and roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                if (dataReader != null) {
                    while (dataReader.Read()) {
                        Assert.AreEqual(role.Id, dataReader["roleId"], "Read Failed");
                        Assert.AreEqual(deviceId, dataReader["deviceId"], "Read Failed");
                        Assert.AreEqual(role.CreatedByUserAccountId, dataReader["CreatedByUserAccountId"], "Read Failed");
                    }
                   
                }
                else {
                    Assert.AreEqual(0, 0, "Read Failed");
                }
            }


        }

        [TestMethod]
        public void GetDeviceDefaultRolesWithPaginationTest() {
            DeviceLogic deviceLogic = new DeviceLogic();
            List<Role> roleList = new List<Role>();
            DataParameters dataParameters = new DataParameters();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;

            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];

            dataReader.Close();
            roleList = deviceLogic.GetDeviceDefaultRoles(deviceId, dataParameters);

            foreach (Role role in roleList) {
                command = Connection.GetSqlStringCommand("select  * from  devicedefaultroles where DeviceId=" + deviceId + " and roleId=" + role.Id);
                dataReader = Connection.ExecuteReader(command);
                if (dataReader != null) {
                    while (dataReader.Read()) {
                        Assert.AreEqual(role.Id, dataReader["roleId"], "Read Failed");
                        Assert.AreEqual(deviceId, dataReader["deviceId"], "Read Failed");
                        Assert.AreEqual(role.CreatedByUserAccountId, dataReader["CreatedByUserAccountId"], "Read Failed");
                    }

                }
                else {
                    Assert.AreEqual(0, 0, "Read Failed");
                }
            }

            Assert.IsNotNull(roleList, "Read Failed with Pagination");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from devicedefaultroles where DeviceId=" + deviceId);
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

        }
    }
}
