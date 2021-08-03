using System.Collections.Generic;
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
        public DiscordSettings()
        {
            //Set Default Fields
            GrabFields = new[] { 0, 1, 2, 3, 5, 6, 7, 8, 9 };
            ImportFields = new[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12 };
        }

        private static readonly DiscordSettingsValidator Validator = new DiscordSettingsValidator();

        [FieldDefinition(0, Label = "Webhook URL", HelpText = "Discord channel webhook url")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "The username to post as, defaults to Discord webhook default")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Avatar", HelpText = "Change the avatar that is used for messages from this integration", Type = FieldType.Textbox)]
        public string Avatar { get; set; }

        [FieldDefinition(3, Label = "Host", Advanced = true, HelpText = "Override the Host that shows for this notification, Blank is machine name", Type = FieldType.Textbox)]
        public string Author { get; set; }

        [FieldDefinition(4, Label = "On Grab Fields", Advanced = true, SelectOptions = typeof(DiscordGrabFieldType), HelpText = "Change the fields that are passed in for this 'on grab' notification", Type = FieldType.TagSelect)]
        public IEnumerable<int> GrabFields { get; set; }

        [FieldDefinition(5, Label = "On Import Fields", Advanced = true, SelectOptions = typeof(DiscordImportFieldType), HelpText = "Change the fields that are passed for this 'on import' notification", Type = FieldType.TagSelect)]
        public IEnumerable<int> ImportFields { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
