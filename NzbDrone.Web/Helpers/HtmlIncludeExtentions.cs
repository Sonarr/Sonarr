using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;

namespace NzbDrone.Web.Helpers
{
    public static class HtmlIncludeExtentions
    {
        private static readonly string versionString;
        private static readonly bool isProduction;

        static HtmlIncludeExtentions()
        {
            versionString = new EnvironmentProvider().Version.ToString().Replace('.', '_');
            isProduction = EnvironmentProvider.IsProduction;
        }

        public static MvcHtmlString IncludeScript(this HtmlHelper helper, string filename)
        {
            var relativePath = "/Scripts/" + filename;
            VerifyFile(helper, relativePath);
            return MvcHtmlString.Create(String.Format("<script type='text/javascript' src='{0}?{1}'></script>", relativePath, versionString));
        }

        public static MvcHtmlString IncludeCss(this HtmlHelper helper, string filename)
        {
            var relativePath = "/Content/" + filename;
            VerifyFile(helper, relativePath);
            return MvcHtmlString.Create(String.Format("<link type='text/css' rel='stylesheet' href='{0}?{1}'/>", relativePath, versionString));
        }

        private static void VerifyFile(HtmlHelper helper, string filename)
        {
            if (isProduction)
                return;

            var path = helper.ViewContext.RequestContext.HttpContext.Server.MapPath(filename);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Included static resource was not found.", path);
            }

        }
    }
}