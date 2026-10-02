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

namespace Unittests.TestLibrary.SqlServerTests.Adapter
{
	[TestFixture]
	public class NWProjectionTests
	{
		#region Class Member Declarations
		private Guid _testRunID;
		#endregion

		#region Tests Setup/teardown
		[TestFixtureSetUp]
		public void Init()
		{
			// give testrunID
			_testRunID = Guid.NewGuid();
			Console.WriteLine( "TestRunID: {0}", _testRunID.ToString() );
			Console.WriteLine( "RT version: {0}.{1}", RuntimeLibraryVersion.Version, RuntimeLibraryVersion.Build );
		}


		[TestFixtureTearDown]
		public void CleanUp()
		{
		}
		#endregion


		[Test]
		public void FetchDataReaderWithProcTest()
		{
			IDataReader reader = null;
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				reader = adapter.FetchDataReader( RetrievalProcedures.GetCustOrdersOrdersCallAsQuery( "CHOPS" ), CommandBehavior.CloseConnection );
				Assert.AreEqual( 4, reader.FieldCount );
				Assert.IsFalse( reader.IsClosed );
				int rowCount = 0;
				while( reader.Read() )
				{
					Console.WriteLine( "Row: {0} | {1} | {2} | {3} |", reader.GetValue( 0 ), reader.GetValue( 1 ), reader.GetValue( 2 ), reader.GetValue( 3 ) );
					rowCount++;
				}
				Assert.AreEqual( 8, rowCount );
			}
			reader.Close();
		}


