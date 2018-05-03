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
        public SeedCriteriaSettingsValidator()
        {
            RuleFor(c => c.SeedRatio).GreaterThan(0.0).When(c => c.SeedRatio.HasValue);
            RuleFor(c => c.SeedTime).GreaterThan(0).When(c => c.SeedTime.HasValue);
            RuleFor(c => c.SeasonPackSeedTime).GreaterThan(0).When(c => c.SeasonPackSeedTime.HasValue);
        }
    }

    public class SeedCriteriaSettings
    {
        private static readonly SeedCriteriaSettingsValidator Validator = new SeedCriteriaSettingsValidator();

        [FieldDefinition(0, Type = FieldType.Textbox, Label = "Seed Ratio", HelpText = "The ratio a torrent should reach before stopping, empty is download client's default", Advanced = true)]
        public double? SeedRatio { get; set; }

        [FieldDefinition(1, Type = FieldType.Textbox, Label = "Seed Time", HelpText = "The time a torrent should be seeded before stopping, empty is download client's default", Advanced = true)]
        public int? SeedTime { get; set; }

        [FieldDefinition(2, Type = FieldType.Textbox, Label = "Season-Pack Seed Time", HelpText = "The time a torrent should be seeded before stopping, empty is download client's default", Advanced = true)]
        public int? SeasonPackSeedTime { get; set; }
    }
}
