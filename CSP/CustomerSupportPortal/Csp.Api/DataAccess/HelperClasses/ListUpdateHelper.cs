using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Delegates;
using Csp.Api.HelperClasses.Literals;

namespace Csp.Api.DataAccess.HelperClasses {


	class ListUpdateHelper {

		#region Private Fields

		/// <summary>
		/// This is the CreatedByUserAccountIdUserAccountId or deletedByUserAccountId used in the delegate
		/// </summary>
		private long userAccountId;

		/// <summary>
		/// This is the list of values in the database, key is the child table and value is the parent table primary keys.
		/// </summary>
		private Dictionary<long, long> databaseKeys = new Dictionary<long, long>();

		/// <summary>
		/// This is the list of values to update the database, key is the child table and value is the parent table primary keys.
		/// </summary>
		private Dictionary<long, long> updateKeys = new Dictionary<long, long>();

		/// <summary>
		/// Event for calling method to create the record.
		/// </summary>
		private event PerformListUpdate onCreate;

		/// <summary>
		/// Event for calling method to delete the record.
		/// </summary>
		private event PerformListUpdate onDelete;


		#endregion

		#region Private Methods

		/// <summary>
		/// Calls the onCreate event for the calling method to create a record in the database for each item in the update
		/// that does not exist in the database.
		/// </summary>
		private void createListObjects() {

			foreach (var keyValuePair in updateKeys) {

				if (!databaseKeys.ContainsKey(keyValuePair.Key)) {
					onCreate(keyValuePair.Value, keyValuePair.Key, userAccountId);
				}
			}
		}


		/// <summary>
		/// Calls the onDelete event for the calling method to delete a record in the database for each item in the database
		/// that does not exist in the update list.
		/// </summary>
		private void deleteListObjects() {

			foreach (var keyValuePair in databaseKeys) {

				if (!updateKeys.ContainsKey(keyValuePair.Key)) {
					onDelete(keyValuePair.Value, keyValuePair.Key, userAccountId);
				}
			}
		}


		#endregion

		#region Public Methods and Constructors

		/// <summary>
		/// Uses events from calling methods to create and delete all records that are in the update list.
		/// </summary>
		public void UpdateDatabase() {
			createListObjects();
			deleteListObjects();
		}

		/// <summary>
		/// Constructor for the LookupUpdateHelper class that is used to update lists of joining tables when the containing object is 
		/// updated.
		/// </summary>
		/// <param name="databaseKeys">The list of values from the database in the format child table primary key, parent table primary key.</param>
		/// <param name="updateKeys">The list of values being updated in the format child table primary key, parent table primary key.</param>
		/// <param name="userAccountId">The user that is making the updates for audit trail.</param>
		/// <param name="onCreate">The method that inserts the record in the database from the calling class.</param>
		/// <param name="onDelete">The method that deletes the record in the database from the calling class.</param>
		public ListUpdateHelper(Dictionary<long, long> databaseKeys, Dictionary<long, long> updateKeys, long userAccountId,
				PerformListUpdate onCreate, PerformListUpdate onDelete) {

			if (databaseKeys == null) {
				throw new ArgumentNullException(String.Format(ExceptionMessages.ArgumentNullException, "databaseKeys", "LookupUpdateHelper"));
			}
			if (updateKeys == null) {
				throw new ArgumentNullException(String.Format(ExceptionMessages.ArgumentNullException, "updateKeys", "LookupUpdateHelper"));
			}

			if (userAccountId <= 0) {
				throw new ArgumentException(String.Format(ExceptionMessages.ArgumentException, "userAccountId", "LookupUpdateHelper"));
			}

			if (onCreate == null) {
				throw new ArgumentNullException(String.Format(ExceptionMessages.ArgumentNullException, "onCreate", "LookupUpdateHelper"));
			}
			if (onDelete == null) {
				throw new ArgumentNullException(String.Format(ExceptionMessages.ArgumentNullException, "onDelete", "LookupUpdateHelper"));
			}


			this.databaseKeys = databaseKeys;
			this.updateKeys = updateKeys;
			this.userAccountId = userAccountId;
			this.onCreate = onCreate;
			this.onDelete = onDelete;
		}


		#endregion

	}

}
