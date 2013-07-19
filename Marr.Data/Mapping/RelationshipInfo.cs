using System;

namespace Marr.Data.Mapping
{
    public class RelationshipInfo : IRelationshipInfo
    {
        public RelationshipTypes RelationType { get; set; }

        public Type EntityType { get; set; }
    }
}
