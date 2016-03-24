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
            RuleFor(c => c.BotName).NotEmpty();
            RuleFor(c => c.Icon).NotEmpty();
        }
    }

    public class SlackSettings : IProviderConfig
    {
        private static readonly SlackSettingsValidator Validator = new SlackSettingsValidator();

        [FieldDefinition(0, Label = "WebHookUrl", HelpText = "slack channel webhook url.", Type = FieldType.Url, HelpLink = "https://my.slack.com/services/new/incoming-webhook/")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "BotName", HelpText = "Name to be used for the notification.",Type = FieldType.Textbox)]
        public string BotName { get; set; }

        [FieldDefinition(2, Label = "Icon", HelpText = "Icon to use.", Type = FieldType.Textbox, HelpLink = "http://www.emoji-cheat-sheet.com/")]
        public string Icon { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
