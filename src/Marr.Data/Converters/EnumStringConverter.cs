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
using Marr.Data.Mapping;

namespace Marr.Data.Converters
{
    public class EnumStringConverter : IConverter
    {
        public object FromDB(ConverterContext context)
        {
            if (context.DbValue == null || context.DbValue == DBNull.Value)
                return null;
            return Enum.Parse(context.ColumnMap.FieldType, (string)context.DbValue);
        }

        public object FromDB(ColumnMap map, object dbValue)
        {
            return FromDB(new ConverterContext { ColumnMap = map, DbValue = dbValue });
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null)
                return DBNull.Value;
            return clrValue.ToString();
        }

        public Type DbType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}
