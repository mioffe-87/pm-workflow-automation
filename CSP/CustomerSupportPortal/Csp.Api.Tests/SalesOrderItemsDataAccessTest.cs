using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.Model;
using Csp.Api.DataAccess;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests {
    /// <summary>
    /// Summary description for SalesOrderItemsDataAccessTest
    /// </summary>
    [TestClass]
    public class SalesOrderItemsDataAccessTest :DBConnections  {
        SalesOrderItemsDataAccess salesOrderItemsDataAccess = new SalesOrderItemsDataAccess();
        DataParameters dataParameters = new DataParameters();

        SalesOrderItem salesOrderItem = new SalesOrderItem();

        long salesorderitemId = 1;
        long salesorderId = 1;
        long Createdby = 1;

        [TestMethod]
        public void SalesOrderItemCreate() {

            //salesOrderItem = new SalesOrderItem();
            //salesOrderItem.Device.Id = 1;
            //salesOrderItem.CreatedByUserAccountId = Createdby;
            //salesOrderItem.EntitlementLicense.Id = 1;
            
           
            //salesOrderItemsDataAccess.Create(salesOrderItem, Createdby);


            ////Assert.IsNotNull(role, "Create Failed");
            ////Assert.AreEqual(role.Id, 2, "Create Failed");

            //DbCommand command = null;

            //command = Connection.GetSqlStringCommand("select * from salesorderitem where SalesOrderItemId=last_insert_id();");

            //IDataReader dataReader = Connection.ExecuteReader(command);


            //if (dataReader != null) {
            //    if (salesOrderItem != null) {
            //        dataReader.Read();
            //        Assert.AreEqual(salesOrderItem.Device.Id, Convert.ToInt32(dataReader["DeviceId"]), "Create Failed");
            //        Assert.AreEqual(salesOrderItem.EntitlementLicense.Id, Convert.ToInt32(dataReader["LicenseId"]), "Create Failed");
                    
            //    }
            //    else {
            //        Assert.Fail("Create Failed");
            //    }
            //}
            //else {
            //    Assert.Fail("Create Failed");
            //}

            //dataReader.Close();


        }

        [TestMethod]
        public void SalesOrderItemReadPagination() {
            //List<SalesOrderItem> salesOrderItems = new List<SalesOrderItem>();

            //dataParameters.PaginationCount = 10;
            //dataParameters.PaginationOffset = 0;
            //salesOrderItems = salesOrderItemsDataAccess.Read(salesorderId, dataParameters);

            //Assert.IsNotNull(salesOrderItems, "Read Failed with Pagination");

            //Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            //DbCommand command = null;

            //command = Connection.GetSqlStringCommand("select count(1) as count from salesorderitem where salesorderId="+ salesorderId);
            //IDataReader dataReader = Connection.ExecuteReader(command);


            //if(dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.AreEqual(salesOrderItems.Count, 0, "Read Failed");
            //}


            //if (salesOrderItems.Count > dataParameters.PaginationCount) {
            //    Assert.Fail("Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(salesOrderItems.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(salesOrderItems.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            //}



        }
    }
}
