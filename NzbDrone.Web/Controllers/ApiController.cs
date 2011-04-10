using System.Web.Mvc;
using NLog;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Web.Controllers
{
    public class ApiController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly PostProcessingProvider _postProcessingProvider;

        public ApiController(PostProcessingProvider postProcessingProvider, ConfigProvider configProvider)
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