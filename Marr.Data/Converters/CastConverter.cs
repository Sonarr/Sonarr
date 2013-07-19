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

namespace Marr.Data.Converters
{
    public class CastConverter<TClr, TDb> : Marr.Data.Converters.IConverter
        where TClr : IConvertible
        where TDb : IConvertible
    {
        #region IConversion Members

        public Type DbType
        {
            get { return typeof(TDb); }
        }

        public object FromDB(Marr.Data.Mapping.ColumnMap map, object dbValue)
        {
            TDb val = (TDb)dbValue;
            return val.ToType(typeof(TClr), System.Globalization.CultureInfo.InvariantCulture);
        }

        public object ToDB(object clrValue)
        {
            TClr val = (TClr)clrValue;
            return val.ToType(typeof(TDb), System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion
    }
}

