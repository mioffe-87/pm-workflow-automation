using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using NW26.SelfServicing.DaoClasses;
using NW26.SelfServicing.EntityClasses;
using NW26.SelfServicing.HelperClasses;
using NW26.SelfServicing.Linq;
using NW26.SelfServicing.RelationClasses;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.TestTypeConverters;


namespace LinqTester.SelfServicingTests
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
			IPredicateExpression filter = new PredicateExpression(OrderFields.CustomerId == "CHOPS");
			List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			valueProjectors.Add(new DataValueProjector("OrderId", 0));
			List<int> orderIds = new List<int>();
			DataProjectorToValueList<int> projector = new DataProjectorToValueList<int>(orderIds);
			TypedListDAO dao = new TypedListDAO();
			dao.GetAsProjection(valueProjectors, projector, null, fields, filter, null, 0, true);

			Assert.AreEqual(8, orderIds.Count);
			List<int> idsToCheckWith = new List<int>() { 10746, 10254, 11041, 10966, 10370, 11029, 10519, 10731 };

			// check if the ids in the list to check are indeed the list to fetch
			var q = from id in orderIds
					where !idsToCheckWith.Contains(id)
					select id;
			Assert.AreEqual(0, q.Count());
		}


		[Test]
		public void TakeWithDistinctOnSmallProjectionTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 select new { o.OrderDate }).Distinct().OrderBy(x => x.OrderDate).Take(128);

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(128, count);
		}


		/// <summary>
		/// Test using All() as last statement in the query.
		/// </summary>
		[Test]
		public void CheckIfTitleOfCourtesyInRegion1IsSolelyMs()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValue()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c.Country).Contains("USA");

			Assert.IsTrue(q);

			q = (from c in metaData.Customer
				 select c.Country).Distinct().Contains("USA");

			Assert.IsTrue(q);
		}


		[Test]
		public void CheckIfListOfTuplesObtainedFromCustomersContainsSpecifiedTuple()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName })
							.Contains(new Pair<string, string>("CHOPS", "CHANGED"));
			Assert.IsTrue(q);

			q = (from c in metaData.Customer
				 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName }).Distinct()
							.Contains(new Pair<string, string>("CHOPS", "CHANGED"));
			Assert.IsTrue(q);
		}


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValueUsingAny()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c.Country).Any(l => l == "USA");
			Assert.IsTrue(q);

			q = (from c in metaData.Customer
				 select c.Country).Distinct().Any(l => l == "USA");
			Assert.IsTrue(q);
		}


		[Test]
		public void CheckIfListOfCountriesAreAllEqualToUSA()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 where c.Country == "USA"
					 select c.Country).All(l => l == "USA");
			Assert.IsTrue(q);

			q = (from c in metaData.Customer
				 where c.Country == "USA"
				 select c.Country).Distinct().All(l => l == "USA");
			Assert.IsTrue(q);
		}


		[Test]
		public void CheckIfListOfTuplesObtainedFromCustomersContainsSpecifiedTupleUsingAnyCall()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName })
							.Any(x => x == new Pair<string, string>("ALFKI", "Alfreds Futterkiste"));
			Assert.IsTrue(q);

			q = (from c in metaData.Customer
				 select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName }).Distinct()
							.Any(x => x == new Pair<string, string>("ALFKI", "Alfreds Futterkiste"));
			Assert.IsTrue(q);
		}


		[Test]
		public void GetOrderTotalOfOrder10254()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = (from od in metaData.OrderDetail
					 where od.OrderId == 10254
					 select od.Quantity * od.UnitPrice).Sum();

			Assert.AreEqual(9987.6M, q);
		}


		/// <summary>
		/// Gets the customer names which have an order filed by employee 3 using correlation relation and filter on related entity.
		/// The projection of the query has a correlation relation in one of the fields. This relation has to be propagated to the main query, otherwise
		/// the query won't include the join on customer.
		/// </summary>
		[Test]
		public void GetCustomerNamesWhichHaveAnOrderFiledByEmployee3UsingCorrelationRelationAndFilterOnRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = (from o in metaData.Order
					 where o.EmployeeId == 3
					 select o.Customer.CompanyName).Distinct();

			IList results = q.ToList();
			Assert.AreEqual(60, results.Count);
		}


		/// <summary>
		/// Test which fetches a list of single values (company names) without any filter.
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetNoFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(from c in metaData.Customer
													select c.CompanyName);
			List<string> companyNames = (List<string>)q.Execute();
			Assert.AreEqual(91, companyNames.Count);
		}


		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithoutDistinct()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
									from c in metaData.Customer
									where c.Country == "USA"
									select c.City);
			List<string> cities = (List<string>)q.Execute();
			Assert.AreEqual(13, cities.Count);
		}


		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinct()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
									from c in metaData.Customer
									where c.Country == "USA"
									select c.City).Distinct();
			List<string> cities = (List<string>)q.Execute();
			// Portland is duplicated, which should be filtered out.
			Assert.AreEqual(12, cities.Count);
		}


		/// <summary>
		/// Test which fetches page 2 from all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default).
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinctAndPaging()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
									from c in metaData.Customer
									where c.Country == "USA"
									select c.City).Distinct().TakePage(3, 5);
			List<string> cities = (List<string>)q.Execute();
			// Portland is duplicated, which should be filtered out. This means that page 3 has just 2 rows.
			Assert.AreEqual(2, cities.Count);
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all cities for customers in the USA).
		/// </summary>
		[Test]
		public void GetValueListWithMaxLimitAndFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
			ILLBLGenProQuery q = (ILLBLGenProQuery)(
									from c in metaData.Customer
									where c.Country == "USA"
									select c.City).Take(5);
			List<string> cities = (List<string>)q.Execute();
			Assert.AreEqual(5, cities.Count);
		}


		[Test]
		public void GetAnonymousTypedListFromCustomerAndOrderData()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					join o in metaData.Order on c.CustomerId equals o.CustomerId
					where c.Country == "Germany"
					select new { Company = c.CompanyName, OrderID = o.OrderId };

			int counter = 0;
			foreach(var v in q)
			{
				counter++;
			}
			Assert.AreEqual(121, counter);
		}


		/// <summary>
		/// Test which uses 'let' to store a value of a field, where null values are replaced by a string, which is later on used as a filter. 
		/// </summary>
		[Test]
		public void LeftJoinEmulatingQueryWithLet()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Aggregate test on Count. It fetches all customers which have more than 6 orders which are filed by employees from the UK. 
		/// The difference here is that it uses a Where instead of a filter in the Count.
		/// This is done by applying a count on the Orders 
		/// </summary>
		[Test]
		public void GetAllCustomerIDsWithTheirTotalNumberOfOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets the maximum order total. This is a test to see if Max works in a single value return setting.
		/// </summary>
		[Test]
		public void GetTheMaximumOrderTotalUsingAnonymousType()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select new { c.CustomerId, TotalAllOrders = c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)) }
					).Max(n => n.TotalAllOrders);

			Assert.AreEqual(112804.1900, q);
		}


		/// <summary>
		/// Gets the maximum order total. This is a test to see if Max works in a single value return setting.
		/// </summary>
		[Test]
		public void GetTheMaximumOrderTotalUsingMaxCallOnSequenceOfValues()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from c in metaData.Customer
					 select c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))
					).Max();

			Assert.AreEqual(112804.1900, q);
		}


		/// <summary>
		/// Gets the total count of distinct employee ids in orders.Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfDistinctEmployeeIdsInOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Order.CountColumn(o => o.EmployeeId, true);

			Assert.AreEqual(9, q);
		}


		/// <summary>
		/// Gets the total count of all employee ids in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfAllEmployeeIdsInOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Order.CountColumn(o => o.EmployeeId);

			Assert.AreEqual(818, q); // equal to the # of rows.
		}


		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheStandardDeviationOverEmployeeIDsInOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Order.StandardDeviation(o => o.EmployeeId);

			Assert.AreEqual(2.4470306717636521, q);
		}


		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheVarianceOverEmployeeIDsInOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Order.Variance(o => o.EmployeeId);

			Assert.AreEqual(5.9879591085520705, q);
		}


		/// <summary>
		/// Gets the list of customer ids plus order total for customers with more than10 orders using duplicated aggregate method calls.
		/// </summary>
		[Test]
		public void GetListOfCustomerIdsPlusOrderTotalForCustomersWithMoreThan10OrdersUsingDuplicatedAggregateMethodCalls()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by. Simple Groupby test
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupBy()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by ordered by country. Simple groupby test with orderby 
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupByOrderedByCountry()
		{
			using(Transaction trans = new Transaction(System.Data.IsolationLevel.ReadCommitted, "Test"))
			{
				LinqMetaData metaData = new LinqMetaData(trans);
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

				// no changes but to test if transaction connections also work.
				trans.Commit();
			}
		}


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by ordered by country filtered on null. simple but where has to be
		/// outside the groupby.
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupByOrderedByCountryFilteredOnNull()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetValueListFromJoinedSetWithIntoAndWithoutSpecifyingDefaultIfEmpty()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets the list of countries with the number of customers who have purchased more than10 orders. This is a test
		/// with groupby field injection from where AND projection which requires folding of groupby query into inner derived table. 
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheNumberOfCustomersWhoHavePurchasedMoreThan10Orders()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Complicated query which tests the folding of the inner query for the groupby, as it has to fold twice, (Sum and Avg both force it to fold).
		/// </summary>
		[Test]
		public void GetComplicatedListWithTwoAggregatesOnAGroupBy()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetSetOfTuplesOfConcreteClassesFromProjection()
		{
			LinqMetaData metaData = new LinqMetaData();

			var q = from c in metaData.Customer
					select new KeyValuePair<string, string>(c.Country, c.City);

			List<KeyValuePair<string, string>> results = q.ToList();
			Assert.AreEqual(91, results.Count);
		}


		[Test]
		public void GetAnonymousTypedListFromCustomersInLondonUsingJoinWithNestedLetAndWhereInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GetAnonymousTypedListFromCustomersInLondonUsingJoinWithSubqueryAndWhereInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GetStringFromCustomerBasedOnIIFStatementInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Tests InvokeExpression handling
		/// </summary>
		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjection()
		{
			Func<string, string> stringChopper = s => s.Substring(0, 3);

			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					select stringChopper(c.CompanyName);

			foreach(var v in q)
			{
				Assert.AreEqual(3, v.Length);
			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelation()
		{
			Func<string, string> stringChopper = s => s.Substring(0, 3);

			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 select stringChopper(o.Customer.CompanyName)).Distinct();

			foreach(var v in q)
			{
				Assert.AreEqual(3, v.Length);
			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelationIntoAnonymousType()
		{
			Func<string, string> stringChopper = s => s.Substring(0, 3);

			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 select new
					 {
						 FirstChars = stringChopper(o.Customer.CompanyName),
						 o.Customer.CompanyName
					 }).Distinct();

			foreach(var v in q)
			{
				Assert.AreEqual(3, v.FirstChars.Length);
				Assert.AreEqual(v.FirstChars, v.CompanyName.Substring(0, 3));
			}
		}


		[Test]
		public void GetListOfBooleansProducedFromBooleanExpressionInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Country == "USA"
					select (c.Country == "USA");

			foreach(var v in q)
			{
				Assert.IsTrue(v);
			}
		}


		[Test]
		public void GetAnonymousTypedListWithBooleanExpressionInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesByAnInstanceMethodOfANewObject()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesByAnInstancePropertyOfANewObject()
		{
			LinqMetaData metaData = new LinqMetaData();
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



		[Test]
		public void ConcatTwoFetchedValuesByAStaticMethod()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesByAnInstanceMethodOfAnExistingObject()
		{
			Concatenator concatter = new Concatenator();
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryReferredToByLetByAStaticMethod()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryByAStaticMethod()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryConvertedToABooleanByAStaticMethod()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingContainsQueryConvertedToAStringByAStaticMethod()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetNestedTreeOfCustomerOrderOrderDetailEmployeeProductInfoUsingNestedQueriesFilteredOnCustomersFromGermany()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetListOfOrderCollectionsViaCustomer()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where !new List<string>() { "FISSA", "PARIS" }.Contains(c.CustomerId)
					select c.Orders;

			int count = 0;
			foreach(var v in q)
			{
				count++;
				Assert.IsTrue(v.Count > 0);		// v is of type EntityCollection<OrderEntity> :)
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GetListOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 where o.EmployeeId == 2
					 select o.Customer).Distinct();

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(62, count);
		}


		[Test]
		public void GetDistinctSetOfCompanyNamesAndEmployeesWhoFiledTheirOrders()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetDistinctSetOfCompanyNamesAndEmployeesWhoFiledTheirOrdersUsingNestedQueryForCompanyName()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
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


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingNestedQuery()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetEmployeeFullnameRelatedToOrder()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingNestedQueryWithoutLimiter()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetCustomerDataFromGermanyAndTheirLast3OrderDataInNestedSet()
		{
			LinqMetaData metaData = new LinqMetaData();
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
				Assert.IsTrue((orderCount > 0) && orderCount <= 3);
			}
			Assert.AreEqual(11, count);
		}


		[Test]
		public void GetCustomerDataAndNestedOrderList()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetCustomerDataAndNestedOrderListUsingParameterizedNestedQuery()
		{
			LinqMetaData metaData = new LinqMetaData();
			DaoBase.ParameterisedPrefetchPathThreshold = 100;
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


		[Test]
		public void GetListOfCompanyNamesUsingCombinationOfMultipleSeparatedQueries()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetListOfCustomersUsingValueListProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetOrderDataWithTotalNumberOfProductsBoughtBelowGivenPrice()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					select new
					{
						o.OrderId,
						o.OrderDate,
						AmountProductsBought = o.OrderDetails.Where(od => od.UnitPrice < 2.0M).Sum(od => od.Quantity)
					};

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(818, count);
		}


		[Test]
		public void GetOrderDataFromOrdersWithMoreThan1ProductBoughtBelowGivenPriceUsingWrappedWhereAndPaging()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = (from o in metaData.Order
					 select new
					 {
						 o.OrderId,
						 ShippedDate = o.ShippedDate ?? DateTime.Now,
						 AmountProductsBought = o.OrderDetails.Where(od => od.UnitPrice < 10.0M).Sum(od => od.Quantity)
					 } into result
					 where result.AmountProductsBought > 1
					 select result).TakePage(2, 5);

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(5, count);
		}


		[Test]
		public void GetListOfAnonymousTypesWithNestedAnonymousTypesOfCustomerData()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GroupByOnMultipleFieldsWithReferenceToKeyInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GetCountriesWithNumberOfCitiesUsingGroupByOnSelectIntoSet()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GroupByWithCompoundKeyAndFieldInRelatedEntity()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GroupByWithAggregateOnNullableTypeWithCoalesce()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void SimpleValueListSelectionOfAllCountriesInCustomers()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GroupByWithCompoundFieldSetAndAggregateOnRelatedEntityInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetAndAggregateOnRelatedEntityInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					group o by new { o.Customer.City } into g
					select new { Key = g.Key, Sum = g.Sum(o => (o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))) };

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(69, count);
		}


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetOfCombinedFieldsAndAggregateOnRelatedEntityInProjection()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from o in metaData.Order
					group o by new { CombinedName = o.Customer.City + o.Customer.Country } into g
					select new { Key = g.Key, Sum = g.Sum(o => (o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))) };

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(70, count);
		}


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetOfCombinedFieldsAndAggregate()
		{
			LinqMetaData metaData = new LinqMetaData();
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



		[Test]
		public void MultiLeftJoinWithDistinctFetchWhereProjectionOfJoinShouldBeUsedInResultQueryExpression()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void JoinOnCharExpression()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void JoinOnConstantChar()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void OrderInfoFetchWith3JoinsToRelatedEntities()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GroupingOfEntitiesByGroupByKeyIntoAnonymousType()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GroupingOfEntitiesByGroupByKeyIntoAnonymousTypeUsingGrouping()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					group c by c.Country into g
					select new
					{
						g.Key,
						CustomersInCountry = g
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



		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariable()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from c in metaData.Customer
					where c.Country != null
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


		[Test]
		public void GetMaxOrdersPerCustomerUsingJoinWithGroupedSet()
		{
			LinqMetaData metaData = new LinqMetaData();

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


		[Test]
		public void GroupByOnJoinedEntitiesWithWhereOnAggregate()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void CombinedExceptIntersectUsage()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void CombinedExceptIntersectUsage2()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q1 = from c in metaData.Customer
					 where c.CompanyName.StartsWith("B")
					 select new Pair<string, string>(){ Value1= c.CustomerId, Value2 = c.CompanyName };

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

        
		[Test]
		public void WhereClauseWithTwoNullableParametersInsideCount()
		{
			LinqMetaData metaData = new LinqMetaData();
			var order = metaData.Order.FirstOrDefault(o => o.OrderId == 10254);
			Assert.IsNotNull(order);

			var q = (from o in metaData.Order
					 where o.EmployeeId == order.EmployeeId
					&& (o.EmployeeId == order.EmployeeId || !o.EmployeeId.HasValue)
					 select o.OrderId).Count();

			Assert.AreEqual(115, q);
		}


		[Test]
		public void DerivedTableBasedDynamicListWithGroupByNativeQueryApiTest()
		{
			// derived table elements. Define a derived query where all orderdetails rows with productid1 are fetched and the count of the other products
			// in that query. 
			int productId = 1;

			PredicateExpression scalarFilter = new PredicateExpression();
			scalarFilter.Add(OrderDetailFields.ProductId.SetObjectAlias("ODI") != productId);
			scalarFilter.AddWithAnd(OrderDetailFields.OrderId.SetObjectAlias("ODI")
					 == OrderDetailFields.OrderId);

			ResultsetFields dtFields = new ResultsetFields(2);
			dtFields.DefineField(OrderDetailFields.OrderId, 0);
			dtFields.DefineField(new EntityField("OtherProductsInGroup",
				 new ScalarQueryExpression(OrderDetailFields.OrderId
					 .SetAggregateFunction(AggregateFunction.Count).SetObjectAlias("ODI"),
					 scalarFilter)
				 ), 1);

			PredicateExpression dtFilter = new PredicateExpression();
			dtFilter.AddWithAnd(OrderDetailFields.ProductId == productId);

			DerivedTableDefinition dt = new DerivedTableDefinition(dtFields, "A", dtFilter);
			dt.AllowDuplicates = false;

			RelationCollection relations = new RelationCollection(new DynamicRelation(dt));
            ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(new EntityField("OtherProductsInGroup", "A", typeof(int)), 0);
			fields.DefineField(new EntityField("OrderId", "A", typeof(int)).SetAggregateFunction(AggregateFunction.Count), 1);
			GroupByCollection groupBy = new GroupByCollection(fields[0]);
			SortExpression sorter = new SortExpression((EntityField)fields[0] | SortOperator.Ascending);
			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, sorter, null, relations, false, groupBy, null, 0, 0);

			Assert.AreEqual(6, results.Rows.Count);
		}
	}
}