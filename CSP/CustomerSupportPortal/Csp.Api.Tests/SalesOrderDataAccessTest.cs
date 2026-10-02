using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.DataAccess;
using Csp.Api.Model;
using System.Data.Common;
using System.Data;

namespace Csp.Api.Tests {
    /// <summary>
    /// Summary description for SalesOrderDataAccessTest
    /// </summary>
    [TestClass]
    public class SalesOrderDataAccessTest : DBConnections {
        SalesOrderDataAccess salesOrderDataAccess = new SalesOrderDataAccess();
        DataParameters dataParameters = new DataParameters();

        SalesOrder salesOrder = new SalesOrder();

        long supportAccountId = 1;
        long salesorderId = 1;
        long Createdby = 1;

        [TestMethod]
        public void SalesOrderCreate() {

            //salesOrder = new SalesOrder();
            //salesOrder.Customer.Id = 1;
            //salesOrder.CreatedByUserAccountId = Createdby;
            ////salesOrder.Items.Id = 1;


            //salesOrderDataAccess.Create(salesOrder, Createdby);


            ////Assert.IsNotNull(role, "Create Failed");
            ////Assert.AreEqual(role.Id, 2, "Create Failed");

            //DbCommand command = null;

            //command = Connection.GetSqlStringCommand("select * from salesorder where salesorderID=last_insert_id();");

            //IDataReader dataReader = Connection.ExecuteReader(command);


            //if (dataReader != null) {
            //    if (salesOrder != null) {
            //        dataReader.Read();
            //        Assert.AreEqual(salesOrder.Customer.Id, Convert.ToInt32(dataReader["SupportAccountId"]), "Create Failed");

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
            //List<SalesOrder> salesOrders = new List<SalesOrder>();

            //dataParameters.PaginationCount = 10;
            //dataParameters.PaginationOffset = 0;
            //salesOrders = salesOrderDataAccess.Read(supportAccountId, dataParameters);

            //Assert.IsNotNull(salesOrders, "Read Failed with Pagination");

            //Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");

            //DbCommand command = null;

            //command = Connection.GetSqlStringCommand("select count(1) as count from salesOrder where SupportAccountId=" + supportAccountId);
            //IDataReader dataReader = Connection.ExecuteReader(command);


            //if(dataReader != null) {
            //    dataReader.Read();
            //    Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            //}
            //else {
            //    Assert.AreEqual(salesOrders.Count, 0, "Read Failed");
            //}


            //if (salesOrders.Count > dataParameters.PaginationCount) {
            //    Assert.Fail("Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(salesOrders.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            //}

            //if (dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
            //    Assert.AreEqual(salesOrders.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            //}
        }

        [TestMethod]
        public void ReadSupportAccountSalesOrdersPagination() {
            List<SalesOrder> salesOrders = new List<SalesOrder>();
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
            salesOrders = salesOrderDataAccess.Read(supportAccountId, dataParameters);

            Assert.IsNotNull(salesOrders, "Read Failed with Pagination");

            Assert.IsNotNull(dataParameters.TotalRecordCount, "Read Failed with Pagination");
             command = null;

            command = Connection.GetSqlStringCommand("select count(1) as count from SalesOrder where SupportAccountId=" + supportAccountId);
             dataReader = Connection.ExecuteReader(command);


            if(dataReader != null) {
                dataReader.Read();
                Assert.AreEqual(dataParameters.TotalRecordCount, Convert.ToInt64(dataReader["count"]), "Read Failed");
            }
            else {
                Assert.AreEqual(salesOrders.Count, 0, "Read Failed");
            }

            dataReader.Close();

            if(salesOrders.Count > dataParameters.PaginationCount) {
                Assert.Fail("Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount >= (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(salesOrders.Count, dataParameters.PaginationCount, "Read Failed with Pagination");
            }

            if(dataParameters.TotalRecordCount < (dataParameters.PaginationOffset + dataParameters.PaginationCount - 1)) {
                Assert.AreEqual(salesOrders.Count, (dataParameters.TotalRecordCount - dataParameters.PaginationOffset + (dataParameters.PaginationOffset == 0 ? 0 : 1)), "Read Failed with Pagination");

            }




            foreach(SalesOrder salesOrder in salesOrders) {
                command = Connection.GetSqlStringCommand("select count(1) as count from SalesOrder where SupportAccountId=" + supportAccountId + " and SalesOrderID=" + salesOrder.Id);
                dataReader = Connection.ExecuteReader(command);
                dataReader.Read();

                Assert.AreEqual(1, Convert.ToInt32(dataReader["count"]), "Read Failed");

                dataReader.Close();
            }


        }

    }
}
