using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.ExternalNotification
{
    public class PlexProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly IConfigService _configService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PlexProvider(HttpProvider httpProvider, IConfigService configService)
        {
            _httpProvider = httpProvider;
            _configService = configService;
        }

        public PlexProvider()
        {
            
        }

        public virtual void Notify(string header, string message)
        {
            //Foreach plex client send a notification
            foreach(var host in _configService.PlexClientHosts.Split(','))
            {
                try
                {
                    var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", header, message);
                    SendCommand(host.Trim(), command, _configService.PlexUsername, _configService.PlexPassword);
                }
                catch(Exception ex)
                {
                    logger.WarnException("Failed to send notification to Plex Client: " + host.Trim(), ex);
                }
            }
        }

        public virtual void UpdateLibrary()
        {
            var host = _configService.PlexServerHost;

            try
            {
                logger.Trace("Sending Update Request to Plex Server");
                var sections = GetSectionKeys(host);
                sections.ForEach(s => UpdateSection(host, s));
            }

            catch(Exception ex)
            {
                logger.WarnException("Failed to Update Plex host: " + host, ex);
                throw;
            }
        }

        public List<int> GetSectionKeys(string host)
        {
            logger.Trace("Getting sections from Plex host: {0}", host);
            var url = String.Format("http://{0}/library/sections", host);
            var xmlStream = _httpProvider.DownloadStream(url, null);
            var xDoc = XDocument.Load(xmlStream);
            var mediaContainer = xDoc.Descendants("MediaContainer").FirstOrDefault();
            var directories = mediaContainer.Descendants("Directory").Where(x => x.Attribute("type").Value == "show");

            return directories.Select(d => Int32.Parse(d.Attribute("key").Value)).ToList();
        }

        public void UpdateSection(string host, int key)
        {
            logger.Trace("Updating Plex host: {0}, Section: {1}", host, key);
            var url = String.Format("http://{0}/library/sections/{1}/refresh", host, key);
            _httpProvider.DownloadString(url);
        }

        public virtual string SendCommand(string host, string command, string username, string password)
        {
            var url = String.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", host, command);

            if (!String.IsNullOrEmpty(username))
            {
                return _httpProvider.DownloadString(url, username, password);
            }

            return _httpProvider.DownloadString(url);
        }

        public virtual void TestNotification(string hosts, string username, string password)
        {
            foreach (var host in hosts.Split(','))
            {
                logger.Trace("Sending Test Notifcation to XBMC Host: {0}", host);
                var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", "Test Notification", "Success! Notifications are setup correctly");
                SendCommand(host.Trim(), command, _configService.PlexUsername, _configService.PlexPassword);
            }
        }
    }
}
