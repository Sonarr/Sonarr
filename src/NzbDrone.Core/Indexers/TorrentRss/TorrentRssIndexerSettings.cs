﻿using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public class TorrentRssIndexerSettingsValidator : AbstractValidator<TorrentRssIndexerSettings>
    {
        public TorrentRssIndexerSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();

            RuleFor(c => c.UiSeedRatio)
                .Must(c => c.IsNullOrWhiteSpace() || double.TryParse(c, out var _))
                .WithMessage("Seed ratio must be a valid decimal number");
        }
    }

    public class TorrentRssIndexerSettings : ITorrentIndexerSettings
    {
        private static readonly TorrentRssIndexerSettingsValidator validator = new TorrentRssIndexerSettingsValidator();

        public double SeedRatio { get; set; }

        public TorrentRssIndexerSettings()
        {
            BaseUrl = string.Empty;
            AllowZeroSize = false;
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Cookie", HelpText = "If you site requires a login cookie to access the rss, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        [FieldDefinition(2, Type = FieldType.Checkbox, Label = "Allow Zero Size", HelpText="Enabling this will allow you to use feeds that don't specify release size, but be careful, size related checks will not be performed.")]
        public bool AllowZeroSize { get; set; }

        [FieldDefinition(3, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(4, Type = FieldType.Textbox, Label = "Seed Ratio", HelpText = "The ratio a torrent should reach before stopping, empty is download client's default")]
        public string UiSeedRatio
        {
            get => SeedRatio.ToString();
            set => SeedRatio = double.Parse(value);
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(validator.Validate(this));
        }
    }
}
