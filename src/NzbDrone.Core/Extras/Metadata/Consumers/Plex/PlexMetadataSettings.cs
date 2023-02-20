using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadataSettingsValidator : AbstractValidator<PlexMetadataSettings>
    {
        public PlexMetadataSettingsValidator()
        {
        }
    }

    public class PlexMetadataSettings : IProviderConfig
    {
        private static readonly PlexMetadataSettingsValidator Validator = new PlexMetadataSettingsValidator();

        public PlexMetadataSettings()
        {
            SeriesPlexMatchFile = true;
        }

        [FieldDefinition(0, Label = "Series Plex Match File", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Creates a .plexmatch file in the series folder")]
        public bool SeriesPlexMatchFile { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
