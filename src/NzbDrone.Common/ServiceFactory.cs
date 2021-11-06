using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NzbDrone.Common
{
    public interface IServiceFactory
    {
        T Build<T>()
            where T : class;
        IEnumerable<T> BuildAll<T>()
            where T : class;
        object Build(Type contract);
        IEnumerable<Type> GetImplementations(Type contract);
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly System.IServiceProvider _container;

        public ServiceFactory(System.IServiceProvider container)
        {
            _container = container;
        }

        public T Build<T>()
            where T : class
        {
            return _container.GetRequiredService<T>();
        }

        public IEnumerable<T> BuildAll<T>()
            where T : class
        {
            return _container.GetServices<T>().GroupBy(c => c.GetType().FullName).Select(g => g.First());
        }

        public object Build(Type contract)
        {
            return _container.GetRequiredService(contract);
        }

        public IEnumerable<Type> GetImplementations(Type contract)
        {
            return _container.GetServices(contract).Select(x => x.GetType());
        }
    }
}
