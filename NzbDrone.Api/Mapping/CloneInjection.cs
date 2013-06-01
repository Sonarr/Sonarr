using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Mapping
{
    public class CloneInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo conventionInfo)
        {
            return conventionInfo.SourceProp.Name == conventionInfo.TargetProp.Name &&
                   conventionInfo.SourceProp.Value != null;
        }

        protected override object SetValue(ConventionInfo conventionInfo)
        {
            if (conventionInfo.SourceProp.Type.IsValueType || conventionInfo.SourceProp.Type == typeof(string))
                return conventionInfo.SourceProp.Value;

            if (conventionInfo.SourceProp.Type.IsArray)
            {
                var array = (Array)conventionInfo.SourceProp.Value;
                var clone = (Array)array.Clone();

                for (var index = 0; index < array.Length; index++)
                {
                    var item = array.GetValue(index);
                    if (!item.GetType().IsValueType && !(item is string))
                    {
                        clone.SetValue(Activator.CreateInstance(item.GetType()).InjectFrom<CloneInjection>(item), index);
                    }
                }

                return clone;
            }

            if (conventionInfo.SourceProp.Type.IsGenericType)
            {
                var genericInterfaces = conventionInfo.SourceProp.Type.GetGenericTypeDefinition().GetInterfaces();
                if (genericInterfaces.Any(d => d == typeof(IEnumerable)))
                {
                    return MapLists(conventionInfo);
                }

                if (genericInterfaces.Any(i => i == typeof(ILazyLoaded)))
                {
                    return MapLazy(conventionInfo);
                }

                //unhandled generic type, you could also return null or throw
                return conventionInfo.SourceProp.Value;
            }

            //for simple object types create a new instace and apply the clone injection on it
            return Activator.CreateInstance(conventionInfo.TargetProp.Type)
                            .InjectFrom<CloneInjection>(conventionInfo.SourceProp.Value);
        }

        private static object MapLazy(ConventionInfo conventionInfo)
        {

            var genericArgument = conventionInfo.SourceProp.Type.GetGenericArguments()[0];

            dynamic lazy = conventionInfo.SourceProp.Value;

            if (lazy.IsLoaded && conventionInfo.TargetProp.Type.IsAssignableFrom(genericArgument))
            {
                return lazy.Value;
            }

            return null;
        }

        private static object MapLists(ConventionInfo conventionInfo)
        {
            var genericArgument = conventionInfo.TargetProp.Type.GetGenericArguments()[0];
            if (genericArgument.IsValueType || genericArgument == typeof(string))
            {
                return conventionInfo.SourceProp.Value;
            }


            var listType = typeof(List<>).MakeGenericType(genericArgument);
            var addMethod = listType.GetMethod("Add");

            var result = Activator.CreateInstance(listType);

            foreach (var sourceItem in (IEnumerable)conventionInfo.SourceProp.Value)
            {
                var e = Activator.CreateInstance(genericArgument).InjectFrom<CloneInjection>(sourceItem);
                addMethod.Invoke(result, new[] {e });
            }

            return result;

        }
    }
}