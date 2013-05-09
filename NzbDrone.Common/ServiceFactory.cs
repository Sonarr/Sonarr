using System;
using System.Linq;
using System.Collections.Generic;
using TinyIoC;

namespace NzbDrone.Common
{
    public interface IServiceFactory
    {
        T Build<T>() where T : class;
        IEnumerable<T> BuildAll<T>() where T : class;
        object Build(Type contract);
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly TinyIoCContainer _container;

        public ServiceFactory(TinyIoCContainer container)
        {
            _container = container;
        }

        public T Build<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public IEnumerable<T> BuildAll<T>() where T : class
        {
            return _container.ResolveAll<T>().GroupBy(c => c.GetType().FullName).Select(g => g.First());
        }

        public object Build(Type contract)
        {
            return _container.Resolve(contract);
        }
    }
}