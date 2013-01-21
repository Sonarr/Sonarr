using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace NzbDrone.Web
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public const string BASE_STYLE = "BASE_STYLE";
        public const string BASE_SCRIPT = "BASE_SCRIPT";
        public const string BACKBONE = "BACKBONE";
        public const string FONTS = "FONTS";
        public const string VALIDATION_SCRIPTS = "VALIDATION_SCRIPTS";
        public const string FILEBROWSER_SCRIPT = "FILEBROWSER_SCRIPT";
        public const string FILEBROWSER_STYLE = "FILEBROWSER_STYLE";

        public void Configure(BundleCollection bundles)
        {
            bundles.AddUrlWithAlias<StylesheetBundle>("//fonts.googleapis.com/css?family=Open+Sans:300italic,400italic,600italic,400,600,300'", FONTS);


            bundles.Add<StylesheetBundle>(BASE_STYLE, new[]{
                "content2\\Bootstrap\\bootstrap.less",
                //"content2\\Bootstrap\\responsive.less",
                //"content2\\bootstrap-metro.css",
                "content2\\base.css",
                "content2\\menu.css",
                "content2\\bootstrap-overrides.css"},
                bundle => bundle.AddReference("/" + FONTS));

            bundles.Add<ScriptBundle>(BASE_SCRIPT, new[]{
                "scripts2\\jquery-1.8.2.js",
                "scripts2\\jquery.livequery.js",
                "scripts2\\bootstrap-mvc.js",
                "scripts2\\bootstrap.js",
                "scripts2\\metro\\jquery.metro.js"});

            bundles.Add<ScriptBundle>(BACKBONE, new[]{
                    "JsLibraries\\underscore.js",
                    "JsLibraries\\backbone.js",
                    "JsLibraries\\backbone.marionette.js",
            
                    "app.js",
                    "AddSeries\\Views\\addSeriesView.js"},
                    bundle => bundle.AddReference("/" + BASE_SCRIPT));


            bundles.AddPerSubDirectory<StylesheetBundle>("AddSeries", bundle => bundle.AddReference("/" + BASE_STYLE));


            bundles.Add<StylesheetBundle>(FILEBROWSER_STYLE, new[]{
                "content2\\stats.css",
                "content2\\file-browser.css",
                "content2\\FancyBox\\jquery.fancybox.css"},
                bundle => bundle.AddReference("/" + BASE_STYLE));

            bundles.Add<ScriptBundle>(FILEBROWSER_SCRIPT, new[]{
                                      "content2\\FancyBox\\jquery.fancybox.js"},
                                       bundle => bundle.AddReference("/" + BASE_SCRIPT));

            bundles.Add<ScriptBundle>(VALIDATION_SCRIPTS, new[]{
                                      "scripts2\\jquery.livequery.js",
                                      "scripts2\\jquery.validate.js",
                                      "scripts2\\jquery.validate.unobtrusive.js",
                                      "scripts2\\bootstrap-mvc.js"},
                                      bundle => bundle.AddReference("/" + BASE_SCRIPT));



        }
    }
}