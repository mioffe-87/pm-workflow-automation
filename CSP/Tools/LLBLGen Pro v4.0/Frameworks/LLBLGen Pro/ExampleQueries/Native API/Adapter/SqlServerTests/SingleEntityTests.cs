using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.Xml;
using System.Data;
using System.Transactions;

using NUnit.Framework;

using UnitTests.Dal.Adapter.EntityClasses;
using UnitTests.Dal.Adapter.HelperClasses;
using UnitTests.Dal.Adapter;
using UnitTests.Dal.Adapter.DatabaseSpecific;
using UnitTests.Dal.Adapter.FactoryClasses;
using UnitTests.Dal.Adapter.TypedListClasses;

//using UnitTests.Dal.Adapter.VB.EntityClasses;
//using UnitTests.Dal.Adapter.VB.HelperClasses;
//using UnitTests.Dal.Adapter.VB;
//using UnitTests.Dal.Adapter.VB.DatabaseSpecific;
//using UnitTests.Dal.Adapter.VB.FactoryClasses;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Threading;
using System.Collections.Generic;

namespace Unittests.TestLibrary.SqlServerTests.Adapter
{

	/// <summary>
	/// SingleEntityTests.
	/// Contains NUnit tests working on single entity objects.
	/// </summary>
	[TestFixture]
	public class SingleEntityTests
	{
		private Guid	_testRunID;
		private bool	_eventHandlerVisited;

		/// <summary>
		/// CTor
		/// </summary>
		public SingleEntityTests()
		{
		}


		/// <summary>
		/// Default event handler for change event tests.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void DefaultEventHandler(object sender, EventArgs e)
		{
			_eventHandlerVisited =true;
		}

		void DefaultPropertyChangedHandler( object sender, System.ComponentModel.PropertyChangedEventArgs e )
		{
			_eventHandlerVisited = true;
		}


		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid();
			Console.WriteLine("TestRunID: {0}", _testRunID.ToString());
			SerializationHelper.Optimization = SerializationOptimization.Fast;
		}


		[Test]
		public void TransactionRollbackStateRollbackTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.StartTransaction(System.Data.IsolationLevel.ReadCommitted, "TestTran");

				// first save a customer.
				CustomerEntity newCustomer1 = EntityCreator.CreateNewCustomer(1);
				newCustomer1.TestRunId = _testRunID;
				((IEntity2)newCustomer1).Validator = null;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer1.VisitingAddress = newAddress;
				newCustomer1.BillingAddress = newAddress;

				Assert.IsTrue(newCustomer1.IsDirty);
				Assert.IsTrue(newCustomer1.IsNew);
				Assert.AreEqual(EntityState.New, newCustomer1.Fields.State);

				bool result = adapter.SaveEntity(newCustomer1, true);
				Assert.IsFalse(newCustomer1.IsDirty);
				Assert.IsFalse(newCustomer1.IsNew);
				Assert.AreEqual(EntityState.Fetched, newCustomer1.Fields.State);

