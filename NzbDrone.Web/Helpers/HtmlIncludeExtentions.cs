using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NzbDrone.Common;

namespace NzbDrone.Web.Helpers
{
    public static class HtmlIncludeExtentions
    {
        private static string _versionString;
        private static bool _isProduction;

        static HtmlIncludeExtentions()
        {
            _versionString = new EnviromentProvider().Version.ToString().Replace('.', '_');
            _isProduction = EnviromentProvider.IsProduction;
        }

        public static MvcHtmlString IncludeScript(this HtmlHelper helper, string filename)
        {
            var relativePath = "/Scripts/" + filename;
            VerifyFile(helper, relativePath);
            return MvcHtmlString.Create(String.Format("<script type='text/javascript' src='{0}?{1}'></script>", relativePath, _versionString));
        }
        
        public static MvcHtmlString IncludeCss(this HtmlHelper helper, string filename)
        {
            var relativePath = "/Content/" + filename;
            VerifyFile(helper, relativePath);
            return MvcHtmlString.Create(String.Format("<link type='text/css' rel='stylesheet' href='{0}?{1}'/>", relativePath, _versionString));
        }

        private static void VerifyFile(HtmlHelper helper, string filename)
        {
            if (!_isProduction)
            {
                var path = helper.ViewContext.RequestContext.HttpContext.Server.MapPath(filename);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("static file not found " + path, path);
                }
            }
        }
    }
}