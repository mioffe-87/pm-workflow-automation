using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NW26.SelfServicing;
using NW26.SelfServicing.CollectionClasses;
using NW26.SelfServicing.EntityClasses;
using NW26.SelfServicing.FactoryClasses;
using NW26.SelfServicing.HelperClasses;
using NW26.SelfServicing.Linq;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.TestTypeConverters;
using SD.Tools.OrmProfiler.Interceptor;


namespace LinqTester.SelfServicingTests
{
	/// <summary>
	/// Unit tests for entity oriented fetches, into entity collections or lists of entities.
	/// </summary>
	[TestFixture]
	public class EntityFetches
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}

		/// <summary>
		/// Simple test which executes a select directly on a table. 
		/// </summary>
		[Test]
		public void GetCustomersWithoutAnyFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Customer;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(91, customers.Count);
		}

		/// <summary>
		/// Simple test which executes Distinct() directly on a table. 
		/// </summary>
		[Test]
		public void GetDistinctCustomersWithoutAnyFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Customer.Distinct();

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(91, customers.Count);
		}


		/// <summary>
		/// Gets the first customer from ordered list.
		/// </summary>
		[Test]
		public void GetFirstCustomerFromOrderedList()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					orderby c.Country ascending, c.City ascending
					select c).First();

			Assert.AreEqual("COMMI", q.CustomerId);
		}
		

		/// <summary>
		/// Gets the first customer from ordered list.
		/// </summary>
		[Test]
		public void GetFirstCustomerFromOrderedListWithFilterOnFirst()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 orderby c.Country ascending, c.City ascending
					 select c).First(c=>c.Country=="USA");

			Assert.AreEqual("RATTC", q.CustomerId);
		}


		/// <summary>
		/// Method will fail, because Single is called on a multi-element set.
		/// </summary>
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void GetSingleCustomerFromOrderedList_FailsOnPurpose()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 orderby c.Country ascending, c.City ascending
					 select c).Single();
		}


		[Test]
		public void GetSingleCustomerFromListUsingSingleWithFilterOnCustomerId()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c).Single(c=>c.CustomerId=="CHOPS");

			Assert.AreEqual("CHOPS", q.CustomerId);
		}


		/// <summary>
		/// Gets a page of entities from a set without any filter.
		/// </summary>
		[Test]
		public void GetPageOfEntitiesFromSetWithoutFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Customer.TakePage(2, 10);

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(10, customers.Count);
		}


		[Test]
		public void GetPageOfEntitiesFromSetWithoutFilterUsingSkipTake()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Customer.Skip(10).Take(10);

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(10, customers.Count);
		}


		/// <summary>
		/// Gets a page of entities from a set with a filter.
		/// </summary>
		[Test]
		public void GetPageOfEntitiesFromSetWithFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (
						from c in metaData.Customer
						where c.Country == "Germany"
						select c
					).TakePage(2, 3);

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(3, customers.Count);
		}


		[Test]
		public void GetPageOfEntitiesFromSetWithFilterUsingSkipTake()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (
						from c in metaData.Customer
						where c.Country == "Germany"
						select c
					).Skip(3).Take(3);

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(3, customers.Count);
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) without any filter or other fancy things.
		/// </summary>
		[Test]
		public void GetEntitySetWithoutAnyFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					select c;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(91, customers.Count);
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) with a single field filter
		/// </summary>
		[Test]
		public void GetEntitySetWithSingleFieldFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Country=="Germany"
					select c;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(11, customers.Count);
		}


		[Test]
		public void GetEntitySetWithSingleFieldFilterWithEqualsTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Country.Equals("Germany")
					select c;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(11, customers.Count);
		}


		[Test]
		public void GetCustomerWithEquationOperatorOnEntireEntity()
		{
			CustomerEntity chopsEntity = new CustomerEntity("CHOPS");

			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c == chopsEntity
					select c;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(1, customers.Count);
			Assert.AreEqual("CHOPS", customers[0].CustomerId);
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) with a multi-predicate filter
		/// </summary>
		[Test]
		public void GetEntitySetWithMultiPredicateFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Country == "USA" && c.Region!="OR"
					select c;

			int count = 0;
			foreach(CustomerEntity c in q)
			{
				count++;
				Assert.AreEqual("USA", c.Country);
				Assert.AreNotEqual("OR", c.Region);
			}
			Assert.AreEqual(9, count);
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all customers in the USA).
		/// </summary>
		[Test]
		public void GetEntitySetWithMaxLimitAndFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
								from c in metaData.Customer
								where c.Country == "USA"
								select c).Take(5);
			CustomerCollection customers = q.Execute<CustomerCollection>();
			Assert.AreEqual(5, customers.Count);
		}


		/// <summary>
		/// Test which fetches customers based on a filter on order.EmployeeId. Simple test for joins
		/// </summary>
		[Test]
		public void GetEntitySetWithFilterOnRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
					from c in metaData.Customer
					join o in metaData.Order on c.CustomerId equals o.CustomerId
					where o.EmployeeId == 6
					select c);
			CustomerCollection customers = (CustomerCollection)q.Execute();
			Assert.AreEqual(55, customers.Count);
		}


		/// <summary>
		/// Test which fetches Employees based on a filter on a join with self and a filter on the joined related entity.
		/// </summary>
		[Test]
		public void GetEntitySetWithFilterOnRelatedEntityWithJoinToSelf()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
					from e in metaData.Employee
					join m in metaData.Employee on e.ReportsTo equals m.EmployeeId
					where m.FirstName == "Andrew"
					select e);
			EmployeeCollection employees = (EmployeeCollection)q.Execute();
			Assert.AreEqual(3, employees.Count);
		}


		/// <summary>
		/// Gets the customers filtered on derived table build from related set from orders, using a join between customers and the derived table
		/// </summary>
		[Test]
		public void GetCustomersFilteredOnDerivedTableBuildFromRelatedSetUsingJoin()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from c in metaData.Customer
						join x in
						(from o in metaData.Order where o.EmployeeId > 3 select new { CustomerId = o.CustomerId, EmployeeId = o.EmployeeId })
							on c.CustomerId equals x.CustomerId
						where x.EmployeeId == 6
						select c);
			CustomerCollection customers = (CustomerCollection)q.Execute();
			Assert.AreEqual(55, customers.Count);
		}


		/// <summary>
		/// Gets orders with a filter on self and with a cross join + normal join.
		/// </summary>
		[Test]
		public void GetOrdersWithCrossJoinWithRelatedEntitiesAndFilterOnSelf()
		{
			LinqMetaData metaData = new LinqMetaData();

			ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from o in metaData.Order
						from p in metaData.Product
						join od in metaData.OrderDetail on
						new { o.OrderId, p.ProductId } equals new { od.OrderId, od.ProductId }
						where o.EmployeeId == 2
						select o);
			OrderCollection orders = (OrderCollection)q.Execute();
			Assert.AreEqual(110, orders.Count);
		}


		/// <summary>
		/// Test which returns customers who have bought product 2 by using just cross joins.
		/// </summary>
		[Test]
		public void GetCustomersWithFilterOnBoughtProductUsingSolelyCrossJoins()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
					from c in metaData.Customer
					from o in metaData.Order
					from od in metaData.OrderDetail
					where od.ProductId == 2
					select c);

			CustomerCollection customers = (CustomerCollection)q.Execute();
			// fetches all 91 customers, though the test is to see if there's valid SQL generated
			Assert.AreEqual(91, customers.Count);
		}


		/// <summary>
		/// Gets the customers from germany who have an order filed by employee 2 where the customer where is in an unexpected place.
		/// </summary>
		[Test]
		public void GetCustomersFromGermanyWithOrderFilteredOnEmployeeUsingMisplacedWhereAndCrossJoin()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
					from c in metaData.Customer
					where c.Country == "Germany"
					from o in metaData.Order
					where o.EmployeeId == 2
					select c);

			CustomerCollection customers = (CustomerCollection)q.Execute();
			Assert.AreEqual(11, customers.Count);
		}


		/// <summary>
		/// Test which uses DefaultIfEmpty and 'into' (so GroupJoin + SelectMany + DefaultIfEmpty) to fetch all customers which have no orders from France
		/// </summary>
		[Test]
		public void LeftJoinUsingDefaultIfEmptyToFetchCustomersWithoutAnOrderFromFrance()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					join o in metaData.Order on c.CustomerId equals o.CustomerId into oc
					where c.Country=="France"
					from x in oc.DefaultIfEmpty()
					where x.OrderId == null
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(1, counter);
		}


		/// <summary>
		/// Test which uses DefaultIfEmpty and 'into' (so GroupJoin + SelectMany + DefaultIfEmpty) to fetch all customers which have no orders
		/// </summary>
		[Test]
		public void LeftJoinUsingDefaultIfEmptyToFetchCustomersWithoutAnOrder()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					join o in metaData.Order on c.CustomerId equals o.CustomerId into oc
					from x in oc.DefaultIfEmpty()
					where x.OrderId == null
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(2, counter);
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					where o.Customer.Country == "Germany"
					select o;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(121, counter);
		}


		/// <summary>
		/// Test which fetches all order details of orders of all customers from spain using the 'Customer' property in order to use an implicit inner join. 
		/// this test is an extention of GetAllOrdersOfCustomersInGermanyWithFilterOnRelatedEntityUsingFieldOnRelationProperty's query. The main painpoint with
		/// this query is that it has multiple levels of related entities. 
		/// </summary>
		[Test]
		public void GetAllOrderDetailsOfOrdersOfCustomersInSpainWithFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from od	in metaData.OrderDetail
					where od.Order.Customer.Country == "Spain"
					select od;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(51, counter);
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// It has a duplicate filter so the relation is added twice. The test checks if the code is able to backtrack to the proper aliases.
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithDuplicateFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					where o.Customer.Country == "Germany"
					&& o.Customer.ContactTitle =="Sales Representative"
					select o;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(38, counter);
		}


		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 10 orders. This is done by applying a count on the Orders set.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan10Orders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Count() > 10
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(27, counter);
		}


		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees with id bigger than 6. 
		/// This is done by applying a count on the Orders set with a filter in the Count as argument.
		/// Primary purpose of the test is to see if the code can deal with a filter in a count aggregate.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesWithAnEmployeeIdBiggerThan6()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Count(o=>o.EmployeeId>6) > 6
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(3, counter);
		}


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK 
		/// This is done by applying a count on the Orders set with a filter in the Count as argument.
		/// Primary purpose of the test is to see if the code can deal with a filter in a count aggregate.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesFromTheUK()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Count(o => o.Employee.Country=="UK") > 6
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(4, counter);
		}


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK. 
		/// The difference here is that it uses a Where instead of a filter in the Count.
		/// This is done by applying a count on the Orders 
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan6OrdersFiledByEmployeesFromTheUKWithCountOnWhereResult()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Where(o => o.Employee.Country == "UK").Count() > 6
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(4, counter);
		}


		/// <summary>
		/// Gets all customers which have worked with more than4 employees for order filing. This is a M:N test with aggregates.
		/// </summary>
		/// <remarks>Bogus test, as it implies a join with 1:n related entities over which the count is executed.</remarks>
		[Test]
		public void GetAllCustomersWhichHaveWorkedWithMoreThan4EmployeesForOrderFiling()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.EmployeeCollectionViaOrder.Count() > 6
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(54, counter);
		}
		

		/// <summary>
		/// Gets all categories which have more than 500 units overall in stock. This is a test which checks if Sum() aggregates over single fields work.
		/// </summary>
		[Test]
		public void GetAllCategoriesWhichHaveMoreThan500UnitsOverallInStock()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Category
					where c.Products.Sum(p=>p.UnitsInStock) > 500
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(2, counter);
		}


		/// <summary>
		/// Test which gets all customers with an order total higher than 5000. This test is to show that nested aggregates work.
		/// </summary>
		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan5000()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)) > 5000
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(54, counter);
		}


		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan1000ForOrdersWithProductsWithAnIdBiggerThan40()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Sum(o => o.OrderDetails.Where(od => od.ProductId > 40).Sum(od => od.Quantity * od.UnitPrice)) > 1000
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(68, counter);
		}


		/// <summary>
		/// Gets all categories which have more than500 units overall in stock sorted by that sum ascending and then by category name descending
		/// </summary>
		[Test]
		public void GetAllCategoriesWhichHaveMoreThan500UnitsOverallInStockSortedBySumDescendingThenCategoryNameAscending()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from c in metaData.Category
						let x = c.Products.Select(p => (int)p.UnitsInStock).Sum()
						where x > 500
						orderby x ascending, c.CategoryName descending
						select c);

			CategoryCollection categories = (CategoryCollection)q.Execute();
			Assert.AreEqual(2, categories.Count);
			Assert.AreEqual(2, categories[0].CategoryId);
			Assert.AreEqual(8, categories[1].CategoryId);
		}


		/// <summary>
		/// Gets all orders from customers from UK sorted by customer.city descending.
		/// </summary>
		[Test]
		public void GetAllOrdersFromCustomersFromUKSortedByCityDescending()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from o in metaData.Order
						where o.Customer.Country == "UK"
						orderby o.Customer.City descending
						select o
						);

			OrderCollection orders = (OrderCollection)q.Execute();
			Assert.AreEqual(56, orders.Count);
			Assert.AreEqual("SEVES", orders[0].CustomerId);
		}


		[Test]
		public void GetAllOrdersFromCustomersFromGermanyUsingJoin()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					join c in metaData.Customer on o.CustomerId equals c.CustomerId
					join od in metaData.OrderDetail on o.OrderId equals od.OrderId
					where o.Customer.Country == "Germany"
					select o;

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(121, count);
		}


		/// <summary>
		/// Gets the customers ordered by country then by city using solely extension methods.
		/// </summary>
		[Test]
		public void GetCustomersOrderedByCountryThenByCityUsingSolelyExtensionMethods()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(metaData.Customer.OrderBy(c=>c.Country).ThenByDescending(c=>c.City));

			CustomerCollection customers = (CustomerCollection)q.Execute();
			Assert.AreEqual(91, customers.Count);
			Assert.AreEqual("COMMI", customers[0].CustomerId);
		}


		/// <summary>
		/// Gets all customers not from germany using a not filter. This tests the ability to use 'not' before a binary expression.
		/// </summary>
		[Test]
		public void GetAllCustomersNotFromGermanyUsingANotFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where !(c.Country=="Germany")
					select c;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(79, counter);
		}


		/// <summary>
		/// Gets the regions which have only employees which have titleofcourtesy set to 'Ms.'. This is a test for the All() extension method.
		/// </summary>
		[Test]
		public void GetAllRegionsWhichHaveOnlyMsEmployees()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from r in metaData.Region
					where r.Employees.All(e => e.TitleOfCourtesy == "Ms.")
					select r;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(2, counter);
		}


		/// <summary>
		/// Gets all orders which have purchased product with id 2. This is a test for the Any() extension method.
		/// </summary>
		[Test]
		public void GetAllOrdersWhichHavePurchasedProduct2()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					where o.OrderDetails.Any(od=>od.ProductId==2)
					select o;

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(43, counter);
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection. This is a simple test with an entity as argument.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimple()
		{
			LinqMetaData metaData = new LinqMetaData();
			OrderEntity order = new OrderEntity();
			order.OrderId = 10254;

			var q = from c in metaData.Customer
					where c.Orders.Contains(order)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("CHOPS", results[0].CustomerId);
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection simple with where on source. Another rather simple Contains test on a collection
		/// with a filter, so the source isn't an entity but a query.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimpleWithWhereOnSource()
		{
			LinqMetaData metaData = new LinqMetaData();
			OrderEntity order = new OrderEntity();
			order.OrderId = 10254;

			var q = from c in metaData.Customer
					where c.Orders.Where(o=>o.EmployeeId==3).Contains(order)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("CHOPS", results[0].CustomerId);
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection with query as operand. This is a tricky situation, where the operand of the 
		/// Contains is a query result. The result has to be obtained during the main query, not separately.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionWithQueryAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();
			OrderEntity order = new OrderEntity();
			order.OrderId = 10254;

			var q = from c in metaData.Customer
					where c.Orders.Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("BLONP", results[0].CustomerId);
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection with query as operand and query on source.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionWithQueryAsOperandAndQueryOnSource()
		{
			LinqMetaData metaData = new LinqMetaData();
			OrderEntity order = new OrderEntity();
			order.OrderId = 10254;

			var q = from c in metaData.Customer
					where c.Orders.Where(o => o.EmployeeId == 2).Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("BLONP", results[0].CustomerId);
		}


		/// <summary>
		/// Simple query which tests the ability of the Contains handler to handle projections on an entity set as source and a constant value as operand.
		/// The query obtains all customers which have orders handled by an employee with id > 3 and which is shipped via shipper 2
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnOrdersCollectionWithSingleFieldProjectionAndConstantOperand()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Where(o => o.EmployeeId > 3).Select(o => o.ShipVia).Contains(2)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(75, results.Count);
		}


		/// <summary>
		/// Gets the customer based on contains call on multi value projection with multi value object as operand.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnMultiValueProjectionWithMultiValueObjectAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
						new { EID = (int?)1, CID = "CHOPS" })
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("CHOPS", results[0].CustomerId);
		}


		/// <summary>
		/// Gets the customer based on contains call on single value projection with single value query as operand.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnSingleValueProjectionWithSingleValueQueryAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where c.Orders.Select(oc => oc.EmployeeId).Contains((from o in metaData.Order where o.CustomerId == "CHOPS" select o.EmployeeId).First())
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(64, results.Count);
		}


		/// <summary>
		/// Gets the customer based on contains call on multi value projection with multi value query as operand.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnMultiValueProjectionWithMultiValueQueryAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
				where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
					(from o in metaData.Order where o.CustomerId == "CHOPS" select new { EID = o.EmployeeId, CID = o.CustomerId }).First())
				select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("CHOPS", results[0].CustomerId);
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized list.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedList()
		{
			LinqMetaData metaData = new LinqMetaData();

			List<string> countries = new List<string>() { "USA", "UK" };
			var q = from c in metaData.Customer
					where countries.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// Similar to GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedList, but now the list is created inside the query.
		/// This tests the ListInit expression materialization.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedList()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where new List<string>() { "USA", "UK" }.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// similar to GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedList but now also uses a CTor call with the initialization
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInPlaceCreatedListAndListCTor()
		{
			LinqMetaData metaData = new LinqMetaData();

			string[] values = new string[] { "UK" };
			var q = from c in metaData.Customer
					where new List<string>(values) { "USA"}.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized enumerable. A linked list is used as the enumerable.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedEnumerable()
		{
			LinqMetaData metaData = new LinqMetaData();

			LinkedList<string> countries = new LinkedList<string>(new string[] { "USA", "UK"});
			var q = from c in metaData.Customer
					where countries.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized array.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedArray()
		{
			LinqMetaData metaData = new LinqMetaData();

			string[] countries = new string[] { "USA", "UK" };
			var q = from c in metaData.Customer
					where countries.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// Test which checks if the provider can combine a query inside another query.
		/// </summary>
		[Test]
		public void GetAllCustomersBasedOnCombinedQuery()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q2 = (from c in metaData.Customer
					  select c.Country).Distinct();

			var q = from c in metaData.Customer
					where q2.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(90, results.Count);
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized array indexed from other array.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedArrayIndexedFromOtherArray()
		{
			LinqMetaData metaData = new LinqMetaData();

			string[][] countries = new string[1][] { new string[] { "USA", "UK" } };
			var q = from c in metaData.Customer
					where countries[0].Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryInPlaceCreatedArray()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where new string[] { "USA", "UK" }.Contains(c.Country)
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(20, results.Count);
		}


		/// <summary>
		/// Uses two queries, first one to obtain the entities to compare against, then the actual query which filters on this set of entities.
		/// This version tests the single PK variant.
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfEntitiesSingleFieldPk()
		{
			// use transaction so connection is shared among the two queries.
			using(Transaction trans = new Transaction(System.Data.IsolationLevel.ReadCommitted, "Test"))
			{
				LinqMetaData metaData = new LinqMetaData(trans);

				var q2 = from c in metaData.Customer where c.Country == "USA" select c;
				List<CustomerEntity> customers = q2.ToList();

				var q = from c in metaData.Customer
						where customers.Contains(c)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(13, results.Count);

				// no changes, just commit
				trans.Commit();
			}
		}


		/// <summary>
		/// Uses two queries, first one to obtain the entities to compare against, then the actual query which filters on this set of entities.
		/// This version tests the compound PK variant.
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfEntitiesMultiFieldPk()
		{
			// use transaction so connection is shared among the two queries.
			using(Transaction trans = new Transaction(System.Data.IsolationLevel.ReadCommitted, "Test"))
			{
				LinqMetaData metaData = new LinqMetaData(trans);

				var q2 = from od in metaData.OrderDetail where od.ProductId < 5 select od;
				List<OrderDetailEntity> orderDetails = q2.ToList();

				var q = from od in metaData.OrderDetail
						where orderDetails.Contains(od)
						select od;

				List<OrderDetailEntity> results = q.ToList();
				Assert.AreEqual(111, results.Count);

				trans.Commit();
			}
		}


		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfValuesUsingQueryWithFieldAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();

			List<int?> employeeIds = new List<int?>() { 1, 2 };

			var q = from c in metaData.Customer
					where employeeIds.Contains((from o in metaData.Order where o.CustomerId == c.CustomerId select o.EmployeeId).First())
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(80, results.Count);
		}


		/// <summary>
		/// Complicated test which uses a compare between a set of tuples in memory with a set of tuples obtained from a table in the database. Matching rows
		/// result in the retrieval of that customer. 
		/// </summary>
		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfTuplesUsingQueryWithProjectionOnCustomClassAsOperand()
		{
			LinqMetaData metaData = new LinqMetaData();

			// tuple list
			List<Pair<string, string>> countryCities = new List<Pair<string, string>>();
			countryCities.Add(new Pair<string, string>("USA", "Portland"));
			countryCities.Add(new Pair<string, string>("Brazil", "Sao Paulo"));

			// now fetch all customers which have a tuple of country/city in the list of countryCities.
			var q = from c in metaData.Customer
					where countryCities.Contains((from c2 in metaData.Customer
										  where c2.CustomerId == c.CustomerId
										  select new Pair<string, string>() { Value1 = c2.Country, Value2 = c2.City }).First())
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(5, results.Count);
		}


		/// <summary>
		/// Gets the customers based on contains call on string field. This results in a LIKE query.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnStringField()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where c.Country.Contains("land")
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(6, results.Count);
		}


		/// <summary>
		/// Gets the orders based on contains call on string field of related entity. 
		/// </summary>
		[Test]
		public void GetOrdersBasedOnContainsCallOnStringFieldOfRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from o in metaData.Order
					where o.Customer.Country.Contains("land")
					select o;

			List<OrderEntity> results = q.ToList();
			Assert.AreEqual(66, results.Count);
		}

		
		/// <summary>
		/// Gets the orders based on contains call on string field producing query of related entity. Uses a query which produces a string as source for Contains.
		/// 'Contains' is here the string.Contains extension method. If 'First()' is removed from the query, 'Contains' is the IEnumerable.Contains() extension method.
		/// </summary>
		[Test]
		public void GetOrdersBasedOnContainsCallOnStringFieldProducingQueryOfRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from o in metaData.Order
					where (from c in metaData.Customer where c.CustomerId==o.CustomerId select c.Country).First().Contains("land")
					select o;

			List<OrderEntity> results = q.ToList();
			Assert.AreEqual(66, results.Count);
		}


		/// <summary>
		/// Gets the customers based on starts with call on string field.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnStartsWithCallOnStringField()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where c.Country.StartsWith("A")
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(5, results.Count);
		}


		[Test]
		public void GetCustomersBasedOnEndsWithCallOnStringField()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					where c.Country.EndsWith("A")
					select c;

			List<CustomerEntity> results = q.ToList();
			Assert.AreEqual(25, results.Count);
		}


		/// <summary>
		/// Gets the 14th element in a sorted list of customers using ElementAt. 
		/// </summary>
		[Test]
		public void Get14thElementInSortedListOfCustomersUsingElementAt()
		{
			LinqMetaData metaData = new LinqMetaData();

			// get 14th element, which is element with index 13.
			var q = (from c in metaData.Customer
					orderby c.Country ascending, c.City descending
					select c).ElementAt(13);

			CustomerEntity customer = q;
			Assert.AreEqual("RICAR", customer.CustomerId);
		}


		/// <summary>
		/// Gets the employees from UK using Except filtering.
		/// </summary>
		[Test]
		public void GetEmployeesFromUKUsingExceptFiltering()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from e in
						metaData.Employee.Except(from e in metaData.Employee where e.Country == "USA" select e)
					select e;

			List<EmployeeEntity> employeesFromUK = q.ToList();
			Assert.AreEqual(4, employeesFromUK.Count);
		}


		/// <summary>
		/// Gets the employees from UK using except filtering without projection.
		/// </summary>
		[Test]
		public void GetEmployeesFromUKUsingExceptFilteringWithoutProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Employee.Except(from e in metaData.Employee where e.Country == "USA" select e);

			List<EmployeeEntity> employeesFromUK = q.ToList();
			Assert.AreEqual(4, employeesFromUK.Count);
		}


		/// <summary>
		/// Test which focuses on Except on an entity collection combined with an aggregate
		/// </summary>
		[Test]
		public void GetAllCustomersWhoHaveMoreThan5OrdersFiledByEmployeesOtherThanEmployee3()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Orders.Except(from o in metaData.Order where o.EmployeeId == 3 select o).Count() > 5
					select c;

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(56, customers.Count);
		}


		[Test]
		public void GetAllCustomersWhoHaveMoreThan5OrdersFiledByEmployeesOtherThanEmployee3UsingInMemoryList()
		{
			// use transaction so connection is shared among the two queries.
			using(Transaction trans = new Transaction(System.Data.IsolationLevel.ReadCommitted, "Test"))
			{
				LinqMetaData metaData = new LinqMetaData(trans);

				var q2 = from o in metaData.Order where o.EmployeeId == 3 select o;
				List<OrderEntity> ordersToExclude = q2.ToList();

				var q = from c in metaData.Customer
						where c.Orders.Except(ordersToExclude).Count() > 5
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(56, customers.Count);

				trans.Commit();
			}
		}


		/// <summary>
		/// Gets all customers who are from the USA and are owner using intersect.
		/// </summary>
		[Test]
		public void GetAllCustomersWhoAreFromTheUSAAndAreOwnerUsingIntersect()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = (from c in metaData.Customer
					 where c.Country == "USA"
					 select c).Intersect(from c in metaData.Customer where c.ContactTitle == "Owner" select c);

			List<CustomerEntity> customers = q.ToList();
			Assert.AreEqual(2, customers.Count);
		}


		[Test]
		public void GetAllCustomersWhoAreFromTheUSAAndAreOwnerUsingIntersectWithInMemorySet()
		{
			// use transaction so connection is shared among the two queries.
			using(Transaction trans = new Transaction(System.Data.IsolationLevel.ReadCommitted, "Test"))
			{
				LinqMetaData metaData = new LinqMetaData(trans);

				var q2 = from c in metaData.Customer where c.ContactTitle == "Owner" select c;
				List<CustomerEntity> owners = q2.ToList();

				var q = (from c in metaData.Customer
						 where c.Country == "USA"
						 select c).Intersect(owners);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(2, customers.Count);

				trans.Commit();
			}
		}


		[Test]
		public void GetAllCustomersWhoHaveOrdersWithMoreThan4ProductsAndWhoHaventPurchasedProduct3UsingNestedQuery()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from o in metaData.Order
					where o.OrderDetails.Except(from od in metaData.OrderDetail where od.ProductId == 3 select od).Count()>4
					select o.Customer;

			int count = 0;

			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(15, count);
		}


		[Test]
		public void GetAllUnitPricesWithProductIdAndOrderIdFromOrderDetails()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(from od in metaData.OrderDetail
								where od.ProductId < 4
								select new OrderDetailEntity{ OrderId = od.OrderId, UnitPrice = (decimal)od.UnitPrice, ProductId = od.ProductId });

			OrderDetailCollection result = (OrderDetailCollection)q.Execute();

			foreach(var v in result)
			{
				Assert.AreEqual(typeof(decimal), v.UnitPrice.GetType());
			}
		}


		[Test]
		public void GetAllEmployeesWithoutPhotoNorNoteFields()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from e in metaData.Employee
					 select e).ExcludeFields(e => e.Photo, e => e.Notes);

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Assert.IsTrue(v.Photo.Length <= 0);
				Assert.IsTrue(v.Notes.Length <= 0);
			}
			Assert.AreEqual(9, count);
		}


		#region PrefetchPath tests
		[Test]
		public void GetAllCustomersWithTheirRelatedOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Assert.IsTrue(v.Orders.Count > 0);
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersAndEmployeesUsingContext()
		{
			LinqMetaData metaData = new LinqMetaData();
			metaData.ContextToUse = new Context();

			// use a query which fetches Employees through orders into the customers and employee through order.
			// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
			var q = (from c in metaData.Customer
					 where c.Country=="Germany"
					 select c).WithPath(
								new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)), 
								new PathEdge<EmployeeEntity>(CustomerEntity.PrefetchPathEmployeeCollectionViaOrder));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// use object instance compare with linear search, otherwise we could get false positives as Equals and GetHashCode are tied to
				// the PK values of the entities, which obviously are equal.
				Assert.IsTrue(v.Orders.Count > 0);
				// get related data to circumvent lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				IEntityCollection employees = (IEntityCollection)customerRelatedData["EmployeeCollectionViaOrder"];
				foreach(OrderEntity o in orders)
				{
					// traverse the employees in v.Employees and do an object instance compare. Has to be there
					bool found = false;
					// work around lazy loading
					Dictionary<string, object> orderRelatedData = o.GetRelatedDataPublic();
					EmployeeEntity employeeRelatedToOrder = (EmployeeEntity)orderRelatedData["Employee"];
					foreach(EmployeeEntity e in employees)
					{
						if((object)e == (object)employeeRelatedToOrder)
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


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersWhichAreHandledByEmployee3()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o => o.EmployeeId == 3));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Assert.AreEqual(3, o.EmployeeId);
					}
				}
			}
			Assert.AreEqual(91, count);
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersInQueryVariant()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer.WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders))
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c;

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue(orders.Count > 0);
			}
			Assert.AreEqual(89, count);
		}

		[Test]
		public void GetAllCustomersWithTheirLast2Orders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null, 
						new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count > 0) && (orders.Count <= 2));
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersFiledByEmployee2()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o=>o.EmployeeId==2,
						new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count >= 0) && (orders.Count <= 2));
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Assert.AreEqual(2, o.EmployeeId);
					}
				}
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersWithSubPaths()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c)
						.WithPath(
							new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null,
									new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2, 
									new PathEdge<OrderDetailEntity>(OrderEntity.PrefetchPathOrderDetails),	// child edge from order
									new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)));		// child edge from order

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count > 0) && (orders.Count <= 2));
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Dictionary<string, object> orderRelatedData = o.GetRelatedDataPublic();
						IEntityCollection orderDetails = (IEntityCollection)orderRelatedData["OrderDetails"];
						EmployeeEntity employee = (EmployeeEntity)orderRelatedData["Employee"];
						Assert.IsTrue(orderDetails.Count > 0);
						Assert.IsNotNull(employee);
					}
				}
			}
			Assert.AreEqual(89, count);
		}


		/// <summary>
		/// Fetches a multi-branch prefetch path: territory m:n employee 1:n Order and employee m:n customer 1:n order.
		/// </summary>
		[Test]
		public void GetTerritoriesWithEmployeesAndCustomersOrdersInComplexPath()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from t in metaData.Territory select t)
						.WithPath(
						new PathEdge<EmployeeEntity>(TerritoryEntity.PrefetchPathEmployeeCollectionViaEmployeeTerritory,
								new PathEdge<OrderEntity>(EmployeeEntity.PrefetchPathOrders),	
								new PathEdge<CustomerEntity>(EmployeeEntity.PrefetchPathCustomerCollectionViaOrder, 
										new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders))
								)
							);

			int count = 0;
			foreach(var v in q)
			{
				count++;
				if(new string[] { "29202", "72716", "75234", "78759" }.Contains(v.TerritoryId))
				{
					// territory has no employees
					continue;
				}
				// work around lazy loading
				Dictionary<string, object> territoryRelatedData = v.GetRelatedDataPublic();
				IEntityCollection employees = (IEntityCollection)territoryRelatedData["EmployeeCollectionViaEmployeeTerritory"];
				Assert.IsTrue(employees.Count > 0);
				foreach(EmployeeEntity e in employees)
				{
					Dictionary<string, object> employeeRelatedData = e.GetRelatedDataPublic();
					IEntityCollection orders = (IEntityCollection)employeeRelatedData["Orders"];
					IEntityCollection customers = (IEntityCollection)employeeRelatedData["CustomerCollectionViaOrder"];
					Assert.IsTrue(orders.Count > 0);
					Assert.IsTrue(customers.Count>0);
					foreach(CustomerEntity c in customers)
					{
						if(!(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId)))
						{
							Dictionary<string, object> customerRelatedData = c.GetRelatedDataPublic();
							IEntityCollection customerOrders = (IEntityCollection)customerRelatedData["Orders"];
							Assert.IsTrue(customerOrders.Count>0);
						}
					}
				}
			}
			Assert.AreEqual(53, count);
		}

		
		[Test]
		public void GetCustomersWithRelatedOrdersAndExcludeFieldsInOrders()
		{
			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList(true);
			excludedFields.Add(OrderFields.OrderDate);
			excludedFields.Add(OrderFields.ShipName);

			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where c.Country == "USA"
					 select c).WithPath(
						new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null, null, 0, excludedFields));
			int count = 0;
			foreach(var v in q)
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


		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithC()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 where o.EmployeeId == 3
					 select o).WithPath(new PathEdge<CustomerEntity>(OrderEntity.PrefetchPathCustomer, c => c.CustomerId.StartsWith("C")));
			int count = 0;
			foreach(var v in q)
			{
				count++;
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				CustomerEntity relatedEntity = (CustomerEntity)customerRelatedData["Customer"];
				if(v.CustomerId.StartsWith("C"))
				{
					Assert.IsNotNull(relatedEntity);
					Assert.AreEqual(v.CustomerId, relatedEntity.CustomerId);
				}
				else
				{
					Assert.IsNull(relatedEntity);
				}
			}
			Assert.AreEqual(115, count);
		}
		#endregion

		#region PrefetchPath tests using LambdaExpression api developed by Jeremy 'Jez' Skinner
		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(p => p.Prefetch(c => c.Orders));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Assert.IsTrue(v.Orders.Count > 0);
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersAndEmployeesUsingContextJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			metaData.ContextToUse = new Context();

			// use a query which fetches Employees through orders into the customers and employee through order.
			// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
			var q = (from c in metaData.Customer
					 where c.Country == "Germany"
					 select c).WithPath(customerPath => customerPath
									.Prefetch<OrderEntity>(c => c.Orders).SubPath(orderPath => orderPath.Prefetch(o => o.Employee))
									.Prefetch<EmployeeEntity>(c => c.EmployeeCollectionViaOrder));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// use object instance compare with linear search, otherwise we could get false positives as Equals and GetHashCode are tied to
				// the PK values of the entities, which obviously are equal.
				Assert.IsTrue(v.Orders.Count > 0);
				// get related data to circumvent lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				IEntityCollection employees = (IEntityCollection)customerRelatedData["EmployeeCollectionViaOrder"];
				foreach(OrderEntity o in orders)
				{
					// traverse the employees in v.Employees and do an object instance compare. Has to be there
					bool found = false;
					// work around lazy loading
					Dictionary<string, object> orderRelatedData = o.GetRelatedDataPublic();
					EmployeeEntity employeeRelatedToOrder = (EmployeeEntity)orderRelatedData["Employee"];
					foreach(EmployeeEntity e in employees)
					{
						if((object)e == (object)employeeRelatedToOrder)
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


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersWhichAreHandledByEmployee3Jez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 3));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Assert.AreEqual(3, o.EmployeeId);
					}
				}
			}
			Assert.AreEqual(91, count);
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersInQueryVariantJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer.WithPath(p => p.Prefetch(c => c.Orders))
					where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					select c;

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue(orders.Count > 0);
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count > 0) && (orders.Count <= 2));
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersFiledByEmployee2Jez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 2).OrderByDescending(o => o.OrderDate).LimitTo(2));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count >= 0) && (orders.Count <= 2));
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Assert.AreEqual(2, o.EmployeeId);
					}
				}
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersWithSubPathsJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c)
						.WithPath(customerPath => customerPath
									.Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2)
										.SubPath(orderPath => orderPath
													.Prefetch(o => o.OrderDetails)
													.Prefetch(o => o.Employee)));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				// work around lazy loading
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				IEntityCollection orders = (IEntityCollection)customerRelatedData["Orders"];
				Assert.IsTrue((orders.Count > 0) && (orders.Count <= 2));
				if(orders.Count > 0)
				{
					foreach(OrderEntity o in orders)
					{
						Dictionary<string, object> orderRelatedData = o.GetRelatedDataPublic();
						IEntityCollection orderDetails = (IEntityCollection)orderRelatedData["OrderDetails"];
						EmployeeEntity employee = (EmployeeEntity)orderRelatedData["Employee"];
						Assert.IsTrue(orderDetails.Count > 0);
						Assert.IsNotNull(employee);
					}
				}
			}
			Assert.AreEqual(89, count);
		}


		/// <summary>
		/// Fetches a multi-branch prefetch path: territory m:n employee 1:n Order and employee m:n customer 1:n order.
		/// </summary>
		[Test]
		public void GetTerritoriesWithEmployeesAndCustomersOrdersInComplexPathJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from t in metaData.Territory select t)
						.WithPath(territoryPath => territoryPath
								.Prefetch<EmployeeEntity>(t => t.EmployeeCollectionViaEmployeeTerritory)
									.SubPath(employeePath => employeePath
												.Prefetch(e => e.Orders)
												.Prefetch<CustomerEntity>(e => e.CustomerCollectionViaOrder)
													.SubPath(customerPath => customerPath.Prefetch(c => c.Orders))));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				if(new string[] { "29202", "72716", "75234", "78759" }.Contains(v.TerritoryId))
				{
					// territory has no employees
					continue;
				}
				// work around lazy loading
				Dictionary<string, object> territoryRelatedData = v.GetRelatedDataPublic();
				IEntityCollection employees = (IEntityCollection)territoryRelatedData["EmployeeCollectionViaEmployeeTerritory"];
				Assert.IsTrue(employees.Count > 0);
				foreach(EmployeeEntity e in employees)
				{
					Dictionary<string, object> employeeRelatedData = e.GetRelatedDataPublic();
					IEntityCollection orders = (IEntityCollection)employeeRelatedData["Orders"];
					IEntityCollection customers = (IEntityCollection)employeeRelatedData["CustomerCollectionViaOrder"];
					Assert.IsTrue(orders.Count > 0);
					Assert.IsTrue(customers.Count > 0);
					foreach(CustomerEntity c in customers)
					{
						if(!(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId)))
						{
							Dictionary<string, object> customerRelatedData = c.GetRelatedDataPublic();
							IEntityCollection customerOrders = (IEntityCollection)customerRelatedData["Orders"];
							Assert.IsTrue(customerOrders.Count > 0);
						}
					}
				}
			}
			Assert.AreEqual(53, count);
		}


		[Test]
		public void GetCustomersWithRelatedOrdersAndExcludeFieldsInOrdersJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where c.Country == "USA"
					 select c).WithPath(customerPath => customerPath
							 .Prefetch<OrderEntity>(c => c.Orders).Exclude(o => o.OrderDate, o => o.ShipName));
			int count = 0;
			foreach(var v in q)
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


		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithCJez()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 where o.EmployeeId == 3
					 select o).WithPath(orderPath => orderPath
							 .Prefetch<CustomerEntity>(o => o.Customer).FilterOn(c => c.CustomerId.StartsWith("C")));

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Dictionary<string, object> customerRelatedData = v.GetRelatedDataPublic();
				CustomerEntity relatedEntity = (CustomerEntity)customerRelatedData["Customer"];
				if(v.CustomerId.StartsWith("C"))
				{
					Assert.IsNotNull(relatedEntity);
					Assert.AreEqual(v.CustomerId, relatedEntity.CustomerId);
				}
				else
				{
					Assert.IsNull(relatedEntity);
				}
			}
			Assert.AreEqual(115, count);
		}
		

		[Test]
		public void GetOrderDetailsWithOrderSortedOnCustomerRelatedToOrder()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from od in metaData.OrderDetail
					 where od.ProductId > 10
					 select od).WithPath(p => p.Prefetch<OrderEntity>(od => od.Order).OrderBy(o => o.Customer.Country));

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(1806, count);
		}
		#endregion

		#region PrefetchPath tests using original PrefetchPath objects.
		/// <summary>
		/// Just one test to see if the mechanism works, as all we have to do is pass the path to the wrapping select in preprocessor
		/// </summary>
		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersOrig()
		{
			IPrefetchPath path = new PrefetchPath(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);

			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
					 select c).WithPath(path);

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Assert.IsTrue(v.Orders.Count > 0);
			}
			Assert.AreEqual(89, count);
		}
		#endregion



		[Test]
		public void GetMaxOrdersPerCustomerUsingJoinWithGroupedSet()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q1 = from o in metaData.Order
					 where o.OrderDate <= new DateTime(1997, 8, 1)
					 && o.EmployeeId != null
					 group o by o.CustomerId into g
					 select new { LastId = g.Max(x => x.OrderId) };

			var q = from o in metaData.Order
					join ids in q1
					on o.OrderId equals ids.LastId
					where ids.LastId > 10600
					select o;

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(12, count);
		}


		[Test]
		public void GetOrdersSortedByShippedDateOrOrderDateUsingCoalesce()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					orderby o.ShippedDate ?? o.OrderDate
					select o;

			int count = 0;
			DateTime? prev = null;
			DateTime? curr = null;
			bool firstTime = true;
			foreach(var v in q)
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


		[Test]
		public void GetAllCustomersWhoHaveOnlyOneOrder()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					group o by o.CustomerId into g
					where g.Count() == 1
					select new
					{
						CustomerID = g.Key,
						Count = g.Count()
					};

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(1, count);
		}

		[Test]
		public void CustomerUsingCustomLinqMetaData()
		{
			SsLinqMetaData metaData = new SsLinqMetaData();
			CustomerEntity alfki = metaData.LoadBase<CustomerEntity>(c => c.CustomerId == "ALFKI");
			Assert.AreEqual("ALFKI", alfki.CustomerId);
		}
	}

		    
	public class SsLinqMetaData : LinqMetaData
	{
		public SsLinqMetaData()
		{
		}

		public SsLinqMetaData(ITransaction transactionToUse)
			: base(transactionToUse)
		{
		}

		public SsLinqMetaData(ITransaction transactionToUse, FunctionMappingStore customFunctionMappings)
			: base(transactionToUse, customFunctionMappings)
		{
		}

		public TEntity LoadBase<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> filterLambda)
			where TEntity : EntityBase
		{
			var q = new DataSource<TEntity>(this.TransactionToUse, new ElementCreator(), this.CustomFunctionMappings, this.ContextToUse);
			return q.Where(filterLambda).FirstOrDefault();
		}
	}
}
