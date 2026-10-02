using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NW26.Adapter.Linq;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;

using LinqExpression = System.Linq.Expressions.Expression;
using SD.LLBLGen.Pro.LinqSupportClasses.ExpressionHandlers;
using NW26.Adapter.EntityClasses;
using NW26.Adapter.FactoryClasses;
using NW26.Adapter.HelperClasses;
using NW26.Adapter.DatabaseSpecific;
using SD.LLBLGen.Pro.QuerySpec;

namespace LinqTester
{
	public enum TestEnum
	{
		Value1,
		Value2
	}


    public class MyLinqMetaData : LinqMetaData
	{
		public MyLinqMetaData()
		{
		}

		public MyLinqMetaData(IDataAccessAdapter adapterToUse)
			: base(adapterToUse)
		{
		}

		public MyLinqMetaData(IDataAccessAdapter adapterToUse, FunctionMappingStore customFunctionMappings)
			: base(adapterToUse, customFunctionMappings)
		{
		}

		public EntityCollection<TEntity> AsCollection<TEntity>()
			where TEntity : EntityBase2, IEntity2
		{
			var queryable = new DataSource2<TEntity>(this.AdapterToUse, new ElementCreator(), this.CustomFunctionMappings, this.ContextToUse);
			return ((ILLBLGenProQuery)queryable).Execute<EntityCollection<TEntity>>();
		}
	}


	public static class PredicateBuilder
	{
		public static System.Linq.Expressions.Expression<Func<T, bool>> Null<T>() { return null; }

		public static System.Linq.Expressions.Expression<Func<T, bool>> Or<T>(this System.Linq.Expressions.Expression<Func<T, bool>> expr1,
															System.Linq.Expressions.Expression<Func<T, bool>> expr2)
		{
			if(expr1 == null)
			{
				return expr2;
			}
			ExpressionReplacer replacer = new ExpressionReplacer(CreateFromToReplaceSet(expr2.Parameters, expr1.Parameters), null, null, null, null);
			LambdaExpression rightExpression = (LambdaExpression)replacer.HandleExpression(expr2);
			
			return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
				  (System.Linq.Expressions.Expression.OrElse(expr1.Body, rightExpression.Body), expr1.Parameters);
		}

		public static System.Linq.Expressions.Expression<Func<T, bool>> And<T>(this System.Linq.Expressions.Expression<Func<T, bool>> expr1,
															 System.Linq.Expressions.Expression<Func<T, bool>> expr2)
		{
			if(expr1 == null)
			{
				return expr2;
			}
			ExpressionReplacer replacer = new ExpressionReplacer(CreateFromToReplaceSet(expr2.Parameters, expr1.Parameters), null, null, null, null);
			LambdaExpression rightExpression = (LambdaExpression)replacer.HandleExpression(expr2);
			return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>
				  (System.Linq.Expressions.Expression.AndAlso(expr1.Body, rightExpression.Body), expr1.Parameters);
		}


		private static Dictionary<LinqExpression, LinqExpression> CreateFromToReplaceSet(IList<ParameterExpression> from, 
											IList<ParameterExpression> to)
		{
			Dictionary<LinqExpression, LinqExpression> toReturn = new Dictionary<LinqExpression, LinqExpression>();
			for(int i = 0; i < from.Count; i++)
			{
				toReturn.Add(from[i], to[i]);
			}
			return toReturn;
		}
	}


	public class Customer
	{
		public Customer() { }
		public Customer(string custId, string contact, string companyNm)
		{
			this.CustId = custId;
			this.Contact = contact;
			this.CompanyNme = companyNm;
		}


		public string CustId { get; set; }
		public string CompanyNme { get; set; }
		public string Contact { get; set; }
		public IEnumerable<Order> Orders { get; set; }
		public IEnumerable<Employee> Employees { get; set; }
	}



	public class OrderDerived : OrderEntity
	{
		public string CompanyName { get; set; }

		protected override IEntityFactory2 CreateEntityFactory()
		{
			return new OrderDerivedFactory();
		}
	}


	public class OrderDerivedFactory : OrderEntityFactory
	{
		public override IEntityCollection2 CreateEntityCollection()
		{
			return new EntityCollection<OrderDerived>(this);
		}

		public override IEntity2 Create()
		{
			return new OrderDerived();
		}
	}


	public class Order2
	{
		public int OId { get; set; }
		public CustomerEntity Customer { get; set; }
	}


