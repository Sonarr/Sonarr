using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaSettingsValidator : AbstractValidator<NyaaSettings>
    {
        public NyaaSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.AdditionalParameters).Matches("(&[a-z]+=[a-z0-9_]+)*", RegexOptions.IgnoreCase);

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());

            RuleFor(c => c.SearchPriority).InclusiveBetween(0, 100);
        }
    }

    public class NyaaSettings : ITorrentIndexerSettings
    {
        private static readonly NyaaSettingsValidator Validator = new NyaaSettingsValidator();

        public NyaaSettings()
        {
            BaseUrl = "";
            AdditionalParameters = "&cats=1_37&filter=1";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            SearchPriority = IndexerDefaults.SEARCH_PRIORITY;
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Additional Parameters", Advanced = true, HelpText = "Please note if you change the category you will have to add required/restricted rules about the subgroups to avoid foreign language releases.")]
        public string AdditionalParameters { get; set; }

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
