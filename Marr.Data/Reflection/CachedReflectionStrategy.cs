using System;
using System.Reflection;
using Fasterflect;

namespace Marr.Data.Reflection
{
    public class CachedReflectionStrategy : IReflectionStrategy
    {

        /// <summary>
        /// Sets an entity field value by name to the passed in 'val'.
        /// </summary>
        public void SetFieldValue<T>(T entity, string fieldName, object val)
        {
            MemberInfo member = entity.GetType().GetMember(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

            try
            {
                // Handle DB null values
                if (val == DBNull.Value)
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        entity.SetFieldValue(member.Name, ReflectionHelper.GetDefaultValue(((FieldInfo)member).FieldType));
                    }
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var pi = (PropertyInfo)member;
                        if (pi.CanWrite)
                        {
                            entity.SetPropertyValue(member.Name, ReflectionHelper.GetDefaultValue(((PropertyInfo)member).PropertyType));
                        }
                    }
                }
                else
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        entity.SetFieldValue(member.Name, val);
                    }
                    else if (member.MemberType == MemberTypes.Property && ((PropertyInfo)member).CanWrite)
                    {
                        member.Set(entity, val);
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
            MemberInfo member = entity.GetType().GetMember(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

            if (member.MemberType == MemberTypes.Field)
            {
                return entity.GetFieldValue(member.Name);
            }

            if (member.MemberType == MemberTypes.Property && ((PropertyInfo)member).CanRead)
            {
                return entity.GetPropertyValue(member.Name);
            }

            throw new DataMappingException(string.Format("The DataMapper could not get the value for {0}.{1}.", entity.GetType().Name, fieldName));
        }

        /// <summary>
        /// Instantiantes a type using the Fasterflect library for increased speed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object CreateInstance(Type type)
        {
            return type.CreateInstance();
        }
    }
}
