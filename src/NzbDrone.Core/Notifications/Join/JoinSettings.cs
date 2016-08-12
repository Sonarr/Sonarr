using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Join
{
    public class JoinSettingsValidator : AbstractValidator<JoinSettings>
    {
        public JoinSettingsValidator()
        {
            RuleFor(c => c.APIKey).NotEmpty();
        }
    }

    public class JoinSettings : IProviderConfig
    {
        private static readonly JoinSettingsValidator Validator = new JoinSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpText = "The API Key from your Join account settings (click Join API button).", HelpLink = "https://joinjoaomgcd.appspot.com/")]
        public string APIKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
