using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;

namespace NzbDrone.Api.FrontendModule
{
    public class BootstrapModule : NancyModule
    {
        private readonly ICompileLess _lessCompiler;

        public BootstrapModule(ICompileLess lessCompiler)
        {
            _lessCompiler = lessCompiler;
            Get[@"static/content/bootstrap/bootstrap.less"] = x => OnGet();
        }

        private Response OnGet()
        {
/*            var urlParts = Request.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (urlParts.Length < 2)
            {
                return new NotFoundResponse();
            }

            urlParts[0] = "NzbDrone.Backbone";

            var filePath = Path.Combine(urlParts);


            if (!File.Exists(filePath))
            {
                return new NotFoundResponse();
            }*/

            var css = _lessCompiler.Compile(Path.Combine("NzbDrone.Backbone","Content","Bootstrap","bootstrap.less"));

            return new TextResponse(HttpStatusCode.OK, css) { ContentType = "text/css" };
        }
    }
}