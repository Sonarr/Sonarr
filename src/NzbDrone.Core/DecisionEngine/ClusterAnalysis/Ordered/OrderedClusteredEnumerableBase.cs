using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.ClusterAnalysis.Ordered
{
    public class OrderedClusteredEnumerableBase<TElement> : IOrderedClusteredEnumerable<TElement>
    {
        protected OrderedClusteredEnumerableBase<TElement> Next;
        protected readonly IEnumerable<TElement> Source;
        
        public IOrderedClusteredEnumerable<TElement> CreateOrderedGroupedEnumerable<TKey>(Func<TElement, TKey> keySelector, bool descending, IComparer<TKey> comparer)
        {
            Append(new OrderedClusteredEnumerable<TElement, TKey>(Source, descending, comparer, s => s.GroupBy(keySelector)));
            return this;
        }

        public IOrderedClusteredEnumerable<TElement> CreateOrderedClusteredEnumerable<TKey>(Func<TElement, double> keySelector, bool descending, double distanceCutPoint)
        {
            Append(new OrderedClusteredEnumerable<TElement, double>(Source, descending, null, s => s.ClusterBy(keySelector, distanceCutPoint)));
            return this;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Apply(Source).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerable<TElement> Apply(IEnumerable<TElement> source)
        {
            return Next == null ? source : Next.Apply(source);
        }

        protected void Append(OrderedClusteredEnumerableBase<TElement> next)
        {
            if (Next == null)
            {
                Next = next;
            }
            else
            {
                Next.Append(next);
            }
        }

        public OrderedClusteredEnumerableBase(IEnumerable<TElement> source)
        {
            Source = source;
        }
    }
}
