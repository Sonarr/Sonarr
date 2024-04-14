using FluentValidation;
using NzbDrone.Core.Annotations;
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

    public class TelegramSettings : NotificationSettingsBase<TelegramSettings>
    {
        private static readonly TelegramSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "NotificationsTelegramSettingsBotToken", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://core.telegram.org/bots")]
        public string BotToken { get; set; }

        [FieldDefinition(1, Label = "NotificationsTelegramSettingsChatId", HelpLink = "http://stackoverflow.com/a/37396871/882971", HelpText = "NotificationsTelegramSettingsChatIdHelpText")]
        public string ChatId { get; set; }

        [FieldDefinition(2, Label = "NotificationsTelegramSettingsTopicId", HelpLink = "https://stackoverflow.com/a/75178418", HelpText = "NotificationsTelegramSettingsTopicIdHelpText")]
        public int? TopicId { get; set; }

        [FieldDefinition(3, Label = "NotificationsTelegramSettingsSendSilently", Type = FieldType.Checkbox, HelpText = "NotificationsTelegramSettingsSendSilentlyHelpText")]
        public bool SendSilently { get; set; }

        [FieldDefinition(4, Label = "NotificationsTelegramSettingsIncludeAppName", Type = FieldType.Checkbox, HelpText = "NotificationsTelegramSettingsIncludeAppNameHelpText")]
        public bool IncludeAppNameInTitle { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
