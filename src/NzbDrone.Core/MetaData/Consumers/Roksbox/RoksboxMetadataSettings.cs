using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Metadata.Consumers.Roksbox
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

        [FieldDefinition(0, Label = "Episode Metadata", Type = FieldType.Checkbox)]
        public Boolean EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "Series Images", Type = FieldType.Checkbox)]
        public Boolean SeriesImages { get; set; }

        [FieldDefinition(2, Label = "Season Images", Type = FieldType.Checkbox)]
        public Boolean SeasonImages { get; set; }

        [FieldDefinition(3, Label = "Episode Images", Type = FieldType.Checkbox)]
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
