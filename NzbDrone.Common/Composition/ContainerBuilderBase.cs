using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Reflection;
using TinyIoC;


namespace NzbDrone.Common.Composition
{
    public abstract class ContainerBuilderBase
    {
        private readonly List<Type> _loadedTypes;

        public IContainer Container { get; private set; }

        protected ContainerBuilderBase(params string[] assemblies)
        {
            Container = new Container(new TinyIoCContainer());

            _loadedTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                _loadedTypes.AddRange(Assembly.Load(assembly).GetTypes());
            }

            AutoRegisterInterfaces();
        }

        private void AutoRegisterInterfaces()
        {
            var loadedInterfaces = _loadedTypes.Where(t => t.IsInterface).ToList();
            var implementedInterfaces = _loadedTypes.SelectMany(t => t.GetInterfaces());

            var contracts = loadedInterfaces.Union(implementedInterfaces).Where(c => !c.IsGenericTypeDefinition && !string.IsNullOrWhiteSpace(c.FullName))
                .Where(c => !c.FullName.StartsWith("System"))
                .Except(new List<Type> { typeof(IMessage), typeof(IEvent), typeof(IContainer) }).Distinct().OrderBy(c => c.FullName);

            foreach (var contract in contracts)
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
            var implementations = GetImplementations(contractType).Where(c => !c.IsGenericTypeDefinition).ToList();



            if (implementations.Count == 0)
            {
                return;
            }
            if (implementations.Count == 1)
            {
                var impl = implementations.Single();

                Trace.WriteLine(string.Format("Registering {0} -> {1}", contractType.FullName, impl.Name));


                if (impl.HasAttribute<SingletonAttribute>())
                {
                    Container.RegisterSingleton(contractType, impl);
                }
                else
                {
                    Container.Register(contractType, impl);
                }
            }
            else
            {
                Trace.WriteLine(string.Format("Registering {0} -> {1}", contractType.FullName, implementations.Count));

                Container.RegisterAll(contractType, implementations);
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