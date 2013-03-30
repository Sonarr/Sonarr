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
using System.Reflection;

namespace Marr.Data.Mapping
{
    public class Relationship
    {
        private IRelationshipInfo _relationshipInfo;
        private MemberInfo _member;
        private ILazyLoaded _lazyLoaded;

        public Relationship(MemberInfo member)
            : this(member, new RelationshipInfo())
        { }

        public Relationship(MemberInfo member, IRelationshipInfo relationshipInfo)
        {
            _member = member;

            Type memberType = ReflectionHelper.GetMemberType(member);

            // Try to determine the RelationshipType
            if (relationshipInfo.RelationType == RelationshipTypes.AutoDetect)
            {
                if (typeof(System.Collections.ICollection).IsAssignableFrom(memberType))
                {
                    relationshipInfo.RelationType = RelationshipTypes.Many;
                }
                else
                {
                    relationshipInfo.RelationType = RelationshipTypes.One;
                }
            }

            // Try to determine the EntityType
            if (relationshipInfo.EntityType == null)
            {
                if (relationshipInfo.RelationType == RelationshipTypes.Many)
                {
                    if (memberType.IsGenericType)
                    {
                        // Assume a Collection<T> or List<T> and return T
                        relationshipInfo.EntityType = memberType.GetGenericArguments()[0];
                    }
                    else
                    {
                        throw new ArgumentException(string.Format(
                            "The DataMapper could not determine the RelationshipAttribute EntityType for {0}.",
                            memberType.Name));
                    }
                }
                else
                {
                    relationshipInfo.EntityType = memberType;
                }
            }

            _relationshipInfo = relationshipInfo;
        }

        public IRelationshipInfo RelationshipInfo
        {
            get { return _relationshipInfo; }
        }

        public MemberInfo Member
        {
            get { return _member; }
        }

        public Type MemberType
        {
            get
            {
                // Assumes that a relationship can only have a member type of Field or Property
                if (Member.MemberType == MemberTypes.Field)
                    return (Member as FieldInfo).FieldType;
                else
                    return (Member as PropertyInfo).PropertyType;
            }
        }

        public bool IsLazyLoaded
        {
            get
            {
                return _lazyLoaded != null;
            }
        }

        public ILazyLoaded LazyLoaded
        {
            get
            {
                return _lazyLoaded;
            }
            set
            {
                _lazyLoaded = value;
            }
        }
    }
}
