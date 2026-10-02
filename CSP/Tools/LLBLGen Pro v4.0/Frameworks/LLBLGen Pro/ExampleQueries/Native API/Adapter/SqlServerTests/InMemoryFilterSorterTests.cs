using System;
using System.ComponentModel;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Diagnostics;

using NUnit.Framework;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Northwind.Adapter;
using Northwind.Adapter.DatabaseSpecific;
using Northwind.Adapter.EntityClasses;
using Northwind.Adapter.FactoryClasses;
using Northwind.Adapter.HelperClasses;
using Northwind.Adapter.TypedListClasses;
using Northwind.Adapter.TypedViewClasses;

using InheritanceOne.Dal.Adapter.EntityClasses;
using InheritanceOne.Dal.Adapter.FactoryClasses;
using InheritanceOne.Dal.Adapter.HelperClasses;
using InheritanceOne.Dal.Adapter.RelationClasses;
using InheritanceOne.Dal.Adapter.TypedListClasses;
using InheritanceOne.Dal.Adapter;

using Ih1DataAccessAdapter = InheritanceOne.Dal.Adapter.DatabaseSpecific.DataAccessAdapter;
using Ih1EmployeeEntity = InheritanceOne.Dal.Adapter.EntityClasses.EmployeeEntity;
using Ih1EmployeeFields = InheritanceOne.Dal.Adapter.HelperClasses.EmployeeFields;
using Ih1HC = InheritanceOne.Dal.Adapter.HelperClasses;
using NWHC = Northwind.Adapter.HelperClasses;

namespace Unittests.TestLibrary.SqlServerTests.Adapter
{
	/// <summary>
	/// Tests for in-memory filtering/sorting.
	/// </summary>
	[TestFixture]
	public class InMemoryFilterSorterTests
	{
		private Guid _testRunID;

		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid();
			Console.WriteLine( "TestRunID: {0}", _testRunID.ToString() );
			Console.WriteLine( "RT version: {0}.{1}", RuntimeLibraryVersion.Version, RuntimeLibraryVersion.Build );
		}


