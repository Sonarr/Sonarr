using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Validation
{
    public class ProfileExistsValidator : PropertyValidator
    {
        private readonly IQualityProfileService _qualityProfileService;

        public ProfileExistsValidator(IQualityProfileService qualityProfileService)
        {
            _qualityProfileService = qualityProfileService;
        }

        protected override string GetDefaultMessageTemplate() => "QualityProfile does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            return _qualityProfileService.Exists((int)context.PropertyValue);
        }
    }
}
