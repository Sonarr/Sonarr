using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Parser.Model
{
    public class GrabbedReleaseInfo
    {
        public string Title { get; set; }
        public string Indexer { get; set; }
        public int IndexerId { get; set; }
        public long Size { get; set; }
        public ReleaseType ReleaseType { get; set; }

        public List<int> EpisodeIds { get; set; }

        public GrabbedReleaseInfo(List<EpisodeHistory> grabbedHistories)
        {
            var grabbedHistory = grabbedHistories.MaxBy(h => h.Date);
            var episodeIds = grabbedHistories.Select(h => h.EpisodeId).Distinct().ToList();

            grabbedHistory.Data.TryGetValue("indexer", out var indexer);
            grabbedHistory.Data.TryGetValue("indexerId", out var indexerIdString);
            grabbedHistory.Data.TryGetValue("size", out var sizeString);
            Enum.TryParse(grabbedHistory.Data.GetValueOrDefault("releaseType"), out ReleaseType releaseType);
            long.TryParse(sizeString, out var size);
            int.TryParse(indexerIdString, out var indexerId);

            Title = grabbedHistory.SourceTitle;
            Indexer = indexer;
            IndexerId = indexerId;
            Size = size;
            EpisodeIds = episodeIds;
            ReleaseType = releaseType;
        }
    }
}
