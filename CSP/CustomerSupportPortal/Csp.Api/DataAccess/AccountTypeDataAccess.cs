using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Csp.Api.DataAccess.HelperClasses;
using System.Linq;

namespace Csp.Api.DataAccess {

	public class AccountTypeDataAccess : BaseDataAccess {

		#region Private Member Variables

		private static List<AccountType> accountTypeList;

		#endregion

		#region Protected Properties

		protected static List<AccountType> AccountTypeList {
			get {
				if (accountTypeList == null) {
					AccountTypeDataAccess accountTypeDataAccess = new AccountTypeDataAccess();
					accountTypeList = accountTypeDataAccess.GetAllAccountTypes(new DataParameters());
				}

				return accountTypeList;
			}
			set {
				accountTypeList = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method is used to retrieve the list of AccountType
		/// </summary>
		/// <param name="parameters">Pagination Parameters</param>
		/// <returns>List<AccountType></returns>
		public List<AccountType> GetAllAccountTypes(DataParameters parameters) {

			List<AccountType> accountTypeList = null;
			AccountType accountType = null;

			using (var command = Connection.GetStoredProcCommand("GetAccountTypes")) {
				Connection.AddPaginationParameters(command, parameters);
				Connection.AddSortParameters(command, parameters);
				Connection.AddSearchParameters(command, parameters);

				using (var dataReader = Connection.ExecuteReader(command)) {
					accountTypeList = new List<AccountType>();

					while (dataReader.Read()) {
						accountType = new AccountType();
						accountType.PopulateProperties(dataReader);
						accountTypeList.Add(accountType);
					}

				}


				if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value) {
					parameters.TotalRecordCount = (long)(command.Parameters["p_TotalRecordCount"].Value);
				}
			}

			return accountTypeList;
		}

		/// <summary>
		/// This method is used to get List of All AccountType
		/// </summary>
		/// <param name="parameters">Pagination Parameter</param>
		/// <returns>List<AccountType></returns>
		public List<AccountType> Read(DataParameters parameters) {

			List<AccountType> accountTypeList = null;

			parameters.TotalRecordCount = AccountTypeList.Count;
			accountTypeList = ((from accountType in AccountTypeList
													select accountType).Skip(Convert.ToInt32(parameters.PaginationOffset))
													.Take(parameters.PaginationCount).ToList());
			return accountTypeList;
		}

		#endregion

	}
}
