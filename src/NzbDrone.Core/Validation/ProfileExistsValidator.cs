using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Validation
{
    public class ProfileExistsValidator : PropertyValidator
    {
        private readonly IProfileService _profileService;

        public ProfileExistsValidator(IProfileService profileService)
            : base("Profile does not exist")
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