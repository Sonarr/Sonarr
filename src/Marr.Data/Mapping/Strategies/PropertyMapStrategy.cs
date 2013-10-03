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
using System.Collections;
using System.Reflection;

namespace Marr.Data.Mapping.Strategies
{
    /// <summary>
    /// Maps all public properties to DB columns.  
    /// </summary>
    public class PropertyMapStrategy : AttributeMapStrategy
    {
        public PropertyMapStrategy(bool publicOnly)
            : base(publicOnly)
        { }

        /// <summary>
        /// Maps properties to DB columns if a ColumnAttribute is not present.
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <param name="member">The current member that is being inspected.</param>
        /// <param name="columnAtt">A ColumnAttribute (is null of one does not exist).</param>
        /// <param name="columnMaps">A list of ColumnMaps.</param>
        /// </summary>
        protected override void CreateColumnMap(Type entityType, MemberInfo member, ColumnAttribute columnAtt, ColumnMapCollection columnMaps)
        {
            if (columnAtt != null)
            {
                // Add columns with ColumnAttribute
                base.CreateColumnMap(entityType, member, columnAtt, columnMaps);
            }
            else
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    // Map public property to DB column
                    columnMaps.Add(new ColumnMap(member));
                }
            }
        }

        /// <summary>
        /// Maps a relationship if a RelationshipAttribute is present.
        /// </summary>
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <param name="member">The current member that is being inspected.</param>
        /// <param name="relationshipAtt">A RelationshipAttribute (is null if one does not exist).</param>
        /// <param name="relationships">A list of Relationships.</param>
        protected override void CreateRelationship(Type entityType, MemberInfo member, RelationshipAttribute relationshipAtt, RelationshipCollection relationships)
        {
            if (relationshipAtt != null)
            {
                // Add relationships by RelationshipAttribute
                base.CreateRelationship(entityType, member, relationshipAtt, relationships);
            }
            else
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = member as PropertyInfo;
                    if (typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        Relationship relationship = new Relationship(member);
                        relationships.Add(relationship);
                    }
                }
            }
        }
    }
}
