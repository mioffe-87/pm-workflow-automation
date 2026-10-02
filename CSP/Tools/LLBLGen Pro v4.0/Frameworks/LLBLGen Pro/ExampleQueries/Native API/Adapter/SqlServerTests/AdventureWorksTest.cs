using System;
using System.Data;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NUnit.Framework;
using System.ComponentModel;

using AdventureWorks.Dal.Adapter.DatabaseSpecific;
using AdventureWorks.Dal.Adapter.EntityClasses;
using AdventureWorks.Dal.Adapter.FactoryClasses;
using AdventureWorks.Dal.Adapter.HelperClasses;
using AdventureWorks.Dal.Adapter.RelationClasses;
using AdventureWorks.Dal.Adapter;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Microsoft.Samples.SqlServer;
using System.Security.Principal;
using AdventureWorks.Dal.Adapter.TypedListClasses;


namespace Unittests.TestLibrary.SqlServerTests.Adapter
{
	/// <summary>
	/// General class for adventure works oriented tests. These tests are done on Sqlserver 2005 and contain special sqlserver 2005 related features.
	/// </summary>
	[TestFixture()]
	public class AdventureWorksTest
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}
		
		
		[Test]
		public void V3DynamicRelationJoinTest()
		{
			var entityType = (EntityType)Enum.Parse(typeof(EntityType), "SalesOrderDetailEntity");
			var salesOrderDetail = GeneralEntityFactory.Create(entityType);

			var dimensionEntityType = (EntityType)Enum.Parse(typeof(EntityType), "SpecialOfferProductEntity");
			var specialOfferProduct = GeneralEntityFactory.Create(dimensionEntityType);

			var fields = new ResultsetFields(4);
			fields.DefineField(salesOrderDetail.Fields["SalesOrderDetailId"], 0, "a");
			fields.DefineField(salesOrderDetail.Fields["SalesOrderId"], 1, "b");
			fields.DefineField(specialOfferProduct.Fields["ProductId"], 2, "d");
			fields.DefineField(specialOfferProduct.Fields["SpecialOfferId"], 3, "e");

			EntityField2 dataareaId = (EntityField2)salesOrderDetail.Fields["OrderQty"];

			DataAccessAdapter adapter = new DataAccessAdapter();
			DataTable dynamicList = new DataTable();
			RelationPredicateBucket filter = new RelationPredicateBucket(dataareaId == 25);

			IPredicateExpression onClause = new PredicateExpression();
			onClause.Add((EntityField2)salesOrderDetail.GetEntityFactory().CreateFields()["ProductId"] ==
						 (EntityField2)specialOfferProduct.GetEntityFactory().CreateFields()["ProductId"]);
			onClause.AddWithAnd((EntityField2)salesOrderDetail.GetEntityFactory().CreateFields()["SpecialOfferId"] ==
								(EntityField2)specialOfferProduct.GetEntityFactory().CreateFields()["SpecialOfferId"]);
			onClause.AddWithAnd((EntityField2)specialOfferProduct.GetEntityFactory().CreateFields()["ModifiedDate"] == DateTime.Now);

			DynamicRelation relation = new DynamicRelation(entityType, JoinHint.Left, dimensionEntityType,
														 string.Empty, string.Empty, onClause);
			filter.Relations.Add(relation);
			adapter.FetchTypedList(fields, dynamicList, filter);

		}


		[Test]
		public void PagingWithFirstFieldExcludedTest()
		{
			var e = new EmployeeEntity();
			ExcludeFieldsList excludeFields = new ExcludeFieldsList(e.Fields[0]);
			var collection = new EntityCollection<EmployeeEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(collection, null, 0, null, null, excludeFields, 2, 10);
				Assert.AreEqual(10, collection.Count);
			}
		}


		[Test]
		public void V3InheritedPkFieldSupportInSubtypeTest()
		{
			EntityCollection<SalesPersonEntity> salesPersons = new EntityCollection<SalesPersonEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(salesPersons, null);
			}
			Assert.AreEqual(17, salesPersons.Count);
		}


		[Test]
		public void UDTXmlDeserializationTest()
		{
			UdtTestEntity udt = new UdtTestEntity(1);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(udt));
			}
			Assert.IsNotNull(udt.Udtfield);
			string xml;
			udt.WriteXml(XmlFormatAspect.Compact25 | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out xml);
			UdtTestEntity deserializedUdt = new UdtTestEntity();
			deserializedUdt.ReadXml(xml);
			ComplexNumber c = deserializedUdt.Udtfield.Value;
			Assert.AreEqual(udt.Udtfield, c);

			UdtTestEntity copy = new UdtTestEntity();
			foreach(EntityField2 field in deserializedUdt.Fields)
			{
				copy.SetNewFieldValue(field.FieldIndex, field.CurrentValue);
				Assert.AreEqual(field.CurrentValue, copy.Fields[field.Name].CurrentValue);
				Assert.IsNotNull(field.CurrentValue);
			}
		}


		[Test]
		public void UDTSaveTest()
		{
			UdtTestEntity toSave = new UdtTestEntity();
			toSave.Udtfield = new ComplexNumber(2.0, 5.0);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(toSave, true));
				Assert.AreEqual(EntityState.Fetched, toSave.Fields.State);
				Assert.IsTrue(toSave.Id > 0);
				Assert.IsTrue(adapter.DeleteEntity(toSave));
			}
		}


		[Test]
		public void TOPWithParameterTest()
		{
			EntityCollection<CreditCardEntity> ccs = new EntityCollection<CreditCardEntity>();
			DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(ccs, null, 10);
			}
			DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);

			Assert.AreEqual(10, ccs.Count);
		}


		[Test]
		public void MultiAttributeAuthorizerTest()
		{
			CustomerEntity c = new CustomerEntity();
			ProductVendorEntity p = new ProductVendorEntity();
			LocationEntity l = new LocationEntity();

			Assert.IsNotNull(c.AuthorizerToUse);
			Assert.IsNotNull(p.AuthorizerToUse);
			Assert.IsNotNull(l.AuthorizerToUse);
			Assert.IsNull(new ProductEntity().AuthorizerToUse);
		}


		[Test]
		public void FastSerializationOfTypedListOnSqlServer2005Test()
		{
			SerializationTestTypedList typedList = new SerializationTestTypedList();
			try
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);

				Assert.AreEqual(7, typedList.Columns.Count);
				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{
					// fetch page 2, of 10 rows
					adapter.FetchTypedList(typedList, null, 0, null, true, 2, 10);
				}
				Assert.AreEqual(7, typedList.Columns.Count);
			}
			finally
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
			}
		}


		[Test]
		public void AuthorizationGetFieldInDatabindingTest()
		{
			SetupThreadPrincipal("TestUser");
			EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(employees, null, 100);
			}
			Assert.AreEqual(100, employees.Count);

			// national id number column is empty:
			foreach(EmployeeEntity e in employees)
			{
				Assert.IsTrue(string.IsNullOrEmpty(e.NationalIdnumber));
			}

			// now via a normal user, column will appear filled.
			SetupThreadPrincipal("Frans");
			foreach(EmployeeEntity e in employees)
			{
				Assert.IsTrue(e.NationalIdnumber.Length>0);
			}
		}


		[Test]
		public void AuthorizationGetSetFieldValueTest()
		{
			SetupThreadPrincipal("TestUser");

			EmployeeEntity e = new EmployeeEntity(1);
			// test if there is an authorizer injected
			Assert.IsNotNull(e.AuthorizerToUse);
			
			// user 'TestUser' isn't allowed to see NationalIDNumber and isn't allowed to set SalariedFlag
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(e));
			}

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
			CreditCardEntity c = new CreditCardEntity(1);
			Assert.IsNotNull(c.AuthorizerToUse);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsFalse(adapter.FetchEntity(c));
			}
			Assert.IsTrue(c.IsNew);
			Assert.IsTrue(c.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));

			// now retry with a different user. THis user should be allowed to fetch the entity
			SetupThreadPrincipal("Frans");
			c = new CreditCardEntity(1);
			Assert.IsNotNull(c.AuthorizerToUse);
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.FetchEntity(c));
			}
			Assert.IsFalse(c.IsNew);
			Assert.IsFalse(c.TestCurrentFieldValueForNull(CreditCardFieldIndex.CardNumber));
			Assert.AreEqual(c.CardNumber, "33332664695310");

			// multi fetch test
			SetupThreadPrincipal("TestUser");
			EntityCollection<CreditCardEntity> cards = new EntityCollection<CreditCardEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// fetch first 1000 cards
				adapter.FetchEntityCollection(cards, null, 1000);
			}

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
			cards = new EntityCollection<CreditCardEntity>();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// fetch first 1000 cards
				adapter.FetchEntityCollection(cards, null, 1000);
			}

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

			EntityCollection<CountryRegionEntity> countryRegions = new EntityCollection<CountryRegionEntity>();
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

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(countryRegions, true, false);
			}

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// still new 
				Assert.IsTrue(countryRegion.IsNew);
			}

			// save with other user.
			SetupThreadPrincipal("Frans");

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(countryRegions, true, false);
			}

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// not new 
				Assert.IsFalse(countryRegion.IsNew);
			}

			// delete the inserted rows
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.DeleteEntityCollection(countryRegions);
			}
		}


		[Test]
		public void AuthorizationUpdateExistingTest()
		{
			SetupThreadPrincipal("TestUser");
			// testuser can't update an entity in CountryRegion

			EntityCollection<CountryRegionEntity> countryRegions = new EntityCollection<CountryRegionEntity>();
			// fetch 2
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(countryRegions, null, 2);
			}

			// update these.
			countryRegions[0].SaveFields("PreTest");
			countryRegions[1].SaveFields("PreTest");
			countryRegions[0].Name += "Changed";
			countryRegions[1].Name += "Changed";
			Assert.IsTrue(countryRegions[0].IsDirty);
			Assert.IsTrue(countryRegions[1].IsDirty);

			// save. Won't work for this user. 
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(countryRegions, true, false);
			}

			foreach(CountryRegionEntity countryRegion in countryRegions)
			{
				// still dirty
				Assert.IsTrue(countryRegion.IsDirty);
			}

			// save with other user.
			SetupThreadPrincipal("Frans");

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(countryRegions, true, false);
			}

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

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.SaveEntityCollection(countryRegions, true, false);
			}
		}



		[Test]
		public void AuthorizationUpdateMultiTest()
		{
			DepartmentEntity updater = new DepartmentEntity();
			DateTime modificationDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			updater.ModifiedDate = modificationDate;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				// Testuser shouldn't be able to update these entities directly
				SetupThreadPrincipal("TestUser");
				Assert.AreEqual(0, adapter.UpdateEntitiesDirectly(updater, null));

				EntityCollection<DepartmentEntity> checker = new EntityCollection<DepartmentEntity>();
				adapter.FetchEntityCollection(checker, null);
				foreach(DepartmentEntity department in checker)
				{
					Assert.AreNotEqual(modificationDate, department.ModifiedDate);
				}

				// try with normal user
				SetupThreadPrincipal("Frans");
				Assert.AreEqual(16, adapter.UpdateEntitiesDirectly(updater, null));
				checker.Clear();
				adapter.FetchEntityCollection(checker, null);
				foreach(DepartmentEntity department in checker)
				{
					Assert.AreEqual(modificationDate, department.ModifiedDate);
				}

				//reset
				updater.ModifiedDate = new DateTime(1998, 1, 1, 12, 0, 0);
				adapter.UpdateEntitiesDirectly(updater, null);
			}
		}
		

		[Test]
		public void AuthorizationDeleteTest()
		{
			// add new one to test delete. 
			CountryRegionEntity toAdd = new CountryRegionEntity();
			toAdd.CountryRegionCode = "XXA";
			toAdd.Name = "XXAstistan";
			toAdd.ModifiedDate = DateTime.Now;
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(toAdd, true));

				// now try to delete this entity using our testuser, which isn't allowed to delete this entity.
				SetupThreadPrincipal("TestUser");

				Assert.IsFalse(adapter.DeleteEntity(toAdd));
				Assert.AreNotEqual(EntityState.Deleted, toAdd.Fields.State);

				// try with normal user
				SetupThreadPrincipal("Frans");
				Assert.IsTrue(adapter.DeleteEntity(toAdd));
				Assert.AreEqual(EntityState.Deleted, toAdd.Fields.State);
			}
		}

		
		[Test]
		public void AuthorizationDeleteDirectlyTest()
		{
			// add new one to test delete. 
			DepartmentEntity toAdd = new DepartmentEntity();
			toAdd.Name = "TestName";
			toAdd.GroupName = "testGroup";
			toAdd.ModifiedDate = DateTime.Now;
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(toAdd, true));

				// now try to delete this entity directly using our testuser, which isn't allowed to delete this entity directly
				SetupThreadPrincipal("TestUser");

				// use the new overload which accepts a type, as that overload performs authorization. 
				Assert.AreEqual(0, adapter.DeleteEntitiesDirectly(typeof(DepartmentEntity), new RelationPredicateBucket(DepartmentFields.Name == "TestName")));

				// try with normal user
				SetupThreadPrincipal("Frans");
				Assert.AreEqual(1, adapter.DeleteEntitiesDirectly(typeof(DepartmentEntity), new RelationPredicateBucket(DepartmentFields.Name == "TestName")));
			}
		}

		[Test]
		public void FetchCustomers()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( customers, new RelationPredicateBucket( CustomerFields.TerritoryId < 5 ) );
			}

			Assert.AreEqual( 8149, customers.Count );
		}


		/// <summary>
		/// Inserts a new entity in a table which has a PK with as default value newsequentialid(). This will generate a GUID inside the db and return it to the 
		/// entity.
		/// </summary>
		[Test]
		public void NewSequentialIdInsertReadBackTest()
		{
			try
			{
				// switch to 2005 mode
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);
				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{

					NewSequentialIdtestEntity newIdEntity = new NewSequentialIdtestEntity();
					newIdEntity.Description = "UnitTestInsert";
					Assert.IsTrue(adapter.SaveEntity(newIdEntity, true));
					Console.WriteLine("Id : {0}", newIdEntity.Id.ToString());
				}
			}
			finally
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
			}
		}


		[Test]
		public void CtePagingTest()
		{
			try
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);
				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{
					EntityCollection<ContactEntity> contacts = new EntityCollection<ContactEntity>(new ContactEntityFactory());
					adapter.FetchEntityCollection(contacts, null, 0,
							new SortExpression(ContactFields.LastName | SortOperator.Descending) & (ContactFields.FirstName | SortOperator.Ascending), 2, 20);
					Assert.AreEqual(19885, contacts[0].ContactId);
					Assert.AreEqual(20, contacts.Count);

					// dynamic list with expression.
					ResultsetFields fields = new ResultsetFields(2);
					fields.DefineField(ContactFields.ContactId, 0, "ContactId");
					fields.DefineField(ContactFields.FirstName, 1, "FullName");
					fields[1].ExpressionToApply = (ContactFields.FirstName + " " + ContactFields.LastName);
					DataTable results = new DataTable();
					adapter.FetchTypedList(fields, results, new RelationPredicateBucket(ContactFields.Title == DBNull.Value), 0, null, true, null, 2, 13);
					Assert.AreEqual(13, results.Rows.Count);
					Assert.AreEqual(978, (int)results.Rows[0]["ContactId"]);
				}
			}
			finally
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
			}
		}


		/// <summary>
		/// As CtePagingTest but now with joins and duplicates so DISTINCT has to be used. This join shouldn't fail because ROWNUMBER ruins the distinct filtering
		/// </summary>
		[Test]
		public void CtePagingWithJoinsAndDistinctTest()
		{
			try
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2005);
				using(DataAccessAdapter adapter = new DataAccessAdapter())
				{
					EntityCollection<ProductEntity> products = new EntityCollection<ProductEntity>();
					RelationPredicateBucket filter = new RelationPredicateBucket();
					filter.Relations.Add(ProductEntity.Relations.PurchaseOrderDetailEntityUsingProductId);
					filter.PredicateExpression.Add(PurchaseOrderDetailFields.OrderQty == 60);

					adapter.FetchEntityCollection(products, filter, 0,
							new SortExpression(ProductFields.ProductId | SortOperator.Ascending), 2, 4);
					Assert.AreEqual(492, products[0].ProductId);
					Assert.AreEqual(4, products.Count);
				}
			}
			finally
			{
				DataAccessAdapter.SetSqlServerCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2000);
			}
		}


		[Test]
		public void PrefetchPathLargeSetTest()
		{
			EntityCollection<SalesOrderHeaderEntity> salesOrders = new EntityCollection<SalesOrderHeaderEntity>();
			PrefetchPath2 path = new PrefetchPath2(EntityType.SalesOrderHeaderEntity);
			path.Add(SalesOrderHeaderEntity.PrefetchPathSalesOrderDetailCollection);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(salesOrders, new RelationPredicateBucket(SalesOrderHeaderFields.ShipMethodId == 5), path);
			}

			Assert.AreEqual(3806, salesOrders.Count);
		}

		[Test]
		public void PrefetchPathLargeSetSmallWindowTest()
		{
			EntityCollection<SalesOrderHeaderEntity> salesOrders = new EntityCollection<SalesOrderHeaderEntity>();
			PrefetchPath2 path = new PrefetchPath2(EntityType.SalesOrderHeaderEntity);
			path.Add(SalesOrderHeaderEntity.PrefetchPathSalesOrderDetailCollection);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(salesOrders, new RelationPredicateBucket(SalesOrderHeaderFields.ShipMethodId == 5), 45, null, path);
			}

			Assert.AreEqual(45, salesOrders.Count);
		}


		#region Helpers


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
