using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Marr.Data.Reflection;

namespace NzbDrone.Core.Datastore
{
    public class DelegateReflectionStrategy : IReflectionStrategy
    {
        private static readonly Dictionary<string, PropertySetterDelegate> SetterCache = new Dictionary<string, PropertySetterDelegate>();
        private static readonly Dictionary<string, PropertyGetterDelegate> GetterCache = new Dictionary<string, PropertyGetterDelegate>();

        private static PropertySetterDelegate SetterFunction(Type entityType, string name)
        {
            PropertySetterDelegate setter;
            var key = string.Concat(entityType.FullName, name);
            if (!SetterCache.TryGetValue(key, out setter))
            {
                var setterMember = entityType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                setter = setterMember.GetPropertySetterFn();
                SetterCache[key] = setter;
            }

            return setter;
        }


        private static PropertyGetterDelegate GetterFunction(Type entityType, string name)
        {
            PropertyGetterDelegate getter;
            var key = string.Concat(entityType.FullName, name);
            if (!GetterCache.TryGetValue(key, out getter))
            {
                var propertyInfo = entityType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                getter = propertyInfo.GetPropertyGetterFn();
                GetterCache[key] = getter;
            }

            return getter;
        }


        public void SetFieldValue<T>(T entity, string fieldName, object val)
        {
            SetterFunction(entity.GetType(), fieldName)(entity, val);
        }

        public object GetFieldValue(object entity, string fieldName)
        {
            return GetterFunction(entity.GetType(), fieldName);
        }

        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }



    }

    public delegate void PropertySetterDelegate(object instance, object value);
    public delegate object PropertyGetterDelegate(object instance);

    public static class PropertyInvoker
    {
        public static PropertySetterDelegate GetPropertySetterFn(this PropertyInfo propertyInfo)
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

            return Expression.Lambda<PropertySetterDelegate>(setterCall, instance, argument).Compile();
#endif
        }

        public static PropertyGetterDelegate GetPropertyGetterFn(this PropertyInfo propertyInfo)
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

                var propertyGetFn = Expression.Lambda<PropertyGetterDelegate>
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