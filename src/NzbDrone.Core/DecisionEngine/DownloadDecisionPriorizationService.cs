using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPriorizationService(IDelayProfileService delayProfileService)
        {
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            IEnumerable<DownloadDecision> notNullDecisions = decisions.Where(c => c.RemoteItem != null);
            IEnumerable<DownloadDecision> movies = notNullDecisions.Where(c => c.RemoteItem is RemoteMovie);
            IEnumerable<DownloadDecision> serie = notNullDecisions.Where(c => c.RemoteItem is RemoteEpisode);

            IEnumerable<DownloadDecision> movieDecision = movies.Where(c => (c.RemoteItem as RemoteMovie).Movie!= null)
                                                                .GroupBy(c => c.RemoteItem.Media.Id, (movieId, d) =>
                                                                {
                                                                    var downloadDecisions = d.ToList();
                                                                    var movie = downloadDecisions.First().RemoteItem.Media;
                                                                    
                                                                    return downloadDecisions
                                                                        .OrderByDescending(c => c.RemoteItem.ParsedInfo.Quality, new QualityModelComparer(movie.Profile))
                                                                        .ThenBy(c => PrioritizeDownloadProtocol(movie.Tags, c.RemoteItem.Release.DownloadProtocol))
                                                                        .ThenBy(c => c.RemoteItem.Release.Size.Round(600.Megabytes()))
                                                                        .ThenByDescending(c => TorrentInfo.GetSeeders(c.RemoteItem.Release))
                                                                        .ThenBy(c => c.RemoteItem.Release.Age);
                                                                })
                                                                .SelectMany(c => c)
                                                                .Union(movies.Where (c => (c.RemoteItem as RemoteMovie).Movie == null))
                                                                .ToList();

            return serie.Where(c=> (c.RemoteItem as RemoteEpisode).Series != null)
                        .GroupBy(c => c.RemoteItem.Media.Id, (seriesId, d) => 
                        {
                            var downloadDecisions = d.ToList();
                            var series = downloadDecisions.First().RemoteItem.Media;
                            
                            return downloadDecisions
                                .OrderByDescending(c => c.RemoteItem.ParsedInfo.Quality, new QualityModelComparer(series.Profile))
                                .ThenBy(c => (c.RemoteItem as RemoteEpisode).Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                                .ThenBy(c => PrioritizeDownloadProtocol(series.Tags, c.RemoteItem.Release.DownloadProtocol))
                                .ThenByDescending(c => (c.RemoteItem as RemoteEpisode).Episodes.Count)
                                .ThenBy(c => c.RemoteItem.Release.Size.Round(200.Megabytes()) / Math.Max(1, (c.RemoteItem as RemoteEpisode).Episodes.Count))
                                .ThenByDescending(c => TorrentInfo.GetSeeders(c.RemoteItem.Release))
                                .ThenBy(c => c.RemoteItem.Release.Age);
                        })
                        .SelectMany(c => c)
                        .Union(serie.Where(c => (c.RemoteItem as RemoteEpisode).Series == null))
                        .Union(movieDecision)
                        .ToList();

        }

        private int PrioritizeDownloadProtocol(HashSet<int> tags, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }
    }
}
