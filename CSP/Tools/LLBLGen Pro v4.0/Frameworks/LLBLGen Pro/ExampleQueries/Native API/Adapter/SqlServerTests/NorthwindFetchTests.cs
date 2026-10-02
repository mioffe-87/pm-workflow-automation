using System;
using System.ComponentModel;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Diagnostics;

using NUnit.Framework;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Northwind.Adapter;
using Northwind.Adapter.DatabaseSpecific;
using Northwind.Adapter.EntityClasses;
using Northwind.Adapter.FactoryClasses;
using Northwind.Adapter.HelperClasses;
using Northwind.Adapter.TypedListClasses;
using Northwind.Adapter.TypedViewClasses;
using System.Xml;
using SD.Tools.OrmProfiler.Interceptor;

//using Northwind.Adapter.VB;
//using Northwind.Adapter.VB.DatabaseSpecific;
//using Northwind.Adapter.VB.EntityClasses;
//using Northwind.Adapter.VB.FactoryClasses;
//using Northwind.Adapter.VB.HelperClasses;
//using Northwind.Adapter.VB.TypedListClasses;
//using Northwind.Adapter.VB.TypedViewClasses;

namespace Unittests.TestLibrary.SqlServerTests.Adapter
{
	/// <summary>
	/// NorthwindFetchTests. 
	/// Contains NUnit tests working on fetches from the Northwind database. 
	/// </summary>
	[TestFixture]
	public class NorthwindFetchTests
	{
		private Guid _testRunID;
		private bool _dereferenceTestFlag = false;


		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid();
			Console.WriteLine( "TestRunID: {0}", _testRunID.ToString() );
			Console.WriteLine( "RT version: {0}.{1}", RuntimeLibraryVersion.Version, RuntimeLibraryVersion.Build );
			SerializationHelper.Optimization = SerializationOptimization.Fast;
		}

		[TestFixtureTearDown]
		public void Teardown()
		{
		}


		[Test]
		public void BulkDeletesFromCollectionTest2()
		{
			var customers = new EntityCollection<CustomerEntity>();
			using(var adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}
			Assert.AreEqual(91, customers.Count);

			var toRemove = new List<CustomerEntity>();
			toRemove.Add(customers[0]);
			toRemove.Add(customers[90]);
			toRemove.Add(customers[32]);
			toRemove.Add(customers[4]);

			int listChangedCounter = 0;

			customers.ListChanged += (s, e) => { listChangedCounter++; };
			Assert.AreEqual(4, customers.RemoveRange(toRemove));
			Assert.AreEqual(1, listChangedCounter);
			Assert.AreEqual(87, customers.Count);
			foreach(var removed in toRemove)
			{
				Assert.IsFalse(customers.Contains(removed));
			}
		}