		[Test]
		public void EntityViewFilteredOnPropertyTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );
				for( int i = 0; i < 20; i++ )
				{
					customers[i].ContactTitle += " ";
				}
				// now create an entityview of all entities which are dirty.
				EntityView2<CustomerEntity> customersChanged = new EntityView2<CustomerEntity>( customers,
					(new EntityProperty("IsDirty")==true));
				Assert.AreEqual( 20, customersChanged.Count );
				foreach( CustomerEntity entity in customersChanged )
				{
					Assert.IsTrue( entity.IsDirty);
				}
			}
		}


		[Test]
		public void LikeFilterInMemoryTest()
		{
			NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchEntityCollection( customers, null );
			}

			EntityView2<CustomerEntity> customerView = customers.DefaultView;

			// filter on A%. Don't specify it's a regex.
			customerView.Filter = (CustomerFields.CompanyName % "A%");
			Assert.AreEqual( 4, customerView.Count );

			// filter on %e%. Dont specify it's a regex
			customerView.Filter = (CustomerFields.CompanyName % "%e%");
			Assert.AreEqual( 76, customerView.Count );

			// filter on %s. Dont specify it's a regex
			customerView.Filter = (CustomerFields.CompanyName % "%s");
			Assert.AreEqual( 23, customerView.Count );

			// all names which contain at most 1 space
			FieldLikePredicate filter = (FieldLikePredicate)(CustomerFields.CompanyName % @"^\w+ \w+ \w+$");
			filter.PatternIsRegEx = true;
			customerView.Filter = filter;
			Assert.AreEqual( 23, customerView.Count );
			foreach( CustomerEntity customer in customerView )
			{
				string[] parts = customer.CompanyName.Split(' ');
				Assert.AreEqual( 3, parts.Length );
			}
		}


		[Test]
		public void EntityViewProjectingOnDataTableNullableFieldsTest()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<OrderEntity> orders = new NWHC.EntityCollection<OrderEntity>( new OrderEntityFactory() );
				adapter.FetchEntityCollection( orders, new RelationPredicateBucket(OrderFields.ShippedDate==DBNull.Value) );
				EntityView2<OrderEntity> ordersView = orders.DefaultView;

				Assert.AreEqual( 19, ordersView.Count );

				List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
				propertyProjectors.Add( new EntityPropertyProjector( OrderFields.CustomerId, "CustomerId" ) );
				propertyProjectors.Add( new EntityPropertyProjector( OrderFields.ShippedDate, "ShippedDate" ) );

				DataTable results = new DataTable();
				ordersView.CreateProjection( propertyProjectors, results );
				Assert.AreEqual( 19, results.Rows.Count );
				Assert.AreEqual( DBNull.Value, results.Rows[0]["ShippedDate"] );
			}
		}



		[Test]
		public void EntityViewFilteringTestTypedCollection()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );
				EntityView2<CustomerEntity> customersInGermanyView = new EntityView2<CustomerEntity>( customers, (CustomerFields.Country == "Germany"), null );
				Assert.AreEqual( 11, customersInGermanyView.Count );

				EntityView2<CustomerEntity> customersWhichAreOwner = new EntityView2<CustomerEntity>( customers, (CustomerFields.ContactTitle == "Owner"), null );
				Assert.AreEqual( 17, customersWhichAreOwner.Count );
				EntityView2<CustomerEntity> customersInGermanyAndOwnerView = new EntityView2<CustomerEntity>( customers,
						 new PredicateExpression( CustomerFields.Country == "Germany" ).AddWithAnd( CustomerFields.ContactTitle == "Owner" ), null );
				Assert.AreEqual( 1, customersInGermanyAndOwnerView.Count );
				customersInGermanyAndOwnerView[0].ContactTitle = "Changed";
				Assert.AreEqual( 0, customersInGermanyAndOwnerView.Count );
			}
		}


		[Test]
		public void EntityViewMultiSorting()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );

				ISortExpression sorter = new SortExpression( CustomerFields.Country | SortOperator.Ascending );
				sorter.Add( CustomerFields.ContactTitle | SortOperator.Descending) ;
				sorter.Add( CustomerFields.CustomerId | SortOperator.Ascending );

				EntityView2<CustomerEntity> customersSorted = new EntityView2<CustomerEntity>( customers, sorter );
				for(int i = 0; i < customersSorted.Count-1; i++)
				{
					Assert.IsTrue(customersSorted[i].Country.CompareTo(customersSorted[i + 1].Country) <= 0);
					if(customersSorted[i].Country == customersSorted[i + 1].Country)
					{
						Assert.IsTrue(customersSorted[i].ContactTitle.CompareTo(customersSorted[i + 1].ContactTitle) >= 0);
						if(customersSorted[i].ContactTitle == customersSorted[i + 1].ContactTitle)
						{
							Assert.IsTrue(customersSorted[i].CustomerId.CompareTo(customersSorted[i + 1].CustomerId) <= 0);
						}
					}
				}
			}
		}


		[Test]
		public void SimpleProjectionTestsToDataTable()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );
				EntityView2<CustomerEntity> customersInGermanyView = new EntityView2<CustomerEntity>( customers, (CustomerFields.Country == "Germany"), null );
				Assert.AreEqual( 11, customersInGermanyView.Count );

				// create projection of these customers of just the city and the customerid.
				List<IEntityPropertyProjector> propertyProjectors= new List<IEntityPropertyProjector>();
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.City, "City" ) );
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CustomerId, "CustomerID" ) );
				DataTable projection = new DataTable();
				customersInGermanyView.CreateProjection( propertyProjectors, projection );
				Assert.AreEqual( 11, projection.Rows.Count );

				// do distinct filtering.
				propertyProjectors = new List<IEntityPropertyProjector>();
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.ContactTitle, "Contact title" ) );
				// any entity property can be used for projection source.
				propertyProjectors.Add( new EntityPropertyProjector( new EntityProperty( "IsNew" ), "Is new" ) );
				projection = new DataTable();
				customersInGermanyView.CreateProjection( propertyProjectors, projection, false );
				Assert.AreEqual( 7, projection.Rows.Count );

				// do distinct filtering and filter the set to project. Re-use previous property projectors. 3 rows match, distinct filtering makes it 2.
				projection = new DataTable();
				customersInGermanyView.CreateProjection( propertyProjectors, projection, false, (CustomerFields.ContactTitle % "Marketing%") );
				Assert.AreEqual( 2, projection.Rows.Count );

				// use alternative projection source based on filter.
				projection = new DataTable();
				propertyProjectors = new List<IEntityPropertyProjector>();
				// bogus data, but performs what we need: for all contacttitles not matching the filter, CustomerId is used.
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.ContactTitle, 
						"Contact title", (CustomerFields.ContactTitle % "Marketing%"), CustomerFields.CustomerId) );
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CustomerId, "CustomerID" ) );
				// create a new projection, with distinct filtering, which gives different results now, because ContactTitle is now sometimes equal to CustomerId
				customersInGermanyView.CreateProjection( propertyProjectors, projection, false );
				Assert.AreEqual( 11, projection.Rows.Count );
				foreach( DataRow row in projection.Rows )
				{
					if( !row["Contact title"].ToString().StartsWith( "Marketing" ) )
					{
						Assert.AreEqual( row["Contact title"], row["CustomerID"] );
					}
				}
			}
		}


		[Test]
		public void SimpleProjectionTestsToEntityCollection()
		{
			using( Ih1DataAccessAdapter adapter = new Ih1DataAccessAdapter() )
			{
				// insert some testdata first.
				InheritanceOneUnitTests ih1Tests = new InheritanceOneUnitTests();
				InheritanceOne.Dal.Adapter.HelperClasses.EntityCollection toDelete = new InheritanceOne.Dal.Adapter.HelperClasses.EntityCollection();
				UnitOfWork2 uow = ih1Tests.CreateTestBatchForSave2( ref toDelete );
				uow.Commit( adapter, true );

				// fetch all managers
				Ih1HC.EntityCollection<ManagerEntity> managers = new Ih1HC.EntityCollection<ManagerEntity>( new ManagerEntityFactory() );
				adapter.FetchEntityCollection( managers, null );
				Assert.AreEqual( 2, managers.Count );

				// now project them onto 2 new clerk entities, by just projecting the employee fields 
				List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
				propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.Id, "Id" ) );
				propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.Name, "Name" ) );
				propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.StartDate, "StartDate" ) );
				propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.WorksForDepartmentId, "WorksForDepartmentId" ) );
				Ih1HC.EntityCollection<ClerkEntity> clerks = new Ih1HC.EntityCollection<ClerkEntity>( new ClerkEntityFactory() );
				EntityView2<ManagerEntity> managersView = managers.DefaultView;
				managersView.Sorter = new SortExpression(ManagerFields.Name | SortOperator.Ascending);

				// project data to transform all managers into clerks.
				managersView.CreateProjection( propertyProjectors, clerks );
				Assert.AreEqual( 2, clerks.Count );
				Assert.AreEqual("Boardmember 1", clerks[0].Name);
				Assert.AreEqual("Manager 1", clerks[1].Name);
				// clean up.
				adapter.DeleteEntityCollection( toDelete );
			}
		}


		[Test]
		public void SimpleProjectionTestsToCustomClasses()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );

				EntityView2<CustomerEntity> allCustomersView = customers.DefaultView;
				List<TestCustomer> customCustomers = new List<TestCustomer>();
				DataProjectorToCustomClass<TestCustomer> customClassProjector = new DataProjectorToCustomClass<TestCustomer>( customCustomers );
				List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CustomerId, "CustomerID" ) );
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.City, "City" ) );
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CompanyName, "CompanyName" ) );
				propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.Country, "Country" ) );
				propertyProjectors.Add( new EntityPropertyProjector( new EntityProperty("IsDirty"), "IsDirty" ) );

				// alter one
				allCustomersView[0].CompanyName += " ";

				// create projection
				allCustomersView.CreateProjection( propertyProjectors, customClassProjector );
				Assert.AreEqual( customCustomers.Count, allCustomersView.Count );

				Assert.IsTrue( customCustomers[0].IsDirty );
				Assert.IsFalse( customCustomers[1].IsDirty );
				Assert.AreEqual(91, customCustomers.Count);
				for(int i = 0; i < customCustomers.Count; i++)
				{
					if(i>0)
					{
						// as companyname of 0 has been altered
						Assert.AreEqual(customCustomers[i].CompanyName, allCustomersView[i].CompanyName);
					}
					Assert.AreEqual(customCustomers[i].City, allCustomersView[i].City);
					if(customCustomers[i].Country != null)
					{
						Assert.AreEqual(customCustomers[i].Country, allCustomersView[i].Country);
					}
					Assert.AreEqual(customCustomers[i].CustomerID, allCustomersView[i].CustomerId);
					Assert.AreEqual(customCustomers[i].IsDirty, allCustomersView[i].IsDirty);
				}
			}
		}

		
		[Test]
		public void PostChangeNoActionTests()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );

				// create a view with the setting no action.
				EntityView2<CustomerEntity> customersGermany = new EntityView2<CustomerEntity>( customers, (CustomerFields.Country == "Germany"),
						new SortExpression( CustomerFields.CustomerId | SortOperator.Ascending ), PostCollectionChangeAction.NoAction );

				Assert.AreEqual( 11, customersGermany.Count );
				// changing one value's country shouldn't make a difference.
				customersGermany[0].Country = "France";
				Assert.AreEqual( 11, customersGermany.Count );
				CustomerEntity newCustomer = new CustomerEntity();
				newCustomer.CustomerId = "XXXXX";
				newCustomer.Country = "France";
				customers.Add( newCustomer );
				Assert.AreEqual( 11, customersGermany.Count );	// no filter applied
				newCustomer.Country = "Germany";
				Assert.AreEqual( 11, customersGermany.Count );
				customers.Remove( customersGermany[0] );
				Assert.AreEqual( 10, customersGermany.Count );
				newCustomer = (CustomerEntity)((IBindingList)customersGermany).AddNew();
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "Germany";
				newCustomer.CustomerId = "AAAAA";
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "France";
				Assert.AreEqual( 11, customersGermany.Count );
				Assert.AreEqual( 10, customersGermany.IndexOf( newCustomer ) );	// no sorter applied
			}
		}

		[Test]
		public void PostChangeReapplySorterTests()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );

				// create a view with the setting no action.
				EntityView2<CustomerEntity> customersGermany = new EntityView2<CustomerEntity>( customers, (CustomerFields.Country == "Germany"),
						new SortExpression( CustomerFields.CustomerId | SortOperator.Ascending ), PostCollectionChangeAction.ReapplySorter );

				Assert.AreEqual( 11, customersGermany.Count );
				// changing one value's country shouldn't make a difference.
				customersGermany[0].Country = "France";
				Assert.AreEqual( 11, customersGermany.Count );
				CustomerEntity newCustomer = new CustomerEntity();
				newCustomer.CustomerId = "XXXXX";
				newCustomer.Country = "France";
				customers.Add( newCustomer );
				Assert.AreEqual( 11, customersGermany.Count );	// no filter applied
				newCustomer.Country = "Germany";
				Assert.AreEqual( 11, customersGermany.Count );
				customers.Remove( customersGermany[0] );
				Assert.AreEqual( 10, customersGermany.Count );
				newCustomer = (CustomerEntity)((IBindingList)customersGermany).AddNew();
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "Germany";
				newCustomer.CustomerId = "AAAAA";
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "France";
				Assert.AreEqual( 11, customersGermany.Count );
				Assert.AreEqual( 0, customersGermany.IndexOf( newCustomer ) );	// sorter applied
			}
		}

		[Test]
		public void PostChangeReapplyFilterSorterTests()
		{
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				NWHC.EntityCollection<CustomerEntity> customers = new NWHC.EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
				adapter.FetchEntityCollection( customers, null );

				// create a view with the setting no action.
				EntityView2<CustomerEntity> customersGermany = new EntityView2<CustomerEntity>( customers, (CustomerFields.Country == "Germany"),
						new SortExpression( CustomerFields.CustomerId | SortOperator.Ascending ), PostCollectionChangeAction.ReapplyFilterAndSorter );

				Assert.AreEqual( 11, customersGermany.Count );
				// changing one value's country should make a difference.
				customersGermany[0].Country = "France";
				Assert.AreEqual( 10, customersGermany.Count );
				CustomerEntity newCustomer = new CustomerEntity();
				newCustomer.CustomerId = "XXXXX";
				newCustomer.Country = "France";
				customers.Add( newCustomer );
				Assert.AreEqual( 10, customersGermany.Count );	// filter applied but no match
				newCustomer.Country = "Germany";
				Assert.AreEqual( 11, customersGermany.Count );  // filter applied and a match
				customers.Remove( customersGermany[0] );
				Assert.AreEqual( 10, customersGermany.Count );
				newCustomer = (CustomerEntity)((IBindingList)customersGermany).AddNew();
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "Germany";
				newCustomer.CustomerId = "AAAAA";
				Assert.AreEqual( 11, customersGermany.Count );
				newCustomer.Country = "France";
				Assert.AreEqual( 10, customersGermany.Count ); // filter applied, no match so removed
				newCustomer.Country = "Germany";
				Assert.AreEqual( 11, customersGermany.Count ); // filter applied, match so added
				Assert.AreEqual( 0, customersGermany.IndexOf( newCustomer ) );	// sorter applied
			}
		}
	}


	/// <summary>
	/// Test class for projection of fetched entities onto custom classes using a custom projector.
	/// </summary>
	public class TestCustomer
	{
		#region Class Member Declarations
		private string _customerID, _companyName, _city, _country;
		private bool _isDirty;
		#endregion

		public TestCustomer()
		{
			_city = string.Empty;
			_companyName = string.Empty;
			_customerID = string.Empty;
			_country = string.Empty;
			_isDirty = false;
		}

		#region Class Property Declarations
		public string CustomerID
		{
			get { return _customerID; }
			set { _customerID = value; }
		}

		public string City
		{
			get { return _city; }
			set { _city = value; }
		}

		public string CompanyName
		{
			get { return _companyName; }
			set { _companyName = value; }
		}

		public string Country
		{
			get { return _country; }
			set { _country = value; }
		}

		public bool IsDirty
		{
			get { return _isDirty; }
			set { _isDirty = value; }
		}

		#endregion	
	}
}
