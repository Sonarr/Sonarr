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
            // Set Default Fields
            GrabFields = new[]
            {
                (int)DiscordGrabFieldType.Overview,
                (int)DiscordGrabFieldType.Rating,
                (int)DiscordGrabFieldType.Genres,
                (int)DiscordGrabFieldType.Quality,
                (int)DiscordGrabFieldType.Group,
                (int)DiscordGrabFieldType.Size,
                (int)DiscordGrabFieldType.Links,
                (int)DiscordGrabFieldType.Release,
                (int)DiscordGrabFieldType.Poster,
                (int)DiscordGrabFieldType.Fanart,
                (int)DiscordGrabFieldType.Indexer,
                (int)DiscordGrabFieldType.CustomFormats,
                (int)DiscordGrabFieldType.CustomFormatScore
            };
            ImportFields = new[]
            {
                (int)DiscordImportFieldType.Overview,
                (int)DiscordImportFieldType.Rating,
                (int)DiscordImportFieldType.Genres,
                (int)DiscordImportFieldType.Quality,
                (int)DiscordImportFieldType.Codecs,
                (int)DiscordImportFieldType.Group,
                (int)DiscordImportFieldType.Size,
                (int)DiscordImportFieldType.Languages,
                (int)DiscordImportFieldType.Subtitles,
                (int)DiscordImportFieldType.Links,
                (int)DiscordImportFieldType.Release,
                (int)DiscordImportFieldType.Poster,
                (int)DiscordImportFieldType.Fanart
            };
            ManualInteractionFields = new[]
            {
                (int)DiscordManualInteractionFieldType.Overview,
                (int)DiscordManualInteractionFieldType.Rating,
                (int)DiscordManualInteractionFieldType.Genres,
                (int)DiscordManualInteractionFieldType.Quality,
                (int)DiscordManualInteractionFieldType.Group,
                (int)DiscordManualInteractionFieldType.Size,
                (int)DiscordManualInteractionFieldType.Links,
                (int)DiscordManualInteractionFieldType.DownloadTitle,
                (int)DiscordManualInteractionFieldType.Poster,
                (int)DiscordManualInteractionFieldType.Fanart
            };
        }

        private static readonly DiscordSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "NotificationsSettingsWebhookUrl", HelpText = "NotificationsDiscordSettingsWebhookUrlHelpText")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "NotificationsDiscordSettingsUsernameHelpText")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "NotificationsDiscordSettingsAvatar", HelpText = "NotificationsDiscordSettingsAvatarHelpText", Type = FieldType.Textbox)]
        public string Avatar { get; set; }

        [FieldDefinition(3, Label = "NotificationsDiscordSettingsAuthor", Advanced = true, HelpText = "NotificationsDiscordSettingsAuthorHelpText", Type = FieldType.Textbox)]
        public string Author { get; set; }

        [FieldDefinition(4, Label = "NotificationsDiscordSettingsOnGrabFields", Advanced = true, SelectOptions = typeof(DiscordGrabFieldType), HelpText = "NotificationsDiscordSettingsOnGrabFieldsHelpText", Type = FieldType.Select)]
        public IEnumerable<int> GrabFields { get; set; }

        [FieldDefinition(5, Label = "NotificationsDiscordSettingsOnImportFields", Advanced = true, SelectOptions = typeof(DiscordImportFieldType), HelpText = "NotificationsDiscordSettingsOnImportFieldsHelpText", Type = FieldType.Select)]
        public IEnumerable<int> ImportFields { get; set; }

        [FieldDefinition(6, Label = "NotificationsDiscordSettingsOnManualInteractionFields", Advanced = true, SelectOptions = typeof(DiscordManualInteractionFieldType), HelpText = "NotificationsDiscordSettingsOnManualInteractionFieldsHelpText", Type = FieldType.Select)]
        public IEnumerable<int> ManualInteractionFields { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
