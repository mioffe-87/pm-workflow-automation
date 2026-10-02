using System;
using System.Collections.Generic;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.Tests;
using System.Data;
using System.Data.Common;


namespace Csp.Api.Tests {

    [TestClass]
   public class LicenseDataAccessTest:DBConnections {
       List<Role> roleList = new List<Role>();
       LicenseDataAccess licenseDataAccess = new LicenseDataAccess();
       DataParameters dataParameters = new DataParameters();
     
        /// <summary>
        /// 
        /// </summary>
        /// 
      [TestMethod]
       public void LicenseDataAccessReadByID() {
           long licenseId = 1;
           Role role = new Role();
           License license = new License();
           DbCommand command = null;
           IDataReader dataReader;
           command = Connection.GetSqlStringCommand("select LicenseId from license order by LicenseId asc limit 1");
            dataReader = Connection.ExecuteReader(command);
           if (dataReader.Read())
               licenseId = (long)dataReader["LicenseId"];
           dataReader.Close();
           license = licenseDataAccess.Read(licenseId);

           command = Connection.GetSqlStringCommand("Select Count(1) as count from license where LicenseId=" + licenseId);
           dataReader = Connection.ExecuteReader(command);

           if (dataReader != null) {
               dataReader.Read();
               Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");
           }
           else {
               Assert.IsNotNull(license, "Read Failed");
           }
           dataReader.Close();
       }

      /// <summary>
      /// 
      /// </summary>
      [TestMethod]
      public void LicenseDataAccessUpdate() {
          DbCommand command = null;
          IDataReader dataReader;
          Role role = new Role();
          License license = new License();
          long licenseId = 1;
          command = Connection.GetSqlStringCommand("select LicenseId from license order by LicenseId asc limit 1");
          dataReader = Connection.ExecuteReader(command);
          if (dataReader.Read())
              licenseId = (long)dataReader["LicenseId"];
          dataReader.Close();

          license = licenseDataAccess.Read(licenseId);
          license.AuthorizationCode = "Update TestCode";
          license.CreatedByUserAccountId = 1;
          license.SupportProvider.Id = 1;
          license.Customer.Id = 1;
          license.Id = licenseId;
          license.LastUpdateByUserAccountId = 1;
          license.LicenseKey = "test6758";

          license = licenseDataAccess.Update(license);

          command = Connection.GetSqlStringCommand("Select * from license where LicenseId=" + license.Id);
          dataReader = Connection.ExecuteReader(command);

          if (dataReader.Read()) {
              Assert.AreEqual(license.Id, (long)dataReader["LicenseId"], "Update Failed");
              Assert.AreEqual(license.LastUpdateByUserAccountId, (long)dataReader["LastUpdateByUserAccountId"], "Update Failed");
              Assert.AreEqual(license.LicenseKey, dataReader["LicenseKey"], "Update Failed");
          }
          else {
              Assert.Fail();
          }

          dataReader.Close();
      }

      [TestMethod]
      public void ReadSupportAccountLicensesPagination() {
          List<License> licenses = new List<License>();
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
          licenses = licenseDataAccess.ReadSupportAccountLicenses(supportAccountId, dataParameters);

          Assert.IsNotNull(licenses, "Read Failed with Pagination");

          Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

          command = null;

          command = Connection.GetSqlStringCommand("select count(1) as count from License where SupportAccountId=" + supportAccountId);
          dataReader = Connection.ExecuteReader(command);


          if(dataReader != null) {
              dataReader.Read();
              Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
          }
          else {
              Assert.AreEqual(licenses.Count, 0, "Read Failed");
          }

          dataReader.Close();



          if(licenses.Count > dataParameters.PaginationCount) {
              Assert.Fail("Read Failed with Pagination");
          }

          if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(licenses.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
          }

          if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(licenses.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

          }



          foreach(License license in licenses) {
              command = Connection.GetSqlStringCommand("select count(1) as count from License where SupportAccountId=" + supportAccountId + " and LicenseID=" + license.Id);
              dataReader = Connection.ExecuteReader(command);
              dataReader.Read();

              Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

              dataReader.Close();
          }


      }
      [TestMethod]
      public void GroupLicenseRead() {
          long groupID = 0;
          List<License> LicenseList = new List<License>();
          DbCommand command = null;

          command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

          IDataReader dataReader = Connection.ExecuteReader(command);


          if(dataReader != null) {
              if(dataReader.Read()) {
                  groupID = Convert.ToInt64(dataReader["groupID"]);

              }
          }

          dataReader.Close();
          LicenseList = licenseDataAccess.ReadGroupLicenses(groupID, dataParameters);
          //Assert.IsNotNull(LicenseList, "Read Failed");
           command = null;

          command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID);
           dataReader = Connection.ExecuteReader(command);


