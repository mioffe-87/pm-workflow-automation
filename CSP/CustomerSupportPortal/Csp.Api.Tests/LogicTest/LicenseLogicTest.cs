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
    public class LicenseLogicTest:DBConnections
    {
        [TestMethod]
        public void GetLicenseTest() {
            long licenseId = 1;
            Role role = new Role();
            License license = new License();
            DbCommand command = null;
            LicenseLogic licenseLogic = new LicenseLogic();
            command = Connection.GetSqlStringCommand("select LicenseId from license order by LicenseId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                licenseId = (long)dataReader["LicenseId"];
            dataReader.Close();

            license = licenseLogic.GetLicense(licenseId);

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

            command = Connection.GetSqlStringCommand("Select * from license where LicenseId=" + licenseId);
            dataReader = Connection.ExecuteReader(command);

            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , "Read Failed");
                
                Assert.AreEqual(license.CreatedByUserAccountId,
                    (dataReader["CreatedByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["CreatedByUserAccountId"]) : 0)
                    , "Read Failed");
               
                Assert.AreEqual(license.LastUpdateByUserAccountId,
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]) : 0)
                    , "Read Failed");
                
                Assert.AreEqual(license.LicenseKey, 
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]):"")
                    , "Read Failed");
                Assert.AreEqual(license.Id, 
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LicenseId"]):0)
                    , "Read   Failed");
                Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , " Code   Failed");
                Assert.AreEqual(license.LicenseKey,
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                    , "License Key   Failed");
                
                Assert.AreEqual(license.DeviceId,
                    (dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.EffectiveDate,
                    (dataReader["EffectiveDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["EffectiveDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.RegistrationDate,
                    (dataReader["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["RegistrationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.ExpirationDate,
                    (dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.GracePeriodDays,
                    (dataReader["GracePeriodDays"] != DBNull.Value ? Convert.ToInt16(dataReader["GracePeriodDays"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.LicenseTermMonths,
                    (dataReader["LicenseTermMonths"] != DBNull.Value ? Convert.ToInt16(dataReader["LicenseTermMonths"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ExtensionRequestSalesforceId,
                    (dataReader["ExtensionRequestSalesforceId"] != DBNull.Value ? Convert.ToInt64(dataReader["ExtensionRequestSalesforceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ParentLicenseId,
                    (dataReader["ParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["ParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.BundleLicenseId,
                    (dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipDate,
                    (dataReader["ShipDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ShipDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.IsActive,
                    (dataReader["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsActive"]) : false), "Read   Failed");
                Assert.AreEqual(license.Notes,
                    (dataReader["Notes"] != DBNull.Value ? Convert.ToString(dataReader["Notes"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipmentNumber,
                    (dataReader["ShipmentNumber"] != DBNull.Value ? Convert.ToInt64(dataReader["ShipmentNumber"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.IsSAPSynched,
                    (dataReader["IsSAPSynched"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsSAPSynched"]) : false)
                    , "Read   Failed");
                Assert.AreEqual(license.SiteLicenseSupportAccountId,
                    (dataReader["SiteLicenseSupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SiteLicenseSupportAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.City,
                    (dataReader["City"] != DBNull.Value ? Convert.ToString(dataReader["City"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.PostalCode,
                    (dataReader["PostalCode"] != DBNull.Value ? Convert.ToString(dataReader["PostalCode"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.BillToCountryId,
                    (dataReader["BillToCountryId"] != DBNull.Value ? Convert.ToInt64(dataReader["BillToCountryId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMQuantity,
                    (dataReader["VMQuantity"] != DBNull.Value ? Convert.ToInt16(dataReader["VMQuantity"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMStubUserAccountId,
                    (dataReader["VMStubUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMStubUserAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMParentLicenseId,
                    (dataReader["VMParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMUseCount,
                    (dataReader["VMUseCount"] != DBNull.Value ? Convert.ToInt16(dataReader["VMUseCount"]) : 0)
                    , "Read   Failed");
           
            }
            else {
                Assert.IsNotNull(license, "Read Failed");
            }
            dataReader.Close();

           
                
        }

        [TestMethod]
        public void GetGroupLicensesTest() {
            long groupID = 0;
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            List<License> LicenseList = new List<License>();
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();
            LicenseList = licenseLogic.GetGroupLicenses(groupID, dataParameters);
            //Assert.IsNotNull(LicenseList, "Read Failed");
            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID);
            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(LicenseList.Count, Convert.ToInt32(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(LicenseList.Count, 0, "Read Failed");
            }

            dataReader.Close();
            foreach (License license in LicenseList) {
                command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID + " and LicenseId=" + license.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }

            foreach(License license in LicenseList) {
                command = Connection.GetSqlStringCommand("select * from  License where LicenseId=" + license.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , "Read Failed");

                Assert.AreEqual(license.CreatedByUserAccountId,
                    (dataReader["CreatedByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["CreatedByUserAccountId"]) : 0)
                    , "Read Failed");

                Assert.AreEqual(license.LastUpdateByUserAccountId,
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]) : 0)
                    , "Read Failed");

                Assert.AreEqual(license.LicenseKey,
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                    , "Read Failed");
                Assert.AreEqual(license.Id,
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , " Code   Failed");
                Assert.AreEqual(license.LicenseKey,
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                    , "License Key   Failed");
              
                Assert.AreEqual(license.DeviceId,
                    (dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.EffectiveDate,
                    (dataReader["EffectiveDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["EffectiveDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.RegistrationDate,
                    (dataReader["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["RegistrationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.ExpirationDate,
                    (dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.GracePeriodDays,
                    (dataReader["GracePeriodDays"] != DBNull.Value ? Convert.ToInt16(dataReader["GracePeriodDays"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.LicenseTermMonths,
                    (dataReader["LicenseTermMonths"] != DBNull.Value ? Convert.ToInt16(dataReader["LicenseTermMonths"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ExtensionRequestSalesforceId,
                    (dataReader["ExtensionRequestSalesforceId"] != DBNull.Value ? Convert.ToInt64(dataReader["ExtensionRequestSalesforceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ParentLicenseId,
                    (dataReader["ParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["ParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.BundleLicenseId,
                    (dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipDate,
                    (dataReader["ShipDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ShipDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.IsActive,
                    (dataReader["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsActive"]) : false), "Read   Failed");
                Assert.AreEqual(license.Notes,
                    (dataReader["Notes"] != DBNull.Value ? Convert.ToString(dataReader["Notes"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipmentNumber,
                    (dataReader["ShipmentNumber"] != DBNull.Value ? Convert.ToInt64(dataReader["ShipmentNumber"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.IsSAPSynched,
                    (dataReader["IsSAPSynched"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsSAPSynched"]) : false)
                    , "Read   Failed");
                Assert.AreEqual(license.SiteLicenseSupportAccountId,
                    (dataReader["SiteLicenseSupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SiteLicenseSupportAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.City,
                    (dataReader["City"] != DBNull.Value ? Convert.ToString(dataReader["City"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.PostalCode,
                    (dataReader["PostalCode"] != DBNull.Value ? Convert.ToString(dataReader["PostalCode"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.BillToCountryId,
                    (dataReader["BillToCountryId"] != DBNull.Value ? Convert.ToInt64(dataReader["BillToCountryId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMQuantity,
                    (dataReader["VMQuantity"] != DBNull.Value ? Convert.ToInt16(dataReader["VMQuantity"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMStubUserAccountId,
                    (dataReader["VMStubUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMStubUserAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMParentLicenseId,
                    (dataReader["VMParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMUseCount,
                    (dataReader["VMUseCount"] != DBNull.Value ? Convert.ToInt16(dataReader["VMUseCount"]) : 0)
                    , "Read   Failed");
           
                dataReader.Close();
            }


        }

        [TestMethod]
        public void GetGroupLicensesPaginationTest() {
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            long groupID = 0;
            List<License> LicenseList = new List<License>();
            DbCommand command = null;

            command = Connection.GetSqlStringCommand("select groupID from csp.SupportAccountGroup order by groupID desc limit 0,1;");

            IDataReader dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                if (dataReader.Read()) {
                    groupID = Convert.ToInt64(dataReader["groupID"]);

                }
            }

            dataReader.Close();
            LicenseList = licenseLogic.GetGroupLicenses(groupID, dataParameters);

            Assert.IsNotNull(LicenseList, "Read Failed with Pagination");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from groupLicenses where groupID=" + groupID);
            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(LicenseList.Count, 0, "Read Failed");
            }


            if (LicenseList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(LicenseList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(LicenseList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");
            }

            foreach(License license in LicenseList) {
                command = Connection.GetSqlStringCommand("select * from  License where LicenseId=" + license.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(license.AuthorizationCode,
                     (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                     , "Read Failed");

                Assert.AreEqual(license.CreatedByUserAccountId,
                    (dataReader["CreatedByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["CreatedByUserAccountId"]) : 0)
                    , "Read Failed");

                Assert.AreEqual(license.LastUpdateByUserAccountId,
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]) : 0)
                    , "Read Failed");

                Assert.AreEqual(license.LicenseKey,
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                    , "Read Failed");
                Assert.AreEqual(license.Id,
                    (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , " Code   Failed");
                Assert.AreEqual(license.LicenseKey,
                    (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                    , "License Key   Failed");
              
                Assert.AreEqual(license.DeviceId,
                    (dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.EffectiveDate,
                    (dataReader["EffectiveDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["EffectiveDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.RegistrationDate,
                    (dataReader["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["RegistrationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.ExpirationDate,
                    (dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.GracePeriodDays,
                    (dataReader["GracePeriodDays"] != DBNull.Value ? Convert.ToInt16(dataReader["GracePeriodDays"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.LicenseTermMonths,
                    (dataReader["LicenseTermMonths"] != DBNull.Value ? Convert.ToInt16(dataReader["LicenseTermMonths"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ExtensionRequestSalesforceId,
                    (dataReader["ExtensionRequestSalesforceId"] != DBNull.Value ? Convert.ToInt64(dataReader["ExtensionRequestSalesforceId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ParentLicenseId,
                    (dataReader["ParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["ParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.BundleLicenseId,
                    (dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipDate,
                    (dataReader["ShipDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ShipDate"]) : new DateTime())
                    , "Read   Failed");
                Assert.AreEqual(license.IsActive,
                    (dataReader["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsActive"]) : false), "Read   Failed");
                Assert.AreEqual(license.Notes,
                    (dataReader["Notes"] != DBNull.Value ? Convert.ToString(dataReader["Notes"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.ShipmentNumber,
                    (dataReader["ShipmentNumber"] != DBNull.Value ? Convert.ToInt64(dataReader["ShipmentNumber"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.IsSAPSynched,
                    (dataReader["IsSAPSynched"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsSAPSynched"]) : false)
                    , "Read   Failed");
                Assert.AreEqual(license.SiteLicenseSupportAccountId,
                    (dataReader["SiteLicenseSupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SiteLicenseSupportAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.City,
                    (dataReader["City"] != DBNull.Value ? Convert.ToString(dataReader["City"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.PostalCode,
                    (dataReader["PostalCode"] != DBNull.Value ? Convert.ToString(dataReader["PostalCode"]) : null)
                    , "Read   Failed");
                Assert.AreEqual(license.BillToCountryId,
                    (dataReader["BillToCountryId"] != DBNull.Value ? Convert.ToInt64(dataReader["BillToCountryId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMQuantity,
                    (dataReader["VMQuantity"] != DBNull.Value ? Convert.ToInt16(dataReader["VMQuantity"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMStubUserAccountId,
                    (dataReader["VMStubUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMStubUserAccountId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMParentLicenseId,
                    (dataReader["VMParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMParentLicenseId"]) : 0)
                    , "Read   Failed");
                Assert.AreEqual(license.VMUseCount,
                    (dataReader["VMUseCount"] != DBNull.Value ? Convert.ToInt16(dataReader["VMUseCount"]) : 0)
                    , "Read   Failed");
           
                dataReader.Close();
            }

        }

     
        [TestMethod]
        public void GetSupportAccountLicensesPaginationTest() {
            List<License> licenses = new List<License>();
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;

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
            licenses = licenseLogic.GetSupportAccountLicenses(supportAccountId, dataParameters);

            Assert.IsNotNull(licenses, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from License where SupportAccountId=" + supportAccountId);
            dataReader = Connection.ExecuteReader(command);


            if (dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(licenses.Count, 0, "Read Failed");
            }

            dataReader.Close();


            if (licenses.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(licenses.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(licenses.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }

            foreach (License license in licenses) {
                command = Connection.GetSqlStringCommand("select count(1) as count from License where SupportAccountId=" + supportAccountId + " and LicenseID=" + license.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }
            foreach(License license in licenses) {
                command = Connection.GetSqlStringCommand("select * from  License where LicenseId=" + license.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();
                Assert.AreEqual(license.AuthorizationCode, Convert.ToString(dataReader["AuthorizationCode"]), "Read Failed");
                if(dataReader["CreatedByUserAccountId"] != DBNull.Value)
                    Assert.AreEqual(license.CreatedByUserAccountId, Convert.ToInt64(dataReader["CreatedByUserAccountId"]), "Read Failed");
                if(dataReader["LastUpdateByUserAccountId"] != DBNull.Value)
                    Assert.AreEqual(license.LastUpdateByUserAccountId, Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]), "Read Failed");
                Assert.AreEqual(license.LicenseKey, Convert.ToString(dataReader["LicenseKey"]), "Read Failed");
                dataReader.Close();
            }

        }

        [TestMethod]
        public void GetDeviceLicensesTest() {
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            List<LicenseListItem> licenseList = new List<LicenseListItem>();
            DbCommand command = null;
            long deviceId = 0;
            command = Connection.GetSqlStringCommand("select DeviceId from device order by DeviceId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            licenseList = licenseLogic.GetDeviceLicenses(deviceId, dataParameters);

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
        public void GetDeviceLicensesWithPaginationTest() {
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            List<LicenseListItem> licenseList = new List<LicenseListItem>();
            DbCommand command = null;

            long deviceId = 1;
            dataParameters.PaginationCount = 10;
            dataParameters.PaginationOffset = 0;
            command = Connection.GetSqlStringCommand("select DeviceId from device  order by DeviceId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                deviceId = (long)dataReader["DeviceId"];
            licenseList = licenseLogic.GetDeviceLicenses(deviceId, dataParameters);
            Assert.IsNotNull(licenseList, "Read Failed");
            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
           

            if (licenseList.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(licenseList.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }
            if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(licenseList.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset), "Read Failed with Pagination");
            }

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
        public void UpdateLicenseTest() {
            long licenseId = 1;
            DbCommand command = null;
            DataParameters dataParameters = new DataParameters();
            LicenseLogic licenseLogic = new LicenseLogic();
            List<License> licenseList = new List<License>();
            License license = new License();
            long PartMasterId = 0;
            long DeviceId = 0;
            long ParentLicenseId = 0;
            command = Connection.GetSqlStringCommand("select LicenseId from license order by LicenseId desc limit 0,1;");
            IDataReader dataReader = Connection.ExecuteReader(command);
            if (dataReader.Read())
                licenseId = (long)dataReader["LicenseId"];
            dataReader.Close();
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select PartMasterId from partmaster order by PartMasterId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                PartMasterId = (long)dataReader["PartMasterId"];
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select DeviceId from Device order by DeviceId desc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                DeviceId = (long)dataReader["DeviceId"];
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select LicenseId from License order by LicenseId asc limit 0,1;");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                ParentLicenseId = (long)dataReader["LicenseId"];
            dataReader.Close();
            long BillToCountryId = 0;
            command = Connection.GetSqlStringCommand("select CountryId from Country order by CountryId asc limit 1");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                BillToCountryId = (long)dataReader["CountryId"];
            dataReader.Close();

            long VMStubUserAccountId = 0;
            command = Connection.GetSqlStringCommand("select UserAccountId from UserAccount order by UserAccountId asc limit 1");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                VMStubUserAccountId = (long)dataReader["UserAccountId"];
            dataReader.Close();
            long SiteLicenseSupportAccountId = 0;

                 command = Connection.GetSqlStringCommand("select SupportAccountId from SupportAccount order by SupportAccountId asc limit 1");
            dataReader = Connection.ExecuteReader(command);
            if(dataReader.Read())
                SiteLicenseSupportAccountId = (long)dataReader["SupportAccountId"];
            dataReader.Close();

            license = licenseLogic.GetLicense(licenseId);
            Assert.AreEqual(license.Id, licenseId, "Read for Update Failed");
            license.Id = licenseId;
            license.AuthorizationCode = "Updated Code";
            license.LicenseKey = "Updated Key";
            license.LastUpdateByUserAccountId = 1;
          
            license.DeviceId = DeviceId;
            license.EffectiveDate = DateTime.Now;
            license.RegistrationDate = DateTime.Now;
            license.ExpirationDate = DateTime.Now;
            license.GracePeriodDays =11;
            license.LicenseTermMonths = 11;
            license.ExtensionRequestSalesforceId = 11;
            license.ParentLicenseId = ParentLicenseId;
            license.BundleLicenseId = ParentLicenseId;
            license.ShipDate = DateTime.Now;
            license.IsActive = true;
            license.Notes = "Testing notes";
            license.ShipmentNumber = 11111;
            license.IsSAPSynched = true;
            license.SiteLicenseSupportAccountId = SiteLicenseSupportAccountId;
            license.City = "New York";
            license.PostalCode = "53266";
            license.BillToCountryId = BillToCountryId;
            license.VMQuantity = 12;
            license.VMStubUserAccountId = VMStubUserAccountId;
            license.VMParentLicenseId = ParentLicenseId;
            license.VMUseCount = 12;

            license = licenseLogic.UpdateLicense(license);
            Assert.IsNotNull(license, "Update Failed");
            Assert.AreEqual(license.Id, licenseId, "Read for Update Failed");
            Assert.AreEqual(license.AuthorizationCode, "Updated Code", " Code for Update Failed");
            Assert.AreEqual(license.LicenseKey, "Updated Key","License Key for Update Failed");
            command = Connection.GetSqlStringCommand("select count(1) as count from license where LicenseId=" + licenseId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Update Failed");
            dataReader.Close();

            command = Connection.GetSqlStringCommand("select * from license where LicenseId=" + licenseId + " ");
            dataReader = Connection.ExecuteReader(command);
            dataReader.Read();
            Assert.AreEqual(license.AuthorizationCode,
                    (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                    , "Read Failed");

            Assert.AreEqual(license.CreatedByUserAccountId,
                (dataReader["CreatedByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["CreatedByUserAccountId"]) : 0)
                , "Read Failed");

            Assert.AreEqual(license.LastUpdateByUserAccountId,
                (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LastUpdateByUserAccountId"]) : 0)
                , "Read Failed");

            Assert.AreEqual(license.LicenseKey,
                (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                , "Read Failed");
            Assert.AreEqual(license.Id,
                (dataReader["LastUpdateByUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["LicenseId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.AuthorizationCode,
                (dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : "")
                , " Code   Failed");
            Assert.AreEqual(license.LicenseKey,
                (dataReader["LicenseKey"] != DBNull.Value ? Convert.ToString(dataReader["LicenseKey"]) : "")
                , "License Key   Failed");
           
            Assert.AreEqual(license.DeviceId,
                (dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.EffectiveDate,
                (dataReader["EffectiveDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["EffectiveDate"]) : new DateTime())
                , "Read   Failed");
            Assert.AreEqual(license.RegistrationDate,
                (dataReader["RegistrationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["RegistrationDate"]) : new DateTime())
                , "Read   Failed");
            Assert.AreEqual(license.ExpirationDate,
                (dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]) : new DateTime())
                , "Read   Failed");
            Assert.AreEqual(license.GracePeriodDays,
                (dataReader["GracePeriodDays"] != DBNull.Value ? Convert.ToInt16(dataReader["GracePeriodDays"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.LicenseTermMonths,
                (dataReader["LicenseTermMonths"] != DBNull.Value ? Convert.ToInt16(dataReader["LicenseTermMonths"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.ExtensionRequestSalesforceId,
                (dataReader["ExtensionRequestSalesforceId"] != DBNull.Value ? Convert.ToInt64(dataReader["ExtensionRequestSalesforceId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.ParentLicenseId,
                (dataReader["ParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["ParentLicenseId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.BundleLicenseId,
                (dataReader["BundleLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["BundleLicenseId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.ShipDate,
                (dataReader["ShipDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ShipDate"]) : new DateTime())
                , "Read   Failed");
            Assert.AreEqual(license.IsActive,
                (dataReader["IsActive"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsActive"]) : false), "Read   Failed");
            Assert.AreEqual(license.Notes,
                (dataReader["Notes"] != DBNull.Value ? Convert.ToString(dataReader["Notes"]) : null)
                , "Read   Failed");
            Assert.AreEqual(license.ShipmentNumber,
                (dataReader["ShipmentNumber"] != DBNull.Value ? Convert.ToInt64(dataReader["ShipmentNumber"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.IsSAPSynched,
                (dataReader["IsSAPSynched"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsSAPSynched"]) : false)
                , "Read   Failed");
            Assert.AreEqual(license.SiteLicenseSupportAccountId,
                (dataReader["SiteLicenseSupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SiteLicenseSupportAccountId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.City,
                (dataReader["City"] != DBNull.Value ? Convert.ToString(dataReader["City"]) : null)
                , "Read   Failed");
            Assert.AreEqual(license.PostalCode,
                (dataReader["PostalCode"] != DBNull.Value ? Convert.ToString(dataReader["PostalCode"]) : null)
                , "Read   Failed");
            Assert.AreEqual(license.BillToCountryId,
                (dataReader["BillToCountryId"] != DBNull.Value ? Convert.ToInt64(dataReader["BillToCountryId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.VMQuantity,
                (dataReader["VMQuantity"] != DBNull.Value ? Convert.ToInt16(dataReader["VMQuantity"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.VMStubUserAccountId,
                (dataReader["VMStubUserAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMStubUserAccountId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.VMParentLicenseId,
                (dataReader["VMParentLicenseId"] != DBNull.Value ? Convert.ToInt64(dataReader["VMParentLicenseId"]) : 0)
                , "Read   Failed");
            Assert.AreEqual(license.VMUseCount,
                (dataReader["VMUseCount"] != DBNull.Value ? Convert.ToInt16(dataReader["VMUseCount"]) : 0)
                , "Read   Failed");
           
            dataReader.Close();

        }
    }
}
