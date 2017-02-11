using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxSettingsValidator : AbstractValidator<RoksboxMetadataSettings>
    {
        public RoksboxSettingsValidator()
        {
        }
    }

    public class RoksboxMetadataSettings : IProviderConfig
    {
        private static readonly RoksboxSettingsValidator Validator = new RoksboxSettingsValidator();

        public RoksboxMetadataSettings()
        {
            EpisodeMetadata = true;
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "Episode Metadata", Type = FieldType.Checkbox, HelpText = "Season##\\filename.xml")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "Series Images", Type = FieldType.Checkbox, HelpText = "Series Title.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(2, Label = "Season Images", Type = FieldType.Checkbox, HelpText = "Season ##.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(3, Label = "Episode Images", Type = FieldType.Checkbox, HelpText = "Season##\\filename.jpg")]
        public bool EpisodeImages { get; set; }
        
        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
