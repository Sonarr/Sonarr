using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Marr.Data.Reflection
{
    public class SimpleReflectionStrategy : IReflectionStrategy
    {

        private static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>();
        private static readonly Dictionary<string, GetterDelegate> GetterCache = new Dictionary<string, GetterDelegate>();
        private static readonly Dictionary<string, SetterDelegate> SetterCache = new Dictionary<string, SetterDelegate>();


        private static MemberInfo GetMember(Type entityType, string name)
        {
            MemberInfo member;
            var key = entityType.FullName + name;
            if (!MemberCache.TryGetValue(key, out member))
            {
                member = entityType.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
                MemberCache[key] = member;
            }

            return member;
        }

        /// <summary>
        /// Gets an entity field value by name.
        /// </summary>
        public object GetFieldValue(object entity, string fieldName)
        {
            var member = GetMember(entity.GetType(), fieldName);

            if (member.MemberType == MemberTypes.Field)
            {
                return (member as FieldInfo).GetValue(entity);
            }
            if (member.MemberType == MemberTypes.Property)
            {
                return BuildGetter(entity.GetType(), fieldName)(entity);
            }
            throw new DataMappingException(string.Format("The DataMapper could not get the value for {0}.{1}.", entity.GetType().Name, fieldName));
        }


        /// <summary>
        /// Instantiates a type using the FastReflector library for increased speed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }




        public GetterDelegate BuildGetter(Type type, string memberName)
        {
            GetterDelegate getter;
            var key = type.FullName + memberName;
            if (!GetterCache.TryGetValue(key, out getter))
            {
                getter = GetPropertyGetter((PropertyInfo)GetMember(type, memberName));
            }

            return getter;
        }

        public SetterDelegate BuildSetter(Type type, string memberName)
        {
            SetterDelegate setter;
            var key = type.FullName + memberName;
            if (!SetterCache.TryGetValue(key, out setter))
            {
                setter = GetPropertySetter((PropertyInfo)GetMember(type, memberName));
            }

            return setter;
        }


        private static SetterDelegate GetPropertySetter(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod();
            if (propertySetMethod == null) return null;

#if NO_EXPRESSIONS
            return (o, convertedValue) =>
            {
                propertySetMethod.Invoke(o, new[] { convertedValue });
                return;
            };
#else
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var instanceParam = Expression.Convert(instance, propertyInfo.DeclaringType);
            var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);

            var setterCall = Expression.Call(instanceParam, propertyInfo.GetSetMethod(), valueParam);

            return Expression.Lambda<SetterDelegate>(setterCall, instance, argument).Compile();
#endif
        }

        private static GetterDelegate GetPropertyGetter(PropertyInfo propertyInfo)
        {

            var getMethodInfo = propertyInfo.GetGetMethod();
            if (getMethodInfo == null) return null;

#if NO_EXPRESSIONS
			return o => propertyInfo.GetGetMethod().Invoke(o, new object[] { });
#else
            try
            {
                var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
                var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.DeclaringType);

                var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
                var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

                var propertyGetFn = Expression.Lambda<GetterDelegate>
                    (
                        oExprCallPropertyGetFn,
                        oInstanceParam
                    ).Compile();

                return propertyGetFn;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
#endif
        }

    }
}
