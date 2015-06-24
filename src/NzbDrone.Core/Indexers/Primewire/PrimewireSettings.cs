using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Primewire
{
    public class PrimewireSettingsValidator : AbstractValidator<PrimewireSettings>
    {
        public PrimewireSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }
    public class PrimewireSettings : IProviderConfig
    {
        private static readonly PrimewireSettingsValidator Validator = new PrimewireSettingsValidator();

        public PrimewireSettings()
        {
            BaseUrl = "http://www.primewire.ag/";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}