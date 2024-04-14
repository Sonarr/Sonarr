using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Slack
{
    public class SlackSettingsValidator : AbstractValidator<SlackSettings>
    {
        public SlackSettingsValidator()
        {
            RuleFor(c => c.WebHookUrl).IsValidUrl();
            RuleFor(c => c.Username).NotEmpty();
        }
    }

    public class SlackSettings : NotificationSettingsBase<SlackSettings>
    {
        private static readonly SlackSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "NotificationsSettingsWebhookUrl", HelpText = "NotificationsSlackSettingsWebhookUrlHelpText", Type = FieldType.Url, HelpLink = "https://my.slack.com/services/new/incoming-webhook/")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "NotificationsSlackSettingsUsernameHelpText", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "NotificationsSlackSettingsIcon", HelpText = "NotificationsSlackSettingsIconHelpText", Type = FieldType.Textbox, HelpLink = "http://www.emoji-cheat-sheet.com/")]
        public string Icon { get; set; }

        [FieldDefinition(3, Label = "NotificationsSlackSettingsChannel", HelpText = "NotificationsSlackSettingsChannelHelpText", Type = FieldType.Textbox)]
        public string Channel { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
