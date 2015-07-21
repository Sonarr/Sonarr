using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TitansOfTv
{
    public class TitansOfTvSettingsValidator : AbstractValidator<TitansOfTvSettings>
    {
        public TitansOfTvSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class TitansOfTvSettings : IProviderConfig
    {
     private static readonly TitansOfTvSettingsValidator Validator = new TitansOfTvSettingsValidator();

        public TitansOfTvSettings()
        {
            BaseUrl = "http://titansof.tv/api";
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API key", HelpText = "Enter your ToTV API key. (My Account->API->Site API Key)")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
