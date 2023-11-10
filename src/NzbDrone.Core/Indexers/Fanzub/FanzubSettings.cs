using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubSettingsValidator : AbstractValidator<FanzubSettings>
    {
        public FanzubSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class FanzubSettings : IIndexerSettings
    {
        private static readonly FanzubSettingsValidator Validator = new FanzubSettingsValidator();

        public FanzubSettings()
        {
            BaseUrl = "http://fanzub.com/rss/";
        }

        [FieldDefinition(0, Label = "IndexerSettingsRssUrl", HelpText = "IndexerSettingsRssUrlHelpText")]
        [FieldToken(TokenField.HelpText, "IndexerSettingsRssUrl", "indexer", "Fanzub")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerSettingsAnimeStandardFormatSearch", Type = FieldType.Checkbox, HelpText = "IndexerSettingsAnimeStandardFormatSearchHelpText")]
        public bool AnimeStandardFormatSearch { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
