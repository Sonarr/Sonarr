using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using TinyIoC;
using NzbDrone.Common.Reflection;

namespace NzbDrone.Common
{
    public abstract class ContainerBuilderBase
    {
        protected readonly TinyIoCContainer Container;
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
            var simpleInterfaces = _loadedTypes.Where(t => t.IsInterface).ToList();
            var appliedInterfaces = _loadedTypes.SelectMany(t => t.GetInterfaces()).Where(i => i.Assembly.FullName.Contains("NzbDrone")).ToList();

            var contracts = simpleInterfaces.Union(appliedInterfaces)
                .Except(new List<Type> { typeof(IMessage), typeof(ICommand), typeof(IEvent) });


            var count = contracts.Count();

            foreach (var contract in simpleInterfaces.Union(contracts))
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
                if (implementations.Single().HasAttribute<SingletonAttribute>())
                {
                    Container.Register(contractType, implementations.Single()).AsSingleton();
                }
                else
                {
                    Container.Register(contractType, implementations.Single()).AsMultiInstance();
                }

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