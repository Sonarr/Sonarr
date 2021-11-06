using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiController]
    public class InitializeJsController : Controller
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private static string _apiKey;
        private static string _urlBase;
        private string _generatedContent;

        public InitializeJsController(IConfigFileProvider configFileProvider,
                                      IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            _apiKey = configFileProvider.ApiKey;
            _urlBase = configFileProvider.UrlBase;
        }

        [HttpGet("/initialize.js")]
        public IActionResult Index()
        {
            return Content(GetContent(), "application/javascript");
        }

        private string GetContent()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var builder = new StringBuilder();
            builder.AppendLine("window.Sonarr = {");
            builder.AppendLine($"  apiRoot: '{_urlBase}/api/v3',");
            builder.AppendLine($"  apiKey: '{_apiKey}',");
            builder.AppendLine($"  release: '{BuildInfo.Release}',");
            builder.AppendLine($"  version: '{BuildInfo.Version.ToString()}',");
            builder.AppendLine($"  branch: '{_configFileProvider.Branch.ToLower()}',");
            builder.AppendLine($"  analytics: {_analyticsService.IsEnabled.ToString().ToLowerInvariant()},");
            builder.AppendLine($"  userHash: '{HashUtil.AnonymousToken()}',");
            builder.AppendLine($"  urlBase: '{_urlBase}',");
            builder.AppendLine($"  isProduction: {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            builder.AppendLine("};");

            _generatedContent = builder.ToString();

            return _generatedContent;
        }
    }
}
