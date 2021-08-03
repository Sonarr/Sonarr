using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Mailgun
{
    public class MailGunSettingsValidator : AbstractValidator<MailgunSettings>
    {
        public MailGunSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.From).NotEmpty();
            RuleFor(c => c.Recipients).NotEmpty();
        }
    }

    public class MailgunSettings : IProviderConfig
    {
      private static readonly  MailGunSettingsValidator Validator = new MailGunSettingsValidator();

      public MailgunSettings()
      {
          Recipients = new string[] { };
      }

      [FieldDefinition(0, Label = "API Key", HelpText = "The API key generated from MailGun")]
      public string ApiKey { get; set; }

      [FieldDefinition(1, Label = "Use EU Endpoint?", HelpText = "You can choose to use the EU MailGun endpoint with this", Type = FieldType.Checkbox)]
      public bool UseEuEndpoint { get; set; }

      [FieldDefinition(2, Label = "From Address")]
      public string From { get; set; }

      [FieldDefinition(3, Label = "Sender Domain")]
      public string SenderDomain { get; set; }

      [FieldDefinition(4, Label = "Recipient Address(es)", Type = FieldType.Tag)]
      public IEnumerable<string> Recipients { get; set; }

      public NzbDroneValidationResult Validate()
      {
          return new NzbDroneValidationResult(Validator.Validate(this));
      }
    }
}
