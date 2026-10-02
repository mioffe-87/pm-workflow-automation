using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using System.Reflection;
using Csp.Api.Model.Attributes;
using Csp.Api.Model;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Security;

namespace Csp.Api.DataAccess.HelperClasses {

	/// <summary>
	/// This class is used to extend the Microsoft.Practices.EnterpriseLibrary.Data.Database class. 
	/// </summary>
	public static class DatabaseExtension {


		#region Private Methods

		/// <summary>
		/// A call to this method is only needed when creating or updating a model's information in the database.
		/// This is a private worker method used to create and add parameters for all properties that need to be stored. 
		/// This method iterates through the properties of a decendant of BaseCspModel and creates 
		/// parameters for each property decorated with a DataAcces attribute.
		/// </summary>
		/// <param name="connection">The Database we are extending</param>
		/// <param name="command">The command that uses the parameters</param>
		/// <param name="model">The decendant of a BaseCspModel</param>
		/// <param name="createOrUpdate">Used to ensure only create or update parameters are created</param>
		private static void addInPropertyParameters(Database connection, DbCommand command, BaseCspModel model, CreateOrUpdate createOrUpdate) {

			string parameterName;
			DataAccessAttribute attribute;
			DbType dataType;
			Type modelType = model.GetType();

			// Get all properties that are decorated with the DataAccessAttribute 
			IEnumerable<PropertyInfo> memberProperties =
					modelType.GetProperties().Where(property => Attribute.IsDefined(property, typeof(DataAccessAttribute)));

			// Iterate through the list of properties
			foreach (var property in memberProperties) {

				//Read the attribute
				attribute = property.GetCustomAttribute<DataAccessAttribute>(true);

				/*If this is for a create statement then only use the property if it is marked to be
				 * used for create. If this is an update statement then only use the property if it 
				 * is marked for update. Otherwise do not use this property.
				 */
				if (((createOrUpdate == CreateOrUpdate.Create) && (!attribute.UseForCreate)) ||
						((createOrUpdate == CreateOrUpdate.Update) && (!attribute.UseForUpdate))) {

					continue;
				}

				//Get the name of the field converted to a property name using an extension class
				parameterName = attribute.StorageName.ToParameterName();
				//Get the datatype specified in the property which defaults to a string. 
				dataType = attribute.DataType;

				object value;

				//If the property is a date time and null or the min value then set the value of the parameter to null; otherwise set it to the value
				if (dataType == DbType.DateTime && (property.GetValue(model) == null || (DateTime)property.GetValue(model) == DateTime.MinValue)) {
					value = DBNull.Value;
				} else {
					value = property.GetValue(model);
				}
				//Add the parameter
				connection.AddInParameter(command, parameterName, dataType, value);

			}
		}

