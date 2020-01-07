using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNetSettingsValidator : AbstractValidator<BroadcastheNetSettings>
    {
        public BroadcastheNetSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator(1.0, 24*60, 5*24*60));
        }
    }

    public class BroadcastheNetSettings : ITorrentIndexerSettings
    {
        private static readonly BroadcastheNetSettingsValidator Validator = new BroadcastheNetSettingsValidator();

        public BroadcastheNetSettings()
        {
            BaseUrl = "http://api.broadcasthe.net/";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            IncludeSceneReleases = true;
            IncludeP2PReleases = true;
            IncludeUserReleases = true;
            IncludeInternalReleases = true;
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(3)]
        public SeedCriteriaSettings SeedCriteria { get; } = new SeedCriteriaSettings();

        [FieldDefinition(4, Type = FieldType.Checkbox, Label = "Include Scene Releases", Advanced = true, HelpText = "Include releases with Scene origin in results")]
        public bool IncludeSceneReleases { get; set; }

        [FieldDefinition(5, Type = FieldType.Checkbox, Label = "Include P2P Releases", Advanced = true, HelpText = "Include releases with P2P origin in results")]
        public bool IncludeP2PReleases { get; set; }

        [FieldDefinition(6, Type = FieldType.Checkbox, Label = "Include User Releases", Advanced = true, HelpText = "Include releases with User origin in results")]
        public bool IncludeUserReleases { get; set; }

        [FieldDefinition(7, Type = FieldType.Checkbox, Label = "Include Internal Releases", Advanced = true, HelpText = "Include releases with Internal origin in results")]
        public bool IncludeInternalReleases { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
