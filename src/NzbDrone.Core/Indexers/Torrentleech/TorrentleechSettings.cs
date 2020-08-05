using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechSettingsValidator : AbstractValidator<TorrentleechSettings>
    {
        public TorrentleechSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());

            RuleFor(c => c.SearchPriority).InclusiveBetween(0, 100);
        }
    }

    public class TorrentleechSettings : ITorrentIndexerSettings
    {
        private static readonly TorrentleechSettingsValidator Validator = new TorrentleechSettingsValidator();

        public TorrentleechSettings()
        {
            BaseUrl = "http://rss.torrentleech.org";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            SearchPriority = IndexerDefaults.SEARCH_PRIORITY;
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(3)]
        public SeedCriteriaSettings SeedCriteria { get; } = new SeedCriteriaSettings();

        [FieldDefinition(4, Type = FieldType.Number, Label = "Search Priority", HelpText = "Search Priority from 0 (Highest) to 100 (Lowest). Default: 100.")]
        public int SearchPriority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
