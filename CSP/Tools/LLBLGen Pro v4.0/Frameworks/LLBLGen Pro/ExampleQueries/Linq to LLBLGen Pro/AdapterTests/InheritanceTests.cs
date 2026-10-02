using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InheritanceTwo.Adapter.DatabaseSpecific;
using InheritanceTwo.Adapter.EntityClasses;
using InheritanceTwo.Adapter.HelperClasses;
using InheritanceTwo.Adapter.Linq;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.LinqSupportClasses;

namespace LinqTester.AdapterTests
{
	[TestFixture]
	public class InheritanceTests
	{
		#region Data insertion routine
		//public void InsertTestData()
		//{
		//    UnitOfWork2 uow = new UnitOfWork2();

		//    MarketingEntity marketingEurope = new MarketingEntity();
		//    marketingEurope.Name = "Marketing Europe";
		//    BoardEntity boardEurope = new BoardEntity();
		//    boardEurope.Name = "Board Europe";

		//    FamilyCarEntity companyCar1 = new FamilyCarEntity();
		//    companyCar1.CarId = 1;
		//    companyCar1.Brand = "Fiat";
		//    companyCar1.HasDrawHook = true;
		//    companyCar1.Price = 10000M;

		//    ClerkEntity clerk1 = new ClerkEntity();
		//    clerk1.Name = "Clerk 1";
		//    clerk1.StartDate = DateTime.Now;
		//    clerk1.JobDescription = "pushing paper";
		//    clerk1.WorksForDepartment = marketingEurope;
		//    uow.AddForSave(clerk1);

		//    ClerkEntity clerk2 = new ClerkEntity();
		//    clerk2.Name = "Clerk 2";
		//    clerk2.StartDate = DateTime.Now;
		//    clerk2.JobDescription = "pushing paper";
		//    clerk2.WorksForDepartment = marketingEurope;
		//    uow.AddForSave(clerk2);

		//    ManagerEntity manager1 = new ManagerEntity();
		//    manager1.Name = "Manager 1";
		//    manager1.StartDate = DateTime.Now;
		//    manager1.WorksForDepartment = marketingEurope;
		//    manager1.ManagesDepartment = marketingEurope;
		//    uow.AddForSave(manager1);

		//    BoardMemberEntity boardMember1 = new BoardMemberEntity();
		//    boardMember1.Name = "Boardmember 1";
		//    boardMember1.StartDate = DateTime.Now;
		//    boardMember1.WorksForDepartment = boardEurope;
		//    boardMember1.ManagesDepartment = marketingEurope;
		//    boardMember1.CompanyCar = companyCar1;
		//    uow.AddForSave(boardMember1);

		//    uow.Commit(new DataAccessAdapter(), true);
		//}
		#endregion


		/// <summary>
		/// Simple fetch with no filter nor projection.
		/// </summary>
		[Test]
		public void GetAllEmployeesWithoutAnyFilterUsingDirectTableAccessTest()
		{
			var f = new FamilyCarEntity();

			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Employee;

				List<EmployeeEntity> employees = q.ToList();
				Assert.AreEqual(4, employees.Count);
			}
		}
		

