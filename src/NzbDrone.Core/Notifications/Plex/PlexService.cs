using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Notifications.Plex
{
    public interface IPlexService
    {
        void Notify(PlexClientSettings settings, string header, string message);
        void UpdateLibrary(PlexServerSettings settings);
    }

    public class PlexService : IPlexService, IExecute<TestPlexClientCommand>, IExecute<TestPlexServerCommand>
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IPlexServerProxy _plexServerProxy;
        private readonly Logger _logger;

        public PlexService(IHttpProvider httpProvider, IPlexServerProxy plexServerProxy, Logger logger)
        {
            _httpProvider = httpProvider;
            _plexServerProxy = plexServerProxy;
            _logger = logger;
        }

        public void Notify(PlexClientSettings settings, string header, string message)
        {
            try
            {
                var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", header, message);
                SendCommand(settings.Host, settings.Port, command, settings.Username, settings.Password);
            }
            catch(Exception ex)
            {
                _logger.WarnException("Failed to send notification to Plex Client: " + settings.Host, ex);
            }
        }

        public void UpdateLibrary(PlexServerSettings settings)
        {
            try
            {
                _logger.Debug("Sending Update Request to Plex Server");
                var sections = GetSectionKeys(settings);
                sections.ForEach(s => UpdateSection(s, settings));
            }

            catch(Exception ex)
            {
                _logger.WarnException("Failed to Update Plex host: " + settings.Host, ex);
                throw;
            }
        }

        private List<int> GetSectionKeys(PlexServerSettings settings)
        {
            _logger.Debug("Getting sections from Plex host: {0}", settings.Host);

            return _plexServerProxy.GetTvSections(settings).Select(s => s.Key).ToList();
        }

        private void UpdateSection(int key, PlexServerSettings settings)
        {
            _logger.Debug("Updating Plex host: {0}, Section: {1}", settings.Host, key);

            _plexServerProxy.Update(key, settings);
        }

        private string SendCommand(string host, int port, string command, string username, string password)
        {
            var url = String.Format("http://{0}:{1}/xbmcCmds/xbmcHttp?command={2}", host, port, command);

            if (!String.IsNullOrEmpty(username))
            {
                return _httpProvider.DownloadString(url, username, password);
            }

            return _httpProvider.DownloadString(url);
        }

        public void Execute(TestPlexClientCommand message)
        {
            _logger.Debug("Sending Test Notifcation to Plex Client: {0}", message.Host);
            var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", "Test Notification", "Success! Notifications are setup correctly");
            var result = SendCommand(message.Host, message.Port, command, message.Username, message.Password);

            if (String.IsNullOrWhiteSpace(result) ||
                result.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                throw new Exception("Unable to connect to Plex Client");
            }
        }

        public void Execute(TestPlexServerCommand message)
        {
            if (!GetSectionKeys(new PlexServerSettings
                                {
                                    Host = message.Host,
                                    Port = message.Port,
                                    Username = message.Username,
                                    Password =  message.Password
                                }).Any())
            {
                throw new Exception("Unable to connect to Plex Server");
            }
        }
    }
}
