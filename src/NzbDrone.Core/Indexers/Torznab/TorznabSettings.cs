using System;
using System.Linq;
using System.Text.RegularExpressions;
using Equ;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabSettingsValidator : AbstractValidator<TorznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList = Array.Empty<string>();

        private static bool ShouldHaveApiKey(TorznabSettings settings)
        {
            return settings.BaseUrl != null && ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new (@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public TorznabSettingsValidator()
        {
            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.Categories.Empty() && c.AnimeCategories.Empty())
                {
                    context.AddFailure("Either 'Categories' or 'Anime Categories' must be provided");
                }
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class TorznabSettings : NewznabSettings, ITorrentIndexerSettings, IEquatable<TorznabSettings>
    {
        private static readonly TorznabSettingsValidator Validator = new ();

        private static readonly MemberwiseEqualityComparer<TorznabSettings> Comparer = MemberwiseEqualityComparer<TorznabSettings>.ByProperties;

        public TorznabSettings()
        {
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(9, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(10)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(11, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }

        public bool Equals(TorznabSettings other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TorznabSettings);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }
}
