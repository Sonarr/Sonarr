using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcMetadataSettings>
    {
    }

    public class XbmcMetadataSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcMetadataSettings()
        {
            SeriesMetadata = true;
            SeriesMetadataEpisodeGuide = false;
            SeriesMetadataUrl = false;
            EpisodeMetadata = true;
            SeriesImages = true;
            SeasonImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "Series Metadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "tvshow.nfo with full series metadata")]
        public bool SeriesMetadata { get; set; }

        [FieldDefinition(1, Label = "Series Metadata Episode Guide", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Include JSON formatted episode guide element in tvshow.nfo (Requires 'Series Metadata')", Advanced = true)]
        public bool SeriesMetadataEpisodeGuide { get; set; }

        [FieldDefinition(2, Label = "Series Metadata URL", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Include TheTVDB show URL in tvshow.nfo (can be combined with 'Series Metadata')", Advanced = true)]
        public bool SeriesMetadataUrl { get; set; }

        [FieldDefinition(3, Label = "Episode Metadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "<filename>.nfo")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(4, Label = "Series Images", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "fanart.jpg, poster.jpg, banner.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(5, Label = "Season Images", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "season##-poster.jpg, season##-banner.jpg, season-specials-poster.jpg, season-specials-banner.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(6, Label = "Episode Images", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "<filename>-thumb.jpg")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
