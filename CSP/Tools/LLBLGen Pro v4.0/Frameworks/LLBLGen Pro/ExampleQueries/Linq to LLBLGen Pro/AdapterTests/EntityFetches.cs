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
using System.Runtime.CompilerServices;

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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(91, customers.Count);
			}
		}



		[Test]
		public void LeftJoinTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into co
						from x in co.DefaultIfEmpty() // x is of type order
						where x != null
						select c;

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void GetCategoriesWithProductsInProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Category
						select new { c.CategoryName, Category = c, Products = c.Products };
				foreach(var v in q)
				{
					Assert.IsNotNull(v.Category);
					Assert.AreEqual(v.CategoryName, v.Category.CategoryName);
					Assert.IsTrue(v.Products.Any());
				}
			}
		}


		[Test]
		public void GetCustomersWithProductFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						join p in metaData.Product on od.ProductId equals p.ProductId
						where p.ProductId == 1
						select c;

				foreach(var v in q)
				{
					Console.WriteLine(v.CompanyName);
				}
			}
		}


		[Test]
		public void GetOrdersFiledAfterGivenDateUsingNullableFieldCompareWithConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate > new DateTime(1998, 1, 1)
						select o;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(260, count);
			}
		}
		

		[Test]
		public void GetOrdersFiledAfterGivenDateUsingNullableFieldValueParameterCompareWithConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate.Value > new DateTime(1998, 1, 1)
						select o;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(260, count);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Distinct();

				List<CustomerEntity> customers = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						orderby c.Country ascending, c.City ascending
						select c).First();

				Assert.AreEqual("COMMI", q.CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 orderby c.Country ascending, c.City ascending
						 select c).First(c=>c.Country=="USA");

				Assert.AreEqual("RATTC", q.CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 orderby c.Country ascending, c.City ascending
						 select c).Single();

			}
		}


		[Test]
		public void GetSingleCustomerFromListUsingSingleWithFilterOnCustomerId()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c).Single(c=>c.CustomerId=="CHOPS");

				Assert.AreEqual("CHOPS", q.CustomerId);

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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.TakePage(2, 10);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(10, customers.Count);
			}
		}


		/// <summary>
		/// Gets a page of entities from a set without any filter.
		/// </summary>
		[Test]
		public void GetPageOfEmployeesFromSetWithoutFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 select e).TakePage(2, 2);

				List<EmployeeEntity> employees = q.ToList();
				Assert.AreEqual(2, employees.Count);
			}
		}


		[Test]
		public void GetPageOfEntitiesFromSetWithoutFilterUsingSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Skip(10).Take(10);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(10, customers.Count);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (
							from c in metaData.Customer
							where c.Country == "Germany"
							select c
						).TakePage(2, 3);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(3, customers.Count);
			}
		}


		[Test]
		public void GetPageOfEntitiesFromSetWithFilterUsingSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (
							from c in metaData.Customer
							where c.Country == "Germany"
							select c
						).Skip(3).Take(3);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(3, customers.Count);
			}
		}


		/// <summary>
		/// Test which fetches a set of entities (customers) without any filter or other fancy things.
		/// </summary>
		[Test]
		public void GetEntitySetWithoutAnyFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(91, customers.Count);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country=="Germany"
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(11, customers.Count);
			}
		}


		[Test]
		public void GetEntitySetWithSingleFieldFilterWithEqualsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.Equals("Germany")
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(11, customers.Count);
			}
		}


		[Test]
		public void GetEntitySetWithSingleFieldFilterWithEqualsTestUsingProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country.Equals("Germany")
						select new CustomerEntity() { CustomerId = c.CustomerId, CompanyName = c.CompanyName };

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(11, customers.Count);
			}
		}


		[Test]
		public void GetCustomerWithEquationOperatorOnEntireEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				CustomerEntity chopsEntity = new CustomerEntity("CHOPS");
				adapter.FetchEntity(chopsEntity);
	
				var q = from c in metaData.Customer
						where c == chopsEntity
						select c;

				List<CustomerEntity> customers = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all customers in the USA).
		/// </summary>
		[Test]
		public void GetEntitySetWithMaxLimitAndFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c).Take(5);
				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						where o.EmployeeId == 6
						select c);
				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from e in metaData.Employee
						join m in metaData.Employee on e.ReportsTo equals m.EmployeeId
						where m.FirstName == "Andrew"
						select e);
				EntityCollection<EmployeeEntity> employees = (EntityCollection<EmployeeEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
							from c in metaData.Customer
							join x in
								(from o in metaData.Order where o.EmployeeId > 3 select new { CustomerId = o.CustomerId, EmployeeId = o.EmployeeId })
									on c.CustomerId equals x.CustomerId
							where x.EmployeeId == 6
							select c);
				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				ILLBLGenProQuery q = (ILLBLGenProQuery)(
							from o in metaData.Order
							from p in metaData.Product
							join od in metaData.OrderDetail on
								new { o.OrderId, p.ProductId } equals new { od.OrderId, od.ProductId }
							where o.EmployeeId == 2
							select o);
				EntityCollection<OrderEntity> orders = (EntityCollection<OrderEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from c in metaData.Customer
						from o in metaData.Order
						from od in metaData.OrderDetail
						where od.ProductId == 2
						select c);

				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
						from c in metaData.Customer
						where c.Country == "Germany"
						from o in metaData.Order
						where o.EmployeeId == 2
						select c);

				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into oc
						from x in oc.DefaultIfEmpty()
						where x.OrderId == null
						&& c.Country=="France"
						select c;

				int counter = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Test which fetches all orders of all customers from germany using the 'Customer' property in order to use an implicit inner join. 
		/// </summary>
		[Test]
		public void GetAllOrdersOfCustomersInGermanyWithFilterOnRelatedEntityUsingFieldOnRelationProperty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Simple aggregate test on Count. It fetches all customers which have more than 10 orders. This is done by applying a count on the Orders set.
		/// </summary>
		[Test]
		public void GetAllCustomersWhichHaveMoreThan10Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Tests propagation of correlationrelation 
		/// </summary>
		[Test]
		public void GetOrderDetailsUsingMultiHopAggregate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						where od.Order.Employee.EmployeeTerritories.Where(et=>et.TerritoryId.Contains("0")).Count() > 3
						select od;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan1000ForOrdersWithProductsWithAnIdBiggerThan40()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan1000ForOrdersWithProductsWithAnIdBiggerThan40_2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let total = c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Product.UnitPrice.Value))
						where total > 1000
						select c;

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(84, counter);
			}
		}


		[Test]
		public void GetAllCustomersWithAnOrderTotalHigherThan1000ForOrdersWithProductsWithAnIdBiggerThan40_3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let total = c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Product.UnitPrice))
						where total > 1000
						select c;

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(84, counter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
							from c in metaData.Category
							let x = c.Products.Select(p => (int)p.UnitsInStock).Sum()
							where x > 500
							orderby x ascending, c.CategoryName descending
							select c);

				EntityCollection<CategoryEntity> categories = (EntityCollection<CategoryEntity>)q.Execute();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
							from o in metaData.Order
							where o.Customer.Country == "UK"
							orderby o.Customer.City descending
							select o
							);

				EntityCollection<OrderEntity> orders = (EntityCollection<OrderEntity>)q.Execute();
				Assert.AreEqual(56, orders.Count);
				Assert.AreEqual("SEVES", orders[0].CustomerId);
			}
		}


		[Test]
		public void GetAllOrdersFromCustomersFromGermanyUsingJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						where o.Customer.Country == "Germany"
						select o;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(metaData.Customer.OrderBy(c=>c.Country).ThenByDescending(c=>c.City));

				EntityCollection<CustomerEntity> customers = (EntityCollection<CustomerEntity>)q.Execute();
				Assert.AreEqual(91, customers.Count);
				Assert.AreEqual("COMMI", customers[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Gets the regions which have only employees which have titleofcourtesy set to 'Ms.'. This is a test for the All() extension method.
		/// </summary>
		[Test]
		public void GetAllRegionsWhichHaveOnlyMsEmployees()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Gets all orders which have purchased product with id 2. This is a test for the Any() extension method.
		/// </summary>
		[Test]
		public void GetAllOrdersWhichHavePurchasedProduct2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		/// <summary>
		/// Gets the customer based on contains call on orders collection. This is a simple test with an entity as argument.
		/// </summary>
		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimple()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity order = new OrderEntity();
				order.OrderId = 10254;

				var q = from c in metaData.Customer
						where c.Orders.Contains(order)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity order = new OrderEntity();
				order.OrderId = 10254;

				var q = from c in metaData.Customer
						where c.Orders.Where(o=>o.EmployeeId==3).Contains(order)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", results[0].CustomerId);
			}
		}


		[Test]
		public void GetCustomerBasedOnContainsCallOnOrdersCollectionSimpleWithWhereOnSourceUsingValueOnNullableType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity order = new OrderEntity();
				order.OrderId = 10254;

				var q = from c in metaData.Customer
						where c.Orders.Where(o => o.EmployeeId.Value == 3).Contains(order)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity order = new OrderEntity();
				order.OrderId = 10254;

				var q = from c in metaData.Customer
						where c.Orders.Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("BLONP", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity order = new OrderEntity();
				order.OrderId = 10254;

				var q = from c in metaData.Customer
						where c.Orders.Where(o => o.EmployeeId == 2).Contains((from o in metaData.Order where o.EmployeeId == 2 select o).First())
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("BLONP", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Orders.Where(o => o.EmployeeId > 3).Select(o => o.ShipVia).Contains(2)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
							new { EID = (int?)1, CID = "CHOPS" })
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Orders.Select(oc => oc.EmployeeId).Contains((from o in metaData.Order where o.CustomerId == "CHOPS" select o.EmployeeId).First())
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

			var q = from c in metaData.Customer
					where c.Orders.Select(oc => new { EID = oc.EmployeeId, CID = oc.CustomerId }).Contains(
						(from o in metaData.Order where o.CustomerId == "CHOPS" select new { EID = o.EmployeeId, CID = o.CustomerId }).First())
					select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(1, results.Count);
				Assert.AreEqual("CHOPS", results[0].CustomerId);
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				List<string> countries = new List<string>() { "USA", "UK" };
				var q = from c in metaData.Customer
						where countries.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where new List<string>() { "USA", "UK" }.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				string[] values = new string[] { "UK" };
				var q = from c in metaData.Customer
						where new List<string>(values) { "USA"}.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				LinkedList<string> countries = new LinkedList<string>(new string[] { "USA", "UK"});
				var q = from c in metaData.Customer
						where countries.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				string[] countries = new string[] { "USA", "UK" };
				var q = from c in metaData.Customer
						where countries.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q2 = (from c in metaData.Customer
						  select c.Country).Distinct();

				var q = from c in metaData.Customer
						where q2.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(90, results.Count);
			}
		}


		/// <summary>
		/// Gets the customers based on range filter of country against in memory materialized array indexed from other array.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryMaterializedArrayIndexedFromOtherArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				string[][] countries = new string[1][] { new string[] { "USA", "UK" } };
				var q = from c in metaData.Customer
						where countries[0].Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(20, results.Count);
			}
		}


		[Test]
		public void GetCustomersBasedOnRangeFilterOfCountryAgainstInMemoryInPlaceCreatedArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where new string[] { "USA", "UK" }.Contains(c.Country)
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(20, results.Count);
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q2 = from c in metaData.Customer where c.Country == "USA" select c;
				List<CustomerEntity> customers = q2.ToList();

				var q = from c in metaData.Customer
						where customers.Contains(c)
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q2 = from od in metaData.OrderDetail where od.ProductId < 5 select od;
				List<OrderDetailEntity> orderDetails = q2.ToList();

				var q = from od in metaData.OrderDetail
						where orderDetails.Contains(od)
						select od;

				List<OrderDetailEntity> results = q.ToList();
				Assert.AreEqual(111, results.Count);
			}
		}


		[Test]
		public void GetEntitySetBasedOnFilterOnInMemorySetOfValuesUsingQueryWithFieldAsOperand()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				List<int?> employeeIds = new List<int?>() { 1, 2 };

				var q = from c in metaData.Customer
						where employeeIds.Contains((from o in metaData.Order where o.CustomerId == c.CustomerId select o.EmployeeId).First())
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

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
		}


		/// <summary>
		/// Gets the customers based on contains call on string field. This results in a LIKE query.
		/// </summary>
		[Test]
		public void GetCustomersBasedOnContainsCallOnStringField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country.Contains("land")
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from o in metaData.Order
						where o.Customer.Country.Contains("land")
						select o;

				List<OrderEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from o in metaData.Order
						where (from c in metaData.Customer where c.CustomerId==o.CustomerId select c.Country).First().Contains("land")
						select o;

				List<OrderEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country.StartsWith("A")
						select c;

				List<CustomerEntity> results = q.ToList();
				Assert.AreEqual(5, results.Count);
			}
		}


		[Test]
		public void GetCustomersBasedOnEndsWithCallOnStringField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country.EndsWith("A")
						select c;

				List<CustomerEntity> results = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				// get 14th element, which is element with index 13.
				var q = (from c in metaData.Customer
						orderby c.Country ascending, c.City descending
						select c).ElementAt(13);

				CustomerEntity customer = q;
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in
							metaData.Employee.Except(from e in metaData.Employee where e.Country == "USA" select e)
						select e;

				List<EmployeeEntity> employeesFromUK = q.ToList();
				Assert.AreEqual(4, employeesFromUK.Count);
			}
		}


		/// <summary>
		/// Gets the employees from UK using except filtering without projection.
		/// </summary>
		[Test]
		public void GetEmployeesFromUKUsingExceptFilteringWithoutProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Employee.Except(from e in metaData.Employee where e.Country == "USA" select e);

				List<EmployeeEntity> employeesFromUK = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Orders.Except(from o in metaData.Order where o.EmployeeId == 3 select o).Count() > 5
						select c;

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(56, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoHaveMoreThan5OrdersFiledByEmployeesOtherThanEmployee3UsingInMemoryList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q2 = from o in metaData.Order where o.EmployeeId == 3 select o;
				List<OrderEntity> ordersToExclude = q2.ToList();

				var q = from c in metaData.Customer
						where c.Orders.Except(ordersToExclude).Count() > 5
						select c;

				List<CustomerEntity> customers = q.ToList();
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = (from c in metaData.Customer
						 where c.Country == "USA"
						 select c).Intersect(from c in metaData.Customer where c.ContactTitle == "Owner" select c);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(2, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoAreFromTheUSAAndAreOwnerUsingIntersectWithInMemorySet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q2 = from c in metaData.Customer where c.ContactTitle == "Owner" select c;
				List<CustomerEntity> owners = q2.ToList();

				var q = (from c in metaData.Customer
						 where c.Country == "USA"
						 select c).Intersect(owners);

				List<CustomerEntity> customers = q.ToList();
				Assert.AreEqual(2, customers.Count);
			}
		}


		[Test]
		public void GetAllCustomersWhoHaveOrdersWithMoreThan4ProductsAndWhoHaventPurchasedProduct3UsingNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

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
		}


		[Test]
		public void GetAllUnitPricesWithProductIdAndOrderIdFromOrderDetails()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(from od in metaData.OrderDetail
										where od.ProductId < 4
										select new OrderDetailEntity{ OrderId = od.OrderId, UnitPrice = (decimal)od.UnitPrice, ProductId = od.ProductId });

				EntityCollection<OrderDetailEntity> result = (EntityCollection<OrderDetailEntity>)q.Execute();

				foreach(var v in result)
				{
					Assert.AreEqual(typeof(decimal), v.UnitPrice.GetType());
				}
			}
		}


		[Test]
		public void GetAllEmployeesWithoutPhotoNorNoteFields()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}
		

		[Test]
		public void GetAllEmployeesWithoutPhotoNorNoteFields2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee.ExcludeFields(x => x.Photo, x => x.Notes)
						 group e by e.ReportsTo into g
						 select g);

				int count = 0;
				foreach(var g in q)
				{
					count++;
					foreach(var v in g)
					{
						Assert.IsTrue(v.Photo.Length <= 0);
						Assert.IsTrue(v.Notes.Length <= 0);
					}
				}
				Assert.AreEqual(6, count);
			}
		}


		[Test]
		public void GetAllEmployeesWithJustAFewFieldsIncluded()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 select e).IncludeFields(e=>e.FirstName, e=>e.Title);

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from p in metaData.Product
						 select p).IncludeFields(p => p.ProductName, p => p.QuantityPerUnit, p=>p.Discontinued);

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where p.Discontinued
						select p;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where p.Discontinued && (p.SupplierId==5)
						select p;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.EmployeeCollectionViaOrder.Count() > 2
						select c;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from e in metaData.Employee
						join o in metaData.Order on e.EmployeeId equals o.EmployeeId
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						where c.CustomerId == "ALFKI"
						select e;

				q = q.OrderBy(e => e.City).ThenBy(e => e.Region);

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.ShippedDate == null
						select o;
				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.CustomerId == "F1111"
						 select c).FirstOrDefault();

				Assert.IsNull(q);
			}
		}


		[Test]
		public void GetAllCustomersFromFrance()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country == "France"
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void GetCustomersInSpecificCountries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				List<string> countries = new List<string>();
				countries.Add("USA");
				countries.Add("UK");

				var q = from c in metaData.Customer
						select c;

				var q2 = q.Where(c => countries.Contains(c.Country));

				foreach(var v in q2)
				{
					Console.WriteLine(v);
				}
			}
		}


		#region PrefetchPath tests using PathEdge api
		[Test]
		public void GetAllCustomersWithTheirRelatedOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}


		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersAndEmployeesUsingContext()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				Context contextToUse = new Context();
				metaData.ContextToUse = contextToUse;

				// use a query which fetches Employees through orders into the customers and employee through order.
				// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
				var q = (from c in metaData.Customer
						 where c.Country=="Germany"
						 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, 
													new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)), 
											new PathEdge<EmployeeEntity>(CustomerEntity.PrefetchPathEmployeeCollectionViaOrder));

				int count = 0;
				foreach(var v in q)
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
		public void GetAllCustomersWithTheirRelatedOrdersWhichAreHandledByEmployee3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o => o.EmployeeId == 3));

				int count = 0;
				foreach(var v in q)
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
		public void GetAllCustomersWithTheirRelatedOrdersInQueryVariant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer.WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders))
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(89, count);
			}
		}

		[Test]
		public void GetAllCustomersWithTheirLast2Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null, 
								new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2));

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue((v.Orders.Count > 0) && (v.Orders.Count<=2));
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetAllCustomersWithTheirLast2OrdersFiledByEmployee2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, o=>o.EmployeeId==2,
								new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c)
							.WithPath(new PathEdge<OrderEntity>(CustomerEntity.PrefetchPathOrders, null,
											new SortExpression((OrderFields.OrderDate | SortOperator.Descending)), 2, 
												new PathEdge<OrderDetailEntity>(OrderEntity.PrefetchPathOrderDetails),	// child edge from order
												new PathEdge<EmployeeEntity>(OrderEntity.PrefetchPathEmployee)));		// child edge from order

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from t in metaData.Territory select t)
							.WithPath(
								new PathEdge<EmployeeEntity>(
										TerritoryEntity.PrefetchPathEmployeeCollectionViaEmployeeTerritory,
											new PathEdge<OrderEntity>(EmployeeEntity.PrefetchPathOrders),	
											new PathEdge<CustomerEntity>(
													EmployeeEntity.PrefetchPathCustomerCollectionViaOrder, 
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
				ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList(true);
				excludedFields.Add(OrderFields.OrderDate);
				excludedFields.Add(OrderFields.ShipName);

				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}

		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithC()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 where o.EmployeeId == 3
						 select o).WithPath(new PathEdge<CustomerEntity>(OrderEntity.PrefetchPathCustomer, c => c.CustomerId.StartsWith("C")));
				int count = 0;
				foreach(var v in q)
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
		public void GetCustomersFromGermanyWithRelatedOrdersAndOrderDetails()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.Country == "Germany"
						 select c).WithPath(new PathEdge<OrderEntity>(
													CustomerEntity.PrefetchPathOrders, 
															new PathEdge<OrderDetailEntity>(OrderEntity.PrefetchPathOrderDetails)));

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(11, count);

				count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(11, count);
			}
		}
		#endregion

		#region PrefetchPath tests using LambdaExpression api developed by Jeremy 'Jez' Skinner
		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(p=>p.Prefetch(c=>c.Orders));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				Context contextToUse = new Context();
				metaData.ContextToUse = contextToUse;

				// use a query which fetches Employees through orders into the customers and employee through order.
				// the employee entity instances in the m:n collection should be the same as the ones referenced through the order instances.
				var q = (from c in metaData.Customer
						 where c.Country == "Germany"
						 select c).WithPath(customerPath => customerPath
									.Prefetch<OrderEntity>(c => c.Orders).SubPath(orderPath => orderPath.Prefetch<EmployeeEntity>(o => o.Employee))
									.Prefetch<EmployeeEntity>(c => c.EmployeeCollectionViaOrder));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 3));

				int count = 0;
				foreach(var v in q)
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
		public void GetAllCustomersWithTheirRelatedOrdersInQueryVariantJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer.WithPath(p=>p.Prefetch(c=>c.Orders))
						where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetAllCustomersFromGermanyWithTheirOrdersAndRelatedEmployeeAndOrderDetailsWithExcludedFieldsJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.Country == "Germany"
						 select c).WithPath<CustomerEntity>(cpath => cpath
								.Prefetch<OrderEntity>(c => c.Orders)
									.SubPath(opath=>opath
										.Prefetch(o=>o.OrderDetails)
										.Prefetch<EmployeeEntity>(o => o.Employee).Exclude(e => e.Photo, e => e.Notes)));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(customerPath => customerPath
								.Prefetch<OrderEntity>(c => c.Orders).FilterOn(o => o.EmployeeId == 2).OrderByDescending(o => o.OrderDate).LimitTo(2));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where !(new string[] { "FISSA", "PARIS" }.Contains(c.CustomerId))
						 select c).WithPath(customerPath => customerPath
									.Prefetch<OrderEntity>(c => c.Orders).OrderByDescending(o => o.OrderDate).LimitTo(2)
										.SubPath(orderPath => orderPath
													.Prefetch(o => o.OrderDetails)
													.Prefetch(o => o.Employee)));

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c).WithPath(customerPath => customerPath
							 .Prefetch<OrderEntity>(c => c.Orders))
							 .Single(c=>c.CustomerId=="CHOPS");

				Assert.AreEqual("CHOPS", q.CustomerId);
				Assert.IsTrue(q.Orders.Count > 0);
			}
		}

		[Test]
		public void GetOrdersWithRelatedCustomersIfCustomerStartsWithCJez()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 where o.EmployeeId == 3
						 select o).WithPath(orderPath => orderPath
							 .Prefetch<CustomerEntity>(o => o.Customer).FilterOn(c => c.CustomerId.StartsWith("C")));
				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}



		[Test]
		public void GetCustomerWithRelatedOrdersSortedByFieldAndLimit()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.CustomerId == "ALFKI"
						 select c)
							 .WithPath(customerPath =>
								 customerPath.Prefetch<OrderEntity>(c => c.Orders).OrderBy(o => o.Freight).LimitTo(2));

				foreach(var v in q)
				{
					Assert.IsTrue(v.Orders.Count <= 2);
					Assert.AreEqual("ALFKI", v.CustomerId);
				}
			}
		}
		#endregion

		#region PrefetchPath tests using original PrefetchPath objects.
		/// <summary>
		/// Just one test to see if the mechanism works, as all we have to do is pass the path to the wrapping select in preprocessor
		/// </summary>
		[Test]
		public void GetAllCustomersWithTheirRelatedOrdersOrig()
		{
			IPrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
			path.Add(CustomerEntity.PrefetchPathOrders);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}
		#endregion


		[Test]
		public void DelegatePredicateWithExpressionFuncOnEntityCollection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer select c;

				EntityCollection<CustomerEntity> customers = ((ILLBLGenProQuery)q).Execute<EntityCollection<CustomerEntity>>();
				Assert.AreEqual(91, customers.Count);

				EntityView2<CustomerEntity> customersFromGermany = new EntityView2<CustomerEntity>(customers, c=>c.Country=="Germany");

				Assert.AreEqual(11, customersFromGermany.Count);

				EntityView2<CustomerEntity> customersFromGermany2 = new EntityView2<CustomerEntity>(customers);
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
				LinqMetaData metaData = new LinqMetaData(adapter);

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
		}


		[Test]
		public void GetOrdersSortedByShippedDateOrOrderDateUsingCoalesce()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
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
		}
		

		[Test]
		public void GetAllCustomersSortedBySecondCharacterInCustomerId()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						orderby c.CustomerId[1]
						select c;

				char prev = ' ';
				char curr = ' ';
				bool firstRun = false;
				foreach(var v in q)
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
				var q = from o in metaData.Order
						join cu in
							(
								from c in metaData.Customer
								where c.Country == "USA"
								select c.CustomerId) on o.CustomerId equals cu
						select o;

				var q2 = from c in metaData.Customer
						 where c.Country == "USA"
						 select c.CustomerId;

				List<string> customerIds = q2.ToList();

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join ods in
							(from od in metaData.OrderDetail
							 group od by od.OrderId into g
							 select new { OrderId = g.Key, Total = g.Sum(x => x.Quantity * x.UnitPrice) })
						  on o.OrderId equals ods.OrderId
						where ods.Total > 5000
						select o;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.PksideCompoundKey
						join f in metaData.FksideCompoundKey on
							new { IdField1 = p.Idfield1, IdField2 = p.Idfield2 } equals new { IdField1 = f.Idfield1.Value, IdField2 = f.Idfield2.Value }
						select f;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "BLAUS"
						orderby (o.ShippedDate.HasValue) ascending, o.EmployeeId descending
						select o;

				int count = 0;

				bool first = true;
				foreach(var v in q)
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
		public void GetOrdersSortedOnABooleanExpressionBasedoNullabilityOfShippedDate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "BLAUS"
						orderby (o.ShippedDate!=null) ascending, o.EmployeeId descending
						select o;

				int count = 0;

				bool first = true;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						where !p.CategoryId.HasValue
						select p;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetOrdersSortedByCustomerCountry()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				IQueryable<OrderEntity> q = from o in metaData.Order
						select o;

				q = q.OrderBy(o => o.Customer.Country);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetOrdersWithCustomerUsingPathAndAppendedOrderBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				adapter.ParameterisedPrefetchPathThreshold = 25;
				var q = (from o in metaData.Order
						 select o).WithPath<OrderEntity>(p => p.Prefetch(o => o.Customer));

				q = q.OrderBy(o=>o.OrderDate).Take(64);
				int count = 0;
				foreach(var v in q)
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
		public void GetOrdersUsingContainsWithNullableFieldAndCoalesceInContainsParameter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				List<int> employees = new List<int>() { 2, 3 };
				var q = from o in metaData.Order
						where employees.Contains(o.EmployeeId==null ? -1 : (int)o.EmployeeId)
						select o;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where (from e in metaData.Employee where e.FirstName.StartsWith("L") select e.EmployeeId).Contains(o.EmployeeId ?? -1)
						select o;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);

				var allEmployees = from e in metaData.Employee select e;

				var employeesCustomer = from e in metaData.Employee
										join o in metaData.Order on e.EmployeeId equals o.EmployeeId
										join c in metaData.Customer on o.CustomerId equals c.CustomerId
										where c.CustomerId == "CHOPS"
										select e;
				var diff = allEmployees.Except(employeesCustomer);

				int count = 0;
				int[] expectedValues = new int[] { 2, 5, 8, 9 };
				foreach(var v in diff)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.CustomerId == "CHOPS"
						from o in c.Orders
						from od in o.OrderDetails
						select od;

				int count = 0;
				int[] expectedOrderIds = new int[] { 10254, 10370, 10519, 10731, 10746, 10966, 11029, 11041 };
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						where o.EmployeeId == (from e in metaData.Employee
											  where e.EmployeeId == o.EmployeeId
											  select e).Min(e => e.EmployeeId)
											  && c.Country=="Germany"
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void GetCustomersUsingPredicateBuilder()
		{
			var predicate = PredicateBuilder.Null<CustomerEntity>();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				predicate = predicate.Or(c => c.City == "London");
				predicate = predicate.Or(c => c.City == "Paris");

				var q = metaData.Customer.Where(predicate);
				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Where(c => c.EmployeeCollectionViaOrder.Any(
									e => e.TerritoryCollectionViaEmployeeTerritory.Any(
										t => t.RegionId == 2)));
				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Where(c => c.Orders.Any(o => o.OrderDetails.Any(od => od.ProductId == 1)));

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(31, count);
			}
		}


		[Test]
		public void GetOrdersOrderedByFieldInNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in GetOrdersInCustomClass(metaData)
						orderby o.RelatedCustomer.CompanyNme
						select o;

				int count = 0;
				string prev=string.Empty, cur=string.Empty;
				foreach(var v in q)
				{
					prev = cur;
					cur = v.RelatedCustomer.CompanyNme;
					if(prev != string.Empty)
					{
						Assert.IsTrue(prev.CompareTo(cur) <= 0);
					}
					count++;
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetCustomerFromOrderUsingMultipleCalls()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				try
				{
					adapter.StartTransaction(System.Data.IsolationLevel.ReadCommitted, "Test");

					var q = (from o in metaData.Order
							 where o.OrderId == 10254
							 select o.Customer).FirstOrDefault();

					Assert.IsNotNull(q);

					q = (from o in metaData.Order
						 where o.OrderId == 10254
						 select o.Customer).FirstOrDefault();

					Assert.IsNotNull(q);
					Assert.AreEqual("CHOPS", q.CustomerId);
				}
				finally
				{
					adapter.Commit();
				}
			}
		}


		[Test]
		public void GetEntitySetUsingProjectionOnDirectEntityType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						where o.EmployeeId > 4
						orderby o.OrderDate ascending
						select new OrderDerived()
						{
							OrderId = o.OrderId,
							CustomerId = o.CustomerId,
							CompanyName = c.CompanyName,
							EmployeeId = o.EmployeeId
						};

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.EmployeeId > 4);
					Assert.IsFalse(string.IsNullOrEmpty(v.CompanyName));
					count++;
				}
				Assert.AreEqual(334, count);
			}
		}


		[Test]
		[ExpectedException(ExpectedException = typeof(NotSupportedException), ExpectedMessage = "The query should have resulted in at most 1 element due to the usage of Single() or SingleOrDefault(), though resulted in 6 elements being returned.")]
		public void GetCustomerUsingSingleAndMultiPredicateFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Single(c => c.Country == "USA" && c.CustomerId.Contains("E"));
			}
		}


		[Test]
		public void GetCustomersWithContainsOnEmptyCollectionAndConstantBoolFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				List<string> ids = new List<string>();
				var q = from c in metaData.Customer
						where (ids.Count > 0) ? ids.Contains(c.CustomerId) : false
						select c;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				List<string> ids = new List<string>();
				var q = from c in metaData.Customer
						where ids.Contains(c.CustomerId) && (ids.Count>0)
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(0, count);
			}
		}


		[Test]
		public void NestedQueryWithFilterOnRelatedEntityUsingSubQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				adapter.ParameterisedPrefetchPathThreshold = 5;
				var q = from p in metaData.Product
						where p.Category.CategoryName == "Beverages"
						select new
						{
							p.ProductName,
							OrderDetails = p.OrderDetails
						};

				int count = 0;
				int totalNumberOfOrderDetails = 0;
				foreach(var v in q)
				{
					count++;
					totalNumberOfOrderDetails += v.OrderDetails.Count;
				}
				Assert.AreEqual(11, count);
				Assert.AreEqual(316, totalNumberOfOrderDetails);
			}
		}


		[Test]
		public void NestedQueryWithFilterOnRelatedEntityUsingSubQuery2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				adapter.ParameterisedPrefetchPathThreshold = 5;
				var q1 = from p in metaData.Product
						select new
						{
							p.ProductName,
							Category = new { p.Category.CategoryId, p.Category.CategoryName},
							OrderDetails = p.OrderDetails
						};
				var q = q1.Where(v=>v.Category.CategoryName == "Beverages");

				int count = 0;
				int totalNumberOfOrderDetails = 0;
				foreach(var v in q)
				{
					count++;
					totalNumberOfOrderDetails += v.OrderDetails.Count;
				}
				Assert.AreEqual(11, count);
				Assert.AreEqual(316, totalNumberOfOrderDetails);
			}
		}


		[Test]
		public void OrderByAfterTakePageFetchToArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order select o;
				q = q.TakePage(2,20).OrderBy(o => o.CustomerId);

				int count = 0;
				foreach(OrderEntity o in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Where(c => c.CustomerId == "CHOPS").SelectMany(c => c.Orders);

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual("CHOPS", v.CustomerId);
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void SelectManyWithNoProjectionQuery2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Where(c=>c.CustomerId=="CHOPS").SelectMany(c=>(IEnumerable<OrderEntity>)c.Orders);

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				int employeeid = 4;
				var siblings = from e in metaData.Employee
							   where e.ReportsTo.Value == (from m in metaData.Employee where m.EmployeeId==employeeid select m.ReportsTo.Value).First()
							   select e;
				int count = 0;
				foreach(var v in siblings)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						select od;

				var q2 = q.Where(od => idList.Contains(new Pair<int, int>() { Value1 = od.OrderId, Value2 = od.ProductId }));
				int count = 0;
				foreach(var v in q2)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						select od;

				var q2 = q.Where(od => idList.Contains(new Pair<int, int>(od.OrderId, od.ProductId )));
				foreach(var v in q2)
				{
				}
			}
		}



		[Test]
		public void ManyLeftJoinsResultInSlowCode()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var queryEntity = metaData.Category;

				var q = from _e1 in metaData.Category
						join _e2 in metaData.Category on _e1.CategoryId equals _e2.CategoryId
						into _e1_e2
						from _e2 in _e1_e2.DefaultIfEmpty()

						join _e3 in metaData.Category on _e2.CategoryId equals _e3.CategoryId
						into _e2_e3
						from _e3 in _e2_e3.DefaultIfEmpty()

						join _e4 in metaData.Category on _e3.CategoryId equals _e4.CategoryId
						into _e3_e4
						from _e4 in _e3_e4.DefaultIfEmpty()

						join _e5 in metaData.Category on _e4.CategoryId equals _e5.CategoryId
						into _e4_e5
						from _e5 in _e4_e5.DefaultIfEmpty()

						join _e6 in metaData.Category on _e5.CategoryId equals _e6.CategoryId
						into _e5_e6
						from _e6 in _e5_e6.DefaultIfEmpty()

						join _e7 in metaData.Category on _e6.CategoryId equals _e7.CategoryId
						into _e6_e7
						from _e7 in _e6_e7.DefaultIfEmpty()

						join _e8 in metaData.Category on _e7.CategoryId equals _e8.CategoryId
						into _e7_e8
						from _e8 in _e7_e8.DefaultIfEmpty()

						join _e9 in metaData.Category on _e8.CategoryId equals _e9.CategoryId
						into _e8_e9
						from _e9 in _e8_e9.DefaultIfEmpty()

						join _e10 in metaData.Category on _e9.CategoryId equals _e10.CategoryId
						into _e9_e10
						from _e10 in _e9_e10.DefaultIfEmpty()

						join _e11 in metaData.Category on _e10.CategoryId equals _e11.CategoryId
						into _e10_e11
						from _e11 in _e10_e11.DefaultIfEmpty()

						join _e12 in metaData.Category on _e11.CategoryId equals _e12.CategoryId
						into _e11_e12
						from _e12 in _e11_e12.DefaultIfEmpty()

						join _e13 in metaData.Category on _e12.CategoryId equals _e13.CategoryId
						into _e12_e13
						from _e13 in _e12_e13.DefaultIfEmpty()

						join _e14 in metaData.Category on _e13.CategoryId equals _e14.CategoryId
						into _e13_e14
						from _e14 in _e13_e14.DefaultIfEmpty()

						join _e15 in metaData.Category on _e14.CategoryId equals _e15.CategoryId
						into _e14_e15
						from _e15 in _e14_e15.DefaultIfEmpty()

						select _e1;

				var list = q.ToList();
			}
		}


		[Test]
		public void FirstOrDefaultOnQueryInWhereFollowedByMemberAccess1()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Orders.First(o=>o.OrderDate!=null).Customer.Country=="USA"
						select e;

				int count = 0;
				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Orders.Where(o => o.OrderDate != null).Select(o => o.Customer).First().Country == "USA"
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(8, v.EmployeeId);
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void FirstOrDefaultOnQueryInWhereFollowedByMemberAccess3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Orders.Where(o => o.OrderDate != null).FirstOrDefault().Customer.Country == "USA"
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(8, v.EmployeeId);
				}

				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void MemberAccessOnQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c).Select(c => c.Country);
				
				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void JoinOverIIFConstructs()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						join e in metaData.Employee on (o.EmployeeId == 3 ? 4 : o.EmployeeId == 4 ? 5 : o.EmployeeId == 5 ? 3 : o.EmployeeId) equals e.EmployeeId
						where o.OrderId > 10256 &&
								c.Country == "USA" &&
								e.LastName.StartsWith("A")
						orderby o.OrderId ascending,
								o.OrderDate descending
						select new { o.OrderId, c.CustomerId, c.CompanyName, e.FirstName, e.LastName };

				foreach(var v in q)
				{
					
				}
			}
		}

		[Test]
		public void CustomEntityCollectionFetchWrapperTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				MyLinqMetaData metaData = new MyLinqMetaData(adapter);
				var customers = metaData.AsCollection<CustomerEntity>();
				Assert.AreEqual(91, customers.Count);
			}
		}


		[Test]
		public void FetchingSecondPageUsingOutOfOrderSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Take(10).Skip(10);
				int count = 0;

				foreach(var v in q)
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
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						orderby (c != null) ? c.CompanyName : null
						select c;
				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void GetAllCustomersWithMoreThan3Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Orders.Count() > 3
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(78, count);
			}
		}

		[Test]
		public void CustomProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						where o.OrderId == 10254
						select new OrderEntity()
						{
							OrderId = o.OrderId,
							EmployeeId = o.EmployeeId,
							Customer = new CustomerEntity() { CustomerId = c.CustomerId, CompanyName = c.CompanyName }
						};

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsNotNull(v.Customer);
					Console.WriteLine("{0} {1}", v.OrderId, v.Customer.CustomerId);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetTop10OrdersFromCustomersUsingOrderOnCustomer()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 join c in metaData.Customer on o.CustomerId equals c.CustomerId
						 orderby c.Country ascending
						 select o).Distinct().Take(10);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(10, count);
			}
		}

		[Test]
		public void MultipleTimesSameJoinTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer.Where(c=>c.Country=="USA")
						 join o in metaData.Order on c.CustomerId equals  o.CustomerId
						 select c;

				var q = from o in metaData.Order
						join c in q1 on o.CustomerId equals c.CustomerId
						select new { c.CustomerId, o.OrderId };

				foreach(var v in q)
				{
					
				}
			}
		}

		[Test]
		public void ListOfCollectionsFetch()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Orders.Any(o => o.OrderDetails.Count() > 0)
						select c.CustomerCustomerDemos;
				foreach(var v in q)
				{
					
				}
			}
		}

		
		[Test]
		public void GetOrdersWithMultiHopDefaultIfEmpty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						from o in e.Orders.DefaultIfEmpty()
						from x in o.Customer.CustomerCustomerDemos.DefaultIfEmpty()
						select o;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(818, count);
			}
		}

		
		[Test]
		public void TestWhetherThereAreDiscontinuedProducts()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Product.Any(p => p.Discontinued);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void TestWhetherThereAreDiscontinuedProducts2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Product.Any(p => !p.Discontinued);
				//var result = metaData.Product.Any(p => !(p.CategoryId.Value == 10));
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void AnyWithNegativePredicate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Product.Any(p => !(p.CategoryId.Value == 10));
				Assert.IsTrue(result);
			}
		}


		[Test]
		public void AllWithNegativePredicate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Product.All(p => !(p.CategoryId.Value == 10));
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void AllWithNegativePredicate2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Product.All(p => (p.CategoryId.Value < 10));
				Assert.IsTrue(result);
			}
		}


		[Test]
		public void ContainsOnToLowerTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Customer.Where(c => c.CompanyName.ToLower().StartsWith("a") == true);
				foreach(var v in q)
				{

				}
			}
		}


		[TestFixtureSetUp]
		public void SetupTests()
		{
			Console.WriteLine("{0}.{1}", RuntimeLibraryVersion.Version, RuntimeLibraryVersion.Build);
		}



		#region Helper methods
		internal static IQueryable<Order> GetOrdersInCustomClass(LinqMetaData metaData)
		{
			return from o in metaData.Order
				   select new Order
				   {
					   OrdId = o.OrderId,
					   RelatedCustomer = new Customer
					   {
						   CompanyNme = o.Customer.CompanyName,
						   CustId = o.Customer.CustomerId
					   }
				   };

		}
		#endregion
	}
}
