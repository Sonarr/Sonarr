using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Marr.Data;

namespace Marr.Data.Reflection
{
    public class SimpleReflectionStrategy : IReflectionStrategy
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
                        (member as FieldInfo).SetValue(entity, ReflectionHelper.GetDefaultValue((member as FieldInfo).FieldType));

                    else if (member.MemberType == MemberTypes.Property)
                    {
                        var pi = (member as PropertyInfo);
                        if (pi.CanWrite)
                            (member as PropertyInfo).SetValue(entity, ReflectionHelper.GetDefaultValue((member as PropertyInfo).PropertyType), null);

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
            MemberInfo member = entity.GetType().GetMember(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];

            if (member.MemberType == MemberTypes.Field)
            {
                return (member as FieldInfo).GetValue(entity);
            }
            else if (member.MemberType == MemberTypes.Property)
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
