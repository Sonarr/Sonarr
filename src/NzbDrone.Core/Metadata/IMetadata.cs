using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata
{
    public interface IMetadata : IProvider
    {
        List<MetadataFile> AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles);
        MetadataFile FindMetadataFile(Series series, string path);

        MetadataFileResult SeriesMetadata(Series series);
        MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile);
        List<ImageFileResult> SeriesImages(Series series);
        List<ImageFileResult> SeasonImages(Series series, Season season);
        List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile);

    }
}
