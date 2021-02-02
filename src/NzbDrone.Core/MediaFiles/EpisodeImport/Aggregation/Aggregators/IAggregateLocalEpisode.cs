using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public interface IAggregateLocalEpisode
    {
        LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }
}
