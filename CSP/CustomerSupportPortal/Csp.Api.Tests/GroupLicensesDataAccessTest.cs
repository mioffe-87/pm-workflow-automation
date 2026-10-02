using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests
{
    /// <summary>
    /// Summary description for GroupLicensesDataAccessTest
    /// </summary>
    [TestClass]
    public class GroupLicensesDataAccessTest:DBConnections 
    {
        
        GroupLicensesDataAccess groupLicensesDataAccess = new GroupLicensesDataAccess();
        DataParameters dataParameters = new DataParameters();
        List<License> LicenseList;
        License License = new License();
        long groupID = 1;
        long Createdby = 1;

        [TestMethod]
        public void GroupLicenseCreate() {

            groupLicensesDataAccess.Create(groupID, 2, Createdby);
            //Assert.IsNotNull(License, "Create Failed");
            //Assert.AreEqual(License.Id,2,"Create Failed");
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID + " and LicenseId=2 ");
            IDataReader dataReader = Connection.ExecuteReader(command);

            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Create Failed");
            dataReader.Close();
        }

        
        [TestMethod]
        public void GroupLicenseDelete()
        {
            groupLicensesDataAccess.Delete(groupID, 2,0);
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID + " and LicenseId=2 ");
            IDataReader dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(0, Convert.ToInt32(dataReader["count"]), "Delete Failed");
            dataReader.Close();
        }
     }
      
    
}
