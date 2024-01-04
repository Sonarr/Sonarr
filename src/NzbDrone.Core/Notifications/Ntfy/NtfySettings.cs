using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Ntfy
{
    public class NtfySettingsValidator : AbstractValidator<NtfySettings>
    {
        public NtfySettingsValidator()
        {
            RuleFor(c => c.Topics).NotEmpty();
            RuleFor(c => c.Priority).InclusiveBetween(1, 5);
            RuleFor(c => c.ServerUrl).IsValidUrl().When(c => !c.ServerUrl.IsNullOrWhiteSpace());
            RuleFor(c => c.ClickUrl).IsValidUrl().When(c => !c.ClickUrl.IsNullOrWhiteSpace());
            RuleFor(c => c.UserName).NotEmpty().When(c => !c.Password.IsNullOrWhiteSpace() && c.AccessToken.IsNullOrWhiteSpace());
            RuleFor(c => c.Password).NotEmpty().When(c => !c.UserName.IsNullOrWhiteSpace() && c.AccessToken.IsNullOrWhiteSpace());
            RuleForEach(c => c.Topics).NotEmpty().Matches("[a-zA-Z0-9_-]+").Must(c => !InvalidTopics.Contains(c)).WithMessage("Invalid topic");
        }

        private static List<string> InvalidTopics => new List<string> { "announcements", "app", "docs", "settings", "stats", "mytopic-rw", "mytopic-ro", "mytopic-wo" };
    }

    public class NtfySettings : IProviderConfig
    {
        private static readonly NtfySettingsValidator Validator = new NtfySettingsValidator();

        public NtfySettings()
        {
            Topics = Array.Empty<string>();
            Priority = 3;
        }

        [FieldDefinition(0, Label = "NotificationsNtfySettingsServerUrl", Type = FieldType.Url, HelpLink = "https://ntfy.sh/docs/install/", HelpText = "NotificationsNtfySettingsServerUrlHelpText")]
        [FieldToken(TokenField.HelpText, "NotificationsNtfySettingsServerUrl", "url", "https://ntfy.sh")]
        public string ServerUrl { get; set; }

        [FieldDefinition(1, Label = "NotificationsNtfySettingsAccessToken", Type = FieldType.Password, Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsNtfySettingsAccessTokenHelpText", HelpLink = "https://docs.ntfy.sh/config/#access-tokens")]
        public string AccessToken { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "NotificationsNtfySettingsUsernameHelpText", Privacy = PrivacyLevel.UserName)]
        public string UserName { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password, HelpText = "NotificationsNtfySettingsPasswordHelpText", Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(NtfyPriority))]
        public int Priority { get; set; }

        [FieldDefinition(5, Label = "NotificationsNtfySettingsTopics", HelpText = "NotificationsNtfySettingsTopicsHelpText", Type = FieldType.Tag)]
        public IEnumerable<string> Topics { get; set; }

        [FieldDefinition(6, Label = "NotificationsNtfySettingsTagsEmojis", Type = FieldType.Tag, HelpText = "NotificationsNtfySettingsTagsEmojisHelpText", HelpLink = "https://ntfy.sh/docs/emojis/")]
        public IEnumerable<string> Tags { get; set; }

        [FieldDefinition(7, Label = "NotificationsNtfySettingsClickUrl", Type = FieldType.Url, HelpText = "NotificationsNtfySettingsClickUrlHelpText")]
        public string ClickUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
