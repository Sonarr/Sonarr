using System;
using System.Collections.Generic;
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

        public SabController(IConfigController config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }

        #region IDownloadClientController Members

        public bool AddByUrl(ItemInfo nzb)
        {
            const string mode = "addurl";
            const string cat = "tv";
            string name = nzb.Link.ToString().Replace("&", "%26");
            string nzbName = HttpUtility.UrlEncode(nzb.Title);

            string action = string.Format("mode={0}&name={1}&cat={2}&nzbname={3}", mode, name, cat, nzbName);
            string request = GetSabRequest(action);

            _logger.DebugFormat("Adding report [{0}] to the queue.", nzbName);

            if (SendRequest(request) == "ok")
                return true;

            return false;
        }

        public bool IsInQueue(Episode epsiode)
        {
            string action = "mode=queue&output=xml";

            XDocument xDoc = XDocument.Load(GetSabRequest(action));

            //If an Error Occurred, retuyrn
            if (xDoc.Descendants("error").Count() != 0)
                return false;

            if (xDoc.Descendants("queue").Count() == 0)
                return false;

            //Get the Count of Items in Queue where 'filename' is Equal to goodName, if not zero, return true (isInQueue)
            if ((from s in xDoc.Descendants("slot") where s.Element("filename").ToString().Equals(epsiode.FeedItem.TitleFix, StringComparison.InvariantCultureIgnoreCase) select s).Count() != 0)
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
            string priority = _config.GetValue("Priority", String.Empty, false);

            return string.Format(
                @"http://{0}/sabnzbd/api?$Action&priority={1}&apikey={2}&ma_username={3}&ma_password={4}",
                sabnzbdInfo, priority, apiKey, username, password).Replace("$Action", action);
        }

        private string SendRequest(string request)
        {
            var webClient = new WebClient();
            string response = webClient.DownloadString(request).Replace("\n", String.Empty);
            _logger.DebugFormat("Queue Repsonse: [{0}]", response);
            return response;
        }
    }
}
