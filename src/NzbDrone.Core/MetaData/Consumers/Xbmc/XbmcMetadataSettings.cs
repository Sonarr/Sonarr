using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Metadata.Consumers.Xbmc
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

        [FieldDefinition(0, Label = "Series Metadata", Type = FieldType.Checkbox)]
        public Boolean SeriesMetadata { get; set; }

        [FieldDefinition(1, Label = "Episode Metadata", Type = FieldType.Checkbox)]
        public Boolean EpisodeMetadata { get; set; }

        [FieldDefinition(2, Label = "Series Images", Type = FieldType.Checkbox)]
        public Boolean SeriesImages { get; set; }

        [FieldDefinition(3, Label = "Season Images", Type = FieldType.Checkbox)]
        public Boolean SeasonImages { get; set; }

        [FieldDefinition(4, Label = "Episode Images", Type = FieldType.Checkbox)]
        public Boolean EpisodeImages { get; set; }
        
        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
