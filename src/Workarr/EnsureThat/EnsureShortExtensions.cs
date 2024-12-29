using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using Workarr.Extensions;

namespace Workarr.EnsureThat
{
    public static class EnsureShortExtensions
    {
        [DebuggerStepThrough]
        public static Param<short> IsLt(this Param<short> param, short limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<short> IsLte(this Param<short> param, short limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<short> IsGt(this Param<short> param, short limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<short> IsGte(this Param<short> param, short limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<short> IsInRange(this Param<short> param, short min, short max)
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
