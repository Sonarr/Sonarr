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

using System.Collections.Generic;

namespace Marr.Data.Mapping
{
    public class RelationshipCollection : List<Relationship>
    {
        /// <summary>
        /// Gets a ColumnMap by its field name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public Relationship this[string fieldName]
        {
            get
            {
                return this.Find(m => m.Member.Name == fieldName);
            }
        }
    }
}
