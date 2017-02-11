using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserMetadataSettings>
    {
        public MediaBrowserSettingsValidator()
        {
        }
    }

    public class MediaBrowserMetadataSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        public MediaBrowserMetadataSettings()
        {
            SeriesMetadata = true;
        }

        [FieldDefinition(0, Label = "Series Metadata", Type = FieldType.Checkbox, HelpText = "series.xml")]
        public bool SeriesMetadata { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
