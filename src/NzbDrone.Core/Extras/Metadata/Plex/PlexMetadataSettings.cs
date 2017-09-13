using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexSettingsValidator : AbstractValidator<PlexMetadataSettings>
    {
        public PlexSettingsValidator()
        {
        }
    }

    public class PlexMetadataSettings : IProviderConfig
    {
        private static readonly PlexSettingsValidator Validator = new PlexSettingsValidator();

        public PlexMetadataSettings()
        {
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "Series Images", Type = FieldType.Checkbox)]
        public bool SeriesImages { get; set; }

        [FieldDefinition(1, Label = "Season Images", Type = FieldType.Checkbox)]
        public bool SeasonImages { get; set; }

        [FieldDefinition(2, Label = "Episode Images", Type = FieldType.Checkbox)]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
