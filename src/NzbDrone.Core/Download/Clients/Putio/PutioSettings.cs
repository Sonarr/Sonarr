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

        [FieldDefinition(0, Label = "OAuth Token", Type = FieldType.Textbox)]
        public string OAuthToken { get; set; }

        [FieldDefinition(1, Label = "Save Parent ID", Type = FieldType.Textbox, HelpText = "Adding a save parent ID specific to Sonarr avoids conflicts with unrelated downloads, but it's optional. Creates a .[SaveParentId] subdirectory in the output directory.")]
        public string SaveParentId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
