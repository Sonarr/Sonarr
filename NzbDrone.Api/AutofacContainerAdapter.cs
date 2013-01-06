using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using ServiceStack.Configuration;

namespace NzbDrone.Api
{
    public class AutofacIocAdapter : IContainerAdapter
    {
        private readonly IContainer _container;

        public AutofacIocAdapter(IContainer container)
        {
            _container = container;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public T TryResolve<T>()
        {
            T result;

            if (_container.TryResolve<T>(out result))
            {
                return result;
            }

            return default(T);
        }
    }
}
