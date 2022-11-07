using System.ComponentModel;
using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public class TorrentBlackholeSettingsValidator : AbstractValidator<TorrentBlackholeSettings>
    {
        public TorrentBlackholeSettingsValidator()
        {
            // Todo: Validate that the path actually exists
            RuleFor(c => c.TorrentFolder).IsValidPath();
            RuleFor(c => c.MagnetFileExtension).NotEmpty();
        }
    }

    public class TorrentBlackholeSettings : IProviderConfig
    {
        public TorrentBlackholeSettings()
        {
            MagnetFileExtension = ".magnet";
            ReadOnly = true;
        }

        private static readonly TorrentBlackholeSettingsValidator Validator = new TorrentBlackholeSettingsValidator();

        [FieldDefinition(0, Label = "Torrent Folder", Type = FieldType.Path, HelpText = "Folder in which Sonarr will store the .torrent file")]
        public string TorrentFolder { get; set; }

        [FieldDefinition(1, Label = "Watch Folder", Type = FieldType.Path, HelpText = "Folder from which Sonarr should import completed downloads")]
        public string WatchFolder { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [FieldDefinition(2, Label = "Save Magnet Files", Type = FieldType.Checkbox, HelpText = "Save the magnet link if no .torrent file is available (only useful if the download client supports magnets saved to a file)")]
        public bool SaveMagnetFiles { get; set; }

        [FieldDefinition(3, Label = "Save Magnet Files", Type = FieldType.Textbox, HelpText = "Extension to use for magnet links, defaults to '.magnet'")]
        public string MagnetFileExtension { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [FieldDefinition(4, Label = "Read Only", Type = FieldType.Checkbox, HelpText = "Instead of moving files this will instruct Sonarr to Copy or Hardlink (depending on settings/system configuration)")]
        public bool ReadOnly { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
