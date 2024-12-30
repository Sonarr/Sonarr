using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using Workarr.Extensions;

namespace Workarr.EnsureThat
{
    public static class EnsureDoubleExtensions
    {
        [DebuggerStepThrough]
        public static Param<double> IsLt(this Param<double> param, double limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<double> IsLte(this Param<double> param, double limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<double> IsGt(this Param<double> param, double limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<double> IsGte(this Param<double> param, double limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<double> IsInRange(this Param<double> param, double min, double max)
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
