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
            //for value types and string just return the value as is
            if (conventionInfo.SourceProp.Type.IsValueType || conventionInfo.SourceProp.Type == typeof(string))
                return conventionInfo.SourceProp.Value;

            //handle arrays
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
                //handle IEnumerable<> also ICollection<> IList<> List<>
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
            return Activator.CreateInstance(conventionInfo.SourceProp.Type)
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
            var t = conventionInfo.SourceProp.Type.GetGenericArguments()[0];
            if (t.IsValueType || t == typeof(string)) return conventionInfo.SourceProp.Value;

            var tlist = typeof(List<>).MakeGenericType(t);
            var list = Activator.CreateInstance(tlist);

            var addMethod = tlist.GetMethod("Add");
            foreach (var o in (IEnumerable)conventionInfo.SourceProp.Value)
            {
                var e = Activator.CreateInstance(t).InjectFrom<CloneInjection>(o);
                addMethod.Invoke(list, new[] { e }); // in 4.0 you can use dynamic and just do list.Add(e);
            }

            return list;
        }
    }
}