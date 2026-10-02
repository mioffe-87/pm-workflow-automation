using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.ServiceModel;
using SD.LLBLGen.Pro.ODataSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using ODataUnitTests.Dal.Adapter.Linq;
using ODataUnitTests.Dal.Adapter.DatabaseSpecific;

namespace ServiceTester
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class UnitTestService : LLBLGenProODataServiceBase<LinqMetaData>
	{
		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			// TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
			// Examples:
			config.SetEntitySetAccessRule("*", EntitySetRights.All);
			// config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
			config.DataServiceBehavior.AcceptProjectionRequests = true;
			config.DataServiceBehavior.AcceptCountRequests = true;
			config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
			config.UseVerboseErrors = true;
			SD.Tools.OrmProfiler.Interceptor.InterceptorCore.Initialize("ServiceTester.UnitTestService");
		}


		/// <summary>
		/// Creates the linq meta data instance.
		/// </summary>
		/// <returns></returns>
		protected override LinqMetaData CreateLinqMetaDataInstance()
		{
			return new LinqMetaData(new DataAccessAdapter());
		}


		/// <summary>
		/// Creates the transaction controller instance.
		/// </summary>
		/// <returns></returns>
		protected override ITransactionController CreateTransactionControllerInstance()
		{
			return new DataAccessAdapter();
		}


		/// <summary>
		/// Creates the unit of work instance.
		/// </summary>
		/// <returns></returns>
		protected override IUnitOfWorkCore CreateUnitOfWorkInstance()
		{
			return new UnitOfWork2();
		}

		protected override string ContainerName
		{
			get
			{
				return "UnitTestService";
			}
		}

		/// <summary>
		/// Gets the container namespace.
		/// </summary>
		protected override string ContainerNamespace
		{
			get
			{
				return "ServiceTester";
			}
		}
	}
}