using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Wdtv
{
    public class WdtvSettingsValidator : AbstractValidator<WdtvMetadataSettings>
    {
    }

    public class WdtvMetadataSettings : IProviderConfig
    {
        private static readonly WdtvSettingsValidator Validator = new WdtvSettingsValidator();

        public WdtvMetadataSettings()
        {
            EpisodeMetadata = true;
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsEpisodeMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Season##\\filename.xml")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsSeriesImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "folder.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Season##\\folder.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Season##\\filename.metathumb")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
