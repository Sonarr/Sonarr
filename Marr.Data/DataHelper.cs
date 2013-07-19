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
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Marr.Data.Mapping;
using System.Linq.Expressions;

namespace Marr.Data
{
    /// <summary>
    /// This class contains misc. extension methods that are used throughout the project.
    /// </summary>
    internal static class DataHelper
    {
        public static bool HasColumn(this IDataReader dr, string columnName) 
        { 
            for (int i=0; i < dr.FieldCount; i++) 
            { 
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) 
                    return true; 
            } 
            return false; 
        }

        public static string ParameterPrefix(this IDbCommand command)
        {
            string commandType = command.GetType().Name.ToLower();
            return commandType.Contains("oracle") ? ":" : "@";
        }
        
        /// <summary>
        /// Returns the mapped name, or the member name.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetTableName(this MemberInfo member)
        {
            string tableName = MapRepository.Instance.GetTableName(member.DeclaringType);
            return tableName ?? member.DeclaringType.Name;
        }

        public static string GetTableName(this Type memberType)
        {
            return MapRepository.Instance.GetTableName(memberType);
        }

        public static string GetColumName(this IColumnInfo col, bool useAltName)
        {
            if (useAltName)
            {
                return col.TryGetAltName();
            }
            else
            {
                return col.Name;
            }
        }

        /// <summary>
        /// Returns the mapped column name, or the member name.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetColumnName(Type declaringType, string propertyName, bool useAltName)
        {
            // Initialize column name as member name
            string columnName = propertyName;

            var columnMap = MapRepository.Instance.GetColumns(declaringType).GetByFieldName(propertyName);

            if (columnMap == null)
            {
                throw new InvalidOperationException(string.Format("Column map missing for field {0}.{1}", declaringType.FullName, propertyName));
            }

            if (useAltName)
            {
                columnName = columnMap.ColumnInfo.TryGetAltName();
            }
            else
            {
                columnName = columnMap.ColumnInfo.Name;
            }

            return columnName;
        }

        /// <summary>
        /// Determines a property name from a passed in expression.
        /// Ex:  p => p.FirstName   ->    "FirstName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetMemberName<T>(this Expression<Func<T, object>> member)
        {
            var memberExpression = (member.Body as MemberExpression);
            if (memberExpression == null)
            {
                memberExpression = (member.Body as UnaryExpression).Operand as MemberExpression;
            }

            return memberExpression.Member.Name;
        }

        public static string GetMemberName(this LambdaExpression exp)
        {
            var memberExpression = (exp.Body as MemberExpression);
            if (memberExpression == null)
            {
                memberExpression = (exp.Body as UnaryExpression).Operand as MemberExpression;
            }

            return memberExpression.Member.Name;
        }

        public static bool ContainsMember(this List<MemberInfo> list, MemberInfo member)
        {
            foreach (var m in list)
            {
                if (m.EqualsMember(member))
                    return true;
            }

            return false;
        }

        public static bool EqualsMember(this MemberInfo member, MemberInfo otherMember)
        {
            return member.Name == otherMember.Name && member.DeclaringType == otherMember.DeclaringType;
        }

        /// <summary>
        /// Determines if a type is not a complex object.
        /// </summary>
        public static bool IsSimpleType(Type type)
        {
            Type underlyingType = !IsNullableType(type) ? type : type.GetGenericArguments()[0];

            return
                underlyingType.IsPrimitive ||
                underlyingType.Equals(typeof(string)) ||
                underlyingType.Equals(typeof(DateTime)) ||
                underlyingType.Equals(typeof(decimal)) ||
                underlyingType.IsEnum;
        }

        public static bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

    }
}
