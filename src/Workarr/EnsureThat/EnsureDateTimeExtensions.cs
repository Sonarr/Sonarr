using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using Workarr.Extensions;

namespace Workarr.EnsureThat
{
    public static class EnsureDateTimeExtensions
    {
        private static readonly DateTime _minTime = new DateTime(1960, 1, 1);

        [DebuggerStepThrough]
        public static Param<DateTime> IsLt(this Param<DateTime> param, DateTime limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsLte(this Param<DateTime> param, DateTime limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotLte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsGt(this Param<DateTime> param, DateTime limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGt, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsGte(this Param<DateTime> param, DateTime limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, StringExtensions.Inject(ExceptionMessages.EnsureExtensions_IsNotGte, param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsInRange(this Param<DateTime> param, DateTime min, DateTime max)
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

        [DebuggerStepThrough]
        public static Param<DateTime> IsUtc(this Param<DateTime> param)
        {
            if (param.Value.Kind != DateTimeKind.Utc)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, "Excepted time to be in UTC but was [{0}]".Inject(param.Value.Kind));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsValid(this Param<DateTime> param)
        {
            return IsGt(param, _minTime);
        }
    }
}
