using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregatePreferredWordScore : IAggregateLocalEpisode
    {
        private readonly IPreferredWordService _preferredWordService;

        public AggregatePreferredWordScore(IPreferredWordService preferredWordService)
        {
            _preferredWordService = preferredWordService;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var series = localEpisode.Series;
            var scores = new List<int>();

            if (localEpisode.FileEpisodeInfo?.ReleaseTitle != null)
            {
                scores.Add(_preferredWordService.Calculate(series, localEpisode.FileEpisodeInfo.ReleaseTitle, 0));
            }

            if (localEpisode.SceneName != null)
            {
                scores.Add(_preferredWordService.Calculate(series, localEpisode.SceneName, 0));
            }

            localEpisode.PreferredWordScore = scores.MaxOrDefault();

            return localEpisode;
        }
    }
}
