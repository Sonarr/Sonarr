using System.ComponentModel;
using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Core.Annotations;
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

    public class TorrentBlackholeSettings : DownloadClientSettingsBase<TorrentBlackholeSettings>
    {
        public TorrentBlackholeSettings()
        {
            MagnetFileExtension = ".magnet";
            ReadOnly = true;
        }

        private static readonly TorrentBlackholeSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "TorrentBlackholeTorrentFolder", Type = FieldType.Path, HelpText = "BlackholeFolderHelpText")]
        [FieldToken(TokenField.HelpText, "TorrentBlackholeTorrentFolder", "extension", ".torrent")]
        public string TorrentFolder { get; set; }

        [FieldDefinition(1, Label = "BlackholeWatchFolder", Type = FieldType.Path, HelpText = "BlackholeWatchFolderHelpText")]
        public string WatchFolder { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [FieldDefinition(2, Label = "TorrentBlackholeSaveMagnetFiles", Type = FieldType.Checkbox, HelpText = "TorrentBlackholeSaveMagnetFilesHelpText")]
        public bool SaveMagnetFiles { get; set; }

        [FieldDefinition(3, Label = "TorrentBlackholeSaveMagnetFilesExtension", Type = FieldType.Textbox, HelpText = "TorrentBlackholeSaveMagnetFilesExtensionHelpText")]
        public string MagnetFileExtension { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [FieldDefinition(4, Label = "TorrentBlackholeSaveMagnetFilesReadOnly", Type = FieldType.Checkbox, HelpText = "TorrentBlackholeSaveMagnetFilesReadOnlyHelpText")]
        public bool ReadOnly { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
