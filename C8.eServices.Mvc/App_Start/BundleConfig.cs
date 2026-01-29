using System.Web.Optimization;

namespace C8.eServices.Mvc
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/HoldOn.min.js",
                        "~/Scripts/multiple-emails.js",
                        "~/Scripts/readonly.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-table.js",
                      "~/Scripts/bootstrap-datepicker.js",
                      "~/Scripts/bootstrap-datepicker.min.js",
                      "~/Scripts/bootbox.min.js",
                      "~/Scripts/bootstrap-multiselect.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate").Include(
                      "~/Scripts/jquery.validate.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-table.css",
                      "~/Content/bootstrap-datepicker.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Content/Custom.css",
                      "~/Content/HoldOn.min.css",
                      "~/Content/multiple-emails.css",
                      "~/Content/jquery.datetimepicker.css",
                      "~/Content/datatables/css/dataTables.bootstrap.min.css",
                      "~/Content/progress-steps.css",
                      "~/Content/bootstrap-multiselect.css"));
        }
    }
}