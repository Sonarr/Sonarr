using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using NzbDrone.Core;
using NzbDrone.Core.Providers;

namespace NzbDrone.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly IPostProcessingProvider _postProcessingProvider;

        public ApiController(IPostProcessingProvider postProcessingProvider)
        {
            _postProcessingProvider = postProcessingProvider;
        }

        public ActionResult ProcessEpisode(string dir, string nzbName)
        {
            _postProcessingProvider.ProcessEpisode(dir, nzbName);
            return Content("ok");
        }
    }
}
