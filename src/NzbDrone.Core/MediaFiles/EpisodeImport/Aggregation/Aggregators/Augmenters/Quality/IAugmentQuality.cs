using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators.Augmenters.Quality
{
    public interface IAugmentQuality
    {
        AugmentQualityResult AugmentQuality(LocalEpisode localEpisode);
    }
}
