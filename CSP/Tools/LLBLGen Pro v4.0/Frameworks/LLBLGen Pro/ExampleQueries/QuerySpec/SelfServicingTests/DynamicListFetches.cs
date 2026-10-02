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
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.SelfServicing;
using NW26.SelfServicing.FactoryClasses;
using NW26.SelfServicing.CollectionClasses;
using NW26.SelfServicing.TypedViewClasses;


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


		#region CachedLambdaDifferentParameterTest helpers
		private void FetchOrdersBasedOnGivenCustomerId(string customerId, int expectedCount)
		{
			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.CustomerId == customerId)
						.Select(() => new { Pk = customerId, OrderId = OrderFields.OrderId.ToValue<int>() });
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(customerId, v.Pk);
			}
			Assert.AreEqual(expectedCount, results.Count);
		}


		private void FetchOrdersBasedOnGivenCustomerId2(string customerId, int expectedCount)
		{
			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.CustomerId == customerId)
						.Select(() => new { Pk = "CHOPS", Pk2 = customerId, OrderId = OrderFields.OrderId.ToValue<int>() });
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual("CHOPS", v.Pk);
				Assert.AreEqual(customerId, v.Pk2);
			}
			Assert.AreEqual(expectedCount, results.Count);
		}


		private void FetchOrdersBasedOnGivenCustomerId3(string customerId, int expectedCount)
		{
			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.CustomerId == customerId)
						.Select(() => new { Pk = "ALFKI", Pk2 = customerId, OrderId = OrderFields.OrderId.ToValue<int>() });
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual("ALFKI", v.Pk);
				Assert.AreEqual(customerId, v.Pk2);
			}
			Assert.AreEqual(expectedCount, results.Count);
		}


		private void FetchOrdersBasedOnGivenCustomerId4(string customerId, int expectedCount)
		{
			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.CustomerId == customerId)
						.Select(() => new { Pk = "ALFKI", OrderId = OrderFields.OrderId.ToValue<int>() });
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual("ALFKI", v.Pk);
			}
			Assert.AreEqual(expectedCount, results.Count);
		}

		private void FetchOrdersBasedOnGivenCustomerId5(string customerId, int expectedCount)
		{
			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.CustomerId == customerId)
						.Select(() => new { Pk = "CHOPS", OrderId = OrderFields.OrderId.ToValue<int>() });
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual("CHOPS", v.Pk);
			}
			Assert.AreEqual(expectedCount, results.Count);
		}
		#endregion

		[Test]
		public void CachedLambdaDifferentParameterTest()
		{
			FetchOrdersBasedOnGivenCustomerId("CHOPS", 8);
			FetchOrdersBasedOnGivenCustomerId("ALFKI", 6);
			FetchOrdersBasedOnGivenCustomerId2("CHOPS", 8);
			FetchOrdersBasedOnGivenCustomerId2("ALFKI", 6);
			FetchOrdersBasedOnGivenCustomerId3("CHOPS", 8);
			FetchOrdersBasedOnGivenCustomerId3("ALFKI", 6);
			FetchOrdersBasedOnGivenCustomerId5("CHOPS", 8);
			FetchOrdersBasedOnGivenCustomerId4("ALFKI", 6);
			FetchOrdersBasedOnGivenCustomerId4("CHOPS", 8);
		}

		
		[Test]
		public void TypedViewFetch()
		{
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = InvoicesFields.CustomerId.ToValue<string>(),
							CustomerName = InvoicesFields.CustomerName.ToValue<string>(),
							OrderId = InvoicesFields.OrderId.ToValue<int>()
						})
						.Where(InvoicesFields.Country.StartsWith("U"));

			var results = new TypedListDAO().FetchQuery(q);
			Assert.AreEqual(444, results.Count);
		}


		[Test]
		public void TypedViewFetch2()
		{
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = InvoicesFields.CustomerId.ToValue<string>(),
							CustomerName = InvoicesFields.CustomerName.ToValue<string>(),
							OrderId = InvoicesFields.OrderId.ToValue<int>()
						})
						.Where(InvoicesFields.Country.StartsWith("U"));

			var tv = new InvoicesTypedView();
			new TypedListDAO().FetchAsDataTable(q, tv);
			Assert.AreEqual(444, tv.Rows.Count);
		}


		[Test]
		public void TypedViewFetch3()
		{
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(InvoicesFields.CustomerId, InvoicesFields.CustomerName, InvoicesFields.OrderId)
						.Where(InvoicesFields.Country.StartsWith("U"));

			var tv = new InvoicesTypedView();
			new TypedListDAO().FetchAsDataTable(q, tv);
			Assert.AreEqual(444, tv.Rows.Count);
		}



		[Test]
		public void DataTableFetchWithExpression()
		{
			// non-aliased expression in projection should get auto-aliased and be used in result.
			var qf = new QueryFactory();
			var q = qf.Create()
				 .Select(() => new
				 {
					 UnitPrice = OrderDetailFields.UnitPrice.ToValue<decimal>(),
					 Quantity = OrderDetailFields.Quantity.ToValue<int>(),
					 Discount = OrderDetailFields.Discount.ToValue<decimal>(),
					 ProductId = OrderDetailFields.ProductId.ToValue<int>(),
					 Total = (OrderDetailFields.UnitPrice * OrderDetailFields.Quantity).ToValue<decimal>()
				 })
				 .Where(OrderDetailFields.ProductId == 2);
			DataTable results = new TypedListDAO().FetchAsDataTable(q);
			Assert.AreEqual(43, results.Rows.Count);
		}


		[Test]
		public void SimpleCustomProjectorFetchTest()
		{
			var qf = new QueryFactory();
			var q = qf.Create()
						.Where(OrderFields.CustomerId == "CHOPS")
						.Select(() => OrderFields.OrderId.ToValue<int>());

			var results = new DataTable();
			var projector = new DataProjectorToDataTable(results);
			new TypedListDAO().FetchAsProjection(null, q, projector);
			Assert.AreEqual(8, results.Rows.Count);
			Assert.AreEqual(1, results.Columns.Count);
		}


		/// <summary>
		/// Test which checks if the value projector class works.
		/// </summary>
		[Test]
		public void GetUsingLLBLGenProProjectionListOfValuesUsingValueProjector()
		{
			// Low level API variant
			//ResultsetFields fields = new ResultsetFields(1);
			//fields.DefineField(OrderFields.OrderId, 0);
			//RelationPredicateBucket filter = new RelationPredicateBucket(OrderFields.CustomerId == "CHOPS");
			//List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			//valueProjectors.Add(new DataValueProjector("OrderId", 0));
			//List<int> orderIds = new List<int>();
			//DataProjectorToValueList<int> projector = new DataProjectorToValueList<int>(orderIds);
			//adapter.FetchProjection(valueProjectors, projector, fields, filter, 0, true);

			// Queryspec variant
			var qf = new QueryFactory();
			var q = qf.Create()
						.Where(OrderFields.CustomerId == "CHOPS")
						.Select(() => OrderFields.OrderId.ToValue<int>());

			// alternative with separate projection and projector lambda
			//var q = qf.Create()
			//            .Where(OrderFields.CustomerId=="CHOPS")
			//            .Select(OrderFields.OrderId)
			//            .WithProjector(r => (int)r[0]);

			var orderIds = new TypedListDAO().FetchQuery(q);

			Assert.AreEqual(8, orderIds.Count);
			List<int> idsToCheckWith = new List<int>() { 10746, 10254, 11041, 10966, 10370, 11029, 10519, 10731 };
			// check if the ids in the list to check are indeed the list to fetch
			var qcheck = from id in orderIds
							where !idsToCheckWith.Contains(id)
							select id;
			Assert.AreEqual(0, qcheck.Count());
		}


		[Test]
		public void GetCountPlusFirst100Orders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.CustomerId.StartsWith("C")
			//        orderby o.CustomerId
			//        select new
			//        {
			//            o.OrderId,
			//            o.CustomerId,
			//            o.EmployeeId,
			//            o.OrderDate
			//        };
			//var count = q.Count();		// should eliminate the OrderBy as it's invalid and unnecessary.
			//var results = q.Take(100);
			var qf = new QueryFactory();
			var q = qf.Order.As("o")
						.Where(OrderFields.CustomerId.StartsWith("C"))
						.OrderBy(OrderFields.CustomerId.Ascending())
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
						});

			// alternative, with separate specific projector lambda definition and projection in the raw variant of Select()
			//var q = qf.Order.As("o")
			//            .Where(OrderFields.CustomerId.StartsWith("C"))
			//            .OrderBy(OrderFields.CustomerId.Ascending())
			//            .Select(OrderFields.OrderId, OrderFields.CustomerId, OrderFields.EmployeeId, OrderFields.OrderDate)
			//            .WithProjector(r =>	new
			//                                {
			//                                    OrderId = (int)r[0],
			//                                    CustomerId = (string)r[1],
			//                                    EmployeeId = (int?)r[2],
			//                                    OrderDate = (DateTime?)r[3]
			//                                });

			// Alternative 2
			//var q = qf.Order.As("o")
			//            .Where(OrderFields.CustomerId.StartsWith("C"))
			//            .OrderBy(OrderFields.CustomerId.Ascending())
			//            .Select(Projection.Full)
			//            .WithProjector(r => new
			//            {
			//                OrderId = (int)r["OrderId"],
			//                CustomerId = (string)r["CustomerId"],
			//                EmployeeId = (int?)r["EmployeeId"],
			//                OrderDate = (DateTime?)r["OrderDate"]
			//            });

			// For the count, create a new query which fetches the countrow from q. Orderby is eliminated from this query as it's invalid in the SQL.
			var count = new TypedListDAO().GetScalar<int>(qf.Create().Select(Functions.CountRow().Source(q.Alias)).From(q));
			// For the results, simply fetch q
			var results = new TypedListDAO().FetchQuery(q.Limit(100));
			int cntr = 0;
			foreach(var v in results)
			{
				cntr++;
				Assert.IsTrue(v.CustomerId.StartsWith("C"));
			}
			Assert.AreEqual(23, cntr);
		}

		[Test]
		public void GetCountPlusFirst100OrdersUsingCountColumn()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.CustomerId.StartsWith("C")
			//        orderby o.CustomerId
			//        select new
			//        {
			//            o.OrderId,
			//            o.CustomerId,
			//            o.EmployeeId,
			//            o.OrderDate
			//        };

			//var count = q.CountColumn(x=>x.OrderId);
			//var results = q.Take(100);
			var qf = new QueryFactory();
			var q = qf.Create("o")
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
						})
						.Where(OrderFields.CustomerId.StartsWith("C"))
						.OrderBy(OrderFields.CustomerId.Ascending());

			// alternative starting with the entity
			//var q = qf.Order.As("o")
			//            .Where(OrderFields.CustomerId.StartsWith("C"))
			//            .OrderBy(OrderFields.CustomerId.Ascending())
			//            .Select(() => new
			//            {
			//                OrderId = OrderFields.OrderId.ToValue<int>(),
			//                CustomerId = OrderFields.CustomerId.ToValue<string>(),
			//                EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
			//                OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
			//            });

			// For the count, create a new query which fetches the countrow from q. Orderby is eliminated from this query as it's invalid in the SQL.
			var count = new TypedListDAO().GetScalar<int>(qf.Create().Select(OrderFields.OrderId.Source(q.Alias).Count()).From(q));
			// For the results, simply fetch q
			var results = new TypedListDAO().FetchQuery(q.Limit(100));

			int cntr = 0;
			foreach(var v in results)
			{
				cntr++;
				Assert.IsTrue(v.CustomerId.StartsWith("C"));
			}
			Assert.AreEqual(23, cntr);
		}


		[Test]
		public void GetFirst100OrdersUsingOrderByOnExistingQuery()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.CustomerId.StartsWith("C")
			//        select new
			//        {
			//            o.OrderId,
			//            o.CustomerId,
			//            o.EmployeeId,
			//            o.OrderDate
			//        };

			//var results = q.Take(100).Distinct().OrderBy(o=>o.CustomerId);

			var qf = new QueryFactory();
			var q = qf.Create("o")
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
						})
						.Where(OrderFields.CustomerId.StartsWith("C"));

			var results = new TypedListDAO().FetchQuery(q.Limit(100).Distinct().OrderBy(OrderFields.CustomerId.Ascending()));

			int cntr = 0;
			foreach(var v in results)
			{
				cntr++;
				Assert.IsTrue(v.CustomerId.StartsWith("C"));
			}
			Assert.AreEqual(23, cntr);
		}


		/// <summary>
		/// Test using All() as last statement in the query.
		/// </summary>
		[Test]
		public void CheckIfTitleOfCourtesyInRegion1IsSolelyMs()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from r in metaData.Region
			//        where r.RegionId == 1
			//        select r.Employees.All(e => e.TitleOfCourtesy == "Ms.");

			var qf = new QueryFactory();
			var q = qf.Region.Where(RegionFields.RegionId == 1)
								.Select(() => qf.Employee
											.CorrelatedOver(EmployeeEntity.Relations.RegionEntityUsingRegionId)
											.All(EmployeeFields.TitleOfCourtesy == "Ms.") // auto-aliased to LLBLV_1
											.ToValue());
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				Assert.IsTrue(v);
				count++;
			}
			Assert.AreEqual(1, count);
		}


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValue()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         select c.Country).Contains("USA");
			//Assert.IsTrue(q);
			//q = (from c in metaData.Customer
			//         select c.Country).Distinct().Contains("USA");
			//Assert.IsTrue(q);

			var qf = new QueryFactory();
			var result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer.Select(CustomerFields.Country).Contains("USA")));
			Assert.IsTrue(result);

			result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer.Select(CustomerFields.Country).Distinct().Contains("USA")));
			Assert.IsTrue(result);
		}


		[Test]
		public void CheckIfListOfTuplesObtainedFromCustomersContainsSpecifiedTuple()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         select new Pair<string, string>() { Value1 = c.CustomerId, Value2 = c.CompanyName })
			//                .Contains(new Pair<string, string>("CHOPS", "CHANGED"));
			//Assert.IsTrue(q);

			// not supported directly, supply the predicates manually
			var qf = new QueryFactory();
			var q = qf.Customer.Select(CustomerFields.CustomerId, CustomerFields.CompanyName);
			var result = new TypedListDAO().GetScalar<bool>(
							qf.Create().Select(q.Any((CustomerFields.CustomerId == "CHOPS").And(CustomerFields.CompanyName == "CHANGED"))));
			Assert.IsTrue(result);
		}


		[Test]
		public void CheckIfListOfCountriesContainsASpecifiedValueUsingAny()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         select c.Country).Any(l => l == "USA");
			//Assert.IsTrue(q);

			//q = (from c in metaData.Customer
			//         select c.Country).Distinct().Any(l => l == "USA");
			//Assert.IsTrue(q);

			var qf = new QueryFactory();
			var result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer.Select(CustomerFields.Country).Any(CustomerFields.Country == "USA")));
			Assert.IsTrue(result);

			result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer.Select(CustomerFields.Country).Distinct().Any(CustomerFields.Country == "USA")));
			Assert.IsTrue(result);
		}


		[Test]
		public void CheckIfListOfCountriesAreAllEqualToUSA()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         where c.Country == "USA"
			//         select c.Country).All(l => l == "USA");
			//Assert.IsTrue(q);
			//q = (from c in metaData.Customer
			//         where c.Country == "USA"
			//         select c.Country).Distinct().All(l => l == "USA");
			//Assert.IsTrue(q);
			var qf = new QueryFactory();
			var result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer
																			.Where(CustomerFields.Country == "USA")
																			.Select(CustomerFields.Country)
																			.All(CustomerFields.Country == "USA")));
			Assert.IsTrue(result);

			result = new TypedListDAO().GetScalar<bool>(qf.Create().Select(qf.Customer
																			.Where(CustomerFields.Country == "USA")
																			.Select(CustomerFields.Country)
																			.Distinct()
																			.All(CustomerFields.Country == "USA")));
			Assert.IsTrue(result);
		}


		[Test]
		public void GetOrderTotalOfOrder10254()
		{
			//LinqMetaData metaData = new LinqMetaData();

			//var q = (from od in metaData.OrderDetail
			//        where od.OrderId == 10254
			//        select od.Quantity * od.UnitPrice).Sum();
			var qf = new QueryFactory();
			var q = qf.OrderDetail
						.Where(OrderDetailFields.OrderId == 10254)
						.Select((OrderDetailFields.Quantity * OrderDetailFields.UnitPrice).Sum());
			var result = new TypedListDAO().GetScalar<decimal>(q);

			Assert.AreEqual(9987.6M, result);
		}


		/// <summary>
		/// Gets the customer names which have an order filed by employee 3 using correlation relation and filter on related entity.
		/// The projection of the query has a correlation relation in one of the fields. This relation has to be propagated to the main query, otherwise
		/// the query won't include the join on customer.
		/// </summary>
		[Test]
		public void GetCustomerNamesWhichHaveAnOrderFiledByEmployee3UsingCorrelationRelationAndFilterOnRelatedEntity()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//        where o.EmployeeId == 3
			//        select o.Customer.CompanyName).Distinct();
			//IList results = q.ToList();
			var qf = new QueryFactory();
			var q = qf.Order
						.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
						.Where(OrderFields.EmployeeId == 3)
						.Select(CustomerFields.CompanyName)
						.Distinct();
			var results = new TypedListDAO().FetchQuery(q);
			Assert.AreEqual(60, results.Count);
		}



		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithoutDistinct()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//ILLBLGenProQuery q = (ILLBLGenProQuery)(
			//                        from c in metaData.Customer
			//                        where c.Country == "USA"
			//                        select c.City);
			//List<string> cities = (List<string>)q.Execute();
			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "USA")
						.Select(() => CustomerFields.City.ToValue<string>());
			var cities = new TypedListDAO().FetchQuery(q);

			Assert.AreEqual(13, cities.Count);
		}


		/// <summary>
		/// Test which fetches all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default)
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinct()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//ILLBLGenProQuery q = (ILLBLGenProQuery)(
			//                        from c in metaData.Customer
			//                        where c.Country == "USA"
			//                        select c.City).Distinct();
			//List<string> cities = (List<string>)q.Execute();
			// Portland is duplicated, which should be filtered out.
			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "USA")
						.Select(() => CustomerFields.City.ToValue<string>())
						.Distinct();
			var cities = new TypedListDAO().FetchQuery(q);
			Assert.AreEqual(12, cities.Count);
		}


		/// <summary>
		/// Test which fetches page 2 from all cities for customers in the USA without a distinct filter, so allowduplicates is set to true (default).
		/// </summary>
		[Test]
		public void GetValueListFromEntitySetWithFilterAndWithDistinctAndPaging()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//ILLBLGenProQuery q = (ILLBLGenProQuery)(
			//                        from c in metaData.Customer
			//                        where c.Country == "USA"
			//                        select c.City).Distinct().TakePage(3, 5);
			//List<string> cities = (List<string>)q.Execute();
			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "USA")
						.Select(() => CustomerFields.City.ToValue<string>())
						.Distinct()
						.Page(3, 5);
			var cities = new TypedListDAO().FetchQuery(q);
			// Portland is duplicated, which should be filtered out. This means that page 3 has just 2 rows.
			Assert.AreEqual(2, cities.Count);
		}


		/// <summary>
		/// Test which sets the MaxNumberOfElementsToRetrieve for the set of values retrieved (all cities for customers in the USA).
		/// </summary>
		[Test]
		public void GetValueListWithMaxLimitAndFilter()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//ILLBLGenProQuery q = (ILLBLGenProQuery)(
			//                        from c in metaData.Customer
			//                        where c.Country == "USA"
			//                        select c.City).Take(5);
			//List<string> cities = (List<string>)q.Execute();
			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "USA")
						.Select(() => CustomerFields.City.ToValue<string>())
						.Limit(5);
			var cities = new TypedListDAO().FetchQuery(q);
			Assert.AreEqual(5, cities.Count);
		}


		[Test]
		public void GetAnonymousTypedListFromCustomerAndOrderData()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId
			//        where c.Country == "Germany"
			//        select new { Company = c.CompanyName, OrderID = o.OrderId };

			var qf = new QueryFactory();
			var q = qf.Customer
							.From(QueryTarget.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
							.Where(CustomerFields.Country == "Germany")
							.Select(() => new
							{
								CompanyName = CustomerFields.CompanyName.ToValue<string>(),
								OrderId = OrderFields.OrderId.ToValue<int>()
							});
			var results = new TypedListDAO().FetchQuery(q);
			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        let x = c.Region ?? "Undef"
			//        where (c.Country != "Germany")
			//            && x != "Undef"
			//        select new { CompName = c.CompanyName };

			// Let isn't directly supported in QuerySpec, so define it outside.
			var x = Functions.Coalesce(CustomerFields.Region, "Undef");
			var qf = new QueryFactory();
			var q = qf.Customer
						.Where((CustomerFields.Country != "Germany").And(x.NotEqual("Undef")))
						.Select(CustomerFields.CompanyName);

			// alternative, simply inline x (prefered)
			//var q = qf.Customer
			//            .Where((CustomerFields.Country != "Germany").And(Functions.Coalesce(CustomerFields.Region, "Undef").NotEqual("Undef")))
			//            .Select(CustomerFields.CompanyName);

			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new { CustomerID = c.CustomerId, NumberOfOrders = c.Orders.Count() };

			var qf = new QueryFactory();
			var q = qf.Customer
						.Select(() => new
						{
							CustomerID = CustomerFields.CustomerId.ToValue<string>(),
							NumberOfOrders = qf.Order
													.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
													.CountRow()
													.ToValue<int>()
						});
			var results = new TypedListDAO().FetchQuery(q);
			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//        select new { c.CustomerId, TotalAllOrders = c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice)) }
			//        ).Max(n=>n.TotalAllOrders);

			// nest the orderdetail scalar query in the projection of the order, which is inside another scalar query.
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(qf.Field("TotalAllOrders").Source("C").Max())
						.From(qf.Customer.As("C")
									.Select(CustomerFields.CustomerId,
											qf.Order
												.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
												.Select(qf.OrderDetail
															.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
															.Select(OrderDetailFields.Quantity * OrderDetailFields.UnitPrice)
															.Sum().As("OrderTotal"))
												.Sum().As("TotalAllOrders")));
			var result = new TypedListDAO().GetScalar<decimal>(q);

			// alternative 
			//var q = qf.Customer.As("C")
			//                .Select(CustomerFields.CustomerId,
			//                        qf.Order
			//                            .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
			//                            .Select(qf.OrderDetail
			//                                        .CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
			//                                        .Select(OrderDetailFields.Quantity * OrderDetailFields.UnitPrice)
			//                                        .Sum().As("OrderRowTotal"))
			//                            .Sum().As("TotalAllOrders"));
			// wrap the query with a max call. QuerySpec works with the real typed value, so Max() simply returns the maximum value of the Sum
			// which is a decimal.
			//var result = new TypedListDAO().GetScalar<decimal>(
			//                            qf.Create()
			//                                .Select(qf.Field("TotalAllOrders").Source(q.Alias).Max())
			//                                .From(q));

			Assert.AreEqual(112804.1900M, result);
		}


		/// <summary>
		/// Gets the maximum order total. This is a test to see if Max works in a single value return setting.
		/// </summary>
		[Test]
		public void GetTheMaximumOrderTotalUsingMaxCallOnSequenceOfValues()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         select c.Orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.UnitPrice))
			//        ).Max();
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(qf.Customer.As("C")
									.Select(CustomerFields.CustomerId,
											qf.Order
												.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
												.Select(qf.OrderDetail
															.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
															.Select(OrderDetailFields.Quantity * OrderDetailFields.UnitPrice)
															.Sum().As("OrderTotal"))
												.Sum().As("TotalAllOrders"))
									.Max());
			var result = new TypedListDAO().GetScalar<decimal>(q);

			Assert.AreEqual(112804.1900, result);
		}


		/// <summary>
		/// Gets the total count of distinct employee ids in orders.Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfDistinctEmployeeIdsInOrders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Order.CountColumn(o=>o.EmployeeId, true);
			var qf = new QueryFactory();
			var q = qf.Create().Select(OrderFields.EmployeeId.CountDistinct());
			var result = new TypedListDAO().GetScalar<int>(q);

			Assert.AreEqual(9, result);
		}


		/// <summary>
		/// Gets the total count of all employee ids in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTotalCountOfAllEmployeeIdsInOrders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Order.CountColumn(o => o.EmployeeId);
			var qf = new QueryFactory();
			var q = qf.Create().Select(OrderFields.EmployeeId.Count());
			var result = new TypedListDAO().GetScalar<int>(q);

			Assert.AreEqual(818, result); // equal to the # of rows.
		}

		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheStandardDeviationOverEmployeeIDsInOrders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Order.StandardDeviation(o => o.EmployeeId);
			var qf = new QueryFactory();
			var q = qf.Create().Select(OrderFields.EmployeeId.StDev());
			var result = new TypedListDAO().GetScalar<double>(q);

			Assert.AreEqual(2.4470306717636521, result);
		}


		/// <summary>
		/// Gets the standard deviation over employeeIds in orders. Test to see if our extension method works
		/// </summary>
		[Test]
		public void GetTheVarianceOverEmployeeIDsInOrders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Order.Variance(o => o.EmployeeId);
			var qf = new QueryFactory();
			var q = qf.Create().Select(OrderFields.EmployeeId.Variance());
			var result = new TypedListDAO().GetScalar<double>(q);

			Assert.AreEqual(5.9879591085520705, result);
		}


		/// <summary>
		/// Gets the list of customer ids plus order total for customers with more than10 orders using duplicated aggregate method calls.
		/// </summary>
		[Test]
		public void GetListOfCustomerIdsPlusOrderTotalForCustomersWithMoreThan10OrdersUsingDuplicatedAggregateMethodCalls()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Orders.Count() > 10
			//        select new { c.CustomerId, TotalAmountOrders = c.Orders.Count() };

			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
									.CountRow().GreaterThan(10))
						.Select(CustomerFields.CustomerId, qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
																.CountRow().As("TotalAmountOrders"));

			// alternative
			//var q = qf.Create()
			//            .Select(qf.Field("CustomerId").Source("c"), qf.Field("TotalAmountOrders").Source("c"))
			//            .From(qf.Customer.As("c")
			//                        .Select(CustomerFields.CustomerId, qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
			//                                                    .CountRow().As("TotalAmountOrders")))
			//            .Where(qf.Field("TotalAmountOrders").Source("c").GreaterThan(10));
			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        select new { Country = g.Key, NumberOfCustomers = g.Count() };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.ToValue<string>(),
							NumberOfCustomers = Functions.CountRow().ToValue<int>()
						})
						.GroupBy(CustomerFields.Country);

			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
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
			LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        orderby g.Key
			//        select new { Country = g.Key, NumberOfCustomers = g.Count() };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.Source("g").ToValue<string>(),
							NumberOfCustomers = qf.Field("NumberOfCustomers").Source("g").ToValue<int>()
						})
						.From(qf.Create("g")
									.Select(CustomerFields.Country, Functions.CountRow().As("NumberOfCustomers"))
									.GroupBy(CustomerFields.Country))
						.OrderBy(CustomerFields.Country.Source("g").Ascending());

			// Variant with separate projection and projector lambda
			//var q = qf.Create()
			//            .Select(CustomerFields.Country.Source("g"), qf.Field("NumberOfCustomers").Source("g"))
			//            .From(qf.Create("g")
			//                        .Select(CustomerFields.Country, Functions.CountRow().As("NumberOfCustomers"))
			//                        .GroupBy(CustomerFields.Country))
			//            .OrderBy(CustomerFields.Country.Source("g").Ascending())
			//            .WithProjector(r=>new { Country=(string)r[0], NumberOfCustomers = (int)r[1]});

			var result = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			string previous = string.Empty, current = string.Empty;
			foreach(var v in result)
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


		/// <summary>
		/// Gets the list of countries with the total number of customers per country using group by ordered by country filtered on null. simple but where has to be
		/// outside the groupby.
		/// </summary>
		[Test]
		public void GetListOfCountriesWithTheTotalNumberOfCustomersPerCountryUsingGroupByOrderedByCountryFilteredOnNull()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        orderby g.Key
			//        where g.Key == null
			//        select new { Country = g.Key, NumberOfCustomers = g.Count() };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.Source("g").ToValue<string>(),
							NumberOfCustomers = qf.Field("NumberOfCustomers").Source("g").ToValue<int>()
						})
						.From(qf.Create("g")
									.Select(CustomerFields.Country, Functions.CountRow().As("NumberOfCustomers"))
									.GroupBy(CustomerFields.Country))
						.Where(CustomerFields.Country.Source("g").IsNull())
						.OrderBy(CustomerFields.Country.Source("g").Ascending());

			var result = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in result)
			{
				counter++;
			}
			Assert.AreEqual(1, counter);
		}


		[Test]
		public void GetValueListFromJoinedSetWithIntoAndWithoutSpecifyingDefaultIfEmpty()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId into co
			//        from x in co
			//        select x.CustomerId;
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(OrderFields.CustomerId)
						.From(qf.Customer.InnerJoin(qf.Order)
											.On(CustomerFields.CustomerId == OrderFields.CustomerId));
			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        orderby g.Key
			//        where g.Sum(n => n.Orders.Count()) > 10
			//        select new { Country = g.Key, NumberOfCustomers = g.Count() };
			var qf = new QueryFactory();
			// QuerySpec supports HAVING clauses, this query variant uses it instead of a where clause outside the groupby query
			var q = qf.Create()
						.Select(CustomerFields.Country.Source("g"), qf.Field("g", "NumberOfCustomers"))
						.From(qf.Create()
									.Select(CustomerFields.Country.Source("c"),
											Functions.CountRow().As("NumberOfCustomers"),
											qf.Field("c", "NumberOfOrders").Sum().As("TotalNumberOfOrders"))
									.From(qf.Customer
												.Select(CustomerFields.Country,
														qf.Order
															.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
															.CountRow().As("NumberOfOrders"))
												.As("c"))
									.GroupBy(CustomerFields.Country.Source("c"))
									.Having(qf.Field("c", "NumberOfOrders").Sum().GreaterThan(10))
									.As("g"))
						.OrderBy(CustomerFields.Country.Source("g").Ascending());


			// alternative, using where outside group by
			//var q = qf.Create()
			//            .Select(CustomerFields.Country.Source("g"), qf.Field("g", "NumberOfCustomers"))
			//            .From(qf.Create()
			//                        .Select(CustomerFields.Country.Source("c"),
			//                                Functions.CountRow().As("NumberOfCustomers"),
			//                                qf.Field("c","NumberOfOrders").Sum().As("TotalNumberOfOrders"))
			//                        .From(qf.Customer
			//                                    .Select(CustomerFields.Country,
			//                                            qf.Order
			//                                                .CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
			//                                                .CountRow().As("NumberOfOrders"))
			//                                    .As("c"))
			//                        .GroupBy(CustomerFields.Country.Source("c"))
			//                        .As("g"))
			//            .Where(qf.Field("g","TotalNumberOfOrders").GreaterThan(10))
			//            .OrderBy(CustomerFields.Country.Source("g").Ascending());


			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        group o by o.Customer.Country into g
			//        orderby g.Key
			//        where g.Sum(n => n.OrderDetails.Count()) > 10
			//        select new { Country = g.Key, Num = g.Average(n => n.OrderDetails.Count(od => od.ProductId == 3)) };

			// In linq folding takes place as SQL can handle 1 aggregate at a time. (you can't do AVG(COUNT(x)))
			// Folding is 'wrapping' a query with a new one and adding the
			// aggregate to execute in the wrapping query's projection, effectively folding the real query inside a new one.
			// In QuerySpec, no folding takes place automatically
			// so we'll write out the end query from top to bottom. The query is bigger than the linq query, but it's close to what's actually
			// executed in SQL (the Linq query produces similar sql), so predictability is higher.
			// To write this query, start with the inner query (the one aliased as 'c' in this query), as that's the only query which produces new
			// data and to which all other data is either based on or related to. As the queryspec is close to the SQL query executed, it can be tweaked
			// better if it performs too slow at runtime due to the data consumed
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(CustomerFields.Country.Source("g"), qf.Field("g", "Num"))
						.From(qf.Create()
									.Select(CustomerFields.Country.Source("sg"),
											qf.Field("sg", "NumberOfODRows").Sum().As("TotalNumberOfODRows"),
											qf.Field("sg", "NumberOfODRowsForAVG").Avg().As("Num"))
									.From(qf.Create()
											.Select(CustomerFields.Country.Source("c"),
													qf.Field("c", "NumberOfODRows"),
													qf.OrderDetail
															.CorrelatedOver(OrderEntity.Relations.OrderDetailEntityUsingOrderId, "c", string.Empty)
															.Where(OrderDetailFields.ProductId == 3)
															.CountRow().As("NumberOfODRowsForAVG"))
											.From(qf.Order
														.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
														.Select(CustomerFields.Country,
																OrderFields.OrderId,	// so outer query can correlate 2nd order detail aggregate properly
																qf.OrderDetail
																		.CorrelatedOver(OrderEntity.Relations.OrderDetailEntityUsingOrderId)
																		.CountRow().As("NumberOfODRows"))
														.As("c"))
											.As("sg"))
									.GroupBy(CustomerFields.Country.Source("sg"))
									.Having(qf.Field("sg", "NumberOfODRows").Sum().GreaterThan(10))
									.As("g"))
						.OrderBy(CustomerFields.Country.Source("g").Ascending());
			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
			{
				counter++;
			}
			Assert.AreEqual(22, counter);
		}


		[Test]
		public void GetComplicatedListWithTwoAggregatesOnAGroupBy2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        group o by o.Customer.Country into g
			//        orderby g.Key
			//        where g.Sum(n => n.OrderDetails.Count()) > 10
			//        select new { Country = g.Key, Num = g.Average(n => n.EmployeeId) };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(CustomerFields.Country.Source("g"), qf.Field("g", "Num"))
						.From(qf.Create()
									.Select(CustomerFields.Country.Source("c"),
											qf.Field("c", "NumberOfODRows").Sum().As("TotalNumberOfODRows"),
											OrderFields.EmployeeId.Source("c").Avg().As("Num"))
									.From(qf.Order
												.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
												.Select(CustomerFields.Country,
														OrderFields.EmployeeId,
														qf.OrderDetail
																.CorrelatedOver(OrderEntity.Relations.OrderDetailEntityUsingOrderId)
																.CountRow().As("NumberOfODRows"))
												.As("c"))
									.GroupBy(CustomerFields.Country.Source("c"))
									.Having(qf.Field("c", "NumberOfODRows").Sum().GreaterThan(10))
									.As("g"))
						.OrderBy(CustomerFields.Country.Source("g").Ascending());
			var results = new TypedListDAO().FetchQuery(q);

			int counter = 0;
			foreach(var v in results)
			{
				counter++;
			}
			Assert.AreEqual(22, counter);
		}


		[Test]
		public void GetSetOfTuplesOfConcreteClassesFromProjection()
		{
			LinqMetaData metaData = new LinqMetaData();

			//var q = from c in metaData.Customer
			//        select new KeyValuePair<string, string>(c.Country, c.City);
			//List<KeyValuePair<string, string>> results = q.ToList();

			var qf = new QueryFactory();
			var q = qf.Customer.Select(() => new KeyValuePair<string, string>(CustomerFields.Country.ToValue<string>(), CustomerFields.City.ToValue<string>()));
			var results = new TypedListDAO().FetchQuery(q);

			Assert.AreEqual(91, results.Count);
		}


		[Test]
		public void GetAnonymousTypedListFromCustomersInLondonUsingJoinWithNestedLetAndWhereInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId
			//        let m = c.Phone
			//        orderby c.City
			//        where c.Country == "UK"
			//        where m != "555-5555"
			//        select new { c.City, c.ContactName } into a
			//        where a.City == "London"
			//        select a;

			// QuerySpec doesn't support Let, so specify the value in full instead. 
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								City = CustomerFields.City.Source("a").ToValue<string>(),
								ContactName = CustomerFields.ContactName.Source("a").ToValue<string>()
							})
							.From(qf.Customer
									.From(QueryTarget.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
									.OrderBy(CustomerFields.City.Ascending())
									.Where(CustomerFields.Country == "UK")
									.AndWhere(CustomerFields.Phone != "555-5555")
									.Select(CustomerFields.City, CustomerFields.ContactName)	// no need for typed projection.
									.As("a"))
							.Where(CustomerFields.City.Source("a") == "London");
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.AreEqual("London", v.City);
			}
			Assert.AreEqual(46, count);		// duplicates through join with orders.
		}


		[Test]
		public void GetStringFromCustomerBasedOnIIFStatementInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            c.Country,
			//            CheckValue = c.Country == "USA" ? c.ContactName : c.ContactTitle,
			//            c.ContactName,
			//            c.ContactTitle
			//        };
			var qf = new QueryFactory();
			var q = qf.Customer
						.Select(() => new
						{
							Country = CustomerFields.Country.ToValue<string>(),
							CheckValue = Functions.IIF(CustomerFields.Country == "USA", CustomerFields.ContactName, CustomerFields.ContactTitle).ToValue<string>(),
							ContactName = CustomerFields.ContactName.ToValue<string>(),
							ContactTitle = CustomerFields.ContactTitle.ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			foreach(var v in results)
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

			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select stringChopper(c.CompanyName);
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => stringChopper(CustomerFields.CompanyName.ToValue<string>())); // call the func in-memory inside the projector lambda
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(3, v.Length);
			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelation()
		{
			Func<string, string> stringChopper = s => s.Substring(0, 3);

			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//         select stringChopper(o.Customer.CompanyName)).Distinct();

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => stringChopper(CustomerFields.CompanyName.ToValue<string>()))
						.Distinct();
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(3, v.Length);
			}
		}


		[Test]
		public void GetFirstCharactersOfCompanyNamesUsingInlineFuncCallInProjectionViaRelationIntoAnonymousType()
		{
			Func<string, string> stringChopper = s => s.Substring(0, 3);

			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//         select new { FirstChars = stringChopper(o.Customer.CompanyName), 
			//                     o.Customer.CompanyName }).Distinct();
			var qf = new QueryFactory();
			// as we're re-using the value from the projection in the projector lambda, this is more efficient:
			var q = qf.Order
						.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
						.Select(CustomerFields.CompanyName)
						.Distinct()
						.WithProjector(r => new { FirstChars = stringChopper((string)r[0]), CompanyName = (string)r[0] });

			// alternative with lambda inside select:
			//var q = qf.Order
			//            .From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
			//            .Select(() => new
			//            {
			//                FirstChars = stringChopper(CustomerFields.CompanyName.ToValue<string>()),
			//                CompanyName = CustomerFields.CompanyName.As("c1").ToValue<string>()			// alias the field, as it's mentioned twice in the projection.
			//            })
			//            .Distinct();


			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(3, v.FirstChars.Length);
				Assert.AreEqual(v.FirstChars, v.CompanyName.Substring(0, 3));
			}
		}


		[Test]
		public void GetListOfBooleansProducedFromBooleanExpressionInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country=="USA"
			//        select (c.Country == "USA");
			var q = new QueryFactory().Customer
							.Where(CustomerFields.Country == "USA")
							.Select(() => (CustomerFields.Country == "USA").ToValue());
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.IsTrue(v);
			}
		}


		[Test]
		public void GetAnonymousTypedListWithBooleanExpressionInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            c.CustomerId,
			//            IsAmerican = (c.Country == "USA"),
			//            c.Country
			//        };
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							IsAmerican = (CustomerFields.Country == "USA").ToValue(),
							Country = CustomerFields.Country.ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			foreach(var v in results)
			{
				Assert.AreEqual((v.Country == "USA"), v.IsAmerican);
			}
		}


		[Test]
		public void ConcatTwoFetchedValuesByAnInstanceMethodOfANewObject()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country != null
			//        select new
			//        {
			//            c.Country,
			//            c.City,
			//            ConcatenatedResult = new Concatenator(c.Country, c.City).ConcatMembers()
			//        };
			var qf = new QueryFactory();
			// this variant is more efficient, as it re-uses the values from the projection in the projection lambda. 
			var q = qf.Create()
							.Select(CustomerFields.Country, CustomerFields.City)
							.Where(CustomerFields.Country.IsNotNull())
							.WithProjector(r => new
							{
								Country = (string)r[0],
								City = (string)r[1],
								ConcatenatedResult = new Concatenator((string)r[0], (string)r[1]).ConcatMembers()
							});
			var results = new TypedListDAO().FetchQuery(q);

			foreach(var v in results)
			{
				Assert.AreEqual(v.Country + v.City, v.ConcatenatedResult);
			}
		}


		[Test]
		public void ConcatTwoFetchedValuesWithOneBeingScalarQueryReferredToByLetByAStaticMethod()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country != null
			//        let x = c.Orders.Count()
			//        select new
			//        {
			//            c.City,
			//            AmountOrders = x,
			//            ConcatenatedResult = Concatenator.Concat(x.ToString(), c.City)
			//        };
			var qf = new QueryFactory();
			var q = qf.Customer
							.Select(CustomerFields.City, qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId).CountRow())
							.Where(CustomerFields.Country.IsNotNull())
							.WithProjector(r => new
							{
								City = (string)r[0],
								AmountOrders = (int)r[1],
								ConcatenatedResult = Concatenator.Concat(r[1].ToString(), (string)r[0])
							});
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(v.AmountOrders + v.City, v.ConcatenatedResult);
			}
		}

		[Test]
		public void GetNestedQueryWithCustomerAndOrdersFromGermany()
		{
			var qf = new QueryFactory();
			var q = qf.Customer
							.Where(CustomerFields.Country == "Germany")
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								Orders = qf.Order
												.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
												.Select(() => new
												{
													OrderId = OrderFields.OrderId.ToValue<int>(),
													OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
												})
												.ToResultset()
							});
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.IsTrue(v.Orders.Count > 0);
			}
			Assert.AreEqual(11, results.Count);
		}


		[Test]
		public void GetNestedTreeOfCustomerOrderOrderDetailEmployeeProductInfoUsingNestedQueriesFilteredOnCustomersFromGermany()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var results = from c in metaData.Customer
			//        where c.Country == "Germany"
			//        select new
			//        {
			//            c.CompanyName,
			//            c.City,
			//            Orders = from o in c.Orders
			//                     select new
			//                     {
			//                         OrderDetails = from od in o.OrderDetails
			//                                        select new
			//                                        {
			//                                            od.Product,
			//                                            od.Quantity,
			//                                            od.UnitPrice
			//                                        },
			//                         o.Employee,
			//                         o.OrderDate
			//                     }
			//        };

			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "Germany")
						.Select(() => new
						{
							CompanyName = CustomerFields.CompanyName.ToValue<string>(),
							City = CustomerFields.City.ToValue<string>(),
							Orders = qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select(() => new
											{
												OrderDetails = qf.OrderDetail
																	.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
																	.Select(() => new
																	{
																		Product = qf.Product
																						.CorrelatedOver(OrderDetailEntity.Relations.ProductEntityUsingProductId)
																						.ToSingleResult(),
																		Quantity = OrderDetailFields.Quantity.ToValue<short>(),
																		UnitPrice = OrderDetailFields.UnitPrice.ToValue<decimal>()
																	})
																	.ToResultset(),
												Employee = qf.Employee
																.CorrelatedOver(EmployeeEntity.Relations.OrderEntityUsingEmployeeId)
																.ToSingleResult(),
												OrderDate = OrderFields.OrderDate.ToValue<DateTime>()
											})
											.ToResultset()
						});
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var results = from c in metaData.Customer
			//              where !new List<string>() { "FISSA", "PARIS" }.Contains(c.CustomerId)
			//              select c.Orders;

			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.CustomerId.NotIn(new List<string>() { "FISSA", "PARIS" }))
						.Select(() => qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.ToResultset()
						);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.Count > 0);		// v is of type EntityCollection<OrderEntity> :)
				Assert.IsTrue(v[0].CustomerId.Length > 0);
			}
			Assert.AreEqual(89, count);
		}

		[Test]
		public void GetListOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//        where o.EmployeeId == 2
			//        select o.Customer).Distinct();

			var qf = new QueryFactory();
			var q = qf.Customer
							.From(QueryTarget.RightJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
							.Where(OrderFields.EmployeeId == 2);
			var results = new CustomerCollection();
			results.GetMulti(q);

			int count = 0;
			foreach(CustomerEntity v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyName.Length > 0);
			}
			Assert.AreEqual(62, count);
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var count = (from o in metaData.Order
			//         where o.EmployeeId == 2
			//         select o.Customer).Count();
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(Functions.CountRow())
						.From(qf.Customer
									.From(QueryTarget.RightJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
									.Where(OrderFields.EmployeeId == 2));
			var count = new TypedListDAO().GetScalar<int>(q);

			Assert.AreEqual(110, count);
		}


		[Test]
		public void GetCountOfCustomerObjectsViaOrderForAllOrdersFiledByEmployee2WithDistinct()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var count = (from o in metaData.Order
			//             where o.EmployeeId == 2
			//             select o.Customer).Distinct().Count();
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(Functions.CountRow())
						.From(qf.Customer
									.From(QueryTarget.RightJoin(qf.Order).On(CustomerFields.CustomerId == OrderFields.CustomerId))
									.Where(OrderFields.EmployeeId == 2)
									.Distinct());
			var count = new TypedListDAO().GetScalar<int>(q);

			Assert.AreEqual(62, count);
		}


		[Test]
		public void GetDistinctSetOfCompanyNamesAndEmployeesWhoFiledTheirOrders()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//         select new
			//         {
			//             o.EmployeeId,
			//             CompanyName = o.Customer.CompanyName
			//         }).Distinct();
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
								CompanyName = CustomerFields.CustomerId.ToValue<string>()
							})
							.From(qf.Order.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var results = (from o in metaData.Order
			//               orderby o.EmployeeId ascending, o.CustomerId ascending
			//               select new
			//               {
			//                   o.EmployeeId,
			//                   CompanyNameValue = (from c in metaData.Customer where c.CustomerId == o.CustomerId select new { c.CompanyName }).Single()
			//               }).Distinct();

			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
								CompanyNameValue = qf.Customer
															.CorrelatedOver(CustomerEntity.Relations.OrderEntityUsingCustomerId)
															.Select(() => new { CompanyName = CustomerFields.CompanyName.ToValue<string>() })
															.ToSingleResult()
							})
							.OrderBy(OrderFields.EmployeeId.Ascending(), OrderFields.CustomerId.Descending())
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyNameValue.CompanyName.Length > 0);
				Assert.IsTrue((v.EmployeeId > 0) && (v.EmployeeId < 10));
			}
			Assert.AreEqual(472, count);
		}

		[Test]
		public void GetEmployeeFullnameRelatedToOrder()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        select new
			//        {
			//            EmployeeName = o.Employee.FirstName + o.Employee.LastName
			//        };
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new { EmployeeName = StringFunctions.Concat(EmployeeFields.FirstName, EmployeeFields.LastName).ToValue<string>() })
						.From(qf.Order.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId));
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.EmployeeName.Length > 0);
			}
			Assert.AreEqual(818, count);
		}


		[Test]
		public void GetOrderIDsWithCompanynameInsideAnonymousTypeUsingNestedQueryWithoutLimiter()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        select new
			//        {
			//            o.OrderId,
			//            CompanyName = (from c in metaData.Customer where c.CustomerId == o.CustomerId select c.CompanyName).ToList()
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								OrderId = OrderFields.OrderId.ToValue<int>(),
								CompanyName = qf.Customer
													.CorrelatedOver(CustomerEntity.Relations.OrderEntityUsingCustomerId)
													.Select(() => CustomerFields.CompanyName.ToValue<string>())
													.ToResultset()
							});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country == "Germany"
			//        select new
			//        {
			//            c.City,
			//            LastFiveOrders = (from o in c.Orders orderby o.OrderDate descending select new { o.OrderId, o.OrderDate }).Take(3),
			//            c.CompanyName
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							City = CustomerFields.City.ToValue<string>(),
							LastFiveOrders = qf.Order
													.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
													.Select(() => new
													{
														OrderId = OrderFields.OrderId.ToValue<int>(),
														OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
													})
													.OrderBy(OrderFields.OrderDate.Descending())
													.Limit(3)
													.ToResultset(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.Country == "Germany");
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            c.Country,
			//            Orders = c.Orders,
			//            c.CompanyName
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.ToValue<string>(),
							Orders = qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId).ToResultset(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						});

			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
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
			DaoBase.ParameterisedPrefetchPathThreshold = 100;		// will make it use an IN clause

			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            c.Country,
			//            Orders = c.Orders,
			//            c.CompanyName
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.ToValue<string>(),
							Orders = qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId).ToResultset(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);
			DaoBase.ParameterisedPrefetchPathThreshold = 50;

			int count = 0;
			foreach(var v in results)
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
		public void GetMinIdAndMaxIdUsingGroupOnCountryFromCustomers()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country == "USA"
			//        group c by c.Country into cc
			//        select new { MinId = cc.Min(c => c.CustomerId), MaxId = cc.Max(c => c.CustomerId) };

			var qf = new QueryFactory();
			// QuerySpec allows fine-grained control over the sql generation, so the group by query can be made more efficient than the linq query.
			var q = qf.Create()
						.Select(() => new
						{
							Country = CustomerFields.Country.ToValue<string>(),
							MinId = CustomerFields.CustomerId.Min().As("MinId").ToValue<string>(),
							MaxId = CustomerFields.CustomerId.Max().As("MaxId").ToValue<string>()
						})
						.Where(CustomerFields.Country == "USA")
						.GroupBy(CustomerFields.Country);

			//  Alternative which is equal to the Linq query but which is a bit less efficient (as it uses a derived table)
			//var q = qf.Create()
			//            .Select(() => new
			//            {
			//                MinId = qf.Field("MinId").Source("g").ToValue<string>(),
			//                MaxId = qf.Field("MaxId").Source("g").ToValue<string>()
			//            })
			//            .From(qf.Create("g")
			//                        .Select(CustomerFields.Country, CustomerFields.CustomerId.Min().As("MinId"), CustomerFields.CustomerId.Max().As("MaxId"))
			//                        .Where(CustomerFields.Country == "USA")
			//                        .GroupBy(CustomerFields.Country));
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual("GREAL", v.MinId);
				Assert.AreEqual("WHITC", v.MaxId);
			}
		}


		[Test]
		public void GetCountOfCustomersAndMaxIdUsingGroupOnConstant()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country=="USA"
			//        group c by 0 into cc
			//        select new { Key = cc.Key, Count = cc.Count(), MaxId = cc.Max(c=>c.CustomerId)};

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Key = qf.Field("C").Source("gs").ToValue<int>(),
							Count = Functions.CountRow().As("Count").ToValue<int>(),
							MaxId = CustomerFields.CustomerId.Source("gs").Max().ToValue<string>()
						})
						.From(qf.Create("gs")
									.Select(Functions.Constant(0).As("C"), CustomerFields.CustomerId)
									.Where(CustomerFields.Country == "USA"))
						.GroupBy(qf.Field("C").Source("gs"));
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.AreEqual(13, v.Count);
				Assert.AreEqual("WHITC", v.MaxId);
			}
		}

		[Test]
		public void GetCountOfCustomersUsingGroupOnConstantIntoObjectArray()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country == "USA"
			//        group c by 0 into cc
			//        select new object[] { cc.Count(), cc.Max(c => c.CustomerId) };
			//object[][] values = q.ToArray();

			// by default queryspec returns data in object arrays
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(Functions.CountRow().As("Count"), CustomerFields.CustomerId.Source("gs").Max())
						.From(qf.Create("gs")
									.Select(Functions.Constant(0).As("C"), CustomerFields.CustomerId)
									.Where(CustomerFields.Country == "USA"))
						.GroupBy(qf.Field("C").Source("gs"));

			var values = new TypedListDAO().FetchQuery(q).ToArray();
			Assert.AreEqual(13, values[0][0]);
			Assert.AreEqual("WHITC", values[0][1]);
		}

		[Test]
		public void GetCompanyNameAndCustomerIdInStringListObjects()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new List<string>() { c.CustomerId, c.CompanyName };
			//List<string>[] values = q.ToArray();

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new List<string>() { CustomerFields.CustomerId.ToValue<string>(), CustomerFields.CompanyName.ToValue<string>() });
			var values = new TypedListDAO().FetchQuery(q).ToArray();

			Assert.AreEqual(91, values.Length);
			foreach(List<string> customerData in values)
			{
				Assert.IsTrue(customerData[0].Length > 0);
				Assert.IsTrue(customerData[1].Length > 0);
			}
		}

		[Test]
		public void GetOrderDataWithTotalNumberOfProductsBoughtBelowGivenPrice()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        select new
			//        {
			//            o.OrderId,
			//            o.OrderDate,
			//            AmountProductsBought = o.OrderDetails.Where(od=>od.UnitPrice<2.0M).Sum(od=>od.Quantity)
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>(),
							AmountProductsBought = qf.OrderDetail
														.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
														.Where(OrderDetailFields.UnitPrice < 2.0M)
														.Sum(OrderDetailFields.Quantity)
														.ToValue<int?>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(818, count);
		}

		[Test]
		public void GetOrderDataFromOrdersWithMoreThan1ProductBoughtBelowGivenPriceUsingWrappedWhereAndPaging()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from o in metaData.Order
			//        select new
			//        {
			//            o.OrderId,
			//            ShippedDate = o.ShippedDate ?? DateTime.Now,
			//            AmountProductsBought = o.OrderDetails.Where(od => od.UnitPrice < 10.0M).Sum(od => od.Quantity)
			//        } into result
			//        where result.AmountProductsBought > 1
			//        select result).TakePage(2,5);

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							OrderId = qf.Field("OrderId").Source("result").ToValue<int>(),
							ShippedDate = qf.Field("ShippedDate").Source("result").ToValue<DateTime?>(),
							AmountProductsBought = qf.Field("AmountProductsBought").Source("result").ToValue<int?>()
						})
						.From(qf.Create("result")
									.Select(OrderFields.OrderId,
											Functions.Coalesce(OrderFields.ShippedDate, DateTime.Now).As("ShippedDate"),
											qf.OrderDetail
												.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
												.Where(OrderDetailFields.UnitPrice < 10.0M)
												.Sum(OrderDetailFields.Quantity)
												.As("AmountProductsBought")))
						.Where(qf.Field("AmountProductsBought").Source("result").GreaterThan(1))
						.Page(2, 5);	// Page on a dynamic query requires a typed projection if derived tables are used, and paging creates a temp table.
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(5, count);
		}


		[Test]
		public void GetOrderDataFromOrdersWithMoreThan1ProductBoughtBelowGivenPriceUsingWrappedWhereAndPaging2()
		{
			// Use the CastTo method to specify the result type of the field, which is required when paging with a temptable.
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(qf.Field("OrderId").Source("result").CastTo<int>(),
							qf.Field("ShippedDate").Source("result").CastTo<DateTime?>(),
							qf.Field("AmountProductsBought").Source("result").CastTo<int?>()
						)
						.From(qf.Create("result")
									.Select(OrderFields.OrderId,
											Functions.Coalesce(OrderFields.ShippedDate, DateTime.Now).As("ShippedDate"),
											qf.OrderDetail
												.CorrelatedOver(OrderDetailEntity.Relations.OrderEntityUsingOrderId)
												.Where(OrderDetailFields.UnitPrice < 10.0M)
												.Sum(OrderDetailFields.Quantity)
												.As("AmountProductsBought")))
						.Where(qf.Field("AmountProductsBought").Source("result").GreaterThan(1))
						.Page(2, 5);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				if(v[0] != null)
				{
					Assert.AreEqual(typeof(int), v[0].GetType());
				}
				count++;
			}
			Assert.AreEqual(5, count);
		}

		[Test]
		public void GetListOfAnonymousTypesWithNestedAnonymousTypesOfCustomerData()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            Address = new { c.City, c.Address, c.Country },
			//            ContactInfo = new { c.ContactTitle, c.ContactName, c.Phone, c.Fax },
			//            Foo = true,
			//            c.CompanyName,
			//            c.CustomerId
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Address = new
							{
								City = CustomerFields.City.ToValue<string>(),
								Address = CustomerFields.Address.ToValue<string>(),
								Country = CustomerFields.Country.ToValue<string>()
							},
							ContactInfo = new
							{
								ContactTitle = CustomerFields.ContactTitle.ToValue<string>(),
								ContactName = CustomerFields.ContactName.ToValue<string>(),
								Phone = CustomerFields.Phone.ToValue<string>(),
								Fax = CustomerFields.Fax.ToValue<string>()
							},
							Foo = true,
							CompanyName = CustomerFields.CompanyName.ToValue<string>(),
							CustomerId = CustomerFields.CustomerId.ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by new { c.Country, c.City } into g
			//        select new { SomeKey = g.Key, NumberOfCustomers = g.Count() };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							SomeKey = new
							{
								Country = CustomerFields.Country.ToValue<string>(),
								City = CustomerFields.City.ToValue<string>()
							},
							NumberOfCustomers = Functions.CountRow().ToValue<int>()
						})
						.GroupBy(CustomerFields.Country, CustomerFields.City);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(70, count);
		}


		[Test]
		public void GroupByWithAggregateOnNullableTypeWithCoalesce()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        group o by o.CustomerId into g
			//        select new { ID = g.Key, Sum = g.Sum(x => (x.EmployeeId ?? 0)) };
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(OrderFields.CustomerId, Functions.Coalesce(OrderFields.EmployeeId, 0).Sum())
						.GroupBy(OrderFields.CustomerId);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(89, count);
		}


		[Test]
		public void GroupByWithDummyAnonymousTypeReferenceFieldSetOfCombinedFieldsAndAggregate()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        group o by new { CombinedName = o.Customer.City + o.Customer.Country } into g
			//        select new { Key = g.Key, Sum = g.Sum(o => o.EmployeeId) };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select((CustomerFields.City + CustomerFields.Country).As("CombinedName"), OrderFields.EmployeeId.Sum())
						.From(qf.Order.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
						.GroupBy((CustomerFields.City + CustomerFields.Country));
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(70, count);
		}


		[Test]
		public void GroupByOnJoinedRelatedEntityWithDefaultIsEmpty()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId into og
			//        from o in og.DefaultIfEmpty()
			//        group o by new
			//        {
			//            c.CustomerId,
			//            c.CompanyName
			//        } into g
			//        select new
			//        {
			//            Customer = g.Key,
			//            LastOrderDate = g.Max(d => d.OrderDate),
			//            TotalOrders = g.Count()
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Customer = new { CustomerId = CustomerFields.CustomerId.ToValue<string>(), CompanyName = CustomerFields.CompanyName.ToValue<string>() },
							LastOrderDate = OrderFields.OrderDate.Max().ToValue<DateTime?>(),
							TotalOrders = Functions.CountRow().ToValue<int>()
						})
						.From(qf.Customer.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
						.GroupBy(CustomerFields.CustomerId, CustomerFields.CompanyName);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.TotalOrders > 0);
			}

			Assert.AreEqual(91, count);
		}


		[Test]
		public void MultiLeftJoinWithDistinctFetchWhereProjectionOfJoinShouldBeUsedInResultQueryExpression()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//            join o in metaData.Order on c.CustomerId equals o.CustomerId into og
			//            from o in og.DefaultIfEmpty()
			//            join od in metaData.OrderDetail on o.OrderId equals od.OrderId into odg
			//            from od in og.DefaultIfEmpty()
			//            select new
			//            {
			//                c.CustomerId,
			//                od.OrderDate
			//            }
			//        ).Distinct();
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
							})
							.From(qf.Customer.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId)
											.LeftJoin(OrderEntity.Relations.OrderDetailEntityUsingOrderId))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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
		public void LeftJoinWithIIFExpressionInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId into og
			//        from o in og.DefaultIfEmpty()
			//        select new
			//        {
			//            c.CustomerId,
			//            c.CompanyName,
			//            HasOrdered = (o.CustomerId != null) ? "Yes they have" : "No they havent"
			//        }).Distinct();
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								CompanyName = CustomerFields.CompanyName.ToValue<string>(),
								HasOrdered = Functions.IIF(OrderFields.CustomerId.IsNotNull(), "Yes they have", "No they havent").ToValue<string>()
							})
							.From(qf.Customer.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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


		[Test]
		public void LeftJoinWithIIFExpressionWithResultIntoHardNullValueInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         join o in metaData.Order on c.CustomerId equals o.CustomerId into og
			//         from o in og.DefaultIfEmpty()
			//         select new
			//         {
			//             c.CustomerId,
			//             c.CompanyName,
			//             HasOrdered = (o.CustomerId != null) ? "Yes they have" : null
			//         }).Distinct();
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								CompanyName = CustomerFields.CompanyName.ToValue<string>(),
								HasOrdered = Functions.IIF(OrderFields.CustomerId.IsNotNull(), "Yes they have", null).ToValue<string>()
							})
							.From(qf.Customer.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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


		[Test]
		public void LeftJoinWithIIFExpressionInsideCoalesceStatementInProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         join o in metaData.Order on c.CustomerId equals o.CustomerId into og
			//         from o in og.DefaultIfEmpty()
			//         select new
			//         {
			//             c.CustomerId,
			//             c.CompanyName,
			//             o.ShipRegion,
			//             HasOrdered = o.ShipRegion ?? ((o.CustomerId != null) ? "Yes they have" : "No they havent")
			//         }).Distinct();

			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								CompanyName = CustomerFields.CompanyName.ToValue<string>(),
								ShipRegion = OrderFields.ShipRegion.ToValue<string>(),
								HasOrdered = Functions.Coalesce(OrderFields.ShipRegion,
																Functions.IIF(OrderFields.CustomerId.IsNotNull(), "Yes they have", "No they havent"))
																.ToValue<string>()
							})
							.From(qf.Customer.LeftJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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


		[Test]
		public void JoinOnCharExpression()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join e in metaData.Employee on c.CustomerId[0] equals e.LastName[0]
			//        select new { c.CustomerId, e.EmployeeId, e.LastName };
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							EmployeeId = EmployeeFields.EmployeeId.ToValue<int>(),
							LastName = EmployeeFields.LastName.ToValue<string>()
						})
						.From(qf.Customer.InnerJoin(qf.Employee)
										.On(StringFunctions.Char(CustomerFields.CustomerId, 0).Equal(StringFunctions.Char(EmployeeFields.LastName, 0))));
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.AreEqual(v.CustomerId[0], v.LastName[0]);
			}
			Assert.AreEqual(43, count);
		}


		[Test]
		public void JoinOnConstantChar()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join e in metaData.Employee on c.CustomerId[0] equals 'C'
			//        select new { c.CustomerId, e.EmployeeId, e.LastName };
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							EmployeeId = EmployeeFields.EmployeeId.ToValue<int>(),
							LastName = EmployeeFields.LastName.ToValue<string>()
						})
						.From(qf.Customer.InnerJoin(qf.Employee).On(StringFunctions.Char(CustomerFields.CustomerId, 0).Equal("C")));
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.AreEqual('C', v.CustomerId[0]);
			}
			Assert.AreEqual(45, count);
		}


		[Test]
		public void GroupingOfEntitiesByGroupByKeyIntoAnonymousType()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        select new
			//        {
			//            g.Key,
			//            CustomersInCountry = (
			//            from c2 in metaData.Customer
			//            where g.Key == c2.Country
			//            select c2)
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Key = CustomerFields.Country.ToValue<string>(),
							CustomersInCountry = qf.Customer.TargetAs("C")
														.CorrelatedOver(CustomerFields.Country.Source("C") == CustomerFields.Country)
														.ToResultset()
						})
						.GroupBy(CustomerFields.Country);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        group c by c.Country into g
			//        select new
			//        {
			//            g.Key,
			//            CustomersInCountry=g
			//        };

			// QuerySpec doesn't have a clause like group into, so the nested query setup,which the linq query results into, has to be
			// written out explicitly, exactly like the previous test. 'CustomersInCountry' isn't an IGrouping, but an IList<T>
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Key = CustomerFields.Country.ToValue<string>(),
							CustomersInCountry = qf.Customer.TargetAs("C")
														.CorrelatedOver(CustomerFields.Country.Source("C") == CustomerFields.Country)
														.ToResultset()
						})
						.GroupBy(CustomerFields.Country);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        where c.Country!=null
			//        group c by c.Country into g
			//        select g;

			// QuerySpec doesn't have a clause like group into, so the nested query setup,which the linq query results into, has to be
			// written out explicitly, exactly like the previous test. The result is stored in an IGrouping<T, U> implementation provided by QuerySpec
			// so consuming code is equal to the one which consumes the linq query.
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new Grouping<string, CustomerEntity>(
									CustomerFields.Country.ToValue<string>(),
									qf.Customer.TargetAs("C")
														.CorrelatedOver(CustomerFields.Country.Source("C") == CustomerFields.Country)
														.ToResultset()))
						.Where(CustomerFields.Country.IsNotNull())
						.GroupBy(CustomerFields.Country);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
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
			//LinqMetaData metaData = new LinqMetaData();

			//var q2 = from o in metaData.Order
			//         where o.OrderDate <= new DateTime(1997, 8, 1)
			//         && o.EmployeeId != null
			//         group o by o.CustomerId into g
			//         select new { LastId = g.Max(x => x.OrderId) };

			//var q1 = from o in metaData.Order
			//        join ids in q2
			//        on o.OrderId equals ids.LastId
			//        select o;

			//var q = from od in metaData.OrderDetail
			//        join o in q1 on od.OrderId equals o.OrderId
			//        group od by od.ProductId into g
			//        select new { g.Key, Total = g.Sum(od => od.Quantity) };

			var qf = new QueryFactory();
			var q2 = qf.Create("q2")
							.Select(OrderFields.OrderId.Max().As("LastId"))
							.Where(OrderFields.OrderDate.LesserEqual(new DateTime(1997, 8, 1))
									.And(OrderFields.EmployeeId.IsNotNull()))
							.GroupBy(OrderFields.CustomerId);

			var q1 = qf.Order.As("q1")
						.From(QueryTarget.InnerJoin(q2).On(OrderFields.OrderId == qf.Field("q2", "LastId")));

			var q = qf.Create()
						.Select(OrderDetailFields.ProductId.As("Key"), OrderDetailFields.Quantity.Sum().As("Total"))
						.From(qf.OrderDetail.InnerJoin(q1).On(OrderDetailFields.OrderId == OrderFields.OrderId.Source("q1")))
						.GroupBy(OrderDetailFields.ProductId);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(66, count);
		}

		[Test]
		public void GetCountOfJoinedSetWithAnonymousTypes()
		{
			//    LinqMetaData metaData = new LinqMetaData();
			//    var q = from c in metaData.Customer
			//            join o in metaData.Order on c.CustomerId equals o.CustomerId
			//            select new { CustomerId = c.CustomerId, CompanyName = c.CompanyName, OrderId = o.OrderId, OrderDate = o.OrderDate };
			//    int count = q.Count();

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(CustomerFields.CustomerId, CustomerFields.CompanyName, OrderFields.OrderId, OrderFields.OrderDate)
						.From(qf.Customer.InnerJoin(CustomerEntity.Relations.OrderEntityUsingCustomerId));
			var count = new TypedListDAO().GetScalar<int>(qf.Create().Select(q.CountRow()));
			Assert.AreEqual(818, count);
		}


		[Test]
		public void GetAnonymousTypeWithIIFAndWrappedWhereOnResultOfIIF()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from o in metaData.Order
			//         select new
			//         {
			//             o.OrderId,
			//             HasRegion = (o.ShipRegion == null) ? true : false
			//         };
			//var q2 = q1.Where(a => a.HasRegion == true);

			var qf = new QueryFactory();
			var q1 = qf.Create("q1")
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							HasRegion = Functions.IIF(OrderFields.ShipRegion.IsNull(), true, false).As("HasRegion").ToValue<bool>()
						});

			// QuerySpec doesn't wrap a query into a derived table if you append a where, so we use the special SelectFrom(query) call for that. 
			var q2 = qf.Create()
							.SelectFrom(q1)
							.Where(qf.Field(q1.Alias, "HasRegion").Equal(true));

			// alternative, verbose version:
			//var q2 = qf.Create()
			//                .Select(() => new
			//                {
			//                    OrderId = qf.Field("q1", "OrderId").ToValue<int>(),
			//                    HasRegion = qf.Field("q1", "HasRegion").ToValue<bool>()
			//                })
			//                .Where(qf.Field("q1", "HasRegion").Equal(true));

			// alternative: Simply repeat the expression in the where clause, no derived table needed. 
			// var q2 = q1.Where(OrderFields.ShipRegion.IsNull());

			var results = new TypedListDAO().FetchQuery(q2);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(498, count);
		}


		[Test]
		public void FetchOneSideOfJoinBetweenTwoSeparateQueries()
		{
			//LinqMetaData metaData = new LinqMetaData();

			//var cq = from c in metaData.Customer
			//         select new { c.CustomerId, c.CompanyName };

			//var oq = from o in metaData.Order
			//         select new { o.OrderId, o.CustomerId, o.EmployeeId, o.OrderDate };

			//var q = (from c in cq
			//         join o in oq on c.CustomerId equals o.CustomerId
			//         where o.EmployeeId == 3
			//         select c).Distinct();

			var qf = new QueryFactory();
			var cq = qf.Create("cq")
							.Select(CustomerFields.CustomerId, CustomerFields.CompanyName);
			var oq = qf.Create("oq")
							.Select(OrderFields.OrderId, OrderFields.CustomerId, OrderFields.EmployeeId, OrderFields.OrderDate);
			var q = qf.Create()
							.Select(cq.Field("CustomerId"), cq.Field("CompanyName"))
							.From(cq.InnerJoin(oq).On(cq.Field("CustomerId").Equal(oq.Field("CustomerId"))))
							.Where(oq.Field("EmployeeId").Equal(3))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(60, count);
		}


		[Test]
		public void FetchOneSideOfJoinBetweenTwoSeparateQueriesUsingCrossJoin()
		{
			//LinqMetaData metaData = new LinqMetaData();

			//var cq = from c in metaData.Customer
			//         select new { Cid= c.CustomerId, CName = c.CompanyName };

			//var oq = from o in metaData.Order
			//         select new { Oid = o.OrderId, OCId = o.CustomerId, OEid = o.EmployeeId, ODate = o.OrderDate };

			//var q = (from c in cq
			//         from o in oq 
			//         where c.Cid == o.OCId
			//         &&  o.OEid == 3
			//         select c).Distinct();

			var qf = new QueryFactory();
			var cq = qf.Create("cq")
							.Select(CustomerFields.CustomerId, CustomerFields.CompanyName);
			var oq = qf.Create("oq")
							.Select(OrderFields.OrderId, OrderFields.CustomerId, OrderFields.EmployeeId, OrderFields.OrderDate);
			var q = qf.Create()
							.Select(cq.Field("CustomerId"), cq.Field("CompanyName"))
							.From(cq.CrossJoin(oq))
							.Where((cq.Field("CustomerId").Equal(oq.Field("CustomerId"))).And(oq.Field("EmployeeId").Equal(3)))
							.Distinct();
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(60, count);
		}

		[Test]
		public void JoinBetweenTwoSeparateQueriesWithOneUsingCustomClassIntoNewAnonymousType()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in GetOrderData(metaData)
			//        join c in metaData.Customer on o.CustomerId equals c.CustomerId
			//        where c.Country == "USA"
			//        select o;

			var qf = new QueryFactory();
			var orderQ = GetOrderData(qf);
			var q = qf.Create()
							.SelectFrom(orderQ)
							.From(orderQ.InnerJoin(qf.Customer).On(orderQ.Field("CustomerId").Equal(CustomerFields.CustomerId)))
							.Where(CustomerFields.Country == "USA");
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(121, count);
		}


		/// <summary>
		/// Gets all customers from countries which have more than 5 customers
		/// </summary>
		[Test]
		public void JoinBetweenGroupedDataQueryAndCustomers()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join cd in GetCustomerCountsPerCountryData(metaData) on c.Country equals cd.Country
			//        where cd.NumberOfCustomers > 5
			//        select cd;

			var qf = new QueryFactory();
			var customerCountQ = GetCustomerCountsPerCountryData(qf);
			var q = qf.Create()
						.SelectFrom(customerCountQ)
						.From(qf.Customer.InnerJoin(customerCountQ).On(CustomerFields.Country.Equal(customerCountQ.Field("Country"))))
						.Where(customerCountQ.Field("NumberOfCustomers").GreaterThan(5));
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(50, count);
		}


		[Test]
		public void BooleanInsideFolderQueryRetrieval()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from c in metaData.Customer
			//         select new
			//         {
			//             c.CustomerId,
			//             HasOrders = (c.Orders.Count() > 0)
			//         }).Where(c => c.CustomerId.StartsWith("C"));

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							HasOrders = qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.CountRow()
											.GreaterThan(0).As("HasOrders")
											.ToValue<bool>()
						})
						.Where(CustomerFields.CustomerId.StartsWith("C"));
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.IsTrue(v.HasOrders);
			}
		}


		[Test]
		public void GroupJoinWithCount()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in metaData.Order on c.CustomerId equals o.CustomerId into co
			//        select new { c.CustomerId, Count = co.Count() };

			// QuerySpec doesn't support group join, as it's closer to SQL. 
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							Count = qf.Order.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId).CountRow().ToValue<int>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			List<string> noOrderCustomers = new List<string>() { "FISSA", "PARIS" };
			foreach(var v in results)
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


		[Test]
		public void GroupByOnJoinWithTwoAggregatesOnJoinedSet()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        join em in metaData.Employee on o.EmployeeId equals em.EmployeeId
			//        where o.OrderDate > new DateTime(1998, 1, 1)
			//        group o by o.CustomerId into g
			//        select new { g.Key, Count = g.Count(), Total = g.Sum(a => a.OrderId) };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							Key = OrderFields.CustomerId.ToValue<string>(),
							Count = Functions.CountRow().As("Count").ToValue<int>(),
							Total = OrderFields.OrderId.Sum().ToValue<int>()
						})
						.From(qf.Order.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
						.Where(OrderFields.OrderDate > new DateTime(1998, 1, 1))
						.GroupBy(OrderFields.CustomerId);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.Count > 0);
				Assert.IsTrue(v.Total > 10000);
			}
			Assert.AreEqual(80, count);
		}


		[Test]
		public void GroupByOnPropertyInFieldTypeAndRetrievalOfGroupByKey()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.CustomerId == "CHOPS"
			//        group o by o.OrderDate.Value.Date into g
			//        select new { Date = g.Key };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(OrderFields.OrderDate.Date().As("Date"))
						.Where(OrderFields.CustomerId == "CHOPS")
						.GroupBy(OrderFields.OrderDate.Date());
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(8, count);
		}


		[Test]
		public void GroupByOnNullableTypeAndFetchingOfGrouping3()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Order
			//                    .Where(o => (o.CustomerId == "CHOPS"))
			//                    .GroupBy(o => o.EmployeeId.Value)
			//                    .Where(g => g.Count() > 1);

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new Grouping<int, OrderEntity>(
											OrderFields.EmployeeId.Source("g").ToValue<int>(),
											qf.Order.TargetAs("o")
												.CorrelatedOver(OrderFields.EmployeeId.Source("o") == OrderFields.EmployeeId.Source("g")).ToResultset()))
						.From(qf.Create("g")
									.Select(OrderFields.EmployeeId, Functions.CountRow().As("Count"))
									.Where(OrderFields.CustomerId == "CHOPS")
									.GroupBy(OrderFields.EmployeeId))
						.Where(qf.Field("g", "Count").GreaterThan(1));
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.Count() > 0);
			}
			Assert.AreEqual(2, count);
		}

		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariableAndNormalWhereClause()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee
			//        where e.Country=="UK"
			//        group e by e.Phone into g
			//        select g;

			// this is a nested query in disguise, so we've to write it as such as QuerySpec is explicit.
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new Grouping<string, EmployeeEntity>(
								EmployeeFields.Phone.ToValue<string>(),
								qf.Employee.As("e")
										.CorrelatedOver(EmployeeFields.Phone.Source("e") == EmployeeFields.Phone).ToResultset()))
						.Where(EmployeeFields.Country == "UK")
						.GroupBy(EmployeeFields.Phone);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				Assert.IsTrue(v.Count() > 0);
				count++;
			}
			Assert.AreEqual(4, count);
		}

		[Test]
		public void GroupingOfEntitiesByGroupByKeyUsingGroupByVariableAndNormalWhereClauseAndCombinedGroupKey()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee
			//        where e.Country == "UK"
			//        group e by new { e.Phone, e.Country } into g
			//        select g;

			// as the previous query, but now we've to make sure two fields are equal in the nested query.

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new Grouping<string, EmployeeEntity>(
								EmployeeFields.Phone.ToValue<string>(),
								qf.Employee.TargetAs("e")
										.CorrelatedOver((EmployeeFields.Phone.Source("e") == EmployeeFields.Phone)
														.And(EmployeeFields.Country.Source("e") == EmployeeFields.Country))
										.ToResultset()))
						.Where(EmployeeFields.Country == "UK")
						.GroupBy(EmployeeFields.Phone, EmployeeFields.Country);
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				Assert.IsTrue(v.Count() > 0);
				count++;
			}
			Assert.AreEqual(4, count);
		}


		[Test]
		public void CustomProjectionThroughMethod()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.CustomerId == "CHOPS"
			//        select ConvertToTarget(o.OrderId, o.CustomerId, o.OrderDate);

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => ConvertToTarget(OrderFields.OrderId.ToValue<int>(), OrderFields.CustomerId.ToValue<string>(),
														OrderFields.OrderDate.ToValue<DateTime?>()))
						.Where(OrderFields.CustomerId == "CHOPS");
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				Assert.AreEqual("CHOPS", v.CustomerId);
				count++;
			}
			Assert.AreEqual(8, count);
		}


		[Test]
		public void GetListOfCustomerIdsWhichHaveOrdersFiledByEmployeesWithIdHigherThan4()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        join o in
			//            (from order in metaData.Order where order.EmployeeId > 4 select order)
			//        on c.CustomerId equals o.CustomerId into co
			//        from o in co.DefaultIfEmpty()
			//        select new { c.CustomerId };

			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => CustomerFields.CustomerId.ToValue<string>())
							.From(qf.Customer.LeftJoin(qf.Order.As("o").Where(OrderFields.EmployeeId > 4))
												.On(CustomerFields.CustomerId == OrderFields.CustomerId.Source("o")));
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(342, count);		// duplicates, but filters on order rows. It should return all customers. 
		}


		[Test]
		public void BooleanConstantInProjectionWithoutNameSpecification()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from p in metaData.Product
			//        select new Product2() { ProdId = p.ProductId, Name = p.ProductName, Discontinued = (p.ProductId < 0) };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							ProdId = ProductFields.ProductId.ToValue<int>(),
							Discontinued = (ProductFields.ProductId < 0).As("Discontinued").ToValue<bool>()
						});
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.IsFalse(v.Discontinued);
			}
		}

		[Test]
		public void ValueListInValueListProjection()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select new
			//        {
			//            c.CustomerId,
			//            OrderIds = c.Orders.Select(o => o.OrderId)
			//        };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							OrderIds = qf.Order
											.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
											.Select(() => OrderFields.OrderId.ToValue<int>())
											.ToResultset()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
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


		[Test]
		public void CountOnSingleEntitySet()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from c in metaData.Customer
			//        select c;
			//var count = q.Count(c => c.Country == "Germany");

			var qf = new QueryFactory();
			var q = qf.Customer
						.Where(CustomerFields.Country == "Germany")
						.Select(Functions.CountRow());
			var count = new TypedListDAO().GetScalar<int>(q);

			Assert.AreEqual(11, count);
		}


		[Test]
		public void SelectOnListWithCustomProjectionInWhere()
		{
			IList<DateTime> dateTimes = new List<DateTime>();
			dateTimes.Add(new DateTime(2008, 12, 1));
			dateTimes.Add(new DateTime(2009, 1, 1));

			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where dateTimes.Select(d => d.Year).Contains(o.EmployeeId.Value)
			//        select o;

			var qf = new QueryFactory();
			var q = qf.Order
						.Where(OrderFields.EmployeeId.In(dateTimes.Select(d => d.Year)));

			var results = new OrderCollection();
			results.GetMulti(q);
		}


		[Test]
		public void GetAllEmployeesWithoutPhotoNorNoteFields2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from e in metaData.Employee.ExcludeFields(x => x.Photo, x => x.Notes)
			//         group e by e.ReportsTo into g
			//         select g);

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new Grouping<int?, EmployeeEntity>(
								EmployeeFields.ReportsTo.ToValue<int?>(),
								qf.Employee.TargetAs("E")
									.CorrelatedOver(EmployeeFields.ReportsTo.Source("E") == EmployeeFields.ReportsTo)
									.Exclude(EmployeeFields.Photo, EmployeeFields.Notes)
									.ToResultset()))
						.GroupBy(EmployeeFields.ReportsTo);
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var g in results)
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

		[Test]
		public void IntersectUsage()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from c in metaData.Customer
			//         where c.CompanyName.StartsWith("B")
			//         select new { c.CustomerId, c.CompanyName };

			//var q2 = from c in metaData.Customer
			//         where c.CompanyName.EndsWith("S")
			//         select new { c.CustomerId, c.CompanyName };

			//var q3 = q1.Intersect(q2);

			var qf = new QueryFactory();
			var q1 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.StartsWith("B"));
			var q2 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.EndsWith("S"));

			var q3 = q1.Intersect(q2);

			var results = new TypedListDAO().FetchQuery(q3);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyName.StartsWith("B"));
				Assert.IsTrue(v.CompanyName.EndsWith("s"));
			}
			Assert.AreEqual(4, count);
		}


		[Test]
		public void ExceptUsage()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from c in metaData.Customer
			//         where c.CompanyName.StartsWith("B")
			//         select new { c.CustomerId, c.CompanyName };

			//var q2 = from c in metaData.Customer
			//         where c.CompanyName.EndsWith("S")
			//         select new { c.CustomerId, c.CompanyName };

			//var q3 = q1.Except(q2);

			var qf = new QueryFactory();
			var q1 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.StartsWith("B"));
			var q2 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.EndsWith("S"));

			var q3 = q1.Except(q2);

			var results = new TypedListDAO().FetchQuery(q3);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyName.StartsWith("B"));
				Assert.IsTrue(!v.CompanyName.EndsWith("s"));
			}
			Assert.AreEqual(3, count);
		}


		[Test]
		public void IntersectUsage2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from c in metaData.Customer
			//         where c.CompanyName.StartsWith("B")
			//         select new { c.CustomerId, c.CompanyName };

			//var q2 = from c in metaData.Customer
			//         where c.CompanyName.EndsWith("S")
			//         select new { c.CustomerId, c.CompanyName };

			//var q3 = q1.Intersect(q2);
			//var q4 = q3.Where(c => c.CompanyName.Length > 4);

			var qf = new QueryFactory();
			var q1 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.StartsWith("B"));
			var q2 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.EndsWith("S"));

			var q3 = q1.Intersect(q2);
			var q4 = q3.AndWhere(CustomerFields.CompanyName.Length().GreaterThan(4));

			var results = new TypedListDAO().FetchQuery(q4);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyName.StartsWith("B"));
				Assert.IsTrue(v.CompanyName.EndsWith("s"));
			}
			Assert.AreEqual(4, count);
		}


		[Test]
		public void ExceptUsage2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from c in metaData.Customer
			//         where c.CompanyName.StartsWith("B")
			//         select new { c.CustomerId, c.CompanyName };

			//var q2 = from c in metaData.Customer
			//         where c.CompanyName.EndsWith("S")
			//         select new { c.CustomerId, c.CompanyName };

			//var q3 = q1.Except(q2);
			//var q4 = q3.Where(c => c.CompanyName.Length > 4);

			var qf = new QueryFactory();
			var q1 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.StartsWith("B"));
			var q2 = qf.Create()
						.Select(() => new
						{
							CustomerId = CustomerFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>()
						})
						.Where(CustomerFields.CompanyName.EndsWith("S"));

			var q3 = q1.Except(q2);
			var q4 = q3.AndWhere(CustomerFields.CompanyName.Length().GreaterThan(4));

			var results = new TypedListDAO().FetchQuery(q4);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.IsTrue(v.CompanyName.StartsWith("B"));
				Assert.IsTrue(!v.CompanyName.EndsWith("s"));
			}
			Assert.AreEqual(3, count);
		}



		[Test]
		public void AnyOverWhereWithPkFilter()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//bool exists = metaData.Customer.Where(c=>c.CustomerId=="CHOPS").Any();
			//Assert.IsTrue(exists);
			//exists = metaData.Customer.Where(c => c.Country == "Germany").Any(c => c.CustomerId == "ALFKI");
			//Assert.IsTrue(exists);

			var qf = new QueryFactory();
			var q = qf.Create().Select(qf.Customer.Where(CustomerFields.CustomerId == "CHOPS").Any());
			var exists = new TypedListDAO().GetScalar<bool>(q);
			Assert.IsTrue(exists);

			var q2 = qf.Create().Select(qf.Customer.Where(CustomerFields.Country == "Germany").Any(CustomerFields.CustomerId == "ALFKI"));
			exists = new TypedListDAO().GetScalar<bool>(q2);
			Assert.IsTrue(exists);
		}


		[Test]
		public void OrderDatesAsUTCDateTimes()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where o.OrderDate != null
			//        select new { DBDate = o.OrderDate, UTCDate = o.OrderDate.Value.ToUniversalTime() };

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							DBDate = OrderFields.OrderDate.As("DBDate").ToValue<DateTime?>(),
							UTCDate = OrderFields.OrderDate.As("UTCDate").ToValue<DateTime>().ToUniversalTime()
						})
						.Where(OrderFields.OrderDate.IsNotNull());
			var results = new TypedListDAO().FetchQuery(q);
			foreach(var v in results)
			{
				Assert.IsNotNull(v.DBDate);
				Assert.AreEqual(v.DBDate.Value.ToUniversalTime(), v.UTCDate);
			}
		}

		[Test]
		public void GetAverageNumberOfProductsPerOrderUsingAverageAggregateAndMNRelationship()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var result = metaData.Order.Average(o => o.ProductCollectionViaOrderDetail.Count());

			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => qf.Order
										.Select(qf.Product
													.From(QueryTarget.InnerJoin(ProductEntity.Relations.OrderDetailEntityUsingProductId))
													.CorrelatedOver(OrderDetailFields.OrderId == OrderFields.OrderId)	// OrderDetails is joined into query
													.CountRow())
									.Avg().ToValue<int>());
			var result = new TypedListDAO().GetScalar<int>(q);
			Assert.AreEqual(2, result);
		}


		[Test]
		public void GroupByWithMaxTest()
		{
			string[] ids = new string[] { "CHOPS", "ALFKI" };
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        where ids.Contains(o.CustomerId)
			//        group o by o.CustomerId into g
			//        select new { CustomerId = g.Key, OrderId = g.Max(e => e.OrderId) };
			var qf = new QueryFactory();
			var q = qf.Create()
							.Select(() => new
							{
								CustomerId = OrderFields.CustomerId.ToValue<string>(),
								OrderId = OrderFields.OrderId.Max().ToValue<int>()
							})
							.Where(OrderFields.CustomerId.In(ids))
							.GroupBy(OrderFields.CustomerId);
			var results = new TypedListDAO().FetchQuery(q);

			foreach(var v in results)
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


		[Test]
		public void ClientSideDistinctWithNullTest()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from e in metaData.Employee
			//         join o in metaData.Order on e.EmployeeId equals o.EmployeeId
			//         where !o.ShippedDate.HasValue
			//         select new { e.EmployeeId, e.Notes, o.ShippedDate }).Distinct();
			var qf = new QueryFactory();
			var q = qf.Create()
						.Select(() => new
						{
							EmployeeId = EmployeeFields.EmployeeId.ToValue<int>(),
							Notes = EmployeeFields.Notes.ToValue<string>(),
							ShippedDate = OrderFields.ShippedDate.ToValue<DateTime?>()
						})
						.From(qf.Order.InnerJoin(OrderEntity.Relations.EmployeeEntityUsingEmployeeId))
						.Where(OrderFields.ShippedDate.IsNull())
						.Distinct();

			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(5, count);
		}


		[Test]
		public void TransactionKeepAliveTest()
		{
			using(var transaction = new Transaction(IsolationLevel.ReadCommitted, "test"))
			{
				//LinqMetaData metaData = new LinqMetaData();
				//var q = from c in metaData.Customer
				//        select new
				//        {
				//            CustomerId = c.CustomerId,
				//            OrderIds = (from o in metaData.Order
				//                        where o.CustomerId == c.CustomerId
				//                        select o.OrderId).ToList()
				//        };
				//foreach(var v in q)
				//{
				//}
				var qf = new QueryFactory();
				var q = qf.Create()
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<string>(),
								OrderIds = qf.Order
												.CorrelatedOver(OrderEntity.Relations.CustomerEntityUsingCustomerId)
												.Select(() => OrderFields.OrderId.ToValue<int>())
												.ToResultset()
							});
				var results = new TypedListDAO().FetchQuery(q, transaction);
				transaction.Commit();
			}
		}

		[Test]
		public void GetEntitySetUsingProjectionOnDirectEntityType()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from o in metaData.Order
			//        join c in metaData.Customer on o.CustomerId equals c.CustomerId
			//        where o.EmployeeId > 4
			//        orderby o.OrderDate ascending
			//        select new OrderDerived()
			//        {
			//            OrderId = o.OrderId,
			//            CustomerId = o.CustomerId,
			//            CompanyName = c.CompanyName,
			//            EmployeeId = o.EmployeeId
			//        };

			var qf = new QueryFactory();
			var q = qf.Order
						.From(QueryTarget.InnerJoin(OrderEntity.Relations.CustomerEntityUsingCustomerId))
						.Where(OrderFields.EmployeeId > 4)
						.OrderBy(OrderFields.OrderDate.Ascending())
						.Select(() => new OrderDerived
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							CompanyName = CustomerFields.CompanyName.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				Assert.IsTrue(v.EmployeeId > 4);
				Assert.IsFalse(string.IsNullOrEmpty(v.CompanyName));
				count++;
			}
			Assert.AreEqual(334, count);
		}


		[Test]
		public void TestWhetherThereAreDiscontinuedProducts2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var result = metaData.Product.Any(p => !p.Discontinued);
			var qf = new QueryFactory();
			var q = qf.Create().Select(qf.Product.Any(ProductFields.Discontinued.NotEqual(true)));
			var result = new TypedListDAO().GetScalar<bool>(q);
			Assert.IsTrue(result);
		}

		[Test]
		public void AllWithNegativePredicate()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var result = metaData.Product.All(p => !(p.CategoryId.Value == 10));
			var qf = new QueryFactory();
			var q = qf.Create().Select(qf.Product.All(Functions.Not(ProductFields.CategoryId == 10)));
			var result = new TypedListDAO().GetScalar<bool>(q);
			Assert.IsTrue(result);
		}

		[Test]
		public void ParentParentParenttest()
		{
			// Linq doesn't work: "Alias error. The same alias is assigned to emp.Employee as to emp.Employee.Employee."
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from emp in metaData.Employee
			//        where emp.EmployeeId == 6
			//        select new
			//        {
			//            id = emp.EmployeeId,
			//            id2 = emp.Employee.EmployeeId,
			//            id3 = emp.Employee.Employee.EmployeeId
			//        };
			//var v = q.First();

			// In QuerySpec, there's no ambiguity about members and types: you always explicitly join and always explicitly define aliases, if required.
			var qf = new QueryFactory();
			var q = qf.Employee
						.From(QueryTarget.LeftJoin(qf.Employee.TargetAs("m1")).On(EmployeeFields.ReportsTo == EmployeeFields.EmployeeId.Source("m1"))
										.LeftJoin(qf.Employee.TargetAs("m2")).On(EmployeeFields.ReportsTo.Source("m1") == EmployeeFields.EmployeeId.Source("m2")))
						.Where(EmployeeFields.EmployeeId == 6)
						.Select(() => new
						{
							id = EmployeeFields.EmployeeId.As("id1").ToValue<int>(),
							id2 = EmployeeFields.EmployeeId.As("id2").Source("m1").ToValue<int>(),
							id3 = EmployeeFields.EmployeeId.As("id3").Source("m2").ToValue<int>()
						});
			var v = new TypedListDAO().GetFirst(q);

			Assert.AreEqual(6, v.id);
			Assert.AreEqual(5, v.id2);
			Assert.AreEqual(4, v.id3);
		}
		

		[Test]
		public void GetOrdersInDataTableTest()
		{
			var qf = new QueryFactory();
			var q = qf.Order.As("o")
						.Where(OrderFields.CustomerId.StartsWith("C"))
						.OrderBy(OrderFields.CustomerId.Ascending())
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
						});
			var dataTable = new TypedListDAO().FetchAsDataTable(q);
			Assert.AreEqual(4, dataTable.Columns.Count);
			Assert.AreEqual(23, dataTable.Rows.Count);
			Assert.AreEqual(typeof(int), dataTable.Columns[0].DataType);
			Assert.AreEqual(typeof(string), dataTable.Columns[1].DataType);
			Assert.AreEqual(typeof(int), dataTable.Columns[2].DataType);
			Assert.AreEqual(typeof(DateTime), dataTable.Columns[3].DataType);
		}

		
		[Test]
		public void InternalsInspectionTest()
		{
			var qf = new QueryFactory();
			var q = qf.Order.As("o")
						.Where(OrderFields.CustomerId.StartsWith("C"))
						.OrderBy(OrderFields.CustomerId.Ascending())
						.Select(() => new
						{
							OrderId = OrderFields.OrderId.ToValue<int>(),
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							EmployeeId = OrderFields.EmployeeId.ToValue<int?>(),
							OrderDate = OrderFields.OrderDate.ToValue<DateTime?>()
						});

			// inspecting internals now is not useful. This is to be done when query is executed. 
			bool callbackCalled = false;
			q.OnPrepareForExecutionCallBack = (o) =>
					{
						// this code is called when the query is executed. 
						callbackCalled = true;
						IDynamicQuery query = o as IDynamicQuery;
						Assert.IsNotNull(query);
						Assert.AreEqual(4, query.Projection.Count);
						Assert.AreEqual(1, query.WhereClause.Count);
					};
			// execute the query, this will call the OnPrepareForExecutionCallback
			var dataTable = new TypedListDAO().FetchAsDataTable(q);
			Assert.AreEqual(4, dataTable.Columns.Count);
			Assert.AreEqual(23, dataTable.Rows.Count);
			Assert.IsTrue(callbackCalled);
		}


		#region Query producers, used in tests which join between queryables.
		//private IQueryable<OrderData> GetOrderData(LinqMetaData metaData)
		//{
		//    return from o in metaData.Order
		//           select new OrderData { CustomerId = o.CustomerId, OrderId = o.OrderId, EmployeeIdAsFloat = o.EmployeeId.Value };
		//}

		private DynamicQuery<OrderData> GetOrderData(QueryFactory qf)
		{
			return qf.Create("oq")
						.Select(() => new OrderData
						{
							CustomerId = OrderFields.CustomerId.ToValue<string>(),
							OrderId = OrderFields.OrderId.ToValue<int>(),
							EmployeeIdAsFloat = OrderFields.EmployeeId.ToValue<int>()
						});
		}


		//private IQueryable<CustomerCountsPerCountry> GetCustomerCountsPerCountryData(LinqMetaData metaData)
		//{
		//    return from c in metaData.Customer
		//           group c by c.Country into g
		//           select new CustomerCountsPerCountry { Country = g.Key, NumberOfCustomers = g.Count() };
		//}

		private DynamicQuery<CustomerCountsPerCountry> GetCustomerCountsPerCountryData(QueryFactory qf)
		{
			return qf.Create("gcc")
						.Select(() => new CustomerCountsPerCountry
						{
							Country = CustomerFields.Country.ToValue<string>(),
							NumberOfCustomers = Functions.CountRow().As("NumberOfCustomers").ToValue<int>()
						})
						.GroupBy(CustomerFields.Country);
		}

		#endregion

		#region helper classes and methods for custom projections
		private TargetObject ConvertToTarget(int orderId, string customerId, DateTime? orderDate)
		{
			return new TargetObject() { OrderId = orderId, CustomerId = customerId, OrderDate = orderDate };
		}
		#endregion
	}
}
