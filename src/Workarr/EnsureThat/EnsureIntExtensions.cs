using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using Workarr.Extensions;

namespace Workarr.EnsureThat
{
    public static class EnsureIntExtensions
    {
        [DebuggerStepThrough]
        public static Param<int> IsLessThan(this Param<int> param, int limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<int> IsLessThanOrEqualTo(this Param<int> param, int limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<int> IsGreaterThan(this Param<int> param, int limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<int> IsGreaterThanZero(this Param<int> param)
        {
            if (param.Value <= 0)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, 0));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<int> IsGreaterOrEqualTo(this Param<int> param, int limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<int> IsInRange(this Param<int> param, int min, int max)
        {
            if (param.Value < min)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotInRange_ToLow, param.Value, min));
            }

            if (param.Value > max)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotInRange_ToHigh, param.Value, max));
            }

            return param;
        }
    }
}
