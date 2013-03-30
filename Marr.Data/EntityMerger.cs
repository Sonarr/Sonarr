using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Marr.Data
{
    /// <summary>
    /// This utility class allows you to join two existing entity collections.
    /// </summary>
    public class EntityMerger
    {
        /// <summary>
        /// Joines to existing entity collections.
        /// </summary>
        /// <typeparam name="TParent">The parent entity type.</typeparam>
        /// <typeparam name="TChild">The child entity type.</typeparam>
        /// <param name="parentList">The parent entities.</param>
        /// <param name="childList">The child entities</param>
        /// <param name="relationship">A predicate that defines the relationship between the parent and child entities.  Returns true if they are related.</param>
        /// <param name="mergeAction">An action that adds a related child to the parent.</param>
        public static void Merge<TParent, TChild>(IEnumerable<TParent> parentList, IEnumerable<TChild> childList, Func<TParent, TChild, bool> relationship, Action<TParent, TChild> mergeAction)
        {
            foreach (TParent parent in parentList)
            {
                foreach (TChild child in childList)
                {
                    if (relationship(parent, child))
                    {
                        mergeAction(parent, child);
                    }
                }
            }
        }
    }
}
