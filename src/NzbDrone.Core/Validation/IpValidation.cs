using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Validation
{
    public static class IpValidation
    {
        public static IRuleBuilderOptions<T, string> ValidIpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x => x.IsValidIpAddress()).WithMessage("Must contain wildcard (*) or a valid IP Address");
        }

        public static IRuleBuilderOptions<T, string> NotListenAllIp4Address<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new RegularExpressionValidator(@"^(?!0\.0\.0\.0)")).WithMessage("Use * instead of 0.0.0.0");
        }
    }
}
