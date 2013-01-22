using System.Linq;
using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace NzbDrone.Web.Backbone.NzbDrone
{
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public const string BASE_STYLE = "BASE_STYLE";
        public const string BACKBONE = "BACKBONE";
        public const string NZBDRONE = "NZBDRONE";
        internal const string FONTS = "FONTS";

        private const string APP_PATH = "_backboneApp";

        public void Configure(BundleCollection bundles)
        {
            bundles.AddUrlWithAlias<StylesheetBundle>("//fonts.googleapis.com/css?family=Open+Sans:300italic,400italic,600italic,400,600,300'", FONTS);

            bundles.AddPerIndividualFile<ScriptBundle>(APP_PATH);

            bundles.Add<StylesheetBundle>(BASE_STYLE, new[]{
                APP_PATH + "\\Content\\Bootstrap\\bootstrap.less",
                APP_PATH + "\\Content\\base.css",
                APP_PATH + "\\Content\\menu.css",
                APP_PATH + "\\AddSeries\\addSeries.css"
            },
                bundle => bundle.AddReference("/" + FONTS));



            bundles.Add<ScriptBundle>(NZBDRONE, new[]{
                      APP_PATH + "\\AddSeries\\AddSeriesLayout.js",

            
            });

/*            bundles.Add<ScriptBundle>(NZBDRONE, new[]{
     
                 },
               bundle => bundle.AddReference("/" + BACKBONE));*/


        }
    }
}