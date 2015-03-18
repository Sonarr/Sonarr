using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaSettingsValidator : AbstractValidator<NyaaSettings>
    {
        public NyaaSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class NyaaSettings : IProviderConfig
    {
        private static readonly NyaaSettingsValidator Validator = new NyaaSettingsValidator();

        public NyaaSettings()
        {
            BaseUrl = "http://www.nyaa.se";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}