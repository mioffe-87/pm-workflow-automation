using System;
using System.Data;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using NUnit.Framework;

using InheritanceOne.Dal.SelfServicing.DaoClasses;
using InheritanceOne.Dal.SelfServicing.EntityClasses;
using InheritanceOne.Dal.SelfServicing.FactoryClasses;
using InheritanceOne.Dal.SelfServicing.HelperClasses;
using InheritanceOne.Dal.SelfServicing.RelationClasses;
using InheritanceOne.Dal.SelfServicing.TypedListClasses;
using InheritanceOne.Dal.SelfServicing.CollectionClasses;
using InheritanceOne.Dal.SelfServicing;

using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Collections;
using InheritanceOne.Dal.SelfServicing.StoredProcedureCallerClasses;

namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
{
	/// <summary>
	/// Generic unit tests for the inheritance db using adapter.
	/// </summary>
	[TestFixture()]
	public class InheritanceOneUnitTests
	{
		[Test]
		public void TypedListWithMultipleEntitiesSameHierarchyTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			int count = 0;
			ClerkManagersTypedList list = new ClerkManagersTypedList();
			list.Fill();
			Assert.AreEqual(4, list.Count);

			count = list.GetDbCount();
			Assert.AreEqual(4, count);

			IEntityFields clerkFields = new ClerkEntityFactory().CreateFields();
			ResultsetFields testFields = new ResultsetFields(clerkFields.Count);
			for(int i = 0; i < clerkFields.Count; i++)
			{
				testFields.DefineField(clerkFields[i], i);
			}
			TypedListDAO dao = new TypedListDAO();
			count = dao.GetDbCount(testFields, null, null, null, null);
			Assert.AreEqual(2, count);

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void FieldCompareValueTestOnSubtypeFieldsWithSuperTypeCollection()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			EmployeeCollection employees = new EmployeeCollection();
			employees.GetMulti(null);
			List<int> matches = employees.FindMatches(ClerkFields.JobDescription == "pushing paper");
			Assert.AreEqual(2, matches.Count);

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void DynamicListWithAutoTypeFilterTest()
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
			Assert.IsTrue(companyCar1.Save());
			Assert.IsTrue(companyCar2.Save());
			Assert.IsTrue(sportsCar1.Save());

			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(FamilyCarFields.Brand, 0);
			fields.DefineField(GermanFamilyCarFields.Price, 1);
			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();

			int count = dao.GetDbCount(fields, null, null, null, null, true);
			Assert.AreEqual(1, count);

			dao.GetMultiAsDataTable(fields, results, 0, null, null, null, true, null, null, 0, 0);
			Assert.AreEqual(1, results.Rows.Count);

			companyCar2.Delete();
			companyCar1.Delete();
			sportsCar1.Delete();
		}


		[Test]
		public void XmlSerializationDeserializationTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			PrefetchPath path = new PrefetchPath((int)EntityType.EmployeeEntity);
			path.Add(BoardMemberEntity.PrefetchPathCompanyCar);
			path.Add(EmployeeEntity.PrefetchPathWorksForDepartment);

			EmployeeCollection employees = new EmployeeCollection();
			employees.GetMulti(null, path);

			string verboseXML = string.Empty;
			employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks, out verboseXML);
			string compactXML = string.Empty;
			employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks | XmlFormatAspect.Compact, out compactXML);
			string compact25XML = string.Empty;
			employees.WriteXml(XmlFormatAspect.DatesInXmlDataType | XmlFormatAspect.MLTextInCDataBlocks | XmlFormatAspect.Compact25, out compact25XML);

			EmployeeCollection eeVerbose = new EmployeeCollection();
			eeVerbose.ReadXml(verboseXML);

			EmployeeCollection eeCompact = new EmployeeCollection();
			eeCompact.ReadXml(compactXML);

			EmployeeCollection eeCompact25 = new EmployeeCollection();
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

			DeleteSavedEntities(toDelete);
		}

		
		[Test]
		public void ResaveAfterRejectionWithValidator()
		{
			MarketingEntity department = new MarketingEntity();
			department.Name = "Marketing EU";
			department.Save();

			ManagerEntity toSave = new ManagerEntity();
			//toSave.Validator = new ManagerValidator();
			toSave.Name = "Sjaak Trekhaak";
			toSave.StartDate = DateTime.Now;
			try
			{
				toSave.Save();
				Assert.IsTrue(((int)toSave.Fields[(int)ManagerFieldIndex.Id].CurrentValue > 0));
				Assert.IsTrue((toSave.Id > 0));
			}
			catch(ORMEntityValidationException ex)
			{
				Console.WriteLine("Exception caught.");
				Assert.AreEqual(ex.EntityValidated, toSave);
				Assert.AreEqual("ManagerEntity.ManagesDepartmentId is required.", ex.Message);
			}
			toSave.ManagesDepartmentId = department.DepartmentId;
			Assert.IsTrue(toSave.Save());
			Assert.IsTrue((toSave.Id > 0));

			toSave.Delete();
			department.Delete();
		}

		
		/// <summary>
		/// Fetches with excluded fields test.
		/// </summary>
		[Test]
		public void FetchesWithExcludedFieldsTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ExcludeIncludeFieldsList excludedFields = new ExcludeIncludeFieldsList();
			excludedFields.Add(EmployeeFields.Name);
			excludedFields.Add(BoardMemberFields.StartDate);
			excludedFields.Add(ClerkFields.JobDescription);

			EmployeeCollection employees = new EmployeeCollection();
			EmployeeCollection employeesVerification = new EmployeeCollection();
			SortExpression sorter = new SortExpression(EmployeeFields.Id | SortOperator.Ascending);
			employees.GetMulti(null, 0, sorter, null, null, excludedFields, 0, 0);
			employeesVerification.GetMulti(null, 0, sorter);

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

			// load excluded fields. 
			employees.FetchExcludedFields(excludedFields);
			for(int i = 0; i < employees.Count; i++)
			{
				Assert.AreEqual(employees[i].Name, employeesVerification[i].Name);
				Assert.AreEqual(employees[i].StartDate, employeesVerification[i].StartDate);
				if(employees[i] is ClerkEntity)
				{
					Assert.AreEqual(((ClerkEntity)employees[i]).JobDescription, ((ClerkEntity)employeesVerification[i]).JobDescription);
				}
			}

			DeleteSavedEntities(toDelete);
		}


		/// <summary>
		/// Tests a dyn. list with a join with 2 instances of the base type. 
		/// </summary>
		[Test]
		public void FetchDynamicListWithTwoInstancesOfBaseType()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ResultsetFields fields = new ResultsetFields(3);
			fields.DefineField(ClerkFields.Name, 0, "ClerkName");
			fields.DefineField(DepartmentFields.Name, 1, "DepartmentName");
			fields.DefineField(ManagerFields.Name, 2, "ManagerName");

			RelationCollection relations = new RelationCollection();
			relations.Add(ClerkEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
			relations.Add(DepartmentEntity.Relations.ManagerEntityUsingManagesDepartmentId);

			SortExpression sorter = new SortExpression(ClerkFields.Name | SortOperator.Ascending);
			sorter.Add(ManagerFields.Name | SortOperator.Ascending);

			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			int count = dao.GetDbCount(fields, null, null, relations, null, true);
			Assert.AreEqual(4, count);
			dao.GetMultiAsDataTable(fields, results, 0, sorter, null, relations, false, null, null, 0, 0);

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
			dao.GetMultiAsDataTable(fields, results, 0, sorter, null, relations, false, null, null, 0, 0);

			DeleteSavedEntities(toDelete);
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
			using(IH1TestScope outerScope = new IH1TestScope())
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
		

		[Test]
		public void ComplexPrefetchPathWithInheritedNodeTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			// fetch CompanyCar - BoardMember - Worksfordepartment. This utilizes a node inherited from employee
			FamilyCarEntity c = new FamilyCarEntity();
			PrefetchPath path = new PrefetchPath((int)EntityType.CompanyCarEntity);
			path.Add(CompanyCarEntity.PrefetchPathBoardMember).SubPath.Add(BoardMemberEntity.PrefetchPathWorksForDepartment);
			DaoBase.ParameterisedPrefetchPathThreshold = 1;

			c.FetchUsingPK(1, path);
			Assert.AreEqual(c.BoardMember.Count, 1);
			Assert.IsNotNull(c.BoardMember[0].WorksForDepartment);

			DeleteSavedEntities(toDelete);
		}

		[Test]
		public void GetDbCountTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ManagerCollection managers = new ManagerCollection();
			Assert.AreEqual(2, managers.GetDbCount());
			DeleteSavedEntities(toDelete);
		}


		/// <summary>
		/// Test which forces the code to follow the the non-optimized path in FetchPrefetchPath to test that code.
		/// </summary>
		[Test]
		public void GetScalarTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			BoardMemberCollection bmc = new BoardMemberCollection();
			int amount = (int)bmc.GetScalar(BoardMemberFieldIndex.Id, null, AggregateFunction.CountDistinct, (EmployeeFields.Name == "Boardmember 1"));
			Assert.AreEqual(1, amount);

			DeleteSavedEntities(toDelete);
		}



		/// <summary>
		/// Test which forces the code to follow the the non-optimized path in FetchPrefetchPath to test that code.
		/// </summary>
		[Test]
		public void OneToManyPrefetchPathTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ManagerCollection employees = new ManagerCollection();

			PrefetchPath path = new PrefetchPath((int)EntityType.ManagerEntity);
			path.Add(ManagerEntity.PrefetchPathManagesDepartment);

			DaoBase.ParameterisedPrefetchPathThreshold = 1;
			employees.GetMulti(null, path);
			DaoBase.ParameterisedPrefetchPathThreshold=50;
			
			Assert.AreEqual(2, employees.Count);

			DeleteSavedEntities(toDelete);
			foreach(IEntity entity in employees)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		[Test]
		public void SingleEntityPolymorphicFetchUsingUC()
		{
			EmployeeCollection employees = new EmployeeCollection();
			List<IEntity> toDelete = new List<IEntity>();

			UnitOfWork uow = CreateTestBatchForSave(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			// fetch boardmember with name 'Boardmember 1' back.
			BoardMemberEntity toFetch = (BoardMemberEntity)EmployeeEntity.FetchPolymorphicUsingUCName(null, "Boardmember 1", null);
			Assert.AreEqual(typeof(BoardMemberEntity), toFetch.GetType());
			Assert.AreEqual(EntityState.Fetched, toFetch.Fields.State);

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void SingleEntityFetchWithContext()
		{
			EmployeeCollection employees = new EmployeeCollection();
			List<IEntity> toDelete = new List<IEntity>();

			UnitOfWork uow = CreateTestBatchForSave(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			// fetch boardmember with name 'Boardmember 1' back.
			BoardMemberEntity toFetch = new BoardMemberEntity();
			Assert.IsTrue(toFetch.FetchUsingUCName("Boardmember 1"));

			Context c = new Context();
			c.Add(toFetch);

			EmployeeEntity e = new BoardMemberEntity();
			Assert.IsTrue(e.FetchUsingPK(toFetch.Id, null, c));
			Assert.AreEqual(e, toFetch);
			Assert.IsTrue((e.Fields==toFetch.Fields));

			EmployeeEntity e2 = EmployeeEntity.FetchPolymorphic(null, toFetch.Id, c);

			Assert.IsTrue((e2==toFetch));
			Assert.AreEqual(e2, toFetch);

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void FetchWithUniqueConstraintTest()
		{
			EmployeeCollection employees = new EmployeeCollection();
			List<IEntity> toDelete = new List<IEntity>();

			UnitOfWork uow = CreateTestBatchForSave(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			// fetch boardmember with name 'Boardmember 1' back.
			BoardMemberEntity fetchBoardMember = new BoardMemberEntity();
			Assert.IsTrue(fetchBoardMember.FetchUsingUCName("Boardmember 1"));

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void PolymorphicPrefetchPathTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			EmployeeCollection employees = new EmployeeCollection();
			PrefetchPath path = new PrefetchPath((int)EntityType.EmployeeEntity);
			path.Add(BoardMemberEntity.PrefetchPathCompanyCar);

			employees.GetMulti(null, path);
			foreach(IEntity e in employees)
			{
				BoardMemberEntity b = e as BoardMemberEntity;
				if(b!=null)
				{
					Assert.IsNotNull(b.CompanyCar);
					Console.WriteLine("Boardmember with company car found: {0}", b.Name);
				}
			}

			DeleteSavedEntities(toDelete);
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

			Assert.IsTrue(userRole.Save(true));
			Assert.IsTrue(userRole.UserId > 0);
			Assert.IsTrue(userRole.RoleId > 0);
			Assert.IsTrue(role.RoleId > 0);
			Assert.IsTrue(user.UserId > 0);
		}


		[Test]
		public void ManyToManySaveWithInheritanceTest2()
		{
			UserEntity user = new UserEntity();
			user.FirstName = "Sjaak";
			user.LastName = "Trekhaak";
			RoleEntity role = new EmployeeRoleEntity();
			Assert.IsTrue(role.Save());
			RoleEntity fetchedRole = RoleEntity.FetchPolymorphic(null, role.RoleId, null);
			UserRoleEntity userRole = new UserRoleEntity();
			userRole.User = user;
			userRole.Role = fetchedRole;

			Assert.IsTrue(userRole.Save(true));
			Assert.IsTrue(userRole.UserId > 0);
			Assert.IsTrue(userRole.RoleId > 0);
			Assert.IsTrue(user.UserId > 0);

			// fetch m:n relation back
			RoleCollection roles = user.RoleCollectionViaUserRole;
			Assert.IsTrue((roles.Count>0));
			Assert.AreEqual(roles[0], role);
		}


		[Test]
		public void DynamicListWithSubTypeFetchTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ResultsetFields fields = new ResultsetFields(3);
			fields.DefineField(BoardMemberFields.Id, 0, "Id");
			fields.DefineField(BoardMemberFields.Name, 1, "Name");
			fields.DefineField(BoardMemberFields.StartDate, 2, "StartDate");

			TypedListDAO dao = new TypedListDAO();
			DataTable results = new DataTable();
			dao.GetMultiAsDataTable(fields, results, 0, null, null, null, true, null, null, 0, 0);

			Assert.AreEqual(1, results.Rows.Count);

			DeleteSavedEntities(toDelete);
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertDeleteManager()
		{
			ManagerEntity newManager = new ManagerEntity();
			// Employee's fields
			newManager.Name = "Sjaak Trekhaak";
			newManager.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newManager.Save());
			Assert.IsTrue( newManager.Delete());

			newManager = new ManagerEntity();
			// Employee's fields
			newManager.Name = "Sjaak Trekhaak";
			newManager.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newManager.Save());
			PredicateExpression additionalFilter = new PredicateExpression();
			additionalFilter.Add(ManagerFields.Name == "Sjaak Trekhaak");
			Assert.IsTrue(newManager.Delete(additionalFilter));
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertDeleteBoardMember()
		{
			BoardMemberEntity newBoardMember = new BoardMemberEntity();
			// Employee's fields
			newBoardMember.Name = "Sjaak Trekhaak";
			newBoardMember.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newBoardMember.Save());
			newBoardMember.Refetch();
			Assert.IsTrue( newBoardMember.Delete());

			newBoardMember = new BoardMemberEntity();
			// Employee's fields
			newBoardMember.Name = "Sjaak Trekhaak";
			newBoardMember.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newBoardMember.Save());
			PredicateExpression additionalFilter = new PredicateExpression();
			additionalFilter.Add(BoardMemberFields.Name == "Sjaak Trekhaak");
			Assert.IsTrue(newBoardMember.Delete(additionalFilter));
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertDeleteBoardMemberWithLimitationFilter()
		{
			BoardMemberEntity newBoardMember = new BoardMemberEntity();
			// Employee's fields
			newBoardMember.Name = "Sjaak Trekhaak";
			newBoardMember.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newBoardMember.Save());
			PredicateExpression additionalFilter = new PredicateExpression();
			additionalFilter.Add(BoardMemberFields.Name == "This Name Is Wrong");
			// delete should fail, as the additional predicate is preventing deletion.
			try
			{
				Assert.IsFalse(newBoardMember.Delete(additionalFilter));
			}
			catch(ORMConcurrencyException)
			{
				// expected
			}

			BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
			Assert.AreEqual(EntityState.Fetched, refetchedBoardMember.Fields.State);

			additionalFilter = new PredicateExpression();
			additionalFilter.Add(BoardMemberFields.Name == "Sjaak Trekhaak");
			Assert.IsTrue(newBoardMember.Delete( additionalFilter));
			// should fail, everything is already gone.
			Assert.IsFalse(refetchedBoardMember.Delete());
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertUpdateDeleteBoardMember()
		{
			BoardMemberEntity newBoardMember = new BoardMemberEntity();
			// Employee's fields
			newBoardMember.Name = "Sjaak Trekhaak";
			newBoardMember.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newBoardMember.Save());

			BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
			Assert.AreEqual(EntityState.Fetched, refetchedBoardMember.Fields.State);

			// update name, which is in employee.
			refetchedBoardMember.Name = "Sjaak Trekhaak Changed";
			Assert.IsTrue(refetchedBoardMember.Save());

			// delete it
			Assert.IsTrue(refetchedBoardMember.Delete());
		}


		/// <summary>
		/// Test to insert a new boardmember entity. This entity consists of 3 tables and derives from manager which derives from employee.
		/// </summary>
		[Test]
		public void InsertUpdateDeleteBoardMemberWithAdditionalFilter()
		{
			BoardMemberEntity newBoardMember = new BoardMemberEntity();
			// Employee's fields
			newBoardMember.Name = "Sjaak Trekhaak";
			newBoardMember.StartDate = DateTime.Now;
			// rest is null.

			Assert.IsTrue( newBoardMember.Save());

			BoardMemberEntity refetchedBoardMember = new BoardMemberEntity(newBoardMember.Id);
			Assert.AreEqual(EntityState.Fetched, refetchedBoardMember.Fields.State);

			// update name, which is in employee.
			refetchedBoardMember.Name = "Sjaak Trekhaak Changed";

			PredicateExpression additionalFilter = new PredicateExpression();
			additionalFilter.Add(BoardMemberFields.Name == "This Name Is Wrong");

			// fails
			try
			{
				refetchedBoardMember.Save(additionalFilter);
			}
			catch(Exception ex)
			{
				Assert.IsNotNull(ex as ORMConcurrencyException);
			}

			newBoardMember.Delete();
		}


		/// <summary>
		/// Tests TargetPerConcreteEntity fetch logic.
		/// </summary>
		[Test]
		public void FetchAllEmployeesAfterUoWTest()
		{
			EmployeeCollection employees = new EmployeeCollection();
			List<IEntity> toDelete = new List<IEntity>();

			UnitOfWork uow = CreateTestBatchForSave(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			employees.GetMulti(null);
			DeleteSavedEntities(toDelete);

			Assert.AreEqual(4, employees.Count);
			foreach(IEntity entity in employees)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		/// <summary>
		/// Tests TargetPerConcreteEntity fetch logic. Fetches all Clerks or Boardmembers
		/// </summary>
		[Test]
		public void FetchAllEmployeesWithTypeFilter()
		{
			EmployeeCollection employees = new EmployeeCollection();
			List<IEntity> toDelete = new List<IEntity>();

			PredicateExpression filter = new PredicateExpression();
			filter.Add(new FieldCompareNullPredicate(BoardMemberFields.Id, true));
			filter.AddWithOr(new FieldCompareNullPredicate(ClerkFields.Id, true));

			UnitOfWork uow = CreateTestBatchForSave(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);
			employees.GetMulti(filter);
			DeleteSavedEntities(toDelete);

			Assert.AreEqual(3, employees.Count);
			foreach(IEntity entity in employees)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		[Test]
		public void FetchAllCompanyCarsWithDepartmentLogic()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			PredicateExpression filter = new PredicateExpression();
			RelationCollection relations = new RelationCollection();
			// use relation between company car and boardmember.
			relations.Add(CompanyCarEntity.Relations.BoardMemberEntityUsingCompanyCarId);
			// use relation inherited from employee
			relations.Add(BoardMemberEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
			// add the filter on departmentname
			filter.Add(DepartmentFields.Name=="Board Europe");
			CompanyCarCollection companyCars = new CompanyCarCollection();
			companyCars.GetMulti(filter, relations);
			Assert.AreEqual(1, companyCars.Count);
			DeleteSavedEntities(toDelete);
		}

		[Test]
		public void FetchAllDepartmentsWithBoardmemberLogic()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			PredicateExpression filter = new PredicateExpression();
			RelationCollection relations = new RelationCollection();
			relations.Add(DepartmentEntity.Relations.EmployeeEntityUsingWorksForDepartmentId);
			// specify boardmember as the starting point, as boardmember is a subtype of employee.
			relations.Add(BoardMemberEntity.Relations.CompanyCarEntityUsingCompanyCarId);
			filter.Add(CompanyCarFields.Brand == "Fiat");
			DepartmentCollection departments = new DepartmentCollection();
			departments.GetMulti(filter, relations);
			Assert.AreEqual(1, departments.Count);

			DeleteSavedEntities(toDelete);
		}

		[Test]
		public void FetchAllManagersWithDepartmentLogic()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ManagerCollection managers = new ManagerCollection();

			PredicateExpression filter = new PredicateExpression();
			RelationCollection relations = new RelationCollection();
			relations.Add(ManagerEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);
			filter.Add(DepartmentFields.Name == "Board Europe");
			managers.GetMulti(filter, relations);
			Assert.AreEqual(1, managers.Count);
			
			DeleteSavedEntities(toDelete);

			foreach(IEntity entity in managers)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}


		/// <summary>
		/// Tests TargetPerEntityHierarchy fetch logic.
		/// </summary>
		[Test]
		public void FetchAllDepartments()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			DepartmentCollection departments = new DepartmentCollection();
			departments.GetMulti(null);

			Assert.AreEqual(2, departments.Count);

			DeleteSavedEntities(toDelete);

			foreach(IEntity entity in departments)
			{
				Console.WriteLine("ID: {0}. Type: {1}", entity.GetCurrentFieldValue(0), entity.GetType().ToString());
			}
		}

		[Test]
		public void FetchManagerAndRelatedDepartment()
		{
			MarketingEntity marketingEurope = new MarketingEntity();
			marketingEurope.Name = "Marketing Europe";
			ManagerEntity manager1 = new ManagerEntity();
			manager1.Name = "Manager 1";
			manager1.StartDate = DateTime.Now;
			manager1.WorksForDepartment = marketingEurope;
			manager1.ManagesDepartment = marketingEurope;

			Assert.IsTrue(manager1.Save(true));

			List<IEntity> toDelete = new List<IEntity>();
			toDelete.Add(manager1);
			toDelete.Add(marketingEurope);

			ManagerEntity m = new ManagerEntity();

			PrefetchPath path = new PrefetchPath((int)EntityType.ManagerEntity);
			path.Add(ManagerEntity.PrefetchPathManagesDepartment);
			Assert.IsTrue( m.FetchUsingPK(manager1.Id, path));

			DeleteSavedEntities(toDelete);
			Assert.AreEqual( typeof(MarketingEntity), m.ManagesDepartment.GetType());
		}


		[Test]
		public void UpdateAllBoardmembersDirectly()
		{
			BoardMemberEntity newValues = new BoardMemberEntity();

			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			// give all boardmembers a new startdate.
			DateTime newStartDate = DateTime.Now;
			newStartDate = newStartDate.AddMilliseconds(0-newStartDate.Millisecond);
			newValues.StartDate = newStartDate;

			BoardMemberCollection updater = new BoardMemberCollection();
			Assert.IsTrue( (updater.UpdateMulti(newValues, null) > 0));
			Thread.Sleep(1000);
			BoardMemberCollection allBoardMembers = new BoardMemberCollection();
			// fetch all boardmembers and verify the updated date
			allBoardMembers.GetMulti(null);
			foreach(BoardMemberEntity boardMember in allBoardMembers)
			{
				// do a compare on date and hour/minute/second, as sqlserver rounds milliseconds so a full compare is not valid
				Assert.IsTrue((newStartDate.Date==boardMember.StartDate.Date) && 
					(newStartDate.Hour==boardMember.StartDate.Hour) &&
					(newStartDate.Minute==boardMember.StartDate.Minute) &&
					(newStartDate.Second==boardMember.StartDate.Second));
			}

			DeleteSavedEntities(toDelete);
		}


		[Test]
		public void UpdateSingleBoardmemberDirectlyWithFilter()
		{
			BoardMemberEntity newValues = new BoardMemberEntity();

			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			DateTime newStartDate = DateTime.Now;
			newStartDate = newStartDate.AddMilliseconds(0 - newStartDate.Millisecond);
			newValues.StartDate = newStartDate;

			BoardMemberCollection updater = new BoardMemberCollection();
			Assert.IsTrue((updater.UpdateMulti(newValues, (BoardMemberFields.CompanyCarId == 1)) > 0));
			Thread.Sleep(1000);
			BoardMemberCollection allBoardMembers = new BoardMemberCollection();
			// fetch all boardmembers and verify the updated date
			allBoardMembers.GetMulti(null);
			foreach(BoardMemberEntity boardMember in allBoardMembers)
			{
				// do a compare on date and hour/minute/second, as sqlserver rounds milliseconds so a full compare is not valid
				Assert.IsTrue((newStartDate.Date == boardMember.StartDate.Date) &&
					(newStartDate.Hour == boardMember.StartDate.Hour) &&
					(newStartDate.Minute == boardMember.StartDate.Minute) &&
					(newStartDate.Second == boardMember.StartDate.Second));
			}

			DeleteSavedEntities(toDelete);
		}



		/// <summary>
		/// Test which fetches fields from different targets of the same hierarchical entity into a dynamic list, and uses a filter without a relation.
		/// This test is to see if the adapter can find out the relations to build for the fields. 
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTest()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(BoardMemberFields.Name, 0, "Name");
			fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID");
			PredicateExpression filter = new PredicateExpression();
			filter.Add(BoardMemberFields.CompanyCarId== 1);
			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, null, filter, null, true, null, null, 0, 0);
			Assert.IsTrue( (results.Rows.Count>0));

			DeleteSavedEntities(toDelete);

			foreach(DataRow row in results.Rows)
			{
				Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
			}
		}


		/// <summary>
		/// Same as SingleEntityMultipleTargetDynamicListTest, but now WITH a relation. 
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTestWithRelation()
		{
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = CreateTestBatchForSave2(ref toDelete);
			uow.Commit(new Transaction(IsolationLevel.ReadCommitted, "Save"), true);

			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(BoardMemberFields.Name, 0, "Name");
			fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID");
			PredicateExpression filter = new PredicateExpression();
			filter.Add(CompanyCarFields.Brand== "Fiat");
			RelationCollection relations = new RelationCollection();
			relations.Add(BoardMemberEntity.Relations.CompanyCarEntityUsingCompanyCarId);
			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, null, filter, relations, true, null, null, 0, 0);
			Assert.IsTrue( (results.Rows.Count>0));

			DeleteSavedEntities(toDelete);

			foreach(DataRow row in results.Rows)
			{
				Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
			}
		}


		/// <summary>
		/// Same as SingleEntityMultipleTargetDynamicListTest, but now WITH a relation. AND aliases
		/// </summary>
		[Test]
		public void SingleEntityMultipleTargetDynamicListTestWithRelationWithAlias()
		{
			MarketingEntity marketingEurope = new MarketingEntity();
			marketingEurope.Name = "Marketing Europe";
			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;
			boardMember1.ManagesDepartment = marketingEurope;
			Assert.IsTrue(boardMember1.Save(true));

			List<IEntity> toDelete = new List<IEntity>();
			toDelete.Add(boardMember1);
			toDelete.Add(marketingEurope);

			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(BoardMemberFields.Name, 0, "Name", "BM");
			fields.DefineField(BoardMemberFields.CompanyCarId, 1, "CompanyCarID", "BM");
			PredicateExpression filter = new PredicateExpression();
			RelationCollection relations = new RelationCollection();
			filter.Add(DepartmentFields.DepartmentId.SetObjectAlias("D") == marketingEurope.DepartmentId);
			relations.Add(BoardMemberEntity.Relations.DepartmentEntityUsingManagesDepartmentId, "BM", "D", JoinHint.None);
			DataTable results = new DataTable();

			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, null, filter, relations, true, null, null, 0, 0);

			Assert.IsTrue( (results.Rows.Count>0));
			DeleteSavedEntities(toDelete);

			foreach(DataRow row in results.Rows)
			{
				Console.WriteLine("Name: {0}. CompanyCarID: {1}", row[0].ToString(), row[1].ToString());
			}
		}


		private UnitOfWork CreateTestBatchForSave(ref List<IEntity> toDelete)
		{
			UnitOfWork uow = new UnitOfWork();
			// first insert 2 clerks, a manager and a boardmember
			ClerkEntity clerk1 = new ClerkEntity();
			clerk1.Name = "Clerk 1";
			clerk1.StartDate = DateTime.Now;
			clerk1.JobDescription = "pushing paper";
			uow.AddForSave(clerk1, null, false);

			ClerkEntity clerk2 = new ClerkEntity();
			clerk2.Name = "Clerk 2";
			clerk2.StartDate = DateTime.Now;
			clerk2.JobDescription = "pushing paper";
			uow.AddForSave(clerk2, null, false);

			ManagerEntity manager1 = new ManagerEntity();
			manager1.Name = "Manager 1";
			manager1.StartDate = DateTime.Now;
			uow.AddForSave(manager1, null, false);

			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;
			uow.AddForSave(boardMember1, null, false);

			toDelete.Add(clerk1);
			toDelete.Add(clerk2);
			toDelete.Add(manager1);
			toDelete.Add(boardMember1);

			return uow;
		}


		internal UnitOfWork CreateTestBatchForSave2(ref List<IEntity> toDelete)
		{
			UnitOfWork uow = new UnitOfWork();

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
			uow.AddForSave(clerk1, true);

			ClerkEntity clerk2 = new ClerkEntity();
			clerk2.Name = "Clerk 2";
			clerk2.StartDate = DateTime.Now;
			clerk2.JobDescription = "pushing paper";
			clerk2.WorksForDepartment = marketingEurope;
			uow.AddForSave(clerk2, true);

			ManagerEntity manager1 = new ManagerEntity();
			manager1.Name = "Manager 1";
			manager1.StartDate = DateTime.Now;
			manager1.WorksForDepartment = marketingEurope;
			manager1.ManagesDepartment = marketingEurope;
			uow.AddForSave(manager1, true);

			BoardMemberEntity boardMember1 = new BoardMemberEntity();
			boardMember1.Name = "Boardmember 1";
			boardMember1.StartDate = DateTime.Now;
			boardMember1.WorksForDepartment = boardEurope;
			boardMember1.ManagesDepartment = marketingEurope;
			boardMember1.CompanyCar = companyCar1;
			uow.AddForSave(boardMember1, true);

			toDelete.Add(clerk1);
			toDelete.Add(clerk2);
			toDelete.Add(manager1);
			toDelete.Add(boardMember1);
			toDelete.Add(companyCar1);
			toDelete.Add(marketingEurope);
			toDelete.Add(boardEurope);
			return uow;
		}


		internal void DeleteSavedEntities( List<IEntity> toDelete )
		{
			foreach( IEntity entity in toDelete )
			{
				entity.Delete();
			}
		}
	
		[TearDown]
		public void CleanUp()
		{
			ActionProcedures.ClearAll();
		}
	}


	public class ManagerValidator : ValidatorBase
	{
		public override void ValidateEntityBeforeSave(IEntityCore involvedEntity)
		{
			base.ValidateEntityBeforeSave(involvedEntity);

			ManagerEntity toCheck = (ManagerEntity)involvedEntity;
			if(toCheck.ManagesDepartmentId == null)
			{
				throw new ORMEntityValidationException("ManagerEntity.ManagesDepartmentId is required.", toCheck);
			}
		}
	}
}
