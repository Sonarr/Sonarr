using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsSettingsValidator : AbstractValidator<OmgwtfnzbsSettings>
    {
        public OmgwtfnzbsSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class OmgwtfnzbsSettings : IIndexerSetting
    {
        private static readonly OmgwtfnzbsSettingsValidator Validator = new OmgwtfnzbsSettingsValidator();

        [FieldDefinition(0, Label = "Username")]
        public String Username { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
