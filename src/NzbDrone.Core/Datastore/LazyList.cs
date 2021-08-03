using System.Collections.Generic;
using Marr.Data;

namespace NzbDrone.Core.Datastore
{
    public class LazyList<T> : LazyLoaded<List<T>>
    {
        public LazyList()
            : this(new List<T>())
        {
        }

        public LazyList(IEnumerable<T> items)
            : base(new List<T>(items))
        {
        }

        public static implicit operator LazyList<T>(List<T> val)
        {
            return new LazyList<T>(val);
        }

        public static implicit operator List<T>(LazyList<T> lazy)
        {
            return lazy.Value;
        }
    }
}
