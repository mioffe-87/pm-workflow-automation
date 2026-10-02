using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

using NUnit.Framework;
using UnitTests.Dal.SelfServicing.DaoClasses;
using UnitTests.Dal.SelfServicing.StoredProcedureCallerClasses;
using UnitTests.Dal.SelfServicing.EntityClasses;
using UnitTests.Dal.SelfServicing.HelperClasses;
using UnitTests.Dal.SelfServicing;
using UnitTests.Dal.SelfServicing.FactoryClasses;
using UnitTests.Dal.SelfServicing.CollectionClasses;

//using UnitTests.Dal.SelfServicing.VB.StoredProcedureCallerClasses;
//using UnitTests.Dal.SelfServicing.VB.EntityClasses;
//using UnitTests.Dal.SelfServicing.VB.HelperClasses;
//using UnitTests.Dal.SelfServicing.VB;
//using UnitTests.Dal.SelfServicing.VB.FactoryClasses;
//using UnitTests.Dal.SelfServicing.VB.CollectionClasses;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Runtime.Serialization.Formatters.Binary;
using System.Transactions;
using UnitTestTransaction = UnitTests.Dal.SelfServicing.HelperClasses.Transaction;
using System.Threading;
using System.Collections.Generic;

namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
{
	/// <summary>
	/// SingleEntityTests.
	/// Contains NUnit tests working on single entity objects.
	/// </summary>
	[TestFixture]
	public class SingleEntityTests
	{
		private Guid _testRunID;
		private bool _eventHandlerVisited;

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
			_eventHandlerVisited = true;
		}


		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid();
			Console.WriteLine("TestRunID: {0}", _testRunID.ToString());
		}


		[Test]
		public void FindMatchesWrongIndexTest()
		{
			// Necessary related entities.
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			EmployeeEntity newEmployee = new EmployeeEntity();
			newEmployee.Name = "John Doe";
			newEmployee.EmployeeSince = DateTime.Now;
			newEmployee.TestRunId = _testRunID;

			OrderCollection orders = new OrderCollection();
			OrderEntity newOrder = new OrderEntity();
			newOrder.Employee = newEmployee;
			newOrder.Customer = newCustomer;
			newOrder.OrderDate = DateTime.Now;
			newOrder.ShippedDate = DateTime.Now;
			newOrder.TestRunId = _testRunID;
			orders.Add(newOrder);

			newOrder = new OrderEntity();
			newOrder.Employee = newEmployee;
			newOrder.Customer = newCustomer;
			newOrder.OrderDate = DateTime.Now;
			newOrder.TestRunId = _testRunID;
			orders.Add(newOrder);

			Assert.AreEqual(5, orders.SaveMulti(true));

			OrderCollection fetchedOrders = new OrderCollection();
			fetchedOrders.GetMulti(OrderFields.CustomerId == newCustomer.CustomerId, 0, new SortExpression(OrderFields.OrderId | SortOperator.Ascending));
			Assert.AreEqual(2, fetchedOrders.Count);

			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareNullPredicate(OrderFields.ShippedDate));
			filter.AddWithAnd(new FieldCompareNullPredicate(OrderFields.RequiredDate));

			List<int> indexes = fetchedOrders.FindMatches(filter);
			Assert.AreEqual(1, indexes.Count);
			Assert.AreEqual(1, indexes[0]);

			fetchedOrders[1].ShippedDate = DateTime.Now;
			newOrder = new OrderEntity();
			newOrder.Employee = newEmployee;
			newOrder.Customer = newCustomer;
			newOrder.OrderDate = DateTime.Now;
			newOrder.TestRunId = _testRunID;
			fetchedOrders.Add(newOrder);

			indexes = fetchedOrders.FindMatches(filter);
			Assert.AreEqual(1, indexes.Count);
			Assert.AreEqual(2, indexes[0]);

			Assert.AreEqual(2, fetchedOrders.SaveMulti(true));
			filter = new PredicateExpression();
			filter.Add(new DelegatePredicate(delegate(IEntityCore toExamine) { return !((OrderEntity)toExamine).ShippedDate.HasValue; }));
			filter.Add(new DelegatePredicate(delegate(IEntityCore toExamine) { return !((OrderEntity)toExamine).RequiredDate.HasValue; }));

			Assert.AreEqual(1, indexes.Count);
			Assert.AreEqual(2, indexes[0]);

			Assert.AreEqual(3, fetchedOrders.DeleteMulti());
			Assert.IsTrue(newEmployee.Delete());
			Assert.IsTrue(newCustomer.Delete());
			Assert.IsTrue(newAddress.Delete());
		}


		[Test]
		public void FindMatchesAfterSaveWithoutRefetchTest()
		{
			bool flagSave = EntityBase.MarkSavedEntitiesAsFetched;
			try
			{
				EntityBase.MarkSavedEntitiesAsFetched = true;
				AddressCollection addresses = new AddressCollection();
				addresses.Add(EntityCreator.CreateNewAddress(4));
				addresses[0].TestRunId = _testRunID;
				Assert.AreEqual(1, addresses.SaveMulti());
				string newZip = "1234AA";
				addresses[0].Zipcode = newZip;
				Assert.AreEqual(1, addresses.SaveMulti());
				List<int> indexes = addresses.FindMatches(AddressFields.Zipcode == newZip);
				Assert.AreEqual(1, indexes.Count);
				Assert.AreEqual(0, indexes[0]);
			}
			finally
			{
				EntityBase.MarkSavedEntitiesAsFetched = flagSave;
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

			Assert.IsTrue(mainData.Save());
			Assert.IsTrue(mainData.Id > 0);
			splitOff.MainDataId = mainData.Id;
			Assert.IsTrue(splitOff.Save());

			// fetch both in new entities
			SplitOffNoBlobDataEntity mainDataFetched = new SplitOffNoBlobDataEntity(mainData.Id);
			Assert.AreEqual(EntityState.Fetched, mainDataFetched.Fields.State);
			Assert.AreEqual(mainData.FirstName, mainDataFetched.FirstName);
			Assert.AreEqual(mainData.LastName, mainDataFetched.LastName);
			SplitOffBlobDataEntity splitOffFetched = new SplitOffBlobDataEntity(mainData.Id);
			Assert.AreEqual(EntityState.Fetched, splitOffFetched.Fields.State);
			Assert.AreEqual(splitOff.Notes, splitOffFetched.Notes);
			Assert.AreEqual(3, splitOffFetched.Photo.Length);

			// just delete 1, no need to delete two.
			Assert.IsTrue(mainData.Delete());
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

			Assert.IsTrue(mainData.Save(true));
			Assert.IsTrue(mainData.Id > 0);
			Assert.AreEqual(mainData.Id, splitOff.MainDataId);

			// fetch both in new entities using prefetch path
			PrefetchPath path = new PrefetchPath(EntityType.SplitOffNoBlobDataEntity);
			path.Add(SplitOffNoBlobDataEntity.PrefetchPathBlobData);
			SplitOffNoBlobDataEntity mainDataFetched = new SplitOffNoBlobDataEntity();
			Assert.IsTrue(mainDataFetched.FetchUsingPK(mainData.Id, path));
			Assert.AreEqual(EntityState.Fetched, mainDataFetched.Fields.State);
			Assert.AreEqual(mainData.FirstName, mainDataFetched.FirstName);
			Assert.AreEqual(mainData.LastName, mainDataFetched.LastName);
			SplitOffBlobDataEntity splitOffFetched = mainDataFetched.BlobData;
			Assert.AreEqual(EntityState.Fetched, splitOffFetched.Fields.State);
			Assert.AreEqual(splitOff.Notes, splitOffFetched.Notes);
			Assert.AreEqual(3, splitOffFetched.Photo.Length);

			// just delete 1, no need to delete two.
			Assert.IsTrue(mainData.Delete());
		}


        
		[Test]
		public void EnumSaveTest()
		{
			EnumTesterEntity et = new EnumTesterEntity();
			et.EnumField = ConsoleColor.Blue;

			Assert.IsTrue(et.Save());
			Assert.IsTrue(et.Id > 0);
			Assert.IsTrue(et.Delete());
		}

		[Test]
		public void EnumFetchTest()
		{
			EnumTesterCollection ets = new EnumTesterCollection();
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Black });
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Blue, NullableEnumField = ConsoleColor.Red });
			ets.Add(new EnumTesterEntity() { EnumField = ConsoleColor.Blue, NullableEnumField = ConsoleColor.Red });
			Assert.AreEqual(3, ets.SaveMulti());
			foreach(var et in ets)
			{
				Assert.IsTrue(et.Id > 0);
			}

			// fetch blues back
			var blues = new EnumTesterCollection();
			blues.GetMulti(EnumTesterFields.EnumField == ConsoleColor.Blue);
			try
			{
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
				Assert.AreEqual(3, ets.DeleteMulti());
			}
		}


		[Test]
		public void OverflowInPrecisionScaleTest()
		{
			EntityBase.BuildInValidationBypassMode = BuildInValidationBypass.NoBypass;

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
			EntityBase.BuildInValidationBypassMode = BuildInValidationBypass.AlwaysBypass;
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
		public void SingleSidedHiddenRelationSaveSyncTest()
		{
			CustomerSyncTestEntity newCustomer = EntityCreator.CreateNewCustomerSyncTest( 1 );
			newCustomer.TestRunId = _testRunID;
			AddressSyncTestEntity newAddress = EntityCreator.CreateNewAddressSyncTest( 1 );
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;

			Assert.IsTrue( newCustomer.Save(true));

			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, newCustomer.Fields.State);
			Assert.IsTrue( (((int)newAddress.Fields[(int)AddressSyncTestFieldIndex.AddressId].CurrentValue) > 0) );
			Assert.IsTrue( (((int)newCustomer.Fields[(int)CustomerSyncTestFieldIndex.CustomerId].CurrentValue) > 0) );
			int addressID = newAddress.AddressId;
			Assert.AreEqual( EntityState.Fetched, newAddress.Fields.State );
			int customerID = newCustomer.CustomerId;
			Assert.AreEqual( EntityState.Fetched, newCustomer.Fields.State );
			Assert.IsTrue( (newCustomer.VisitingAddressId == addressID) );
			Assert.AreEqual( newAddress, newCustomer.VisitingAddress );
			Assert.IsTrue( (newAddress == newCustomer.VisitingAddress) );
			Assert.AreEqual( ((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID );
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

			Assert.IsTrue( newOrder.Save( true ) );
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

			Assert.IsTrue( newCompany.Save( true ) );
			CompanyEntity fetchedCompany = new CompanyEntity( companyId );
			Assert.IsFalse( fetchedCompany.CompanyProperty[0].IsDirty );
		}


		[Test]
		public void PhantomInsertPreventionTest()
		{
			CustomerEntity newCustomer = new CustomerEntity();
			AddressEntity ba = new AddressEntity();
			AddressEntity va = new AddressEntity();
			newCustomer.VisitingAddress = va;
			newCustomer.BillingAddress = ba;

			// as everything is empty, and Address isn't saved because it's not dirty, customer shouldn't be saved as well.
			Assert.IsTrue( newCustomer.Save( true ) );
			Assert.AreEqual( EntityState.New, newCustomer.Fields.State );
			Assert.AreEqual( EntityState.New, ba.Fields.State );
			Assert.AreEqual( EntityState.New, va.Fields.State );
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

			customer.Save(true);
			CustomerCollection customers = new CustomerCollection();
			customers.DeleteMulti(CustomerFields.CustomerId==customer.CustomerId);
			AddressCollection addresses = new AddressCollection();
			addresses.DeleteMulti(AddressFields.AddressId==newAddress.AddressId);
		}


		[Test]
		public void SaveUpdateTargetPerEntityHierarchyGuid()
		{
			Guid pk = Guid.NewGuid();
			DerivedType1Entity d1 = new DerivedType1Entity();
			d1.TestrunId = _testRunID;
			d1.Id = pk;
			d1.Element2 = "Teststring";
			Assert.IsTrue( d1.Save() );

			DerivedType1Entity fetchedD1 = new DerivedType1Entity(pk);
			Assert.AreEqual( EntityState.Fetched, fetchedD1.Fields.State);

			fetchedD1.Element2 += "Changed";
			Assert.IsTrue(fetchedD1.Save());
		}


		[Test]
		public void SaveUpdateSpecialProduct()
		{
			try
			{
				ProductEntity p = EntityCreator.CreateNewProduct(1);
				SpecialProductEntity sp = new SpecialProductEntity();
				sp.FullDescription = p.FullDescription;
				sp.ShortDescription = p.ShortDescription;
				sp.Price = p.Price;
				sp.ActionCode = 1;
				sp.ActionDate = DateTime.Now;
				sp.TestRunId = _testRunID;
				sp.SpTestRunId = _testRunID;
				sp.ProductId = p.ProductId;
				Assert.IsTrue(sp.Save());

				SpecialProductEntity fetchedSP = new SpecialProductEntity(sp.ProductId);
				Assert.AreEqual(EntityState.Fetched, fetchedSP.Fields.State);

				fetchedSP.FullDescription += "lalala";
				Assert.IsTrue(fetchedSP.Save());
			}
			catch(ORMQueryExecutionException ex)
			{
				Console.WriteLine("#########################################################################");
				Console.WriteLine(ex.QueryExecuted);
				Console.WriteLine("#########################################################################");
				throw;
			}
		}
		

		[Test]
		public void SaveUpdateProduct()
		{
			ProductEntity p = EntityCreator.CreateNewProduct(1);
			p.TestRunId = _testRunID;
			Assert.IsTrue(p.Save());

			ProductEntity fetchedP = new ProductEntity(p.ProductId);
			Assert.AreEqual(EntityState.Fetched, fetchedP.Fields.State);

			fetchedP.FullDescription += "lalala";
			Assert.IsTrue(fetchedP.Save());
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

			Assert.IsTrue(customer.Save(true));
			// now alter customer and formulate an update restriction, which makes the update to fail.
			customer.CustomerSince = DateTime.Now;
			Assert.IsFalse(customer.Save((CustomerFields.CustomerSince<new DateTime(2000, 1, 1))));
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

			Assert.IsTrue(customer.Save(true));
			try
			{
				Assert.IsFalse(customer.Delete((CustomerFields.CustomerId != customer.CustomerId)));
			}
			catch(ORMConcurrencyException)
			{
				// expected
			}
		}


		/// <summary>
		/// Tests the insertion of an Address entity. No fancy stuff, just a single insert.
		/// </summary>
		[Test]
		public void InsertAddressTestSimple()
		{
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			bool result = newAddress.Save();
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			int addressID = newAddress.AddressId;
			Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
		}

		[Test]
		public void NullableDateTimeTest()
		{
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			OrderEntity newOrder = new OrderEntity();
			newOrder.Customer = newCustomer;
			newOrder.OrderDate = DateTime.Now;
			newOrder.RequiredDate = DateTime.Now;
			newOrder.TestRunId = _testRunID;

			Assert.IsTrue(newOrder.Save(true));

			OrderEntity fetchedOrder = new OrderEntity(newOrder.OrderId);
			Assert.IsTrue(fetchedOrder.SetNewFieldValue("RequiredDate", null));
			Assert.IsTrue(fetchedOrder.Save());
			OrderEntity fetchedOrder2 = new OrderEntity(newOrder.OrderId);
			Assert.IsTrue(fetchedOrder2.TestOriginalFieldValueForNull(OrderFieldIndex.RequiredDate));
			Assert.IsNull(fetchedOrder2.Fields["RequiredDate"].DbValue);
			Assert.IsNull(fetchedOrder2.Fields["RequiredDate"].CurrentValue);
			Assert.IsNull( fetchedOrder2.RequiredDate );
		}

		/// <summary>
		/// Tests the insertion of a Customer entity and an VisitingAddress entity. This is done using a System.UnitTestTransaction transaction scope. 
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
				using( UnitTestTransaction trans = new UnitTestTransaction(System.Data.IsolationLevel.ReadCommitted, "Test") )
				{
					newCustomer.TestRunId = _testRunID;
					newAddress.TestRunId = _testRunID;
					newCustomer.VisitingAddress = newAddress;
					newCustomer.BillingAddress = newAddress;
					trans.Add( newCustomer );
					trans.Add( newAddress );
					bool result = newCustomer.Save( true );
					Assert.AreEqual( true, result );
					Assert.AreEqual( EntityState.OutOfSync, newAddress.Fields.State );
					Assert.AreEqual( EntityState.OutOfSync, newCustomer.Fields.State );
					Assert.IsTrue( (((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0) );
					Assert.IsTrue( (((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0) );
					addressId = newAddress.AddressId;
					Assert.AreEqual( EntityState.Fetched, newAddress.Fields.State );
					customerId = newCustomer.CustomerId;
					Assert.AreEqual( EntityState.Fetched, newCustomer.Fields.State );
					Assert.IsTrue( (newCustomer.VisitingAddressId == addressId) );
					Assert.AreEqual( newAddress, newCustomer.VisitingAddress );
					Assert.IsTrue( (newAddress == newCustomer.VisitingAddress) );
					Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);

				}

				// do not call Complete, as we want to rollback the transaction and see if the rollback indeed succeeds.
				// ts.Complete();
			}

			// let the DTC and the system.transactions threads deal with the objects. 
			Thread.Sleep( 1000 );

			// test if the data is still there. Shouldn't be as the transaction has been rolled back. 
			CustomerEntity fetchedCustomer = new CustomerEntity( customerId );
			Assert.AreEqual( EntityState.New, fetchedCustomer.Fields.State );
			AddressEntity fetchedAddress = new AddressEntity( addressId);
			Assert.AreEqual( EntityState.New, fetchedAddress.Fields.State );
		}
		

		/// <summary>
		/// Tests the insertion of a Customer entity and an Address entity plus an update with ARITHABORT ON
		/// </summary>
		[Test]
		public void InsertUpdateCustomerTestSimpleWithArithAbort()
		{
			CommonDaoBase.SetArithAbortFlag(true);

			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			bool result = newCustomer.Save(true);
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, newCustomer.Fields.State);
			Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
			Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
			int addressID = newAddress.AddressId;
			Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
			int customerID = newCustomer.CustomerId;
			Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
			Assert.IsTrue((newCustomer.VisitingAddressId == addressID));
			Assert.AreEqual(newAddress, newCustomer.VisitingAddress);
			Assert.IsTrue((newAddress == newCustomer.VisitingAddress));
			Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);

			// update customer
			newCustomer.CompanyName += " ";
			Assert.IsTrue(newCustomer.Save());
		}

		/// <summary>
		/// Tests the insertion of a Customer entity and an Address entity. 
		/// </summary>
		[Test]
		public void InsertCustomerTestSimple()
		{
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			bool result = newCustomer.Save(true);
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, newCustomer.Fields.State);
			Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
			Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
			int addressID = newAddress.AddressId;
			Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
			int customerID = newCustomer.CustomerId;
			Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
			Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
			Assert.AreEqual(newAddress, newCustomer.VisitingAddress);
			Assert.IsTrue((newAddress==newCustomer.VisitingAddress));
			Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
		}


		[Test]
		public void InsertCustomerTestSimpleWithBinarySerialization()
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

			bool result = toSave.Save(true);
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, addressSaved.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, toSave.Fields.State);
			Assert.IsTrue((((int)addressSaved.Fields[(int)AddressFieldIndex.AddressId].CurrentValue) > 0));
			Assert.IsTrue((((int)toSave.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue) > 0));
			int addressID = addressSaved.AddressId;
			Assert.AreEqual(EntityState.Fetched, addressSaved.Fields.State);
			int customerID = toSave.CustomerId;
			Assert.AreEqual(EntityState.Fetched, toSave.Fields.State);
			Assert.IsTrue((toSave.VisitingAddressId == addressID));
			Assert.AreEqual(addressSaved, toSave.VisitingAddress);
			Assert.IsTrue((addressSaved == toSave.VisitingAddress));
			Assert.AreEqual(((IEntityCore)addressSaved).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);
		}


		/// <summary>
		/// Tests the insertion of a Customer entity and an Address entity which are first serialized/deserialized and then saved.
		/// </summary>
		[Test]
		public void InsertCustomerTestSimpleWithSerialization()
		{
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			
			// serialize to disk
			FileStream writer = new FileStream("Customer.xml", FileMode.Create, FileAccess.Write);
			//SoapFormatter formatter = new SoapFormatter();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(writer, newCustomer);
			writer.Close();

			FileStream reader = new FileStream("Customer.xml", FileMode.Open, FileAccess.Read);
			formatter = new BinaryFormatter();
			CustomerEntity newCustomerDeserialized = (CustomerEntity)formatter.Deserialize(reader);
			reader.Close();

			bool result = newCustomerDeserialized.Save(true);
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, newCustomerDeserialized.Fields.State);
			Assert.IsTrue((((int)newCustomerDeserialized.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
			Assert.IsTrue(newCustomerDeserialized.VisitingAddressId>0);
			int customerID = newCustomerDeserialized.CustomerId;
			Assert.AreEqual(EntityState.Fetched, newCustomerDeserialized.Fields.State);
		}


		[Test]
		public void RemoveFromCollectionTest()
		{
			ProductCollection products = new ProductCollection();
			products.Add(EntityCreator.CreateNewProduct(1));
			products.Add(EntityCreator.CreateNewProduct(2));
			products[0].TestRunId = _testRunID;
			products[1].TestRunId = _testRunID;

			Assert.IsTrue((products.SaveMulti() > 0));

			ProductEntity toRemove = new ProductEntity(products[0].ProductId);

			products.Remove(toRemove);
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

			Assert.IsTrue(customer.Save(true));

			ProductCollection productsCollection = new ProductCollection();
			productsCollection.DoNotPerformAddIfPresent=false;
			products = new ProductEntity[10];
			for (int i = 0; i < products.Length; i++)
			{
				products[i] = EntityCreator.CreateNewProduct(1);
				products[i].TestRunId = _testRunID;
				productsCollection.Add(products[i]);
			}
			int result = productsCollection.SaveMulti(true);
			Assert.AreEqual(10, result);
		}

		/// <summary>
		/// Tests the delete multi functionality. 
		/// </summary>
		[Test]
		public void DeleteMultiTest()
		{
			ProductCollection products = new ProductCollection();
			products.Add(EntityCreator.CreateNewProduct(1));
			products.Add(EntityCreator.CreateNewProduct(2));
			products[0].TestRunId = _testRunID;
			products[1].TestRunId = _testRunID;

			Assert.IsTrue((products.SaveMulti() > 0));
			Assert.AreEqual(EntityState.OutOfSync, products[0].Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, products[1].Fields.State);

			// refetch the entities
			products[0].Refetch();
			products[1].Refetch();
			Assert.AreEqual(EntityState.Fetched, products[0].Fields.State);
			Assert.AreEqual(EntityState.Fetched, products[1].Fields.State);

			// perform a delete multi
			Assert.IsTrue((products.DeleteMulti() > 0));
			Assert.AreEqual(0, products.Count);
		}


		/// <summary>
		/// Tests field versioning in an entity using simple steps. 
		/// </summary>
		[Test]
		public void FieldsVersioningTestSimple()
		{
			CustomerEntity customer = EntityCreator.CreateNewCustomer(1);
			CustomerEntity customer2 = EntityCreator.CreateNewCustomer(2);
			customer.IsNew = false;
			customer2.IsNew = false;

			// save fields
			customer.SaveFields("BeforeUpdate");

			// update all fields with values from customer2
			for (int i = 1; i < 6; i++)
			{
				customer.Fields[i].CurrentValue = customer2.Fields[i].CurrentValue;
			}

			// set PK differently as it is readonly due to the identity tag.
			customer.Fields[0].ForcedCurrentValueWrite(customer2.Fields[0].CurrentValue);

			Assert.AreEqual(customer.CompanyEmailAddress, customer2.CompanyEmailAddress);
			Assert.AreEqual(customer.CompanyName, customer2.CompanyName);
			Assert.AreEqual(customer.ContactPerson, customer2.ContactPerson);
			Assert.AreEqual(customer.CustomerId, customer2.CustomerId);
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


		[Test]
		public void UpdatePkTest()
		{
			ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
			newProduct.TestRunId = _testRunID;
			Console.WriteLine("Current Product PK = {0}", newProduct.ProductId.ToString());
			bool result = newProduct.Save();
			Assert.IsTrue(result);

			// refetch from db.
			newProduct.Refetch();

			// alter PK
			newProduct.ProductId = Guid.NewGuid();
			Console.WriteLine("New Product PK = {0}", newProduct.ProductId.ToString());
			
			// resave the product.
			result = newProduct.Save();
			Assert.IsTrue(result);
		}

		/// <summary>
		/// Tests the DbValue feature for optimistic concurrency. It writes a customer object to the db, and 
		/// updates the data locally and controls with a predicate which filters on the DbValue of a field the update of the data.
		/// The update then has to succeed.
		/// </summary>
		[Test]
		public void DbValueFeatureTest()
		{
			// Save customer.
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			bool result = newCustomer.Save(true);
			Assert.IsTrue(result);

			// force a refetch. This has to set the DbValues.
			int customerID = newCustomer.CustomerId;
			Assert.IsTrue(customerID > 0);

			// alter row
			newCustomer.CompanyName = ConstantsEnums.Customer2CompanyName;

			// setup filter: update the rows with the old, company name, which is located in DbValue
			IPredicate filter = CustomerFields.CompanyName == newCustomer.Fields[(int)CustomerFieldIndex.CompanyName].DbValue;

			bool saveResult = newCustomer.Save(filter, false);
			Assert.IsTrue(saveResult);
		}


		/// <summary>
		/// Tests the Rollback/Savepoint feature of a transaction.
		/// </summary>
		[Test]
		public void SavepointRollbackTest()
		{
			UnitTestTransaction t = new UnitTestTransaction( System.Data.IsolationLevel.ReadCommitted, "SavepointRollbackTestTrans" );
			// first save an address
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			t.Add(newAddress);
			bool addressSaveResult = newAddress.Save();
			Assert.IsTrue(addressSaveResult);
			int addressID = newAddress.AddressId;

			// save the transaction
			t.Save("TestSavepoint");

			// save customer
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			t.Add(newCustomer);
			bool customerSaveResult = newCustomer.Save();
			Assert.IsTrue(customerSaveResult);

			int customerID = newCustomer.CustomerId;

			// rollback trans to save point.
			t.Rollback("TestSavepoint");

			// commit trans.
			t.Commit();

			// load back customer, should fail.
			CustomerEntity customerSaved = new CustomerEntity(customerID);
			Assert.AreEqual(EntityState.New, customerSaved.Fields.State);

			// load back address, should succeed
			AddressEntity addressSaved = new AddressEntity(addressID);
			Assert.AreEqual(EntityState.Fetched, addressSaved.Fields.State);
		}

		/// <summary>
		/// Tests an expression using update. This is only possible with a direct update of existing entities.
		/// </summary>
		[Test]
		public void UpdateWithExpressionsTest()
		{
			// save 2 product entities.
			ProductEntity newProduct1 = EntityCreator.CreateNewProduct(1);
			newProduct1.TestRunId = _testRunID;
			bool result = newProduct1.Save();
			Assert.IsTrue(result);
			ProductEntity newProduct2 = EntityCreator.CreateNewProduct(2);
			newProduct2.TestRunId = _testRunID;
			result = newProduct2.Save();
			Assert.IsTrue(result);

			Guid productID1 = newProduct1.ProductId;
			Guid productID2 = newProduct2.ProductId;

			ProductEntity updateProduct = new ProductEntity();
			IExpression updateExpression = new Expression(
				EntityFieldFactory.Create(ProductFieldIndex.Price),
				ExOp.Mul,
				4);

			IPredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareRangePredicate(
				EntityFieldFactory.Create(ProductFieldIndex.ProductId),
				newProduct1.ProductId, newProduct2.ProductId));
			updateProduct.Fields[(int)ProductFieldIndex.Price].ExpressionToApply = updateExpression;
			ProductCollection products = new ProductCollection();
			int rowsAffected = products.UpdateMulti(updateProduct, filter);
			Assert.AreEqual(2, rowsAffected);

			// refetch entities
			ProductEntity product1 = new ProductEntity(productID1);
			ProductEntity product2 = new ProductEntity(productID2);

			Assert.AreEqual((ConstantsEnums.Product1Price * 4), product1.Price);
			Assert.AreEqual((ConstantsEnums.Product2Price * 4), product2.Price);
		}


		/// <summary>
		/// Tests if the reference to a related entity is reset (to null) when the fk field this relation is build upon is
		/// set to a different value.
		/// </summary>
		[Test]
		public void FkFieldRelatedEntityResetTest()
		{
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			bool result = newCustomer.Save(true);
			Assert.AreEqual(true, result);
			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, newCustomer.Fields.State);
			Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
			Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
			int addressID = newAddress.AddressId;
			Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
			int customerID = newCustomer.CustomerId;
			Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
			Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
			Assert.AreEqual(newAddress, newCustomer.VisitingAddress);
			Assert.IsTrue((newAddress==newCustomer.VisitingAddress));
			Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);

			
			// set FK field to same value. Nothing should happen
			newCustomer.VisitingAddressId = addressID;
			Assert.AreEqual(newCustomer.VisitingAddress, newAddress);

			// set FK field to different value. Reference should be reset
			newCustomer.VisitingAddressId = addressID+1;
			// the next statement will pull the same object from the db, as lazy loading flags are reset as well.
			// The instance should be different from newAddress, as a new object is returned because the one fetched isn't found. 
			Assert.IsTrue(newCustomer.VisitingAddress != newAddress);

			Assert.IsTrue((newCustomer.VisitingAddress.Fields.State == EntityState.New));
			// shouldn't reset other reference to same object.
			Assert.AreEqual(newCustomer.BillingAddress, newAddress);
		}




		/// <summary>
		/// Tests whether FK fields are rolled back properly, if the transaction rolls back in the middle of the work. 
		/// </summary>
		[Test]
		public void FkRollbackTest()
		{
			UnitTests.Dal.SelfServicing.HelperClasses.Transaction trans = new UnitTests.Dal.SelfServicing.HelperClasses.Transaction( System.Data.IsolationLevel.ReadCommitted, "Test" );
			
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer( 1 );
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress( 1 );
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			// billing address is null so will cause a rollback. 
			Assert.IsNull( newCustomer.Fields[(int)CustomerFieldIndex.VisitingAddressId].CurrentValue );

			trans.Add( newCustomer );
			trans.Add( newAddress );
			try
			{
				Assert.IsFalse( newAddress.Save( true ) );
				throw new Exception( "Shouldn't reach this line" );
			}
			catch( ORMQueryExecutionException )
			{
				// roll back transaction
				trans.Rollback();
			}
			finally
			{
				trans.Dispose();
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
			CustomerEntity newCustomer = EntityCreator.CreateNewCustomer(1);
			newCustomer.TestRunId = _testRunID;
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newCustomer.VisitingAddress = newAddress;
			newCustomer.BillingAddress = newAddress;
			UnitOfWork uow = new UnitOfWork();
			bool addResult = uow.AddForSave(newCustomer, true);
			Assert.IsTrue(addResult);

			// create a product
			ProductEntity newProduct = EntityCreator.CreateNewProduct(1);
			newProduct.TestRunId = _testRunID;
			bool result = newProduct.Save();
			Assert.IsTrue(result);
			Guid prodID = newProduct.ProductId;

			addResult = uow.AddForDelete(newProduct);
			Assert.IsTrue(addResult);

			// test pre-commit state.
			Assert.IsTrue(newCustomer.IsNew);
			Assert.IsTrue(newAddress.IsNew);
			Assert.AreEqual(EntityState.Fetched, newProduct.Fields.State);

			// commit
			uow.Commit( new UnitTestTransaction( System.Data.IsolationLevel.ReadCommitted, "UOWTest" ), true );

			// test save actions of UOW.
			Assert.AreEqual(EntityState.OutOfSync, newAddress.Fields.State);
			Assert.AreEqual(EntityState.OutOfSync, newCustomer.Fields.State);
			Assert.IsTrue((((int)newAddress.Fields[(int)AddressFieldIndex.AddressId].CurrentValue)>0));
			Assert.IsTrue((((int)newCustomer.Fields[(int)CustomerFieldIndex.CustomerId].CurrentValue)>0));
			int addressID = newAddress.AddressId;
			Assert.AreEqual(EntityState.Fetched, newAddress.Fields.State);
			int customerID = newCustomer.CustomerId;
			Assert.AreEqual(EntityState.Fetched, newCustomer.Fields.State);
			Assert.IsTrue((newCustomer.VisitingAddressId==addressID));
			Assert.AreEqual(newAddress, newCustomer.VisitingAddress);
			Assert.IsTrue((newAddress==newCustomer.VisitingAddress));
			Assert.AreEqual(((IEntityCore)newAddress).ObjectID, ((IEntityCore)newCustomer.VisitingAddress).ObjectID);

			// test delete actions of UOW:
			ProductEntity savedProduct = new ProductEntity(prodID);
			Assert.IsTrue(savedProduct.IsNew);
			Assert.AreEqual(EntityState.Deleted, newProduct.Fields.State);
		}


		/// <summary>
		/// Tests the UnitOfWork class and the re-ordering of work. It first inserts an entity which is inserted again in the unit of work.
		/// If the delete isn't scheduled before the insert, the unitofwork will fail.
		/// </summary>
		[Test]
		public void UnitOfWorkTestWithReorder()
		{
			ProductEntity p = EntityCreator.CreateNewProduct(1);
			Guid productId = p.ProductId;
			p.TestRunId = _testRunID;
			Assert.IsTrue(p.Save());

			List<UnitOfWorkBlockType> commitOrder = new List<UnitOfWorkBlockType>();
			commitOrder.Add(UnitOfWorkBlockType.Deletes);
			commitOrder.Add(UnitOfWorkBlockType.Inserts);

			UnitOfWork uow = new UnitOfWork(commitOrder);
			uow.AddForDelete(p);

			ProductEntity secondTry = EntityCreator.CreateNewProduct(1);
			secondTry.ProductId = productId;
			secondTry.TestRunId = _testRunID;
			uow.AddForSave(secondTry);

			// commit
			int result = uow.Commit(new UnitTestTransaction(System.Data.IsolationLevel.ReadCommitted, "UOWTest"), true);
			Assert.AreEqual(2, result);

			Assert.AreEqual(EntityState.Deleted, p.Fields.State);
		}


		/// <summary>
		/// Tests the new option to signal the entity what to do when a related entity is not found. 
		/// </summary>
		[Test]
		public void RelatedEntityNotFoundTest()
		{
			AddressEntity newAddress = EntityCreator.CreateNewAddress(1);
			newAddress.TestRunId = _testRunID;
			newAddress.Save();

			Assert.IsTrue(newAddress.CustomerBillingAddressReturnsNewIfNotFound);
			CustomerEntity relatedCustomer = newAddress.CustomerBillingAddress;
			Assert.IsNotNull(relatedCustomer);
			Assert.AreEqual(EntityState.New, relatedCustomer.Fields.State);

			newAddress.CustomerVisitingAddressReturnsNewIfNotFound = false;
			CustomerEntity nullCustomer = newAddress.CustomerVisitingAddress;
			Assert.IsNull(nullCustomer);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			// remove all data for this testrun
			ActionProcedures.ClearAll();
			//ActionProcedures.ClearAll();
		}
	}




	/// <summary>
	/// Auditor class for all entities in write unit tests. 
	/// </summary>
	[DependencyInjectionInfo(typeof(IEntity), "AuditorToUse", TargetNamespaceFilter = "UnitTests.Dal.SelfServicing")]
	[Serializable]
	public class GeneralAuditor : AuditorBase
	{
		private enum AuditType
		{
			DeleteOfEntity = 1,
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
			Console.WriteLine("AuditType: DirectDeleteOfEntities. EntityType: {0}.", typeOfEntity.FullName);
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
			Console.WriteLine("AuditType: DirectUpdateOfEntities. EntityName: {0}. Number of entities updated: {1}", entity.LLBLGenProEntityName, numberOfEntitiesUpdated);
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
			Console.WriteLine("AuditType: EntityFieldSet. EntityName: {0}. FieldIndex: {1}. OriginalValue {2}. CurrentValue: {3}", entity.LLBLGenProEntityName,
					fieldIndex, originalValueAsString, currentValue);
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
			Console.WriteLine(">>>>>>>>>>>>>>>>>> Transaction Commit <<<<<<<<<<<<<<<<<<<<<<<<");
			_auditInfoEntities.Clear();
		}
	}
}
