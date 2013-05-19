using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Notifications.Plex
{
    public class PlexProvider
    {
        private readonly IHttpProvider _httpProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PlexProvider(IHttpProvider httpProvider)
        {
            _httpProvider = httpProvider;
        }

        public virtual void Notify(PlexClientSettings settings, string header, string message)
        {
            try
            {
                var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", header, message);
                SendCommand(settings.Host, command, settings.Username, settings.Password);
            }
            catch(Exception ex)
            {
                logger.WarnException("Failed to send notification to Plex Client: " + settings.Host, ex);
            }
        }

        public virtual void UpdateLibrary(string host)
        {
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

        public virtual void TestNotification(string host, string username, string password)
        {
            logger.Trace("Sending Test Notifcation to XBMC Host: {0}", host);
            var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", "Test Notification", "Success! Notifications are setup correctly");
            SendCommand(host.Trim(), command, username, password);
        }
    }
}
