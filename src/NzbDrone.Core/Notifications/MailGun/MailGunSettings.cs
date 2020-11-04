using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.MailGun
{
    public class MailGunSettingsValidator : AbstractValidator<MailGunSettings>
    {
        public MailGunSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.From).NotEmpty();
            RuleFor(c => c.Recipients).NotEmpty();
        }
    }
    
    public class MailGunSettings : IProviderConfig
    {
      private static readonly  MailGunSettingsValidator Validator = new MailGunSettingsValidator();

      public MailGunSettings()
      {
          BaseUrlUs = "https://api.mailgun.net/v3";
          BaseUrlEu = "https://api.eu.mailgun.net/v3";
          Recipients = new string[] { };
      }
      
      public string BaseUrlUs { get; set; }
      public string BaseUrlEu { get; set; }
      
      [FieldDefinition(1, Label = "API Key", HelpText = "The API key generated from MailGun")]
      public string ApiKey { get; set; }
      
      [FieldDefinition(2, Label = "Use EU endpoint?", HelpText = "You can choose to use the EU MailGun endpoint with this", Type = FieldType.Checkbox)]
      public bool IsEu { get; set; }
      
      [FieldDefinition(3, Label = "From Address")]
      public string From { get; set; }
      
      [FieldDefinition(4, Label = "The sender domain")]
      public string Domain { get; set; }
      
      [FieldDefinition(5, Label = "Recipient Address(es)", Type = FieldType.Tag)]
      public IEnumerable<string> Recipients { get; set; }

      [FieldDefinition(9, Label = "Test mode", HelpText = "Send messages in test mode", Advanced = true, Type = FieldType.Checkbox)]
      public bool TestMode { get; set; }
      
      public NzbDroneValidationResult Validate()
      {
          return new NzbDroneValidationResult(Validator.Validate(this));
      }
    }
}