	public class Order
	{
		public int OrdId { get; set; }
		public Customer RelatedCustomer { get; set; }
		public IList<Product> Products { get; set; }
		public Employee RelatedEmployee { get; set; }
	}


	public class Employee
	{
		public int EmployeeId { get; set;}
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}


	public class Product
	{
		public int ProdId { get; set; }
		public string Name { get; set; }
	}

	public class Product2 : Product
	{
		public Product2() : base() { }
		public Product2(int prodId, string name, bool discontinued)
		{
			this.ProdId = prodId;
			this.Name = name;
			this.Discontinued = discontinued;
		}
		public bool Discontinued { get; set; }
	}


	public class TargetObject
	{
		public int OrderId { get; set; }
		public string CustomerId { get; set; }
		public DateTime? OrderDate { get; set; }
	}


	public class Pair<T, U>
	{
		public Pair()
		{
		}

		public Pair(T value1, U value2)
		{
			this.Value1 = value1;
			this.Value2 = value2;
		}

		public T Value1 { get; set; }
		public U Value2 { get; set; }
	}


	public class Concatenator
	{
		#region Class Member Declarations
		private string _val1, _val2;
		#endregion

		public Concatenator()
		{
		}


		public Concatenator(string val1, string val2)
		{
			_val1 = val1;
			_val2 = val2;
		}

		public string ConcatMembers()
		{
			return _val1 + _val2;
		}


		public static string Concat(string val1, string val2)
		{
			return new Concatenator(val1, val2).ConcatMembers();
		}


		public string ConcatValues(string val1, string val2)
		{
			_val1 = val1;
			_val2 = val2;
			return ConcatMembers();
		}


		#region Class Property Declarations
		public string Val1
		{
			get { return _val1; }
			set { _val1 = value; }
		}

		public string Val2
		{
			get { return _val2; }
			set { _val2 = value; }
		}

		public string ConcattedResult
		{
			get { return ConcatMembers(); }
		}
		#endregion
	}


	/// <summary>
	/// Function stubs for functions which are actually in the db. To define them here, we can use them in linq queries and define mappings on them to be used
	/// in the db.
	/// </summary>
	internal class NorthwindFunctions
	{
	    /// <summary>
	    /// Calculates the order total.
	    /// </summary>
	    /// <param name="orderId">The order id.</param>
	    /// <param name="useDiscounts">if set to <c>true</c> [use discounts].</param>
	    /// <returns></returns>
	    public static decimal CalculateOrderTotal(int orderId, bool useDiscounts)
	    {
	        return 0.0M;
	    }

	    /// <summary>
	    /// Fulls the text search.
	    /// </summary>
	    /// <param name="fieldToSearch">The field to search.</param>
	    /// <param name="toFind">To find.</param>
	    /// <returns></returns>
	    public static bool FullTextSearch(string fieldToSearch, string toFind)
	    {
	        return true;
	    }
	}


	/// <summary>
	/// Function stubs for functions which are actually in the db. To define them here, we can use them in query spec queries and define mappings on them to be used
	/// in the db.
	/// </summary>
	internal class NorthwindFunctionsQS
	{
		/// <summary>
		/// Calculates the order total.
		/// </summary>
		/// <param name="orderId">The order id.</param>
		/// <param name="useDiscounts">if set to <c>true</c> [use discounts].</param>
		/// <returns></returns>
		public static FunctionMappingExpression CalculateOrderTotal(IEntityFieldCore orderId, bool useDiscounts)
		{
			return new FunctionMappingExpression(typeof(NorthwindFunctionsQS), "CalculateOrderTotal", 2, orderId, useDiscounts);
		}

		/// <summary>
		/// Fulls the text search.
		/// </summary>
		/// <param name="fieldToSearch">The field to search.</param>
		/// <param name="toFind">To find.</param>
		/// <returns></returns>
		public static FunctionMappingExpression FullTextSearch(IEntityFieldCore fieldToSearch, string toFind)
		{
			return new FunctionMappingExpression(typeof(NorthwindFunctionsQS), "FullTextSearch", 2, fieldToSearch, toFind);
		}
	}



