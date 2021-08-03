using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Validation
{
    public class ProfileExistsValidator : PropertyValidator
    {
        private readonly IQualityProfileService _qualityProfileService;

        public ProfileExistsValidator(IQualityProfileService qualityProfileService)
            : base("QualityProfile does not exist")
        {
            _qualityProfileService = qualityProfileService;
        }

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
