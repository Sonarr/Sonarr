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

namespace Marr.Data.Mapping
{
    /// <summary>
    /// Defines a field as a related entity that needs to be created at filled with data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RelationshipAttribute : Attribute, IRelationshipInfo
    {
        /// <summary>
        /// Defines a data relationship.
        /// </summary>
        public RelationshipAttribute()
            : this(RelationshipTypes.AutoDetect)
        { }

        /// <summary>
        /// Defines a data relationship.
        /// </summary>
        /// <param name="relationType"></param>
        public RelationshipAttribute(RelationshipTypes relationType)
        {
            RelationType = relationType;
        }

        /// <summary>
        /// Defines a One-ToMany data relationship for a given type.
        /// </summary>
        /// <param name="entityType">The type of the child entity.</param>
        public RelationshipAttribute(Type entityType)
            : this(entityType, RelationshipTypes.AutoDetect)
        { }

        /// <summary>
        /// Defines a data relationship.
        /// </summary>
        /// <param name="entityType">The type of the child entity.</param>
        /// <param name="relationType">The relationship type can be "One" or "Many".</param>
        public RelationshipAttribute(Type entityType, RelationshipTypes relationType)
        {
            EntityType = entityType;
            RelationType = relationType;
        }

        #region IRelationshipInfo Members

        /// <summary>
        /// Gets or sets the relationship type can be "One" or "Many".
        /// </summary>
        public RelationshipTypes RelationType { get; set; }

        /// <summary>
        /// Gets or sets the type of the child entity.
        /// </summary>
        public Type EntityType { get; set; }

        #endregion
    }
}
