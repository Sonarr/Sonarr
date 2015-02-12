using System;
using FluentValidation;
using FluentValidation.Results;
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
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public String BaseUrl { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}