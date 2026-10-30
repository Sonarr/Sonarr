using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers
{
    public class SeedCriteriaSettingsValidator : AbstractValidator<SeedCriteriaSettings>
    {
        public SeedCriteriaSettingsValidator(double seedRatioMinimum = 0.0, int seedTimeMinimum = 0, int seasonPackSeedTimeMinimum = 0)
        {
            RuleFor(c => c.SeedRatio).GreaterThan(0.0)
                .When(c => c.SeedRatio.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.SeedTime).GreaterThan(0)
                .When(c => c.SeedTime.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.SeasonPackSeedRatio).GreaterThan(0.0)
                .When(c => c.SeasonPackSeedRatio.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            RuleFor(c => c.SeasonPackSeedTime).GreaterThan(0)
                .When(c => c.SeasonPackSeedTime.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            if (seedRatioMinimum != 0.0)
            {
                RuleFor(c => c.SeedRatio).GreaterThanOrEqualTo(seedRatioMinimum)
                    .When(c => c.SeedRatio > 0.0)
                    .AsWarning()
                    .WithMessage($"Under {seedRatioMinimum} leads to H&R");

                RuleFor(c => c.SeasonPackSeedRatio).GreaterThanOrEqualTo(seedRatioMinimum)
                    .When(c => c.SeasonPackSeedRatio > 0.0)
                    .AsWarning()
                    .WithMessage($"Under {seedRatioMinimum} leads to H&R");
            }

            if (seedTimeMinimum != 0)
            {
                RuleFor(c => c.SeedTime).GreaterThanOrEqualTo(seedTimeMinimum)
                    .When(c => c.SeedTime > 0)
                    .AsWarning()
                    .WithMessage($"Under {seedTimeMinimum} leads to H&R");
            }

            if (seasonPackSeedTimeMinimum != 0)
            {
                RuleFor(c => c.SeasonPackSeedTime).GreaterThanOrEqualTo(seasonPackSeedTimeMinimum)
                    .When(c => c.SeasonPackSeedTime > 0)
                    .AsWarning()
                    .WithMessage($"Under {seasonPackSeedTimeMinimum} leads to H&R");
            }
        }
    }

    public class SeedCriteriaSettings : PropertywiseEquatable<SeedCriteriaSettings>
    {
        [FieldDefinition(0, Type = FieldType.Number, Label = "IndexerSettingsSeedRatio", HelpText = "IndexerSettingsSeedRatioHelpText")]
        public double? SeedRatio { get; set; }

        [FieldDefinition(1, Type = FieldType.Number, Label = "IndexerSettingsSeedTime", Unit = "minutes", HelpText = "IndexerSettingsSeedTimeHelpText")]
        public int? SeedTime { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, Label = "IndexerSettingsSeasonPackSeedGoal", SelectOptions = typeof(SeasonPackSeedGoal), HelpText = "IndexerSettingsSeasonPackSeedGoalHelpText")]
        public int SeasonPackSeedGoal { get; set; }

        [FieldDefinition(3, Type = FieldType.Number, Label = "IndexerSettingsSeasonPackSeedRatio", HelpText = "IndexerSettingsSeasonPackSeedRatioHelpText")]
        public double? SeasonPackSeedRatio { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "IndexerSettingsSeasonPackSeedTime", Unit = "minutes", HelpText = "IndexerSettingsSeasonPackSeedTimeHelpText")]
        public int? SeasonPackSeedTime { get; set; }
    }
}
