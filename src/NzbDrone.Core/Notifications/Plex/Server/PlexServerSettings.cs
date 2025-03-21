using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexServerSettingsValidator : AbstractValidator<PlexServerSettings>
    {
        public PlexServerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.MapFrom).NotEmpty().Unless(c => c.MapTo.IsNullOrWhiteSpace());
            RuleFor(c => c.MapTo).NotEmpty().Unless(c => c.MapFrom.IsNullOrWhiteSpace());
        }
    }

    public class PlexServerSettings : NotificationSettingsBase<PlexServerSettings>
    {
        private static readonly PlexServerSettingsValidator Validator = new();

        public PlexServerSettings()
        {
            Host = "";
            Port = 32400;
            UpdateLibrary = true;
            SignIn = "startOAuth";
        }

        [JsonIgnore]
        [FieldDefinition(0, Label = "NotificationsPlexSettingsServer", Type = FieldType.Select, SelectOptionsProviderAction = "servers", HelpText = "NotificationsPlexSettingsServerHelpText")]
        public string Server { get; set; }

        [FieldDefinition(1, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(2, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(3, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "NotificationsSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "serviceName", "Plex")]
        public bool UseSsl { get; set; }

        [FieldDefinition(4, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "ConnectionSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "connectionName", "Plex")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]/plex")]
        public string UrlBase { get; set; }

        [FieldDefinition(5, Label = "NotificationsPlexSettingsAuthToken", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, Advanced = true)]
        public string AuthToken { get; set; }

        [FieldDefinition(6, Label = "NotificationsPlexSettingsAuthenticateWithPlexTv", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        [FieldDefinition(7, Label = "NotificationsSettingsUpdateLibrary", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(8, Label = "NotificationsSettingsUpdateMapPathsFrom", Type = FieldType.Textbox, Advanced = true, HelpText = "NotificationsSettingsUpdateMapPathsFromSeriesHelpText")]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsFrom", "serviceName", "Plex")]
        public string MapFrom { get; set; }

        [FieldDefinition(9, Label = "NotificationsSettingsUpdateMapPathsTo", Type = FieldType.Textbox, Advanced = true, HelpText = "NotificationsSettingsUpdateMapPathsToSeriesHelpText")]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsTo", "serviceName", "Plex")]
        public string MapTo { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Host);

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
