using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using TVDBSharp.Models.DAO;
using TVDBSharp.Models.Enums;
using TVDBSharp.Utilities;

namespace TVDBSharp.Models
{
    /// <summary>
    ///     Provides builder classes for complex entities.
    /// </summary>
    public class Builder
    {
        private const string UriPrefix = "http://thetvdb.com/banners/";
        private readonly IDataProvider _dataProvider;

        /// <summary>
        ///     Initializes a new Builder object with the given <see cref="IDataProvider" />.
        /// </summary>
        /// <param name="dataProvider">The DataProvider used to retrieve XML responses.</param>
        public Builder(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        ///     Builds a show object from the given show ID.
        /// </summary>
        /// <param name="showID">ID of the show to serialize into a <see cref="Show" /> object.</param>
        /// <returns>Returns the Show object.</returns>
        public Show BuildShow(int showID)
        {
            var builder = new ShowBuilder(_dataProvider.GetShow(showID));
            return builder.GetResult();
        }

        public Episode BuildEpisode(int episodeId, string lang)
        {
            var builder = new EpisodeBuilder(_dataProvider.GetEpisode(episodeId, lang).Descendants("Episode").First());
            return builder.GetResult();
        }

        public Updates BuildUpdates(Interval interval)
        {
            var builder = new UpdatesBuilder(_dataProvider.GetUpdates(interval));
            return builder.GetResult();
        }

        /// <summary>
        ///     Returns a list of <see cref="Show" /> objects that match the given query.
        /// </summary>
        /// <param name="query">Query the search is performed with.</param>
        /// <param name="results">Maximal amount of shows the resultset should return.</param>
        /// <returns>Returns a list of show objects.</returns>
        public List<Show> Search(string query, int results)
        {
            var shows = new List<Show>();
            var doc = _dataProvider.Search(query);

            foreach (var element in doc.Descendants("Series").Take(results))
            {
                var id = int.Parse(element.GetXmlData("seriesid"));

                try
                {
                    var response = _dataProvider.GetShow(id);
                    shows.Add(new ShowBuilder(response).GetResult());
                }
                catch (WebException ex)
                {
                    
                }
               
            }

            return shows;
        }

        private static Uri GetBannerUri(string uriSuffix)
        {
            return new Uri(UriPrefix + uriSuffix, UriKind.Absolute);
        }

        private class ShowBuilder
        {
            private readonly Show _show;

            public ShowBuilder(XDocument doc)
            {
                _show = new Show();
                _show.Id = int.Parse(doc.GetSeriesData("id"));
                _show.ImdbId = doc.GetSeriesData("IMDB_ID");
                _show.Name = doc.GetSeriesData("SeriesName");
                _show.Language = doc.GetSeriesData("Language");
                _show.Network = doc.GetSeriesData("Network");
                _show.Description = doc.GetSeriesData("Overview");
                _show.Rating = string.IsNullOrWhiteSpace(doc.GetSeriesData("Rating"))
                    ? (double?) null
                    : Convert.ToDouble(doc.GetSeriesData("Rating"),
                        System.Globalization.CultureInfo.InvariantCulture);
                _show.RatingCount = string.IsNullOrWhiteSpace(doc.GetSeriesData("RatingCount"))
                    ? 0
                    : Convert.ToInt32(doc.GetSeriesData("RatingCount"));
                _show.Runtime = string.IsNullOrWhiteSpace(doc.GetSeriesData("Runtime"))
                    ? (int?) null
                    : Convert.ToInt32(doc.GetSeriesData("Runtime"));
                _show.Banner = GetBannerUri(doc.GetSeriesData("banner"));
                _show.Fanart = GetBannerUri(doc.GetSeriesData("fanart"));
                _show.LastUpdated = string.IsNullOrWhiteSpace(doc.GetSeriesData("lastupdated"))
                    ? (long?) null
                    : Convert.ToInt64(doc.GetSeriesData("lastupdated"));
                _show.Poster = GetBannerUri(doc.GetSeriesData("poster"));
                _show.Zap2ItID = doc.GetSeriesData("zap2it_id");
                _show.FirstAired = string.IsNullOrWhiteSpace(doc.GetSeriesData("FirstAired"))
                    ? (DateTime?) null
                    : Utils.ParseDate(doc.GetSeriesData("FirstAired"));
                _show.AirTime = string.IsNullOrWhiteSpace(doc.GetSeriesData("Airs_Time"))
                    ? (TimeSpan?) null
                    : Utils.ParseTime(doc.GetSeriesData("Airs_Time"));
                _show.AirDay = string.IsNullOrWhiteSpace(doc.GetSeriesData("Airs_DayOfWeek"))
                    ? (Frequency?) null
                    : (Frequency) Enum.Parse(typeof (Frequency), doc.GetSeriesData("Airs_DayOfWeek"));
                _show.Status = string.IsNullOrWhiteSpace(doc.GetSeriesData("Status"))
                    ? Status.Unknown
                    : (Status) Enum.Parse(typeof (Status), doc.GetSeriesData("Status"));
                _show.ContentRating = Utils.GetContentRating(doc.GetSeriesData("ContentRating"));
                _show.Genres =
                    new List<string>(doc.GetSeriesData("Genre")
                        .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries));
                _show.Actors =
                    new List<string>(doc.GetSeriesData("Actors")
                        .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries));
                _show.Episodes = new EpisodesBuilder(doc).BuildEpisodes();
            }

