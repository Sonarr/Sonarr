using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioSettingsValidator : AbstractValidator<PutioSettings>
    {
        public PutioSettingsValidator()
        {
            RuleFor(c => c.OAuthToken).NotEmpty().WithMessage("Please provide an OAuth token");
            RuleFor(c => c.DownloadPath).IsValidPath().WithMessage("Please provide a valid local path");
            RuleFor(c => c.SaveParentId).Matches(@"^\.?[0-9]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters 0-9");
        }
    }

    public class PutioSettings : IProviderConfig
    {
        private static readonly PutioSettingsValidator Validator = new PutioSettingsValidator();

        public PutioSettings()
        {
            Url = "https://api.put.io/v2";
        }

        public string Url { get; }

        [FieldDefinition(0, Label = "OAuth Token", Type = FieldType.Password)]
        public string OAuthToken { get; set; }

        [FieldDefinition(1, Label = "Save Parent Folder ID", Type = FieldType.Textbox, HelpText = "Adding a parent folder ID specific to Sonarr avoids conflicts with unrelated non-Sonarr downloads. Using a parent folder is optional, but strongly recommended.")]
        public string SaveParentId { get; set; }

        [FieldDefinition(2, Label = "Download completed transfers", Type = FieldType.Checkbox, HelpText = "If enabled, Sonarr will download completed files from Put.io. If you manually sync with rclone or similar, disable this")]
        public bool DownloadFiles { get; set; }

        [FieldDefinition(3, Label = "Download Path", Type = FieldType.Path, HelpText = "Path were Put.io is downloading to or if downloading is disabled where the mounts are expected")]
        public string DownloadPath { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
