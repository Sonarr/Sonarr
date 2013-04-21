using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Mapping
{
    public static class ValueInjectorExtensions
    {
        public static TTarget InjectTo<TTarget>(this object source) where TTarget : new()
        {
            var targetType = typeof(TTarget);

            if (targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() != null &&
                targetType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)) &&
                source.GetType().IsGenericType &&
                source.GetType().GetGenericTypeDefinition() != null &&
                source.GetType().GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
            {

                var result = new TTarget();

                var listSubType = targetType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(listSubType);
                var addMethod = listType.GetMethod("Add");

                foreach (var sourceItem in (IEnumerable)source)
                {
                    var e = Activator.CreateInstance(listSubType).InjectFrom(sourceItem);
                    addMethod.Invoke(result, new[] { e });
                }

                return result;
            }

            return (TTarget)new TTarget().InjectFrom(source);
        }
    }
}