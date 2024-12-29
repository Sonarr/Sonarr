using FluentValidation;
using Workarr.Annotations;
using Workarr.Validation;

namespace Workarr.ImportLists.Rss.Plex
{
    public class PlexRssImportSettingsValidator : AbstractValidator<PlexRssImportSettings>
    {
        public PlexRssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class PlexRssImportSettings : RssImportBaseSettings<PlexRssImportSettings>
    {
        private static readonly PlexRssImportSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "ImportListsSettingsRssUrl", Type = FieldType.Textbox, HelpLink = "https://app.plex.tv/desktop/#!/settings/watchlist")]
        public override string Url { get; set; }

        public override WorkarrValidationResult Validate()
        {
            return new WorkarrValidationResult(Validator.Validate(this));
        }
    }
}
