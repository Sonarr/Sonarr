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
            Deprecated = true;
        }

        [FieldDefinition(0, Label = "MetadataKometaDeprecatedSetting", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, Hidden = HiddenType.Hidden)]
        public bool Deprecated { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
