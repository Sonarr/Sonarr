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
            DeleteImported = false;
        }

        public string Url { get; }

        [FieldDefinition(0, Label = "OAuth Token", Type = FieldType.Password)]
        public string OAuthToken { get; set; }

        [FieldDefinition(1, Label = "Save Parent Folder ID", Type = FieldType.Textbox, HelpText = "Adding a parent folder ID specific to Sonarr avoids conflicts with unrelated non-Sonarr downloads. Using a parent folder is optional, but strongly recommended.")]
        public string SaveParentId { get; set; }

        [FieldDefinition(2, Label = "Download Path", Type = FieldType.Path, HelpText = "Path were Sonarr will expect the files to get downloaded to. Note: This client does not download finished transfers automatically. Instead make sure that you download them outside of Sonarr e.g. with rclone")]
        public string DownloadPath { get; set; }

        [FieldDefinition(3, Label = "Delete imported files", Type = FieldType.Checkbox, HelpText = "Delete the files on put.io when Sonarr marks them as successfully imported")]
        public bool DeleteImported { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
