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

        [FieldDefinition(0, Label = "MetadataSettingsSeriesMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataHelpText")]
        public bool SeriesMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsSeriesMetadataEpisodeGuide", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataEpisodeGuideHelpText", Advanced = true)]
        public bool SeriesMetadataEpisodeGuide { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsSeriesMetadataUrl", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsSeriesMetadataUrlHelpText", Advanced = true)]
        public bool SeriesMetadataUrl { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsEpisodeMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "<filename>.nfo")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(4, Label = "MetadataSettingsEpisodeMetadataImageThumbs", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "MetadataXmbcSettingsEpisodeMetadataImageThumbsHelpText", Advanced = true)]
        public bool EpisodeImageThumb { get; set; }

        [FieldDefinition(5, Label = "MetadataSettingsSeriesImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "fanart.jpg, poster.jpg, banner.jpg")]
        public bool SeriesImages { get; set; }

        [FieldDefinition(6, Label = "MetadataSettingsSeasonImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "season##-poster.jpg, season##-banner.jpg, season-specials-poster.jpg, season-specials-banner.jpg")]
        public bool SeasonImages { get; set; }

        [FieldDefinition(7, Label = "MetadataSettingsEpisodeImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "<filename>-thumb.jpg")]
        public bool EpisodeImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
