using System.Web.Optimization;
using System.Web.UI.WebControls;

namespace RegistrarPfi
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                            "~/Scripts/validation.js",
                            "~/Scripts/jquery-maskedinput.js",
                            "~/Scripts/bootbox.js",
                            "~/Scripts/selection.js",
                            "~/Scripts/SiteScripts.js",
                            "~/Scripts/session.js",
                            "~/Scripts/SiteNotificationsHandler.js",
                            "~/Scripts/autoRefreshPanel.js",
                            "~/Scripts/image-control.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/_layout.css",
                        "~/Content/Accounts.css",
                        "~/Content/popup.css",
                        "~/Content/Selection.css",
                        "~/Content/site.css",
                        "~/Content/menu.css", 
                        "~/Content/media.css",
                        "~/Content/Icons.css",
                        "~/Content/image-control.css",
                        "~/Content/jqui-custom-datepicker.css",
                        "~/Content/Students.css",
                        "~/Content/Teachers.css",
                        "~/Content/Courses.css"));
        }
    }
}
