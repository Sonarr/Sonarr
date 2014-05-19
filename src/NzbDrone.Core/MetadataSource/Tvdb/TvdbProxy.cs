using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Tv;
using RestSharp;

namespace NzbDrone.Core.MetadataSource.Tvdb
{
    public interface ITvdbProxy
    {
        List<Episode> GetEpisodeInfo(int tvdbSeriesId);
    }

    public class TvdbProxy : ITvdbProxy
    {
        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId)
        {
            var client = BuildClient("series");
            var request = new RestRequest(tvdbSeriesId + "/all");

            var response = client.Execute(request);

            var xml = XDocument.Load(new StringReader(response.Content));

            var episodes = xml.Descendants("Episode").Select(MapEpisode).ToList();
            var series = MapSeries(xml.Element("Series"));

            return new Tuple<Series, List<Episode>>(series, episodes);
        }

        public List<Episode> GetEpisodeInfo(int tvdbSeriesId)
        {
            return GetSeriesInfo(tvdbSeriesId).Item2;
        }

        private static IRestClient BuildClient(string resource)
        {
            return new RestClient(String.Format("http://thetvdb.com/data/{0}", resource));
        }

        private static Series MapSeries(XElement item)
        {
            //TODO: We should map all the data incase we want to actually use it
            var series = new Series();

            return series;
        }

        private static Episode MapEpisode(XElement item)
        {
            //TODO: We should map all the data incase we want to actually use it
            var episode = new Episode();
            episode.SeasonNumber = item.TryGetValue("SeasonNumber", 0);
            episode.EpisodeNumber = item.TryGetValue("EpisodeNumber", 0);
            episode.AbsoluteEpisodeNumber = item.TryGetValue("absolute_number", 0);

            return episode;
        }
    }
}
