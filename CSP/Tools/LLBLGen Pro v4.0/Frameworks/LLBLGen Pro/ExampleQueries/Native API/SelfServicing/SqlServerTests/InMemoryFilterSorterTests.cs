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

using Northwind.SelfServicing;
using Northwind.SelfServicing.CollectionClasses;
using Northwind.SelfServicing.DaoClasses;
using Northwind.SelfServicing.EntityClasses;
using Northwind.SelfServicing.FactoryClasses;
using Northwind.SelfServicing.HelperClasses;
using Northwind.SelfServicing.TypedListClasses;
using Northwind.SelfServicing.TypedViewClasses;

using SD.LLBLGen.Pro.ORMSupportClasses;

using InheritanceOne.Dal.SelfServicing.EntityClasses;
using InheritanceOne.Dal.SelfServicing.FactoryClasses;
using InheritanceOne.Dal.SelfServicing.HelperClasses;
using InheritanceOne.Dal.SelfServicing.RelationClasses;
using InheritanceOne.Dal.SelfServicing.TypedListClasses;
using InheritanceOne.Dal.SelfServicing;
using InheritanceOne.Dal.SelfServicing.CollectionClasses;

using Ih1EmployeeEntity = InheritanceOne.Dal.SelfServicing.EntityClasses.EmployeeEntity;
using Ih1EmployeeFields = InheritanceOne.Dal.SelfServicing.HelperClasses.EmployeeFields;
using Ih1Transaction = InheritanceOne.Dal.SelfServicing.HelperClasses.Transaction;

namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
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
		public void EntityViewFilteringTestTypedCollection()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(null);
			EntityView<CustomerEntity> customersInGermanyView = new EntityView<CustomerEntity>( customers, (CustomerFields.Country == "Germany"), null );
			Assert.AreEqual( 11, customersInGermanyView.Count );
			for( int i = 0; i < customersInGermanyView.Count; i++ )
			{
				Console.WriteLine( "{0}: CustomerID: {1}. CompanyName: {2}",
					i, customersInGermanyView[i].CustomerId, customersInGermanyView[i].CompanyName );
			}
			EntityView<CustomerEntity> customersWhichAreOwner = new EntityView<CustomerEntity>( customers, (CustomerFields.ContactTitle == "Owner"), null );
			Assert.AreEqual( 17, customersWhichAreOwner.Count );
			EntityView<CustomerEntity> customersInGermanyAndOwnerView = new EntityView<CustomerEntity>( customers,
					 new PredicateExpression( CustomerFields.Country == "Germany" ).AddWithAnd( CustomerFields.ContactTitle == "Owner" ), null );
			Assert.AreEqual( 1, customersInGermanyAndOwnerView.Count );

			customersInGermanyAndOwnerView[0].ContactTitle = "Changed";
			Assert.AreEqual( 0, customersInGermanyAndOwnerView.Count );
		}


		[Test]
		public void EntityViewMultiSorting()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti( null );

			ISortExpression sorter = new SortExpression( CustomerFields.Country | SortOperator.Ascending );
			sorter.Add( CustomerFields.ContactTitle | SortOperator.Descending) ;
			sorter.Add( CustomerFields.CustomerId | SortOperator.Ascending );

			EntityView<CustomerEntity> customersSorted = new EntityView<CustomerEntity>( customers, sorter );

			for(int i = 0; i < customersSorted.Count - 1; i++)
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


		[Test]
		public void SimpleProjectionTestsToDataTable()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti(null);
			EntityView<CustomerEntity> customersInGermanyView = new EntityView<CustomerEntity>( customers, (CustomerFields.Country == "Germany"), null );
			Assert.AreEqual( 11, customersInGermanyView.Count );

			// create projection of these customers of just the city and the customerid.
			List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
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
					"Contact title", (CustomerFields.ContactTitle % "Marketing%"), CustomerFields.CustomerId ) );
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


		/// <summary>
		/// Tests a projection of data onto read-only properties. 
		/// </summary>
		[Test]
		public void ReadOnlyFieldProjectionsOnEntitiesTest()
		{
			OrderCollection orders = new OrderCollection();
			orders.GetMulti( null, 10 );

			List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
			propertyProjectors.Add( new EntityPropertyProjector( OrderFields.OrderId, "OrderId" ) );
			propertyProjectors.Add( new EntityPropertyProjector( OrderFields.CustomerId, "CustomerId" ) );

			OrderCollection destination = new OrderCollection();
			EntityView<OrderEntity> ordersView = orders.DefaultView;

			ordersView.CreateProjection( propertyProjectors, destination );
			Assert.AreEqual( 10, destination.Count );
			for( int i = 0; i < orders.Count; i++ )
			{
				Assert.AreEqual( orders[i].OrderId, destination[i].OrderId );
				Assert.AreEqual( orders[i].CustomerId, destination[i].CustomerId );
			}
		}


		[Test]
		public void SimpleProjectionTestsToEntityCollection()
		{
			// insert some testdata first.
			InheritanceOneUnitTests ih1Tests = new InheritanceOneUnitTests();
			List<IEntity> toDelete = new List<IEntity>();
			UnitOfWork uow = ih1Tests.CreateTestBatchForSave2( ref toDelete );
			uow.Commit( new Ih1Transaction( IsolationLevel.ReadCommitted, "Save" ), true );

			// fetch all managers
			ManagerCollection managers = new ManagerCollection();
			managers.GetMulti( null );
			Assert.AreEqual( 2, managers.Count );

			// now project them onto 2 new clerk entities, by just projecting the employee fields 
			List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
			propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.Id, "Id" ) );
			propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.Name, "Name" ) );
			propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.StartDate, "StartDate" ) );
			propertyProjectors.Add( new EntityPropertyProjector( Ih1EmployeeFields.WorksForDepartmentId, "WorksForDepartmentId" ) );
			ClerkCollection clerks = new ClerkCollection();
			EntityView<ManagerEntity> managersView = managers.DefaultView;
			managersView.Sorter = new SortExpression(ManagerFields.Name | SortOperator.Ascending);

			// project data to transform all managers into clerks.
			managersView.CreateProjection( propertyProjectors, clerks );
			Assert.AreEqual( 2, clerks.Count );

			Assert.AreEqual("Boardmember 1", clerks[0].Name);
			Assert.AreEqual("Manager 1", clerks[1].Name);

			// Clean up.
			ih1Tests.DeleteSavedEntities( toDelete );
		}


		[Test]
		public void SimpleProjectionTestsToCustomClasses()
		{
			CustomerCollection customers = new CustomerCollection();
			customers.GetMulti( null );

			EntityView<CustomerEntity> allCustomersView = customers.DefaultView;
			List<TestCustomer> customCustomers = new List<TestCustomer>();
			DataProjectorToCustomClass<TestCustomer> customClassProjector = new DataProjectorToCustomClass<TestCustomer>( customCustomers );
			List<IEntityPropertyProjector> propertyProjectors = new List<IEntityPropertyProjector>();
			propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CustomerId, "CustomerID" ) );
			propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.City, "City" ) );
			propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.CompanyName, "CompanyName" ) );
			propertyProjectors.Add( new EntityPropertyProjector( CustomerFields.Country, "Country" ) );
			propertyProjectors.Add( new EntityPropertyProjector( new EntityProperty( "IsDirty" ), "IsDirty" ) );

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
				if(i > 0)
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

	/// <summary>
	/// Projector to project raw projection data onto a custom customer class.
	/// </summary>
	public class DataProjectorToCustomClass<T> : IEntityDataProjector
		where T : class, new()
	{
		#region Class Member Declarations
		private List<T> _destination;
		private Dictionary<string, PropertyDescriptor> _propertyDescriptors;
		#endregion

		/// <summary>
		/// CTor
		/// </summary>
		/// <param name="destination">destination collection of new projection results</param>
		public DataProjectorToCustomClass( List<T> destination )
		{
			if( destination == null )
			{
				throw new ArgumentNullException( "destination", "destination can't be null" );
			}

			_destination = destination;

			// cache descriptors.
			T dummy = new T();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties( dummy );
			_propertyDescriptors = new Dictionary<string, PropertyDescriptor>( properties.Count );
			foreach( PropertyDescriptor property in properties )
			{
				_propertyDescriptors.Add( property.Name, property );
			}
		}

		#region IEntityDataProjector Members
		/// <summary>
		/// Adds a new projection result to the container contained into this instance. The container has to be set in the constructor.
		/// </summary>
		/// <param name="propertyProjectors"></param>
		/// <param name="rawProjectionResult">The raw projection result.</param>
		public void AddProjectionResultToContainer( List<IEntityPropertyProjector> propertyProjectors, object[] rawProjectionResult )
		{
			T newInstance = new T();

			for( int i = 0; i < propertyProjectors.Count; i++ )
			{
				PropertyDescriptor property = null;
				if( _propertyDescriptors.TryGetValue( propertyProjectors[i].ProjectedResultName, out property ) )
				{
					// has such a property, set it.
					object value = rawProjectionResult[i];
					if( value.Equals( DBNull.Value ) )
					{
						value = null;
					}
					property.SetValue( newInstance, value );
				}
			}

			_destination.Add( newInstance );
		}
		#endregion

		#region Class Property Declarations
		/// <summary>
		/// Gets the destination.
		/// </summary>
		/// <value>The destination.</value>
		public List<T> Destination
		{
			get { return _destination; }
		}
		#endregion
	}
}

