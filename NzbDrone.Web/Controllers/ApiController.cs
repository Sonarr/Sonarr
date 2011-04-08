using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using NLog;
using NzbDrone.Core;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly PostProcessingProvider _postProcessingProvider;
        private readonly IConfigProvider _configProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ApiController(PostProcessingProvider postProcessingProvider, IConfigProvider configProvider)
        {
            _postProcessingProvider = postProcessingProvider;
            _configProvider = configProvider;
        }

        public ActionResult ProcessEpisode(string apiKey, string dir, string nzbName, string category)
        {
            if (apiKey != _configProvider.ApiKey)
            {
                Logger.Warn("API Key from Post Processing Script is Invalid");
                return Content("Invalid API Key");
            }

            if (_configProvider.SabTvCategory == category)
            {
                _postProcessingProvider.ProcessEpisode(dir, nzbName);
                return Content("ok");
            }

            return Content("Category doesn't match what was configured for SAB TV Category...");
        }
    }
}
