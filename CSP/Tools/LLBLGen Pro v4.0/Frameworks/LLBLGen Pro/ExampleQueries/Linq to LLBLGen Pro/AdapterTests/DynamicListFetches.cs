using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NW26.Adapter.DatabaseSpecific;
using NW26.Adapter.EntityClasses;
using NW26.Adapter.HelperClasses;
using NW26.Adapter.Linq;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;

using LinqExpression = System.Linq.Expressions.Expression;
using System.Data;
using SD.LLBLGen.Pro.DQE.SqlServer;

namespace LinqTester.AdapterTests
{
	/// <summary>
	/// Unit tests for dynamic list fetches, which are lists of values or anonymous types or fetches of custom types.
	/// </summary>
	[TestFixture]
	public class DynamicListFetches
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}

		/// <summary>
		/// Test which checks if the value projector class works.
		/// </summary>
		[Test]
		public void GetUsingLLBLGenProProjectionListOfValuesUsingValueProjector()
		{
			ResultsetFields fields = new ResultsetFields(1);
			fields.DefineField(OrderFields.OrderId, 0);
			RelationPredicateBucket filter = new RelationPredicateBucket(OrderFields.CustomerId == "CHOPS");
			List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			valueProjectors.Add(new DataValueProjector("OrderId", 0));
			List<int> orderIds = new List<int>();
			DataProjectorToValueList<int> projector = new DataProjectorToValueList<int>(orderIds);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchProjection(valueProjectors, projector, fields, filter, 0, true);
			}

			Assert.AreEqual(8, orderIds.Count);
			List<int> idsToCheckWith = new List<int>() { 10746, 10254, 11041, 10966, 10370, 11029, 10519, 10731 };
			// check if the ids in the list to check are indeed the list to fetch
			var q = from id in orderIds
					where !idsToCheckWith.Contains(id)
					select id;
			Assert.AreEqual(0, q.Count());
		}


		[Test]
		public void GetCountPlusFirst100Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId.StartsWith("C")
						orderby o.CustomerId
						select new
						{
							o.OrderId,
							o.CustomerId,
							o.EmployeeId,
							o.OrderDate
						};
				var count = q.Count();		// should eliminate the OrderBy as it's invalid and unnecessary.
				var results = q.Take(100);

				int cntr = 0;
				foreach(var v in results)
				{
					cntr++;
					Assert.IsTrue(v.CustomerId.StartsWith("C"));
				}
				Assert.AreEqual(23, cntr);
			}
		}


		[Test]
		public void GetCountPlusFirst100OrdersUsingCountColumn()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId.StartsWith("C")
						orderby o.CustomerId
						select new
						{
							o.OrderId,
							o.CustomerId,
							o.EmployeeId,
							o.OrderDate
						};

				var count = q.CountColumn(x=>x.OrderId);
				var results = q.Take(100);

				int cntr = 0;
				foreach(var v in results)
				{
					cntr++;
					Assert.IsTrue(v.CustomerId.StartsWith("C"));
				}
				Assert.AreEqual(23, cntr);
			}
		}


		[Test]
		public void GetFirst100OrdersUsingOrderByOnExistingQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId.StartsWith("C")
						select new
						{
							o.OrderId,
							o.CustomerId,
							o.EmployeeId,
							o.OrderDate
						};

				var results = q.Take(100).Distinct().OrderBy(o=>o.CustomerId);

				int cntr = 0;
				foreach(var v in results)
				{
					cntr++;
					Assert.IsTrue(v.CustomerId.StartsWith("C"));
				}
				Assert.AreEqual(23, cntr);
			}
		}


		/// <summary>
		/// Test using All() as last statement in the query.
		/// </summary>
		[Test]
		public void CheckIfTitleOfCourtesyInRegion1IsSolelyMs()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from r in metaData.Region
						where r.RegionId == 1
						select r.Employees.All(e => e.TitleOfCourtesy == "Ms.");

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValue()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c.Country).Contains("USA");

				Assert.IsTrue(q);

				q = (from c in metaData.Customer
						 select c.Country).Distinct().Contains("USA");

				Assert.IsTrue(q);
			}
		}


		[Test]
		public void CheckIfListOfTuplesObtainedFromCustomersContainsSpecifiedTuple()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName })
								.Contains(new Pair<string, string>("CHOPS", "CHANGED"));
				Assert.IsTrue(q);
				
				q = (from c in metaData.Customer
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName }).Distinct()
								.Contains(new Pair<string, string>("CHOPS", "CHANGED"));
				Assert.IsTrue(q);
			}
		}


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValueUsingAny()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c.Country).Any(l => l == "USA");
				Assert.IsTrue(q);
			
				q = (from c in metaData.Customer
						 select c.Country).Distinct().Any(l => l == "USA");
				Assert.IsTrue(q);
			}
		}


		[Test]
		public void CheckIfListOfCountriesAreAllEqualToUSA()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.Country=="USA"
						 select c.Country).All(l => l == "USA");
				Assert.IsTrue(q);

				q = (from c in metaData.Customer
						 where c.Country == "USA"
						 select c.Country).Distinct().All(l => l == "USA");
				Assert.IsTrue(q);
			}
		}


		[Test]
		public void CheckIfListOfTuplesObtainedFromCustomersContainsSpecifiedTupleUsingAnyCall()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName })
								.Any(x => x == new Pair<string, string>("ALFKI", "Alfreds Futterkiste"));
				Assert.IsTrue(q);

				q = (from c in metaData.Customer
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName }).Distinct()
								.Any(x => x == new Pair<string, string>("ALFKI", "Alfreds Futterkiste"));
				Assert.IsTrue(q);
			}
		}


		[Test]
		public void GetOrderTotalOfOrder10254()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = (from od in metaData.OrderDetail
						where od.OrderId == 10254
						select od.Quantity * od.UnitPrice).Sum();

				Assert.AreEqual(9987.6M, q);
			}
		}


		/// <summary>
		/// Gets the customer names which have an order filed by employee 3 using correlation relation and filter on related entity.
		/// The projection of the query has a correlation relation in one of the fields. This relation has to be propagated to the main query, otherwise
		/// the query won't include the join on customer.
		/// </summary>
		[Test]
		public void GetCustomerNamesWhichHaveAnOrderFiledByEmployee3UsingCorrelationRelationAndFilterOnRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = (from o in metaData.Order
						where o.EmployeeId == 3
						select o.Customer.CompanyName).Distinct();

				IList results = q.ToList();
				Assert.AreEqual(60, results.Count);
			}
		}


		/// <summary>
		/// Test which fetches a list of single values (company names) without any filter.
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetNoFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(from c in metaData.Customer
										select c.CompanyName);
				List<string> companyNames = (List<string>)q.Execute();
				Assert.AreEqual(91, companyNames.Count);
			}
		}


		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithoutDistinct()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c.City);
				List<string> cities = (List<string>)q.Execute();
				Assert.AreEqual(13, cities.Count);
			}
		}


		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinct()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c.City).Distinct();
				List<string> cities = (List<string>)q.Execute();
				// Portland is duplicated, which should be filtered out.
				Assert.AreEqual(12, cities.Count);
			}
		}


		/// <summary>
		/// Test which fetches page 2 from all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default).
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinctAndPaging()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c.City).Distinct().TakePage(3, 5);
				List<string> cities = (List<string>)q.Execute();
				// Portland is duplicated, which should be filtered out. This means that page 3 has just 2 rows.
				Assert.AreEqual(2, cities.Count);
			}
		}


		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinctAndPagingUsingSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c.City).Distinct().Skip(10).Take(5);
				List<string> cities = (List<string>)q.Execute();
				// Portland is duplicated, which should be filtered out. This means that page 3 has just 2 rows.
				Assert.AreEqual(2, cities.Count);
			}
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all cities for customers in the USA).
		/// </summary>
		[Test]
		public void GetValueListWithMaxLimitAndFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				ILLBLGenProQuery q = (ILLBLGenProQuery)(
										from c in metaData.Customer
										where c.Country == "USA"
										select c.City).Take(5);
				List<string> cities = (List<string>)q.Execute();
				Assert.AreEqual(5, cities.Count);
			}
		}


		[Test]
		public void GetAnonymousTypedListFromCustomerAndOrderData()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						where c.Country == "Germany"
						select new { Company = c.CompanyName, OrderID = o.OrderId };

				int counter =0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(121, counter);
			}
		}


		/// <summary>
		/// Test which uses 'let' to store a value of a field, where null values are replaced by a string, which is later on used as a filter. 
		/// </summary>
		[Test]
		public void LeftJoinEmulatingQueryWithLet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let x = c.Region ?? "Undef"
						where (c.Country != "Germany")
							&& x != "Undef"
						select new { CompName = c.CompanyName };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(34, counter);
			}
		}


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK. 
		/// The difference here is that it uses a Where instead of a filter in the Count.
		/// This is done by applying a count on the Orders 
		/// </summary>
		[Test]
		public void GetAllCustomerIDsWithTheirTotalNumberOfOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new { CustomerID = c.CustomerId, NumberOfOrders = c.Orders.Count() };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
					Console.WriteLine("CustomerID: {0}. NumberOfOrders: {1}", v.CustomerID, v.NumberOfOrders);
				}
				Assert.AreEqual(91, counter);
			}
		}


		/// <summary>
		/// Gets the maximum order total. This is a test to see if Max works in a single value return setting.
		/// </summary>
		[Test]
		public void GetTheMaximumOrderTotalUsingAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						select new { c.CustomerId, TotalAllOrders = c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)) }
						).Max(n=>n.TotalAllOrders);

				Assert.AreEqual(112804.1900, q);
			}
		}


		/// <summary>
		/// Gets the maximum order total. This is a test to see if Max works in a single value return setting.
		/// </summary>
		[Test]
		public void GetTheMaximumOrderTotalUsingMaxCallOnSequenceOfValues()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))
						).Max();

				Assert.AreEqual(112804.1900, q);
			}
		}


		/// <summary>
		/// Gets the total count of distinct employee ids in orders.Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfDistinctEmployeeIdsInOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.CountColumn(o=>o.EmployeeId, true);

				Assert.AreEqual(9, q);
			}
		}


		/// <summary>
		/// Gets the total count of all employee ids in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfAllEmployeeIdsInOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.CountColumn(o => o.EmployeeId);

				Assert.AreEqual(818, q); // equal to the # of rows.
			}
		}


		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheStandardDeviationOverEmployeeIDsInOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.StandardDeviation(o => o.EmployeeId);

				Assert.AreEqual(2.4470306717636521, q); 
			}
		}


		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheVarianceOverEmployeeIDsInOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.Variance(o => o.EmployeeId);

				Assert.AreEqual(5.9879591085520705, q);
			}
		}


		/// <summary>
		/// Gets the list of customer ids plus order total for customers with more than10 orders using duplicated aggregate method calls.
		/// </summary>
		[Test]
		public void GetListOfCustomerIdsPlusOrderTotalForCustomersWithMoreThan10OrdersUsingDuplicatedAggregateMethodCalls()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Orders.Count() > 10
						select new { c.CustomerId, TotalAmountOrders = c.Orders.Count() };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(27, counter);
			}
		}


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by. Simple Groupby test
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						select new { Country = g.Key, NumberOfCustomers = g.Count() };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
					Console.WriteLine("Country: {0}. # of customers: {1}", v.Country, v.NumberOfCustomers);
				}
				Assert.AreEqual(23, counter);
			}
		}


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by ordered by country. Simple groupby test with orderby 
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupByOrderedByCountry()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						orderby g.Key
						select new { Country = g.Key, NumberOfCustomers = g.Count() };

				int counter = 0;
				string previous = string.Empty, current = string.Empty;
				foreach(var v in q)
				{
					previous = current;
					current = v.Country;
					if(current == null)
					{
						current = string.Empty;
					}
					Assert.IsTrue(current.CompareTo(previous) >= 0);
					counter++;
				}
				Assert.AreEqual(23, counter);
			}
		}


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by ordered by country filtered on null. simple but where has to be
		/// outside the groupby.
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupByOrderedByCountryFilteredOnNull()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						orderby g.Key
						where g.Key == null
						select new { Country = g.Key, NumberOfCustomers = g.Count() };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(1, counter);
			}
		}


		[Test]
		public void GetValueListFromJoinedSetWithIntoAndWithoutSpecifyingDefaultIfEmpty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into co
						from x in co
						select x.CustomerId;

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(818, counter);
			}
		}


		/// <summary>
		/// Gets the list of countries with the number of customers who have purchased more than10 orders. This is a test
		/// with groupby field injection from where AND projection which requires folding of groupby query into inner derived table. 
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheNumberOfCustomersWhoHavePurchasedMoreThan10Orders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						orderby g.Key
						where g.Sum(n => n.Orders.Count()) > 10
						select new { Country = g.Key, NumberOfCustomers = g.Count() };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(18, counter);
			}
		}


		/// <summary>
		/// Complicated query which tests the folding of the inner query for the groupby, as it has to fold twice, (Sum and Avg both force it to fold).
		/// </summary>
		[Test]
		public void GetComplicatedListWithTwoAggregatesOnAGroupBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by o.Customer.Country into g
						orderby g.Key
						where g.Sum(n => n.OrderDetails.Count()) > 10
						select new { Country = g.Key, Num = g.Average(n => n.OrderDetails.Count(od => od.ProductId == 3)) };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(22, counter);
			}
		}


		[Test]
		public void GetComplicatedListWithTwoAggregatesOnAGroupBy2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by o.Customer.Country into g
						orderby g.Key
						where g.Sum(n => n.OrderDetails.Count()) > 10
						select new { Country = g.Key, Num = g.Average(n => n.EmployeeId) };

				int counter = 0;
				foreach(var v in q)
				{
					counter++;
				}
				Assert.AreEqual(22, counter);
			}
		}


		[Test]
		public void GetSetOfTuplesOfConcreteClassesFromProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						select new KeyValuePair<string, string>(c.Country, c.City);

				List<KeyValuePair<string, string>> results = q.ToList();
				Assert.AreEqual(91, results.Count);
			}
		}


		[Test]
		public void GetAnonymousTypedListFromCustomersInLondonUsingJoinWithNestedLetAndWhereInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						let m = c.Phone
						orderby c.City
						where c.Country == "UK"
						where m != "555-5555"
						select new { c.City, c.ContactName } into a
						where a.City == "London"
						select a;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual("London", v.City);
				}
				Assert.AreEqual(46, count);		// duplicates through join with orders.
			}
		}


		[Test]
		public void GetAnonymousTypedListFromCustomersInLondonUsingJoinWithSubqueryAndWhereInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						orderby c.City
						where c.Country == "UK"
						select new { c.City, c.ContactName } into a
						where a.City == "London"
						select a;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual("London", v.City);
				}
				Assert.AreEqual(46, count);		// duplicates through join with orders.
			}
		}


		[Test]
		public void GetStringFromCustomerBasedOnIIFStatementInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.Country,
							CheckValue = c.Country == "USA" ? c.ContactName : c.ContactTitle,
							c.ContactName,
							c.ContactTitle
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Country == "USA" ? v.ContactName : v.ContactTitle, v.CheckValue);
				}
			}
		}


		/// <summary>
		/// Tests InvokeExpression handling
		/// </summary>
		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Func<string, string> stringChopper = s=>s.Substring(0, 3);

				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select stringChopper(c.CompanyName);

				foreach(var v in q)
				{
					Assert.AreEqual(3, v.Length);
				}
						
			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelation()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Func<string, string> stringChopper = s => s.Substring(0, 3);

				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 select stringChopper(o.Customer.CompanyName)).Distinct();

				foreach(var v in q)
				{
					Assert.AreEqual(3, v.Length);
				}

			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelationIntoAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Func<string, string> stringChopper = s => s.Substring(0, 3);

				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 select new { FirstChars = stringChopper(o.Customer.CompanyName), 
									 o.Customer.CompanyName }).Distinct();

				foreach(var v in q)
				{
					Assert.AreEqual(3, v.FirstChars.Length);
					Assert.AreEqual(v.FirstChars, v.CompanyName.Substring(0, 3));
				}
			}
		}


		[Test]
		public void GetListOfBooleansProducedFromBooleanExpressionInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country=="USA"
						select (c.Country == "USA");

				foreach(var v in q)
				{
					Assert.IsTrue(v);
				}
			}
		}


		[Test]
		public void GetAnonymousTypedListWithBooleanExpressionInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.CustomerId,
							IsAmerican = (c.Country == "USA"),
							c.Country
						};

				foreach(var v in q)
				{
					Assert.AreEqual((v.Country == "USA"), v.IsAmerican);
				}
			}
		}
		

		[Test]
		public void ConcatTwoFetchedValuesByAnInstanceMethodOfANewObject()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.Country,
							c.City,
							ConcatenatedResult = new Concatenator(c.Country, c.City).ConcatMembers()
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Country + v.City, v.ConcatenatedResult);
				}
			}
		}


		[Test]
		public void ConcatTwoFetchedValuesByAnInstancePropertyOfANewObject()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.Country,
							c.City,
							ConcatenatedResult = new Concatenator(c.Country, c.City).ConcattedResult
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Country + v.City, v.ConcatenatedResult);
				}
			}
		}



		[Test]
		public void ConcatTwoFetchedValuesByAStaticMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.Country,
							c.City,
							ConcatenatedResult = Concatenator.Concat(c.Country, c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Country + v.City, v.ConcatenatedResult);
				}
			}
		}

	
		[Test]
		public void ConcatTwoFetchedValuesByAnInstanceMethodOfAnExistingObject()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Concatenator concatter = new Concatenator();
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.Country,
							c.City,
							ConcatenatedResult = concatter.ConcatValues(c.Country, c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Country + v.City, v.ConcatenatedResult);
				}
			}
		}


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryReferredToByLetByAStaticMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						let x = c.Orders.Count()
						select new
						{
							c.City,
							AmountOrders = x,
							ConcatenatedResult = Concatenator.Concat(x.ToString(), c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.AmountOrders + v.City, v.ConcatenatedResult);
				}
			}
		}

	
		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryByAStaticMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.City,
							AmountOrders = (from o in c.Orders select o).Count(),
							ConcatenatedResult = Concatenator.Concat((from o in c.Orders select o).Count().ToString(), c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.AmountOrders + v.City, v.ConcatenatedResult);
				}
			}
		}


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryConvertedToABooleanByAStaticMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.City,
							AmountOrders = (from o in c.Orders select o).Count(),
							ConcatenatedResult = Concatenator.Concat(((from o in c.Orders select o).Count() > 4).ToString(), c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual((v.AmountOrders > 4).ToString() + v.City, v.ConcatenatedResult);
				}
			}
		}

		
		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingContainsQueryConvertedToAStringByAStaticMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country != null
						select new
						{
							c.City,
							Contains3 = (from o in c.Orders select o.EmployeeId).Contains(3),
							ConcatenatedResult = Concatenator.Concat((from o in c.Orders select o.EmployeeId).Contains(3).ToString(), c.City)
						};

				foreach(var v in q)
				{
					Assert.AreEqual(v.Contains3.ToString() + v.City, v.ConcatenatedResult);
				}
			}
		}


		[Test]
		public void GetNestedTreeOfCustomerOrderOrderDetailEmployeeProductInfoUsingNestedQueriesFilteredOnCustomersFromGermany()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country == "Germany"
						select new
						{
							c.CompanyName,
							c.City,
							Orders = from o in c.Orders
									 select new
									 {
										 OrderDetails = from od in o.OrderDetails
														select new
														{
															od.Product,
															od.Quantity,
															od.UnitPrice
														},
										 o.Employee,
										 o.OrderDate
									 }
						};

				int count = 0;
				foreach(var v in q)
				{
					int orderCount = 0;
					foreach(var orderData in v.Orders)
					{
						int orderDetailsCount = 0;
						foreach(var orderDetailsData in orderData.OrderDetails)
						{
							orderDetailsCount++;
							Assert.IsNotNull(orderDetailsData.Product);
						}
						Assert.IsTrue(orderDetailsCount > 0);
						Assert.IsNotNull(orderData.Employee);
						orderCount++;
					}
					Assert.IsTrue(orderCount > 0);
					count++;
				}
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void GetListOfOrderCollectionsViaCustomer()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where !new List<string>(){"FISSA", "PARIS"}.Contains(c.CustomerId)
						select c.Orders;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Count > 0);		// v is of type EntityCollection<OrderEntity> :)
					Assert.IsTrue(v[0].CustomerId.Length > 0);
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetListOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						where o.EmployeeId == 2
						select o.Customer).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.CompanyName.Length > 0);
				}
				Assert.AreEqual(62, count);
			}
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from o in metaData.Order
						 where o.EmployeeId == 2
						 select o.Customer).Count();

				Assert.AreEqual(110, count);
			}
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2WithDistinct()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from o in metaData.Order
							 where o.EmployeeId == 2
							 select o.Customer).Distinct().Count();

				Assert.AreEqual(62, count);
			}
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2InAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from o in metaData.Order
							 where o.EmployeeId == 2
							 select new
							 {
								o.Customer
							 }).Count();

				Assert.AreEqual(110, count);
			}
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2InAnonymousTypeWithDistinct()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from o in metaData.Order
							 where o.EmployeeId == 2
							 select new
							 {
								 o.Customer
							 }).Distinct().Count();

				Assert.AreEqual(62, count);
			}
		}
		

		[Test]
		public void GetDistinctSetOfCompanyNamesAndEmployeesWhoFiledTheirOrders()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 select new
						 {
							 o.EmployeeId,
							 CompanyName = o.Customer.CompanyName
						 }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.CompanyName.Length > 0);
					Assert.IsTrue((v.EmployeeId > 0) && (v.EmployeeId < 10));
				}
				Assert.AreEqual(472, count);
			}
		}


		[Test]
		public void GetDistinctSetOfCompanyNamesAndEmployeesWhoFiledTheirOrdersUsingNestedQueryForCompanyName()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 orderby o.EmployeeId ascending, o.CustomerId ascending
						 select new
						 {
							 o.EmployeeId,
							 CompanyNameValue = (from c in metaData.Customer where c.CustomerId == o.CustomerId select new { c.CompanyName }).Single()
						 }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.CompanyNameValue.CompanyName.Length > 0);
					Assert.IsTrue((v.EmployeeId > 0) && (v.EmployeeId < 10));
				}
				Assert.AreEqual(472, count);
			}
		}


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							o.OrderId,
							CompanyName = o.Customer.CompanyName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.CompanyName.Length > 0);
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							o.OrderId,
							CompanyName = (from c in metaData.Customer where c.CustomerId == o.CustomerId select c.CompanyName).Single()
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.CompanyName.Length > 0);
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetEmployeeFullnameRelatedToOrder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							EmployeeName = o.Employee.FirstName + o.Employee.LastName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.EmployeeName.Length > 0);
				}
				Assert.AreEqual(818, count);
			}
		}
		

		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingNestedQueryWithoutLimiter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							o.OrderId,
							CompanyName = (from c in metaData.Customer where c.CustomerId == o.CustomerId select c.CompanyName).ToList()
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(v.OrderId == 10253)
					{
						// case difference
						Assert.AreEqual(0, v.CompanyName.Count);
					}
					else
					{
						Assert.IsTrue(v.CompanyName[0].Length > 0);
					}
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetCustomerDataFromGermanyAndTheirLast3OrderDataInNestedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country == "Germany"
						select new
						{
							c.City,
							LastFiveOrders = (from o in c.Orders orderby o.OrderDate descending select new { o.OrderId, o.OrderDate }).Take(3),
							c.CompanyName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					int orderCount = v.LastFiveOrders.Count();
					Assert.IsTrue((orderCount > 0) && orderCount<=3);
				}
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void GetCustomerDataAndNestedOrderList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.Country,
							Orders = c.Orders,
							c.CompanyName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if((v.CompanyName == "FISSA Fabrica Inter. Salchichas S.A.") || (v.CompanyName == "Paris spécialités"))
					{
						// no orders
						continue;
					}
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetCustomerIdsWithOrdersInAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.CustomerId,
							c.CompanyName,
							Orders = metaData.Order.Where(o => o.CustomerId == c.CustomerId)
						};
				q = q.Select(i => i).Take(5);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if((v.CompanyName == "FISSA Fabrica Inter. Salchichas S.A.") || (v.CompanyName == "Paris spécialités"))
					{
						// no orders
						continue;
					}
					Assert.IsTrue(v.Orders.Count() > 0);
				}
				Assert.AreEqual(5, count);
			}
		}

		[Test]
		public void GetCustomerDataAndNestedOrderListUsingParameterizedNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				adapter.ParameterisedPrefetchPathThreshold = 100;
				var q = from c in metaData.Customer
						select new
						{
							c.Country,
							Orders = c.Orders,
							c.CompanyName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if((v.CompanyName == "FISSA Fabrica Inter. Salchichas S.A.") || (v.CompanyName == "Paris spécialités"))
					{
						// no orders
						continue;
					}
					Assert.IsTrue(v.Orders.Count > 0);
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetListOfCompanyNamesUsingCombinationOfMultipleSeparatedQueries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = metaData.Customer;
				var q2 = q1.Where(c => c.Country == "Germany");
				var q3 = q2.Where(c => c.Orders.Count() > 7);
				var q4 = q3.Select(c => c.CompanyName);

				int count = 0;
				foreach(var v in q4)
				{
					count++;
					Console.WriteLine(v);
				}
				Assert.AreEqual(6, count);
			}
		}


		[Test]
		public void GetListOfCustomersUsingValueListProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new CustomerEntity { CompanyName = c.CompanyName, CustomerId = c.CustomerId };

				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.City.Length == 0); // not fetched.
					Assert.IsTrue(v.CompanyName.Length > 0);
					count++;
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetMinIdAndMaxIdUsingGroupOnConstantFromCustomers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country == "USA"
						group c by c.Country into cc
						select new { MinId = cc.Min(c => c.CustomerId), MaxId = cc.Max(c => c.CustomerId) };

				foreach(var v in q)
				{
					Assert.AreEqual("GREAL", v.MinId);
					Assert.AreEqual("WHITC", v.MaxId);
				}
			}
		}


		[Test]
		public void GetCountOfCustomersAndMaxIdUsingGroupOnConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country=="USA"
						group c by 0 into cc
						select new { Key = cc.Key, Count = cc.Count(), MaxId = cc.Max(c=>c.CustomerId)};

				foreach(var v in q)
				{
					Assert.AreEqual(13, v.Count);
					Assert.AreEqual("WHITC", v.MaxId);
				}
			}
		}


		[Test]
		public void GetCountOfCustomersUsingGroupOnConstantIntoObjectArray()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country == "USA"
						group c by 0 into cc
						select new object[] { cc.Count(), cc.Max(c => c.CustomerId) };

				object[][] values = q.ToArray();

				Assert.AreEqual(13, values[0][0]);
				Assert.AreEqual("WHITC", values[0][1]);
			}
		}


		[Test]
		public void GetCompanyNameAndCustomerIdInStringArrays()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						select new string[] { c.CustomerId, c.CompanyName };

				string[][] values = q.ToArray();

				Assert.AreEqual(91, values.Length);
				foreach(string[] customerData in values)
				{
					Assert.IsTrue(customerData[0].Length > 0);
					Assert.IsTrue(customerData[1].Length > 0);
				}
			}
		}


		[Test]
		public void GetCompanyNameAndCustomerIdInStringListObjects()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						select new List<string>() { c.CustomerId, c.CompanyName };

				List<string>[] values = q.ToArray();

				Assert.AreEqual(91, values.Length);
				foreach(List<string> customerData in values)
				{
					Assert.IsTrue(customerData[0].Length > 0);
					Assert.IsTrue(customerData[1].Length > 0);
				}
			}
		}


		[Test]
		public void GetOrderDataWithTotalNumberOfProductsBoughtBelowGivenPrice()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							o.OrderId,
							o.OrderDate,
							AmountProductsBought = o.OrderDetails.Where(od=>od.UnitPrice<2.0M).Sum(od=>od.Quantity)
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetOrderDataFromOrdersWithMoreThan1ProductBoughtBelowGivenPriceUsingWrappedWhereAndPaging()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						select new
						{
							o.OrderId,
							ShippedDate = o.ShippedDate ?? DateTime.Now,
							AmountProductsBought = o.OrderDetails.Where(od => od.UnitPrice < 10.0M).Sum(od => od.Quantity)
						} into result
						where result.AmountProductsBought > 1
						select result).TakePage(2,5);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(5, count);
			}
		}


		[Test]
		public void GetListOfAnonymousTypesWithNestedAnonymousTypesOfCustomerData()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							Address = new { c.City, c.Address, c.Country },
							ContactInfo = new { c.ContactTitle, c.ContactName, c.Phone, c.Fax },
							Foo = true,
							c.CompanyName,
							c.CustomerId
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Foo);
					Assert.IsNotNull(v.Address);
					Assert.IsNotNull(v.ContactInfo);
					Assert.IsTrue(v.CompanyName.Length > 0);
					Assert.IsTrue(v.CustomerId.Length > 0);
					Assert.IsTrue(v.Address.City.Length > 0);
					Assert.IsTrue(v.Address.Address.Length > 0);
					Assert.IsTrue(v.ContactInfo.ContactTitle.Length > 0);
					Assert.IsTrue(v.ContactInfo.ContactName.Length > 0);
					Assert.IsTrue(v.ContactInfo.Phone.Length > 0);
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GroupByOnMultipleFieldsWithReferenceToKeyInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						group c by new { c.Country, c.City } into g
						select new { SomeKey = g.Key, NumberOfCustomers = g.Count() };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(70, count);
			}
		}


		[Test]
		public void GetCountriesWithNumberOfCitiesUsingGroupByOnSelectIntoSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						where c.Country != null
						select new { c.Country, c.City } into cv
						group cv by cv.Country into g
						select new { Country = g.Key, NumberOfCities = g.Count() };
				
				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(22, count);
			}
		}


		[Test]
		public void GroupByWithCompoundKeyAndFieldInRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from o in metaData.Order
						group o by new { o.CustomerId, o.Customer.Country } into g
						select g.Key;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GroupByWithAggregateOnNullableTypeWithCoalesce()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from o in metaData.Order
						group o by o.CustomerId into g
						select new { ID = g.Key, Sum = g.Sum(x => (x.EmployeeId ?? 0)) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(89, count);
			}
		}
		

		[Test]
		public void SimpleValueListSelectionOfAllCountriesInCustomers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 where c.Country != null
						 select c.Country).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(22, count);
			}
		}


		[Test]
		public void GroupByWithCompoundFieldSetAndAggregateOnRelatedEntityInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
							  group o by new { o.OrderDate, o.Customer.City } into g
							  select new { Key = g.Key, Sum = g.Sum(o => (o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(810, count);
			}
		}


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetAndAggregateOnRelatedEntityInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by new { o.Customer.City} into g
						select new { Key = g.Key, Sum = g.Sum(o => (o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(69, count);
			}
		}

		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetOfCombinedFieldsAndAggregateOnRelatedEntityInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by new { CombinedName = o.Customer.City + o.Customer.Country } into g
						select new { Key = g.Key , Sum = g.Sum(o => (o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(70, count);
			}
		}


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetOfCombinedFieldsAndAggregate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by new { CombinedName = o.Customer.City + o.Customer.Country } into g
						select new { Key = g.Key, Sum = g.Sum(o => o.EmployeeId) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(70, count);
			}
		}


		[Test]
		public void GroupByOnFieldInRelatedEntityWithAggregateOnFieldInOtherRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
							  where o.OrderId < 10600
							  group o by o.CustomerId into g
							  select new { Key = g.Key, Data = g.Max(a => a.Employee.BirthDate) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(83, count);
			}
		}


		[Test]
		public void GroupByOnJoinedRelatedEntityWithDefaultIsEmpty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into og
						from o in og.DefaultIfEmpty()
						group o by new
						{
							c.CustomerId,
							c.CompanyName
						} into g
						select new
						{
							Customer = g.Key,
							LastOrderDate = g.Max(d => d.OrderDate),
							TotalOrders = g.Count()
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.TotalOrders > 0);
				}

				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void MultiLeftJoinWithDistinctFetchWhereProjectionOfJoinShouldBeUsedInResultQueryExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
							join o in metaData.Order on c.CustomerId equals o.CustomerId into og
							from o in og.DefaultIfEmpty()
							join od in metaData.OrderDetail on o.OrderId equals od.OrderId into odg
							from od in og.DefaultIfEmpty()
							select new
							{
								c.CustomerId,
								od.OrderDate
							}
						).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.IsNull(v.OrderDate);
					}
					else
					{
						Assert.IsTrue(v.OrderDate.HasValue);
					}
				}
				Assert.AreEqual(813, count);
			}
		}


		[Test]
		public void LeftJoinWithIIFExpressionInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into og
						from o in og.DefaultIfEmpty()
						select new
						{
							c.CustomerId,
							c.CompanyName,
							HasOrdered = (o.CustomerId != null) ? "Yes they have" : "No they havent"
						}).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.AreEqual("No they havent", v.HasOrdered);
					}
					else
					{
						Assert.AreEqual("Yes they have", v.HasOrdered);
					}
				}
				Assert.AreEqual(91, count);

			}
		}


		[Test]
		public void LeftJoinWithIIFExpressionWithResultIntoHardNullValueInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 join o in metaData.Order on c.CustomerId equals o.CustomerId into og
						 from o in og.DefaultIfEmpty()
						 select new
						 {
							 c.CustomerId,
							 c.CompanyName,
							 HasOrdered = (o.CustomerId != null) ? "Yes they have" : null
						 }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.IsNull(v.HasOrdered);
					}
					else
					{
						Assert.AreEqual("Yes they have", v.HasOrdered);
					}
				}
				Assert.AreEqual(91, count);

			}
		}



		[Test]
		public void LeftJoinWithIIFExpressionInsideCoalesceStatementInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 join o in metaData.Order on c.CustomerId equals o.CustomerId into og
						 from o in og.DefaultIfEmpty()
						 select new
						 {
							 c.CustomerId,
							 c.CompanyName,
							 o.ShipRegion,
							 HasOrdered = o.ShipRegion ?? ((o.CustomerId != null) ? "Yes they have" : "No they havent")
						 }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.AreEqual("No they havent", v.HasOrdered);
					}
					else
					{
						if(v.ShipRegion == null)
						{
							Assert.AreEqual("Yes they have", v.HasOrdered);
						}
						else
						{
							Assert.AreEqual(v.ShipRegion, v.HasOrdered);
						}
					}
				}
				Assert.AreEqual(91, count);

			}
		}


		/// <summary>
		/// It's not really useful to test this on case insensitive catalogs.
		/// </summary>
		[Test]
		public void OrderDetailsFetchWithDerivedTableAsOneSideOfJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.EmployeeId == 4
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						where od.ProductId == 57
						select new
						{
							o.OrderId,
							od.ProductId
						};
				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void JoinOnCharExpression()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join e in metaData.Employee on c.CustomerId[0] equals e.LastName[0]
						select new { c.CustomerId, e.EmployeeId, e.LastName };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(v.CustomerId[0], v.LastName[0]);
				}
				Assert.AreEqual(43, count);
			}
		}


		[Test]
		public void JoinOnConstantChar()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join e in metaData.Employee on c.CustomerId[0] equals 'C'
						select new { c.CustomerId, e.EmployeeId, e.LastName };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual('C', v.CustomerId[0]);
				}
				Assert.AreEqual(45, count);
			}
		}
		

		[Test]
		public void OrderInfoFetchWith3JoinsToRelatedEntities()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from e in metaData.Employee
						join e2 in metaData.Employee on e.EmployeeId equals e2.ReportsTo into j1
						join o in metaData.Order on e.EmployeeId equals o.EmployeeId into j2
						join et in metaData.EmployeeTerritory on e.EmployeeId equals et.EmployeeId into j3
						from x1 in j1.DefaultIfEmpty()
						from x2 in j2.DefaultIfEmpty()
						from x3 in j3.DefaultIfEmpty()
						select new { e.LastName, e.EmployeeId, Manager = x1.LastName, x2.OrderId, x3.TerritoryId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(5791, count);
			}
		}


		[Test]
		public void GroupingOfEntitiesByGroupByKeyIntoAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						select new
						{
							g.Key,
							CustomersInCountry = (
							from c2 in metaData.Customer
							where g.Key == c2.Country
							select c2)
						};

				int count = 0;
				foreach(var v in q)
				{
					int customerCount = 0;
					foreach(var c in v.CustomersInCountry)
					{
						Assert.AreEqual(v.Key, c.Country);
						customerCount++;
					}
					if(v.Key == null)
					{
						Assert.AreEqual(0, customerCount);
					}
					else
					{
						Assert.IsTrue(customerCount > 0);
					}
					count++;
				}
				Assert.AreEqual(23, count);
			}
		}


		[Test]
		public void GroupingOfEntitiesByGroupByKeyIntoAnonymousTypeUsingGrouping()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						group c by c.Country into g
						select new
						{
							g.Key,
							CustomersInCountry=g
						};

				int count = 0;
				foreach(var v in q)
				{
					int customerCount = 0;
					foreach(var c in v.CustomersInCountry)
					{
						Assert.AreEqual(v.Key, c.Country);
						customerCount++;
					}
					if(v.Key == null)
					{
						Assert.AreEqual(0, customerCount);
					}
					else
					{
						Assert.IsTrue(customerCount > 0);
					}
					count++;
				}
				Assert.AreEqual(23, count);
			}
		}



		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariable()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where c.Country!=null
						group c by c.Country into g
						select g;
				int count = 0;
				foreach(var v in q)
				{
					int customerCount = 0;
					foreach(var c in v)
					{
						Assert.AreEqual(v.Key, c.Country);
						customerCount++;
					}
					Assert.IsTrue(customerCount > 0);
					count++;
				}
				Assert.AreEqual(22, count);
			}
		}
		

		[Test]
		public void GetMaxOrdersPerCustomerUsingJoinWithGroupedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q2 = from o in metaData.Order
						 where o.OrderDate <= new DateTime(1997, 8, 1)
						 && o.EmployeeId != null
						 group o by o.CustomerId into g
						 select new { LastId = g.Max(x => x.OrderId) };

				var q1 = from o in metaData.Order
						join ids in q2
						on o.OrderId equals ids.LastId
						select o;

				var q = from od in metaData.OrderDetail
						join o in q1 on od.OrderId equals o.OrderId
						group od by od.ProductId into g
						select new { g.Key, Total = g.Sum(od => od.Quantity) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(66, count);
			}
		}

		[Test]
		public void GetPageOfCustomerDataJoinedWithOrdersOrderedByTwoFields()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						select new { CustomerId = c.CustomerId, CompanyName = c.CompanyName, OrderId = o.OrderId, OrderDate = o.OrderDate };
				var q2 = q.OrderByDescending(c2 => c2.OrderDate).ThenByDescending(c2 => c2.CompanyName).TakePage(2, 5);

				int count = 0;
				foreach(var v in q2)
				{
					count++;
				}
				Assert.AreEqual(5, count);
			}
		}


		[Test]
		public void GetCountOfJoinedSetWithAnonymousTypes()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						select new { CustomerId = c.CustomerId, CompanyName = c.CompanyName, OrderId = o.OrderId, OrderDate = o.OrderDate };
				int count = q.Count();
				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void GetAnonymousTypeWithIIFAndWrappedWhereOnResultOfIIF()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from o in metaData.Order
						 select new
						 {
							 o.OrderId,
							 HasRegion = (o.ShipRegion == null) ? true : false
						 };

				var q2 = q1.Where(a => a.HasRegion == true);

				int count = 0;
				foreach(var v in q2)
				{
					count++;
				}
				Assert.AreEqual(498, count);
			}
		}


		[Test]
		public void GetCountUsingMethodCallOnQueryable()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer select c;
				int count = (int)q.Provider.Execute(LinqExpression.Call(typeof(Queryable), "Count", new Type[] { q.ElementType }, q.Expression));

				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetCountOfAnonymousTypeInstancesFromCustomers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from c in metaData.Customer 
							 select new { CustomerId = c.CustomerId, CompanyName = c.CompanyName }).OrderBy(x=>x.CustomerId).Count();

				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GetCountOfAnonymousTypeInstancesFromCustomers2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var count = (from c in metaData.Customer
							 where c.Country=="Germany"
							 select c.CustomerId).Count();

				Assert.AreEqual(11, count);
			}
		}

		[Test]
		public void FetchOneSideOfJoinBetweenTwoSeparateQueries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var cq = from c in metaData.Customer
						 select new { c.CustomerId, c.CompanyName };

				var oq = from o in metaData.Order
						 select new { o.OrderId, o.CustomerId, o.EmployeeId, o.OrderDate };

				var q = (from c in cq
						 join o in oq on c.CustomerId equals o.CustomerId
						 where o.EmployeeId == 3
						 select c).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(60, count);
			}
		}


		[Test]
		public void FetchOneSideOfJoinBetweenTwoSeparateQueriesUsingCrossJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var cq = from c in metaData.Customer
						 select new { Cid= c.CustomerId, CName = c.CompanyName };

				var oq = from o in metaData.Order
						 select new { Oid = o.OrderId, OCId = o.CustomerId, OEid = o.EmployeeId, ODate = o.OrderDate };

				var q = (from c in cq
						 from o in oq 
						 where c.Cid == o.OCId
						 &&  o.OEid == 3
						 select c).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(60, count);
			}
		}


		[Test]
		public void JoinBetweenTwoSeparateQueriesIntoNewAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var cq = from c in metaData.Customer
						 select new { c.CustomerId, c.CompanyName };

				var oq = from o in metaData.Order
						 select new { o.OrderId, o.CustomerId, o.EmployeeId, o.OrderDate };

				var q = from c in cq
						 join o in oq on c.CustomerId equals o.CustomerId
						 where o.EmployeeId == 3
						 select new { CustomerData = c, OrderData = o };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(115, count);		// join creates duplicates which can't be filtered out as orderid is present in final resultset.
			}
		}

		[Test]
		public void JoinBetweenTwoSeparateQueriesWithOneUsingCustomClassIntoNewAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in GetOrderData(metaData)
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						where c.Country == "USA"
						select o;

				int count=0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(121, count);
			}
		}


		/// <summary>
		/// Gets all customers from countries which have more than 5 customers
		/// </summary>
		[Test]
		public void JoinBetweenGroupedDataQueryAndCustomers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join cd in GetCustomerCountsPerCountryData(metaData) on c.Country equals cd.Country
						where cd.NumberOfCustomers > 5
						select cd;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(50, count);
			}
		}


		[Test]
		public void MultiJoinWithSameEntityIntoSingleValueSelect()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join c in metaData.Customer on o.CustomerId equals c.CustomerId
						join e in metaData.Employee on o.EmployeeId equals e.EmployeeId
						where o.ShipVia == 1
						select o;

				int count=0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(244, count);
			}
		}


		[Test]
		public void BooleanInsideFolderQueryRetrieval()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select new
						 {
							 c.CustomerId,
							 HasOrders = (c.Orders.Count() > 0)
						 }).Where(c => c.CustomerId.StartsWith("C"));
				foreach(var v in q)
				{
					Console.WriteLine(v);
				}
			}
		}


		[Test]
		public void GroupJoinWithCount()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into co
						select new { c.CustomerId, Count = co.Count() };

				List<string> noOrderCustomers = new List<string>() { "FISSA", "PARIS" };
				foreach(var v in q)
				{
					if(noOrderCustomers.Contains(v.CustomerId))
					{
						Assert.AreEqual(0, v.Count);
					}
					else
					{
						Assert.IsTrue(v.Count > 0);
					}
				}

			}
		}


		[Test]
		public void GroupByOnJoinWithTwoAggregatesOnJoinedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join em in metaData.Employee on o.EmployeeId equals em.EmployeeId
						where o.OrderDate > new DateTime(1998, 1, 1)
						group o by o.CustomerId into g
						select new { g.Key, Count = g.Count(), Total = g.Sum(a => a.OrderId) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Count > 0);
					Assert.IsTrue(v.Total > 10000);
				}
				Assert.AreEqual(80, count);
			}
		}


		[Test]
		public void GroupByOnPropertyInFieldTypeAndRetrievalOfGroupByKey()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "CHOPS"
						group o by o.OrderDate.Value.Date into g
						select new { Date = g.Key };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void GroupByOnNullableTypeAndFetchingOfGrouping3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.Where(o => (o.CustomerId == "CHOPS")).GroupBy(o => o.EmployeeId.Value).Where(g => g.Count() > 1);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Count() > 0);
				}
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GroupByOnNullableTypeAndFetchingOfGrouping2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Order.Where(o => (o.CustomerId == "CHOPS")).GroupBy(o => o.OrderDate.Value);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Count() > 0);
				}
				Assert.AreEqual(8, count);
			}
		}
		
		[Test]
		public void GroupByOnNullableTypeAndFetchingOfGrouping()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "CHOPS"
						group o by o.OrderDate.Value into g
						select g;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.Count() > 0);
				}
				Assert.AreEqual(8, count);
			}
		}


		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariableAndNormalWhereClause()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Country=="UK"
						group e by e.Phone into g
						select g;
				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.Count() > 0);
					count++;
				}
				Assert.AreEqual(4, count);
			}
		}


		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariableAndNormalWhereClauseAndCombinedGroupKey()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where e.Country == "UK"
						group e by new { e.Phone, e.Country } into g
						select g;
				int count = 0;
				foreach(var v in q)
				{
					Assert.IsTrue(v.Count() > 0);
					count++;
				}
				Assert.AreEqual(4, count);
			}
		}


		[Test]
		public void CustomProjectionThroughMethod()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.CustomerId == "CHOPS"
						select ConvertToTarget(o.OrderId, o.CustomerId, o.OrderDate);

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
		public void CustomProjectionOnEntityJoinedWithNormalJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 join o in metaData.Order on c.CustomerId equals o.CustomerId
						 select c;

				var q = (from c in q1
						select new { c.CustomerId, c.CompanyName }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GetListOfCustomerIdsWhichHaveOrdersFiledByEmployeesWithIdHigherThan4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in
							(from order in metaData.Order where order.EmployeeId > 4 select order)
						on c.CustomerId equals o.CustomerId into co
						from o in co.DefaultIfEmpty()
						select new { c.CustomerId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(342, count);		// duplicates, but filters on order rows. It should return all customers. 
			}
		}

		[Test]
		public void GroupByOnRelatedProperty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by o.Customer.CompanyName into g
						select new { g.Key, NumberOfOrders = g.Count()};

				int count=0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "PARIS", "FISSA" }.Contains(v.Key))
					{
						Assert.AreEqual(0, v.NumberOfOrders);
					}
					else
					{
						Assert.IsTrue(v.NumberOfOrders > 0);
					}
				}
				Assert.AreEqual(89, count);
			}
		}


		[Test]
		public void GroupByOnConstantWithTertaryOperator()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Employee
						 group o by 1 into g
						 select new { City = g.Key, CityCount = g.Sum(a => a.EmployeeId > 2 ? a.EmployeeId : 0) });

				int count = 0;
				foreach(var v in q)
				{
					Assert.AreEqual(1, v.City);
					Assert.AreEqual(42, v.CityCount);
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void MultiDefaultIfEmptyInLineWithGroupJoins()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join e in metaData.Employee on o.EmployeeId equals e.EmployeeId into employees
						from emp in employees.DefaultIfEmpty()
						join et in metaData.EmployeeTerritory on emp.EmployeeId equals et.EmployeeId into empterritories
						from empterritory in empterritories.DefaultIfEmpty()
						select new
						{
							o.CustomerId,
							o.OrderId,
							o.OrderDate
						};

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void SingleOrDefaultOnEmptySequenceOfValueTypedValues()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 where o.CustomerId == "YYYYY"
						 select o.OrderId).SingleOrDefault();

				Assert.AreEqual(0, q);
			}
		}


		[Test]
		public void BooleanConstantInProjectionWithoutNameSpecification()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						select new Product2() { ProdId = p.ProductId, Name = p.ProductName, Discontinued = (p.ProductId < 0) };

				foreach(var v in q)
				{
					Assert.IsFalse(v.Discontinued);
				}
			}
		}

		[Test]
		public void JoinSortingWithOutsideOrderBy()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from p in metaData.Product
						 join c in metaData.Category
						 on p.CategoryId equals c.CategoryId
						 select new ProductEntity() { ProductId = p.ProductId, ProductName = p.ProductName, Discontinued = p.Discontinued })
						 .OrderBy(product => product.ProductName);

				foreach(var v in q)
				{
				}
			}
		}

		[Test]
		public void ValueListInValueListProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.CustomerId,
							OrderIds = c.Orders.Select(o => o.OrderId)
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.AreEqual(0, v.OrderIds.Count());
					}
					else
					{
						Assert.IsTrue(v.OrderIds.Count() > 0);
					}
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void OrderByOnDefaultIfEmptyResult()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Category
						join p in metaData.Product
						on c.CategoryId equals p.ProductId into cp
						from product in cp.DefaultIfEmpty()
						orderby product.ProductName descending
						select new { c.CategoryName, product.ProductName };

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void CountOnSingleEntitySet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select c;

				var count = q.Count(c => c.Country == "Germany");
				Assert.AreEqual(11, count);
			}
		}


		[Test]
		public void CountOnEntityWithFilterOnRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Order
						select c;

				var count = q.Count(o => o.Customer.Country == "Germany");
				Assert.AreEqual(121, count);
			}
		}


		[Test]
		public void SelectOnListWithCustomProjectionInWhere()
		{
			IList<DateTime> dateTimes = new List<DateTime>();
			dateTimes.Add(new DateTime(2008, 12, 1));
			dateTimes.Add(new DateTime(2009, 1, 1));

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where dateTimes.Select(d => d.Year).Contains(o.EmployeeId.Value)
						select o;

				foreach(var v in q)
				{
				}
			}			
		}


		[Test]
		public void GroupByOnJoinedSetMultipleFieldsWithMultipleAggregatesOnFieldsInJoinedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						join o in metaData.Order on e.EmployeeId equals o.EmployeeId
						group new { e, o } by new { EID = e.EmployeeId, OID = o.EmployeeId } into g
						select new
						{
							Id = g.Key,
							Val1 = g.Sum(p => p.e.EmployeeId),
							Val2 = g.Sum(p => p.o.Freight.Value)
						};
				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void MultipleConstantsOfSameValueInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new { c.CustomerId, A = 2, B = 2, C = 1 };
				foreach(var v in q)
				{
					Assert.AreEqual(2, v.B);
					Assert.AreEqual(2, v.A);
					Assert.AreEqual(1, v.C);
				}
			}
		}


		[Test]
		public void CustomerIdWithEmployeeIdOfLatestOrder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let id = (from o in c.Orders orderby o.OrderDate descending select o.EmployeeId).FirstOrDefault()
						let id2 = (from o in c.Orders orderby o.OrderDate descending select o.OrderId).FirstOrDefault()
						select new { c.CustomerId, ID = id, ID2 = id2 };

				int count = 0;
				string[] idsWithoutOrders = new string[] { "FISSA", "PARIS"};
				foreach(var v in q)
				{
					count++;
					if(!idsWithoutOrders.Contains(v.CustomerId))
					{
						Assert.IsTrue(v.ID > 0);
						Assert.IsTrue(v.ID2 > 0);
					}
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GroupByOnLetQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let id = (from o in c.Orders orderby o.OrderDate descending select o.EmployeeId).FirstOrDefault()
						group id by c.CustomerId into g
						select new { g.Key, Total = g.Sum() };

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void CountInProjectionOnRelatedSet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						where (from o in c.Orders select o).Count() > 0
						select new
						{
							c.CompanyName,
							OrderCount = (from o in c.Orders select o).Count()
						};
				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void NestedQueryWthSameFieldsAsOuterUsingAny()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select new
						{
							FooId = e.EmployeeId,
							OtherId = e.ReportsTo,
							NestedElements = from o in e.Orders
											 where o.OrderDetails.Any()
											 select new
											 {
												 BarId = o.OrderId,
												 OtherId = o.EmployeeId,
												 Cnt = o.OrderDetails.Count()
											 }
						};

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void GroupingWithSumOnCount()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Category
							group c by c.CategoryId into g
							select new { g.Key, y = g.Sum(x => x.Products.Count()) };

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void DynamicListWithConstantEnumInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				List<string> bla = new List<string>();
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							Foo = TestEnum.Value1,
							Bla = bla,
							c.CustomerId,
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(TestEnum.Value1, v.Foo);
					Assert.AreEqual(bla, v.Bla);
				}
				Assert.AreEqual(91, count);
			}
		}


		[Test]
		public void GroupByInJoinedSubQueryWithLeftJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in 
							(
								from order in metaData.Order
								group order by order.CustomerId into g
								select new { CustomerId = g.Key, Frequency = (int?)g.Count()}
							) on c.CustomerId equals o.CustomerId into co
						from v in co.DefaultIfEmpty()
						where c.Country=="USA"
						select new { c.CustomerId, Frequency = v.Frequency??0};

				foreach(var v in q)
				{
					if((v.CustomerId != "FISSA") && (v.CustomerId != "PARIS"))
					{
						Assert.IsTrue(v.Frequency > 0);
					}
					else
					{
						Assert.AreEqual(0, v.Frequency);
					}
				}
			}
		}


		[Test]
		public void NestedQueryWithJoinInProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new
						{
							c.CustomerId,
							OrderData = from o in c.Orders
										join od in metaData.OrderDetail on o.OrderId equals od.OrderId
										where o.CustomerId == c.CustomerId
										select new { o.OrderId, od.ProductId }
						};
				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void NestedScalarQueryWithContainsAndOrderby()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new {
									c.CustomerId,
									EmployeeId = (from o in c.Orders
												  where o.EmployeeId.HasValue
														&& new Int32[] { 1, 2, 3 }.Contains(o.EmployeeId.Value)
												  orderby o.EmployeeId.Value ascending
												  select o.EmployeeId.Value).FirstOrDefault()
								   };

				foreach(var v in q)
				{}
			}
		}

		[Test]
		public void FirstOrDefaultOnStringValue_TestsForEnumerableChecks()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 select c.CompanyName).FirstOrDefault();

				Assert.IsNotNull(q);
				Assert.IsTrue(q.Length> 0);
			}
		}


		[Test]
		public void NestedGroupByWithCasingCorrectTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						group o by o.CustomerId into g
						select g;

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void GetAllCustomerIdPlusOrderIdUsingLeftJoin()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						from order in customer.Orders.DefaultIfEmpty()
						where order.OrderId == null
						select new { CustomerId = customer.CustomerId, order.OrderId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId));
				}
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GetAllCustomerIdPlusOrderIdUsingLeftJoin2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into corders
						from co in corders.DefaultIfEmpty()
						select new { c.CustomerId, OrderId = (int?)co.OrderId };

				foreach(var v in q)
				{
					if(new string[] { "FISSA", "PARIS" }.Contains(v.CustomerId))
					{
						Assert.IsNull(v.OrderId);
					}
					else
					{
						Assert.IsTrue(v.OrderId >= 10248);
					}
				}
			}
		}


		[Test]
		public void GroupByOnNestedQueryElement()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 select c;

				var q = from c in q1
						group c by c.CustomerId into g
						select g.Key;

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void GroupByWithOrderByOnMaxValue()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.EmployeeId == 2
						group o by o.CustomerId
						into g
							orderby g.Max(o => o.OrderDate) ascending
							select new { CustomerId = g.Key, NumberOfOrders = g.Count() };

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void GroupByOnJoinedEntitiesWithWhereOnAggregate()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						group new { c, o, od} by new { od.OrderId, c.CompanyName } into g
						select new { CompanyName = g.Key.CompanyName, Total = g.Sum(x => x.od.UnitPrice * x.od.Quantity) };

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void GroupByOnJoinedEntitiesWithWhereOnAggregate2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						group new { c, o } by new { o.OrderId, c.CompanyName } into g
						select new { CompanyName = g.Key.CompanyName, Total = g.Sum(x => x.o.EmployeeId.Value * 10) };

				foreach(var v in q)
				{

				}
			}
		}

	
		[Test]
		public void GroupByOnJoinedEntitiesWithWhereOnAggregate3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						join p in metaData.Product on od.ProductId equals p.ProductId
						group new { c, o, od, p } by new { od.OrderId, c.ContactTitle } into g
						where g.Sum(x => x.od.Quantity * x.p.UnitPrice.Value) > 100.0M
						select new { CompanyName = g.Key.ContactTitle, Total = g.Sum(x => x.od.UnitPrice * x.od.Quantity) };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(784, count);
			}
		}

		[Test]
		public void GroupByOnJoinedEntitiesWithWhereOnAggregate4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						join p in metaData.Product on od.ProductId equals p.ProductId
						group new { c, o, od, p } by new { od.OrderId, c.CompanyName } into g
						where g.Sum(x => x.od.Quantity * x.p.UnitPrice.Value) > 100.0M
							&& g.Sum(x => x.od.UnitPrice * x.od.Quantity) > 200.0M
						select new { CompanyName = g.Key.CompanyName, Total = g.Sum(x => x.od.UnitPrice * x.od.Quantity) };

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void CountOverSimpleQueryWithNullableFieldComparison()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				OrderEntity ot = new OrderEntity();
				ot.EmployeeId = 4;
				string cid = "ALFKI";

				var q = from o in metaData.Order
						where o.EmployeeId == ot.EmployeeId && o.CustomerId == cid
						select new { o.CustomerId, o.OrderId };
				int count = q.Count();
				Assert.AreEqual(2, count);
				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void AppendedQueryOverDistinctFilteredQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var customers = (from c in metaData.Customer
								 select c).Distinct();

				var q = from c in customers
						where c.CompanyName.Contains("a")
						select c;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(76, count);
			}
		}


		[Test]
		public void CombinedExceptIntersectUsage()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 where c.CompanyName.StartsWith("B")
						 select new { c.CustomerId, c.CompanyName };

				var q2 = from c in metaData.Customer
						 where c.CompanyName.EndsWith("S")
						 select new { c.CustomerId, c.CompanyName };

				var q3 = q1.Intersect(q2);

				int count = 0;
				foreach(var v in q3)
				{
					count++;
					Assert.IsTrue(v.CompanyName.StartsWith("B"));
					Assert.IsTrue(v.CompanyName.EndsWith("s"));
				}
				Assert.AreEqual(4, count);

				var q4 = q1.Except(q2);
				count = 0;
				foreach(var v in q4)
				{
					count++;
					Assert.IsTrue(v.CompanyName.StartsWith("B"));
					Assert.IsTrue(!v.CompanyName.EndsWith("s"));
				}
				Assert.AreEqual(3, count);

			}
		}

		
		[Test]
		public void CombinedExceptIntersectUsage2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 where c.CompanyName.StartsWith("B")
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName };

				var q2 = from c in metaData.Customer
						 where c.CompanyName.EndsWith("S")
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName };
				var l = q2.ToList();

				var q3 = q1.Intersect(l);
				int count = 0;
				foreach(var v in q3)
				{
					count++;
					Assert.IsTrue(v.Value2.StartsWith("B"));
					Assert.IsTrue(v.Value2.EndsWith("s"));
				}
				Assert.AreEqual(4, count);

				var q4 = q1.Except(l);
				count = 0;
				foreach(var v in q4)
				{
					count++;
					Assert.IsTrue(v.Value2.StartsWith("B"));
					Assert.IsTrue(!v.Value2.EndsWith("s"));
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void CombinedExceptIntersectUsage3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 where c.CompanyName.StartsWith("B")
						 select new { c.CustomerId, c.CompanyName };

				var q2 = from c in metaData.Customer
						 where c.CompanyName.EndsWith("S")
						 select new { c.CustomerId, c.CompanyName };

				var q3 = q1.Intersect(q2);
				var q4 = q3.Where(c => c.CompanyName.Length > 4);
				int count = 0;
				foreach(var v in q4)
				{
					count++;
					Assert.IsTrue(v.CompanyName.StartsWith("B"));
					Assert.IsTrue(v.CompanyName.EndsWith("s"));
				}
				Assert.AreEqual(4, count);

				var q5 = q1.Except(q2);
				var q6 = q5.Where(c => c.CompanyName.Length > 4);
				count = 0;
				foreach(var v in q6)
				{
					count++;
					Assert.IsTrue(v.CompanyName.StartsWith("B"));
					Assert.IsTrue(!v.CompanyName.EndsWith("s"));
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void CombinedExceptIntersectUsage4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						 where c.CompanyName.StartsWith("B")
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName };

				var q2 = from c in metaData.Customer
						 where c.CompanyName.EndsWith("S")
						 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName };
				var l = q2.ToList();

				var q3 = q1.Intersect(l);
				var q4 = q3.Where(c => c.Value2.Length > 4);
				int count = 0;
				foreach(var v in q4)
				{
					count++;
					Assert.IsTrue(v.Value2.StartsWith("B"));
					Assert.IsTrue(v.Value2.EndsWith("s"));
				}
				Assert.AreEqual(4, count);

				var q5 = q1.Except(l);
				var q6 = q5.Where(c => c.Value2.Length > 4);
				count = 0;
				foreach(var v in q6)
				{
					count++;
					Assert.IsTrue(v.Value2.StartsWith("B"));
					Assert.IsTrue(!v.Value2.EndsWith("s"));
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void ExpressionInProjectionWithoutSourceTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new { Foo = (o.EmployeeId.Value * o.OrderId) };

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void MultipleEntityFetchesIntoSingleAnonymousTypeWithAppendedWhereClause()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from o in metaData.Order
						 select o)
						 .Where(o=>o.Customer.Country == "Germany" || o.Employee.ReportsTo==2);

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void MultiDynamicWhereClausesAddedWithContains()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = from o in metaData.Order
								join c in metaData.Customer on o.CustomerId equals c.CustomerId into oc
								from x in oc.DefaultIfEmpty()
								select new { CustomerId = x.CustomerId, CompanyName = x.CompanyName, Country = x.Country };

				string searchTerms = "U A";
				var searchCriteria = searchTerms.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

				foreach(var search in searchCriteria)
				{
					string value = search;
					customers = customers.Where(p => p.Country.Contains(value));
				}
				var ids = (from c in customers select c.CustomerId).ToArray();
				foreach(var v in ids)
				{
				}
			}
		}

		[Test]
		public void CorrelatedSubQueryWithCategoryNameInAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from p in metaData.Product
						select new
						{
							Productid = p.ProductId,
							Productname = p.ProductName,
							CategoryName = (from c in metaData.Category where c.CategoryId == p.CategoryId select c).First().CategoryName
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(v.Productid == 75)
					{
						Assert.IsTrue(string.IsNullOrEmpty(v.CategoryName));
					}
					else
					{
						Assert.IsFalse(string.IsNullOrEmpty(v.CategoryName));
					}
				}

				Assert.AreEqual(77, count);
			}
		}


		[Test]
		public void AggregateInWhereOverJoinAndRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						join et in metaData.EmployeeTerritory on e.EmployeeId equals et.EmployeeId
						where e.Orders.Count() > 0
						select new { e.EmployeeId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(49, count);
			}
		}


		[Test]
		public void DerivedTableWithJoinsWrappedWithWhereClauseFirstFieldAliasTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						where o.EmployeeId.Value == 2
						select new {
									CID = c.CustomerId,
									CompanyName = c.CompanyName,
									ProductId = od.ProductId
						};

				var q = q1.Where(t => t.ProductId == 1);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(5, count);
			}
		}


		[Test]
		public void DerivedTableWithWhereUsingLeftJoinAndCount()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q1 = metaData.Order.Where(o => o.EmployeeId == 2);

				var q = from o in q1
						join c in metaData.Customer on o.CustomerId equals c.CustomerId into oc
						from x in oc.DefaultIfEmpty()
						orderby x.CompanyName ascending 
						select o;

				q = q.WithPath(path => path.Prefetch<OrderDetailEntity>(p => p.OrderDetails));

				int count = q.Count();
				Assert.AreEqual(110, count);
			}
		}


		[Test]
		public void AnyOverWhereWithPkFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				bool exists = metaData.Customer.Where(c=>c.CustomerId=="CHOPS").Any();
				Assert.IsTrue(exists);
				exists = metaData.Customer.Where(c => c.Country == "Germany").Any(c => c.CustomerId == "ALFKI");
				Assert.IsTrue(exists);
			}
		}


		[Test]
		public void OrderDatesAsUTCDateTimes()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderDate != null
						select new { DBDate = o.OrderDate, UTCDate = o.OrderDate.Value.ToUniversalTime() };

				foreach(var v in q)
				{
					Assert.IsNotNull(v.DBDate);
					Assert.AreEqual(v.DBDate.Value.ToUniversalTime(), v.UTCDate);
				}
			}
		}

		[Test]
		public void GroupOnCharAndGroupdCharRetrieval()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						let val = c.CustomerId[0].ToString()
						group c by val into g
						select new { Val = g.Key, Count = g.Count() };

				foreach(var v in q)
				{
					Console.WriteLine("{0} : {1}", v.Val, v.Count);
				}
			}
		}


		[Test]
		public void GetCustomersWithOrdersFiledByAllEmployeesSpecified()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var customers = from c in metaData.Customer select c;
				var empIds = new[] { 1, 2, 3 };
				foreach(var empId in empIds)
				{
					var cloneid = empId;
					customers = (from c in customers
								 where c.Orders.Any(o => o.EmployeeId.Value == cloneid)
								 select c);
				}
				var q = customers.OrderBy(c=>c.CustomerId);

				foreach(var v in q)
				{
					Console.WriteLine("{0}", v.CustomerId);
				}
			}
		}


		[Test]
		public void DateTimeFetchesWithUTCConversion()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new {
									 MyDateNotNull = MyDateTimeFunctions.ConvertToUTC(o.OrderDate), 
									 MyDate = MyDateTimeFunctions.ConvertToUTC(o.RequiredDate)
								 };

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void GetAverageNumberOfProductsPerOrderUsingAverageAggregateAndMNRelationship()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var result = metaData.Order.Average(o => o.ProductCollectionViaOrderDetail.Count());
				Assert.AreEqual(2, result);
			}
		}


		[Test]
		public void GetAverageNumberOfProductsPerOrderUsingAverageAggregateAndMNRelationship2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select o.ProductCollectionViaOrderDetail.Count();
				var result = q.Average();
				Assert.AreEqual(2, result);
			}
		}


		[Test]
		public void WhereClauseWithTwoNullableParametersInsideCount()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var order = metaData.Order.FirstOrDefault(o => o.OrderId == 10254);
				Assert.IsNotNull(order);

				var q = (from o in metaData.Order
						 where o.CustomerId == order.CustomerId
						&& (o.EmployeeId == order.EmployeeId || !o.EmployeeId.HasValue)
						select o.OrderId).Count();

				Assert.AreEqual(2, q);
			}
		}


		[Test]
		public void JoinOverMultipleFieldsWithEarlierDefinedQueryTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q1 = from e in metaData.Employee
						 select new { e.EmployeeId, e.RegionId };

				var q = from e in metaData.Employee
						join e2 in q1 on new { e.EmployeeId, e.RegionId } equals new { e2.EmployeeId, e2.RegionId } into ee2
						from e3 in ee2.DefaultIfEmpty()
						select new { e3.EmployeeId, e3.RegionId };

				foreach(var v in q)
				{
					
				}
			}
		}
		

		[Test]
		public void OrderByAttachedToCustomProjection()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId
						select new { CID = c.CustomerId, CN = c.CompanyName };
				q = q.OrderBy(v => v.CN);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(818, count);
			}
		}


		[Test]
		public void MemberAccessOnFirstInProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						select new
						{
							cid = o.CustomerId,
							cust = (from c in metaData.Customer
									where c.CustomerId == o.CustomerId
									select new
									{
										A = string.Concat(string.Concat(c.ContactTitle, c.ContactName), c.CompanyName)
									}).First().A
						};

				foreach(var v in q)
				{
					Console.WriteLine("{0} {1}", v.cid, v.cust);
				}
			}
		}

		[Test]
		public void FirstOnMtoNRelatedentityForRelatedValuesTest2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						orderby c.Country ascending
						select new
						{
							c.CustomerId,
							Name = c.EmployeeCollectionViaOrder.First().FirstName + c.EmployeeCollectionViaOrder.First().LastName
						};

				foreach(var v in q)
				{
					Console.WriteLine("{0} {1}", v.CustomerId, /*v.CompanyName, v.Country,*/ v.Name); // v.FirstName + v.LastName);
				}
			}
		}


		[Test]
		public void DuplicateCaseStatementsOnGroupByWithDualFieldsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						group od by new { od.OrderId, od.ProductId } into odg
						select new
						{
							odg.Key.OrderId,
							odg.Key.ProductId,
							Totaal1 = odg.Sum(x => x.Quantity > 10 ? x.UnitPrice : 0M),
							Totaal2 = odg.Sum(x => x.Quantity > 10 ? x.Discount : 0f)
						};

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void DuplicateCaseStatementsOnGroupByWithDualFieldsTest2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from od in metaData.OrderDetail
						group od by new { od.OrderId, od.ProductId } into odg
						select new
						{
							odg.Key.OrderId,
							odg.Key.ProductId,
							Totaal1 = odg.Sum(x => x.Quantity * x.UnitPrice),
							Totaal2 = odg.Sum(x => x.Quantity * (x.UnitPrice - (decimal)x.Discount))
						};

				foreach(var v in q)
				{

				}
			}
		}



		[Test]
		public void EntityMaterializationThroughNestedQueriesTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.EmployeeId.Value == 2
						select new Order2 { OId = o.OrderId, Customer = o.Customer };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsNotNull(v.Customer);
				}
				Assert.AreEqual(110, count);
			}
		}


		[Test]
		public void GroupByWithMaxTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string[] ids = new string[] { "CHOPS", "ALFKI" };
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where ids.Contains(o.CustomerId)
						group o by o.CustomerId into g
						select new { CustomerId = g.Key, OrderId = g.Max(e => e.OrderId) };

				foreach(var v in q)
				{
					switch(v.CustomerId)
					{
						case "ALFKI":
							Assert.AreEqual(11011, v.OrderId);
							break;
						case "CHOPS":
							Assert.AreEqual(11041, v.OrderId);
							break;
					}
				}
			}
		}


		[Test]
		public void LeftJoinWithCustomProjectionIndexProblemTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from e in metaData.Employee
						 select new
						 {
							 e.EmployeeId,
							 e.FirstName,
							 e.LastName,
							 e.RegionId		// has higher ordinal than Order, so this will crash if a bug is present.
						 };

				var q = from o in metaData.Order
						join e in q1 on o.EmployeeId equals e.EmployeeId into co
						from e in co.DefaultIfEmpty()
						select new
						{
							o.OrderId,
							o.CustomerId,
							e.RegionId,
							e.EmployeeId
						};

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void LeftJoinWithCustomProjectionIndexProblemTest2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						join o in metaData.Order on c.CustomerId equals o.CustomerId into co
						from o in co
						join e in metaData.Employee on o.EmployeeId equals e.EmployeeId
						select new { c.CustomerId, e.EmployeeId, o.OrderId };

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void ClientSideDistinctWithNullTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 join o in metaData.Order on e.EmployeeId equals o.EmployeeId
						 where !o.ShippedDate.HasValue
						 select new { e.EmployeeId, e.Notes, o.ShippedDate }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(5, count);
			}
		}
		

		[Test]
		public void DefaultIfEmptyOnJoinedProjectionSetTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from od in metaData.OrderDetail
						 join o in metaData.Order on od.OrderId equals o.OrderId
						 select new { od.ProductId, o.CustomerId };

				var q = from q1v in q1
						join c in metaData.Customer on q1v.CustomerId equals c.CustomerId into c_join
						from cj in c_join.DefaultIfEmpty()
						select new { q1v.ProductId, cj.Country };

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void LeftJoinWithGroupJoinTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customerIdsWithOrders = from o in metaData.Order
											group o by o.CustomerId into g
											select new { g.Key };

				var q = from c in metaData.Customer
						join o in customerIdsWithOrders on c.CustomerId equals o.Key into co
						from x in co.DefaultIfEmpty()
						select new { Id = c.CustomerId };

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void AggregateWithBooleanExpressionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						where o.OrderId == 10261
						select new
						{
							Orderid = o.OrderId,
							CountOfNotDiscontinuedProducts = Convert.ToInt32(o.OrderDetails.Count(od => od.Product.Discontinued)),
						};
				foreach(var v in q)
				{
					Assert.AreEqual(1, v.CountOfNotDiscontinuedProducts);
				}
			}
		}


		[Test]
		public void MultipleCastsInProjectionWithSubqueryTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from o in metaData.Order
						join cust in metaData.Customer on o.CustomerId equals cust.CustomerId
						where o.OrderId == 10261
						select new
						{
							CountOfDiscontinuedProducts = Convert.ToInt32(o.OrderDetails.Count(od => od.Product.Discontinued == true)),
							CountOfNotDiscontinuedProducts = Convert.ToInt32(o.OrderDetails.Count(od => od.Product.Discontinued == false)),
						};
				foreach(var v in q)
				{
					Assert.AreEqual(1, v.CountOfNotDiscontinuedProducts);
					Assert.AreEqual(1, v.CountOfDiscontinuedProducts);
				}
			}
		}


		[Test]
		public void GetCustomersWithOrdersFiledByAllEmployeesSpecified2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var cids = from o in metaData.Order select new { o.CustomerId };
				var empIds = new[] { 1, 2, 3 };

				foreach(var empId in empIds)
				{
					var clonedid = empId;
					cids = (from c0 in cids
							from c1 in metaData.Order
							where c0.CustomerId == c1.CustomerId
							&& c1.EmployeeId.Value == clonedid
							select new { c1.CustomerId });
				}

				var q = (from c in metaData.Customer
						 from c1 in cids
						 where c.CustomerId == c1.CustomerId
						 select c);

				foreach(var v in q)
				{
					Console.WriteLine("{0}", v.CustomerId);
				}
			}
		}


		[Test]
		public void MultiFirstCallsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from c in metaData.Customer
						 from o in c.Orders
						 select new
						 {
							(
								from od in metaData.OrderDetail
								where od.OrderId == o.OrderId
								join p in metaData.Product on od.ProductId equals p.ProductId
								select p
							).First().CategoryId,
							(
								from od in metaData.OrderDetail
								where od.OrderId == o.OrderId
								join p in metaData.Product on od.ProductId equals p.ProductId
								select p
							).First().ProductName
						 });
				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void DefaultIfEmptyOnMultipleJoinSameQueryFragment()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var prodQ = from p in metaData.Product
							select new { p.ProductId, p.ProductName };

				var q1 = from c in metaData.Customer
						 join o in metaData.Order on c.CustomerId equals o.CustomerId
						 join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						 join p in prodQ on od.ProductId equals p.ProductId
						 select new { CID= c.CustomerId, OID=o.OrderId, PID =p.ProductId, ODate = o.OrderDate };

				var q2 = from c in metaData.Customer
						 join o in metaData.Order on c.CustomerId equals o.CustomerId
						 join od in metaData.OrderDetail on o.OrderId equals od.OrderId
						 select new { CID = c.CustomerId, OID = o.OrderId, ODate = o.OrderDate };

				var q = from v1 in q1
						join v2 in q2 on new {v1.CID, v1.OID } equals new {v2.CID, v2.OID} into v12
						from x in v12.DefaultIfEmpty()
						select x;

				foreach(var v in q)
				{
					
				}
			}
		}

		
		[Test]
		public void PagingWithTempTableAndDbFunctionCallOnStringTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 join o in metaData.Order on e.EmployeeId equals o.EmployeeId.Value
						 select new { FullName = e.FirstName + ", " + e.LastName, e.EmployeeId }).Distinct();
				q = q.Skip(3).Take(3);

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void NoLockWithJoinTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				try
				{
					LinqMetaData metaData = new LinqMetaData(adapter);
					DynamicQueryEngine.UseNoLockHintOnSelects = true;
					var q = from c in metaData.Customer
							join o in metaData.Order on c.CustomerId equals o.CustomerId
							select c.CompanyName;

					foreach(var v in q)
					{

					}
				}
				finally
				{
					DynamicQueryEngine.UseNoLockHintOnSelects = false;
				}
			}
		}
		

		[Test]
		public void GetListOfCategoryNamesWithContinuedProducts()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from category in metaData.Category
						where category.CategoryId == 2
						select new
						{
							category.CategoryName,
							containedProducts = (from product in metaData.Product
												 where
													!product.Discontinued
													&&
													product.CategoryId == category.CategoryId
												 select product.ProductName)
						};

				foreach(var v in q)
				{
					Assert.AreEqual("Condiments", v.CategoryName);
					Assert.AreEqual(11, v.containedProducts.Count());
				}
			}
		}


		[Test]
		public void GetByteArrayWithFirst()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 where e.EmployeeId == 2
						 select e.Photo).FirstOrDefault();
				Assert.IsNotNull(q);
			}
		}


		[Test]
		public void GetSingleValueWithFirst()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 where e.EmployeeId == 2
						 select e.RegionId).FirstOrDefault();
				Assert.IsNotNull(q);
			}
		}


		[Test]
		public void TransactionKeepAliveTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.StartTransaction(IsolationLevel.ReadCommitted, "test");
				Assert.IsTrue(adapter.IsTransactionInProgress);
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from c in metaData.Customer
						select new
						{
							CustomerId = c.CustomerId,
							OrderIds = (from o in metaData.Order
										where o.CustomerId == c.CustomerId
										select o.OrderId).ToList()
						};
				foreach(var v in q)
				{

				}
				Assert.IsTrue(adapter.IsTransactionInProgress);
				adapter.Commit();
				Assert.IsFalse(adapter.IsTransactionInProgress);
			}
		}


		[Test]
		public void NullInProjectionAsConstant()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new { c.CustomerId, Foo = (string)null, c.CompanyName };

				foreach(var v in q)
				{
					Assert.IsNull(v.Foo);
				}
			}
		}


		[Test]
		public void NullInProjectionAsConstant2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new Customer() { CustId = c.CustomerId, Contact = null, CompanyNme = c.CompanyName };

				foreach(var v in q)
				{
					Assert.IsNull(v.Contact);
				}
			}
		}

		[Test]
		public void NullInProjectionAsConstant3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from c in metaData.Customer
						select new Customer(c.CustomerId, null, c.CompanyName);

				foreach(var v in q)
				{
					Assert.IsNull(v.Contact);
				}
			}
		}

		#region Query producers, used in tests which join between queryables.
		private IQueryable<OrderData> GetOrderData(LinqMetaData metaData)
		{
			return from o in metaData.Order
				   select new OrderData { CustomerId = o.CustomerId, OrderId = o.OrderId, EmployeeIdAsFloat = o.EmployeeId.Value };
		}

		private IQueryable<CustomerCountsPerCountry> GetCustomerCountsPerCountryData(LinqMetaData metaData)
		{
			return from c in metaData.Customer
				   group c by c.Country into g
				   select new CustomerCountsPerCountry { Country = g.Key, NumberOfCustomers = g.Count() };
		}
		#endregion

		#region helper classes and methods for custom projections

		private TargetObject ConvertToTarget(int orderId, string customerId, DateTime? orderDate)
		{
			return new TargetObject() { OrderId = orderId, CustomerId=customerId, OrderDate = orderDate};
		}


		private System.Linq.Expressions.Expression<Func<ProductEntity, Product>> CreateProductProjector()
		{
			return p=>new Product()
			{
				Name = p.ProductName,
				ProdId = p.ProductId
			};
		}

		public static class MyDateTimeFunctions
		{
			public static DateTime? ConvertToUTC(DateTime? toConvert)
			{
				return toConvert.HasValue ? toConvert.Value.ToUniversalTime() : toConvert;
			}
		}
		#endregion
	}
}
