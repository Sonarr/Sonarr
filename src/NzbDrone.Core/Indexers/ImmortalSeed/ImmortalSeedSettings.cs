using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.ImmortalSeed
{
    public class ImmortalSeedSettingsValidator : AbstractValidator<ImmortalSeedSettings>
    {
        public ImmortalSeedSettingsValidator()
        {
        }
    }

    public class ImmortalSeedSettings : IProviderConfig
    {
        private static readonly ImmortalSeedSettingsValidator validator = new ImmortalSeedSettingsValidator();

        public ImmortalSeedSettings()
        {
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public String BaseUrl { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}