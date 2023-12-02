using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserSettings>
    {
        public MediaBrowserSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.MapFrom).NotEmpty().Unless(c => c.MapTo.IsNullOrWhiteSpace());
            RuleFor(c => c.MapTo).NotEmpty().Unless(c => c.MapFrom.IsNullOrWhiteSpace());
        }
    }

    public class MediaBrowserSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        public MediaBrowserSettings()
        {
            Port = 8096;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "NotificationsSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "serviceName", "Emby/Jellyfin")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "NotificationsEmbySettingsSendNotifications", HelpText = "NotificationsEmbySettingsSendNotificationsHelpText", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(5, Label = "NotificationsSettingsUpdateLibrary", HelpText = "NotificationsEmbySettingsUpdateLibraryHelpText", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(6, Label = "NotificationsSettingsUpdateMapPathsFrom", HelpText = "NotificationsSettingsUpdateMapPathsFromHelpText", Type = FieldType.Textbox, Advanced = true)]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsFrom", "serviceName", "Emby/Jellyfin")]
        public string MapFrom { get; set; }

        [FieldDefinition(7, Label = "NotificationsSettingsUpdateMapPathsTo", HelpText = "NotificationsSettingsUpdateMapPathsToHelpText", Type = FieldType.Textbox, Advanced = true)]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsTo", "serviceName", "Emby/Jellyfin")]
        public string MapTo { get; set; }

        [JsonIgnore]
        public string Address => $"{Host.ToUrlHost()}:{Port}";

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) && Port > 0;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
