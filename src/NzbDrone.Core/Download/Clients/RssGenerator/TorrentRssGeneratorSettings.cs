using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.RssGenerator
{
    public class TorrentRssGeneratorSettingsValidator : AbstractValidator<TorrentRssGeneratorSettings>
    {
        public TorrentRssGeneratorSettingsValidator()
        {
            this.RuleFor(c => c.IncompleteFolder).IsValidPath().When(x => x.IncompleteFolder.IsNotNullOrWhiteSpace());
            this.RuleFor(c => c.WatchFolder).IsValidPath().When(x => x.WatchFolder.IsNotNullOrWhiteSpace());
        }
    }

    public class TorrentRssGeneratorSettings : IProviderConfig
    {
        public TorrentRssGeneratorSettings()
        {
            this.ReadOnly = true;
        }

        private static readonly TorrentRssGeneratorSettingsValidator Validator = new TorrentRssGeneratorSettingsValidator();

        [FieldDefinition(0, Label = "Incomplete Folder", Type = FieldType.Path, HelpText = "Folder in which Sonarr will try and match ongoing downloads for progress. This may not always work 100% - best effort and all that.")]
        public string IncompleteFolder { get; set; }

        [FieldDefinition(1, Label = "Watch Folder", Type = FieldType.Path, HelpText = "Folder from which Sonarr should import completed downloads")]
        public string WatchFolder { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [FieldDefinition(2, Label = "Read Only", Type = FieldType.Checkbox, HelpText = "Instead of moving files this will instruct Sonarr to Copy or Hardlink (depending on settings/system configuration)")]
        public bool ReadOnly { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
