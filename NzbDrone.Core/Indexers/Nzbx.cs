using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using Newtonsoft.Json;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Nzbx;

namespace NzbDrone.Core.Indexers
{
    class Nzbx : IndexerBase
    {
        public Nzbx(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
        {
        }

        public override string Name
        {
            get { return "nzbx"; }
        }

        protected override string[] Urls
        {
            get
            { 
                return new string[]
                {
                    String.Format("https://nzbx.co/api/recent?category=tv")
                };
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return true;
                //return !string.IsNullOrWhiteSpace(_configProvider.OmgwtfnzbsUsername) &&
                //       !string.IsNullOrWhiteSpace(_configProvider.OmgwtfnzbsApiKey);
            }
        }

        protected override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<String>();

            searchUrls.Add(String.Format("https://nzbx.co/api/search?q={0}+S{1:00}E{2:00}", seriesTitle, seasonNumber, episodeNumber));

            return searchUrls;
        }

        protected override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            searchUrls.Add(String.Format("https://nzbx.co/api/search?q={0}+{1:yyyy MM dd}", seriesTitle, date));

            return searchUrls;
        }

        protected override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<String>();

            searchUrls.Add(String.Format("https://nzbx.co/api/search?q={0}+S{1:00}", seriesTitle, seasonNumber));

            return searchUrls;
        }

        protected override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            searchUrls.Add(String.Format("https://nzbx.co/api/search?q={0}+S{1:00}E{2}", seriesTitle, seasonNumber, episodeWildcard));

            return searchUrls;
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            throw new NotImplementedException();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            throw new NotImplementedException();
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            throw new NotImplementedException();
        }

        public override IList<EpisodeParseResult> FetchRss()
        {
            _logger.Debug("Fetching feeds from " + Name);

            var result = new List<EpisodeParseResult>();

            if (!IsConfigured)
            {
                _logger.Warn("Indexer '{0}' isn't configured correctly. please reconfigure the indexer in settings page.", Name);
                return result;
            }

            foreach (var url in Urls)
            {
                var response = Download(url);

                if (response != null)
                {
                    var feed = JsonConvert.DeserializeObject<List<NzbxRecentItem>>(response);

                    foreach (var item in feed)
                    {
                        try
                        {
                            var episodeParseResult = Parser.ParseTitle(item.Name);
                            if (episodeParseResult != null)
                            {
                                episodeParseResult.Age = DateTime.Now.Date.Subtract(item.PostDate).Days;
                                episodeParseResult.OriginalString = item.Name;
                                episodeParseResult.SceneSource = true;
                                episodeParseResult.NzbUrl = String.Format("http://nzbx.co/nzb?{0}*|*{1}", item.Guid, item.Name);
                                episodeParseResult.NzbInfoUrl = String.Format("http://nzbx.co/d?{0}", item.Guid);
                                episodeParseResult.Indexer = Name;
                                episodeParseResult.Size = item.Size;

                                result.Add(episodeParseResult);
                            }
                        }
                        catch (Exception itemEx)
                        {
                            itemEx.Data.Add("FeedUrl", url);
                            itemEx.Data.Add("Item", item.Name);
                            _logger.ErrorException("An error occurred while processing feed item", itemEx);
                        }

                    }
                }
            }
            
            _logger.Debug("Finished processing feeds from " + Name);
            return result;
        }

        protected List<EpisodeParseResult> Fetch(IEnumerable<string> urls)
        {
            var result = new List<EpisodeParseResult>();
            
            if (!IsConfigured)
            {
                _logger.Warn("Indexer '{0}' isn't configured correctly. please reconfigure the indexer in settings page.", Name);
                return result;
            }

            foreach (var url in urls)
            {
                var response = Download(url);

                if(response != null)
                {
                    var feed = JsonConvert.DeserializeObject<List<NzbxSearchItem>>(response);

                    foreach (var item in feed)
                    {
                        try
                        {
                            var episodeParseResult = Parser.ParseTitle(item.Name);
                            if (episodeParseResult != null)
                            {
                                episodeParseResult.Age = DateTime.Now.Date.Subtract(item.PostDate).Days;
                                episodeParseResult.OriginalString = item.Name;
                                episodeParseResult.SceneSource = true;
                                episodeParseResult.NzbUrl = item.Nzb;
                                episodeParseResult.NzbInfoUrl = String.Format("http://nzbx.co/d?{0}", item.Guid);
                                episodeParseResult.Indexer = Name;
                                episodeParseResult.Size = item.Size;

                                result.Add(episodeParseResult);
                            }
                        }
                        catch (Exception itemEx)
                        {
                            itemEx.Data.Add("FeedUrl", url);
                            itemEx.Data.Add("Item", item.Name);
                            _logger.ErrorException("An error occurred while processing feed item", itemEx);
                        }

                    }
                }
            }

            return result;
        }

        private string Download(string url)
        {
            try
            {
                _logger.Trace("Downloading RSS " + url);

                return _httpProvider.DownloadString(url, Credentials);
            }
            catch (WebException webException)
            {
                if (webException.Message.Contains("503"))
                {
                    _logger.Warn("{0} server is currently unavailable.{1} {2}", Name, url, webException.Message);
                }
                else
                {
                    webException.Data.Add("FeedUrl", url);
                    _logger.ErrorException("An error occurred while processing feed. " + url, webException);
                }
            }
            catch (Exception feedEx)
            {
                feedEx.Data.Add("FeedUrl", url);
                _logger.ErrorException("An error occurred while processing feed. " + url, feedEx);
            }

            return null;
        }
    }
}
