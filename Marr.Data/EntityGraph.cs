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
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Linq;
using Marr.Data.Mapping;
using System.Reflection;

namespace Marr.Data
{
    /// <summary>
    /// Holds metadata about an object graph that is being queried and eagerly loaded.
    /// Contains all metadata needed to instantiate the object and fill it with data from a DataReader.
    /// Does not iterate through lazy loaded child relationships.
    /// </summary>
    internal class EntityGraph : IEnumerable<EntityGraph>
    {
        private MapRepository _repos;
        private EntityGraph _parent;
        private Type _entityType;
        private Relationship _relationship;
        private ColumnMapCollection _columns;
        private RelationshipCollection _relationships;
        private List<EntityGraph> _children;
        private object _entity;
        private GroupingKeyCollection _groupingKeyColumns;
        private Dictionary<string, EntityReference> _entityReferences;

        public IList RootList { get; private set; }

        /// <summary>
        /// Recursively builds an entity graph of the given parent type.
        /// </summary>
        /// <param name="entityType"></param>
        public EntityGraph(Type entityType, IList rootList)
            : this(entityType, null, null) // Recursively constructs hierarchy
        {
            RootList = rootList;
        }

        /// <summary>
        /// Recursively builds entity graph hierarchy.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parent"></param>
        /// <param name="relationship"></param>
        private EntityGraph(Type entityType, EntityGraph parent, Relationship relationship)
        {
            _repos = MapRepository.Instance;

            _entityType = entityType;
            _parent = parent;
            _relationship = relationship;
            _columns = _repos.GetColumns(entityType);
            _relationships = _repos.GetRelationships(entityType);
            _children = new List<EntityGraph>();
            Member = relationship != null ? relationship.Member : null;
            _entityReferences = new Dictionary<string, EntityReference>();

            // Create a new EntityGraph for each child relationship that is not lazy loaded
            foreach (Relationship childRelationship in this.Relationships)
            {
                if (!childRelationship.IsLazyLoaded)
                {
                    _children.Add(new EntityGraph(childRelationship.RelationshipInfo.EntityType, this, childRelationship));
                }
            }
        }

        public MemberInfo Member { get; private set; }

        /// <summary>
        /// Gets the parent of this EntityGraph.
        /// </summary>
        public EntityGraph Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Gets the Type of this EntityGraph.
        /// </summary>
        public Type EntityType
        {
            get { return _entityType; }
        }

