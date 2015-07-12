using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Validation
{
    public class LanguageProfileExistsValidator : PropertyValidator
    {
        private readonly ILanguageProfileService _profileService;

        public LanguageProfileExistsValidator(ILanguageProfileService profileService)
            : base("Language profile does not exist")
        {
            _profileService = profileService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            return _profileService.Exists((int)context.PropertyValue);
        }
    }
}