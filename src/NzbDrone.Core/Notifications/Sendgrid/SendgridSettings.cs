using System.Data;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Sendgrid
{

    public class SendgridSettingsValidator : AbstractValidator<SendgridSettings>
    {
        public SendgridSettingsValidator()
        {
            RuleFor(c => SendgridSettings.ApiKey).NotEmpty();
            RuleFor(c => c.From).NotEmpty();
            RuleFor(c => c.To).NotEmpty();
        }
    }
    
    public class SendgridSettings : IProviderConfig {
        
        private static SendgridSettingsValidator Validator = new SendgridSettingsValidator();


        public SendgridSettings() { }
        
        [FieldDefinition(0, Label = "Api Key", HelpText = "The API key for SendGrid")]
        public static string ApiKey { get; set; }
        
        [FieldDefinition(1, Label = "From Address")]
        public string From { get; set; }
        
        [FieldDefinition(2, Label= "Recipient Address")]
        public string To {get; set; }
        
        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}