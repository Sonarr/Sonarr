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
using System.Data.SqlClient;

namespace Marr.Data.Parameters
{
    public class SqlDbTypeBuilder : IDbTypeBuilder
    {
        public Enum GetDbType(Type type)
        {
            if (type == typeof(String))
                return SqlDbType.VarChar;

            if (type == typeof(Int32))
                return SqlDbType.Int;

            if (type == typeof(Decimal))
                return SqlDbType.Decimal;

            if (type == typeof(DateTime))
                return SqlDbType.DateTime;

            if (type == typeof(Boolean))
                return SqlDbType.Bit;

            if (type == typeof(Int16))
                return SqlDbType.SmallInt;

            if (type == typeof(Int64))
                return SqlDbType.BigInt;

            if (type == typeof(Double))
                return SqlDbType.Float;

            if (type == typeof(Char))
                return SqlDbType.Char;

            if (type == typeof(Byte))
                return SqlDbType.Binary;

            if (type == typeof(Byte[]))
                return SqlDbType.VarBinary;

            if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;

            return SqlDbType.Variant;
        }

        public void SetDbType(IDbDataParameter param, Enum dbType)
        {
            var sqlDbParam = (SqlParameter)param;
            sqlDbParam.SqlDbType = (SqlDbType)dbType;
        }
    }
}
