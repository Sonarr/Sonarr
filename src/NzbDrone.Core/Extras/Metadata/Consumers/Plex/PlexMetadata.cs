using System.Collections.Generic;
using System.IO;
using System.Text;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadata : MetadataBase<PlexMetadataSettings>
    {
        public override string Name => "Plex";

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var relativePath = series.Path.GetRelativePath(path);

            if (relativePath == ".plexmatch")
            {
                return new MetadataFile
                {
                    SeriesId = series.Id,
                    Consumer = GetType().Name,
                    RelativePath = series.Path.GetRelativePath(path),
                    Type = MetadataType.SeriesMetadata
                };
            }

            return null;
        }

        public override MetadataFileResult SeriesMetadata(Series series)
        {
            if (!Settings.SeriesPlexMatchFile)
            {
                return null;
            }

            var content = new StringBuilder();

            content.AppendLine($"Title: {series.Title}");
            content.AppendLine($"Year: {series.Year}");
            content.AppendLine($"TvdbId: {series.TvdbId}");
            content.AppendLine($"ImdbId: {series.ImdbId}");

            return new MetadataFileResult(".plexmatch", content.ToString());
        }

        public override MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile)
        {
            return null;
        }

        public override List<ImageFileResult> SeriesImages(Series series)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> SeasonImages(Series series, Season season)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile)
        {
            return new List<ImageFileResult>();
        }
    }
}
