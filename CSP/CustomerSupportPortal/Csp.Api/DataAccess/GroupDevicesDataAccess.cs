using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Csp.Api.DataAccess.HelperClasses;

namespace Csp.Api.DataAccess {

	public class GroupDevicesDataAccess : BaseDataAccess {

		#region Public Methods

		/// <summary>
		/// This method is used to create relation between group and device based on paameter passed
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="deviceId">Device Id</param>
		/// <param name="createdByUserAccountId">Created By User Account Id</param>
		/// <returns>void</returns>
		public void Create(long groupId, long deviceId, long createdByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("CreateGroupDevice")) {

				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInCreatedByUserAccountIdParameter(command, createdByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}


		/// <summary>
		/// This method is used to delete device relation with a group based on parameter passed
		/// </summary>
		/// <param name="groupId">Group Id</param>
		/// <param name="deviceId">Device Id</param>
		/// <returns>void</returns>
		public void Delete(long groupId, long deviceId, long deletedByUserAccountId) {

			using (var command = Connection.GetStoredProcCommand("DeleteGroupDevice")) {

				Connection.AddInGroupIdParameter(command, groupId);
				Connection.AddInDeviceIdParameter(command, deviceId);
				Connection.AddInDeletedByUserAccountIdParameter(command, deletedByUserAccountId);

				Connection.ExecuteNonQuery(command);
			}
		}



		#endregion

	}
}
