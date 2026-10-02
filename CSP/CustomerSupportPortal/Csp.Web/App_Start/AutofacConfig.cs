using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Castle.Components.DictionaryAdapter;
using Csp.Api.Services.Contracts;
using Csp.Web.Modules;

namespace Csp.Web.App_Start {
	public class AutofacConfig {
		public static void Register() {
			var builder = new ContainerBuilder();

			builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
			builder.RegisterModule(new AutofacRegistryModule());
			builder.RegisterFilterProvider();
            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);

			builder.Register(
					daf => new DictionaryAdapterFactory().GetAdapter<IApplicationSettings>(ConfigurationManager.AppSettings))
						 .SingleInstance();
			builder.Register(
					daf => new DictionaryAdapterFactory().GetAdapter<IConnectionStrings>(GetConnectionStrings()))
						 .SingleInstance();

			IContainer container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}

		private static NameValueCollection GetConnectionStrings() {
			var connections = new NameValueCollection();

			foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings) {
				connections.Add(connection.Name, connection.ConnectionString);
			}

			return connections;
		}
	}
}