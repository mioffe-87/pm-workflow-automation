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
using SD.Tools.OrmProfiler.Interceptor;

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
		}

		/// <summary>
		/// Simple fetch with no filter nor projection.
		/// </summary>
		[Test]
		public void GetAllEmployeesWithoutAnyFilterUsingDirectTableAccessTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Employee;

			List<EmployeeEntity> employees = q.ToList();
			Assert.AreEqual(4, employees.Count);
		}


		[Test]
		public void GetAllEmployeesWithoutAnyFilterTest()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = from e in metaData.Employee
					select e;

			List<EmployeeEntity> employees = q.ToList();
			Assert.AreEqual(4, employees.Count);
		}


		/// <summary>
		/// This test uses a cast and a type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembers()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// This test uses a cast and no type filter to obtain only boardmembers and fetch their companycarid's
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersWithoutExplicitTypeFilter()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetAllCompanyCarIDAndBoardMemberIdsFromBoardMembersWithoutExplicitTypeFilterIntoAnonymousType()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetAllEmployeesWhoAreNotAManager()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetAllBoardMembersUsingOfType()
		{
			LinqMetaData metaData = new LinqMetaData();
			var q = metaData.Employee.OfType<BoardMemberEntity>();

			int count = 0;
			foreach(var v in q)
			{
				count++;
			}
			Assert.AreEqual(1, count);
		}


		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersUsingOfType()
		{
			LinqMetaData metaData = new LinqMetaData();
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

		
		[Test]
		public void DynamicListWithTypeFilterOnSubTypeTest()
		{
			ResultsetFields fields = new ResultsetFields(2);
			fields.DefineField(EmployeeFields.Name, 0, "EmployeeName");
			fields.DefineField(DepartmentFields.Name, 1, "DepartmentName");
			RelationCollection relations = new RelationCollection();
			relations.Add(EmployeeEntity.Relations.DepartmentEntityUsingWorksForDepartmentId);

			// filter on managers only
			IPredicateExpression filter = new PredicateExpression(ManagerEntity.GetEntityTypeFilter());

			DataTable results = new DataTable();
			TypedListDAO dao = new TypedListDAO();
			dao.GetMultiAsDataTable(fields, results, 0, null, filter, relations, true, null, null, 0, 0);
			Assert.AreEqual(2, results.Rows.Count);

			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetAllBoardMembersWithTypeAsConstruction()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets all company car Ids from board members using as and let. THis is similar to GetAllCompanyCarIDsFromBoardMembers, but now 
		/// let is used with 'as' to obtain a boardmemberentity. 
		/// </summary>
		[Test]
		public void GetAllCompanyCarIDsFromBoardMembersUsingAsAndLet()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		/// <summary>
		/// Gets all boardmembers who have company car with ID 1.
		/// </summary>
		[Test]
		public void GetAllBoardmembersWhoHaveCompanyCar1()
		{
			LinqMetaData metaData = new LinqMetaData();
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


		[Test]
		public void GetEmployeeDataWithNestedDeparmentData()
		{
			LinqMetaData metaData = new LinqMetaData();
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
}
