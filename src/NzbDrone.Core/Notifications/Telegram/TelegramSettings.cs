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
            RuleFor(c => c.ChatID).NotEmpty();
        }
    }

    public class TelegramSettings : IProviderConfig
    {
        private static readonly TelegramSettingsValidator Validator = new TelegramSettingsValidator();

        public TelegramSettings()
        {
        }

        [FieldDefinition(0, Label = "Bot Token", HelpLink = "https://core.telegram.org/bots")]
        public string BotToken { get; set; }

        [FieldDefinition(1, Label = "Chat ID", HelpLink = "https://telegram.org/")]
        public string ChatID { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ChatID) && !string.IsNullOrWhiteSpace(BotToken);
            }
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
