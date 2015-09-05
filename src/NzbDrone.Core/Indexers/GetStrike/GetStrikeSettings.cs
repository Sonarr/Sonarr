using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.GetStrike
{
    public class GetStrikeSettingsValidator : AbstractValidator<GetStrikeSettings>
    {
        public GetStrikeSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class GetStrikeSettings : IProviderConfig
    {
        private static readonly GetStrikeSettingsValidator Validator = new GetStrikeSettingsValidator();

        public GetStrikeSettings()
        {
            BaseUrl = "http://getstrike.net/api/v2";
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing.")]
        public string BaseUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