		[Test]
		public void FullJoinTest()
		{
			var fields = new ResultsetFields(2);
			fields.DefineField(EmployeeFields.EmployeeId.SetObjectAlias("E"), 0, "EID");
			fields.DefineField(EmployeeFields.EmployeeId.SetObjectAlias("M"), 1, "MID");
			var filter = new RelationPredicateBucket();
			filter.Relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "E", "M", JoinHint.Full);
			var sorter = new SortExpression(EmployeeFields.EmployeeId.SetObjectAlias("E") | SortOperator.Ascending);
			var results = new DataTable();
			using(var adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, filter, 0, sorter, true);
			}
			Assert.AreEqual(13, results.Rows.Count);
			Assert.AreEqual(1, results.Rows[4]["EID"]);
		}


		[Test]
		public void FillTypedViewWithProcCall()
		{
			OrdersOnCountryTypedView ordersTv = new OrdersOnCountryTypedView();
			CustomersOnCountryTypedView customersTv = new CustomersOnCountryTypedView();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedView(ordersTv, RetrievalProcedures.GetQueryForOrdersOnCountryTypedView("USA"));
				adapter.FetchTypedView(customersTv, RetrievalProcedures.GetQueryForCustomersOnCountryTypedView("USA"));
			}
			Assert.AreEqual(121, ordersTv.Count);
			Assert.AreEqual(13, customersTv.Count);
		}


		[Test]
		public void DynamicListWithFieldWithoutSourceAndExpressionTest()
		{
			ResultsetFields fields = new ResultsetFields(3);

			// use expression on two fields, then add it to a new entityfield instance so the dqe can't figure out what to target. 
			Expression exp = OrderFields.OrderId * OrderFields.EmployeeId;
			fields[0] = new EntityField2("Test", exp, typeof(int));
			fields[1] = OrderFields.OrderId;
			fields[2] = OrderFields.EmployeeId;

			DataTable results = new DataTable();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, null);
			}
			foreach(DataRow row in results.Rows)
			{
				Assert.AreEqual(row[0], (int)row[1] * (int)row[2]);
			}
		}


		[Test]
		public void CopyToCallOnInterfaceOfCollectionTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}
			Assert.AreEqual(91, customers.Count);
			IEntityCollection2 customersCollection = (IEntityCollection2)customers;
			IEntity2[] entityArray = new IEntity2[customers.Count];
			customersCollection.CopyTo(entityArray, 0);
			for(int i = 0; i < customers.Count; i++)
			{
				Assert.AreEqual(customers[i], entityArray[i]);
			}
		}


		[Test]
		public void ManyToManyWithSingleParentMultipleChildrenTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathEmployees);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, 1, null, path);
			}
			Assert.AreEqual(1, customers.Count);
			Assert.IsTrue(customers[0].Employees.Count > 1);
		}


		[Test]
		public void SortingEntityViewOn3Fields()
		{
			// fetch orders data
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>();

			IRelationPredicateBucket filter = new RelationPredicateBucket(OrderFields.CustomerId == "AROUT");
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(orders, filter);
			}

			// the sortExpression to apply to the view
			SortExpression sorter = new SortExpression();
			sorter.Add(OrderFields.ShipVia | SortOperator.Ascending);
			sorter.Add(OrderFields.EmployeeId | SortOperator.Ascending);
			sorter.Add(OrderFields.Freight | SortOperator.Ascending);

			// create the view
			EntityView2<OrderEntity> ordersView = new EntityView2<OrderEntity>(orders, sorter);

			// see the contents of the sorted view
			int currentEmployeeId = 0, previousEmployeeId = 0, currentShipVia = 0, previousShipVia = 0;
			decimal currentFreight = 0.0M, previousFreight = 0.0M;
			foreach(OrderEntity o in ordersView)
			{
				Console.WriteLine("{0}\t{1}\t{2}", o.ShipVia, o.EmployeeId, o.Freight);
				previousShipVia = currentShipVia;
				currentShipVia = o.ShipVia.Value;
				previousEmployeeId = currentEmployeeId;
				currentEmployeeId = o.EmployeeId.Value;
				previousFreight = currentFreight;
				currentFreight = o.Freight.Value;
				Assert.IsTrue(previousShipVia <= currentShipVia);
				if(previousShipVia == currentShipVia)
				{
					Assert.IsTrue(previousEmployeeId <= currentEmployeeId);
					if(previousEmployeeId == currentEmployeeId)
					{
						Assert.IsTrue(previousFreight <= currentFreight);
					}
				}
			}
		}


		[Test]
		public void CancellingAddNewIndexTestOnView()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}
			Assert.AreEqual(91, customers.Count);

			bool cancelFlag = true;

			customers.EntityAdding += delegate(object sender, CancelableCollectionChangedEventArgs e) { e.Cancel = cancelFlag; };

			EntityView2<CustomerEntity> view = customers.DefaultView;
			((IBindingList)view).AddNew();
			Assert.AreEqual(91, customers.Count);
			Assert.AreEqual(91, view.Count);
			cancelFlag = false;
			((IBindingList)view).AddNew();
			Assert.AreEqual(92, customers.Count);
			Assert.AreEqual(92, view.Count);
		}


		[Test]
		public void DynamicListWithNonMatchingDataTableFetch()
		{
			DataTable table = new DataTable();
			table.Columns.Add("CustomerId", typeof(string));
			table.Columns.Add("CompanyName", typeof(string));

			ResultsetFields fields = new ResultsetFields(4);
			fields.DefineField(CustomerFields.CustomerId, 0);
			fields.DefineField(CustomerFields.CompanyName, 1);
			fields.DefineField(CustomerFields.City, 2);
			fields.DefineField(CustomerFields.Country, 3);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, table, null);
			}

			Assert.AreEqual(4, table.Columns.Count);
			Assert.AreEqual("City", table.Columns[2].ColumnName);
			Assert.AreEqual("Country", table.Columns[3].ColumnName);
			Assert.AreEqual(91, table.Rows.Count);
		}


		[Test]
		public void DynamicListWithNonMatchingDataTableFetch2()
		{
			DataTable table = new DataTable();
			table.Columns.Add("CustomerId", typeof(string));
			table.Columns.Add("DummyField", typeof(DateTime));
			table.Columns.Add("CompanyName", typeof(string));

			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(CustomerFields.CustomerId, 0);
			fields.DefineField(CustomerFields.CompanyName, 1);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, table, null);
			}

			Assert.AreEqual(3, table.Columns.Count);
			Assert.AreEqual(91, table.Rows.Count);
		}


		/// <summary>
		/// This test relies on the FK in order 10253 being HANAr, instead of HANAR. 
		/// </summary>
		[Test]
		public void PrefetchPathWithFKWithDifferentCasing()
		{
			EntityField2.CaseSensitiveStringHashCodes = false;
			try
			{
				OrderEntity o = new OrderEntity(10253);
				PrefetchPath2 path = new PrefetchPath2(EntityType.OrderEntity);
				path.Add(OrderEntity.PrefetchPathCustomers);

				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{
					Assert.IsTrue(adapter.FetchEntity(o, path));
				}

				Assert.AreEqual(EntityState.Fetched, o.Fields.State);
				Assert.IsNotNull(o.Customers);
			}
			finally
			{
				EntityField2.CaseSensitiveStringHashCodes = true;
			}
		}


		[Test]
		public void ThreeFieldSortInmemoryTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}

			Assert.AreEqual(91, customers.Count);

			SortExpression sorter = new SortExpression();
			sorter.Add(CustomerFields.Country | SortOperator.Ascending);
			sorter.Add(CustomerFields.City | SortOperator.Ascending);
			sorter.Add(CustomerFields.CustomerId| SortOperator.Ascending);

			EntityView2<CustomerEntity> customerView = new EntityView2<CustomerEntity>(customers, sorter);
			Assert.AreEqual(91, customerView.Count);

			SortExpression dbSorter = new SortExpression();
			dbSorter.Add(CustomerFields.Country | SortOperator.Ascending);
			dbSorter.Add(CustomerFields.City | SortOperator.Ascending);
			dbSorter.Add(CustomerFields.CustomerId | SortOperator.Ascending);

			EntityCollection<CustomerEntity> customersCompare = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customersCompare, null, 0, dbSorter);
			}

			EntityView2<CustomerEntity> dbCustomerView = new EntityView2<CustomerEntity>(customersCompare);
			Assert.AreEqual(91, dbCustomerView.Count);
			for(int i = 0; i < customerView.Count; i++)
			{
				Assert.AreEqual(customerView[i].CustomerId, dbCustomerView[i].CustomerId);
			}

			customerView.Sorter = sorter;
			for(int i = 0; i < customerView.Count; i++)
			{
				Assert.AreEqual(customerView[i].CustomerId, dbCustomerView[i].CustomerId);
			} 
		}


		[Test]
		public void DynamicListWithoutResultsDataTableConstructionTest()
		{
			DataTable results = new DataTable();
			ResultsetFields fields = new ResultsetFields(3);
			fields.DefineField(CustomerFields.CustomerId, 0);
			fields.DefineField(CustomerFields.CompanyName, 1);
			fields.DefineField(CustomerFields.Country, 2);
			Assert.AreEqual(0, results.Columns.Count);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, new RelationPredicateBucket(CustomerFields.CustomerId == "AAAAP"));
			}
			Assert.AreEqual(0, results.Rows.Count);
			Assert.AreEqual(3, results.Columns.Count);
			Assert.AreEqual("CustomerId", results.Columns[0].ColumnName);
			Assert.AreEqual("CompanyName", results.Columns[1].ColumnName);
			Assert.AreEqual("Country", results.Columns[2].ColumnName);
		}


		[Test]
		public void CreateDataSetFromPrefetchPath()
		{
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders)
					.SubPath.Add(OrderEntity.PrefetchPathOrderDetails).SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);
			path.Add(CustomerEntity.PrefetchPathEmployees);
			path.Add(CustomerEntity.PrefetchPathShippers);

			DataSet emptyDataSet = GeneralUtils.ProduceEmptyDataSet(path, new ElementCreator());
			Assert.AreEqual(6, emptyDataSet.Tables.Count);
			Assert.IsTrue(emptyDataSet.Tables.Contains("CustomerEntity"));
			Assert.IsTrue(emptyDataSet.Tables.Contains("OrderEntity"));
			Assert.IsTrue(emptyDataSet.Tables.Contains("ProductEntity"));
			Assert.IsTrue(emptyDataSet.Tables.Contains("OrderDetailsEntity"));
			Assert.IsTrue(emptyDataSet.Tables.Contains("EmployeeEntity"));
			Assert.IsTrue(emptyDataSet.Tables.Contains("ShipperEntity"));
		}


		[Test]
		public void TypedListWithBlobAndDistinctTest()
		{
			EmployeeBlobTypedList tlist = new EmployeeBlobTypedList();
			IRelationPredicateBucket filterBucket = tlist.GetRelationInfo();
			filterBucket.Relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(tlist.GetFieldsInfo(), tlist, filterBucket, false);
			}

			Assert.AreEqual(9, tlist.Rows.Count);
		}


		[Test]
		public void ConcatOfStringsUsingDbFunctionCallExpressionWithOperator()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(EmployeeFields.EmployeeId, 0);
			fields.DefineField(new EntityField2("Full name", 
					new DbFunctionCall(string.Empty, string.Empty, "{0} + {1}", new object[] { EmployeeFields.FirstName, EmployeeFields.LastName})), 1);
			DataTable results = new DataTable();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, null);
			}
			foreach(DataRow row in results.Rows)
			{
				Console.WriteLine("ID: {0}. Fullname: '{1}'", row[0], row[1]);
			}
		}


		/// <summary>
		/// Test which checks if the entities which are tracked using a delete tracker,aren't serializing over XML their related entities.
		/// </summary>
		[Test]
		public void XMLSerializationDeserializationOfDeleteTrackedEntitiesTest()
		{
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				PrefetchPath2 path = new PrefetchPath2(EntityType.OrderEntity);
				path.Add(OrderEntity.PrefetchPathOrderDetails);
				adapter.FetchEntityCollection(orders, new RelationPredicateBucket(OrderFields.CustomerId == "CHOPS"), path);
			}

			Assert.AreEqual(8, orders.Count);

			EntityCollection<OrderEntity> tracker = new EntityCollection<OrderEntity>();
			orders.RemovedEntitiesTracker = tracker;

			// delete the first 2 orders
			orders.RemoveAt(0);
			orders.RemoveAt(0);

			Assert.AreEqual(2, tracker.Count);

			// now serialize to XML the entities in the tracker.
			string xml = string.Empty;
			tracker.WriteXml(XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out xml);

			// deserialize the orders back into new entities
			EntityCollection<OrderEntity> deserializedOrders = new EntityCollection<OrderEntity>();
			deserializedOrders.ReadXml(xml);

			Assert.AreEqual(0, deserializedOrders[0].OrderDetails.Count);
			Assert.AreEqual(0, deserializedOrders[1].OrderDetails.Count);

			// serialize/deserialize orders with tracker to see if the tracker is serialized as well
			xml = string.Empty;
			orders.WriteXml(XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out xml);

			deserializedOrders = new EntityCollection<OrderEntity>();
			deserializedOrders.ReadXml(xml);
			Assert.AreEqual(orders.Count, deserializedOrders.Count);
			Assert.IsNotNull(deserializedOrders.RemovedEntitiesTracker);
			Assert.AreEqual(orders.RemovedEntitiesTracker.Count, deserializedOrders.RemovedEntitiesTracker.Count);

			xml = string.Empty;
			orders.WriteXml(XmlFormatAspect.Compact | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out xml);
			//StreamWriter writer = new StreamWriter("orders.xml");
			//writer.Write(xml);
			//writer.Close();

			deserializedOrders = new EntityCollection<OrderEntity>();
			deserializedOrders.ReadXml(xml);
			Assert.AreEqual(orders.Count, deserializedOrders.Count);
			Assert.IsNotNull(deserializedOrders.RemovedEntitiesTracker);
			Assert.AreEqual(orders.RemovedEntitiesTracker.Count, deserializedOrders.RemovedEntitiesTracker.Count);
		}


		[Test]
		public void XmlSerializationTest()
		{
			//XmlHelper.CultureNameForXmlValueConversion = "nl-NL";
			CustomerEntity chops = new CustomerEntity("CHOPS");
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntity(chops);
				adapter.FetchEntityCollection(chops.Orders, chops.GetRelationInfoOrders());
			}

			string chopsAsXml = string.Empty;
			chops.WriteXml(out chopsAsXml);
			CustomerEntity chopsFromXml = new CustomerEntity();
			chopsFromXml.ReadXml(chopsAsXml);

			StreamWriter writer = new StreamWriter("chopsOrdersVerbose.xml");
			writer.Write(chopsAsXml);
			writer.Close();

			string chopsAsCompactXml = string.Empty;
			chops.WriteXml(XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out chopsAsCompactXml);
			writer = new StreamWriter("chopsOrdersCompact.xml");
			writer.Write(chopsAsCompactXml);
			writer.Close();

			CustomerEntity chopsFromCompactXml = new CustomerEntity();
			chopsFromCompactXml.ReadXml(chopsAsCompactXml);

			string chopsAsCompactXmlFromCompactXml = string.Empty;
			chopsFromCompactXml.WriteXml(XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out chopsAsCompactXmlFromCompactXml);
			writer = new StreamWriter("chopsOrdersCompactFromCompact.xml");
			writer.Write(chopsAsCompactXmlFromCompactXml);
			writer.Close();

			Assert.AreEqual(chopsAsCompactXml, chopsAsCompactXmlFromCompactXml);
		}

	
		[Test]
		public void IntermediateEntityPrefetchPathTest()
		{
			EntityCollection<OrderDetailsEntity> ods = new EntityCollection<OrderDetailsEntity>();
			PrefetchPath2 path = new PrefetchPath2(EntityType.OrderDetailsEntity);
			path.Add(OrderDetailsEntity.PrefetchPathOrders);
			path.Add(OrderDetailsEntity.PrefetchPathProducts);
			RelationPredicateBucket filter = new RelationPredicateBucket(OrderDetailsFields.ProductId == 1);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 10;
				adapter.FetchEntityCollection(ods, filter, path);
			}

			Assert.AreEqual(38, ods.Count);
			foreach(OrderDetailsEntity od in ods)
			{
				Assert.IsNotNull(od.Orders);
				Assert.IsNotNull(od.Products);
			}
		}


		[Test]
		public void DynamicListWithGroupByAndExpressionTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(CustomerFields.ContactName, 0);
			fields.DefineField(new EntityField2("Total",
									(OrderDetailsFields.Quantity * OrderDetailsFields.UnitPrice), AggregateFunction.Sum), 1);
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(CustomerEntity.Relations.OrderEntityUsingCustomerId);
			filter.Relations.Add(OrderEntity.Relations.OrderDetailsEntityUsingOrderId);
			GroupByCollection groupBy = new GroupByCollection();
			groupBy.Add(fields[0]);
			DataTable table = new DataTable();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, table, filter, 0, null, true, groupBy);
			}

			Assert.AreEqual(89, table.Rows.Count);
		}


		[Test]
		public void PagingWithExcludedFieldsTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(CustomerFields.Country);

			using(DataAccessAdapter adapter = new DataAccessAdapter(true))
			{
				for(int i = 1; i < 10; i++)
				{
					customers.Clear();
					adapter.FetchEntityCollection(customers, null, 0, null, null, excludedFields, i, 10);

					Assert.AreEqual(10, customers.Count);
					foreach(CustomerEntity c in customers)
					{
						Assert.AreEqual(string.Empty, c.Country);
					}
				}
			}
		}


		[Test]
		public void Compact25XmlSerializationTest()
		{
			CustomerEntity customer = new CustomerEntity("CHOPS");
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders, 4);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(customer, path));
			}

			Assert.AreEqual(4, customer.Orders.Count);

			XmlWriter writer = XmlWriter.Create("customerOrdersBefore.xml");
			customer.WriteXml(writer, XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks);
			writer.Close();

			customer.Orders[0].ShipName = "NewShip";
			writer = XmlWriter.Create("customerOrdersAfter.xml");
			customer.WriteXml(writer, XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks);
			writer.Close();

			customer.Orders.Insert(0, new OrderEntity());
			writer = XmlWriter.Create("customerOrdersAfterWithNew.xml");
			customer.WriteXml(writer, XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks);
			writer.Close();

			CustomerEntity customerD = new CustomerEntity();
			XmlReader reader = XmlReader.Create("customerOrdersAfterWithNew.xml");
			customerD.ReadXml(reader);
			Assert.AreEqual(customer.Orders.Count, customerD.Orders.Count);

			writer = XmlWriter.Create("customerOrdersAfterDeserialization.xml");
			customerD.WriteXml(writer, XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks);
			writer.Close();
		}

		
		[Test]
		public void FastSerializationWithTrackerCollection()
		{
			SerializationHelper.Optimization = SerializationOptimization.Fast;
			CustomerEntity c = new CustomerEntity("CHOPS");
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(c, path));
			}
			Assert.AreEqual(8, c.Orders.Count);
			Assert.IsNull(c.Orders.RemovedEntitiesTracker);

			// first simple serialization without tracker.
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, c);
			stream.Seek(0, SeekOrigin.Begin);
			CustomerEntity deserializedC = (CustomerEntity)formatter.Deserialize(stream);
			stream.Close();

			Assert.AreEqual(8, deserializedC.Orders.Count);
			Assert.IsNull(deserializedC.Orders.RemovedEntitiesTracker);

			// add tracker to c.Orders
			c.Orders.RemovedEntitiesTracker = new EntityCollection<OrderEntity>();
			List<OrderEntity> toRemove = new List<OrderEntity>(c.Orders);
			Assert.AreEqual(8, toRemove.Count);
			foreach(OrderEntity o in toRemove)
			{
				c.Orders.Remove(o);
			}

			Assert.AreEqual(0, c.Orders.Count);
			Assert.AreEqual(8, c.Orders.RemovedEntitiesTracker.Count);

			foreach(OrderEntity o in toRemove)
			{
				Assert.IsTrue(c.Orders.RemovedEntitiesTracker.Contains(o));
			}

			stream = new MemoryStream();
			formatter.Serialize(stream, c);
			stream.Seek(0, SeekOrigin.Begin);
			deserializedC = (CustomerEntity)formatter.Deserialize(stream);
			stream.Close();
			Assert.AreEqual(0, deserializedC.Orders.Count);
			Assert.IsNotNull(deserializedC.Orders.RemovedEntitiesTracker);
			Assert.AreEqual(8, deserializedC.Orders.RemovedEntitiesTracker.Count);

			SerializationHelper.Optimization = SerializationOptimization.None;
		}


		[Test]
		public void PrefetchPathWithTopAndOrderBySubQueryTest()
		{
			PrefetchPath2 path = new PrefetchPath2(EntityType.EmployeeEntity);
			path.Add(EmployeeEntity.PrefetchPathOrders);
			SortExpression sorter = new SortExpression(EmployeeFields.LastName | SortOperator.Ascending);
			EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>();
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(EmployeeEntity.Relations.RegionEntityUsingRegionId);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 3;
				adapter.FetchEntityCollection(employees, filter, 6, sorter, path);
			}

			Assert.AreEqual(4, employees.Count);
		}


		[Test]
		public void PrefetchPathWithCustomFilterRelationsSerializationTest()
		{
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			IPrefetchPathElement2 element = path.Add(CustomerEntity.PrefetchPathOrders);
			IEntityRelation relation = CustomerEntity.Relations.OrderEntityUsingCustomerId;
			relation.CustomFilter = new PredicateExpression(OrderFields.EmployeeId == 1);
			relation.CustomFilter.AddWithAnd(OrderFields.ShipCountry == "Germany");
			element.FilterRelations.Add(relation);

			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			formatter.Serialize(stream, path);
			stream.Seek(0, SeekOrigin.Begin);
			PrefetchPath2 pathD = (PrefetchPath2)formatter.Deserialize(stream);
			stream.Close();

			Assert.AreEqual(path.Count, pathD.Count);
			IPrefetchPathElement2 elementD = pathD[0];
			Assert.AreEqual(element.FilterRelations.Count, elementD.FilterRelations.Count);
			Assert.AreEqual(((IEntityRelation)element.FilterRelations[0]).CustomFilter.Count, ((IEntityRelation)elementD.FilterRelations[0]).CustomFilter.Count);
		}


		/// <summary>
		/// Updates the entities directly with expression test.
		/// </summary>
		[Test]
		public void UpdateEntitiesDirectlyWithExpressionTest()
		{
			OrderDetailsEntity orderDetail = new OrderDetailsEntity();
			orderDetail.Fields[(int)OrderDetailsFieldIndex.UnitPrice].ExpressionToApply =
				(OrderDetailsFields.UnitPrice + (OrderDetailsFields.UnitPrice * 0.01f));
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.CatalogNameUsageSetting = CatalogNameUsage.ForceName;
				adapter.CatalogNameToUse = "Northwind2";

				int count = adapter.UpdateEntitiesDirectly(orderDetail, new RelationPredicateBucket(OrderDetailsFields.OrderId==10254));
				Assert.AreEqual(9, count);
			}
		}


		[Test]
		public void GenericFetchNewEntityTest()
		{
			OrderEntity order = new OrderEntity(10254);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue( adapter.FetchEntity(order));
				CustomerEntity c = adapter.FetchNewEntity<CustomerEntity>(order.GetRelationInfoCustomers());
				Assert.AreEqual(c.CustomerId, order.CustomerId);
			}
		}

		[Test]
		public void ManyToManyPrefetchPathWithRelationsBasedFilterTest()
		{
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathEmployees);

			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(CustomerEntity.Relations.OrderEntityUsingCustomerId);
			filter.Relations.Add(OrderEntity.Relations.EmployeeEntityUsingEmployeeId);
			filter.PredicateExpression.Add(EmployeeFields.FirstName == "Laura");

			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, filter, 0, null, path);
			}

			Assert.AreEqual(54, customers.Count);
		}


		[Test]
		public void DBFunctionCallWithExpressionFilterTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			string[] postalCodes = new string[] { "10", "80" };
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, new RelationPredicateBucket(new EntityField2("PostalCode",
						new DbFunctionCall("SUBSTRING", new object[] { CustomerFields.PostalCode, 1, 2 })) == postalCodes));
			}
			Assert.AreEqual(7, customers.Count);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ScaleOverflowTestNone()
		{
			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.None;
			OrderDetailsEntity od = new OrderDetailsEntity();
			od.UnitPrice = 1M/ 3M;
		}


		[Test]
		public void ScaleOverflowTestRoundTruncate()
		{
			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.Round;
			OrderDetailsEntity od = new OrderDetailsEntity();
			od.UnitPrice = 1M / 3M;

			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.Round;
			OrderDetailsEntity od2 = new OrderDetailsEntity();
			od.UnitPrice = 1M / 3M;

			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.None;
		}


		[Test]
		public void FilteringViewWithEmptyPredicateExpressionTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}
			Assert.AreEqual(91, customers.Count);

			EntityView2<CustomerEntity> customerView = new EntityView2<CustomerEntity>(customers);
			customerView.Filter = new PredicateExpression();
			Assert.AreEqual(91, customerView.Count);

			customerView.Filter = new PredicateExpression((CustomerFields.Country == "Germany"));
			Assert.AreEqual(11, customerView.Count);

			customerView.Filter = (CustomerFields.Country == "Germany");
			Assert.AreEqual(11, customerView.Count);
		}


		[Test]
		public void DbFunctionCallWithConstantsTest()
		{
			ResultsetFields fields = new ResultsetFields( 2 );
			fields.DefineField( OrderFields.OrderId, 0, "OrderID" );
			fields.DefineField( OrderFields.OrderDate, 1, "Month" );

			fields[1].ExpressionToApply = new DbFunctionCall("CAST({0} AS bigint)",
					new object[] { new DbFunctionCall("MONTH", new object[] { OrderFields.OrderDate }) });

			DataTable results = new DataTable();
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchTypedList( fields, results, null );
			}
			foreach( DataRow row in results.Rows )
			{
				Assert.AreEqual(typeof(Int64), row[1].GetType());
			}
		}


		[Test]
		public void CaseStatementUsingDbFunctionCallTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(SupplierFields.Id, 0);
			fields.DefineField(SupplierFields.Region, 1);

			fields[1].ExpressionToApply = new DbFunctionCall("CASE {0} WHEN 'MA' THEN 'Massachusetts' WHEN 'OR' THEN 'Oregon' ELSE 'Unknown' END", new object[] { SupplierFields.Region });
			DataTable results = new DataTable();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, new RelationPredicateBucket(SupplierFields.Country=="USA"), 0, 
						new SortExpression(SupplierFields.Id | SortOperator.Ascending), true);
			}

			Assert.AreEqual(4, results.Rows.Count);
			Assert.AreEqual("Unknown", (string)results.Rows[0][1]);
			Assert.AreEqual("Unknown", (string)results.Rows[1][1]);
			Assert.AreEqual("Oregon", (string)results.Rows[2][1]);
			Assert.AreEqual("Massachusetts", (string)results.Rows[3][1]);
		}


		[Test]
		public void RemovedEntitiesTrackerTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);
				adapter.FetchEntityCollection(customers, new RelationPredicateBucket(CustomerFields.Country == "Germany"), path);
			}

			EntityCollection<OrderEntity> tracker = new EntityCollection<OrderEntity>();
			customers[0].Orders.RemovedEntitiesTracker = tracker;
			customers[2].Orders.RemovedEntitiesTracker = tracker;

			OrderEntity o0,o1, o2;
			o0 = customers[0].Orders[0];
			o1 = customers[1].Orders[0];
			o2 = customers[2].Orders[0];

			customers[0].Orders.Remove(o0);
			customers[1].Orders.Remove(o1);
			customers[2].Orders.Remove(o2);

			Assert.IsTrue(tracker.Contains(o0));
			Assert.IsTrue(tracker.Contains(o2));
			Assert.IsFalse(tracker.Contains(o1));

			Assert.AreEqual(2, tracker.Count);

			Assert.IsFalse(customers[0].Orders.Contains(o0));
			Assert.IsFalse(customers[1].Orders.Contains(o1));
			Assert.IsFalse(customers[2].Orders.Contains(o2));
		}


		[Test]
		public void FetchSingleEntityWithUCWithExcludingFieldsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity c = new CustomerEntity();
				c.CompanyName = "The Big Cheese";
				ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
				excludedFields.Add(CustomerFields.ContactName);
				excludedFields.Add(CustomerFields.Country);
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(c, c.ConstructFilterForUCCompanyName(), null, null, excludedFields));

				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
			}
		}


		[Test]
		public void FetchWithExcludedFieldsTest()
		{
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(CustomerFields.ContactName);
			excludedFields.Add(CustomerFields.Country);

			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			EntityCollection<CustomerEntity> customersVerification = new EntityCollection<CustomerEntity>();
			SortExpression sorter = new SortExpression(CustomerFields.CustomerId | SortOperator.Descending);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, 0, sorter, null, excludedFields);
				adapter.FetchEntityCollection(customersVerification, null, 0, sorter);
			}

			Assert.AreEqual(91, customers.Count);
			Assert.AreEqual(91, customersVerification.Count);

			foreach(CustomerEntity customer in customers)
			{
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
			}

			CustomerEntity c = new CustomerEntity("CHOPS");
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntity(c, null, null, excludedFields);
			}

			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));

			// load the excluded fields into the entities already loaded:
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 5;
				adapter.FetchExcludedFields(customers, excludedFields);
				adapter.FetchExcludedFields(c, excludedFields);
			}

			for(int i = 0; i < customers.Count; i++)
			{
				Assert.AreEqual(customers[i].ContactName, customersVerification[i].ContactName);
				Assert.AreEqual(customers[i].Country, customersVerification[i].Country);
			}
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
		}


		[Test]
		public void ExcludedFieldsTestWithPrefetchPath()
		{
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(OrderFields.OrderDate);
			excludedFields.Add(OrderFields.CustomerId);		// will be ignored, as it's an FK.
			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders, excludedFields);

			EntityCollection<CustomerEntity> customers =new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, 25, null, path);
			}

			Assert.AreEqual(25, customers.Count);

			foreach(CustomerEntity c in customers)
			{
				foreach(OrderEntity o in c.Orders)
				{
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.OrderDate));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.CustomerId));
				}
			}
		}


		[Test]
		public void FetchSingleEntityWithUCWithIncludingFieldsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity c = new CustomerEntity();
				c.CompanyName = "The Big Cheese";
				ExcludeIncludeFieldsList includedFields = new ExcludeIncludeFieldsList(false);
				includedFields.Add(CustomerFields.CompanyName);
				includedFields.Add(CustomerFields.ContactName);
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(c, c.ConstructFilterForUCCompanyName(), null, null, includedFields));

				Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.CustomerId));
				Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.CompanyName));
				Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Address));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.City));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactTitle));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Fax));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Phone));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.PostalCode));
				Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Region));
			}
		}


		[Test]
		public void FetchWithIncludedFieldsTest()
		{
			ExcludeIncludeFieldsList includedFields = new ExcludeIncludeFieldsList(false);
			includedFields.Add(CustomerFields.ContactName);
			includedFields.Add(CustomerFields.CompanyName);

			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			EntityCollection<CustomerEntity> customersVerification = new EntityCollection<CustomerEntity>();
			SortExpression sorter = new SortExpression(CustomerFields.CustomerId | SortOperator.Descending);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, 0, sorter, null, includedFields);
				adapter.FetchEntityCollection(customersVerification, null, 0, sorter);
			}

			Assert.AreEqual(91, customers.Count);
			Assert.AreEqual(91, customersVerification.Count);

			foreach(CustomerEntity customer in customers)
			{
				Assert.IsFalse(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.CustomerId));
				Assert.IsFalse(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.CompanyName));
				Assert.IsFalse(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Address));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.City));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactTitle));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Fax));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Phone));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.PostalCode));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Region));
			}

			CustomerEntity c = new CustomerEntity("CHOPS");
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntity(c, null, null, includedFields);
			}

			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.CustomerId));
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.CompanyName));
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Address));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.City));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactTitle));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Fax));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Phone));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.PostalCode));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Region));

			// load the excluded fields into the entities already loaded:
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 5;
				adapter.FetchExcludedFields(customers, includedFields);
				adapter.FetchExcludedFields(c, includedFields);
			}

			for(int i = 0; i < customers.Count; i++)
			{
				for(int j = 0; j < (int)CustomerFieldIndex.AmountOfFields; j++)
				{
					Assert.AreEqual(customers[i].GetCurrentFieldValue(j), customersVerification[i].GetCurrentFieldValue(j));
				}
			}
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.City));
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
		}


		[Test]
		public void IncludedFieldsTestWithPrefetchPath()
		{
			ExcludeIncludeFieldsList includedFields = new ExcludeIncludeFieldsList(false);
			// FK's are automatically fetched, including PK's. So we just have to specify the fields which aren't an FK or PK and have to be
			// fetched into the list. Pick 2 ship* fields. 
			includedFields.Add(OrderFields.ShipAddress);
			includedFields.Add(OrderFields.ShipCity);

			PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders, includedFields);

			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, 25, null, path);
			}

			Assert.AreEqual(25, customers.Count);

			foreach(CustomerEntity c in customers)
			{
				foreach(OrderEntity o in c.Orders)
				{
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.OrderDate));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.Freight));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.RequiredDate));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipCountry));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipName));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShippedDate));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipPostalCode));
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipRegion));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.CustomerId));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.EmployeeId));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.OrderId));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipCity));
					Assert.IsFalse(o.TestCurrentFieldValueForNull(OrderFieldIndex.ShipAddress));
				}
			}
		}


		[Test]
		public void DIScopeBasedInjectionTest()
		{
			// first normal injection via attributes. The customer entity isn't mentioned in the scope definitions so it should keep getting its
			// original general cpf
			ProductEntity p = new ProductEntity();
			Assert.AreEqual(typeof(GeneralNorthwindCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
			CustomerEntity c = new CustomerEntity();
			Assert.AreEqual(typeof(GeneralNorthwindCPF), c.ConcurrencyPredicateFactoryToUse.GetType());

			Stopwatch sw = new Stopwatch();
			sw.Start();
			TestScope scope = new TestScope();
			sw.Stop();
			scope.Dispose();

			Console.WriteLine("Creation of scope took: {0}", sw.ElapsedMilliseconds);
			sw.Reset();
			sw.Start();
			scope.BeginScope();
			sw.Stop();
			Console.WriteLine("Creation of scope took: {0}", sw.ElapsedMilliseconds);
			scope.EndScope();

			// not affected by the scope just started and ended.
			p = new ProductEntity();
			Assert.AreEqual(typeof(GeneralNorthwindCPF), p.ConcurrencyPredicateFactoryToUse.GetType());

			// using one scope
			using(TestScope outerScope = new TestScope())
			{
				p = new ProductEntity();
				Assert.AreEqual(typeof(ProductSpecificCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
				c = new CustomerEntity();
				Assert.AreEqual(typeof(GeneralNorthwindCPF), c.ConcurrencyPredicateFactoryToUse.GetType());
			}

			// using nested scopes
			using(TestScope outerScope = new TestScope())
			{
				p = new ProductEntity();
				Assert.AreEqual(typeof(ProductSpecificCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
				c = new CustomerEntity();
				Assert.AreEqual(typeof(GeneralNorthwindCPF), c.ConcurrencyPredicateFactoryToUse.GetType());
				using(NestedTestScope innerSCope = new NestedTestScope())
				{
					p = new ProductEntity();
					Assert.AreEqual(typeof(NestedProductSpecificCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
					c = new CustomerEntity();
					Assert.AreEqual(typeof(GeneralNorthwindCPF), c.ConcurrencyPredicateFactoryToUse.GetType());
				}
				p = new ProductEntity();
				Assert.AreEqual(typeof(ProductSpecificCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
			}
			p = new ProductEntity();
			Assert.AreEqual(typeof(GeneralNorthwindCPF), p.ConcurrencyPredicateFactoryToUse.GetType());

			// re-using the earlier created scope, so we don't have to rebuild the inner type cache:
			try
			{
				scope.BeginScope();
				p = new ProductEntity();
				Assert.AreEqual(typeof(ProductSpecificCPF), p.ConcurrencyPredicateFactoryToUse.GetType());
			}
			finally
			{
				scope.EndScope();
			}
		}


		[Test]
		public void FullTextSearchPredicateTest()
		{
			EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(employees, new RelationPredicateBucket(
						new FieldFullTextSearchPredicate(EmployeeFields.Notes, null, FullTextSearchOperator.Contains, "BA")));
			}
		}


		[Test]
		public void DelegatePredicateTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
			}

			Assert.AreEqual(91, customers.Count);
			// create a view from all customers which have a customername with length > 20 characters.
			EntityView2<CustomerEntity> customersWithLongName = new EntityView2<CustomerEntity>(customers,
							new DelegatePredicate(delegate(IEntityCore toExamine) { return ((CustomerEntity)toExamine).CompanyName.Length > 20; }));
			Assert.AreEqual(30, customersWithLongName.Count);
		}


		[Test]
		public void MultiLevelFilterTestWithAggregatePredicate()
		{
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, path);
			}

			Assert.AreEqual(91, customers.Count);

			// in memory filtering of customers, based on property of related entity. Get all customers which have orders with a total > 5000
			// order total is calculated by SUMming the orderdetail total values. 
			// order detail total is (UnitPrice * quantity) - ((UnitPrice * quantity)* discount)
			IPredicate filter = new MemberPredicate(
							CustomerEntity.MemberNames.Orders,	// the member to apply the specified filter on.
							MemberOperator.Any,   // operator for applying contained filter.
							new AggregateSetPredicate(		// aggregate predicate to apply on a member
								OrderEntity.MemberNames.OrderDetails,	// member to apply the aggregate on
								AggregateSetFunction.Sum,		// aggregate function to apply. Only simple aggregates are supported or a custom delegate
								((OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity)-
									((OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity) 
										* OrderDetailsFields.Discount)),		// value producer expression. 
								ComparisonOperator.GreaterThan,		// comparison operator for the aggregateset predicate to become true
								5000.0M,				// value to compare with
								null				// filter which specifies the set to apply the aggregate on.
								)					
							);

			EntityView2<CustomerEntity> filteredCustomers = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(14, filteredCustomers.Count);

			// filter on customers to find all customers with at least 10 orders
			filter = new AggregateSetPredicate(		// aggregate predicate to apply on a member
					CustomerEntity.MemberNames.Orders,	// member to apply the aggregate on
					AggregateSetFunction.Count,	// aggregate function to apply. Only simple aggregates are supported + custom.
					OrderFields.OrderId,		// value producer. Can be a field as well.
					ComparisonOperator.GreaterThan,		// comparison operator for the aggregateset predicate to become true
					10,
					null);					// value to compare with

			filteredCustomers = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(27, filteredCustomers.Count);

			// filter on customers which have an order with order lines with quantity > 10. 
			filter = new MemberPredicate(
							CustomerEntity.MemberNames.Orders,	// the member to apply the specified filter on.
							MemberOperator.Any,   // operator for applying contained filter.
							new AggregateSetPredicate(		// aggregate predicate to apply on a member
								OrderEntity.MemberNames.OrderDetails,	// member to apply the aggregate on
								AggregateSetFunction.Count,		// aggregate function to apply. Only simple aggregates are supported or a custom delegate
								OrderDetailsFields.OrderId,		// value producer. Can be a field as well.
								ComparisonOperator.GreaterThan,		// comparison operator for the aggregateset predicate to become true
								5,				// value to compare with
								(OrderDetailsFields.Quantity > (short)10)				// filter which specifies the set to apply the aggregate on.
							));

			filteredCustomers = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(2, filteredCustomers.Count);

			// custom aggregate which applies to property so more powerful operations can be performed. 
			// filter on customers where all orderid's added is greater than 80000 (just to illustrate the point of using a delegate)
			filter = new AggregateSetPredicate(		// aggregate predicate to apply on a member. 
			                    CustomerEntity.MemberNames.Orders,	// member to apply the aggregate on
			                    // custom function to apply on the set of produced values 
			                    delegate(IList valuesToAggregate) { int toReturn = 0; foreach(int i in valuesToAggregate) { toReturn+=i;}; return toReturn;},
			                    delegate(IEntityCore toInterpret) { return ((OrderEntity)toInterpret).OrderId;},
								ComparisonOperator.GreaterEqual,
								80000,
								null
			            );
			filteredCustomers = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(48, filteredCustomers.Count);
		}


		[Test]
		public void MultiLevelFilterTestCustomerOrders()
		{
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, path);
			}

			// in memory filtering of customers, based on property of related entity. Get all customers which have orders with product 1
			IPredicate filter = new MemberPredicate(
							CustomerEntity.MemberNames.Orders,	// the member to apply the specified filter on.
							MemberOperator.Any,   // operator for applying contained filter.
							new MemberPredicate(	// the filter to apply to each element in member (or member itself)
								OrderEntity.MemberNames.OrderDetails,
									MemberOperator.Any,
									(OrderDetailsFields.ProductId == 1))		// the filter to apply to each element in member (or member itself)
							);

			EntityView2<CustomerEntity> customersWithOrders = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(31, customersWithOrders.Count);

			// m:1 filters
			path = new PrefetchPath2((int)EntityType.OrderDetailsEntity);
			path.Add(OrderDetailsEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathCustomers);
			EntityCollection<OrderDetailsEntity> orderDetails = new EntityCollection<OrderDetailsEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(orderDetails, null, path);
			}

			// filter all order details of which the orders are placed by customers from germany.
			filter = new MemberPredicate(
							OrderDetailsEntity.MemberNames.Orders,
							MemberOperator.Any,
							new MemberPredicate(
								OrderEntity.MemberNames.Customers,
								MemberOperator.Any,
								(CustomerFields.Country=="Germany"))		// additional filter.
						);

			EntityView2<OrderDetailsEntity> orderDetailsFromGermans = new EntityView2<OrderDetailsEntity>(orderDetails, filter);
			Assert.AreEqual(303, orderDetailsFromGermans.Count);

			filter.Negate = true;
			orderDetailsFromGermans = new EntityView2<OrderDetailsEntity>(orderDetails, filter);
			Assert.AreEqual(1712, orderDetailsFromGermans.Count);

			filter = new MemberPredicate(CustomerEntity.MemberNames.Orders, MemberOperator.Null, null);
			EntityView2<CustomerEntity> customersWithoutOrders = new EntityView2<CustomerEntity>(customers, filter);
			Assert.AreEqual(2, customersWithoutOrders.Count);

		}


		[Test]
		public void CreateHierarchicalProjectionsWithoutSpecifyingProjectorsTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			IPrefetchPathElement2 element = path.Add(CustomerEntity.PrefetchPathOrders);
			element.SubPath.Add(OrderEntity.PrefetchPathEmployees);
			element.SubPath.Add(OrderEntity.PrefetchPathOrderDetails).SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, path);
			}

			DataSet result = new DataSet("projectionResult");

			customers.CreateHierarchicalProjection(result);
			Assert.AreEqual(5, result.Tables.Count);
			Assert.IsNotNull(result.Tables["CustomerEntity"]);
			Assert.IsNotNull(result.Tables["OrderEntity"]);
			Assert.IsNotNull(result.Tables["EmployeeEntity"]);
			Assert.IsNotNull(result.Tables["OrderDetailsEntity"]);
			Assert.IsNotNull(result.Tables["ProductEntity"]);
			Assert.IsNull(result.Tables["Dummy"]);

			// check PK's
			Assert.AreEqual(result.Tables["CustomerEntity"].Columns["CustomerId"], result.Tables["CustomerEntity"].PrimaryKey[0]);
			Assert.AreEqual(result.Tables["OrderEntity"].Columns["OrderId"], result.Tables["OrderEntity"].PrimaryKey[0]);
			Assert.AreEqual(result.Tables["EmployeeEntity"].Columns["EmployeeId"], result.Tables["EmployeeEntity"].PrimaryKey[0]);
			Assert.AreEqual(result.Tables["OrderDetailsEntity"].Columns["OrderId"], result.Tables["OrderDetailsEntity"].PrimaryKey[0]);
			Assert.AreEqual(result.Tables["OrderDetailsEntity"].Columns["ProductId"], result.Tables["OrderDetailsEntity"].PrimaryKey[1]);
			Assert.AreEqual(result.Tables["ProductEntity"].Columns["ProductId"], result.Tables["ProductEntity"].PrimaryKey[0]);

			Assert.AreEqual(91, result.Tables["CustomerEntity"].Rows.Count);
			Assert.AreEqual(818, result.Tables["orderEntity"].Rows.Count);
			Assert.AreEqual(9, result.Tables["employeeEntity"].Rows.Count);
			Assert.AreEqual(2015, result.Tables["orderDetailsEntity"].Rows.Count);
			Assert.AreEqual(74, result.Tables["productEntity"].Rows.Count);

			Assert.AreEqual(EntityFieldsFactory.CreateEntityFieldsObject(EntityType.CustomerEntity).Count, result.Tables["CustomerEntity"].Columns.Count);
			Assert.AreEqual(EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderEntity).Count, result.Tables["OrderEntity"].Columns.Count);
			Assert.AreEqual(EntityFieldsFactory.CreateEntityFieldsObject(EntityType.EmployeeEntity).Count, result.Tables["EmployeeEntity"].Columns.Count);
			Assert.AreEqual(EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderDetailsEntity).Count, result.Tables["OrderDetailsEntity"].Columns.Count);
			Assert.AreEqual(EntityFieldsFactory.CreateEntityFieldsObject(EntityType.ProductEntity).Count, result.Tables["ProductEntity"].Columns.Count);

			Dictionary<Type, IEntityCollection2> projectionResults = new Dictionary<Type, IEntityCollection2>();
			customers.CreateHierarchicalProjection(projectionResults);
			Assert.AreEqual(5, projectionResults.Count);
			Assert.AreEqual(result.Tables["CustomerEntity"].Rows.Count, projectionResults[typeof(CustomerEntity)].Count);
			Assert.AreEqual(result.Tables["OrderEntity"].Rows.Count, projectionResults[typeof(OrderEntity)].Count);
			Assert.AreEqual(result.Tables["EmployeeEntity"].Rows.Count, projectionResults[typeof(EmployeeEntity)].Count);
			Assert.AreEqual(result.Tables["OrderDetailsEntity"].Rows.Count, projectionResults[typeof(OrderDetailsEntity)].Count);
			Assert.AreEqual(result.Tables["ProductEntity"].Rows.Count, projectionResults[typeof(ProductEntity)].Count);
		}


		[Test]
		public void CreateHierarchicalProjectionsTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			IPrefetchPathElement2 element = path.Add(CustomerEntity.PrefetchPathOrders);
			element.SubPath.Add(OrderEntity.PrefetchPathEmployees);
			element.SubPath.Add(OrderEntity.PrefetchPathOrderDetails).SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null, path);
			}

			// setup projections per type.
			List<IEntityPropertyProjector> customerProjections = EntityFields2.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.CustomerEntity));
			List<IEntityPropertyProjector> orderProjections = EntityFields2.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderEntity));
			customerProjections.Add(new EntityPropertyProjector(new EntityProperty("IsNew"), "IsNew"));

			List<IEntityPropertyProjector> employeeProjections = EntityFields2.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.EmployeeEntity));
			List<IEntityPropertyProjector> orderDetailsProjections = EntityFields2.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderDetailsEntity));
			List<IEntityPropertyProjector> productProjections = EntityFields2.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.ProductEntity));

			List<IViewProjectionData> projectionData = new List<IViewProjectionData>();
			projectionData.Add(new ViewProjectionData<CustomerEntity>(customerProjections, (CustomerFields.Country == "Germany"), true)) ;
			projectionData.Add(new ViewProjectionData<OrderEntity>(orderProjections, null, false));
			projectionData.Add(new ViewProjectionData<EmployeeEntity>(employeeProjections));
			projectionData.Add(new ViewProjectionData<OrderDetailsEntity>(orderDetailsProjections, (OrderDetailsFields.Discount > 0.0f), false));
			projectionData.Add(new ViewProjectionData<ProductEntity>(productProjections, (ProductFields.Discontinued == false), true));
			
			DataSet result = new DataSet("projectionResult");
			customers.CreateHierarchicalProjection(projectionData, result);
			Assert.AreEqual(5, result.Tables.Count);
			Assert.IsNotNull(result.Tables["CustomerEntity"]);
			Assert.IsNotNull(result.Tables["OrderEntity"]);
			Assert.IsNotNull(result.Tables["EmployeeEntity"]);
			Assert.IsNotNull(result.Tables["OrderDetailsEntity"]);
			Assert.IsNotNull(result.Tables["ProductEntity"]);

			Assert.AreEqual(customerProjections.Count, result.Tables["CustomerEntity"].Columns.Count);
			Assert.AreEqual(orderProjections.Count, result.Tables["orderEntity"].Columns.Count);
			Assert.AreEqual(employeeProjections.Count, result.Tables["employeeEntity"].Columns.Count);
			Assert.AreEqual(orderDetailsProjections.Count, result.Tables["orderDetailsEntity"].Columns.Count);
			Assert.AreEqual(productProjections.Count, result.Tables["productEntity"].Columns.Count);

			Assert.AreEqual(11, result.Tables["CustomerEntity"].Rows.Count);

			Dictionary<Type, IEntityCollection2> projectionResults = new Dictionary<Type, IEntityCollection2>();
			customers.CreateHierarchicalProjection(projectionData, projectionResults);
			Assert.AreEqual(5, projectionResults.Count);
			Assert.AreEqual(result.Tables["CustomerEntity"].Rows.Count, projectionResults[typeof(CustomerEntity)].Count);
			Assert.AreEqual(result.Tables["OrderEntity"].Rows.Count, projectionResults[typeof(OrderEntity)].Count);
			Assert.AreEqual(result.Tables["EmployeeEntity"].Rows.Count, projectionResults[typeof(EmployeeEntity)].Count);
			Assert.AreEqual(result.Tables["OrderDetailsEntity"].Rows.Count, projectionResults[typeof(OrderDetailsEntity)].Count);
			Assert.AreEqual(result.Tables["ProductEntity"].Rows.Count, projectionResults[typeof(ProductEntity)].Count);
		}


		[Test]
		public void EntityViewInMemoryFilteringOnNullTest()
		{
			EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(employees, null);
			}

			Assert.AreEqual(9, employees.Count);
			EntityView2<EmployeeEntity> employeesView = new EntityView2<EmployeeEntity>(employees);
			Assert.AreEqual(9, employeesView.Count);
			employeesView.Filter = new PredicateExpression(EmployeeFields.ReportsTo == DBNull.Value);
			Assert.AreEqual(1, employeesView.Count);
		}



		[Test]
		public void NestedDbFunctionCallTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(OrderDetailsFields.OrderId, 0);
			fields.DefineField(new EntityField2("Result", new DbFunctionCall("ROUND", 
					new object[] { new DbFunctionCall("ROUND", new object[] { (OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity), -1 }), -1})), 1);
			DataTable results = new DataTable();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, new RelationPredicateBucket(OrderDetailsFields.OrderId == 10248));
			}

			Assert.AreEqual(results.Rows[0][1], 170.0M);
			Assert.AreEqual(results.Rows[1][1], 100.0M);
			Assert.AreEqual(results.Rows[2][1], 170.0M);
		}


		[Test]
		public void InMemoryNullDateFilterTest()
		{
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>();

			OrderEntity newOrder = new OrderEntity();
			newOrder.OrderDate = null;
			DateTime date = DateTime.Now;
			newOrder.RequiredDate = date;

			orders.Add(newOrder);

			EntityView2<OrderEntity> view = orders.DefaultView;
			view.Filter = (OrderFields.OrderDate > DateTime.Now);
			Assert.AreEqual(0, view.Count);

			view.Filter = (OrderFields.RequiredDate == date);

			Assert.AreEqual(1, view.Count);

			view.Filter = null;
			Assert.AreEqual(1, view.Count);
			view.Sorter = new SortExpression(OrderFields.OrderDate | SortOperator.Ascending);
		}
		

		[Test]
		public void UpdatingCollectionWithSortedAndFilteredView()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>();
			DataAccessAdapter adapter = new DataAccessAdapter();
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);
			adapter.FetchEntityCollection(customers, new RelationPredicateBucket(CustomerFields.Country == "USA"), path);
			CustomerEntity customer = customers[0];
			EntityView2<OrderEntity> ordersView = customers[0].Orders.DefaultView;
			Assert.AreEqual(11, ordersView.Count);

			ordersView.Sorter = new SortExpression(OrderFields.OrderDate | SortOperator.Ascending);
			ordersView.Filter = new PredicateExpression(OrderFields.EmployeeId == 4);
			Assert.AreEqual(3, ordersView.Count);

			OrderEntity toAdd = new OrderEntity();
			toAdd.EmployeeId = 4;
			toAdd.OrderDate = DateTime.Now;
			customer.Orders.Add(toAdd);
			Assert.AreEqual(4, ordersView.Count);

			// also test it with a collection which isn't contained inside a collection.
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>();
			adapter.FetchEntityCollection(orders, new RelationPredicateBucket(OrderFields.CustomerId == "GREAL"));
			EntityView2<OrderEntity> ordersView2 = orders.DefaultView;
			ordersView2.Sorter = new SortExpression(OrderFields.OrderDate | SortOperator.Ascending);
			ordersView2.Filter = new PredicateExpression(OrderFields.EmployeeId == 4);
			Assert.AreEqual(3, ordersView2.Count);
			OrderEntity toAdd2 = new OrderEntity();
			toAdd2.EmployeeId = 4;
			toAdd2.OrderDate = DateTime.Now;
			orders.Add(toAdd2);
			Assert.AreEqual(4, ordersView2.Count);
		}


		[Test]
		public void IDataErrorInfoXmlSerializationTest()
		{
			OrderEntity o = new OrderEntity(10254);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntity(o);
			}

			IDataErrorInfo oError = o as IDataErrorInfo;
			o.SetEntityError("EntityError for o: Id > 10 & smaller than nothin'");
			o.SetEntityFieldError("OrderId", "Error message for OrderId", false);
			o.SetEntityFieldError("CustomerId", "Error message for CustomerId", false);
			o.SetEntityFieldError("CustomerId", "Second line of error message for CustomerId", true);

			string xml = string.Empty;
			o.WriteXml(out xml);
			StreamWriter writer = new StreamWriter("OrderXml.xml");
			writer.Write(xml);
			writer.Close();

			OrderEntity oFromXml = new OrderEntity();
			oFromXml.ReadXml(xml);
			IDataErrorInfo oFromXmlError = oFromXml as IDataErrorInfo;
			Assert.AreEqual(oError.Error, oFromXmlError.Error);
			Assert.AreEqual(oError["OrderId"], oFromXmlError["OrderId"]);
			Assert.AreEqual(oError["CustomerId"], oFromXmlError["CustomerId"]);

			Assert.AreEqual(oError["OrderId"], "Error message for OrderId");
		}


		[Test]
		public void ScalarQueryWithoutTableReferenceTest()
		{
			ResultsetFields fields = new ResultsetFields(1);
			fields[0] = new EntityField2("UpperCaseString", new DbFunctionCall("UPPER", new object[] { "test string" }));

			string toConvert = "test string";
			string result = string.Empty;
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				result = (string)adapter.GetScalar(new EntityField2("UpperCaseString", new DbFunctionCall("UPPER", new object[] { toConvert })), AggregateFunction.None);
			}
			Assert.AreEqual(toConvert.ToUpperInvariant(), result);
		}


		[Test]
		public void IListFieldCompareRangeTest()
		{
			List<int> range = new List<int>();
			range.AddRange(new int[] { 10254, 10255, 10256, 10257 });
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>(new OrderEntityFactory());
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(orders, new RelationPredicateBucket(OrderFields.OrderId==range));
			}

			Assert.AreEqual(4, orders.Count);
		}


		[Test]
		public void ReferenceDeferenceTest()
		{
			CustomerEntity customer = new CustomerEntity("BLONP");
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntity(customer);
			}

			OrderEntity order = new OrderEntity();
			order.Customers = customer;
			Assert.AreEqual("BLONP", order.CustomerId);
			Assert.AreEqual(customer, order.Customers);

			order.CustomerId = "BLONP";
			// shouldn't dereference
			Assert.AreEqual(customer, order.Customers);

			order.CustomerId = "CHOPS";
			// should dereference
			Assert.IsNull(order.Customers);

			// reset reference
			order.Customers = customer;
			Assert.AreEqual("BLONP", order.CustomerId);
			Assert.AreEqual(customer, order.Customers);
		}


		/// <summary>
		/// Tests the new ScalarQueryExpression object by fetching 4 fields from customer and appending 2 scalar expression subqueries:
		/// the # of orders and the latest order date.
		/// </summary>
		[Test]
		public void ScalarQueryExpressionTest()
		{
			ResultsetFields fields = new ResultsetFields( 6 );
			fields.DefineField( CustomerFields.CustomerId, 0 );
			fields.DefineField( CustomerFields.CompanyName, 1 );
			fields.DefineField( CustomerFields.City, 2 );
			fields.DefineField( CustomerFields.Region, 3 );
			fields.DefineField( new EntityField2( "NumberOfOrders",
								new ScalarQueryExpression( OrderFields.OrderId.SetAggregateFunction( AggregateFunction.Count ),
									(CustomerFields.CustomerId == OrderFields.CustomerId) ) ), 4 );
			fields.DefineField( new EntityField2( "LastOrderDate",
								new ScalarQueryExpression( OrderFields.OrderDate.SetAggregateFunction( AggregateFunction.Max ),
									(CustomerFields.CustomerId == OrderFields.CustomerId))), 5);

			DataTable results = new DataTable();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchTypedList(fields, results, new RelationPredicateBucket(CustomerFields.Country=="USA"), 0, 
						new SortExpression(CustomerFields.CompanyName | SortOperator.Ascending), true);
			}

			Assert.AreEqual(13, results.Rows.Count);
			foreach(DataRow row in results.Rows)
			{
				Assert.IsTrue(row["NumberOfOrders"].ToString().Length > 0);
				Assert.IsTrue(row["LastOrderDate"].ToString().Length > 0);
			}
		}


		[Test]
		public void PrefetchPathFetchTestWithNullFKFields()
		{
			PrefetchPath2 path = new PrefetchPath2( (int)EntityType.EmployeeEntity );
			path.Add( EmployeeEntity.PrefetchPathRegion_ );

			EntityCollection employees = new EntityCollection( new EmployeeEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( employees, null, path );
			}

			Assert.AreEqual( 9, employees.Count );
			int i = 0;
			foreach( EmployeeEntity employee in employees )
			{
				if( employee.Region_ == null )
				{
					i++;
				}
			}

			Assert.AreEqual( 5, i );
		}


		[Test]
		public void PrefetchPathFetchInsideTransaction()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				PrefetchPath2 path = new PrefetchPath2( (int)EntityType.OrderDetailsEntity );
				path.Add( OrderDetailsEntity.PrefetchPathOrders );

				OrderDetailsEntity orderDetail = new OrderDetailsEntity( 10254, 13 );
				adapter.StartTransaction( IsolationLevel.ReadCommitted, "Test" );
				try
				{
					adapter.FetchEntity( orderDetail, path );
					adapter.Commit();
				}
				catch
				{
					adapter.Rollback();
					throw;
				}
			}
		}

		[Test]
		public void EntityViewTest()
		{
			ProductEntity p1 = new ProductEntity();
			ProductEntity p2 = new ProductEntity();
			ProductEntity p3 = new ProductEntity();

			p1.ProductName = "Product 1";
			p2.ProductName = "Product 2";
			p3.ProductName = "Product 3";

			EntityCollection<ProductEntity> list = new EntityCollection<ProductEntity>();

			list.Add( p1 );
			list.Add( p2 );
			list.Add( p3 );

			Assert.AreSame( p1, list[0] );
			Assert.AreSame( p2, list[1] );
			Assert.AreSame( p3, list[2] );

			Assert.AreSame( p1, list.DefaultView[0] );
			Assert.AreSame( p2, list.DefaultView[1] );
			Assert.AreSame( p3, list.DefaultView[2] );

			list.RemoveAt( 1 );
			list.Insert( 0, p2 );

			Assert.AreSame( p1, list[1] );
			Assert.AreSame( p2, list[0] );
			Assert.AreSame( p3, list[2] );

			// the view adds new entities at the end of the view, if no sorter is specified. This is standard behavior.
			Assert.AreSame( p1, list.DefaultView[1] );
			Assert.AreSame( p2, list.DefaultView[0] );
			Assert.AreSame( p3, list.DefaultView[2] );
		}


		/// <summary>
		/// A m:1 B prefetch path over A.Bs.As and sort on As. 
		/// </summary>
		[Test]
		public void SortSubPathPrefetchPathTest()
		{
			OrderEntity order = new OrderEntity( 10254 );
			PrefetchPath2 path = new PrefetchPath2( (int)EntityType.OrderEntity );
			SortExpression sorter = new SortExpression();
			sorter.Add( OrderFields.EmployeeId | SortOperator.Ascending );
			sorter.Add( OrderFields.RequiredDate | SortOperator.Ascending );

			Context c = new Context();
			order.ActiveContext = c;

			path.Add( OrderEntity.PrefetchPathCustomers ).SubPath.Add( CustomerEntity.PrefetchPathOrders, 0, null, null, sorter );

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntity( order, path );
			}

			Assert.AreEqual( 8, order.Customers.Orders.Count );
			Assert.AreEqual( order, order.Customers.Orders[0] );
			Assert.IsNotNull( order.Customers.Orders[0].Customers );
		}


		[Test]
		[ExpectedException(typeof(ORMInvalidFieldReadException))]
		public void IllegalFieldReadTest()
		{
			try
			{
				EntityBase2.MakeInvalidFieldReadsFatal = true;
				CustomerEntity c = new CustomerEntity();
				string customerID = c.CustomerId;
			}
			finally
			{
				EntityBase2.MakeInvalidFieldReadsFatal = false;
			}
		}


		public delegate void ActionRoutine_DataBindingWithNewRowThroughCodeTestCallBack( EntityCollection<OrderEntity> orders );
		/// <summary>
		/// Action routine belonging to DataBindingWithNewRowThroughCodeTest. Adds a new entity to the passed in collection (Order)
		/// </summary>
		/// <param name="parameters">The parameters. Should contain 1 item, the entity collection</param>
		public void ActionRoutine_DataBindingWithNewRowThroughCodeTest(EntityCollection<OrderEntity> orders)
		{
			orders.Add( new OrderEntity() );
		}


		/// <summary>
		/// Test which checks whether there are duplicate values in the IN query for the subpath. This shouldn't be the case. 
		/// </summary>
		[Test]
		public void DuplicateValueFilterPrefetchPathTest()
		{
			EntityCollection<OrderDetailsEntity> orderDetails = new EntityCollection<OrderDetailsEntity>( new OrderDetailsEntityFactory() );
			PrefetchPath2 path = new PrefetchPath2( (int)EntityType.OrderDetailsEntity );
			path.Add( OrderDetailsEntity.PrefetchPathProducts );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.ParameterisedPrefetchPathThreshold = 100;
				adapter.FetchEntityCollection( orderDetails, null, 50, null, path );
			}
		}


		[Test]
		public void IndexOfTest()
		{
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>( new OrderEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( orders, null, 10 );
			}
			OrderEntity reference = orders[5];

			Assert.AreEqual( 5, orders.IndexOf( reference ) );
			orders.Remove( orders[1] );
			Assert.AreEqual( 4, orders.IndexOf( reference ) );
			orders.RemoveAt( 1 );
			Assert.AreEqual( 3, orders.IndexOf( reference ) );
			orders.Insert( 2, new OrderEntity() );
			Assert.AreEqual( 4, orders.IndexOf( reference ) );
			orders.Add( new OrderEntity() );
			Assert.AreEqual( 4, orders.IndexOf( reference ) );
		}


		/// <summary>
		/// Tests a dyn fetch with various function calls.
		/// </summary>
		[Test]
		public void DbFunctionCallTest1()
		{
			ResultsetFields fields = new ResultsetFields( 8 );
			fields.DefineField( OrderFields.OrderId, 0, "OrderID" );
			fields.DefineField( OrderFields.OrderDate, 1, "Month" );
			fields.DefineField( OrderFields.OrderDate, 2, "Year" );
			fields.DefineField( OrderFields.OrderDate, 3, "YearPlus4" );
			fields.DefineField( OrderFields.OrderDate, 4, "OrderTotalWithDiscounts" );
			fields.DefineField( OrderFields.OrderDate, 5, "OrderTotalWithoutDiscounts" );
			fields.DefineField( OrderFields.OrderDate, 6, "YearOfGetDate" );
			fields.DefineField( OrderFields.OrderDate, 7, "RealOrderDate" );
			
			fields[1].ExpressionToApply = new DbFunctionCall( "MONTH", new object[] { OrderFields.OrderDate } );
			fields[2].ExpressionToApply = new DbFunctionCall( "YEAR", new object[] { OrderFields.OrderDate } );
			fields[3].ExpressionToApply = new Expression( new DbFunctionCall( "YEAR", new object[] { OrderFields.OrderDate } ), ExOp.Add, 4 );
			fields[4].ExpressionToApply = new DbFunctionCall( "dbo", "fn_CalculateOrderTotal", new object[] { OrderFields.OrderId, 1 } );
			fields[5].ExpressionToApply = new DbFunctionCall( "dbo", "fn_CalculateOrderTotal", new object[] { OrderFields.OrderId, 0 } );
			fields[6].ExpressionToApply = new DbFunctionCall( "YEAR", new object[] { new DbFunctionCall( "GETDATE", null )  } );

			DataTable results = new DataTable();
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchTypedList( fields, results, null );
			}
			foreach( DataRow row in results.Rows )
			{
				DateTime realOrderDate = (DateTime)row[7];
				Assert.AreEqual( (int)row[1], realOrderDate.Month );
				Assert.AreEqual( (int)row[2], realOrderDate.Year );
				Assert.AreEqual( (int)row[3], realOrderDate.Year + 4 );
				Assert.AreEqual( (int)row[6], DateTime.Now.Year );
			}
		}


		[Test]
		public void CaseSensitiveCompareValueTest()
		{
			FieldCompareValuePredicate filter = (FieldCompareValuePredicate)(CustomerFields.Country == "GERMANY");
			filter.CaseSensitiveCollation = true;

			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( customers, new RelationPredicateBucket( filter ) );
			}

			EntityView2<CustomerEntity> view = new EntityView2<CustomerEntity>( customers );
			view.Filter = filter;
			Assert.AreEqual( 11, view.Count );
			view.Filter = null;
			filter.CaseSensitiveCollation = false;
			view.Filter = filter;
			Assert.AreEqual( 0, view.Count );
		}


		/// <summary>
		/// Test which uses a function call in a filter. 
		/// </summary>
		[Test]
		public void DbFunctionCallTest2()
		{
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>( new OrderEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( orders, new RelationPredicateBucket(
					(new EntityField2("fcall", new DbFunctionCall( "dbo", "fn_CalculateOrderTotal", new object[] { OrderFields.OrderId, 1 } )) > 4000.0)));
			}
			Assert.AreEqual( 64, orders.Count );
		}


		[Test]
		public void TypedListRelationWithSelfTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EmployeeWithSelfTypedList tlist = new EmployeeWithSelfTypedList();
				adapter.FetchTypedList(tlist.GetFieldsInfo(), tlist, tlist.GetRelationInfo());

				Assert.AreEqual(12, tlist.Rows.Count);
			}
		}


		[Test]
		public void FetchTypedViewWithTypeConverterTest()
		{
			using(DataAccessAdapter	adapter = new DataAccessAdapter())
			{
				TypeConverterTestTvTypedView testTV = new TypeConverterTestTvTypedView();
				adapter.FetchTypedView(testTV);
				foreach(TypeConverterTestTvRow row in testTV.Rows)
				{
					Assert.AreEqual(typeof(bool), row.ShipVia.GetType());
				}
			}
		}

		
		[Test]
		public void CustomFilterReplaceOnClauseTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				EntityCollection customers = new EntityCollection( new CustomerEntityFactory() );
				RelationPredicateBucket filter = new RelationPredicateBucket();
				IEntityRelation relation = CustomerEntity.Relations.OrderEntityUsingCustomerId;
				relation.CustomFilter = new PredicateExpression( CustomerFields.CustomerId == OrderFields.CustomerId );
				relation.CustomFilterReplacesOnClause = true;
				filter.Relations.Add( relation );
				filter.PredicateExpression.Add( OrderFields.EmployeeId == 2 );

				adapter.FetchEntityCollection( customers, filter );
				Assert.AreEqual( 62, customers.Count );
			}
		}


		[Test]
		public void FieldAliasWithExpressionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ResultsetFields fields = new ResultsetFields(1);
				fields.DefineField(CustomerFields.CompanyName, 0, "CompanyName");
				IEntityField2 field = fields[0];
				Expression toApply = (CustomerFields.CompanyName + ":");
				field.ExpressionToApply = toApply;

				DataTable table = new DataTable();
				adapter.FetchTypedList(fields, table, null);

				Assert.AreEqual("CompanyName", table.Columns[0].ColumnName);
			}
		}


		[Test]
		public void CrossJoinHintTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(ProductEntity.Relations.SupplierEntityUsingSupplierId, JoinHint.Cross);
				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(ProductFields.ProductId, 0, "ProductID");
				fields.DefineField(SupplierFields.Id, 1, "SupplierID");

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);
				Assert.AreEqual(2233, results.Rows.Count);
			}
		}


		[Test]
		public void FieldCompareRangeTestWithExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ArrayList values = new ArrayList();
				values.Add(4);
				values.Add(6);
				RelationPredicateBucket filter = new RelationPredicateBucket( OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == values);
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);
				Assert.AreEqual(574, orders.Count);
			}
		}


		[Test]
		public void FieldCompareValueTestWithExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket( 
					OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == 6);
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);
				Assert.AreEqual(254, orders.Count);
			}
		}


		[Test]
		public void FieldCompareExpressionTestWithExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket( 
						OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == OrderFields.EmployeeId);
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);
				Assert.AreEqual(135, orders.Count);
			}
		}


		[Test]
		public void FieldCompareSetTestWithExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket(new FieldCompareSetPredicate(
						OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2), null,
						OrderFields.EmployeeId.SetObjectAlias("O"), null, SetOperator.In, 
						(OrderFields.OrderId.SetObjectAlias("O")==OrderFields.OrderId)));

				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);
				Assert.AreEqual(135, orders.Count);
			}
		}


		[Test]
		public void FetchNewEntityWithPrefetchPathTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket((CustomerFields.CustomerId == "CHOPS"));
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);
				CustomerEntity customer = (CustomerEntity)adapter.FetchNewEntity(new CustomerEntityFactory(), filter, path);
				Assert.AreEqual(EntityState.Fetched, customer.Fields.State);
				Assert.AreEqual(8, customer.Orders.Count);
			}
		}


		[Test]
		public void FetchNewEntityWithPrefetchPathAndContextTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// first fetch CHOPS and add it to a context
				Context c = new Context();
				CustomerEntity chops = new CustomerEntity("CHOPS");
				Assert.IsTrue(adapter.FetchEntity(chops, c));
				Assert.AreEqual(c, chops.ActiveContext);

				RelationPredicateBucket filter = new RelationPredicateBucket((CustomerFields.CustomerId == "CHOPS"));
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);
				CustomerEntity customer = (CustomerEntity)adapter.FetchNewEntity(new CustomerEntityFactory(), filter, path, c);
				Assert.AreEqual(EntityState.Fetched, customer.Fields.State);
				Assert.AreEqual(8, customer.Orders.Count);
				Assert.AreEqual(chops, customer);
				Assert.AreSame(chops, customer);
			}
		}



		[Test]
		public void FieldBetweenPredicateTest()
		{
			RelationPredicateBucket filter = new RelationPredicateBucket(new FieldBetweenPredicate(OrderFields.EmployeeId, null, 2, 4));
			EntityCollection orders = new EntityCollection(new OrderEntityFactory());
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(orders, filter);
				Assert.IsTrue((orders.Count>0));

				foreach(OrderEntity o in orders)
				{
					Assert.IsTrue(((o.EmployeeId>=2) && (o.EmployeeId<=4)));
				}

				filter = new RelationPredicateBucket(new FieldBetweenPredicate(OrderFields.ShippedDate, null, OrderFields.OrderDate, OrderFields.RequiredDate, true));
				orders.Clear();
				adapter.FetchEntityCollection(orders, filter);
				Assert.AreEqual(38, orders.Count);
			}
		}


		[Test]
		public void FetchSingleEntityWithUCTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity customer1 = new CustomerEntity();
				customer1.CompanyName = "The Big Cheese";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(customer1, customer1.ConstructFilterForUCCompanyName()));
			}
		}


		[Test]
		public void FetchSingleEntityWithContextAndUCTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity customer1 = new CustomerEntity();
				customer1.CompanyName = "The Big Cheese";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(customer1, customer1.ConstructFilterForUCCompanyName()));
				Context c = new Context();
				c.Add(customer1);

				CustomerEntity customer2 = new CustomerEntity();
				customer2.CompanyName = "The Big Cheese";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(customer2, customer2.ConstructFilterForUCCompanyName(), c));
				Assert.AreEqual(customer1, customer2);
				Assert.AreSame(customer1.Fields, customer2.Fields);
			}
		}


		[Test]
		public void FetchSingleEntityWithContextTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity customer1 = new CustomerEntity("CHOPS");
				Assert.IsTrue(adapter.FetchEntity(customer1));
				Context c = new Context();
				c.Add(customer1);

				CustomerEntity customer2 = new CustomerEntity("CHOPS");
				Assert.IsTrue(adapter.FetchEntity(customer2, c));
				Assert.AreEqual(customer1, customer2);
				Assert.AreSame(customer1.Fields, customer2.Fields);
			}
		}


		[Test]
		public void ComplexExpressionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// SUM((UnitPrice*Quantity)-(UnitPrice*Quantity*Discount/100)) / SUM(Quantity) FROM [Order Details]
				// UnitPrice * Quantity
				IExpression eP1 = OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity;
				// (UnitPrice * Quantity) * Discount
				IExpression eP2 = new Expression(eP1, ExOp.Mul, OrderDetailsFields.Discount);
				// ((UnitPrice * Quantity) * Discount) / 100
				IExpression eP3 = new Expression(eP2, ExOp.Div, 100);
				// (UnitPrice * Quantity)-((UnitPrice * Quantity) * Discount) / 100
				IExpression eP4 = new Expression(eP1, ExOp.Sub, eP3);

				// sum((UnitPrice * Quantity)-((PrecioLocal * Quantity) * Discount) / 100)
				IEntityField2 op1= OrderDetailsFields.UnitPrice;
				op1.ExpressionToApply=eP4;
				op1.AggregateFunctionToApply=AggregateFunction.Sum;

				// sum(Quantity)
				IEntityField2 op2=OrderDetailsFields.Quantity;
				op2.AggregateFunctionToApply=AggregateFunction.Sum;

				// sum((UnitPrice * Quantity)-((UnitPrice * Quantity) * Discount) / 100) / sum(Quantity)
				IExpression expRes=new Expression(op1, ExOp.Div, op2);

				IPredicate filter = OrderDetailsFields.OrderId==10254;
				object r=adapter.GetScalar(OrderDetailsFields.UnitPrice, expRes, AggregateFunction.None, filter);
				Decimal result = Convert.ToDecimal( r );
				Assert.AreEqual( 7.88685265796607M, result );
			}
		}


		[Test]
		public void GetLastOrderForCustomerFetchTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());

				IEntityField2 field = OrderFields.OrderDate;
				IEntityField2 subQField = OrderFields.OrderDate;
				subQField.AggregateFunctionToApply = AggregateFunction.Max;
				subQField.ObjectAlias = "o";
				PredicateExpression subQFilter = new PredicateExpression();
				subQFilter.Add(OrderFields.CustomerId == OrderFields.CustomerId.SetObjectAlias("o"));
				FieldCompareSetPredicate filter = new FieldCompareSetPredicate(field, null,
						subQField, null, SetOperator.Equal, subQFilter);
				RelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.PredicateExpression.Add(filter);
				adapter.FetchEntityCollection(orders, bucket, 0);
				Assert.AreEqual(90, orders.Count);
			}
		}

		[Test]
		public void FetchNewEntityTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.CustomerId == "CHOPS");
				filter.PredicateExpression.AddWithAnd(CustomerFields.City == "Unknown");

				CustomerEntity chops = (CustomerEntity)adapter.FetchNewEntity(new CustomerEntityFactory(), filter);
				Assert.AreEqual(EntityState.OutOfSync, chops.Fields.State);
			}
		}


		[Test]
		public void PkFetchTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				OrderEntity o = new OrderEntity(10254);
				Assert.IsTrue(adapter.FetchEntity(o));
			}
		}


		[Test]
		public void DereferenceTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				_dereferenceTestFlag = false;
				CustomerEntity c = new CustomerEntity("CHOPS");
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);

				adapter.FetchEntity(c, path);

				OrderEntity o = (OrderEntity)c.Orders[0];
				o.EntityContentsChanged+=new EventHandler(o_EntityContentsChanged);
				Assert.IsFalse(_dereferenceTestFlag);
				o.CustomerId = "BLONP";
				Assert.IsTrue(_dereferenceTestFlag);
				Assert.IsNull(o.Customers);
			}
		}


		[Test]
		public void OptimizingPrefetchPathsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity c = new CustomerEntity("CHOPS");
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);

				adapter.FetchEntity(c, path);
			}
		}

		/// <summary>
		/// Tests the fetch of a normal typed list, generated by llblgen pro and the designer.
		/// Using a normal filter on Orders of 'CACTU', which should result in 6 rows.
		/// </summary>
		[Test]
		public void NormalTypedListFetchTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
				IRelationPredicateBucket filter = orderCustomer.GetRelationInfo();
				filter.PredicateExpression.Add(new FieldCompareValuePredicate(OrderFields.CustomerId, null, 
					ComparisonOperator.Equal, "CACTU"));
				adapter.FetchTypedList(orderCustomer.GetFieldsInfo(), orderCustomer, filter);
				Assert.AreEqual(6, orderCustomer.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the group by clause functionality.
		/// </summary>
		[Test]
		public void GroupByTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
				IRelationPredicateBucket filter = orderCustomer.GetRelationInfo();
				filter.PredicateExpression.Add(new FieldCompareValuePredicate(OrderFields.CustomerId, null, 
					ComparisonOperator.Equal, "CACTU"));
				// create group by clause
				IGroupByCollection groupByClause = new GroupByCollection();
				EntityFields2 fields = (EntityFields2)orderCustomer.GetFieldsInfo();

				for (int i = 0; i < fields.Count; i++)
				{
					groupByClause.Add(fields[i]);
				}

				adapter.FetchTypedList(orderCustomer.GetFieldsInfo(), orderCustomer, filter, 0, null, true, groupByClause);
				Assert.AreEqual(6, orderCustomer.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}
		
		[Test]
		public void GroupByWithDynamicListTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
				fields.DefineField(OrderDetailsFields.UnitPrice, 1, "Total");

				IGroupByCollection groupByClause = new GroupByCollection();

				groupByClause.Add( fields[0] );
				// construct Having filter. 
				// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) )
				groupByClause.HavingClause = new PredicateExpression(
					fields[1]
					.SetExpression(
					(OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity) - 
					((OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity) * OrderDetailsFields.Discount))
					.SetAggregateFunction(AggregateFunction.Sum)
					> 1000.0f);
				DataTable orderTotals = new DataTable();
				adapter.FetchTypedList(fields, orderTotals, null, 0, null, true, groupByClause);
				Assert.AreEqual(386, orderTotals.Rows.Count);
				foreach(DataRow row in orderTotals.Rows)
				{
					Assert.IsTrue(Convert.ToDecimal(row["Total"]) > 1000.0M);
				}
			}
		}


		[Test]
		public void GroupByTestCustomList()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(3);

			fields.DefineField(EmployeeFields.FirstName, 0, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 1, "LastNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 2, "AmountEmployees", "Employee", AggregateFunction.Count);
			IRelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "Employee", "Manager", JoinHint.None);
			filter.PredicateExpression.Add(EmployeeFields.LastName.SetObjectAlias("Manager") == "Buchanan");
			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			groupByClause.Add(fields[1]);

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, filter, 0, new SortExpression(new SortClause(fields[2], null, SortOperator.Ascending)), true, groupByClause);

				Assert.AreEqual(1, tlist.Rows.Count);
				Assert.AreEqual(2, (int)tlist.Rows[0]["AmountEmployees"]);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the group by clause functionality with an expression using joins and aliasses
		/// </summary>
		[Test]
		public void GroupByTestCustomListWithExpression()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(3);

			fields.DefineField(EmployeeFields.FirstName, 0, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 1, "LastNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 2, "MaxEmployeeId", "EmployeeAlias", AggregateFunction.Max);

			// Add expression: EmployeeId + 10
			IEntityField2 field = EmployeeFields.EmployeeId;
			field.ObjectAlias = "EmployeeAlias";
			IExpression maxPlusTen = new Expression(
				field,
				ExOp.Add,
				10);
			fields[2].ExpressionToApply = maxPlusTen;

			IRelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "EmployeeAlias", "Manager", JoinHint.None);
			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			groupByClause.Add(fields[1]);

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, filter, 0, null, true, groupByClause);

				Assert.AreEqual(17, (int)tlist.Rows[0]["MaxEmployeeId"]);
				Assert.AreEqual(18, (int)tlist.Rows[1]["MaxEmployeeId"]);
				Assert.AreEqual(5, tlist.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void GroupByOnExpressionTest()
		{
			ResultsetFields fields = new ResultsetFields( 2 );
			fields.DefineField( OrderFields.OrderDate, 0, "MonthOrder" );
			fields.DefineField( OrderFields.OrderId, 1, "NumberOfOrdersInMonth" );
			fields[0].ExpressionToApply = new DbFunctionCall( "month", new object[] { OrderFields.OrderDate } );
			fields[1].AggregateFunctionToApply = AggregateFunction.Count;

			SortExpression sorter = new SortExpression( new SortClause(fields[0], null,  SortOperator.Ascending ));
			GroupByCollection groupBy = new GroupByCollection();
			groupBy.Add( fields[0] );

			DataTable results = new DataTable();
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchTypedList( fields, results, null, 0, sorter, true, groupBy );
			}

			Assert.AreEqual( 12, results.Rows.Count );
			for( int i = 0; i < 12; i++ )
			{
				Assert.AreEqual( (i + 1), (int)results.Rows[i][0] );
				switch( i+1 )
				{
					// test some values
					case 2:
						Assert.AreEqual( 82, (int)results.Rows[i][1] );
						break;
					case 6:
						Assert.AreEqual( 29, (int)results.Rows[i][1] );
						break;
					case 8:
						Assert.AreEqual( 57, (int)results.Rows[i][1] );
						break;
					case 11:
						Assert.AreEqual( 58, (int)results.Rows[i][1] );
						break;
					case 12:
						Assert.AreEqual( 79, (int)results.Rows[i][1] );
						break;

				}
			}
		}


		/// <summary>
		/// Tests simple expressions on a single entity. (orderdetail)
		/// </summary>
		[Test]
		public void ExpressionTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
			fields.DefineField(OrderDetailsFields.ProductId, 1, "ProductId");
			fields.DefineField(OrderDetailsFields.UnitPrice, 2, "TotalPrice");	// use unitprice field because it is already of the type we want.
			fields.DefineField(OrderDetailsFields.UnitPrice, 3, "UnitPrice");
			fields.DefineField(OrderDetailsFields.Quantity, 4, "Quantity");
			fields.DefineField(OrderDetailsFields.Discount, 5, "Discount");

			// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) )
			IExpression productPriceExpression = OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity;
			IExpression discountExpression = new Expression(productPriceExpression, ExOp.Mul, OrderDetailsFields.Discount);
			IExpression totalPriceExpression = new Expression(productPriceExpression, ExOp.Sub, discountExpression);
			fields[2].ExpressionToApply = totalPriceExpression;

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, null, true);

				// test the values.
				for (int i = 0; i < tlist.Rows.Count; i++)
				{
					float productPrice = (float)(((decimal)tlist.Rows[i]["UnitPrice"]) * ((Int16)tlist.Rows[i]["Quantity"]));
					float totalPrice = productPrice - (productPrice * (float)tlist.Rows[i]["Discount"]);

					Assert.AreEqual( Math.Round(Convert.ToDecimal(tlist.Rows[i]["TotalPrice"]), 3), Math.Round(Convert.ToDecimal(totalPrice), 3) );
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void ExpressionTestSimpleGroupByAggregateHaving()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(2);

			fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
			fields.DefineField(OrderDetailsFields.UnitPrice, 1, "TotalPrice");	// use unitprice field because it is already of the type we want.

			// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) )
			IExpression productPriceExpression = OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity;
			IExpression discountExpression = new Expression(productPriceExpression, ExOp.Mul, OrderDetailsFields.Discount);
			IExpression totalPriceExpression = new Expression(
				productPriceExpression,
				ExOp.Sub,
				discountExpression);
			fields[1].ExpressionToApply = totalPriceExpression;
			fields[1].AggregateFunctionToApply = AggregateFunction.Sum;
			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			IPredicateExpression havingFilter = new PredicateExpression();
			havingFilter.Add(new FieldCompareValuePredicate(fields[1], null, ComparisonOperator.GreaterThan, 1000.0f));
			groupByClause.HavingClause = havingFilter;

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, null, 0, null, true, groupByClause);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests simple expressions on a single entity. (orderdetail)
		/// </summary>
		[Test]
		public void ExpressionTestSimple2()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
			fields.DefineField(OrderDetailsFields.ProductId, 1, "ProductId");
			fields.DefineField(OrderDetailsFields.UnitPrice, 2, "TotalPrice");	// use unitprice field because it is already of the type we want.
			fields.DefineField(OrderDetailsFields.UnitPrice, 3, "UnitPrice");
			fields.DefineField(OrderDetailsFields.Quantity, 4, "Quantity");
			fields.DefineField(OrderDetailsFields.Discount, 5, "Discount");

			// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) ) + 10
			IExpression productPriceExpression = OrderDetailsFields.UnitPrice* OrderDetailsFields.Quantity;
			IExpression discountExpression = new Expression(productPriceExpression, ExOp.Mul, OrderDetailsFields.Discount);
			IExpression totalPriceExpression = new Expression(
				productPriceExpression,
				ExOp.Sub,
				discountExpression);
			IExpression fullExpression = new Expression(
				totalPriceExpression, 
				ExOp.Add,
				10);
			fields[2].ExpressionToApply = fullExpression;

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, null, true);

				// test the values.
				for (int i = 0; i < tlist.Rows.Count; i++)
				{
					float productPrice = (float)(((decimal)tlist.Rows[i]["UnitPrice"]) * ((Int16)tlist.Rows[i]["Quantity"]));
					float totalPrice = (productPrice - (productPrice * (float)tlist.Rows[i]["Discount"])) + 10.0f;

					Assert.AreEqual( Math.Round(Convert.ToDecimal(tlist.Rows[i]["TotalPrice"]), 3), Math.Round(Convert.ToDecimal(totalPrice), 3) );
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}




		/// <summary>
		/// Tests a typed list fetch of a typed list constructed in code, into a normal datatable.
		/// This test uses a join with self an a filter on aliases.
		/// </summary>
		[Test]
		public void CustomConstructedTypedListFetchTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "Employee");
			fields.DefineField(EmployeeFields.FirstName, 1, "FirstName", "Employee");
			fields.DefineField(EmployeeFields.LastName, 2, "LastName", "Employee");
			fields.DefineField(EmployeeFields.EmployeeId, 3, "ManagerId", "Manager");
			fields.DefineField(EmployeeFields.FirstName, 4, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 5, "LastNameManager", "Manager");
			IRelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "Employee", "Manager", JoinHint.None);
			filter.PredicateExpression.Add(EmployeeFields.LastName.SetObjectAlias("Manager") == "Buchanan");

			try
			{
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, filter);

				// has to be reading 2 rows
				Assert.AreEqual(2, tlist.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void FieldCompareExpressionTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// fetch all orders which have a difference between orderdate and shipping date of 5 days.
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(OrderFields.ShippedDate == (OrderFields.OrderDate + 5));
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);

				Assert.AreEqual(63, orders.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void FieldCompareValueWithFieldExpressionTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// fetch all orders which have a difference between orderdate and shipping date of 5 days.
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				IExpression fieldExpression = OrderFields.ShippedDate- OrderFields.OrderDate;
				FieldCompareValuePredicate fieldFilter = new FieldCompareValuePredicate(
					OrderFields.OrderDate,	// not important which field, just pick one, is replaced with expression anyway.
					null,
					ComparisonOperator.Equal,
					5);
				fieldFilter.FieldCore.ExpressionToApply = fieldExpression;
				filter.PredicateExpression.Add(fieldFilter);
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter);

				EntityCollection<OrderEntity> orders2 = new EntityCollection<OrderEntity>( new OrderEntityFactory() );
				Assert.AreEqual(63, orders.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Sorts the collection using the Sort(index, direction) method (Orders, descending on OrderDate)
		/// </summary>
		[Test]
		public void ClientSideCollectionSortTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, null);

				orders.Sort((int)OrderFieldIndex.OrderDate, ListSortDirection.Descending);

				for (int i = 0; i < orders.Count-1; i++)
				{
					Assert.IsTrue(!( ((OrderEntity)orders[i]).OrderDate.GetValueOrDefault().CompareTo(((OrderEntity)orders[i+1]).OrderDate) < 0));
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the scalar execution functionality with a simple count(*) call
		/// </summary>
		[Test]
		public void ScalarTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				int amountOrderRows = (int)adapter.GetScalar(OrderFields.OrderId, AggregateFunction.CountRow);
				Assert.AreEqual(818, amountOrderRows);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the scalar execution functionality with a bogus expression and an aggregate:
		/// Max(EmployeeID - ShipVia) on orders
		/// </summary>
		[Test]
		public void ScalarTestExpression()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IExpression subExpression = OrderFields.EmployeeId - OrderFields.ShipVia;
				int maxValue = (int)adapter.GetScalar(OrderFields.OrderId, subExpression, AggregateFunction.Max);
				Assert.AreEqual(8, maxValue);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the GetDbCount feature: get the amount of orders for all customers from france.
		/// </summary>
		[Test]
		public void GetDbCountTestWithFilter()
		{

			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.Country == "France");
				filter.Relations.Add(OrderEntity.Relations.CustomerEntityUsingCustomerId);
				int amountOrderRows = (int)adapter.GetDbCount(new OrderEntityFactory().CreateFields(), filter, null, false);

				Assert.AreEqual(74, amountOrderRows);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void GetDbCountWithGroupBy()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				ResultsetFields fields = new ResultsetFields(3);
				fields.DefineField(OrderFields.OrderId, 0, "OrderID");
				fields.DefineField(CustomerFields.CompanyName, 1, "CompanyName");
				fields.DefineField(CustomerFields.ContactName, 2, "ContactName");

				RelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.Relations.Add(OrderEntity.Relations.CustomerEntityUsingCustomerId);
				bucket.PredicateExpression.Add(CustomerFields.CustomerId == "CACTU");

				GroupByCollection groupBy = new GroupByCollection();
				groupBy.Add(fields[1]);
				groupBy.Add(fields[2]);
				groupBy.Add(fields[0]);

				// first do the normal way, using getscalar. This returns the wrong value of '1'.
				int amountOrderRows = (int)adapter.GetScalar(fields[0], null, AggregateFunction.CountRow, bucket.PredicateExpression, groupBy, bucket.Relations);
				Assert.AreEqual(1, amountOrderRows);

				// now using the new getdbcount
				int noOfRows = adapter.GetDbCount(fields, bucket, groupBy, true);
				Assert.AreEqual(6, noOfRows);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void GetDbCountTestWithNoFilterEntityCollection()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>();
				int count = adapter.GetDbCount(orders, null);

				Assert.AreEqual(818, count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void GetDbCountTestWithEmptyFilter()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IPredicateExpression filter = new PredicateExpression();
				int amountOrderRows = (int)adapter.GetScalar(OrderFields.OrderId, null, AggregateFunction.CountRow, filter);

				Assert.AreEqual(818, amountOrderRows);

				int noOrderRows = adapter.GetDbCount(new OrderEntity().Fields, null);
				Assert.AreEqual(818, noOrderRows);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the FieldCompareSetPredicate using a simple subquery.
		/// select * from products where productid not in (select productid from [order details])
		/// or: all products which are not sold in an order.
		/// </summary>
		[Test]
		public void FieldCompareSetTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.PredicateExpression.Add(new FieldCompareSetPredicate(
						ProductFields.ProductId,	null,
						OrderDetailsFields.ProductId, null,
						SetOperator.In,
						null, 
						true));
				EntityCollection notSoldProducts = new EntityCollection(new ProductEntityFactory());
				adapter.FetchEntityCollection(notSoldProducts, bucket);

				Assert.AreEqual(3, notSoldProducts.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests a more complex usage of the fieldcompareset predicate: Get employee with the last order they closed.
		/// SELECT  E.EmployeeID, O.OrderID
		/// FROM	Employees E INNER JOIN Orders O
		///			ON E.EmployeeID = O.EmployeeID
		/// WHERE	O.OrderId =
		/// 		(
		///		 		SELECT TOP 1 OrderId FROM Orders 
		/// 			WHERE EmployeeID = E.EmployeeID
		/// 			ORDER BY OrderDate DESC
		/// 		)
		/// </summary>
		[Test]
		public void FieldCompareSetTestComplex()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(2);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "E");
			fields.DefineField(OrderFields.OrderId, 1, "OrderId", "O");

			try
			{
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.Relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId, "E", "O", JoinHint.None);
				bucket.PredicateExpression.Add(new FieldCompareSetPredicate(
					OrderFields.OrderId,	null,
					OrderFields.OrderId, null,
					SetOperator.Equal,
					new FieldCompareExpressionPredicate(
						OrderFields.EmployeeId, null,
						ComparisonOperator.Equal,
						new Expression(fields[0])),
					null, 
					"O",
					1,
					new SortExpression(OrderFields.OrderDate | SortOperator.Descending))
					);

				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, bucket);

				Assert.AreEqual(9, tlist.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Test similar to FieldCompareSetTestComplex, but now tests the Expand() feature of an entityfields object.
		/// Tests a more complex usage of the fieldcompareset predicate: Get employee with the last order they closed.
		/// SELECT  E.EmployeeID, O.OrderID
		/// FROM	Employees E INNER JOIN Orders O
		///			ON E.EmployeeID = O.EmployeeID
		/// WHERE	O.OrderId =
		/// 		(
		///		 		SELECT TOP 1 OrderId FROM Orders 
		/// 			WHERE EmployeeID = E.EmployeeID
		/// 			ORDER BY OrderDate DESC
		/// 		)
		/// </summary>
		[Test]
		public void FieldCompareSetTestComplexExpandedFieldsObject()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(1);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "E");

			// add a cell.
			fields.Expand(1);
			fields.DefineField(OrderFields.OrderId, 1, "OrderId", "O");

			try
			{
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.Relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId, "E", "O", JoinHint.None);
				bucket.PredicateExpression.Add(new FieldCompareSetPredicate(
					OrderFields.OrderId,	null,
					OrderFields.OrderId, null,
					SetOperator.Equal,
					new FieldCompareExpressionPredicate(
					OrderFields.EmployeeId, null,
					ComparisonOperator.Equal,
					new Expression(fields[0])),
					null, 
					"O",
					1,
					new SortExpression(OrderFields.OrderDate | SortOperator.Descending)
					));

				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, bucket);

				Assert.AreEqual(9, tlist.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}
		
		/// <summary>
		/// Tests the entity collection fetch simple, with nulls
		/// </summary>
		[Test]
		public void EntityCollectionFetchSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				adapter.FetchEntityCollection(customers, null, 0, null, null);
				Assert.IsTrue((customers.Count>0));
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the context feature, with a collection fetch and an entity fetch.
		/// </summary>
		[Test]
		public void ContextTestWithEntityCollectionFetchSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				Context testContext = new Context();
				EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>(new CustomerEntityFactory());
				testContext.Add(customers);
				adapter.FetchEntityCollection(customers, null);
				Assert.IsTrue((customers.Count>0));

				// dummy entity to find the index of Chops
				CustomerEntity dummy = new CustomerEntity("CHOPS");
				dummy.IsNew = false;
				int index = customers.IndexOf(dummy);
				Assert.IsTrue(index>=0);

				// first test if we can get the CHOPS customer out of the context:
				CustomerEntity chops = (CustomerEntity)testContext.Get(new CustomerEntityFactory(), "CHOPS");
				Assert.IsTrue(customers.Contains(chops));

				Assert.AreEqual(((IEntityCore)chops).ObjectID, ((IEntityCore)customers[index]).ObjectID);

				// fetch a new one, then get the unique one if availanle (in this case, it is)
				CustomerEntity chops2 = new CustomerEntity("CHOPS");
				Assert.IsTrue(adapter.FetchEntity(chops2));

				chops2 = (CustomerEntity)testContext.Get(chops2);
				Assert.AreEqual(chops, chops2);
				Assert.IsTrue((chops == chops2));
			}
			finally
			{
				adapter.Dispose();
			}
		}



		/// <summary>
		/// Tests the paging facility when fetching an entity collection, with a simple straight forward query (Select all orders, ordered by OrderID asc)
		/// </summary>
		[Test]
		public void EntityCollectionPagingTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderId | SortOperator.Ascending);

				EntityCollection pagedOrders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(pagedOrders, null, 0, sorter, 4, 10);

				Assert.AreEqual(10, pagedOrders.Count);
				Assert.AreEqual(10278, ((OrderEntity)pagedOrders[0]).OrderId);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// PredicateExpression nesting test
		/// </summary>
		[Test]
		public void EntityCollectionPredicateNestedTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				IPredicateExpression exp = new PredicateExpression();
				exp.Add(CustomerFields.Country == "France");
				filter.PredicateExpression.Add(exp);

				// create empty field object, doesn't matter
				IEntityField2 newIDField = CustomerFields.Country;
				newIDField.Alias = "NEWID()";
				newIDField.ExpressionToApply = new Expression();

				ISortExpression sorter = new SortExpression(new SortClause(newIDField, null, SortOperator.Ascending));

				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				adapter.FetchEntityCollection(customers, filter, 0, sorter);

			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests various ways to sort a fetched entity collection in memory.
		/// </summary>
		[Test]
		public void EntityCollectionSortTests()
		{
			EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(customers, null);
				Assert.AreEqual(91, customers.Count);

				// sort descending on company name, using property name, using default comparer
				customers.Sort("CompanyName", ListSortDirection.Descending, null);
				Assert.AreEqual("Wolski  Zajazd", ((CustomerEntity)customers[0]).CompanyName);

				// sort descending on ContactTitle, using field index, using default comparer
				customers.Sort((int)CustomerFieldIndex.ContactTitle, ListSortDirection.Descending, null);
				Assert.AreEqual("Sales Representative", ((CustomerEntity)customers[0]).ContactTitle);
			}
		}


		/// <summary>
		/// Tests a paging query using a join.
		/// </summary>
		[Test]
		public void EntityCollectionJoinPagingFirstPageTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(OrderEntity.Relations.OrderDetailsEntityUsingOrderId);
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderId | SortOperator.Ascending);
				
				adapter.FetchEntityCollection(orders, filter, 0, sorter, 1, 13);
				Assert.AreEqual(13, orders.Count);
				Assert.AreEqual(10248, ((OrderEntity)orders[0]).OrderId);
			}
		}



		/// <summary>
		/// Tests a paging query using a join.
		/// </summary>
		[Test]
		public void EntityCollectionJoinPagingTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(OrderEntity.Relations.OrderDetailsEntityUsingOrderId);
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderId | SortOperator.Ascending);
				
				adapter.FetchEntityCollection(orders, filter, 0, sorter, 3, 13);
				Assert.AreEqual(13, orders.Count);
				Assert.AreEqual(10274, ((OrderEntity)orders[0]).OrderId);
			}
		}


		/// <summary>
		/// Tests the paging facility when fetching an entity collection, with a more sophisticated query 
		/// </summary>
		[Test]
		public void EntityCollectionPagingTestComplex()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// fetch all orders which have a difference between orderdate and shipping date of 5 days.
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				IExpression fieldExpression = OrderFields.ShippedDate - OrderFields.OrderDate;
				FieldCompareValuePredicate fieldFilter = new FieldCompareValuePredicate(
					OrderFields.OrderDate,	// not important which field, just pick one, is replaced with expression anyway.
					null,
					ComparisonOperator.Equal,
					5);
				fieldFilter.FieldCore.ExpressionToApply = fieldExpression;
				filter.PredicateExpression.Add(fieldFilter);
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderId | SortOperator.Ascending);

				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				adapter.FetchEntityCollection(orders, filter, 0, sorter, 2, 11);

				Assert.AreEqual(11, orders.Count);
				Assert.AreEqual(10372, ((OrderEntity)orders[0]).OrderId);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void TypedListPagingTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderId | SortOperator.Ascending);
				adapter.FetchTypedList(orderCustomer.GetFieldsInfo(), orderCustomer, orderCustomer.GetRelationInfo(), 0, sorter, true, null, 3, 12);
				Assert.AreEqual(12, orderCustomer.Rows.Count);
				Assert.AreEqual(10272, orderCustomer[0].OrderId);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void DynamicListWithObeyWeakRelationsTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(CustomerFields.CustomerId, 0, "CustomerID");
				fields.DefineField(OrderFields.OrderId, 1, "OrderID");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.ObeyWeakRelations = true;
				filter.Relations.Add(CustomerEntity.Relations.OrderEntityUsingCustomerId);

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);
				Assert.AreEqual(820, results.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void RefetchWithContextAndDirtyEntityTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Console.WriteLine( "Start: {0}", DateTime.Now.ToString( "hh:mm:ss.ffff" ) );
				// first fetch the customer
				CustomerEntity customer = new CustomerEntity( "CHOPS" );
				Assert.IsTrue( adapter.FetchEntity( customer ) );

				// do a m:n fetch of Customer - Employee and then also prefetch Employee.Orders and Order.OrderDetails.
				// add root to context so everything is fetched and added to the context.
				Context c = new Context();
				c.Add( customer );
				PrefetchPath2 path = new PrefetchPath2( (int)EntityType.EmployeeEntity );
				path.Add( EmployeeEntity.PrefetchPathOrders ).SubPath.Add( OrderEntity.PrefetchPathOrderDetails );
				adapter.FetchEntityCollection( customer.Employees, customer.GetRelationInfoEmployees(), 0, null, path );
				Console.WriteLine( "End: {0}", DateTime.Now.ToString( "hh:mm:ss.ffff" ) );

				// make one entity dirty and check if the collection contains dirty contents.
				((OrderEntity)((EmployeeEntity)customer.Employees[0]).Orders[0]).OrderDate = DateTime.Now;
				Assert.IsTrue( ((EmployeeEntity)customer.Employees[0]).Orders.ContainsDirtyContents );
			}
		}


		[Test]
		public void PrefetchPathTestSimpleRowLimit()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.Country == "Mexico");
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				IPredicateExpression nodeFilter = new PredicateExpression();
				nodeFilter.Add(OrderFields.Freight > 20.0);
				path.Add(CustomerEntity.PrefetchPathOrders, 1, nodeFilter);
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				adapter.FetchEntityCollection(customers, filter, path);

				Assert.AreEqual(5, customers.Count);

				foreach(CustomerEntity customer in customers)
				{
					if(customer.CustomerId=="CENTC")
					{
						// this one has to have 0 orders, as all its orders have a freight < 20
						Assert.AreEqual(0, customer.Orders.Count);
					}
					else
					{
						Assert.AreEqual(1, customer.Orders.Count);
					}
				}
			}
		}


		[Test]
		public void PrefetchPathTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);

				adapter.FetchEntityCollection(customers, null, prefetchPath);

				ArrayList customersWithoutOrders = new ArrayList();
				customersWithoutOrders.Add("FISSA");
				customersWithoutOrders.Add("PARIS");
				for (int i = 0; i < customers.Count; i++)
				{
					for (int j = 0; j < ((CustomerEntity)customers[i]).Orders.Count; j++)
					{
						Assert.AreEqual(((CustomerEntity)customers[i]).CustomerId, ((OrderEntity)((CustomerEntity)customers[i]).Orders[j]).CustomerId);
					}
					if(((CustomerEntity)customers[i]).Orders.Count<=0)
					{
						Assert.IsTrue(customersWithoutOrders.Contains(((CustomerEntity)customers[i]).CustomerId));
					}
				}

				Assert.IsFalse(customers[0].IsDirty);
				Assert.IsFalse(customers[0].Fields.IsDirty);
				Assert.IsFalse(((CustomerEntity)customers[0]).Orders[0].IsDirty);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void PrefetchPathWithPagingTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);

				SortExpression sorter = new SortExpression(CustomerFields.CustomerId | SortOperator.Ascending);
				adapter.FetchEntityCollection(customers, null, 0, sorter, path, 3, 7);
				Assert.AreEqual(7, customers.Count);
				Assert.AreEqual("FAMIA", ((CustomerEntity)customers[6]).CustomerId);
				Assert.AreEqual(6, ((CustomerEntity)customers[6]).Orders.Count);
			}
		}


		[Test]
		public void PrefetchPathSingleEntityManyToOne()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				OrderEntity order = new OrderEntity(10254);
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.OrderEntity);
				path.Add(OrderEntity.PrefetchPathCustomers);
				path.Add(OrderEntity.PrefetchPathEmployees);
				Assert.IsTrue(adapter.FetchEntity(order, path));
				Assert.IsNotNull(order.Customers);
				Assert.IsNotNull(order.Employees);
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load one customer and all its orders.
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleSingleEntity()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity customer = new CustomerEntity("BLONP");
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
				adapter.FetchEntity(customer, prefetchPath);
				Assert.AreEqual(11, customer.Orders.Count);
				Assert.IsFalse(customer.IsNew);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void StoredProcedureCallTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				DataTable results = RetrievalProcedures.CustOrderHist("CHOPS", adapter);
				Assert.AreEqual(22, results.Rows.Count);
			}
		}


		[Test]
		public void SetContextMNTest()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
			PrefetchPath2 path = new PrefetchPath2( (int)EntityType.CustomerEntity );
			path.Add( CustomerEntity.PrefetchPathEmployees ).SubPath.Add(EmployeeEntity.PrefetchPathCustomers);
			Context c = new Context();
			customers.ActiveContext = c;
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( customers, new RelationPredicateBucket( CustomerFields.Country == "Germany" ), path );
			}

			foreach( CustomerEntity customer in customers )
			{
				foreach( EmployeeEntity employee in customer.Employees )
				{
					Assert.IsTrue((employee.Customers.Count>0));
					int index = employee.Customers.IndexOf( customer );
					Assert.AreEqual( ((IEntityCore)employee.Customers[index]).ObjectID, ((IEntityCore)customer).ObjectID );
				}
			}
		}
		

		[Test]
		public void RefetchPrefetchPathWithoutContextTest()
		{
			CustomerEntity customer = new CustomerEntity( "CHOPS" );
			PrefetchPath2 path = new PrefetchPath2( (int)EntityType.CustomerEntity );
			path.Add( CustomerEntity.PrefetchPathOrders );

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Assert.IsTrue( adapter.FetchEntity( customer, path ) );
			}
			Assert.AreEqual( 8, customer.Orders.Count );

			Context c = new Context();
			c.Add( customer );

			path = new PrefetchPath2( (int)EntityType.CustomerEntity );
			path.Add( CustomerEntity.PrefetchPathOrders ).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Assert.IsTrue( adapter.FetchEntity( customer, path ) );
			}
			Assert.AreEqual( 8, customer.Orders.Count );
		}


		[Test]
		public void RefetchPrefetchPathTestSimpleSingleEntityWithContext()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				Context testContext = new Context();
				CustomerEntity customer = new CustomerEntity("BLONP");
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders);
				adapter.FetchEntity(customer, prefetchPath, testContext);
				Assert.AreEqual(11, customer.Orders.Count);
				Assert.IsFalse(customer.IsNew);
				foreach(OrderEntity o in customer.Orders)
				{
					Assert.IsTrue((o.OrderDetails.Count<=0));
				}

				Assert.IsNotNull(customer.ActiveContext);

				// fetch the order details into the orders.
				prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);

				adapter.FetchEntity(customer, prefetchPath);
				Assert.IsNotNull(customer.ActiveContext);
				Assert.AreEqual(11, customer.Orders.Count);
				Assert.IsFalse(customer.IsNew);
				foreach(OrderEntity o in customer.Orders)
				{
					Assert.IsTrue((o.OrderDetails.Count>0));
				}

				List<Type> contextTypes = testContext.GetAllTypes();
				Assert.AreEqual(3, contextTypes.Count);
				Assert.IsTrue(contextTypes.Contains(typeof(CustomerEntity)));
				Assert.IsTrue(contextTypes.Contains(typeof(OrderEntity)));
				Assert.IsTrue(contextTypes.Contains(typeof(OrderDetailsEntity)));

				List<IEntityCore> customerObjects = testContext.GetAll(typeof(CustomerEntity));
				Assert.AreEqual(1, customerObjects.Count);

				List<IEntityCore> orderObjects = testContext.GetAll(typeof(OrderEntity));
				Assert.AreEqual(11, orderObjects.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void PrefetchPathTestSimpleSingleEntityUC()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity customer = new CustomerEntity();
				customer.CompanyName = "Blauer See Delikatessen";
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
				adapter.FetchEntityUsingUniqueConstraint(customer, customer.ConstructFilterForUCCompanyName(), prefetchPath);
				Assert.IsFalse(customer.IsNew);
				Assert.AreEqual(7, customer.Orders.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void StringConcatExpressionTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IExpression fullNameLeftSideExpression = EmployeeFields.FirstName + " "; 
				IExpression fullNameExpression = new Expression(fullNameLeftSideExpression, ExOp.Add, EmployeeFields.LastName);
				IEntityField2 fullNameField = EmployeeFields.FirstName;
				fullNameField.ExpressionToApply = fullNameExpression; 
				IPredicate fullNamePredicate = new FieldLikePredicate(fullNameField, null, "%ert K%");

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(fullNamePredicate);

				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				adapter.FetchEntityCollection(employees, filter);
				Assert.AreEqual(1, employees.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void RandomFetchTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IEntityField2 newIDField = CustomerFields.Country;
				newIDField.Alias = "NEWID()";
				newIDField.ExpressionToApply = new Expression();

				ISortExpression sorter = new SortExpression(new SortClause(newIDField, null, SortOperator.Ascending));
				EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>(new CustomerEntityFactory());
				adapter.FetchEntityCollection(customers, null, 0, sorter);

				Assert.AreEqual(91, customers.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void ComplexPrefetchPathMultiMNFetch()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				RegionEntity region = new RegionEntity(1);
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.RegionEntity);
				IPrefetchPathElement2 empEl =  prefetchPath.Add(RegionEntity.PrefetchPathTerritories).SubPath.Add(
						TerritoryEntity.PrefetchPathEmployees);
				IPrefetchPathElement2 custEl = empEl.SubPath.Add(EmployeeEntity.PrefetchPathCustomers);
				empEl.SubPath.Add(EmployeeEntity.PrefetchPathOrders);
				custEl.SubPath.Add(CustomerEntity.PrefetchPathOrders);
				adapter.FetchEntity(region, prefetchPath);

				Assert.AreEqual("Eastern", region.RegionDescription.Trim());
				Assert.AreEqual(19, region.Territories.Count);
				foreach(TerritoryEntity territory in region.Territories)
				{
					Assert.IsTrue(territory.Employees.Count > 0);
					foreach(EmployeeEntity employee in territory.Employees)
					{
						Assert.IsTrue(employee.Orders.Count > 0);
						Assert.IsTrue(employee.Customers.Count > 0);
						foreach(CustomerEntity customer in employee.Customers)
						{
							Assert.IsTrue(customer.Orders.Count > 0);
						}
					}
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void PrefetchPathTestManyToManyFilter2()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				// redundant range filter, to make sure the m:n prefetch path will produce a combined OR and AND query which should have the
				// OR statements (from the parameterized prefetch path) inside a predicate expression.
				RelationPredicateBucket customerFilter = new RelationPredicateBucket( CustomerFields.CustomerId == new string[] { "CHOPS", "VINET", "BLONP", "HANAR", "VICTE" } );
				EntityCollection customers = new EntityCollection( new CustomerEntityFactory() );
				IPrefetchPath2 prefetchPath = new PrefetchPath2( (int)EntityType.CustomerEntity );
				prefetchPath.Add( CustomerEntity.PrefetchPathEmployees, 0, new PredicateExpression( EmployeeFields.EmployeeId == 5 ) );
				adapter.FetchEntityCollection( customers, customerFilter, prefetchPath );
				Assert.AreEqual( 5, customers.Count );
				foreach( CustomerEntity customer in customers )
				{
					Console.WriteLine( "CustomerID: {0}. #employees: {1}", customer.CustomerId, customer.Employees.Count );
					if( (customer.CustomerId == "CHOPS") || (customer.CustomerId == "VICTE") )
					{
						Assert.IsTrue( (customer.Employees.Count == 0) );
					}
					else
					{
						Assert.IsTrue( (customer.Employees.Count == 1) );
					}
				}
			}
		}


		[Test]
		public void PrefetchPathTestManyToManyFilter()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathEmployees).SubPath.Add(EmployeeEntity.PrefetchPathShippers);
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.CustomerId == "CHOPS");
				adapter.FetchEntityCollection(customers, filter, prefetchPath);
				Assert.AreEqual(1, customers.Count);
				CustomerEntity chops = (CustomerEntity)customers[0];
				Assert.AreEqual(5, chops.Employees.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void PrefetchPathTestSimpleSingleEntityManyToMany()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity customer = new CustomerEntity("BLONP");
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathEmployees);
				adapter.FetchEntity(customer, prefetchPath);
				Assert.IsFalse(customer.IsNew);
				Assert.AreEqual(7, customer.Employees.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void MultiNodePrefetchPathTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.EmployeeEntity);
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderDate | SortOperator.Descending);
				prefetchPath.Add(EmployeeEntity.PrefetchPathOrders, 1, null, null, sorter).SubPath.Add(OrderEntity.PrefetchPathShippers);
				prefetchPath.Add(EmployeeEntity.PrefetchPathShippers);
				prefetchPath.Add(EmployeeEntity.PrefetchPathCustomers).SubPath.Add(CustomerEntity.PrefetchPathOrders);

				adapter.FetchEntityCollection(employees, null, prefetchPath);

				for (int i = 0; i < employees.Count; i++)
				{
					OrderEntity o1 =(OrderEntity)((EmployeeEntity)employees[i]).Orders[0];
					OrderEntity o2 = (OrderEntity)((EmployeeEntity)employees[i]).Orders[0];
					Assert.AreEqual( o1.ShipVia, o2.Shippers.ShipperId);
					Assert.AreEqual(1, ((EmployeeEntity)employees[i]).Orders.Count);

					Assert.IsTrue(( ((EmployeeEntity)employees[i]).Shippers.Count > 0));
					Assert.IsTrue(( ((EmployeeEntity)employees[i]).Customers.Count > 0));

					foreach(CustomerEntity c in ((EmployeeEntity)employees[i]).Customers)
					{
						Assert.IsTrue( c.Orders.Count > 0);
					}
				}

				// check if all elements are empty.
				for(int i=0;i<prefetchPath.Count;i++)
				{
					PrefetchPathElement2 element = (PrefetchPathElement2)prefetchPath[i];

					Assert.AreEqual(0, element.RetrievalCollection.Count);
					if(element.SubPath.Count>0)
					{
						for(int j=0;j<element.SubPath.Count;j++)
						{
							PrefetchPathElement2 subElement = (PrefetchPathElement2)element.SubPath[j];
							Assert.AreEqual(0, subElement.RetrievalCollection.Count);
						}
					}
				}
			}
		}


		[Test]
		public void FkFieldChangeResetTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				CustomerEntity c = new CustomerEntity( "CHOPS" );
				PrefetchPath2 path = new PrefetchPath2( (int)EntityType.CustomerEntity );
				path.Add( CustomerEntity.PrefetchPathOrders );
				Assert.IsTrue( adapter.FetchEntity( c, path ) );

				OrderEntity order = c.Orders[0];
				order.CustomerId = "CHNG";
				Assert.IsNull( order.Customers );
				Assert.IsFalse( c.Orders.Contains( order ) );
			}
		}


		[Test]
		public void PrefetchPathTestMultiLevel()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection orders = new EntityCollection(new OrderEntityFactory());
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(OrderEntity.Relations.OrderDetailsEntityUsingOrderId);
				filter.Relations.Add(OrderDetailsEntity.Relations.ProductEntityUsingProductId);
				filter.PredicateExpression.Add(ProductFields.SupplierId == 3);
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.OrderEntity);
				path.Add(OrderEntity.PrefetchPathOrderDetails).SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);

				adapter.FetchEntityCollection(orders, filter, path);

				Assert.IsTrue((orders.Count > 0));
				foreach(OrderEntity order in orders)
				{
					foreach(OrderDetailsEntity orderDetails in order.OrderDetails)
					{
						Assert.IsNotNull(orderDetails.Products);
					}
				}
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality. Loads all employees and per employee the last order object they closed.
		/// Similar to FieldCompareSetTestComplex but now with entities.
		/// </summary>
		[Test]
		public void PrefetchPathTestComplex()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.EmployeeEntity);
				ISortExpression sorter = new SortExpression();
				sorter.Add(OrderFields.OrderDate | SortOperator.Descending);
				prefetchPath.Add(EmployeeEntity.PrefetchPathOrders, 1, null, null, sorter).SubPath.Add(OrderEntity.PrefetchPathShippers);

				adapter.FetchEntityCollection(employees, null, prefetchPath);

				for (int i = 0; i < employees.Count; i++)
				{
					Assert.AreEqual( ((OrderEntity)((EmployeeEntity)employees[i]).Orders[0]).ShipVia, 
							((OrderEntity)((EmployeeEntity)employees[i]).Orders[0]).Shippers.ShipperId);
					Assert.AreEqual(1, ((EmployeeEntity)employees[i]).Orders.Count);
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the Exists operator with this query:
		/// SELECT * FROM Customer where exists (SELECT * from orders where customerid=customer.customerid and employeeid=2)
		/// </summary>
		[Test]
		public void FieldCompareSetWithExistsOperatorTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				IPredicateExpression subQueryFilter = new PredicateExpression();
				subQueryFilter.Add(OrderFields.CustomerId == CustomerFields.CustomerId);
				subQueryFilter.AddWithAnd(OrderFields.EmployeeId ==2);
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.PredicateExpression.Add(new FieldCompareSetPredicate(null, null,
						OrderFields.OrderId, null, SetOperator.Exist, subQueryFilter));
				adapter.FetchEntityCollection(customers, bucket);
				Assert.AreEqual(62, customers.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the row limit with a multi-entity spanning join. 
		/// </summary>
		[Test]
		public void RowLimitTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);
				filter.PredicateExpression.Add(OrderFields.ShipVia == 2);
				adapter.FetchEntityCollection(employees, filter, 6);
				Assert.AreEqual(6, employees.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the row limit with a multi-entity spanning join using typedlist fetch code.
		/// </summary>
		[Test]
		public void RowLimitTestTypedList()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			ResultsetFields fields = new ResultsetFields(3);
			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId");
			fields.DefineField(EmployeeFields.Photo, 1, "Photo");
			fields.DefineField(OrderFields.OrderId, 2, "OrderId");
			
			try
			{
				IRelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);
				filter.PredicateExpression.Add(OrderFields.ShipVia == 2);
				DataTable tlist = new DataTable();
				adapter.FetchTypedList(fields, tlist, filter, 6, false);
				Assert.AreEqual(6, tlist.Rows.Count);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Calculates with an expression the product price and then calculates the total price for order 10254
		/// </summary>
		[Test]
		public void ScalarTestAggregate()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				IExpression productPriceExpression = OrderDetailsFields.Quantity * OrderDetailsFields.UnitPrice;
				IPredicate filter = OrderDetailsFields.OrderId == 10254;
				decimal orderPrice = (decimal)adapter.GetScalar(OrderDetailsFields.OrderId, productPriceExpression, AggregateFunction.Sum, filter);
				Assert.AreEqual(9987.6M, orderPrice);
			}
			finally
			{
				adapter.Dispose();
			}
		}

		/// <summary>
		/// Tests readxml / write xml with prefetch paths. Uses normal, verbose XML
		/// </summary>
		[Test]
		public void ReadXmlWriteXmlWithPrefetchPathsTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.Country == "France");
				adapter.FetchEntityCollection(customers, filter, prefetchPath);

				Assert.AreEqual(11, customers.Count);

				string customersXml = string.Empty;
				customers.WriteXml(out customersXml);

				EntityCollection customersFromXml = new EntityCollection(new CustomerEntityFactory());
				customersFromXml.ReadXml(customersXml);

				Assert.AreEqual(11, customersFromXml.Count);

				// the other way around.
				EntityCollection orderDetails = new EntityCollection(new OrderDetailsEntityFactory());
				prefetchPath = new PrefetchPath2((int)EntityType.OrderDetailsEntity);
				prefetchPath.Add(OrderDetailsEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathCustomers);
				filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(OrderDetailsFields.ProductId == 3);
				adapter.FetchEntityCollection(orderDetails, filter, prefetchPath);

				Assert.AreEqual(11, orderDetails.Count);
				foreach(OrderDetailsEntity od in orderDetails)
				{
					Assert.AreEqual(od.OrderId,od.Orders.OrderId);
					Assert.AreEqual(od.Orders.CustomerId, od.Orders.Customers.CustomerId);
				}

				string orderDetailsXml = string.Empty;
				orderDetails.WriteXml(out orderDetailsXml);

				StreamWriter writer = new StreamWriter("orderdetailsVerbose.xml");
				writer.Write(orderDetailsXml);
				writer.Close();

				EntityCollection orderDetailsFromXml = new EntityCollection(new OrderDetailsEntityFactory());
				orderDetailsFromXml.ReadXml(orderDetailsXml);

				Assert.AreEqual(11, orderDetailsFromXml.Count);

				foreach(OrderDetailsEntity od in orderDetailsFromXml)
				{
					Assert.AreEqual(od.OrderId,od.Orders.OrderId);
					Assert.AreEqual(od.Orders.CustomerId, od.Orders.Customers.CustomerId);
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests readxml / write xml with prefetch paths. Uses compact XML
		/// </summary>
		[Test]
		public void ReadCompactXmlWriteCompactXmlWithPrefetchPathsTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection customers = new EntityCollection(new CustomerEntityFactory());
				IPrefetchPath2 prefetchPath = new PrefetchPath2((int)EntityType.CustomerEntity);
				prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CustomerFields.Country == "France");
				adapter.FetchEntityCollection(customers, filter, prefetchPath);

				Assert.AreEqual(11, customers.Count);

				string customersXml = string.Empty;
				customers.WriteXml(XmlFormatAspect.Compact | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out customersXml);

				EntityCollection customersFromXml = new EntityCollection(new CustomerEntityFactory());
				customersFromXml.ReadXml(customersXml);

				Assert.AreEqual(11, customersFromXml.Count);

				// the other way around.
				EntityCollection orderDetails = new EntityCollection(new OrderDetailsEntityFactory());
				prefetchPath = new PrefetchPath2((int)EntityType.OrderDetailsEntity);
				prefetchPath.Add(OrderDetailsEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathCustomers);
				filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(OrderDetailsFields.ProductId == 3);
				adapter.FetchEntityCollection(orderDetails, filter, prefetchPath);

				Assert.AreEqual(11, orderDetails.Count);
				foreach(OrderDetailsEntity od in orderDetails)
				{
					Assert.AreEqual(od.OrderId,od.Orders.OrderId);
					Assert.AreEqual(od.Orders.CustomerId, od.Orders.Customers.CustomerId);
				}

				string orderDetailsXml = string.Empty;
				orderDetails.WriteXml(XmlFormatAspect.Compact | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out orderDetailsXml);

				StreamWriter writer = new StreamWriter("orderdetailsCompact.xml");
				writer.Write(orderDetailsXml);
				writer.Close();

				EntityCollection orderDetailsFromXml = new EntityCollection(new OrderDetailsEntityFactory());
				orderDetailsFromXml.ReadXml(orderDetailsXml);

				Assert.AreEqual(11, orderDetailsFromXml.Count);

				foreach(OrderDetailsEntity od in orderDetailsFromXml)
				{
					Assert.AreEqual(od.OrderId,od.Orders.OrderId);
					Assert.AreEqual(od.Orders.CustomerId, od.Orders.Customers.CustomerId);
				}
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void ContextCollectionRefetchPrefetchPath2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>(new CustomerEntityFactory());
				// first fetch all customers which have orders
				RelationPredicateBucket filter = new RelationPredicateBucket(
					new FieldCompareSetPredicate(CustomerFields.CustomerId, null,
					OrderFields.CustomerId, null,
					SetOperator.In, null));

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);
				path.Add(CustomerEntity.PrefetchPathEmployees);


				adapter.FetchEntityCollection(customers, filter);
				Assert.AreEqual(89, customers.Count);

				// create range filter for all the customers.
				ArrayList customerIDs = new ArrayList(89);
				Hashtable objectIDs = new Hashtable(89);
				foreach(CustomerEntity customer in customers)
				{
					customerIDs.Add(customer.CustomerId);
					objectIDs.Add(((IEntityCore)customer).ObjectID, null);
				}

				PrefetchPath2 path2 = new PrefetchPath2((int)EntityType.CustomerEntity);
				path2.Add(CustomerEntity.PrefetchPathOrders)
							.SubPath.Add(OrderEntity.PrefetchPathOrderDetails)
								.SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);

				path2.Add(CustomerEntity.PrefetchPathEmployees).SubPath.Add(EmployeeEntity.PrefetchPathEmployeeTerritories);

				filter = new RelationPredicateBucket((CustomerFields.CustomerId == customerIDs));
				// refetch collection
				Context c = new Context();
				c.Add(customers);
				adapter.FetchEntityCollection(customers, filter, path2);

				foreach(CustomerEntity customer in customers)
				{
					// check if this instance is the same as we had before
					Assert.IsTrue( objectIDs.ContainsKey(((IEntityCore)customer).ObjectID));
					// orders and employees have to be filled.
					Assert.IsTrue( (customer.Orders.Count>0));
					Assert.IsTrue( (customer.Employees.Count > 0));
				}
			}
		}


		[Test]
		public void ContextCollectionRefetchPrefetchPath()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>(new CustomerEntityFactory());
				// first fetch all customers which have orders
				RelationPredicateBucket filter = new RelationPredicateBucket(
						new FieldCompareSetPredicate(CustomerFields.CustomerId, null,
							OrderFields.CustomerId, null, 
							SetOperator.In, null));
				adapter.FetchEntityCollection(customers, filter);
				Assert.AreEqual(89, customers.Count);

				// create range filter for all the customers.
				ArrayList customerIDs = new ArrayList(89);
				Hashtable objectIDs = new Hashtable(89);
				foreach(CustomerEntity customer in customers)
				{
					customerIDs.Add(customer.CustomerId);
					objectIDs.Add(((IEntityCore)customer).ObjectID, null);
				}

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathOrders);
				path.Add(CustomerEntity.PrefetchPathEmployees);

				//filter = new RelationPredicateBucket((CustomerFields.CustomerId == customerIDs));
				// refetch collection
				Context c = new Context();
				c.Add(customers);
				adapter.FetchEntityCollection(customers, filter, path);

				foreach(CustomerEntity customer in customers)
				{
					// check if this instance is the same as we had before
					Assert.IsTrue( objectIDs.ContainsKey(((IEntityCore)customer).ObjectID));
					// orders and employees have to be filled.
					Assert.IsTrue( (customer.Orders.Count>0));
					Assert.IsTrue( (customer.Employees.Count > 0));
				}
			}
		}


		/// <summary>
		/// An 1:n related subentity is refetched and its child entities are loaded into this entity. Test to see if it is still present in the
		/// parent collection.
		/// </summary>
		[Test]
		public void ContextSubEntityRefetchTest()
		{
			CustomerEntity chops = new CustomerEntity("CHOPS");
			PrefetchPath2 path = new PrefetchPath2((int)EntityType.CustomerEntity);
			SortExpression sorter = new SortExpression(OrderFields.OrderId | SortOperator.Ascending);
			path.Add(CustomerEntity.PrefetchPathOrders, 0, null, null, sorter);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(chops, path));
				Context c = new Context();

				Assert.AreEqual(8, chops.Orders.Count);

				OrderEntity o = (OrderEntity)chops.Orders[0];
				Assert.AreEqual(10254, o.OrderId);

				// fetch the order details.
				PrefetchPath2 odPath = new PrefetchPath2((int)EntityType.OrderEntity);
				odPath.Add(OrderEntity.PrefetchPathOrderDetails);

				c.Add(o);
				Assert.IsTrue( adapter.FetchEntity(o, odPath));
				Assert.AreEqual(9, o.OrderDetails.Count);

				Assert.IsNotNull(o.Customers);
				Assert.IsTrue(chops.Orders.Contains(o));
			}
		}


		[Test]
		public void CompactXmlReadWriteSingleEntityTest()
		{
			CustomerEntity source = new CustomerEntity("CHOPS");
			using (DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(source));

				string sourceAsXml = string.Empty;
				source.WriteXml(XmlFormatAspect.Compact | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out sourceAsXml);

				CustomerEntity destination = new CustomerEntity();
				destination.ReadXml(sourceAsXml);

				Assert.IsTrue(destination.Equals(source));
			}
		}



		[Test]
		public void RecursiveSaveTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			adapter.StartTransaction(IsolationLevel.ReadCommitted, "inserttest");

			OrderEntity order = new OrderEntity();
			order.CustomerId = "ALFKI";
			order.EmployeeId = 1;
			OrderDetailsEntity orderDetail = new OrderDetailsEntity();
			orderDetail.ProductId = 1;
			orderDetail.Quantity = 4;
			orderDetail.UnitPrice = 12.5M;
			orderDetail.Discount = 0;
			orderDetail.Orders = order; // Not really necessary
			try
			{
				Assert.IsTrue(adapter.SaveEntity(order, true));
				Assert.AreEqual(order.OrderId, orderDetail.OrderId);
				Assert.IsTrue((order.OrderId>0));
			}
			finally
			{
				adapter.Rollback();
			}
		}


		[Test]
		public void OrderRecursiveSaveWithValidator()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.CatalogNameToUse = "Northwind2";
				adapter.CatalogNameUsageSetting = CatalogNameUsage.ForceName;

				OrderEntity o = new OrderEntity(10254, new OrderValidator());
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.OrderEntity);
				path.Add(OrderEntity.PrefetchPathOrderDetails);
				Assert.IsTrue(adapter.FetchEntity(o, path));
				o.Freight += 0.1M;
				Assert.IsTrue(adapter.SaveEntity(o, true));
			}
		}


		/// <summary>
		/// Used by dereference test.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void o_EntityContentsChanged(object sender, EventArgs e)
		{
			OrderEntity o = (OrderEntity)sender;

			Assert.IsNull(o.Customers);
			_dereferenceTestFlag = true;
		}
	}



	/// <summary>
	/// Summary description for EntityRetrievalTests.
	/// </summary>
	[TestFixture]
	public class EntityRetrievalTests
	{
		public EntityRetrievalTests()
		{
		}

		[Test]
		public void GetCategoryTest()
		{
			int threadCount = 5;
			int iterationCount = 5;

			Thread[] threadList = new Thread[ threadCount ];
			for( int i = 0; i < threadCount; i++ )
			{
				CategoryWorker authWorker = new CategoryWorker( iterationCount );
				Thread th = new Thread( new ThreadStart( authWorker.Execute ) );
				threadList[ i ] = th;
				th.Start();
			}

			foreach( Thread thread in threadList )
			{
				thread.Join();
			}
		}
	}


	public class CategoryWorker
	{
		int iterationCount = 0;

		public CategoryWorker( int iterationCount )
		{
			this.iterationCount = iterationCount;
		}
		public void Execute()
		{
			for( int i = 0; i < iterationCount; i++ )
			{
				try
				{
					DataAccessAdapter adapter = new DataAccessAdapter();
					EntityCollection categories = new EntityCollection( new CategoryEntityFactory() );
					adapter.FetchEntityCollection( categories, null );
					Console.WriteLine( "Category count: " + categories.Count );
					Console.WriteLine( "Iteration: " + i );
				}
				catch( Exception e )
				{
					Console.WriteLine( e.ToString() );
				}
			}
		}
	}


	public class ExtendedCustomerEntityFactory : CustomerEntityFactory
	{
		/// <summary>
		/// Create the fields for the customer entity and adjust the fields collection
		/// with the entity field object which contains the scalar expression
		/// </summary>
		/// <returns></returns>
		public override IEntityFields2 CreateFields()
		{
			IEntityFields2 toReturn = base.CreateFields();
			toReturn.Expand(1);
			IEntityField2 scalarField = new EntityField2("NumberOfOrders",
					new ScalarQueryExpression(OrderFields.OrderId.SetAggregateFunction(
						AggregateFunction.Count),
					(CustomerFields.CustomerId == OrderFields.CustomerId)));
			toReturn.DefineField(scalarField, toReturn.Count - 1);
			return toReturn;
		}
	}


	public class CustomerEntityEx : CustomerEntity
	{
		public CustomerEntityEx(string customerID)
			: base(customerID)
		{
		}

		protected override IEntityFactory2 CreateEntityFactory()
		{
			return new ExtendedCustomerEntityFactory();
		}

		protected override IEntityFields2 CreateFields()
		{
			return CreateEntityFactory().CreateFields();
		}
		

		public Nullable<int> NumberOfOrders
		{
			// read the last field in the fieldslist, as we've added our scalar
			// expression field to the end of the fields list.
			get
			{
				object value = this.Fields[this.Fields.Count - 1].CurrentValue;
				if(value != null)
				{
					return Convert.ToInt32(value);
				}
				else
				{
					return null;
				}
			}
		}

	}
}
