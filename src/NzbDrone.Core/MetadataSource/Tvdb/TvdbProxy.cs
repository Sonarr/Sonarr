using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tvdb
{
    public interface ITvdbProxy
    {
        List<Episode> GetEpisodeInfo(int tvdbSeriesId);
    }

    public class TvdbProxy : ITvdbProxy
    {
        private readonly IHttpClient _httpClient;

        public TvdbProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<Episode> GetEpisodeInfo(int tvdbSeriesId)
        {
            var httpRequest = new HttpRequest("http://thetvdb.com/data/series/{tvdbId}/all/");
            httpRequest.AddSegment("tvdbId", tvdbSeriesId.ToString());
            var response = _httpClient.Get(httpRequest);

            var xml = XDocument.Load(new StringReader(response.Content));
            var episodes = xml.Descendants("Episode").Select(MapEpisode).ToList();
            return episodes;
        }

        private static Episode MapEpisode(XElement item)
        {
            //TODO: We should map all the data incase we want to actually use it
            var episode = new Episode();
            episode.SeasonNumber = item.TryGetValue("SeasonNumber", 0);
            episode.EpisodeNumber = item.TryGetValue("EpisodeNumber", 0);

            if (item.TryGetValue("absolute_number").IsNotNullOrWhiteSpace())
            {
                episode.AbsoluteEpisodeNumber = item.TryGetValue("absolute_number", 0);
            }

            return episode;
        }
    }
}
