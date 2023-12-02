using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Emby
{
    public interface IMediaBrowserService
    {
        void Notify(MediaBrowserSettings settings, string title, string message);
        void Update(MediaBrowserSettings settings, Series series, string updateType);
        ValidationFailure Test(MediaBrowserSettings settings);
    }

    public class MediaBrowserService : IMediaBrowserService
    {
        private readonly MediaBrowserProxy _proxy;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public MediaBrowserService(MediaBrowserProxy proxy, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void Update(MediaBrowserSettings settings, Series series, string updateType)
        {
            HashSet<string> paths;

            paths = _proxy.GetPaths(settings, series);

            var mappedPath = new OsPath(series.Path);

            if (settings.MapTo.IsNotNullOrWhiteSpace())
            {
                mappedPath = new OsPath(settings.MapTo) + (mappedPath - new OsPath(settings.MapFrom));
            }

            paths.Add(mappedPath.ToString());

            foreach (var path in paths)
            {
                _proxy.Update(settings, path, updateType);
            }
        }

        public ValidationFailure Test(MediaBrowserSettings settings)
        {
            try
            {
                _logger.Debug("Testing connection to MediaBrowser: {0}", settings.Address);

                Notify(settings, "Test from Sonarr", "Success! MediaBrowser has been successfully configured!");
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("NotificationsValidationInvalidApiKey"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
