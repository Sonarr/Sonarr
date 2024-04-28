using DryIoc;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Extensions
{
    public static class CompositionExtensions
    {
        public static IContainer AddDatabase(this IContainer container)
        {
            container.RegisterDelegate<IDbFactory, IMainDatabase>(f => new MainDatabase(f.Create()), Reuse.Singleton);

            return container;
        }

        public static IContainer AddLogDatabase(this IContainer container)
        {
            container.RegisterDelegate<IDbFactory, ILogDatabase>(f => new LogDatabase(f.Create(MigrationType.Log)), Reuse.Singleton);

            return container;
        }

        public static IContainer AddDummyDatabase(this IContainer container)
        {
            container.RegisterInstance<IMainDatabase>(new MainDatabase(null));

            return container;
        }

        public static IContainer AddDummyLogDatabase(this IContainer container)
        {
            container.RegisterInstance<ILogDatabase>(new LogDatabase(null));

            return container;
        }
    }
}
