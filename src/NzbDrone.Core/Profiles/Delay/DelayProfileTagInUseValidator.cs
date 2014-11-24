using System.Collections.Generic;
using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Profiles.Delay
{
    public class DelayProfileTagInUseValidator : PropertyValidator
    {
        private readonly IDelayProfileService _delayProfileService;

        public DelayProfileTagInUseValidator(IDelayProfileService delayProfileService)
            : base("One or more tags is used in another profile")
        {
            _delayProfileService = delayProfileService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;

            var delayProfile = new DelayProfile();
            delayProfile.InjectFrom(context.ParentContext.InstanceToValidate);

            var collection = context.PropertyValue as HashSet<int>;

            if (collection == null || collection.Empty()) return true;

            return _delayProfileService.All().None(d => d.Id != delayProfile.Id && d.Tags.Intersect(collection).Any());
        }
    }
}
