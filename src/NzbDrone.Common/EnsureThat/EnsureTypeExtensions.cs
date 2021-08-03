using System;
using System.Diagnostics;
using NzbDrone.Common.EnsureThat.Resources;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.EnsureThat
{
    public static class EnsureTypeExtensions
    {
        private static class Types
        {
            internal static readonly Type IntType = typeof(int);

            internal static readonly Type ShortType = typeof(short);

            internal static readonly Type DecimalType = typeof(decimal);

            internal static readonly Type DoubleType = typeof(double);

            internal static readonly Type FloatType = typeof(float);

            internal static readonly Type BoolType = typeof(bool);

            internal static readonly Type DateTimeType = typeof(DateTime);

            internal static readonly Type StringType = typeof(string);
        }

        [DebuggerStepThrough]
        public static TypeParam IsInt(this TypeParam param)
        {
            return IsOfType(param, Types.IntType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsShort(this TypeParam param)
        {
            return IsOfType(param, Types.ShortType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsDecimal(this TypeParam param)
        {
            return IsOfType(param, Types.DecimalType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsDouble(this TypeParam param)
        {
            return IsOfType(param, Types.DoubleType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsFloat(this TypeParam param)
        {
            return IsOfType(param, Types.FloatType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsBool(this TypeParam param)
        {
            return IsOfType(param, Types.BoolType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsDateTime(this TypeParam param)
        {
            return IsOfType(param, Types.DateTimeType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsString(this TypeParam param)
        {
            return IsOfType(param, Types.StringType);
        }

        [DebuggerStepThrough]
        public static TypeParam IsOfType(this TypeParam param, Type type)
        {
            if (!param.Type.Equals(type))
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name,
                    ExceptionMessages.EnsureExtensions_IsNotOfType.Inject(param.Type.FullName));
            }

            return param;
        }
    }
}
