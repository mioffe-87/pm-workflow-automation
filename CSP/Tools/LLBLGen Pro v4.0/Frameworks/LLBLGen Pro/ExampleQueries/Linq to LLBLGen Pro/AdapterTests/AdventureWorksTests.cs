using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using AdventureWorks.Dal.Adapter.DatabaseSpecific;
using AdventureWorks.Dal.Adapter.EntityClasses;
using AdventureWorks.Dal.Adapter.HelperClasses;
using AdventureWorks.Dal.Adapter.Linq;

using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.Tools.OrmProfiler.Interceptor;


namespace LinqTester.AdapterTests
{
	[TestFixture]
	public class AdventureWorksTests
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}


		[Test]
		public void GroupByWithHopTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.SalesOrderDetail
							.Where(srr => srr.SalesOrderHeader.CustomerId == 676
									&& srr.SalesOrderHeader.ShipDate != null)
							.GroupBy(srr => new { srr.SalesOrderId, srr.SalesOrderHeader.ShipMethodId, srr.SalesOrderHeader.ShipDate })
							.Select(x => new
							{
								Date = x.Key.ShipDate,
								ShipMethodId = x.Key.ShipMethodId,
								Value = x.Average(z => (decimal)z.OrderQuantity)
							});
				foreach(var v in q)
				{

				}
			}
		}



		[Test]
		public void AnyOverWhere()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				bool exists = metaData.Customer.Where(c => c.CustomerId == 15758).Any();
				Assert.IsTrue(exists);
			}
		}


		[Test]
		public void FetchEntityWithRenamedPkField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from a in metaData.Address
						where a.Id == 3
						select a;

				if(q.Count() > 1)
				{
					Console.WriteLine("error");
				}
			}
		}


		[Test]
		public void NullExceptionWithMissingOrderByTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from soh in metaData.SalesOrderHeader
				         where soh.SalesOrderId < 43690
				         from sod in soh.SalesOrderDetailCollection
				         select new {
				                    	soh.SalesOrderId,
				                    	metaData.Product.First(p => p.ProductId == sod.ProductId).Name,
										sod.SalesOrderDetailId,
						 });

				foreach(var v in q)
				{
					
				}
			}
		}
        

		/// <summary>
		/// Test should produce data obtained with 2 left joins, where 1 is a leftjoin forced by the left join of the relation it is joined to (instead of an
		/// inner join)
		/// </summary>
		[Test]
		public void GetDynamicListFromDataWithImplicitWeakRelationFollow()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);

				var q = from p in metaData.Product
						orderby p.ProductId ascending
						select new
						{
							p.ProductId,
							p.Name,
							CategoryName = p.ProductSubcategory.ProductCategory.Name
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}

				Assert.AreEqual(504, count);
			}
		}

		
		[Test]
		public void GetContactFromOrderUsingMultipleCalls()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				try
				{
					adapter.StartTransaction(System.Data.IsolationLevel.Snapshot, "Test");

					var q = (from o in metaData.SalesOrderHeader
							 where o.SalesOrderId == 43659
							 select o.Contact).FirstOrDefault();

					Assert.IsNotNull(q);

					q = (from o in metaData.SalesOrderHeader
						 where o.SalesOrderId == 43659
						 select o.Contact).FirstOrDefault();

					Assert.IsNotNull(q);
					Assert.AreEqual("Hendergart", q.LastName);
				}
				finally
				{
					adapter.Commit();
				}
			}
		}


		[Test]
		public void MultiHopRelationFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Individual
						from customerAddress in customer.CustomerAddressCollection
						select new
						{
							customerAddress.Address.AddressLine1,
							customerAddress.Address.AddressLine2,
							customerAddress.Address.City,
							AddressType = customerAddress.AddressType.Name,
							customer.Contact.Title,
							customer.Contact.FirstName,
							customer.Contact.MiddleName,
							customer.Contact.LastName,
							customer.Contact.Suffix,
							customer.Contact.EmailAddress,
							customer.Contact.EmailPromotion,
							CountryRegionName = customerAddress.Address.StateProvince.CountryRegion.Name,
							StateProvinceName = customerAddress.Address.StateProvince.Name,
							customer.CustomerId
						};
				q= q.Take(14);
				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(14, count);
			}
		}

		[Test]
		public void MultiHopWithSelectManyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from soh in metaData.SalesOrderHeader
						from customerAddress in soh.Customer.CustomerAddressCollection
						where customerAddress.Address.PostalCode == "98011"
						select soh;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(18, count);
			}
		}

		[Test]
		public void DefaultIfEmptyOnRelatedEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						 from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
						 where soh.SalesOrderId == null
						 select customer;
				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(66, count);
			}
		}


		[Test]
		public void DefaultIfEmptyOnRelatedEntityWithMultipleHops()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						from ca in customer.CustomerAddressCollection
						from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
						where ca.AddressId != null
						where soh.SalesOrderId == null
						select customer;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(66, count);
			}
		}


		[Test]
		public void DefaultIfEmptyOnRelatedEntityWithMultipleHops2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
						from ca in customer.CustomerAddressCollection
						where soh.SalesOrderId == null
						where ca.AddressId != null
						select customer;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(66, count);
			}
		}


		[Test]
		public void DefaultIfEmptyOnRelatedEntityWithMultipleHops3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						from ca in customer.CustomerAddressCollection
						from soh in customer.SalesOrderHeaderCollection
						where ca.AddressId != null
						where soh.SalesOrderId == null
						select customer;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(0, count);
			}
		}


		/// <summary>
		/// Tests if a misplaced where still leads to a proper query.
		/// </summary>
		[Test]
		public void DefaultIfEmptyOnRelatedEntityWithMultipleHops4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						from ca in customer.CustomerAddressCollection
						where ca.AddressId != null
						from soh in customer.SalesOrderHeaderCollection
						where soh.SalesOrderId == null
						select customer;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(0, count);
			}
		}


		[Test]
		public void MultipleDynamicWhereClauses()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string firstName = "a";
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.SalesOrderHeader.AsQueryable();
				q = q.Where(soh => (soh.Customer as IndividualEntity).Contact.FirstName.Contains(firstName));
				q = q.Where(soh => soh.Customer.CustomerAddressCollection.Any(ca => ca.Address.StateProvince.Name == "Arizona"));

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void WhereInNestedFromCombinedWithDefaultIfEmpty()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from customer in metaData.Customer
						where customer.CustomerId < 100
						from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
						select new { customer.CustomerId, soh.SalesOrderId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(586, count);
			}
		}


		[Test]
		public void CustomProjectionIntoAnonymousTypeOnSelectManyList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from soh in metaData.SalesOrderHeader
						from sod in soh.SalesOrderDetailCollection
						select soh;
				var q = (from soh in q1
						select new { soh.SalesOrderId }).Distinct();

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.IsTrue(v.SalesOrderId > 0);
				}
				Assert.AreEqual(31465, count);
			}
		}


		[Test]
		public void CustomProjectionIntoAnonymousTypeUsingMultipleQueries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from customer in metaData.Customer
						 where customer.CustomerId<100
						 from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
						 select new { customer.CustomerId, soh.SalesOrderId };
				var z = q1.ToList();

				int count = 0;
				foreach(var v in z)
				{
					count++;
				}
				Assert.AreEqual(586, count);
			}
		}

		[Test]
		public void CustomProjectionIntoAnonymousTypeUsingDefaultIfEmptyOnNestedFromsWithEmbeddedWhere()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = metaData.Customer.AsQueryable();
				var q = from c in customers
						from soh in c.SalesOrderHeaderCollection.Where(soh => soh.SalesOrderId < 10).DefaultIfEmpty()
						select new { c.CustomerId, soh.SalesOrderId };

				foreach(var v in q)
				{
				}
			}
		}


		/// <summary>
		/// Just for query construction tests that the queries don't crash
		/// </summary>
		[Test]
		public void MiscellaneousTests3()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from soh in metaData.SalesOrderHeader
						from sod in soh.SalesOrderDetailCollection
						select soh;

				var x = (from soh in q
						 select new { soh.SalesOrderId, soh.Customer.AccountNumber }).ToList();

			}
		}


		/// <summary>
		/// Just for query construction tests that the queries don't crash
		/// </summary>
		[Test]
		public void MiscellaneousTests4()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from soh in metaData.SalesOrderHeader
						from sod in soh.SalesOrderDetailCollection
						select soh;

				var x = (from soh in q
						 select new { soh.SalesOrderId, soh.Customer.AccountNumber, soh.CreditCard.CardNumber }).ToList();

			}
		}

		[Test]
		public void MiscellaneousTests5()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var sohquery = from soh in metaData.SalesOrderHeader select soh;

				var w = (from soh in sohquery
						 from sod in soh.SalesOrderDetailCollection
						 select new { soh.SalesOrderId, soh.Customer.AccountNumber, soh.CreditCard.CardNumber }).ToList();

			}
		}

		[Test]
		public void MiscellaneousTests6()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = metaData.Customer.AsQueryable();
				var customersDerivedTable = from customer in customers select customer; //Using this to force a derived table causes a crash
				customersDerivedTable = customers.Where(c => c.CustomerId > 10); //To force a derived table

				var q = from customer in customersDerivedTable
						join soh in
							(from s in metaData.SalesOrderHeader where s.SalesPersonId > 22 select s) on customer.CustomerId equals soh.CustomerId into oc
						from nullableSOH in oc.DefaultIfEmpty()

						from ca in customer.CustomerAddressCollection.DefaultIfEmpty()
						select new { customer.CustomerId, ca.AddressId, nullableSOH.SalesOrderId, customer.SalesTerritory.Name };
				q = q.Take(10);

				var l = q.ToList();
			}
		}

		[Test]
		public void MiscellaneousTests7()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = metaData.Customer.AsQueryable();
				var q = from customer in customers
						join soh in
							(from s in metaData.SalesOrderHeader where s.SalesPersonId > 22 select s) on customer.CustomerId equals soh.CustomerId into join1
						join ca in 
							(from custaddr in metaData.CustomerAddress where custaddr.AddressId > 100 select custaddr) 
								on customer.CustomerId equals ca.CustomerId into join2
						from xca in join2.DefaultIfEmpty()
						from xsoh in join1.DefaultIfEmpty()
						select new { customer.CustomerId, xca.AddressId, xsoh.SalesOrderId, customer.SalesTerritory.Name };
				q = q.Take(10);

				var l = q.ToList();
			}
		}


		[Test]
		public void MiscellaneousTests8()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = metaData.Customer.AsQueryable();
				var q = from customer in customers
						join ca in
							(from custaddr in metaData.CustomerAddress where custaddr.AddressId > 100 select custaddr)
								on customer.CustomerId equals ca.CustomerId into join2
						from xca in join2.DefaultIfEmpty()
						join soh in
							(from s in metaData.SalesOrderHeader where s.SalesPersonId > 22 select s) on customer.CustomerId equals soh.CustomerId into join1
						from xsoh in join1.DefaultIfEmpty()
						select new { customer.CustomerId, xca.AddressId, xsoh.SalesOrderId, customer.SalesTerritory.Name };
				q = q.Take(10);

				var l = q.ToList();
			}
		}


		[Test]
		public void MiscellaneousTests9()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var customers = metaData.Customer.AsQueryable().Select(c=>c);
				customers = customers.Where(c => c.CustomerId > 10);
				var q = from c in customers
						from soh in c.SalesOrderHeaderCollection
						select new
						{
							Scalar = (from a in metaData.Address
									  from ca in a.CustomerAddressCollection
									  where ca.CustomerId == c.CustomerId
									  select a).Count()
						};

				foreach(var v in q)
				{
				}

			}
		}


		[Test]
		public void MiscellaneousTests10()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from ca in metaData.CustomerAddress
						where ca.Address.PostalCode == "98011"
						group ca by ca.Address.StateProvince.CountryRegion.Name into g
						select new { g.Key, Amount = g.Count() };

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests11()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from Address in metaData.Address
						from EmployeeAddress in Address.EmployeeAddressCollection
						from Individual in EmployeeAddress.Employee.Contact.IndividualCollection
						select Address;

				foreach(var v in q)
				{ 
				}
			}
		}


		[Test]
		public void MiscellaneousTests12()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from Address in metaData.Address
						from EmployeeAddress in Address.EmployeeAddressCollection
						select EmployeeAddress.Employee.Contact;

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests13()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.CustomerAddress.Select(ca => ca.Address.StateProvince.CountryRegion);

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests17()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from soh in metaData.SalesOrderHeader
						from sod in soh.SalesOrderDetailCollection
						from sod2 in soh.SalesOrderDetailCollection
						where sod.SalesOrderId > 10 && sod2.SalesOrderId > 1
						select soh;

				var w = (from soh in q
						 from sod3 in soh.SalesOrderDetailCollection
						 select soh).ToList(); //crash
			}
		}


		[Test]
		public void MiscellaneousTests18()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var c = from Address in metaData.Address
					from EmployeeAddress in Address.EmployeeAddressCollection
					select Address;

				var q = from Address in c
						from EmployeeAddress in Address.EmployeeAddressCollection
						from SalesOrderHeader in Address.SalesOrderHeaderCollection.DefaultIfEmpty()
						select Address;

				foreach(var v in q)
				{
				}
			}
		}


		[Test]
		public void PrefetchPathTestUsingInheritanceAndSubqueries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				adapter.ParameterisedPrefetchPathThreshold = 1;
				var q = (from s in metaData.SalesPerson
						 select s).WithPath(
							new PathEdge<SalesTerritoryEntity>(SalesPersonEntity.PrefetchPathSalesTerritory)
											);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					if(new[] { 268, 284, 288 }.Contains(v.EmployeeId))
					{
						Assert.IsNull(v.SalesTerritory);
					}
					else
					{
						Assert.IsNotNull(v.SalesTerritory);
					}
				}
				Assert.AreEqual(17, count);
			}
		}


		[Test]
		public void DoubleAndDecimalMathCalculationsWithLet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new AdventureWorksFunctionMappings());
				const double EarthRadius = 3963.1;
				var q = from address in metaData.CreativeAddress
						let lat1 = address.LatitudeF.Value * Math.PI / 180
						let lon1 = address.LongitudeF.Value * Math.PI / 180
						let x2 = AdventureWorksFunctions.Sin(lat1) *
									AdventureWorksFunctions.Cos(lat1)
						let d = EarthRadius * (-1 * AdventureWorksFunctions.Atan(x2 / AdventureWorksFunctions.Sqrt(1 - x2 * x2)) + Math.PI / 2)
						where address.Latitude.HasValue && address.Longitude.HasValue
						select d;

				foreach(var d in q)
				{
					Assert.AreEqual(5524.3839485018443, d);
				}
			}
		}


		[Test]
		public void ContainsWithMultipleValuePairsAndWrappedQueriesTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = metaData.WorkOrderRouting.AsQueryable();

				var lst = new List<Pair<int, int>>();
				lst.Add(new Pair<int, int> { Value1 = 1, Value2 = 722 });
				lst.Add(new Pair<int, int> { Value1 = 2, Value2 = 725 });
				lst.Add(new Pair<int, int> { Value1 = 3, Value2 = 726 });

				q1 = q1.Where(
				 p => lst.Contains(
						 new Pair<int, int>
						 {
							 Value1 = p.WorkOrder.WorkOrderId,
							 Value2 = p.WorkOrder.ProductId
						 }
						 )
				 );


				var q = q1.Select(
							 p => new
							 {
								 p.ProductId,
								 p.WorkOrder.WorkOrderId
							 }).ToList();

				foreach(var v in q)
				{
					
				}
			}
		}



		[Test]
		public void ProjectionToCustomClassUsingNullableValueThroughScalarQueryTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from p in metaData.WorkOrder
						 select new BusinessObject()
						 {
							 ProductId = p.ProductId,
							 ModifiedDate = p.WorkOrderRoutingCollection.Max(x => x.ModifiedDate)

						 });

				int count = 0;
				foreach(var v in q)
				{
					if(!v.ModifiedDate.HasValue)
					{
						count++;
					}
				}

				Assert.IsTrue(count>0);
			}
		}

        
		[Test]
		public void FetchingSecondPageUsingOutOfOrderSkipTake()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Contact.Take(10).Skip(10);
				int count = 0;

				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(10, count);
			}
		}


		[Test]
		public void CompareToOnGuidTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter, new AdventureWorksGuidFunctionMappings());
				var q = (from a in metaData.Address select a)
						.Where(a => a.Rowguid.CompareTo(new Guid("57f73b00-26b1-11df-8600-001c2311de1b")) < 0);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(6, count);
			}
		}

        
		[Test]
		public void MultipleTimesSameJoinSameAliasProblem()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var pQuery = metaData.Product.AsQueryable();
				var sQuery = metaData.ProductSubcategory.AsQueryable();
				pQuery = pQuery.Where(x => x.ProductId != 0); //example code
				var q1 = from p in pQuery
						 join sc in sQuery on p.ProductSubcategoryId equals sc.ProductSubcategoryId
						 select p;

				var pList = (from sc in metaData.ProductSubcategory
							 join p in q1 on sc.ProductSubcategoryId equals p.ProductSubcategoryId
							 select new
							 {
								 Value1 = p.ProductId,
								 Value2 = sc.ProductCategoryId,
							 }).ToList();

				foreach(var v in q1)
				{

				}
			}
		}
	}


	internal class AdventureWorksGuidFunctionMappings : FunctionMappingStore
	{
		public AdventureWorksGuidFunctionMappings()
			: base()
		{
			this.Add(new FunctionMapping(typeof(Guid), "CompareTo", 1, "CASE WHEN {0} < {1} THEN -1 WHEN {0} = {1} THEN 0 ELSE 1 END"));
		}
	}


	internal class BusinessObject
	{
		public int ProductId { get; set; }
		public DateTime? ModifiedDate { get; set; }
	}
}
