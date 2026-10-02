using System;
using System.Collections.Generic;
using System.Linq;
using NW26.Adapter;
using NW26.Adapter.DatabaseSpecific;
using NW26.Adapter.EntityClasses;
using NW26.Adapter.HelperClasses;
using NW26.Adapter.Linq;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.TestTypeConverters;
using SD.LLBLGen.Pro.LinqSupportClasses.ExpressionClasses;
using NUnit.Framework;
using NW26.Adapter.FactoryClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using System.Threading;

namespace LinqTester.AdapterTests
{
	/// <summary>
	/// Unit tests for entity oriented fetches, into entity collections or lists of entities.
	/// </summary>
	[TestFixture]
	public class EntityFetches
	{
		/// <summary>
		/// Simple test which executes a select directly on a table. 
		/// </summary>
		[Test]
		public void GetCustomersWithoutAnyFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer;
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var customers = adapter.FetchQuery(qf.Customer);
				Assert.AreEqual(91, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoPurchasedFromASupplierInFranceTest()
		{
			var customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget
									.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId)
									.InnerJoin(OrderEntity.Relations.OrderDetailEntityUsingOrderId)
									.InnerJoin(OrderDetailEntity.Relations.ProductEntityUsingProductId)
									.InnerJoin(ProductEntity.Relations.SupplierEntityUsingSupplierId))
							.Where(SupplierFields.Country == "France");

