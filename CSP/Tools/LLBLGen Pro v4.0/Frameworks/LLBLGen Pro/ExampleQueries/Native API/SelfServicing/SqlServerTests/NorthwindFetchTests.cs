using System;
using System.ComponentModel;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Northwind.SelfServicing;
using Northwind.SelfServicing.CollectionClasses;
using Northwind.SelfServicing.DaoClasses;
using Northwind.SelfServicing.EntityClasses;
using Northwind.SelfServicing.FactoryClasses;
using Northwind.SelfServicing.HelperClasses;
using Northwind.SelfServicing.TypedListClasses;
using Northwind.SelfServicing.TypedViewClasses;
using Northwind.SelfServicing.StoredProcedureCallerClasses;
using System.Diagnostics;
using SD.LLBLGen.Pro.DQE.SqlServer;

//using Northwind.SelfServicing.VB;
//using Northwind.SelfServicing.VB.CollectionClasses;
//using Northwind.SelfServicing.VB.DaoClasses;
//using Northwind.SelfServicing.VB.EntityClasses;
//using Northwind.SelfServicing.VB.FactoryClasses;
//using Northwind.SelfServicing.VB.HelperClasses;
//using Northwind.SelfServicing.VB.TypedListClasses;
//using Northwind.SelfServicing.VB.TypedViewClasses;


namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
{
	/// <summary>
	/// NorthwindFetchTests. 
	/// Contains NUnit tests working on fetches from the Northwind database. 
	/// </summary>
	[TestFixture]
	public class NorthwindFetchTests
	{
		private Guid _testRunID;

		/// <summary>
		/// CTor
		/// </summary>
		public NorthwindFetchTests()
		{
		}

		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid(); 
			Console.WriteLine("TestRunID: {0}", _testRunID.ToString());
		}


		[Test]
		public void FillTypedViewWithProcCall()
		{
			OrdersOnCountryTypedView ordersTv = new OrdersOnCountryTypedView();
			CustomersOnCountryTypedView customersTv = new CustomersOnCountryTypedView();
			ordersTv.Fill(null, "USA");
			customersTv.Fill(null, "USA");
			Assert.AreEqual(121, ordersTv.Count);
			Assert.AreEqual(13, customersTv.Count);
		}


		[Test]
		public void OrderByOnAggregateInGroupedSubQuery()
		{
			OrderCollection orders = new OrderCollection();
			GroupByCollection groupBy = new GroupByCollection();
			groupBy.Add(OrderDetailsFields.OrderId);
			SortExpression sorter = new SortExpression();
			ISortClause clause = new SortClause(OrderDetailsFields.OrderId.SetAggregateFunction(AggregateFunction.Count), SortOperator.Descending);
			clause.EmitAliasForExpressionAggregateField = false;
			sorter.Add(clause);

			PredicateExpression filter = new PredicateExpression(
					new FieldCompareSetPredicate(OrderFields.OrderId, OrderDetailsFields.OrderId, SetOperator.In, null, null, "", 5, sorter, false, groupBy));
			orders.GetMulti(filter);
			Assert.AreEqual(5, orders.Count);
		}


		[Test]
		public void LazyLoadingViaFetchByPkTest()
		{
			OrderEntity o = new OrderEntity();
			o.FetchUsingPK(10254);
			Assert.IsFalse(o.IsNew);
			int numberOfProducts = o.Products.Count;
			Assert.AreEqual(9, numberOfProducts);

			CustomerEntity c = new CustomerEntity();
			c.FetchUsingPK("CHOPS");
			Assert.IsFalse(c.IsNew);
			int numberOfOrders = c.Orders.Count;
			Assert.AreEqual(8, numberOfOrders);
		}

		
		[Test]
		public void RemovedEntitiesTrackerTest()
		{
			CustomerCollection customers = new CustomerCollection();
			PrefetchPath path = new PrefetchPath(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);
			customers.GetMulti(CustomerFields.Country == "Germany", path);

			OrderCollection tracker = new OrderCollection();
			customers[0].Orders.RemovedEntitiesTracker = tracker;
			customers[2].Orders.RemovedEntitiesTracker = tracker;

			OrderEntity o0, o1, o2;
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
			CustomerEntity c = new CustomerEntity();
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(CustomerFields.ContactName);
			excludedFields.Add(CustomerFields.Country);
			Assert.IsTrue(c.FetchUsingUCCompanyName("The Big Cheese", null, null, excludedFields));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
		}

		[Test]
		public void FetchWithExcludedFieldsTest()
		{
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(CustomerFields.ContactName);
			excludedFields.Add(CustomerFields.Country);

			CustomerCollection customers = new CustomerCollection();
			CustomerCollection customersVerification = new CustomerCollection();
			SortExpression sorter = new SortExpression(CustomerFields.CustomerId | SortOperator.Ascending);
			customers.GetMulti(null, 0, sorter, null, null, excludedFields, 0, 0);
			customersVerification.GetMulti(null, 0, sorter);
			Assert.AreEqual(91, customers.Count);
			Assert.AreEqual(91, customersVerification.Count);
			foreach(CustomerEntity customer in customers)
			{
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
				Assert.IsTrue(customer.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
			}

			// fetch the excluded fields into the existing entities. Use a threshold of 5 to get multiple batches
			DaoBase.ParameterisedPrefetchPathThreshold = 5;
			customers.FetchExcludedFields(excludedFields);
			// verify
			for(int i = 0; i < customers.Count; i++)
			{
				Assert.AreEqual(customers[i].Country, customersVerification[i].Country);
				Assert.AreEqual(customers[i].ContactName, customersVerification[i].ContactName);
			}

			CustomerEntity c = new CustomerEntity();
			c.FetchUsingPK("CHOPS", null, null, excludedFields);
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));

			c.FetchExcludedFields(excludedFields);
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.Country));
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CustomerFieldIndex.ContactName));
		}


		[Test]
		public void ExcludedFieldsTestWithPrefetchPath()
		{
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(OrderFields.OrderDate);
			PrefetchPath path = new PrefetchPath(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders, excludedFields);

			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(null, 25, null, null, path, excludedFields, 0, 0);
			Assert.AreEqual(25, customers.Count);

			foreach(CustomerEntity c in customers)
			{
				foreach(OrderEntity o in c.Orders)
				{
					Assert.IsTrue(o.TestCurrentFieldValueForNull(OrderFieldIndex.OrderDate));
				}
			}
		}


		[Test]
		public void DIAttributeBasedInjectionTest()
		{
			CustomerEntity c = new CustomerEntity();
			Assert.AreEqual(typeof(GeneralNorthwindCPF), c.ConcurrencyPredicateFactoryToUse.GetType());
			// order has its own, directly targeted to order. This thus should overrule the interface based one
			OrderEntity o = new OrderEntity();
			Assert.AreEqual(typeof(OrderConcurrencyPredicateFactory), o.ConcurrencyPredicateFactoryToUse.GetType());

			// as the generalconcurrencypredicatefactory is defined as a new instance per call, all instances are unique
			ProductEntity p = new ProductEntity();
			Assert.AreNotEqual(p.ConcurrencyPredicateFactoryToUse, c.ConcurrencyPredicateFactoryToUse);
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
			}

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
			EmployeeCollection employees = new EmployeeCollection();
			employees.GetMulti(new FieldFullTextSearchPredicate(EmployeeFields.Notes, FullTextSearchOperator.Contains, "BA"));

			Assert.AreEqual(4, employees.Count);
		}


		[Test]
		public void MultiLevelFilterTestCustomerOrders()
		{
			PrefetchPath path = new PrefetchPath((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			CustomerCollection customers = new CustomerCollection();

			customers.GetMulti(null, path);

			// in memory filtering of customers, based on property of related entity. Get all customers which have orders with product 1
			IPredicate filter = new MemberPredicate(
							CustomerEntity.MemberNames.Orders,	// the member to apply the specified filter on.
							MemberOperator.Any,   // operator for applying contained filter.
							new MemberPredicate(	// the filter to apply to each element in member (or member itself)
								OrderEntity.MemberNames.OrderDetails,
									MemberOperator.Any,
									(OrderDetailsFields.ProductId == 1))		// the filter to apply to each element in member (or member itself)
							);

			EntityView<CustomerEntity> customersWithOrders = new EntityView<CustomerEntity>(customers, filter);
			Assert.AreEqual(31, customersWithOrders.Count);

			// m:1 filters
			path = new PrefetchPath((int)EntityType.OrderDetailsEntity);
			path.Add(OrderDetailsEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathCustomers);
			OrderDetailsCollection orderDetails = new OrderDetailsCollection();
			orderDetails.GetMulti(null, path);

			// filter all order details of which the orders are placed by customers from germany.
			filter = new MemberPredicate(
							OrderDetailsEntity.MemberNames.Orders,
							MemberOperator.Any,
							new MemberPredicate(
								OrderEntity.MemberNames.Customers,
								MemberOperator.Any,
								(CustomerFields.Country == "Germany"))		// additional filter.
						);

			EntityView<OrderDetailsEntity> orderDetailsFromGermans = new EntityView<OrderDetailsEntity>(orderDetails, filter);
			Assert.AreEqual(303, orderDetailsFromGermans.Count);

			filter.Negate = true;
			orderDetailsFromGermans = new EntityView<OrderDetailsEntity>(orderDetails, filter);
			Assert.AreEqual(1712, orderDetailsFromGermans.Count);
		}



		[Test]
		public void MultiLevelFilterTestWithAggregatePredicate()
		{
			PrefetchPath path = new PrefetchPath((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			CustomerCollection customers = new CustomerCollection();

			customers.GetMulti(null, path);

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
				((OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity) -
				((OrderDetailsFields.UnitPrice * OrderDetailsFields.Quantity)
				* OrderDetailsFields.Discount)),		// value producer expression. 
				ComparisonOperator.GreaterThan,		// comparison operator for the aggregateset predicate to become true
				5000.0M,				// value to compare with
				null				// filter which specifies the set to apply the aggregate on.
				)
				);

			EntityView<CustomerEntity> filteredCustomers = new EntityView<CustomerEntity>(customers, filter);
			Assert.AreEqual(14, filteredCustomers.Count);

			// filter on customers to find all customers with at least 10 orders
			filter = new AggregateSetPredicate(		// aggregate predicate to apply on a member
				CustomerEntity.MemberNames.Orders,	// member to apply the aggregate on
				AggregateSetFunction.Count,	// aggregate function to apply. Only simple aggregates are supported + custom.
				OrderFields.OrderId,		// value producer. Can be a field as well.
				ComparisonOperator.GreaterThan,		// comparison operator for the aggregateset predicate to become true
				10,
				null);					// value to compare with

			filteredCustomers = new EntityView<CustomerEntity>(customers, filter);
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

			filteredCustomers = new EntityView<CustomerEntity>(customers, filter);
			Assert.AreEqual(2, filteredCustomers.Count);

			// custom aggregate which applies to property so more powerful operations can be performed. 
			// filter on customers where all orderid's added is greater than 80000 (just to illustrate the point of using a delegate)
			filter = new AggregateSetPredicate(		// aggregate predicate to apply on a member. 
				CustomerEntity.MemberNames.Orders,	// member to apply the aggregate on
				// custom function to apply on the set of produced values 
				delegate(IList valuesToAggregate) { int toReturn = 0; foreach(int i in valuesToAggregate) { toReturn += i; }; return toReturn; },
				delegate(IEntityCore toInterpret) { return ((OrderEntity)toInterpret).OrderId; },
				ComparisonOperator.GreaterEqual,
				80000,
				null
				);
			filteredCustomers = new EntityView<CustomerEntity>(customers, filter);
			Assert.AreEqual(48, filteredCustomers.Count);
		}

		
		[Test]
		public void CreateHierarchicalProjectionsTest()
		{
			CustomerCollection customers = new CustomerCollection();
			PrefetchPath path = new PrefetchPath((int)EntityType.CustomerEntity);
			IPrefetchPathElement element = path.Add(CustomerEntity.PrefetchPathOrders);
			element.SubPath.Add(OrderEntity.PrefetchPathEmployees);
			element.SubPath.Add(OrderEntity.PrefetchPathOrderDetails).SubPath.Add(OrderDetailsEntity.PrefetchPathProducts);

			customers.GetMulti(null, path);

			// setup projections per type.
			List<IEntityPropertyProjector> customerProjections = EntityFields.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.CustomerEntity));
			List<IEntityPropertyProjector> orderProjections = EntityFields.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderEntity));
			customerProjections.Add(new EntityPropertyProjector(new EntityProperty("IsNew"), "IsNew"));

			List<IEntityPropertyProjector> employeeProjections = EntityFields.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.EmployeeEntity));
			List<IEntityPropertyProjector> orderDetailsProjections = EntityFields.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.OrderDetailsEntity));
			List<IEntityPropertyProjector> productProjections = EntityFields.ConvertToProjectors(
																		EntityFieldsFactory.CreateEntityFieldsObject(EntityType.ProductEntity));

			List<IViewProjectionData> projectionData = new List<IViewProjectionData>();
			projectionData.Add(new ViewProjectionData<CustomerEntity>(customerProjections, (CustomerFields.Country == "Germany"), true));
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

			Dictionary<Type, IEntityCollection> projectionResults = new Dictionary<Type, IEntityCollection>();
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
			EmployeeCollection employees = new EmployeeCollection();
			employees.GetMulti(null);

			Assert.AreEqual(9, employees.Count);
			EntityView<EmployeeEntity> employeesView = new EntityView<EmployeeEntity>(employees);
			Assert.AreEqual(9, employeesView.Count);
			employeesView.Filter = new PredicateExpression(EmployeeFields.ReportsTo == DBNull.Value);
			Assert.AreEqual(1, employeesView.Count);
		}


		[Test]
		public void UpdatingCollectionWithSortedAndFilteredView()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(CustomerFields.Country == "USA");
			CustomerEntity customer = customers[0];
			EntityView<OrderEntity> ordersView = customers[0].Orders.DefaultView;
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
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(OrderFields.CustomerId == "GREAL");
			EntityView<OrderEntity> ordersView2 = orders.DefaultView;
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
		public void IListFieldCompareRangeTest()
		{
			List<int> range = new List<int>();
			range.AddRange(new int[] { 10254, 10255, 10256, 10257 });
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(OrderFields.OrderId == range);
			Assert.AreEqual(4, orders.Count);
		}


		[Test]
		public void DoNotAddIfPresentTest()
		{
			CustomerEntity customer = new CustomerEntity("CHOPS");
			customer.GetMultiOrders(false).DoNotPerformAddIfPresent = true;
			Assert.IsTrue(customer.Orders.DoNotPerformAddIfPresent);
			Assert.IsTrue(customer.GetMultiOrders(false).DoNotPerformAddIfPresent);
			OrderEntity newOrder = new OrderEntity(10254);
			Assert.AreEqual("CHOPS", newOrder.CustomerId);
			newOrder.IsNew = true;
			customer.Orders.Add(newOrder);
			Assert.AreEqual(9, customer.Orders.Count);
			customer.Orders.Add(newOrder);
			Assert.AreEqual(9, customer.Orders.Count);
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
			fields.DefineField( new EntityField( "NumberOfOrders",
								new ScalarQueryExpression( OrderFields.OrderId.SetAggregateFunction( AggregateFunction.Count ),
									(CustomerFields.CustomerId == OrderFields.CustomerId) ) ), 4 );
			fields.DefineField( new EntityField( "LastOrderDate",
								new ScalarQueryExpression( OrderFields.OrderDate.SetAggregateFunction( AggregateFunction.Max ),
									(CustomerFields.CustomerId == OrderFields.CustomerId) ) ), 5 );

			DataTable results = new DataTable();

			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, new SortExpression( CustomerFields.CompanyName | SortOperator.Ascending ),
					( CustomerFields.Country == "USA" ), null, true, null, null, 0, 0);

			Assert.AreEqual(13, results.Rows.Count);
			foreach(DataRow row in results.Rows)
			{
				Assert.IsTrue(row["NumberOfOrders"].ToString().Length > 0);
				Assert.IsTrue(row["LastOrderDate"].ToString().Length > 0);
			}
		}


		[Test]
		public void ReturnNewIfNotFoundTest()
		{
			OrderEntity order = new OrderEntity();
			order.CustomersReturnsNewIfNotFound = true;
			CustomerEntity relatedCustomer = order.Customers;
			Assert.IsNotNull( relatedCustomer );

			order.CustomersReturnsNewIfNotFound = false;
			relatedCustomer = order.Customers;
			Assert.IsNull( relatedCustomer );
		}


		[Test]
		public void InvalidFieldReadExceptionTest()
		{
			OrderEntity order = new OrderEntity();
			// should work fine.
			DateTime? orderDate = order.OrderDate;

			bool exceptionCaught = false;
			try
			{
				EntityBase.MakeInvalidFieldReadsFatal = true;
				orderDate = order.OrderDate;
			}
			catch( ORMInvalidFieldReadException )
			{
				// valid. 
				exceptionCaught = true;
			}
			finally
			{
				EntityBase.MakeInvalidFieldReadsFatal = false;
			}

			Assert.IsTrue( exceptionCaught );
		}

		
		[Test]
		public void FkFieldChangeResetTest()
		{
			CustomerEntity c = new CustomerEntity( "CHOPS" );
			OrderEntity order = c.Orders[0];		// fetches orders.
			CustomerEntity reference = (CustomerEntity)order.GetType().BaseType.InvokeMember( "_customers",
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, order, new object[] { } );
			Assert.IsNotNull( reference );

			order.CustomerId = "CHNG";
			// test if the reference is null. Can't be done through property as that will trigger lazy loading. 
			reference = (CustomerEntity)order.GetType().BaseType.InvokeMember( "_customers",
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, order, new object[] { } );

			Assert.IsNull( reference);
			Assert.IsFalse( c.Orders.Contains( order ) );
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
			fields[6].ExpressionToApply = new DbFunctionCall( "YEAR", new object[] { new DbFunctionCall( "GETDATE", null ) } );

			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable( fields, results, 0, null, null, null, true, null, null, 0, 0 );
			foreach( DataRow row in results.Rows )
			{
				DateTime realOrderDate = (DateTime)row[7];
				Assert.AreEqual( (int)row[1], realOrderDate.Month );
				Assert.AreEqual( (int)row[2], realOrderDate.Year );
				Assert.AreEqual( (int)row[3], realOrderDate.Year + 4 );
				Assert.AreEqual( (int)row[6], DateTime.Now.Year );
			}
		}


		/// <summary>
		/// Test which uses a function call in a filter. 
		/// </summary>
		[Test]
		public void DbFunctionCallTest2()
		{
			OrderCollection orders = new OrderCollection();
			orders.GetMulti((new EntityField2( "fcall", new DbFunctionCall( "dbo", "fn_CalculateOrderTotal", new object[] { OrderFields.OrderId, 1 } ) ) > 4000.0) ) ;
			Assert.AreEqual( 64, orders.Count );
		}


		[Test]
		public void NullableValuesTest()
		{
			OrderCollection orders = new OrderCollection();
			orders.GetMulti( OrderFields.ShippedDate == DBNull.Value );
			Assert.IsTrue( (orders.Count > 0) );
			Assert.IsNull( orders[0].ShippedDate );
		}


		[Test]
		public void FetchDataReaderWithProcTest()
		{
			IDataReader reader = null;
			TypedListDAO dao = new TypedListDAO();
			
			reader = dao.GetAsDataReader(null, RetrievalProcedures.GetCustOrdersOrdersCallAsQuery( "CHOPS" ), CommandBehavior.CloseConnection );
			Assert.AreEqual( 4, reader.FieldCount );
			Assert.IsFalse( reader.IsClosed );
			int rowCount = 0;
			while( reader.Read() )
			{
				Console.WriteLine( "Row: {0} | {1} | {2} | {3} |", reader.GetValue( 0 ), reader.GetValue( 1 ), reader.GetValue( 2 ), reader.GetValue( 3 ) );
				rowCount++;
			}
			Assert.AreEqual( 8, rowCount );
			reader.Close();
		}


		[Test]
		public void FetchDataReaderWithDynamicFieldsTest()
		{
			IDataReader reader = null;
			EntityFields fields = new EntityFields( 3 );
			fields[0] = CustomerFields.CustomerId;
			fields[1] = CustomerFields.CompanyName;
			fields[2] = OrderFields.OrderId;
			PredicateExpression filter = new PredicateExpression( CustomerFields.Country == "Germany" );
			RelationCollection relations = new RelationCollection();
			relations.Add( CustomerEntity.Relations.OrderEntityUsingCustomerId );

			TypedListDAO dao = new TypedListDAO();
			reader = dao.GetAsDataReader( null, fields, filter, relations, CommandBehavior.CloseConnection, 0, true );
			Assert.AreEqual( 3, reader.FieldCount );
			Assert.IsFalse( reader.IsClosed );
			int rowCount = 0;
			while( reader.Read() )
			{
				Console.WriteLine( "Row: {0} | {1} | {2} |", reader.GetValue( 0 ), reader.GetValue( 1 ), reader.GetValue( 2 ) );
				rowCount++;
			}
			Assert.AreEqual( 121, rowCount );
			reader.Close();
		}
		

		[Test]
		public void ProcFetchWithProjectionToDataTableTest()
		{
			DataTable results = new DataTable();
			DataProjectorToDataTable projector = new DataProjectorToDataTable( results );
			List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			valueProjectors.Add( new DataValueProjector( "OrderID", 0, typeof( int ) ) );
			valueProjectors.Add( new DataValueProjector( "OrderDate", 1, typeof( DateTime ) ) );
			valueProjectors.Add( new DataValueProjector( "RequiredDate", 2, typeof( DateTime ) ) );
			valueProjectors.Add( new DataValueProjector( "ShippedDate", 3, typeof( DateTime ) ) );

			using( IRetrievalQuery query = RetrievalProcedures.GetCustOrdersOrdersCallAsQuery( "CHOPS" ) )
			{
				TypedListDAO dao = new TypedListDAO();
				dao.GetAsProjection( valueProjectors, projector, null, query );
			}
			Assert.AreEqual( 8, results.Rows.Count );
			List<int> orderIDs = new List<int>();
			orderIDs.AddRange(new int[] { 10254, 10370, 10519, 10731, 10746, 10966, 11029, 11041 });
			foreach(DataRow row in results.Rows)
			{
				Assert.IsTrue(orderIDs.Contains((int)row[0]));
			}
		}


		/// <summary>
		/// Test with multiple-resultset returning proc which is projected onto a dataset.
		/// </summary>
		[Test]
		public void ProcFetchProjectionToDataSet()
		{
			DataSet results = new DataSet();
			results.Tables.Add( "Customers" );
			results.Tables.Add( "Orders" );

			using( IRetrievalQuery query = RetrievalProcedures.GetCustomersAndOrdersOnCountryCallAsQuery( "Germany" ) )
			{
				TypedListDAO dao = new TypedListDAO();
				using( IDataReader reader = dao.GetAsDataReader( null, query, CommandBehavior.CloseConnection ) )
				{
					// first resultset: Customers.
					List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
					valueProjectors.Add( new DataValueProjector( "CustomerID", 0, typeof( string ) ) );
					valueProjectors.Add( new DataValueProjector( "CompanyName", 1, typeof( string ) ) );
					// resultset contains more rows, we just project those 2.
					DataProjectorToDataTable projector = new DataProjectorToDataTable( results.Tables["Customers"] );
					dao.GetAsProjection( valueProjectors, projector, reader );

					// second resultset: Orders. 
					valueProjectors = new List<IDataValueProjector>();
					valueProjectors.Add( new DataValueProjector( "OrderID", 0, typeof( int ) ) );
					valueProjectors.Add( new DataValueProjector( "CustomerID", 1, typeof( string ) ) );
					valueProjectors.Add( new DataValueProjector( "OrderDate", 3, typeof( DateTime ) ) );
					reader.NextResult();
					projector = new DataProjectorToDataTable( results.Tables["Orders"] );
					dao.GetAsProjection( valueProjectors, projector, reader );

					reader.Close();
				}
			}
			Assert.AreEqual( 11, results.Tables["Customers"].Rows.Count );
			Assert.AreEqual( 121, results.Tables["Orders"].Rows.Count );
			Assert.AreEqual( 2, results.Tables["Customers"].Columns.Count );
			Assert.AreEqual( 3, results.Tables["Orders"].Columns.Count );
		}


		/// <summary>
		/// Test with multiple-resultset returning proc which is projected onto two entity collections (one per resultset)
		/// </summary>
		[Test]
		public void ProcFetchProjectionToEntityCollections()
		{
			CustomerCollection customers = new CustomerCollection();
			OrderCollection orders = new OrderCollection();

			using( IRetrievalQuery query = RetrievalProcedures.GetCustomersAndOrdersOnCountryCallAsQuery( "Germany" ) )
			{
				TypedListDAO dao = new TypedListDAO();
				using( IDataReader reader = dao.GetAsDataReader(null, query, CommandBehavior.CloseConnection ) )
				{
					// first resultset: Customers.
					List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
					// project value on index 0 in resultset row onto customerid
					valueProjectors.Add( new DataValueProjector( CustomerFieldIndex.CustomerId.ToString(), 0, typeof( string ) ) );
					// project value on index 1 in resultset row onto companyname
					valueProjectors.Add( new DataValueProjector( CustomerFieldIndex.CompanyName.ToString(), 1, typeof( string ) ) );
					// resultset contains more rows, we just project those 2.
					DataProjectorToIEntityCollection projector = new DataProjectorToIEntityCollection( customers );
					dao.GetAsProjection( valueProjectors, projector, reader );

					// second resultset: Orders. 
					valueProjectors = new List<IDataValueProjector>();
					//valueProjectors.Add( new DataValueProjector( OrderFieldIndex.OrderId.ToString(), 0, typeof( int ) ) );
					valueProjectors.Add( new DataValueProjector( OrderFieldIndex.CustomerId.ToString(), 1, typeof( string ) ) );
					valueProjectors.Add( new DataValueProjector( OrderFieldIndex.OrderDate.ToString(), 3, typeof( DateTime ) ) );
					reader.NextResult();
					projector = new DataProjectorToIEntityCollection( orders );
					dao.GetAsProjection( valueProjectors, projector, reader );

					reader.Close();
				}
			}
			Assert.AreEqual( 11, customers.Count );
			Assert.AreEqual( 121, orders.Count );
		}


		[Test]
		public void EntityViewFilteringTestTypedCollection()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti( null );
			EntityView<CustomerEntity> customersInGermanyView = new EntityView<CustomerEntity>( customers, (CustomerFields.Country == "Germany"), null );
			Assert.AreEqual( 11, customersInGermanyView.Count );
			for( int i = 0; i < customersInGermanyView.Count; i++ )
			{
				Console.WriteLine( "{0}: CustomerID: {1}. CompanyName: {2}",
					i, customersInGermanyView[i].CustomerId, customersInGermanyView[i].CompanyName );
			}
			EntityView<CustomerEntity> customersWhichAreOwner = new EntityView<CustomerEntity>( customers, (CustomerFields.ContactTitle == "Owner"), null );
			Assert.AreEqual( 17, customersWhichAreOwner.Count );
			EntityView<CustomerEntity> customersInGermanyAndOwnerView = new EntityView<CustomerEntity>( customers,
					 new PredicateExpression( CustomerFields.Country == "Germany" ).AddWithAnd( CustomerFields.ContactTitle == "Owner" ), null );
			Assert.AreEqual( 1, customersInGermanyAndOwnerView.Count );
		}


		[Test]
		public void CrossJoinHintTest()
		{
			RelationCollection relations = new RelationCollection();
			relations.Add(ProductEntity.Relations.SupplierEntityUsingSupplierId, JoinHint.Cross);
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(ProductFields.ProductId, 0, "ProductID");
			fields.DefineField(SupplierFields.Id, 1, "SupplierID");

			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();

			dao.GetMultiAsDataTable(fields, results, 0, null, null, relations, true, null, null, 0, 0);
			Assert.AreEqual(2233, results.Rows.Count);
		}


		[Test]
		public void OverridenNotPredicateExpressionConstructionTest()
		{
			IPredicate filter = !(CustomerFields.Country=="France");
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(filter);
			Assert.AreEqual(79, customers.Count);
		}

		[Test]
		public void OverridenOrPredicateExpressionConstructionTest()
		{
			IPredicate filter = (CustomerFields.Country=="France") | (CustomerFields.Country=="Germany");
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(filter);
			Assert.AreEqual(22, customers.Count);
		}


		[Test]
		public void FieldCompareValueTestWithExpression()
		{
			IPredicate filter = (OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == 6);
			OrderCollection orders= new OrderCollection();
			orders.GetMulti(filter);
			Assert.AreEqual(254, orders.Count);
		}


		/// <summary>
		/// Test field with expression compared to expression.
		/// </summary>
		[Test]
		public void FieldCompareExpressionTestWithExpression()
		{
			IPredicate filter = (OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == OrderFields.EmployeeId);
			OrderCollection orders= new OrderCollection();
			orders.GetMulti(filter);
			Assert.AreEqual(135, orders.Count);
		}


		[Test]
		public void FieldCompareRangeTestWithExpression()
		{
			ArrayList values = new ArrayList();
			values.Add(4);
			values.Add(6);
			IPredicate filter = (OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2) == values);
			OrderCollection orders= new OrderCollection();
			orders.GetMulti(filter);
			Assert.AreEqual(574, orders.Count);
		}


		[Test]
		public void FieldCompareSetTestWithExpression()
		{
			IPredicate filter = new FieldCompareSetPredicate(
				OrderFields.ShipVia.SetExpression(OrderFields.ShipVia * 2),
				OrderFields.EmployeeId.SetObjectAlias("O"), SetOperator.In, 
				(OrderFields.OrderId.SetObjectAlias("O")==OrderFields.OrderId));

			OrderCollection orders= new OrderCollection();
			orders.GetMulti(filter);
			Assert.AreEqual(135, orders.Count);
		}


		[Test]
		public void FetchSingleEntityWithContextAndUCTest()
		{
			CustomerEntity customer1 = new CustomerEntity();
			Assert.IsTrue( customer1.FetchUsingUCCompanyName("The Big Cheese") );
			Assert.AreEqual(EntityState.Fetched, customer1.Fields.State);
			Context c = new Context();
			c.Add(customer1);

			CustomerEntity customer2 = new CustomerEntity();
			Assert.IsTrue( customer2.FetchUsingUCCompanyName("The Big Cheese", null, c) );
			Assert.AreEqual(customer1, customer2);
			Assert.AreSame(customer1.Fields, customer2.Fields);
		}


		[Test]
		public void FetchSingleEntityWithContextTest()
		{
			CustomerEntity customer1 = new CustomerEntity("CHOPS");
			Assert.AreEqual(EntityState.Fetched, customer1.Fields.State);
			Context c = new Context();
			c.Add(customer1);

			CustomerEntity customer2 = new CustomerEntity();
			Assert.IsTrue(customer2.FetchUsingPK("CHOPS", null, c));
			Assert.AreEqual(customer1, customer2);
			Assert.AreSame(customer1.Fields, customer2.Fields);
		}
		
		[Test]
		public void DupeTestWithEntityCollection()
		{
			CustomerCollection customers = new CustomerCollection();
			// fetch 10 customers.
			customers.GetMulti(null, 10);
			Assert.AreEqual(10, customers.Count);

			// now fetch all of them, again into the same collection. No dupes should be added.
			customers.GetMulti(null);
			Assert.AreEqual(91, customers.Count);
		}


		[Test]
		public void FieldBetweenPredicateTest()
		{
			IPredicate filter = new FieldBetweenPredicate(OrderFields.EmployeeId, 2, 4);
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(filter);
			Assert.IsTrue((orders.Count>0));

			foreach(OrderEntity o in orders)
			{
				Assert.IsTrue(((o.EmployeeId>=2) && (o.EmployeeId<=4)));
			}

			filter = new FieldBetweenPredicate(OrderFields.ShippedDate, OrderFields.OrderDate, OrderFields.RequiredDate);
			filter.Negate=true;
			orders.Clear();
			orders.GetMulti(filter);
			Assert.AreEqual(38, orders.Count);
		}


		[Test]
		public void PrefetchPathWithPagingTest()
		{
			CustomerCollection customers = new CustomerCollection();
			PrefetchPath path = new PrefetchPath((int)EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);

			SortExpression sorter = new SortExpression(CustomerFields.CustomerId | SortOperator.Ascending);
			customers.GetMulti(null, 0, sorter, null, path, 3, 7);
			Assert.AreEqual(7, customers.Count);
			Assert.AreEqual("FAMIA", customers[6].CustomerId);
			Assert.AreEqual(6, customers[6].Orders.Count);
		}


		/// <summary>
		/// Tests simple equals functionality
		/// </summary>
		[Test]
		public void EqualsTest()
		{
			EmployeeEntity e = new EmployeeEntity(1);
			EmployeeEntity e2 = new EmployeeEntity(1);

			Assert.IsTrue(e.Equals(e2));

			ProductEntity p = new ProductEntity(1);
			Assert.IsFalse(e.Equals(p));
		}


		/// <summary>
		/// Tests the fetch of a normal typed list, generated by llblgen pro and the designer.
		/// Using a normal filter on Orders of 'CACTU', which should result in 6 rows.
		/// </summary>
		[Test]
		public void NormalTypedListFetchTest()
		{
			OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(EntityFieldFactory.Create(OrderFieldIndex.CustomerId), ComparisonOperator.Equal, "CACTU"));
			orderCustomer.Fill(0, null, true, filter);
			Assert.AreEqual(6, orderCustomer.Rows.Count);
		}


		/// <summary>
		/// Tests the lazy loading code with a context (is the same for 1:1 and m:1 relations). 
		/// </summary>
		[Test]
		public void ManyToOneLazyLoadingContextTest()
		{
			Context testContext = new Context();
			// fetch customer.
			CustomerEntity c = new CustomerEntity("CHOPS");
			testContext.Add(c);

			OrderEntity o = new OrderEntity(10254);
			testContext.Add(o);

			// now fetch the related customer. This is the same customer as loaded into c and should be the same reference.
			CustomerEntity c2 = o.Customers;
			Assert.AreSame(c, c2);
			Assert.AreEqual(c, c2);
		}


		[Test]
		public void RandomFetchTest()
		{
			IEntityField newIDField = EntityFieldFactory.Create(CustomerFieldIndex.Country);
			newIDField.Alias = "NEWID()";
			newIDField.ExpressionToApply = new Expression();

			ISortExpression sorter = new SortExpression(new SortClause(newIDField, SortOperator.Ascending));
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(null, 0, sorter, null);
			Assert.AreEqual(91, customers.Count);
		}


		/// <summary>
		/// Tests a typed list fetch of a typed list constructed in code, into a normal datatable.
		/// This test uses a join with self an a filter on aliases.
		/// </summary>
		[Test]
		public void CustomConstructedTypedListOrderByTest()
		{
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "Employee");
			fields.DefineField(EmployeeFields.FirstName, 1, "FirstName", "Employee");
			fields.DefineField(EmployeeFields.LastName, 2, "LastName", "Employee");
			fields.DefineField(EmployeeFields.EmployeeId, 3, "ManagerId", "Manager");
			fields.DefineField(EmployeeFields.FirstName, 4, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 5, "LastNameManager", "Manager");
			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "Employee", "Manager", JoinHint.None);

			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(fields[0], SortOperator.Ascending));
			sorter.Add(new SortClause(fields[3], SortOperator.Ascending));

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();

			dao.GetMultiAsDataTable(fields, tlist, 0, sorter, null, relations, false, null, null, 0, 0);
		}


		/// <summary>
		/// Fetches a typed list with a blob field
		/// </summary>
		[Test]
		public void TypedListBlobTest()
		{
			EmployeeBlobTypedList tlist = new EmployeeBlobTypedList();
			tlist.Fill();
			Assert.IsTrue((tlist.Count>0));
		}

		/// <summary>
		/// Tests the filter capability of A.GetMultiB() method calls of entity A for retrieving related entities B filtered.
		/// </summary>
		[Test]
		public void GetMultiWithFilterTestOnContainedEntityCollection()
		{
			CustomerEntity chops = new CustomerEntity("CHOPS");
			IPredicateExpression filter = new PredicateExpression(new FieldCompareValuePredicate(OrderFields.EmployeeId, ComparisonOperator.Equal, 6));
			chops.GetMultiOrders(true, new OrderEntityFactory(), filter);
			Assert.AreEqual(1, chops.Orders.Count);
		}


		/// <summary>
		/// Tests a typed list fetch of a typed list constructed in code, into a normal datatable.
		/// This test uses a join with self an a filter on aliases.
		/// </summary>
		[Test]
		public void CustomConstructedTypedListFetchTest()
		{
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "Employee");
			fields.DefineField(EmployeeFields.FirstName, 1, "FirstName", "Employee");
			fields.DefineField(EmployeeFields.LastName, 2, "LastName", "Employee");
			fields.DefineField(EmployeeFields.EmployeeId, 3, "ManagerId", "Manager");
			fields.DefineField(EmployeeFields.FirstName, 4, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 5, "LastNameManager", "Manager");
			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "Employee", "Manager", JoinHint.None);
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(EmployeeFields.LastName, ComparisonOperator.Equal, "Buchanan", "Manager"));

			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(EmployeeFields.LastName, SortOperator.Descending, "Manager"));

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();

			// Get count
			EntityFields scalarFields = new EntityFields(1);
			scalarFields[0]=fields[0];
			scalarFields[0].AggregateFunctionToApply = AggregateFunction.CountRow;
			int count = (int)dao.GetScalar(scalarFields, null, filter, relations, null, false);
			scalarFields[0].AggregateFunctionToApply=AggregateFunction.None;
			Assert.AreEqual(2, count);

			dao.GetMultiAsDataTable(fields, tlist, 0, sorter, filter, relations, false, null, null, 0, 0);

			// has to be reading 3 rows
			Assert.AreEqual(2, tlist.Rows.Count);
		}


		[Test]
		public void GroupByOnExpressionTest()
		{
			ResultsetFields fields = new ResultsetFields( 2 );
			fields.DefineField( OrderFields.OrderDate, 0, "MonthOrder" );
			fields.DefineField( OrderFields.OrderId, 1, "NumberOfOrdersInMonth" );
			fields[0].ExpressionToApply = new DbFunctionCall( "month", new object[] { OrderFields.OrderDate } );
			fields[1].AggregateFunctionToApply = AggregateFunction.Count;

			SortExpression sorter = new SortExpression( new SortClause( fields[0], SortOperator.Ascending ) );
			GroupByCollection groupBy = new GroupByCollection();
			groupBy.Add( fields[0] );

			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable( fields, results, 0, sorter, null, null, true, groupBy, null, 0, 0 );

			Assert.AreEqual( 12, results.Rows.Count );
			for( int i = 0; i < 12; i++ )
			{
				Assert.AreEqual( (i + 1), (int)results.Rows[i][0] );
				switch( i + 1 )
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
		/// Tests the group by clause functionality.
		/// </summary>
		[Test]
		public void GroupByTest()
		{
			OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(OrderFields.CustomerId, ComparisonOperator.Equal, "CACTU"));

			// create group by clause
			IGroupByCollection groupByClause = new GroupByCollection();
			EntityFields fields = (EntityFields)((ITypedListLgp)orderCustomer).BuildResultset();

			for (int i = 0; i < fields.Count; i++)
			{
				groupByClause.Add(fields[i]);
			}


			orderCustomer.Fill(0, null, true, filter, null, groupByClause);
			Assert.AreEqual(6, orderCustomer.Rows.Count);
		}


		[Test]
		public void GroupByAmountOrdersPerCustomerTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(OrderFields.CustomerId, 0, "CustomerID");
			fields.DefineField(OrderFields.OrderId, 1, "AmountOrders", "", AggregateFunction.CountDistinct);

			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, null, null, true, groupByClause, null, 0, 0);
			Assert.AreEqual(89, tlist.Rows.Count);
			foreach(DataRow row in tlist.Rows)
			{
				Assert.IsTrue(Convert.ToInt32(row[1]) > 0);
			}
		}


		/// <summary>
		/// Tests the group by clause functionality.
		/// </summary>
		[Test]
		public void GroupByTestCustomList()
		{
			ResultsetFields fields = new ResultsetFields(3);

			fields.DefineField(EmployeeFields.FirstName, 0, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 1, "LastNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 2, "AmountEmployees", "Employee", AggregateFunction.Count);
			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "Employee", "Manager", JoinHint.None);
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(EmployeeFields.LastName, ComparisonOperator.Equal, "Buchanan", "Manager"));

			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			groupByClause.Add(fields[1]);

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, filter, relations, true, groupByClause, null, 0, 0);

			Assert.AreEqual(1, tlist.Rows.Count);
			Assert.AreEqual(2, (int)tlist.Rows[0]["AmountEmployees"]);
		}


		/// <summary>
		/// Tests simple expressions on a single entity. (orderdetail)
		/// </summary>
		[Test]
		public void ExpressionTestSimple()
		{
			ResultsetFields fields = new ResultsetFields(6);

			fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
			fields.DefineField(OrderDetailsFields.ProductId, 1, "ProductId");
			fields.DefineField(OrderDetailsFields.UnitPrice, 2, "TotalPrice");	// use unitprice field because it is already of the type we want.
			fields.DefineField(OrderDetailsFields.UnitPrice, 3, "UnitPrice");
			fields.DefineField(OrderDetailsFields.Quantity, 4, "Quantity");
			fields.DefineField(OrderDetailsFields.Discount, 5, "Discount");

			// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) )
			IExpression productPriceExpression = new Expression(
				EntityFieldFactory.Create(OrderDetailsFieldIndex.UnitPrice),
				ExOp.Mul,
				EntityFieldFactory.Create(OrderDetailsFieldIndex.Quantity));
			IExpression discountExpression = new Expression(
				productPriceExpression, 
				ExOp.Mul,
				EntityFieldFactory.Create(OrderDetailsFieldIndex.Discount));
			IExpression totalPriceExpression = new Expression(
				productPriceExpression,
				ExOp.Sub,
				discountExpression);
			fields[2].ExpressionToApply = totalPriceExpression;

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, null, null, true, null, null, 0, 0);

			// test the values.
			for (int i = 0; i < tlist.Rows.Count; i++)
			{
				float productPrice = (float)(((decimal)tlist.Rows[i]["UnitPrice"]) * ((Int16)tlist.Rows[i]["Quantity"]));
				float totalPrice = productPrice - (productPrice * (float)tlist.Rows[i]["Discount"]);

				Assert.AreEqual( Math.Round(Convert.ToDecimal(tlist.Rows[i]["TotalPrice"]), 3), Math.Round(Convert.ToDecimal(totalPrice), 3) );
			}
		}



		/// <summary>
		/// Tests the group by clause functionality with an expression using joins and aliasses
		/// </summary>
		[Test]
		public void GroupByTestCustomListWithExpression()
		{
			ResultsetFields fields = new ResultsetFields(3);

			fields.DefineField(EmployeeFields.FirstName, 0, "FirstNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 1, "LastNameManager", "Manager");
			fields.DefineField(EmployeeFields.LastName, 2, "MaxEmployeeId", "EmployeeAlias", AggregateFunction.Max);

			// Add expression: EmployeeId + 10
			IEntityField field = EntityFieldFactory.Create(EmployeeFieldIndex.EmployeeId);
			field.ObjectAlias = "EmployeeAlias";
			IExpression maxPlusTen = new Expression(
				field,
				ExOp.Add,
				10);
			fields[2].ExpressionToApply = maxPlusTen;

			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, "EmployeeAlias", "Manager", JoinHint.None);
			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			groupByClause.Add(fields[1]);

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, null, relations, true, groupByClause, null, 0, 0);

			Assert.AreEqual(17, (int)tlist.Rows[0]["MaxEmployeeId"]);
			Assert.AreEqual(18, (int)tlist.Rows[1]["MaxEmployeeId"]);
			Assert.AreEqual(5, tlist.Rows.Count);
		}


		/// <summary>
		/// Tests simple expressions on a single entity (orderdetail), using a groupby on orderID and calculates the total price of the complete order.
		/// it also tests the having clause, it selects rows with a totalprice > 1000,00
		/// </summary>
		[Test]
		public void ExpressionTestSimpleGroupByAggregateHaving()
		{
			ResultsetFields fields = new ResultsetFields(2);

			fields.DefineField(OrderDetailsFields.OrderId, 0, "OrderId");
			fields.DefineField(OrderDetailsFields.UnitPrice, 1, "TotalPrice");	// use unitprice field because it is already of the type we want.

			// Expression for total price: ((unitprice * quantity) - ((unitprice * quantity) * discount) )
			IExpression productPriceExpression = new Expression(
				EntityFieldFactory.Create(OrderDetailsFieldIndex.UnitPrice),
				ExOp.Mul,
				EntityFieldFactory.Create(OrderDetailsFieldIndex.Quantity));
			IExpression discountExpression = new Expression(
				productPriceExpression, 
				ExOp.Mul,
				EntityFieldFactory.Create(OrderDetailsFieldIndex.Discount));
			IExpression totalPriceExpression = new Expression(
				productPriceExpression,
				ExOp.Sub,
				discountExpression);
			fields[1].ExpressionToApply = totalPriceExpression;
			fields[1].AggregateFunctionToApply = AggregateFunction.Sum;
			IGroupByCollection groupByClause = new GroupByCollection();
			groupByClause.Add(fields[0]);
			IPredicateExpression havingFilter = new PredicateExpression();
			havingFilter.Add(new FieldCompareValuePredicate(fields[1], ComparisonOperator.GreaterThan, 1000.0f));
			groupByClause.HavingClause = havingFilter;

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, null, null, true, groupByClause, null, 0, 0);
		}


		/// <summary>
		/// Tests the FieldCompareExpression predicate, doesn't use the PredicateFactory helperclass.
		/// </summary>
		[Test]
		public void FieldCompareExpressionTest()
		{
			// fetch all orders which have a difference between orderdate and shipping date of 5 days.
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareExpressionPredicate(
				EntityFieldFactory.Create(OrderFieldIndex.ShippedDate),
				ComparisonOperator.Equal,
				new Expression(
				EntityFieldFactory.Create(OrderFieldIndex.OrderDate),
				ExOp.Add,
				5)));
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(filter);

			Assert.AreEqual(63, orders.Count);
		}


		/// <summary>
		/// Tests the FieldCompareValue predicate, doesn't use the PredicateFactory helperclass, and mimics the same 
		/// filter as FieldCompareExpressionTest, but now the expression is defined on the field.
		/// </summary>
		[Test]
		public void FieldCompareValueWithFieldExpressionTest()
		{
			// fetch all orders which have a difference between orderdate and shipping date of 5 days.
			IPredicateExpression filter = new PredicateExpression();
			IExpression fieldExpression = new Expression(
				EntityFieldFactory.Create(OrderFieldIndex.ShippedDate),
				ExOp.Sub,
				EntityFieldFactory.Create(OrderFieldIndex.OrderDate));
			FieldCompareValuePredicate fieldFilter = new FieldCompareValuePredicate(
				EntityFieldFactory.Create(OrderFieldIndex.OrderDate),	// not important which field, just pick one, is replaced with expression anyway.
				ComparisonOperator.Equal,
				5);
			fieldFilter.FieldCore.ExpressionToApply = fieldExpression;
			filter.Add(fieldFilter);
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(filter);

			Assert.AreEqual(63, orders.Count);
		}


		/// <summary>
		/// Sorts the collection using the Sort(index, direction) method (Orders, descending on OrderDate)
		/// </summary>
		[Test]
		public void ClientSideCollectionSortTest()
		{
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(null);

			orders.Sort((int)OrderFieldIndex.OrderDate, ListSortDirection.Descending);

			for (int i = 0; i < orders.Count-1; i++)
			{
				Assert.IsTrue(!( orders[i].OrderDate.GetValueOrDefault().CompareTo(orders[i+1].OrderDate) < 0));
			}
		}


		/// <summary>
		/// Tests the scalar execution functionality with a simple count(*) call
		/// </summary>
		[Test]
		public void ScalarTestSimple()
		{
			OrderCollection orders = new OrderCollection();
			int amountOrderRows = (int)orders.GetScalar(OrderFieldIndex.OrderId, AggregateFunction.CountRow);
			Assert.AreEqual(818, amountOrderRows);
		}


		/// <summary>
		/// Tests the scalar execution functionality with a bogus expression and an aggregate:
		/// Max(EmployeeID - ShipVia) on orders
		/// </summary>
		[Test]
		public void ScalarTestExpression()
		{
			OrderCollection orders = new OrderCollection();
			IExpression subExpression = new Expression(
				EntityFieldFactory.Create(OrderFieldIndex.EmployeeId),
				ExOp.Sub,
				EntityFieldFactory.Create(OrderFieldIndex.ShipVia));
			int maxValue = (int)orders.GetScalar(OrderFieldIndex.OrderId, subExpression, AggregateFunction.Max);
			Assert.AreEqual(8, maxValue);
		}


		/// <summary>
		/// Calculates with an expression the product price and then calculates the total price for order 10254
		/// </summary>
		[Test]
		public void ScalarTestAggregate()
		{
			OrderDetailsCollection orderDetails = new OrderDetailsCollection();
			IExpression productPriceExpression = new Expression(
				EntityFieldFactory.Create(OrderDetailsFieldIndex.Quantity),
				ExOp.Mul,
				EntityFieldFactory.Create(OrderDetailsFieldIndex.UnitPrice));
			IPredicate filter = new FieldCompareValuePredicate(OrderDetailsFields.OrderId, ComparisonOperator.Equal, 10254);
			decimal orderPrice = (decimal)orderDetails.GetScalar(OrderDetailsFieldIndex.OrderId, productPriceExpression, AggregateFunction.Sum, filter);
			Assert.AreEqual(9987.6M, orderPrice);
		}


		/// <summary>
		/// Tests the GetDbCount feature added to a collection. No filter is applied
		/// </summary>
		[Test]
		public void GetDbCountTestSimple()
		{
			OrderCollection orders = new OrderCollection();
			int amountOrderRows = orders.GetDbCount();
			Assert.AreEqual(818, amountOrderRows);
		}


		/// <summary>
		/// Tests the GetDbCount feature, using a multi-entity filter on order-customer.
		/// </summary>
		[Test]
		public void GetDbCountTestWithFilter()
		{
			OrderCollection orders = new OrderCollection();
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(CustomerFields.Country, ComparisonOperator.Equal, "France"));
			IRelationCollection relations = new RelationCollection();
			relations.Add(OrderEntity.Relations.CustomerEntityUsingCustomerId);
			int amount = orders.GetDbCount(filter, relations);
			Assert.AreEqual(74, amount);
		}


		/// <summary>
		/// Tests the FieldCompareSetPredicate using a simple subquery.
		/// select * from products where productid not in (select productid from [order details])
		/// or: all products which are not sold in an order.
		/// </summary>
		[Test]
		public void FieldCompareSetTestSimple()
		{
			IPredicateExpression filter = new PredicateExpression(new FieldCompareSetPredicate(
				EntityFieldFactory.Create(ProductFieldIndex.ProductId),
				EntityFieldFactory.Create(OrderDetailsFieldIndex.ProductId),
				SetOperator.In,
				null, 
				true));
			ProductCollection  notSoldProducts = new ProductCollection();
			notSoldProducts.GetMulti(filter);
			Assert.AreEqual(3, notSoldProducts.Count);
		}


		[Test]
		public void DistinctViolationTest()
		{
			CategoryCollection categories = new CategoryCollection();
			RelationCollection relations = new RelationCollection();
			IPredicate filter = new FieldCompareValuePredicate(ProductFields.SupplierId, ComparisonOperator.Equal, 2);
			relations.Add(CategoryEntity.Relations.ProductEntityUsingCategoryId);
			categories.GetMulti(filter, relations);
			Assert.AreEqual(1, categories.Count);
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
			ResultsetFields fields = new ResultsetFields(2);

			fields.DefineField(EmployeeFields.EmployeeId, 0, "EmployeeId", "E");
			fields.DefineField(OrderFields.OrderId, 1, "OrderId", "O");
			RelationCollection relationsTest = new RelationCollection();
			relationsTest.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);

			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId, "E", "O", JoinHint.None);
			IPredicate filter = new FieldCompareSetPredicate(
				EntityFieldFactory.Create(OrderFieldIndex.OrderId),
				EntityFieldFactory.Create(OrderFieldIndex.OrderId),
				SetOperator.Equal,
				new FieldCompareExpressionPredicate(
				EntityFieldFactory.Create(OrderFieldIndex.EmployeeId),
				ComparisonOperator.Equal,
				new Expression(fields[0])),
				relationsTest, 
				"O",
				1,
				new SortExpression(new SortClause(OrderFields.OrderDate, SortOperator.Descending))
				);

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, filter, relations, true, null, null, 0, 0);

			Assert.AreEqual(9, tlist.Rows.Count);
		}


		/// <summary>
		/// Tests a complex paging query with a sorter on a non-selectlist field.
		/// </summary>
		[Test]
		public void EntityCollectionPagingComplexSorter()
		{
			ProductCollection products = new ProductCollection();
			ISortExpression sorter = new SortExpression(new SortClause(SupplierFields.CompanyName, SortOperator.Ascending));
			IRelationCollection relations = new RelationCollection();
			relations.Add(ProductEntity.Relations.SupplierEntityUsingSupplierId);
			IPredicate filter = new FieldCompareValuePredicate(ProductFields.CategoryId, ComparisonOperator.LesserThan, 8);
			products.GetMulti(filter, 0, sorter, relations, 1, 10);
			Assert.AreEqual(10, products.Count);
			Assert.AreEqual(38, products[0].ProductId);
		}


		/// <summary>
		/// Tests the clearing of a contained collection and the dereference code.
		/// </summary>
		[Test]
		public void ContainedEntityCollectionClearTest()
		{
			CustomerEntity c = new CustomerEntity("CHOPS");
			OrderCollection orders = c.Orders;

			// now clear the orders
			orders.Clear();
		}


		/// <summary>
		/// Tests to see if a dereferenced entity is indeed setting the FK field(s) to null
		/// </summary>
		[Test]
		public void FkResetTest()
		{
			OrderEntity order = new OrderEntity(10254);
			CustomerEntity customer = order.Customers;		// lazy loading
			Assert.AreEqual(customer.CustomerId, order.CustomerId);
			Assert.IsFalse(customer.IsNew);
			Assert.IsTrue(customer.Orders.Contains(order));

			// dereference
			order.Customers = null;
			Assert.IsNull(order.Fields[(int)OrderFieldIndex.CustomerId].CurrentValue);
		}


		/// <summary>
		/// Tests a complex paging query with duplicates to see if the code can still retrieve a full page with non-duplicate
		/// objects without using DISTINCT
		/// This test will fail if the client side paging code fails, which means that the page loaded will have 3 objects, however the page 
		/// should have 1 object.
		/// It first fetches page 2, then page 3. Page 2 has to have 3 objects, page 3 has to have 1 object.
		/// </summary>
		/// <remarks>Fails until fix is implemented. BUG 96</remarks>
		[Test]
		public void EntityCollectionPagingComplexNonDistinctDuplicate()
		{
			EmployeeCollection employees = new EmployeeCollection();
			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);
			IPredicate filter = new FieldLikePredicate(OrderFields.CustomerId, "A%");
			employees.GetMulti(filter, 0, null, relations, 2, 3);
			Assert.AreEqual(3, employees.Count);
			employees.Clear();
			employees.DoNotPerformAddIfPresent = false;
			employees.GetMulti(filter, 0, null, relations, 3, 3);
			Assert.AreEqual(1, employees.Count);
		}


		/// <summary>
		/// Tests the paging facility when fetching an entity collection, with a simple straight forward query (Select all orders, ordered by OrderID asc)
		/// </summary>
		[Test]
		public void EntityCollectionPagingTestSimple()
		{
			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(OrderFields.OrderId, SortOperator.Ascending));

			OrderCollection pagedOrders = new OrderCollection();
			pagedOrders.GetMulti(null, 0, sorter, null, 4, 10);

			Assert.AreEqual(10, pagedOrders.Count);
			Assert.AreEqual(10278, pagedOrders[0].OrderId);
		}


		/// <summary>
		/// Tests the paging facility when fetching an entity collection, with a more sophisticated query 
		/// </summary>
		[Test]
		public void EntityCollectionPagingTestComplex()
		{
			// fetch all orders which have a difference between orderdate and shipping date of 5 days.
			IPredicateExpression filter = new PredicateExpression();
			IExpression fieldExpression = new Expression(
				EntityFieldFactory.Create(OrderFieldIndex.ShippedDate),
				ExOp.Sub,
				EntityFieldFactory.Create(OrderFieldIndex.OrderDate));
			FieldCompareValuePredicate fieldFilter = new FieldCompareValuePredicate(
				EntityFieldFactory.Create(OrderFieldIndex.OrderDate),	// not important which field, just pick one, is replaced with expression anyway.
				ComparisonOperator.Equal,
				5);
			fieldFilter.FieldCore.ExpressionToApply = fieldExpression;
			filter.Add(fieldFilter);
			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(OrderFields.OrderId, SortOperator.Ascending));

			OrderCollection orders = new OrderCollection();
			orders.GetMulti(filter, 0, sorter, null, 2, 11);

			Assert.AreEqual(11, orders.Count);
			Assert.AreEqual(10372, orders[0].OrderId);
		}


		/// <summary>
		/// Tests the fetch of a normal typed list, generated by llblgen pro and the designer.
		/// </summary>
		[Test]
		public void TypedListPagingTest()
		{
			OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(OrderFields.OrderId, SortOperator.Ascending));
			orderCustomer.Fill(0, sorter, true, null, null, null, 3, 12);
			Assert.AreEqual(12, orderCustomer.Rows.Count);
			Assert.AreEqual(10272, orderCustomer[0].OrderId);
		}


		/// <summary>
		/// Tests the GetDbCount feature for a typed list.
		/// </summary>
		[Test]
		public void GetDbCountTypedListTest()
		{
			OrderCustomerTypedList orderCustomer = new OrderCustomerTypedList();
			int dbCount = orderCustomer.GetDbCount(true);
			Assert.AreEqual(818, dbCount);
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load all customers and all their orders.
		/// </summary>
		[Test]
		public void PrefetchPathTestSimple()
		{
			CustomerCollection customers = new CustomerCollection();
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			customers.GetMulti(null, prefetchPath);

			List<string> customersWithoutOrders = new List<string>();
			customersWithoutOrders.Add("FISSA");
			customersWithoutOrders.Add("PARIS");
			for (int i = 0; i < customers.Count; i++)
			{
				// get member collections to circumvent lazyloading so to check if prefetch paths really did work.
				List<IEntityCollection> memberCollections =((IEntity)customers[i]).GetMemberEntityCollections();
				// orders is second instance in collection
				OrderCollection orders = (OrderCollection)memberCollections[1];
				for (int j = 0; j < orders.Count; j++)
				{
					Assert.AreEqual(customers[i].CustomerId, orders[j].CustomerId);
				}
				if(orders.Count<=0)
				{
					Assert.IsTrue(customersWithoutOrders.Contains(customers[i].CustomerId));
				}
			}
		}


		/// <summary>
		/// Tests refetching the same prefetch path with extra nodes. This works with a Context for uniquing.
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleRefetchUniquingTest()
		{
			// first fetch the path without the order details
			Context testContext = new Context();
			CustomerCollection customers = new CustomerCollection();
			testContext.Add(customers);
			SortExpression sorter = new SortExpression(new SortClause(CustomerFields.CustomerId, SortOperator.Ascending));
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			customers.GetMulti(null, 0, sorter, null, prefetchPath);
			// check that no order details were loaded.
			for (int i = 0; i < customers.Count; i++)
			{
				// get member collections to circumvent lazyloading so to check if prefetch paths really did work.
				List<IEntityCollection> memberCollections = ((IEntity)customers[i]).GetMemberEntityCollections();
				// orders is second instance in collection
				OrderCollection orders = (OrderCollection)memberCollections[1];
				for (int j = 0; j < orders.Count; j++)
				{
					Assert.AreEqual(customers[i].CustomerId, orders[j].CustomerId);
				}
				// another test tests the prefetch path functionality. This test assumes the orders are loaded.
				if(orders.Count>0)
				{
					foreach(OrderEntity o in orders)
					{
						// grab OrderDetails collection in order. This is first collection
						OrderDetailsCollection orderDetails = (OrderDetailsCollection)((IEntity)o).GetMemberEntityCollections()[0];
						Assert.IsTrue((orderDetails.Count<=0));
					}
				}
			}

			CustomerEntity c1 = customers[0];

			// now refetch the path, WITH the order details. This fails if uniquing fails. 
			prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			// refetch
			customers.GetMulti(null, 0, sorter, null, prefetchPath);

			CustomerEntity c2 = customers[0];

			// test uniquing
			Assert.AreEqual(c1, c2);
			Assert.IsFalse(c1.IsDirty);
			Assert.IsFalse(c2.IsDirty);
			// change field
			c1.CompanyName+="foo";
			// c2's company name field has to be changed now
			Assert.IsTrue(c2.IsDirty);
			Assert.IsTrue(c2.Fields["CompanyName"].IsChanged);

			ArrayList customersWithoutOrders = new ArrayList();
			customersWithoutOrders.Add("FISSA");
			customersWithoutOrders.Add("PARIS");
			for (int i = 0; i < customers.Count; i++)
			{
				// get member collections to circumvent lazyloading so to check if prefetch paths really did work.
				List<IEntityCollection> memberCollections = ((IEntity)customers[i]).GetMemberEntityCollections();
				// orders is second instance in collection
				OrderCollection orders = (OrderCollection)memberCollections[1];
				for (int j = 0; j < orders.Count; j++)
				{
					Assert.AreEqual(customers[i].CustomerId, orders[j].CustomerId);
				}
				if(orders.Count<=0)
				{
					Assert.IsTrue(customersWithoutOrders.Contains(customers[i].CustomerId));
				}
				else
				{
					// check for order details. Has to be loaded.
					foreach(OrderEntity o in orders)
					{
						// grab OrderDetails collection in order. This is first collection
						OrderDetailsCollection orderDetails = (OrderDetailsCollection)((IEntity)o).GetMemberEntityCollections()[0];
						Assert.IsTrue((orderDetails.Count>0));
					}
				}
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load all customers from france and all their related employees (m:n)
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleMultiEntityManyToMany()
		{
			CustomerCollection customers = new CustomerCollection();
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathEmployees);
			IPredicate filter = new FieldCompareValuePredicate(CustomerFields.Country, ComparisonOperator.Equal, "France");
			customers.GetMulti(filter, prefetchPath);

			for (int i = 0; i < customers.Count; i++)
			{
				// get the alreadyFetchedEmployeesFlag:
				bool alreadyFetchedEmployees = (bool)customers[i].GetType().BaseType.InvokeMember("_alreadyFetchedEmployees", 
						BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, customers[i], new object [] {});
				
				Assert.IsTrue(alreadyFetchedEmployees);

				if(customers[i].CustomerId!="PARIS")
				{
					Assert.IsTrue(customers[i].Employees.Count>0);
				}
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load one customer and all its employees (m:n)
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleSingleEntityManyToMany()
		{
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathEmployees);
			CustomerEntity customer = new CustomerEntity("BLONP", prefetchPath);
			bool alreadyFetchedEmployees = (bool)customer.GetType().BaseType.InvokeMember("_alreadyFetchedEmployees", 
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, customer, new object [] {});
			Assert.IsTrue(alreadyFetchedEmployees);

			Assert.IsFalse(customer.IsNew);
			Assert.AreEqual(7, customer.Employees.Count);
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load one customer and all its employees (m:n)
		/// This test follows the unoptimized codepath
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleSingleEntityManyToManyForcedCodePath()
		{
			DaoBase.ParameterisedPrefetchPathThreshold=0;
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathEmployees);
			CustomerEntity customer = new CustomerEntity("BLONP", prefetchPath);
			bool alreadyFetchedEmployees = (bool)customer.GetType().BaseType.InvokeMember("_alreadyFetchedEmployees", 
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, customer, new object [] {});
			Assert.IsTrue(alreadyFetchedEmployees);

			Assert.IsFalse(customer.IsNew);
			Assert.AreEqual(7, customer.Employees.Count);
			DaoBase.ParameterisedPrefetchPathThreshold=50;
		}


		/// <summary>
		/// Tests the prefetch path functionality. Simple test: load one customer and all its orders, based on the customer's Unique constraint.
		/// </summary>
		[Test]
		public void PrefetchPathTestSimpleSingleEntityUC()
		{
			CustomerEntity customer = new CustomerEntity();
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathOrders).SubPath.Add(OrderEntity.PrefetchPathOrderDetails);
			customer.FetchUsingUCCompanyName("Blauer See Delikatessen", prefetchPath);
			bool alreadyFetchedOrders = (bool)customer.GetType().BaseType.InvokeMember("_alreadyFetchedOrders", 
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, customer, new object [] {});
			Assert.IsTrue(alreadyFetchedOrders);
			Assert.IsFalse(customer.IsNew);
			Assert.AreEqual(7, customer.Orders.Count);
		}


		/// <summary>
		/// Tests the prefetch path functionality. Loads all employees and per employee the last order object they closed.
		/// Similar to FieldCompareSetTestComplex but now with entities.
		/// </summary>
		[Test]
		public void PrefetchPathTestComplex()
		{
			EmployeeCollection employees = new EmployeeCollection();
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.EmployeeEntity);
			ISortExpression sorter = new SortExpression();
			sorter.Add(new SortClause(OrderFields.OrderDate, SortOperator.Descending));
			prefetchPath.Add(EmployeeEntity.PrefetchPathOrders, 1, null, null, sorter).SubPath.Add(OrderEntity.PrefetchPathShippers);

			employees.GetMulti(null, prefetchPath);
			ShipperEntity shipper1 = employees[0].Orders[0].Shippers;

			for (int i = 0; i < employees.Count; i++)
			{
				Assert.AreEqual(employees[i].Orders[0].ShipVia, employees[i].Orders[0].Shippers.ShipperId);
			}
		}


		/// <summary>
		/// Tests if passing null values to GetMultiManyToOne will call GetMulti() instead.
		/// </summary>
		[Test]
		public void GetMultiManyToOneTest()
		{
			OrderCollection orders = new OrderCollection();
			orders.GetMulti(null);

			OrderCollection	ordersToTest = new OrderCollection();
			ordersToTest.GetMultiManyToOne(null, null, null);

			Assert.AreEqual(orders.Count, ordersToTest.Count);
			Assert.IsTrue(orders.Count>0);
		}


		[Test]
		public void PrefetchPathTestManyToManyFilter2()
		{
			// redundant range filter, to make sure the m:n prefetch path will produce a combined OR and AND query which should have the
			// OR statements (from the parameterized prefetch path) inside a predicate expression.
			CustomerCollection customers = new CustomerCollection();
			IPrefetchPath prefetchPath = new PrefetchPath( (int)EntityType.CustomerEntity );
			prefetchPath.Add( CustomerEntity.PrefetchPathEmployees, 0, new PredicateExpression( EmployeeFields.EmployeeId == 5 ) );
			customers.GetMulti( (CustomerFields.CustomerId == new string[] { "CHOPS", "VINET", "BLONP", "HANAR", "VICTE" }), prefetchPath );
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


		/// <summary>
		/// Tests the prefetch path feature with a filter on the related entities and a subpath.
		/// </summary>
		[Test]
		public void PrefetchPathTestFilterSubpath()
		{
			CustomerCollection customers = new CustomerCollection();
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.CustomerEntity);
			prefetchPath.Add(CustomerEntity.PrefetchPathOrders, 1, new PredicateExpression(
					new FieldCompareValuePredicate(OrderFields.EmployeeId, ComparisonOperator.Equal, 1))).SubPath.Add(
					OrderEntity.PrefetchPathOrderDetails);
			customers.GetMulti(null, prefetchPath);
			for (int i = 0; i < customers.Count; i++)
			{
				// get the alreadyFetchedOrdersFlag:
				bool alreadyFetchedOrders = (bool)customers[i].GetType().BaseType.InvokeMember("_alreadyFetchedOrders", 
					BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance, null, customers[i], new object [] {});
				
				Assert.IsTrue(alreadyFetchedOrders);

				Assert.IsTrue(customers[i].Orders.Count<2);
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality with a relation using filter on the root entities and a double path from the root entity.
		/// </summary>
		[Test]
		public void PrefetchPathTestMultiLevelMultiEntityFilter()
		{
			OrderCollection orders = new OrderCollection();
			IRelationCollection	relations = new RelationCollection();
			relations.Add(OrderEntity.Relations.CustomerEntityUsingCustomerId);
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(CustomerFields.Country, ComparisonOperator.Equal, "France"));

			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.OrderEntity);
			prefetchPath.Add(OrderEntity.PrefetchPathCustomers);
			prefetchPath.Add(OrderEntity.PrefetchPathEmployees);

			orders.GetMulti(filter, 0, null, relations, prefetchPath);
			for (int i = 0; i < orders.Count; i++)
			{
				Assert.IsNotNull(orders[i].Customers);
				Assert.IsNotNull(orders[i].Employees);
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality with a sorter
		/// </summary>
		[Test]
		public void PrefetchPathTestMultiLevelSimpleWithSorting()
		{
			OrderCollection orders = new OrderCollection();
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(OrderFields.CustomerId, ComparisonOperator.Equal, "CHOPS"));
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.OrderEntity);
			SortExpression orderSorter = new SortExpression(new SortClause(OrderFields.OrderDate, SortOperator.Ascending));
			SortExpression orderDetailSorter = new SortExpression(new SortClause(OrderDetailsFields.ProductId, SortOperator.Ascending));

			prefetchPath.Add(OrderEntity.PrefetchPathOrderDetails, -1, null, null, orderDetailSorter);
			orders.GetMulti(filter, -1, orderSorter, null, prefetchPath);
			Assert.IsTrue((orders.Count>0));
			for (int i = 0; i < orders.Count; i++)
			{
				Assert.IsTrue( (orders[i].OrderDetails.Count>0));
			}
		}


		/// <summary>
		/// Tests the prefetch path functionality with a sorter
		/// </summary>
		[Test]
		public void PrefetchPathTestMultiLevelSimpleWithSortingOnRelatedTable()
		{
			OrderCollection orders = new OrderCollection();
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(OrderFields.CustomerId, ComparisonOperator.Equal, "CHOPS"));
			IPrefetchPath prefetchPath = new PrefetchPath((int)EntityType.OrderEntity);
			SortExpression orderSorter = new SortExpression(new SortClause(OrderFields.OrderDate, SortOperator.Ascending));
			SortExpression orderDetailSorter = new SortExpression(new SortClause(ProductFields.SupplierId, SortOperator.Ascending));
			RelationCollection relations = new RelationCollection();
			relations.Add(OrderDetailsEntity.Relations.ProductEntityUsingProductId, JoinHint.Left);

			prefetchPath.Add(OrderEntity.PrefetchPathOrderDetails, -1, null, relations, orderDetailSorter);
			orders.GetMulti(filter, -1, orderSorter, null, prefetchPath);
			Assert.IsTrue((orders.Count>0));
			for (int i = 0; i < orders.Count; i++)
			{
				Assert.IsTrue( (orders[i].OrderDetails.Count>0));
			}
		}


		/// <summary>
		/// Tests the join hint feature
		/// </summary>
		[Test]
		public void JoinHintTest()
		{
			ResultsetFields fields = new ResultsetFields(2);

			fields.DefineField(ProductFields.ProductId, 0, "ProductId");
			fields.DefineField(OrderDetailsFields.OrderId, 1, "OrderId");

			IRelationCollection relations = new RelationCollection();
			relations.Add(OrderDetailsEntity.Relations.ProductEntityUsingProductId, JoinHint.Right);
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareNullPredicate(OrderDetailsFields.ProductId));

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, filter, relations, true, null, null, 0, 0);

			// has to be reading 3 rows
			Assert.AreEqual(3, tlist.Rows.Count);

			relations = new RelationCollection();
			relations.Add(OrderDetailsEntity.Relations.ProductEntityUsingProductId, JoinHint.Inner);
			filter = new PredicateExpression();
			filter.Add(new FieldCompareNullPredicate(OrderDetailsFields.ProductId));
			tlist = new DataTable();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, filter, relations, true, null, null, 0, 0);
			// has to be reading 0 rows
			Assert.AreEqual(0, tlist.Rows.Count);

			relations = new RelationCollection();
			relations.Add(ProductEntity.Relations.OrderDetailsEntityUsingProductId, JoinHint.Left);
			filter = new PredicateExpression();
			filter.Add(new FieldCompareNullPredicate(OrderDetailsFields.ProductId));
			tlist = new DataTable();
			dao.GetMultiAsDataTable(fields, tlist, 0, null, filter, relations, true, null, null, 0, 0);
			// has to be reading 3 rows
			Assert.AreEqual(3, tlist.Rows.Count);
		}


		/// <summary>
		/// Tests the row limit with a multi-entity spanning join. 
		/// </summary>
		[Test]
		public void RowLimitTest()
		{
			EmployeeCollection employees = new EmployeeCollection(); 
			IRelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.OrderEntityUsingEmployeeId);
			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(OrderFields.ShipVia, ComparisonOperator.Equal, 2));
			employees.GetMulti(filter, 6, null, relations);

			Assert.AreEqual(6, employees.Count);

			CustomerCollection customers = new CustomerCollection();
			relations = new RelationCollection();
			relations.Add(CustomerEntity.Relations.OrderEntityUsingCustomerId);
			filter = new PredicateExpression();
			filter.Add(new FieldCompareValuePredicate(OrderFields.EmployeeId, ComparisonOperator.Equal, 2));
			customers.GetMulti(filter, 10, null, relations);

			Assert.AreEqual(10, customers.Count);
		}


		/// <summary>
		/// Tests if the DISTINCT keyword is still added to the filter.
		/// </summary>
		[Test]
		public void DistinctTypedListTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(CustomerFields.Country, 0, "Land", "C");
			fields.DefineField(OrderFields.EmployeeId, 1, "Employee", "O");
			IRelationCollection relations = new RelationCollection();
			fields[0].ExpressionToApply = new Expression(EntityFieldFactory.Create(CustomerFieldIndex.Country));
			((IEntityField)fields[0].ExpressionToApply.LeftOperand.Contents).ObjectAlias="C";
			relations.Add(CustomerEntity.Relations.OrderEntityUsingCustomerId, "C", "O", JoinHint.None);
			SortExpression sorter = new SortExpression(new SortClause(CustomerFields.Country, SortOperator.Ascending, "C"));
			sorter[0].FieldToSortCore.Alias="Land";
			sorter[0].FieldToSortCore.ExpressionToApply = fields[0].ExpressionToApply;

			DataTable tlist = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, tlist, 0, sorter, null, relations, false, null, null, 0, 0);
			Assert.AreEqual(179, tlist.Rows.Count);
		}
	}
}
