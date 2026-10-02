using System;
using System.Collections.Generic;
using System.Data;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;

namespace Csp.Api.DataAccess {
	public class ProductModelDataAccess : BaseDataAccess {
		/// <summary>
		/// This Method is used for creating ProductModel
		/// </summary>
		/// <param name="productModel">Product Model</param>
		/// <returns>ProductModel</returns>
		public ProductModel Create(ProductModel productModel) {
			productModel.CreatedOn = DateTime.Now;
			const string sqlQuery = @"INSERT INTO models
                                    (
                                      Name
																		 ,ProductLine
																		 ,Platform
																		 ,MinimumRequiredVersion
																		 ,MaximumSupportedVersion
                                     ,CreatedOn
                                     ,CreatedByUserAccountId
                                     ,IsActive
                                    )
                                    VALUES
                                    (
                                      @p_Name
                                     ,@p_ProductLine
                                     ,@p_Platform
																		 ,@p_MinimumRequiredVersion
																		 ,@p_MaximumSupportedVersion
                                     ,@p_CreatedOn
                                     ,@p_CreatedByUserAccountId
                                     ,1
                                    );SELECT LAST_INSERT_ID();";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInModelParameters(command, productModel, CreateOrUpdate.Create);

				var modelId = Convert.ToInt64(Connection.ExecuteScalar(command));

				return Read(modelId);
			}
		}

		/// <summary>
		/// This method is used for retrieving ProductModel details based on ModelId
		/// </summary>
		/// <param name="modelId">Model Id</param>
		/// <returns>ProductModel</returns>
		public ProductModel Read(long modelId) {
			const string sqlQuery = "SELECT * FROM models WHERE ModelId = @p_ModelId;";
			var productModel = new ProductModel();

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_ModelId", DbType.Int64, modelId);

				using (IDataReader dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						productModel.PopulateProperties(dataReader);
					}
				}
			}

			return productModel;
		}

		/// <summary>
		///This Method is used to Update ProductModel Details
		/// </summary>
		/// <param name="productModel">ProductModel</param>
		/// <returns>ProductModel</returns>
		public ProductModel Update(ProductModel productModel) {
			const string sqlQuery = @"UPDATE models 
                                    SET
                                      Name = @p_Name
                                     ,ProductLine = @p_ProductLine
                                     ,Platform = @p_Platform
                                     ,MinimumRequiredVersion = @p_MinimumRequiredVersion
                                     ,MaximumSupportedVersion = @p_MaximumSupportedVersion
                                     ,LastUpdate = @p_LastUpdate
                                     ,LastUpdateByUserAccountId = @p_LastUpdateByUserAccountId
                                    WHERE
                                      ModelId = @p_ModelId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInModelParameters(command, productModel, CreateOrUpdate.Update);

				Connection.ExecuteNonQuery(command);
			}

			return Read(productModel.ModelId);
		}

		/// <summary>
		///This Method is used to Delete ProductModel Details
		/// </summary>
		/// <param name="productModel">ProductModel</param>
		public void Delete(ProductModel productModel) {
			const string sqlQuery = @"UPDATE models 
																SET 
																	IsActive = 0
																WHERE
																	ModelId = @p_ModelId";

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInModelParameters(command, productModel, CreateOrUpdate.Update);

				Connection.ExecuteNonQuery(command);
			}
		}

		/// <summary>
		/// This method is used for retrieving ProductModel details based on productModelName
		/// </summary>
		/// <param name="productModelName">productModelName</param>
		/// <returns>ProductModel</returns>
		public ProductModel GetProductModelByName(string productModelName) {
			const string sqlQuery = "SELECT * FROM models WHERE Name = @p_Name;";
			ProductModel productModel = null;

			using (var command = Connection.GetSqlStringCommand(sqlQuery)) {
				Connection.AddInParameter(command, "@p_Name", DbType.String, productModelName);

				using (var dataReader = Connection.ExecuteReader(command)) {
					if (dataReader.Read()) {
						productModel = new ProductModel();
						productModel.PopulateProperties(dataReader);
					}
				}
			}

			return productModel;
		}

		/// <summary>
		/// This method is used to return all active ProductModel
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public List<ProductModelListItem> ReadProductModels(DataParameters parameters) {
			List<ProductModelListItem> productModelListItems = null;

			using (var command = Connection.GetStoredProcCommand("GetProductModels")) {
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {

					productModelListItems = new List<ProductModelListItem>();
					while (dataReader.Read()) {

						var productModelListItem = new ProductModelListItem();
						productModelListItem.PopulateProperties(dataReader);
						productModelListItems.Add(productModelListItem);
					}
				}

				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return productModelListItems;
		}
	}
}