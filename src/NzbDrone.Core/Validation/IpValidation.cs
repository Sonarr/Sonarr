using System.Net;
using System.Net.Sockets;
using FluentValidation;
using FluentValidation.Validators;

namespace NzbDrone.Core.Validation
{
    public static class IpValidation
    {
        public static IRuleBuilderOptions<T, string> ValidIp4Address<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x =>
            {
                IPAddress parsedAddress;

                if (!IPAddress.TryParse(x, out parsedAddress))
                {
                    return false;
                }

                if (parsedAddress.Equals(IPAddress.Parse("255.255.255.255")))
                {
                    return false;
                }

                return parsedAddress.AddressFamily == AddressFamily.InterNetwork;
            }).WithMessage("Must contain wildcard (*) or a valid IPv4 Address");
        }

        public static IRuleBuilderOptions<T, string> NotListenAllIp4Address<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new RegularExpressionValidator(@"^(?!0\.0\.0\.0)")).WithMessage("Use * instead of 0.0.0.0");
        }
    }
}
