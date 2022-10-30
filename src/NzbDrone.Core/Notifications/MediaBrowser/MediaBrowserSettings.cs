﻿using FluentValidation;
using Newtonsoft.Json;
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

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Emby over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Send Notifications", HelpText = "Have MediaBrowser send notifications to configured providers", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(5, Label = "Update Library", HelpText = "Update Library on Import, Rename, or Delete?", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [JsonIgnore]
        public string Address => $"{Host}:{Port}";

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) && Port > 0;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
