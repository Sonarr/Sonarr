using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Common.Composition
{
    public class KnownTypes
    {
        private List<Type> _knownTypes;

        // So unity can resolve for tests
        public KnownTypes()
            : this(new List<Type>())
        {
        }

        public KnownTypes(List<Type> loadedTypes)
        {
            _knownTypes = loadedTypes;
        }

        public IEnumerable<Type> GetImplementations(Type contractType)
        {
            return _knownTypes
                .Where(implementation =>
                    contractType.IsAssignableFrom(implementation) &&
                    !implementation.IsInterface &&
                    !implementation.IsAbstract);
        }
    }
}
