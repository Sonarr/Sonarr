using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssIndexerSettingsValidator : AbstractValidator<TorrentRssIndexerSettings>
    {
        public TorrentRssIndexerSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class TorrentRssIndexerSettings : IProviderConfig
    {
        private static readonly TorrentRssIndexerSettingsValidator validator = new TorrentRssIndexerSettingsValidator();

        public TorrentRssIndexerSettings()
        {
            BaseUrl = string.Empty;
            ValidEntryPercentage = 75;
            ValidSizeThresholdMegabytes = 12;
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public String BaseUrl { get; set; }

        [FieldDefinition(1, Advanced = true, Label = "Valid Entry Percentage", HelpText = "Percentage of entries in the RSS feed which need to be parseable")]
        public int ValidEntryPercentage { get; set; }

        [FieldDefinition(2, Advanced = true, Label = "Valid Size Threshold (MB)", HelpText = "Threshold of Parsed Torrent Size to consider it valid")]
        public int ValidSizeThresholdMegabytes { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(validator.Validate(this));
        }
    }
}