          if(dataReader != null) {
              dataReader.Read();
              Assert.AreEqual(LicenseList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
          }
          else {
              Assert.AreEqual(LicenseList.Count, 0, "Read Failed");
          }

          dataReader.Close();
          foreach(License License in LicenseList) {
              command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID + " and LicenseId=" + License.Id);
              dataReader = Connection.ExecuteReader(command);
              dataReader.Read();
              Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

              dataReader.Close();
          }




      }

      [TestMethod]
      public void GroupLicenseReadPagination() {

          dataParameters.PaginationCount = 10;
          dataParameters.PaginationOffset = 0;
          long groupID = 0;
          List<License> LicenseList = new List<License>();
          DbCommand command = null;

          command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

          IDataReader dataReader = Connection.ExecuteReader(command);


          if(dataReader != null) {
              if(dataReader.Read()) {
                  groupID = Convert.ToInt64(dataReader["groupID"]);

              }
          }

          dataReader.Close();
          LicenseList = licenseDataAccess.ReadGroupLicenses(groupID, dataParameters);

          Assert.IsNotNull(LicenseList, "Read Failed with Pagination");
          Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
           command = null;

          command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID);
           dataReader = Connection.ExecuteReader(command);


          if(dataReader != null) {
              dataReader.Read();
              Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
          }
          else {
              Assert.AreEqual(LicenseList.Count, 0, "Read Failed");
          }


          if(LicenseList.Count > dataParameters.PaginationCount) {
              Assert.Fail("Read Failed with Pagination");
          }

          if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(LicenseList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
          }

          if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(LicenseList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");
          }

      }

      [TestMethod]
      public void DefaultDeviceLicenseReadTest() {
          dataParameters = new DataParameters();

          List<LicenseListItem> licenseList = new List<LicenseListItem>();
          DbCommand command = null;
          long deviceId = 0;
          command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId asc limit 1");
          IDataReader dataReader = Connection.ExecuteReader(command);
          if(dataReader.Read())
              deviceId = (long)dataReader["DeviceId"];
          licenseList = licenseDataAccess.ReadDeviceLicenses(deviceId, dataParameters);
          command = Connection.GetStoredProcCommand("GetLicensesByDeviceId");
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
              while(dataReader.Read()) {
                  if(dataReader["LicenseId"] != DBNull.Value)
                      if(licenseList.Find(x => x.Id == Convert.ToInt64(dataReader["LicenseId"])) == null) {
                          Assert.Fail("Read Fail");
                      }
                  if(dataReader["LicenseKey"] != DBNull.Value)
                      if(licenseList.Find(x => x.LicenseKey == Convert.ToString(dataReader["LicenseKey"])) == null) {
                          Assert.Fail("Read Fail");
                      }
                  if(dataReader["LicenseName"] != DBNull.Value)
                      if(licenseList.Find(x => x.LicenseName == Convert.ToString(dataReader["LicenseName"])) == null) {
                          Assert.Fail("Read Fail");
                      }

                  if(dataReader["AuthorizationCode"] != DBNull.Value)
                      if(licenseList.Find(x => x.AuthorizationCode == Convert.ToString(dataReader["AuthorizationCode"])) == null) {
                          Assert.Fail("Read Fail");
                      }
                  if(dataReader["ExpirationDate"] != DBNull.Value)
                      if(licenseList.Find(x => x.ExpirationDate == Convert.ToDateTime(dataReader["ExpirationDate"])) == null) {
                          Assert.Fail("Read Fail");
                      }

              }
          }
          else {
              Assert.AreEqual(licenseList.Count, 0, "Read Failed");
          }
            
      }

       

      [TestMethod]
      public void ReadDeviceLicenseWithPaginationTest() {
          DbCommand command = null;
          dataParameters = new DataParameters();

          List<LicenseListItem> licenseList = new List<LicenseListItem>();
          long deviceId = 1;
          dataParameters.PaginationCount = 10;
          dataParameters.PaginationOffset = 0;
          command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId desc limit 1");
          IDataReader dataReader = Connection.ExecuteReader(command);
          if(dataReader.Read())
              deviceId = (long)dataReader["DeviceId"];
          licenseList = licenseDataAccess.ReadDeviceLicenses(deviceId, dataParameters);
          Assert.IsNotNull(licenseList, "Read Failed");
          Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
          Assert.AreNotEqual(dataParameters.TotalRecordCount, 0, "Read Failed with Pagination");

          if(licenseList.Count > dataParameters.PaginationCount) {
              Assert.Fail("Read Failed with Pagination");
          }
          if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(licenseList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
          }
          if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
              Assert.AreEqual(licenseList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
          }
      }

    
    }
}
