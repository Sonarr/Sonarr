using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyIoC;

namespace NzbDrone.Common
{
    public abstract class ContainerBuilderBase
    {
        protected TinyIoCContainer Container;
        private readonly List<Type> _loadedTypes;

        protected ContainerBuilderBase(params string[] assemblies)
        {
            Container = new TinyIoCContainer();

            _loadedTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                _loadedTypes.AddRange(Assembly.Load(assembly).GetTypes());
            }

            AutoRegisterInterfaces();
        }

        private void AutoRegisterInterfaces()
        {
            var interfaces = _loadedTypes.Where(t => t.IsInterface);

            foreach (var contract in interfaces)
            {
                AutoRegisterImplementations(contract);
            }
        }

        protected void AutoRegisterImplementations<TContract>()
        {
            AutoRegisterImplementations(typeof(TContract));
        }

        private void AutoRegisterImplementations(Type contractType)
        {
            var implementations = GetImplementations(contractType).ToList();

            if (implementations.Count == 0)
            {
                return;
            }
            if (implementations.Count == 1)
            {
                Container.Register(contractType, implementations.Single()).AsMultiInstance();
                Container.RegisterMultiple(contractType, implementations).AsMultiInstance();
            }
            else
            {
                Container.RegisterMultiple(contractType, implementations).AsMultiInstance();
            }
        }

        private IEnumerable<Type> GetImplementations(Type contractType)
        {
            return _loadedTypes
                .Where(implementation =>
                       contractType.IsAssignableFrom(implementation) &&
                       !implementation.IsInterface &&
                       !implementation.IsAbstract
                );
        }
    }
}