using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcMetadataSettings>
    {
        public XbmcSettingsValidator()
        {
        }
    }

    public class XbmcMetadataSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcMetadataSettings()
        {
            SeriesMetadata = true;
            EpisodeMetadata = true;
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "Series Metadata", Type = FieldType.Checkbox, HelpText = "tvshow.nfo")]
        public bool SeriesMetadata { get; set; }

        [FieldDefinition(1, Label = "Episode Metadata", Type = FieldType.Checkbox, HelpText = "Season##\\filename.nfo")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(2, Label = "Series Images", Type = FieldType.Checkbox, HelpText = "fanart.jpg, poster.jpg, banner.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(3, Label = "Season Images", Type = FieldType.Checkbox, HelpText = "season##-poster.jpg, season##-banner.jpg, season-specials-poster.jpg, season-specials-banner.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(4, Label = "Episode Images", Type = FieldType.Checkbox, HelpText = "Season##\\filename-thumb.jpg")]
        public bool EpisodeImages { get; set; }
        
        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
