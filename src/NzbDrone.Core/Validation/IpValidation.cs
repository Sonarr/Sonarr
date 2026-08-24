using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Network;

namespace NzbDrone.Core.Validation
{
    public static class IpValidation
    {
        public static IRuleBuilderOptions<T, string> ValidIpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(x => x.IsValidIpAddress()).WithMessage("Must contain wildcard (*) or a valid IP Address");
        }

        public static IRuleBuilderOptions<T, string> ValidIpNetworks<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(IPNetworkParser.IsValidList).WithMessage("Must be a comma separated list of IP addresses or networks in CIDR notation, such as 172.17.0.1, 10.0.0.0/8 or fc00::/7");
        }
    }
}
