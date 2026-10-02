using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using InheritanceTwo.SelfServicing.DaoClasses;
using InheritanceTwo.SelfServicing.EntityClasses;
using InheritanceTwo.SelfServicing.HelperClasses;
using InheritanceTwo.SelfServicing.Linq;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using InheritanceTwo.SelfServicing.FactoryClasses;
using InheritanceTwo.SelfServicing.CollectionClasses;
using SD.LLBLGen.Pro.QuerySpec.SelfServicing;
using SD.LLBLGen.Pro.QuerySpec;

namespace LinqTester.SelfServicingTests
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


		[TestFixtureSetUp]
		public void SetupTests()
		{
			SD.Tools.OrmProfiler.Interceptor.InterceptorCore.Initialize("QuerySpec: InheritanceTests, Selfservicing");
		}


		/// <summary>
		/// Simple fetch with no filter nor projection.
		/// </summary>
		[Test]
		public void GetAllEmployeesWithoutAnyFilterUsingDirectTableAccessTest()
		{
			var f = new FamilyCarEntity();

			//LinqMetaData metaData = new LinqMetaData();
			//var q = metaData.Employee;
			//List<EmployeeEntity> employees = q.ToList();

			var q = new QueryFactory().Employee;
			var employees = new EmployeeCollection();
			employees.GetMulti(q);

			Assert.AreEqual(4, employees.Count);
		}


		[Test]
		public void PkFilterOfTPEHFetchTest()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from j in metaData.JapaneseFamilyCar
			//        where j.CarId==1
			//        select j;

			var qf = new QueryFactory();
			var q = qf.JapaneseFamilyCar.Where(JapaneseFamilyCarFields.CarId == 1);
			var results = new JapaneseFamilyCarCollection();
			results.GetMulti(q);

			foreach(var v in results)
			{

			}
		}


		/// <summary>
		/// This test uses a cast and a type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembers()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee.Where(e => e is BoardMemberEntity).Cast<BoardMemberEntity>()
			//        select e.CompanyCarId;

			var qf = new QueryFactory();
			var q = qf.Employee
						.OfEntityType<BoardMemberEntity>()		// defines filter, doesn't cast. Not really necessary if it's followed by .CastTo
						.CastTo<BoardMemberEntity>()	// casts, also type filters. 
						.Select(() => BoardMemberFields.CompanyCarId.ToValue<int>());
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			int companyCarID = 0;
			foreach(var v in results)
			{
				count++;
				companyCarID = v;
			}
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, companyCarID);
		}


		[Test]
		public void GetAllEmployeesWhoWorkForMarketing()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var results = metaData.Employee.Where(e => e.WorksForDepartment is MarketingEntity);

			var qf = new QueryFactory();
			var q = qf.Employee
							.From(QueryTarget.InnerJoin(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId))
							.Where(MarketingEntity.GetEntityTypeFilter());

			// alternative
			//var q = qf.Employee
			//            .WhereExists(qf.Department
			//                                .CorrelatedOver(DepartmentEntity.Relations.EmployeeEntityUsingWorksForDepartmentId)
			//                                .OfEntityType<MarketingEntity>());
			var results = new EmployeeCollection();
			results.GetMulti(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(3, count);
		}


		/// <summary>
		/// This test uses a cast and no type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersWithoutExplicitTypeFilter()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee.Cast<BoardMemberEntity>()
			//        select e.CompanyCarId;

			var qf = new QueryFactory();
			var q = qf.Employee
						.OfEntityType<BoardMemberEntity>()	// type filter necessary, cast doesn't add type filter.
						.CastTo<BoardMemberEntity>()	// casts
						.Select(() => BoardMemberFields.CompanyCarId.ToValue<int>());
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			int companyCarID = 0;
			foreach(var v in results)
			{
				count++;
				companyCarID = v;
			}
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, companyCarID);
		}


		[Test]
		public void GetAllEmployeesWhoAreNotAManager()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee
			//        where !(e is ManagerEntity)
			//        select e;

			var qf = new QueryFactory();
			var q = qf.Employee.NotOfEntityType<ManagerEntity>();
			var results = new EmployeeCollection();
			results.GetMulti(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
			}
			Assert.AreEqual(2, count);
		}


		[Test]
		public void DynamicListWithTypeFilterOnSubTypeTest()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee
			//        join d in metaData.Department on e.WorksForDepartmentId equals d.DepartmentId
			//        where e is ManagerEntity
			//        select new { EmployeeName = e.Name, DepartmentName = d.Name };

			var qf = new QueryFactory();
			var q = qf.Employee
						.OfEntityType<ManagerEntity>()
						.From(QueryTarget.InnerJoin(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId))
						.Select(() => new
						{
							EmployeeName = EmployeeFields.Name.As("EmployeeName").ToValue<string>(),
							DepartmentName = DepartmentFields.Name.As("DepartmentName").ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				Console.WriteLine("Employee: {0}. Department: {1}", v.EmployeeName, v.DepartmentName);
				count++;
			}
			Assert.AreEqual(2, count);
		}


		[Test]
		public void GetEmployeeDataWithNestedDeparmentData()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from e in metaData.Employee
			//        select new
			//        {
			//            e.Name,
			//            e.WorksForDepartment
			//        };

			var qf = new QueryFactory();
			var q = qf.Employee
						.Select(() => new
						{
							Name = EmployeeFields.Name.ToValue<string>(),
							WorksForDepartment = qf.Department
														.CorrelatedOver(DepartmentEntity.Relations.EmployeeEntityUsingWorksForDepartmentId)
														.ToSingleResult()
						});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Console.WriteLine("{0}, {1}", v.Name, v.WorksForDepartment.Name);
			}
			Assert.AreEqual(4, count);
		}


		[Test]
		public void PolymorphicPrefetchPathFetch()
		{
			DaoBase.ParameterisedPrefetchPathThreshold = 1;
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from e in metaData.Employee
			//         select e).WithPath(new PathEdge<CompanyCarEntity>(BoardMemberEntity.PrefetchPathCompanyCar));

			var qf = new QueryFactory();
			var q = qf.Employee
							.WithPath(BoardMemberEntity.PrefetchPathCompanyCar);
			var results = new EmployeeCollection();
			results.GetMulti(q);
			DaoBase.ParameterisedPrefetchPathThreshold = 50;
			foreach(var v in results)
			{
				var vBoardMember = v as BoardMemberEntity;
				if(vBoardMember != null)
				{
					Assert.IsNotNull(vBoardMember.CompanyCar);
				}
			}
		}


		[Test]
		public void PolymorphicPrefetchPathFetchUsingJezExtensions()
		{
			DaoBase.ParameterisedPrefetchPathThreshold = 1;
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from d in metaData.Department
			//         select d).WithPath(departmentPath => departmentPath
			//                .Prefetch<EmployeeEntity>(dep => dep.Employees).SubPath<BoardMemberEntity>(bmPath => bmPath
			//                    .Prefetch<FamilyCarEntity>(bm => bm.CompanyCar)));

			var qf = new QueryFactory();
			var q = qf.Department
							.WithPath(DepartmentEntity.PrefetchPathEmployees
															.WithSubPath(BoardMemberEntity.PrefetchPathCompanyCar
																								.WithFilter(FamilyCarEntity.GetEntityTypeFilter())));
			var results = new DepartmentCollection();
			results.GetMulti(q);
			DaoBase.ParameterisedPrefetchPathThreshold = 50;
			int count = 0;
			int boardMemberCounter = 0;
			foreach(DepartmentEntity v in results)
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


		[Test]
		public void PolymorphicPrefetchPathFetchUsingJezExtensionsParameterized()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = (from d in metaData.Department
			//         select d).WithPath(departmentPath => departmentPath
			//                .Prefetch<EmployeeEntity>(dep => dep.Employees).SubPath<BoardMemberEntity>(bmPath => bmPath
			//                    .Prefetch<FamilyCarEntity>(bm => bm.CompanyCar)));

			// will use parameterized prefetch path query.
			var qf = new QueryFactory();
			var q = qf.Department
							.WithPath(DepartmentEntity.PrefetchPathEmployees
															.WithSubPath(BoardMemberEntity.PrefetchPathCompanyCar
																								.WithFilter(FamilyCarEntity.GetEntityTypeFilter())));
			var results = new DepartmentCollection();
			results.GetMulti(q);

			int count = 0;
			int boardMemberCounter = 0;
			foreach(DepartmentEntity v in results)
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


		[Test]
		public void GetDepartmentsWhichHaveABoardMemberAmongTheirWorkforce()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Department
			//        where d.Employees.Any(e => e is BoardMemberEntity)
			//        select d;

			var qf = new QueryFactory();
			var q = qf.Department
						.Where(qf.Employee
									.CorrelatedOver(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId)
									.Any(BoardMemberEntity.GetEntityTypeFilter()));
			var results = new DepartmentCollection();
			results.GetMulti(q);

			int count = 0;
			foreach(DepartmentEntity v in results)
			{
				count++;
				Assert.AreEqual("Board Europe", v.Name);
			}
			Assert.AreEqual(1, count);
		}


		[Test]
		public void GetCountOfDepartmentsWhichHaveABoardMemberAmongTheirWorkforce()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Department
			//        where d.Employees.Any(e => e is BoardMemberEntity)
			//        select d;
			//Assert.AreEqual(1, q.Count());

			var qf = new QueryFactory();
			var q = qf.Department
						.Where(qf.Employee
									.CorrelatedOver(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId)
									.Any(BoardMemberEntity.GetEntityTypeFilter()));
			var results = new TypedListDAO().GetScalar<int>(qf.Create().Select(Functions.CountRow()).From(q));
			Assert.AreEqual(1, results);
		}


		[Test]
		public void CountDistinctOnRelatedTypeOfInheritanceHierarchyTest()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Department
			//        join e in metaData.Employee on d.DepartmentId equals e.WorksForDepartmentId
			//        select d;
			//int count = q.CountColumn(d => d.DepartmentId, true);
			//Assert.AreEqual(2, count);

			//count = metaData.Department.Where(d=>d.Employees.Count() > 0).CountColumn(d => d.DepartmentId, true);
			//Assert.AreEqual(2, count);

			var qf = new QueryFactory();
			var q = qf.Department.As("d")	// alias is required, as q will be a derived table, due to the join it encloses.
						.From(QueryTarget.InnerJoin(DepartmentEntity.Relations.EmployeeEntityUsingWorksForDepartmentId));
			var count = new TypedListDAO().GetScalar<int>(qf.Create()
														.Select(DepartmentFields.DepartmentId.Source("d").CountDistinct())
														.From(q));
			Assert.AreEqual(2, count);
		}


		[Test]
		public void GroupByOnElementFromNestedQuery()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q1 = from c in metaData.Clerk
			//        select c;
			//var q = from c in q1
			//        group c by c.JobDescription into g
			//        select g.Key;
			//var results = q.ToList();

			var qf = new QueryFactory();
			var q = qf.Clerk
						.Select(ClerkFields.JobDescription)
						.GroupBy(ClerkFields.JobDescription);
			var results = new TypedListDAO().FetchQuery(q);
			Assert.AreEqual(2, results.Count);
		}


		[Test]
		public void CountOfDerivedTypeInTPETest()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var c = metaData.BoardMember.Count();
			//Assert.AreEqual(1, c);

			var qf = new QueryFactory();
			var result = new TypedListDAO().GetScalar<int>(qf.Create().Select(qf.BoardMember.CountRow()));
			Assert.AreEqual(1, result);
		}


		[Test]
		public void GetManagersAndMarketingDepartmentData()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Marketing
			//        join b in metaData.BoardMember on d.DepartmentId equals b.ManagesDepartmentId
			//        select new { b.Name, d.MeetingRoomCode, b.CompanyCarId };
			var qf = new QueryFactory();
			var q = qf.Marketing
							.From(QueryTarget.InnerJoin(qf.BoardMember).On(MarketingFields.DepartmentId == BoardMemberFields.ManagesDepartmentId))
							.Select(() => new
							{
								Name = BoardMemberFields.Name.ToValue<string>(),
								MeetingRoomCode = MarketingFields.MeetingRoomCode.ToValue<int?>(),
								CompanyCarId = BoardMemberFields.CompanyCarId.ToValue<int?>()
							});
			var results = new TypedListDAO().FetchQuery(q);

			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.AreEqual("Boardmember 1", v.Name);
				Assert.AreEqual(1, v.CompanyCarId);
			}
			Assert.AreEqual(1, count);
		}


		[Test]
		public void GetManagersAndMarketingDepartmentData2()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Department
			//        join b in metaData.Employee.Cast<BoardMemberEntity>() on d.DepartmentId equals b.ManagesDepartmentId
			//        select new { b.Name, d.DepartmentId, b.CompanyCarId };

			var qf = new QueryFactory();
			var q = qf.Marketing
							.From(QueryTarget.InnerJoin(qf.Employee.CastTo<BoardMemberEntity>())
													.On(MarketingFields.DepartmentId == BoardMemberFields.ManagesDepartmentId))
							.Select(() => new
							{
								Name = BoardMemberFields.Name.ToValue<string>(),
								MeetingRoomCode = MarketingFields.MeetingRoomCode.ToValue<int?>(),
								CompanyCarId = BoardMemberFields.CompanyCarId.ToValue<int?>()
							});
			var results = new TypedListDAO().FetchQuery(q);
			int count = 0;
			foreach(var v in results)
			{
				count++;
				Assert.AreEqual("Boardmember 1", v.Name);
				Assert.AreEqual(1, v.CompanyCarId);
			}
			Assert.AreEqual(1, count);
		}


		[Test]
		public void AbstractSupertypeFetchWithNullFilter()
		{
			//LinqMetaData metaData = new LinqMetaData();
			//var q = from d in metaData.Department
			//        select new { test = d == null ? string.Empty : d.Name };

			var qf = new QueryFactory();
			var q = qf.Department
						.Select(() => new
						{
							test = Functions.IIF(DepartmentFields.DepartmentId.IsNull(), string.Empty, DepartmentFields.Name).ToValue<string>()
						});
			var results = new TypedListDAO().FetchQuery(q);
		}
	}
}
