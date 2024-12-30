using FluentValidation;
using Workarr.Annotations;
using Workarr.ThingiProvider;
using Workarr.Validation;

namespace Workarr.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadataSettingsValidator : AbstractValidator<PlexMetadataSettings>
    {
    }

    public class PlexMetadataSettings : IProviderConfig
    {
        private static readonly PlexMetadataSettingsValidator Validator = new PlexMetadataSettingsValidator();

        public PlexMetadataSettings()
        {
            SeriesPlexMatchFile = true;
        }

        [FieldDefinition(0, Label = "MetadataPlexSettingsSeriesPlexMatchFile", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataPlexSettingsSeriesPlexMatchFileHelpText")]
        public bool SeriesPlexMatchFile { get; set; }

        [FieldDefinition(0, Label = "MetadataPlexSettingsEpisodeMappings", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataPlexSettingsEpisodeMappingsHelpText")]
        public bool EpisodeMappings { get; set; }

        public WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
