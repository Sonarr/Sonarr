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
using System.Text;
using System.Data;
using System.Reflection;
using Marr.Data.Converters;

namespace Marr.Data.Mapping
{
    /// <summary>
    /// Contains information about the class fields and their associated stored proc parameters
    /// </summary>
    public class ColumnMap
    {
        /// <summary>
        /// Creates a column map with an empty ColumnInfo object.
        /// </summary>
        /// <param name="member">The .net member that is being mapped.</param>
        public ColumnMap(MemberInfo member)
            : this(member, new ColumnInfo())
        { }

        public ColumnMap(MemberInfo member, IColumnInfo columnInfo)
        {
            FieldName = member.Name;

            // If the column name is not specified, the field name will be used.
            if (string.IsNullOrEmpty(columnInfo.Name))
                columnInfo.Name = member.Name;

            FieldType = ReflectionHelper.GetMemberType(member);

            Type paramNetType = FieldType;
            MapRepository repository = MapRepository.Instance;

            IConverter converter = repository.GetConverter(FieldType);
            if (converter != null)
            {
                // Handle conversions
                paramNetType = converter.DbType;
            }

            // Get database specific DbType and store with column map in cache    
            DBType = repository.DbTypeBuilder.GetDbType(paramNetType);

            ColumnInfo = columnInfo;
        }
        
        public string FieldName { get; set; }
        public Type FieldType { get; set; }
        public Enum DBType { get; set; }
        public IColumnInfo ColumnInfo { get; set; }
    }
}
