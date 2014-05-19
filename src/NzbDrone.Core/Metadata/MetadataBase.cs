using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata where TSettings : IProviderConfig, new()
    {
        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                return new List<ProviderDefinition>();
            }
        }

        public ProviderDefinition Definition { get; set; }

        public abstract List<MetadataFile> AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles);
        public abstract MetadataFile FindMetadataFile(Series series, string path);

        public abstract MetadataFileResult SeriesMetadata(Series series);
        public abstract MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile);
        public abstract List<ImageFileResult> SeriesImages(Series series);
        public abstract List<ImageFileResult> SeasonImages(Series series, Season season);
        public abstract List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile);

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
