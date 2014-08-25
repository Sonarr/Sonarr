using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Pushalot
{
    public class PushalotSettingsValidator : AbstractValidator<PushalotSettings>
    {
        public PushalotSettingsValidator()
        {
            RuleFor(c => c.AuthToken).NotEmpty();
        }
    }

    public class PushalotSettings : IProviderConfig
    {
        private static readonly PushalotSettingsValidator Validator = new PushalotSettingsValidator();

        [FieldDefinition(0, Label = "Authorization Token", HelpLink = "https://pushalot.com/manager/authorizations")]
        public String AuthToken { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushalotPriority))]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !String.IsNullOrWhiteSpace(AuthToken);
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
