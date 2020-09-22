using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
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

    public class SlackSettings : IProviderConfig
    {
        private static readonly SlackSettingsValidator Validator = new SlackSettingsValidator();

        [FieldDefinition(0, Label = "Webhook URL", HelpText = "Slack channel webhook url", Type = FieldType.Url, HelpLink = "https://my.slack.com/services/new/incoming-webhook/")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "Choose the username that this integration will post as", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Icon", HelpText = "Change the icon that is used for messages from this integration (Emoji or URL)", Type = FieldType.Textbox, HelpLink = "http://www.emoji-cheat-sheet.com/")]
        public string Icon { get; set; }

        [FieldDefinition(3, Label = "Channel", HelpText = "Overrides the default channel for the incoming webhook (#other-channel)", Type = FieldType.Textbox)]
        public string Channel { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
