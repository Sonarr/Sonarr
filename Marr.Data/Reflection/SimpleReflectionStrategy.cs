using System;
using System.Collections.Generic;
using System.Reflection;

namespace Marr.Data.Reflection
{
    public class SimpleReflectionStrategy : IReflectionStrategy
    {

        private static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>();


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
        /// Sets an entity field value by name to the passed in 'val'.
        /// </summary>
        public void SetFieldValue<T>(T entity, string fieldName, object val)
        {
            var member = GetMember(entity.GetType(), fieldName);

            try
            {
                // Handle DB null values
                if (val == DBNull.Value)
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        (member as FieldInfo).SetValue(entity,
                                                       ReflectionHelper.GetDefaultValue((member as FieldInfo).FieldType));
                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var pi = (member as PropertyInfo);
                        if (pi.CanWrite)
                            (member as PropertyInfo).SetValue(entity,
                                                              ReflectionHelper.GetDefaultValue(
                                                                  (member as PropertyInfo).PropertyType), null);

                    }
                }
                else
                {
                    if (member.MemberType == MemberTypes.Field)
                        (member as FieldInfo).SetValue(entity, val);

                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var pi = (member as PropertyInfo);
                        if (pi.CanWrite)
                            (member as PropertyInfo).SetValue(entity, val, null);

                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("The DataMapper was unable to load the following field: {0}.  \nDetails: {1}", fieldName, ex.Message);
                throw new DataMappingException(msg, ex);
            }
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
                if ((member as PropertyInfo).CanRead)
                    return (member as PropertyInfo).GetValue(entity, null);
            }
            throw new DataMappingException(string.Format("The DataMapper could not get the value for {0}.{1}.", entity.GetType().Name, fieldName));
        }

        /// <summary>
        /// Instantiantes a type using the FastReflector library for increased speed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