	/// <summary>
	/// Mappings of .net methods/functions to db functions in the northwind catalog.
	/// </summary>
	internal class NorthwindFunctionMappings : FunctionMappingStore
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="NorthwindFunctionMappings"/> class.
	    /// </summary>
	    public NorthwindFunctionMappings()
	        : base()
	    {
	        // define the mapping. SQLServer 2000 needs the schema to be present for the function call, so we specify that as well.
	        this.Add(new FunctionMapping(typeof(NorthwindFunctions), "CalculateOrderTotal", 2, "fn_CalculateOrderTotal({0}, {1})", "Northwind", "dbo"));
	        // DaysInMonth(2)
	        this.Add(new FunctionMapping(typeof(DateTime), "DaysInMonth", 2, "DATEDIFF(d, CONVERT(datetime, '' + CONVERT(NVARCHAR(4), {0}) + '-' + CONVERT(NVARCHAR(4), {1}) + '-01'), DATEADD(m, 1, CONVERT(datetime, '' + CONVERT(NVARCHAR(4), {0}) + '-' + CONVERT(NVARCHAR(4), {1}) + '-01')))"));
	        // FullTextSearch(2) on 1 field
	        this.Add(new FunctionMapping(typeof(NorthwindFunctions), "FullTextSearch", 2, "CONTAINS({0}, {1})"));
	    }
	}


	/// <summary>
	/// Mappings of .net methods/functions to db functions in the northwind catalog.
	/// </summary>
	internal class NorthwindFunctionMappingsQS : FunctionMappingStore
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NorthwindFunctionMappings"/> class.
		/// </summary>
		public NorthwindFunctionMappingsQS()
			: base()
		{
			// define the mapping. SQLServer 2000 needs the schema to be present for the function call, so we specify that as well.
			this.Add(new FunctionMapping(typeof(NorthwindFunctionsQS), "CalculateOrderTotal", 2, "fn_CalculateOrderTotal({0}, {1})", "Northwind", "dbo"));
			// DaysInMonth(2)
			this.Add(new FunctionMapping(typeof(DateTime), "DaysInMonth", 2, "DATEDIFF(d, CONVERT(datetime, '' + CONVERT(NVARCHAR(4), {0}) + '-' + CONVERT(NVARCHAR(4), {1}) + '-01'), DATEADD(m, 1, CONVERT(datetime, '' + CONVERT(NVARCHAR(4), {0}) + '-' + CONVERT(NVARCHAR(4), {1}) + '-01')))"));
			// FullTextSearch(2) on 1 field
			this.Add(new FunctionMapping(typeof(NorthwindFunctionsQS), "FullTextSearch", 2, "CONTAINS({0}, {1})"));
		}
	}


	internal class AdventureWorksFunctions
	{
		public static double Sin(double a)
		{
			return 0.0;
		}

		public static double Cos(double a)
		{
			return 0.0;
		}

		public static double Atan(double a)
		{
			return 0.0;
		}

		public static double Sqrt(double a)
		{
			return 0.0;
		}
	}


	internal class AdventureWorksFunctionsQS
	{
		public static FunctionMappingExpression Sin(object a)
		{
			return new FunctionMappingExpression(typeof(AdventureWorksFunctionsQS), "Sin", 1, a);
		}

		public static FunctionMappingExpression Cos(object a)
		{
			return new FunctionMappingExpression(typeof(AdventureWorksFunctionsQS), "Cos", 1, a);
		}

		public static FunctionMappingExpression Atan(object a)
		{
			return new FunctionMappingExpression(typeof(AdventureWorksFunctionsQS), "Atan", 1, a);
		}

		public static FunctionMappingExpression Sqrt(object a)
		{
			return new FunctionMappingExpression(typeof(AdventureWorksFunctionsQS), "Sqrt", 1, a);
		}
	}
	
	internal class AdventureWorksFunctionMappings : FunctionMappingStore
	{
		public AdventureWorksFunctionMappings()
		{
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctions), "Sin", 1, "SIN({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctions), "Cos", 1, "COS({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctions), "Atan", 1, "ATAN({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctions), "Sqrt", 1, "SQRT({0})"));
		}
	}


	internal class AdventureWorksFunctionMappingsQS : FunctionMappingStore
	{
		public AdventureWorksFunctionMappingsQS()
		{
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctionsQS), "Sin", 1, "SIN({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctionsQS), "Cos", 1, "COS({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctionsQS), "Atan", 1, "ATAN({0})"));
			this.Add(new FunctionMapping(typeof(AdventureWorksFunctionsQS), "Sqrt", 1, "SQRT({0})"));
		}
	}



	internal class OrderData
	{
		public int OrderId { get; set; }
		public Double EmployeeIdAsFloat { get; set; }
		public string CustomerId { get; set; }
	}

	internal class CustomerCountsPerCountry
	{
		public string Country { get; set; }
		public int NumberOfCustomers { get; set; }
	}
}
