using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Telegram
{
    public class TelegramSettingsValidator : AbstractValidator<TelegramSettings>
    {
        public TelegramSettingsValidator()
        {
            RuleFor(c => c.BotToken).NotEmpty();
            RuleFor(c => c.ChatId).NotEmpty();
            RuleFor(c => c.TopicId).Must(topicId => !topicId.HasValue || topicId > 1)
                                   .WithMessage("Topic ID must be greater than 1 or empty");
        }
    }

    public class TelegramSettings : IProviderConfig
    {
        private static readonly TelegramSettingsValidator Validator = new TelegramSettingsValidator();

        [FieldDefinition(0, Label = "Bot Token", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://core.telegram.org/bots")]
        public string BotToken { get; set; }

        [FieldDefinition(1, Label = "Chat ID", HelpLink = "http://stackoverflow.com/a/37396871/882971", HelpText = "You must start a conversation with the bot or add it to your group to receive messages")]
        public string ChatId { get; set; }

        [FieldDefinition(2, Label = "Topic ID", HelpLink = "https://stackoverflow.com/a/75178418", HelpText = "Specify a Topic ID to send notifications to that topic. Leave blank to use the general topic (Supergroups only)")]
        public int? TopicId { get; set; }

        [FieldDefinition(3, Label = "Send Silently", Type = FieldType.Checkbox, HelpText = "Sends the message silently. Users will receive a notification with no sound")]
        public bool SendSilently { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
