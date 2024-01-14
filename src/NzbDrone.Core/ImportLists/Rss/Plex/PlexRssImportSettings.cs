using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Rss.Plex
{
    public class PlexRssImportSettingsValidator : AbstractValidator<PlexRssImportSettings>
    {
        public PlexRssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class PlexRssImportSettings : RssImportBaseSettings
    {
        private PlexRssImportSettingsValidator Validator => new ();

        [FieldDefinition(0, Label = "ImportListsSettingsRssUrl", Type = FieldType.Textbox, HelpLink = "https://app.plex.tv/desktop/#!/settings/watchlist")]
        public override string Url { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
