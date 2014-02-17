using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Metadata.Consumers.Fake
{
    public class FakeMetadataSettingsValidator : AbstractValidator<FakeMetadataSettings>
    {
        public FakeMetadataSettingsValidator()
        {
        }
    }

    public class FakeMetadataSettings : IProviderConfig
    {
        private static readonly FakeMetadataSettingsValidator Validator = new FakeMetadataSettingsValidator();

        public FakeMetadataSettings()
        {
            FakeSetting = true;
        }

        [FieldDefinition(0, Label = "Fake Setting", Type = FieldType.Checkbox)]
        public Boolean FakeSetting { get; set; }
        
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
