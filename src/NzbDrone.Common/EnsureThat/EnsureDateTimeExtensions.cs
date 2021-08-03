using System;
using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.EnsureThat
{
    public static class EnsureDateTimeExtensions
    {
        private static readonly DateTime _minTime = new DateTime(1960, 1, 1);

        [DebuggerStepThrough]
        public static Param<DateTime> IsLt(this Param<DateTime> param, DateTime limit)
        {
            if (param.Value >= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotLt.Inject(param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsLte(this Param<DateTime> param, DateTime limit)
        {
            if (!(param.Value <= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotLte.Inject(param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsGt(this Param<DateTime> param, DateTime limit)
        {
            if (param.Value <= limit)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotGt.Inject(param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsGte(this Param<DateTime> param, DateTime limit)
        {
            if (!(param.Value >= limit))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotGte.Inject(param.Value, limit));
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<DateTime> IsInRange(this Param<DateTime> param, DateTime min, DateTime max)
        {
            if (param.Value < min)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotInRange_ToLow.Inject(param.Value, min));
            }

            if (param.Value > max)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotInRange_ToHigh.Inject(param.Value, max));
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
