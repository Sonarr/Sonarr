using System.Collections.Generic;
using System.Collections;

namespace Marr.Data
{
    /// <summary>
    /// Stores an entity along with all of its 1-M IList references.
    /// </summary>
    public class EntityReference
    {
        public EntityReference(object entity)
        {
            Entity = entity;
            ChildLists = new Dictionary<string, IList>();
        }

        public object Entity { get; private set; }
        public Dictionary<string, IList> ChildLists { get; private set; }

        public void AddChildList(string memberName, IList list)
        {
            if (ChildLists.ContainsKey(memberName))
                ChildLists[memberName] = list;
            else
                ChildLists.Add(memberName, list);
        }
    }
}