				// rollback transaction
				adapter.Rollback();
				Assert.IsTrue(newCustomer1.IsDirty);
				Assert.IsTrue(newCustomer1.IsNew);
				Assert.AreEqual(EntityState.New, newCustomer1.Fields.State);
			}
		}


		[Test]
		public void SplitOffEntitySaveNoReference()
		{
			SplitOffNoBlobDataEntity mainData = new SplitOffNoBlobDataEntity();
			mainData.FirstName = "John";
			mainData.LastName = "Doe";

			SplitOffBlobDataEntity splitOff = new SplitOffBlobDataEntity();
			splitOff.Notes = "These are the notes";
			splitOff.Photo = new byte[] { 0, 1, 2 };

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(mainData, true));
				Assert.IsTrue(mainData.Id > 0);
				splitOff.MainDataId = mainData.Id;
				Assert.IsTrue(adapter.SaveEntity(splitOff, true));

				// fetch both in new entities
				SplitOffNoBlobDataEntity mainDataFetched = new SplitOffNoBlobDataEntity(mainData.Id);
				Assert.IsTrue(adapter.FetchEntity(mainDataFetched));
				Assert.AreEqual(mainData.FirstName, mainDataFetched.FirstName);
				Assert.AreEqual(mainData.LastName, mainDataFetched.LastName);
				SplitOffBlobDataEntity splitOffFetched = new SplitOffBlobDataEntity(mainData.Id);
				Assert.IsTrue(adapter.FetchEntity(splitOffFetched));
				Assert.AreEqual(splitOff.Notes, splitOffFetched.Notes);
				Assert.AreEqual(3, splitOffFetched.Photo.Length);

				// just delete 1, no need to delete two.
				Assert.IsTrue(adapter.DeleteEntity(mainData));
			}
		}


		[Test]
		public void SplitOffEntitySaveWithReference()
		{
			SplitOffNoBlobDataEntity mainData = new SplitOffNoBlobDataEntity();
			mainData.FirstName = "John";
			mainData.LastName = "Doe";

			SplitOffBlobDataEntity splitOff = new SplitOffBlobDataEntity();
			splitOff.Notes = "These are the notes";
			splitOff.Photo = new byte[] { 0, 1, 2 };

			mainData.BlobData = splitOff;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(mainData, true));
				Assert.IsTrue(mainData.Id > 0);
				Assert.AreEqual(mainData.Id, splitOff.MainDataId);

				// fetch both in new entities using prefetch path
				PrefetchPath2 path = new PrefetchPath2(EntityType.SplitOffNoBlobDataEntity);
				path.Add(SplitOffNoBlobDataEntity.PrefetchPathBlobData);
				SplitOffNoBlobDataEntity mainDataFetched = new SplitOffNoBlobDataEntity(mainData.Id);
				Assert.IsTrue(adapter.FetchEntity(mainDataFetched, path));
				Assert.AreEqual(mainData.FirstName, mainDataFetched.FirstName);
				Assert.AreEqual(mainData.LastName, mainDataFetched.LastName);
				SplitOffBlobDataEntity splitOffFetched = mainDataFetched.BlobData;
				Assert.IsTrue(adapter.FetchEntity(splitOffFetched));
				Assert.AreEqual(splitOff.Notes, splitOffFetched.Notes);
				Assert.AreEqual(3, splitOffFetched.Photo.Length);

				// just delete 1, no need to delete two.
				Assert.IsTrue(adapter.DeleteEntity(mainData));
			}
		}


		[Test]
		public void SplitOffEntityFetchWithFilterOnMain()
		{
			SplitOffNoBlobDataEntity mainData = new SplitOffNoBlobDataEntity();
			mainData.FirstName = "John";
			mainData.LastName = "Doe";

			SplitOffBlobDataEntity splitOff = new SplitOffBlobDataEntity();
			splitOff.Notes = "These are the notes";
			splitOff.Photo = new byte[] { 0, 1, 2 };

			mainData.BlobData = splitOff;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(mainData, true));
				Assert.IsTrue(mainData.Id > 0);
				Assert.AreEqual(mainData.Id, splitOff.MainDataId);

				// fetch blob with filter on main
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(SplitOffBlobDataEntity.Relations.SplitOffNoBlobDataEntityUsingMainDataId, "NB");
				filter.PredicateExpression.Add(SplitOffNoBlobDataFields.FirstName.SetObjectAlias("NB") == "John");
				SplitOffBlobDataEntity splitOffFetched = adapter.FetchNewEntity<SplitOffBlobDataEntity>(filter);
				Assert.AreEqual(splitOff.Notes, splitOffFetched.Notes);
				Assert.AreEqual(3, splitOffFetched.Photo.Length);

				// just delete 1, no need to delete two.
				Assert.IsTrue(adapter.DeleteEntity(mainData));
			}
		}


		[Test]
		public void EnumSaveTest()
		{
			EnumTesterEntity et = new EnumTesterEntity();
			et.EnumField = ConsoleColor.Blue;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(et));
				Assert.IsTrue(et.Id > 0);

				Assert.IsTrue(adapter.DeleteEntity(et));
			}
		}


		[Test]
		public void EnumFetchTest()
		{
			EntityCollection<EnumTesterEntity> ets = new EntityCollection<EnumTesterEntity>();
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Black});
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Blue, NullableEnumField = ConsoleColor.Red});
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Blue, NullableEnumField = ConsoleColor.Red});
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.AreEqual(3, adapter.SaveEntityCollection(ets));
				foreach(var et in ets)
				{
					Assert.IsTrue(et.Id > 0);
				}

				// fetch blues back
				var blues = new EntityCollection<EnumTesterEntity>();
				try
				{
					adapter.FetchEntityCollection(blues, new RelationPredicateBucket(EnumTesterFields.EnumField == ConsoleColor.Blue));
					Assert.AreEqual(2, blues.Count);
					foreach(var et in blues)
					{
						Assert.AreEqual(ConsoleColor.Blue, et.EnumField);
						Assert.AreEqual(ConsoleColor.Blue, et.Fields[(int)EnumTesterFieldIndex.EnumField].CurrentValue);
						Assert.AreEqual(ConsoleColor.Red, et.NullableEnumField.Value);
						Assert.AreEqual(ConsoleColor.Red, ((Nullable<ConsoleColor>)et.Fields[(int)EnumTesterFieldIndex.NullableEnumField].CurrentValue).Value);
					}
				}
				finally
				{
					Assert.AreEqual(3, adapter.DeleteEntityCollection(ets));
				}
			}
		}


		[Test]
		public void NullValueInCharFieldSaveTest()
		{
			AddressEntity address = new AddressEntity();
			address.StreetName = "blaatstraat";
			address.HouseNumber = 13;
			address.City = "Duckstad";
			address.Country = "Disneyland";
			address.TestRunId = Guid.NewGuid();
			address.Zipcode = null;
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(address, true));
			}
			Assert.IsTrue(address.AddressId > 0);
			Assert.IsNull(address.Fields["Zipcode"].CurrentValue);
		}


		[Test]
		public void OverflowInPrecisionScaleWithScaleCorrectionTest()
		{
			EntityBase2.BuildInValidationBypassMode = BuildInValidationBypass.NoBypass;

			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.Round;

			ProductEntity p = new ProductEntity();
			p.Price = 123.45M;
			p.Price = 123.123M; // overflow on scale, should be rounded
			Assert.AreEqual(p.Price, 123.12M);
			p.Price = 123.1255M; // overflow on scale, should be rounded
			Assert.AreEqual(p.Price, 123.13M);

			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.Truncate;
			p.Price = 123.123M; // overflow on scale, should be truncated
			Assert.AreEqual(p.Price, 123.12M);
			p.Price = 123.1255M; // overflow on scale, should be truncated
			Assert.AreEqual(p.Price, 123.12M);

			EntityBase2.ScaleOverflowCorrectionActionToUse = ScaleOverflowCorrectionAction.None;
		}


		[Test]
		public void OverflowInPrecisionScaleTest()
		{
			EntityBase2.BuildInValidationBypassMode = BuildInValidationBypass.NoBypass;

			ProductEntity p = new ProductEntity();
			p.Price = 123.45M;

			try
			{
				p.Price = 123456789.123M; // overflow on both precision and scale.
			}
			catch(ArgumentOutOfRangeException ex)
			{
				Console.WriteLine(ex.Message);
			}
			catch(Exception ex)
			{
				// other exception: fail
				Assert.Fail("Unexpected exception: " + ex.Message);
			}

			try
			{
				p.Price = 123433356789.3M; // overflow on precision
			}
			catch(ArgumentOutOfRangeException ex)
			{
				Console.WriteLine(ex.Message);
			}
			catch(Exception ex)
			{
				// other exception: fail
				Assert.Fail("Unexpected exception: " + ex.Message);
			}

			try
			{
				p.Price = 123433.3000M; // overflow on scale
			}
			catch(ArgumentOutOfRangeException ex)
			{
				Console.WriteLine(ex.Message);
			}
			catch(Exception ex)
			{
				// other exception: fail
				Assert.Fail("Unexpected exception: " + ex.Message);
			}

			// switch off bypassing
			EntityBase2.BuildInValidationBypassMode = BuildInValidationBypass.AlwaysBypass;
			try
			{
				p.Price = 123433.3000M; // overflow on scale
			}
			catch(ArgumentOutOfRangeException)
			{
				Assert.Fail("No exception should be raised, build-in validation should be bypassed");
			}
			catch(Exception ex)
			{
				// other exception: fail
				Assert.Fail("Unexpected exception: " + ex.Message);
			}
			EntityBase2.BuildInValidationBypassMode = BuildInValidationBypass.NoBypass;

		}


		[Test]
		public void InMemoryLikeFilterTest()
		{
			// create a set of products and filter these using a like predicate, in-memory
			EntityCollection<ProductEntity> products = new EntityCollection<ProductEntity>(new ProductEntityFactory());
			ProductEntity newProduct = new ProductEntity();
			newProduct.ShortDescription = "0 Application";
			products.Add(newProduct);
			newProduct = new ProductEntity();
			newProduct.ShortDescription = "0.1 Application";
			products.Add(newProduct);
			newProduct = new ProductEntity();
			newProduct.ShortDescription = "1 Application";
			products.Add(newProduct);
			newProduct = new ProductEntity();
			newProduct.ShortDescription = "1.1 Application";
			products.Add(newProduct);
			newProduct = new ProductEntity();
			newProduct.ShortDescription = "1.1.1 Application";
			products.Add(newProduct);
			newProduct = new ProductEntity();
			newProduct.ShortDescription = "1.2 Application";
			products.Add(newProduct);

			Assert.AreEqual(6, products.Count);

			List<int> matches = products.FindMatches((ProductFields.ShortDescription % "1.% Application"));
			Assert.AreEqual(3, matches.Count);
			matches = products.FindMatches((ProductFields.ShortDescription % "1.1% Application"));
			Assert.AreEqual(2, matches.Count);

			matches = products.FindMatches((ProductFields.ShortDescription % "% Application"));
			Assert.AreEqual(6, matches.Count);

		}

		[Test]
		public void SingleSidedHiddenRelationSaveSyncTest()
		{
			CustomerSyncTestEntity newCustomer = EntityCreator.CreateNewCustomerSyncTest( 1 );
			newCustomer.TestRunId = _testRunID;
			AddressSyncTestEntity newAddress = EntityCreator.CreateNewAddressSyncTest( 1 );
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Assert.IsTrue( adapter.SaveEntity( newCustomer, true, true ) );
			}

			Assert.AreEqual( EntityState.Fetched, newAddress.Fields.State );
			Assert.AreEqual( EntityState.Fetched, newCustomer.Fields.State );
			Assert.IsTrue( (((int)newAddress.Fields[(int)AddressSyncTestFieldIndex.AddressId].CurrentValue) > 0) );
			Assert.IsTrue( (((int)newCustomer.Fields[(int)CustomerSyncTestFieldIndex.CustomerId].CurrentValue) > 0) );
			int addressID = newAddress.AddressId;
			int customerID = newCustomer.CustomerId;
			Assert.IsTrue( (newCustomer.VisitingAddressId == addressID) );
			Assert.AreEqual( newAddress, newCustomer.BillingAddress );
			Assert.IsTrue( (newAddress == newCustomer.BillingAddress) );
			Assert.AreEqual( ((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID );
			Assert.AreEqual( ((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID );
		}


		[Test]
		public void SingleSidedHiddenRelationSaveSyncTest2()
		{
			CustomerSyncTestEntity newCustomer = EntityCreator.CreateNewCustomerSyncTest( 1 );
			newCustomer.TestRunId = _testRunID;
			AddressSyncTestEntity newAddress = EntityCreator.CreateNewAddressSyncTest( 1 );
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;

			OrderSyncTestEntity newOrder = new OrderSyncTestEntity();
			newOrder.CustomerSyncTest = newCustomer;
			newOrder.TestRunId = _testRunID;
			newOrder.OrderDate = DateTime.Now;

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Assert.IsTrue( adapter.SaveEntity( newOrder, true, true ) );
			}
		}

		[Test]
		public void BinaryIsDirtyTest()
		{
			// create Company
			byte[] companyId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
			CompanyEntity newCompany = new CompanyEntity();
			newCompany.CompanyId = companyId;
			newCompany.TestrunId = _testRunID;
			newCompany.CompanyName = "Soldes";
			CompanyPropertyEntity newProperty = new CompanyPropertyEntity();
			newProperty.PropertyId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };
			newProperty.PropertyValue = "Test 1 2 3";
			newProperty.TestrunId = _testRunID;
			newCompany.CompanyProperty.Add( newProperty );

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				Assert.IsTrue( adapter.SaveEntity( newCompany ) );

				// refetch the stuff.
				CompanyEntity fetchedCompany = new CompanyEntity( companyId );
				adapter.FetchEntity( fetchedCompany );
				adapter.FetchEntityCollection( fetchedCompany.CompanyProperty, fetchedCompany.GetRelationInfoCompanyProperty() );
				Assert.IsFalse( fetchedCompany.CompanyProperty[0].IsDirty );
			}
		}


		[Test]
		public void TypedListWithNullValues()
		{
			ActionProcedures.ClearAll();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ProductEntity p = EntityCreator.CreateNewProduct(1);
				p.TestRunId = _testRunID;
				p.SetNewFieldValue((int)ProductFieldIndex.FullDescription, null);
				adapter.SaveEntity(p);

				Product_TypedList products = new Product_TypedList();
				adapter.FetchTypedList(products);
				Assert.AreEqual(1, products.Rows.Count);

				Assert.IsTrue(products[0].IsFullDescriptionNull());
				
				MemoryStream memStream = new MemoryStream();
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memStream, products);

				// seek back, deserialize in other bucket.
				memStream.Seek(0, SeekOrigin.Begin);
				Product_TypedList deserializedProducts = (Product_TypedList)formatter.Deserialize(memStream);
				memStream.Close();

				Assert.IsTrue( deserializedProducts[0].IsFullDescriptionNull() );
			}
		}


		[Test]
		public void PrimaryKeyFieldUpdateTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// save customer, address, order, product and orderrow
				CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
				customer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				customer.VisitingAddress = newAddress;
				customer.BillingAddress = newAddress;

				OrderEntity order = new OrderEntity();
				order.Customer = customer;
				order.TestRunId = _testRunID;
				order.OrderDate = DateTime.Now;

				ProductEntity product = EntityCreator.CreateNewProduct(1);
				product.TestRunId = _testRunID;

				OrderRowEntity row = new OrderRowEntity();
				row.Order = order;
				row.Product = product;
				row.TestRunId = _testRunID;
				row.Quantity = 10;
				row.PriceInvoiced = 10.0M;

				Assert.IsTrue(adapter.SaveEntity(customer));
				ProductEntity product2 = EntityCreator.CreateNewProduct(2);
				product2.TestRunId = _testRunID;
				Assert.IsTrue( adapter.SaveEntity(product2) );

				OrderRowEntity pkChanger = new OrderRowEntity(order.OrderId, product.ProductId);
				adapter.FetchEntity(pkChanger);
				Assert.IsTrue( pkChanger.SetNewFieldValue((int)OrderRowFieldIndex.ProductId, product2.ProductId) );
				Assert.IsTrue(adapter.SaveEntity(pkChanger));
			}
		}


		[Test]
		public void UpdateDirectlyWithCASEStatementOnFieldExpressionTest()
		{
			Console.WriteLine(RuntimeLibraryVersion.Build);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// save customer, address, order
				CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
				customer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				customer.VisitingAddress = newAddress;
				customer.BillingAddress = newAddress;

				OrderEntity order = new OrderEntity();
				order.Customer = customer;
				order.TestRunId = _testRunID;
				order.OrderDate = DateTime.Now;
				order.RequiredDate = DateTime.Now;
				order.ShippedDate = DateTime.Now;

				Assert.IsTrue(adapter.SaveEntity(customer));
				
				// update order directly using a case statement.
				RelationPredicateBucket orderFilter = new RelationPredicateBucket();
				orderFilter.PredicateExpression.Add(OrderFields.OrderId == order.OrderId);
				OrderEntity orderUpdater = new OrderEntity();
				orderUpdater.RequiredDate = null;
				orderUpdater.Fields[(int)OrderFieldIndex.ShippedDate].ExpressionToApply = new DbFunctionCall(
									"CASE WHEN {0} IS NOT NULL THEN NULL ELSE {1} END",
									new object[] { OrderFields.RequiredDate, OrderFields.ShippedDate});
				Assert.AreEqual(1, adapter.UpdateEntitiesDirectly(orderUpdater, orderFilter));
			}
		}


		[Test]
		[ExpectedException(typeof(ORMConcurrencyException))]
		public void SaveEntityWithUpdateRestrictionTest()
		{
			// create customer object.
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(customer, true, true));
				// now alter customer and formulate an update restriction, which makes the update to fail.
				customer.CustomerSince = DateTime.Now;
				Assert.IsFalse(adapter.SaveEntity(customer, true, new PredicateExpression(CustomerFields.CustomerSince<new DateTime(2000, 1, 1))));
			}
		}


		[Test]
		public void SaveEntityCollectionReturnValueTest()
		{
			// first insert 5 products
			EntityCollection products = new EntityCollection(new ProductEntityFactory());
			for (int i = 0; i < 5; i++)
			{
				ProductEntity newProduct = EntityCreator.CreateNewProduct(i+1);
				newProduct.TestRunId = _testRunID;
				products.Add(newProduct);
			}

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.AreEqual(5, adapter.SaveEntityCollection(products, true, true));

				// refetch them
				EntityCollection productsFetched = new EntityCollection(new ProductEntityFactory());
				adapter.FetchEntityCollection(productsFetched, null);

				// change them
				for (int i = 0; i < 5; i++)
				{
					((ProductEntity)productsFetched[i]).ShortDescription+=i.ToString();
				}

				Assert.AreEqual(5, adapter.SaveEntityCollection(productsFetched, true, true));
			}
		}


		[Test]
		public void DeleteEntitiesDirectlyTest()
		{
			// create customer object.
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(customer, true, true));
				adapter.DeleteEntitiesDirectly("CustomerEntity", new RelationPredicateBucket(CustomerFields.CustomerId==customer.CustomerId));
				adapter.DeleteEntitiesDirectly("AddressEntity", new RelationPredicateBucket(AddressFields.AddressId==newAddress.AddressId));
			}
		}


		[Test]
		public void DeleteEntityWithRestrictionTest()
		{
			// create customer object.
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(customer, true, true));
				PredicateExpression filter = new PredicateExpression(CustomerFields.CustomerId!=customer.CustomerId);
				try
				{
					Assert.IsFalse(adapter.DeleteEntity(customer, filter));
				}
				catch(ORMConcurrencyException)
				{
					// expected
				}
			}
		}

		[Test]
		public void EntityCollectionSaveTest()
		{
			// create customer object.
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			ProductEntity[] products = new ProductEntity[2];
			for (int i = 0; i < products.Length; i++)
			{
				products[i] = EntityCreator.CreateNewProduct(1);
				products[i].TestRunId = _testRunID;
			}

			OrderEntity[] orders = new OrderEntity[2];
			for (int i = 0; i < orders.Length; i++)
			{
				orders[i] = EntityCreator.CreateNewOrder();
				orders[i].Customer = customer;
				orders[i].TestRunId = _testRunID;
				orders[i].OrderDate = DateTime.Now;
			}

			// now create for each order an order line with a product.
			foreach(OrderEntity order in orders)
			{
				foreach(ProductEntity product in products)
				{
					OrderRowEntity orderRow = new OrderRowEntity();
					orderRow.Order = order;
					orderRow.Product = product;
					orderRow.Quantity=1;
					orderRow.TestRunId = _testRunID;
					orderRow.PriceInvoiced = product.Price.GetValueOrDefault();
				}
			}

			// save hierarchy, using a collection save
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(customer.Orders, true, true);
			}

			EntityCollection productsCollection = new EntityCollection(new ProductEntityFactory());
			productsCollection.DoNotPerformAddIfPresent=false;
			products = new ProductEntity[10];
			for (int i = 0; i < products.Length; i++)
			{
				products[i] = EntityCreator.CreateNewProduct(1);
				products[i].TestRunId = _testRunID;
				productsCollection.Add(products[i]);
			}

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				int result = adapter.SaveEntityCollection(productsCollection, true, false);
				Assert.AreEqual(10, result);
			}
		}


		/// <summary>
		/// Tests the entity.FieldChanged event handlers, both for entity fields and for related entity fields.
		/// </summary>
		[Test]
		public void ChangedEventHandlerTests()
		{
			// first create an order and a customer entity
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			OrderEntity order = new OrderEntity();
			order.Customer = customer;
			order.TestRunId = _testRunID;
			order.OrderDate = DateTime.Now;

			using( DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// save it 
				Assert.IsTrue(adapter.SaveEntity(customer, true));
			}

			// now, bind an eventhandler to customer's CompanyName and test if a change will trigger the event handler.
			customer.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( DefaultPropertyChangedHandler );
			_eventHandlerVisited=false;
			customer.CompanyName+="C";	// change the value
			customer.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( DefaultPropertyChangedHandler );
			Assert.IsTrue( _eventHandlerVisited );

			// now test the field mapped onto related field changed events. First direct changes:
			order.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( DefaultPropertyChangedHandler );
			_eventHandlerVisited=false;
			customer.CompanyName+="C";	// change the value
			Assert.IsTrue(_eventHandlerVisited);
			// then indirect changes
			_eventHandlerVisited=false;
			order.CustomerCompanyName+="C";	// change the value
			Assert.IsTrue(_eventHandlerVisited);
		}



		/// <summary>
		/// Tests a multiple hierarchical delete using the unit of work. This still can go wrong in odd situations, as the problem
		/// itself is unsolvable outside the database (i.e.: it's not possible to prevent FK violations for 100%)
		/// </summary>
		[Test]
		public void UnitOfWorkDeleteTest()
		{
			// first create an order and a customer entity
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			customer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			customer.VisitingAddress = newAddress;
			customer.BillingAddress = newAddress;

			OrderEntity order1 = new OrderEntity();
			order1.Customer = customer;
			order1.TestRunId = _testRunID;
			order1.OrderDate = DateTime.Now;
			OrderEntity order2 = new OrderEntity();
			order2.Customer = customer;
			order2.TestRunId = _testRunID;
			order2.OrderDate = DateTime.Now;

			using( DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// save it 
				Assert.IsTrue(adapter.SaveEntity(customer, true));

				int customerId = customer.CustomerId;

				// now delete the bunch using a unit of work object.
				UnitOfWork2 uow = new UnitOfWork2();

				uow.AddCollectionForDelete(customer.Orders);
				Assert.IsTrue(uow.AddForDelete(customer));
				Assert.IsTrue(uow.AddForDelete(newAddress));
				uow.Commit(adapter, true);

				CustomerEntity refetchedCustomer = new CustomerEntity(customerId);
				Assert.IsFalse(adapter.FetchEntity(refetchedCustomer));
				Assert.AreEqual(EntityState.OutOfSync, refetchedCustomer.Fields.State);
			}
		}

		[Test]
		public void InsertNullTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EmployeeEntity newEmployee1 = new EmployeeEntity();
				newEmployee1.Name = "Frans Bouma";
				newEmployee1.EmployeeSince = DateTime.Now;
				newEmployee1.TestRunId = _testRunID;

				// add other employee.
				EmployeeEntity newEmployee2 = new EmployeeEntity();
				newEmployee2.Name = "John Doe";
				newEmployee2.EmployeeSince = DateTime.Now;
				newEmployee2.ManagedByEmployee = newEmployee1;
				newEmployee2.TestRunId = _testRunID;

				Assert.IsTrue(adapter.SaveEntity(newEmployee1, true));
				Assert.IsFalse(newEmployee1.IsNew);
				Assert.IsFalse(newEmployee2.IsNew);
				Assert.IsTrue(newEmployee1.TestOriginalFieldValueForNull(EmployeeFieldIndex.ManagedBy));
				Assert.IsFalse(newEmployee2.TestOriginalFieldValueForNull(EmployeeFieldIndex.ManagedBy));

				// set to -1 first.
				newEmployee2.ManagedBy = -1;

				if(newEmployee2.ManagedBy==-1)
				{
					// set to null
					Assert.IsTrue(newEmployee2.SetNewFieldValue((int)EmployeeFieldIndex.ManagedBy, null));
					Assert.IsNull(newEmployee2.Fields[(int)EmployeeFieldIndex.ManagedBy].CurrentValue);
					newEmployee2.ManagedByEmployee = null;
				}

				Assert.IsTrue(adapter.SaveEntity(newEmployee2, true));
				Assert.IsTrue(newEmployee2.TestOriginalFieldValueForNull(EmployeeFieldIndex.ManagedBy));
			}
		}


		/// <summary>
		/// Tests the insertion of an VisitingAddress entity. No fancy stuff, just a single insert.
		/// </summary>
		[Test]
		public void InsertAddressTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// do not refetch on save, to test state flag.
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newAddress, false);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
				adapter.FetchEntity(newAddress);
				int addressID = newAddress.AddressId;
				Assert.IsTrue((addressID > 0));
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the insertion of an VisitingAddress entity. It uses a context to refetch it and to see if the Context promotes a saved entity to
		/// a fetched entity and that the entity can be grabbed from the context.
		/// </summary>
		[Test]
		public void InsertAddressWithContextSimpleTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				Context testContext = new Context();
				// do not refetch on save, to test state flag.
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				// add new entity to context. It will become available after teh save.
				testContext.Add(newAddress);
				newAddress.TestRunId = _testRunID;
				// do not refetch, do not save recursive
				bool result = adapter.SaveEntity(newAddress, false, null, false);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
				adapter.FetchEntity(newAddress);
				int addressID = newAddress.AddressId;
				Assert.IsTrue((addressID > 0));
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);

				AddressEntity cachedEntity = (AddressEntity)testContext.Get(new AddressEntityFactory(), newAddress.AddressId);
				Assert.AreEqual(cachedEntity, newAddress);
				Assert.IsTrue((cachedEntity==newAddress));
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the insertion of a Customer entity and an VisitingAddress entity. This is done using a System.Transaction transaction scope. 
		/// </summary>
		[Test]
		public void InsertCustomerTestSimpleWithSystemTransactionTest()
		{
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer( 1 );
			AddressEntity newAddress = EntityCreator.CreateNewAddress( 1 );
			int customerId = 0;
			int addressId = 0;

			using( TransactionScope ts = new TransactionScope() )
			{
				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{
					// save 2 entities non-recursive. This should be done in one transaction, namely the transaction scope we've started. 
					newCustomer.TestRunId = _testRunID;
					newAddress.TestRunId = _testRunID;
					newCustomer.VisitingAddress = newAddress;
					newCustomer.BillingAddress = newAddress;
					//Assert.IsTrue( adapter.SaveEntity( newAddress, true, false ) );
					//Assert.IsTrue( adapter.SaveEntity( newCustomer, true, false ) );
					Assert.IsTrue( adapter.SaveEntity( newCustomer, true) );

					Assert.AreEqual( EntityState.Fetched, newAddress.Fields.State );
					Assert.AreEqual( EntityState.Fetched, newCustomer.Fields.State );
					Assert.IsTrue( (((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0) );
					Assert.IsTrue( (((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0) );
					addressId = newAddress.AddressId;
					customerId = newCustomer.CustomerId;
					Assert.IsTrue( (newCustomer.VisitingAddressId == addressId) );
					Assert.AreEqual( newAddress, newCustomer.BillingAddress );
					Assert.IsTrue( (newAddress == newCustomer.BillingAddress) );
					Assert.AreEqual( ((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID );
					Assert.AreEqual( ((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID );

					newCustomer.CompanyEmailAddress += " ";
					newAddress.StreetName += " ";
					using(DataAccessAdapter nestedAdapter = new DataAccessAdapter())
					{
						Assert.IsTrue(adapter.SaveEntity(newCustomer, true));
					}

					Console.WriteLine("AddressID in scope of dataaccessadapter: {0}",newAddress.AddressId);
					Console.WriteLine( "Address participates in transaction in scope of dataaccessadapter: {0}", ((ITransactionalElement)newAddress).ParticipatesInTransaction );
					Console.WriteLine("Address ObjectID in scope of dataaccessadapter: {0}", ((IEntityCore)newAddress).ObjectID);
				}
				Console.WriteLine( "AddressID in scope of transactionscope: {0}", newAddress.AddressId );
				Console.WriteLine( "Address participates in transaction in scope of transactionscope: {0}", ((ITransactionalElement)newAddress).ParticipatesInTransaction );
				Console.WriteLine( "Address ObjectID in scope of transactionscope: {0}", ((IEntityCore)newAddress).ObjectID );

				// do not call Complete, as we want to rollback the transaction and see if the rollback indeed succeeds.
				// ts.Complete();
			}

			// let the DTC and the system.transactions threads deal with the objects. 
			Thread.Sleep( 1000 );

			// test if the data is still there. Shouldn't be as the transaction has been rolled back. 
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				CustomerEntity fetchedCustomer = new CustomerEntity( customerId );
				Assert.IsFalse( adapter.FetchEntity( fetchedCustomer ) );
				AddressEntity fetchedAddress = new AddressEntity( addressId );
				Assert.IsFalse( adapter.FetchEntity( fetchedAddress ) );
			}
		}


		[Test]
		public void TransactionScopeTest()
		{
			string idScopeOne = string.Empty;
			string idScopeTwo = string.Empty;
			string idScopeThree = string.Empty;
			Transaction systemTransaction = null;
			using( TransactionScope scopeOne = new TransactionScope() )
			{
				idScopeOne = Transaction.Current.TransactionInformation.LocalIdentifier;
				systemTransaction = Transaction.Current;
				using( TransactionScope scopeTwo = new TransactionScope(TransactionScopeOption.RequiresNew) )
				{
					if( systemTransaction != Transaction.Current )
					{
						Console.WriteLine("Scope 2 has different transaction object");
					}
					Transaction systemTransaction2 = Transaction.Current;
					idScopeTwo = Transaction.Current.TransactionInformation.LocalIdentifier;
					using( TransactionScope scopeThree = new TransactionScope() )
					{
						if( systemTransaction != Transaction.Current )
						{
							Console.WriteLine( "Scope 3 has different transaction object than scope 1" );
						}
						if( systemTransaction2 != Transaction.Current )
						{
							Console.WriteLine( "Scope 3 has different transaction object than scope 2" );
						}
						idScopeThree = Transaction.Current.TransactionInformation.LocalIdentifier;
					}
				}
			}

			Console.WriteLine(idScopeOne);
			Console.WriteLine(idScopeTwo);
			Console.WriteLine(idScopeThree);
		}


		/// <summary>
		/// Tests the update of a customer when the PK field has been removed. It should throw an exception. 
		/// </summary>
		[Test]
		[ExpectedException(typeof(ORMQueryConstructionException))]
		public void UpdateWithNoPKTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId == addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress == newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				CustomerEntity fetchedCustomer = new CustomerEntity(customerID);
				adapter.FetchEntity(fetchedCustomer);

				fetchedCustomer.Fields.PrimaryKeyFields.Clear();
				Assert.AreEqual(0, fetchedCustomer.Fields.PrimaryKeyFields.Count);
				fetchedCustomer.CompanyEmailAddress += " ";
				fetchedCustomer.Validator = null;
				adapter.SaveEntity(fetchedCustomer);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the insertion of a Customer entity and an VisitingAddress entity. 
		/// </summary>
		[Test]
		public void InsertCustomerTestSimpleWithoutRefetch()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();
			bool markerFlagSave = EntityBase2.MarkSavedEntitiesAsFetched;
			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				EntityBase2.MarkSavedEntitiesAsFetched = true;
				bool result = adapter.SaveEntity(newCustomer);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId == addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress == newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				CustomerEntity fetchedCustomer = new CustomerEntity(customerID);
				adapter.FetchEntity(fetchedCustomer);
			}
			finally
			{
				adapter.Dispose();
				EntityBase2.MarkSavedEntitiesAsFetched = markerFlagSave;
			}
		}



		/// <summary>
		/// Tests the insertion of a Customer entity and an VisitingAddress entity, the customer entity has been extended.
		/// </summary>
		[Test]
		public void InsertCustomerTestWithExtendedFields()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntityExSe newCustomer = new CustomerEntityExSe();
				newCustomer.CompanyName = ConstantsEnums.Customer1CompanyName;
				newCustomer.CustomerSince = new DateTime(2000, 1, 1);
				newCustomer.ContactPerson = ConstantsEnums.Customer1ContactPerson;
				newCustomer.CompanyEmailAddress = ConstantsEnums.Customer1CompanyEmailAddress;
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				OrderEntity o = new OrderEntity();
				o.OrderDate = DateTime.Now;
				newCustomer.Orders.Add(o);
				o.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId == addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress == newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				PrefetchPath2 path = new PrefetchPath2(EntityType.CustomerEntity);
				path.Add(CustomerEntity.PrefetchPathBillingAddress);
				path.Add(CustomerEntity.PrefetchPathVisitingAddress);
				CustomerEntityExSe fetchedCustomer = new CustomerEntityExSe(customerID);
				adapter.FetchEntity(fetchedCustomer, path);
				Assert.AreEqual(1, fetchedCustomer.NumberOfOrders);

				fetchedCustomer.CompanyName += " ";
				adapter.SaveEntity(fetchedCustomer);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		public void InsertCustomerTestSimpleWithSerialization()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				BinaryFormatter formatter = new BinaryFormatter();
				MemoryStream stream = new MemoryStream();
				formatter.Serialize(stream, newCustomer);
				stream.Seek(0, SeekOrigin.Begin);
				CustomerEntity toSave = (CustomerEntity)formatter.Deserialize(stream);
				stream.Close();
				AddressEntity addressSaved = toSave.BillingAddress;

				bool result = adapter.SaveEntity(toSave, true);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, addressSaved.Fields.State);
				Assert.AreEqual(EntityState.Fetched, toSave.Fields.State);
				Assert.IsTrue((((int)addressSaved.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
				Assert.IsTrue((((int)toSave.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
				int addressID = addressSaved.AddressId;
				int customerID = toSave.CustomerId;
				Assert.IsTrue((toSave.VisitingAddressId == addressID));
				Assert.AreEqual(addressSaved, toSave.BillingAddress);
				Assert.IsTrue((addressSaved == toSave.BillingAddress));
				Assert.AreEqual(((IEntityCore)addressSaved).ObjectID, ((IEntityCore)toSave.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)addressSaved).ObjectID, ((IEntityCore)toSave.BillingAddress).ObjectID);

				CustomerEntity fetchedCustomer = new CustomerEntity(customerID);
				adapter.FetchEntity(fetchedCustomer);
			}
			finally
			{
				adapter.Dispose();
			}
		}

		
		/// <summary>
		/// Tests the insertion of a Customer entity and an VisitingAddress entity. Uses a context to see if the saved entities are added and promoted to 
		/// non-new entities after the transaction succeeds.
		/// </summary>
		[Test]
		public void InsertCustomerWithContextTestSimple()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				Context testContext = new Context();
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				testContext.Add(newCustomer);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				testContext.Add(newAddress);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress==newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				CustomerEntity fetchedCustomer = (CustomerEntity)testContext.Get(new CustomerEntityFactory(), customerID);
				Assert.AreEqual(fetchedCustomer, newCustomer);
				Assert.IsFalse(fetchedCustomer.IsNew);
				Assert.IsTrue((fetchedCustomer==newCustomer));
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests field versioning in an entity using simple steps. 
		/// </summary>
		[Test]
		public void FieldsVersioningTestSimple()
		{
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			CustomerEntity customer2 = EntityCreator.CreateNewCustomer(2);

			// save fields
			customer.SaveFields("BeforeUpdate");
			customer.IsNew = false;
			customer2.IsNew = false;

			// update all fields with values from customer2
			for (int i = 0; i < 6; i++)
			{
				customer.Fields[i].CurrentValue = customer2.Fields[i].CurrentValue;
			}

			Assert.AreEqual(customer.CompanyEmailAddress, customer2.CompanyEmailAddress);
			Assert.AreEqual(customer.CompanyName, customer2.CompanyName);
			Assert.AreEqual(customer.ContactPerson, customer2.ContactPerson);
			Assert.AreEqual(customer.CustomerSince, customer2.CustomerSince);
			Assert.AreEqual(customer.VisitingAddressId, customer2.VisitingAddressId);

			// roll back values
			customer.RollbackFields("BeforeUpdate");

			customer2 = EntityCreator.CreateNewCustomer(1);
			customer2.IsNew = false;
			Assert.AreEqual(customer.CompanyEmailAddress, customer2.CompanyEmailAddress);
			Assert.AreEqual(customer.CompanyName, customer2.CompanyName);
			Assert.AreEqual(customer.ContactPerson, customer2.ContactPerson);
			Assert.AreEqual(customer.CustomerSince, customer2.CustomerSince);
			Assert.AreEqual(customer.VisitingAddressId, customer2.VisitingAddressId);
		}


		/// <summary>
		/// Tests the change of a pk value of an entity that is already in the database
		/// </summary>
		[Test]
		public void UpdatePkTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
				newProduct.TestRunId = _testRunID;
				Console.WriteLine("Current Product PK = {0}", newProduct.ProductId.ToString());
				bool result = adapter.SaveEntity(newProduct, true);
				Assert.IsTrue(result);

				// alter PK
				newProduct.ProductId = Guid.NewGuid();
				Console.WriteLine("New Product PK = {0}", newProduct.ProductId.ToString());
				
				// resave the product.
				result = adapter.SaveEntity(newProduct, false);
				Assert.IsTrue(result);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the DbValue feature for optimistic concurrency. It writes a customer object to the db, and 
		/// updates the data locally and controls with a predicate which filters on the DbValue of a field the update of the data.
		/// The update then has to succeed.
		/// </summary>
		[Test]
		public void DbValueFeatureTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// Save customer with refetch.
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.IsTrue(result);

				int customerID = newCustomer.CustomerId;
				Assert.IsTrue(customerID > 0);

				// alter row
				newCustomer.CompanyName = ConstantsEnums.Customer2CompanyName;

				// setup filter: update the rows with the old, company name, which is located in DbValue
				IPredicateExpression filter = new PredicateExpression(CustomerFields.CompanyName == newCustomer.Fields[(int)CustomerFieldIndex.CompanyName].DbValue);

				bool saveResult = adapter.SaveEntity(newCustomer, false, filter);
				Assert.IsTrue(saveResult);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests if setting an entity field's value with the same value is indeed not making the entity dirty/field marked as changed
		/// </summary>
		[Test]
		public void FieldValueChangeSameValueTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.AreEqual(true, result);

				// save has been done ok.
				newCustomer.CompanyName = ConstantsEnums.Customer1CompanyName;
				Assert.AreEqual(false, newCustomer.IsDirty);
				Assert.AreEqual(false, newCustomer.Fields[(int)CustomerFieldIndex.CompanyName].IsChanged);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests the Rollback/Savepoint feature of a transaction.
		/// </summary>
		[Test]
		public void SavepointRollbackTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				adapter.StartTransaction(System.Data.IsolationLevel.ReadCommitted, "SavepointRollbackTestTrans");

				// first save an address
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;

				bool addressSaveResult = adapter.SaveEntity(newAddress, true);
				Assert.IsTrue(addressSaveResult);
				int addressID = newAddress.AddressId;

				// save the transaction
				adapter.SaveTransaction("TestSavepoint");

				// save customer
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool customerSaveResult = adapter.SaveEntity(newCustomer, true);
				Assert.IsTrue(customerSaveResult);

				int customerID = newCustomer.CustomerId;

				// rollback trans to save point.
				adapter.Rollback("TestSavepoint");

				// commit trans.
				adapter.Commit();

				// load back customer, should fail.
				CustomerEntity customerSaved = new CustomerEntity(customerID);
				bool customerFetchResult = adapter.FetchEntity(customerSaved);
				Assert.AreEqual(false, customerFetchResult);
				Assert.AreEqual(EntityState.OutOfSync, customerSaved.Fields.State);

				// load back address, should succeed
				AddressEntity addressSaved = new AddressEntity(addressID);
				adapter.FetchEntity(addressSaved);
				Assert.AreEqual(EntityState.Fetched, addressSaved.Fields.State);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests an expression using update. This is only possible with a direct update of existing entities.
		/// </summary>
		[Test]
		public void UpdateWithExpressionsTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				// save 2 product entities.
				ProductEntity newProduct1 = EntityCreator.CreateNewProduct(1);
				newProduct1.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newProduct1, true);
				Assert.IsTrue(result);
				ProductEntity newProduct2 = EntityCreator.CreateNewProduct(2);
				newProduct2.TestRunId = _testRunID;
				result = adapter.SaveEntity(newProduct2, true);
				Assert.IsTrue(result);

				ProductEntity updateProduct = new ProductEntity();
				IExpression updateExpression = ProductFields.Price * 4;

				updateProduct.Fields[(int)ProductFieldIndex.Price].ExpressionToApply = updateExpression;
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				bucket.PredicateExpression.Add(new FieldCompareRangePredicate(ProductFields.ProductId, null,newProduct1.ProductId, newProduct2.ProductId));
				int rowsAffected = adapter.UpdateEntitiesDirectly(updateProduct, bucket);
				Assert.AreEqual(2, rowsAffected);

				// refetch entities
				ProductEntity product1 = new ProductEntity();
				product1.ProductId = newProduct1.ProductId;
				adapter.FetchEntity(product1);
				ProductEntity product2 = new ProductEntity();
				product2.ProductId = newProduct2.ProductId;
				adapter.FetchEntity(product2);

				Assert.AreEqual((newProduct1.Price * 4), product1.Price);
				Assert.AreEqual((newProduct2.Price * 4), product2.Price);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests if the reference to a related entity is reset (to null) when the fk field this relation is build upon is
		/// set to a different value.
		/// </summary>
		[Test]
		public void FkFieldRelatedEntityResetTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				bool result = adapter.SaveEntity(newCustomer, true);
				Assert.AreEqual(true, result);
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress==newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				// set FK field to same value. Nothing should happen
				newCustomer.VisitingAddressId = addressID;
				Assert.AreEqual(newCustomer.VisitingAddress, newAddress);

				// set FK field to different value. Reference should be reset
				newCustomer.VisitingAddressId = addressID+1;
				Assert.IsNull(newCustomer.VisitingAddress);
				Assert.IsNull( newAddress.CustomerVisitingAddress );
				// shouldn't reset other reference to same object.
				Assert.AreEqual(newCustomer.BillingAddress, newAddress);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		/// <summary>
		/// Tests whether FK fields are rolled back properly, if the transaction rolls back in the middle of the work. 
		/// </summary>
		[Test]
		public void FkRollbackTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer( 1 );
			try
			{
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress( 1 );
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				// billing address is null so will cause a rollback. 
				// clear validator, which is set automatically
				newCustomer.Validator = null;

				Assert.IsNull(newCustomer.Fields[(int)CustomerFieldIndex.VisitingAddressId].CurrentValue);
				Assert.IsFalse(adapter.SaveEntity(newAddress, true, true));
				throw new Exception( "Shouldn't reach this line" );
			}
			catch( ORMQueryExecutionException )
			{
				// roll back transaction
				adapter.Rollback();
			}
			finally
			{
				adapter.Dispose();
			}

			// this value should be null, as the value should have been rolled back completely.
			Assert.IsNull( newCustomer.Fields[(int)CustomerFieldIndex.VisitingAddressId].CurrentValue );
		}


		/// <summary>
		/// Tests the UnitOfWork class. 
		/// </summary>
		[Test]
		public void UnitOfWorkTest()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
				newCustomer.TestRunId = _testRunID;
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				newCustomer.VisitingAddress = newAddress;
				newCustomer.BillingAddress = newAddress;
				UnitOfWork2 uow = new UnitOfWork2();
				bool addResult = uow.AddForSave(newCustomer, true);
				Assert.IsTrue(addResult);

				// create a product
				ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
				newProduct.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newProduct, true);
				Assert.IsTrue(result);
				Guid prodID = newProduct.ProductId;

				addResult = uow.AddForDelete(newProduct);
				Assert.IsTrue(addResult);

				// test pre-commit state.
				Assert.IsTrue(newCustomer.IsNew);
				Assert.IsTrue(newAddress.IsNew);
				Assert.AreEqual(EntityState.Fetched, newProduct.Fields.State);

				// commit
				uow.Commit(adapter, true);

				// test save actions of UOW.
				Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
				Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
				Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
				Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
				int addressID = newAddress.AddressId;
				int customerID = newCustomer.CustomerId;
				Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
				Assert.AreEqual(newAddress, newCustomer.BillingAddress);
				Assert.IsTrue((newAddress==newCustomer.BillingAddress));
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
				Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.BillingAddress).ObjectID);

				// test delete actions of UOW:
				ProductEntity savedProduct = new ProductEntity(prodID);
				adapter.FetchEntity(savedProduct);
				Assert.IsTrue(savedProduct.IsNew);
				Assert.AreEqual(EntityState.Deleted, newProduct.Fields.State);
			}
			finally
			{
				adapter.Dispose();
			}
		}

		
		/// <summary>
		/// Tests the UnitOfWork class and the re-ordering of work. It first inserts an entity which is inserted again in the unit of work.
		/// If the delete isn't scheduled before the insert, the unitofwork will fail.
		/// </summary>
		[Test]
		public void UnitOfWorkTestWithReorder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ProductEntity p = EntityCreator.CreateNewProduct(1);
				Guid productId = p.ProductId;
				p.TestRunId = _testRunID;
				Assert.IsTrue(adapter.SaveEntity(p));

				List<UnitOfWorkBlockType> commitOrder = new List<UnitOfWorkBlockType>();
				commitOrder.Add(UnitOfWorkBlockType.Deletes);
				commitOrder.Add(UnitOfWorkBlockType.Inserts);

				UnitOfWork2 uow = new UnitOfWork2(commitOrder);
				uow.AddForDelete(p);

				ProductEntity secondTry = EntityCreator.CreateNewProduct(1);
				secondTry.ProductId = productId;
				secondTry.TestRunId = _testRunID;
				uow.AddForSave(secondTry);

				// commit
				int result = uow.Commit(adapter, true);
				Assert.AreEqual(2, result);

				Assert.AreEqual(EntityState.Deleted, p.Fields.State);
			}
		}


		/// <summary>
		/// Tests if the unitofwork can be serialized/deserialized when delegates are stored inside itself .
		/// </summary>
		[Test]
		public void UnitOfWorkSerializingTest()
		{
			UnitOfWork2 uow = new UnitOfWork2();
			uow.AddCallBack(new ActionProcedures.ClearTestRunDataCallBack(ActionProcedures.ClearTestRunData), UnitOfWorkCallBackScheduleSlot.PreEntityDelete, true, new object[1] {_testRunID});

			MemoryStream memstream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();

			// serialize the uow to the memstream
			formatter.Serialize(memstream, uow);

			// deserialize the uow back.
			memstream.Seek(0, SeekOrigin.Begin);
			UnitOfWork2 deserializedUoW = (UnitOfWork2)formatter.Deserialize(memstream);
			memstream.Close();

			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			using (DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(newAddress));

				deserializedUoW.Commit(adapter, true);

				AddressEntity toFetch = new AddressEntity(newAddress.AddressId);
				Assert.IsFalse(adapter.FetchEntity(toFetch));
			}
		}


		/// <summary>
		/// Tests the unit of work callback system for stored procedures. It first inserts an entity, then calls the procedure to remove it, then
		/// checks if the entity is indeed removed.
		/// </summary>
		[Test]
		public void UnitOfWorkCallbackTest()
		{
			UnitOfWork2 uow = new UnitOfWork2();
			uow.AddCallBack(new ActionProcedures.ClearTestRunDataCallBack(ActionProcedures.ClearTestRunData), UnitOfWorkCallBackScheduleSlot.PreEntityDelete, true, _testRunID);

			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			using (DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(newAddress));

				uow.Commit(adapter, true);

				AddressEntity toFetch = new AddressEntity(newAddress.AddressId);
				Assert.IsFalse(adapter.FetchEntity(toFetch));
			}
		}


		/// <summary>
		/// Tests the UnitOfWork class using collections being added to the unitofwork.
		/// </summary>
		[Test]
		public void UnitOfWorkTestCollections()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection addresses = new EntityCollection(new AddressEntityFactory());
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(2);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(3);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(4);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);

				UnitOfWork2 uow = new UnitOfWork2();
				uow.AddCollectionForSave(addresses, true, false); 

				// create a product
				ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
				newProduct.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newProduct, true);
				Assert.IsTrue(result);
				Guid prodID = newProduct.ProductId;

				bool addResult = uow.AddForDelete(newProduct);
				Assert.IsTrue(addResult);

				// test pre-commit state.
				for (int i = 0; i < addresses.Count; i++)
				{
					Assert.IsTrue(addresses[i].IsNew);
				}
				Assert.AreEqual(EntityState.Fetched, newProduct.Fields.State);

				// commit
				uow.Commit(adapter, true);

				// test save actions of UOW.
				for (int i = 0; i < addresses.Count; i++)
				{
					Assert.AreEqual(EntityState.Fetched, addresses[i].Fields.State);
					Assert.IsTrue((((int)addresses[i].Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
				}

				// test delete actions of UOW:
				ProductEntity savedProduct = new ProductEntity(prodID);
				adapter.FetchEntity(savedProduct);
				Assert.IsTrue(savedProduct.IsNew);
				Assert.AreEqual(EntityState.Deleted, newProduct.Fields.State);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[Test]
		public void UnitOfWorkTestCollectionsWithSerialization()
		{
			DataAccessAdapter adapter = new DataAccessAdapter();

			try
			{
				EntityCollection addresses = new EntityCollection(new AddressEntityFactory());
				AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(2);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(3);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);
				newAddress = EntityCreator.CreateNewAddress(4);
				newAddress.TestRunId = _testRunID;
				addresses.Add(newAddress);

				UnitOfWork2 uow = new UnitOfWork2();
				uow.AddCollectionForSave(addresses, true, false);

				// create a product
				ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
				newProduct.TestRunId = _testRunID;
				bool result = adapter.SaveEntity(newProduct, true);
				Assert.IsTrue(result);
				Guid prodID = newProduct.ProductId;

				bool addResult = uow.AddForDelete(newProduct);
				Assert.IsTrue(addResult);

				// test pre-commit state.
				for(int i = 0; i < addresses.Count; i++)
				{
					Assert.IsTrue(addresses[i].IsNew);
				}
				Assert.AreEqual(EntityState.Fetched, newProduct.Fields.State);

				SerializationHelper.Optimization = SerializationOptimization.Fast;
				MemoryStream stream = new MemoryStream();
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, uow);

				stream.Seek(0, SeekOrigin.Begin);
				UnitOfWork2 uowD = (UnitOfWork2)formatter.Deserialize(stream);
				stream.Close();

				IEntityCollection2 addressesD = uowD.GetCollectionElementsToSave()[0].Collection;

				// commit
				uowD.Commit(adapter, true);


				// test save actions of UOW.
				for(int i = 0; i < addressesD.Count; i++)
				{
					Assert.AreEqual(EntityState.Fetched, addressesD[i].Fields.State);
					Assert.IsTrue((((int)addressesD[i].Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
				}

				// test delete actions of UOW:
				ProductEntity savedProduct = new ProductEntity(prodID);
				adapter.FetchEntity(savedProduct);
				Assert.IsTrue(savedProduct.IsNew);
			}
			finally
			{
				adapter.Dispose();
			}
		}


		[TestFixtureTearDown]
		public void CleanUp()
		{
			// remove all data for this testrun
			ActionProcedures.ClearTestRunData(_testRunID);
			//ActionProcedures.ClearAll();
		}
	}


	/// <summary>
	/// Auditor class for all entities in write unit tests. 
	/// </summary>
	[DependencyInjectionInfo(typeof(IEntity2), "AuditorToUse", TargetNamespaceFilter="UnitTests.Dal.Adapter")]
	[Serializable]
	public class GeneralAuditor : AuditorBase
	{
		private enum AuditType
		{
			DeleteOfEntity=1,
			DirectDeleteOfEntities,
			DirectUpdateOfEntities,
			DereferenceOfRelatedEntity,
			ReferenceOfRelatedEntity,
			EntityFieldSet,
			InsertOfNewEntity,
			UpdateOfExistingEntity
		}

		private List<AuditInfoEntity> _auditInfoEntities;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneralAuditor"/> class.
		/// </summary>
		public GeneralAuditor()
		{
			_auditInfoEntities = new List<AuditInfoEntity>();
		}


		/// <summary>
		/// Audits the successful delete of an entity from the database
		/// </summary>
		/// <param name="entity">The entity which was deleted.</param>
		/// <remarks>As the entity passed in was deleted succesfully, reading values from the passed in entity is only possible in this routine. After this call, the
		/// state of the entity will be reset to Deleted again and reading the fields will result in an exception. It's also recommended not to reference
		/// the passed in entity in any audit entity you might want to persist as the entity doesn't exist anymore in the database.</remarks>
		public override void AuditDeleteOfEntity(IEntityCore entity)
		{
			AuditInfoEntity auditInfo = new AuditInfoEntity();
			auditInfo.AffectedEntityName = entity.LLBLGenProEntityName;
			auditInfo.ActionDateTime = DateTime.Now;
			auditInfo.ActionType = (int)AuditType.DeleteOfEntity;
			_auditInfoEntities.Add(auditInfo);
		}

		/// <summary>
		/// Audits the successful direct delete of entities in the database
		/// </summary>
		/// <param name="typeOfEntity">The type of entity of which entities were deleted.</param>
		/// <param name="filter">The filter to filter out the entities to delete. Can be null and can be an IPredicateExpression.</param>
		/// <param name="relations">The relations to use with the filter. Can be null.</param>
		/// <param name="numberOfEntitiesDeleted">The number of entities deleted.</param>
		public override void AuditDirectDeleteOfEntities(Type typeOfEntity, IPredicate filter, IRelationCollection relations, int numberOfEntitiesDeleted)
		{
			//Console.WriteLine("AuditType: DirectDeleteOfEntities. EntityType: {0}.", typeOfEntity.FullName);
		}

		/// <summary>
		/// Audits the successful dereference of related entity from the entity passed in.
		/// </summary>
		/// <param name="entity">The entity of which the related entity was dereferenced from.</param>
		/// <param name="relatedEntity">The related entity which was dereferenced from entity</param>
		/// <param name="mappedFieldName">Name of the mapped field onto the relation from entity to related entity for which the related entity was dereferenced.</param>
		public override void AuditDereferenceOfRelatedEntity(IEntityCore entity, IEntityCore relatedEntity, string mappedFieldName)
		{
			AuditInfoEntity auditInfo = new AuditInfoEntity();
			auditInfo.AffectedEntityName = entity.LLBLGenProEntityName;
			auditInfo.ActionDateTime = DateTime.Now;
			auditInfo.ActionType = (int)AuditType.DereferenceOfRelatedEntity;
			auditInfo.ActionData = string.Format("RelatedEntityName: {0}. MappedFieldName: {1}", relatedEntity.LLBLGenProEntityName, mappedFieldName);
			_auditInfoEntities.Add(auditInfo);
		}

		/// <summary>
		/// Audits the succesful direct update of entities in the database.
		/// </summary>
		/// <param name="entity">The entity with the changed values which is used to produce the update query.</param>
		/// <param name="filter">The filter to filter out the entities to update. Can be null and can be an IPredicateExpression.</param>
		/// <param name="relations">The relations to use with the filter. Can be null.</param>
		/// <param name="numberOfEntitiesUpdated">The number of entities updated.</param>
		public override void AuditDirectUpdateOfEntities(IEntityCore entity, IPredicate filter, IRelationCollection relations, int numberOfEntitiesUpdated)
		{
			//Console.WriteLine("AuditType: DirectUpdateOfEntities. EntityName: {0}. Number of entities updated: {1}", entity.LLBLGenProEntityName, numberOfEntitiesUpdated);
		}

		/// <summary>
		/// Audits when an entity field is set succesfully to a new value.
		/// </summary>
		/// <param name="entity">The entity a field was set to a new value.</param>
		/// <param name="fieldIndex">Index of the field which got a new value.</param>
		/// <param name="originalValue">The original value of the field with the index passed in before it received a new value.</param>
		public override void AuditEntityFieldSet(IEntityCore entity, int fieldIndex, object originalValue)
		{
			string originalValueAsString = string.Empty;
			string currentValue = string.Empty;
			if(originalValue != null)
			{
				originalValueAsString = originalValue.ToString();
			}
			else
			{
				originalValueAsString = "NULL";
			}
			if(entity.GetCurrentFieldValue(fieldIndex) != null)
			{
				currentValue = entity.GetCurrentFieldValue(fieldIndex).ToString();
			}
			else
			{
				currentValue = "NULL";
			}
			//Console.WriteLine("AuditType: EntityFieldSet. EntityName: {0}. FieldIndex: {1}. OriginalValue {2}. CurrentValue: {3}", entity.LLBLGenProEntityName,
			//		fieldIndex, originalValueAsString, currentValue);
		}

		/// <summary>
		/// Audits the successful insert of a new entity into the database.
		/// </summary>
		/// <param name="entity">The entity saved successfully into the database.</param>
		public override void AuditInsertOfNewEntity(IEntityCore entity)
		{
			AuditInfoEntity auditInfo = new AuditInfoEntity();
			auditInfo.AffectedEntityName = entity.LLBLGenProEntityName;
			auditInfo.ActionDateTime = DateTime.Now;
			auditInfo.ActionType = (int)AuditType.InsertOfNewEntity;
			_auditInfoEntities.Add(auditInfo);
		}

		/// <summary>
		/// Audits the successful reference of related entity from the entity passed in.
		/// </summary>
		/// <param name="entity">The entity of which the related entity was dereferenced from.</param>
		/// <param name="relatedEntity">The related entity which was dereferenced from entity</param>
		/// <param name="mappedFieldName">Name of the mapped field onto the relation from entity to related entity for which the related entity was referenced.</param>
		public override void AuditReferenceOfRelatedEntity(IEntityCore entity, IEntityCore relatedEntity, string mappedFieldName)
		{
			AuditInfoEntity auditInfo = new AuditInfoEntity();
			auditInfo.AffectedEntityName = entity.LLBLGenProEntityName;
			auditInfo.ActionDateTime = DateTime.Now;
			auditInfo.ActionType = (int)AuditType.ReferenceOfRelatedEntity;
			auditInfo.ActionData = string.Format("RelatedEntityName: {0}. MappedFieldName: {1}", relatedEntity.LLBLGenProEntityName, mappedFieldName);
			_auditInfoEntities.Add(auditInfo);
		}

		/// <summary>
		/// Audits the successful update of an existing entity in the database
		/// </summary>
		/// <param name="entity">The entity updated successfully in the database.</param>
		public override void AuditUpdateOfExistingEntity(IEntityCore entity)
		{
			AuditInfoEntity auditInfo = new AuditInfoEntity();
			auditInfo.AffectedEntityName = entity.LLBLGenProEntityName;
			auditInfo.ActionDateTime = DateTime.Now;
			auditInfo.ActionType = (int)AuditType.UpdateOfExistingEntity;
			_auditInfoEntities.Add(auditInfo);
		}

		/// <summary>
		/// Gets the audit entities to save. Audit entities contain the audit information stored inside this auditor.
		/// </summary>
		/// <returns>The list of audit entities to save, or null if there are no audit entities to save</returns>
		/// <remarks>Do not remove the audit entities and audit information from this auditor when this method is called, as the transaction in which
		/// the save takes place can fail and retried which will result in another call to this method</remarks>
		public override System.Collections.IList GetAuditEntitiesToSave()
		{
			return _auditInfoEntities;
		}


		/// <summary>
		/// The transaction with which the audit entities requested from GetAuditEntitiesToSave were saved.
		/// Use this method to clear any audit data in this auditor as all audit information is persisted successfully.
		/// </summary>
		public override void TransactionCommitted()
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>> Transaction Commit <<<<<<<<<<<<<<<<<<<<<<<<");
			_auditInfoEntities.Clear();
		}
	}


	public class ExtendedCustomerSeEntityFactory : CustomerEntityFactory
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


	public class CustomerEntityExSe : CustomerEntity
	{
		public CustomerEntityExSe() : base() { }

		public CustomerEntityExSe(int customerID)
			: base(customerID)
		{
		}

		protected override IEntityFactory2 CreateEntityFactory()
		{
			return new ExtendedCustomerSeEntityFactory();
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
