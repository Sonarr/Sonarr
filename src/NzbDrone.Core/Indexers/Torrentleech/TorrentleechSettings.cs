using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class TorrentleechSettingsValidator : AbstractValidator<TorrentleechSettings>
    {
        public TorrentleechSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class TorrentleechSettings : IProviderConfig
    {
        private static readonly TorrentleechSettingsValidator validator = new TorrentleechSettingsValidator();

        public TorrentleechSettings()
        {
            BaseUrl = "http://rss.torrentleech.org";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public String ApiKey { get; set; }

        public ValidationResult Validate()
        {
            return validator.Validate(this);
        }
    }
}