using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Configuration;
using System.Data.Common;
using System.Data;


namespace Csp.Api.Tests {
    /// <summary>
    ///     Summary description for GroupDevicesDataAccessTest
    /// </summary>
    [TestClass]
    public class GroupDevicesDataAccessTest : DBConnections {

        GroupDevicesDataAccess groupDevicesDataAccess = new GroupDevicesDataAccess();
        DataParameters dataParameters = new DataParameters();
        List<Device> deviceList;
        Device device = new Device();
        long groupID = 1;
        long Createdby = 1;

        [TestMethod]
        public void GroupDeviceCreate() {

            groupDevicesDataAccess.Create(groupID, 2, Createdby);
            //Assert.IsNotNull(device, "Create Failed");
            //Assert.AreEqual(device.Id,2,"Create Failed");
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where groupID=" + groupID + " and deviceId=2 ");
            IDataReader dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

       

        [TestMethod]
        public void GroupDeviceDelete() {
            groupDevicesDataAccess.Delete(groupID, 2, 0);
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupdevices where groupID=" + groupID + " and deviceId=2 ");
            IDataReader dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
    }

}
