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
using SD.LLBLGen.Pro.QuerySpec;
using AdventureWorks.Dal.Adapter.FactoryClasses;
using System.Diagnostics;
using SD.LLBLGen.Pro.QuerySpec.Adapter;


namespace LinqTester.AdapterTests
{
	public class ProductDTO
	{
		public string Name{ get; set; }
		public int[] BillOfMaterialIDs { get; set; }
		public int[] WorkOrderIDs { get; set; }
	}


	[TestFixture]
	public class AdventureWorksTests
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}


		[Test]
		public void TwoNestedSetFetches()
		{
			using(var adapter = new DataAccessAdapter())
			{
				var qf = new QueryFactory();

				// Product == MainTable
				// BillOfMaterial == AttributeOne
				// WorkOrder == AttributeTwo

				var join = qf.Product.InnerJoin(qf.BillOfMaterials).On(ProductFields.ProductId == BillOfMaterialsFields.ComponentId);
				join = join.InnerJoin(qf.WorkOrder).On(ProductFields.ProductId== WorkOrderFields.ProductId);


				//var q = qf.Create().Select(() => new ProductDTO()
				var q = qf.Product.Select(()=>new ProductDTO()
				{
					Name = ProductFields.Name.ToValue<string>(),

					BillOfMaterialIDs = qf.BillOfMaterials.As("a1")
										.CorrelatedOver(BillOfMaterialsEntity.Relations.ProductEntityUsingComponentId)
										.Select(() => BillOfMaterialsFields.BillOfMaterialsId.ToValue<int>()).ToResultset().ToArray(),

					WorkOrderIDs = qf.WorkOrder.As("a2")
										.CorrelatedOver(WorkOrderEntity.Relations.ProductEntityUsingProductId)
										.Select(() => WorkOrderFields.WorkOrderId.ToValue<int>()).ToResultset().ToArray(),

				//}).From(join).Distinct();
				}).Distinct();
				var data = adapter.FetchQuery(q);
			}
		}


		[Test]
		public void AnyOverWhere()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//bool exists = metaData.Customer.Where(c => c.CustomerId == 15758).Any();
				var qf = new QueryFactory();
				var q = qf.Create().Select(qf.Customer.Where(CustomerFields.CustomerId == 15758).Any());
				// alternative
				// var q = qf.Create().Select(Functions.IIF(qf.Customer.Where(CustomerFields.CustomerId == 15758).Any(), true, false));
				var exists = adapter.FetchScalar<bool>(q);
				Assert.IsTrue(exists);
			}
		}


		[Test]
		public void AnyOverWhere2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//bool exists = metaData.Individual.Where(c => c.CustomerId == 15758).Any();
				var qf = new QueryFactory();
				var q = qf.Create().Select(qf.Individual.Where(IndividualFields.CustomerId == 15758).Any());
				// alternative
				//var q = qf.Create().Select(Functions.IIF(qf.Individual.Where(IndividualFields.CustomerId == 15758).Any(), true, false));

				var exists = adapter.FetchScalar<bool>(q);
				Assert.IsTrue(exists);
			}
		}
				
		[Test]
		public void FetchEntityWithRenamedPkField()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from a in metaData.Address
				//        where a.Id == 3
				//        select a;

				var qf = new QueryFactory();
				var q = qf.Address.Where(AddressFields.Id == 3);

				var addresses = adapter.FetchQuery(q);
				Assert.AreEqual(1, addresses.Count);
			}
		}


		[Test]
		public void NullExceptionWithMissingOrderByTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from soh in metaData.SalesOrderHeader
				//         where soh.SalesOrderId < 43690
				//         from sod in soh.SalesOrderDetailCollection
				//         //orderby soh.SalesOrderId
				//         select new {
				//                        soh.SalesOrderId,
				//                        metaData.Product.First(p => p.ProductId == sod.ProductId).Name,
				//                        sod.SalesOrderDetailId,
				//         });

				var qf = new QueryFactory();
				var q = qf.SalesOrderHeader
								.From(QueryTarget.InnerJoin(SalesOrderHeaderEntity.Relations.SalesOrderDetailEntityUsingSalesOrderId))
								.Where(SalesOrderHeaderFields.SalesOrderId < 43690)
								.Select(SalesOrderHeaderFields.SalesOrderId,
										qf.Product.Where(ProductFields.ProductId == SalesOrderDetailFields.ProductId)
										          .Select(ProductFields.Name).ToScalar(true).As("Name"),
										SalesOrderDetailFields.SalesOrderDetailId);
				var rawResults = adapter.FetchQuery(q);

				foreach(var v in rawResults)
				{
					
				}
			}
		}


		[Test]
		public void GetDynamicListWithNestedHierarchyInJoinTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// Linq variant crashes. 
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from reason in metaData.Reason
				//        join time in
				//            (
				//                from timeHeader in
				//                    (
				//                        from timeHeader in
				//                            (
				//                                from timeHeader in metaData.NonPresentTimeHeader.Where(header => header.Id == 6)
				//                                join userDetail in metaData.UserDetail on timeHeader.UserId equals userDetail.Id into userJoin
				//                                from user in userJoin.DefaultIfEmpty()
				//                                select timeHeader
				//                            )
				//                        join userDetail in metaData.UserDetail on timeHeader.ApprovedFromId equals userDetail.Id into approverJoin
				//                        from approver in approverJoin.DefaultIfEmpty()
				//                        select timeHeader
				//                    )
				//                join time in metaData.NonPresentTime on timeHeader.Id equals time.HeaderId into timeJoin
				//                from joinedTime in timeJoin.DefaultIfEmpty()
				//                select joinedTime
				//            ) on reason.Id equals time.ReasonId into reasonJoin
				//        from joinedReason in reasonJoin.DefaultIfEmpty()
				//        select new
				//        {
				//            ReasonID = reason.Id,
				//            Reason = reason.Reason,
				//            HeaderID = joinedReason.HeaderId ?? -1,
				//            TimeID = joinedReason.Id,
				//            Notes = joinedReason.Notes,
				//            DateStart = joinedReason.DateStart,
				//            DateEnd = joinedReason.DateEnd
				//        };

				var qf = new QueryFactory();
				var q = qf.Reason
							.From(QueryTarget.LeftJoin(qf.NonPresentTime.As("NPT")
															.From(QueryTarget.LeftJoin(qf.NonPresentTimeHeader)
																				.On(NonPresentTimeHeaderFields.Id == NonPresentTimeFields.HeaderId)
																			 .LeftJoin(qf.UserDetail).On(NonPresentTimeHeaderFields.UserId == UserDetailFields.Id)
																			 .LeftJoin(qf.UserDetail.TargetAs("Approver"))
																				.On(NonPresentTimeHeaderFields.ApprovedFromId == UserDetailFields.Id.Source("Approver")))
															.Where(NonPresentTimeFields.Id == 6))
												.On(ReasonFields.Id == NonPresentTimeFields.ReasonId.Source("NPT")))
							.Select(ReasonFields.Id.As("ReasonID"),
									ReasonFields.Reason,
									Functions.Coalesce(NonPresentTimeFields.HeaderId.Source("NPT"), -1).As("HeaderId"),
									NonPresentTimeFields.Id.Source("NPT").As("TimeID"),
									NonPresentTimeFields.Notes.Source("NPT"),
									NonPresentTimeFields.DateStart.Source("NPT"),
									NonPresentTimeFields.DateEnd.Source("NPT"));

				var results = adapter.FetchQuery(q);

				foreach(var v in results)
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from p in metaData.Product
				//        orderby p.ProductId ascending
				//        select new
				//        {
				//            p.ProductId,
				//            p.Name,
				//            CategoryName = p.ProductSubcategory.ProductCategory.Name
				//        };

				var qf = new QueryFactory();
				// uses a lambda to create a typed resultset. 
				var q = qf.Create()
							.Select(() => new
							{
								ProductId = ProductFields.ProductId.ToValue<int>(),
								Name = ProductFields.Name.ToValue<string>(),
								CategoryName = ProductCategoryFields.Name.As("CategoryName").ToValue<string>()
							})
							.From(qf.Product.LeftJoin(ProductEntity.Relations.ProductSubcategoryEntityUsingProductSubcategoryId)
											 .LeftJoin(ProductSubcategoryEntity.Relations.ProductCategoryEntityUsingProductCategoryId))
							.OrderBy(ProductFields.ProductId.Ascending());

				// alternative using untyped resultset
				//var q = qf.Create()
				//            .Select(ProductFields.ProductId, ProductFields.Name, ProductCategoryFields.Name.As("CategoryName"))
				//            .From(qf.Product.LeftJoin(ProductEntity.Relations.ProductSubcategoryEntityUsingProductSubcategoryId)
				//                             .LeftJoin(ProductSubcategoryEntity.Relations.ProductCategoryEntityUsingProductCategoryId))
				//            .OrderBy(ProductFields.ProductId.Ascending());

				//Alternative with typed projection illustrating numeric indexing, named indexing and type conversion using Get with name indexing
				//var q = qf.Create()
				//            .Select(ProductFields.ProductId,
				//                    ProductFields.Name,
				//                    ProductCategoryFields.Name.As("CategoryName"))
				//            .From(qf.Product.LeftJoin(ProductEntity.Relations.ProductSubcategoryEntityUsingProductSubcategoryId)
				//                             .LeftJoin(ProductSubcategoryEntity.Relations.ProductCategoryEntityUsingProductCategoryId))
				//            .OrderBy(ProductFields.ProductId.Ascending())
				//            .WithProjector(p => new { ProductId = (int)p[0], Name = (string)p["Name"], CategoryName = p.Get<string>("CategoryName") });

				var results = adapter.FetchQuery(q);
				// alternative projections / fetch method: Here, results will be a List<object[]> instead, ignoring the projector.
				//var results = adapter.FetchQueryRaw(q);

				int count = 0;
				foreach(var v in results)
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
				try
				{
					adapter.StartTransaction(System.Data.IsolationLevel.Snapshot, "Test");

					//LinqMetaData metaData = new LinqMetaData(adapter);
					//var q = (from o in metaData.SalesOrderHeader
					//         where o.SalesOrderId == 43659
					//         select o.Contact).FirstOrDefault();
					var qf = new QueryFactory();
					var q = qf.Contact
								.From(QueryTarget.InnerJoin(ContactEntity.Relations.SalesOrderHeaderEntityUsingContactId))
								.Where(SalesOrderHeaderFields.SalesOrderId == 43659);
					var e = adapter.FetchFirst(q);

					Assert.IsNotNull(e);

					q = qf.Contact
								.From(QueryTarget.InnerJoin(ContactEntity.Relations.SalesOrderHeaderEntityUsingContactId))
								.Where(SalesOrderHeaderFields.SalesOrderId == 43659);
					e = adapter.FetchFirst(q);

					Assert.AreEqual("Hendergart", e.LastName);
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from customer in metaData.Individual
				//        from customerAddress in customer.CustomerAddressCollection
				//        select new
				//        {
				//            customerAddress.Address.AddressLine1,
				//            customerAddress.Address.AddressLine2,
				//            customerAddress.Address.City,
				//            AddressType = customerAddress.AddressType.Name,
				//            customer.Contact.Title,
				//            customer.Contact.FirstName,
				//            customer.Contact.MiddleName,
				//            customer.Contact.LastName,
				//            customer.Contact.Suffix,
				//            customer.Contact.EmailAddress,
				//            customer.Contact.EmailPromotion,
				//            CountryRegionName = customerAddress.Address.StateProvince.CountryRegion.Name,
				//            StateProvinceName = customerAddress.Address.StateProvince.Name,
				//            customer.CustomerId
				//        };
				//q = q.Take(14);

				var qf = new QueryFactory();
				// QuerySpec doesn't relationship hopping, you have to specify all joins manually. 
				var q = qf.Create()
								.Select(AddressFields.AddressLine1, 
										AddressFields.AddressLine2,
										AddressFields.City,
										AddressTypeFields.Name.As("AddressType"),
										ContactFields.Title,
										ContactFields.FirstName,
										ContactFields.MiddleName,
										ContactFields.LastName,
										ContactFields.Suffix,
										ContactFields.EmailAddress,
										ContactFields.EmailPromotion,
										CountryRegionFields.Name.As("CountryRegionName"),
										StateProvinceFields.Name.As("StateProvinceName"),
										IndividualFields.CustomerId)
								.From(qf.Individual.InnerJoin(IndividualEntity.Relations.CustomerAddressEntityUsingCustomerId)
													.InnerJoin(CustomerAddressEntity.Relations.AddressEntityUsingAddressId)
													.InnerJoin(CustomerAddressEntity.Relations.AddressTypeEntityUsingAddressTypeId)
													.InnerJoin(IndividualEntity.Relations.ContactEntityUsingContactId)
													.InnerJoin(AddressEntity.Relations.StateProvinceEntityUsingStateProvinceId)
													.InnerJoin(StateProvinceEntity.Relations.CountryRegionEntityUsingCountryRegionCode))
								.Limit(14);
				// just fetch the raw list, we don't need a projection to a typed element
				var results = adapter.FetchQuery(q);	
				int count = 0;
				foreach(var v in results)
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from soh in metaData.SalesOrderHeader
				//        from customerAddress in soh.Customer.CustomerAddressCollection
				//        where customerAddress.Address.PostalCode == "98011"
				//        select soh;

				var qf = new QueryFactory();
				var q = qf.SalesOrderHeader
							.From(QueryTarget.InnerJoin(SalesOrderHeaderEntity.Relations.CustomerEntityUsingCustomerId)
											.InnerJoin(CustomerEntity.Relations.CustomerAddressEntityUsingCustomerId)
											.InnerJoin(CustomerAddressEntity.Relations.AddressEntityUsingAddressId))
							.Where(AddressFields.PostalCode == "98011");
				var results = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in results)
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from customer in metaData.Customer
				//         from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
				//         where soh.SalesOrderId == null
				//         select customer;

				// QuerySpec is simpler, it forces you to specify the relationships explicitly
				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget.LeftJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
								.Where(SalesOrderHeaderFields.SalesOrderId.IsNull());
				var results = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in results)
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from customer in metaData.Customer
				//        from ca in customer.CustomerAddressCollection
				//        from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
				//        where ca.AddressId != null
				//        where soh.SalesOrderId == null
				//        select customer;

				var qf = new QueryFactory();
				var q = qf.Customer
								.From(QueryTarget.InnerJoin(CustomerEntity.Relations.CustomerAddressEntityUsingCustomerId)
												.LeftJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
								.Where(CustomerAddressFields.AddressId.IsNotNull())
								.AndWhere(SalesOrderHeaderFields.SalesOrderId.IsNull());
				var results = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in results)
				{
					count++;
				}
				Assert.AreEqual(66, count);
			}
		}


		[Test]
		public void MultipleDynamicWhereClauses()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				string firstName = "a";
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = metaData.SalesOrderHeader.AsQueryable();
				//q = q.Where(soh => (soh.Customer as IndividualEntity).Contact.FirstName.Contains(firstName));
				//q = q.Where(soh => soh.Customer.CustomerAddressCollection.Any(ca => ca.Address.StateProvince.Name == "Arizona"));

				var qf = new QueryFactory();
				var q = qf.SalesOrderHeader
							.From(QueryTarget.InnerJoin(SalesOrderHeaderEntity.Relations.CustomerEntityUsingCustomerId)
											 .InnerJoin(IndividualEntity.Relations.ContactEntityUsingContactId))
							.Where(ContactFields.FirstName.Contains(firstName))
							.AndWhere(qf.CustomerAddress
											.CorrelatedOver(CustomerAddressEntity.Relations.CustomerEntityUsingCustomerId)
											.From(QueryTarget.InnerJoin(CustomerAddressEntity.Relations.AddressEntityUsingAddressId)
															 .InnerJoin(AddressEntity.Relations.StateProvinceEntityUsingStateProvinceId))
											.Any(StateProvinceFields.Name == "Arizona"));
				var results = adapter.FetchQuery(q);

				foreach(var v in results)
				{
				}
			}
		}

		
		[Test]
		public void CustomProjectionIntoAnonymousTypeOnSelectManyList()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q1 = from soh in metaData.SalesOrderHeader
				//        from sod in soh.SalesOrderDetailCollection
				//        select soh;
				//var q = (from soh in q1
				//        select new { soh.SalesOrderId }).Distinct();

				var qf = new QueryFactory();
				var q1 = qf.SalesOrderHeader
								.From(QueryTarget.InnerJoin(SalesOrderHeaderEntity.Relations.SalesOrderDetailEntityUsingSalesOrderId));

				var q = q1
						.Select(() => new { SalesOrderId = SalesOrderHeaderFields.SalesOrderId.ToValue<int>() })
						.Distinct();

				// Alternative with separate projection and typed projector
				//var q = q1
				//        .Select(SalesOrderHeaderFields.SalesOrderId)
				//        .Distinct()
				//        .WithProjector(p => new { SalesOrderId = (int)p[0] });

				var results = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in results)
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q1 = from customer in metaData.Customer
				//         where customer.CustomerId<100
				//         from soh in customer.SalesOrderHeaderCollection.DefaultIfEmpty()
				//         select new { customer.CustomerId, soh.SalesOrderId };
				//var z = q1.ToList();

				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.LeftJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
							.Where(CustomerFields.CustomerId < 100)
							.Select(() => new
							{
								CustomerId = CustomerFields.CustomerId.ToValue<int>(),
								SalesOrderId = SalesOrderHeaderFields.SalesOrderId.ToValue<int?>()
							});

				// Alternative with separate projection and typed projector
				//var q = qf.Customer
				//                .From(QueryTarget.LeftJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
				//                .Where(CustomerFields.CustomerId < 100)
				//                .Select(CustomerFields.CustomerId, SalesOrderHeaderFields.SalesOrderId)
				//                .WithProjector(p => new { CustomerId = (int)p["CustomerId"], SalesOrderId = (int?)p["SalesOrderId"] });

				var z = adapter.FetchQuery(q);
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
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = metaData.Customer.AsQueryable();
				//var q = from c in customers
				//        from soh in c.SalesOrderHeaderCollection.Where(soh => soh.SalesOrderId < 10).DefaultIfEmpty()
				//        select new { c.CustomerId, soh.SalesOrderId };

				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.LeftJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
							.Where(SalesOrderHeaderFields.SalesOrderId < 10)
							.Select(CustomerFields.CustomerId, SalesOrderHeaderFields.SalesOrderId);
				var results = adapter.FetchQuery(q);
				
				foreach(var v in results)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests6()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = metaData.Customer.AsQueryable();
				//var customersDerivedTable = from customer in customers select customer; //Using this to force a derived table causes a crash
				//customersDerivedTable = customers.Where(c => c.CustomerId > 10); //To force a derived table

				//var q = from customer in customersDerivedTable
				//        join soh in
				//            (from s in metaData.SalesOrderHeader where s.SalesPersonId > 22 select s) on customer.CustomerId equals soh.CustomerId into oc
				//        from nullableSOH in oc.DefaultIfEmpty()

				//        from ca in customer.CustomerAddressCollection.DefaultIfEmpty()
				//        select new { customer.CustomerId, ca.AddressId, nullableSOH.SalesOrderId, customer.SalesTerritory.Name };
				//q = q.Take(10);

				//var l = q.ToList();

				var qf = new QueryFactory();
				var customers = qf.Customer
									.Where(CustomerFields.CustomerId > 10);

				var q = customers
							.From(QueryTarget.LeftJoin(qf.SalesOrderHeader.As("soh")	// needs alias as the where clause will make it a separate query.
																.Where(SalesOrderHeaderFields.SalesOrderId > 22))
													.On(CustomerFields.CustomerId == SalesOrderHeaderFields.CustomerId.Source("soh"))
											.LeftJoin(CustomerEntity.Relations.CustomerAddressEntityUsingCustomerId)
											.InnerJoin(CustomerEntity.Relations.SalesTerritoryEntityUsingTerritoryId))
							.Select(CustomerFields.CustomerId, CustomerAddressFields.AddressId, SalesOrderHeaderFields.SalesOrderId.Source("soh"), 
									SalesTerritoryFields.Name);

				var results = adapter.FetchQuery(q);

			}
		}

		[Test]
		public void MiscellaneousTests7()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = metaData.Customer.AsQueryable();
				//var q = from customer in customers
				//        join soh in
				//            (from s in metaData.SalesOrderHeader where s.SalesPersonId > 22 select s) on customer.CustomerId equals soh.CustomerId into join1
				//        join ca in 
				//            (from custaddr in metaData.CustomerAddress where custaddr.AddressId > 100 select custaddr) 
				//                on customer.CustomerId equals ca.CustomerId into join2
				//        from xca in join2.DefaultIfEmpty()
				//        from xsoh in join1.DefaultIfEmpty()
				//        select new { customer.CustomerId, xca.AddressId, xsoh.SalesOrderId, customer.SalesTerritory.Name };
				//q = q.Take(10);

				//var l = q.ToList();
				var qf = new QueryFactory();
				var q = qf.Customer
							.From(QueryTarget.LeftJoin(qf.SalesOrderHeader)
													.On((CustomerFields.CustomerId == SalesOrderHeaderFields.SalesOrderId)
															.And(SalesOrderHeaderFields.SalesOrderId > 22))
											.LeftJoin(qf.CustomerAddress)
													.On((CustomerFields.CustomerId == CustomerAddressFields.CustomerId)
															.And(CustomerAddressFields.AddressId > 100))
											.InnerJoin(CustomerEntity.Relations.SalesTerritoryEntityUsingTerritoryId))
							.Select(CustomerFields.CustomerId, CustomerAddressFields.AddressId, SalesOrderHeaderFields.SalesOrderId, SalesTerritoryFields.Name);

				// Alternative
				//var q = qf.Customer
				//                .From(QueryTarget.LeftJoin(qf.SalesOrderHeader.As("soh").Where(SalesOrderHeaderFields.SalesOrderId > 22))
				//                                        .On(CustomerFields.CustomerId == SalesOrderHeaderFields.SalesOrderId.Source("soh"))
				//                                .LeftJoin(qf.CustomerAddress.As("ca").Where(CustomerAddressFields.AddressId > 100))
				//                                        .On(CustomerFields.CustomerId == CustomerAddressFields.CustomerId.Source("ca"))
				//                                .InnerJoin(CustomerEntity.Relations.SalesTerritoryEntityUsingTerritoryId))
				//                .Select(CustomerFields.CustomerId, CustomerAddressFields.AddressId.Source("ca"), SalesOrderHeaderFields.SalesOrderId.Source("soh"),
				//                        SalesTerritoryFields.Name);
				var results = adapter.FetchQuery(q);
			}
		}

		[Test]
		public void MiscellaneousTests9()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var customers = metaData.Customer.AsQueryable().Select(c=>c);
				//customers = customers.Where(c => c.CustomerId > 10);
				//var q = from c in customers
				//        from soh in c.SalesOrderHeaderCollection
				//        select new
				//        {
				//            Scalar = (from a in metaData.Address
				//                      from ca in a.CustomerAddressCollection
				//                      where ca.CustomerId == c.CustomerId
				//                      select a).Count()
				//        };

				var qf = new QueryFactory();
				var q = qf.Customer
							.Where(CustomerFields.CustomerId > 10)
							.From(QueryTarget.InnerJoin(CustomerEntity.Relations.SalesOrderHeaderEntityUsingCustomerId))
							.Select(qf.CustomerAddress
											.CorrelatedOver(CustomerEntity.Relations.CustomerAddressEntityUsingCustomerId)
											.From(QueryTarget.InnerJoin(CustomerAddressEntity.Relations.AddressEntityUsingAddressId))
											.CountRow().As("Scalar"));
				var results = adapter.FetchQuery(q);

				foreach(var v in results)
				{
				}

			}
		}


		[Test]
		public void MiscellaneousTests10()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from ca in metaData.CustomerAddress
				//        where ca.Address.PostalCode == "98011"
				//        group ca by ca.Address.StateProvince.CountryRegion.Name into g
				//        select new { g.Key, Amount = g.Count() };

				var qf = new QueryFactory();
				var q = qf.CustomerAddress
								.From(QueryTarget.InnerJoin(CustomerAddressEntity.Relations.AddressEntityUsingAddressId)
												.InnerJoin(AddressEntity.Relations.StateProvinceEntityUsingStateProvinceId)
												.InnerJoin(StateProvinceEntity.Relations.CountryRegionEntityUsingCountryRegionCode))
								.Where(AddressFields.PostalCode == "98011")
								.Select(CountryRegionFields.Name, Functions.CountRow())
								.GroupBy(CountryRegionFields.Name);
				var results = adapter.FetchQuery(q);

				foreach(var v in results)
				{
				}
			}
		}


		[Test]
		public void GetAmountOfAddresses()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var amount = metaData.Address.Count();

				var qf = new QueryFactory();
				var q = qf.Address.Select(Functions.CountRow());

				// alternative:
				//var q = qf.Create()
				//            .Select(Functions.CountRow())
				//            .From(qf.Address);

				var amount = adapter.FetchScalar<int>(q);
				Assert.AreEqual(19615, amount);
			}
		}


		[Test]
		public void MiscellaneousTests14()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// Linq: in this form, it crashes with null-ref. Possibly due to the multi-hop in the on clause. 
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from address in metaData.Address
				//        join employeeAddress in metaData.EmployeeAddress on address.Id equals employeeAddress.AddressId
				//        join individual in metaData.Individual on employeeAddress.Employee.Contact.ContactId equals individual.ContactId
				//        select address;

				var qf = new QueryFactory();
				var q = qf.Address
							.From(QueryTarget.InnerJoin(AddressEntity.Relations.EmployeeAddressEntityUsingAddressId)
											.InnerJoin(EmployeeAddressEntity.Relations.EmployeeEntityUsingEmployeeId)
											.InnerJoin(EmployeeEntity.Relations.ContactEntityUsingContactId)
											.InnerJoin(IndividualEntity.Relations.ContactEntityUsingContactId));
				var results = adapter.FetchQuery(q);

				foreach(var v in results)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests15()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// Get all individuals which have a contact which has a related employee which has 1 or more addresses.

				// Linq: In this form, it crashes with re-aliasing due to inheritance.
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from c in metaData.Contact
				//        where c.EmployeeCollection.Any(e=>e.EmployeeAddressCollection.Count()>0)
				//        select c.IndividualCollection;
				var qf = new QueryFactory();
				var q = qf.Individual
								.From(QueryTarget.InnerJoin(qf.Contact.As("c")
																.Where(qf.Employee
																			.CorrelatedOver(EmployeeEntity.Relations.ContactEntityUsingContactId)
																			.Any(qf.EmployeeAddress
																						.CorrelatedOver(EmployeeAddressEntity.Relations.EmployeeEntityUsingEmployeeId)
																						.CountRow().GreaterThan(0))))
													.On(IndividualFields.ContactId==ContactFields.ContactId.Source("c")));
				var results = adapter.FetchQuery(q);
				foreach(var v in results)
				{
				}
			}
		}


		[Test]
		public void MiscellaneousTests17()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = from soh in metaData.SalesOrderHeader
				//        from sod in soh.SalesOrderDetailCollection
				//        from sod2 in soh.SalesOrderDetailCollection
				//        where sod.SalesOrderId > 10 && sod2.SalesOrderId > 1
				//        select soh;

				//var w = (from soh in q
				//         from sod3 in soh.SalesOrderDetailCollection
				//         select soh).ToList();

				var qf = new QueryFactory();
				var q = qf.SalesOrderHeader.As("soh")
							.From(QueryTarget.InnerJoin(SalesOrderHeaderEntity.Relations.SalesOrderDetailEntityUsingSalesOrderId, string.Empty, "sod")
											.InnerJoin(SalesOrderHeaderEntity.Relations.SalesOrderDetailEntityUsingSalesOrderId, string.Empty, "sod2"))
							.Where((SalesOrderDetailFields.SalesOrderId.Source("sod") > 10).And(SalesOrderDetailFields.SalesOrderId.Source("sod2") > 1));

				// re-calling From() on 'q' will overwrite the from clause so we're creating a new EntityQuery<T>
				var w = qf.SalesOrderHeader.TargetAs("soh")
								.From(q.InnerJoin(SalesOrderHeaderEntity.Relations.SalesOrderDetailEntityUsingSalesOrderId, "soh", "sod3"));
				var results = adapter.FetchQuery(w);

			}
		}


		[Test]
		public void MiscellaneousTests18()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var c = from Address in metaData.Address
				//    from EmployeeAddress in Address.EmployeeAddressCollection
				//    select Address;

				//var q = from Address in c
				//        from EmployeeAddress in Address.EmployeeAddressCollection
				//        from SalesOrderHeader in Address.SalesOrderHeaderCollection.DefaultIfEmpty()
				//        select Address;

				var qf = new QueryFactory();
				var c = qf.Address.As("c")
							.From(QueryTarget.InnerJoin(AddressEntity.Relations.EmployeeAddressEntityUsingAddressId));
				var q = qf.Address.TargetAs("c")
							.From(c.InnerJoin(AddressEntity.Relations.EmployeeAddressEntityUsingAddressId, "c", string.Empty)
									.LeftJoin(AddressEntity.Relations.SalesOrderHeaderEntityUsingBillToAddressId, "c", string.Empty));
				var results = adapter.FetchQuery(q);

				foreach(var v in results)
				{
				}
			}
		}

		[Test]
		public void PrefetchPathTestUsingInheritanceAndSubqueries()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 1;
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from s in metaData.SalesPerson
				//         select s).WithPath(
				//            new PathEdge<SalesTerritoryEntity>(SalesPersonEntity.PrefetchPathSalesTerritory)
				//                            );

				var qf = new QueryFactory();
				var q = qf.SalesPerson
								.WithPath(SalesPersonEntity.PrefetchPathSalesTerritory);
				var results = adapter.FetchQuery(q);

				int count = 0;
				foreach(SalesPersonEntity v in results)
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
				const double EarthRadius = 3963.1;
				//LinqMetaData metaData = new LinqMetaData(adapter, new AdventureWorksFunctionMappings());
				//var results = (from address in metaData.CreativeAddress
				//               let lat1 = address.LatitudeF.Value * Math.PI / 180
				//               let x2 = AdventureWorksFunctions.Sin(lat1) *
				//                           AdventureWorksFunctions.Cos(lat1)
				//               let d = EarthRadius * (-1 * AdventureWorksFunctions.Atan(x2 / AdventureWorksFunctions.Sqrt(1 - x2 * x2)) + Math.PI / 2)
				//               where address.Latitude.HasValue && address.Longitude.HasValue
				//               select d).ToList();

				// QuerySpec doesn't support 'let', the value has to specified as-is in the query.
				var qf = new QueryFactory();
				var q = qf.Create()
							.Select(() =>
									Functions.Constant(EarthRadius)
										.Mul(Functions.Constant(-1)
											.Mul(
												AdventureWorksFunctionsQS.Atan(
														AdventureWorksFunctionsQS.Sin((CreativeAddressFields.LatitudeF * Math.PI) / 180)
														.Mul(AdventureWorksFunctionsQS.Cos((CreativeAddressFields.LatitudeF * Math.PI) / 180))
													.Div(
														AdventureWorksFunctionsQS.Sqrt(
															Functions.Constant(1)
																.Sub((AdventureWorksFunctionsQS.Sin((CreativeAddressFields.LatitudeF * Math.PI) / 180)
																		.Mul(AdventureWorksFunctionsQS.Cos((CreativeAddressFields.LatitudeF * Math.PI) / 180)))
																	 .Mul(AdventureWorksFunctionsQS.Sin((CreativeAddressFields.LatitudeF * Math.PI) / 180)
																		.Mul(AdventureWorksFunctionsQS.Cos((CreativeAddressFields.LatitudeF * Math.PI) / 180)))))
														))
												)
											.Add(Functions.Constant(Math.PI / 2))
										)
								.As("d")
								.ToValue<double>())
							.Where(CreativeAddressFields.Latitude.IsNotNull().And(CreativeAddressFields.Longitude.IsNotNull()));
				q.CustomFunctionMappingStore = new AdventureWorksFunctionMappingsQS();
				var results = adapter.FetchQuery(q);
				foreach(var d in results)
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
				var lst = new List<Pair<int, int>>();
				lst.Add(new Pair<int, int> { Value1 = 1, Value2 = 722 });
				lst.Add(new Pair<int, int> { Value1 = 2, Value2 = 725 });
				lst.Add(new Pair<int, int> { Value1 = 3, Value2 = 726 });

				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q1 = metaData.WorkOrderRouting.AsQueryable();

				//q1 = q1.Where(
				// p => lst.Contains(
				//         new Pair<int, int>
				//         {
				//             Value1 = p.WorkOrder.WorkOrderId,
				//             Value2 = p.WorkOrder.ProductId
				//         }
				//         )
				// );

				//var results = q1.Select(
				//             p => new
				//             {
				//                 p.ProductId,
				//                 p.WorkOrder.WorkOrderId
				//             }).ToList();

				var qf = new QueryFactory();
				var q1 = qf.WorkOrderRouting
				            .From(QueryTarget.InnerJoin(WorkOrderRoutingEntity.Relations.WorkOrderEntityUsingWorkOrderId));
				// the Contains isn't directly supported, as it's a projection oriented contains. Append the predicates manually.
				foreach(var tuple in lst)
				{
				    q1.OrWhere((WorkOrderFields.WorkOrderId== tuple.Value1).And(WorkOrderFields.ProductId== tuple.Value2));
				}
				// append projection
				var q = q1.Select(WorkOrderRoutingFields.ProductId, WorkOrderFields.WorkOrderId);
				var results = adapter.FetchQuery(q);
				foreach(var v in results)
				{
					
				}
			}
		}


		[Test]
		public void ProjectionToCustomClassUsingNullableValueThroughScalarQueryTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter);
				//var q = (from p in metaData.WorkOrder
				//         select new BusinessObject()
				//         {
				//             ProductId = p.ProductId,
				//             ModifiedDate = p.WorkOrderRoutingCollection.Max(x => x.ModifiedDate)
				//         });

				var qf = new QueryFactory();

				var q = qf.WorkOrder
								.Select(() => new BusinessObject()
												{
													ProductId = WorkOrderFields.ProductId.ToValue<int>(),
													ModifiedDate = qf.WorkOrderRouting
																		.CorrelatedOver(WorkOrderRoutingEntity.Relations.WorkOrderEntityUsingWorkOrderId)
																		.Max(WorkOrderRoutingFields.ModifiedDate)
																		.ToValue<DateTime?>()
												});

				// Alternative with separate projection and projector.
				//var q = qf.WorkOrder.Select(WorkOrderFields.ProductId,
				//                            qf.WorkOrderRouting
				//                                    .CorrelatedOver(WorkOrderRoutingEntity.Relations.WorkOrderEntityUsingWorkOrderId)
				//                                    .Max(WorkOrderRoutingFields.ModifiedDate).As("ModifiedDate"))
				//                    .WithProjector(wo => new BusinessObject() 
				//                                            { 
				//                                                ProductId = (int)wo["ProductId"], 
				//                                                ModifiedDate = (DateTime?)wo["ModifiedDate"] 
				//                                            });
				// alternative 
				//var q = qf.WorkOrder.Select(WorkOrderFields.ProductId,
				//                            qf.Create().Select(WorkOrderRoutingFields.ModifiedDate.Max())
				//                                        .Where(WorkOrderFields.ProductId == WorkOrderRoutingFields.ProductId)
				//                                        .ToScalar().As("ModifiedDate"))
				//                    .WithProjector(wo => new BusinessObject()
				//                    {
				//                        ProductId = (int)wo["ProductId"],
				//                        ModifiedDate = (DateTime?)wo["ModifiedDate"]
				//                    });

				var results = adapter.FetchQuery(q);
				int count = 0;
				foreach(var v in results)
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
		public void CompareToOnGuidTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				//LinqMetaData metaData = new LinqMetaData(adapter, new AdventureWorksGuidFunctionMappings());
				//var results = (from a in metaData.Address select a)
				//        .Where(a => a.Rowguid.CompareTo(new Guid("57f73b00-26b1-11df-8600-001c2311de1b")) < 0);

				var qf = new QueryFactory();
				var q = qf.Address
							.Where(Functions.Compare(AddressFields.Rowguid, new Guid("57f73b00-26b1-11df-8600-001c2311de1b")).LesserThan(0));
				var results = adapter.FetchQuery(q);

				int count = 0;
				foreach(var v in results)
				{
					count++;
				}
				Assert.AreEqual(6, count);
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
