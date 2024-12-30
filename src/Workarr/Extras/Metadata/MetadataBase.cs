using FluentValidation.Results;
using Workarr.Extras.Metadata.Files;
using Workarr.MediaFiles;
using Workarr.ThingiProvider;
using Workarr.Tv;

namespace Workarr.Extras.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata
        where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }

        public ValidationResult Test()
        {
            return new ValidationResult();
        }

        public virtual string GetFilenameAfterMove(Series series, EpisodeFile episodeFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(series.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(Path.Combine(series.Path, episodeFile.RelativePath), extension);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Series series, string path);

        public abstract MetadataFileResult SeriesMetadata(Series series, SeriesMetadataReason reason);
        public abstract MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile);
        public abstract List<ImageFileResult> SeriesImages(Series series);
        public abstract List<ImageFileResult> SeasonImages(Series series, Season season);
        public abstract List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile);

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
