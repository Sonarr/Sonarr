using System;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications.Prowl
{
    public class ProwlSettingsValidator : AbstractValidator<ProwlSettings>
    {
        public ProwlSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class ProwlSettings : IProviderConfig
    {
        private static readonly ProwlSettingsValidator Validator = new ProwlSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpLink = "https://www.prowlapp.com/api_settings.php")]
        public String ApiKey { get; set; }

        [FieldDefinition(1, Label = "Priority", Type = FieldType.Select, SelectOptions= typeof(ProwlPriority) )]
        public Int32 Priority { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ApiKey) && Priority != null & Priority >= -2 && Priority <= 2;
            }
        }

        public ValidationResult Validate()
        {
            return Validator.Validate(this);
        }
    }
}
