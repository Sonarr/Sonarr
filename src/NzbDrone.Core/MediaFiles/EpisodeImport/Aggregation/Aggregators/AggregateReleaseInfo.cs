using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateReleaseInfo : IAggregateLocalEpisode
    {
        private readonly IHistoryService _historyService;

        public AggregateReleaseInfo(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                return localEpisode;
            }

            var grabbedHistories = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == EpisodeHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistories.Empty())
            {
                return localEpisode;
            }

            var episodeIds = grabbedHistories.Select(h => h.EpisodeId).Distinct().ToList();
            var grabbedHistory = grabbedHistories.First();
            var releaseInfo = new GrabbedReleaseInfo();

            grabbedHistory.Data.TryGetValue("indexer", out var indexer);
            grabbedHistory.Data.TryGetValue("size", out var sizeString);
            long.TryParse(sizeString, out var size);

            releaseInfo.Title = grabbedHistory.SourceTitle;
            releaseInfo.Indexer = indexer;
            releaseInfo.Size = size;
            releaseInfo.EpisodeIds = episodeIds;

            localEpisode.Release = releaseInfo;

            return localEpisode;
        }
    }
}
