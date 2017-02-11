using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            RuleFor(c => c.SeasonPackSeedTime).GreaterThan(0)
                .When(c => c.SeasonPackSeedTime.HasValue)
                .AsWarning().WithMessage("Should be greater than zero");

            if (seedRatioMinimum != 0.0)
            {
                RuleFor(c => c.SeedRatio).GreaterThanOrEqualTo(seedRatioMinimum)
                    .When(c => c.SeedRatio > 0.0)
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

    public class SeedCriteriaSettings
    {
        private static readonly SeedCriteriaSettingsValidator Validator = new SeedCriteriaSettingsValidator();

        [FieldDefinition(0, Type = FieldType.Textbox, Label = "Seed Ratio", HelpText = "The ratio a torrent should reach before stopping, empty is download client's default", Advanced = true)]
        public double? SeedRatio { get; set; }

        [FieldDefinition(1, Type = FieldType.Number, Label = "Seed Time", Unit = "minutes", HelpText = "The time a torrent should be seeded before stopping, empty is download client's default", Advanced = true)]
        public int? SeedTime { get; set; }

        [FieldDefinition(2, Type = FieldType.Number, Label = "Season-Pack Seed Time", Unit = "minutes", HelpText = "The time a torrent should be seeded before stopping, empty is download client's default", Advanced = true)]
        public int? SeasonPackSeedTime { get; set; }
    }
}
