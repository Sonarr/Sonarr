using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System.Reflection;

namespace NzbDrone.Api.Mapping
{
    public class CloneInjection : LoopInjection
    {
        protected override void Execute(PropertyInfo sp, object source, object target)
        {
            var tp = target.GetType().GetProperty(sp.Name);
            if (tp == null) return;
            var val = sp.GetValue(source);
            if (val == null) return;

            tp.SetValue(target, GetClone(sp, tp, val));
        }

        private static object GetClone(PropertyInfo sp, PropertyInfo tp, object sourceValue)
        {
            if (sp.PropertyType == tp.PropertyType)
            {
                return sourceValue;
            }

            if (sp.PropertyType.IsValueType || sp.PropertyType == typeof(string))
            {
                return sourceValue;
            }

            if (sp.PropertyType.IsArray)
            {
                var arr = sourceValue as Array;
                var arrClone = arr.Clone() as Array;

                for (int index = 0; index < arr.Length; index++)
                {
                    var a = arr.GetValue(index);
                    if (a.GetType().IsValueType || a is string) continue;

                    arrClone.SetValue(Activator.CreateInstance(a.GetType()).InjectFrom<CloneInjection>(a), index);
                }

                return arrClone;
            }

            if (sp.PropertyType.IsGenericType)
            {
                var genericInterfaces = sp.PropertyType.GetGenericTypeDefinition().GetInterfaces();
                if (genericInterfaces.Any(d => d == typeof(IEnumerable)))
                {
                    var genericArgument = tp.PropertyType.GetGenericArguments()[0];
                    return MapLists(genericArgument, sourceValue);
                }

                if (genericInterfaces.Any(i => i == typeof(ILazyLoaded)))
                {
                    return MapLazy(sp, tp, sourceValue);
                }

                //unhandled generic type, you could also return null or throw
                return sourceValue;
            }

            //for simple object types create a new instace and apply the clone injection on it
            return Activator.CreateInstance(tp.PropertyType).InjectFrom<CloneInjection>(sourceValue);
        }

        private static object MapLazy(PropertyInfo sp, PropertyInfo tp, object sourceValue)
        {
            var sourceArgument = sp.PropertyType.GetGenericArguments()[0];

            dynamic lazy = sourceValue;

            if (lazy.IsLoaded && lazy.Value != null)
            {
                if (tp.PropertyType.IsAssignableFrom(sourceArgument))
                {
                    return lazy.Value;
                }

                var genericArgument = tp.PropertyType;

                if (genericArgument.IsValueType || genericArgument == typeof(string))
                {
                    return lazy.Value;
                }

                if (genericArgument.IsGenericType)
                {
                    if (sp.PropertyType.GetGenericTypeDefinition().GetInterfaces().Any(d => d == typeof(IEnumerable)))
                    {
                        return MapLists(genericArgument, lazy.Value);
                    }
                }

                return Activator.CreateInstance(genericArgument).InjectFrom((object)lazy.Value);
            }

            return null;
        }

        private static object MapLists(Type targetType, object sourceValue)
        {
            if (targetType.IsValueType || targetType == typeof(string))
            {
                return sourceValue;
            }

            var listType = typeof(List<>).MakeGenericType(targetType);
            var addMethod = listType.GetMethod("Add");

            var result = Activator.CreateInstance(listType);

            foreach (var sourceItem in (IEnumerable)sourceValue)
            {
                var e = Activator.CreateInstance(targetType).InjectFrom<CloneInjection>(sourceItem);
                addMethod.Invoke(result, new[] { e });
            }

            return result;
        }
    }
}
