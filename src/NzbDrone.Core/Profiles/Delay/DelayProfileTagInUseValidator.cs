using System.Collections.Generic;
using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Profiles.Delay
{
    public class DelayProfileTagInUseValidator : PropertyValidator
    {
        private readonly IDelayProfileService _delayProfileService;

        public DelayProfileTagInUseValidator(IDelayProfileService delayProfileService)
        {
            _delayProfileService = delayProfileService;
        }

        protected override string GetDefaultMessageTemplate() => "One or more tags is used in another profile";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            if (context.PropertyValue is not HashSet<int> collection || collection.Empty())
            {
                return true;
            }

            return _delayProfileService.All().None(d => d.Id != instanceId && d.Tags.Intersect(collection).Any());
        }
    }
}
