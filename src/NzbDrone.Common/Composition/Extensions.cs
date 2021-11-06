using System.Collections.Generic;
using System.Linq;
using DryIoc;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Composition.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static Rules WithNzbDroneRules(this Rules rules)
        {
            return rules.WithMicrosoftDependencyInjectionRules()
                .WithAutoConcreteTypeResolution()
                .WithDefaultReuse(Reuse.Singleton);
        }

        public static IContainer AddStartupContext(this IContainer container, StartupContext context)
        {
            container.RegisterInstance<IStartupContext>(context, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            return container;
        }

        public static IContainer AutoAddServices(this IContainer container, List<string> assemblyNames)
        {
            var assemblies = AssemblyLoader.Load(assemblyNames);

            container.RegisterMany(assemblies,
                serviceTypeCondition: type => type.IsInterface && !string.IsNullOrWhiteSpace(type.FullName) && !type.FullName.StartsWith("System"),
                reuse: Reuse.Singleton);

            container.RegisterMany(assemblies,
                serviceTypeCondition: type => !type.IsInterface && !string.IsNullOrWhiteSpace(type.FullName) && !type.FullName.StartsWith("System"),
                reuse: Reuse.Transient);

            var knownTypes = new KnownTypes(assemblies.SelectMany(x => x.GetTypes()).ToList());
            container.RegisterInstance(knownTypes);

            return container;
        }
    }
}
