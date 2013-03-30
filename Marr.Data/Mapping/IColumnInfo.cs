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
using System.Data.Common;
using System.Data.OleDb;

namespace Marr.Data.Mapping
{
    public interface IColumnInfo
    {
        string Name { get; set;  }
        string AltName { get; set; }
        int Size { get; set;  }
        bool IsPrimaryKey { get; set; }
        bool IsAutoIncrement { get; set; }
        bool ReturnValue { get; set; }
        ParameterDirection ParamDirection { get; set; }
        string TryGetAltName();
    }
   
}
