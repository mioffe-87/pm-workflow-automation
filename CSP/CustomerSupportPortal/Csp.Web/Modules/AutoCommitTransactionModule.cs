using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Services.Contracts;
using PAN.Common.Infrastructure.Data.Persistence;

namespace Csp.Web.Modules {
	public class AutoCommitTransactionModule : IHttpModule {
		public void Init(HttpApplication context) {
			context.PreRequestHandlerExecute += OnPreRequestHandlerExecute;
			context.PostRequestHandlerExecute += OnPostRequestHandlerExecute;
		}

		public void Dispose() {

		}

		private void OnPostRequestHandlerExecute(object sender, EventArgs e) {
			if (!RequiresTransaction())
				return;

			var unitOfWork = DependencyResolver.Current.GetService<INHibernateUnitOfWork>();

			if (HttpContext.Current.Request.RequestType == "GET")
				unitOfWork.CloseSession();
			else
				unitOfWork.Commit(false);
		}


		private void OnPreRequestHandlerExecute(object sender, EventArgs e) {
			if (!RequiresTransaction())
				return;

			var unitOfWork = DependencyResolver.Current.GetService<INHibernateUnitOfWork>();

            if (HttpContext.Current.Request.RequestType == "GET") {
                unitOfWork.BeginNewSession();
            }
            else {
                unitOfWork.BeginNewTransaction();
            }
        }

		private bool RequiresTransaction() {
			var applicationSettings = DependencyResolver.Current.GetService<IApplicationSettings>();
			string path = HttpContext.Current.Request.Path;

            var exceptions = applicationSettings.StaticResources.Split(new[] { ',' });

            return !exceptions.Any(path.StartsWith);
		}
	}
}