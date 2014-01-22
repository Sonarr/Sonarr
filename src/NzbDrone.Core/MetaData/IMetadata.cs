using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata
{
    public interface IMetadata : IProvider
    {
        void OnSeriesUpdated(Series series);
        void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload);
        void AfterRename(Series series);
    }
}
