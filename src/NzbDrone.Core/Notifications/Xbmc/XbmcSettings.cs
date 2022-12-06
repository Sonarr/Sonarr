using System.ComponentModel;
using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcSettings>
    {
        public XbmcSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.DisplayTime).GreaterThanOrEqualTo(2);
        }
    }

    public class XbmcSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcSettings()
        {
            Port = 8080;
            DisplayTime = 5;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Kodi over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [DefaultValue(5)]
        [FieldDefinition(5, Label = "Display Time", HelpText = "How long the notification will be displayed for (In seconds)")]
        public int DisplayTime { get; set; }

        [FieldDefinition(6, Label = "GUI Notification", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(7, Label = "Update Library", HelpText = "Update Library on Import & Rename?", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(8, Label = "Clean Library", HelpText = "Clean Library after update?", Type = FieldType.Checkbox)]
        public bool CleanLibrary { get; set; }

        [FieldDefinition(9, Label = "Always Update", HelpText = "Update Library even when a video is playing?", Type = FieldType.Checkbox)]
        public bool AlwaysUpdate { get; set; }

        [JsonIgnore]
        public string Address => $"{Host.ToUrlHost()}:{Port}";

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
