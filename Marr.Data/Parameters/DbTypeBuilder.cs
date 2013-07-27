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
using System.Data;

namespace Marr.Data.Parameters
{
    public class DbTypeBuilder : IDbTypeBuilder
    {
        public Enum GetDbType(Type type)
        {
            if (type == typeof(String))
                return DbType.String;

            if (type == typeof(Int32))
                return DbType.Int32;

            if (type == typeof(Decimal))
                return DbType.Decimal;

            if (type == typeof(DateTime))
                return DbType.DateTime;

            if (type == typeof(Boolean))
                return DbType.Boolean;

            if (type == typeof(Int16))
                return DbType.Int16;

            if (type == typeof(Single))
                return DbType.Single;

            if (type == typeof(Int64))
                return DbType.Int64;

            if (type == typeof(Double))
                return DbType.Double;

            if (type == typeof(Byte))
                return DbType.Byte;

            if (type == typeof(Byte[]))
                return DbType.Binary;

            if (type == typeof(Guid))
                return DbType.Guid;

            return DbType.Object;
        }

        public void SetDbType(IDbDataParameter param, Enum dbType)
        {
            param.DbType = (DbType)dbType;
        }
    }
}
