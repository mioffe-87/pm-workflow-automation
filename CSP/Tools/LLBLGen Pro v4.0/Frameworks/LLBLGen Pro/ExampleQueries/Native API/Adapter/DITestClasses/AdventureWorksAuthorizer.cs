using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Threading;

using AdventureWorks.Dal.Adapter;
using AdventureWorks.Dal.Adapter.EntityClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace Authorizers
{
	public class AdventureWorksAuthorizerBase : AuthorizerBase
	{
		/// <summary>
		/// Gets the current identity.
		/// </summary>
		/// <returns></returns>
		protected IIdentity GetCurrentIdentity()
		{
			return Thread.CurrentPrincipal.Identity;
		}
	}


	[DependencyInjectionInfo(typeof(LocationEntity), "AuthorizerToUse")]
	[DependencyInjectionInfo(typeof(ProductVendorEntity), "AuthorizerToUse")]
	[DependencyInjectionInfo(typeof(CustomerEntity), "AuthorizerToUse")]
	public class GeneralAdventureWorksAuthorizer : AdventureWorksAuthorizerBase
	{
		public override bool CanGetFieldValue(IEntityCore entity, int fieldIndex)
		{
			return base.CanGetFieldValue(entity, fieldIndex);
		}
	}



	/// <summary>
	/// Class which is used to authorize AdventureWorks entity access. 
	/// </summary>
	/// <remarks>Used as a singleton.</remarks>
	[DependencyInjectionInfo(typeof(EmployeeEntity), "AuthorizerToUse", ContextType = DependencyInjectionContextType.Singleton)]
	public class AdventureWorksEmployeeAuthorizer : AdventureWorksAuthorizerBase
	{
		public AdventureWorksEmployeeAuthorizer()
		{
			// If stuff has to be injected into this authorizer, one can do so by using the di provider as illustrated below.
			DependencyInjectionInfoProviderSingleton.PerformDependencyInjection(this);
		}

		/// <summary>
		/// Determines whether the caller can obtain the value for the field with the index specified from the entity type specified.
		/// </summary>
		/// <param name="entity">The entity instance to obtain the value from.</param>
		/// <param name="fieldIndex">Index of the field to obtain the value for.</param>
		/// <returns>True if the caller is allowed to obtain the value, false otherwise</returns>
		public override bool CanGetFieldValue(IEntityCore entity, int fieldIndex)
		{
			bool toReturn = ((EntityType)entity.LLBLGenProEntityTypeValue == EntityType.EmployeeEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();

			switch((EmployeeFieldIndex)fieldIndex)
			{
				case EmployeeFieldIndex.NationalIdnumber:
					if(currentIdentity.Name.StartsWith("TestUser"))
					{
						// deny
						toReturn = false;
					}
					break;
				default:
					toReturn = true;
					break;
			}

			return toReturn;
		}


		/// <summary>
		/// Determines whether the caller can set the value for the field with the index specified of the entity type specified.
		/// </summary>
		/// <param name="entity">The entity instance the field is located in.</param>
		/// <param name="fieldIndex">Index of the field to set the value of.</param>
		/// <returns>true if the caller is allowed to set the value, false otherwise</returns>
		public override bool CanSetFieldValue(IEntityCore entity, int fieldIndex)
		{
			bool toReturn = ((EntityType)entity.LLBLGenProEntityTypeValue == EntityType.EmployeeEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();

			switch((EmployeeFieldIndex)fieldIndex)
			{
				case EmployeeFieldIndex.SalariedFlag:
					if(currentIdentity.Name.StartsWith("TestUser"))
					{
						// deny
						toReturn = false;
					}
					break;
				default:
					toReturn = true;
					break;
			}

			return toReturn;
		}
	}


	/// <summary>
	/// Class which is used to authorize AdventureWorks entity access. 
	/// </summary>
	/// <remarks>Used as a singleton.</remarks>
	[DependencyInjectionInfo(typeof(CreditCardEntity), "AuthorizerToUse", ContextType=DependencyInjectionContextType.Singleton)]
	public class AdventureWorksCreditCardAuthorizer : AdventureWorksAuthorizerBase
	{
		/// <summary>
		/// Gets the result hint what to do when authorization fails when fetch a new entity.
		/// </summary>
		/// <returns>
		/// any of the FetchNewAuthorizationFailureResultHint values
		/// </returns>
		public override FetchNewAuthorizationFailureResultHint GetFetchNewAuthorizationFailureResultHint()
		{
			return FetchNewAuthorizationFailureResultHint.ClearData; // default is throw away. THis override gives us empty entities instead.
		}

		/// <summary>
		/// Determines whether the caller is allowed to load the data into the entity instance specified.
		/// </summary>
		/// <param name="entity">The entity instance to fill with data</param>
		/// <returns>true if the caller is allowed to load the data in the entity specified.</returns>
		/// <remarks>Data inside the entity is the data fetched from the db. If the method returns false, the entity will be reset with a new set of empty fields</remarks>
		public override bool CanLoadEntity(IEntityCore entity)
		{
			bool toReturn = ((EntityType)entity.LLBLGenProEntityTypeValue == EntityType.CreditCardEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
		}
	}


	/// <summary>
	/// Class which is used to authorize adventure works entity access
	/// </summary>
	/// <remarks>Used as a singleton.</remarks>
	[DependencyInjectionInfo(typeof(DepartmentEntity), "AuthorizerToUse", ContextType = DependencyInjectionContextType.Singleton)]
	public class AdventureWorksDepartmentAuthorizer : AdventureWorksAuthorizerBase
	{
		/// <summary>
		/// Determines whether the caller is allowed to update entities directly in the database.
		/// </summary>
		/// <param name="entity">the entity which is passed in to the method to batch update the entities directly in the database, e.g. UpdateMulti (Selfservicing)
		/// or UpdateEntitiesDirectly (adapter)</param>
		/// <returns>true if the caller is allowed to perform the update, false otherwise</returns>
		public override bool CanBatchUpdateEntitiesDirectly(IEntityCore entity)
		{
			bool toReturn = ((EntityType)entity.LLBLGenProEntityTypeValue == EntityType.DepartmentEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
		}


		/// <summary>
		/// Determines whether the caller is allowed to delete entities directly in the database.
		/// </summary>
		/// <param name="typeOfEntity">the type of the entity to batch delete instances of directly in the database, e.g. DeleteMulti (Selfservicing)
		/// or DeleteEntitiesDirectly (adapter)</param>
		/// <returns>true if the caller is allowed to perform the delete, false otherwise</returns>
		public override bool CanBatchDeleteEntitiesDirectly(Type typeOfEntity)
		{
			bool toReturn = ((EntityType)base.GetEntityTypeValueForType(typeOfEntity) == EntityType.DepartmentEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
		}

	}


	/// <summary>
	/// Class which is used to authorize adventure works entity access
	/// </summary>
	/// <remarks>Used as a singleton.</remarks>
	[DependencyInjectionInfo(typeof(CountryRegionEntity), "AuthorizerToUse", ContextType = DependencyInjectionContextType.Singleton)]
	public class AdventureWorksCountryRegionAuthorizer : AdventureWorksAuthorizerBase
	{
		/// <summary>
		/// Determines whether the caller is allowed to save the new instance passed in.
		/// </summary>
		/// <param name="entityToSave">The entity to save.</param>
		/// <returns>true if the caller is allowed to save the new instance passed in, false otherwise</returns>
		public override bool CanSaveNewEntity(IEntityCore entityToSave)
		{
			bool toReturn = ((EntityType)entityToSave.LLBLGenProEntityTypeValue == EntityType.CountryRegionEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
			
		}


		/// <summary>
		/// Determines whether the caller is allowed to save the modified existing instance passed in.
		/// </summary>
		/// <param name="entityToSave">The entity to save.</param>
		/// <returns>true if the caller is allowed to save the new instance passed in, false otherwise</returns>
		public override bool CanSaveExistingEntity(IEntityCore entityToSave)
		{
			bool toReturn = ((EntityType)entityToSave.LLBLGenProEntityTypeValue == EntityType.CountryRegionEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
		}
		

		/// <summary>
		/// Determines whether the caller is allowed to delete of the entity type passed in
		/// </summary>
		/// <param name="entity">The entity to delete.</param>
		/// <returns>true if the caller is allowed to delete the entity passed in, false otherwise</returns>
		public override bool CanDeleteEntity(IEntityCore entity)
		{
			bool toReturn = ((EntityType)entity.LLBLGenProEntityTypeValue == EntityType.CountryRegionEntity);
			IIdentity currentIdentity = base.GetCurrentIdentity();
			if(currentIdentity.Name.StartsWith("TestUser"))
			{
				// deny
				toReturn = false;
			}
			return toReturn;
		}
	}
}
