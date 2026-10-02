using System;
using System.Data;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using AdventureWorks.Dal.SelfServicing.DaoClasses;
using NUnit.Framework;
using System.ComponentModel;

using AdventureWorks.Dal.SelfServicing.HelperClasses;
using AdventureWorks.Dal.SelfServicing.EntityClasses;
using AdventureWorks.Dal.SelfServicing.FactoryClasses;
using AdventureWorks.Dal.SelfServicing.RelationClasses;
using AdventureWorks.Dal.SelfServicing;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Transactions;
using AdventureWorks.Dal.SelfServicing.CollectionClasses;
using System.Security.Principal;


namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
{
	/// <summary>
	/// General class for adventure works oriented tests. These tests are done on Sqlserver 2005 and contain special sqlserver 2005 related features.
	/// </summary>
	[TestFixture()]
	public class AdventureWorksTest
	{
		[Test]
		public void AuthorizationGetSetFieldValueTest()
		{
			SetupThreadPrincipal("TestUser");

			EmployeeEntity e = new EmployeeEntity();
			// test if there is an authorizer injected
			Assert.IsNotNull(e.AuthorizerToUse);

			// user 'TestUser' isn't allowed to see NationalIDNumber and isn't allowed to set SalariedFlag
			Assert.IsTrue(e.FetchUsingPK(1));

			// not allowed to see this value, so the value should be null.
			Assert.AreEqual(string.Empty, e.NationalIdnumber);

			// get other field, which is allowed
			Assert.AreEqual(16, e.ManagerId);

			// the SalariedFlag field isn't settable by this user. 
			Assert.AreEqual(false, e.SalariedFlag);
			e.SalariedFlag = true;
			Assert.AreEqual(false, e.SalariedFlag);
			Assert.IsFalse(e.SetNewFieldValue((int)EmployeeFieldIndex.SalariedFlag, true));
			Assert.AreEqual(false, e.SalariedFlag);

			// test another field to show that field manipulation does work on other fields.
			Assert.AreEqual(21, e.VacationHours);
			e.VacationHours++;
			Assert.AreEqual(22, e.VacationHours);
			Assert.IsTrue(e.SetNewFieldValue((int)EmployeeFieldIndex.VacationHours, (short)30));
			Assert.AreEqual(30, e.VacationHours);
		}


		/// <summary>
		/// Tests if the authorization works: it should deny the data for creditcard entities though should allow other data. 
		/// </summary>
		[Test]
		public void AuthorizationLoadEntityTest()
		{
			SetupThreadPrincipal("TestUser");

			// single entity fetch test
			CreditCardEntity c = new CreditCardEntity();
			Assert.IsNotNull(c.AuthorizerToUse);
			Assert.IsFalse(c.FetchUsingPK(1));
			Assert.IsTrue(c.IsNew);
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));

			// now retry with a different user. THis user should be allowed to fetch the entity
			SetupThreadPrincipal("Frans");
			c = new CreditCardEntity(1);
			Assert.IsNotNull(c.AuthorizerToUse);
			Assert.IsTrue(c.FetchUsingPK(1));
			Assert.IsFalse(c.IsNew);
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));
			Assert.AreEqual(c.CardNumber, "33332664695310");

			// multi fetch test
			SetupThreadPrincipal("TestUser");
			CreditCardCollection cards = new CreditCardCollection();
			cards.GetMulti(null, 1000);

			// there are still 1000 entities fetched
			Assert.AreEqual(1000, cards.Count);

			// however they're all empty, as this user isn't allowed to fetch creditcard entity data.
			foreach(CreditCardEntity card in cards)
			{
				Assert.IsTrue(card.IsNew);
				Assert.IsTrue(card.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));
			}

			// retry with normal user
			SetupThreadPrincipal("Frans");
			cards = new CreditCardCollection();

			cards.GetMulti(null, 1000);

			// there are still 1000 entities fetched
			Assert.AreEqual(1000, cards.Count);

			// however they're all empty, as this user isn't allowed to fetch creditcard entity data.
			foreach(CreditCardEntity card in cards)
			{
				Assert.IsFalse(card.IsNew);
				Assert.IsFalse(card.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));
			}
		}


		[Test]
		public void AuthorizationInsertNewTest()
		{
			SetupThreadPrincipal("TestUser");
			// testuser can't insert a new entity in CountryRegion

			CountryRegionCollection countryRegions = new CountryRegionCollection();
			// add 2 new ones. 
			CountryRegionEntity toAdd = new CountryRegionEntity();
			toAdd.CountryRegionCode = "XXA";
			toAdd.Name = "XXAstistan";
			toAdd.ModifiedDate = DateTime.Now;
			countryRegions.Add(toAdd);

			toAdd = new CountryRegionEntity();
			toAdd.CountryRegionCode = "XXB";
			toAdd.Name = "XXBestistan";
			toAdd.ModifiedDate = DateTime.Now;
			countryRegions.Add(toAdd);

			countryRegions.SaveMulti();

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// still new 
				Assert.IsTrue(countryRegion.IsNew);
			}

			// save with other user.
			SetupThreadPrincipal("Frans");

			countryRegions.SaveMulti();

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// not new 
				Assert.IsFalse(countryRegion.IsNew);
			}
			countryRegions.DeleteMulti();
		}


		[Test]
		public void AuthorizationUpdateExistingTest()
		{
			SetupThreadPrincipal("TestUser");
			// testuser can't update an entity in CountryRegion

			CountryRegionCollection countryRegions = new CountryRegionCollection();
			countryRegions.GetMulti(null, 2);

			// update these.
			countryRegions[0].SaveFields("PreTest");
			countryRegions[1].SaveFields("PreTest");
			countryRegions[0].Name += "Changed";
			countryRegions[1].Name += "Changed";
			Assert.IsTrue(countryRegions[0].IsDirty);
			Assert.IsTrue(countryRegions[1].IsDirty);

			// save. Won't work for this user. 
			countryRegions.SaveMulti();

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// still dirty
				Assert.IsTrue(countryRegion.IsDirty);
			}

			// save with other user.
			SetupThreadPrincipal("Frans");

			countryRegions.SaveMulti();

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// not dirty
				Assert.IsFalse(countryRegion.IsNew);
			}

			// undo changes
			countryRegions[0].RollbackFields("PreTest");
			countryRegions[1].RollbackFields("PreTest");
			countryRegions[0].IsDirty = true;
			countryRegions[0].Fields[(int)CountryRegionFieldIndex.Name].IsChanged = true;
			countryRegions[1].IsDirty = true;
			countryRegions[1].Fields[(int)CountryRegionFieldIndex.Name].IsChanged = true;

			countryRegions.SaveMulti();
		}


		[Test]
		public void AuthorizationUpdateMultiTest()
		{
			DepartmentEntity updater = new DepartmentEntity();
			DateTime modificationDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			updater.ModifiedDate = modificationDate;

			DepartmentCollection departments = new DepartmentCollection();
			// Testuser shouldn't be able to update these entities directly
			SetupThreadPrincipal("TestUser");
			Assert.AreEqual(0, departments.UpdateMulti(updater, null));

			DepartmentCollection checker = new DepartmentCollection();
			checker.GetMulti(null);
			foreach(DepartmentEntity department in checker)
			{
				Assert.AreNotEqual(modificationDate, department.ModifiedDate);
			}

			// try with normal user
			SetupThreadPrincipal("Frans");
			Assert.AreEqual(16, departments.UpdateMulti(updater, null));
			checker.Clear();
			checker.GetMulti(null);
			foreach(DepartmentEntity department in checker)
			{
				Assert.AreEqual(modificationDate, department.ModifiedDate);
			}

			// reset
			updater.ModifiedDate = new DateTime(1998, 1, 1, 12, 0, 0);
			departments.UpdateMulti(updater, null);
		}


		[Test]
		public void AuthorizationDeleteTest()
		{
			// add new one to test delete. 
			CountryRegionEntity toAdd = new CountryRegionEntity();
			toAdd.CountryRegionCode = "XXA";
			toAdd.Name = "XXAstistan";
			toAdd.ModifiedDate = DateTime.Now;
			Assert.IsTrue(toAdd.Save());

			// now try to delete this entity using our testuser, which isn't allowed to delete this entity.
			SetupThreadPrincipal("TestUser");

			Assert.IsFalse(toAdd.Delete());
			Assert.AreNotEqual(EntityState.Deleted, toAdd.Fields.State);

			// try with normal user
			SetupThreadPrincipal("Frans");
			Assert.IsTrue(toAdd.Delete());
			Assert.AreEqual(EntityState.Deleted, toAdd.Fields.State);
		}


		[Test]
		public void AuthorizationDeleteDirectlyTest()
		{
			// add new one to test delete. 
			DepartmentEntity toAdd = new DepartmentEntity();
			toAdd.Name = "TestName";
			toAdd.GroupName = "testGroup";
			toAdd.ModifiedDate = DateTime.Now;
			Assert.IsTrue(toAdd.Save());

			// now try to delete this entity directly using our testuser, which isn't allowed to delete this entity directly
			SetupThreadPrincipal("TestUser");

			DepartmentCollection departments = new DepartmentCollection();
			Assert.AreEqual(0, departments.DeleteMulti((DepartmentFields.Name == "TestName")));

			// try with normal user
			SetupThreadPrincipal("Frans");
			Assert.AreEqual(1, departments.DeleteMulti((DepartmentFields.Name == "TestName")));
		}



		/// <summary>
		/// Inserts a new entity in a table which has a PK with as default value newsequentialid(). This will generate a GUID inside the db and return it to the 
		/// entity.
		/// </summary>
		[Test]
		public void NewSequentialIdInsertReadBackTest()
		{
			// switch to 2005 mode
			CommonDaoBase.SetSqlServerCompatibilityLevel( SqlServerCompatibilityLevel.SqlServer2005 );

			NewSequentialIdtestEntity newIdEntity = new NewSequentialIdtestEntity();
			newIdEntity.Description = "UnitTestInsert";
			Assert.IsTrue( newIdEntity.Save());
			Guid id = newIdEntity.Id;
			Assert.IsTrue(id != Guid.Empty);
			CommonDaoBase.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
		}


		[Test]
		public void SubTypeWithSequentialIDTest()
		{
			// switch to 2005 mode
			CommonDaoBase.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);

			SequentialIdsubTypeEntity toSave = new SequentialIdsubTypeEntity();
			toSave.Description = "Test";
			toSave.IntValue = 0;
			Assert.IsTrue(toSave.Save());
			Assert.IsTrue(toSave.Id != Guid.Empty);
			CommonDaoBase.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
		}


		[Test]
		public void InsertNewWithTransactionScope()
		{
			using(TransactionScope ts = new TransactionScope())
			{
				CreditCardEntity ccard = new CreditCardEntity();

				ccard.FetchUsingPK(1);
				int expYear = ccard.ExpYear;
				ccard.ExpYear += 1;

				Assert.IsTrue(ccard.Save(true));
				int newExpYear = ccard.ExpYear;
				Assert.AreEqual(expYear + 1, newExpYear);
				ts.Complete();
			}
		}


		[Test]
		public void PrefetchPathLargeSetTest()
		{
			SalesOrderHeaderCollection salesOrders = new SalesOrderHeaderCollection();
			PrefetchPath path = new PrefetchPath(EntityType.SalesOrderHeaderEntity);
			path.Add(SalesOrderHeaderEntity.PrefetchPathSalesOrderDetailCollection);

			salesOrders.GetMulti((SalesOrderHeaderFields.ShipMethodId == 5), path);
			Assert.AreEqual(3806, salesOrders.Count);
		}

		[Test]
		public void PrefetchPathLargeSetSmallWindowTest()
		{
			SalesOrderHeaderCollection salesOrders = new SalesOrderHeaderCollection();
			PrefetchPath path = new PrefetchPath(EntityType.SalesOrderHeaderEntity);
			path.Add(SalesOrderHeaderEntity.PrefetchPathSalesOrderDetailCollection);

			salesOrders.GetMulti((SalesOrderHeaderFields.ShipMethodId == 5), 45, null, null, path);

			Assert.AreEqual(45, salesOrders.Count);
		}

		#region helpers
		/// <summary>
		/// Setups the thread principal.
		/// </summary>
		private void SetupThreadPrincipal(string userName)
		{
			string[] rolesArray = new string[] { "Managers", "Executives" };
			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(userName), rolesArray);
		}
		#endregion
	}
}
