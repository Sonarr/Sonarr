using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marr.Data.Mapping
{
    public class RelationshipInfo : IRelationshipInfo
    {
        public RelationshipTypes RelationType { get; set; }

        public Type EntityType { get; set; }
    }
}
