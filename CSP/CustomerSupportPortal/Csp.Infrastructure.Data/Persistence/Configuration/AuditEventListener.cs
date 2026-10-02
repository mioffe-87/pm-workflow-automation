using System;
using System.Web;
using Csp.Api.EntityClasses;
using Csp.Api.Security;
using NHibernate.Event;
using NHibernate.Persister.Entity;

namespace Csp.Infrastructure.Data.Persistence.Configuration {
	public class AuditEventListener : IPreInsertEventListener, IPreUpdateEventListener {
		#region IPreInsertEventListener Members

		public bool OnPreInsert(PreInsertEvent @event) {
			var createAuditEntity = @event.Entity as ICspCreateAuditEntity;
			if (createAuditEntity == null)
				return false;

			var userId = ((CspPrincipal)HttpContext.Current.User).UserAccount.Id;
			var dateTime = DateTime.Now;

			Set(@event.Persister, @event.State, "CreatedByUserAccountId", userId);
			Set(@event.Persister, @event.State, "CreatedOn", dateTime);

			if (userId > 0) {
				createAuditEntity.CreatedByUserAccountId = userId;
			}

			createAuditEntity.CreatedOn = dateTime;

			var changeAuditableEntity = @event.Entity as ICspUpdateAuditEntity;
			if (changeAuditableEntity == null)
				return false;

			Set(@event.Persister, @event.State, "LastUpdateByUserAccountId", userId);
			Set(@event.Persister, @event.State, "LastUpdate", dateTime);

			changeAuditableEntity.LastUpdateByUserAccountId = userId;
			changeAuditableEntity.LastUpdate = dateTime;

			return false;
		}

		#endregion

		#region IPreUpdateEventListener Members

		public bool OnPreUpdate(PreUpdateEvent @event) {
			var changeAuditEntity = @event.Entity as ICspUpdateAuditEntity;
			if (changeAuditEntity == null)
				return false;

			var userId = ((CspPrincipal)HttpContext.Current.User).UserAccount.Id;
			var dateTime = DateTime.Now;

			Set(@event.Persister, @event.State, "LastUpdateByUserAccountId", userId);
			Set(@event.Persister, @event.State, "LastUpdate", dateTime);

			if (userId > 0) {
				changeAuditEntity.LastUpdateByUserAccountId = userId;
			}

			changeAuditEntity.LastUpdate = dateTime;

			return false;
		}

		#endregion

		private static void Set(IEntityPersister persister, object[] state, string name, object value) {
			int index = Array.IndexOf(persister.PropertyNames, name);

			if (index == -1)
				return;

			state[index] = value;
		}
	}
}
