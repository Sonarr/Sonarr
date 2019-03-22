using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discord
{
    public class DiscordSettingsValidator : AbstractValidator<DiscordSettings>
    {
        public DiscordSettingsValidator()
        {
            RuleFor(c => c.WebHookUrl).IsValidUrl();
        }
    }

    public class DiscordSettings : IProviderConfig
    {
        private static readonly DiscordSettingsValidator Validator = new DiscordSettingsValidator();

        [FieldDefinition(0, Label = "Webhook URL", HelpText = "Discord channel webhook url")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", HelpText = "The username to post as, defaults to Discord webhook default")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Avatar", HelpText = "Change the avatar that is used for messages from this integration", Type = FieldType.Textbox)]
        public string Avatar { get; set; }


        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