        /// <summary>
        /// Gets a boolean than indicates whether this entity is the root node in the graph.
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return _parent == null;
            }
        }

        /// <summary>
        /// Gets a boolean that indicates whether this entity is a child.
        /// </summary>
        public bool IsChild
        {
            get
            {
                return _parent != null;
            }
        }

        /// <summary>
        /// Gets the columns mapped to this entity.
        /// </summary>
        public ColumnMapCollection Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets the relationships mapped to this entity.
        /// </summary>
        public RelationshipCollection Relationships
        {
            get { return _relationships; }
        }

        /// <summary>
        /// A list of EntityGraph objects that hold metadata about the child entities that will be loaded.
        /// </summary>
        public List<EntityGraph> Children
        {
            get { return _children; }
        }
        
        /// <summary>
        /// Adds an entity to the appropriate place in the object graph.
        /// </summary>
        /// <param name="entityInstance"></param>
        public void AddEntity(object entityInstance)
        {
            _entity = entityInstance;

            // Add newly created entityInstance to list (Many) or set it to field (One)
            if (this.IsRoot)
            {
                RootList.Add(entityInstance);
            }
            else if (_relationship.RelationshipInfo.RelationType == RelationshipTypes.Many)
            {
                var list = _parent._entityReferences[_parent.GroupingKeyColumns.GroupingKey]
                    .ChildLists[_relationship.Member.Name];

                list.Add(entityInstance);
            }
            else // RelationTypes.One
            {
                _repos.ReflectionStrategy.SetFieldValue(_parent._entity, _relationship.Member.Name, entityInstance);
            }

            EntityReference entityRef = new EntityReference(entityInstance);
            _entityReferences.Add(GroupingKeyColumns.GroupingKey, entityRef);

            InitOneToManyChildLists(entityRef);           
        }

        /// <summary>
        /// Initializes the owning lists on many-to-many Children.
        /// </summary>
        /// <param name="entityInstance"></param>
        private void InitOneToManyChildLists(EntityReference entityRef)
        {
            // Get a reference to the parent's the childrens' OwningLists to the parent entity
            for (int i = 0; i < Relationships.Count; i++)
            {
                Relationship relationship = Relationships[i];
                if (relationship.RelationshipInfo.RelationType == RelationshipTypes.Many)
                {
                    try
                    {
                        IList list = (IList)_repos.ReflectionStrategy.CreateInstance(relationship.MemberType);
                        _repos.ReflectionStrategy.SetFieldValue(entityRef.Entity, relationship.Member.Name, list);
                        
                        // Save a reference to each 1-M list
                        entityRef.AddChildList(relationship.Member.Name, list);
                    }
                    catch (Exception ex)
                    {
                        throw new DataMappingException(
                            string.Format("{0}.{1} is a \"Many\" relationship type so it must derive from IList.",
                                entityRef.Entity.GetType().Name, relationship.Member.Name),
                            ex);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively adds primary key columns from contiguous child graphs with a one-to-one relationship type to the pKeys collection..
        /// </summary>
        /// <param name="pKeys"></param>
        /// <param name="entity"></param>
        private void AddOneToOneChildKeys(ColumnMapCollection pKeys, EntityGraph entity)
        {
            var oneToOneChildren = entity.Children
                .Where(c => c._relationship.RelationshipInfo.RelationType == RelationshipTypes.One);

            foreach (var child in oneToOneChildren)
            {
                pKeys.AddRange(child.Columns.PrimaryKeys);
                AddOneToOneChildKeys(pKeys, child);
            }
        }

        /// <summary>
        /// Concatenates the values of the GroupingKeys property and compares them
        /// against the LastKeyGroup property.  Returns true if the values are different,
        /// or false if the values are the same.
        /// The currently concatenated keys are saved in the LastKeyGroup property.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool IsNewGroup(DbDataReader reader)
        {
            bool isNewGroup = false;

            // Get primary keys from parent entity and any one-to-one child entites
            GroupingKeyCollection groupingKeyColumns = this.GroupingKeyColumns;

            // Concatenate column values
            KeyGroupInfo keyGroupInfo = groupingKeyColumns.CreateGroupingKey(reader);
            
            if (!keyGroupInfo.HasNullKey && !_entityReferences.ContainsKey(keyGroupInfo.GroupingKey))
            {
                isNewGroup = true;
            }

            return isNewGroup;
        }
        
        /// <summary>
        /// Gets the GroupingKeys for this entity.  
        /// GroupingKeys determine when to create and add a new entity to the graph.
        /// </summary>
        /// <remarks>
        /// A simple entity with no relationships will return only its PrimaryKey columns.
        /// A parent entity with one-to-one child relationships will include its own PrimaryKeys,
        /// and it will recursively traverse all Children with one-to-one relationships and add their PrimaryKeys.
        /// A child entity that has a one-to-one relationship with its parent will use the same 
        /// GroupingKeys already defined by its parent.
        /// </remarks>
        public GroupingKeyCollection GroupingKeyColumns
        {
            get
            {
                if (_groupingKeyColumns == null)
                    _groupingKeyColumns = GetGroupingKeyColumns();

                return _groupingKeyColumns;
            }
        }

        /// <summary>
        /// Gets a list of keys to group by.
        /// </summary>
        /// <remarks>
        /// When converting an unnormalized set of data from a database view,
        /// a new entity is only created when the grouping keys have changed.
        /// NOTE: This behavior works on the assumption that the view result set
        /// has been sorted by the root entity primary key(s), followed by the
        /// child entity primary keys.
        /// </remarks>
        /// <returns></returns>
        private GroupingKeyCollection GetGroupingKeyColumns()
        {
            // Get primary keys for this parent entity
            GroupingKeyCollection groupingKeyColumns = new GroupingKeyCollection();
            groupingKeyColumns.PrimaryKeys.AddRange(Columns.PrimaryKeys);

            // The following conditions should fail with an exception:
            // 1) Any parent entity (entity with children) must have at least one PK specified or an exception will be thrown
            // 2) All 1-M relationship entities must have at least one PK specified
            // * Only 1-1 entities with no children are allowed to have 0 PKs specified.
            if ((groupingKeyColumns.PrimaryKeys.Count == 0 && _children.Count > 0) ||
                (groupingKeyColumns.PrimaryKeys.Count == 0 && !IsRoot && _relationship.RelationshipInfo.RelationType == RelationshipTypes.Many))
                throw new MissingPrimaryKeyException(string.Format("There are no primary key mappings defined for the following entity: '{0}'.", this.EntityType.Name));

            // Add parent's keys
            if (IsChild)
                groupingKeyColumns.ParentPrimaryKeys.AddRange(Parent.GroupingKeyColumns);

            return groupingKeyColumns;
        }

        #region IEnumerable<EntityGraph> Members

        public IEnumerator<EntityGraph> GetEnumerator()
        {
            return TraverseGraph(this);
        }

        /// <summary>
        /// Recursively traverses through every entity in the EntityGraph.
        /// </summary>
        /// <param name="entityGraph"></param>
        /// <returns></returns>
        private static IEnumerator<EntityGraph> TraverseGraph(EntityGraph entityGraph)
        {
            Stack<EntityGraph> stack = new Stack<EntityGraph>();
            stack.Push(entityGraph);

            while (stack.Count > 0)
            {
                EntityGraph node = stack.Pop();
                yield return node;

                foreach (EntityGraph childGraph in node.Children)
                {
                    stack.Push(childGraph);
                }
            }
        }


        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}

public struct KeyGroupInfo
{
    private string _groupingKey;
    private bool _hasNullKey;

    public KeyGroupInfo(string groupingKey, bool hasNullKey)
    {
        _groupingKey = groupingKey;
        _hasNullKey = hasNullKey;
    }

    public string GroupingKey 
    { 
        get { return _groupingKey; } 
    }

    public bool HasNullKey 
    {
        get { return _hasNullKey; } 
    }
}