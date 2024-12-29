using FluentValidation;
using Workarr.Annotations;
using Workarr.ThingiProvider;
using Workarr.Validation;

namespace Workarr.Extras.Metadata.Consumers.Kometa
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

        public WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