		/// <summary>
		/// A call to this method is only needed when creating or updating a model's information in the database.
		/// This is a private worker method used to create and add parameters for a Model that is a composite
		/// or aggregate object stored as a property. This method iterates through the properties of a decendant of 
		/// BaseCspModel and creates parameters for each property decorated with a DataAccessModel attribute unless key 
		/// only is specified in the DataAccessModel attribute. If Key only is specified then only the key field is set.  
		/// </summary>
		/// <param name="connection">The database connection we are extending</param>
		/// <param name="command">The command that uses the parameters</param>
		/// <param name="model">a Decendant of BaseCspModel</param>
		/// <param name="createOrUpdate">Used to ensure only create or update parameters are created</param>
		/// <param name="overrideUseKeyOnly">If true only the key will be populated and the value of the attribute will be ignored.</param>
		private static void addInModelPropertyParameters(Database connection, DbCommand command, BaseCspModel model, CreateOrUpdate createOrUpdate, bool overrideUseKeyOnly) {
			string parameterName;
			string propertyName;
			DataAccessModelAttribute attribute;
			DbType dataType;
			Type modelType = model.GetType();
			BaseCspModel propertyObject;
			/*Get all the properties that have the DataAccessAttribute, this is the attribute that defines an property
             * that has an object as the value that is a BaseCspModel object.
             */
			IEnumerable<PropertyInfo> memberProperties =
		modelType.GetProperties().Where(property => Attribute.IsDefined(property, typeof(DataAccessModelAttribute)));

			//iterate through the properties
			foreach (var property in memberProperties) {

				//Get the DataAccessModelAttribute
				attribute = property.GetCustomAttribute<DataAccessModelAttribute>(true);


				/*If this is for a create statement then only use the property if it is marked to be
				 * used for create. If this is an update statement then only use the property if it 
				 * is marked for update. Otherwise do not use this property.
				 */
				if (((createOrUpdate == CreateOrUpdate.Create) && (!attribute.UseForCreate)) ||
						((createOrUpdate == CreateOrUpdate.Update) && (!attribute.UseForUpdate))) {

					continue;
				}
				//Get the property name of the key field in the object
				propertyName = attribute.PropertyName;
				//Get the field name of the property in the database and convert to a parameter using an extension class
				parameterName = attribute.StorageName.ToParameterName();

				//Get the object that is in the property
				propertyObject = (BaseCspModel)property.GetValue(model);
				modelType = propertyObject.GetType();
				dataType = attribute.DataType;

				//If we are to only use the key value of the object then grab the value for the key field only
				if (attribute.UseKeyOnly || overrideUseKeyOnly) {

					connection.AddInParameter(command, parameterName, dataType, modelType.GetProperty(propertyName).GetValue(propertyObject));

				} else {
					//Otherwise call addInPropertyParameters to add all values of the object as parameters
					addInPropertyParameters(connection, command, propertyObject, createOrUpdate);

				}
			}
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Helper method to add in all the values of a property decorated with a DataAccess or DataAccessModel attribute
		/// as parameters. This method is only designed to be used in an create or update statement to the database. 
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="model">The BaseCspModel used to retrieve the parameters from</param>
		/// <param name="createOrUpdate">Set to create if creating a record or update if updating a record in the database with the parameters</param>
		/// <param name="overrideUseKeyOnly">Default is false. Set to true if you only want the key field of a object in a property. 
		///  This is only used if the DataAccessModel.UseKeyOnly is false but only the key is used in this instance.   </param>
		public static void AddInModelParameters(this Database connection, DbCommand command, BaseCspModel model, CreateOrUpdate createOrUpdate, bool overrideUseKeyOnly = false) {

			//Add all of the properties that hold database values
			addInPropertyParameters(connection, command, model, createOrUpdate);
			//Add in all of the properties of objects in the model that have DataAccess attributes
			addInModelPropertyParameters(connection, command, model, createOrUpdate, overrideUseKeyOnly);
		}


		/// <summary>
		/// Helper method to add in a RoleId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="roleId">The ID used as the value for the parameter</param>
		public static void AddInRoleIdParameter(this Database connection, DbCommand command, long roleId) {

			connection.AddInParameter(command, RoleFields.RoleId.ToParameterName(), DbType.Int64, roleId);
		}

		/// <summary>
		/// Helper method to add in a roleCategory as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="roleCategory">The ID used as the value for the parameter </param>
		public static void AddInRoleCategory(this Database connection, DbCommand command, RoleCategory roleCategory) {

			connection.AddInParameter(command, RoleFields.Category.ToParameterName(), DbType.Int32, roleCategory);
		}

		/// <summary>
		/// Helper method to add in a MembershipId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="membershipId">The ID used as the value for the parameter</param>
		public static void AddInMembershipIdParameter(this Database connection, DbCommand command, long membershipId) {

			connection.AddInParameter(command, MembershipFields.MembershipId.ToParameterName(), DbType.Int64, membershipId);
		}


		/// <summary>
		/// Helper method to add in a RoleId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="userAccountId">The ID used as the value for the parameter</param>
		public static void AddInUserAccountIdParameter(this Database connection, DbCommand command, long userAccountId) {

			connection.AddInParameter(command, UserAccountFields.UserAccountId.ToParameterName(), DbType.Int64, userAccountId);
		}

		/// <summary>
		/// Helper method to add in a SupportAccountId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="supportAccountId">The ID used as the value for the parameter</param>
		public static void AddInSupportAccountIdParameter(this Database connection, DbCommand command, long supportAccountId) {

			connection.AddInParameter(command, SupportAccountFields.SupportAccountId.ToParameterName(), DbType.Int64, supportAccountId);
		}

		/// <summary>
		/// Helper method to add in a DeviceId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="deviceId">The ID used as the value for the parameter</param>
		public static void AddInDeviceIdParameter(this Database connection, DbCommand command, long deviceId) {

			connection.AddInParameter(command, DeviceFields.DeviceId.ToParameterName(), DbType.Int64, deviceId);
		}

		/// <summary>
		/// Helper method to add in a licenseId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="licenseId">The ID used as the value for the paramter</param>
		public static void AddInLicenseIdParameter(this Database connection, DbCommand command, long licenseId) {

			connection.AddInParameter(command, LicenseFields.LicenseId.ToParameterName(), DbType.Int64, licenseId);
		}

        /// <summary>
        /// Helper method to add in a license AuthCode as a Parameter
        /// </summary>
        /// <param name="connection">The database object that is extended extending</param>
        /// <param name="command">The command that the parameters are for</param>
        /// <param name="licenseId">The ID used as the value for the paramter</param>
        public static void AddInLicenseAuthCodeParameter(this Database connection, DbCommand command, string authCode)
        {
            connection.AddInParameter(command, LicenseFields.AuthorizationCode.ToParameterName(), DbType.String, authCode);
        }

		/// <summary>
		/// Helper method to add in a the UserAccountId as a Parameter of the user that deletes a record
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="deletedByUserAccountId">The UserAccountId that is the value for the parameter</param>
		public static void AddInDeletedByUserAccountIdParameter(this Database connection, DbCommand command, long deletedByUserAccountId) {

			connection.AddInParameter(command, AuditCspModelClassFields.DeletedByUserAccountId.ToParameterName(), DbType.Int64, deletedByUserAccountId);
		}
		/// <summary>
		/// Helper method to add in a the UserAccountId as a Parameter of the user that is viewing the data. 
		/// This is used to implement the data viewing security of the system. The security classifications 
		/// of roles and membership are used to determine and filter the data the user can view.
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="restrictedByUserAccountId">The UserAccountId that is the value for the parameter</param>
		public static void AddInRestrictedByUserAccountId(this Database connection, DbCommand command, long restrictedByUserAccountId) {

			connection.AddInParameter(command, DataParameterFields.RestrictedByUserAccountId.ToParameterName(), DbType.Int64, restrictedByUserAccountId);
		}

		/// <summary>
		/// Helper method to add in a the UserAccountId as a Parameter of the user that creates a record
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="createdByUserAccountId">The UserAccountId that is the value for the parameter</param>
		public static void AddInCreatedByUserAccountIdParameter(this Database connection, DbCommand command, long createdByUserAccountId) {

			connection.AddInParameter(command, AuditCspModelClassFields.CreatedByUserAccountId.ToParameterName(), DbType.Int64, createdByUserAccountId);
		}

		/// <summary>
		/// Helper method to add in a GroupId as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="groupId">The ID used as the value for the parameter</param>
		public static void AddInGroupIdParameter(this Database connection, DbCommand command, long groupId) {

			connection.AddInParameter(command, GroupFields.GroupId.ToParameterName(), DbType.Int64, groupId);
		}



		/// <summary>
		/// Takes the pagination values from a DataParameters object and converts them to database parameters
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="paginationParameters">The DataParameters object that holds the pagination values used in the parameters</param>
		public static void AddPaginationParameters(this Database connection, DbCommand command, DataParameters paginationParameters) {

			connection.AddInParameter(command, DataParameterFields.PaginationOffset.ToParameterName(), DbType.Int64, paginationParameters.PaginationOffset);
			connection.AddInParameter(command, DataParameterFields.PaginationCount.ToParameterName(), DbType.Int32, paginationParameters.PaginationCount);
			connection.AddOutParameter(command, DataParameterFields.TotalRecordCount.ToParameterName(), DbType.Int64, 0);
		}

		/// <summary>
		/// Takes the search values from a DataParameters object and converts them to database parameters
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="searchParameters">The DataParameters object that holds the search values used in the parameters</param>
		public static void AddSearchParameters(this Database connection, DbCommand command, DataParameters searchParameters) {

			connection.AddInParameter(command, DataParameterFields.SearchField.ToParameterName(), DbType.String, searchParameters.SearchField);
			connection.AddInParameter(command, DataParameterFields.SearchValue.ToParameterName(), DbType.String, searchParameters.SearchValue);

		}

		/// <summary>
		/// Takes the sort values from a DataParameters object and converts them to database parameters
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="sortParameters">The DataParameters object that holds the sort values used in the parameters</param>
		public static void AddSortParameters(this Database connection, DbCommand command, DataParameters sortParameters) {

			connection.AddInParameter(command, DataParameterFields.SortField.ToParameterName(), DbType.String, sortParameters.SortField);
			connection.AddInParameter(command, DataParameterFields.SortDirection.ToParameterName(), DbType.String, sortParameters.SortDirection.ToSqlOrderBy());

		}

		/// <summary>
		/// Helper method to add in a email address as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="email">The email address that is used as the value for the parameter</param>
		public static void AddInEmailParameter(this Database connection, DbCommand command, string email) {
			connection.AddInParameter(command, UserAccountFields.Email.ToParameterName(), DbType.String, email);
		}

		/// <summary>
		/// Helper method to add in a includeAll boolean value as a Parameter
		/// </summary>
		/// <param name="connection">The database object that is extended extending</param>
		/// <param name="command">The command that the parameters are for</param>
		/// <param name="includeAll">The boolean used as the value for the paramter</param>
		public static void AddInIncludeAllParameter(this Database connection, DbCommand command, bool includeAll) {

			connection.AddInParameter(command, SupportAccountFields.IncludeAll.ToParameterName(), DbType.Boolean, includeAll);
		}

	    private static bool IsNullableType(Type valueType)
	    {
	        return (valueType.IsGenericType && valueType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
	    }

	    public static T GetColumnValue<T>(this IDataReader reader, string columnName)
	    {
	        object value = reader[columnName];
	        Type valueType = typeof(T);
	        if (value != DBNull.Value)
	        {
	            if (!IsNullableType(valueType))
	            {
	                return (T)Convert.ChangeType(value, valueType);
	            }
	            else
	            {
	                NullableConverter nc = new NullableConverter(valueType);
	                return (T)Convert.ChangeType(value, nc.UnderlyingType);
	            }
	        }

	        return default(T);
	    }

		#endregion
	}
}
