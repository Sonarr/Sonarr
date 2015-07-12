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
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^https?://", RegexOptions.IgnoreCase)).WithMessage("must start with http:// or https://");
        }

        public static IRuleBuilderOptions<T, string> ValidHost<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^[-_a-z0-9.]+$", RegexOptions.IgnoreCase)).WithMessage("must be valid Host without http://");
        }

        public static IRuleBuilderOptions<T, string> ValidRootUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            return ruleBuilder.SetValidator(new RegularExpressionValidator("^https?://[-_a-z0-9.]+", RegexOptions.IgnoreCase)).WithMessage("must be valid URL that starts with http(s)://");
        }

        public static IRuleBuilderOptions<T, string> ValidUrlBase<T>(this IRuleBuilder<T, string> ruleBuilder, string example = "/sonarr")
        {
            return ruleBuilder.SetValidator(new RegularExpressionValidator(@"^(?!\/?https?://[-_a-z0-9.]+)", RegexOptions.IgnoreCase)).WithMessage($"Must be a valid URL path (ie: '{example}')");
        }

        public static IRuleBuilderOptions<T, int> ValidPort<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new InclusiveBetweenValidator(1, 65535))
                              .Must(x =>
                              {
                                  if (x <= 1024)
                                  {
                                      if (x == 80 || x == 443)
                                      {
                                          return true;
                                      }

                                      return false;
                                  }

                                  return true;
                              });
        }

        public static IRuleBuilderOptions<T, TProp> AsWarning<T, TProp>(this IRuleBuilderOptions<T, TProp> ruleBuilder)
        {
            return ruleBuilder.WithState(v => NzbDroneValidationState.Warning);
        }
    }
}
