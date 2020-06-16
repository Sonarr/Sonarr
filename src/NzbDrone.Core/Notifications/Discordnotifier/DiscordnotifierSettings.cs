using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discordnotifier
{
    public class DiscordnotifierSettingsValidator : AbstractValidator<DiscordnotifierSettings>
    {
        public DiscordnotifierSettingsValidator()
        {
            RuleFor(c => c.APIKey).NotEmpty();
        }
    }

    public class DiscordnotifierSettings : IProviderConfig
    {
        private static readonly DiscordnotifierSettingsValidator Validator = new DiscordnotifierSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpText = "Your API key from your profile", HelpLink = "https://discordnotifier.com")]
        public string APIKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