		[Test]
		public void FetchDataReaderWithDynamicFieldsTest()
		{
			IDataReader reader = null;
			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				EntityFields2 fields = new EntityFields2( 3 );
				fields[0] = CustomerFields.CustomerId;
				fields[1] = CustomerFields.CompanyName;
				fields[2] = OrderFields.OrderId;
				RelationPredicateBucket filter = new RelationPredicateBucket( CustomerFields.Country == "Germany" );
				filter.Relations.Add( CustomerEntity.Relations.OrderEntityUsingCustomerId );
				reader = adapter.FetchDataReader( fields, filter, CommandBehavior.CloseConnection, 0, true );
				Assert.AreEqual( 3, reader.FieldCount );
				Assert.IsFalse( reader.IsClosed );
				int rowCount = 0;
				while( reader.Read() )
				{
					Console.WriteLine( "Row: {0} | {1} | {2} |", reader.GetValue( 0 ), reader.GetValue( 1 ), reader.GetValue( 2 ) );
					rowCount++;
				}
				Assert.AreEqual( 121, rowCount );
			}
			reader.Close();
		}


		[Test]
		public void ProcFetchWithProjectionToDataTableTest()
		{
			DataTable results = new DataTable();
			DataProjectorToDataTable projector = new DataProjectorToDataTable( results );
			List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			valueProjectors.Add( new DataValueProjector( "OrderID", 0, typeof( int ) ) );
			valueProjectors.Add( new DataValueProjector( "OrderDate", 1, typeof( DateTime ) ) );
			valueProjectors.Add( new DataValueProjector( "RequiredDate", 2, typeof( DateTime ) ) );
			valueProjectors.Add( new DataValueProjector( "ShippedDate", 3, typeof( DateTime ) ) );

			using( IRetrievalQuery query = RetrievalProcedures.GetCustOrdersOrdersCallAsQuery( "CHOPS" ) )
			{
				using( DataAccessAdapter adapter = new DataAccessAdapter() )
				{
					adapter.FetchProjection( valueProjectors, projector, query );
				}
			}
			Assert.AreEqual( 8, results.Rows.Count );
			List<int> orderIDs = new List<int>();
			orderIDs.AddRange(new int[] { 10254, 10370, 10519, 10731, 10746, 10966, 11029, 11041 });
			foreach(DataRow row in results.Rows)
			{
				Assert.IsTrue(orderIDs.Contains((int)row[0]));
			}
		}


		/// <summary>
		/// Test with multiple-resultset returning proc which is projected onto a dataset.
		/// </summary>
		[Test]
		public void ProcFetchProjectionToDataSet()
		{
			DataSet results = new DataSet();
			results.Tables.Add( "Customers" );
			results.Tables.Add( "Orders" );

			using( IRetrievalQuery query = RetrievalProcedures.GetCustomersAndOrdersOnCountryCallAsQuery( "Germany" ) )
			{
				using( DataAccessAdapter adapter = new DataAccessAdapter() )
				{
					using( IDataReader reader = adapter.FetchDataReader( query, CommandBehavior.CloseConnection ) )
					{
						// first resultset: Customers.
						List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
						valueProjectors.Add( new DataValueProjector( "CustomerID", 0, typeof( string ) ) );
						valueProjectors.Add( new DataValueProjector( "CompanyName", 1, typeof( string ) ) );
						// resultset contains more rows, we just project those 2.
						DataProjectorToDataTable projector = new DataProjectorToDataTable( results.Tables["Customers"] );
						adapter.FetchProjection( valueProjectors, projector, reader );

						// second resultset: Orders. 
						valueProjectors = new List<IDataValueProjector>();
						valueProjectors.Add( new DataValueProjector( "OrderID", 0, typeof( int ) ) );
						valueProjectors.Add( new DataValueProjector( "CustomerID", 1, typeof( string ) ) );
						valueProjectors.Add( new DataValueProjector( "OrderDate", 3, typeof( DateTime ) ) );
						reader.NextResult();
						projector = new DataProjectorToDataTable( results.Tables["Orders"] );
						adapter.FetchProjection( valueProjectors, projector, reader );

						reader.Close();
					}
				}
			}
			Assert.AreEqual( 11, results.Tables["Customers"].Rows.Count );
			Assert.AreEqual( 121, results.Tables["Orders"].Rows.Count );
			Assert.AreEqual( 2, results.Tables["Customers"].Columns.Count );
			Assert.AreEqual( 3, results.Tables["Orders"].Columns.Count );
		}


		/// <summary>
		/// Test with multiple-resultset returning proc which is projected onto two entity collections (one per resultset)
		/// </summary>
		[Test]
		public void ProcFetchProjectionToEntityCollections()
		{
			EntityCollection<CustomerEntity> customers = new EntityCollection<CustomerEntity>( new CustomerEntityFactory() );
			EntityCollection<OrderEntity> orders = new EntityCollection<OrderEntity>( new OrderEntityFactory() );

			using( IRetrievalQuery query = RetrievalProcedures.GetCustomersAndOrdersOnCountryCallAsQuery( "Germany" ) )
			{
				using( DataAccessAdapter adapter = new DataAccessAdapter() )
				{
					using( IDataReader reader = adapter.FetchDataReader( query, CommandBehavior.CloseConnection ) )
					{
						// first resultset: Customers.
						List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
						// project value on index 0 in resultset row onto customerid
						valueProjectors.Add( new DataValueProjector( CustomerFields.CustomerId.ToString(), 0, typeof( string ) ) );
						// project value on index 1 in resultset row onto companyname
						valueProjectors.Add( new DataValueProjector( CustomerFields.CompanyName.ToString(), 1, typeof( string ) ) );
						// resultset contains more rows, we just project those 2.
						DataProjectorToIEntityCollection2 projector = new DataProjectorToIEntityCollection2( customers );
						adapter.FetchProjection( valueProjectors, projector, reader );

						// second resultset: Orders. 
						valueProjectors = new List<IDataValueProjector>();
						valueProjectors.Add( new DataValueProjector( OrderFields.OrderId.ToString(), 0, typeof( int ) ) );
						valueProjectors.Add( new DataValueProjector( OrderFields.CustomerId.ToString(), 1, typeof( string ) ) );
						valueProjectors.Add( new DataValueProjector( OrderFields.OrderDate.ToString(), 3, typeof( DateTime ) ) );
						reader.NextResult();
						projector = new DataProjectorToIEntityCollection2( orders );
						adapter.FetchProjection( valueProjectors, projector, reader );

						reader.Close();
					}
				}
			}
			Assert.AreEqual( 11, customers.Count );
			Assert.AreEqual( 121, orders.Count );
		}

		
		/// <summary>
		/// Fetches a fields set from the customers table and projects them onto custom customer classes.
		/// </summary>
		[Test]
		public void FieldsFetchProjectionToCustomClass()
		{
			List<CustomCustomer> customClasses = new List<CustomCustomer>();
			EntityFields2 fields = new EntityFields2( 4 );
			fields[0] = CustomerFields.City;
			fields[1] = CustomerFields.CompanyName;
			fields[2] = CustomerFields.CustomerId;
			fields[3] = CustomerFields.Country;

			DataProjectorToCustomClass<CustomCustomer> projector = new DataProjectorToCustomClass<CustomCustomer>( customClasses );
			List<IDataValueProjector> valueProjectors = new List<IDataValueProjector>();
			valueProjectors.Add( new DataValueProjector( "City", 0, typeof( string ) ) );
			valueProjectors.Add( new DataValueProjector( "CompanyName", 1, typeof( string ) ) );
			valueProjectors.Add( new DataValueProjector( "CustomerID", 2, typeof( string ) ) );
			valueProjectors.Add( new DataValueProjector( "Country", 3, typeof( string ) ) );

			using( DataAccessAdapter adapter = new DataAccessAdapter() )
			{
				adapter.FetchProjection( valueProjectors, projector, fields, null, 0, true );
			}

			Assert.AreEqual(91, customClasses.Count);
			foreach(CustomCustomer c in customClasses)
			{
				Assert.IsTrue(c.CustomerID.Length > 0);
				Assert.IsTrue(c.CompanyName.Length > 0);
			}
		}
	}



	/// <summary>
	/// Test class for projection of fetched entities onto custom classes using a custom projector.
	/// </summary>
	public class CustomCustomer
	{
		#region Class Member Declarations
		private string _customerID, _companyName, _city, _country;
		#endregion

		public CustomCustomer()
		{
			_city = string.Empty;
			_companyName = string.Empty;
			_customerID = string.Empty;
			_country = string.Empty;
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
		#endregion
	}
}
