using System.IO;
using System.Text;
using Nancy;
using Nancy.Responses;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.Frontend
{
    public class InitializeJsModule : NancyModule
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private static string _apiKey;
        private static string _urlBase;
        private string _generatedContent;

        public InitializeJsModule(IConfigFileProvider configFileProvider,
                                  IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            _apiKey = configFileProvider.ApiKey;
            _urlBase = configFileProvider.UrlBase;

            Get["/initialize.js"] = x => Index();
        }

        private Response Index()
        {
            // TODO: Move away from window.Sonarr and prefetch the information returned here when starting the UI
            return new StreamResponse(GetContentStream, "application/javascript");
        }

        private Stream GetContentStream()
        {
            var text = GetContent();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            return stream;
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
            builder.AppendLine($"  urlBase: '{_urlBase}',");
            builder.AppendLine($"  isProduction: {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            builder.AppendLine("};");

            _generatedContent = builder.ToString();

            return _generatedContent;
        }
    }
}
