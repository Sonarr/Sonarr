using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Validation
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, int> ValidId<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new GreaterThanValidator(0));
        }

        public static IRuleBuilderOptions<T, int> IsZero<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EqualValidator(0));
        }

        public static IRuleBuilderOptions<T, string> HaveHttpProtocol<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^http(s)?://", RegexOptions.IgnoreCase)).WithMessage("must start with http:// or https://");
        }

        public static IRuleBuilderOptions<T, string> ValidRootUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^http(?:s)?://[a-z0-9-.]+", RegexOptions.IgnoreCase)).WithMessage("must be valid URL that");
        }

        public static IRuleBuilderOptions<T, int> ValidPort<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new InclusiveBetweenValidator(1, 65535));
        }

        public static IRuleBuilderOptions<T, string> ValidIp4Address<T>(this IRuleBuilder<T, string> ruleBuilder)
        {

            return ruleBuilder.Must(x =>
            {
                IPAddress parsedAddress;
                if (!IPAddress.TryParse(x, out parsedAddress))
                {
                    return false;                    
                }

                return parsedAddress.AddressFamily == AddressFamily.InterNetwork;
            });
        }

        public static IRuleBuilderOptions<T, Language> ValidLanguage<T>(this IRuleBuilder<T, Language> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new LangaugeValidator());
        }
    }
}