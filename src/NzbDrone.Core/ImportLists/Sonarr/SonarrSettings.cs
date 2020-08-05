using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Sonarr
{
    public class SonarrSettingsValidator : AbstractValidator<SonarrSettings>
    {
        public SonarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class SonarrSettings : IImportListSettings
    {
        private static readonly SonarrSettingsValidator Validator = new SonarrSettingsValidator();

        public SonarrSettings()
        {
            BaseUrl = "";
            ApiKey = "";
            ProfileIds = new int[] { };
        }

        [FieldDefinition(0, Label = "Full URL", HelpText = "URL, including port, of the Sonarr V3 instance to import from")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key", HelpText = "Apikey of the Sonarr V3 instance to import from")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Device, Label = "Profiles", HelpText = "Profiles from the source instance to import from")]
        public IEnumerable<int> ProfileIds { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
