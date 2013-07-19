using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;

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
        private readonly Logger _logger;

        public PlexService(IHttpProvider httpProvider, Logger logger)
        {
            _httpProvider = httpProvider;
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
                _logger.Trace("Sending Update Request to Plex Server");
                var sections = GetSectionKeys(settings);
                sections.ForEach(s => UpdateSection(settings, s));
            }

            catch(Exception ex)
            {
                _logger.WarnException("Failed to Update Plex host: " + settings.Host, ex);
                throw;
            }
        }

        public List<int> GetSectionKeys(PlexServerSettings settings)
        {
            _logger.Trace("Getting sections from Plex host: {0}", settings.Host);
            var url = String.Format("http://{0}:{1}/library/sections", settings.Host, settings.Port);
            var xmlStream = _httpProvider.DownloadStream(url, null);
            var xDoc = XDocument.Load(xmlStream);
            var mediaContainer = xDoc.Descendants("MediaContainer").FirstOrDefault();
            var directories = mediaContainer.Descendants("Directory").Where(x => x.Attribute("type").Value == "show");

            return directories.Select(d => Int32.Parse(d.Attribute("key").Value)).ToList();
        }

        public void UpdateSection(PlexServerSettings settings, int key)
        {
            _logger.Trace("Updating Plex host: {0}, Section: {1}", settings.Host, key);
            var url = String.Format("http://{0}:{1}/library/sections/{2}/refresh", settings.Host, settings.Port, key);
            _httpProvider.DownloadString(url);
        }

        public string SendCommand(string host, int port, string command, string username, string password)
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
            _logger.Trace("Sending Test Notifcation to Plex Client: {0}", message.Host);
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
            if (!GetSectionKeys(new PlexServerSettings {Host = message.Host, Port = message.Port}).Any())
            {
                throw new Exception("Unable to connect to Plex Server");
            }
        }
    }
}
