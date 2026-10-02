using Autofac;
using Csp.Infrastructure.Data.Persistence.Configuration;
using PAN.Common.Infrastructure.Data.Persistence.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Module = Autofac.Module;

namespace Csp.Web.Modules {
	public class AutofacRegistryModule : Module {
		protected override void Load(ContainerBuilder builder) {
			RegisterAssemblyTypes(builder);
		}

		private static void RegisterAssemblyTypes(ContainerBuilder builder) {
			Assembly currentAssembly = Assembly.GetExecutingAssembly();
			string currentAssemblyPath = currentAssembly.CodeBase;
			string assemplyDirectoryPath = Path.GetDirectoryName(currentAssemblyPath);

			if (assemplyDirectoryPath != null) {
				string binFolderpath = assemplyDirectoryPath.Remove(0, 6);

				string[] files = Directory.GetFiles(binFolderpath, "*.dll")?.Where(f => !f.Contains("grpc_csharp_ext.x")).ToArray();

				builder.RegisterAssemblyTypes(files.Select(Assembly.LoadFrom).Where(asm => asm.FullName.Contains("PAN.") || asm.FullName.Contains("Csp.")).ToArray()).Where(IsInjectableType).Except
					 <SessionSourceConfiguration>(cfg => cfg.As<ISessionSourceConfiguration>().SingleInstance())
					 .AsImplementedInterfaces().InstancePerLifetimeScope();
			}
		}

		private static bool IsInjectableType(Type t) {
			if (t != null) {
				return t.FullName.StartsWith("PAN.Common.Core.Persistence") ||
							 t.FullName.StartsWith("PAN.Common.Infrastructure.Data.Persistence") ||
							 t.FullName.StartsWith("Csp.Api.Services") ||
							 t.FullName.StartsWith("Csp.Api.Logic") ||
							 t.FullName.StartsWith("Csp.Api.DataAccess") ||
							 t.FullName.StartsWith("Csp.Api.HelperClasses") ||
							 t.FullName.StartsWith("Csp.Infrastructure.Data.Persistence") ||
							 t.FullName.StartsWith("Csp.Infrastructure.Data.WebServices") ||
							 t.FullName.StartsWith("Csp.Web.HelperClasses");
			}

			return false;
		}
	}
}