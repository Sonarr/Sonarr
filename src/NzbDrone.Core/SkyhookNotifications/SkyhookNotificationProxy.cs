using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.SkyhookNotifications
{
    public interface ISkyhookNotificationProxy
    {
        List<SkyhookNotification> GetNotifications();
    }

    public class SkyhookNotificationProxy : ISkyhookNotificationProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly Logger _logger;

        public SkyhookNotificationProxy(IHttpClient httpClient, ISonarrCloudRequestBuilder requestBuilder, Logger logger)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder.Services;
            _logger = logger;
        }

        public List<SkyhookNotification> GetNotifications()
        {
            return new List<SkyhookNotification>
            {
                new SkyhookNotification
                {
                    Type = SkyhookNotificationType.UrlBlacklist,
                    Title = "Nyaa Indexer shut down",
                    Message = "Official news is that Nyaa shut down and the domain will expire in a few months, therefore the indexer is forcibly disabled in Sonarr. If a substitute comes available you can update the url in the indexer settings.",
                    RegexMatch = @"://www\.nyaa\.se(/|$)"
                }
            };
            /*
            var notificationsRequest = _requestBuilder.Create()
                                                      .Resource("/notifications")
                                                      .AddQueryParam("version", BuildInfo.Version)
                                                      .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                                                      .Build();

            try
            {
                var response = _httpClient.Get<List<SkyhookNotification>>(notificationsRequest);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get information update from {0}", notificationsRequest.Url.Host);
                return new List<SkyhookNotification>();
            }*/
        }
    }
}
