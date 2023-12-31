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

        [FieldDefinition(0, Label = "Webhook URL", HelpText = "Discord channel webhook url")]
        public string WebHookUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "The username to post as, defaults to Discord webhook default")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Avatar", HelpText = "Change the avatar that is used for messages from this integration", Type = FieldType.Textbox)]
        public string Avatar { get; set; }

        [FieldDefinition(3, Label = "Author", Advanced = true, HelpText = "Override the embed author that shows for this notification, Blank is instance name", Type = FieldType.Textbox)]
        public string Author { get; set; }

        [FieldDefinition(4, Label = "On Grab Fields", Advanced = true, SelectOptions = typeof(DiscordGrabFieldType), HelpText = "Change the fields that are passed in for this 'on grab' notification", Type = FieldType.Select)]
        public IEnumerable<int> GrabFields { get; set; }

        [FieldDefinition(5, Label = "On Import Fields", Advanced = true, SelectOptions = typeof(DiscordImportFieldType), HelpText = "Change the fields that are passed for this 'on import' notification", Type = FieldType.Select)]
        public IEnumerable<int> ImportFields { get; set; }

        [FieldDefinition(6, Label = "On Manual Interaction Fields", Advanced = true, SelectOptions = typeof(DiscordManualInteractionFieldType), HelpText = "Change the fields that are passed for this 'on manual interaction' notification", Type = FieldType.Select)]
        public IEnumerable<int> ManualInteractionFields { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
