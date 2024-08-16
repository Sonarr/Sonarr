using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Kometa
{
    public class KometaSettingsValidator : AbstractValidator<KometaMetadataSettings>
    {
    }

    public class KometaMetadataSettings : IProviderConfig
    {
        private static readonly KometaSettingsValidator Validator = new KometaSettingsValidator();

        public KometaMetadataSettings()
        {
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsSeriesImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Poster.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Season##.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "S##E##.jpg")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
