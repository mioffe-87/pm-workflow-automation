using System;
using System.Data;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NUnit.Framework;
using System.ComponentModel;

using InheritanceOne.Dal.Adapter.DatabaseSpecific;
using InheritanceOne.Dal.Adapter.EntityClasses;
using InheritanceOne.Dal.Adapter.FactoryClasses;
using InheritanceOne.Dal.Adapter.HelperClasses;
using InheritanceOne.Dal.Adapter.RelationClasses;
using InheritanceOne.Dal.Adapter.TypedListClasses;
using InheritanceOne.Dal.Adapter;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Collections.Generic;
using System.Collections;

namespace Unittests.TestLibrary.SqlServerTests.Adapter
{

	/// <summary>
	/// Generic unit tests for the inheritance db using adapter.
	/// </summary>
	[TestFixture()]
	public class InheritanceOneUnitTests
	{
		[TestFixtureSetUp]
		public void SetupTests()
		{
		}


		[Test]
		public void EmptySimpleFilterTest()
		{
			var boardMembers = new EntityCollection<BoardMemberEntity>();
			var employees = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
			using(var adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(employees, null);
			}
		}
        

		[Test]
		public void GetScalarWithFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				try
				{
					UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
					uow.Commit(adapter, true);

					ResultsetFields fields = new ResultsetFields(1);
					fields.DefineField(ManagerFields.Id, 0, "ManagerId");
					IPredicateExpression filter = new PredicateExpression(ManagerFields.Name == "Manager 1");

					int managerId = (int)adapter.GetScalar(fields, filter, null);
					Assert.IsTrue(managerId > 0);
				}
				finally
				{
					adapter.DeleteEntityCollection(toDelete);
				}
			}
		}


		[Test]
		public void DynamicListWithTypeFilterOnSubTypeTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(EmployeeFields.Name, 0, "EmployeeName");
				fields.DefineField(DepartmentFields.Name, 1, "DepartmentName");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);

				// filter on managers only
				filter.PredicateExpression.Add(ManagerEntity.GetEntityTypeFilter());

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter, true);

				Assert.AreEqual(2, results.Rows.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void OneToOnePkPkSyncTest()
		{
			ManagerEntity m = new ManagerEntity();
			m.Name = "John Doe";
			m.StartDate = DateTime.Now;
			m.BoardMemberCore = new BoardMemberCoreEntity();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(m, true, false));
				Assert.IsFalse(m.IsNew);
				Assert.IsTrue(m.Id>0);
				Assert.IsTrue(adapter.SaveEntity(m.BoardMemberCore, true, false));
				Assert.IsFalse(m.BoardMemberCore.IsNew);
				Assert.IsTrue(m.BoardMemberCore.BoardMemberId>0);

				adapter.DeleteEntity(m.BoardMemberCore);
				adapter.DeleteEntity(m);
			}
		}


		[Test]
		public void DynamicListWithTypeFilterOnSubTypeWithAliasesTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(EmployeeFields.Name, 0, "EmployeeName", "E");
				fields.DefineField(DepartmentFields.Name, 1, "DepartmentName", "D");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId, "E", "D", JoinHint.Inner);

				// filter on managers only
				filter.PredicateExpression.Add(ManagerFields.Id.SetObjectAlias("E") != DBNull.Value);

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter, true);

				Assert.AreEqual(2, results.Rows.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void HierarchicalProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				adapter.FetchEntityCollection(employees, null);

				DataSet results = new DataSet();
				employees.CreateHierarchicalProjection(results);
				Assert.AreEqual(3, results.Tables.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void TypedListWithMultipleEntitiesSameHierarchyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ClerkManagersTypedList list = new ClerkManagersTypedList();
				adapter.FetchTypedList(list);
				Assert.AreEqual(4, list.Count);

				int count = adapter.GetDbCount(list.GetFieldsInfo(), list.GetRelationInfo());
				Assert.AreEqual(4, count);

				IEntityFields2 clerkFields = new ClerkEntityFactory().CreateFields();
				ResultsetFields testFields = new ResultsetFields(clerkFields.Count);
				for (int i = 0; i < clerkFields.Count; i++)
				{
					testFields.DefineField(clerkFields[i], i);
				}

				count = adapter.GetDbCount(testFields, null);
				Assert.AreEqual(2, count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void GetDbCountWithFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				RelationPredicateBucket filter = new RelationPredicateBucket(ClerkEntity.GetEntityTypeFilter());
				EntityCollection<EmployeeEntity> clerks = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				adapter.FetchEntityCollection(clerks, filter);
				int clerkCount = adapter.GetDbCount(clerks, filter);
				Assert.AreEqual(clerkCount, clerks.Count);
				Assert.AreEqual(2, clerkCount);

				adapter.DeleteEntityCollection(toDelete);
			}
		}



		[Test]
		public void TypedListWithInheritanceTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				// Fetch Typedlist with all boardmembers and their company cars.
				BoardMemberCompanyCarTypedList list = new BoardMemberCompanyCarTypedList();
				adapter.FetchTypedList(list.GetFieldsInfo(), list, list.GetRelationInfo());

				Assert.AreEqual(1, list.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void ManyToManyTestWithHierarchiesBothSidesPlusFilter()
		{
			EntityCollection<MtoMsubAEntity> subAs = new EntityCollection<MtoMsubAEntity>();
			PrefetchPath2 path = new PrefetchPath2(EntityType.MtoMsubAEntity);
			path.Add(MtoMsubAEntity.PrefetchPathMtoMsuperBcollectionViaMtoMintermediateAb);

			RelationPredicateBucket filter = new RelationPredicateBucket();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(subAs, null, path);
			}

			Assert.AreEqual(2, subAs.Count);
		}


		[Test]
		public void SubtypesWithRelationAndSameParentAutoAliasTest()
		{
			// get all folders which have files with a size >= 200.
			EntityCollection<FolderEntity> folders = new EntityCollection<FolderEntity>();
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(FolderEntity.Relations.FileEntityUsingParentId, "File");
			filter.PredicateExpression.Add(FileFields.Filesize.SetObjectAlias("File") >= 200);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(folders, filter);
			}

			Assert.AreEqual(2, folders.Count);
		}


		[Test]
		public void SubtypesWithRelationAndSameParentWhereSubTypeIsSubTypeOfEntityInRelationTest()
		{
			// get all folders which have zip files.
			EntityCollection<FolderEntity> folders = new EntityCollection<FolderEntity>();
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.Relations.Add(FolderEntity.Relations.FileEntityUsingParentId, "File");
			filter.PredicateExpression.Add(ArchiveFields.PackType.SetObjectAlias("File") == "zip");

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.FetchEntityCollection(folders, filter);
			}

			Assert.AreEqual(1, folders.Count);
		}


		[Test]
		public void DynamicListWithAutoTypeFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				FamilyCarEntity companyCar1 = new FamilyCarEntity();
				companyCar1.CarId = 1;
				companyCar1.Brand = "Fiat";
				companyCar1.HasDrawHook = true;
				companyCar1.Price = 10000M;
				GermanFamilyCarEntity companyCar2 = new GermanFamilyCarEntity();
				companyCar2.CarId = 2;
				companyCar2.Brand = "Mercedes";
				companyCar2.HasDrawHook = true;
				companyCar2.Price = 25000M;
				SportsCarEntity sportsCar1 = new SportsCarEntity();
				sportsCar1.CarId = 3;
				sportsCar1.Brand = "Ferrari";
				sportsCar1.IsCabrio = false;
				sportsCar1.Price = 1000000M;
				Assert.IsTrue(adapter.SaveEntity(companyCar1));
				Assert.IsTrue(adapter.SaveEntity(companyCar2));
				Assert.IsTrue(adapter.SaveEntity(sportsCar1));

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(FamilyCarFields.Brand, 0);
				fields.DefineField(GermanFamilyCarFields.Price, 1);
				DataTable results = new DataTable();

				int count = adapter.GetDbCount(fields, null);
				Assert.AreEqual(1, count);

				adapter.FetchTypedList(fields, results, null);

				Assert.AreEqual(1, results.Rows.Count);
				
				adapter.DeleteEntity(companyCar2);
				adapter.DeleteEntity(companyCar1);
				adapter.DeleteEntity(sportsCar1);
			}
		}

		[Test]
		public void FilterWithAutoAdditionOfTypeFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				FamilyCarEntity companyCar1 = new FamilyCarEntity();
				companyCar1.CarId = 1;
				companyCar1.Brand = "Fiat";
				companyCar1.HasDrawHook = true;
				companyCar1.Price = 10000M;
				GermanFamilyCarEntity companyCar2 = new GermanFamilyCarEntity();
				companyCar2.CarId = 2;
				companyCar2.Brand = "Mercedes";
				companyCar2.HasDrawHook = true;
				companyCar2.Price = 25000M;
				SportsCarEntity sportsCar1 = new SportsCarEntity();
				sportsCar1.CarId = 3;
				sportsCar1.Brand = "Ferrari";
				sportsCar1.IsCabrio = false;
				sportsCar1.Price = 1000000M;
				BoardMemberEntity bm1 = new BoardMemberEntity();
				bm1.Name = "BM1";
				bm1.StartDate = DateTime.Now;
				bm1.CompanyCar = companyCar1;
				BoardMemberEntity bm2 = new BoardMemberEntity();
				bm2.Name = "BM2";
				bm2.StartDate = DateTime.Now;
				bm2.CompanyCar = companyCar2;
				BoardMemberEntity bm3 = new BoardMemberEntity();
				bm3.Name = "BM3";
				bm3.StartDate = DateTime.Now;
				bm3.CompanyCar = sportsCar1;

				Assert.IsTrue(adapter.SaveEntity(companyCar1));
				Assert.IsTrue(adapter.SaveEntity(companyCar2));
				Assert.IsTrue(adapter.SaveEntity(sportsCar1));

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(CompanyCarFields.Brand, 0);
				fields.DefineField(BoardMemberFields.Name, 1);
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(FamilyCarEntity.Relations.BoardMemberEntityUsingCompanyCarId);

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);

				try
				{
					Assert.AreEqual(2, results.Rows.Count);
				}
				finally
				{
					adapter.DeleteEntity(bm3);
					adapter.DeleteEntity(bm2);
					adapter.DeleteEntity(bm1);
					adapter.DeleteEntity(companyCar2);
					adapter.DeleteEntity(companyCar1);
					adapter.DeleteEntity(sportsCar1);
				}
			}
		}


		[Test]
		public void ArtificialAliasTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(ClerkEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
				filter.Relations.Add(DepartmentEntity.Relations.ManagerEntityUsingManagesDepartmentId, "Manager");
				filter.PredicateExpression.Add(ManagerFields.Name.SetObjectAlias("Manager") == "Manager 1");

				EntityCollection<ClerkEntity> clerks = new EntityCollection<ClerkEntity>();
				adapter.FetchEntityCollection(clerks, filter);

				Assert.AreEqual(2, clerks.Count);
				Console.WriteLine(clerks[0].Name);
				Console.WriteLine(clerks[1].Name);

				adapter.DeleteEntityCollection(toDelete);
			}
		}

		
		[Test]
		public void XmlSerializationDeserializationTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.EmployeeEntity);
				path.Add(BoardMemberEntity.PrefetchPathCompanyCar);
				path.Add(EmployeeEntity.PrefetchPathWorksForDepartment);

				EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());

				adapter.FetchEntityCollection(employees, null, path);

				string verboseXML = string.Empty;
				employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out verboseXML);
				string compactXML = string.Empty;
				employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks | XmlFormatAspect.Compact, out compactXML);
				string compact25XML = string.Empty;
				employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks | XmlFormatAspect.Compact25, out compact25XML);

				EntityCollection<EmployeeEntity> eeVerbose = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				eeVerbose.ReadXml(verboseXML);

				EntityCollection<EmployeeEntity> eeCompact = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				eeCompact.ReadXml(compactXML);

				EntityCollection<EmployeeEntity> eeCompact25 = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				eeCompact25.ReadXml(compact25XML);

				StreamWriter writer = new StreamWriter("employeesVerbose.xml");
				writer.Write(verboseXML);
				writer.Close();

				writer = new StreamWriter("employeesCompact.xml");
				writer.Write(compactXML);
				writer.Close();

				writer = new StreamWriter("employeesCompact25.xml");
				writer.Write(compact25XML);
				writer.Close();

				for(int i = 0; i < employees.Count; i++)
				{
					Assert.AreEqual(employees[i].GetType(), eeVerbose[i].GetType());
					Assert.AreEqual(employees[i].GetType(), eeCompact[i].GetType());
					Assert.AreEqual(employees[i].GetType(), eeCompact25[i].GetType());
					Assert.AreEqual(employees[i].WorksForDepartment.GetType(), eeVerbose[i].WorksForDepartment.GetType());
					Assert.AreEqual(employees[i].WorksForDepartment.GetType(), eeCompact[i].WorksForDepartment.GetType());
					Assert.AreEqual(employees[i].WorksForDepartment.GetType(), eeCompact25[i].WorksForDepartment.GetType());
					if(employees[i] is BoardMemberEntity)
					{
						Assert.AreEqual(((BoardMemberEntity)employees[i]).CompanyCar.GetType(), ((BoardMemberEntity)eeVerbose[i]).CompanyCar.GetType());
						Assert.AreEqual(((BoardMemberEntity)employees[i]).CompanyCar.GetType(), ((BoardMemberEntity)eeCompact[i]).CompanyCar.GetType());
						Assert.AreEqual(((BoardMemberEntity)employees[i]).CompanyCar.GetType(), ((BoardMemberEntity)eeCompact25[i]).CompanyCar.GetType());
					}
				}

				adapter.DeleteEntityCollection(toDelete);
			}
		}



		[Test]
		public void DIScopeBasedInjectionTest()
		{
			// first normal injection via attributes. The customer entity isn't mentioned in the scope definitions so it should keep getting its
			// original general cpf
			ManagerEntity m = new ManagerEntity();
			Assert.AreEqual(typeof(GeneralIH1CPF), m.ConcurrencyPredicateFactoryToUse.GetType());
			FamilyCarEntity f = new FamilyCarEntity();
			Assert.AreEqual(typeof(GeneralIH1CPF), f.ConcurrencyPredicateFactoryToUse.GetType());

			// using one scope
			using(IH1TestScope outerScope = new	IH1TestScope())
			{
				m = new ManagerEntity();
				Assert.AreEqual(typeof(EmployeeSpecificCPF), m.ConcurrencyPredicateFactoryToUse.GetType());
				BoardMemberEntity bm = new BoardMemberEntity();
				Assert.AreEqual(typeof(EmployeeSpecificCPF), bm.ConcurrencyPredicateFactoryToUse.GetType());

				f = new FamilyCarEntity();
				Assert.AreEqual(typeof(GeneralIH1CPF), f.ConcurrencyPredicateFactoryToUse.GetType());
			}

			// using nested scopes
			using(IH1TestScope outerScope = new IH1TestScope())
			{
				m = new ManagerEntity();
				Assert.AreEqual(typeof(EmployeeSpecificCPF), m.ConcurrencyPredicateFactoryToUse.GetType());
				BoardMemberEntity bm = new BoardMemberEntity();
				Assert.AreEqual(typeof(EmployeeSpecificCPF), bm.ConcurrencyPredicateFactoryToUse.GetType());
				f = new FamilyCarEntity();
				Assert.AreEqual(typeof(GeneralIH1CPF), f.ConcurrencyPredicateFactoryToUse.GetType());

				using(NestedIH1TestScope innerScope = new NestedIH1TestScope())
				{
					m = new ManagerEntity();
					Assert.AreEqual(typeof(NestedManagerSpecificCPF), m.ConcurrencyPredicateFactoryToUse.GetType());
					bm = new BoardMemberEntity();
					Assert.AreEqual(typeof(EmployeeSpecificCPF), bm.ConcurrencyPredicateFactoryToUse.GetType());
					f = new FamilyCarEntity();
					Assert.AreEqual(typeof(GeneralIH1CPF), f.ConcurrencyPredicateFactoryToUse.GetType());
				}
			}
		}


		/// <summary>
		/// Fetches with excluded fields test.
		/// </summary>
		[Test]
		public void FetchesWithExcludedFieldsTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
				excludedFields.Add(EmployeeFields.Name);
				excludedFields.Add(BoardMemberFields.StartDate);
				excludedFields.Add(ClerkFields.JobDescription);

				EntityCollection<EmployeeEntity> employees = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				EntityCollection<EmployeeEntity> employeesVerification = new EntityCollection<EmployeeEntity>(new EmployeeEntityFactory());
				SortExpression sorter = new SortExpression(EmployeeFields.Id | SortOperator.Ascending);
				adapter.FetchEntityCollection(employees, null, 0, sorter, null, excludedFields);
				adapter.FetchEntityCollection(employeesVerification, null, 0, sorter);

				foreach(EmployeeEntity e in employees)
				{
					Assert.IsTrue(e.TestCurrentFieldValueForNull(EmployeeFieldIndex.Name));
					Assert.IsTrue(e.TestCurrentFieldValueForNull(EmployeeFieldIndex.StartDate));
					ClerkEntity c = e as ClerkEntity;
					if(c != null)
					{
						Assert.IsTrue(c.TestCurrentFieldValueForNull(ClerkFieldIndex.JobDescription));
					}
				}

				// fetch the excluded fields into the set
				adapter.FetchExcludedFields(employees, excludedFields);
				for(int i = 0; i < employees.Count; i++)
				{
					Assert.AreEqual(employees[i].Name, employeesVerification[i].Name);
					Assert.AreEqual(employees[i].StartDate, employeesVerification[i].StartDate);
					if(employees[i].GetType() == typeof(ClerkEntity))
					{
						Assert.AreEqual(((ClerkEntity)employees[i]).JobDescription, ((ClerkEntity)employeesVerification[i]).JobDescription);
					}
				}

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchAllBoardMembersWithCompanyCarLogic()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(BoardMemberEntity.Relations.CompanyCarEntityUsingCompanyCarId);
				filter.PredicateExpression.Add(CompanyCarFields.Brand == "Fiat");
				EntityCollection<BoardMemberEntity> boardMembers = new EntityCollection<BoardMemberEntity>();
				adapter.FetchEntityCollection(boardMembers, filter);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchAllFamilycars()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection<FamilyCarEntity> familyCars = new EntityCollection<FamilyCarEntity>();
				adapter.FetchEntityCollection(familyCars, null);

				adapter.DeleteEntityCollection(toDelete);
			}
		}



		/// <summary>
		/// Tests a dyn. list with a join with 2 instances of the base type. 
		/// </summary>
		[Test]
		public void FetchDynamicListWithTwoInstancesOfBaseType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(3);
				fields.DefineField(ClerkFields.Name, 0, "ClerkName");
				fields.DefineField(DepartmentFields.Name, 1, "DepartmentName");
				fields.DefineField(ManagerFields.Name, 2, "ManagerName");

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(ClerkEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
				filter.Relations.Add(DepartmentEntity.Relations.ManagerEntityUsingManagesDepartmentId);

				SortExpression sorter = new SortExpression(ClerkFields.Name | SortOperator.Ascending);
				sorter.Add(ManagerFields.Name | SortOperator.Ascending);

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter, false);

				Assert.AreEqual(4, results.Rows.Count);
				for(int i = 0; i < results.Rows.Count; i++)
				{
					switch(i)
					{
						case 0:
							Assert.AreEqual("Clerk 1", results.Rows[i]["ClerkName"].ToString());
							Assert.AreEqual("Boardmember 1", results.Rows[i]["ManagerName"].ToString());
							break;
						case 1:
							Assert.AreEqual("Clerk 1", results.Rows[i]["ClerkName"].ToString());
							Assert.AreEqual("Manager 1", results.Rows[i]["ManagerName"].ToString());
							break;
						case 2:
							Assert.AreEqual("Clerk 2", results.Rows[i]["ClerkName"].ToString());
							Assert.AreEqual("Boardmember 1", results.Rows[i]["ManagerName"].ToString());
							break;
						case 3:
							Assert.AreEqual("Clerk 2", results.Rows[i]["ClerkName"].ToString());
							Assert.AreEqual("Manager 1", results.Rows[i]["ManagerName"].ToString());
							break;
					}
				}
				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void PkFkSyncFromSubTypeTest()
		{
			Employee2Entity toInsert = new Employee2Entity();
			toInsert.FirstName = "Sjaak";
			toInsert.LastName = "TrekHaak";
			toInsert.EmployeeNumber = 123;
			PaycheckEntity pc1 = new PaycheckEntity();
			pc1.Amount = 10.0M;
			PaycheckEntity pc2 = new PaycheckEntity();
			pc2.Amount = 10000.0M;
			toInsert.Paycheck.Add(pc1);
			toInsert.Paycheck.Add(pc2);

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(toInsert, true, true));
			}

			Assert.IsTrue((toInsert.PersonId > 0));
			Assert.AreEqual(toInsert.PersonId, pc1.PersonId);
			Assert.AreEqual(toInsert.PersonId, pc2.PersonId);
		}


		[Test]
		public void XmlDeserializationPKUnChangedTest()
		{
			ManagerEntity toSerialize = new ManagerEntity();
			toSerialize.Name = "John Foo";
			Assert.IsFalse(toSerialize.Fields[(int)ManagerFieldIndex.Id].IsChanged);
			string xml = String.Empty;
			toSerialize.WriteXml(XmlFormatAspect.Compact | XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out xml);
			ManagerEntity deserialized = new ManagerEntity();
			deserialized.ReadXml(xml);
			Assert.IsFalse(deserialized.Fields[(int)ManagerFieldIndex.Id].IsChanged);
		}


		[Test]
		public void ComplexPrefetchPathWithInheritedNodeTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 1;
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				// fetch CompanyCar - BoardMember - Worksfordepartment. This utilizes a node inherited from employee
				FamilyCarEntity c = new FamilyCarEntity(1);
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.CompanyCarEntity);
				path.Add(CompanyCarEntity.PrefetchPathBoardMember).SubPath.Add(BoardMemberEntity.PrefetchPathWorksForDepartment);

				adapter.FetchEntity(c, path);
				Assert.AreEqual(c.BoardMember.Count, 1);
				Assert.IsNotNull(c.BoardMember[0].WorksForDepartment);
				adapter.DeleteEntityCollection(toDelete);
			}
		}

	
		/// <summary>
		/// Test which forces the code to follow the the non-optimized path in FetchPrefetchPath to test that code.
		/// </summary>
		[Test]
		public void ContextWithInheritanceTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				BoardMemberEntity toTest = null;
				foreach(IEntity2 e in toDelete)
				{
					if(e is BoardMemberEntity)
					{
						toTest = (BoardMemberEntity)e;
						break;
					}
				}
				Assert.IsNotNull(toTest);

				Context c = new Context();
				c.Add(toTest);

				BoardMemberEntity theSame = (BoardMemberEntity)c.Get(new BoardMemberEntityFactory(), toTest.Id);
				Assert.AreEqual(theSame, toTest);
				Assert.AreSame(theSame, toTest);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		/// <summary>
		/// Specifies the derived type as the rootnode in the path.
		/// </summary>
		[Test]
		public void PolymorphicPrefetchPathWithDerivedRootNode()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection boardMembers = new EntityCollection(new BoardMemberEntityFactory());

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.BoardMemberEntity);
				path.Add(BoardMemberEntity.PrefetchPathWorksForDepartment);
				adapter.FetchEntityCollection(boardMembers, null, path);
				
				adapter.DeleteEntityCollection(toDelete);

				foreach(BoardMemberEntity entity in boardMembers)
				{
					Assert.IsNotNull(entity.WorksForDepartment);
				}
			}
		}
		
		[Test]
		public void ProjectionFetchTestWithTargetPerEntityInheritance()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2( ref toDelete );
				uow.Commit( adapter, true );

				IEntityFields2 fields = new BoardMemberEntityFactory().CreateFields();
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(BoardMemberEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
				filter.Relations.Add(DepartmentEntity.Relations.ManagerEntityUsingManagesDepartmentId);

				IDataReader reader = adapter.FetchDataReader( fields, filter, CommandBehavior.CloseConnection, 0, true );
				while( reader.Read() )
				{
					for( int i = 0; i < reader.FieldCount; i++ )
					{
						Console.Write("{0} | ", reader.GetValue(i));
					}
					Console.WriteLine("");
				}
				reader.Close();

				adapter.DeleteEntityCollection( toDelete );
			}
		}



		/// <summary>
		/// Test which forces the code to follow the the non-optimized path in FetchPrefetchPath to test that code.
		/// </summary>
		[Test]
		public void OneToManyPrefetchPathTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection employees = new EntityCollection(new ManagerEntityFactory());

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.ManagerEntity);
				path.Add(ManagerEntity.PrefetchPathManagesDepartment);

				adapter.ParameterisedPrefetchPathThreshold = 1;
				adapter.FetchEntityCollection(employees, null, path);
				
				Assert.AreEqual(2, employees.Count);

				adapter.DeleteEntityCollection(toDelete);
				foreach(IEntity2 entity in employees)
				{
					Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
				}
			}
		}

	
		[Test]
		public void GetDbCountTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);
				IEntityFields2 fields = EntityFieldsFactory.CreateEntityFieldsObject(EntityType.ManagerEntity);
				ManagerEntity m = new ManagerEntity();

				Assert.AreEqual(2, adapter.GetDbCount(fields, null));

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchSingleEntityWithUCTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ManagerEntity manager = new ManagerEntity();
				manager.Name = "Boardmember 1";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(manager, manager.ConstructFilterForUCName()));

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchNewEntityPolymorphic()
		{
			EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);
				uow.Commit(adapter, true);

				BoardMemberEntity toFetch = new BoardMemberEntity();
				toFetch.Name = "Boardmember 1";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(toFetch, toFetch.ConstructFilterForUCName()));
				int id = toFetch.Id;

				RelationPredicateBucket filter = new RelationPredicateBucket((EmployeeFields.Id == id));
				EmployeeEntity e = (EmployeeEntity)adapter.FetchNewEntity(new EmployeeEntityFactory(), filter);

				BoardMemberEntity b = e as BoardMemberEntity;
				Assert.IsNotNull(b);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void SingleEntityFetchWithContext()
		{
			EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);
				uow.Commit(adapter, true);

				BoardMemberEntity toFetch = new BoardMemberEntity();
				toFetch.Name = "Boardmember 1";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(toFetch, toFetch.ConstructFilterForUCName()));

				Context c = new Context();
				c.Add(toFetch);

				EmployeeEntity e = new BoardMemberEntity(toFetch.Id);
				Assert.IsTrue(adapter.FetchEntity(e, c));
				Assert.AreEqual(e, toFetch);
				Assert.IsTrue((e.Fields==toFetch.Fields));

				EmployeeEntity e2 = (EmployeeEntity)adapter.FetchNewEntity(new BoardMemberEntityFactory(), 
						new RelationPredicateBucket((BoardMemberFields.Id == e.Id)), c);

				Assert.IsTrue((e2==toFetch));
				Assert.AreEqual(e2, toFetch);
				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchWithUniqueConstraintTest()
		{
			EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);
				uow.Commit(adapter, true);

				BoardMemberEntity toFetch = new BoardMemberEntity();
				toFetch.Name = "Boardmember 1";
				Assert.IsTrue(adapter.FetchEntityUsingUniqueConstraint(toFetch, toFetch.ConstructFilterForUCName()));

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void HierarchicalSerializationTest()
		{
			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;

			BoardEntity boardEurope = new BoardEntity();
			boardEurope.Name = "Board Europe";
			
			FamilyCarEntity companyCar1 = new FamilyCarEntity();
			companyCar1.CarId = 1;
			companyCar1.Brand = "Fiat";
			companyCar1.HasDrawHook = true;
			companyCar1.Price = 10000M;

			boardMember1.WorksForDepartment = boardEurope;
			boardMember1.CompanyCar = companyCar1;

			MemoryStream memStream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(memStream, boardMember1);

			// seek back, deserialize in other bucket.
			memStream.Seek(0, SeekOrigin.Begin);
			BoardMemberEntity deserializedBoardMember1 = (BoardMemberEntity)formatter.Deserialize(memStream);
			memStream.Close();
		}


		/// <summary>
		/// Test to see if after a savefields the links to supertype/subtype fields are corrected as well.
		/// </summary>
		[Test]
		public void SaveFieldsCloneWithCorrectLinkCorrectionTest()
		{
			ManagerEntity m = new ManagerEntity();
			// see if the original is linked OK
			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id_Employee], m.Fields[(int)ManagerFieldIndex.Id].LinkedSuperTypeField);
			// subtype field link
			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id], ((EntityField2)m.Fields[(int)ManagerFieldIndex.Id_Employee]).LinkedSubTypeFields[0]);

			m.SaveFields("Test");

			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id_Employee], m.Fields[(int)ManagerFieldIndex.Id].LinkedSuperTypeField);
			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id], ((EntityField2)m.Fields[(int)ManagerFieldIndex.Id_Employee]).LinkedSubTypeFields[0]);			// rollback to set the fields to the clones

			m.RollbackFields("Test");
			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id_Employee], m.Fields[(int)ManagerFieldIndex.Id].LinkedSuperTypeField);
			Assert.AreEqual(m.Fields[(int)ManagerFieldIndex.Id], ((EntityField2)m.Fields[(int)ManagerFieldIndex.Id_Employee]).LinkedSubTypeFields[0]);
		}


		[Test]
		public void ManyToManySaveWithInheritanceTest()
		{
			UserEntity user = new UserEntity();
			user.FirstName = "Sjaak";
			user.LastName = "Trekhaak";
			RoleEntity role = new EmployeeRoleEntity();
			UserRoleEntity userRole = new UserRoleEntity();
			userRole.User = user;
			userRole.Role = role;

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(userRole, true, true));
				Assert.IsTrue(userRole.UserId > 0);
				Assert.IsTrue(userRole.RoleId > 0);
				Assert.IsTrue(role.RoleId > 0);
				Assert.IsTrue(user.UserId > 0);

				// fetch m:n back
				adapter.FetchEntityCollection(user.RoleCollectionViaUserRole, user.GetRelationInfoRoleCollectionViaUserRole());
				Assert.IsTrue((user.RoleCollectionViaUserRole.Count>0));
				Assert.AreEqual(role, user.RoleCollectionViaUserRole[0]);

				Assert.IsTrue(adapter.DeleteEntity(userRole));
				Assert.IsTrue(adapter.DeleteEntity(role));
				Assert.IsTrue(adapter.DeleteEntity(user));
			}
		}


		[Test]
		public void ManyToManySaveWithInheritanceTest2()
		{
			UserEntity user = new UserEntity();
			user.FirstName = "Sjaak";
			user.LastName = "Trekhaak";
			RoleEntity role = new EmployeeRoleEntity();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				Assert.IsTrue(adapter.SaveEntity(role));
				RoleEntity fetchedRole = new EmployeeRoleEntity(role.RoleId);
				Assert.IsTrue(adapter.FetchEntity(fetchedRole));
				UserRoleEntity userRole = new UserRoleEntity();
				userRole.User = user;
				userRole.Role = fetchedRole;

				Assert.IsTrue(adapter.SaveEntity(userRole, true, true));
				Assert.IsTrue(userRole.UserId > 0);
				Assert.IsTrue(userRole.RoleId > 0);
				Assert.IsTrue(user.UserId > 0);

				Assert.IsTrue(adapter.DeleteEntity(userRole));
				Assert.IsTrue(adapter.DeleteEntity(role));
				Assert.IsTrue(adapter.DeleteEntity(user));
			}
		}


		[Test]
		public void DynamicListWithTwoSubTypesWithSameSuperTypeTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(ManagerFields.Name, 0, "ManangerName");
				fields.DefineField(ClerkFields.Name, 1, "ClerkName");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(DepartmentEntity.Relations.ManagerEntityUsingManagesDepartmentId);
				filter.Relations.Add(ClerkEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
                
				DataTable table = new DataTable();
				adapter.FetchTypedList(fields, table, filter);
				Assert.AreEqual(4, table.Rows.Count);
				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void DynamicListWithJoinHintTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(ManagerFields.Name, 0, "Name");
				fields.DefineField(DepartmentFields.Name, 1, "DepartmentName");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(ManagerEntity.Relations.DepartmentEntityUsingManagesDepartmentId, JoinHint.Left);

				DataTable table = new DataTable();
				adapter.FetchTypedList(fields, table, filter);
				adapter.DeleteEntityCollection(toDelete);
			}
		}



		[Test]
		public void PolymorphicPrefetchPathTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);

				uow.Commit(adapter, true);
				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.EmployeeEntity);
				path.Add(BoardMemberEntity.PrefetchPathCompanyCar);

				adapter.FetchEntityCollection(employees, null, path);
				foreach(IEntity2 e in employees)
				{
					BoardMemberEntity b = e as BoardMemberEntity;
					if(b!=null)
					{
						Assert.IsNotNull(b.CompanyCar);
						Console.WriteLine("Boardmember with company car found: {0}", b.Name);
					}
				}
				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void PolymorphicPrefetchPathSingleEntityTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);

				uow.Commit(adapter, true);
				EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
				PrefetchPath2 path = new PrefetchPath2((int)EntityType.EmployeeEntity);
				path.Add(BoardMemberEntity.PrefetchPathCompanyCar);

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(BoardMemberEntity.GetEntityTypeFilter());
				adapter.FetchEntityCollection(employees, filter, path);
				foreach(IEntity2 e in employees)
				{
					BoardMemberEntity b = e as BoardMemberEntity;
					if(b!=null)
					{
						Assert.IsNotNull(b.CompanyCar);
						Console.WriteLine("Boardmember with company car found: {0}", b.Name);
					}
				}

				int id = ((BoardMemberEntity)employees[0]).Id;

				BoardMemberEntity bm = new BoardMemberEntity(id);
				path = new PrefetchPath2((int)EntityType.BoardMemberEntity);
				path.Add(BoardMemberEntity.PrefetchPathCompanyCar);
				Assert.IsTrue(adapter.FetchEntity(bm, path));

				Assert.IsNotNull(bm.CompanyCar);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void DynamicListFetchWithSingleEntity()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(3);
				fields.DefineField(BoardMemberFields.Id, 0, "Id");
				fields.DefineField(BoardMemberFields.Name, 1, "Name");
				fields.DefineField(BoardMemberFields.StartDate, 2, "StartDate");

				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, null);

				Assert.AreEqual("Boardmember 1", results.Rows[0][1]);
				Assert.AreEqual(1, results.Rows.Count);
				
				adapter.DeleteEntityCollection(toDelete);
			}
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertDeleteBoardMember()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				BoardMemberEntity newBoardMember = new BoardMemberEntity();
				// Employee's fields
				newBoardMember.Name = "Sjaak Trekhaak";
				newBoardMember.StartDate = DateTime.Now;
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newBoardMember, true));
				Assert.IsTrue(newBoardMember.Id > 0);
				Assert.IsTrue(adapter.DeleteEntity(newBoardMember));

				newBoardMember = new BoardMemberEntity();
				// Employee's fields
				newBoardMember.Name = "Sjaak Trekhaak";
				newBoardMember.StartDate = DateTime.Now;
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newBoardMember, true));
				PredicateExpression additionalFilter = new PredicateExpression();
				additionalFilter.Add(BoardMemberFields.Name =="Sjaak Trekhaak");
				Assert.IsTrue(adapter.DeleteEntity(newBoardMember, additionalFilter));
			}
		}


		[Test]
		public void InsertUpdateDeleteClerk()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				ClerkEntity newClerk = new ClerkEntity();
				// Employee's fields
				newClerk.Name = "Sjaak Trekhaak";
				newClerk.StartDate = DateTime.Now;
				newClerk.JobDescription = "Pushing Paper";
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newClerk, true));

				ClerkEntity refetchedClerk = new ClerkEntity(newClerk.Id);
				Assert.IsTrue(adapter.FetchEntity(refetchedClerk));

				// update jobdescription
				refetchedClerk.JobDescription = "Managing boxes";
				Assert.IsTrue(adapter.SaveEntity(refetchedClerk));

				// delete it
				Assert.IsTrue(adapter.DeleteEntity(refetchedClerk));
			}
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertDeleteBoardMemberWithLimitationFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				BoardMemberEntity newBoardMember = new BoardMemberEntity();
				// Employee's fields
				newBoardMember.Name = "Sjaak Trekhaak";
				newBoardMember.StartDate = DateTime.Now;
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newBoardMember, true));
				PredicateExpression additionalFilter = new PredicateExpression();
				additionalFilter.Add(BoardMemberFields.Name == "This Name Is Wrong");
				// delete should fail, as the additional predicate is preventing deletion.
				try
				{
					Assert.IsFalse(adapter.DeleteEntity(newBoardMember, additionalFilter));
				}
				catch(ORMConcurrencyException)
				{
					// expected
				}

				BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
				Assert.IsTrue(adapter.FetchEntity(refetchedBoardMember));

				additionalFilter = new PredicateExpression();
				additionalFilter.Add(BoardMemberFields.Name == "Sjaak Trekhaak");
				Assert.IsTrue(adapter.DeleteEntity(newBoardMember, additionalFilter));
				// should fail, everything is already gone.
				Assert.IsFalse(adapter.DeleteEntity(refetchedBoardMember));
			}
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertUpdateDeleteBoardMember()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				BoardMemberEntity newBoardMember = new BoardMemberEntity();
				// Employee's fields
				newBoardMember.Name = "Sjaak Trekhaak";
				newBoardMember.StartDate = DateTime.Now;
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newBoardMember, true));

				BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
				Assert.IsTrue(adapter.FetchEntity(refetchedBoardMember));

				// update name, which is in employee.
				refetchedBoardMember.Name = "Sjaak Trekhaak Changed";
				Assert.IsTrue(adapter.SaveEntity(refetchedBoardMember));

				// delete it
				Assert.IsTrue(adapter.DeleteEntity(refetchedBoardMember));
			}
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertUpdateDeleteBoardMemberWithAdditionalFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				BoardMemberEntity newBoardMember = new BoardMemberEntity();
				// Employee's fields
				newBoardMember.Name = "Sjaak Trekhaak";
				newBoardMember.StartDate = DateTime.Now;
				// rest is null.

				Assert.IsTrue(adapter.SaveEntity(newBoardMember, true));

				BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
				Assert.IsTrue(adapter.FetchEntity(refetchedBoardMember));

				// update name, which is in employee.
				refetchedBoardMember.Name = "Sjaak Trekhaak Changed";

				PredicateExpression additionalFilter = new PredicateExpression();
				additionalFilter.Add(BoardMemberFields.Name == "This Name Is Wrong");

				// fails
				try
				{
					adapter.SaveEntity(refetchedBoardMember, false, additionalFilter);
				}
				catch(Exception ex)
				{
					Assert.IsNotNull(ex as ORMConcurrencyException);
				}

				adapter.DeleteEntity(newBoardMember);
			}
		}


		/// <summary>
		/// Tests TargetPerEntity fetch logic.
		/// </summary>
		[Test]
		public void FetchAllEmployeesAfterUoWTest()
		{
			EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);

				uow.Commit(adapter, true);
				adapter.FetchEntityCollection(employees, null);
				adapter.DeleteEntityCollection(toDelete);
			}

			Assert.AreEqual(4, employees.Count);
			foreach(IEntity2 entity in employees)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		/// <summary>
		/// Tests TargetPerEntity fetch logic. Fetches all Clerks or Boardmembers
		/// </summary>
		[Test]
		public void FetchAllManagersWithTypeFilter()
		{
			EntityCollection managers = new EntityCollection(new ManagerEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.PredicateExpression.Add(ManagerEntity.GetEntityTypeFilter());

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);

				uow.Commit(adapter, true);
				adapter.FetchEntityCollection(managers, filter);
				adapter.DeleteEntityCollection(toDelete);
			}

			Assert.AreEqual(2, managers.Count);
			foreach(IEntity2 entity in managers)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}



		/// <summary>
		/// Tests TargetPerEntity fetch logic. Fetches all Clerks or Boardmembers
		/// </summary>
		[Test]
		public void FetchAllEmployeesWithTypeFilter()
		{
			EntityCollection employees = new EntityCollection(new EmployeeEntityFactory());
			EntityCollection toDelete = new EntityCollection();
			RelationPredicateBucket filter = new RelationPredicateBucket();
			filter.PredicateExpression.Add(new FieldCompareNullPredicate(BoardMemberFields.Id, null, true));
			filter.PredicateExpression.AddWithOr(new FieldCompareNullPredicate(ClerkFields.Id, null, true));

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				UnitOfWork2 uow = CreateTestBatchForSave(ref toDelete);

				uow.Commit(adapter, true);
				adapter.FetchEntityCollection(employees, filter);
				adapter.DeleteEntityCollection(toDelete);
			}

			Assert.AreEqual(3, employees.Count);
			foreach(IEntity2 entity in employees)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		[Test]
		public void FetchAllCompanyCarsWithDepartmentLogic()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);

				uow.Commit(adapter, true);
				RelationPredicateBucket filter = new RelationPredicateBucket();
				// use relation between company car and boardmember.
				filter.Relations.Add(CompanyCarEntity.Relations.BoardMemberEntityUsingCompanyCarId);
				// use relation inherited from employee
				filter.Relations.Add(BoardMemberEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
				// add the filter on departmentname
				filter.PredicateExpression.Add(DepartmentFields.Name == "Board Europe");
				EntityCollection companyCars = new EntityCollection(new CompanyCarEntityFactory());
				adapter.FetchEntityCollection(companyCars, filter);
				Assert.AreEqual(1, companyCars.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}


		[Test]
		public void FetchAllDepartmentsWithBoardmemberLogic()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(DepartmentEntity.Relations.EmployeeEntityUsingWorksForDepartmentId);
				// specify boardmember as the starting point, as boardmember is a subtype of employee.
				filter.Relations.Add(BoardMemberEntity.Relations.CompanyCarEntityUsingCompanyCarId);
				filter.PredicateExpression.Add(CompanyCarFields.Brand == "Fiat");
				EntityCollection departments = new EntityCollection(new DepartmentEntityFactory());
				adapter.FetchEntityCollection(departments, filter);
				Assert.AreEqual(1, departments.Count);

				adapter.DeleteEntityCollection(toDelete);
			}
		}

		[Test]
		public void FetchAllManagersWithDepartmentLogic()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection managers = new EntityCollection(new ManagerEntityFactory());

				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.Relations.Add(ManagerEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
				filter.PredicateExpression.Add(DepartmentFields.Name == "Board Europe");
				adapter.FetchEntityCollection(managers, filter);
				Assert.AreEqual(1, managers.Count);
				
				adapter.DeleteEntityCollection(toDelete);

				foreach(IEntity2 entity in managers)
				{
					Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
				}
			}
		}


		[Test]
		public void FetchAllManagers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection managers = new EntityCollection(new ManagerEntityFactory());
				adapter.FetchEntityCollection(managers, null);
				Assert.AreEqual(2, managers.Count);
				
				adapter.DeleteEntityCollection(toDelete);

				foreach(IEntity2 entity in managers)
				{
					Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
				}
			}
		}


		[Test]
		public void FetchAllMarketingDepartments()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection marketingDeps = new EntityCollection(new MarketingEntityFactory());
				adapter.FetchEntityCollection(marketingDeps, null);
				Assert.AreEqual(1, marketingDeps.Count);
				
				adapter.DeleteEntityCollection(toDelete);

				foreach(IEntity2 entity in marketingDeps)
				{
					Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
				}
			}
		}


		/// <summary>
		/// Tests TargetPerEntityHierarchy fetch logic.
		/// </summary>
		[Test]
		public void FetchAllDepartments()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				EntityCollection departments = new EntityCollection(new DepartmentEntityFactory());
				adapter.FetchEntityCollection(departments, null);

				Assert.AreEqual(2, departments.Count);

				adapter.DeleteEntityCollection(toDelete);

				foreach(IEntity2 entity in departments)
				{
					Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
				}
			}
		}


		[Test]
		public void FetchManagerAndRelatedDepartment()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				MarketingEntity marketingEurope = new MarketingEntity();
				marketingEurope.Name = "Marketing Europe";
				ManagerEntity manager1 = new ManagerEntity();
				manager1.Name = "Manager 1";
				manager1.StartDate = DateTime.Now;
				manager1.WorksForDepartment = marketingEurope;
				manager1.ManagesDepartment = marketingEurope;

				Assert.IsTrue(adapter.SaveEntity(manager1, false));

				EntityCollection toDelete = new EntityCollection();
				toDelete.Add(manager1);
				toDelete.Add(marketingEurope);

				ManagerEntity m = new ManagerEntity(manager1.Id);

				PrefetchPath2 path = new PrefetchPath2((int)EntityType.ManagerEntity);
				path.Add(ManagerEntity.PrefetchPathManagesDepartment);
				Assert.IsTrue( adapter.FetchEntity(m, path) );

				adapter.DeleteEntityCollection(toDelete);
				Assert.AreEqual( typeof(MarketingEntity), m.ManagesDepartment.GetType());
			}
		}


		[Test]
		public void UpdateAllBoardmembersDirectly()
		{
			BoardMemberEntity newValues = new BoardMemberEntity();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				// give all boardmembers a new startdate.
				DateTime newStartDate = DateTime.Now;
				newStartDate = newStartDate.AddMilliseconds(0-newStartDate.Millisecond);
				newValues.StartDate = newStartDate;

				Assert.IsTrue( (adapter.UpdateEntitiesDirectly(newValues, null) > 0));
				Thread.Sleep(1000);
				EntityCollection allBoardMembers = new EntityCollection(new BoardMemberEntityFactory());
				// fetch all boardmembers and verify the updated date
				adapter.FetchEntityCollection(allBoardMembers, null);
				foreach(BoardMemberEntity boardMember in allBoardMembers)
				{
					// do a compare on date and hour/minute/second, as sqlserver rounds milliseconds so a full compare is not valid
					Assert.IsTrue((newStartDate.Date==boardMember.StartDate.Date) && 
						(newStartDate.Hour==boardMember.StartDate.Hour) &&
						(newStartDate.Minute==boardMember.StartDate.Minute) &&
						(newStartDate.Second==boardMember.StartDate.Second));
				}

				adapter.DeleteEntityCollection(toDelete);
			}
		}



		[Test]
		public void UpdateSingleBoardmemberDirectlyThroughFilter()
		{
			BoardMemberEntity newValues = new BoardMemberEntity();
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				// give all boardmembers a new startdate.
				DateTime newStartDate = DateTime.Now;
				newStartDate = newStartDate.AddMilliseconds(0 - newStartDate.Millisecond);
				newValues.StartDate = newStartDate;

				RelationPredicateBucket filter = new RelationPredicateBucket(BoardMemberFields.CompanyCarId==1);

				Assert.IsTrue((adapter.UpdateEntitiesDirectly(newValues, filter)> 0));
				Thread.Sleep(1000);
				EntityCollection allBoardMembers = new EntityCollection(new BoardMemberEntityFactory());
				// fetch all boardmembers and verify the updated date
				adapter.FetchEntityCollection(allBoardMembers, null);
				foreach(BoardMemberEntity boardMember in allBoardMembers)
				{
					// do a compare on date and hour/minute/second, as sqlserver rounds milliseconds so a full compare is not valid
					Assert.IsTrue((newStartDate.Date == boardMember.StartDate.Date) &&
						(newStartDate.Hour == boardMember.StartDate.Hour) &&
						(newStartDate.Minute == boardMember.StartDate.Minute) &&
						(newStartDate.Second == boardMember.StartDate.Second));
				}

				adapter.DeleteEntityCollection(toDelete);
			}
		}



		/// <summary>
		/// Test which fetches fields from different targets of the same hierarchical entity into a dynamic list, and uses a filter without a relation.
		/// This test is to see if the adapter can find out the relations to build for the fields. 
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(BoardMemberFields.Name, 0, "Name");
				fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(BoardMemberFields.CompanyCarId == 1);
				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);
				Assert.IsTrue( (results.Rows.Count>0));

				adapter.DeleteEntityCollection(toDelete);

				foreach(DataRow row in results.Rows)
				{
					Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
				}
			}
		}

		/// <summary>
		/// Same as SingleEntityMultipleTargetDynamicListTest, but now WITH a relation. 
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTestWithRelation()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection toDelete = new EntityCollection();
				UnitOfWork2 uow = CreateTestBatchForSave2(ref toDelete);
				uow.Commit(adapter, true);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(BoardMemberFields.Name, 0, "Name");
				fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(CompanyCarFields.Brand == "Fiat");
				filter.Relations.Add(BoardMemberEntity.Relations.CompanyCarEntityUsingCompanyCarId);
				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);
				Assert.IsTrue( (results.Rows.Count>0));

				adapter.DeleteEntityCollection(toDelete);

				foreach(DataRow row in results.Rows)
				{
					Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
				}
			}
		}


		[Test]
		public void SubtypeTargetPerEntityHierarchyFetch()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				EntityCollection<CompanyCarEntity> toSave = new EntityCollection<CompanyCarEntity>(new CompanyCarEntityFactory());

				// insert test data.
				FamilyCarEntity fiat = new FamilyCarEntity();
				fiat.CarId = 1;
				fiat.Brand = "Fiat";
				fiat.HasDrawHook = true;
				fiat.Price = 10000M;
				toSave.Add(fiat);


				SportsCarEntity porsche = new SportsCarEntity();
				porsche.CarId = 2;
				porsche.Brand = "Porsche";
				porsche.IsCabrio = true;
				porsche.Price = 150000M;
				toSave.Add(porsche);

				Assert.AreEqual(2, adapter.SaveEntityCollection(toSave));

				EntityCollection<FamilyCarEntity> familyCars = new EntityCollection<FamilyCarEntity>();
				adapter.FetchEntityCollection(familyCars, null);

				Assert.AreEqual(1, familyCars.Count);
				Assert.AreEqual("Fiat", familyCars[0].Brand);

				Assert.AreEqual(2, adapter.DeleteEntityCollection(toSave));
			}
		}


		/// <summary>
		/// Same as SingleEntityMultipleTargetDynamicListTest, but now WITH a relation. AND aliases
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTestWithRelationWithAlias()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				MarketingEntity marketingEurope = new MarketingEntity();
				marketingEurope.Name = "Marketing Europe";
				BoardMemberEntity boardMember1 = new BoardMemberEntity();
				boardMember1.Name = "Boardmember 1";
				boardMember1.StartDate = DateTime.Now;
				boardMember1.ManagesDepartment = marketingEurope;
				Assert.IsTrue(adapter.SaveEntity(boardMember1, false));

				EntityCollection toDelete = new EntityCollection();
				toDelete.Add(boardMember1);
				toDelete.Add(marketingEurope);

				ResultsetFields fields = new ResultsetFields(2);
				fields.DefineField(BoardMemberFields.Name, 0, "Name", "BM");
				fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID", "BM");
				RelationPredicateBucket filter = new RelationPredicateBucket();
				filter.PredicateExpression.Add(DepartmentFields.DepartmentId.SetObjectAlias("D") == marketingEurope.DepartmentId);
				filter.Relations.Add(BoardMemberEntity.Relations.DepartmentEntityUsingManagesDepartmentId, "BM", "D", JoinHint.None);
				DataTable results = new DataTable();
				adapter.FetchTypedList(fields, results, filter);

				Assert.IsTrue( (results.Rows.Count>0));
				adapter.DeleteEntityCollection(toDelete);

				foreach(DataRow row in results.Rows)
				{
					Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
				}
			}
		}


		internal UnitOfWork2 CreateTestBatchForSave( ref EntityCollection toDelete )
		{
			UnitOfWork2 uow = new UnitOfWork2();
			// first insert 2 clerks, a manager and a boardmember
			ClerkEntity clerk1 = new ClerkEntity();
			clerk1.Name = "Clerk 1";
			clerk1.StartDate = DateTime.Now;
			clerk1.JobDescription = "pushing paper";
			uow.AddForSave(clerk1, null, false, false);

			ClerkEntity clerk2 = new ClerkEntity();
			clerk2.Name = "Clerk 2";
			clerk2.StartDate = DateTime.Now;
			clerk2.JobDescription = "pushing paper";
			uow.AddForSave(clerk2, null, false, false);

			ManagerEntity manager1 = new ManagerEntity();
			manager1.Name = "Manager 1";
			manager1.StartDate = DateTime.Now;
			uow.AddForSave(manager1, null, false, false);

			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;
			uow.AddForSave(boardMember1, null, false, false);

			toDelete.Add(clerk1);
			toDelete.Add(clerk2);
			toDelete.Add(manager1);
			toDelete.Add(boardMember1);

			return uow;
		}

		[Test]
		public void CreateTestData()
		{
			EntityCollection toDelete = new EntityCollection();
			var uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new DataAccessAdapter(), true);
		}


		internal UnitOfWork2 CreateTestBatchForSave2(ref EntityCollection toDelete)
		{
			UnitOfWork2 uow = new UnitOfWork2();

			MarketingEntity marketingEurope = new MarketingEntity();
			marketingEurope.Name = "Marketing Europe";
			BoardEntity boardEurope = new BoardEntity();
			boardEurope.Name = "Board Europe";

			FamilyCarEntity companyCar1 = new FamilyCarEntity();
			companyCar1.CarId = 1;
			companyCar1.Brand = "Fiat";
			companyCar1.HasDrawHook = true;
			companyCar1.Price = 10000M;

			ClerkEntity clerk1 = new ClerkEntity();
			clerk1.Name = "Clerk 1";
			clerk1.StartDate = DateTime.Now;
			clerk1.JobDescription = "pushing paper";
			clerk1.WorksForDepartment = marketingEurope;
			uow.AddForSave(clerk1);

			ClerkEntity clerk2 = new ClerkEntity();
			clerk2.Name = "Clerk 2";
			clerk2.StartDate = DateTime.Now;
			clerk2.JobDescription = "pushing paper";
			clerk2.WorksForDepartment = marketingEurope;
			uow.AddForSave(clerk2);

			ManagerEntity manager1 = new ManagerEntity();
			manager1.Name = "Manager 1";
			manager1.StartDate = DateTime.Now;
			manager1.WorksForDepartment = marketingEurope;
			manager1.ManagesDepartment = marketingEurope;
			uow.AddForSave(manager1);

			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;
			boardMember1.WorksForDepartment = boardEurope;
			boardMember1.ManagesDepartment = marketingEurope;
			boardMember1.CompanyCar = companyCar1;
			uow.AddForSave(boardMember1);

			toDelete.Add(clerk1);
			toDelete.Add(clerk2);
			toDelete.Add(manager1);
			toDelete.Add(boardMember1);
			toDelete.Add(companyCar1);
			toDelete.Add(marketingEurope);
			toDelete.Add(boardEurope);
			return uow;
		}


		private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_eventHandlerVisited = true;
		}

		private void EntityContentsChangedHandler( object sender, EventArgs e )
		{
			_eventFiredCounter++;
		}


		[TearDown]
		public void CleanUp()
		{
			ActionProcedures.ClearAll();
		}
	}
}
