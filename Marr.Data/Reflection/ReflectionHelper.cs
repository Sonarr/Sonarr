/*  Copyright (C) 2008 - 2011 Jordan Marr

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library. If not, see <http://www.gnu.org/licenses/>. */

using System;
using System.Reflection;

namespace Marr.Data
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Converts a DBNull.Value to a null for a reference field,
        /// or the default value of a value field.
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type fieldType)
        {
            if (fieldType.IsGenericType)
            {
                return null;
            }
            else if (fieldType.IsValueType)
            {
                return Activator.CreateInstance(fieldType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the CLR data type of a MemberInfo.  
        /// If the type is nullable, returns the underlying type.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(MemberInfo member)
        {
            Type memberType = null;
            if (member.MemberType == MemberTypes.Property)
                memberType = (member as PropertyInfo).PropertyType;
            else if (member.MemberType == MemberTypes.Field)
                memberType = (member as FieldInfo).FieldType;
            else
                memberType = typeof(object);

            // Handle nullable types - get underlying type
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                memberType = memberType.GetGenericArguments()[0];
            }

            return memberType;
        }
    }
}
