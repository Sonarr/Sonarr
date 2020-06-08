using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        int Order { get; }
        string Name { get; }
        AugmentQualityResult AugmentQuality(LocalEpisode localEpisode, DownloadClientItem downloadClientItem);
    }
}
