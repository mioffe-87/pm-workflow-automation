using System;
using System.Collections.Generic;
using System.Text;
using SD.LLBLGen.Pro.ORMSupportClasses;

// Namespace with test classes for DI testing. 
namespace Unittests.TestLibrary.SqlServerTests.SelfServicing
{

	/// <summary>
	/// Testclass which is injected in all entities of the Northwind assembly
	/// </summary>
	[DependencyInjectionInfo(typeof(IEntity), "ConcurrencyPredicateFactoryToUse", ContextType = DependencyInjectionContextType.NewInstancePerTarget, 
		TargetNamespaceFilter="Northwind")]
	[Serializable]
	public class GeneralNorthwindCPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			Console.WriteLine("Create Predicate called on entity: {0}", ((IEntity)containingEntity).LLBLGenProEntityName);
			return null;
		}
	}


	/// <summary>
	/// Testclass which is injected in all entities of the Northwind assembly
	/// </summary>
	[DependencyInjectionInfo(typeof(IEntity), "ConcurrencyPredicateFactoryToUse", ContextType = DependencyInjectionContextType.NewInstancePerTarget,
	   TargetNamespaceFilter = "InheritanceOne")]
	[Serializable]
	public class GeneralIH1CPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			Console.WriteLine("Create Predicate called on entity: {0}", ((IEntity)containingEntity).LLBLGenProEntityName);
			return null;
		}
	}


	/// <summary>
	/// Specific CPF for product, to test scopes. 
	/// </summary>
	[Serializable]
	public class ProductSpecificCPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			return null;
		}
	}


	/// <summary>
	/// Specific CPF for product, to test nested scopes. 
	/// </summary>
	[Serializable]
	public class NestedProductSpecificCPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			return null;
		}
	}


	/// <summary>
	/// Specific CPF for Employee (IH1), to test scopes. 
	/// </summary>
	[Serializable]
	public class EmployeeSpecificCPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			return null;
		}
	}


	/// <summary>
	/// Specific CPF for Manager (IH1), to test nested scopes. 
	/// </summary>
	[Serializable]
	public class NestedManagerSpecificCPF : IConcurrencyPredicateFactory
	{
		public IPredicateExpression CreatePredicate(ConcurrencyPredicateType predicateTypeToCreate, object containingEntity)
		{
			return null;
		}
	}


	/// <summary>
	/// Testscope for inheritance DI testing
	/// </summary>
	public class IH1TestScope : DependencyInjectionScope
	{
		protected override void InitializeScope()
		{
			AddInjectionInfo(typeof(EmployeeSpecificCPF), typeof(InheritanceOne.Dal.SelfServicing.EntityClasses.EmployeeEntity), "ConcurrencyPredicateFactoryToUse",
					DependencyInjectionTargetKind.Hierarchy, DependencyInjectionContextType.NewInstancePerTarget);
		}
	}


	/// <summary>
	/// Testscope for inheritance DI testing
	/// </summary>
	public class NestedIH1TestScope : DependencyInjectionScope
	{
		protected override void InitializeScope()
		{
			// define an absolute injection. This means that employee and boardmember will get the EmployeeSpecificCPF, manager will get the nestedmanagerspecificcpf
			AddInjectionInfo(typeof(NestedManagerSpecificCPF), typeof(InheritanceOne.Dal.SelfServicing.EntityClasses.ManagerEntity), "ConcurrencyPredicateFactoryToUse",
					DependencyInjectionTargetKind.Absolute, DependencyInjectionContextType.NewInstancePerTarget);
		}
	}
	

	/// <summary>
	/// Test scope to test scope specific DI
	/// </summary>
	public class TestScope : DependencyInjectionScope
	{
		/// <summary>
		/// Initializes the scope. Use this method to add injection info to the scope by subsequential calls to AddInjectionInfo/&gt;
		/// </summary>
		protected override void InitializeScope()
		{
			AddInjectionInfo(typeof(ProductSpecificCPF), typeof(Northwind.SelfServicing.EntityClasses.ProductEntity), "ConcurrencyPredicateFactoryToUse");
		}
	}


	/// <summary>
	/// Test scope to test nested scopes DI 
	/// </summary>
	public class NestedTestScope : DependencyInjectionScope
	{
		/// <summary>
		/// Initializes the scope. Use this method to add injection info to the scope by subsequential calls to AddInjectionInfo/&gt;
		/// </summary>
		protected override void InitializeScope()
		{
			AddInjectionInfo(typeof(NestedProductSpecificCPF), typeof(Northwind.SelfServicing.EntityClasses.ProductEntity), "ConcurrencyPredicateFactoryToUse");
		}
	}
}
