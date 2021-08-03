using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using NzbDrone.Common.EnsureThat.Resources;

namespace NzbDrone.Common.EnsureThat
{
    public static class EnsureCollectionExtensions
    {
        [DebuggerStepThrough]
        public static Param<T> HasItems<T>(this Param<T> param)
            where T : class, ICollection
        {
            if (param.Value == null || param.Value.Count < 1)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<Collection<T>> HasItems<T>(this Param<Collection<T>> param)
        {
            if (param.Value == null || param.Value.Count < 1)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<IEnumerable<T>> HasItems<T>(this Param<IEnumerable<T>> param)
        {
            if (param.Value == null || !param.Value.Any())
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<T[]> HasItems<T>(this Param<T[]> param)
        {
            if (param.Value == null || param.Value.Length < 1)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<List<T>> HasItems<T>(this Param<List<T>> param)
        {
            if (param.Value == null || param.Value.Count < 1)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }

        [DebuggerStepThrough]
        public static Param<IDictionary<TKey, TValue>> HasItems<TKey, TValue>(this Param<IDictionary<TKey, TValue>> param)
        {
            if (param.Value == null || param.Value.Count < 1)
            {
                throw ExceptionFactory.CreateForParamValidation(param.Name, ExceptionMessages.EnsureExtensions_IsEmptyCollection);
            }

            return param;
        }
    }
}
