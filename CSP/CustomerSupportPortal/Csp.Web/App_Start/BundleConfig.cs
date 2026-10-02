using System.Web.Optimization;

namespace Csp.Web.App_Start {
	public class BundleConfig {
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles) {

			#region Styles

            // The Kendo CSS bundle
            bundles.Add(new StyleBundle("~/Content/kendo/2016.1.412/kendocss").Include(
                    "~/Content/kendo/2016.1.412/kendo.common.min.css",
                    "~/Content/kendo/2016.1.412/kendo.material.min.css"));

			bundles.Add(new StyleBundle("~/Content/SiteIE8").Include(
					"~/Content/SiteIE8.css"));

			#endregion


			#region JavaScript
			// jQuery bundle

			bundles.Add(new ScriptBundle("~/bundles/alljquery").Include(
				"~/Scripts/jquery-{version}.js",
				"~/Scripts/jquery-migrate-1.2.1.min.js",
				"~/Scripts/jquery.unobtrusive-ajax.min.js",
				"~/Scripts/jquery.validate.min.js",
				"~/Scripts/jquery.validate.unobtrusive.min.js",
				"~/Scripts/jquery.blockUI.js",
				"~/Scripts/underscore-min.js",
				"~/Scripts/moment.min.js",
				"~/Scripts/moment-timezone.min.js",
				"~/Scripts/moment-timezone-data.js",
				"~/Scripts/jquery.filedownload.js"
				));

			// The Kendo JavaScript bundle
            bundles.Add(new ScriptBundle("~/bundles/kendo/2016.1.412/kendojs").Include(
                            "~/Scripts/kendo/2016.1.412/kendo.all.min.js",
                            "~/Scripts/kendo/2016.1.412/kendo.aspnetmvc.min.js"));

			// Common JavaScript bundle
			bundles.Add(new ScriptBundle("~/bundles/custom").Include(
									"~/Scripts/csp.js",
									"~/Scripts/Custom/csp.common.js"));

            bundles.Add(new ScriptBundle("~/bundles/bulkreg").Include(
                                    "~/Scripts/Custom/csp.bulkregistration.js",
                                    "~/Scripts/Custom/PrepBulkReg/DragAndDrop.js",
                                    "~/Scripts/Custom/PrepBulkReg/DeviceDefaults.js",
                                    "~/Scripts/Custom/PrepBulkReg/GridSchemas.js",
                                    "~/Scripts/Custom/PrepBulkReg/Grids.js"));

            /*
             * 
<script src="~/Scripts/Custom/PrepBulkReg/DragAndDrop.js"></script>
<script src="~/Scripts/Custom/PrepBulkReg/DeviceDefaults.js"></script>
<script src="~/Scripts/Custom/PrepBulkReg/Grids.js"></script>
             */
            #endregion

            #region Reset Ignore List
            // Clear all items from the ignore list to allow minified CSS and JavaScript files in debug mode
			bundles.IgnoreList.Clear();

			// Add back the default ignore list rules sans the ones which affect minified files and debug mode
			bundles.IgnoreList.Ignore("*.intellisense.js");
			bundles.IgnoreList.Ignore("*-vsdoc.js");
			bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
			#endregion
		}
	}
}