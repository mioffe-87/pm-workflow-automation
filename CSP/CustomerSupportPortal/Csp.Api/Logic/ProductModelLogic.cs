using System.Collections.Generic;
using System.Transactions;
using Csp.Api.DataAccess;
using Csp.Api.Model;

namespace Csp.Api.Logic {
	public class ProductModelLogic : BaseLogic {
		/// <summary>
		/// This Method is used for creating ProductModel
		/// </summary>
		/// <param name="productModel"></param>
		/// <returns>ProductModel</returns>
		public ProductModel CreateProductModel(ProductModel productModel) {
			ProductModel resultProductModel = null;
			var productModelDataAccess = new ProductModelDataAccess();

			using (var transactionScope = new TransactionScope()) {
				resultProductModel = productModelDataAccess.Create(productModel);

				transactionScope.Complete();
			}

			return resultProductModel;
		}

		/// <summary>
		/// This metod is used for updating ProductModel
		/// </summary>
		/// <param name="productModel"></param>
		/// <returns>ProductModel</returns>
		public ProductModel UpdateProductModel(ProductModel productModel) {
			ProductModel resultProductModel = null;
			var productModelDataAccess = new ProductModelDataAccess();

			using (var transactionScope = new TransactionScope()) {
				resultProductModel = productModelDataAccess.Update(productModel);

				transactionScope.Complete();
			}

			return resultProductModel;
		}

		/// <summary>
		/// This method is used for retrieving ProductModel details based on ModelId
		/// </summary>
		/// <param name="modelId"></param>
		/// <returns>SupportAccount</returns>
		public ProductModel GetProductModel(long modelId) {
			var productModelDataAccess = new ProductModelDataAccess();
			var productModel = productModelDataAccess.Read(modelId);

			return productModel;
		}

		/// <summary>
		/// This method is used for retrieving ProductModel existance based on productModelName
		/// </summary>
		/// <param name="productModelName"></param>
		/// <returns>true if exists</returns>
		public bool IsProductModelExists(string productModelName) {
			var productModelDataAccess = new ProductModelDataAccess();
			var productModel = productModelDataAccess.GetProductModelByName(productModelName);

			return (productModel != null);
		}

		/// <summary>
		/// This method is used for updating ProductModel
		/// </summary>
		/// <param name="productModel"></param>
		public void DeleteProductModel(ProductModel productModel) {
			var productModelDataAccess = new ProductModelDataAccess();

			using (var transactionScope = new TransactionScope()) {
				productModelDataAccess.Delete(productModel);

				transactionScope.Complete();
			}
		}

		/// <summary>
		/// This method returns all active ProductModel
		/// </summary>
		/// <param name="dataParameters"></param>
		/// <returns></returns>
		public List<ProductModelListItem> GetProductModels(DataParameters dataParameters) {
			var productModelDataAccess = new ProductModelDataAccess();
			var productModelListItems = productModelDataAccess.ReadProductModels(dataParameters);

			return productModelListItems;
		}
	}
}