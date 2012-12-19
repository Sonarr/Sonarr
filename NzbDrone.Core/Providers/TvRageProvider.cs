using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model.TvRage;

namespace NzbDrone.Core.Providers
{
    public class TvRageProvider
    {
        private readonly HttpProvider _httpProvider;
        private const string TVRAGE_APIKEY = "NW4v0PSmQIoVmpbASLdD";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
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
                    show.ShowId = Int32.Parse(s.Element("showid").Value);
                    show.Name = s.Element("name").Value;
                    show.Link = s.Element("link").Value;
                    show.Country = s.Element("country").Value;

                    DateTime started;
                    if (DateTime.TryParse(s.Element("started").Value, out started)) ;
                    show.Started = started;

                    DateTime ended;
                    if (DateTime.TryParse(s.Element("ended").Value, out ended)) ;
                    show.Ended = ended;

                    if (show.Ended < new DateTime(1900, 1, 1))
                        show.Ended = null;

                    show.Seasons = Int32.Parse(s.Element("seasons").Value);
                    show.Status = s.Element("status").Value;
                    show.RunTime = Int32.Parse(s.Element("runtime").Value);
                    show.AirTime = DateTime.Parse(s.Element("airtime").Value);
                    show.AirDay = ParseDayOfWeek(s.Element("airday"));

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
                show.ShowId = Int32.Parse(s.Element("showid").Value);
                show.Name = s.Element("showname").Value;
                show.Link = s.Element("showlink").Value;
                show.Seasons = Int32.Parse(s.Element("seasons").Value);
                show.Started = Int32.Parse(s.Element("started").Value);

                DateTime startDate;
                if (DateTime.TryParse(s.Element("startdate").Value, out startDate)) ;
                show.StartDate = startDate;

                DateTime ended;
                if (DateTime.TryParse(s.Element("ended").Value, out ended)) ;
                show.Ended = ended;

                show.OriginCountry = s.Element("origin_country").Value;
                show.Status = s.Element("status").Value;
                show.RunTime = Int32.Parse(s.Element("runtime").Value);
                show.Network = s.Element("network").Value;
                show.AirTime = DateTime.Parse(s.Element("airtime").Value);
                show.AirDay = ParseDayOfWeek(s.Element("airday"));
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
                        episode.EpisodeNumber = Int32.Parse(e.Element("epnum").Value);
                        episode.SeasonNumber = Int32.Parse(e.Element("seasonnum").Value);
                        episode.ProductionCode = e.Element("prodnum").Value;
                        episode.AirDate = DateTime.Parse(e.Element("airdate").Value);
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

        internal DayOfWeek? ParseDayOfWeek(XElement element)
        {
            if(element == null)
                return null;

            if(String.IsNullOrWhiteSpace(element.Value))
                return null;

            try
            {
                return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), element.Value);
            }
            catch(Exception)
            {
            }

            return null;
        }
    }
}
