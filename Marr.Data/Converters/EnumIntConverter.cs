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
    public class EnumIntConverter : IConverter
    {
        public object FromDB(ColumnMap map, object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return null;
            else
                return Enum.ToObject(map.FieldType, (int)dbValue);
        }

        public object ToDB(object clrValue)
        {
            if (clrValue == null)
                return DBNull.Value;
            else
                return (int)clrValue;
        }

        public Type DbType
        {
            get
            {
                return typeof(int);
            }
        }
    }
}
