using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Delay;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Profiles.Delay
{
    public class DelayProfileValidator : PropertyValidator
    {
        public DelayProfileValidator()
            : base("Usenet or Torrent must be enabled")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var delayProfile = new DelayProfile();
            delayProfile.InjectFrom(context.ParentContext.InstanceToValidate);

            if (!delayProfile.EnableUsenet && !delayProfile.EnableTorrent)
            {
                return false;
            }

            return true;
        }
    }
}
