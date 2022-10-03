using FluentValidation;

using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Custom
{
    public class CustomSettingsValidator : AbstractValidator<CustomSettings>
    {
        public CustomSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class CustomSettings : IImportListSettings
    {
        private static readonly CustomSettingsValidator Validator = new CustomSettingsValidator();

        public CustomSettings()
        {
            BaseUrl = "";
        }

        [FieldDefinition(0, Label = "List URL", HelpText = "The URL for the series list")]
        public string BaseUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
