using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
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

    public class MailgunSettings : NotificationSettingsBase<MailgunSettings>
    {
      private static readonly  MailGunSettingsValidator Validator = new ();

      public MailgunSettings()
      {
          Recipients = Array.Empty<string>();
      }

      [FieldDefinition(0, Label = "ApiKey", HelpText = "NotificationsMailgunSettingsApiKeyHelpText")]
      public string ApiKey { get; set; }

      [FieldDefinition(1, Label = "NotificationsMailgunSettingsUseEuEndpoint", HelpText = "NotificationsMailgunSettingsUseEuEndpointHelpText", Type = FieldType.Checkbox)]
      public bool UseEuEndpoint { get; set; }

      [FieldDefinition(2, Label = "NotificationsEmailSettingsFromAddress")]
      public string From { get; set; }

      [FieldDefinition(3, Label = "NotificationsMailgunSettingsSenderDomain")]
      public string SenderDomain { get; set; }

      [FieldDefinition(4, Label = "NotificationsEmailSettingsRecipientAddress", Type = FieldType.Tag)]
      public IEnumerable<string> Recipients { get; set; }

      public override NzbDroneValidationResult Validate()
      {
          return new NzbDroneValidationResult(Validator.Validate(this));
      }
    }
}
