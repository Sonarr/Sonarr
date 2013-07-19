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

namespace Marr.Data.Mapping.Strategies
{
    /// <summary>
    /// A strategy for creating mappings for a given entity.
    /// </summary>
    public interface IMapStrategy
    {
        /// <summary>
        /// Creates a table map for a given entity type.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        string MapTable(Type entityType);

        /// <summary>
        /// Creates a ColumnMapCollection for a given entity type.
        /// </summary>
        /// <param name="entityType">The entity that is being mapped.</param>
        ColumnMapCollection MapColumns(Type entityType);

        /// <summary>
        /// Creates a RelationshpCollection for a given entity type.
        /// </summary>
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <returns></returns>
        RelationshipCollection MapRelationships(Type entityType);
    }
}
