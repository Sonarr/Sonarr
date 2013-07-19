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
using System.Reflection;

namespace Marr.Data.Mapping.Strategies
{
    /// <summary>
    /// Maps fields or properties that are marked with the ColumnAttribute.
    /// </summary>
    public class AttributeMapStrategy : ReflectionMapStrategyBase
    {
        public AttributeMapStrategy()
            : base()
        { }

        public AttributeMapStrategy(bool publicOnly)
            : base(publicOnly)
        { }

        public AttributeMapStrategy(BindingFlags flags)
            : base(flags)
        { }

        /// <summary>
        /// Registers any member with a ColumnAttribute as a ColumnMap.
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <param name="member">The current member that is being inspected.</param>
        /// <param name="columnAtt">A ColumnAttribute (is null of one does not exist).</param>
        /// <param name="columnMaps">A list of ColumnMaps.</param>
        /// </summary>
        protected override void CreateColumnMap(Type entityType, MemberInfo member, ColumnAttribute columnAtt, ColumnMapCollection columnMaps)
        {
            if (columnAtt != null)
            {
                ColumnMap columnMap = new ColumnMap(member, columnAtt);
                columnMaps.Add(columnMap);
            }
        }

        /// <summary>
        /// Registers any member with a RelationshipAttribute as a relationship.
        /// </summary>
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <param name="member">The current member that is being inspected.</param>
        /// <param name="relationshipAtt">A RelationshipAttribute (is null if one does not exist).</param>
        /// <param name="relationships">A list of Relationships.</param>
        protected override void CreateRelationship(Type entityType, MemberInfo member, RelationshipAttribute relationshipAtt, RelationshipCollection relationships)
        {
            if (relationshipAtt != null)
            {
                Relationship relationship = new Relationship(member, relationshipAtt);
                relationships.Add(relationship);
            }
        }
    }
}
