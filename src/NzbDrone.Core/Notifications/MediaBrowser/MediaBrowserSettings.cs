using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserSettings>
    {
        public MediaBrowserSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.UrlBase).ActuallyValidUrlBase();
        }
    }

    public class MediaBrowserSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; } = 8096;

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Emby over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the Emby server url, see http://[host]:[port]/[urlBase]")]
        public string UrlBase { get; set; } = "";

        [FieldDefinition(4, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(5, Label = "Send Notifications", HelpText = "Have MediaBrowser send notfications to configured providers", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(6, Label = "Update Library", HelpText = "Update Library on Import & Rename?", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [JsonIgnore]
        public string Address => $"{(UseSsl ? "https" : "http")}://{Host}:{Port}{(String.IsNullOrEmpty(UrlBase) ? "" : $"/{UrlBase}")}";

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) && Port > 0;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
