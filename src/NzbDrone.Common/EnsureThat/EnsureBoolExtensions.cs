using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;

namespace NzbDrone.Common.EnsureThat
{
    public static class EnsureBoolExtensions
    {
        [DebuggerStepThrough]
        public static Param<bool> IsTrue(this Param<bool> param)
        {
            if (!param.Value)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotTrue);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<bool> IsFalse(this Param<bool> param)
        {
            if (param.Value)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsNotFalse);
            }

            return param;
        }
    }
}
