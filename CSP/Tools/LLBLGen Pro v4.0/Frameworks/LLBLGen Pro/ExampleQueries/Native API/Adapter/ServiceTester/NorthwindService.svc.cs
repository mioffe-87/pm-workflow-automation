using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.ServiceModel;
using Northwind.Adapter.Linq;
using Northwind.Adapter.DatabaseSpecific;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Linq.Expressions;
using Northwind.Adapter.EntityClasses;
using SD.LLBLGen.Pro.ODataSupportClasses;
using System.Reflection;

namespace ServiceTester
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class NorthwindService : LLBLGenProODataServiceBase<LinqMetaData>
	{
		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			// TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
			// Examples:
			config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
			// config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
			config.DataServiceBehavior.AcceptProjectionRequests = true;
			config.SetServiceOperationAccessRule("*", ServiceOperationRights.AllRead);
			config.UseVerboseErrors = true;
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


		[WebGet]
		public IQueryable<ProductEntity> GetProductByProductName(string productName)
		{
			IQueryable<ProductEntity> q = this.CurrentDataSource.Product;
			if(!string.IsNullOrEmpty(productName))
			{
				q = q.Where(p => p.ProductName == productName);
			}
			return q;
		}


		[QueryInterceptor("Product")]
		public Expression<Func<ProductEntity, bool>> OnReadProducts()
		{
			return p => p.Discontinued == false;
		}


		protected override string ContainerName
		{
			get
			{
				return "NorthwindService";
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
		

		// Override to manage returned exceptions.
		protected override void HandleException(HandleExceptionArgs args)
		{
			// Handle exceptions raised in service operations.
			if(args.Exception.GetType() ==
				typeof(TargetInvocationException)
				&& args.Exception.InnerException != null)
			{
				if(args.Exception.InnerException.GetType()
					== typeof(DataServiceException))
				{

					// Unpack the DataServiceException.
					args.Exception = args.Exception.InnerException as DataServiceException;

				}
				else
				{
					// Return a new DataServiceException as "400: bad request."
					args.Exception =
						new DataServiceException(400,
							args.Exception.InnerException.Message);
				}
			}
		}
	}
}
