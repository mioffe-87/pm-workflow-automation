using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Security.Principal;
using System.Web.Routing;
using System.Web.Security;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.App_Start;
using Csp.Web.HelperClasses;
using Csp.Api.Logic;
using Csp.Api.Security;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators;
using FluentValidation.Mvc;
using Kendo.Mvc.Infrastructure;
using System.ComponentModel.Design;
using Elmah;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;

namespace Csp.Web {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class WebApiApplication : System.Web.HttpApplication {


		protected void Application_Start() {
		    try
		    {
		        ConfigValues.InitConfig();
		        //Set the elmah connection string in runtime.
		        var elmahConnectionString = ConfigValues.GetElmahConnectionString();
		        var service = ServiceCenter.Current;
		        ServiceCenter.Current = context =>
		        {
		            var connectionString = elmahConnectionString;
		            var container = new ServiceContainer(service(context));
		            var log = new MySqlErrorLog(connectionString) { ApplicationName = "CSP" };
		            container.AddService(typeof(ErrorLog), log);
		            return container;
		        };
		    }
		    catch (Exception ex)
		    {
		        //send the exception email and method
		        //EmailHelper.SendExceptionEmail(ex, null);
		    }

            DataAnnotationsModelValidatorProvider.RegisterAdapter(
        typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(AssertThatAttribute), typeof(AssertThatValidator));

            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["ExcludeHelpPage"]))
            {
                AreaRegistration.RegisterAllAreas();
            }
            
            GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			AutofacConfig.Register();
			FluentValidationModelValidatorProvider.Configure();

			// Clears all previously registered view engines.
			ViewEngines.Engines.Clear();

			// Registers the Razor view engine.
			ViewEngines.Engines.Add(new RazorViewEngine());

			AutoMapperConfig.Configure();

			ModelBinders.Binders.Add(typeof(string), new TrimModelBinder());

            // Replace JavaScript serializer for Kendo controls with NewtonSoft
            DI.Current.Register<IJavaScriptInitializer>(() => new KendoJavaScriptInitializer());

//#if DEBUG
//			HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
//#endif

            // Comments By LK On 07/01/2019
            // Disable bundle optimization for Phase 1
            //#if !DEBUG
            //    BundleTable.EnableOptimizations = true;
            //#endif
        }

		protected void Application_BeginRequest() {

            if (!Request.IsLocal) {
                var urlValidator = new UrlValidator(Request.Url);
                if (!urlValidator.IsFullyQualifiedHost()) {
                    Response.Redirect(urlValidator.GetFullyQualifiedUrl());
                }
            }
		}

		private void Application_Error(object sender, EventArgs e) {

			var exception = Server.GetLastError().GetBaseException();
			var httpException = exception as HttpException;
			var is404 = (httpException != null && httpException.GetHttpCode() == 404);

			if (!is404) {

				var configValues = new ConfigValues();
				var currentStateLogging = Convert.ToBoolean(configValues.GetAppSetting(AppSettings.CurrentStateLogging));
				if (currentStateLogging) {
					try {

						Elmah.ErrorSignal.FromCurrentContext().Raise(CurrentStateException.GetCurrentStateException());

					} catch (Exception ex) {

						Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
					}
				}

				if (Request.IsLocal) return;

				EmailHelper.SendExceptionEmail(exception);
			}

			// Code that runs when an unhandled error occurs
			var errorAction = Request.IsAjaxRequest() ? "PartialError" : "Error";
			var url =
				RouteTable.Routes.GetVirtualPath(HttpContext.Current.Request.RequestContext,
																				 new RouteValueDictionary(new { Controller = "Error", action = errorAction }))
									.VirtualPath;

			Response.Clear();
			Server.ClearError();

			HttpContext.Current.Response.Redirect(url, true);
		}
	}
}