				adapter.FetchQuery(q, customers);
				Assert.AreEqual(69, customers.Count);
			}
		}



		[Test]
		public void GetCustomersWithProductFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        join o in metaData.Order on c.CustomerId equals o.CustomerId
				//        join od in metaData.OrderDetail on o.OrderId equals od.OrderId
				//        join p in metaData.Product on od.ProductId equals p.ProductId
				//        where p.ProductId == 1
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId)
									.InnerJoin(OrderEntity.Relations.OrderDetailEntityUsingOrderId)
									.InnerJoin(OrderDetailEntity.Relations.ProductEntityUsingProductId))
							.Where(ProductFields.ProductId == 1);

				Assert.AreEqual(31, adapter.FetchQuery(q).Count);
			}
		}

		[Test]
		public void GetCustomersWithProductFilter2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        join o in metaData.Order on c.CustomerId equals o.CustomerId
				//        join od in metaData.OrderDetail on o.OrderId equals od.OrderId
				//        join p in metaData.Product on od.ProductId equals p.ProductId
				//        where p.ProductId == 1
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.InnerJoin(qf.Order).On(CustomerFields.CustomerId==OrderFields.CustomerId)
									.InnerJoin(qf.OrderDetail).On(OrderFields.OrderId==OrderDetailFields.OrderId)
									.InnerJoin(qf.Product).On(OrderDetailFields.ProductId==ProductFields.ProductId))
							.Where(ProductFields.ProductId == 1);

				Assert.AreEqual(31, adapter.FetchQuery(q).Count);
			}
		}
		

		[Test]
		public void GetOrdersFiledAfterGivenDateUsingNullableFieldCompareWithConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.OrderDate > new DateTime(1998, 1, 1)
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order.Where(OrderFields.OrderDate > new DateTime(1998, 1, 1));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(260, results.Count);
			}
		}
		

		[Test]
		public void GetOrdersFiledAfterGivenDateUsingNullableFieldValueParameterCompareWithConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.OrderDate.Value > new DateTime(1998, 1, 1)
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order.Where(OrderFields.OrderDate > new DateTime(1998, 1, 1));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(260, results.Count);
			}
		}


		/// <summary>
		/// Simple test which executes Distinct() directly on a table. 
		/// </summary>
		[Test]
		public void GetDistinctCustomersWithoutAnyFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Distinct();

				// distinct is automatic
				var efq = new QueryFactory();
				var q = efq.Customer
								.From(QueryTarget.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(91, customers.Count);
			}
		}


		/// <summary>
		/// Gets the first customer from ordered list.
		/// </summary>
		[Test]
		public void GetFirstCustomerFromOrderedList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//        orderby c.Country ascending, c.City ascending
				//        select c).First();

				var qf = new QueryFactory();
				var q = qf.Customer
							.OrderBy(CustomerFields.Country.Ascending(), CustomerFields.City.Ascending())
							.Limit(1);
				var customer = adapter.FetchFirst(q);

				Assert.AreEqual("COMMI", customer.CustomerId);
			}
		}
		

		/// <summary>
		/// Gets the first customer from ordered list.
		/// </summary>
		[Test]
		public void GetFirstCustomerFromOrderedListWithFilterOnFirst()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         orderby c.Country ascending, c.City ascending
				//         select c).First(c=>c.Country=="USA");

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.Country=="USA")
							.OrderBy(CustomerFields.Country.Ascending(), CustomerFields.City.Ascending())
							.Limit(1);
				var customer = adapter.FetchFirst(q);

				Assert.AreEqual("RATTC", customer.CustomerId);
			}
		}


		/// <summary>
		/// Method will fail, because Single is called on a multi-element set.
		/// </summary>
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void GetSingleCustomerFromOrderedList_FailsOnPurpose()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         orderby c.Country ascending, c.City ascending
				//         select c).Single();

				var qf = new QueryFactory();
				var q = qf.Customer
							.OrderBy(CustomerFields.Country.Ascending(), CustomerFields.City.Ascending());
				var customer = adapter.FetchSingle(q);
			}
		}


		[Test]
		public void GetSingleCustomerFromListUsingSingleWithFilterOnCustomerId()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         select c).Single(c=>c.CustomerId=="CHOPS");

				var qf = new QueryFactory();
				var q = qf.Customer.Where(CustomerFields.CustomerId=="CHOPS");
				var customer = adapter.FetchSingle(q);

				Assert.AreEqual("CHOPS", customer.CustomerId);
			}
		}


		/// <summary>
		/// Gets a page of entities from a set without any filter.
		/// </summary>
		[Test]
		public void GetPageOfEntitiesFromSetWithoutFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.TakePage(2, 10);
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Page(2, 10);
				var results = adapter.FetchQuery(q);

				Assert.AreEqual(10, results.Count);
			}
		}


		[Test]
		public void GetPageOfEntitiesFromSetWithoutFilterUsingSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Skip(10).Take(10);
				//List<CustomerEntity> customers = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer.Page(2, 10);
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(10, results.Count);
			}
		}


		/// <summary>
		/// Gets a page of entities from a set with a filter.
		/// </summary>
		[Test]
		public void GetPageOfEntitiesFromSetWithFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (
				//            from c in metaData.Customer
				//            where c.Country == "Germany"
				//            select c
				//        ).TakePage(2, 3);
				//List<CustomerEntity> customers = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "Germany")
								.Page(2, 3);
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(3, customers.Count);
			}
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) with a single field filter
		/// </summary>
		[Test]
		public void GetEntitySetWithSingleFieldFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Country=="Germany"
				//        select c;
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer.Where(CustomerFields.Country == "Germany");
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(11, customers.Count);
			}
		}


		[Test]
		public void GetCustomerWithEquationOperatorOnEntireEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				CustomerEntity chopsEntity = new CustomerEntity("CHOPS");
				adapter.FetchEntity(chopsEntity);

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c == chopsEntity
				//        select c;
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId == chopsEntity.CustomerId);
				var  customers = adapter.FetchQuery(q, new EntityCollection<CustomerEntity>());

				Assert.AreEqual(1, customers.Count);
				Assert.AreEqual("CHOPS", customers[0].CustomerId);
			}
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) with a multi-predicate filter
		/// </summary>
		[Test]
		public void GetEntitySetWithMultiPredicateFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Country == "USA" && c.Region!="OR"
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where((CustomerFields.Country == "USA").And(CustomerFields.Region != "OR"));

				int count = 0;
				foreach(CustomerEntity c in adapter.FetchQuery(q))
				{
					count++;
					Assert.AreEqual("USA", c.Country);
					Assert.AreNotEqual("OR", c.Region);
				}
				Assert.AreEqual(9, count);
			}
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all customers in the USA).
		/// </summary>
		[Test]
		public void GetEntitySetWithMaxLimitAndFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//                        from c in metaData.Customer
				//                        where c.Country == "USA"
				//                        select c).Take(5);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.Country == "USA")
							.Limit(5);
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(5, customers.Count);
			}
		}


		/// <summary>
		/// Test which fetches customers based on a filter on order.EmployeeId. Simple test for joins
		/// </summary>
		[Test]
		public void GetEntitySetWithFilterOnRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//        from c in metaData.Customer
				//        join o in metaData.Order on c.CustomerId equals o.CustomerId
				//        where o.EmployeeId == 6
				//        select c);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();

				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget.InnerJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
								.Where(OrderFields.EmployeeId == 6);

				// alternatively
				//var q = qf.Customer
				//                .From(Joins.Inner(CustomerEntity.Relations.OrderEntityUsingCustomerId))
				//                .Where(OrderFields.EmployeeId == 6);

				// or
				//var q = qf.Customer
				//                .From(QueryTarget.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
				//                .Where(OrderFields.EmployeeId == 6);

				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(55, customers.Count);
			}
		}


		/// <summary>
		/// Test which fetches Employees based on a filter on a join with self and a filter on the joined related entity.
		/// </summary>
		[Test]
		public void GetEntitySetWithFilterOnRelatedEntityWithJoinToSelf()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//        from e in metaData.Employee
				//        join m in metaData.Employee on e.ReportsTo equals m.EmployeeId
				//        where m.FirstName == "Andrew"
				//        select e);
				//EntityCollection<EmployeeEntity> employees = (EntityCollection<EmployeeEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Employee
							.From(QueryTarget.InnerJoin(qf.Employee.As("m")).On(EmployeeFields.ReportsTo == EmployeeFields.EmployeeId.Source("m")))
							.Where(EmployeeFields.FirstName.Source("m") == "Andrew");

				// alternatively
				//var q = qf.Employee
				//    .From(QueryTarget.InnerJoin(EmployeeEntity.Relations.EmployeeEntityUsingEmployeeIdReportsTo, string.Empty, "m"))
				//    .Where(EmployeeFields.FirstName.Source("m") == "Andrew");

				var employees = adapter.FetchQuery(q);

				Assert.AreEqual(3, employees.Count);
			}
		}


		/// <summary>
		/// Gets the customers filtered on derived table build from related set from orders, using a join between customers and the derived table
		/// </summary>
		[Test]
		public void GetCustomersFilteredOnDerivedTableBuildFromRelatedSetUsingJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//            from c in metaData.Customer
				//            join x in
				//                (from o in metaData.Order where o.EmployeeId > 3 select new { CustomerId = o.CustomerId, EmployeeId = o.EmployeeId })
				//                    on c.CustomerId equals x.CustomerId
				//            where x.EmployeeId == 6
				//            select c);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget
									.InnerJoin(qf.Create("x")
													.Select(OrderFields.CustomerId, OrderFields.EmployeeId)
													.Where(OrderFields.EmployeeId > 3))
										.On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("x")))
							.Where(OrderFields.EmployeeId.Source("x") == 6);

				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(55, customers.Count);
			}
		}


		/// <summary>
		/// Gets the customers filtered on derived table build from related set from orders, using a join between customers and the derived table
		/// </summary>
		[Test]
		public void GetCustomersFilteredOnDerivedTableBuildFromRelatedSetUsingJoin2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//            from c in metaData.Customer
				//            join x in
				//                (from o in metaData.Order where o.EmployeeId > 3 select new { CustomerId = o.CustomerId, EmployeeId = o.EmployeeId })
				//                    on c.CustomerId equals x.CustomerId
				//            where x.EmployeeId == 6
				//            select c);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget
									.InnerJoin(qf.Create("x")
													.Select(OrderFields.CustomerId.As("f1"), OrderFields.EmployeeId.As("f2"))
													.Where(OrderFields.EmployeeId > 3))
										.On(CustomerFields.CustomerId == qf.Field("f1").Source("x")))
							.Where(qf.Field("f2").Source("x") == 6);
				
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(55, customers.Count);
			}
		}



		/// <summary>
		/// Gets orders with a filter on self and with a cross join + normal join.
		/// </summary>
		[Test]
		public void GetOrdersWithCrossJoinWithRelatedEntitiesAndFilterOnSelf()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//            from o in metaData.Order
				//            from p in metaData.Product
				//            join od in metaData.OrderDetail on
				//                new { o.OrderId, p.ProductId } equals new { od.OrderId, od.ProductId }
				//            where o.EmployeeId == 2
				//            select o);
				//EntityCollection<OrderEntity> orders = (EntityCollection<OrderEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Order
							   .From(QueryTarget
									.CrossJoin(qf.Product)
									.InnerJoin(qf.OrderDetail)
										.On((OrderDetailFields.ProductId == ProductFields.ProductId).And(OrderDetailFields.OrderId == OrderFields.OrderId)))
								.Where(OrderFields.EmployeeId == 2);
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(110, orders.Count);
			}
		}


		/// <summary>
		/// Test which returns customers who have bought product 2 by using just cross joins.
		/// </summary>
		[Test]
		public void GetCustomersWithFilterOnBoughtProductUsingSolelyCrossJoins()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//        from c in metaData.Customer
				//        from o in metaData.Order
				//        from od in metaData.OrderDetail
				//        where od.ProductId == 2
				//        select c);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget
									.CrossJoin(qf.Order)
									.CrossJoin(qf.OrderDetail))
							.Where(OrderDetailFields.ProductId == 2);
				var customers = adapter.FetchQuery(q);
				// fetches all 91 customers, though the test is to see if there's valid SQL generated
				Assert.AreEqual(91, customers.Count);
			}
		}


		/// <summary>
		/// Gets the customers from germany who have an order filed by employee 2 where the customer where is in an unexpected place.
		/// </summary>
		[Test]
		public void GetCustomersFromGermanyWithOrderFilteredOnEmployeeUsingMisplacedWhereAndCrossJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//        from c in metaData.Customer
				//        where c.Country == "Germany"
				//        from o in metaData.Order
				//        where o.EmployeeId == 2
				//        select c);
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "Germany")
								.From(QueryTarget.CrossJoin(qf.Order
																  .Where(OrderFields.EmployeeId == 2)
																  .As("o")));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(11, customers.Count);
			}
		}


		/// <summary>
		/// Test which uses DefaultIfEmpty and 'into' (so GroupJoin + SelectMany + DefaultIfEmpty) to fetch all customers which have no orders from France
		/// </summary>
		[Test]
		public void LeftJoinUsingDefaultIfEmptyToFetchCustomersWithoutAnOrderFromFrance()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//				  join o in metaData.Order on c.CustomerId equals o.CustomerId into oc
				//				  from x in oc.DefaultIfEmpty()
				//				  where x.OrderId == null
				//			      && c.Country=="France"
				//				  select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget
										.LeftJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
								.Where(OrderFields.OrderId.IsNull().And(CustomerFields.Country == "France"));
				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(1, counter);
			}
		}


		/// <summary>
		/// Test which uses DefaultIfEmpty and 'into' (so GroupJoin + SelectMany + DefaultIfEmpty) to fetch all customers which have no orders
		/// </summary>
		[Test]
		public void LeftJoinUsingDefaultIfEmptyToFetchCustomersWithoutAnOrder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//                join o in metaData.Order on c.CustomerId equals o.CustomerId into oc
				//                from x in oc.DefaultIfEmpty()
				//                where x.OrderId == null
				//                select c;
				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget
										.LeftJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
								.Where(OrderFields.OrderId == DBNull.Value);
				var customers = adapter.FetchQuery(q);


				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(2, counter);
			}
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//			   where o.Customer.Country == "Germany"
				//			   select o;

				// QuerySpec is more closer to SQL so joins have to be expressed explicitly
				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Where(CustomerFields.Country == "Germany");
				var orders = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in orders)
				{
					counter++;
				}
				Assert.AreEqual(121, counter);
			}
		}


		/// <summary>
		/// Test which fetches all order details of orders of all customers from spain using the 'Customer' property in order to use an implicit inner join. 
		/// this test is an extention of GetAllOrdersOfCustomersInGermanyWithFilterOnRelatedEntityUsingFieldOnRelationProperty's query. The main painpoint with
		/// this query is that it has multiple levels of related entities. 
		/// </summary>
		[Test]
		public void GetAllOrderDetailsOfOrdersOfCustomersInSpainWithFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orderDetails = from od in metaData.OrderDetail
				//                   where od.Order.Customer.Country == "Spain"
				//                   select od;

				// QuerySpec is more closer to SQL so joins have to be expressed explicitly
				var qf = new QueryFactory();
				var q = qf.OrderDetail
							.From(QueryTarget.InnerJoin(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
										.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Where(CustomerFields.Country == "Spain");

				var orderDetails = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in orderDetails)
				{
					counter++;
				}
				Assert.AreEqual(51, counter);
			}
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// It has a duplicate filter so the relation is added twice. The test checks if the code is able to backtrack to the proper aliases.
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithDuplicateFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//             where o.Customer.Country == "Germany"
				//             && o.Customer.ContactTitle == "Sales Representative"
				//             select o;
				// QuerySpec is more closer to SQL so joins have to be expressed explicitly
				var qf = new QueryFactory();
				var q = qf.Order
							  .From(QueryTarget.As("o").InnerJoin(qf.Customer)
											.On(OrderFields.CustomerId.Source("o") == CustomerFields.CustomerId))
							  .Where((CustomerFields.Country == "Germany").And(CustomerFields.ContactTitle == "Sales Representative"));

				var orders = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in orders)
				{
					counter++;
				}
				Assert.AreEqual(38, counter);
			}
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// It has a duplicate filter so the relation is added twice. The test checks if the code is able to backtrack to the proper aliases.
		/// Variant uses different aliasing for target
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithDuplicateFilterOnRelatedEntityUsingFieldOnRelationProperty2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//             where o.Customer.Country == "Germany"
				//             && o.Customer.ContactTitle == "Sales Representative"
				//             select o;
				// QuerySpec is more closer to SQL so joins have to be expressed explicitly
				var qf = new QueryFactory();
				var q = qf.Order
							  .TargetAs("o")
							  .From(QueryTarget.InnerJoin(qf.Customer)
											.On(OrderFields.CustomerId.Source("o") == CustomerFields.CustomerId))
							  .Where((CustomerFields.Country == "Germany").And(CustomerFields.ContactTitle == "Sales Representative"));

				var orders = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in orders)
				{
					counter++;
				}
				Assert.AreEqual(38, counter);
			}
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// It has a duplicate filter so the relation is added twice. The test checks if the code is able to backtrack to the proper aliases.
		/// Variant uses different aliasing for target
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithDuplicateFilterOnRelatedEntityUsingFieldOnRelationProperty3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//             where o.Customer.Country == "Germany"
				//             && o.Customer.ContactTitle == "Sales Representative"
				//             select o;
				// QuerySpec is more closer to SQL so joins have to be expressed explicitly
				var qf = new QueryFactory();
				var q = qf.Order
							  .TargetAs("o")
							  .From(QueryTarget.InnerJoin(qf.Customer)
											.On(OrderFields.CustomerId.Source("o") == CustomerFields.CustomerId))
							  .Where((CustomerFields.Country == "Germany").And(CustomerFields.ContactTitle == "Sales Representative"));

				var orders = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in orders)
				{
					counter++;
				}
				Assert.AreEqual(38, counter);
			}
		}


		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 10 orders.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan10Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Count() > 10
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.Where(CustomerFields.CustomerId == OrderFields.CustomerId)
											.CountRow().GreaterThan(10));
				// alternative
				//var q = qf.Customer
				//                .Where(qf.Order.As("o")
				//                            .Where(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o"))
				//                            .Count().GreaterThan(10));
				var customers = adapter.FetchQuery(q);
				
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(27, counter);
			}
		}


		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 10 orders.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan10Orders2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Count() > 10
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.TargetAs("c")
								.Where(qf.Order
											.Where(CustomerFields.CustomerId.Source("c") == OrderFields.CustomerId)
											.CountRow().GreaterThan(10));
				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(27, counter);
			}
		}


		[Test]
		public void GetAllCustomersWhichHaveMoreThan10Orders3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Count() > 10
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.CountRow().GreaterThan(10));
				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(27, counter);
			}
		}



		[Test]
		public void AliasOnJoinedQueryTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget.InnerJoin(qf.Order
																.Where(OrderFields.EmployeeId>2)
																.As("o"))
													.On(CustomerFields.CustomerId == qf.Field("CustomerId").Source("o")));
				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(89, counter);
			}
		}



		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees with id bigger than 6. 
		/// This is done by applying a count on the Orders set with a filter in the Count as argument.
		/// Primary purpose of the test is to see if the code can deal with a filter in a count aggregate.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesWithAnEmployeeIdBiggerThan6()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//                where c.Orders.Count(o => o.EmployeeId > 6) > 6
				//                select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.Where((CustomerFields.CustomerId == OrderFields.CustomerId)
													.And(OrderFields.EmployeeId > 6))
											.CountRow().GreaterThan(6));
				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(3, counter);
			}
		}


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK 
		/// This is done by applying a count on the Orders set with a filter in the Count as argument.
		/// Primary purpose of the test is to see if the code can deal with a filter in a count aggregate.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesFromTheUK()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//                where c.Orders.Count(o => o.Employee.Country == "UK") > 6
				//                select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(qf.Order
										.From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
										.Where((CustomerFields.CustomerId == OrderFields.CustomerId)
												.And(EmployeeFields.Country == "UK"))
										.CountRow().GreaterThan(6));

				// alternative
				//var q = qf.Customer
				//            .Where(qf.Order
				//                        .From(QueryTarget.InnerJoin(qf.Employee).On(OrderFields.EmployeeId==EmployeeFields.EmployeeId))
				//                        .Where((CustomerFields.CustomerId == OrderFields.CustomerId)
				//                                .And(EmployeeFields.Country == "UK"))
				//                        .Count().GreaterThan(6));

				// alternative 2, forcing a row limit
				//var q = qf.Customer
				//            .Where(qf.Order
				//                        .From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
				//                        .Where((CustomerFields.CustomerId == OrderFields.CustomerId)
				//                                .And(EmployeeFields.Country == "UK"))
				//                        .CountRow().ForceRowLimit().GreaterThan(6));
				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(4, counter);
			}
		}


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK. 
		/// The difference here is that it uses a Where instead of a filter in the Count.
		/// This is done by applying a count on the Orders 
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesFromTheUKWithCountOnWhereResult()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Where(o => o.Employee.Country == "UK").Count() > 6
				//        select c;

				// same as previous test as QuerySpec is closer to SQL.
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
											.Where(EmployeeFields.Country == "UK")
											.CountRow().GreaterThan(6));

				// Alternative
				//var q = qf.Customer
				//            .Where(qf.Order
				//                        .From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
				//                        .Where((CustomerFields.CustomerId == OrderFields.CustomerId)
				//                                .And(EmployeeFields.Country == "UK"))
				//                        .Count().GreaterThan(6));

				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(4, counter);
			}
		}


		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesFromTheUKWithCountOnWhereResult2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Where(o => o.Employee.Country == "UK").Count() > 6
				//        select c;

				// same as previous test as QuerySpec is closer to SQL.
				var qf = new QueryFactory();
				var q = qf.Customer
								.TargetAs("c")
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId, string.Empty, "c")
											.From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
											.Where(EmployeeFields.Country == "UK")
											.CountRow().GreaterThan(6));

				var customers = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(4, counter);
			}
		}



		/// <summary>
		/// Gets all customers which have worked with more than4 employees for order filing. This is a M:N test with aggregates.
		/// </summary>
		/// <remarks>Bogus test, as it implies a join with 1:n related entities over which the count is executed.</remarks>
		[Test]
		public void GetAllCustomersWhichHaveWorkedWithMoreThan4EmployeesForOrderFiling()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//                where c.EmployeeCollectionViaOrder.Count() > 6
				//                select c;

				// QuerySpec is closer to SQL, so no m:n wrappers. 
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Employee
											.From(QueryTarget.InnerJoin(EmployeeEntity.Relations.OrderEntityUsingEmployeeId))
											.Where(CustomerFields.CustomerId == OrderFields.CustomerId)
											.CountRow().GreaterThan(6));

				// Alternative
				//var q = qf.Customer
				//                .Where(qf.Employee
				//                            .From(QueryTarget.InnerJoin(EmployeeEntity.Relations.OrderEntityUsingEmployeeId))
				//                            .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
				//                            .Count().GreaterThan(6));

				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(54, counter);
			}
		}


		/// <summary>
		/// Tests propagation of correlationrelation 
		/// </summary>
		[Test]
		public void GetOrderDetailsUsingMultiHopAggregate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orderDetails = from od in metaData.OrderDetail
				//					 where od.Order.Employee.EmployeeTerritories.Where(et=>et.TerritoryId.Contains("0")).Count() > 3
				//					 select od;

				var qf = new QueryFactory();
				var q = qf.OrderDetail
								.Where(qf.EmployeeTerritory
												.From(QueryTarget
														.InnerJoin(EmployeeTerritoryEntity.Relations.EmployeeEntityUsingEmployeeId)
														.InnerJoin(EmployeeEntity.Relations.OrderEntityUsingEmployeeId))
												.CorrelatedOver(OrderEntity.Relations.OrderDetailEntityUsingOrderId)
												.Where(EmployeeTerritoryFields.TerritoryId.Contains("0"))
												.CountRow().GreaterThan(3));

				// Alternative:
				//var q = qf.OrderDetail
				//                .Where(qf.EmployeeTerritory
				//                            .From(QueryTarget
				//                                    .InnerJoin(qf.Employee).On(EmployeeTerritoryFields.EmployeeId == EmployeeFields.EmployeeId)
				//                                    .InnerJoin(qf.Order).On(EmployeeFields.EmployeeId == OrderFields.EmployeeId))
				//                            .Where(EmployeeTerritoryFields.TerritoryId.Contains("0")
				//                                        .And(OrderFields.OrderId == OrderDetailFields.OrderId))
				//                            .Count().GreaterThan(3));

				var orderDetails = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in orderDetails)
				{
					count++;
				}
				Assert.AreEqual(851, count);
			}
		}
		

		/// <summary>
		/// Gets all categories which have more than 500 units overall in stock. This is a test which checks if Sum() aggregates over single fields work.
		/// </summary>
		[Test]
		public void GetAllCategoriesWhichHaveMoreThan500UnitsOverallInStock()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Category
				//        where c.Products.Sum(p=>p.UnitsInStock) > 500
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Category
								.Where(qf.Product
											.CorrelatedOver(ProductEntity.Relations.CategoryEntityUsingCategoryId)
											.Sum(ProductFields.UnitsInStock).GreaterThan(500));
				var categories = adapter.FetchQuery(q);
				
				int counter = 0;
				foreach(var v in categories)
				{
					counter++;
				}
				Assert.AreEqual(2, counter);
			}
		}


		/// <summary>
		/// Test which gets all customers with an order total higher than 5000. This test is to show that nested aggregates work.
		/// </summary>
		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan5000()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = from c in metaData.Customer
				//                where c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)) > 5000
				//                select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											 .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											 .Select(qf.OrderDetail
															.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
															.Select((OrderDetailFields.Quantity * OrderDetailFields.UnitPrice).As("MV"))
															.Sum().As("S1Sum"))
											 .Sum()
										.GreaterThan(5000));

				// alternative
				//var q = qf.Customer
				//                .Where(qf.Order
				//                             .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
				//                             .Select(qf.OrderDetail
				//                                            .CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
				//                                            .Select((OrderDetailFields.Quantity * OrderDetailFields.UnitPrice).As("MV"))
				//                                            .Sum(qf.Field("MV"))
				//                                     .As("S1Sum"))
				//                             .Sum(qf.Field("S1Sum"))
				//                        .GreaterThan(5000));


				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(54, counter);
			}
		}

		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan1000ForOrdersWithProductsWithAnIdBiggerThan40()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Sum(o => o.OrderDetails.Where(od => od.ProductId > 40).Sum(od => od.Quantity * od.UnitPrice)) > 1000
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											 .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											 .Select(qf.OrderDetail
															.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
															.Where(OrderDetailFields.ProductId > 40)
															.Select((OrderDetailFields.Quantity * OrderDetailFields.UnitPrice).As("MV"))
															.Sum().As("S1Sum"))
											 .Sum()
										.GreaterThan(1000));

				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(68, counter);
			}
		}



		/// <summary>
		/// Gets all categories which have more than500 units overall in stock sorted by that sum ascending and then by category name descending
		/// </summary>
		[Test]
		public void GetAllCategoriesWhichHaveMoreThan500UnitsOverallInStockSortedBySumDescendingThenCategoryNameAscending()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//            from c in metaData.Category
				//            let x = c.Products.Select(p => (int)p.UnitsInStock).Sum()
				//            where x > 500
				//            orderby x ascending, c.CategoryName descending
				//            select c);
				//EntityCollection<CategoryEntity> categories = (EntityCollection<CategoryEntity>)q.Execute();

				var qf = new QueryFactory();
				var q = qf.Category
								.From(qf.Category
											.TargetAs("C")
											.Select(Projection.Full, qf.Product
																			.CorrelatedOver(ProductEntity.Relations.CategoryEntityUsingCategoryId, string.Empty, "C")
																			.Select(ProductFields.UnitsInStock)
																			.Sum().As("x"))
											.As("C2"))	// specifies the alias on the derived table with category + the sum field 'x'
								.TargetAs("C2")	// specifies the target of the category fetch to be the derived table 'C2' 
								.Where(qf.Field("x").Source("C2") > 500)
								.OrderBy(qf.Field("x").Source("C2").Ascending(), CategoryFields.CategoryName.Source("C2").Ascending());

				var categories = adapter.FetchQuery(q, new EntityCollection<CategoryEntity>());
				Assert.AreEqual(2, categories.Count);
				Assert.AreEqual(2, categories[0].CategoryId);
				Assert.AreEqual(8, categories[1].CategoryId);
			}
		}


		/// <summary>
		/// Gets all orders from customers from UK sorted by customer.city descending.
		/// </summary>
		[Test]
		public void GetAllOrdersFromCustomersFromUKSortedByCityDescending()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(
				//            from o in metaData.Order
				//            where o.Customer.Country == "UK"
				//            orderby o.Customer.City descending
				//            select o
				//            );
				//EntityCollection<OrderEntity> orders = (EntityCollection<OrderEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Order
							 .From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							 .Where(CustomerFields.Country == "UK")
							 .OrderBy(CustomerFields.City.Descending());
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(56, orders.Count);
				Assert.AreEqual("SEVES", ((OrderEntity)orders[0]).CustomerId);
			}
		}


		[Test]
		public void GetAllOrdersFromCustomersFromGermanyUsingJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        join c in metaData.Customer on o.CustomerId equals c.CustomerId
				//        where o.Customer.Country == "Germany"
				//        select o;
				var qf = new QueryFactory();
				var q = qf.Order
							 .From(QueryTarget.InnerJoin(qf.Customer).On(OrderFields.CustomerId == CustomerFields.CustomerId))
							 .Where(CustomerFields.Country == "Germany");
				var orders = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in orders)
				{
					count++;
				}
				Assert.AreEqual(121, count);
			}
		}


		/// <summary>
		/// Gets the customers ordered by country then by city using solely extension methods.
		/// </summary>
		[Test]
		public void GetCustomersOrderedByCountryThenByCityUsingSolelyExtensionMethods()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//ILLBLGenProQuery q = (ILLBLGenProQuery)(metaData.Customer.OrderBy(c=>c.Country).ThenByDescending(c=>c.City));
				//EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				var qf = new QueryFactory();
				var q = qf.Customer.OrderBy(CustomerFields.Country.Ascending(), CustomerFields.City.Descending());
				var customers = adapter.FetchQuery(q);

				Assert.AreEqual(91, customers.Count);
				Assert.AreEqual("COMMI", ((CustomerEntity)customers[0]).CustomerId);
			}
		}


		/// <summary>
		/// Gets all customers not from germany using a not filter. This tests the ability to use 'not' before a binary expression.
		/// </summary>
		[Test]
		public void GetAllCustomersNotFromGermanyUsingANotFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where !(c.Country=="Germany")
				//        select c;
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(Functions.Not(CustomerFields.Country == "Germany"));
				var customers = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(79, counter);
			}
		}


		[Test]
		public void GetAllCustomersFromGermanyUsingAFilter()
		{
			var customers = new EntityCollection<CustomerEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where (c.Country=="Germany")
				//        select c;
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "Germany");
				adapter.FetchQuery(q, customers);
				int counter = 0;
				foreach(var v in customers)
				{
					counter++;
				}
				Assert.AreEqual(11, counter);
			}
		}


		[Test]
		public void GetAllRelatedOrdersOfEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var myCustomer = adapter.FetchNewEntity<CustomerEntity>(new RelationPredicateBucket(CustomerFields.CustomerId == "CHOPS"));
				Assert.IsNotNull(myCustomer);
				Assert.IsFalse(myCustomer.IsNew);
				var qf = new QueryFactory();
				var q = qf.Order
							.Where(qf.Customer.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
									.Contains(myCustomer));
				adapter.FetchQuery(q, myCustomer.Orders);
				int counter = 0;
				foreach(var v in myCustomer.Orders)
				{
					counter++;
				}
				Assert.AreEqual(8, counter);
			}
		}


		[Test]
		public void GetAllRelatedOrdersOfEntity2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var myCustomer = adapter.FetchNewEntity<CustomerEntity>(new RelationPredicateBucket(CustomerFields.CustomerId == "CHOPS"));
				Assert.IsNotNull(myCustomer);
				Assert.IsFalse(myCustomer.IsNew);
				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Where(CustomerFields.CustomerId==myCustomer.CustomerId);
				adapter.FetchQuery(q, myCustomer.Orders);
				int counter = 0;
				foreach(var v in myCustomer.Orders)
				{
					counter++;
				}
				Assert.AreEqual(8, counter);
			}
		}



		/// <summary>
		/// Gets the regions which have only employees which have titleofcourtesy set to 'Ms.'. This is a test for the All() extension method.
		/// </summary>
		[Test]
		public void GetAllRegionsWhichHaveOnlyMsEmployees()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from r in metaData.Region
				//        where r.Employees.All(e => e.TitleOfCourtesy == "Ms.")
				//        select r;
				var qf = new QueryFactory();
				var q = qf.Region
							  .Where(qf.Employee
											.CorrelatedOver(EmployeeEntity.Relations.RegionEntityUsingRegionId)
											.All(EmployeeFields.TitleOfCourtesy == "Ms."));

				// alternative
				//var q = qf.Region
				//              .Where(Functions.NotExists
				//                            (qf.Employee
				//                                    .CorrelatedOver(EmployeeEntity.Relations.RegionEntityUsingRegionId)
				//                                    .Where(EmployeeFields.TitleOfCourtesy != "Ms.")));

				// alternative
				//var q = qf.Region
				//                .WhereNotExists(qf.Employee
				//                                        .CorrelatedOver(EmployeeEntity.Relations.RegionEntityUsingRegionId)
				//                                        .Where(EmployeeFields.TitleOfCourtesy != "Ms."));


				var regions = adapter.FetchQuery(q);

				int counter = 0;
				foreach(var v in regions)
				{
					counter++;
				}
				Assert.AreEqual(2, counter);
			}
		}


		/// <summary>
		/// Gets all orders which have purchased product with id 2. This is a test for the Any() extension method.
		/// </summary>
		[Test]
		public void GetAllOrdersWhichHavePurchasedProduct2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.OrderDetails.Any(od=>od.ProductId==2)
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order
							 .Where(qf.OrderDetail
											.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
											.Any(OrderDetailFields.ProductId == 2));

				// Alternative
				//var q = qf.Order
				//             .WhereExists(qf.OrderDetail
				//                                .CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
				//                                .Where(OrderDetailFields.ProductId == 2));

				var orders = adapter.FetchQuery(q);
				int counter = 0;
				foreach(var v in orders)
				{
					counter++;
				}
				Assert.AreEqual(43, counter);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection. This is a simple test with an entity as argument.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimple()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Contains(order)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				OrderEntity order = new OrderEntity();
				// set pk field. It's an identity field, so use forced write
				order.Fields[(int)OrderFieldIndex.OrderId].ForcedCurrentValueWrite(10254);

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Contains(order));
				var results = adapter.FetchQuery(q);

				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimple_2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				OrderEntity order = new OrderEntity();
				// set pk field. It's an identity field, so use forced write
				order.Fields[(int)OrderFieldIndex.OrderId].ForcedCurrentValueWrite(10254);

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order.TargetAs("o")
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId, "o", string.Empty)
											.Where(OrderFields.EmployeeId.Source("o") > 2)
											.Contains(order));
				var results = adapter.FetchQuery(q);

				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", ((CustomerEntity)results[0]).CustomerId);
			}
		}
		

		/// <summary>
		/// Gets the customer based on contains call on orders collection simple with where on source. Another rather simple Contains test on a collection
		/// with a filter, so the source isn't an entity but a query.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimpleWithWhereOnSource()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				OrderEntity order = new OrderEntity();
				// set pk field. It's an identity field, so use forced write
				order.Fields[(int)OrderFieldIndex.OrderId].ForcedCurrentValueWrite(10254);

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Where(o=>o.EmployeeId==3).Contains(order)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Where(OrderFields.EmployeeId == 3)
											.Contains(order));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection with query as operand. This is a tricky situation, where the operand of the 
		/// Contains is a query result. The result has to be obtained during the main query, not separately.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionWithQueryAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Contains(qf.Order
															.Where(OrderFields.EmployeeId==2)
															.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("BLONP", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection with query as operand and query on source.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionWithQueryAsOperandAndQueryOnSource()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				OrderEntity order = new OrderEntity();
				// set pk field. It's an identity field, so use forced write
				order.Fields[(int)OrderFieldIndex.OrderId].ForcedCurrentValueWrite(10254);
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Where(o => o.EmployeeId == 2).Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.Where(OrderFields.EmployeeId==2)
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Contains(qf.Order
															.Where(OrderFields.EmployeeId == 2)
															.Limit(1)));

				var results = adapter.FetchQuery(q);
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("BLONP", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		/// <summary>
		/// Simple query which tests the ability of the Contains handler to handle projections on an entity set as source and a constant value as operand.
		/// The query obtains all customers which have orders handled by an employee with id > 3 and which is shipped via shipper 2
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnOrdersCollectionWithSingleFieldProjectionAndConstantOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Where(o => o.EmployeeId > 3).Select(o => o.ShipVia).Contains(2)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Where(OrderFields.EmployeeId > 3)
											.Select(OrderFields.ShipVia)
											.Contains(2));

				// alternative
				//var q = qf.Customer
				//                .Where(qf.Order
				//                            .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
				//                            .Where(OrderFields.EmployeeId > 3)
				//                            .Any(OrderFields.ShipVia == 2));
				
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(75, results.Count);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on multi value projection with multi value object as operand.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnMultiValueProjectionWithMultiValueObjectAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
				//            new { EID = (int?)1, CID = "CHOPS" })
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select(OrderFields.EmployeeId.As("EID"), OrderFields.CustomerId.As("CID"))
											.Contains(new { EID = 1, CID = "CHOPS" }));

				var results = adapter.FetchQuery(q);
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on single value projection with single value query as operand.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnSingleValueProjectionWithSingleValueQueryAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Select(oc => oc.EmployeeId).Contains((from o in metaData.Order where o.CustomerId == "CHOPS" select o.EmployeeId).First())
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select(OrderFields.EmployeeId)
											.Contains(qf.Order
															.Where(OrderFields.CustomerId == "CHOPS")
															.Select(OrderFields.EmployeeId)
															.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(64, results.Count);
			}
		}


		[Test]
		public void GetCustomersBasedOnContainsCallOnSingleValueProjectionWithSingleValueQueryAsOperandWithExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Customer.TargetAs("C")
								.Where(qf.Order.TargetAs("Foo")
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId, "Foo", "C")
											.Select((OrderFields.EmployeeId.Source("Foo") * 2).As("x"))
											.Contains(qf.Order
															.Where(OrderFields.CustomerId == "CHOPS")
															.Select((OrderFields.EmployeeId * 2).As("x"))
															.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(64, results.Count);
			}
		}


		[Test]
		public void GetCustomersBasedOnContainsCallOnSingleValueProjectionWithSingleValueQueryAsOperandWithExpression2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Customer.TargetAs("C")
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId, string.Empty, "C")
											.Select((OrderFields.EmployeeId * 2).As("x"))
											.Contains(qf.Order
															.Where(OrderFields.CustomerId == "CHOPS")
															.Select((OrderFields.EmployeeId * 2).As("x"))
															.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(64, results.Count);
			}
		}


		[Test]
		public void GetCustomersBasedOnContainsCallOnSingleValueProjectionWithSingleValueQueryAsOperandWithExpression3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select((OrderFields.EmployeeId * 2).As("x"))
											.Contains(qf.Order
															.Where(OrderFields.CustomerId == "CHOPS")
															.Select((OrderFields.EmployeeId * 2).As("x"))
															.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(64, results.Count);
			}
		}


		/// <summary>
		/// Gets the customer based on contains call on multi value projection with multi value query as operand.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnMultiValueProjectionWithMultiValueQueryAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
				//            (from o in metaData.Order where o.CustomerId == "CHOPS" select new { EID = o.EmployeeId, CID = o.CustomerId }).First())
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
							  .Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select(OrderFields.EmployeeId.As("EID"), OrderFields.CustomerId.As("CID"))
											.Contains(qf.Order
														.Where(OrderFields.CustomerId == "CHOPS")
														.Select(OrderFields.EmployeeId.As("EID"), OrderFields.CustomerId.As("CID"))
														.Limit(1)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", ((CustomerEntity)results[0]).CustomerId);
			}
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized list.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				List<string> countries = new List<string>() { "USA", "UK" };
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where countries.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(countries));
				// alternative
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.In("USA", "UK"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(20, results.Count);
			}
		}


		/// <summary>
		/// Similar to GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedList, but now the list is created inside the query.
		/// This tests the ListInit expression materialization.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where new List<string>() { "USA", "UK" }.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(new List<string>() { "USA", "UK" }));
				// alternative
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.In("USA", "UK"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(20, results.Count);
			}
		}


		/// <summary>
		/// similar to GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedList but now also uses a CTor call with the initialization
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedListAndListCTor()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string[] values = new string[] { "UK" };
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where new List<string>(values) { "USA"}.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				// QuerySpec doesn't suffer from normal code merged into the query, so query looks the same as the others.
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(new List<string>(values) { "USA" }));
				// alternative
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.In("USA", "UK"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(20, results.Count);
			}
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized enumerable. A linked list is used as the enumerable.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedEnumerable()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinkedList<string> countries = new LinkedList<string>(new string[] { "USA", "UK" });
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where countries.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(countries));

				// alternative
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.In("USA", "UK"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(20, results.Count);
			}
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized array.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string[] countries = new string[] { "USA", "UK" };
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where countries.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(countries));

				// alternative
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.In("USA", "UK"));

				var results = adapter.FetchQuery(q);
				Assert.AreEqual(20, results.Count);
			}
		}


		/// <summary>
		/// Test which checks if the provider can combine a query inside another query.
		/// </summary>
		[Test]
		public void GetAllCustomersBasedOnCombinedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q2 = (from c in metaData.Customer
				//          select c.Country).Distinct();
				//var q = from c in metaData.Customer
				//        where q2.Contains(c.Country)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				// distinct is defined in the query, but as it's converted to a field compare set predicate, the distinct isn't necessary: an IN(query) predicate
				// doesn't need a distinct in the query so it's filtered out.
				var q2 = qf.Customer.Select(CustomerFields.Country).Distinct();
				var q = qf.Customer
								.Where(CustomerFields.Country.In(q2));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(90, results.Count);
			}
		}
		

		/// <summary>
		/// Uses two queries, first one to obtain the entities to compare against, then the actual query which filters on this set of entities.
		/// This version tests the single PK variant.
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfEntitiesSingleFieldPk()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q2 = from c in metaData.Customer where c.Country == "USA" select c;
				//List<CustomerEntity> customers = q2.ToList();
				//var q = from c in metaData.Customer
				//        where customers.Contains(c)
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q2 = qf.Customer.Where(CustomerFields.Country == "USA");
				var customers = adapter.FetchQuery(q2, new EntityCollection<CustomerEntity>());
				var q = qf.Customer.Intersect(customers);
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(13, results.Count);
			}
		}


		/// <summary>
		/// Uses two queries, first one to obtain the entities to compare against, then the actual query which filters on this set of entities.
		/// This version tests the compound PK variant.
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfEntitiesMultiFieldPk()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q2 = from od in metaData.OrderDetail where od.ProductId < 5 select od;
				//List<OrderDetailEntity> orderDetails = q2.ToList();
				//var q = from od in metaData.OrderDetail
				//        where orderDetails.Contains(od)
				//        select od;
				//List<OrderDetailEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q2 = qf.OrderDetail.Where(OrderDetailFields.ProductId < 5);
				var orderDetails = adapter.FetchQuery(q2, new EntityCollection<OrderDetailEntity>());
				var q = qf.OrderDetail.Intersect(orderDetails);
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(111, results.Count);
			}
		}


		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfValuesUsingQueryWithFieldAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//List<int?> employeeIds = new List<int?>() { 1, 2 };
				//var q = from c in metaData.Customer
				//        where employeeIds.Contains((from o in metaData.Order where o.CustomerId == c.CustomerId select o.EmployeeId).First())
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
								.WhereExists(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Where(OrderFields.EmployeeId.In(1, 2)));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(80, results.Count);
			}
		}


		/// <summary>
		/// Complicated test which uses a compare between a set of tuples in memory with a set of tuples obtained from a table in the database. Matching rows
		/// result in the retrieval of that customer. 
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfTuplesUsingQueryWithProjectionOnCustomClassAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// tuple list
				List<Pair<string, string>> countryCities = new List<Pair<string, string>>();
				countryCities.Add(new Pair<string, string>("USA", "Portland"));
				countryCities.Add(new Pair<string, string>("Brazil", "Sao Paulo"));

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//// now fetch all customers which have a tuple of country/city in the list of countryCities.
				//var q = from c in metaData.Customer
				//        where countryCities.Contains((from c2 in metaData.Customer
				//                                      where c2.CustomerId == c.CustomerId
				//                                      select new Pair<string, string>() { Value1 = c2.Country, Value2 = c2.City }).First())
				//        select c;
				// List<CustomerEntity> results = q.ToList();

				// not directly supported, so we've to append the predicates ourselves. 
				var qf = new QueryFactory();
				var q = qf.Customer;
				foreach(var tuple in countryCities)
				{
					q.OrWhere((CustomerFields.Country == tuple.Value1).And(CustomerFields.City == tuple.Value2));
				}
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(5, results.Count);
			}
		}


		/// <summary>
		/// Gets the customers based on contains call on string field. This results in a LIKE query.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnStringField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Country.Contains("land")
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country.Contains("land"));

				// alternative 
				//var q = qf.Customer
				//                .Where(CustomerFields.Country.Like("%land%"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(6, results.Count);
			}
		}


		/// <summary>
		/// Gets the orders based on contains call on string field of related entity. 
		/// </summary>
		[Test]
		public void GetOrdersBasedOnContainsCallOnStringFieldOfRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.Customer.Country.Contains("land")
				//        select o;
				//List<OrderEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(qf.Customer).On(OrderFields.CustomerId == CustomerFields.CustomerId))
							.Where(CustomerFields.Country.Like("%land%"));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(66, results.Count);
			}
		}


		/// <summary>
		/// Gets the orders based on contains call on string field producing query of related entity. Uses a query which produces a string as source for Contains.
		/// 'Contains' is here the string.Contains extension method. If 'First()' is removed from the query, 'Contains' is the IEnumerable.Contains() extension method.
		/// </summary>
		[Test]
		public void GetOrdersBasedOnContainsCallOnStringFieldProducingQueryOfRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where (from c in metaData.Customer where c.CustomerId==o.CustomerId select c.Country).First().Contains("land")
				//        select o;
				//List<OrderEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Order
								.WhereExists(qf.Customer
													.CorrelatedOver(CustomerEntity.Relations.OrderEntityUsingCustomerId)
													.Where(CustomerFields.Country.Contains("land")));

				// alternative
				//var q = qf.Order
				//            .WhereExists(qf.Customer
				//                                .CorrelatedOver(CustomerEntity.Relations.OrderEntityUsingCustomerId)
				//                                .Where(CustomerFields.Country.Like("%land%")));
				var results = adapter.FetchQuery(q);
				Assert.AreEqual(66, results.Count);
			}
		}


		/// <summary>
		/// Gets the customers based on starts with call on string field.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnStartsWithCallOnStringField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Country.StartsWith("A")
				//        select c;
				//List<CustomerEntity> results = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer.Where(CustomerFields.Country.StartsWith("A"));

				// alternative
				// var q = qf.Customer.Where(CustomerFields.Country.Like("A%"));

				var results = adapter.FetchQuery(q);
				Assert.AreEqual(5, results.Count);
			}
		}

		[Test]
		public void GetCustomersBasedOnEndsWithCallOnStringField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Country.EndsWith("A")
				//        select c;
				//List<CustomerEntity> results = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer.Where(CustomerFields.Country.EndsWith("A"));

				// alternative
				// var q = qf.Customer.Where(CustomerFields.Country.Like("%A"));

				var results = adapter.FetchQuery(q);
				Assert.AreEqual(25, results.Count);
			}
		}


		/// <summary>
		/// Gets the 14th element in a sorted list of customers using ElementAt. 
		/// </summary>
		[Test]
		public void Get14thElementInSortedListOfCustomersUsingElementAt()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//// get 14th element, which is element with index 13.
				//var q = (from c in metaData.Customer
				//        orderby c.Country ascending, c.City descending
				//        select c).ElementAt(13);
				//CustomerEntity customer = q;

				// ElementAt is just paging with pagesize 1. ElementAt is 0 based, so we have to fetch page _14_ as paging is 1 based.
				var qf = new QueryFactory();
				var q = qf.Customer
								.OrderBy(CustomerFields.Country.Ascending(), CustomerFields.City.Descending())
								.Page(14, 1);
				CustomerEntity customer = adapter.FetchFirst(q);
				Assert.AreEqual("RICAR", customer.CustomerId);
			}
		}


		/// <summary>
		/// Gets the employees from UK using Except filtering.
		/// </summary>
		[Test]
		public void GetEmployeesFromUKUsingExceptFiltering()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from e in
				//            metaData.Employee.Except(from e in metaData.Employee where e.Country == "USA" select e)
				//        select e;
				//List<EmployeeEntity> employeesFromUK = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Employee.Except(qf.Employee.Where(EmployeeFields.Country == "USA"));

				// alternative
				//var q = qf.Employee.TargetAs("E")
				//                .WhereNotExists(qf.Employee
				//                                        .Where((EmployeeFields.Country == "USA")
				//                                                .And(EmployeeFields.EmployeeId.Source("E") == EmployeeFields.EmployeeId)));
				var employeesFromUK = adapter.FetchQuery(q);
				Assert.AreEqual(4, employeesFromUK.Count);
			}
		}


		/// <summary>
		/// Test which focuses on Except on an entity collection combined with an aggregate
		/// </summary>
		[Test]
		public void GetAllCustomersWhoHaveMoreThan5OrdersFiledByEmployeesOtherThanEmployee3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Except(from o in metaData.Order where o.EmployeeId == 3 select o).Count() > 5
				//        select c;
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Except(qf.Order.Where(OrderFields.EmployeeId == 3))
											.CountRow().GreaterThan(5));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(56, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoHaveMoreThan5OrdersFiledByEmployeesOtherThanEmployee3UsingInMemoryList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q2 = from o in metaData.Order where o.EmployeeId == 3 select o;
				//List<OrderEntity> ordersToExclude = q2.ToList();
				//var q = from c in metaData.Customer
				//        where c.Orders.Except(ordersToExclude).Count() > 5
				//        select c;
				//List<CustomerEntity> customers = q.ToList();
				var qf = new QueryFactory();
				var q2 = qf.Order.Where(OrderFields.EmployeeId == 3);
				var ordersToExclude = adapter.FetchQuery(q2, new EntityCollection<OrderEntity>());
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Except(ordersToExclude)
											.CountRow().GreaterThan(5));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(56, customers.Count);
			}
		}


		/// <summary>
		/// Gets all customers who are from the USA and are owner using intersect.
		/// </summary>
		[Test]
		public void GetAllCustomersWhoAreFromTheUSAAndAreOwnerUsingIntersect()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where c.Country == "USA"
				//         select c).Intersect(from c in metaData.Customer where c.ContactTitle == "Owner" select c);
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "USA")
								.Intersect(qf.Customer.Where(CustomerFields.ContactTitle == "Owner"));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(2, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoAreFromTheUSAAndAreOwnerUsingIntersectWithInMemorySet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q2 = from c in metaData.Customer where c.ContactTitle == "Owner" select c;
				//List<CustomerEntity> owners = q2.ToList();
				//var q = (from c in metaData.Customer
				//         where c.Country == "USA"
				//         select c).Intersect(owners);
				//List<CustomerEntity> customers = q.ToList();

				var qf = new QueryFactory();
				var q2 = qf.Customer.Where(CustomerFields.ContactTitle == "Owner");
				var owners = adapter.FetchQuery(q2, new EntityCollection<CustomerEntity>());
				var q = qf.Customer
								.Where(CustomerFields.Country == "USA")
								.Intersect(owners);
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(2, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoHaveOrdersWithMoreThan4ProductsAndWhoHaventPurchasedProduct3UsingNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.OrderDetails.Except(from od in metaData.OrderDetail where od.ProductId == 3 select od).Count() > 4
				//        select o.Customer;
				//var customers = q.ToList();

				// as we're fetching customers, the query focuses on customer fetching. QuerySpec doesn't support fetching a related entity
				// from a query.
				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget.InnerJoin(qf.Order).On(CustomerFields.CustomerId==OrderFields.CustomerId))
								.Where(qf.OrderDetail
												.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId) // tie OrderDetail to the joined Order query.
												.Except(qf.OrderDetail.Where(OrderDetailFields.ProductId == 3))
												.CountRow().GreaterThan(4));
				var customers = adapter.FetchQuery(q);
				Assert.AreEqual(15, customers.Count);
			}
		}


		[Test]
		public void GetAllEmployeesWithoutPhotoNorNoteFields()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var employees = (from e in metaData.Employee
				//                 select e).ExcludeFields(e => e.Photo, e => e.Notes);

				var qf = new QueryFactory();
				var q = qf.Employee
								.Exclude(EmployeeFields.Photo, EmployeeFields.Notes);
				var employees = adapter.FetchQuery(q, new EntityCollection<EmployeeEntity>());
				int count = 0;
				foreach(var v in employees)
				{
					count++;
					Assert.IsTrue(v.Photo.Length <= 0);
					Assert.IsTrue(v.Notes.Length <= 0);
				}
				Assert.AreEqual(9, count);
			}
		}


		[Test]
		public void GetAllEmployeesWithJustAFewFieldsIncluded()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				//var employees = (from e in metaData.Employee
				//                 select e).IncludeFields(e => e.FirstName, e => e.Title);
				var qf = new QueryFactory();
				var q = qf.Employee
								.Include(EmployeeFields.FirstName, EmployeeFields.Title);
				var employees = adapter.FetchQuery(q, new EntityCollection<EmployeeEntity>());

				int count = 0;
				foreach(var v in employees)
				{
					count++;
					Assert.IsTrue(v.Photo.Length <= 0);
					Assert.IsTrue(v.Notes.Length <= 0);
					Assert.IsTrue(v.FirstName.Length > 0);
					Assert.IsTrue(v.Title.Length > 0);
				}
				Assert.AreEqual(9, count);
			}
		}


		[Test]
		public void GetAllProductsWithJustAFewFieldsIncluded()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var products = (from p in metaData.Product
				//                select p).IncludeFields(p => p.ProductName, p => p.QuantityPerUnit, p => p.Discontinued);

				var qf = new QueryFactory();
				var q = qf.Product.Include(ProductFields.ProductName, ProductFields.QuantityPerUnit, ProductFields.Discontinued);
				var products = adapter.FetchQuery(q, new EntityCollection<ProductEntity>());
				int count = 0;
				foreach(var v in products)
				{
					count++;
					Assert.IsFalse(v.ReorderLevel.HasValue);
					Assert.IsTrue(v.ProductName.Length > 0);
					Assert.IsTrue(v.QuantityPerUnit.Length > 0);
				}
				Assert.AreEqual(77, count);
			}
		}


		[Test]
		public void GetListOfProductsWhichAreDiscontinued()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from p in metaData.Product
				//        where p.Discontinued
				//        select p;
				var qf = new QueryFactory();

				// QuerySpec requires the full expression
				var q = qf.Product.Where(ProductFields.Discontinued == true);
				var products = adapter.FetchQuery(q);
				int count = 0;
				foreach(ProductEntity v in products)
				{
					Assert.IsTrue(v.Discontinued);
					count++;
				}
				Assert.AreEqual(55, count);
			}
		}


		[Test]
		public void GetListOfProductsWhichAreDiscontinuedAndSuppliedBySupplier5()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var products = from p in metaData.Product
				//               where p.Discontinued && (p.SupplierId == 5)
				//               select p;

				var qf = new QueryFactory();
				var q = qf.Product.Where((ProductFields.Discontinued == true).And(ProductFields.SupplierId == 5));

				var products = adapter.FetchQuery(q);

				int count = 0;
				foreach(ProductEntity v in products)
				{
					Assert.IsTrue(v.Discontinued);
					Assert.IsTrue(v.SupplierId == 5);
					count++;
				}
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GetListOfCustomersWhichHaveOrdersFiledByMoreThan2EmployeesUsingMNRelation()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.EmployeeCollectionViaOrder.Count() > 2
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.From(QueryTarget.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
											.CountRow().GreaterThan(2));
				var customers = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(86, count);
			}
		}


		/// <summary>
		/// HomePhone is renamed to Phone. This query will fail if the provider incorrectly produces fields targeting the derived table.
		/// </summary>
		[Test]
		public void GetListOfEmployeesUsingDoubleSorterAndRenamedEmployeeField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from e in metaData.Employee
				//        join o in metaData.Order on e.EmployeeId equals o.EmployeeId
				//        join c in metaData.Customer on o.CustomerId equals c.CustomerId
				//        where c.CustomerId == "ALFKI"
				//        select e;
				//q = q.OrderBy(e => e.City).ThenBy(e => e.Region);

				var qf = new QueryFactory();
				var q2 = qf.Employee
								.From(QueryTarget.InnerJoin(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
										.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
								.Where(CustomerFields.CustomerId == "ALFKI");

				// QuerySpec allows augmenting of the actual query without derived table wrapping behind the scenes. To force the derived table wrapping
				// the test requires, we wrap the query with a new query and specify the 'From' clause to obtain the data from that query instead. We
				// have to alias the wrapped query and use the alias given as the alias to target in e.g. the order by.
				var q = qf.Employee.TargetAs("E")
								.From(q2.As("E"))
								.OrderBy(EmployeeFields.City.Source("E").Ascending(), EmployeeFields.Region.Source("E").Ascending());

				// alternatively, the orderby could have been added to q2 directly without causing a derived table, which isn't doable in Linq.:
				// No aliasing is happening so no source alias required.
				// q2.OrderBy(EmployeeFields.City.Ascending(), EmployeeFields.Region.Ascending());

				var employees = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in employees)
				{
					count++;
				}
				Assert.AreEqual(4, count);
			}
		}


		[Test]
		public void GetOrdersWithNulledShippedDate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where o.ShippedDate == null
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order.Where(OrderFields.ShippedDate.IsNull());
				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(OrderEntity v in orders)
				{
					count++;
					Assert.IsNull(v.ShippedDate);
				}
				Assert.AreEqual(19, count);
			}
		}


		[Test]
		public void GetCustomerUsingFirstWithUnknownID()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where c.CustomerId == "F1111"
				//         select c).FirstOrDefault();
				var qf = new QueryFactory();
				var q = qf.Customer.Where(CustomerFields.CustomerId == "F1111");
				var c = adapter.FetchFirst(q);

				Assert.IsNull(c);
			}
		}


		[Test]
		public void GetEntitiesUsingBetweenPredicate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from e in metaData.Employee
				//        where e.EmployeeId < 5 && e.EmployeeId > 1
				//        select e;
				var qf = new QueryFactory();
				// plain values. A begin/end value can be a field, expression or plain value.
				var q = qf.Employee
								.Where(EmployeeFields.EmployeeId.Between(2, 4));

				var employees = adapter.FetchQuery(q);
				Assert.AreEqual(3, employees.Count);
			}
		}


		[Test]
		public void GetEntitiesUsingBetweenPredicate2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				// fields as value. A begin/end value can be a field, expression or plain value.
				var q = qf.Order.Where(OrderFields.ShippedDate.Between(OrderFields.OrderDate, OrderFields.RequiredDate));
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(761, orders.Count);
			}
		}


		[Test]
		public void GetEntitiesUsingBetweenPredicate3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();

				// fields as value. A begin/end value can be a field, expression or plain value.
				var q = qf.Order.Where(OrderFields.ShippedDate.NotBetween(OrderFields.OrderDate, OrderFields.RequiredDate));
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(38, orders.Count);
			}
		}


		[Test]
		public void GetEntitiesUsingBetweenPredicate4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();

				// expression as value
				var q = qf.Order.Where(OrderFields.ShippedDate.Between(OrderFields.OrderDate, (OrderFields.RequiredDate + 3)));
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(774, orders.Count);
			}
		}



		[Test]
		public void GetEntitiesUsingBetweenPredicate5()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				// what to compare can be a field or expression, here we use an expression.
				var q = qf.Order.Where((OrderFields.EmployeeId + 2).Between(5, 8));
				var orders = adapter.FetchQuery(q);
				Assert.AreEqual(400, orders.Count);
			}
		}


		[Test]
		public void DelegatePredicateWithExpressionFuncOnEntityCollection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer select c;
				//EntityCollection<CustomerEntity> customers = ((ILLBLGenProQuery)q).Execute<EntityCollection<CustomerEntity>>();
				var qf = new QueryFactory();
				var customers = adapter.FetchQuery(qf.Customer, new EntityCollection<CustomerEntity>());

				Assert.AreEqual(91, customers.Count);
				var customersFromGermany = new EntityView2<CustomerEntity>(customers, c=>c.Country=="Germany");

				Assert.AreEqual(11, customersFromGermany.Count);

				var customersFromGermany2 = new EntityView2<CustomerEntity>(customers);
				// set filter using DelegatePredicate instance
				customersFromGermany2.Filter = new DelegatePredicate<CustomerEntity>(c => c.Country == "Germany");
				Assert.AreEqual(11, customersFromGermany2.Count);
			}
		}


		[Test]
		public void GetMaxOrdersPerCustomerUsingJoinWithGroupedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q1 = from o in metaData.Order
				//         where o.OrderDate <= new DateTime(1997, 8, 1)
				//         && o.EmployeeId != null
				//         group o by o.CustomerId into g
				//         select new { LastId = g.Max(x => x.OrderId) };
				//var q = from o in metaData.Order
				//        join ids in q1
				//        on o.OrderId equals ids.LastId
				//        where ids.LastId > 10600
				//        select o;
				//var orders = q.ToList();

				var qf = new QueryFactory();
				// As 1 query:
				var q = qf.Order
							.From(QueryTarget
									.InnerJoin(qf.Create("q1")
													.Select(OrderFields.OrderId.Max().As("LastId"), OrderFields.CustomerId)
													.Where((OrderFields.OrderDate <= new DateTime(1997, 8, 1))
																.And(OrderFields.EmployeeId.IsNotNull()))
													.GroupBy(OrderFields.CustomerId))
										.On(OrderFields.OrderId == qf.Field("LastId").Source("q1")))
							.Where(qf.Field("LastId").Source("q1") > 10600);

				//// Alternative, defined in 2 separated queries like the linq query
				////var q1 = qf.Create("q1")
				////            .Select(OrderFields.OrderId.Max().As("LastId"), OrderFields.CustomerId)
				////            .Where((OrderFields.OrderDate <= new DateTime(1997, 8, 1))
				////                        .And(OrderFields.EmployeeId.IsNotNull()))
				////            .GroupBy(OrderFields.CustomerId);
				////var q = qf.Order
				////            .From(QueryTarget.InnerJoin(q1).On(OrderFields.OrderId == qf.Field("LastId").Source("q1")))
				////            .Where(qf.Field("LastId").Source("q1") > 10600);

				var orders = adapter.FetchQuery(q);

				Assert.AreEqual(12, orders.Count);
			}
		}


		[Test]
		public void GetOrdersSortedByShippedDateOrOrderDateUsingCoalesce()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        orderby o.ShippedDate ?? o.OrderDate
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order
							.OrderBy(Functions.Coalesce(OrderFields.ShippedDate, OrderFields.OrderDate).Ascending());
				var results = adapter.FetchQuery(q);
				int count = 0;
				DateTime? prev = null;
				DateTime? curr = null;
				bool firstTime = true;
				foreach(OrderEntity v in results)
				{
					prev = curr;
					curr = v.ShippedDate ?? v.OrderDate;
					if(!firstTime)
					{
						Assert.IsTrue(prev <= curr);
					}
					count++;
					firstTime = false;
				}

				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetAllCustomersSortedBySecondCharacterInCustomerId()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        orderby c.CustomerId[1]
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer.OrderBy(CustomerFields.CustomerId.Char(1).Ascending());

				// Alternative
				//var q = qf.Customer
				//                .OrderBy(StringFunctions.Char(CustomerFields.CustomerId, 1).Ascending());

				var customers = adapter.FetchQuery(q);
				char prev = ' ';
				char curr = ' ';
				bool firstRun = false;
				foreach(CustomerEntity v in customers)
				{
					prev = curr;
					curr = v.CustomerId[1];
					if(firstRun)
					{
						firstRun = false;
						continue;
					}
					Assert.IsTrue(prev <= curr);
				}
			}
		}

		[Test]
		public void GetAllOrdersFromCustomersFromTheUSAUsingQueryInJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        join cu in
				//            (
				//                from c in metaData.Customer
				//                where c.Country == "USA"
				//                select c.CustomerId) on o.CustomerId equals cu
				//        select o;

				// data for verification.
				var q2 = from c in metaData.Customer
						 where c.Country == "USA"
						 select c.CustomerId;
				List<string> customerIds = q2.ToList();

				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(qf.Customer.As("cu")
																.Where(CustomerFields.Country == "USA")
																.Select(CustomerFields.CustomerId))
												.On(OrderFields.CustomerId == CustomerFields.CustomerId.Source("cu")));
				// alternative:
				//var q = qf.Order
				//            .From(QueryTarget.InnerJoin(qf.Customer.As("cu")
				//                                                .Where(CustomerFields.Country == "USA")
				//                                                .Select(CustomerFields.CustomerId))
				//                                .On(OrderFields.CustomerId == qf.Field("CustomerId").Source("cu")));

				var orders = adapter.FetchQuery(q);
				int count = 0;
				foreach(OrderEntity v in orders)
				{
					Assert.IsTrue(customerIds.Contains(v.CustomerId));
					count++;
				}
				Assert.AreEqual(121, count);
			}
		}


		[Test]
		public void GetAllOrdersWithATotalHigherThan5000()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        join ods in
				//            (from od in metaData.OrderDetail
				//             group od by od.OrderId into g
				//             select new { OrderId = g.Key, Total = g.Sum(x => x.Quantity * x.UnitPrice) })
				//          on o.OrderId equals ods.OrderId
				//        where ods.Total > 5000
				//        select o;

				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(qf.Create("ods")
															.Select(OrderDetailFields.OrderId,
																	(OrderDetailFields.Quantity * OrderDetailFields.UnitPrice).Sum().As("Total"))
															.GroupBy(OrderDetailFields.OrderId))
												.On(OrderFields.OrderId == qf.Field("OrderId").Source("ods")))
							.Where(qf.Field("Total").Source("ods") > 5000);

				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in orders)
				{
					count++;
				}
				Assert.AreEqual(38, count);
			}
		}

		[Test]
		public void CompoundPKJoinFetch()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var pks = from p in metaData.PksideCompoundKey
				//          join f in metaData.FksideCompoundKey on
				//              new { IdField1 = p.Idfield1, IdField2 = p.Idfield2 } equals new { IdField1 = f.Idfield1.Value, IdField2 = f.Idfield2.Value }
				//          select f;

				// QuerySpec focuses on what you want to obtain, so as the Fk side has to be fetched we'll start with that
				var qf = new QueryFactory();
				var q = qf.FksideCompoundKey
								.From(QueryTarget.InnerJoin(qf.PksideCompoundKey)
														.On((FksideCompoundKeyFields.Idfield1 == PksideCompoundKeyFields.Idfield1)
																.And(FksideCompoundKeyFields.Idfield2 == PksideCompoundKeyFields.Idfield2)));

				// alternatively, we can use the relationship generated in the code so we don't have to worry about which field relates to what.
				//var q = qf.FksideCompoundKey
				//                .From(QueryTarget.InnerJoin(FksideCompoundKeyEntity.Relations.PksideCompoundKeyEntityUsingIdfield1Idfield2));

				var pks = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in pks)
				{
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void GetOrdersSortedOnHasValueOfShippedDate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//             where o.CustomerId == "BLAUS"
				//             orderby (o.ShippedDate.HasValue) ascending, o.EmployeeId descending
				//             select o;

				// Sort on the case whether shippeddate is nullable or not. 
				var qf = new QueryFactory();
				var q = qf.Order
							.Where(OrderFields.CustomerId == "BLAUS")
							.OrderBy(OrderFields.ShippedDate.IsNotNull().AsBooleanValue().Ascending(), OrderFields.EmployeeId.Descending());

				var orders = adapter.FetchQuery(q);
				int count = 0;
				bool first = true;
				foreach(OrderEntity v in orders)
				{
					if(first)
					{
						Assert.IsNull(v.ShippedDate);
						first = false;
					}
					else
					{
						Assert.IsNotNull(v.ShippedDate);
					}
					count++;
				}

				Assert.AreEqual(7, count);
			}
		}


		[Test]
		public void GetProductsWhichHaveNoCategoryIDSetUsingHasValue()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from p in metaData.Product
				//        where !p.CategoryId.HasValue
				//        select p;
				var qf = new QueryFactory();
				var q = qf.Product
							.Where(ProductFields.CategoryId.IsNull());
				var products = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in products)
				{
					count++;
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetOrdersUsingContainsWithNullableFieldAndCoalesceInContainsParameter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				List<int> employees = new List<int>() { 2, 3 };

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var orders = from o in metaData.Order
				//             where employees.Contains(o.EmployeeId == null ? -1 : (int)o.EmployeeId)
				//             select o;

				var qf = new QueryFactory();
				var q = qf.Order
							.Where(Functions.IIF(OrderFields.EmployeeId.IsNull(), -1, OrderFields.EmployeeId)
										.In(employees));
				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(OrderEntity v in orders)
				{
					Assert.IsTrue(employees.Contains((int)v.EmployeeId));
					count++;
				}
				Assert.AreEqual(225, count);
			}
		}


		[Test]
		public void GetOrdersUsingContainsWithNullableFieldAndCoalesceInContainsParameterAndContainsOnQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order
				//        where (from e in metaData.Employee where e.FirstName.StartsWith("L") select e.EmployeeId).Contains(o.EmployeeId ?? -1)
				//        select o;
				var qf = new QueryFactory();
				var q = qf.Order
							.Where(Functions.Coalesce(OrderFields.EmployeeId, -1)
												.In(qf.Employee
															.Where(EmployeeFields.FirstName.StartsWith("L"))
															.Select(EmployeeFields.EmployeeId)));

				// alternative:
				//var q = qf.Order
				//            .Where(qf.Employee
				//                        .Where(EmployeeFields.FirstName.StartsWith("L"))
				//                        .Select(EmployeeFields.EmployeeId)
				//                        .Contains(Functions.Coalesce(OrderFields.EmployeeId, -1)));

				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in orders)
				{
					count++;
				}
				Assert.AreEqual(92, count);
			}
		}


		[Test]
		public void GetAllEmployeesWhoHaventFiledOrderForCustomerUsingExceptOnNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var allEmployees = from e in metaData.Employee select e;
				//var employeesCustomer = from e in metaData.Employee
				//                        join o in metaData.Order on e.EmployeeId equals o.EmployeeId
				//                        join c in metaData.Customer on o.CustomerId equals c.CustomerId
				//                        where c.CustomerId == "CHOPS"
				//                        select e;
				//var diff = allEmployees.Except(employeesCustomer);

				var qf = new QueryFactory();
				var allEmployees = qf.Employee;
				var employeesCustomer = qf.Employee
											.From(QueryTarget.InnerJoin(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
															 .InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
											.Where(CustomerFields.CustomerId == "CHOPS");
				var diffQ = allEmployees.Except(employeesCustomer);

				var diff = adapter.FetchQuery(diffQ);

				int count = 0;
				int[] expectedValues = new int[] { 2, 5, 8, 9 };
				foreach(EmployeeEntity v in diff)
				{
					count++;
					Assert.IsTrue(expectedValues.Contains(v.EmployeeId));
				}
				Assert.AreEqual(4, count);
			}
		}

		[Test]
		public void GetAllOrderDetailsForCustomerChopsUsingCrossJoinAndRelatedProperties()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.CustomerId == "CHOPS"
				//        from o in c.Orders
				//        from od in o.OrderDetails
				//        select od;
				var qf = new QueryFactory();

				// QuerySpec focuses on what you want to retrieve, so start with that.
				var q = qf.OrderDetail
								.From(QueryTarget.InnerJoin(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
												 .InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
								.Where(CustomerFields.CustomerId == "CHOPS");
				var ods = adapter.FetchQuery(q);

				int count = 0;
				int[] expectedOrderIds = new int[] { 10254, 10370, 10519, 10731, 10746, 10966, 11029, 11041 };
				foreach(OrderDetailEntity v in ods)
				{
					count++;
					Assert.IsTrue(expectedOrderIds.Contains(v.OrderId));
				}
				Assert.AreEqual(29, count);
			}
		}


		[Test]
		public void GetCustomersWithOrdersFiledByTheMaxEmployeeIdToTestScalarInWhereClause()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        join o in metaData.Order on c.CustomerId equals o.CustomerId
				//        where o.EmployeeId == (from e in metaData.Employee
				//                              where e.EmployeeId == o.EmployeeId
				//                              select e).Min(e => e.EmployeeId)
				//                              && c.Country=="Germany"
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.InnerJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
							.Where(OrderFields.EmployeeId.Equal(qf.Employee
																		.CorrelatedOver(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
																		.Select(EmployeeFields.EmployeeId) // to optimize projection
																		.Min(EmployeeFields.EmployeeId))
															.And(CustomerFields.Country == "Germany"));

				// an alternative (different scalar query construction)
				//var q = qf.Customer
				//            .From(QueryTarget.InnerJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
				//            .Where(OrderFields.EmployeeId.Equal(qf.Employee
				//                                                        .CorrelatedOver(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
				//                                                        .Select(EmployeeFields.EmployeeId.Min())
				//                                                        .ToScalar())	// ToScalar is required as the predicate requires a value to compare with
				//                                            .And(CustomerFields.Country == "Germany"));

				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(11, count);
			}
		}

		[Test]
		public void GetCustomersUsingPredicateBuilder()
		{

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//var predicate = PredicateBuilder.Null<CustomerEntity>();
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//predicate = predicate.Or(c => c.City == "London");
				//predicate = predicate.Or(c => c.City == "Paris");
				//var q = metaData.Customer.Where(predicate);

				var qf = new QueryFactory();
				IPredicate predicate = CustomerFields.City == "London";
				predicate = predicate.Or(CustomerFields.City == "Paris");
				var q = qf.Customer.Where(predicate);
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					Assert.IsTrue((v.City == "London") || (v.City == "Paris"));
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}

		[Test]
		public void GetCustomersWithEmployeesInGivenTerritoryUsingNestedAny()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = metaData.Customer.Where(c => c.EmployeeCollectionViaOrder.Any(
				//                    e => e.TerritoryCollectionViaEmployeeTerritory.Any(
				//                        t => t.RegionId == 2)));

				// QuerySpec can't refer to m:n relations through a single element, so we've to write these out.
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Where(qf.Employee
										.CorrelatedOver(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
										.Any(qf.Territory
													.From(QueryTarget.InnerJoin(TerritoryEntity.Relations.EmployeeTerritoryEntityUsingTerritoryId))
													.Any((EmployeeFields.EmployeeId == EmployeeTerritoryFields.EmployeeId)
														   .And(TerritoryFields.RegionId == 2))));

				// Alternative
				//var q = qf.Customer
				//                .WhereExists(qf.Employee
				//                                    .From(QueryTarget.InnerJoin(qf.Order)
				//                                                .On((EmployeeFields.EmployeeId == OrderFields.EmployeeId)
				//                                                        .And(CustomerFields.CustomerId == OrderFields.CustomerId)))
				//                                    .WhereExists(qf.Territory
				//                                                        .From(QueryTarget.InnerJoin(qf.EmployeeTerritory)
				//                                                                        .On((TerritoryFields.TerritoryId == EmployeeTerritoryFields.TerritoryId)
				//                                                                                .And(EmployeeTerritoryFields.EmployeeId == EmployeeFields.EmployeeId)))
				//                                                        .Where(TerritoryFields.RegionId == 2)));

				// another alternative is to simply join everything, the inner joins will do the filtering. This can be slightly less efficient
				//var q = qf.Customer
				//             .From(QueryTarget.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId)
				//                              .InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId)
				//                              .InnerJoin(EmployeeEntity.Relations.EmployeeTerritoryEntityUsingEmployeeId)
				//                              .InnerJoin(EmployeeTerritoryEntity.Relations.TerritoryEntityUsingTerritoryId))
				//            .Where(TerritoryFields.RegionId == 2);
				
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(74, count);
			}
		}


		[Test]
		public void GetAllCustomersWhoPurchasedProduct1UsingNestedAny()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Where(c => c.Orders.Any(o => o.OrderDetails.Any(od => od.ProductId == 1)));
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Any(qf.OrderDetail
														.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
														.Any(OrderDetailFields.ProductId == 1)));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(31, count);
			}
		}


		[Test]
		public void GetCustomerFromOrderUsingMultipleCalls()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				var qf = new QueryFactory();
				try
				{
					adapter.StartTransaction(System.Data.IsolationLevel.ReadCommitted, "Test");
					//var q = (from o in metaData.Order
					//         where o.OrderId == 10254
					//         select o.Customer).FirstOrDefault();
					var q = qf.Order
								.Where(OrderFields.OrderId == 10254);
					var o = adapter.FetchFirst(q);

					Assert.IsNotNull(o);

					//q = (from o in metaData.Order
					//     where o.OrderId == 10254
					//     select o.Customer).FirstOrDefault();
					o = adapter.FetchFirst(q);

					Assert.IsNotNull(o);
					Assert.AreEqual("CHOPS", o.CustomerId);
				}
				finally
				{
					adapter.Commit();
				}
			}
		}


		[Test]
		[ExpectedException(ExpectedException = typeof(NotSupportedException), ExpectedMessage = "The query executed should have resulted in 1 element due to the usage of FetchSingle(), though resulted in 6 elements being returned.")]
		public void GetCustomerUsingSingleAndMultiPredicateFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Single(c => c.Country == "USA" && c.CustomerId.Contains("E"));
				var qf = new QueryFactory();
				var q = qf.Customer.Where((CustomerFields.Country == "USA").And(CustomerFields.CustomerId.Contains("E")));
				var c = adapter.FetchSingle(q);
			}
		}


		[Test]
		public void GetCustomersWithContainsOnEmptyCollectionAndConstantBoolFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				List<string> ids = new List<string>();
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where (ids.Count > 0) ? ids.Contains(c.CustomerId) : false
				//        select c;

				// QuerySpec can't evaluate code fragments inside the query as it's a specification mechanism. 
				// however the native query api can deal with empty collection comparisons
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.CustomerId.In(ids));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(0, count);
			}
		}


		[Test]
		public void GetCustomersWithContainsOnEmptyCollectionAndConstantBoolFilter2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				List<string> ids = new List<string>();
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where ids.Contains(c.CustomerId) && (ids.Count>0)
				//        select c;

				// QuerySpec can't evaluate code fragments inside the query as it's a specification mechanism. 
				// however the native query api can deal with empty collection comparisons
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.CustomerId.In(ids));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(0, count);
			}
		}


		[Test]
		public void OrderByAfterTakePageFetchToArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from o in metaData.Order select o;
				//q = q.TakePage(2,20).OrderBy(o => o.CustomerId);
				var qf = new QueryFactory();
				var q = qf.Order;
				q.Page(2, 20).OrderBy(OrderFields.CustomerId.Ascending());

				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(OrderEntity o in orders)
				{
					count++;
				}
				Assert.AreEqual(20, count);
			}
		}


		[Test]
		public void SelectManyWithNoProjectionQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Where(c => c.CustomerId == "CHOPS").SelectMany(c => c.Orders);

				var qf = new QueryFactory();
				// QuerySpec focuses on what you want to retrieve, so start with that.
				var q = qf.Order
							.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Where(OrderFields.CustomerId == "CHOPS");
				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(OrderEntity v in orders)
				{
					Assert.AreEqual("CHOPS", v.CustomerId);
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void TreeFetchingWithScalarQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				int employeeid = 4;
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var siblings = from e in metaData.Employee
				//               where e.ReportsTo.Value == (from m in metaData.Employee where m.EmployeeId==employeeid select m.ReportsTo.Value).First()
				//               select e;

				var qf = new QueryFactory();
				var q = qf.Employee
							.Where(EmployeeFields.ReportsTo == qf.Employee
																		.Where(EmployeeFields.EmployeeId == employeeid)
																		.Select(EmployeeFields.ReportsTo)
																		.ToScalar(true));
				var siblings = adapter.FetchQuery(q);
				int count = 0;
				foreach(EmployeeEntity v in siblings)
				{
					Assert.AreEqual(2, v.ReportsTo.Value);
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void FilterOnQueryWithContainsAndPredefinedPairs()
		{
			var idList = new List<Pair<int, int>>();
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 13 });
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 14 });
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 15 });

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from od in metaData.OrderDetail
				//        select od;
				//var q2 = q.Where(od => idList.Contains(new Pair<int, int>() { Value1 = od.OrderId, Value2 = od.ProductId }));

				var qf = new QueryFactory();
				var q = qf.OrderDetail;
				var q2 = q.Where(new[] {OrderDetailFields.OrderId.As("Value1"), OrderDetailFields.ProductId.As("Value2")}
										.In(idList));
				var orderDetails = adapter.FetchQuery(q2);
				int count = 0;
				foreach(OrderDetailEntity v in orderDetails)
				{
					count++;
					Assert.AreEqual(10254, v.OrderId);
					Assert.IsTrue(new int[] { 13, 14, 15}.Contains(v.ProductId));
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		[ExpectedException(typeof(ORMQueryConstructionException))]
		public void FilterOnQueryWithContainsAndPredefinedPairsUsingCTor()
		{
			var idList = new List<Pair<int, int>>();
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 13 });
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 14 });
			idList.Add(new Pair<int, int>() { Value1 = 10254, Value2 = 15 });

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from od in metaData.OrderDetail
				//        select od;
				//var q2 = q.Where(od => idList.Contains(new Pair<int, int>(od.OrderId, od.ProductId )));


				var qf = new QueryFactory();
				var q = qf.OrderDetail;
				var q2 = q.Where(new[] { OrderDetailFields.OrderId, OrderDetailFields.ProductId }.In(idList));
				var orderDetails = adapter.FetchQuery(q2);

				foreach(var v in orderDetails)
				{
				}
			}
		}


		[Test]
		public void ManyLeftJoinsInSingleQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var queryEntity = metaData.Category;

				//var q = from _e1 in metaData.Category
				//        join _e2 in metaData.Category on _e1.CategoryId equals _e2.CategoryId
				//        into _e1_e2
				//        from _e2 in _e1_e2.DefaultIfEmpty()

				//        join _e3 in metaData.Category on _e2.CategoryId equals _e3.CategoryId
				//        into _e2_e3
				//        from _e3 in _e2_e3.DefaultIfEmpty()

				//        join _e4 in metaData.Category on _e3.CategoryId equals _e4.CategoryId
				//        into _e3_e4
				//        from _e4 in _e3_e4.DefaultIfEmpty()

				//        join _e5 in metaData.Category on _e4.CategoryId equals _e5.CategoryId
				//        into _e4_e5
				//        from _e5 in _e4_e5.DefaultIfEmpty()

				//        join _e6 in metaData.Category on _e5.CategoryId equals _e6.CategoryId
				//        into _e5_e6
				//        from _e6 in _e5_e6.DefaultIfEmpty()

				//        join _e7 in metaData.Category on _e6.CategoryId equals _e7.CategoryId
				//        into _e6_e7
				//        from _e7 in _e6_e7.DefaultIfEmpty()

				//        join _e8 in metaData.Category on _e7.CategoryId equals _e8.CategoryId
				//        into _e7_e8
				//        from _e8 in _e7_e8.DefaultIfEmpty()

				//        join _e9 in metaData.Category on _e8.CategoryId equals _e9.CategoryId
				//        into _e8_e9
				//        from _e9 in _e8_e9.DefaultIfEmpty()

				//        join _e10 in metaData.Category on _e9.CategoryId equals _e10.CategoryId
				//        into _e9_e10
				//        from _e10 in _e9_e10.DefaultIfEmpty()

				//        join _e11 in metaData.Category on _e10.CategoryId equals _e11.CategoryId
				//        into _e10_e11
				//        from _e11 in _e10_e11.DefaultIfEmpty()

				//        join _e12 in metaData.Category on _e11.CategoryId equals _e12.CategoryId
				//        into _e11_e12
				//        from _e12 in _e11_e12.DefaultIfEmpty()

				//        join _e13 in metaData.Category on _e12.CategoryId equals _e13.CategoryId
				//        into _e12_e13
				//        from _e13 in _e12_e13.DefaultIfEmpty()

				//        join _e14 in metaData.Category on _e13.CategoryId equals _e14.CategoryId
				//        into _e13_e14
				//        from _e14 in _e13_e14.DefaultIfEmpty()

				//        join _e15 in metaData.Category on _e14.CategoryId equals _e15.CategoryId
				//        into _e14_e15
				//        from _e15 in _e14_e15.DefaultIfEmpty()

				//        select _e1;
				//var list = q.ToList();

				var qf = new QueryFactory();
				var q = qf.Category
							.From(QueryTarget.LeftJoin(qf.Category.As("e1")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e1"))
											.LeftJoin(qf.Category.As("e2")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e2"))
											.LeftJoin(qf.Category.As("e3")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e3"))
											.LeftJoin(qf.Category.As("e4")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e4"))
											.LeftJoin(qf.Category.As("e5")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e5"))
											.LeftJoin(qf.Category.As("e6")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e6"))
											.LeftJoin(qf.Category.As("e7")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e7"))
											.LeftJoin(qf.Category.As("e8")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e8"))
											.LeftJoin(qf.Category.As("e9")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e9"))
											.LeftJoin(qf.Category.As("e10")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e10"))
											.LeftJoin(qf.Category.As("e11")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e11"))
											.LeftJoin(qf.Category.As("e12")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e12"))
											.LeftJoin(qf.Category.As("e13")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e13"))
											.LeftJoin(qf.Category.As("e14")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e14"))
											.LeftJoin(qf.Category.As("e15")).On(CategoryFields.CategoryId == CategoryFields.CategoryId.Source("e15")));
				var list = adapter.FetchQuery(q);
			
			}
		}


		[Test]
		public void FirstOrDefaultOnQueryInWhereFollowedByMemberAccess1()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var employees = from e in metaData.Employee
				//                where e.Orders.First(o => o.OrderDate != null).Customer.Country == "USA"
				//                select e;

				var qf = new QueryFactory();
				var q = qf.Employee
								.Where(qf.Create("c")
											.From(qf.Customer.InnerJoin(qf.Order.As("o")
																			.CorrelatedOver(OrderEntity.Relations.EmployeeEntityUsingEmployeeId)
																			.Where(OrderFields.OrderDate.IsNotNull()))
																	.On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o")))
											.Select(CustomerFields.Country)
											.ToScalar(true).Equal("USA"));

				// alternative
				//var q = qf.Employee
				//                .WhereExists(qf.Customer
				//                            .From(QueryTarget.InnerJoin(qf.Order.As("o")
				//                                                            .CorrelatedOver(OrderEntity.Relations.EmployeeEntityUsingEmployeeId)
				//                                                            .Where(OrderFields.OrderDate.IsNotNull())
				//                                                            .Limit(1))
				//                                            .On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o")))
				//                            .Where(CustomerFields.Country == "USA"));
				var employees = adapter.FetchQuery(q);
				int count = 0;
				foreach(EmployeeEntity v in employees)
				{
					count++;
					Assert.AreEqual(8, v.EmployeeId);
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void FirstOrDefaultOnQueryInWhereFollowedByMemberAccess2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from e in metaData.Employee
				//        where e.Orders.Where(o => o.OrderDate != null).Select(o => o.Customer).First().Country == "USA"
				//        select e;

				// same query as previous unit test.
				var qf = new QueryFactory();
				var q = qf.Employee
								.Where(qf.Create("c")
											.From(qf.Customer.InnerJoin(qf.Order.As("o")
																			.CorrelatedOver(OrderEntity.Relations.EmployeeEntityUsingEmployeeId)
																			.Where(OrderFields.OrderDate.IsNotNull()))
																	.On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o")))
											.Select(CustomerFields.Country)
											.ToScalar(true).Equal("USA"));
				var employees = adapter.FetchQuery(q);
				int count = 0;
				foreach(EmployeeEntity v in employees)
				{
					count++;
					Assert.AreEqual(8, v.EmployeeId);
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void FetchingSecondPageUsingOutOfOrderSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.Customer.Take(10).Skip(10);

				var qf = new QueryFactory();
				var q = qf.Customer.Page(2, 10);
				var customers = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(10, count);
			}
		}


		[Test]
		public void OrderByWithIIFFailureTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        orderby (c != null) ? c.CompanyName : null
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer
							.OrderBy(Functions.IIF(CustomerFields.CustomerId.IsNotNull(), CustomerFields.CompanyName, DBNull.Value).Ascending());
				var customers = adapter.FetchQuery(q);
				foreach(var v in customers)
				{
				}
			}
		}


		[Test]
		public void GetAllCustomersWithMoreThan3Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Customer
				//        where c.Orders.Count() > 3
				//        select c;

				var qf = new QueryFactory();
				var q = qf.Customer.Where(qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId).CountRow().GreaterThan(3));
				// alternative
				//var q = qf.Customer.Where(qf.Order.Where(CustomerFields.CustomerId==OrderFields.CustomerId).CountRow().GreaterThan(3));

				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in customers)
				{
					count++;
				}
				Assert.AreEqual(78, count);
			}
		}


		[Test]
		public void GetTop10OrdersFromCustomersUsingOrderOnCustomer()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from o in metaData.Order
				//         join c in metaData.Customer on o.CustomerId equals c.CustomerId
				//         orderby c.Country ascending
				//         select o).Distinct().Take(10);

				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.OrderBy(CustomerFields.Country.Ascending())
							.Limit(10);
				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in orders)
				{
					count++;
				}
				Assert.AreEqual(10, count);
			}
		}


		[Test]
		public void ListOfCollectionsFetch()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var ccds = from c in metaData.Customer
				//            where c.Orders.Any(o => o.OrderDetails.Count() > 0)
				//            select c.CustomerCustomerDemos;


				// QuerySpec doesn't support the fetch of collections of entities, just 1 collection of entities. This results in 1 query instead of
				// the 2 of the linq query (which uses nested queries to build the list of collections).
				var qf = new QueryFactory();
				var q = qf.CustomerCustomerDemo
								.From(QueryTarget.InnerJoin(CustomerEntity.Relations.CustomerCustomerDemoEntityUsingCustomerId)
												 .InnerJoin(qf.Order.As("o")
																.Where(qf.OrderDetail
																				.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
																				.Any()))
														.On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o")));
				
				//// ALternative
				////var q = qf.CustomerCustomerDemo
				////                .WhereExists(qf.Customer
				////                            .CorrelatedOver(CustomerEntity.Relations.CustomerCustomerDemoEntityUsingCustomerId)
				////                            .WhereExists(qf.Order
				////                                        .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
				////                                        .Where(qf.OrderDetail
				////                                                        .CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
				////                                                        .Any())));

				var ccds = adapter.FetchQuery(q);
				foreach(var v in ccds)
				{
					
				}
			}
		}


		[Test]
		public void GetOrdersWithMultiHopDefaultIfEmpty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from e in metaData.Employee
				//        from o in e.Orders.DefaultIfEmpty()
				//        from x in o.Customer.CustomerCustomerDemos.DefaultIfEmpty()
				//        select o;

				// QUeryspec explicitly defines the joins to follow. It starts with the entity to fetch, Order. 
				// The linq query above ... without checking, can you tell whether it's employee left join order or order left join employee? ;)
				var qf = new QueryFactory();
				var q = qf.Order
							.From(QueryTarget.RightJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId)
											 .LeftJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											 .LeftJoin(CustomerEntity.Relations.CustomerCustomerDemoEntityUsingCustomerId));
				var orders = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in orders)
				{
					count++;
				}
				Assert.AreEqual(818, count);
			}
		}


		#region PrefetchPath tests using PathEdge api
		[Test]
		public void GetOrdersWithCustomerUsingPathAndAppendedOrderBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 25;
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from o in metaData.Order
				//         select o).WithPath<OrderEntity>(p => p.Prefetch(o => o.Customer));
				//q = q.OrderBy(o=>o.OrderDate).Take(64);

				var qf = new QueryFactory();
				var q = qf.Order
							.WithPath(OrderEntity.PrefetchPathCustomer)
							.Limit(64)
							.OrderBy(OrderFields.OrderDate.Ascending());
				var orders = adapter.FetchQuery(q);

				int count = 0;
				foreach(OrderEntity v in orders)
				{
					count++;
					if(v.OrderId == 10253)
					{
						// casing difference
						Assert.IsNull(v.Customer);
					}
					else
					{
						Assert.IsNotNull(v.Customer);
					}
				}

				Assert.AreEqual(64, count);
			}
		}

		[Test]
		public void GetAllCustomersWithTheirRelatedOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders));

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
								.WithPath(CustomerEntity.PrefetchPathOrders);
				var customers = adapter.FetchQuery(q);

				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersAndEmployeesUsingContext()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Context contextToUse = new Context();

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//metaData.ContextToUse = contextToUse;
				//// use a query which fetches Employees through orders into the customers and employee through order.
				//// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
				//var q = (from c in metaData.Customer
				//         where c.Country=="Germany"
				//         select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, 
				//                                    new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)), 
				//                            new PathEdge<EmployeeEntity>(CustomerEntity.PrefetchPathEmployeeCollectionViaOrder));

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "Germany")
								.WithPath(CustomerEntity.PrefetchPathOrders.WithSubPath(OrderEntity.PrefetchPathEmployee),
										  CustomerEntity.PrefetchPathEmployeeCollectionViaOrder);
				q.ContextToUse = contextToUse;
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					// use object instance compare with linear search, otherwise we could get false positives as Equals and GetHashCode are tied to
					// the PK values of the entities, which obviously are equal.
					Assert.IsTrue(v.Orders.Count > 0);
					foreach(OrderEntity o in v.Orders)
					{
						// traverse the employees in v.Employees and do an object instance compare. Has to be there
						bool found = false;
						foreach(EmployeeEntity e in v.EmployeeCollectionViaOrder)
						{
							if((object)e == (object)o.Employee)
							{
								found = true;
								break;
							}
						}
						Assert.IsTrue(found);
					}
				}
				Assert.AreEqual(11, count);
			}
		}



		[Test]
		public void GetAllOrdersWithRelatedCustomerWhichAreHandledByEmployee2()
		{
			var orders = new EntityCollection<OrderEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Order
							.Where(OrderFields.EmployeeId == 2)
							.WithPath(OrderEntity.PrefetchPathCustomer);

				adapter.FetchQuery(q, orders);
			}

			Assert.AreEqual(110, orders.Count);
			foreach(var v in orders)
			{
				Assert.IsNotNull(v.Customer);
			}
		}


		[Test]
		public void GetAllEmployeesWithTheirLastOrder()
		{
			var employees= new EntityCollection<EmployeeEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();
				var q = qf.Employee
							.WithPath(EmployeeEntity.PrefetchPathOrders
											.WithOrdering(OrderFields.OrderDate.Descending())
											.WithLimit(1));

				adapter.FetchQuery(q, employees);
			}

			Assert.AreEqual(9, employees.Count);
			foreach(var v in employees)
			{
				Assert.AreEqual(1, v.Orders.Count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersWhichAreHandledByEmployee3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o => o.EmployeeId == 3));

				var qf = new QueryFactory();
				var q = qf.Customer
							.WithPath(CustomerEntity.PrefetchPathOrders.WithFilter(OrderFields.EmployeeId == 3));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.AreEqual(3, o.EmployeeId);
						}
					}
				}
				Assert.AreEqual(91, count);
			}
		}

	
		[Test]
		public void GetAllCustomersWithTheirLast2OrdersFiledByEmployee2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o=>o.EmployeeId==2,
				//                new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2));
				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
							.WithPath(CustomerEntity.PrefetchPathOrders
															.WithFilter(OrderFields.EmployeeId == 2)
															.WithOrdering(OrderFields.OrderDate.Ascending())
															.WithLimit(2));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue((v.Orders.Count >= 0) && (v.Orders.Count <= 2));
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.AreEqual(2, o.EmployeeId);
						}
					}
				}
				Assert.AreEqual(89, count);
			}
		}

		[Test]
		public void GetAllCustomersWithTheirLast2OrdersWithSubPaths()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c)
				//            .WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null,
				//                            new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2, 
				//                                new PathEdge<OrderDetailEntity>(OrderEntity.PrefetchPathOrderDetails),	// child edge from order
				//                                new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)));		// child edge from order

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
							.WithPath(CustomerEntity.PrefetchPathOrders
														.WithOrdering(OrderFields.OrderDate.Ascending())
														.WithLimit(2)
														.WithSubPath(OrderEntity.PrefetchPathOrderDetails,
																	 OrderEntity.PrefetchPathEmployee));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue((v.Orders.Count > 0) && (v.Orders.Count <= 2));
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.IsTrue(o.OrderDetails.Count > 0);
							Assert.IsNotNull(o.Employee);
						}
					}
				}
				Assert.AreEqual(89, count);
			}
		}


		/// <summary>
		/// Fetches a multi-branch prefetch path: territory m:n employee 1:n Order and employee m:n customer 1:n order.
		/// </summary>
		[Test]
		public void GetTerritoriesWithEmployeesAndCustomersOrdersInComplexPath()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from t in metaData.Territory select t)
				//            .WithPath(
				//                new PathEdge<EmployeeEntity>(
				//                        TerritoryEntity.PrefetchPathEmployeeCollectionViaEmployeeTerritory,
				//                            new PathEdge<OrderEntity>(EmployeeEntity.PrefetchPathOrders),	
				//                            new PathEdge<CustomerEntity>(
				//                                    EmployeeEntity.PrefetchPathCustomerCollectionViaOrder, 
				//                                        new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders))
				//                        )
				//                    );

				var qf = new QueryFactory();
				var q = qf.Territory
								.WithPath(TerritoryEntity.PrefetchPathEmployeeCollectionViaEmployeeTerritory
															.WithSubPath(EmployeeEntity.PrefetchPathOrders,
																		 EmployeeEntity.PrefetchPathCustomerCollectionViaOrder
																							.WithSubPath(CustomerEntity.PrefetchPathOrders)));
				var territories = adapter.FetchQuery(q);
				int count = 0;
				foreach(TerritoryEntity v in territories)
				{
					count++;
					if(new string[] { "29202", "72716", "75234", "78759" }.Contains(v.TerritoryId))
					{
						// territory has no employees
						continue;
					}
					Assert.IsTrue(v.EmployeeCollectionViaEmployeeTerritory.Count > 0);
					foreach(EmployeeEntity e in v.EmployeeCollectionViaEmployeeTerritory)
					{
						Assert.IsTrue(e.Orders.Count > 0);
						Assert.IsTrue(e.CustomerCollectionViaOrder.Count>0);
						foreach(CustomerEntity c in e.CustomerCollectionViaOrder)
						{
							if(!(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId)))
							{
								Assert.IsTrue(c.Orders.Count>0);
							}
						}
					}
				}
				Assert.AreEqual(53, count);
			}
		}


		[Test]
		public void GetCustomersWithRelatedOrdersAndExcludeFieldsInOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList(true);
				//excludedFields.Add(OrderFields.OrderDate);
				//excludedFields.Add(OrderFields.ShipName);

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where c.Country == "USA"
				//         select c).WithPath(
				//            new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null, null, 0, excludedFields));

				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "USA")
								.WithPath(CustomerEntity.PrefetchPathOrders
															.WithExclusions(OrderFields.OrderDate, OrderFields.ShipName));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
					foreach(OrderEntity o in v.Orders)
					{
						Assert.IsNull(o.OrderDate);
						Assert.AreEqual(string.Empty, o.ShipName);
					}
				}
				Assert.AreEqual(13, count);
			}
		}

	
		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithC()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from o in metaData.Order
				//         where o.EmployeeId == 3
				//         select o).WithPath(new PathEdge<CustomerEntity>(OrderEntity.PrefetchPathCustomer, c => c.CustomerId.StartsWith("C")));
				var qf = new QueryFactory();
				var q = qf.Order
							.Where(OrderFields.EmployeeId == 3)
							.WithPath(OrderEntity.PrefetchPathCustomer
														.WithFilter(CustomerFields.CustomerId.StartsWith("C")));
				var orders = adapter.FetchQuery(q);
				int count = 0;
				foreach(OrderEntity v in orders)
				{
					count++;
					if(v.CustomerId.StartsWith("C"))
					{
						Assert.IsNotNull(v.Customer);
						Assert.AreEqual(v.CustomerId, v.Customer.CustomerId);
					}
					else
					{
						Assert.IsNull(v.Customer);
					}
				}
				Assert.AreEqual(115, count);
			}
		}
		#endregion

		#region PrefetchPath tests using LambdaExpression api developed by Jeremy 'Jez' Skinner
		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(p=>p.Prefetch(c=>c.Orders));

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
							.WithPath(CustomerEntity.PrefetchPathOrders);
				var customers = adapter.FetchQuery(q);

				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(89, count);
			}
		}

		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersAndEmployeesUsingContextJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Context contextToUse = new Context();

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//metaData.ContextToUse = contextToUse;
				// use a query which fetches Employees through orders into the customers and employee through order.
				// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
				//var q = (from c in metaData.Customer
				//         where c.Country == "Germany"
				//         select c).WithPath(customerPath => customerPath
				//                    .Prefetch<OrderEntity>(c => c.Orders).SubPath(orderPath => orderPath.Prefetch<EmployeeEntity>(o => o.Employee))
				//                    .Prefetch<EmployeeEntity>(c => c.EmployeeCollectionViaOrder));
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.Country == "Germany")
								.WithPath(CustomerEntity.PrefetchPathOrders.WithSubPath(OrderEntity.PrefetchPathEmployee),
										  CustomerEntity.PrefetchPathEmployeeCollectionViaOrder);
				q.ContextToUse = contextToUse;
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					// use object instance compare with linear search, otherwise we could get false positives as Equals and GetHashCode are tied to
					// the PK values of the entities, which obviously are equal.
					Assert.IsTrue(v.Orders.Count > 0);
					foreach(OrderEntity o in v.Orders)
					{
						// traverse the employees in v.Employees and do an object instance compare. Has to be there
						bool found = false;
						foreach(EmployeeEntity e in v.EmployeeCollectionViaOrder)
						{
							if((object)e == (object)o.Employee)
							{
								found = true;
								break;
							}
						}
						Assert.IsTrue(found);
					}
				}
				Assert.AreEqual(11, count);
			}
		}

		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersWhichAreHandledByEmployee3Jez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         select c).WithPath(customerPath => customerPath
				//                .Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 3));

				var qf = new QueryFactory();
				var q = qf.Customer
							.WithPath(CustomerEntity.PrefetchPathOrders.WithFilter(OrderFields.EmployeeId == 3));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.AreEqual(3, o.EmployeeId);
						}
					}
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetAllCustomersFromGermanyWithTheirOrdersAndRelatedEmployeeAndOrderDetailsWithExcludedFieldsJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where c.Country == "Germany"
				//         select c).WithPath<CustomerEntity>(cpath => cpath
				//                .Prefetch<OrderEntity>(c => c.Orders)
				//                    .SubPath(opath=>opath
				//                        .Prefetch(o=>o.OrderDetails)
				//                        .Prefetch<EmployeeEntity>(o => o.Employee).Exclude(e => e.Photo, e => e.Notes)));

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.Country == "Germany")
							.WithPath(CustomerEntity.PrefetchPathOrders
														.WithSubPath(OrderEntity.PrefetchPathOrderDetails,
																	 OrderEntity.PrefetchPathEmployee.WithExclusions(EmployeeFields.Photo, EmployeeFields.Notes)));

				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
					foreach(OrderEntity o in v.Orders)
					{
						Assert.IsNotNull(o.Employee);
						Assert.IsTrue(o.OrderDetails.Count > 0);
					}
				}
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 1;
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(customerPath => customerPath
				//                .Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2));
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
								.WithPath(CustomerEntity.PrefetchPathOrders
															.WithOrdering(OrderFields.OrderDate.Descending())
															.WithLimit(2));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue((v.Orders.Count > 0) && (v.Orders.Count <= 2));
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersFiledByEmployee2Jez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(customerPath => customerPath
				//                .Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 2).OrderByDescending(o => o.OrderDate).LimitTo(2));

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
							.WithPath(CustomerEntity.PrefetchPathOrders
															.WithFilter(OrderFields.EmployeeId == 2)
															.WithOrdering(OrderFields.OrderDate.Ascending())
															.WithLimit(2));

				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue((v.Orders.Count >= 0) && (v.Orders.Count <= 2));
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.AreEqual(2, o.EmployeeId);
						}
					}
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersWithSubPathsJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
				//         select c).WithPath(customerPath => customerPath
				//                    .Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2)
				//                        .SubPath(orderPath => orderPath
				//                                    .Prefetch(o => o.OrderDetails)
				//                                    .Prefetch(o => o.Employee)));

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId.NotIn(new string[] { "FISSA", "PARIS" }))
							.WithPath(CustomerEntity.PrefetchPathOrders
														.WithOrdering(OrderFields.OrderDate.Ascending())
														.WithLimit(2)
														.WithSubPath(OrderEntity.PrefetchPathOrderDetails,
																	 OrderEntity.PrefetchPathEmployee));
				var customers = adapter.FetchQuery(q);
				int count = 0;
				foreach(CustomerEntity v in customers)
				{
					count++;
					Assert.IsTrue((v.Orders.Count > 0) && (v.Orders.Count <= 2));
					if(v.Orders.Count > 0)
					{
						foreach(OrderEntity o in v.Orders)
						{
							Assert.IsTrue(o.OrderDetails.Count > 0);
							Assert.IsNotNull(o.Employee);
						}
					}
				}
				Assert.AreEqual(89, count);
			}
		}


		/// <summary>
		/// Fetches a multi-branch prefetch path: territory m:n employee 1:n Order and employee m:n customer 1:n order.
		/// </summary>
		[Test]
		public void GetTerritoriesWithEmployeesAndCustomersOrdersInComplexPathJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from t in metaData.Territory select t)
				//            .WithPath(territoryPath => territoryPath
				//                .Prefetch<EmployeeEntity>(t => t.EmployeeCollectionViaEmployeeTerritory)
				//                    .SubPath(employeePath => employeePath
				//                                .Prefetch(e => e.Orders)
				//                                .Prefetch<CustomerEntity>(e => e.CustomerCollectionViaOrder)
				//                                    .SubPath(customerPath => customerPath.Prefetch(c => c.Orders))));

				var qf = new QueryFactory();
				var q = qf.Territory
								.WithPath(TerritoryEntity.PrefetchPathEmployeeCollectionViaEmployeeTerritory
															.WithSubPath(EmployeeEntity.PrefetchPathOrders,
																		 EmployeeEntity.PrefetchPathCustomerCollectionViaOrder
																							.WithSubPath(CustomerEntity.PrefetchPathOrders)));
				var territories = adapter.FetchQuery(q);
				int count = 0;
				foreach(TerritoryEntity v in territories)
				{
					count++;
					if(new string[] { "29202", "72716", "75234", "78759" }.Contains(v.TerritoryId))
					{
						// territory has no employees
						continue;
					}
					Assert.IsTrue(v.EmployeeCollectionViaEmployeeTerritory.Count > 0);
					foreach(EmployeeEntity e in v.EmployeeCollectionViaEmployeeTerritory)
					{
						Assert.IsTrue(e.Orders.Count > 0);
						Assert.IsTrue(e.CustomerCollectionViaOrder.Count > 0);
						foreach(CustomerEntity c in e.CustomerCollectionViaOrder)
						{
							if(!(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId)))
							{
								Assert.IsTrue(c.Orders.Count > 0);
							}
						}
					}
				}
				Assert.AreEqual(53, count);
			}
		}


		[Test]
		public void GetSingleCustomerWithRelatedOrdersJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         select c).WithPath(customerPath => customerPath
				//             .Prefetch<OrderEntity>(c => c.Orders))
				//             .Single(c=>c.CustomerId=="CHOPS");
				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId == "CHOPS")
							.WithPath(CustomerEntity.PrefetchPathOrders);
				var customer = adapter.FetchSingle(q);
				
				Assert.AreEqual("CHOPS", customer.CustomerId);
				Assert.IsTrue(customer.Orders.Count > 0);
			}
		}

		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithCJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from o in metaData.Order
				//         where o.EmployeeId == 3
				//         select o).WithPath(orderPath => orderPath
				//             .Prefetch<CustomerEntity>(o => o.Customer).FilterOn(c => c.CustomerId.StartsWith("C")));

				var qf = new QueryFactory();
				var q = qf.Order
							.Where(OrderFields.EmployeeId == 3)
							.WithPath(OrderEntity.PrefetchPathCustomer
														.WithFilter(CustomerFields.CustomerId.StartsWith("C")));
				var orders = adapter.FetchQuery(q);
				int count = 0;
				foreach(OrderEntity v in orders)
				{
					count++;
					if(v.CustomerId.StartsWith("C"))
					{
						Assert.IsNotNull(v.Customer);
						Assert.AreEqual(v.CustomerId, v.Customer.CustomerId);
					}
					else
					{
						Assert.IsNull(v.Customer);
					}
				}
				Assert.AreEqual(115, count);
			}
		}


		[Test]
		public void GetOrderDetailsWithOrderSortedOnCustomerRelatedToOrder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from od in metaData.OrderDetail
				//         where od.ProductId > 10
				//         select od).WithPath(p => p.Prefetch<OrderEntity>(od => od.Order).OrderBy(o => o.Customer.Country));
				var qf = new QueryFactory();
				var q = qf.OrderDetail
								.Where(OrderDetailFields.ProductId > 10)
								.WithPath(OrderDetailEntity.PrefetchPathOrder
																.WithJoins(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
																.WithOrdering(CustomerFields.Country.Ascending()));
				var orderDetails = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in orderDetails)
				{
					count++;
				}
				Assert.AreEqual(1806, count);
			}
		}


		[Test]
		public void GetCustomerWithRelatedOrdersSortedByFieldAndLimit()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from c in metaData.Customer
				//         where c.CustomerId == "ALFKI"
				//         select c)
				//             .WithPath(customerPath =>
				//                 customerPath.Prefetch<OrderEntity>(c => c.Orders).OrderBy(o => o.Freight).LimitTo(2));
				var qf = new QueryFactory();
				var q = qf.Customer
								.Where(CustomerFields.CustomerId == "ALFKI")
								.WithPath(CustomerEntity.PrefetchPathOrders
															.WithOrdering(OrderFields.Freight.Ascending())
															.WithLimit(2));
				var customers = adapter.FetchQuery(q);
				foreach(CustomerEntity v in customers)
				{
					Assert.IsTrue(v.Orders.Count <= 2);
					Assert.AreEqual("ALFKI", v.CustomerId);
				}
			}
		}
		#endregion


		[TestFixtureSetUp]
		public void SetupTests()
		{
		}


		[TestFixtureTearDown]
		public void SetupTeardown()
		{
			Thread.Sleep(1000);
		}

	}
}