            public Show GetResult()
            {
                return _show;
            }
        }

        public class EpisodeBuilder
        {
            private readonly Episode _episode;

            public EpisodeBuilder(XElement episodeNode)
            {
                _episode = new Episode
                {
                    Id = int.Parse(episodeNode.GetXmlData("id")),
                    Title = episodeNode.GetXmlData("EpisodeName"),
                    Description = episodeNode.GetXmlData("Overview"),
                    EpisodeNumber = int.Parse(episodeNode.GetXmlData("EpisodeNumber")),
                    Director = episodeNode.GetXmlData("Director"),
                    EpisodeImage = GetBannerUri(episodeNode.GetXmlData("filename")),
                    FirstAired =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("FirstAired"))
                            ? (DateTime?) null
                            : Utils.ParseDate(episodeNode.GetXmlData("FirstAired")),
                    GuestStars =
                        new List<string>(episodeNode.GetXmlData("GuestStars")
                            .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)),
                    ImdbId = episodeNode.GetXmlData("IMDB_ID"),
                    Language = episodeNode.GetXmlData("Language"),
                    LastUpdated =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("lastupdated"))
                            ? 0L
                            : Convert.ToInt64(episodeNode.GetXmlData("lastupdated")),
                    Rating =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("Rating"))
                            ? (double?) null
                            : Convert.ToDouble(episodeNode.GetXmlData("Rating"),
                                System.Globalization.CultureInfo.InvariantCulture),
                    RatingCount =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("RatingCount"))
                            ? 0
                            : Convert.ToInt32(episodeNode.GetXmlData("RatingCount")),
                    SeasonId = int.Parse(episodeNode.GetXmlData("seasonid")),
                    SeasonNumber = int.Parse(episodeNode.GetXmlData("SeasonNumber")),
                    SeriesId = int.Parse(episodeNode.GetXmlData("seriesid")),
                    ThumbHeight =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("thumb_height"))
                            ? (int?) null
                            : Convert.ToInt32(episodeNode.GetXmlData("thumb_height")),
                    ThumbWidth =
                        string.IsNullOrWhiteSpace(episodeNode.GetXmlData("thumb_width"))
                            ? (int?) null
                            : Convert.ToInt32(episodeNode.GetXmlData("thumb_width")),
                    TmsExport = episodeNode.GetXmlData("tms_export"),
                    Writers =
                        new List<string>(episodeNode.GetXmlData("Writer")
                            .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
                };
            }

            public Episode GetResult()
            {
                return _episode;
            }
        }

        private class EpisodesBuilder
        {
            private readonly XDocument _doc;

            public EpisodesBuilder(XDocument doc)
            {
                _doc = doc;
            }

            public List<Episode> BuildEpisodes()
            {
                var result = new List<Episode>();

                foreach (var episodeNode in _doc.Descendants("Episode"))
                {
                    var episode = new EpisodeBuilder(episodeNode).GetResult();
                    result.Add(episode);
                }

                return result;
            }
        }

        public class UpdatesBuilder
        {
            private readonly Updates _updates;

            public UpdatesBuilder(XDocument doc)
            {
                if (doc.Root != null)
                {
                    _updates = new Updates
                    {
                        Time = int.Parse(doc.Root.Attribute("time").Value),
                        UpdatedSeries = doc.Root.Elements("Series")
                            .Select(elt => new UpdatedSerie
                            {
                                Id = int.Parse(elt.Element("id").Value),
                                Time = int.Parse(elt.Element("time").Value)
                            })
                            .ToList(),
                        UpdatedEpisodes = doc.Root.Elements("Episode")
                            .Select(elt => new UpdatedEpisode
                            {
                                Id = int.Parse(elt.Element("id").Value),
                                SerieId = int.Parse(elt.Element("Series").Value),
                                Time = int.Parse(elt.Element("time").Value)
                            })
                            .ToList(),
                        UpdatedBanners = doc.Root.Elements("Banner")
                            .Select(elt => new UpdatedBanner
                            {
                                SerieId = int.Parse(elt.Element("Series").Value),
                                Format = elt.Element("format").Value,
                                Language =
                                    elt.Elements("language").Select(n => n.Value).FirstOrDefault() ?? string.Empty,
                                Path = elt.Element("path").Value,
                                Type = elt.Element("type").Value,
                                SeasonNumber = elt.Elements("SeasonNumber").Any()
                                    ? int.Parse(elt.Element("SeasonNumber").Value)
                                    : (int?) null,
                                Time = int.Parse(elt.Element("time").Value)
                            })
                            .ToList()
                    };
                }
            }

            public Updates GetResult()
            {
                return _updates;
            }
        }
    }
}