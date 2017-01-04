using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Analytics
{
    public interface IAnalyticsService
    {
        bool IsEnabled { get; }
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly IConfigFileProvider _configFileProvider;

        public AnalyticsService(IConfigFileProvider configFileProvider)
        {
            _configFileProvider = configFileProvider;
        }

        public bool IsEnabled => _configFileProvider.AnalyticsEnabled && RuntimeInfo.IsProduction;
    }
}