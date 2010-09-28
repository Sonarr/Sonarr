using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using log4net;
using System.Xml.Linq;
using System.Xml;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Controllers
{
    public class SabController : IDownloadClientController
    {
        private readonly IConfigController _config;
        private readonly ILog _logger;
        private readonly IHttpController _http;

        public SabController(IConfigController config, ILog logger, IHttpController http)
        {
            _config = config;
            _logger = logger;
            _http = http;
        }

        #region IDownloadClientController Members

        public bool AddByUrl(ItemInfo nzb)
        {
            const string mode = "addurl";
            const string cat = "tv";
            string priority = _config.GetValue("Priority", String.Empty, false);
            string name = nzb.Link.ToString().Replace("&", "%26");
            string nzbName = HttpUtility.UrlEncode(nzb.Title);

            string action = string.Format("mode={0}&name={1}&priority={2}&cat={3}&nzbname={4}", mode, name, priority, cat, nzbName);
            string request = GetSabRequest(action);

            _logger.DebugFormat("Adding report [{0}] to the queue.", nzbName);

            string response = _http.GetRequest(request).Replace("\n", String.Empty);
            _logger.DebugFormat("Queue Repsonse: [{0}]", response);

            if (response == "ok")
                return true;

            return false;
        }

        public bool IsInQueue(Episode epsiode)
        {
            string action = "mode=queue&output=xml";
            string request = GetSabRequest(action);
            string response = _http.GetRequest(request);

            XDocument xDoc = XDocument.Parse(response);

            //If an Error Occurred, retuyrn)
            if (xDoc.Descendants("error").Count() != 0)
                return false;

            if (xDoc.Descendants("queue").Count() == 0)
                return false;

            //Get the Count of Items in Queue where 'filename' is Equal to goodName, if not zero, return true (isInQueue)))
            if ((from s in xDoc.Descendants("slot") where s.Element("filename").Value.Equals(epsiode.FeedItem.TitleFix, StringComparison.InvariantCultureIgnoreCase) select s).Count() != 0)
            {
                _logger.DebugFormat("Episode in queue - '{0}'", epsiode.FeedItem.TitleFix);

                return true;
            }

            return false; //Not in Queue
        }

        #endregion

        private string GetSabRequest(string action)
        {
            string sabnzbdInfo = _config.GetValue("SabnzbdInfo", String.Empty, false);
            string username = _config.GetValue("Username", String.Empty, false);
            string password = _config.GetValue("Password", String.Empty, false);
            string apiKey = _config.GetValue("ApiKey", String.Empty, false);

            return string.Format(
                @"http://{0}/sabnzbd/api?$Action&apikey={1}&ma_username={2}&ma_password={3}",
                sabnzbdInfo, apiKey, username, password).Replace("$Action", action);
        }
    }
}
