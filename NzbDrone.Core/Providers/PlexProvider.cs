using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class PlexProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly ConfigProvider _configProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public PlexProvider(HttpProvider httpProvider, ConfigProvider configProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;
        }

        public PlexProvider()
        {
            
        }

        public virtual void Notify(string header, string message)
        {
            //Foreach plex client send a notification
            foreach(var host in _configProvider.PlexClientHosts.Split(','))
            {
                try
                {
                    var command = String.Format("ExecBuiltIn(Notification({0}, {1}))", header, message);
                    SendCommand(host, command, _configProvider.PlexUsername, _configProvider.PlexPassword);
                }
                catch(Exception ex)
                {
                    logger.WarnException("Failed to send notification to Plex Client: " + host, ex);
                }
            }
        }

        public virtual void UpdateLibrary()
        {
            var host = _configProvider.PlexServerHost;

            try
            {
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
    }
}
