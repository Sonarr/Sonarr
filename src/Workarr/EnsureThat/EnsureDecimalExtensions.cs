using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using Workarr.Extensions;

namespace Workarr.EnsureThat
{
    public static class EnsureDecimalExtensions
    {
        [DebuggerStepThrough]
        public static Param<decimal> IsLt(this Param<decimal> param, decimal limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<decimal> IsLte(this Param<decimal> param, decimal limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<decimal> IsGt(this Param<decimal> param, decimal limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<decimal> IsGte(this Param<decimal> param, decimal limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<decimal> IsInRange(this Param<decimal> param, decimal min, decimal max)
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
