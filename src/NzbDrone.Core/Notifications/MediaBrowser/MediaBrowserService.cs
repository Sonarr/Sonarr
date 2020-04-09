using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
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
        private readonly Logger _logger;

        public MediaBrowserService(MediaBrowserProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public void Notify(MediaBrowserSettings settings, string title, string message)
        {
            _proxy.Notify(settings, title, message);
        }

        public void Update(MediaBrowserSettings settings, Series series, string updateType)
        {
            _proxy.Update(settings, series.Path, updateType);
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
                    return new ValidationFailure("ApiKey", "API Key is incorrect");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message: " + ex.Message);
            }

            return null;
        }
    }
}
