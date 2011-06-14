using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class SabProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        public SabProvider()
        {
        }

        [Inject]
        public SabProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }

        public virtual bool AddByUrl(string url, string title)
        {
            string cat = _configProvider.SabTvCategory;
            int priority = (int)_configProvider.SabTvPriority;
            string name = GetNzbName(url);
            string nzbName = HttpUtility.UrlEncode(title);

            string action = string.Format("mode=addurl&name={0}&priority={1}&pp=3&cat={2}&nzbname={3}",
                name, priority, cat, nzbName);

            if (url.ToLower().Contains("newzbin"))
            {
                action = action.Replace("mode=addurl", "mode=addid");
            }

            string request = GetSabRequest(action);

            Logger.Info("Adding report [{0}] to the queue.", title);

            string response = _httpProvider.DownloadString(request).Replace("\n", String.Empty);
            Logger.Debug("Queue Response: [{0}]", response);

            if (response == "ok")
                return true;

            Logger.Warn("SAB returned unexpected response '{0}'", response);

            return false;
        }

        private static string GetNzbName(string urlString)
        {
            var url = new Uri(urlString);
            if (url.Host.ToLower().Contains("newzbin"))
            {
                var postId = Regex.Match(urlString, @"\d{5,10}").Value;
                return postId;
            }

            return urlString.Replace("&", "%26");
        }

        public virtual bool IsInQueue(string title)
        {
            const string action = "mode=queue&output=xml";
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            XDocument xDoc = XDocument.Parse(response);

            //If an Error Occurred, return)
            if (xDoc.Descendants("error").Count() != 0)
                throw new ApplicationException(xDoc.Descendants("error").FirstOrDefault().Value);

            if (xDoc.Descendants("queue").Count() == 0)
            {
                Logger.Debug("SAB Queue is empty. retiring false");
                return false;
            }
            //Get the Count of Items in Queue where 'filename' is Equal to goodName, if not zero, return true (isInQueue)))
            if (
                (xDoc.Descendants("slot").Where(
                    s => s.Element("filename").Value.Equals(title, StringComparison.InvariantCultureIgnoreCase))).Count() !=
                0)
            {
                Logger.Debug("Episode in queue - '{0}'", title);

                return true;
            }

            return false; //Not in Queue
        }

        private string GetSabRequest(string action)
        {
            return string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 _configProvider.SabHost,
                                 _configProvider.SabPort,
                                 action,
                                 _configProvider.SabApiKey,
                                 _configProvider.SabUsername,
                                 _configProvider.SabPassword);
        }

        public virtual String GetSabTitle(EpisodeParseResult parseResult)
        {
            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<String>();

            foreach (var episode in parseResult.EpisodeNumbers)
            {
                episodeString.Add(String.Format("{0}x{1}", parseResult.SeasonNumber, episode));
            }

            var epNumberString = String.Join("-", episodeString);

            var result = String.Format("{0} - {1} - {2} [{3}]", new DirectoryInfo(parseResult.Series.Path).Name, epNumberString, parseResult.Episodes.FirstOrDefault().Title, parseResult.Quality.QualityType);

            if (parseResult.Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }
    }
}