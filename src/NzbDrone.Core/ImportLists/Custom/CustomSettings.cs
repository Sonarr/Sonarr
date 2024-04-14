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

    public class CustomSettings : ImportListSettingsBase<CustomSettings>
    {
        private static readonly CustomSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "ImportListsCustomListSettingsUrl", HelpText = "ImportListsCustomListSettingsUrlHelpText")]
        public override string BaseUrl { get; set; } = string.Empty;

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
