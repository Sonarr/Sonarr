using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public class TorrentRssIndexerSettingsValidator : AbstractValidator<TorrentRssIndexerSettings>
    {
        public TorrentRssIndexerSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());

            RuleFor(c => c.SearchPriority).InclusiveBetween(0, 100);
        }
    }

    public class TorrentRssIndexerSettings : ITorrentIndexerSettings
    {
        private static readonly TorrentRssIndexerSettingsValidator validator = new TorrentRssIndexerSettingsValidator();

        public TorrentRssIndexerSettings()
        {
            BaseUrl = string.Empty;
            AllowZeroSize = false;
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            SearchPriority = IndexerDefaults.SEARCH_PRIORITY;
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Cookie", HelpText = "If you site requires a login cookie to access the rss, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        [FieldDefinition(2, Type = FieldType.Checkbox, Label = "Allow Zero Size", HelpText="Enabling this will allow you to use feeds that don't specify release size, but be careful, size related checks will not be performed.")]
        public bool AllowZeroSize { get; set; }

        [FieldDefinition(3, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(4)]
        public SeedCriteriaSettings SeedCriteria { get; } = new SeedCriteriaSettings();

        [FieldDefinition(5, Type = FieldType.Number, Label = "Search Priority", HelpText = "Search Priority from 0 (Highest) to 100 (Lowest). Default: 100.")]
        public int SearchPriority { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(validator.Validate(this));
        }
    }
}
