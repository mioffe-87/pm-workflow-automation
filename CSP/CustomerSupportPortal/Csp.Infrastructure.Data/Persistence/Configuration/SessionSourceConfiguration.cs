using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;
using FluentNHibernate;
using FluentNHibernate.Cfg.Db;
using NHibernate.Event;
using PAN.Common.Infrastructure.Data.Persistence.Configuration;
using System.Collections.Generic;

namespace Csp.Infrastructure.Data.Persistence.Configuration {
	public class SessionSourceConfiguration : ISessionSourceConfiguration {
		private readonly IApplicationSettings _applicationSettings;
		private readonly IConnectionStrings _connectionStrings;

		public SessionSourceConfiguration(IApplicationSettings applicationSettings, IConnectionStrings connectionStrings, IPreInsertEventListener preInsertEventListener, IPreUpdateEventListener preUpdateEventListener) {
			_applicationSettings = applicationSettings;
			_connectionStrings = connectionStrings;

			SessionSource = CreateSessionSource(new CspPersistenceModel());
			SessionSource.Configuration.EventListeners.PreInsertEventListeners = new[] { preInsertEventListener };
			SessionSource.Configuration.EventListeners.PreUpdateEventListeners = new[] { preUpdateEventListener };
		}

		public ISessionSource CreateSessionSource(PersistenceModel model) {
			/*
			FluentConfiguration fluentConfiguration = Fluently.Configure()
					//.ProxyFactoryFactory("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle")
					.Database(MySQLConfiguration.Standard.ConnectionString(_connectionStrings.ContractManagementConnection).ShowSql().AdoNetBatchSize(50));

			return new SessionSource(fluentConfiguration.BuildConfiguration().Properties, model);*/

			var properties = GetProperties();


			properties["command_timeout"] = "120";

			return new SessionSource(properties, model);
		}

		public ISessionSource SessionSource { get; set; }

		private IDictionary<string, string> GetProperties() {
			return MySQLConfiguration
					.Standard
                    .ConnectionString(Secrets.CspDatabase)
					.AdoNetBatchSize(50)
					.ShowSql()
					.ToProperties();
		}
	}
}