		[Test]
		public void AnyOnSubTypeTPEHTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				// there's 1 sportscar, but it's a ferrari, so query should return false.
				var exists = metaData.SportsCar.Any(c => c.Brand == "Ford");
				Assert.IsFalse(exists);
			}
		}


		[Test]
		public void GetEmployeesWithDepartmentsInProjectionTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select new { Employee = e, EmployeeName = e.Name, Departments = e.WorksForDepartment };
				foreach(var v in q)
				{
					Console.WriteLine("{0} ({1}). Works for department: {2} ({3})", v.EmployeeName, v.Employee.GetType().Name, 
								v.Departments.Name, v.Departments.GetType().Name);
				}
			}
		}
		

		[Test]
		public void PkFilterOfTPEHFetchTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from j in metaData.JapaneseFamilyCar
						where j.CarId==1
						select j;

				foreach(var v in q)
				{
					
				}
			}
		}


		/// <summary>
		/// Fetch to show how inherited entity can be fetched without instantiating the subtypes in their own type.
		/// </summary>
		[Test]
		public void GetManagerDataWithoutPolymorphism()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Manager
						select new ManagerEntity { Id = e.Id, Name = e.Name, WorksForDepartmentId = e.WorksForDepartmentId, StartDate = e.StartDate, 
												ManagesDepartmentId = e.ManagesDepartmentId };

				List<ManagerEntity> managers = q.ToList();
				Assert.AreEqual(2, managers.Count);
				foreach(IEntity2 e in managers)
				{
					Assert.AreEqual(6, e.Fields.Count);	// boardmember is also fetched as solely manager.
					Assert.AreNotEqual(typeof(BoardMemberEntity), e.GetType());
				}
			}
		}


		[Test]
		public void GetAllEmployeesWithoutAnyFilterTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select e;


				List<EmployeeEntity> employees = q.ToList();
				Assert.AreEqual(4, employees.Count);
			}
		}


		/// <summary>
		/// This test uses a cast and a type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembers()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee.Where(e => e is BoardMemberEntity).Cast<BoardMemberEntity>()
						select e.CompanyCarId;

				int count = 0;
				int companyCarID = 0;
				foreach(var v in q)
				{
					count++;
					companyCarID=v.Value;
				}
				Assert.AreEqual(1, count);
				Assert.AreEqual(1, companyCarID);
			}
		}


		[Test]
		public void GetAllEmployeesWhoWorkForMarketing()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Employee.Where(e => e.WorksForDepartment is MarketingEntity);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		/// <summary>
		/// This test uses a cast and no type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersWithoutExplicitTypeFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee.Cast<BoardMemberEntity>()
						select e.CompanyCarId;

				int count = 0;
				int companyCarID = 0;
				foreach(var v in q)
				{
					count++;
					companyCarID = v.Value;
				}
				Assert.AreEqual(1, count);
				Assert.AreEqual(1, companyCarID);
			}
		}


		[Test]
		public void GetAllCompanyCarIDAndBoardMemberIdsFromBoardMembersWithoutExplicitTypeFilterIntoAnonymousType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee.Cast<BoardMemberEntity>()
						select new { e.Id, CompanyCarId = (int)e.CompanyCarId };

				int count = 0;
				int companyCarID = 0;
				foreach(var v in q)
				{
					count++;
					companyCarID = v.CompanyCarId;
				}
				Assert.AreEqual(1, count);
				Assert.AreEqual(1, companyCarID);
			}
		}


		[Test]
		public void GetAllEmployeesWhoAreNotAManager()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where !(e is ManagerEntity)
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GetAllBoardMembersUsingOfType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = metaData.Employee.OfType<BoardMemberEntity>();

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersUsingOfType()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee.OfType<BoardMemberEntity>()
						select e.CompanyCarId;

				int count = 0;
				int companyCarID = 0;
				foreach(var v in q)
				{
					count++;
					companyCarID = v.Value;
				}
				Assert.AreEqual(1, count);
				Assert.AreEqual(1, companyCarID);
			}
		}

		
		[Test]
		public void DynamicListWithTypeFilterOnSubTypeTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
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

				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						join d in metaData.Department on e.WorksForDepartmentId equals d.DepartmentId
						where e is ManagerEntity
						select new { EmployeeName = e.Name, DepartmentName = d.Name };

				int count = 0;
				foreach(var v in q)
				{
					Console.WriteLine("Employee: {0}. Department: {1}", v.EmployeeName, v.DepartmentName);
					count++;
				}
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GetAllBoardMembersWithTypeAsConstruction()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where (e as BoardMemberEntity)!=null
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		/// <summary>
		/// Gets all company car Ids from board members using as and let. THis is similar to GetAllCompanyCarIDsFromBoardMembers, but now 
		/// let is used with 'as' to obtain a boardmemberentity. 
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersUsingAsAndLet()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						let x = e as BoardMemberEntity
						where (x != null) && x.CompanyCarId == 1
						select x;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		/// <summary>
		/// Gets all boardmembers who have company car with ID 1.
		/// </summary>
		[Test]
		public void GetAllBoardmembersWhoHaveCompanyCar1()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						where (e as BoardMemberEntity).CompanyCarId == 1
						select e;

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetEmployeeDataWithNestedDeparmentData()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select new
						{
							e.Name,
							e.WorksForDepartment
						};

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Console.WriteLine("{0}, {1}", v.Name, v.WorksForDepartment.Name);
				}
				Assert.AreEqual(4, count);
			}
		}


		[Test]
		public void PolymorphicPrefetchPathFetch()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
				         select e).WithPath(new PathEdge<CompanyCarEntity>(BoardMemberEntity.PrefetchPathCompanyCar));
				adapter.ParameterisedPrefetchPathThreshold = 1;
				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void PolymorphicPrefetchPathFetchUsingJezExtensions()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from d in metaData.Department
						 select d).WithPath(departmentPath => departmentPath
								.Prefetch<EmployeeEntity>(dep => dep.Employees).SubPath<BoardMemberEntity>(bmPath => bmPath
									.Prefetch<FamilyCarEntity>(bm => bm.CompanyCar)));
				adapter.ParameterisedPrefetchPathThreshold = 1;
				int count = 0;
				int boardMemberCounter = 0;
				foreach(var v in q)
				{
					count++;
					foreach(EmployeeEntity e in v.Employees)
					{
						BoardMemberEntity bm = e as BoardMemberEntity;
						if(bm != null)
						{
							Assert.IsTrue(bm.CompanyCar is FamilyCarEntity);
							boardMemberCounter++;
						}
					}
				}
				Assert.AreEqual(2, count);
				Assert.AreEqual(1, boardMemberCounter);
			}
		}
        

		[Test]
		public void PolymorphicPrefetchPathFetchUsingJezExtensionsParameterized()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from d in metaData.Department
						 select d).WithPath(departmentPath => departmentPath
								.Prefetch<EmployeeEntity>(dep => dep.Employees).SubPath<BoardMemberEntity>(bmPath => bmPath
									.Prefetch<FamilyCarEntity>(bm => bm.CompanyCar)));
				int count = 0;
				int boardMemberCounter = 0;
				foreach(var v in q)
				{
					count++;
					foreach(EmployeeEntity e in v.Employees)
					{
						BoardMemberEntity bm = e as BoardMemberEntity;
						if(bm != null)
						{
							Assert.IsTrue(bm.CompanyCar is FamilyCarEntity);
							boardMemberCounter++;
						}
					}
				}
				Assert.AreEqual(2, count);
				Assert.AreEqual(1, boardMemberCounter);
			}
		}


		[Test]
		public void GetEmployeesAndDepartmentsWithPrefetchPath()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				adapter.ParameterisedPrefetchPathThreshold = 1;
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = (from e in metaData.Employee
						 select e).WithPath(p => p.Prefetch(e => e.WorksForDepartment));
				int count = 0;
				foreach(var v in q)
				{
					Assert.IsNotNull(v.WorksForDepartment);
					count++;
				}
				Assert.AreEqual(4, count);
			}
		}


		[Test]
		public void GetDepartmentsWhichHaveABoardMemberAmongTheirWorkforce()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Department
						where d.Employees.Any(e => e is BoardMemberEntity)
						select d;

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual("Board Europe", v.Name);
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetCountOfDepartmentsWhichHaveABoardMemberAmongTheirWorkforce()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Department
						where d.Employees.Any(e => e is BoardMemberEntity)
						select d;

				Assert.AreEqual(1, q.Count());
			}
		}


		[Test]
		public void FetchOfTargetPerEntityInheritanceWithAppendedWhere()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from b in metaData.BoardMember
						select b;

				q = q.Where(b => b.CompanyCarId > 0);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(4, v.Id);
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void FetchOfTargetPerEntityInheritanceWithAppendedWhereUsingPredicateBuilder()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var predicate = PredicateBuilder.Null<BoardMemberEntity>();
				predicate = predicate.And(b => b.CompanyCarId > 0);
				var q = from b in metaData.BoardMember
						select b;

				q = q.Where(predicate);

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual(4, v.Id);
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void FetchOfTargetPerEntityInheritanceWithAppendedWhere2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from e in metaData.Employee
						select e;

				q = q.Where(e => e.WorksForDepartmentId == 1);

				int count = 0;
				foreach(var v in q)
				{
					count++;
				}
				Assert.AreEqual(3, count);
			}
		}


		[Test]
		public void CountDistinctOnRelatedTypeOfInheritanceHierarchyTest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Department
						join e in metaData.Employee on d.DepartmentId equals e.WorksForDepartmentId
						select d;
				int count = q.CountColumn(d => d.DepartmentId, true);
				Assert.AreEqual(2, count);

				count = metaData.Department.Where(d=>d.Employees.Count() > 0).CountColumn(d => d.DepartmentId, true);
				Assert.AreEqual(2, count);
			}
		}


		[Test]
		public void GroupByOnElementFromNestedQuery()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Clerk
				        select c;

				var q = from c in q1
						group c by c.JobDescription into g
						select g.Key;

				foreach(var v in q)
				{
					
				}
			}
		}


		[Test]
		public void GroupByOnElementFromNestedQuery2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Clerk
						 select c;

				var q = from c in q1
						group c by c.WorksForDepartment.Name into g
						select g.Key;

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void ReferenceToInheritedFieldThroughSeveralQueryFragments()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q1 = from c in metaData.Clerk
						 select c;

				var q2 = from c in q1
						 select new { c.JobDescription };

				var q = from c in q2
						select c.JobDescription;

				foreach(var v in q)
				{

				}
			}
		}


		[Test]
		public void CountOfDerivedTypeInTPETest()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var c = metaData.BoardMember.Count();
				Assert.AreEqual(1, c);
			}
		}

		
		[Test]
		public void GetManagersAndMarketingDepartmentData()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Marketing
						join b in metaData.BoardMember on d.DepartmentId equals b.ManagesDepartmentId
						select new { b.Name, d.MeetingRoomCode, b.CompanyCarId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual("Boardmember 1", v.Name);
					Assert.AreEqual(1, v.CompanyCarId);
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void GetManagersAndMarketingDepartmentData2()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Department
						join b in metaData.Employee.Cast<BoardMemberEntity>() on d.DepartmentId equals b.ManagesDepartmentId
						select new { b.Name, d.DepartmentId, b.CompanyCarId };

				int count = 0;
				foreach(var v in q)
				{
					count++;
					Assert.AreEqual("Boardmember 1", v.Name);
					Assert.AreEqual(1, v.CompanyCarId);
				}
				Assert.AreEqual(1, count);
			}
		}


		[Test]
		public void AbstractSupertypeFetchWithNullFilter()
		{
			using(DataAccessAdapter adapter = new DataAccessAdapter())
			{
				LinqMetaData metaData = new LinqMetaData(adapter);
				var q = from d in metaData.Department
						select new { test = d == null ? string.Empty : d.Name };

				foreach(var v in q)
				{

				}

			}
		}


		[TestFixtureSetUp]
		public void SetupTests()
		{
		}
	}
}
