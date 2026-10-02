using System;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.Persistence.Configuration;
using Moq;
using NHibernate.Event;
using PAN.Common.Core.Persistence;
using PAN.Common.Infrastructure.Data.Persistence;

namespace Csp.Infrastructure.Data.Tests.NHHelpers {
	public class GetNhRepo : IDisposable {

		public readonly NHibernateUnitOfWork UnitOfWork;
		public readonly IRepository Repository;

		public GetNhRepo() {

			var applicationSettings = new Mock<IApplicationSettings>();
			var connectionStrings = new Mock<IConnectionStrings>();
			var preInsertEventListener = new Mock<IPreInsertEventListener>();
			var preUpdateEventListener = new Mock<IPreUpdateEventListener>();

			//connectionStrings.Setup(x => x.CspDatabase).Returns("Datasource=sjccsqld.paloaltonetworks.local;Database=dbtests_csp;uid=dbtests_user;pwd=dbtests_pass;Connect Timeout=45;default command timeout=60;Max Pool Size=50; Min Pool Size=10");
			connectionStrings.Setup(x => x.CspDatabase).Returns("Datasource=localhost;Database=csp;uid=testuser;pwd=testuser;Connect Timeout=45;default command timeout=60;Max Pool Size=50; Min Pool Size=10");

			var session = new SessionSourceConfiguration(applicationSettings.Object, connectionStrings.Object, preInsertEventListener.Object, preUpdateEventListener.Object);
			UnitOfWork = new NHibernateUnitOfWork(session);
			Repository = new NHibernateGenericRepository(UnitOfWork);

			UnitOfWork.BeginNewTransaction();
		}

		public void Dispose() {
			UnitOfWork.Commit(false);
		}
	}
}