﻿using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookSettingsValidator : AbstractValidator<WebhookSettings>
    {
        public WebhookSettingsValidator()
        {
            RuleFor(c => c.Url).IsValidUrl();
            RuleForEach(c => c.Headers).Matches("^\\S+: ?\\S+$").WithMessage("Header must have the \"Header-Name: Value\" format.");
        }
    }

    public class WebhookSettings : NotificationSettingsBase<WebhookSettings>
    {
        private static readonly WebhookSettingsValidator Validator = new ();

        public WebhookSettings()
        {
            Method = Convert.ToInt32(WebhookMethod.POST);
            Headers = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "NotificationsSettingsWebhookUrl", Type = FieldType.Url)]
        public string Url { get; set; }

        [FieldDefinition(1, Label = "NotificationsSettingsWebhookMethod", Type = FieldType.Select, SelectOptions = typeof(WebhookMethod), HelpText = "NotificationsSettingsWebhookMethodHelpText")]
        public int Method { get; set; }

        [FieldDefinition(2, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "NotificationsSettingsWebhookHeaders", Type = FieldType.Tag, Advanced = true, Placeholder = "Header-Name: Value")]
        public IEnumerable<string> Headers { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
