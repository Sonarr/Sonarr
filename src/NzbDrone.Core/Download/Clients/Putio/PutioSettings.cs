using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioSettingsValidator : AbstractValidator<PutioSettings>
    {
        public PutioSettingsValidator()
        {
            RuleFor(c => c.OAuthToken).NotEmpty().WithMessage("Please provide an OAuth token");
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

        [FieldDefinition(1, Label = "Save Parent ID", Type = FieldType.Textbox, HelpText = "If you provide a folder id here the torrents will be saved in that directory")]
        public string SaveParentId { get; set; }

        [FieldDefinition(2, Label = "Disable Download", Type = FieldType.Checkbox, HelpText = "If enabled, Sonarr will not download completed files from Put.io. Useful if you manually sync with rclone or similar")]
        public bool DisableDownload { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
