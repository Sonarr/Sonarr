using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.TvRage;

namespace NzbDrone.Core.Providers
{
    public class TvRageProvider
    {
        private readonly HttpProvider _httpProvider;
        private const string TVRAGE_APIKEY = "NW4v0PSmQIoVmpbASLdD";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TvRageProvider(HttpProvider httpProvider)
        {
            _httpProvider = httpProvider;
        }

        public TvRageProvider()
        {
        }

        public virtual IList<TvRageSearchResult> SearchSeries(string title)
        {
            var searchResults = new List<TvRageSearchResult>();

            var xmlStream = _httpProvider.DownloadStream("http://services.tvrage.com/feeds/full_search.php?show=" + title, null);

            var xml = XDocument.Load(xmlStream);
            var shows = xml.Descendants("Results").Descendants("show");

            foreach (var s in shows)
            {
                try
                {
                    var show = new TvRageSearchResult();
                    show.ShowId = s.Element("showid").ConvertTo<Int32>();
                    show.Name = s.Element("name").Value;
                    show.Link = s.Element("link").Value;
                    show.Country = s.Element("country").Value;

                    show.Started = s.Element("started").ConvertTo<DateTime>();
                    show.Ended = s.Element("ended").ConvertTo<DateTime>();

                    if (show.Ended < new DateTime(1900, 1, 1))
                        show.Ended = null;

                    show.Seasons = s.Element("seasons").ConvertTo<Int32>();
                    show.Status = s.Element("status").Value;
                    show.RunTime = s.Element("seasons").ConvertTo<Int32>();
                    show.AirTime = s.Element("seasons").ConvertTo<DateTime>();
                    show.AirDay = s.Element("airday").ConvertToDayOfWeek();

                    searchResults.Add(show);
                }

                catch (Exception ex)
                {
                    logger.DebugException("Failed to parse TvRage Search Result. Search Term : " + title, ex);
                }
            }

            return searchResults;
        }

        public virtual TvRageSeries GetSeries(int id)
        {
            var url = string.Format("http://services.tvrage.com/feeds/showinfo.php?key={0}&sid={1}", TVRAGE_APIKEY, id);
            var xmlStream = _httpProvider.DownloadStream(url, null);
            var xml = XDocument.Load(xmlStream);
            var s = xml.Descendants("Showinfo").First();
            try
            {
                if(s.Element("showid") == null)
                {
                    logger.Warn("TvRage did not return valid series info for id: {0}", id);
                    return null;
                }

                var show = new TvRageSeries();
                show.ShowId = s.Element("showid").ConvertTo<Int32>();
                show.Name = s.Element("showname").Value;
                show.Link = s.Element("showlink").Value;
                show.Seasons = s.Element("seasons").ConvertTo<Int32>();
                show.Started = s.Element("started").ConvertTo<Int32>();

                show.StartDate = s.Element("startdate").ConvertTo<DateTime>();
                show.Ended = s.Element("ended").ConvertTo<DateTime>();

                show.OriginCountry = s.Element("origin_country").Value;
                show.Status = s.Element("status").Value;
                show.RunTime = s.Element("runtime").ConvertTo<Int32>();
                show.Network = s.Element("network").Value;
                show.AirTime = s.Element("airtime").ConvertTo<DateTime>();
                show.AirDay = s.Element("airday").ConvertToDayOfWeek();
                show.UtcOffset = GetUtcOffset(s.Element("timezone").Value);
                return show;
            }

            catch (Exception ex)
            {
                logger.DebugException("Failed to parse ShowInfo for ID: " + id, ex);
                return null;
            }
        }

        public virtual List<TvRageEpisode> GetEpisodes(int id)
        {
            var url = String.Format("http://services.tvrage.com/feeds/episode_list.php?key={0}&sid={1}", TVRAGE_APIKEY, id);
            var xmlStream = _httpProvider.DownloadStream(url, null);
            var xml = XDocument.Load(xmlStream);
            var show = xml.Descendants("Show");
            var seasons = show.Descendants("Season");

            var episodes = new List<TvRageEpisode>();

            foreach (var season in seasons)
            {
                var eps = season.Descendants("episode");

                foreach (var e in eps)
                {
                    try
                    {
                        var episode = new TvRageEpisode();
                        episode.EpisodeNumber = e.Element("epnum").ConvertTo<Int32>();
                        episode.SeasonNumber = e.Element("seasonnum").ConvertTo<Int32>();
                        episode.ProductionCode = e.Element("prodnum").Value;
                        episode.AirDate = e.Element("airdate").ConvertTo<DateTime>();
                        episode.Link = e.Element("link").Value;
                        episode.Title = e.Element("title").Value;
                        episodes.Add(episode);
                    }

                    catch (Exception ex)
                    {
                        logger.DebugException("Failed to parse TV Rage episode", ex);
                    }
                }
            }

            return episodes;
        }

        internal int GetUtcOffset(string timeZone)
        {
            if (String.IsNullOrWhiteSpace(timeZone))
                return 0;

            var offsetString = timeZone.Substring(3, 2);
            int offset;

            if (!Int32.TryParse(offsetString, out offset))
                return 0;

            if (timeZone.IndexOf("+DST", StringComparison.CurrentCultureIgnoreCase) > 0)
                offset++;

            return offset;
        }
    }
}
