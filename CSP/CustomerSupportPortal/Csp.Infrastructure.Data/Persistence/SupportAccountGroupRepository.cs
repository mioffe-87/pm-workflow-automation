using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;

namespace Csp.Infrastructure.Data.Persistence {
	public class SupportAccountGroupRepository : ISupportAccountGroupRepository {
		public List<GroupListItem> ReadSupportAccountGroups(long supportAccountId, long restrictedByUserAccountId, RoleSecurity[] roleSecurities, DataParameters parameters) {

			var db = SessionDbContext.CspObjectContext;
			{

				var isSuper =
					roleSecurities.Any(
						x =>
						x.SecurityClassification == RoleSecurity.SecurityClassifications.Super &&
						x.SupportAccountId == supportAccountId);

				var isTac =
					roleSecurities.Any(
						x =>
						x.SecurityClassification == RoleSecurity.SecurityClassifications.Tac
						&& x.SupportAccountId.HasValue
						&& x.SupportAccountId != supportAccountId);

				IQueryable<GroupListItem> results;

				var minDateTime = DateTime.MinValue;

				if (isSuper || isTac) {

					results =
						(
							from sag in db.supportaccountgroups

							join createdBy in db.useraccounts on sag.CreatedByUserAccountId equals createdBy.UserAccountId into
								tempCreatedBy
							from leftCreatedBy in tempCreatedBy.DefaultIfEmpty()

							join updatedBy in db.useraccounts on sag.LastUpdateByUserAccountId equals updatedBy.UserAccountId into
								tempUpdatedBy
							from leftUpdatedBy in tempUpdatedBy.DefaultIfEmpty()

							where sag.SupportAccountId == supportAccountId
							select new GroupListItem {
								GroupId = sag.GroupId,
								SupportAccountGroupName = sag.Name,
								Description = sag.Description,
								CreatedOn = sag.CreatedOn ?? minDateTime,
								//LastUpdate = sag.LastUpdate ?? minDateTime,
								CreatedByEmail = leftCreatedBy == null ? "" : leftCreatedBy.Email,
								CreatedByUserAccountId = sag.CreatedByUserAccountId ?? 0,
								LastUpdateByEmail = leftUpdatedBy == null ? "" : leftUpdatedBy.Email,
								LastUpdateByUserAccountId = sag.LastUpdateByUserAccountId ?? 0,
							}).Distinct();
				} else {

					var utcNow = DateTime.Now;

					results =
						(
							from sag in db.supportaccountgroups
							join mem in db.memberships on sag.GroupId equals mem.GroupId
							join mr in db.membershiproles on mem.MembershipId equals mr.MembershipId

							join createdBy in db.useraccounts on sag.CreatedByUserAccountId equals createdBy.UserAccountId into
								tempCreatedBy
							from leftCreatedBy in tempCreatedBy.DefaultIfEmpty()

							join updatedBy in db.useraccounts on sag.LastUpdateByUserAccountId equals updatedBy.UserAccountId into
								tempUpdatedBy
							from leftUpdatedBy in tempUpdatedBy.DefaultIfEmpty()

							where
								(mem.ExpirationDate >= utcNow || !mem.ExpirationDate.HasValue)
								&& mem.ActivationDate <= utcNow
								&& mem.UserAccountId == restrictedByUserAccountId
								&& mr.role.SecurityClassification == (int)RoleSecurity.SecurityClassifications.Standard
								&& sag.SupportAccountId == supportAccountId

							select new GroupListItem {
								GroupId = sag.GroupId,
								SupportAccountGroupName = sag.Name,
								Description = sag.Description,
								CreatedOn = sag.CreatedOn ?? minDateTime,
								//LastUpdate = sag.LastUpdate ?? minDateTime,
								CreatedByEmail = leftCreatedBy == null ? "" : leftCreatedBy.Email,
								CreatedByUserAccountId = sag.CreatedByUserAccountId ?? 0,
								LastUpdateByEmail = leftUpdatedBy == null ? "" : leftUpdatedBy.Email,
								LastUpdateByUserAccountId = sag.LastUpdateByUserAccountId ?? 0,
							}).Distinct();
				}

				//filters
				if (!String.IsNullOrEmpty(parameters.SearchValue)) {
					switch (parameters.SearchField) {

						case "SupportAccountGroupName":
							results = results.Where(u => u.SupportAccountGroupName.Contains(parameters.SearchValue));
							break;
						case "Description":
							results = results.Where(u => u.Description.Contains(parameters.SearchValue));
							break;
						case "CreatedByEmail":
							results = results.Where(u => u.CreatedByEmail.Contains(parameters.SearchValue));
							break;
						case "CreatedOn":
							DateTime dtCreatedOn;
							DateTime.TryParse(parameters.SearchValue, out dtCreatedOn);
							var maxRange = dtCreatedOn.AddDays(1);
							results = results.Where(u => u.CreatedOn >= dtCreatedOn && u.CreatedOn < maxRange);
							break;
					}
				}

				//sort
				switch (parameters.SortField) {

					case "SupportAccountGroupName":
						results = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															? results.OrderByDescending(u => u.SupportAccountGroupName)
															: results.OrderBy(u => u.SupportAccountGroupName);

						break;
					case "Description":
						results = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															? results.OrderByDescending(u => u.Description)
															: results.OrderBy(u => u.Description);
						break;
					case "CreatedByEmail":
						results = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															? results.OrderByDescending(u => u.CreatedByEmail)
															: results.OrderBy(u => u.CreatedByEmail);
						break;
					case "CreatedOn":
						results = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															? results.OrderByDescending(u => u.CreatedOn)
															: results.OrderBy(u => u.CreatedOn);
						break;
					default:
						results = results.OrderBy(u => u.SupportAccountGroupName);
						break;

				}

				//skip take
				parameters.TotalRecordCount = results.Count();

				if (parameters.PaginationCount != Int32.MaxValue) {
					results = results.Skip(Convert.ToInt32(parameters.PaginationOffset)).Take(parameters.PaginationCount);
				}

				return results.ToList();
			}
		}

		public long GetSupportAccountIdByGroupId(long groupId) {

			using (var db = new cspEntities()) {
				return db.supportaccountgroups.Single(x => x.GroupId == groupId).SupportAccountId;
			}
		}

	    public void AddDeviceCategoryDevicesToGroup(long groupId, long? deviceCategoryId, long createdByUserAccountId, string deviceType)
	    {
            var db = SessionDbContext.CspObjectContext;

            db.Database.ExecuteSqlCommand(
                "CALL AddDeviceCategoryDevicesToGroup(@p_GroupId, @p_DeviceCategoryId, @p_CreatedByUserAccountId, @p_DeviceType);",
                new MySqlParameter("p_GroupId", groupId),
                new MySqlParameter("p_DeviceCategoryId", deviceCategoryId),
                new MySqlParameter("p_CreatedByUserAccountId", createdByUserAccountId),
                new MySqlParameter("p_DeviceType", deviceType));
	    }
	}
}
