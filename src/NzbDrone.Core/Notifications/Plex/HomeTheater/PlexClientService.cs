﻿using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Notifications.Plex.HomeTheater
{
    public interface IPlexClientService
    {
        void Notify(PlexClientSettings settings, string header, string message);
        ValidationFailure Test(PlexClientSettings settings);
    }

    public class PlexClientService : IPlexClientService
    {
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public PlexClientService(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public void Notify(PlexClientSettings settings, string header, string message)
        {
            try
            {
                var command = string.Format("ExecBuiltIn(Notification({0}, {1}))", header, message);
                SendCommand(settings.Address, command, settings.Username, settings.Password);
            }
            catch(Exception ex)
            {
                _logger.Warn(ex, "Failed to send notification to Plex Client: " + settings.Host);
            }
        }

        private string SendCommand(string url, string command, string username, string password)
        {
            var full_url = $"{url}/xbmcCmds/xbmcHttp?command={command}";

            if (!string.IsNullOrEmpty(username))
            {
                return _httpProvider.DownloadString(full_url, username, password);
            }

            return _httpProvider.DownloadString(full_url);
        }

        public ValidationFailure Test(PlexClientSettings settings)
        {
            try
            {
                _logger.Debug("Sending Test Notifcation to Plex Client: {0}", settings.Host);
                var command = string.Format("ExecBuiltIn(Notification({0}, {1}))", "Test Notification", "Success! Notifications are setup correctly");
                var result = SendCommand(settings.Address, command, settings.Username, settings.Password);

                if (string.IsNullOrWhiteSpace(result) ||
                    result.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    throw new Exception("Unable to connect to Plex Client");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
