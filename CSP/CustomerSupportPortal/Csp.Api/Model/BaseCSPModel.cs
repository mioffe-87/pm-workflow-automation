using System;
using System.Data;
using System.Reflection;
using Csp.Api.Model.Attributes;
using System.Collections.Generic;

using System.Linq;

namespace Csp.Api.Model {

	public abstract class BaseCspModel {

		#region Private Methods

		private class PropertyCache {

			public PropertyInfo PropertyInfo { get; set; }
			public string FieldName { get; set; }

			public PropertyCache(PropertyInfo propertyInfo, string fieldName) {

				PropertyInfo = propertyInfo;
				FieldName = fieldName;

			}

		}

		private static Dictionary<Type, List<PropertyCache>> cache = new Dictionary<Type, List<PropertyCache>>();


		private bool IfTypeExistsInCache(Type type) {

			if (!cache.ContainsKey(type)) {
				cache.Add(type, new List<PropertyCache>());
				return false; //return false as it does not exist in cache
			}
			return true; //return true as it exists in the cache
		}

		private bool fieldExists(IDataReader reader, string fieldName) {
			try {
				reader.GetOrdinal(fieldName);
				return true;
			} catch (IndexOutOfRangeException) {
				return false;
			}
		}

		private void populateUncachedMemberProperties(BaseCspModel model, Type modelType, IDataReader reader) {

			string fieldName;
			DataAccessAttribute attribute;

			var memberProperties =
							modelType.GetProperties().Where(property => Attribute.IsDefined(property, typeof(DataAccessAttribute)));

			foreach (var property in memberProperties.ToList()) {


				attribute = property.GetCustomAttribute<DataAccessAttribute>(true);
				fieldName = attribute.StorageName;
				cache[modelType].Add(new PropertyCache(property, fieldName));
				if ((fieldExists(reader, fieldName)) && (reader[fieldName] != DBNull.Value)) {

					var val = reader[fieldName];
					//Added this code to support byte[] data in ENG QA where the mysql version is older and needs this
					if (val is byte[] && property.PropertyType == typeof(string)) {
						val = System.Text.Encoding.UTF8.GetString((byte[])reader[fieldName]);
					}

					if (modelType.Name == "LicenseListItem" && fieldName == "IsPerpetual" && val is Int32 && property.PropertyType == typeof(bool)) {
						val = val.ToString() == "0" ? false : true;
					}

                    if (fieldName == "EnableEmailMfa" && property.PropertyType == typeof(bool))
                    {
                        val = val.ToString() == "0" ? false : true;
                    }

                    property.SetValue(model, val);

				}
			}

		}


		private void populateCachedMemberProperties(BaseCspModel model, Type modelType, IDataReader reader) {

			string fieldName;
			PropertyInfo property;

			var memberCache = cache[modelType];

			foreach (var propertyCache in memberCache.ToList()) {

				fieldName = propertyCache.FieldName;
				property = propertyCache.PropertyInfo;

				if ((fieldExists(reader, fieldName)) && (reader[fieldName] != DBNull.Value)) {

					var val = reader[fieldName];
					//Added this code to support byte[] data in ENG QA where the mysql version is older and needs this
					if (val is byte[] && property.PropertyType == typeof(string)) {
						val = System.Text.Encoding.UTF8.GetString((byte[])reader[fieldName]);
					}

					if (modelType.Name == "LicenseListItem" && fieldName == "IsPerpetual" && val is Int32 && property.PropertyType == typeof(bool)) {
						val = val.ToString() == "0" ? false : true;
					}

                    if (fieldName == "EnableEmailMfa" &&  property.PropertyType == typeof(bool))
                    {
                        val = val.ToString() == "0" ? false : true;
                    }

                    property.SetValue(model, val);

				}
			}

		}


		private void populateMemberProperties(BaseCspModel model, Type modelType, IDataReader reader) {

			if (IfTypeExistsInCache(modelType)) {
				populateCachedMemberProperties(model, modelType, reader);
			} else {
				populateUncachedMemberProperties(model, modelType, reader);
			}
		}


		private void populateModelProperties(Type modelType, IDataReader reader, bool overrideUseKeyOnly) {

			string fieldName;
			string propertyName;
			BaseCspModel modelObject;
			Type modelObjectType;
			DataAccessModelAttribute attribute;

			var modelProperties =
					modelType.GetProperties().Where(property => Attribute.IsDefined(property, typeof(DataAccessModelAttribute)));

			foreach (var property in modelProperties.ToList()) {

				attribute = property.GetCustomAttribute<DataAccessModelAttribute>(true);
				propertyName = attribute.PropertyName;
				fieldName = attribute.StorageName;
				modelObject = (BaseCspModel)property.GetValue(this);
				modelObjectType = modelObject.GetType();

				if (attribute.UseKeyOnly || overrideUseKeyOnly) {

					if ((fieldExists(reader, fieldName)) && (reader[fieldName] != DBNull.Value)) {

						var modelProp = modelObjectType.GetProperty(propertyName);
						if (modelProp != null) {
							modelProp.SetValue(modelObject, reader[fieldName]);
						}
					}

				} else {
					populateMemberProperties(modelObject, modelObjectType, reader);

				}
			}
		}

		#endregion

		#region Public Properties

		public void PopulateProperties(IDataReader reader, bool overrideKeysOnly = false) {

			var modelType = this.GetType();

			populateMemberProperties(this, modelType, reader);
			populateModelProperties(modelType, reader, overrideKeysOnly);

		}

		#endregion

	}
}
