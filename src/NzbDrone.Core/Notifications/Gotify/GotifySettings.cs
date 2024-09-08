using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Gotify
{
    public class GotifySettingsValidator : AbstractValidator<GotifySettings>
    {
        public GotifySettingsValidator()
        {
            RuleFor(c => c.Server).IsValidUrl();
            RuleFor(c => c.AppToken).NotEmpty();

            RuleFor(c => c.MetadataLinks).Custom((links, context) =>
            {
                foreach (var link in links)
                {
                    if (!Enum.IsDefined(typeof(MetadataLinkType), link))
                    {
                        context.AddFailure("MetadataLinks", $"MetadataLink is not valid: {link}");
                    }
                }
            });

            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.MetadataLinks.Empty())
                {
                    return;
                }

                if (!c.MetadataLinks.Contains(c.PreferredMetadataLink))
                {
                    context.AddFailure("PreferredMetadataLink", "Must be a selected link");
                }
            });
        }
    }

    public class GotifySettings : NotificationSettingsBase<GotifySettings>
    {
        private static readonly GotifySettingsValidator Validator = new ();

        public GotifySettings()
        {
            Priority = 5;
            MetadataLinks = Enumerable.Empty<int>();
            PreferredMetadataLink = (int)MetadataLinkType.Tvdb;
        }

        [FieldDefinition(0, Label = "NotificationsGotifySettingsServer", HelpText = "NotificationsGotifySettingsServerHelpText")]
        public string Server { get; set; }

        [FieldDefinition(1, Label = "NotificationsGotifySettingsAppToken", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsGotifySettingsAppTokenHelpText")]
        public string AppToken { get; set; }

        [FieldDefinition(2, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(GotifyPriority), HelpText = "NotificationsGotifySettingsPriorityHelpText")]
        public int Priority { get; set; }

        [FieldDefinition(3, Label = "NotificationsGotifySettingIncludeSeriesPoster", Type = FieldType.Checkbox, HelpText = "NotificationsGotifySettingIncludeSeriesPosterHelpText")]
        public bool IncludeSeriesPoster { get; set; }

        [FieldDefinition(4, Label = "NotificationsGotifySettingsMetadataLinks", Type = FieldType.Select, SelectOptions = typeof(MetadataLinkType), HelpText = "NotificationsGotifySettingsMetadataLinksHelpText")]
        public IEnumerable<int> MetadataLinks { get; set; }

        [FieldDefinition(5, Label = "NotificationsGotifySettingsPreferredMetadataLink", Type = FieldType.Select, SelectOptions = typeof(MetadataLinkType), HelpText = "NotificationsGotifySettingsPreferredMetadataLinkHelpText")]
        public int PreferredMetadataLink { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
