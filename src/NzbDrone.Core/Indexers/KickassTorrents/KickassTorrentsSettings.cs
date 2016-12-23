using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrentsSettingsValidator : AbstractValidator<KickassTorrentsSettings>
    {
        public KickassTorrentsSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class KickassTorrentsSettings : IProviderConfig
    {
        private static readonly KickassTorrentsSettingsValidator Validator = new KickassTorrentsSettingsValidator();

        public KickassTorrentsSettings()
        {
            BaseUrl = "";
            VerifiedOnly = true;
        }

        [FieldDefinition(0, Label = "Website URL", HelpText = "Please verify that the url you enter is a trustworthy site.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Verified Only", Type = FieldType.Checkbox, HelpText = "By setting this to No you will likely get more junk and unconfirmed releases, so use it with caution.")]
        public bool VerifiedOnly { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}