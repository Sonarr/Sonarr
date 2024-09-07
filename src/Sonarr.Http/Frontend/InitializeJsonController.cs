using System;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InitializeJsonController : Controller
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUserService _userService;

        private static string _apiKey;
        private static string _urlBase;
        private string _generatedContent;

        public InitializeJsonController(IConfigFileProvider configFileProvider,
                                      IAnalyticsService analyticsService,
                                      IHttpContextAccessor httpContextAccessor,
                                      IUserService userService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;
            _urlBase = configFileProvider.UrlBase;

            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        [HttpGet("/initialize.json")]
        public IActionResult Index()
        {
            _apiKey = GetCurrentUser()?.ApiKey;
            return Content(GetContent(), "application/json");
        }

        private string GetContent()
        {
            if (RuntimeInfo.IsProduction && _generatedContent != null)
            {
                return _generatedContent;
            }

            var currentUser = GetCurrentUser();
            var role = currentUser != null ? currentUser.Role.ToString() : "Admin";

            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine($"  \"apiRoot\": \"{_urlBase}/api/v3\",");
            builder.AppendLine($"  \"apiKey\": \"{_apiKey}\",");
            builder.AppendLine($"  \"role\": \"{role}\",");
            builder.AppendLine($"  \"release\": \"{BuildInfo.Release}\",");
            builder.AppendLine($"  \"version\": \"{BuildInfo.Version.ToString()}\",");
            builder.AppendLine($"  \"instanceName\": \"{_configFileProvider.InstanceName.ToString()}\",");
            builder.AppendLine($"  \"theme\": \"{_configFileProvider.Theme.ToString()}\",");
            builder.AppendLine($"  \"branch\": \"{_configFileProvider.Branch.ToLower()}\",");
            builder.AppendLine($"  \"analytics\": {_analyticsService.IsEnabled.ToString().ToLowerInvariant()},");
            builder.AppendLine($"  \"userHash\": \"{HashUtil.AnonymousToken()}\",");
            builder.AppendLine($"  \"urlBase\": \"{_urlBase}\",");
            builder.AppendLine($"  \"isProduction\": {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            builder.AppendLine("}");

            _generatedContent = builder.ToString();

            return _generatedContent;
        }

        private User GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext.User;

            var usernameClaim = user.FindFirst("user");
            var identifierClaim = user.FindFirst("identifier");

            if (usernameClaim == null || identifierClaim == null)
            {
                return null;
            }

            var identifier = Guid.Parse(identifierClaim.Value);
            return _userService.FindUser(identifier);
        }
    }
}
