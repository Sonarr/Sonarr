using System.Diagnostics;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat.Resources;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.EnsureThat
{
    [DebuggerStepThrough]
    public static class EnsureStringExtensions
    {
        [DebuggerStepThrough]
        public static Param<string> IsNotNullOrWhiteSpace(this Param<string> param)
        {
            if (string.IsNullOrWhiteSpace(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrWhiteSpace);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> IsNotNullOrEmpty(this Param<string> param)
        {
            if (string.IsNullOrEmpty(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrEmpty);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> HasLengthBetween(this Param<string> param, int minLength, int maxLength)
        {
            if (string.IsNullOrEmpty(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrEmpty);
            }

            var length = param.Value.Length;

            if (length < minLength)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotInRange_ToShort.Inject(minLength, maxLength, length));
            }

            if (length > maxLength)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotInRange_ToLong.Inject(minLength, maxLength, length));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> IsLongerThan(this Param<string> param, int minLength)
        {
            if (string.IsNullOrEmpty(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrEmpty);
            }

            var length = param.Value.Length;

            if (length < minLength)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, "The string is not long enough. Must be at least '{0}' but was '{1}' characters long.".Inject(minLength, length));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> Matches(this Param<string> param, string match)
        {
            return Matches(param, new Regex(match));
        }

        [DebuggerStepThrough]
        public static Param<string> Matches(this Param<string> param, Regex match)
        {
            if (!match.IsMatch(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_NoMatch.Inject(param.Value, match));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> IsRelativePath(this Param<string> param)
        {
            if (string.IsNullOrWhiteSpace(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrWhiteSpace);
            }

            if (!param.Value.EndsWith("\\"))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, string.Format("value [{0}]  is not a valid relative path. relative paths must end with \\", param.Value));
            }

            if (param.Value.Length > 1 && param.Value.StartsWith("\\"))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, string.Format("value [{0}]  is not a valid relative path. relative paths can not start with \\", param.Value));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<string> IsValidPath(this Param<string> param, PathValidationType validationType)
        {
            if (string.IsNullOrWhiteSpace(param.Value))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotNullOrWhiteSpace);
            }

            if (param.Value.IsPathValid(validationType))
            {
                return param;
            }

            if (OsInfo.IsWindows)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, string.Format("value [{0}]  is not a valid Windows path. paths must be a full path eg. C:\\Windows", param.Value));
            }

            throw ExceptionFactory.CreateForParamValidation(param.Name, string.Format("value [{0}]  is not a valid *nix path. paths must start with /", param.Value));
        }
    }
}
