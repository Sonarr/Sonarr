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
using System.Linq;
using System.Text;
using System.Reflection;

namespace Marr.Data.Mapping.Strategies
{
    /// <summary>
    /// Iterates through the members of an entity based on the BindingFlags, and provides an abstract method for adding ColumnMaps for each member.
    /// </summary>
    public abstract class ReflectionMapStrategyBase : IMapStrategy
    {
        private BindingFlags _bindingFlags;

        /// <summary>
        /// Loops through members with the following BindingFlags:
        /// Instance | NonPublic | Public | FlattenHierarchy
        /// </summary>
        public ReflectionMapStrategyBase()
        {
            _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        }

        /// <summary>
        /// Loops through members with the following BindingFlags:
        /// Instance | Public | FlattenHierarchy | NonPublic (optional)
        /// </summary>
        /// <param name="publicOnly"></param>
        public ReflectionMapStrategyBase(bool publicOnly)
        {
            if (publicOnly)
            {
                _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            }
            else
            {
                _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            }
        }

        /// <summary>
        /// Loops through members based on the passed in BindingFlags.
        /// </summary>
        /// <param name="bindingFlags"></param>
        public ReflectionMapStrategyBase(BindingFlags bindingFlags)
        {
            _bindingFlags = bindingFlags;
        }

        public string MapTable(Type entityType)
        {
            object[] atts = entityType.GetCustomAttributes(typeof(TableAttribute), true);
            if (atts.Length > 0)
            {
                return (atts[0] as TableAttribute).Name;
            }
            else
            {
                return entityType.Name;
            }
        }

        /// <summary>
        /// Implements IMapStrategy.
        /// Loops through filtered members and calls the virtual "CreateColumnMap" void for each member.
        /// Subclasses can override CreateColumnMap to customize adding ColumnMaps.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public ColumnMapCollection MapColumns(Type entityType)
        {
            ColumnMapCollection columnMaps = new ColumnMapCollection();

            MemberInfo[] members = entityType.GetMembers(_bindingFlags);
            foreach (var member in members)
            {
                ColumnAttribute columnAtt = GetColumnAttribute(member);
                CreateColumnMap(entityType, member, columnAtt, columnMaps);
            }
            
            return columnMaps;
        }

        public RelationshipCollection MapRelationships(Type entityType)
        {
            RelationshipCollection relationships = new RelationshipCollection();

            MemberInfo[] members = entityType.GetMembers(_bindingFlags);
            foreach (MemberInfo member in members)
            {
                RelationshipAttribute relationshipAtt = GetRelationshipAttribute(member);
                CreateRelationship(entityType, member, relationshipAtt, relationships);
            }

            return relationships;
        }

        protected ColumnAttribute GetColumnAttribute(MemberInfo member)
        {
            if (member.IsDefined(typeof(ColumnAttribute), false))
            {
                return (ColumnAttribute)member.GetCustomAttributes(typeof(ColumnAttribute), false)[0];
            }

            return null;
        }

        protected RelationshipAttribute GetRelationshipAttribute(MemberInfo member)
        {
            if (member.IsDefined(typeof(RelationshipAttribute), false))
            {
                return (RelationshipAttribute)member.GetCustomAttributes(typeof(RelationshipAttribute), false)[0];
            }

            return null;
        }
        
        /// <summary>
        /// Inspect a member and optionally add a ColumnMap.
        /// </summary>
        /// <param name="entityType">The entity type that is being mapped.</param>
        /// <param name="member">The member that is being mapped.</param>
        /// <param name="columnMaps">The ColumnMapCollection that is being created.</param>
        protected abstract void CreateColumnMap(Type entityType, MemberInfo member, ColumnAttribute columnAtt, ColumnMapCollection columnMaps);

        /// <summary>
        /// Inspect a member and optionally add a Relationship.
        /// </summary>
        /// <param name="entityType">The entity that is being mapped.</param>
        /// <param name="member">The current member that is being inspected.</param>
        /// <param name="relationshipAtt">A RelationshipAttribute (is null if one does not exist).</param>
        /// <param name="relationships">A list of Relationships.</param>
        protected abstract void CreateRelationship(Type entityType, MemberInfo member, RelationshipAttribute relationshipAtt, RelationshipCollection relationships);
    }
}
